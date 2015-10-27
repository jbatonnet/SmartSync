using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;
using SharpCompress.Archive.Tar;

namespace SmartSync.Common
{
    public class TarDirectory : Directory
    {
        public override string Name
        {
            get
            {
                string name = "/" + directory.Key.TrimEnd('/');
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
                return "/" + directory.Key.TrimEnd('/');
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
                foreach (TarArchiveEntry entry in storage.Archive.Entries)
                {
                    if (!entry.IsDirectory)
                        continue;
                    if (entry.Key == directory.Key)
                        continue;
                    if (!entry.Key.StartsWith(directory.Key))
                        continue;

                    string name = entry.Key.Substring(directory.Key.Length).TrimEnd('/');
                    if (name.Contains('/'))
                        continue;

                    yield return new TarDirectory(storage, this, entry);
                }
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (TarArchiveEntry entry in storage.Archive.Entries)
                {
                    if (entry.IsDirectory)
                        continue;
                    if (!entry.Key.StartsWith(directory.Key))
                        continue;

                    string name = entry.Key.Substring(directory.Key.Length);
                    if (name.Contains('/'))
                        continue;

                    yield return new TarFile(storage, this, entry);
                }
            }
        }

        internal TarStorage storage;
        internal Directory parent;
        internal TarArchiveEntry directory;

        public TarDirectory(TarStorage storage, Directory parent, TarArchiveEntry directory)
        {
            this.storage = storage;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            throw new NotImplementedException();
        }
        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override File CreateFile(string name)
        {
            throw new NotImplementedException();
        }
        public override void DeleteFile(File file)
        {
            throw new NotImplementedException();
        }
    }
}