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
                Func<ZipArchiveEntry, bool> filter = e =>
                {
                    string entryFullName = e.FullName;

                    // Skip files
                    if (entryFullName[entryFullName.Length - 1] != '/')
                        return false;

                    // Skip non direct children
                    for (int i = 0; i < entryFullName.Length - 1; i++)
                        if (entryFullName[i] == '/')
                            return false;

                    return true;
                };

                return storage.Archive.Entries.AsParallel()
                                              .Where(filter)
                                              .Select(e => new ZipDirectory(storage, this, e));
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                Func<ZipArchiveEntry, bool> filter = e =>
                {
                    string entryFullName = e.FullName;

                    // Skip directories
                    if (entryFullName[entryFullName.Length - 1] == '/')
                        return false;

                    // Skip non direct children
                    for (int i = 0; i < entryFullName.Length - 1; i++)
                        if (entryFullName[i] == '/')
                            return false;

                    return true;
                };

                return storage.Archive.Entries.AsParallel()
                                              .Where(filter)
                                              .Select(e => new ZipFile(storage, this, e));
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