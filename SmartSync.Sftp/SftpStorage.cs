using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

using Renci.SshNet;
using System.Text.RegularExpressions;

namespace SmartSync.Sftp
{
    public class SftpStorage : Storage
    {
        private static Regex filePattern = new Regex(@"^(?<Permissions>[-drwx]{10})\s+[^\s]+\s+(?<User>[^\s]+)\s+(?<Group>[^\s]+)\s+(?<Size>[^\s]+)\s+(?<Date>[0-9-]{10} [0-9:.]+ [0-9+]+)\s+(?<Name>.+)$", RegexOptions.Compiled);

        public string Host { get; set; }
        public ushort Port { get; set; } = 22;

        public string User { get; set; }
        public string Password { get; set; }

        public string Path { get; set; } = "/";

        public string PostSync { get; set; }

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

            if (SshClient == null)
            {
                foreach (Directory directory in base.GetAllDirectories(exclusions))
                    yield return directory;

                yield break;
            }

            SshCommand command = SshClient.RunCommand("ls -alR --time-style=full-iso " + Path);
            if (command.ExitStatus < 0)
            {
                foreach (Directory directory in base.GetAllDirectories(exclusions))
                    yield return directory;

                yield break;
            }

            // Command output looks like this block
            //   /data/web/julien/www/Content:
            //   total 12
            //   drwxr-xr-x 3 pi pi 4096 2016-11-01 17:01:38.064671398 +0100 .
            //   drwxr-xr-x 9 pi pi 4096 2016-11-01 17:03:46.933370227 +0100 ..
            //   drwxr-xr-x 2 pi pi 4096 2016-11-01 17:01:54.284507630 +0100 Articles
            //   
            //   /data/web/julien/www/Content/Articles:
            //   total 72
            //   drwxr-xr-x 2 pi pi  4096 2016-11-01 17:01:54.284507630 +0100 .
            //   drwxr-xr-x 3 pi pi  4096 2016-11-01 17:01:38.064671398 +0100 ..
            //   -rw-r--r-- 1 pi pi 13414 2016-10-23 19:27:41.000000000 +0200 c-sharp-utilisation-linq.md
            //   -rw-r--r-- 1 pi pi 21944 2016-10-23 19:28:37.000000000 +0200 interoperabilite-systemes.md
            //   -rw-r--r-- 1 pi pi 11665 2016-10-27 22:38:12.000000000 +0200 securite-les-injections-sql.md
            //   -rw-r--r-- 1 pi pi  8651 2016-10-23 19:28:04.000000000 +0200 unification-contenu-fournisseur.md

            List<Directory> result = new List<Directory>();

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

                        PopulateFiles(directory, reader);
                        yield return directory;

                        continue;
                    }

                    // Children directories
                    string parentPath = Combine(path, "..");
                    SftpCachedDirectory parent = directories[parentPath];

                    directory = new SftpCachedDirectory(this, parent, path, name);
                    directories.Add(path, directory);
                    parent.directories.Add(directory);

                    path = directory.Path + "/";
                    if (exclusions != null && exclusions.Any(e => MatchPattern(path, e)))
                        continue;

                    PopulateFiles(directory, reader);
                    yield return directory;
                }
            }
        }
        private void PopulateFiles(SftpCachedDirectory directory, System.IO.StringReader reader)
        {
            while (true)
            {
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    break;
                if (line.StartsWith("total"))
                    continue;
                if (line.StartsWith("d"))
                    continue;

                // -rw-r--r-- 1 pi pi 11665 2016-10-27 22:38:12.000000000 +0200 securite-les-injections-sql.md
                Match match = filePattern.Match(line);
                if (!match.Success)
                    continue;

                string name = match.Groups["Name"].Value;
                string size = match.Groups["Size"].Value;
                string date = match.Groups["Date"].Value;

                SftpCachedFile file = new SftpCachedFile(directory.Storage as SftpStorage, directory, directory.path + "/" + name, name, ulong.Parse(size), DateTime.Parse(date));
                directory.files.Add(file);
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

        public override void EndSync()
        {
            if (!string.IsNullOrWhiteSpace(PostSync))
            {
                if (SshClient == null)
                {
                    Console.WriteLine("Could not run post sync script because no SSH session were created");
                    return;
                }

                Console.WriteLine("Running post sync script");

                SshCommand command = SshClient.RunCommand(PostSync);
                Console.WriteLine(command.Result);
            }
        }

        public override string ToString()
        {
            return string.Format("Sftp {{ Host: {0}, Port: {1}, Path: {2} }}", Host, Port, Path);
        }
    }
}