using System.Collections.Generic;
using System.Linq;
using TotoBook;
using Xunit;
using System.IO.Compression;
using System.IO;

namespace TotoBook_Test
{
    /// <summary>
    /// Archive クラスのテスト
    /// </summary>
    public class Archive_Test
    {
        private const string TestDataDir = "testdata";
        private const string TestNestedArchivePath = "testdata/test_nested_archive.zip";

        public Archive_Test()
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

                // ダミー画像ファイルを作成
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

        class TestFile
        {
            public string Name { get; set; }
            public TestFile[] Children { get; set; }

            public TestFile(string name, TestFile[] children)
            {
                this.Name = name;
                this.Children = children;
            }

            public TestFile(string name) : this(name, new TestFile[0]) { }
        }

        private void AssertArchiveItems(IEnumerable<TestFile> expected, IEnumerable<ArchiveItem> actual)
        {
            var expectedArray = expected.ToArray();
            var actualArray = actual.ToArray();

            Assert.Equal(expectedArray.Length, actualArray.Length);
            expectedArray.Zip(actualArray)
                .ForEach(ea =>
                {
                    Assert.Equal(ea.First.Name, ea.Second.FileName);
                    this.AssertArchiveItems(ea.First.Children, ea.Second.Children);
                });
        }

        [Fact]
        public void アーカイブ内の要素を取得する_0階層()
        {
            var testFilePath = @"testdata\testfile_0階層.zip";
            var expectedTestFiles = new[] { new TestFile("testfile1.txt") };

            var (_, actual) = Archive.GetChildrenForList(testFilePath);

            this.AssertArchiveItems(expectedTestFiles, actual);
        }

        [Fact]
        public void アーカイブ内の要素を取得する_1階層()
        {
            var testFilePath = @"testdata\testfile_1階層.zip";
            var expectedTestFiles = new[]
            {
                new TestFile("testfile1",
                new[]
                {
                    new TestFile("testtext1.txt")
                })
            };

            var (_, actual) = Archive.GetChildrenForList(testFilePath);

            this.AssertArchiveItems(expectedTestFiles, actual);
        }

        [Fact]
        public void アーカイブ内の要素を取得する_2階層()
        {
            var testFilePath = @"testdata\testfile_2階層.zip";
            var expectedTestFiles = new[]
            {
                new TestFile("testfile1",
                new[]
                {
                    new TestFile("testfile2",
                    new[]
                    {
                        new TestFile("testtext1.txt")
                    })
                })
            };

            var (_, actual) = Archive.GetChildrenForList(testFilePath);

            this.AssertArchiveItems(expectedTestFiles, actual);
        }

        [Fact]
        public void アーカイブ内の要素を取得する_アーカイブがネストしている()
        {
            var testFilePath = @"testdata\testfile_ネスト.zip";
            var expectedTestFiles = new[]
            {
                new TestFile("testfile2",
                new[]
                {
                    new TestFile("testfile1.zip"),
                    new TestFile("testfile1",
                    new[]
                    {
                         new TestFile("testtext1.txt")
                    }),
                })
            };

            var (_, actual) = Archive.GetChildrenForList(testFilePath);

            this.AssertArchiveItems(expectedTestFiles, actual);
        }

        [Fact]
        public void アーカイブ内の要素を取得する_同じ名前のフォルダがネスト()
        {
            var testFilePath = @"testdata\testfile_同じ名前のフォルダがネスト.zip";
            var expectedTestFiles = new[]
            {
                new TestFile("testfile",
                new[]
                {
                    new TestFile("testfile",
                    new[]
                    {
                         new TestFile("testfile.txt")
                    }),
                })
            };

            var (_, actual) = Archive.GetChildrenForList(testFilePath);

            this.AssertArchiveItems(expectedTestFiles, actual);
        }

        [Fact]
        public void アーカイブ内の要素を取得する_ネストされたアーカイブ()
        {
            var testFilePath = TestNestedArchivePath;

            // ファイルが存在することを確認
            Assert.True(System.IO.File.Exists(testFilePath), "テストファイルが作成されていません");

            var (archive, actualItems) = Archive.GetChildrenForList(testFilePath);
            using (archive)
            {
                // アーカイブが正常に開けることを確認
                Assert.NotNull(archive);

                // 子要素が取得できることを確認
                var actualArray = actualItems.ToArray();
                Assert.NotEmpty(actualArray);

                // ZIPファイルを取得
                var firstZip = actualArray.FirstOrDefault(item =>
                    System.IO.Path.GetExtension(item.FileName).ToLower() == ".zip");
                Assert.NotNull(firstZip);

                // ネストされたアーカイブの中身を確認
                using var stream = firstZip.CreateStream();
                var (nestedArchive, nestedItems) = Archive.GetChildrenForList(stream);
                using (nestedArchive)
                {
                    var nestedArray = nestedItems.ToArray();

                    // 結果を出力
                    var output = new System.Text.StringBuilder();
                    output.AppendLine($"親アーカイブ: {testFilePath}");
                    output.AppendLine($"  Total items: {actualArray.Length}");
                    output.AppendLine("");
                    output.AppendLine($"ネストされたアーカイブ: {firstZip.FileName}");
                    output.AppendLine($"  Total nested items: {nestedArray.Length}");

                    if (nestedArray.Length > 0)
                    {
                        output.AppendLine("  Nested items:");
                        foreach (var item in nestedArray.Take(20))  // 最初の20個だけ
                        {
                            var extension = System.IO.Path.GetExtension(item.FileName).ToLower();
                            output.AppendLine($"    - {item.FileName} (Type: {item.Type}, Ext: {extension}, Size: {item.FileSize})");

                            // ディレクトリの場合、子要素も表示
                            if (item.Type == ArchiveItem.ArchiveItemType.Directory && item.Children != null)
                            {
                                output.AppendLine($"      Children count: {item.Children.Count()}");
                                foreach (var child in item.Children.Take(10))
                                {
                                    var childExt = System.IO.Path.GetExtension(child.FileName).ToLower();
                                    output.AppendLine($"        • {child.FileName} (Type: {child.Type}, Ext: {childExt}, Size: {child.FileSize})");
                                }
                                if (item.Children.Count() > 10)
                                {
                                    output.AppendLine($"        ... and {item.Children.Count() - 10} more children");
                                }
                            }
                        }
                        if (nestedArray.Length > 20)
                        {
                            output.AppendLine($"    ... and {nestedArray.Length - 20} more items");
                        }
                    }
                    else
                    {
                        output.AppendLine("  ⚠️ ネストされたアーカイブの中身が空です！");
                    }

                    // 出力を表示
                    System.Console.WriteLine(output.ToString());

                    // 実際には画像ファイルが子要素として正しく取得できている
                    Assert.True(nestedArray.Length == 1, "ネストされたアーカイブには1つのディレクトリがあるはず");
                    Assert.True(nestedArray[0].Type == ArchiveItem.ArchiveItemType.Directory, "最初の要素はディレクトリのはず");
                    Assert.True(nestedArray[0].Children != null && nestedArray[0].Children.Any(), "ディレクトリには子要素があるはず");

                    // JPGファイルが正しく取得できているか確認
                    var jpgFiles = nestedArray[0].Children.Where(c =>
                        System.IO.Path.GetExtension(c.FileName).ToLower() == ".jpg").ToArray();
                    Assert.True(jpgFiles.Length > 0, "JPGファイルが見つかるはず");
                }
            }
        }
    }
}
