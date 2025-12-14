using System.Collections.Generic;
using System.IO;
using System.Linq;
using TotoBook.ViewModel;
using Xunit;

namespace TotoBook_Test
{
    public class FileNameComparer_Test
    {
        private void TestFileNameCompare(IEnumerable<string> test, IEnumerable<string> expected)
        {
            var actual = test.OrderBy(n => n, new FileNameComparer()).ToArray();

            foreach (var (e, a) in expected.Zip(actual))
            {
                Assert.Equal(e, a);
            }
        }

        [Fact]
        public void ファイル名のソート()
        {
            var test = new[] {
                "kaiji09_008.jpg",
                "kaiji09_006-007.jpg",
                "kaiji09_002.jpg",
                "kaiji09_000b.jpg",
                "kaiji09_000a.jpg",
            };

            var expected = new[]
            {
                "kaiji09_000a.jpg",
                "kaiji09_000b.jpg",
                "kaiji09_002.jpg",
                "kaiji09_006-007.jpg",
                "kaiji09_008.jpg",
            };

            TestFileNameCompare(test, expected);
        }

        [Fact]
        public void ファイル名のソート2()
        {
            var test = new[] {
                "Kaiji 07 (10).jpg",
                "Kaiji 07 (2).jpg",
                "Kaiji 07 (1).jpg",
            };

            var expected = new[]
            {
                "Kaiji 07 (1).jpg",
                "Kaiji 07 (2).jpg",
                "Kaiji 07 (10).jpg",
            };

            TestFileNameCompare(test, expected);
        }

        //[Fact]
        //public void シンプルなテスト()
        //{
        //    Assert.True(true);
        //}

        [Fact]
        public void ファイル名のソート3()
        {
            var a = new DirectoryInfo("C:\\Users\\owner\\Pictures\\books\\[島袋光年] トリコ")
                .EnumerateFileSystemInfos()
                .ToArray();

            throw new System.NotImplementedException();
        }
    }
}
