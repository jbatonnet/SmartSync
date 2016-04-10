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
            try
            {
                SshClient = new SshClient(Host, Port, User, Password);
                SshClient.Connect();
            }
            catch
            {
                SshClient = null;
            }
        }

        public override IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            Initialize();

            if (true) // SshClient == null)
            {
                foreach (Directory directory in base.GetAllDirectories(exclusions))
                    yield return directory;

                yield break;
            }
            else
            {
                SshCommand command = SshClient.RunCommand("ls -alR " + Path);
                if (command.ExitStatus != 0)
                {
                    foreach (Directory directory in base.GetAllDirectories(exclusions))
                        yield return directory;

                    yield break;
                }

                // Command output lokks like this block
                //   /data/sync/projects/TramUrWay/TramUrWay.Android/obj/Debug/resourcecache/A749388BD973E6B4D7AC19568DB99B28:
                //   total 1016
                //   drwxrwxrwx 1 pi pi    4096 Apr  7 14:10 .
                //   drwxrwxrwx 1 pi pi    4096 Apr  7 14:10 ..
                //   drwxrwxrwx 1 pi pi       0 Apr  7 14:11 aapt
                //   drwxrwxrwx 1 pi pi       0 Apr  7 14:11 aidl
                //   -rwxrwxrwx 1 pi pi     852 Apr  7 09:34 AndroidManifest.xml
                //   -rwxrwxrwx 1 pi pi    4047 Apr  7 09:34 annotations.zip
                //   drwxrwxrwx 1 pi pi       0 Apr  7 14:10 assets
                //   -rwxrwxrwx 1 pi pi 1022851 Apr  7 09:34 classes.jar
                //   drwxrwxrwx 1 pi pi       0 Apr  7 14:11 libs
                //   drwxrwxrwx 1 pi pi       0 Apr  7 14:10 res

                using (System.IO.StringReader reader = new System.IO.StringReader(command.Result))
                {
                    Dictionary<string, SftpCachedDirectory> directories = new Dictionary<string, SftpCachedDirectory>();

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;
                        if (!line.EndsWith(":"))
                            continue;

                        string path = Combine(Path, line.Remove(line.Length - 1));
                        string name = System.IO.Path.GetFileName(path);
                        SftpCachedDirectory directory;

                        // Root directory
                        if (path == Path)
                        {
                            directory = new SftpCachedDirectory(this, null, path, name);
                            directories.Add(path, directory);
                            yield return directory;
                            continue;
                        }

                        // Children directories
                        string parentPath = Combine(path, "..");
                        SftpCachedDirectory parent = directories[parentPath];

                        directory = new SftpCachedDirectory(this, parent, path, name);
                        directories.Add(path, directory);
                        parent.directories.Add(directory);

                        if (exclusions != null && exclusions.Any(e => MatchPattern(path + "/", e)))
                            continue;
                        yield return directory;

                        // Files
                        while (true)
                        {
                            line = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line))
                                break;
                            if (line.StartsWith("total"))
                                continue;
                            if (line.StartsWith("d"))
                                continue;

                            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length < 9)
                                continue;

                            //string name = 
                        }
                    }
                }
            }
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