using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

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
                foreach (ZipEntry entry in storage.Zip.EntriesSorted)
                {
                    if (!entry.IsDirectory)
                        continue;

                    string name = entry.FileName.TrimEnd('/');
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
                foreach (ZipEntry entry in storage.Zip.EntriesSorted)
                {
                    if (entry.IsDirectory)
                        continue;
                    if (entry.FileName.Contains('/'))
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

        public override Directory CreateDirectory(string name)
        {
            ZipEntry entry = storage.Zip.AddDirectoryByName(name);
            return new ZipDirectory(storage, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            ZipDirectory zipDirectory = directory as ZipDirectory;
            storage.Zip.RemoveEntry(zipDirectory.directory);
        }

        public override File CreateFile(string name)
        {
            ZipEntry entry = storage.Zip.AddEntry(name, new byte[0]);
            return new ZipFile(storage, this, entry);
        }
        public override void DeleteFile(File file)
        {
            ZipFile zipFile = file as ZipFile;
            storage.Zip.RemoveEntry(zipFile.file);
        }
    }
}