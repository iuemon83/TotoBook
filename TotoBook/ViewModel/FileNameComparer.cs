using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TotoBook.ViewModel
{
    public class FileNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var fileNameX = Path.GetFileNameWithoutExtension(x);
            var fileNameY = Path.GetFileNameWithoutExtension(y);

            var matchesX = Regex.Matches(fileNameX, @"[0-9]+");
            var matchesY = Regex.Matches(fileNameY, @"[0-9]+");
            var maxNumberLength = Math.Max(
                matchesX.IsEmpty() ? 0 : matchesX.Max(m => m.Length),
                matchesY.IsEmpty() ? 0 : matchesY.Max(m => m.Length));

            var replacedX = Regex.Replace(fileNameX, @"[0-9]+",
                match => match.Value.PadLeft(maxNumberLength, '0'));

            var replacedY = Regex.Replace(fileNameY, @"[0-9]+",
                match => match.Value.PadLeft(maxNumberLength, '0'));

            return replacedX.CompareTo(replacedY);
        }
    }
}
