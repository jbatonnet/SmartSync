using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class ZipStorage : Storage
    {
        public Storage Storage { get; set; }
        public string Path { get; set; }
        public CompressionLevel Compression { get; set; } = CompressionLevel.Fastest;

        public override Directory Root
        {
            get
            {
                Initialize();
                return root;
            }
        }

        internal ZipArchive Archive { get; private set; }

        private File zipFile;
        private Stream zipStream;
        private ZipRoot root;

        public ZipStorage() { }
        public ZipStorage(Storage storage, string path)
        {
            Storage = storage;
            Path = path;
        }

        public void Initialize()
        {
            if (zipFile != null)
                return;

            zipFile = Storage.GetFile(Path);
            if (zipFile == null)
            {
                int separator = Path.LastIndexOf('/');
                Directory directory = Storage.GetDirectory(Path.Substring(0, separator + 1));
                zipFile = directory.CreateFile(Path.Substring(separator + 1));
            }

            Flush();
        }

        internal void Flush()
        {
            if (Archive != null)
                Archive.Dispose();

            zipStream = zipFile.Open(FileAccess.ReadWrite);
            Archive = new ZipArchive(zipStream, ZipArchiveMode.Update);
            root = new ZipRoot(this);
        }

        public override void Dispose()
        {
            if (Archive != null)
                Archive.Dispose();
        }

        public override string ToString()
        {
            return string.Format("Zip {{ Storage: {0}, Path: {1} }}", Storage, Path);
        }
    }
}