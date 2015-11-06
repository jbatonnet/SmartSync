using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using SmartSync.Common;
using SmartSync.GoogleDrive.Properties;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace SmartSync.GoogleDrive
{
    public class SettingsDataStore : IDataStore
    {
        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                Settings.Default.User = null;
                Settings.Default.Save();
            });
        }
        public Task DeleteAsync<T>(string key)
        {
            return Task.Run(() =>
            {
                switch (key)
                {
                    case "user": Settings.Default.User = null; break;
                    default:
                        throw new Exception("Unable to find the specified key in user settings : " + key);
                }
                
                Settings.Default.Save();
            });
        }
        public Task<T> GetAsync<T>(string key)
        {
            return Task.Run(() =>
            {
                string value = null;

                switch (key)
                {
                    case "user": value = Settings.Default.User; break;
                    default:
                        throw new Exception("Unable to find the specified key in user settings : " + key);
                }

                if (value == null)
                    return default(T);

                return JsonConvert.DeserializeObject<T>(value);
            });
        }
        public Task StoreAsync<T>(string key, T value)
        {
            return Task.Run(() =>
            {
                string json = JsonConvert.SerializeObject(value);

                switch (key)
                {
                    case "user": Settings.Default.User = json; break;
                    default:
                        throw new Exception("Unable to find the specified key in user settings : " + key);
                }

                Settings.Default.Save();
            });
        }
    }

    public class GoogleDriveStorage : Storage
    {
        static GoogleDriveStorage()
        {
            Bootstrap.Initialize();
        }

        private static string ApplicationName = "SmartSync";
        private static string[] Scopes = { DriveService.Scope.Drive };

        public string Path { get; set; } = "/";
        public bool UseTrash { get; set; } = true;
        public bool UseVersioning { get; set; } = true;

        public override Directory Root
        {
            get
            {
                Initialize();
                return new GoogleDriveDirectory(this, null, root);
            }
        }

        internal DriveService Service { get; private set; }
        internal UserCredential Credential { get; private set; }

        private Google.Apis.Drive.v2.Data.File root;

        private void Initialize()
        {
            if (Service != null)
                return;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SmartSync.GoogleDrive.ClientSecret.json"))
            {
                Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new SettingsDataStore()).Result;
            }
            
            // Create Drive API service.
            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = ApplicationName,
            });

            // Locate specified path
            if (!IsPathValid(Path) || Path[0] != '/')
                throw new Exception("The specified path is not a valid path");

            string[] parts = Path.Trim('/').Split('/');
            root = Service.Files.Get("root").Execute();

            foreach (string part in parts)
            {
                FilesResource.ListRequest request = Service.Files.List();
                request.Q = string.Format("'{0}' in parents and trashed = false and mimeType = '{1}' and title = '{2}'", root.Id, GoogleDriveDirectory.MimeType, part);
                request.Fields = "items(id,title,fileSize,mimeType)";

                Google.Apis.Drive.v2.Data.File[] folders = request.Execute().Items.ToArray();
                root = folders.SingleOrDefault();

                if (root == null)
                    throw new Exception("Could not find the specified folder");
            }
        }

        public override IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            Initialize();
            return GetAllDirectories_QueryEachLevel(exclusions);
        }
        private IEnumerable<Directory> GetAllDirectories_QueryAllDirectories(string[] exclusions = null)
        {
            FilesResource.ListRequest request = Service.Files.List();
            request.Q = string.Format("trashed = false and mimeType = '{0}'", GoogleDriveDirectory.MimeType);
            request.Fields = "items(id,title,parents,fileSize)";
            request.MaxResults = 1000;

            Google.Apis.Drive.v2.Data.File[] folders = request.Execute().Items.ToArray();
            Tuple<string, string>[] folderParents = folders.SelectMany(f => f.Parents.Select(p => new Tuple<string, string>(f.Id, p.Id))).ToArray();

            // Rebuild folder hierarchy
            List<Google.Apis.Drive.v2.Data.File> subFolders = new List<Google.Apis.Drive.v2.Data.File>() { root };
            while (true)
            {
                Google.Apis.Drive.v2.Data.File[] pass = subFolders.SelectMany(f => folderParents.Where(p => p.Item2 == f.Id).Select(p => folders.First(t => t.Id == p.Item1)))
                                                                  .Except(subFolders)
                                                                  .ToArray();
                if (pass.Length == 0)
                    break;

                subFolders.AddRange(pass);
            }

            // Wrap all folders and rebuild parents
            List<GoogleDriveDirectory> directories = new List<GoogleDriveDirectory>();
            foreach (Google.Apis.Drive.v2.Data.File folder in subFolders)
            {
                if (folder.Id == root.Id)
                    directories.Add(new GoogleDriveDirectory(this, null, folder));
                else
                    directories.Add(new GoogleDriveDirectory(this, directories.First(d => d.folder.Id == folder.Parents.Single().Id), folder));
            }

            // Return each directory
            foreach (GoogleDriveDirectory directory in directories)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(directory.Path, e)))
                    continue;

                yield return directory;
            }
        }
        private IEnumerable<Directory> GetAllDirectories_QueryEachLevel(string[] exclusions = null)
        {
            List<Google.Apis.Drive.v2.Data.File> folders = new List<Google.Apis.Drive.v2.Data.File>() { root };
            Google.Apis.Drive.v2.Data.File[] pass = new Google.Apis.Drive.v2.Data.File[] { root };

            // Rebuild folder hierarchy
            while (true)
            {
                string parentsQuery = string.Join(" or ", pass.Select(f => string.Format("'{0}' in parents", f.Id)));

                FilesResource.ListRequest request = Service.Files.List();
                request.Q = string.Format("trashed = false and mimeType = '{0}' and ({1})", GoogleDriveDirectory.MimeType, parentsQuery);
                request.Fields = "items(id,title,parents,fileSize)";
                request.MaxResults = 1000;

                pass = request.Execute().Items.ToArray();
                folders.AddRange(pass);

                if (pass.Length == 0)
                    break;
            }
            
            // Wrap all folders and rebuild parents
            List<GoogleDriveDirectory> directories = new List<GoogleDriveDirectory>();
            foreach (Google.Apis.Drive.v2.Data.File folder in folders)
            {
                if (folder.Id == root.Id)
                    directories.Add(new GoogleDriveDirectory(this, null, folder));
                else
                    directories.Add(new GoogleDriveDirectory(this, directories.First(d => d.folder.Id == folder.Parents.Single().Id), folder));
            }

            // Return each directory
            foreach (GoogleDriveDirectory directory in directories)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(directory.Path, e)))
                    continue;

                yield return directory;
            }
        }

        public override IEnumerable<File> GetAllFiles(string[] exclusions = null)
        {
            GoogleDriveDirectory[] directories = GetAllDirectories(exclusions).OfType<GoogleDriveDirectory>().ToArray();
            string parentsQuery = string.Join(" or ", directories.Select(d => string.Format("'{0}' in parents", d.folder.Id)));

            // Retrieve each directories files
            FilesResource.ListRequest request = Service.Files.List();
            request.Q = string.Format("trashed = false and mimeType != '{0}' and ({1})", GoogleDriveDirectory.MimeType, parentsQuery);
            request.Fields = "items(id,title,parents,fileSize,modifiedDate,md5Checksum,downloadUrl)";
            request.MaxResults = 1000;

            Google.Apis.Drive.v2.Data.File[] files = request.Execute().Items.ToArray();

            // Return each directory
            foreach (Google.Apis.Drive.v2.Data.File file in files)
            {
                GoogleDriveFile googleDriveFile = new GoogleDriveFile(this, directories.First(d => d.folder.Id == file.Parents.Single().Id), file);
                if (exclusions != null && exclusions.Any(e => MatchPattern(googleDriveFile.Path, e)))
                    continue;

                yield return googleDriveFile;
            }
        }

        public override string ToString()
        {
            return string.Format("Google Drive {{ Path: {0} }}", Path);
        }
    }
}