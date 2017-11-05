using System;
using System.Runtime.InteropServices;

namespace TotoBook.Spi
{
    public static class Win32
    {
        //
        // LoadLibrary
        //
        [DllImport("kernel32", SetLastError = true)]
        public extern static IntPtr LoadLibrary(string lpFileName);

        //
        // GetProcAddress
        //
        [DllImport("kernel32", SetLastError = true)]
        public extern static IntPtr GetProcAddress(
            IntPtr hModule, string lpProcName);

        //
        // LocalLock
        //
        [DllImport("kernel32")]
        public extern static IntPtr LocalLock(IntPtr hMem);

        //
        // LocalUnLock
        //
        [DllImport("kernel32")]
        public extern static bool LocalUnlock(IntPtr hMem);

        //
        // LocalFree
        //
        [DllImport("kernel32")]
        public extern static IntPtr LocalFree(IntPtr hMem);

        //
        // LocalSize
        //
        [DllImport("kernel32")]
        public extern static uint LocalSize(IntPtr hMem);

        public static BITMAPFILEHEADER GetBF(BITMAPINFO bi)
        {
            var biHeader = bi.bmiHeader;

            var bf = new BITMAPFILEHEADER();
            bf.bfSize = (uint)((((biHeader.biWidth * biHeader.biBitCount + 0x1f) >> 3) & ~3) * biHeader.biHeight);
            bf.bfOffBits = (uint)(Marshal.SizeOf(bf) + Marshal.SizeOf(biHeader));
            if (biHeader.biBitCount <= 8)
            {
                uint palettes = biHeader.biClrUsed;
                if (palettes == 0)
                    palettes = 1u << biHeader.biBitCount;
                bf.bfOffBits += palettes << 2;
            }
            bf.bfSize += bf.bfOffBits;
            bf.bfType = Win32.BM;
            bf.bfReserved1 = 0;
            bf.bfReserved2 = 0;

            return bf;
        }

        //
        // STRUCT for DIBs
        //
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPFILEHEADER
        {
            public ushort bfType;
            public uint bfSize;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfOffBits;
        }
        public const ushort BM = 0x4d42;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public RGBQUAD biColors;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10; // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}
