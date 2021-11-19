using System.Collections.Generic;
using System.Linq;
using TotoBook;
using Xunit;

namespace TotoBook_Test
{
    /// <summary>
    /// Archive �N���X�̃e�X�g
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
        public void �A�[�J�C�u���̗v�f���擾����_0�K�w()
        {
            var testFilePath = @"testdata\testfile_0�K�w.zip";
            var expectedTestFiles = new[] { new TestFile("testfile1.txt") };

            var (_, actual) = Archive.GetChildrenForList(testFilePath);

            this.AssertArchiveItems(expectedTestFiles, actual);
        }

        [Fact]
        public void �A�[�J�C�u���̗v�f���擾����_1�K�w()
        {
            var testFilePath = @"testdata\testfile_1�K�w.zip";
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
        public void �A�[�J�C�u���̗v�f���擾����_2�K�w()
        {
            var testFilePath = @"testdata\testfile_2�K�w.zip";
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
        public void �A�[�J�C�u���̗v�f���擾����_�A�[�J�C�u���l�X�g���Ă���()
        {
            var testFilePath = @"testdata\testfile_�l�X�g.zip";
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
        public void �A�[�J�C�u���̗v�f���擾����_�������O�̃t�H���_���l�X�g()
        {
            var testFilePath = @"testdata\testfile_�������O�̃t�H���_���l�X�g.zip";
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
