using System;
using System.IO;
using YamlDotNet.Serialization;

namespace TotoBook
{
    /// <summary>
    /// アプリケーションの設定
    /// </summary>
    class ApplicationSettings
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

        public static void SaveSettingsToFile()
        {
            var serializer = new SerializerBuilder().Build();
            File.WriteAllText(SettingsFilePath, serializer.Serialize(Instance));
        }

        /// <summary>
        /// 対応するアーカイブファイルの拡張子のリストを取得します。
        /// </summary>
        public string[] ArchiveExtensions { get; private set; } = new[] { ".zip", ".rar", ".cbz", };

        /// <summary>
        /// 対応する画像ファイルの拡張子のリストを取得します。
        /// </summary>
        public string[] FileExtensions { get; private set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", };

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
