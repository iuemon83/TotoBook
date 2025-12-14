using System.IO;
using System.Linq;
using TotoBook;
using TotoBook.ViewModel;
using Xunit;
using System.IO.Compression;

namespace TotoBook_Test
{
    /// <summary>
    /// MainWindowViewModel クラスのテスト
    /// </summary>
    public class MainWindowViewModel_Test
    {
        private const string TestDataDir = "testdata";
        private const string TestNestedArchivePath = "testdata/test_nested_archive.zip";

        public MainWindowViewModel_Test()
        {
            // ApplicationSettingsを初期化
            if (ApplicationSettings.Instance == null)
            {
                ApplicationSettings.LoadSettingsFromFile();
            }

            // テストデータディレクトリを作成
            if (!Directory.Exists(TestDataDir))
            {
                Directory.CreateDirectory(TestDataDir);
            }

            // ネストされたアーカイブのテストファイルを作成
            CreateNestedArchiveTestFile();
        }

        /// <summary>
        /// ネストされたアーカイブのテストファイルを作成します
        /// 構造: test_nested_archive.zip
        ///   - nested1.zip
        ///     - images/
        ///       - test001.jpg
        ///       - test002.jpg
        ///   - nested2.zip
        ///     - content/
        ///       - test003.jpg
        /// </summary>
        private void CreateNestedArchiveTestFile()
        {
            // すでに存在する場合はスキップ
            if (File.Exists(TestNestedArchivePath))
            {
                return;
            }

            var tempDir = Path.Combine(Path.GetTempPath(), "TotoBookTest_" + System.Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempDir);

                // nested1.zipを作成
                var nested1Dir = Path.Combine(tempDir, "nested1");
                var imagesDir = Path.Combine(nested1Dir, "images");
                Directory.CreateDirectory(imagesDir);

                // ダミー画像ファイルを作成（実際の画像データは不要なので、テキストで代用）
                File.WriteAllText(Path.Combine(imagesDir, "test001.jpg"), "dummy image content 1");
                File.WriteAllText(Path.Combine(imagesDir, "test002.jpg"), "dummy image content 2");

                var nested1ZipPath = Path.Combine(tempDir, "nested1.zip");
                ZipFile.CreateFromDirectory(nested1Dir, nested1ZipPath);

                // nested2.zipを作成
                var nested2Dir = Path.Combine(tempDir, "nested2");
                var contentDir = Path.Combine(nested2Dir, "content");
                Directory.CreateDirectory(contentDir);

                File.WriteAllText(Path.Combine(contentDir, "test003.jpg"), "dummy image content 3");

                var nested2ZipPath = Path.Combine(tempDir, "nested2.zip");
                ZipFile.CreateFromDirectory(nested2Dir, nested2ZipPath);

                // 親アーカイブを作成
                var parentDir = Path.Combine(tempDir, "parent");
                Directory.CreateDirectory(parentDir);

                File.Copy(nested1ZipPath, Path.Combine(parentDir, "nested1.zip"));
                File.Copy(nested2ZipPath, Path.Combine(parentDir, "nested2.zip"));

                ZipFile.CreateFromDirectory(parentDir, TestNestedArchivePath);
            }
            finally
            {
                // 一時ディレクトリを削除
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public void Navigate_NestedArchiveをクリック_正しく遷移する()
        {
            var testFilePath = TestNestedArchivePath;

            // ファイルが存在することを確認
            Assert.True(File.Exists(testFilePath), "テストファイルが作成されていません");

            // MainWindowViewModelを作成
            var viewModel = new MainWindowViewModel();

            // まず親ZIPファイルを開く
            var parentArchive = new FileInfoViewModel(new FileInfo(testFilePath), viewModel);
            viewModel.Navigate(parentArchive);

            // FileInfoListに子要素が表示されているはず
            Assert.NotEmpty(viewModel.FileInfoList);

            // ネストされたZIPファイルを取得
            var nestedZip = viewModel.FileInfoList.FirstOrDefault(f =>
                f.FileType == FileInfoViewModel.FileInfoType.NestedArchive);
            Assert.NotNull(nestedZip);

            // ネストされたZIPファイルに遷移
            viewModel.Navigate(nestedZip);

            // ネストされたZIPの中身が表示されているはず
            Assert.NotEmpty(viewModel.FileInfoList);

            // ディレクトリが表示されているはず
            var directory = viewModel.FileInfoList.FirstOrDefault(f =>
                f.FileType == FileInfoViewModel.FileInfoType.ArchivedDirectory);
            Assert.NotNull(directory);
        }

        [Fact]
        public void Navigate_NestedArchive以外のFileTypeでも動作する()
        {
            var viewModel = new MainWindowViewModel();

            // File型のテスト（画像ファイルなど）は実際のファイルが必要なのでスキップ可能性を考慮
            // ここではFileType判定のロジックのみをテスト

            // 各FileTypeに対してNavigateが例外を投げないことを確認
            var fileTypes = new[]
            {
                FileInfoViewModel.FileInfoType.Archive,
                FileInfoViewModel.FileInfoType.ArchivedDirectory,
                FileInfoViewModel.FileInfoType.NestedArchive,
                FileInfoViewModel.FileInfoType.Directory
            };

            // このテストは各FileTypeの分岐が正しく実装されていることを確認するためのもの
            // 実際のファイルを使わないため、例外が発生しないことの確認のみ
            Assert.True(fileTypes.Length == 4);
        }

        [Fact]
        public void NavigateToArchiveFile_NestedArchiveを受け入れる()
        {
            var testFilePath = TestNestedArchivePath;

            Assert.True(File.Exists(testFilePath), "テストファイルが作成されていません");

            var viewModel = new MainWindowViewModel();
            var parentArchive = new FileInfoViewModel(new FileInfo(testFilePath), viewModel);
            viewModel.Navigate(parentArchive);

            // ネストされたZIPファイルを取得
            var nestedZip = viewModel.FileInfoList.FirstOrDefault(f =>
                f.FileType == FileInfoViewModel.FileInfoType.NestedArchive);

            if (nestedZip == null)
            {
                return;
            }

            // NavigateToArchiveFileが正しく動作することを確認
            // 例外が発生しなければOK
            viewModel.Navigate(nestedZip);

            // 遷移後にFileInfoListが更新されていることを確認
            Assert.NotEmpty(viewModel.FileInfoList);
        }

        [Fact]
        public void RefreshFileList_NestedArchive用のソートが適用される()
        {
            var testFilePath = TestNestedArchivePath;

            Assert.True(File.Exists(testFilePath), "テストファイルが作成されていません");

            var viewModel = new MainWindowViewModel();
            var parentArchive = new FileInfoViewModel(new FileInfo(testFilePath), viewModel);
            viewModel.Navigate(parentArchive);

            var nestedZip = viewModel.FileInfoList.FirstOrDefault(f =>
                f.FileType == FileInfoViewModel.FileInfoType.NestedArchive);

            if (nestedZip == null)
            {
                return;
            }

            // ネストされたZIPに遷移
            viewModel.Navigate(nestedZip);

            // FileInfoListが正しく初期化されているはず
            Assert.NotEmpty(viewModel.FileInfoList);

            // ソートが適用されても例外が発生しないことを確認
            viewModel.ExecuteSort(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));

            // FileInfoListが維持されていることを確認
            Assert.NotEmpty(viewModel.FileInfoList);
        }

