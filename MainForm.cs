using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DhcpScanner
{
    public partial class MainForm : Form
    {
        private readonly DhcpScanner _scanner;
        private bool _isScanning;

        public MainForm()
        {
            InitializeComponent();
            _scanner = new DhcpScanner();
            _isScanning = false;

            // 绑定事件
            _scanner.ServerFound += Scanner_ServerFound;
            _scanner.ScanProgress += Scanner_ScanProgress;
            _scanner.ScanCompleted += Scanner_ScanCompleted;
            _scanner.ScanError += Scanner_ScanError;

            // 初始布局按钮位置
            PanelSearch_Resize(this, EventArgs.Empty);
        }

        /// <summary>
        /// 获取默认起始IP（本机IP，最后一段为1）
        /// </summary>
        private static string GetDefaultStartIp()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var parts = ip.ToString().Split('.');
                        if (parts.Length == 4)
                            return $"{parts[0]}.{parts[1]}.{parts[2]}.1";
                    }
                }
            }
            catch { }
            return "192.168.1.1";
        }

        /// <summary>
        /// 获取默认结束IP（本机IP，最后一段为254）
        /// </summary>
        private static string GetDefaultEndIp()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var parts = ip.ToString().Split('.');
                        if (parts.Length == 4)
                            return $"{parts[0]}.{parts[1]}.{parts[2]}.254";
                    }
                }
            }
            catch { }
            return "192.168.1.254";
        }

        /// <summary>
        /// 开始扫描按钮点击事件
        /// </summary>
        private async void ButtonScan_Click(object sender, EventArgs e)
        {
            if (_isScanning)
            {
                MessageBox.Show("扫描正在进行中，请等待完成或停止扫描。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string startIp = $"{(int)nStart1.Value}.{(int)nStart2.Value}.{(int)nStart3.Value}.{(int)nStart4.Value}";
            string endIp = $"{(int)nEnd1.Value}.{(int)nEnd2.Value}.{(int)nEnd3.Value}.{(int)nEnd4.Value}";

            if (string.IsNullOrEmpty(startIp) || string.IsNullOrEmpty(endIp))
            {
                MessageBox.Show("请输入起始IP和结束IP！", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 清除之前的标签页
            tabControlResults.TabPages.Clear();

            // 更新UI状态
            _isScanning = true;
            buttonScan.Enabled = false;
            buttonStop.Enabled = true;
            buttonClear.Enabled = false;
            buttonExport.Enabled = false;
            progressBarScan.Value = 0;
            toolStripStatusLabel.Text = $"正在扫描 {startIp} ~ {endIp}...";
            toolStripStatusCount.Text = "发现 0 个设备";

            try
            {
                await _scanner.StartIpRangeScanAsync(startIp, endIp);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("TOO_MANY_SUBNETS:"))
                {
                    int count = int.Parse(ex.Message.Split(':')[1]);
                    ShowTooManySubnetsMessage(count);
                }
                else
                {
                    MessageBox.Show($"扫描出错: {ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _isScanning = false;
                buttonScan.Enabled = true;
                buttonStop.Enabled = false;
                buttonClear.Enabled = true;
                buttonExport.Enabled = true;
            }
        }

        /// <summary>
        /// 停止扫描按钮点击事件
        /// </summary>
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (_isScanning)
            {
                _scanner.StopScan();
                toolStripStatusLabel.Text = "扫描已停止";
            }
        }

        /// <summary>
        /// 清空结果按钮点击事件
        /// </summary>
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            tabControlResults.TabPages.Clear();
            progressBarScan.Value = 0;
            toolStripStatusCount.Text = "发现 0 个DHCP服务器";
            toolStripStatusLabel.Text = "就绪";
        }

        /// <summary>
        /// 导出结果按钮点击事件
        /// </summary>
        private void ButtonExport_Click(object sender, EventArgs e)
        {
            if (tabControlResults.TabPages.Count == 0)
            {
                MessageBox.Show("没有可导出的数据！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV文件 (*.csv)|*.csv|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                FileName = $"DHCP扫描结果_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = "csv"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportToCsv(saveFileDialog.FileName);
                    MessageBox.Show($"数据已成功导出到:\n{saveFileDialog.FileName}", "成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 导出数据到CSV文件（所有标签页）
        /// </summary>
        private void ExportToCsv(string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);

            writer.WriteLine("网段,IP地址,MAC地址,主机名,延迟(ms),DHCP服务器,状态");

            foreach (TabPage tab in tabControlResults.TabPages)
            {
                if (tab.Controls[0] is SubnetResultPanel panel)
                {
                    string subnet = tab.Text.Replace("网段 ", "");
                    foreach (var row in panel.GetRows())
                    {
                        string ip = row.Cells["IpAddress"].Value?.ToString() ?? "";
                        string mac = row.Cells["MacAddress"].Value?.ToString() ?? "";
                        string host = row.Cells["HostName"].Value?.ToString() ?? "";
                        string ping = row.Cells["PingMs"].Value?.ToString() ?? "";
                        string router = row.Cells["IsRouter"].Value?.ToString() ?? "";
                        string status = row.Cells["Status"].Value?.ToString() ?? "";

                        writer.WriteLine($"{EscapeCsvField(subnet)},{EscapeCsvField(ip)},{EscapeCsvField(mac)},{EscapeCsvField(host)},{EscapeCsvField(ping)},{EscapeCsvField(router)},{EscapeCsvField(status)}");
                    }
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                return $"\"{field.Replace("\"", "\"\"")}\"";
            return field;
        }

        /// <summary>
        /// 发现设备事件处理
        /// </summary>
        private void Scanner_ServerFound(object? sender, DhcpServerInfo serverInfo)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Scanner_ServerFound(sender, serverInfo)));
                return;
            }
        }

        /// <summary>
        /// 扫描进度更新事件处理
        /// </summary>
        private void Scanner_ScanProgress(object? sender, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Scanner_ScanProgress(sender, progress)));
                return;
            }

            progressBarScan.Value = Math.Min(progress, 100);
            toolStripStatusLabel.Text = $"正在扫描... {progress}%";
        }

        /// <summary>
        /// 扫描完成事件处理 —— 按网段分组，每个网段一个标签页
        /// </summary>
        private void Scanner_ScanCompleted(object? sender, List<DhcpServerInfo> results)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Scanner_ScanCompleted(sender, results)));
                return;
            }

            progressBarScan.Value = 100;
            toolStripStatusLabel.Text = "正在整理结果...";

            // 按网段分组
            var groups = results
                .GroupBy(r => string.Join(".", r.IpAddress.ToString().Split('.').Take(3)))
                .OrderBy(g => g.Key)
                .ToList();

            tabControlResults.TabPages.Clear();

            foreach (var group in groups)
            {
                var subnetResults = group.OrderBy(r =>
                {
                    var parts = r.IpAddress.ToString().Split('.');
                    return long.Parse(parts[0]) << 24 | long.Parse(parts[1]) << 16 | long.Parse(parts[2]) << 8 | long.Parse(parts[3]);
                }).ToList();

                var panel = new SubnetResultPanel
                {
                    Dock = DockStyle.Fill
                };
                panel.PopulateData(subnetResults);

                var onlineCount = subnetResults.Count(x => x.IsActive);
                var routerCount = subnetResults.Count(x => x.IsDhcpServer);

                var tab = new TabPage($"网段 {group.Key}");
                tab.Controls.Add(panel);
                tabControlResults.TabPages.Add(tab);
            }

            // 统计总数
            int totalOnline = results.Count(x => x.IsActive);
            int totalRouter = results.Count(x => x.IsDhcpServer);
            int totalNoDevice = results.Count - totalOnline;

            toolStripStatusCount.Text = $"在线: {totalOnline}，无设备: {totalNoDevice}，DHCP服务器: {totalRouter}";
            toolStripStatusLabel.Text = "扫描完成";

            string message = $"扫描完成！\n\n共扫描 {results.Count} 个IP\n在线设备: {totalOnline}\n无设备: {totalNoDevice}\nDHCP服务器: {totalRouter}";
            MessageBox.Show(message, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 扫描错误事件处理
        /// </summary>
        private void Scanner_ScanError(object? sender, string errorMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Scanner_ScanError(sender, errorMessage)));
                return;
            }

            toolStripStatusLabel.Text = $"扫描出错: {errorMessage}";
            MessageBox.Show($"扫描过程中发生错误:\n{errorMessage}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isScanning)
            {
                var result = MessageBox.Show("扫描正在进行中，确定要退出吗？", "确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _scanner.StopScan();
            }

            base.OnFormClosing(e);
        }

        /// <summary>
        /// 搜索面板大小变化时，重新布局按钮位置
        /// </summary>
        private void PanelSearch_Resize(object? sender, EventArgs e)
        {
            if (flowButtons == null || panelSearch == null) return;

            int ipControlsEndX = nEnd4.Location.X + nEnd4.Width + 20;
            int flowTopY = nEnd4.Location.Y - 4;

            int availableWidth = panelSearch.Width - ipControlsEndX - 20;
            if (availableWidth < 220) availableWidth = 220;

            flowButtons.Location = new System.Drawing.Point(ipControlsEndX, flowTopY);
            flowButtons.Size = new System.Drawing.Size(availableWidth, 100);
        }

        /// <summary>
        /// 显示网段过多提示（带灰色副标题）
        /// </summary>
        private static void ShowTooManySubnetsMessage(int count)
        {
            using var form = new Form
            {
                Text = "提示",
                Size = new System.Drawing.Size(360, 180),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = System.Drawing.Color.White,
            };

            var labelMain = new Label
            {
                Text = $"最多测100个网段",
                Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                AutoSize = false,
                Location = new System.Drawing.Point(20, 25),
                Size = new System.Drawing.Size(310, 35),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            var labelSub = new Label
            {
                Text = $"当前有 {count} 个网段\n什么鬼，谁家网段那么多",
                Font = new System.Drawing.Font("Microsoft YaHei", 9F),
                ForeColor = System.Drawing.Color.FromArgb(160, 160, 160),
                AutoSize = false,
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(310, 45),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(80, 30),
                Location = new System.Drawing.Point(135, 105),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Microsoft YaHei", 9F),
                Cursor = System.Windows.Forms.Cursors.Hand,
            };
            btnOk.FlatAppearance.BorderSize = 0;

            form.Controls.AddRange(new Control[] { labelMain, labelSub, btnOk });
            form.AcceptButton = btnOk;

            form.ShowDialog();
        }
    }
}
