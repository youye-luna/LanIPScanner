using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DhcpScanner
{
    /// <summary>
    /// 扫描历史窗口（查看/删除/清空历史扫描记录）
    /// </summary>
    public class HistoryForm : Form
    {
        private readonly ListView _list;
        private readonly Label _lblEmpty;
        private readonly List<ScanHistoryRecord> _records;

        /// <summary>
        /// 用户选择要查看的历史记录（点击"查看"或双击时设置）
        /// </summary>
        public ScanHistoryRecord? SelectedRecord { get; private set; }

        public HistoryForm()
        {
            _records = ScanHistoryStore.Load();

            Text = Lang.Get("HistoryTitle");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(720, 460);
            BackColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);

            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream("DhcpScanner.app.ico");
                if (stream != null)
                    Icon = new Icon(stream);
            }
            catch { }

            // 顶部提示
            var lblHint = new Label
            {
                Text = Lang.Get("HistoryHint"),
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("Microsoft YaHei", 8.5F)
            };

            // 历史列表
            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Microsoft YaHei", 9.5F),
            };
            _list.Columns.Add(Lang.Get("ColHistoryTime"), 130);
            _list.Columns.Add(Lang.Get("ColHistoryRange"), 260);
            _list.Columns.Add(Lang.Get("ColHistoryTotal"), 90);
            _list.Columns.Add(Lang.Get("ColHistoryOnline"), 90);
            _list.Columns.Add(Lang.Get("ColHistoryDhcp"), 120);
            _list.DoubleClick += List_DoubleClick;

            // 空状态提示
            _lblEmpty = new Label
            {
                Text = Lang.Get("HistoryEmpty"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(170, 170, 170),
                Font = new Font("Microsoft YaHei", 11F),
                Visible = _records.Count == 0
            };

            var panelList = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 6, 12, 6) };
            panelList.Controls.Add(_lblEmpty);
            panelList.Controls.Add(_list);
            _list.Visible = _records.Count > 0;

            // 底部按钮（从右到左：关闭/清空/删除/查看）
            var btnView = new Button { Text = Lang.Get("HistoryView"), FlatStyle = FlatStyle.System, Size = new Size(85, 30), Cursor = Cursors.Hand };
            btnView.Click += BtnView_Click;

            var btnDelete = new Button { Text = Lang.Get("HistoryDelete"), FlatStyle = FlatStyle.System, Size = new Size(85, 30), Cursor = Cursors.Hand };
            btnDelete.Click += BtnDelete_Click;

            var btnClear = new Button { Text = Lang.Get("HistoryClear"), FlatStyle = FlatStyle.System, Size = new Size(85, 30), Cursor = Cursors.Hand };
            btnClear.Click += BtnClear_Click;

            var btnClose = new Button { Text = Lang.Get("Close"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.System, Size = new Size(85, 30), Cursor = Cursors.Hand };

            var panelButtons = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(12, 8, 12, 8) };
            panelButtons.Controls.Add(btnClose);
            panelButtons.Controls.Add(btnClear);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnView);
            panelButtons.Resize += (_, _) =>
            {
                int x = panelButtons.ClientSize.Width - panelButtons.Padding.Right;
                foreach (Control c in panelButtons.Controls)
                {
                    c.Location = new Point(x - c.Width, panelButtons.Padding.Top);
                    x = c.Left - 8;
                }
            };

            Controls.Add(panelList);
            Controls.Add(panelButtons);
            Controls.Add(lblHint);
            CancelButton = btnClose;

            ReloadList();
        }

        /// <summary>
        /// 刷新列表显示
        /// </summary>
        private void ReloadList()
        {
            _list.Items.Clear();
            foreach (var r in _records)
            {
                int total = r.Devices.Count;
                int online = r.Devices.Count(d => d.IsActive);
                int dhcp = r.Devices.Count(d => d.IsDhcpServer);

                var item = new ListViewItem(r.ScanTime.ToString(Lang.Get("HistoryDateFormat")));
                item.SubItems.Add(string.Format(Lang.Get("HistoryRangeFormat"), r.StartIp, r.EndIp));
                item.SubItems.Add(total.ToString());
                item.SubItems.Add(online.ToString());
                item.SubItems.Add(dhcp.ToString());
                item.Tag = r;
                _list.Items.Add(item);
            }
            _lblEmpty.Visible = _records.Count == 0;
            _list.Visible = _records.Count > 0;
        }

        private void List_DoubleClick(object? sender, EventArgs e)
        {
            ViewSelected();
        }

        private void BtnView_Click(object? sender, EventArgs e)
        {
            ViewSelected();
        }

        private void ViewSelected()
        {
            if (_list.SelectedItems.Count == 0 || _list.SelectedItems[0].Tag is not ScanHistoryRecord record)
            {
                MessageBox.Show(Lang.Get("HistorySelectFirst"), Lang.Get("Tip"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedRecord = record;
            DialogResult = DialogResult.OK;
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_list.SelectedItems.Count == 0 || _list.SelectedItems[0].Tag is not ScanHistoryRecord record)
            {
                MessageBox.Show(Lang.Get("HistorySelectFirst"), Lang.Get("Tip"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(Lang.Get("HistoryConfirmDelete"), Lang.Get("Confirm"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            ScanHistoryStore.Delete(record);
            _records.Remove(record);
            ReloadList();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (_records.Count == 0)
                return;
            if (MessageBox.Show(Lang.Get("HistoryConfirmClear"), Lang.Get("Confirm"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            ScanHistoryStore.Clear();
            _records.Clear();
            ReloadList();
        }
    }
}
