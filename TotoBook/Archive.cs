using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TotoBook.ViewModel;

namespace TotoBook
{
    class Archive
    {
        /// <summary>
        /// ファイル一覧に使用するための子要素を取得します。
        /// </summary>
        /// <returns></returns>
        public static (IArchive Archive, IEnumerable<FileInfoViewModel> Children) GetChildrenForList(string fullName, MainWindowViewModel mainWindowViewModel, FileInfoViewModel parent)
        {
            var opts = new ReaderOptions();
            var encoding = Encoding.GetEncoding(932);
            opts.ArchiveEncoding = new ArchiveEncoding
            {
                CustomDecoder = (data, x, y) =>
                {
                    return encoding.GetString(data);
                }
            };

            var archive = ArchiveFactory.Open(fullName, opts);

            var children = GetArchiveItemList(archive.Entries)
                .Select(item => new FileInfoViewModel(item, mainWindowViewModel, parent))
                .ToArray();

            return (archive, children);
        }

        /// <summary>
        /// アーカイブ内のファイルの一覧を取得します。
        /// </summary>
        /// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        /// <returns></returns>
        private static IEnumerable<ArchiveItem> GetArchiveItemList(IEnumerable<Spi.FileInfo> source)
        {
            var sourceArray = source.ToArray();
            var archivedDirectoryList = sourceArray
                .GroupBy(f => f.Path.Split(Path.DirectorySeparatorChar)[0]);

            return archivedDirectoryList
                .SelectMany(g =>
                {
                    // アーカイブファイル内で直下に置かれているファイル
                    if (string.IsNullOrEmpty(g.Key))
                    {
                        return g.Select(f => new ArchiveItem()
                        {
                            Type = ArchiveItem.ArchiveItemType.File,
                            //Name = f.FileName,
                            FileName = f.FileName,
                            FullName = f.FileName,
                            FileSize = f.FileSize,
                            TimeStamp = f.TimeStamp,
                            Position = f.Position,
                        });
                    }

                    var children = GetArchiveItemList(g.Select(f =>
                    {
                        f.Path = f.Path.Replace(g.Key + Path.DirectorySeparatorChar, "");
                        return f;
                    }))
                    .ToArray();

                    //var fileInfo = new Spi.FileInfo()
                    //{
                    //    FileName = g.Key,
                    //    Path = g.Key,
                    //    CompSize = 0,
                    //    CRC = 0,
                    //    FileSize = 0,
                    //    Method = "",
                    //    Position = 0,
                    //    TimeStamp = 0,
                    //};

                    var archivedDirectory = new ArchiveItem()
                    {
                        Type = ArchiveItem.ArchiveItemType.Directory,
                        //Name = g.Key,
                        FileName = g.Key,
                        FullName = g.Key,
                        FileSize = 0,
                        TimeStamp = 0,
                        Position = 0,
                        Children = children,
                    };

                    return new[] { archivedDirectory };
                });
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
                    //Name = f.Key,
                    FullName = entry.Key,
                    FileName = entry.Key,
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
        /// アーカイブ内のファイルの一覧を取得します。
        /// </summary>
        /// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        /// <returns></returns>
        private static IEnumerable<ArchiveItem> AggregateItems(IEnumerable<ArchiveItem> source)
        {
            var sourceArray = source.ToArray();
            var archivedDirectoryList = sourceArray
                .GroupBy(entry => entry.FileName.Split(Path.DirectorySeparatorChar)[0].Split('/')[0]);

            return archivedDirectoryList
                .SelectMany(g =>
                {
                    var files = g.ToArray();
                    var isDirectory = files.Length != 1;

                    // アーカイブファイル内で直下に置かれているファイル
                    if (!isDirectory)
                    {
                        return files;
                    }

                    var directoryName = g.Key;

                    var archiveItems = files.Select(f =>
                    {
                        f.FileName = f.FileName
                            .Replace(g.Key + Path.DirectorySeparatorChar, "")
                            .Replace(g.Key + "/", "");
                        return f;
                    });

                    var children = AggregateItems(archiveItems).ToArray();

                    var archivedDirectory = new ArchiveItem()
                    {
                        Type = ArchiveItem.ArchiveItemType.Directory,
                        //Name = directoryName,
                        FileName = directoryName,
                        FullName = directoryName,
                        Children = children,
                    };

                    return new[] { archivedDirectory };
                });
        }
    }
}
