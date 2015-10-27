using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpCompress.Archive.Tar;

namespace SmartSync.Common
{
    public class TarRoot : Directory
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
                foreach (TarArchiveEntry entry in storage.Archive.Entries)
                {
                    if (!entry.IsDirectory)
                        continue;

                    string name = entry.Key.TrimEnd('/');
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
                    if (entry.Key.Contains('/'))
                        continue;

                    yield return new TarFile(storage, this, entry);
                }
            }
        }

        private TarStorage storage;

        public TarRoot(TarStorage storage)
        {
            this.storage = storage;
        }

        public override Directory CreateDirectory(string name)
        {
            TarArchiveEntry entry = storage.Archive.AddEntry(name + "/", null);
            return new TarDirectory(storage, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override File CreateFile(string name)
        {
            TarArchiveEntry entry = storage.Archive.AddEntry(name, null);
            return new TarFile(storage, this, entry);
        }
        public override void DeleteFile(File file)
        {
            throw new NotImplementedException();
        }
    }
}