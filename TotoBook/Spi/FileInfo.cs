using System.Runtime.InteropServices;

namespace TotoBook.Spi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Method;

        public int Position;

        public int CompSize;

        public int FileSize;

        public int TimeStamp;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string Path;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string FileName;

        public int CRC;
    }
}
