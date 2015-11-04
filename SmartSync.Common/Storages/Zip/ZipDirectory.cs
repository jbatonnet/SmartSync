using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class ZipDirectory : Directory
    {
        public override string Name
        {
            get
            {
                string name = "/" + directory.FullName.TrimEnd('/');
                return name.Substring(name.LastIndexOf('/') + 1);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override string Path
        {
            get
            {
                return "/" + directory.FullName.TrimEnd('/');
            }
        }
        public override Directory Parent
        {
            get
            {
                return parent;
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
                    if (!entry.FullName.StartsWith(directory.FullName))
                        continue;
                    if (entry.FullName == directory.FullName)
                        continue;

                    string name = entry.FullName.Substring(directory.FullName.Length).TrimEnd('/');
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
                    if (entry.FullName.EndsWith("/"))
                        continue;
                    if (!entry.FullName.StartsWith(directory.FullName))
                        continue;

                    string name = entry.FullName.Substring(directory.FullName.Length);
                    if (name.Contains('/'))
                        continue;

                    yield return new ZipFile(storage, this, entry);
                }
            }
        }

        internal ZipStorage storage;
        internal Directory parent;
        internal ZipArchiveEntry directory;

        public ZipDirectory(ZipStorage storage, Directory parent, ZipArchiveEntry directory)
        {
            this.storage = storage;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            ZipArchiveEntry entry = storage.Archive.CreateEntry(directory.FullName + name + "/");
            return new ZipDirectory(storage, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            ZipDirectory zipDirectory = directory as ZipDirectory;
            zipDirectory.directory.Delete();
        }

        public override File CreateFile(string name)
        {
            ZipArchiveEntry entry = storage.Archive.CreateEntry(directory.FullName + name, storage.Compression);
            return new ZipFile(storage, this, entry);
        }
        public override void DeleteFile(File file)
        {
            ZipFile zipFile = file as ZipFile;
            zipFile.file.Delete();
        }
    }
}