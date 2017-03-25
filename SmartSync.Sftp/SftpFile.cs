using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bedrock.Common;

using Renci.SshNet;

namespace SmartSync.Sftp
{
    public class SftpFile : File
    {
        public override string Name
        {
            get
            {
                return file.Name;
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

        public override DateTime Date
        {
            get
            {
                return file.LastWriteTime;
            }
            set
            {
                Renci.SshNet.Sftp.SftpFileAttributes attributes = storage.SftpClient.GetAttributes(file.FullName);

                attributes.LastWriteTime = value;

                storage.SftpClient.SetAttributes(file.FullName, attributes);
            }
        }
        public override ulong Size
        {
            get
            {
                return (ulong)file.Length;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private SftpStorage storage;
        private SftpDirectory parent;
        private Renci.SshNet.Sftp.SftpFile file;

        public SftpFile(SftpStorage storage, SftpDirectory parent, Renci.SshNet.Sftp.SftpFile file)
        {
            this.storage = storage;
            this.parent = parent;
            this.file = file;
        }

        public override System.IO.Stream Open(System.IO.FileAccess access)
        {
            return storage.SftpClient.Open(file.FullName, System.IO.FileMode.Open, access);
        }
    }

    public class SftpCachedFile : File
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

        public override DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                Renci.SshNet.Sftp.SftpFileAttributes attributes = storage.SftpClient.GetAttributes(path);

                attributes.LastWriteTime = value;
                date = value;

                storage.SftpClient.SetAttributes(path, attributes);
            }
        }
        public override ulong Size
        {
            get
            {
                return size;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private SftpStorage storage;
        private SftpCachedDirectory parent;

        private string path;
        private string name;
        private ulong size;
        private DateTime date;

        public SftpCachedFile(SftpStorage storage, SftpCachedDirectory parent, string path, string name, ulong size, DateTime date)
        {
            this.storage = storage;
            this.parent = parent;

            this.path = path;
            this.name = name;
            this.size = size;
            this.date = date;
        }

        public override System.IO.Stream Open(System.IO.FileAccess access)
        {
            return storage.SftpClient.Open(path, System.IO.FileMode.Open, access);
        }
    }
}