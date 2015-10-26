using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

namespace SmartSync.Engine
{
    public class ZipRoot : Directory
    {
        public override string Name
        {
            get
            {
                return "<Root>";
            }
        }
        public override Directory Parent
        {
            get
            {
                return null;
            }
        }
        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (ZipEntry entry in zip.EntriesSorted)
                {
                    if (!entry.IsDirectory)
                        continue;

                    string name = entry.FileName.TrimEnd('/');
                    if (name.Contains('/'))
                        continue;
                    
                    yield return new ZipDirectory(zip, this, entry);
                }
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (ZipEntry entry in zip.EntriesSorted)
                {
                    if (entry.IsDirectory)
                        continue;
                    if (entry.FileName.Contains('/'))
                        continue;

                    yield return new ZipFile(zip, this, entry);
                }
            }
        }

        private Ionic.Zip.ZipFile zip;

        public ZipRoot(Ionic.Zip.ZipFile zip)
        {
            this.zip = zip;
        }

        private void Initialize()
        {
        }

        public override Directory CreateDirectory(string name)
        {
            ZipEntry entry = zip.AddDirectoryByName(name);
            return new ZipDirectory(zip, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            ZipDirectory zipDirectory = directory as ZipDirectory;
            zip.RemoveEntry(zipDirectory.directory);
        }

        public override File CreateFile(string name)
        {
            ZipEntry entry = zip.AddEntry(name, new byte[0]);
            return new ZipFile(zip, this, entry);
        }
        public override void DeleteFile(File file)
        {
            ZipFile zipFile = file as ZipFile;
            zip.RemoveEntry(zipFile.file);
        }
    }
}