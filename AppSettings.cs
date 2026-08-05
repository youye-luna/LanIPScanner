using System;
using System.IO;
using System.Text.Json;

namespace DhcpScanner
{
    /// <summary>
    /// 界面语言
    /// </summary>
    public enum AppLanguage
    {
        /// <summary>简体中文</summary>
        Chinese = 0,
        /// <summary>English</summary>
        English = 1,
        /// <summary>繁体中文（台湾）</summary>
        TraditionalChinese = 2,
        /// <summary>繁体中文（香港/澳门）</summary>
        TraditionalChineseHk = 3
    }

    /// <summary>
    /// 扫描历史保存方式
    /// </summary>
    public enum HistorySaveMode
    {
        /// <summary>按时间保存（保留最近 N 天）</summary>
        ByTime = 0,
        /// <summary>按数量保存（保留最近 N 条）</summary>
        ByCount = 1
    }

    /// <summary>
    /// 应用设置（持久化到安装目录下的 settings.json）
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 界面语言
        /// </summary>
        public AppLanguage Language { get; set; } = AppLanguage.Chinese;

        /// <summary>
        /// 扫描并发线程数
        /// </summary>
        public int ScanThreads { get; set; } = 30;

        /// <summary>
        /// 历史数据保存方式（按时间/按数量，互斥）
        /// </summary>
        public HistorySaveMode HistorySaveMode { get; set; } = HistorySaveMode.ByCount;

        /// <summary>
        /// 按时间保存时保留的天数（近14天/半个月/一个月/一年/自定义天数，0 表示永不清除）
        /// </summary>
        public int HistorySaveDays { get; set; } = 30;

        /// <summary>
        /// 按数量保存时保留的记录条数（近30/60/90/100个）
        /// </summary>
        public int HistorySaveMaxRecords { get; set; } = 100;

        /// <summary>
        /// 配置文件所在目录（程序所在目录）
        /// </summary>
        private static readonly string ConfigDir = AppContext.BaseDirectory;

        /// <summary>
        /// 配置文件完整路径
        /// </summary>
        private static readonly string FilePath = Path.Combine(ConfigDir, "settings.json");

        /// <summary>
        /// 从配置文件加载设置，首次运行时自动检测系统语言并保存
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                    if (settings != null)
                    {
                        settings.ScanThreads = Math.Clamp(settings.ScanThreads, 1, 100);
                        settings.HistorySaveDays = Math.Clamp(settings.HistorySaveDays, 0, 3650);
                        settings.HistorySaveMaxRecords = Math.Clamp(settings.HistorySaveMaxRecords, 1, 10000);
                        return settings;
                    }
                }
            }
            catch { }

            // 首次运行：根据系统语言自动设置，然后保存配置文件
            var appSettings = new AppSettings { Language = DetectSystemLanguage() };
            appSettings.Save();
            return appSettings;
        }

        /// <summary>
        /// 根据系统区域设置自动检测语言
        /// </summary>
        private static AppLanguage DetectSystemLanguage()
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            if (culture.TwoLetterISOLanguageName == "zh")
            {
                // 简体中文：zh-Hans, zh-CN, zh-SG 等
                // 繁体中文：zh-TW, zh-HK, zh-MO 等
                if (culture.Name.StartsWith("zh-Hans") || culture.Name == "zh-CN" || culture.Name == "zh-SG")
                    return AppLanguage.Chinese;
                // 香港/澳门使用繁体，但用词与台湾不同（如 網絡/資料）
                if (culture.Name.StartsWith("zh-Hant-HK") || culture.Name == "zh-HK" || culture.Name == "zh-MO")
                    return AppLanguage.TraditionalChineseHk;
                return AppLanguage.TraditionalChinese;
            }
            return AppLanguage.English;
        }

        /// <summary>
        /// 保存设置到配置文件
        /// </summary>
        public void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
