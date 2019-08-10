using GalaSoft.MvvmLight;

namespace TotoBook.ViewModel
{
    class PreferenceDialogViewModel : ViewModelBase
    {
        private string _PluginDirectoryPath;

        /// <summary>
        /// プラグインフォルダーのパス
        /// </summary>
        public string PluginDirectoryPath
        {
            get { return this._PluginDirectoryPath; }
            set
            {
                if (this._PluginDirectoryPath == value) return;

                this._PluginDirectoryPath = value;
                this.RaisePropertyChanged();
            }
        }

        private double _AutoPagerInterval;

        /// <summary>
        /// 自動ページ送りの間隔
        /// </summary>
        public double AutoPagerInterval
        {
            get { return this._AutoPagerInterval; }
            set
            {
                if (this._AutoPagerInterval == value) return;

                this._AutoPagerInterval = value;
                this.RaisePropertyChanged();
            }
        }

        public PreferenceDialogViewModel()
        {
            this.PluginDirectoryPath = ApplicationSettings.Instance.PluginDirectoryPath;
            this.AutoPagerInterval = ApplicationSettings.Instance.AutoPagerInterval;
        }

        public void Execute()
        {
            ApplicationSettings.Instance.PluginDirectoryPath = this.PluginDirectoryPath;
            ApplicationSettings.Instance.AutoPagerInterval = this.AutoPagerInterval;

            ApplicationSettings.SaveSettingsToFile();
        }
    }
}
