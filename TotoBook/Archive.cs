using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TotoBook.Spi;
using TotoBook.ViewModel;

namespace TotoBook
{
    class Archive
    {
        /// <summary>
        /// ファイル一覧に使用するための子要素を取得します。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FileInfoViewModel> GetChildrenForList(string fullName, MainWindowViewModel mainWindowViewModel, FileInfoViewModel parent, FileInfoViewModel archiveParent)
        {
            switch (Path.GetExtension(fullName).ToLower())
            {
                case ".zip":
                    //ZIP書庫を開く
                    using (var a = ZipFile.OpenRead(fullName))
                    //Encodingを指定する場合は、次のようにする
                    //using (ZipArchive a = ZipFile.Open(@"C:\test\1.zip",
                    //    ZipArchiveMode.Read,
                    //    System.Text.Encoding.GetEncoding("shift_jis")))
                    {
                        //書庫内のファイルとディレクトリを列挙する
                        //foreach (var e in a.Entries)
                        //{
                        //    Console.WriteLine("名前       : {0}", e.Name);
                        //    //ディレクトリ付きのファイル名
                        //    Console.WriteLine("フルパス   : {0}", e.FullName);
                        //    Console.WriteLine("サイズ     : {0}", e.Length);
                        //    Console.WriteLine("圧縮サイズ : {0}", e.CompressedLength);
                        //    Console.WriteLine("更新日時   : {0}", e.LastWriteTime);
                        //}

                        return a.Entries
                            .Select(e => new FileInfoViewModel(e, mainWindowViewModel, parent, archiveParent))
                            .ToArray();
                    }

                default:
                    var fileInfoList = Spi.SpiManager.GetArchiveInfo(fullName).ToArray();
                    return GetArchiveItemList(fileInfoList)
                        .Select(a => new FileInfoViewModel(a, mainWindowViewModel, parent, archiveParent));
            }
        }

        /// <summary>
        /// アーカイブ内のファイルの一覧を取得します。
        /// </summary>
        /// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        /// <param name="parent">source の親要素</param>
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
                            Name = f.FileName,
                            FileInfo = f,
                        });
                    }

                    var children = GetArchiveItemList(g.Select(f =>
                    {
                        f.Path = f.Path.Replace(g.Key + Path.DirectorySeparatorChar, "");
                        return f;
                    }))
                    .ToArray();

                    var fileInfo = new Spi.FileInfo()
                    {
                        FileName = g.Key,
                        Path = g.Key,
                        CompSize = 0,
                        CRC = 0,
                        FileSize = 0,
                        Method = "",
                        Position = 0,
                        TimeStamp = 0,
                    };

                    var archivedDirectory = new ArchiveItem()
                    {
                        Type = ArchiveItem.ArchiveItemType.Directory,
                        Name = g.Key,
                        FileInfo = fileInfo,
                        Children = children,
                    };

                    return new[] { archivedDirectory };
                });
        }

        /// <summary>
        /// このファイルへアクセスするためのStream を取得します。
        /// </summary>
        /// <returns></returns>
        public static Stream GetFileStream(FileInfoViewModel archiveParent, ArchiveEntry archiveEntry)
        {
            switch (Path.GetExtension(archiveParent.FullName).ToLower())
            {
                case ".zip":
                    using (var a = ZipFile.OpenRead(archiveParent.FullName))
                    {
                        var entry = a.GetEntry(archiveEntry.FileName);
                        if (entry == null)
                        {
                            return null;
                        }
                        else
                        {
                            var ms = new MemoryStream();
                            using (var fs = entry.Open())
                            {
                                fs.CopyTo(ms);
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            return ms;
                        }
                    }

                default:
                    var bytes = Spi.SpiManager.GetFile(archiveParent.FullName, archiveEntry.Position);

                    return new MemoryStream(bytes);
                    //return SpiManager.GetPictureStream(fileInfo.FileName, bytes);
            }
        }
    }
}
