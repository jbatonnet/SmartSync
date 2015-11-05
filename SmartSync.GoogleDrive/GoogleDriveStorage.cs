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
        private static string ApplicationName = "SmartSync";
        private static string[] Scopes = { DriveService.Scope.Drive };

        public string Path { get; set; } = "/";
        public bool UseTrash { get; set; } = true;
        public override Directory Root
        {
            get
            {
                Initialize();
                return new GoogleDriveDirectory(this, null, root);
            }
        }

        internal DriveService Service { get; private set; }
        private Google.Apis.Drive.v2.Data.File root;

        private void Initialize()
        {
            if (Service != null)
                return;

            UserCredential credential;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SmartSync.GoogleDrive.ClientSecret.json"))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new SettingsDataStore()).Result;
            }

            // Create Drive API service.
            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
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

        /*public override IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            Initialize();

            FilesResource.ListRequest request = Service.Files.List();
            request.Q = string.Format("trashed = false and mimeType = '{0}'", GoogleDriveDirectory.MimeType);
            request.Fields = "items(id,title,fileSize,mimeType)";

            Google.Apis.Drive.v2.Data.File[] folders = request.Execute().Items.ToArray();
            foreach (Google.Apis.Drive.v2.Data.File folder in folders)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(folder.Title, e))) // FIXME: Filter exclusions on full path
                    continue;

                yield return new GoogleDriveDirectory(this, folder);
            }
        }
        public override IEnumerable<File> GetAllFiles(string[] exclusions = null)
        {
            Initialize();

            FilesResource.ListRequest request = Service.Files.List();
            request.Q = string.Format("trashed = false and mimeType != '{0}'", GoogleDriveDirectory.MimeType);
            request.Fields = "items(id,title,fileSize,mimeType)";

            Google.Apis.Drive.v2.Data.File[] files = request.Execute().Items.ToArray();
            foreach (Google.Apis.Drive.v2.Data.File file in files)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(file.Title, e))) // FIXME: Filter exclusions on full path
                    continue;

                yield return new GoogleDriveFile(this, file);
            }
        }*/
    }
}