using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;
using Ionic.Zlib;

namespace SmartSync.Common
{
    public class ZipStorage : Storage
    {
        public Storage Storage { get; set; }
        public string Path { get; set; }
        //public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Default;

        public override Directory Root
        {
            get
            {
                Initialize();
                return root;
            }
        }

        internal Ionic.Zip.ZipFile Zip { get; private set; }

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
            zipStream = zipFile.Open(FileAccess.Read);
            Zip = Ionic.Zip.ZipFile.Read(zipStream);

            //Zip.CompressionMethod = CompressionMethod.Deflate;
            //Zip.CompressionLevel = CompressionLevel;

            root = new ZipRoot(this);
        }

        public override void Dispose()
        {
            Initialize();

            using (Stream stream = zipFile.Open(FileAccess.Write))
                Zip.Save(stream);
        }
    }
}