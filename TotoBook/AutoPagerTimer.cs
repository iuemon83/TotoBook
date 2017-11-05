using System;
using System.Windows.Threading;

namespace TotoBook
{
    /// <summary>
    /// 自動ページ送り用のタイマー
    /// </summary>
    class AutoPagerTimer
    {
        /// <summary>
        /// タイマー
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// タイマーが有効になっている場合はTrue、そうでなければFalse を取得します。
        /// </summary>
        public bool IsEnabled
        {
            get { return this.timer.IsEnabled; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action">ページ送り処理</param>
        public AutoPagerTimer(Action action)
        {
            this.timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(ApplicationSettings.Instance.AutoPagerInterval)
            };
            this.timer.Tick += new EventHandler((o, ev) => action());
            this.timer.Start();
        }

        /// <summary>
        /// タイマーを開始します。
        /// </summary>
        public void Start()
        {
            this.timer.Start();
        }

        /// <summary>
        /// タイマーを停止します。
        /// </summary>
        public void Stop()
        {
            this.timer.Stop();
        }

        /// <summary>
        /// タイマーをリセットします。
        /// </summary>
        public void Reset()
        {
            this.timer.Interval = TimeSpan.FromSeconds(ApplicationSettings.Instance.AutoPagerInterval);
        }

        /// <summary>
        /// タイマーが有効、無効を切り替えます。
        /// </summary>
        public void Toggle()
        {
            if (this.timer.IsEnabled)
            {
                this.timer.Stop();
            }
            else
            {
                this.timer.Start();
            }
        }
    }
}
