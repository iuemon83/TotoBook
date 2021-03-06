﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TotoBook.ViewModel
{
    /// <summary>
    /// メインウィンドウのビューモデル
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs> ScrollFileListToSelectedItem;

        /// <summary>
        /// 自動ページ送り用のタイマー
        /// </summary>
        private readonly AutoPagerTimer autoPagerTimer;

        /// <summary>
        /// 現在表示中のディレクトリ
        /// </summary>
        private FileInfoViewModel currentDirectory = null;

        /// <summary>
        /// 表示中の右側の画像ファイル
        /// </summary>
        private FileInfoViewModel rightFile;

        /// <summary>
        /// 表示中の左側の画像ファイル
        /// </summary>
        private FileInfoViewModel leftFile;

        /// <summary>
        /// 履歴管理
        /// </summary>
        private readonly BrowseHistory historyService = new BrowseHistory();

        private SortDescription directorySortDescription = new SortDescription("Name", ListSortDirection.Ascending);

        private SortDescription archiveSortDescription = new SortDescription("Name", ListSortDirection.Ascending);

        /// <summary>
        /// ファイルツリーのルート要素
        /// </summary>
        public ObservableCollection<FileInfoViewModel> FileTreeRoot { get; set; } = new ObservableCollection<FileInfoViewModel>();

        /// <summary>
        /// ファイルリストの要素
        /// </summary>
        private ObservableCollection<FileInfoViewModel> _fileInfoList = new ObservableCollection<FileInfoViewModel>();
        public ObservableCollection<FileInfoViewModel> FileInfoList
        {
            get { return this._fileInfoList; }
            set
            {
                _fileInfoList = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 選択中のファイルリスト要素
        /// </summary>
        private FileInfoViewModel selectedFileInfo;

        /// <summary>
        /// 選択中のファイルリスト要素
        /// </summary>
        public FileInfoViewModel SelectedFileInfo
        {
            get { return this.selectedFileInfo; }
            set
            {
                if (value != this.selectedFileInfo)
                {
                    this.selectedFileInfo = value;
                    this.RaisePropertyChanged(nameof(this.SelectedFileInfo));
                    this.RaisePropertyChanged(nameof(this.CurrentFilePath));
                }
            }
        }

        /// <summary>
        /// 右画像
        /// </summary>
        private ImageSource rightImageSource;
        public ImageSource RightImageSource
        {
            get { return this.rightImageSource; }
            set
            {
                this.rightImageSource = value;
                this.RaisePropertyChanged(nameof(this.RightImageSource));
            }
        }

        /// <summary>
        /// 左画像
        /// </summary>
        private ImageSource leftImageSource;
        public ImageSource LeftImageSource
        {
            get { return this.leftImageSource; }
            set
            {
                this.leftImageSource = value;
                this.RaisePropertyChanged(nameof(this.LeftImageSource));
            }
        }

        /// <summary>
        /// 右画像のサイズ
        /// </summary>
        private Rect rightImageRect;
        public Rect RightImageRect
        {
            get { return this.rightImageRect; }
            set
            {
                this.rightImageRect = value;
                this.RaisePropertyChanged(nameof(this.RightImageRect));
            }
        }

        /// <summary>
        /// 左画像のサイズ
        /// </summary>
        private Rect leftImageRect;
        public Rect LeftImageRect
        {
            get { return this.leftImageRect; }
            set
            {
                this.leftImageRect = value;
                this.RaisePropertyChanged(nameof(this.LeftImageRect));
            }
        }

        /// <summary>
        /// 合計ページ数
        /// </summary>
        private int totalPageCount;
        public int TotalPageCount
        {
            get { return this.totalPageCount; }
            set
            {
                this.totalPageCount = value;
                this.RaisePropertyChanged(nameof(this.TotalPageCount));
            }
        }

        /// <summary>
        /// 現在表示中のページ番号
        /// </summary>
        private string currentPageNumber;
        public string CurrentPageNumber
        {
            get { return this.currentPageNumber; }
            set
            {
                this.currentPageNumber = value;
                this.RaisePropertyChanged(nameof(this.CurrentPageNumber));
            }
        }

        /// <summary>
        /// 自動ページ送り機能が有効な場合はTrue、そうでなければFalse を取得します。
        /// </summary>
        private bool enableAutoPager;
        public bool IsEnabledAutoPager
        {
            get { return this.enableAutoPager; }
            set
            {
                this.enableAutoPager = value;
                this.RaisePropertyChanged(nameof(this.IsEnabledAutoPager));
            }
        }

        public string CurrentFilePath => this.SelectedFileInfo?.FullName ?? "";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            ApplicationSettings.LoadSettingsFromFile();

            this.autoPagerTimer = new AutoPagerTimer(() => this.ToNextScene());

            Spi.SpiManager.Load(ApplicationSettings.Instance.PluginDirectoryPath);

            this.RightImageRect = default;
            this.LeftImageRect = default;

            this.GetRoots()
                .ForEach(root =>
                {
                    this.FileTreeRoot.Add(root);
                });
        }

        public override void Cleanup()
        {
            base.Cleanup();

            try
            {
                this.currentDirectory?.Cleanup();
            }
            catch { }

            try
            {
                this.rightFile?.Cleanup();
            }
            catch { }

            try
            {
                this.leftFile.Cleanup();
            }
            catch { }

            foreach (var fileInfo in this.FileInfoList)
            {
                try
                {
                    fileInfo?.Cleanup();
                }
                catch { }
            }
        }

        /// <summary>
        /// 指定したインデックスの次のページのファイルインデックスを取得します。
        /// </summary>
        /// <param name="index">基準となるファイルインデックス</param>
        /// <returns></returns>
        public int GetNextPageFileIndex(int index)
        {
            for (var i = index + 1; i < this.FileInfoList.Count; i++)
            {
                if (this.FileInfoList[i].FileType != FileInfoViewModel.FileInfoType.File) continue;
                return i;
            }

            for (var i = 0; i < index; i++)
            {
                if (this.FileInfoList[i].FileType != FileInfoViewModel.FileInfoType.File) continue;
                return i;
            }

            return -1;
        }

        /// <summary>
        /// 指定したインデックスの前のページのファイルインデックスを取得します。
        /// </summary>
        /// <param name="index">基準となるファイルインデックス</param>
        /// <returns></returns>
        public int GetPrevPageFileIndex(int index)
        {
            for (var i = index - 1; i >= 0; i--)
            {
                if (this.FileInfoList[i].FileType != FileInfoViewModel.FileInfoType.File) continue;
                return i;
            }

            for (var i = this.FileInfoList.Count - 1; i > index; i--)
            {
                if (this.FileInfoList[i].FileType != FileInfoViewModel.FileInfoType.File) continue;
                return i;
            }

            return -1;
        }

        /// <summary>
        /// 指定したファイルの次の画像ファイルを取得します。
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private FileInfoViewModel GetNextFile(FileInfoViewModel file)
        {
            var index = this.FileInfoList.IndexOf(file);
            var nextFileIndex = this.GetNextPageFileIndex(index);
            return nextFileIndex == -1
                ? null
                : this.FileInfoList[nextFileIndex];
        }

        /// <summary>
        /// 指定したファイルの前のファイルを取得します。
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private FileInfoViewModel GetPrevFile(FileInfoViewModel file)
        {
            var index = this.FileInfoList.IndexOf(file);
            var prevFileIndex = this.GetPrevPageFileIndex(index);
            return prevFileIndex == -1
                ? null
                : this.FileInfoList[prevFileIndex];
        }

        /// <summary>
        /// 次のページへ移動します。
        /// </summary>
        public void ToNextPage()
        {
            var rightFile = this.GetNextFile(this.rightFile);
            if (rightFile == null) return;

            this.Navigate(rightFile);
        }

        /// <summary>
        /// 次のシーンへ移動します。
        /// </summary>
        public void ToNextScene()
        {
            var rightFile = this.GetNextFile(this.leftFile ?? this.rightFile);
            if (rightFile == null) return;

            this.Navigate(rightFile);
        }

        /// <summary>
        /// 前のページへ移動します。
        /// </summary>
        public void ToPrevPage()
        {
            var rightFile = this.GetPrevFile(this.rightFile);
            if (rightFile == null) return;

            this.Navigate(rightFile);
        }

        /// <summary>
        /// 前のシーンへ移動します。
        /// </summary>
        public void ToPrevScene()
        {
            var prevFile = this.GetPrevFile(this.rightFile);
            if (prevFile == null) return;

            this.NavigateToPhotoFileReverse(prevFile);
        }

        /// <summary>
        /// 一つ上の階層へ移動します。
        /// </summary>
        public void NavigateToParent()
        {
            if (this.currentDirectory == null) return;

            var selectItem = this.currentDirectory;
            var parent = this.currentDirectory.Parent;

            if (parent == null)
            {
                this.NavigateRoots();
            }
            else
            {
                this.SelectFileListItemCommand(parent);
            }

            this.SelectedFileInfo = this.FileInfoList.FirstOrDefault(f => f.Name == selectItem.Name);
        }

        /// <summary>
        /// ファイルツリーのルートの一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfoViewModel> GetRoots()
        {
            return DriveInfo.GetDrives().Select(d => new DirectoryInfo(d.Name))
                .Concat(new[]
                {
                    new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)),
                    new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                    new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
                    new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                    new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)),
                })
                .Select(d => new FileInfoViewModel(d, this));
        }

        /// <summary>
        /// ファイルツリーのルートへ移動します。
        /// </summary>
        public void NavigateRoots()
        {
            this.RefreshFileList(this.GetRoots());
        }

        /// <summary>
        /// 指定したファイルに対応するファイルツリーのノードを選択状態にします。
        /// </summary>
        /// <param name="fileInfo">対象のファイル</param>
        /// <returns></returns>
        public bool SelectTreeNode(FileInfoViewModel fileInfo)
        {
            if (fileInfo.FileType != FileInfoViewModel.FileInfoType.Directory) return false;

            var candidates = this.FileTreeRoot.AsEnumerable();
            fileInfo.GetAncestors().ForEach(f =>
            {
                var node = candidates?.FirstOrDefault(t => t.Name.ToLower() == f.Name.ToLower());
                if (node == null) return;

                node.IsExpanded = true;
                candidates = node.Children;
            });

            var targetNode = candidates?.FirstOrDefault(t => t.Name.ToLower() == fileInfo.Name.ToLower());
            if (targetNode != null)
            {
                targetNode.IsSelected = false;
                targetNode.IsSelected = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ディレクトリを指定した場合は、そこに移動します。
        /// ファイルを指定した場合は、そのファイルを表示します。
        /// </summary>
        /// <param name="nextFileInfo">移動先のディレクトリまたはファイル</param>
        public void Navigate(FileInfoViewModel nextFileInfo)
        {
            switch (nextFileInfo.FileType)
            {
                case FileInfoViewModel.FileInfoType.File:
                    this.NavigateToPhotoFile(nextFileInfo);
                    break;

                case FileInfoViewModel.FileInfoType.Archive:
                case FileInfoViewModel.FileInfoType.ArchivedDirectory:
                    this.NavigateToArchiveFile(nextFileInfo);
                    break;

                case FileInfoViewModel.FileInfoType.Directory:
                    this.NavigateToDirectory(nextFileInfo);
                    break;

                default:
                    // なにもしない
                    break;
            }
        }

        /// <summary>
        /// 指定したアーカイブファイルへ移動します。
        /// </summary>
        /// <param name="dist">対象のアーカイブファイル</param>
        private void NavigateToArchiveFile(FileInfoViewModel dist)
        {
            if (dist.FileType != FileInfoViewModel.FileInfoType.Archive
                && dist.FileType != FileInfoViewModel.FileInfoType.ArchivedDirectory) return;

            this.SetCurrentDirectory(dist);
        }

        /// <summary>
        /// 指定したディレクトリへ移動します。
        /// </summary>
        /// <param name="dist">対象のディレクトリ</param>
        private void NavigateToDirectory(FileInfoViewModel dist)
        {
            if (dist.FileType != FileInfoViewModel.FileInfoType.Directory) return;

            this.SetCurrentDirectory(dist);
        }

        private void SetCurrentDirectory(FileInfoViewModel dist)
        {
            this.autoPagerTimer.Stop();
            this.IsEnabledAutoPager = false;

            if (dist != null
                && this.currentDirectory != null
                && !dist.FullName.Contains(this.currentDirectory.FullName))
            {
                this.currentDirectory?.Cleanup();
            }
            this.currentDirectory = dist;

            var children = dist.GetChildrenForList();
            this.RefreshFileList(children);
        }

        ///// <summary>
        ///// 指定した画像ファイルへ移動します。
        ///// </summary>
        ///// <param name="dist">対象の画像ファイル</param>
        private void NavigateToPhotoFile(FileInfoViewModel dist)
        {
            if (dist.FileType != FileInfoViewModel.FileInfoType.File) return;

            this.autoPagerTimer.Reset();

            using (var stream1 = dist.GetFileStream())
            {
                var bitmapImage1 = new BitmapImage();
                bitmapImage1.BeginInit();
                bitmapImage1.StreamSource = stream1;
                bitmapImage1.EndInit();

                if (bitmapImage1.Width < bitmapImage1.Height)
                {
                    var dist2 = this.GetNextFile(dist);

                    using var stream2 = dist2.GetFileStream();
                    var bitmapImage2 = new BitmapImage();
                    bitmapImage2.BeginInit();
                    bitmapImage2.StreamSource = stream2;
                    bitmapImage2.EndInit();

                    if (bitmapImage2.Width < bitmapImage2.Height)
                    {
                        this.DisplayImages(dist, bitmapImage1, dist2, bitmapImage2);
                    }
                    else
                    {
                        this.DisplayImages(dist, bitmapImage1, null, null);
                    }
                }
                else
                {
                    this.DisplayImages(dist, bitmapImage1, null, null);
                }
            }

            this.ScrollFileListToSelectedItem?.Invoke(this, EventArgs.Empty);
        }

        ///// <summary>
        ///// 指定した画像ファイルへ逆順で移動します。
        ///// </summary>
        ///// <param name="dist">対象の画像ファイル</param>
        private void NavigateToPhotoFileReverse(FileInfoViewModel dist)
        {
            if (dist.FileType != FileInfoViewModel.FileInfoType.File) return;

            this.autoPagerTimer.Reset();

            using (var stream1 = dist.GetFileStream())
            {
                var bitmapImage1 = new BitmapImage();
                bitmapImage1.BeginInit();
                bitmapImage1.StreamSource = stream1;
                bitmapImage1.EndInit();

                if (bitmapImage1.Width < bitmapImage1.Height)
                {
                    var dist2 = this.GetPrevFile(dist);

                    using var stream2 = dist2.GetFileStream();
                    var bitmapImage2 = new BitmapImage();
                    bitmapImage2.BeginInit();
                    bitmapImage2.StreamSource = stream2;
                    bitmapImage2.EndInit();

                    if (bitmapImage2.Width < bitmapImage2.Height)
                    {
                        this.DisplayImages(dist2, bitmapImage2, dist, bitmapImage1);
                    }
                    else
                    {
                        this.DisplayImages(dist, bitmapImage1, null, null);
                    }
                }
                else
                {
                    this.DisplayImages(dist, bitmapImage1, null, null);
                }
            }

            this.ScrollFileListToSelectedItem?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 指定した左右の画像を画面に表示します。
        /// </summary>
        /// <param name="rightFile"></param>
        /// <param name="rightImage"></param>
        /// <param name="leftFile"></param>
        /// <param name="leftImage"></param>
        private void DisplayImages(FileInfoViewModel rightFile, BitmapImage rightImage, FileInfoViewModel leftFile, BitmapImage leftImage)
        {
            if (rightImage == null) throw new ArgumentNullException(nameof(rightImage));

            this.FileInfoList.ForEach(f => f.IsDisplayed = false);

            if (rightFile != null) rightFile.IsDisplayed = true;
            if (leftFile != null) leftFile.IsDisplayed = true;

            this.rightFile = rightFile;
            this.leftFile = leftFile;

            var rectHeight = leftImage == null
                ? rightImage.Height
                : Math.Min(rightImage.Height, leftImage.Height);

            if (leftImage == null)
            {
                this.leftImageRect = default;
            }
            else
            {
                var leftRectWidth = leftImage.Width * rectHeight / leftImage.Height;
                this.LeftImageRect = new Rect(0, 0, leftRectWidth, rectHeight);
            }

            this.LeftImageSource = leftImage;

            var rightRectWidth = rightImage.Width * rectHeight / rightImage.Height;
            this.RightImageRect = new Rect(this.leftImageRect.Width, 0, rightRectWidth, rectHeight);
            this.RightImageSource = rightImage;

            this.SelectedFileInfo = this.FileInfoList.FirstOrDefault(f => f.Name == rightFile.Name);
        }

        /// <summary>
        /// 指定したファイルで、ファイルの一覧を更新します。
        /// </summary>
        /// <param name="fileInfoList"></param>
        private void RefreshFileList(IEnumerable<FileInfoViewModel> fileInfoList)
        {
            var fileInfoArray = fileInfoList
                .Where(f => f.FileType != FileInfoViewModel.FileInfoType.Unknown)
                .ToArray();

            this.FileInfoList.Clear();
            this.RightImageSource = null;
            this.LeftImageSource = null;

            this.CurrentPageNumber = "0";
            this.TotalPageCount = fileInfoArray
                .Count(fileinfo => fileinfo.FileType == FileInfoViewModel.FileInfoType.File);

            fileInfoArray
                .ForEach(fileInfo =>
                {
                    this.FileInfoList.Add(fileInfo);
                });

            var currentSort = this.currentDirectory?.FileType switch
            {
                FileInfoViewModel.FileInfoType.Archive => this.archiveSortDescription,
                FileInfoViewModel.FileInfoType.ArchivedDirectory => this.archiveSortDescription,
                FileInfoViewModel.FileInfoType.Directory => this.directorySortDescription,
                _ => this.directorySortDescription
            };

            this.ExecuteSort(currentSort);
            this.SelectedFileInfo = this.FileInfoList.FirstOrDefault();
        }

        /// <summary>
        /// 前の履歴へ戻るコマンド
        /// </summary>
        public void BackHistoryCommand()
        {
            if (!this.historyService.EnableBack) return;

            this.Navigate(this.historyService.Back());
        }

        /// <summary>
        ///次の履歴へ移動するコマンド
        /// </summary>
        public void NextHistoryCommand()
        {
            if (!this.historyService.EnableNext) return;

            this.Navigate(this.historyService.Next());
        }

        /// <summary>
        /// ファイルリストの項目選択コマンド
        /// </summary>
        /// <param name="file"></param>
        public void SelectFileListItemCommand(FileInfoViewModel file)
        {
            var isSelected = this.SelectTreeNode(file);

            if (!isSelected) this.Navigate(file);
        }

        /// <summary>
        /// ファイルツリーの項目選択コマンド
        /// </summary>
        /// <param name="file"></param>
        public void SelectFileTreeItemCommand(FileInfoViewModel file)
        {
            this.Navigate(file);
            this.historyService.Visit(file);
        }

        public void DeleteFileListItemCommand(FileInfoViewModel file, Func<bool> confirm)
        {
            var deleteIndex = this.FileInfoList.IndexOf(file);
            var nextSelectIndex = deleteIndex == this.FileInfoList.Count - 1
                ? deleteIndex - 1
                : deleteIndex;

            if (Directory.Exists(file.FullName))
            {
                // ディレクトリの場合
                if (!confirm()) return;

                Directory.Delete(file.FullName, true);
                this.FileInfoList.Remove(file);
            }

            if (File.Exists(file.FullName))
            {
                // ファイルの場合
                if (!confirm()) return;

                File.Delete(file.FullName);
                this.FileInfoList.Remove(file);
            }

            if (this.FileInfoList.IsEmpty()) return;

            this.SelectedFileInfo = this.FileInfoList[nextSelectIndex];
        }

        /// <summary>
        /// 自動ページ送りの有効、無効を切り替えます。
        /// </summary>
        public void ToggleAutoPager()
        {
            if (this.SelectedFileInfo.FileType == FileInfoViewModel.FileInfoType.File)
            {
                this.Navigate(this.SelectedFileInfo);
            }

            this.autoPagerTimer.Toggle();
            this.IsEnabledAutoPager = this.autoPagerTimer.IsEnabled;
        }

        public void ExecuteSort(SortDescription sortDescription)
            => this.ExecuteSort(sortDescription.PropertyName, sortDescription.Direction);

        public void ExecuteSort(string propertyName, ListSortDirection direction)
        {

            var isAsc = direction == ListSortDirection.Ascending;

            var sortedQuery = propertyName switch
            {
                "Name" => isAsc
                    ? FileInfoList.OrderBy(file => file.Name, new FileNameComparer())
                    : FileInfoList.OrderByDescending(file => file.Name, new FileNameComparer()),
                "Size" => isAsc
                    ? FileInfoList.OrderBy(file => file.Size)
                    : FileInfoList.OrderByDescending(file => file.Size),
                "Type" => isAsc
                    ? FileInfoList.OrderBy(file => file.Type)
                    : FileInfoList.OrderByDescending(file => file.Type),
                "LastUpdateDate" => isAsc
                    ? FileInfoList.OrderBy(file => file.LastUpdateDate)
                    : FileInfoList.OrderByDescending(file => file.LastUpdateDate),
                _ => throw new ArgumentException(),
            };
            FileInfoList = new ObservableCollection<FileInfoViewModel>(sortedQuery);

            ////ソートアイコン表示をViewに依頼
            //Messenger.Instance.Send<SortMessage, MainWindow>(new SortMessage(propertyName, direction));

            //ソート情報保持
            switch (this.currentDirectory?.FileType)
            {
                case FileInfoViewModel.FileInfoType.Archive:
                case FileInfoViewModel.FileInfoType.ArchivedDirectory:
                    this.archiveSortDescription = new SortDescription(propertyName, direction);
                    break;

                case FileInfoViewModel.FileInfoType.Directory:
                    this.directorySortDescription = new SortDescription(propertyName, direction);
                    break;
            }
        }

        public void RefreshFiles()
        {
            var children = this.currentDirectory.GetChildrenForList();
            this.RefreshFileList(children);
        }
    }
}
