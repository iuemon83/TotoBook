﻿using GalaSoft.MvvmLight;
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

                    using (var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon))
                    {
                        var iconSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        IconPathCache.Add(extension, iconSource);

                        return iconSource;
                    }
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

                using (var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon))
                {
                    var iconSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    if (directoryInfo.Attributes == FileAttributes.Directory) IconPathCache.Add("", iconSource);
                    return iconSource;
                }
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
        private MainWindowViewModel mainWindowViewModel;

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
                    this.icon = GetIconPath(this.fileSystemInfo);
                }
                return this.icon;
            }
            set { this.icon = value; }
        }

        private FileSystemInfo fileSystemInfo { get; set; }
        private Spi.FileInfo fileInfo { get; set; }

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

        private FileInfoViewModel _archiveParent = null;
        private FileInfoViewModel[] _archiveChildren;

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

            this.fileSystemInfo = source;
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
                case FileInfo f:
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

        public FileInfoViewModel(Spi.FileInfo source, MainWindowViewModel mainWindowViewModel, FileInfoViewModel parent, FileInfoViewModel archiveParent)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            this.IsExpanded = false;
            this.Add(Dummy);

            this.fileInfo = source;
            this.FullName = source.FileName;
            this.Name = source.FileName;
            this.LastUpdateDate = source.TimeStamp.ToString();
            this.Type = "";
            this.Size = source.FileSize.ToString();
            var extension = Path.GetExtension(source.FileName).ToLower();

            this.FileType = (string.IsNullOrEmpty(extension) || ApplicationSettings.Instance.ArchiveExtensions.Contains(extension))
                ? FileInfoType.ArchivedDirectory
                : ApplicationSettings.Instance.FileExtensions.Contains(extension)
                    ? FileInfoType.File
                    : FileInfoType.Unknown;

            this.Parent = parent;

            this._archiveParent = archiveParent;
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

            if (this.fileSystemInfo == null) return;

            var directory = this.fileSystemInfo as DirectoryInfo;
            if (directory == null) return;

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
            if (this.fileSystemInfo != null)
            {
                return Spi.SpiManager.GetPictureStream(this.fileSystemInfo.FullName);
            }
            else
            {
                return Spi.SpiManager.GetFile(this._archiveParent.FullName, this.fileInfo);
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
                    var fileInfoList = Spi.SpiManager.GetArchiveInfo(this.FullName).ToArray();
                    return this.GetArchivedFileList(fileInfoList, this);

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
                    return this._archiveChildren;

                default:
                    return new FileInfoViewModel[0];
            }
        }

        /// <summary>
        /// アーカイブ内のファイルの一覧を取得します。
        /// </summary>
        /// <param name="source">アーカイブファイルを表すFileInfo インスタンス</param>
        /// <param name="parent">source の親要素</param>
        /// <returns></returns>
        private IEnumerable<FileInfoViewModel> GetArchivedFileList(IEnumerable<Spi.FileInfo> source, FileInfoViewModel parent)
        {
            var sourceArray = source.ToArray();
            var nestedArchiveList = sourceArray
                .GroupBy(f => f.Path.Split(Path.DirectorySeparatorChar)[0]);

            return nestedArchiveList
                .SelectMany(g =>
                {
                    // アーカイブファイル内で直下に置かれているファイル
                    if (string.IsNullOrEmpty(g.Key)) return g.Select(f => new FileInfoViewModel(f, this.mainWindowViewModel, parent, this));

                    // 以下はアーカイブファイル内で入れ子になっているもの
                    var archiveFileInfo = new Spi.FileInfo()
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

                    var archive = new FileInfoViewModel(archiveFileInfo, this.mainWindowViewModel, parent, this);

                    var children = this.GetArchivedFileList(
                        g.Select(f =>
                        {
                            f.Path = f.Path.Replace(g.Key + Path.DirectorySeparatorChar, "");
                            return f;
                        }),
                        archive);

                    archive._archiveChildren = children.ToArray();

                    return new[] { archive };
                });
        }
    }
}
