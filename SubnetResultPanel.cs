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
            _grid.Columns["MacAddress"].Width = 180;
            _grid.Columns["HostName"].Width = 170;
            _grid.Columns["PingMs"].Width = 90;
            _grid.Columns["IsRouter"].Width = 120;
            _grid.Columns["Status"].Width = 90;

            _grid.CellContentClick += Grid_CellContentClick;

            // IP网格
            _ipGrid = new IPGridPanel
            {
                Dock = DockStyle.Right,
                Width = 480,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };

            // 添加控件（先加右再加左，保证Dock顺序正确）
            Controls.Add(_grid);
            Controls.Add(_ipGrid);
        }

        /// <summary>
        /// 填充数据
        /// </summary>
        public void PopulateData(List<DhcpServerInfo> results)
        {
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

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _grid.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];
            if (cell.Tag?.ToString() == "dhcp")
            {
                string ip = cell.Value?.ToString() ?? "";
                if (!string.IsNullOrEmpty(ip))
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
                }
            }
        }
    }
}
