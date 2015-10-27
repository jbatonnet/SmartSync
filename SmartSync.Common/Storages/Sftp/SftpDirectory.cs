using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;

namespace SmartSync.Common
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
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (Renci.SshNet.Sftp.SftpFile file in storage.Client.ListDirectory(directory.FullName))
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
                foreach (Renci.SshNet.Sftp.SftpFile file in storage.Client.ListDirectory(directory.FullName))
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
            storage.Client.CreateDirectory(directory.FullName + "/" + name);
            return GetDirectory(name);
        }
        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override File CreateFile(string name)
        {
            storage.Client.Create(directory.FullName + "/" + name);
            return GetFile(name);
        }
        public override void DeleteFile(File file)
        {
            throw new NotImplementedException();
        }
    }
}