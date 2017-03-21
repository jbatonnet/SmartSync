using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

using Renci.SshNet;

namespace SmartSync.Sftp
{
    public class SftpDirectory : Directory
    {
        public override string Name
        {
            get
            {
                return directory.Name;
            }
            set
            {
                throw new NotImplementedException();
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
                foreach (Renci.SshNet.Sftp.SftpFile file in storage.SftpClient.ListDirectory(directory.FullName))
                {
                    if (file.Name == "." || file.Name == "..")
                        continue;

                    if (file.Attributes.IsDirectory)
                        yield return new SftpDirectory(storage, this, file);
                }
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (Renci.SshNet.Sftp.SftpFile file in storage.SftpClient.ListDirectory(directory.FullName))
                    if (file.Attributes.IsRegularFile)
                        yield return new SftpFile(storage, this, file);
            }
        }

        internal SftpStorage storage;
        internal SftpDirectory parent;
        internal Renci.SshNet.Sftp.SftpFile directory;

        public SftpDirectory(SftpStorage storage, SftpDirectory parent, Renci.SshNet.Sftp.SftpFile directory)
        {
            this.storage = storage;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            string path = directory.FullName + "/" + name;
            storage.SftpClient.CreateDirectory(path);

            Renci.SshNet.Sftp.SftpFile result = storage.SftpClient.Get(path);
            if (result == null)
                throw new System.IO.IOException("Unable to create the specified directory");

            return new SftpDirectory(storage, this, result);
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            string path = this.directory.FullName + "/" + directory.Name;
            storage.SftpClient.DeleteDirectory(path);
        }

        public override File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            string path = directory.FullName + "/" + name;
            storage.SftpClient.Create(path).Close();

            Renci.SshNet.Sftp.SftpFile result = storage.SftpClient.Get(path);
            if (result == null)
                throw new System.IO.IOException("Unable to create the specified file");

            return new SftpFile(storage, this, result);
        }
        public override void DeleteFile(File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified file could not be found");

            string path = directory.FullName + "/" + file.Name;
            storage.SftpClient.DeleteFile(path);
        }
    }

    public class SftpCachedDirectory : Directory
    {
        public override string Name
        {
            get
            {
                return name;
            }
            set
            {
                throw new NotImplementedException();
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
                return directories;
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                return files;
            }
        }

        internal SftpStorage storage;
        internal Directory parent;

        internal string path;
        private string name;
        internal List<SftpCachedDirectory> directories;
        internal List<SftpCachedFile> files;

        public SftpCachedDirectory(SftpStorage storage, Directory parent, string path, string name)
        {
            this.storage = storage;
            this.parent = parent;

            this.path = path;
            this.name = name;
            this.directories = new List<SftpCachedDirectory>();
            this.files = new List<SftpCachedFile>();
        }

        public override Directory CreateDirectory(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            string path = this.path + "/" + name;
            storage.SftpClient.CreateDirectory(path);

            Renci.SshNet.Sftp.SftpFile result = storage.SftpClient.Get(path);
            if (result == null)
                throw new System.IO.IOException("Unable to create the specified directory");

            return null; // new SftpDirectory(storage, this, result); // FIXME: Generate cached directory
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            string path = this.path + "/" + directory.Name;
            storage.SftpClient.DeleteDirectory(path);
        }

        public override File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            string path = this.path + "/" + name;
            storage.SftpClient.Create(path).Close();

            Renci.SshNet.Sftp.SftpFile result = storage.SftpClient.Get(path);
            if (result == null)
                throw new System.IO.IOException("Unable to create the specified file");

            return null; // new SftpFile(storage, this, result); // FIXME: Generate cached file
        }
        public override void DeleteFile(File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified file could not be found");

            string path = this.path + "/" + file.Name;
            storage.SftpClient.DeleteFile(path);
        }
    }
}