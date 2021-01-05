using System;
using System.Collections.Generic;
using System.IO;

namespace TotoBook
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

        ///// <summary>
        ///// 名前
        ///// </summary>
        //public string Name { get; set; }

        /// <summary>
        /// 種類
        /// </summary>
        public ArchiveItemType Type { get; set; }

        ///// <summary>
        ///// ファイルの情報
        ///// </summary>
        //public FileInfo FileInfo { get; set; }

        public string FullName { get; set; }

        public string FileName { get; set; }

        public long TimeStamp { get; set; }

        public long FileSize { get; set; }

        public int Position { get; set; }

        /// <summary>
        /// 子要素
        /// </summary>
        public IReadOnlyList<ArchiveItem> Children { get; set; } = new ArchiveItem[0];

        public Func<Stream> CreateStream { get; set; }
    }
}
