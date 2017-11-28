using System.Collections.Generic;
using System.IO;

namespace TotoBook.ViewModel
{
    class FileNameComparer : IComparer<FileInfoViewModel>
    {
        public int Compare(FileInfoViewModel x, FileInfoViewModel y)
        {
            var fileNameX = Path.GetFileNameWithoutExtension(x.Name);
            var fileNameY = Path.GetFileNameWithoutExtension(y.Name);

            if (int.TryParse(fileNameX, out var intx))
            {
                if (int.TryParse(fileNameY, out var inty))
                {
                    return intx.CompareTo(inty);
                }
                else
                {
                    return fileNameX.CompareTo(fileNameY);
                }
            }
            else
            {
                return fileNameX.CompareTo(fileNameY);
            }
        }
    }
}
