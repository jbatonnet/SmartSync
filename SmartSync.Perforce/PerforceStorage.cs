using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Perforce.P4;
using Microsoft.Win32;

using SmartSync.Common;

namespace SmartSync.Perforce
{
    using File = System.IO.File;

    public class PerforceStorage : BasicStorage
    {
        private const string AddressKey = "P4PORT";
        private const string UserKey = "P4USER";
        private const string PasswordKey = "P4PASSWD";
        private const string ClientKey = "P4CLIENT";

        public string Address { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Client { get; set; }

        public bool Submit { get; set; } = false;
        public string Changelist { get; set; } = "Synchronization by SmartSync";

        public override Common.Directory Root
        {
            get
            {
                Initialize();
                return new PerforceDirectory(this, Path);
            }
        }

        internal Server Server { get; private set; }
        internal Repository Repository { get; private set; }
        internal Connection Connection { get; private set; }

        private void Initialize()
        {
            if (Repository != null)
                return;

            string address = Address;
            string user = User;
            string password = Password;
            string client = Client;

            #region Server information discovery

            // Get environment informations
            if (address == null)
            {
                address = Environment.GetEnvironmentVariable(AddressKey);
                user = Environment.GetEnvironmentVariable(UserKey);
                password = Environment.GetEnvironmentVariable(PasswordKey);
                client = Environment.GetEnvironmentVariable(ClientKey);
            }

            // Try to find a p4config.txt file
            if (address == null)
            {
                DirectoryInfo directory = Path;

                while (directory != null && !File.Exists(System.IO.Path.Combine(directory.FullName, "p4config.txt")))
                    directory = directory.Parent;

                if (directory != null)
                {
                    string[] lines = File.ReadAllLines(System.IO.Path.Combine(directory.FullName, "p4config.txt"));

                    foreach (string line in lines)
                    {
                        int separator = line.IndexOf('=');
                        if (separator == -1)
                            continue;

                        string key = line.Substring(0, separator).Trim();
                        string value = line.Substring(separator + 1).Trim();

                        switch (key)
                        {
                            case AddressKey: address = value; break;
                            case UserKey: user = value; break;
                            case PasswordKey: password = value; break;
                            case ClientKey: client = value; break;
                        }
                    }
                }
            }

            // Find the registry key
            if (address == null)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Perforce\environment", false);

                if (key != null)
                {
                    address = key.GetValue(AddressKey, null) as string;
                    user = key.GetValue(UserKey, null) as string;
                    password = key.GetValue(PasswordKey, null) as string;
                    client = key.GetValue(ClientKey, null) as string;
                }
            }

            // Try to detect user if necessary
            if (user == null)
                user = Environment.UserName;

            // Just fail of we were not able to find server information
            if (address == null || user == null)
                throw new Exception("Unable to find Perforce server information");

            #endregion

            // Setup the server
            Server = new Server(new ServerAddress(address));
            Repository = new Repository(Server);
            Connection = Repository.Connection;

            // Try to detect client if necessary
            if (client == null)
            {
                Client[] clients = Repository.GetClients(new ClientsCmdOptions(ClientsCmdFlags.None, user, null, 0, null))?.ToArray();
                client = clients.FirstOrDefault(c => Path.FullName.ToLower().StartsWith(c.Root.ToLower()))?.Name;
            }

            // Setup the connection
            Connection.UserName = user;
            Connection.Client = new Client();
            Connection.Client.Name = client;

            Options options = new Options();
            options["ProgramName"] = Assembly.GetExecutingAssembly().GetName().Name;
            options["ProgramVersion"] = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Perform the connection
            Connection.Connect(options);
        }

        public override IEnumerable<Common.Directory> GetAllDirectories(string[] exclusions = null)
        {
            return base.GetAllDirectories(exclusions);
        }
        public override IEnumerable<Common.File> GetAllFiles(string[] exclusions = null)
        {
            return base.GetAllFiles(exclusions);
        }
        public override Common.Directory GetDirectory(string path)
        {
            return base.GetDirectory(path);
        }
        public override Common.File GetFile(string path)
        {
            return base.GetFile(path);
        }
    }
}