using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static TotoBook.Spi.Win32;

namespace TotoBook.Spi
{
    class SusiePlugin
    {
        public SusiePluginType Type { get; private set; }

        public static SusiePlugin Load(string filePath)
        {
            var ptr = Win32.LoadLibrary(filePath);
            if (ptr == IntPtr.Zero) return null;

            var result = new SusiePlugin();

            IntPtr tmp;
            tmp = Win32.GetProcAddress(ptr, "GetPluginInfo");
            if (tmp == IntPtr.Zero) return null;

            result.getPluginInfo = Marshal.GetDelegateForFunctionPointer<Susie.GetPluginInfo>(tmp);

            var buf = new StringBuilder(1000);
            if (result.getPluginInfo(0, buf, buf.Capacity) == 0) return null;

            var version = buf.ToString();

            tmp = Win32.GetProcAddress(ptr, "IsSupported");
            if (tmp != IntPtr.Zero)
            {
                result.isSupported = Marshal.GetDelegateForFunctionPointer<Susie.IsSupported>(tmp);
            }

            if (version.EndsWith("IN"))
            {
                result.Type = SusiePluginType.Import;

                tmp = Win32.GetProcAddress(ptr, "GetPictureInfo");
                if (tmp != IntPtr.Zero)
                {
                    result.getPictureInfo = Marshal.GetDelegateForFunctionPointer<Susie.GetPictureInfo>(tmp);
                }

                tmp = Win32.GetProcAddress(ptr, "GetPicture");
                if (tmp != IntPtr.Zero)
                {
                    result.getPicture = Marshal.GetDelegateForFunctionPointer<Susie.GetPicture>(tmp);
                }

                tmp = Win32.GetProcAddress(ptr, "GetPreview");
                if (tmp != IntPtr.Zero)
                {
                    result.getPreview = Marshal.GetDelegateForFunctionPointer<Susie.GetPreview>(tmp);
                }
            }
            else if (version.EndsWith("AM"))
            {
                result.Type = SusiePluginType.Archive;

                tmp = Win32.GetProcAddress(ptr, "GetArchiveInfo");
                if (tmp != IntPtr.Zero)
                {
                    result.getArchiveInfo = Marshal.GetDelegateForFunctionPointer<Susie.GetArchiveInfo>(tmp);
                }

                tmp = Win32.GetProcAddress(ptr, "GetFileInfo");
                if (tmp != IntPtr.Zero)
                {
                    result.getFileInfo = Marshal.GetDelegateForFunctionPointer<Susie.GetFileInfo>(tmp);
                }

                tmp = Win32.GetProcAddress(ptr, "GetFile");
                if (tmp != IntPtr.Zero)
                {
                    result.getFile = Marshal.GetDelegateForFunctionPointer<Susie.GetFile>(tmp);
                }
            }

            return result;
        }

        private Susie.GetPluginInfo getPluginInfo;
        private Susie.IsSupported isSupported;
        private Susie.GetPictureInfo getPictureInfo;
        private Susie.GetPicture getPicture;
        private Susie.GetPreview getPreview;

        private Susie.GetArchiveInfo getArchiveInfo;
        private Susie.GetFileInfo getFileInfo;
        private Susie.GetFile getFile;

        public bool IsSupported(string filePath, byte[] buf)
        {
            return this.isSupported(filePath, buf) != 0;
        }

        public PictureInfo GetPictureInfo(string filePath)
        {
            var buf = File.ReadAllBytes(filePath);

            this.getPictureInfo(buf, buf.Length, InputFlag.Memory, out PictureInfo pictureInfo);

            return pictureInfo;
        }

        public Bitmap GetPicture(string filePath)
        {
            var buf = File.ReadAllBytes(filePath);

            using (var stream = this.GetPictureStream(filePath, buf))
            {
                return new Bitmap(stream);
            }
        }

        public Stream GetPictureStream(string filePath)
        {
            var buf = File.ReadAllBytes(filePath);
            return this.GetPictureStream(filePath, buf);
        }

