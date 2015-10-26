using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;

namespace SmartSync.Engine
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
        public override DateTime Date
        {
            get
            {
                return file.LastWriteTime;
            }
            set
            {
                file.LastWriteTime = value;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private SftpClient client;
        private SftpDirectory parent;
        private Renci.SshNet.Sftp.SftpFile file;

        public SftpFile(SftpClient client, SftpDirectory parent, Renci.SshNet.Sftp.SftpFile file)
        {
            this.client = client;
            this.parent = parent;
            this.file = file;
        }

        public override Stream Open(FileAccess access)
        {
            return client.Open(file.FullName, FileMode.Open, access);
        }
    }
}