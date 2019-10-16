using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TotoBook.ViewModel
{
    /// <summary>
    /// ファイル
    /// </summary>
    public class FileInfoViewModel : ViewModelBase, IFileListItemViewModel, IFileTreeItemViewModel
    {
        public static FileInfoViewModel Dummy
        {
            get
            {
                return new FileInfoViewModel();
            }
        }

        private static Dictionary<string, BitmapSource> IconPathCache { get; set; } = new Dictionary<string, BitmapSource>();

        private static BitmapSource GetIconPath(FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null) return null;
            if (fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory)) return GetIconPathForDirectory(fileSystemInfo);

            var extension = Path.GetExtension(fileSystemInfo.Name);
            if (IconPathCache.TryGetValue(extension, out var result))
            {
                return result;
            }
            else
            {
                Spi.Win32.SHFILEINFO shinfo = new Spi.Win32.SHFILEINFO();
                try
                {
                    Spi.Win32.SHGetFileInfo($"x{extension}", 0, out shinfo, (uint)Marshal.SizeOf(typeof(Spi.Win32.SHFILEINFO)), Spi.Win32.SHGFI_ICON | Spi.Win32.SHGFI_SMALLICON | Spi.Win32.SHGFI_USEFILEATTRIBUTES);
                    if (shinfo.hIcon == IntPtr.Zero) return null;

                    using var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
                    var iconSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    IconPathCache.Add(extension, iconSource);

                    return iconSource;
                }
                finally
                {
                    Spi.Win32.DestroyIcon(shinfo.hIcon);
                }
            }
        }

        private static BitmapSource GetIconPathForDirectory(FileSystemInfo directoryInfo)
        {
            if (directoryInfo.Attributes == FileAttributes.Directory
                && IconPathCache.ContainsKey("")) return IconPathCache[""];

            Spi.Win32.SHFILEINFO shinfo = new Spi.Win32.SHFILEINFO();
            try
            {
                Spi.Win32.SHGetFileInfo(directoryInfo.FullName, 0, out shinfo, (uint)Marshal.SizeOf(typeof(Spi.Win32.SHFILEINFO)), Spi.Win32.SHGFI_ICON | Spi.Win32.SHGFI_SMALLICON);
                if (shinfo.hIcon == IntPtr.Zero) return null;

                using var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
                var iconSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                if (directoryInfo.Attributes == FileAttributes.Directory) IconPathCache.Add("", iconSource);
                return iconSource;
            }
            finally
            {
                Spi.Win32.DestroyIcon(shinfo.hIcon);
            }
        }

        [Flags]
        public enum FileInfoType
        {
            Unknown = 0,
            File = 1,
            Directory = 1 << 1,
            Archive = 1 << 2,
            ArchivedDirectory = 1 << 3,
        }

        /// <summary>
        /// メインウィンドウのビューモデル
        /// </summary>
        private readonly MainWindowViewModel mainWindowViewModel;

        public string FullName { get; set; }
        public string Name { get; set; }
        public string LastUpdateDate { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }

        private bool isDisplayed;
        public bool IsDisplayed
        {
            get { return this.isDisplayed; }
            set
            {
                if (this.isDisplayed != value)
                {
                    this.isDisplayed = value;
                    this.RaisePropertyChanged(nameof(this.IsDisplayed));
                }
            }
        }

        private BitmapSource icon;
        public BitmapSource Icon
        {
            get
            {
                if (this.icon == null)
                {
                    this.icon = GetIconPath(this.FileSystemInfo);
                }
                return this.icon;
            }
            set { this.icon = value; }
        }

        private FileSystemInfo FileSystemInfo { get; set; }
        private ArchiveItem ArchiveItem { get; set; }

        public FileInfoType FileType { get; set; }

        private bool _IsExpanded = false;
        private bool _IsSelected = false;
        private FileInfoViewModel _Parent = null;
        private ObservableCollection<FileInfoViewModel> _Children = null;

        /// <summary>
        /// 要素が展開されている場合はTrue、そうでなければFalse を取得します。
        /// </summary>
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (this._IsExpanded == value) return;

                if (value) this.LoadChildren();

                _IsExpanded = value;
                this.RaisePropertyChanged(nameof(this.IsExpanded));
            }
        }

        /// <summary>
        /// 親要素
        /// </summary>
        IFileTreeItemViewModel IFileTreeItemViewModel.Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// 親要素
        /// </summary>
        public FileInfoViewModel Parent
        {
            get { return _Parent; }
            set { _Parent = value; this.RaisePropertyChanged(nameof(this.Parent)); }
        }

        /// <summary>
        /// 子要素のリスト
        /// </summary>
        IEnumerable<IFileTreeItemViewModel> IFileTreeItemViewModel.Children
        {
            get { return _Children; }
        }

        /// <summary>
        /// 子要素のリスト
        /// </summary>
        public ObservableCollection<FileInfoViewModel> Children
        {
            get { return _Children; }
            set { _Children = value; this.RaisePropertyChanged(nameof(this.Children)); }
        }

        /// <summary>
        /// 選択されている場合はTure、そうでなければFalse を取得します。
        /// </summary>
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected == value) return;

                _IsSelected = value;
                this.RaisePropertyChanged(nameof(this.IsSelected));

                if (value)
                {
                    this.mainWindowViewModel?.SelectFileTreeItemCommand(this);
                }
            }
        }

        private readonly FileInfoViewModel[] archiveChildren;

        private FileInfoViewModel()
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="source"></param>
        public FileInfoViewModel(FileSystemInfo source, MainWindowViewModel mainWindowViewModel)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            this.IsExpanded = false;
            this.Add(Dummy);

            this.FileSystemInfo = source;
            this.FullName = source.FullName;
            this.Name = source.Name;
            this.LastUpdateDate = source.LastWriteTime.ToString();
            this.FileType = source is DirectoryInfo
                ? FileInfoType.Directory
                : ApplicationSettings.Instance.ArchiveExtensions.Contains(source.Extension.ToLower())
                    ? FileInfoType.Archive
                    : ApplicationSettings.Instance.FileExtensions.Contains(source.Extension.ToLower())
                        ? FileInfoType.File
                        : FileInfoType.Unknown;

            switch (source)
            {
                case System.IO.FileInfo f:
                    this.Type = "";
                    this.Size = f.Length.ToString();
                    this.Parent = f.Directory == null
                        ? null
                        : new FileInfoViewModel(f.Directory, mainWindowViewModel);
                    break;

                case DirectoryInfo d:
                    this.Type = "";
                    this.Size = "";
                    this.Parent = d.Parent == null
                        ? null
                        : new FileInfoViewModel(d.Parent, mainWindowViewModel);
                    break;
            }
        }

        //public FileInfoViewModel(IEntry source, MainWindowViewModel mainWindowViewModel, FileInfoViewModel parent, FileInfoViewModel archiveParent)
        //{
        //    this.mainWindowViewModel = mainWindowViewModel;
        //    this.IsExpanded = false;
        //    this.Add(Dummy);

        //    this.ArchiveItem = new ArchiveItem()
        //    {
        //        FullName = source.Key,
        //        FileName = source.Key,
        //        FileSize = source.Size,
        //        TimeStamp = source.CreatedTime?.Ticks ?? 0
        //    };
        //    this.FullName = this.ArchiveItem.FullName;
        //    this.Name = this.ArchiveItem.FileName;
        //    this.LastUpdateDate = this.ArchiveItem.TimeStamp.ToString();
        //    this.Type = "";
        //    this.Size = this.ArchiveItem.FileSize.ToString();
        //    this.Parent = parent;
        //    this.archiveParent = archiveParent;

        //    var extension = Path.GetExtension(this.ArchiveItem.FileName).ToLower();
        //    this.FileType = ApplicationSettings.Instance.FileExtensions.Contains(extension)
        //        ? FileInfoType.File
        //        : FileInfoType.Unknown;
        //}

        public FileInfoViewModel(ArchiveItem source, MainWindowViewModel mainWindowViewModel, FileInfoViewModel parent)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            this.IsExpanded = false;
            this.Add(Dummy);

            //this.ArchiveItem = new ArchiveItem()
            //{
            //    FullName = source.FileInfo.FileName,
            //    FileName = source.FileInfo.FileName,
            //    FileSize = source.FileInfo.FileSize,
            //    TimeStamp = source.FileInfo.TimeStamp
            //};
            this.ArchiveItem = source;
            this.FullName = this.ArchiveItem.FileName;
            this.Name = this.ArchiveItem.FileName;
            this.LastUpdateDate = this.ArchiveItem.TimeStamp.ToString();
            this.Type = "";
            this.Size = this.ArchiveItem.FileSize.ToString();
            this.Parent = parent;

            switch (source.Type)
            {
                case ArchiveItem.ArchiveItemType.Directory:
                    this.FileType = FileInfoType.ArchivedDirectory;
                    break;

                case ArchiveItem.ArchiveItemType.File:
                    var extension = Path.GetExtension(this.ArchiveItem.FileName).ToLower();

                    this.FileType = ApplicationSettings.Instance.ArchiveExtensions.Contains(extension)
                        ? FileInfoType.ArchivedDirectory
                        : ApplicationSettings.Instance.FileExtensions.Contains(extension)
                            ? FileInfoType.File
                            : FileInfoType.Unknown;
                    break;
            }

            this.archiveChildren = source.Children
                .Select(c => new FileInfoViewModel(c, mainWindowViewModel, this))
                .ToArray();
        }

        /// <summary>
        /// 指定した要素を子要素として追加します。
        /// </summary>
        /// <param name="child"></param>
        public void Add(FileInfoViewModel child)
        {
            if (null == Children) Children = new ObservableCollection<FileInfoViewModel>();
            child.Parent = this;
            Children.Add(child);
        }

        private void LoadChildren()
        {
            this.Children?.Clear();

            if (this.FileSystemInfo == null) return;

            if (!(this.FileSystemInfo is DirectoryInfo directory)) return;

            directory.GetDirectories()
                .Where(d => !d.Attributes.HasFlag(FileAttributes.System))
                .ForEach(d =>
                {
                    this.Add(new FileInfoViewModel(d, this.mainWindowViewModel));
                });
        }

        /// <summary>
        /// このファイルへアクセスするためのStream を取得します。
        /// </summary>
        /// <returns></returns>
        public Stream GetFileStream()
        {
            if (this.FileSystemInfo == null)
            {
                return this.ArchiveItem.CreateStream();

                //return Archive.GetFileStream(this.archiveParent, this.ArchiveItem);

                //switch (Path.GetExtension(this._archiveParent.FullName).ToLower())
                //{
                //    case ".zip":
                //        using (var a = ZipFile.OpenRead(this._archiveParent.FullName))
                //        {
                //            var entry = a.GetEntry(this.fileInfo.FileName);
                //            if (entry == null)
                //            {
                //                return null;
                //            }
                //            else
                //            {
                //                var ms = new MemoryStream();
                //                using (var fs = entry.Open())
                //                {
                //                    fs.CopyTo(ms);
                //                }

                //                ms.Seek(0, SeekOrigin.Begin);

                //                return ms;
                //            }
                //        }

                //    default:
                //        var bytes = Spi.SpiManager.GetFile(this._archiveParent.FullName, this.fileInfo.Position);

                //        return new MemoryStream(bytes);
                //        //return SpiManager.GetPictureStream(fileInfo.FileName, bytes);
                //}
            }
            else
            {
                //return Spi.SpiManager.GetPictureStream(this.fileSystemInfo.FullName);

                var ms = new MemoryStream();
                using (var fs = File.OpenRead(this.FileSystemInfo.FullName))
                {
                    fs.CopyTo(ms);
                }
                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
        }

        public IEnumerable<FileInfoViewModel> GetAncestors()
        {
            var ancestors = new List<FileInfoViewModel>();
            var parent = this.Parent;
            while (parent != null)
            {
                ancestors.Add(parent);
                parent = parent.Parent;
            }

            ancestors.Reverse();

            return ancestors;
        }

        /// <summary>
        /// ファイル一覧に使用するための子要素を取得します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfoViewModel> GetChildrenForList()
        {
            switch (this.FileType)
            {
                case FileInfoType.Archive:
                    //switch (Path.GetExtension(this.Name).ToLower())
                    //{
                    //    case ".zip":
                    //        //ZIP書庫を開く
                    //        using (var a = ZipFile.OpenRead(this.FullName))
                    //        //Encodingを指定する場合は、次のようにする
                    //        //using (ZipArchive a = ZipFile.Open(@"C:\test\1.zip",
                    //        //    ZipArchiveMode.Read,
                    //        //    System.Text.Encoding.GetEncoding("shift_jis")))
                    //        {
                    //            //書庫内のファイルとディレクトリを列挙する
                    //            //foreach (var e in a.Entries)
                    //            //{
                    //            //    Console.WriteLine("名前       : {0}", e.Name);
                    //            //    //ディレクトリ付きのファイル名
                    //            //    Console.WriteLine("フルパス   : {0}", e.FullName);
                    //            //    Console.WriteLine("サイズ     : {0}", e.Length);
                    //            //    Console.WriteLine("圧縮サイズ : {0}", e.CompressedLength);
                    //            //    Console.WriteLine("更新日時   : {0}", e.LastWriteTime);
                    //            //}

                    //            return a.Entries
                    //                .Select(e => new FileInfoViewModel(e, this.mainWindowViewModel, this, this))
                    //                .ToArray();
                    //        }

                    //    default:
                    //        var fileInfoList = Spi.SpiManager.GetArchiveInfo(this.FullName).ToArray();
                    //        return this.GetArchiveItemList(fileInfoList)
                    //            .Select(a => new FileInfoViewModel(a, this.mainWindowViewModel, this, this));
                    //}

                    return Archive.GetChildrenForList(this.FullName, this.mainWindowViewModel, this);

                case FileInfoType.Directory:
                    try
                    {
                        return new DirectoryInfo(this.FullName)
                            .EnumerateFileSystemInfos()
                            .Where(file => !file.Attributes.HasFlag(FileAttributes.System))
                            .Select(file => new FileInfoViewModel(file, this.mainWindowViewModel));
                    }
                    catch (IOException)
                    {
                        return new FileInfoViewModel[0];
                    }

                case FileInfoType.ArchivedDirectory:
                    return this.archiveChildren;

                default:
                    return new FileInfoViewModel[0];
            }
        }

        ///// <summary>
        ///// アーカイブ内のファイルの一覧を取得します。
        ///// </summary>
        ///// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        ///// <param name="parent">source の親要素</param>
        ///// <returns></returns>
        //private IEnumerable<ArchiveItem> GetArchiveItemList(IEnumerable<Spi.FileInfo> source)
        //{
        //    var sourceArray = source.ToArray();
        //    var archivedDirectoryList = sourceArray
        //        .GroupBy(f => f.Path.Split(Path.DirectorySeparatorChar)[0]);

        //    return archivedDirectoryList
        //        .SelectMany(g =>
        //        {
        //            // アーカイブファイル内で直下に置かれているファイル
        //            if (string.IsNullOrEmpty(g.Key))
        //            {
        //                return g.Select(f => new ArchiveItem()
        //                {
        //                    Type = ArchiveItem.ArchiveItemType.File,
        //                    Name = f.FileName,
        //                    FileInfo = f,
        //                });
        //            }

        //            var children = this.GetArchiveItemList(g.Select(f =>
        //            {
        //                f.Path = f.Path.Replace(g.Key + Path.DirectorySeparatorChar, "");
        //                return f;
        //            }))
        //            .ToArray();

        //            var fileInfo = new Spi.FileInfo()
        //            {
        //                FileName = g.Key,
        //                Path = g.Key,
        //                CompSize = 0,
        //                CRC = 0,
        //                FileSize = 0,
        //                Method = "",
        //                Position = 0,
        //                TimeStamp = 0,
        //            };

        //            var archivedDirectory = new ArchiveItem()
        //            {
        //                Type = ArchiveItem.ArchiveItemType.Directory,
        //                Name = g.Key,
        //                FileInfo = fileInfo,
        //                Children = children,
        //            };

        //            return new[] { archivedDirectory };
        //        });
        //}
    }
}
