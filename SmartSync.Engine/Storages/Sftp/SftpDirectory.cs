using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;

namespace SmartSync.Engine
{
    public class SftpDirectory : Directory
    {
        public override string Name
        {
            get
            {
                return directory.Name;
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
                foreach (Renci.SshNet.Sftp.SftpFile file in client.ListDirectory(directory.FullName))
                {
                    if (file.Name == "." || file.Name == "..")
                        continue;

                    if (file.Attributes.IsDirectory)
                        yield return new SftpDirectory(client, this, file);
                }
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (Renci.SshNet.Sftp.SftpFile file in client.ListDirectory(directory.FullName))
                    if (file.Attributes.IsRegularFile)
                        yield return new SftpFile(client, this, file);
            }
        }

        private SftpClient client;
        private SftpDirectory parent;
        private Renci.SshNet.Sftp.SftpFile directory;

        public SftpDirectory(SftpClient client, SftpDirectory parent, Renci.SshNet.Sftp.SftpFile directory)
        {
            this.client = client;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            client.CreateDirectory(directory.FullName + "/" + name);
            return GetDirectory(name);
        }
        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override File CreateFile(string name)
        {
            client.Create(directory.FullName + "/" + name);
            return GetFile(name);
        }
        public override void DeleteFile(File file)
        {
            throw new NotImplementedException();
        }
    }
}