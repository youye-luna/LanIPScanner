using System;
using System.Drawing;
using System.Windows.Forms;

namespace DhcpScanner
{
    /// <summary>
    /// 设置窗口（界面语言、扫描线程数）
    /// </summary>
    public class SettingsForm : Form
    {
        private readonly ComboBox _comboLanguage;
        private readonly NumericUpDown _numThreads;
        private readonly AppSettings _settings;

        public SettingsForm()
        {
            _settings = AppSettings.Load();

            Text = Lang.Get("SettingsTitle");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(380, 200);
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

            var lblLanguage = new Label
            {
                Text = Lang.Get("LanguageLabel"),
                Location = new Point(20, 28),
                AutoSize = true
            };

            _comboLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(150, 24),
                Size = new Size(200, 25)
            };
            _comboLanguage.Items.Add("中文");
            _comboLanguage.Items.Add("English");
            _comboLanguage.Items.Add("繁體中文");
            _comboLanguage.SelectedIndex = (int)_settings.Language;

            var lblThreads = new Label
            {
                Text = Lang.Get("ThreadsLabel"),
                Location = new Point(20, 68),
                AutoSize = true
            };

            _numThreads = new NumericUpDown
            {
                Location = new Point(150, 64),
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
                Location = new Point(20, 100),
                Size = new Size(330, 34),
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("Microsoft YaHei", 8F)
            };

            var btnOk = new Button
            {
                Text = Lang.Get("Ok"),
                FlatStyle = FlatStyle.System,
                Size = new Size(85, 30),
                Location = new Point(180, 150),
                Cursor = Cursors.Hand
            };
            btnOk.Click += (_, _) =>
            {
                _settings.Language = (AppLanguage)_comboLanguage.SelectedIndex;
                _settings.ScanThreads = (int)_numThreads.Value;
                _settings.Save();
                DialogResult = DialogResult.OK;
            };

            var btnCancel = new Button
            {
                Text = Lang.Get("Cancel"),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.System,
                Size = new Size(85, 30),
                Location = new Point(275, 150),
                Cursor = Cursors.Hand
            };

            Controls.AddRange(new Control[] { lblLanguage, _comboLanguage, lblThreads, _numThreads, lblHint, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