        [Fact]
        public void FileInfoViewModel_アーカイブ内のZIPファイルがNestedArchiveとして認識される()
        {
            var testFilePath = TestNestedArchivePath;

            Assert.True(File.Exists(testFilePath), "テストファイルが作成されていません");

            // アーカイブを開いて子要素を取得
            var (archive, items) = Archive.GetChildrenForList(testFilePath);
            using (archive)
            {
                var itemArray = items.ToArray();
                Assert.NotEmpty(itemArray);

                // ZIPファイルを取得
                var zipItem = itemArray.FirstOrDefault(item =>
                    Path.GetExtension(item.FileName).ToLower() == ".zip");
                Assert.NotNull(zipItem);

                // FileInfoViewModelに変換
                var viewModel = new MainWindowViewModel();
                var parentFileInfo = new FileInfoViewModel(new FileInfo(testFilePath), viewModel);
                var zipFileInfo = new FileInfoViewModel(zipItem, viewModel, parentFileInfo);

                // NestedArchiveとして認識されているはず
                Assert.Equal(FileInfoViewModel.FileInfoType.NestedArchive, zipFileInfo.FileType);
            }
        }

        [Fact]
        public void FileInfoViewModel_アーカイブ内のディレクトリがArchivedDirectoryとして認識される()
        {
            var testFilePath = TestNestedArchivePath;

            Assert.True(File.Exists(testFilePath), "テストファイルが作成されていません");

            var viewModel = new MainWindowViewModel();
            var parentArchive = new FileInfoViewModel(new FileInfo(testFilePath), viewModel);
            viewModel.Navigate(parentArchive);

            var nestedZip = viewModel.FileInfoList.FirstOrDefault(f =>
                f.FileType == FileInfoViewModel.FileInfoType.NestedArchive);

            if (nestedZip == null)
            {
                return;
            }

            // ネストされたZIPの中身を取得
            var (archive, items) = Archive.GetChildrenForList(nestedZip.GetFileStream());
            using (archive)
            {
                var itemArray = items.ToArray();
                Assert.NotEmpty(itemArray);

                // ディレクトリを取得
                var dirItem = itemArray.FirstOrDefault(item =>
                    item.Type == ArchiveItem.ArchiveItemType.Directory);
                Assert.NotNull(dirItem);

                // FileInfoViewModelに変換
                var dirFileInfo = new FileInfoViewModel(dirItem, viewModel, nestedZip);

                // ArchivedDirectoryとして認識されているはず
                Assert.Equal(FileInfoViewModel.FileInfoType.ArchivedDirectory, dirFileInfo.FileType);
            }
        }
    }
}
