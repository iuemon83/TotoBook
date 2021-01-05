using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TotoBook.Spi
{
    class SpiManager
    {
        private static SusiePlugin[] photoPluginList;
        private static SusiePlugin[] archivePluginList;

        public static void Load(string path)
        {
            if (!Directory.Exists(path)) return;

            var photoPluginList = new List<SusiePlugin>();
            var archivePluginList = new List<SusiePlugin>();
            Directory.EnumerateFiles(path, "*.spi")
                .Select(spi => SusiePlugin.Load(spi))
                .ForEach(spi =>
                {
                    if (spi.Type == SusiePluginType.Import)
                    {
                        photoPluginList.Add(spi);
                    }
                    else
                    {
                        archivePluginList.Add(spi);
                    }
                });

            SpiManager.photoPluginList = photoPluginList.ToArray();
            SpiManager.archivePluginList = archivePluginList.ToArray();
        }

        public static Stream GetPictureStream(string filePath)
        {
            return photoPluginList
                .Select(spi => spi.GetPictureStream(filePath))
                .First(stream => stream != null);
        }

        public static Stream GetPictureStream(string fileName, byte[] buf)
        {
            return photoPluginList
                .Select(spi => spi.GetPictureStream(fileName, buf))
                .First(stream => stream != null);
        }

        public static IEnumerable<FileInfo> GetArchiveInfo(string archiveFilePath)
        {
            return archivePluginList
                .Select(spi => spi.GetArchiveInfo(archiveFilePath))
                .First(stream => stream != null);
        }

        public static byte[] GetFile(string archiveFilePath, int position)
        {
            return archivePluginList
                .Select(spi => spi.GetFile(archiveFilePath, position))
                .First(b => b != null);
        }
    }
}
