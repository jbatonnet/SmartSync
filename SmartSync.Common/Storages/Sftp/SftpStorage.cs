using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace SmartSync.Common
{
    public class SftpStorage : Storage
    {
        public string Host { get; set; }
        public ushort Port { get; set; } = 22;

        public string User { get; set; }
        public string Password { get; set; }

        public string Path { get; set; } = "/";

        public override Directory Root
        {
            get
            {
                Initialize();
                return new SftpDirectory(this, null, Client.Get(Path));
            }
        }

        internal SftpClient Client { get; private set; }

        public SftpStorage() { }
        public SftpStorage(string host, string user, string password, string path = "/")
        {
            Host = host;
            User = user;
            Password = password;
            Path = path;
        }
        public SftpStorage(string host, ushort port, string user, string password, string path = "/")
        {
            Host = host;
            Port = port;
            User = user;
            Password = password;
            Path = path;
        }

        public void Initialize()
        {
            if (Client != null)
                return;

            Client = new SftpClient(Host, Port, User, Password);
            Client.Connect();
        }

        public override string ToString()
        {
            return string.Format("Sftp {{ Host: {0}, Port: {1} }}", Host, Port);
        }
    }
}