using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TotoBook.Spi
{
    class Susie
    {
        public delegate int GetPluginInfo(
            int infono,
            [Out, MarshalAs(UnmanagedType.LPStr,SizeParamIndex = 2)]
            StringBuilder buf,
            int buflen);

        public delegate int IsSupported(
              [In, MarshalAs(UnmanagedType.LPStr)]
            string filename,
              [In]byte[] dw);

        public delegate int GetPictureInfo(
            [In]byte[] buf,
            int len,
            InputFlag flag,
            out PictureInfo lpInfo);

        public delegate int ProgressCallback(int nNum, int nDenom, long lData);

        public delegate int GetPicture(
            [In]byte[] buf,
            int len,
            InputFlag flag,
            out IntPtr pHBInfo,
            out IntPtr PHBm,
            ProgressCallback lpPrgressCallback,
            int lData);

        public delegate int GetPreview(
            [In]byte[] buf,
            int len,
            InputFlag flag,
            out IntPtr pHBInfo,
            out IntPtr PHBm,
            ProgressCallback lpPrgressCallback,
            int lData);

        public delegate int GetArchiveInfo(
            [In, MarshalAs(UnmanagedType.LPStr)]
            string buf,
            int len,
            InputFlag flag,
            out IntPtr lphInfo);

        public delegate int GetFileInfo(
            [In, MarshalAs(UnmanagedType.LPStr)]
            string buf,
            int len,
            [In, MarshalAs(UnmanagedType.LPStr)]
            string filename,
            uint flag,
            out FileInfo lpInfo);

        public delegate int GetFile(
            [In, MarshalAs(UnmanagedType.LPStr)]
            string src,
            int len,
            out IntPtr dest,
            uint flag,
            ProgressCallback prgressCallback,
            int lData);
    }
}
