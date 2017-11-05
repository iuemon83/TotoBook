using System.Windows.Media.Imaging;

namespace TotoBook
{
    public interface IFileListItemViewModel
    {
        bool IsDisplayed { get; set; }
        BitmapSource Icon { get; set; }
    }
}
