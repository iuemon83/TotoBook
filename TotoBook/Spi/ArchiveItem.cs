using System.Collections.Generic;

namespace TotoBook.Spi
{
    /// <summary>
    /// アーカイブされた要素
    /// </summary>
    public class ArchiveItem
    {
        /// <summary>
        /// アーカイブされた要素の種類
        /// </summary>
        public enum ArchiveItemType
        {
            Directory,
            File
        }

        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 種類
        /// </summary>
        public ArchiveItemType Type { get; set; }

        /// <summary>
        /// ファイルの情報
        /// </summary>
        public FileInfo FileInfo { get; set; }

        /// <summary>
        /// 子要素
        /// </summary>
        public IReadOnlyList<ArchiveItem> Children { get; set; } = new ArchiveItem[0];
    }
}
