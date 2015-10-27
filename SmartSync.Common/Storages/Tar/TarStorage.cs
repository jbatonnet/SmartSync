using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpCompress.Archive.Tar;
using SharpCompress.Common;

namespace SmartSync.Common
{
    public class TarStorage : Storage
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

        internal TarArchive Archive { get; private set; }

        private File tarFile;
        private Stream tarStream;
        private TarRoot root;

        public TarStorage() { }
        public TarStorage(Storage storage, string path)
        {
            Storage = storage;
            Path = path;
        }

        public void Initialize()
        {
            if (tarFile != null)
                return;

            tarFile = Storage.GetFile(Path);
            tarStream = tarFile.Open(FileAccess.Read);
            Archive = TarArchive.Open(tarStream);

            root = new TarRoot(this);
        }

        public override void Dispose()
        {
            Initialize();

            using (Stream stream = tarFile.Open(FileAccess.Write))
                Archive.SaveTo(stream, null);
        }
    }
}