using System.Collections.Generic;
using System.Linq;
using TotoBook;
using Xunit;

namespace TotoBook_Test
{
    /// <summary>
    /// Archive クラスのテスト
    /// </summary>
    public class Archive_Test
    {
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
    }
}
