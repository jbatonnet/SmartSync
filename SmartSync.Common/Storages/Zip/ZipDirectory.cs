using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

namespace SmartSync.Common
{
    public class ZipDirectory : Directory
    {
        public override string Name
        {
            get
            {
                string name = "/" + directory.FileName.TrimEnd('/');
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
                return "/" + directory.FileName.TrimEnd('/');
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
                foreach (ZipEntry entry in storage.Zip.EntriesSorted)
                {
                    if (!entry.IsDirectory)
                        continue;
                    if (entry.FileName == directory.FileName)
                        continue;
                    if (!entry.FileName.StartsWith(directory.FileName))
                        continue;

                    string name = entry.FileName.Substring(directory.FileName.Length).TrimEnd('/');
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
                    if (!entry.FileName.StartsWith(directory.FileName))
                        continue;

                    string name = entry.FileName.Substring(directory.FileName.Length);
                    if (name.Contains('/'))
                        continue;

                    yield return new ZipFile(storage, this, entry);
                }
            }
        }

        internal ZipStorage storage;
        internal Directory parent;
        internal ZipEntry directory;

        public ZipDirectory(ZipStorage storage, Directory parent, ZipEntry directory)
        {
            this.storage = storage;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            ZipEntry entry = storage.Zip.AddDirectoryByName(directory.FileName + name);
            return new ZipDirectory(storage, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            ZipDirectory zipDirectory = directory as ZipDirectory;
            storage.Zip.RemoveEntry(zipDirectory.directory);
        }

        public override File CreateFile(string name)
        {
            ZipEntry entry = storage.Zip.AddEntry(directory.FileName + name, new byte[0]);
            return new ZipFile(storage, this, entry);
        }
        public override void DeleteFile(File file)
        {
            ZipFile zipFile = file as ZipFile;
            storage.Zip.RemoveEntry(zipFile.file);
        }
    }
}