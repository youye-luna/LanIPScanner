using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DhcpScanner
{
    /// <summary>
    /// 扫描结果信息
    /// </summary>
    public class DhcpServerInfo
    {
        public IPAddress IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string HostName { get; set; }
        public DateTime ResponseTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDhcpServer { get; set; }
        public long PingMs { get; set; }

        public DhcpServerInfo()
        {
            IpAddress = IPAddress.None;
            MacAddress = string.Empty;
            HostName = string.Empty;
            ResponseTime = DateTime.Now;
            IsActive = false;
            IsDhcpServer = false;
            PingMs = 0;
        }
    }

    /// <summary>
    /// 网络扫描器
    /// </summary>
    public class DhcpScanner
    {
        private readonly List<DhcpServerInfo> _discoveredServers;
        private readonly object _lock = new();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isScanning;

        public event EventHandler<DhcpServerInfo>? ServerFound;
        public event EventHandler<int>? ScanProgress;
        public event EventHandler<List<DhcpServerInfo>>? ScanCompleted;
        public event EventHandler<string>? ScanError;

        public DhcpScanner()
        {
            _discoveredServers = new List<DhcpServerInfo>();
            _cancellationTokenSource = new CancellationTokenSource();
            _isScanning = false;
        }

        public IReadOnlyList<DhcpServerInfo> DiscoveredServers => _discoveredServers.AsReadOnly();
        public bool IsScanning => _isScanning;

        /// <summary>
        /// 扫描并发线程数
        /// </summary>
        public int MaxParallelism { get; set; } = 30;

        /// <summary>
        /// 开始扫描网络设备
        /// </summary>
        public async Task StartScanAsync(string ipRange, int startRange = 1, int endRange = 254)
        {
            if (_isScanning)
                throw new InvalidOperationException("扫描已在进行中");

            _isScanning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _discoveredServers.Clear();

            try
            {
                int total = endRange - startRange + 1;
                int completed = 0;
                var allResults = new List<DhcpServerInfo>();
                var resultsLock = new object();

                // 多线程并行扫描
                await Parallel.ForEachAsync(
                    Enumerable.Range(startRange, total),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = MaxParallelism,
                        CancellationToken = _cancellationTokenSource.Token
                    },
                    async (i, token) =>
                    {
                        string ip = $"{ipRange}.{i}";
                        var result = await ScanSingleIpAsync(ip, token);

                        lock (resultsLock)
                        {
                            allResults.Add(result);
                        }

                        // 更新进度
                        int current = Interlocked.Increment(ref completed);
                        int progress = (int)((double)current / total * 100);
                        ScanProgress?.Invoke(this, progress);
                    });

                // 按IP最后一段排序（升序）
                allResults.Sort((a, b) =>
                {
                    string aLast = a.IpAddress.ToString().Split('.').Last();
                    string bLast = b.IpAddress.ToString().Split('.').Last();
                    if (int.TryParse(aLast, out int aNum) && int.TryParse(bLast, out int bNum))
                        return aNum.CompareTo(bNum);
                    return string.Compare(a.IpAddress.ToString(), b.IpAddress.ToString());
                });

                // 一次性通知所有结果
                foreach (var r in allResults)
                {
                    ServerFound?.Invoke(this, r);
                }

                ScanCompleted?.Invoke(this, allResults);
            }
            catch (OperationCanceledException)
            {
                // 用户取消
            }
            catch (Exception ex)
            {
                ScanError?.Invoke(this, ex.Message);
            }
            finally
            {
                _isScanning = false;
            }
        }

        /// <summary>
        /// 扫描单个IP
        /// </summary>
        private async Task<DhcpServerInfo> ScanSingleIpAsync(string ip, CancellationToken cancellationToken)
        {
            var serverInfo = new DhcpServerInfo { IpAddress = IPAddress.Parse(ip) };

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, 500);

                serverInfo.ResponseTime = DateTime.Now;
                serverInfo.IsActive = reply.Status == IPStatus.Success;
                serverInfo.PingMs = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;

                if (reply.Status == IPStatus.Success)
                {
                    // 并行获取MAC地址和主机名
                    var macTask = GetMacAddressAsync(ip);
                    var hostTask = GetHostNameAsync(ip);

                    await Task.WhenAll(macTask, hostTask);

                    serverInfo.MacAddress = await macTask;
                    serverInfo.HostName = await hostTask;

                    // 检测是否可能是路由器/网关
                    serverInfo.IsDhcpServer = await IsLikelyRouterOrDhcp(ip);
                }
            }
            catch
            {
                // 忽略单个IP扫描失败
            }

            return serverInfo;
        }

        /// <summary>
        /// 判断是否可能是路由器或DHCP服务器
        /// </summary>
        private async Task<bool> IsLikelyRouterOrDhcp(string ip)
        {
            try
            {
                // 方法1：检查常见路由器端口 (80, 443, 8080)
                if (await CheckPortAsync(ip, 80, 300) || 
                    await CheckPortAsync(ip, 443, 300) ||
                    await CheckPortAsync(ip, 8080, 300))
                {
                    return true;
                }

                // 方法2：检查DHCP端口 (67)
                if (await CheckPortAsync(ip, 67, 300))
                {
                    return true;
                }

                // 方法3：检查DNS端口 (53)
                if (await CheckPortAsync(ip, 53, 300))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查TCP端口是否开放
        /// </summary>
        private async Task<bool> CheckPortAsync(string ip, int port, int timeoutMs)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(timeoutMs);
                
                if (await Task.WhenAny(connectTask, timeoutTask) == connectTask)
                {
                    await connectTask;
                    return client.Connected;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取MAC地址
        /// </summary>
        private async Task<string> GetMacAddressAsync(string ip)
        {
            try
            {
                // 先尝试从ARP缓存获取
                var mac = GetMacFromArpCache(ip);
                if (!string.IsNullOrEmpty(mac))
                    return mac;

                // 发送ARP请求
                using var ping = new Ping();
                await ping.SendPingAsync(ip, 200);

                // 再次尝试从ARP缓存获取
                return GetMacFromArpCache(ip) ?? "未知";
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 从ARP缓存获取MAC地址
        /// </summary>
        private string? GetMacFromArpCache(string ip)
        {
            try
            {
                var p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "arp",
                        Arguments = "-a " + ip,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                // 解析ARP输出
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(ip))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            string mac = parts[1].Trim();
                            if (mac.Contains("-") && mac.Length == 17)
                                return mac.ToUpper();
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// 获取主机名
        /// </summary>
        private async Task<string> GetHostNameAsync(string ip)
        {
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(ip);
                return hostEntry.HostName;
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 多网段扫描
        /// </summary>
        public async Task StartMultiSubnetScanAsync(List<string> subnets, int startRange = 1, int endRange = 254)
        {
            if (_isScanning)
                throw new InvalidOperationException("扫描已在进行中");

            _isScanning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _discoveredServers.Clear();

            try
            {
                int totalPerSubnet = endRange - startRange + 1;
                int totalIps = totalPerSubnet * subnets.Count;
                int completed = 0;
                var allResults = new List<DhcpServerInfo>();
                var resultsLock = new object();

                foreach (var subnet in subnets)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    await Parallel.ForEachAsync(
                        Enumerable.Range(startRange, totalPerSubnet),
                        new ParallelOptions
                        {
                            MaxDegreeOfParallelism = MaxParallelism,
                            CancellationToken = _cancellationTokenSource.Token
                        },
                        async (i, token) =>
                        {
                            string ip = $"{subnet}.{i}";
                            var result = await ScanSingleIpAsync(ip, token);

                            lock (resultsLock)
                            {
                                allResults.Add(result);
                            }

                            int current = Interlocked.Increment(ref completed);
                            int progress = (int)((double)current / totalIps * 100);
                            ScanProgress?.Invoke(this, progress);
                        });
                }

                // 按网段再按IP最后一段排序
                allResults.Sort((a, b) =>
                {
                    string aIp = a.IpAddress.ToString();
                    string bIp = b.IpAddress.ToString();
                    var aParts = aIp.Split('.');
                    var bParts = bIp.Split('.');
                    // 先比较前三段
                    for (int i = 0; i < 3; i++)
                    {
                        if (int.TryParse(aParts[i], out int aNum) && int.TryParse(bParts[i], out int bNum))
                        {
                            int cmp = aNum.CompareTo(bNum);
                            if (cmp != 0) return cmp;
                        }
                    }
                    // 再比较最后一段
                    if (int.TryParse(aParts[3], out int aLast) && int.TryParse(bParts[3], out int bLast))
                        return aLast.CompareTo(bLast);
                    return string.Compare(aIp, bIp);
                });

                foreach (var r in allResults)
                {
                    ServerFound?.Invoke(this, r);
                }

                ScanCompleted?.Invoke(this, allResults);
            }
            catch (OperationCanceledException)
            {
                // 用户取消
            }
            catch (Exception ex)
            {
                ScanError?.Invoke(this, ex.Message);
            }
            finally
            {
                _isScanning = false;
            }
        }

        /// <summary>
        /// 判断IP是否为内网（私有）地址
        /// 10.0.0.0/8、172.16.0.0/12、192.168.0.0/16
        /// </summary>
        public static bool IsPrivateIp(string ip)
        {
            var parts = ip.Split('.');
            if (parts.Length != 4) return false;
            if (!int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b)) return false;
            // 10.0.0.0/8
            if (a == 10) return true;
            // 172.16.0.0/12
            if (a == 172 && b >= 16 && b <= 31) return true;
            // 192.168.0.0/16
            if (a == 192 && b == 168) return true;
            return false;
        }

        /// <summary>
        /// 完整IP范围扫描（如 192.168.1.1 至 192.168.3.254）
        /// </summary>
        public async Task StartIpRangeScanAsync(string startIp, string endIp)
        {
            if (_isScanning)
                throw new InvalidOperationException("扫描已在进行中");

            _isScanning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _discoveredServers.Clear();

            long startNum = IpToLong(startIp);
            long endNum = IpToLong(endIp);

            if (startNum > endNum)
                throw new ArgumentException("起始IP不能大于结束IP");

            // 构建有效IP列表（跳过最后一段为0的IP，且只保留内网地址）
            var ipList = new List<long>();
            for (long n = startNum; n <= endNum; n++)
            {
                if ((n & 0xFF) != 0 && IsPrivateIp(LongToIp(n)))
                    ipList.Add(n);
            }

            if (ipList.Count == 0)
            {
                _isScanning = false;
                throw new ArgumentException("扫描范围内不包含内网地址");
            }

            // 统计网段数量（前三段相同为一个网段），超过100个直接抛出
            int subnetCount = ipList.Select(ip => ip >> 8).Distinct().Count();
            if (subnetCount > 100)
            {
                _isScanning = false;
                throw new InvalidOperationException($"TOO_MANY_SUBNETS:{subnetCount}");
            }

            try
            {
                int total = ipList.Count;
                int completed = 0;
                var allResults = new List<DhcpServerInfo>();
                var resultsLock = new object();

                await Parallel.ForEachAsync(
                    ipList,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = MaxParallelism,
                        CancellationToken = _cancellationTokenSource.Token
                    },
                    async (ipNum, token) =>
                    {
                        string ip = LongToIp(ipNum);
                        var result = await ScanSingleIpAsync(ip, token);

                        lock (resultsLock)
                        {
                            allResults.Add(result);
                        }

                        int current = Interlocked.Increment(ref completed);
                        int progress = (int)((double)current / total * 100);
                        ScanProgress?.Invoke(this, progress);
                    });

                // 按IP数值排序
                allResults.Sort((a, b) =>
                {
                    long aNum = IpToLong(a.IpAddress.ToString());
                    long bNum = IpToLong(b.IpAddress.ToString());
                    return aNum.CompareTo(bNum);
                });

                foreach (var r in allResults)
                {
                    ServerFound?.Invoke(this, r);
                }

                ScanCompleted?.Invoke(this, allResults);
            }
            catch (OperationCanceledException)
            {
                // 用户取消
            }
            catch (Exception ex)
            {
                ScanError?.Invoke(this, ex.Message);
            }
            finally
            {
                _isScanning = false;
            }
        }

        /// <summary>
        /// 将IP地址转为long数值
        /// </summary>
        private static long IpToLong(string ip)
        {
            var parts = ip.Split('.');
            if (parts.Length != 4)
                throw new ArgumentException($"IP格式不正确: {ip}");
            return (long.Parse(parts[0]) << 24) + (long.Parse(parts[1]) << 16) +
                   (long.Parse(parts[2]) << 8) + long.Parse(parts[3]);
        }

        /// <summary>
        /// 将long数值转为IP地址字符串
        /// </summary>
        private static string LongToIp(long value)
        {
            return $"{(value >> 24) & 0xFF}.{(value >> 16) & 0xFF}.{(value >> 8) & 0xFF}.{value & 0xFF}";
        }

        /// <summary>
        /// 停止扫描
        /// </summary>
        public void StopScan()
        {
            if (_isScanning)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// 获取本地网络IP范围
        /// </summary>
        public static string GetLocalNetworkRange()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var parts = ip.ToString().Split('.');
                        if (parts.Length == 4)
                            return $"{parts[0]}.{parts[1]}.{parts[2]}";
                    }
                }
            }
            catch { }

            return "192.168.1";
        }

        /// <summary>
        /// 获取所有本地网络子网
        /// </summary>
        public static List<string> GetLocalNetworkSubnets()
        {
            var subnets = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var parts = ip.ToString().Split('.');
                        if (parts.Length == 4)
                        {
                            string subnet = $"{parts[0]}.{parts[1]}.{parts[2]}";
                            if (!subnets.Contains(subnet))
                                subnets.Add(subnet);
                        }
                    }
                }
            }
            catch { }

            if (subnets.Count == 0)
                subnets.Add("192.168.1");

            return subnets;
        }

        /// <summary>
        /// 获取网关IP
        /// </summary>
        public static string? GetGatewayIp()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface.OperationalStatus == OperationalStatus.Up)
                    {
                        var props = iface.GetIPProperties();
                        foreach (var gateway in props.GatewayAddresses)
                        {
                            if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return gateway.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
