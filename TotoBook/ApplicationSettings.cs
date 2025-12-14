using System;
using System.IO;
using YamlDotNet.Serialization;

namespace TotoBook
{
    /// <summary>
    /// アプリケーションの設定
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// シングルトン用インスタンス を取得します。
        /// </summary>
        public static ApplicationSettings Instance { get; private set; }

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.yaml");

        public static void LoadSettingsFromFile()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    // 設定ファイルがなければ作成する
                    var serializer = new SerializerBuilder().Build();
                    File.WriteAllText(SettingsFilePath, serializer.Serialize(new ApplicationSettings()));
                }

                var deserializer = new DeserializerBuilder().Build();
                var text = File.ReadAllText(SettingsFilePath);
                Instance = deserializer.Deserialize<ApplicationSettings>(text);
            }
            catch (Exception ex)
            {
                // エラーが発生した場合はデフォルト設定を使用
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                Instance = new ApplicationSettings();
            }
        }

        public static void SaveSettingsToFile()
        {
            try
            {
                var serializer = new SerializerBuilder().Build();
                File.WriteAllText(SettingsFilePath, serializer.Serialize(Instance));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 対応するアーカイブファイルの拡張子のリストを取得します。
        /// </summary>
        public string[] ArchiveExtensions { get; private set; } = new[] { ".zip", ".rar", ".cbz", };

        /// <summary>
        /// 対応する画像ファイルの拡張子のリストを取得します。
        /// </summary>
        public string[] FileExtensions { get; private set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", };

        /// <summary>
        /// Susie プラグインが格納されているディレクトリのパスを取得します。
        /// </summary>
        public string PluginDirectoryPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "plugin");

        /// <summary>
        /// 自動ページ送りの感覚を取得します。（秒）
        /// </summary>
        public double AutoPagerInterval { get; set; } = 2;
    }
}
