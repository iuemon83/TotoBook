namespace TotoBook.ViewModel
{
    class PreferenceDialogViewModel
    {
        /// <summary>
        /// 自動ページ送りの間隔
        /// </summary>
        public double AutoPagerInterval { get; set; }

        public PreferenceDialogViewModel()
        {
            this.AutoPagerInterval = ApplicationSettings.Instance.AutoPagerInterval;
        }

        public void Execute()
        {
            ApplicationSettings.Instance.AutoPagerInterval = this.AutoPagerInterval;
        }
    }
}
