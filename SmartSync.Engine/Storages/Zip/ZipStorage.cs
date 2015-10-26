using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

namespace SmartSync.Engine
{
    public class ZipStorage : Storage
    {
        public Storage Storage { get; set; }
        public string Path { get; set; }

        public override Directory Root
        {
            get
            {
                Initialize();
                return root;
            }
        }

        private File zipFile;
        private Stream zipStream;
        private Ionic.Zip.ZipFile zip;
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
            zip = Ionic.Zip.ZipFile.Read(zipStream);

            root = new ZipRoot(zip);
        }

        public override void Dispose()
        {
            Initialize();

            using (Stream stream = zipFile.Open(FileAccess.Write))
                zip.Save(stream);
        }
    }
}