        public Stream GetPictureStream(string fileName, byte[] buf)
        {
            if (this.getPicture == null) return null;

            if (this.Type != SusiePluginType.Import) return null;
            if (!this.IsSupported(fileName, buf)) return null;
            if (this.getPicture(buf, buf.Length, InputFlag.Memory, out IntPtr hBInfo, out IntPtr hBm, null, 0) != 0) return null;

            try
            {
                IntPtr pBInfo = Win32.LocalLock(hBInfo);
                IntPtr pBm = Win32.LocalLock(hBm);

                var bi = Marshal.PtrToStructure<BITMAPINFO>(pBInfo);
                var bf = Win32.GetBF(bi);

                byte[] mem = new byte[bf.bfSize];
                GCHandle handle = GCHandle.Alloc(bf, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(handle.AddrOfPinnedObject(), mem, 0, Marshal.SizeOf(bf));
                }
                finally
                {
                    handle.Free();
                }
                Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), (int)bf.bfOffBits - Marshal.SizeOf(bf));
                Marshal.Copy(pBm, mem, (int)bf.bfOffBits, (int)(bf.bfSize - bf.bfOffBits));

                return new MemoryStream(mem);
            }
            finally
            {
                Win32.LocalUnlock(hBInfo);
                Win32.LocalFree(hBInfo);
                Win32.LocalUnlock(hBm);
                Win32.LocalFree(hBm);
            }
        }

        public Stream GetPreviewStream(byte[] buf)
        {
            if (this.getPicture == null) return null;

            if (this.Type != SusiePluginType.Import) return null;
            //if (this.isSupported("", buf) == 0) return null;

            if (this.getPreview(buf, buf.Length, InputFlag.Memory, out IntPtr hBInfo, out IntPtr hBm, null, 0) != 0) return null;

            try
            {
                IntPtr pBInfo = Win32.LocalLock(hBInfo);
                IntPtr pBm = Win32.LocalLock(hBm);

                var bi = Marshal.PtrToStructure<BITMAPINFO>(pBInfo);
                var bf = Win32.GetBF(bi);

                byte[] mem = new byte[bf.bfSize];
                GCHandle handle = GCHandle.Alloc(bf, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(handle.AddrOfPinnedObject(), mem, 0, Marshal.SizeOf(bf));
                }
                finally
                {
                    handle.Free();
                }
                Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), (int)bf.bfOffBits - Marshal.SizeOf(bf));
                Marshal.Copy(pBm, mem, (int)bf.bfOffBits, (int)(bf.bfSize - bf.bfOffBits));

                return new MemoryStream(mem);
            }
            finally
            {
                Win32.LocalUnlock(hBInfo);
                Win32.LocalFree(hBInfo);
                Win32.LocalUnlock(hBm);
                Win32.LocalFree(hBm);
            }
        }

        public IEnumerable<FileInfo> GetArchiveInfo(string filePath)
        {
            var isSucceeded = this.getArchiveInfo(filePath, 0, InputFlag.Disk, out IntPtr ptr);
            if (isSucceeded != 0) return null;

            return this.GetArchiveInfo(ptr);
        }

        private IEnumerable<FileInfo> GetArchiveInfo(IntPtr archiveInfo)
        {
            while (true)
            {
                var fileInfo = Marshal.PtrToStructure<FileInfo>(archiveInfo);
                if (fileInfo.Method == "") break;

                yield return fileInfo;

                archiveInfo = IntPtr.Add(archiveInfo, Marshal.SizeOf<FileInfo>());
            }
        }

        public FileInfo GetFileInfo(string archiveFilePath, string filePath)
        {
            this.getFileInfo(archiveFilePath, 0, filePath, 0x0081, out FileInfo fileInfo);

            return fileInfo;
        }

        public byte[] GetFile(string archiveFilePath, int filePosition)
        {
            var returnValue = this.getFile(archiveFilePath, filePosition, out IntPtr ptr, 0x0100, null, 0);
            if (returnValue != 0) return null;

            try
            {
                var pptr = Win32.LocalLock(ptr);
                var size = (int)Win32.LocalSize(pptr);

                var buf = new byte[size];
                Marshal.Copy(pptr, buf, 0, buf.Length);

                return buf;
            }
            finally
            {
                Win32.LocalUnlock(ptr);
                Win32.LocalFree(ptr);
            }
        }
    }
}
