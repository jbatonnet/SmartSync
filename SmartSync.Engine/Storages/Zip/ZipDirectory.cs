using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

namespace SmartSync.Engine
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
        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (ZipEntry entry in zip.EntriesSorted)
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
                    if (!entry.FileName.StartsWith(directory.FileName))
                        continue;

                    string name = entry.FileName.Substring(directory.FileName.Length);
                    if (name.Contains('/'))
                        continue;

                    yield return new ZipFile(zip, this, entry);
                }
            }
        }

        internal Ionic.Zip.ZipFile zip;
        internal Directory parent;
        internal ZipEntry directory;

        public ZipDirectory(Ionic.Zip.ZipFile zip, Directory parent, ZipEntry directory)
        {
            this.zip = zip;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            ZipEntry entry = zip.AddDirectoryByName(directory.FileName + name);
            return new ZipDirectory(zip, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            ZipDirectory zipDirectory = directory as ZipDirectory;
            zip.RemoveEntry(zipDirectory.directory);
        }

        public override File CreateFile(string name)
        {
            ZipEntry entry = zip.AddEntry(directory.FileName + name, new byte[0]);
            return new ZipFile(zip, this, entry);
        }
        public override void DeleteFile(File file)
        {
            ZipFile zipFile = file as ZipFile;
            zip.RemoveEntry(zipFile.file);
        }
    }
}