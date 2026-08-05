using System;
using System.Drawing;
using System.Windows.Forms;

namespace DhcpScanner
{
    /// <summary>
    /// 设置窗口（界面语言、扫描线程数、历史数据保存配置）
    /// </summary>
    public class SettingsForm : Form
    {
        private readonly ComboBox _comboLanguage;
        private readonly NumericUpDown _numThreads;
        private readonly RadioButton _radioByTime;
        private readonly RadioButton _radioByCount;
        private readonly Panel _panelTimeRange;
        private readonly Panel _panelCountRange;
        private readonly RadioButton _radioDays14;
        private readonly RadioButton _radioDaysHalf;
        private readonly RadioButton _radioDaysMonth;
        private readonly RadioButton _radioDaysYear;
        private readonly RadioButton _radioNever;
        private readonly RadioButton _radioCustom;
        private readonly NumericUpDown _numCustomDays;
        private readonly RadioButton _radioCount30;
        private readonly RadioButton _radioCount60;
        private readonly RadioButton _radioCount90;
        private readonly RadioButton _radioCount100;
        private readonly AppSettings _settings;

        public SettingsForm()
        {
            _settings = AppSettings.Load();

            Text = Lang.Get("SettingsTitle");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(460, 500);
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

            // 界面语言
            var lblLanguage = new Label
            {
                Text = Lang.Get("LanguageLabel"),
                Location = new Point(20, 24),
                AutoSize = true
            };

            _comboLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(150, 20),
                Size = new Size(200, 25)
            };
            // 中文系列排在英文前面
            _comboLanguage.Items.Add("简体中文");
            _comboLanguage.Items.Add("繁體中文（台灣）");
            _comboLanguage.Items.Add("繁體中文（香港/澳門）");
            _comboLanguage.Items.Add("English");
            _comboLanguage.SelectedIndex = _comboLanguage.Items.IndexOf(LanguageDisplay(_settings.Language));

            // 扫描线程数
            var lblThreads = new Label
            {
                Text = Lang.Get("ThreadsLabel"),
                Location = new Point(20, 64),
                AutoSize = true
            };

            _numThreads = new NumericUpDown
            {
                Location = new Point(150, 60),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 100,
                Value = Math.Clamp(_settings.ScanThreads, 1, 100),
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblHint = new Label
            {
                Text = Lang.Get("ThreadsHint"),
                Location = new Point(20, 98),
                Size = new Size(420, 34),
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("Microsoft YaHei", 8F)
            };

            //
            // 数据保存方式（互斥单选）
            //
            var groupMethod = new GroupBox
            {
                Text = Lang.Get("SaveMethodGroup"),
                Location = new Point(15, 140),
                Size = new Size(430, 68),
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
            };

            _radioByTime = new RadioButton
            {
                Text = Lang.Get("SaveByTime"),
                Location = new Point(24, 32),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 9F)
            };
            _radioByTime.CheckedChanged += (_, _) => UpdateRangeVisibility();

            _radioByCount = new RadioButton
            {
                Text = Lang.Get("SaveByCount"),
                Location = new Point(190, 32),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 9F)
            };
            _radioByCount.CheckedChanged += (_, _) => UpdateRangeVisibility();

            groupMethod.Controls.Add(_radioByTime);
            groupMethod.Controls.Add(_radioByCount);

            //
            // 保存范围（随保存方式动态显示）
            //
            var groupRange = new GroupBox
            {
                Text = Lang.Get("SaveRangeGroup"),
                Location = new Point(15, 218),
                Size = new Size(430, 124),
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
            };

