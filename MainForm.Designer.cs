namespace DhcpScanner
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream("DhcpScanner.app.ico");
                if (stream != null)
                    this.Icon = new System.Drawing.Icon(stream);
            }
            catch { }
            this.Text = "局域网设备扫描工具";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(1040, 700);
            this.BackColor = System.Drawing.Color.White;

            // 创建控件
            this.panelSearch = new System.Windows.Forms.Panel();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.panelProgress = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelStatus = new System.Windows.Forms.StatusStrip();
            this.tabControlResults = new System.Windows.Forms.TabControl();

            // 标题
            this.labelTitle = new System.Windows.Forms.Label();

            // 起始IP
            this.labelStartIp = new System.Windows.Forms.Label();
            this.ipStart = new IpAddressControl();

            // 至
            this.labelTo = new System.Windows.Forms.Label();

            // 结束IP
            this.labelEndIp = new System.Windows.Forms.Label();
            this.ipEnd = new IpAddressControl();

            // 按钮
            this.buttonScan = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.buttonHistory = new System.Windows.Forms.Button();

            // 进度条
            this.progressBarScan = new System.Windows.Forms.ProgressBar();

            // 状态栏
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusCount = new System.Windows.Forms.ToolStripStatusLabel();

            // 默认值
            var defaultStart = GetDefaultStartIp().Split('.');
            var defaultEnd = GetDefaultEndIp().Split('.');

            //
            // panelSearch
            //
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearch.Height = 112;
            this.panelSearch.Padding = new System.Windows.Forms.Padding(20, 12, 20, 8);
            this.panelSearch.BackColor = System.Drawing.Color.FromArgb(240, 244, 247);
            this.panelSearch.Resize += new System.EventHandler(this.PanelSearch_Resize);

            //
            // labelTitle
            //
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(20, 8);
            this.labelTitle.Text = "搜索范围设置";
            this.labelTitle.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);

            //
            // labelStartIp
            //
            this.labelStartIp.AutoSize = true;
            this.labelStartIp.Location = new System.Drawing.Point(20, 38);
            this.labelStartIp.Text = "起始IP:";
            this.labelStartIp.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.labelStartIp.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);

            // 起始IP输入框（原生IP地址控件）
            this.ipStart.Location = new System.Drawing.Point(80, 34);
            this.ipStart.Size = new System.Drawing.Size(248, 28);
            this.ipStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.ipStart.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipStart.SetAddress(string.Join(".", defaultStart));

            //
            // labelTo
            //
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(335, 38);
            this.labelTo.Text = "至";
            this.labelTo.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.labelTo.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);

            //
            // labelEndIp
            //
            this.labelEndIp.AutoSize = true;
            this.labelEndIp.Location = new System.Drawing.Point(365, 38);
            this.labelEndIp.Text = "结束IP:";
            this.labelEndIp.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.labelEndIp.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);

            // 结束IP输入框（原生IP地址控件）
            this.ipEnd.Location = new System.Drawing.Point(425, 34);
            this.ipEnd.Size = new System.Drawing.Size(248, 28);
            this.ipEnd.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.ipEnd.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipEnd.SetAddress(string.Join(".", defaultEnd));

            //
            // flowButtons
            //
            this.flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowButtons.WrapContents = true;
            this.flowButtons.AutoSize = false;
            this.flowButtons.Padding = new System.Windows.Forms.Padding(0);
            this.flowButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flowButtons.BackColor = System.Drawing.Color.Transparent;

            //
            // buttonScan
            //
            this.buttonScan.Size = new System.Drawing.Size(100, 30);
            this.buttonScan.Text = "开始扫描";
            this.buttonScan.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScan.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.buttonScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonScan.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonScan.Click += new System.EventHandler(this.ButtonScan_Click);

            //
            // buttonStop
            //
            this.buttonStop.Size = new System.Drawing.Size(100, 30);
            this.buttonStop.Text = "停止扫描";
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonStop.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.buttonStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStop.Enabled = false;
            this.buttonStop.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonStop.Click += new System.EventHandler(this.ButtonStop_Click);

            //
            // buttonClear
            //
            this.buttonClear.Size = new System.Drawing.Size(100, 30);
            this.buttonClear.Text = "清空结果";
            this.buttonClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClear.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.buttonClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonClear.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonClear.Click += new System.EventHandler(this.ButtonClear_Click);

            //
            // buttonExport
            //
            this.buttonExport.Size = new System.Drawing.Size(100, 30);
            this.buttonExport.Text = "导出结果";
            this.buttonExport.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonExport.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.buttonExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonExport.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonExport.Click += new System.EventHandler(this.ButtonExport_Click);

            //
            // buttonSettings
            //
            this.buttonSettings.Size = new System.Drawing.Size(100, 30);
            this.buttonSettings.Text = "设置";
            this.buttonSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSettings.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.buttonSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSettings.Click += new System.EventHandler(this.ButtonSettings_Click);

            //
            // buttonHistory
            //
            this.buttonHistory.Size = new System.Drawing.Size(100, 30);
            this.buttonHistory.Text = "历史";
            this.buttonHistory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonHistory.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.buttonHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonHistory.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonHistory.Click += new System.EventHandler(this.ButtonHistory_Click);

            //
            // panelProgress
            //
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Height = 32;
            this.panelProgress.Padding = new System.Windows.Forms.Padding(20, 7, 20, 7);
            this.panelProgress.BackColor = System.Drawing.Color.White;

            //
            // progressBarScan
            //
            this.progressBarScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBarScan.Style = System.Windows.Forms.ProgressBarStyle.Continuous;

            //
            // tabControlResults
            //
            this.tabControlResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlResults.Font = new System.Drawing.Font("Microsoft YaHei", 9.5F);
            this.tabControlResults.Padding = new System.Drawing.Point(12, 4);

            //
            // panelBottom
            //
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 30;
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(240, 244, 247);

            //
            // panelStatus
            //
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripStatusLabel,
                this.toolStripStatusCount
            });

            //
            // toolStripStatusLabel
            //
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Text = "就绪";
            this.toolStripStatusLabel.Spring = true;
            this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            //
            // toolStripStatusCount
            //
            this.toolStripStatusCount.Name = "toolStripStatusCount";
            this.toolStripStatusCount.Text = "发现 0 个DHCP服务器";
            this.toolStripStatusCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 添加控件到面板
            this.panelSearch.Controls.Add(this.labelTitle);
            this.panelSearch.Controls.Add(this.labelStartIp);
            this.panelSearch.Controls.Add(this.ipStart);
            this.panelSearch.Controls.Add(this.labelTo);
            this.panelSearch.Controls.Add(this.labelEndIp);
            this.panelSearch.Controls.Add(this.ipEnd);
            this.panelSearch.Controls.Add(this.flowButtons);
            this.panelSearch.Controls.Add(this.buttonSettings);
            this.panelSearch.Controls.Add(this.buttonHistory);

            this.flowButtons.Controls.Add(this.buttonScan);
            this.flowButtons.Controls.Add(this.buttonStop);
            this.flowButtons.Controls.Add(this.buttonClear);
            this.flowButtons.Controls.Add(this.buttonExport);

            this.panelProgress.Controls.Add(this.progressBarScan);

            this.panelBottom.Controls.Add(this.panelStatus);

            // 添加面板到窗体
            this.Controls.Add(this.tabControlResults);
            this.Controls.Add(this.panelProgress);
            this.Controls.Add(this.panelSearch);
            this.Controls.Add(this.panelBottom);
        }

        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.TabControl tabControlResults;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.StatusStrip panelStatus;

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelStartIp;
        private IpAddressControl ipStart;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.Label labelEndIp;
        private IpAddressControl ipEnd;
        private System.Windows.Forms.Button buttonScan;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.Button buttonHistory;

        private System.Windows.Forms.ProgressBar progressBarScan;

        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCount;
    }
}
