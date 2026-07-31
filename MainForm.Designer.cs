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
            this.nStart1 = new System.Windows.Forms.NumericUpDown();
            this.nStart2 = new System.Windows.Forms.NumericUpDown();
            this.nStart3 = new System.Windows.Forms.NumericUpDown();
            this.nStart4 = new System.Windows.Forms.NumericUpDown();
            this.dotS1 = new System.Windows.Forms.Label();
            this.dotS2 = new System.Windows.Forms.Label();
            this.dotS3 = new System.Windows.Forms.Label();

            // 至
            this.labelTo = new System.Windows.Forms.Label();

            // 结束IP
            this.labelEndIp = new System.Windows.Forms.Label();
            this.nEnd1 = new System.Windows.Forms.NumericUpDown();
            this.nEnd2 = new System.Windows.Forms.NumericUpDown();
            this.nEnd3 = new System.Windows.Forms.NumericUpDown();
            this.nEnd4 = new System.Windows.Forms.NumericUpDown();
            this.dotE1 = new System.Windows.Forms.Label();
            this.dotE2 = new System.Windows.Forms.Label();
            this.dotE3 = new System.Windows.Forms.Label();

            // 按钮
            this.buttonScan = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();

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
            this.panelSearch.Height = 110;
            this.panelSearch.Padding = new System.Windows.Forms.Padding(20, 15, 20, 10);
            this.panelSearch.BackColor = System.Drawing.Color.FromArgb(240, 244, 247);
            this.panelSearch.Resize += new System.EventHandler(this.PanelSearch_Resize);

            //
            // labelTitle
            //
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(20, 15);
            this.labelTitle.Text = "搜索范围设置";
            this.labelTitle.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);

            //
            // labelStartIp
            //
            this.labelStartIp.AutoSize = true;
            this.labelStartIp.Location = new System.Drawing.Point(20, 52);
            this.labelStartIp.Text = "起始IP:";
            this.labelStartIp.Font = new System.Drawing.Font("Microsoft YaHei", 9F);

            // 起始IP 4段
            int sy = 49;
            SetupNumeric(this.nStart1, 80, sy, int.Parse(defaultStart[0]));
            SetupDot(this.dotS1, 132, sy);
            SetupNumeric(this.nStart2, 146, sy, int.Parse(defaultStart[1]));
            SetupDot(this.dotS2, 198, sy);
            SetupNumeric(this.nStart3, 212, sy, int.Parse(defaultStart[2]));
            SetupDot(this.dotS3, 264, sy);
            SetupNumeric(this.nStart4, 278, sy, int.Parse(defaultStart[3]));

            //
            // labelTo
            //
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(335, 52);
            this.labelTo.Text = "至";
            this.labelTo.Font = new System.Drawing.Font("Microsoft YaHei", 9F);

            //
            // labelEndIp
            //
            this.labelEndIp.AutoSize = true;
            this.labelEndIp.Location = new System.Drawing.Point(365, 52);
            this.labelEndIp.Text = "结束IP:";
            this.labelEndIp.Font = new System.Drawing.Font("Microsoft YaHei", 9F);

            // 结束IP 4段
            int ey = 49;
            SetupNumeric(this.nEnd1, 425, ey, int.Parse(defaultEnd[0]));
            SetupDot(this.dotE1, 477, ey);
            SetupNumeric(this.nEnd2, 491, ey, int.Parse(defaultEnd[1]));
            SetupDot(this.dotE2, 543, ey);
            SetupNumeric(this.nEnd3, 557, ey, int.Parse(defaultEnd[2]));
            SetupDot(this.dotE3, 609, ey);
            SetupNumeric(this.nEnd4, 623, ey, int.Parse(defaultEnd[3]));

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
            this.buttonScan.Size = new System.Drawing.Size(100, 35);
            this.buttonScan.Text = "开始扫描";
            this.buttonScan.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.buttonScan.ForeColor = System.Drawing.Color.White;
            this.buttonScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonScan.Font = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Bold);
            this.buttonScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonScan.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.buttonScan.Click += new System.EventHandler(this.ButtonScan_Click);

            //
            // buttonStop
            //
            this.buttonStop.Size = new System.Drawing.Size(100, 35);
            this.buttonStop.Text = "停止扫描";
            this.buttonStop.BackColor = System.Drawing.Color.FromArgb(200, 50, 50);
            this.buttonStop.ForeColor = System.Drawing.Color.White;
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.Font = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Bold);
            this.buttonStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStop.Enabled = false;
            this.buttonStop.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.buttonStop.Click += new System.EventHandler(this.ButtonStop_Click);

            //
            // buttonClear
            //
            this.buttonClear.Size = new System.Drawing.Size(100, 35);
            this.buttonClear.Text = "清空结果";
            this.buttonClear.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.buttonClear.ForeColor = System.Drawing.Color.White;
            this.buttonClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClear.Font = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Bold);
            this.buttonClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonClear.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.buttonClear.Click += new System.EventHandler(this.ButtonClear_Click);

            //
            // buttonExport
            //
            this.buttonExport.Size = new System.Drawing.Size(100, 35);
            this.buttonExport.Text = "导出结果";
            this.buttonExport.BackColor = System.Drawing.Color.FromArgb(0, 150, 100);
            this.buttonExport.ForeColor = System.Drawing.Color.White;
            this.buttonExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExport.Font = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Bold);
            this.buttonExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonExport.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.buttonExport.Click += new System.EventHandler(this.ButtonExport_Click);

            //
            // panelProgress
            //
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Height = 25;
            this.panelProgress.Padding = new System.Windows.Forms.Padding(20, 5, 20, 5);
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
            this.panelSearch.Controls.Add(this.nStart1);
            this.panelSearch.Controls.Add(this.dotS1);
            this.panelSearch.Controls.Add(this.nStart2);
            this.panelSearch.Controls.Add(this.dotS2);
            this.panelSearch.Controls.Add(this.nStart3);
            this.panelSearch.Controls.Add(this.dotS3);
            this.panelSearch.Controls.Add(this.nStart4);
            this.panelSearch.Controls.Add(this.labelTo);
            this.panelSearch.Controls.Add(this.labelEndIp);
            this.panelSearch.Controls.Add(this.nEnd1);
            this.panelSearch.Controls.Add(this.dotE1);
            this.panelSearch.Controls.Add(this.nEnd2);
            this.panelSearch.Controls.Add(this.dotE2);
            this.panelSearch.Controls.Add(this.nEnd3);
            this.panelSearch.Controls.Add(this.dotE3);
            this.panelSearch.Controls.Add(this.nEnd4);
            this.panelSearch.Controls.Add(this.flowButtons);

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

        private void SetupNumeric(System.Windows.Forms.NumericUpDown n, int x, int y, int value)
        {
            n.Location = new System.Drawing.Point(x, y);
            n.Size = new System.Drawing.Size(50, 25);
            n.Minimum = 0;
            n.Maximum = 255;
            n.Value = value;
            n.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            n.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            n.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }

        private void SetupDot(System.Windows.Forms.Label l, int x, int y)
        {
            l.Text = ".";
            l.Location = new System.Drawing.Point(x, y);
            l.Size = new System.Drawing.Size(12, 25);
            l.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold);
            l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            l.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
        }

        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.TabControl tabControlResults;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.StatusStrip panelStatus;

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelStartIp;
        private System.Windows.Forms.NumericUpDown nStart1;
        private System.Windows.Forms.NumericUpDown nStart2;
        private System.Windows.Forms.NumericUpDown nStart3;
        private System.Windows.Forms.NumericUpDown nStart4;
        private System.Windows.Forms.Label dotS1;
        private System.Windows.Forms.Label dotS2;
        private System.Windows.Forms.Label dotS3;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.Label labelEndIp;
        private System.Windows.Forms.NumericUpDown nEnd1;
        private System.Windows.Forms.NumericUpDown nEnd2;
        private System.Windows.Forms.NumericUpDown nEnd3;
        private System.Windows.Forms.NumericUpDown nEnd4;
        private System.Windows.Forms.Label dotE1;
        private System.Windows.Forms.Label dotE2;
        private System.Windows.Forms.Label dotE3;
        private System.Windows.Forms.Button buttonScan;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonExport;

        private System.Windows.Forms.ProgressBar progressBarScan;

        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCount;
    }
}
