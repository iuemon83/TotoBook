using System.Windows.Media.Imaging;

namespace TotoBook.ViewModel
{
    public interface IFileListItemViewModel
    {
        bool IsDisplayed { get; set; }
        BitmapSource Icon { get; set; }
    }
}
