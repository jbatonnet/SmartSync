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
            zipStream = zipFile.Open(FileAccess.ReadWrite);
            Archive = new ZipArchive(zipStream, ZipArchiveMode.Update);

            root = new ZipRoot(this);
        }

        public override void Dispose()
        {
            Initialize();
            Archive.Dispose();
        }
    }
}