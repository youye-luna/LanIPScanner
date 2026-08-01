using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DhcpScanner
{
    /// <summary>
    /// 单个网段的结果面板（DataGridView + IPGridPanel）
    /// </summary>
    public class SubnetResultPanel : UserControl
    {
        private readonly DataGridView _grid;
        private readonly IPGridPanel _ipGrid;
        private List<DhcpServerInfo> _results = new();

        public SubnetResultPanel()
        {
            // 表格
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Microsoft YaHei", 10F),
                ColumnHeadersHeight = 35,
                EnableHeadersVisualStyles = false,
            };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 220, 240);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);

            // 不显示"网段"列（每个标签页已经按网段分组了）
            _grid.Columns.Add("IpAddress", "IP地址");
            _grid.Columns.Add("MacAddress", "MAC地址");
            _grid.Columns.Add("HostName", "主机名");
            _grid.Columns.Add("PingMs", "延迟(ms)");
            _grid.Columns.Add("IsRouter", "DHCP服务器");
            _grid.Columns.Add("Status", "状态");

            _grid.Columns["IpAddress"].Width = 140;
            _grid.Columns["MacAddress"].Width = 160;
            _grid.Columns["HostName"].Width = 150;
            _grid.Columns["PingMs"].Width = 80;
            _grid.Columns["IsRouter"].Width = 110;
            _grid.Columns["Status"].Width = 80;

            _grid.CellDoubleClick += Grid_CellDoubleClick;

            // IP网格
            _ipGrid = new IPGridPanel
            {
                Dock = DockStyle.Right,
                Width = 480,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _ipGrid.CellClicked += IpGrid_CellClicked;
            _ipGrid.CellDoubleClicked += IpGrid_CellDoubleClicked;

            // 添加控件（先加右再加左，保证Dock顺序正确）
            Controls.Add(_grid);
            Controls.Add(_ipGrid);
        }

        /// <summary>
        /// 填充数据
        /// </summary>
        public void PopulateData(List<DhcpServerInfo> results)
        {
            _results = results;
            _grid.Rows.Clear();
            _ipGrid.ResetAllColors();

            foreach (var info in results)
            {
                string statusText = info.IsActive ? "在线" : "无设备";
                string pingText = info.IsActive ? info.PingMs.ToString() : "-";

                int rowIndex = _grid.Rows.Add(
                    info.IpAddress.ToString(),
                    info.IsActive ? info.MacAddress : "-",
                    info.IsActive ? info.HostName : "-",
                    pingText,
                    info.IsDhcpServer ? "是" : "否",
                    statusText
                );

                var row = _grid.Rows[rowIndex];

                if (info.IsDhcpServer)
                {
                    row.Cells["IsRouter"].Style.ForeColor = Color.Red;
                    row.Cells["IsRouter"].Style.Font = new Font(_grid.Font, FontStyle.Bold);
                    row.Cells["IpAddress"].Style.ForeColor = Color.FromArgb(0, 102, 204);
                    row.Cells["IpAddress"].Style.Font = new Font(_grid.Font, FontStyle.Underline);
                    row.Cells["IpAddress"].Tag = "dhcp";
                }

                if (info.IsActive)
                {
                    row.Cells["Status"].Style.ForeColor = Color.Green;
                    row.Cells["Status"].Style.Font = new Font(_grid.Font, FontStyle.Bold);
                }
                else
                {
                    row.Cells["Status"].Style.ForeColor = Color.Gray;
                    row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                }

                // 更新IP网格颜色
                var parts = info.IpAddress.ToString().Split('.');
                if (parts.Length == 4 && int.TryParse(parts[3], out int ipLast))
                {
                    int index = ipLast - 1;
                    if (index >= 0 && index < 255)
                    {
                        if (info.IsDhcpServer)
                            _ipGrid.SetIpColor(index, Color.FromArgb(255, 138, 128));
                        else if (info.IsActive)
                            _ipGrid.SetIpColor(index, Color.FromArgb(33, 150, 243));
                        else
                            _ipGrid.SetIpColor(index, Color.FromArgb(76, 175, 80));
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前表格所有行数据（用于导出）
        /// </summary>
        public IReadOnlyList<DataGridViewRow> GetRows()
        {
            return _grid.Rows.Cast<DataGridViewRow>().ToList().AsReadOnly();
        }

        /// <summary>
        /// IP分布图格子单击：跳转到表格中对应IP行并高亮
        /// </summary>
        private void IpGrid_CellClicked(int ipIndex)
        {
            int ipLast = ipIndex + 1;
            string suffix = "." + ipLast;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                string ip = row.Cells["IpAddress"].Value?.ToString() ?? "";
                if (!ip.EndsWith(suffix, StringComparison.Ordinal)) continue;

                _grid.ClearSelection();
                _grid.CurrentCell = row.Cells["IpAddress"];
                row.Selected = true;
                _grid.FirstDisplayedScrollingRowIndex = row.Index;
                return;
            }
        }

        /// <summary>
        /// IP分布图格子双击：跳转并弹出详情
        /// </summary>
        private void IpGrid_CellDoubleClicked(int ipIndex)
        {
            int ipLast = ipIndex + 1;
            string suffix = "." + ipLast;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                string ip = row.Cells["IpAddress"].Value?.ToString() ?? "";
                if (!ip.EndsWith(suffix, StringComparison.Ordinal)) continue;

                _grid.ClearSelection();
                _grid.CurrentCell = row.Cells["IpAddress"];
                row.Selected = true;
                _grid.FirstDisplayedScrollingRowIndex = row.Index;
                ShowDetailDialog(row);
                return;
            }
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _grid.Rows[e.RowIndex];
            ShowDetailDialog(row);
        }

        /// <summary>
        /// 弹出设备详情对话框
        /// </summary>
        private void ShowDetailDialog(DataGridViewRow row)
        {
            string ip = row.Cells["IpAddress"].Value?.ToString() ?? "";
            string mac = row.Cells["MacAddress"].Value?.ToString() ?? "";
            string host = row.Cells["HostName"].Value?.ToString() ?? "";
            string ping = row.Cells["PingMs"].Value?.ToString() ?? "";
            string dhcp = row.Cells["IsRouter"].Value?.ToString() ?? "";
            string status = row.Cells["Status"].Value?.ToString() ?? "";
            bool isActive = status == "在线";
            bool isDhcp = dhcp == "是";

            var statusColor = isDhcp ? Color.FromArgb(255, 138, 128) : isActive ? Color.FromArgb(33, 150, 243) : Color.FromArgb(158, 158, 158);

            var form = new Form
            {
                Text = $"设备详情 - {ip}",
                Size = new Size(420, 400),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
            };

            // 状态颜色条（顶部）
            var colorBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 6,
                BackColor = statusColor,
            };
            form.Controls.Add(colorBar);

            // 标题
            var lblTitle = new Label
            {
                Text = ip,
                Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = true,
                Location = new Point(20, 20),
            };
            form.Controls.Add(lblTitle);

            // 状态标签
            var lblStatusTag = new Label
            {
                Text = isDhcp ? "DHCP服务器" : isActive ? "在线" : "无设备",
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = statusColor,
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Location = new Point(20, 48),
            };
            form.Controls.Add(lblStatusTag);

            int y = 85;
            int labelX = 25;
            int valueX = 105;
            var labelFont = new Font("Microsoft YaHei", 9.5F);
            var valueFont = new Font("Microsoft YaHei", 9.5F, FontStyle.Bold);

            var fields = new (string label, string value, Color color)[]
            {
                ("IP 地 址", ip, Color.FromArgb(40, 40, 40)),
                ("MAC 地址", mac, Color.FromArgb(40, 40, 40)),
                ("主 机 名", host, Color.FromArgb(40, 40, 40)),
                ("延迟", ping == "-" ? "-" : ping + " ms", isActive ? Color.FromArgb(76, 175, 80) : Color.Gray),
                ("DHCP服务器", dhcp, isDhcp ? Color.Red : Color.FromArgb(40, 40, 40)),
            };

            foreach (var (label, value, color) in fields)
            {
                form.Controls.Add(new Label
                {
                    Text = label,
                    Font = labelFont,
                    ForeColor = Color.FromArgb(120, 120, 120),
                    AutoSize = true,
                    Location = new Point(labelX, y),
                });
                form.Controls.Add(new Label
                {
                    Text = value,
                    Font = valueFont,
                    ForeColor = color,
                    AutoSize = true,
                    Location = new Point(valueX, y),
                });
                y += 28;
            }

            // Ping按钮
            var btnPing = new Button
            {
                Text = "Ping",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnPing.FlatAppearance.BorderSize = 0;
            btnPing.Click += (_, _) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k ping {ip} -t",
                        UseShellExecute = true,
                    });
                }
                catch { }
            };

            // 访问后台按钮（仅DHCP服务器显示）
            var btnWeb = new Button
            {
                Text = "访问后台",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = isDhcp,
            };
            btnWeb.FlatAppearance.BorderSize = 0;
            btnWeb.Click += (_, _) =>
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"http://{ip}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch { }
            };

            // IE访问后台按钮（仅DHCP服务器显示）
            var btnIe = new Button
            {
                Text = "IE访问",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = isDhcp,
            };
            btnIe.FlatAppearance.BorderSize = 0;
            btnIe.Click += (_, _) =>
            {
                try
                {
                    // 完全复刻 VBScript 调用方式
                    var ieType = Type.GetTypeFromProgID("InternetExplorer.Application");
                    if (ieType == null)
                    {
                        MessageBox.Show("IE COM 组件未注册，请确认已启用 Internet Explorer 11 功能。",
                            "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    dynamic ie = Activator.CreateInstance(ieType)!;
                    ie!.Navigate("about:blank");
                    ie.Visible = 1;
                    // 延迟跳转，等待 IE 窗口显示
                    var timer = new System.Windows.Forms.Timer { Interval = 500 };
                    timer.Tick += (_, _) =>
                    {
                        timer.Stop();
                        try { ie.Navigate($"http://{ip}"); } catch { }
                        try { System.Runtime.InteropServices.Marshal.ReleaseComObject(ie); } catch { }
                    };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法启动 IE 浏览器：{ex.Message}\n请确认已启用 Internet Explorer 11 功能。",
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            // 按钮排列（两行）
            var btnClose = new Button
            {
                Text = "关闭",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => form.Close();

            int btnW = 120, btnH = 35, gap = 15;
            var allBtns = new List<Button> { btnPing, btnWeb, btnIe, btnClose };
            var visibleBtns = allBtns.Where(b => b.Visible).ToList();
            int cols = 2;
            int rows = (visibleBtns.Count + cols - 1) / cols;
            int totalW = cols * btnW + (cols - 1) * gap;
            int startX = (form.ClientSize.Width - totalW) / 2;
            int btnY = y + 10;

            for (int i = 0; i < visibleBtns.Count; i++)
            {
                int r = i / cols;
                int c = i % cols;
                visibleBtns[i].Location = new Point(startX + c * (btnW + gap), btnY + r * (btnH + gap));
                form.Controls.Add(visibleBtns[i]);
            }

            form.Show();
        }
    }
}
