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
        public static IEnumerable<FileInfoViewModel> GetChildrenForList(string fullName, MainWindowViewModel mainWindowViewModel, FileInfoViewModel parent)
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

            MainWindowViewModel.CurrentArchive?.Dispose();
            MainWindowViewModel.CurrentArchive = ArchiveFactory.Open(fullName, opts);

            return GetArchiveItemList(MainWindowViewModel.CurrentArchive.Entries)
                .Select(item => new FileInfoViewModel(item, mainWindowViewModel, parent))
                .ToArray();

            //using (var archive = ArchiveFactory.Open(fullName, opts))
            //{
            //    //foreach (var entry in archive.Entries)
            //    //{
            //    //    if (entry.Key.Contains("\\") || entry.Key.Contains("/"))
            //    //    {
            //    //        // ディレクトリ
            //    //        var directoryName = Path.GetDirectoryName(entry.Key);
            //    //        if (!directories.Contains(directoryName))
            //    //        {
            //    //            directories.Add(directoryName);
            //    //            yield return new FileInfoViewModel(entry, mainWindowViewModel, parent, archiveParent);
            //    //        }
            //    //    }
            //    //    else
            //    //    {
            //    //        // ファイル
            //    //        yield return new FileInfoViewModel(entry, mainWindowViewModel, parent, archiveParent);
            //    //    }
            //    //}

            //    return GetArchiveItemList(archive.Entries)
            //        .Select(item => new FileInfoViewModel(item, mainWindowViewModel, parent))
            //        .ToArray();

            //    //return archive.Entries
            //    //    .Select(entry => new FileInfoViewModel(entry, mainWindowViewModel, parent, archiveParent))
            //    //    .ToArray();
            //}

            //var fileInfoList = Spi.SpiManager.GetArchiveInfo(fullName).ToArray();
            //return GetArchiveItemList(fileInfoList)
            //    .Select(a => new FileInfoViewModel(a, mainWindowViewModel, parent, archiveParent));
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
            return AggregateItems(source
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
                        using (var entryStream = entry.OpenEntryStream())
                        {
                            var ms = new MemoryStream();
                            entryStream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            return ms;
                        }
                    }
                }));


            //var sourceArray = source.ToArray();
            //var archivedDirectoryList = sourceArray
            //    .GroupBy(f => f.Key.Split(Path.DirectorySeparatorChar)[0].Split('/')[0]);

            //return archivedDirectoryList
            //    .SelectMany(g =>
            //    {
            //        var files = g.ToArray();
            //        var isDirectory = files.Length != 1;

            //        // アーカイブファイル内で直下に置かれているファイル
            //        if (!isDirectory)
            //        {
            //            return files.Select(f => new ArchiveItem()
            //            {
            //                Type = ArchiveItem.ArchiveItemType.File,
            //                //Name = f.Key,
            //                FullName = f.Key,
            //                FileName = f.Key,
            //                FileSize = f.Size,
            //                TimeStamp = f.CreatedTime?.Ticks ?? 0,
            //                Position = 0,
            //            });
            //        }

            //        var directoryName = g.Key;

            //        var children = GetArchiveItemList(files.Select(f => new Spi.FileInfo()
            //        {
            //            FileName = Path.GetFileName(f.Key),
            //            FileSize = (int)f.Size,
            //            Path = f.Key.Replace(directoryName + Path.DirectorySeparatorChar, ""),
            //            TimeStamp = (int)(f.CreatedTime?.Ticks ?? 0),
            //        }))
            //        .ToArray();

            //        var archivedDirectory = new ArchiveItem()
            //        {
            //            Type = ArchiveItem.ArchiveItemType.Directory,
            //            //Name = directoryName,
            //            FileName = directoryName,
            //            FullName = directoryName,
            //            Children = children,
            //        };

            //        return new[] { archivedDirectory };
            //    });
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

                    var children = AggregateItems(files.Select(f =>
                    {
                        f.FileName = f.FileName
                            .Replace(g.Key + Path.DirectorySeparatorChar, "")
                            .Replace(g.Key + "/", "");
                        return f;
                    }))
                    .ToArray();

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

        /// <summary>
        /// このファイルへアクセスするためのStream を取得します。
        /// </summary>
        /// <returns></returns>
        public static Stream GetFileStream(FileInfoViewModel archiveParent, ArchiveItem archiveItem)
        {
            var entry = MainWindowViewModel.CurrentArchive.Entries
                .FirstOrDefault(e => e.Key == archiveItem.FullName);

            using (var entryStream = entry.OpenEntryStream())
            {
                var ms = new MemoryStream();
                entryStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }

            //var opts = new ReaderOptions();
            //var encoding = Encoding.GetEncoding(932);
            //opts.ArchiveEncoding = new ArchiveEncoding
            //{
            //    CustomDecoder = (data, x, y) =>
            //    {
            //        return encoding.GetString(data);
            //    }
            //};

            //using (var archive = ArchiveFactory.Open(archiveParent.FullName, opts))
            //{
            //    var entry = archive.Entries
            //        .FirstOrDefault(e => e.Key == archiveItem.FullName);

            //    using (var entryStream = entry.OpenEntryStream())
            //    {
            //        var ms = new MemoryStream();
            //        entryStream.CopyTo(ms);
            //        ms.Seek(0, SeekOrigin.Begin);
            //        return ms;
            //    }
            //}

            //var bytes = Spi.SpiManager.GetFile(archiveParent.FullName, archiveEntry.Position);

            //return new MemoryStream(bytes);
            ////return SpiManager.GetPictureStream(fileInfo.FileName, bytes);
        }
    }
}
