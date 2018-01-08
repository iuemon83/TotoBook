using System;
using System.IO;

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
        /// スタティックコンストラクタ
        /// </summary>
        static ApplicationSettings()
        {
            Instance = new ApplicationSettings();
        }

        /// <summary>
        /// 対応するアーカイブファイルの拡張子のリストを取得します。
        /// </summary>
        public string[] ArchiveExtensions { get; private set; } = new[] { ".zip", ".rar", };

        /// <summary>
        /// 対応する画像ファイルの拡張子のリストを取得します。
        /// </summary>
        public string[] FileExtensions { get; private set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", };

        /// <summary>
        /// Susie プラグインが格納されているディレクトリのパスを取得します。
        /// </summary>
        public string PluginDirectoryPath { get; private set; } = Path.Combine(Environment.CurrentDirectory, "plugin");

        /// <summary>
        /// 自動ページ送りの感覚を取得します。（秒）
        /// </summary>
        public double AutoPagerInterval { get; set; } = 2;
    }
}
