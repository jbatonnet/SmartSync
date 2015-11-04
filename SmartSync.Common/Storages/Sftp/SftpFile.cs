using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;

namespace SmartSync.Common
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
                Renci.SshNet.Sftp.SftpFileAttributes attributes = storage.Client.GetAttributes(file.FullName);

                attributes.LastWriteTime = value;

                storage.Client.SetAttributes(file.FullName, attributes);
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

        public override Stream Open(FileAccess access)
        {
            return storage.Client.Open(file.FullName, FileMode.Open, access);
        }
    }
}