namespace TotoBook
{
    class ArchiveEntry
    {
        public string FullName { get; set; }
        public string FileName { get; set; }

        public long TimeStamp { get; set; }

        public long FileSize { get; set; }

        public int Position { get; set; }
    }
}
