using System.Collections.Generic;
using TotoBook.ViewModel;

namespace TotoBook
{
    class HistoryService
    {
        private readonly Stack<FileInfoViewModel> backList = new Stack<FileInfoViewModel>();
        private readonly Stack<FileInfoViewModel> nextList = new Stack<FileInfoViewModel>();

        private FileInfoViewModel current = null;

        public bool EnableBack
        {
            get { return this.backList.Count != 0; }
        }

        public bool EnableNext
        {
            get { return this.nextList.Count != 0; }
        }

        public void AddNewCurrent(FileInfoViewModel current)
        {
            if (this.current != null) this.backList.Push(this.current);
            this.nextList.Clear();
            this.current = current;
        }

        public FileInfoViewModel Back()
        {
            if (!this.EnableBack) return null;

            var result = this.backList.Pop();
            this.nextList.Push(this.current);
            this.current = result;

            return result;
        }

        public FileInfoViewModel Next()
        {
            if (!this.EnableNext) return null;

            var result = this.nextList.Pop();
            this.backList.Push(this.current);
            this.current = result;

            return result;
        }
    }
}
