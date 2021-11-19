using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TotoBook
{
    public class Archive
    {
        /// <summary>
        /// ファイル一覧に使用するための子要素を取得します。
        /// </summary>
        /// <returns></returns>
        public static (IArchive Archive, IEnumerable<ArchiveItem> Children) GetChildrenForList(string fullName)
        {
            var opts = new ReaderOptions();
            var encoding = Encoding.GetEncoding(932);
            opts.ArchiveEncoding = new ArchiveEncoding
            {
                CustomDecoder = (data, x, y) => encoding.GetString(data)
            };

            var archive = ArchiveFactory.Open(fullName, opts);

            return (archive, GetArchiveItemList(archive.Entries));
        }

        /// <summary>
        /// ファイル一覧に使用するための子要素を取得します。
        /// </summary>
        /// <returns></returns>
        public static (IArchive Archive, IEnumerable<ArchiveItem> Children) GetChildrenForList(Stream source)
        {
            var opts = new ReaderOptions();
            var encoding = Encoding.GetEncoding(932);
            opts.ArchiveEncoding = new ArchiveEncoding
            {
                CustomDecoder = (data, x, y) => encoding.GetString(data)
            };

            var archive = ArchiveFactory.Open(source, opts);

            return (archive, GetArchiveItemList(archive.Entries));
        }

        /// <summary>
        /// アーカイブ内のファイルの一覧を取得します。
        /// </summary>
        /// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        /// <returns></returns>
        private static IEnumerable<ArchiveItem> GetArchiveItemList(IEnumerable<IArchiveEntry> source)
        {
            var archiveItems = source
                .Where(entry => !entry.IsDirectory)
                .Select(entry => new ArchiveItem()
                {
                    Type = ArchiveItem.ArchiveItemType.File,
                    FullName = entry.Key.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                    FileName = entry.Key.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                    FileSize = entry.Size,
                    TimeStamp = entry.CreatedTime?.Ticks ?? 0,
                    Position = 0,
                    CreateStream = () =>
                    {
                        using var entryStream = entry.OpenEntryStream();
                        var ms = new MemoryStream();
                        entryStream.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        return ms;
                    }
                });

            return AggregateItems(archiveItems);
        }

        /// <summary>
        /// 並列になっている要素を階層構造に組み立てなおして取得します。
        /// </summary>
        /// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        /// <returns></returns>
        private static IEnumerable<ArchiveItem> AggregateItems(IEnumerable<ArchiveItem> source)
        {
            var sourceArray = source.ToArray();
            var archivedDirectoryList = sourceArray
                .GroupBy(entry => entry.FileName.Split(Path.DirectorySeparatorChar)[0]);

            return archivedDirectoryList
                .SelectMany(g =>
                {
                    var files = g.ToArray();
                    var isDirectory = files[0].FileName.Contains(Path.DirectorySeparatorChar);

                    // アーカイブファイル内で直下に置かれているファイル
                    if (!isDirectory)
                    {
                        return files;
                    }

                    var directoryName = g.Key;

                    var archiveItems = files.Select(f =>
                    {
                        f.FileName = f.FileName.Substring(f.FileName.IndexOf(Path.DirectorySeparatorChar) + 1);
                        return f;
                    });

                    var children = AggregateItems(archiveItems).ToArray();

                    var archivedDirectory = new ArchiveItem()
                    {
                        Type = ArchiveItem.ArchiveItemType.Directory,
                        FileName = directoryName,
                        FullName = directoryName,
                        Children = children,
                    };

                    return new[] { archivedDirectory };
                });
        }
    }
}
