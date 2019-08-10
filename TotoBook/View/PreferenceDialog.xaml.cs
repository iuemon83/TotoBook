using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using TotoBook.ViewModel;

namespace TotoBook.View
{
    /// <summary>
    /// PreferenceDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PreferenceDialog : Window
    {
        public PreferenceDialog()
        {
            InitializeComponent();
        }

        private PreferenceDialogViewModel viewmodel;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewmodel = new PreferenceDialogViewModel();
            this.DataContext = this.viewmodel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewmodel.Execute();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenPluginFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                DefaultFileName = this.viewmodel.PluginDirectoryPath,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.viewmodel.PluginDirectoryPath = dialog.FileName;
            }
        }
    }
}
