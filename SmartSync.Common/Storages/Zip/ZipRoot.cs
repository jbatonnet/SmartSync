using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class ZipRoot : Directory
    {
        public override string Name
        {
            get
            {
                return "<Root>";
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override Directory Parent
        {
            get
            {
                return null;
            }
        }
        public override Storage Storage
        {
            get
            {
                return storage;
            }
        }

        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (ZipArchiveEntry entry in storage.Archive.Entries)
                {
                    if (!entry.FullName.EndsWith("/"))
                        continue;

                    string name = entry.FullName.TrimEnd('/');
                    if (name.Contains('/'))
                        continue;
                    
                    yield return new ZipDirectory(storage, this, entry);
                }
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (ZipArchiveEntry entry in storage.Archive.Entries)
                {
                    if (entry.FullName.Contains('/'))
                        continue;

                    yield return new ZipFile(storage, this, entry);
                }
            }
        }

        private ZipStorage storage;

        public ZipRoot(ZipStorage storage)
        {
            this.storage = storage;
        }

        private void Initialize()
        {
        }

        public override Directory GetDirectory(string path)
        {
            ZipArchiveEntry entry = storage.Archive.GetEntry(path.TrimEnd('/') + "/");
            return new ZipDirectory(storage, null, entry);
        }

        public override Directory CreateDirectory(string name)
        {
            ZipArchiveEntry entry = storage.Archive.CreateEntry(name + "/");
            return new ZipDirectory(storage, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            ZipDirectory zipDirectory = directory as ZipDirectory;
            zipDirectory.directory.Delete();
        }

        public override File CreateFile(string name)
        {
            ZipArchiveEntry entry = storage.Archive.CreateEntry(name, storage.Compression);
            return new ZipFile(storage, this, entry);
        }
        public override void DeleteFile(File file)
        {
            ZipFile zipFile = file as ZipFile;
            zipFile.file.Delete();
        }
    }
}