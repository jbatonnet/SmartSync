using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

using Renci.SshNet;

namespace SmartSync.Sftp
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
                return new SftpDirectory(this, null, SftpClient.Get(Path));
            }
        }

        internal SftpClient SftpClient { get; private set; }
        internal SshClient SshClient { get; private set; }

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
            if (SftpClient != null)
                return;

            SftpClient = new SftpClient(Host, Port, User, Password);
            SftpClient.Connect();

            // Try to setup a ssh connection to speedup file listing
            /*try
            {
                SshClient = new SshClient(Host, Port, User, Password);
                SshClient.Connect();
            }
            catch
            {
                SshClient = null;
            }*/
        }

        public override IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            Initialize();

            //if (SshClient == null)
            {
                foreach (Directory directory in base.GetAllDirectories(exclusions))
                    yield return directory;

                yield break;
            }

            /*SshCommand command = SshClient.RunCommand("ls -alR " + Path);
            if (command.ExitStatus != 0)
            {
                foreach (Directory directory in base.GetAllDirectories(exclusions))
                    yield return directory;

                yield break;
            }
            
            using (System.IO.StringReader reader = new System.IO.StringReader(command.Result))
            {
                Dictionary<string, SftpDirectory> directories = new Dictionary<string, SftpDirectory>();

                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (!line.EndsWith(":"))
                        continue;

                    string path = Combine(Path, line.Remove(line.Length - 1));
                    SftpDirectory directory;

                    if (path == Path)
                    {
                        directory = new SftpDirectory(this, null, SftpClient.Get(path));
                        directories.Add(path, directory);
                        continue;
                    }

                    string parentPath = Combine(path, "..");
                    SftpDirectory parent = directories[parentPath];

                    directory = new SftpDirectory(this, parent, SftpClient.Get(path));
                    directories.Add(path, directory);
                    yield return directory;
                }
            }*/
        }
        private static string Combine(string left, string right)
        {
            string path = System.IO.Path.Combine(left, right).Replace("\\", "/");
            List<string> parts = path.Split('/').ToList();

            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] == ".")
                {
                    parts.RemoveAt(i);
                    i--;
                    continue;
                }

                if (parts[i] == "..")
                {
                    if (i > 0)
                    {
                        parts.RemoveRange(i - 1, 2);
                        i -= 2;
                    }
                    continue;
                }
            }

            return string.Join("/", parts);
        }

        public override string ToString()
        {
            return string.Format("Sftp {{ Host: {0}, Port: {1} }}", Host, Port);
        }
    }
}