using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TotoBook.View;
using TotoBook.ViewModel;

namespace TotoBook
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel = new MainWindowViewModel();
            this.ViewModel.ScrollFileListToSelectedItem += (_, __) => this.ScrollFileListToSelectedItem();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// 左ペインの幅
        /// </summary>
        private GridLength fileListWidth;

        /// <summary>
        /// ビューモデル
        /// </summary>
        private readonly MainWindowViewModel ViewModel;

        /// <summary>
        /// ロード時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.fileListWidth = this.ColumnDefinition1.Width;

            this.ViewModel.NavigateRoots();
            this.SetFocusFileList();
        }

        /// <summary>
        /// ファイル一覧のダブルクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dataGridRow = (DataGridRow)sender;
            var fileInfoViewModel = (FileInfoViewModel)dataGridRow.Item;
            this.SelectFileListItem(fileInfoViewModel);
        }

        /// <summary>
        /// ファイル一覧のキーダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        var dataGridRow = (DataGridRow)sender;
                        var fileInfoViewModel = (FileInfoViewModel)dataGridRow.Item;
                        this.SelectFileListItem(fileInfoViewModel);
                        e.Handled = true;
                        break;
                    }

                case Key.Delete:
                    {
                        var dataGridRow = (DataGridRow)sender;
                        var fileInfoViewModel = (FileInfoViewModel)dataGridRow.Item;
                        this.DeleteFileListItem(fileInfoViewModel);
                        e.Handled = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// ファイル一覧の項目を選択します。
        /// </summary>
        /// <param name="fileInfoViewModel"></param>
        private void SelectFileListItem(FileInfoViewModel fileInfoViewModel)
        {
            this.ViewModel.SelectFileListItemCommand(fileInfoViewModel);

            this.ScrollFileListToSelectedItem();

            if (fileInfoViewModel.FileType == FileInfoViewModel.FileInfoType.File)
            {
                this.SetFocusImage();
            }
            else
            {
                this.SetFocusFileList();
            }
        }

        /// <summary>
        /// ファイル一覧で選択中のファイルを削除します。
        /// </summary>
        /// <param name="fileInfoViewModel"></param>
        private void DeleteFileListItem(FileInfoViewModel fileInfoViewModel)
        {
            this.ViewModel.DeleteFileListItemCommand(fileInfoViewModel, () =>
                MessageBox.Show($"「{fileInfoViewModel.Name}」を削除します。よろしいですか？", "ファイルの削除", MessageBoxButton.YesNo) == MessageBoxResult.Yes
            );



            this.SetFocusFileList();
        }

        /// <summary>
        /// 画像表示ペインのマウスホイールイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageContainer_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                this.ViewModel.ToPrevScene();
            }
            else
            {
                this.ViewModel.ToNextScene();
            }
        }

        /// <summary>
        /// 画像表示ペインのキーダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageContainer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.Key)
            {
                case System.Windows.Input.Key.Right:
                    this.ViewModel.ToPrevPage();
                    break;

                case System.Windows.Input.Key.Left:
                    this.ViewModel.ToNextPage();
                    break;

                case System.Windows.Input.Key.Up:
                    this.ViewModel.ToPrevScene();
                    break;

                case System.Windows.Input.Key.Down:
                    this.ViewModel.ToNextScene();
                    break;

                default:
                    e.Handled = false;
                    break;
            }
        }

        /// <summary>
        /// 画像表示ペインのマウスクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageContainer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ImageContainer.Focus();

            if (e.ClickCount == 2)
            {
                this.ToggleFullScreen();
            }
        }

        /// <summary>
        /// 上ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToParent();
        }

        /// <summary>
        /// ウィンドウのキーダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.Key)
            {
                case System.Windows.Input.Key.Back:
                    this.NavigateToParent();
                    break;

                case Key.F11:
                    this.ToggleFullScreen();
                    break;

                default:
                    e.Handled = false;
                    break;
            }
        }

        /// <summary>
        /// 一つ上の階層に移動します。
        /// </summary>
        private void NavigateToParent()
        {
            var isFocusFileTree = this.FileTree.IsFocused;
            this.ViewModel.NavigateToParent();
            this.ChangeToNormalScreen();
            this.ScrollFileListToSelectedItem();
            if (isFocusFileTree)
            {
                this.FileTree.Focus();
            }
            else
            {
                this.SetFocusFileList();
            }
        }

        /// <summary>
        /// ファイル一覧を選択項目までスクロールさせます。
        /// </summary>
        private void ScrollFileListToSelectedItem()
        {
            this.FileList.UpdateLayout();

            if (this.FileList.Items.IsEmpty) return;

            var selectedItem = this.FileList.SelectedItem;
            if (selectedItem != null) this.FileList.ScrollIntoView(selectedItem);
        }

        /// <summary>
        /// 画像表示ペインにフォーカスをセットします。
        /// </summary>
        private void SetFocusImage()
        {
            this.ImageContainer.Focus();
        }

        /// <summary>
        /// ファイル一覧にフォーカスをセットします。
        /// </summary>
        private void SetFocusFileList()
        {
            this.FileList.UpdateLayout();
            if (this.FileList.Items.IsEmpty) return;

            var row = (DataGridRow)this.FileList.ItemContainerGenerator.ContainerFromIndex(this.FileList.SelectedIndex);
            row?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        /// <summary>
        /// 次のシーンボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToNextSceneButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToNextScene();
        }

        /// <summary>
        /// 次のページボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToNextPage();
        }

        /// <summary>
        /// 前のページボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToPrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToPrevPage();
        }

        /// <summary>
        /// 前のシーンボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToPrevSceneButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToPrevScene();
        }

        /// <summary>
        /// ウィンドウサイズをフルスクリーンにします。
        /// </summary>
        private void ChangeToFullScreen()
        {
            this.fileListWidth = this.ColumnDefinition1.Width;

            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;

            this.MenuBar.Visibility = Visibility.Collapsed;
            this.ToolBar.Visibility = Visibility.Collapsed;
            this.AddressBar.Visibility = Visibility.Collapsed;
            this.ToolBar2.Visibility = Visibility.Collapsed;

            this.ColumnDefinition1.Width = new GridLength(0);
            this.ColumnDefinition2.Width = new GridLength(0);

            this.Cursor = Cursors.None;
        }

        /// <summary>
        /// ウィンドウサイズを通常にします。
        /// </summary>
        private void ChangeToNormalScreen()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.SingleBorderWindow;

            this.MenuBar.Visibility = Visibility.Visible;
            this.ToolBar.Visibility = Visibility.Visible;
            this.AddressBar.Visibility = Visibility.Visible;
            this.ToolBar2.Visibility = Visibility.Visible;

            this.ColumnDefinition1.Width = this.fileListWidth;
            this.ColumnDefinition2.Width = new GridLength(5);

            this.Cursor = null;
        }

        /// <summary>
        /// フルスクリーンなら解除します。
        /// フルスクリーンでないなら、フルスクリーンにします。
        /// </summary>
        private void ToggleFullScreen()
        {
            var isFullScreen = this.WindowStyle == WindowStyle.None;

            if (isFullScreen)
            {
                this.ChangeToNormalScreen();
            }
            else
            {
                this.ChangeToFullScreen();
            }
        }

        /// <summary>
        /// 自動ページ送りボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoPagerButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToggleAutoPager();
        }

        private void BackHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.BackHistoryCommand();
        }

        private void NextHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.NextHistoryCommand();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FileList_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            var preDir = e.Column.SortDirection;
            var newDir = preDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            //ViewModelのメソッドを呼び出し
            this.ViewModel.ExecuteSort(e.Column.SortMemberPath, newDir);

            //※ItemsSourceへのバインドを先に強制評価する
            this.FileList.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget();

            //ソートアイコン表示
            var clm = this.FileList.Columns.First(c => c.SortMemberPath == e.Column.SortMemberPath);
            clm.SortDirection = newDir;
        }

        private void PreferenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new PreferenceDialog()
            {
                Owner = this
            }
            .ShowDialog();
        }
    }
}