            // 时间范围选项（独立容器，避免与数量选项互相排斥）
            _panelTimeRange = new Panel
            {
                Location = new Point(6, 22),
                Size = new Size(418, 96),
                BackColor = Color.Transparent
            };
            _radioDays14 = new RadioButton { Text = Lang.Get("Range14Days"), Location = new Point(18, 10), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioDaysHalf = new RadioButton { Text = Lang.Get("RangeHalfMonth"), Location = new Point(230, 10), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioDaysMonth = new RadioButton { Text = Lang.Get("RangeOneMonth"), Location = new Point(18, 38), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioDaysYear = new RadioButton { Text = Lang.Get("RangeOneYear"), Location = new Point(230, 38), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioNever = new RadioButton { Text = Lang.Get("RangeNever"), Location = new Point(18, 66), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioCustom = new RadioButton { Text = Lang.Get("RangeCustom"), Location = new Point(230, 66), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };

            _numCustomDays = new NumericUpDown
            {
                Location = new Point(330, 66),
                Size = new Size(72, 25),
                Minimum = 1,
                Maximum = 3650,
                Value = 30,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle
            };
            _radioCustom.CheckedChanged += (_, _) => _numCustomDays.Enabled = _radioCustom.Checked;

            _panelTimeRange.Controls.AddRange(new Control[] { _radioDays14, _radioDaysHalf, _radioDaysMonth, _radioDaysYear, _radioNever, _radioCustom, _numCustomDays });

            // 数量范围选项（独立容器）
            _panelCountRange = new Panel
            {
                Location = new Point(6, 22),
                Size = new Size(418, 72),
                BackColor = Color.Transparent
            };
            _radioCount30 = new RadioButton { Text = Lang.Get("Range30"), Location = new Point(18, 20), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioCount60 = new RadioButton { Text = Lang.Get("Range60"), Location = new Point(230, 20), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioCount90 = new RadioButton { Text = Lang.Get("Range90"), Location = new Point(18, 48), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _radioCount100 = new RadioButton { Text = Lang.Get("Range100"), Location = new Point(230, 48), AutoSize = true, Font = new Font("Microsoft YaHei", 9F) };
            _panelCountRange.Controls.AddRange(new Control[] { _radioCount30, _radioCount60, _radioCount90, _radioCount100 });

            groupRange.Controls.Add(_panelTimeRange);
            groupRange.Controls.Add(_panelCountRange);

            // 根据配置初始化选中项
            InitRangeSelection();
            _radioByTime.Checked = _settings.HistorySaveMode != HistorySaveMode.ByCount;
            _radioByCount.Checked = _settings.HistorySaveMode == HistorySaveMode.ByCount;
            UpdateRangeVisibility();
            // 自定义天数输入框仅在"自定义天数"选中时可用
            _numCustomDays.Enabled = _radioCustom.Checked;

            // 保存设置按钮（应用并保存历史保存配置）
            var btnSaveSettings = new Button
            {
                Text = Lang.Get("SaveSettings"),
                FlatStyle = FlatStyle.System,
                Size = new Size(110, 32),
                Location = new Point(15, 352),
                Cursor = Cursors.Hand
            };
            btnSaveSettings.Click += (_, _) =>
            {
                ApplySaveConfig();
                _settings.Save();
                ScanHistoryStore.Prune();
                MessageBox.Show(Lang.Get("SaveConfigSuccess"), Lang.Get("Success"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // 关于按钮（打开关于窗口）
            var btnAbout = new Button
            {
                Text = Lang.Get("About"),
                FlatStyle = FlatStyle.System,
                Size = new Size(85, 32),
                Location = new Point(135, 352),
                Cursor = Cursors.Hand
            };
            btnAbout.Click += (_, _) =>
            {
                using var aboutForm = new AboutForm();
                aboutForm.ShowDialog(this);
            };

            // 确定按钮（保存全部设置）
            var btnOk = new Button
            {
                Text = Lang.Get("Ok"),
                FlatStyle = FlatStyle.System,
                Size = new Size(85, 32),
                Location = new Point(270, 452),
                Cursor = Cursors.Hand
            };
            btnOk.Click += (_, _) =>
            {
                _settings.Language = LanguageParse(_comboLanguage.SelectedItem?.ToString() ?? "简体中文");
                _settings.ScanThreads = (int)_numThreads.Value;
                ApplySaveConfig();
                _settings.Save();
                ScanHistoryStore.Prune();
                DialogResult = DialogResult.OK;
            };

            var btnCancel = new Button
            {
                Text = Lang.Get("Cancel"),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.System,
                Size = new Size(85, 32),
                Location = new Point(365, 452),
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[] {
                lblLanguage, _comboLanguage, lblThreads, _numThreads, lblHint,
                groupMethod, groupRange, btnSaveSettings, btnAbout, btnOk, btnCancel
            });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        /// <summary>
        /// 根据已保存的配置初始化范围单选选中项
        /// </summary>
        private void InitRangeSelection()
        {
            switch (_settings.HistorySaveDays)
            {
                case 14: _radioDays14.Checked = true; break;
                case 15: _radioDaysHalf.Checked = true; break;
                case 30: _radioDaysMonth.Checked = true; break;
                case 365: _radioDaysYear.Checked = true; break;
                case 0: _radioNever.Checked = true; break;
                default:
                    // 自定义天数
                    _radioCustom.Checked = true;
                    _numCustomDays.Value = Math.Clamp(_settings.HistorySaveDays, 1, 3650);
                    break;
            }

            switch (_settings.HistorySaveMaxRecords)
            {
                case 30: _radioCount30.Checked = true; break;
                case 60: _radioCount60.Checked = true; break;
                case 90: _radioCount90.Checked = true; break;
                default: _radioCount100.Checked = true; break;
            }
        }

        /// <summary>
        /// 根据保存方式切换显示对应的范围选项
        /// </summary>
        private void UpdateRangeVisibility()
        {
            bool byTime = _radioByTime.Checked;
            _panelTimeRange.Visible = byTime;
            _panelCountRange.Visible = !byTime;
        }

        /// <summary>
        /// 将当前界面选择写入设置对象
        /// </summary>
        private void ApplySaveConfig()
        {
            _settings.HistorySaveMode = _radioByTime.Checked ? HistorySaveMode.ByTime : HistorySaveMode.ByCount;

            if (_settings.HistorySaveMode == HistorySaveMode.ByTime)
            {
                if (_radioDays14.Checked) _settings.HistorySaveDays = 14;
                else if (_radioDaysHalf.Checked) _settings.HistorySaveDays = 15;
                else if (_radioDaysMonth.Checked) _settings.HistorySaveDays = 30;
                else if (_radioDaysYear.Checked) _settings.HistorySaveDays = 365;
                else if (_radioNever.Checked) _settings.HistorySaveDays = 0;
                else _settings.HistorySaveDays = (int)_numCustomDays.Value; // 自定义天数
            }
            else
            {
                if (_radioCount30.Checked) _settings.HistorySaveMaxRecords = 30;
                else if (_radioCount60.Checked) _settings.HistorySaveMaxRecords = 60;
                else if (_radioCount90.Checked) _settings.HistorySaveMaxRecords = 90;
                else _settings.HistorySaveMaxRecords = 100;
            }
        }

        /// <summary>
        /// 枚举 → 下拉显示文本
        /// </summary>
        private static string LanguageDisplay(AppLanguage language) => language switch
        {
            AppLanguage.English => "English",
            AppLanguage.TraditionalChinese => "繁體中文（台灣）",
            AppLanguage.TraditionalChineseHk => "繁體中文（香港/澳門）",
            _ => "简体中文"
        };

        /// <summary>
        /// 下拉显示文本 → 枚举
        /// </summary>
        private static AppLanguage LanguageParse(string text) => text switch
        {
            "English" => AppLanguage.English,
            "繁體中文（台灣）" => AppLanguage.TraditionalChinese,
            "繁體中文（香港/澳門）" => AppLanguage.TraditionalChineseHk,
            _ => AppLanguage.Chinese
        };
    }
}
