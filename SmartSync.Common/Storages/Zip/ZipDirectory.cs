using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

using Bedrock.Common;

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
                string directoryFullName = directory.FullName;

                Func<ZipArchiveEntry, bool> filter = e =>
                {
                    string entryFullName = e.FullName;

                    // Skip files
                    if (entryFullName[entryFullName.Length - 1] != '/')
                        return false;

                    // Skip non subdirectories
                    if (entryFullName.Length <= directoryFullName.Length)
                        return false;

                    for (int i = directoryFullName.Length - 1; i >= 0; i--)
                        if (entryFullName[i] != directoryFullName[i])
                            return false;

                    // Skip non direct children
                    for (int i = directoryFullName.Length; i < entryFullName.Length - 1; i++)
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
                string directoryFullName = directory.FullName;

                Func<ZipArchiveEntry, bool> filter = e =>
                {
                    string entryFullName = e.FullName;

                    // Skip directories
                    if (entryFullName[entryFullName.Length - 1] == '/')
                        return false;

                    // Skip non subdirectories
                    if (e.FullName.Length <= directoryFullName.Length)
                        return false;

                    for (int i = directoryFullName.Length - 1; i >= 0; i--)
                        if (e.FullName[i] != directoryFullName[i])
                            return false;

                    // Skip non direct children
                    for (int i = directoryFullName.Length; i < e.FullName.Length - 1; i++)
                        if (e.FullName[i] == '/')
                            return false;

                    return true;
                };

                return storage.Archive.Entries.AsParallel()
                                              .Where(filter)
                                              .Select(e => new ZipFile(storage, this, e));
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
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            ZipArchiveEntry entry = storage.Archive.CreateEntry(directory.FullName + name + "/");
            if (entry == null)
                throw new System.IO.IOException("Unable to create the specified directory");

            storage.Modified = true;
            return new ZipDirectory(storage, this, entry);
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            ZipDirectory zipDirectory = directory as ZipDirectory;
            zipDirectory.directory.Delete();

            storage.Modified = true;
        }

        public override File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            ZipArchiveEntry entry = storage.Archive.CreateEntry(directory.FullName + name, storage.Compression);
            if (entry == null)
                throw new System.IO.IOException("Unable to create the specified file");

            storage.Modified = true;
            return new ZipFile(storage, this, entry);
        }
        public override void DeleteFile(File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            ZipFile zipFile = file as ZipFile;
            zipFile.file.Delete();

            storage.Modified = true;
        }
    }
}