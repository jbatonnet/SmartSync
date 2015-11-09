using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.WindowsForms;
using SmartSync.OneDrive.Properties;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;

namespace SmartSync.OneDrive
{
    public class OneDriveInfoProvider : ServiceInfoProvider
    {
        public OneDriveInfoProvider() : base(new FormsWebAuthenticationUi()) { }

        public override async Task<ServiceInfo> GetServiceInfo(AppConfig appConfig, CredentialCache credentialCache, IHttpProvider httpProvider)
        {
            ServiceInfo serviceInfo = await base.GetServiceInfo(appConfig, credentialCache, httpProvider);

            if (credentialCache.cacheDictionary.Count > 0)
            {
                var credentialPair = credentialCache.cacheDictionary.First();
                serviceInfo.UserId = credentialPair.Key.UserId;
            }

            return serviceInfo;
        }
    }

    public class OneDriveStorage : Storage
    {
        private const string applicationId = "000000004416B479";
        private const string applicationReturnUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly static string[] applicationScopes = new[] { "wl.signin", "wl.offline_access", "onedrive.readwrite" };
        private const string applicationSecret = "GYdcxFCOMNAdwA2Ha5PIHFX-Xs0klVex";

        public string Path { get; set; } = "/";
        public override Directory Root
        {
            get
            {
                Initialize();
                return new OneDriveDirectory(this, null, root);
            }
        }

        internal IOneDriveClient Client { get; private set; }
        internal AccountSession Session { get; private set; }

        private Item root;

        private void Initialize()
        {
            if (Client != null)
                return;

            // Load credential cache
            CredentialCache credentialCache = new CredentialCache();
            if (!string.IsNullOrEmpty(Settings.Default.Credentials))
                credentialCache.InitializeCacheFromBlob(Convert.FromBase64String(Settings.Default.Credentials));

            // Authenticate with OneDrive
            Client = OneDriveClient.GetMicrosoftAccountClient(applicationId, applicationReturnUrl, applicationScopes, applicationSecret, credentialCache, null, new OneDriveInfoProvider());
            Task<AccountSession> oneDriveSessionTask = Client.AuthenticateAsync();
            oneDriveSessionTask.Wait();
            Session = oneDriveSessionTask.Result;

            // Save credentials
            if (Session == null)
            {
                Settings.Default.Credentials = null;
                Settings.Default.Save();
            }
            else if (credentialCache.HasStateChanged)
            {
                Settings.Default.Credentials = Convert.ToBase64String(credentialCache.GetCacheBlob());
                Settings.Default.Save();
            }

            // Find specified root folder
            if (!Path.StartsWith("/") || !IsPathValid(Path))
                throw new Exception("The specified path is not valid");

            Task<Item> task;
            if (Path.Length == 1)
                task = Client.Drive.Root.Request().GetAsync();
            else
                task = Client.Drive.Root.ItemWithPath(Path.Trim('/')).Request().GetAsync();

            task.Wait();
            root = task.Result;
        }

        public /*override*/ Directory _GetDirectory(string path)
        {
            Initialize();

            if (!Path.StartsWith("/") || !IsPathValid(Path))
                throw new Exception("The specified path is not valid");

            path = Path.Trim('/') + "/" + path.Trim('/');

            Task<Item> task = Client.Drive.Root.ItemWithPath(path).Request().GetAsync();
            task.Wait();

            // TODO: Rebuild directory hierarchy

            return new OneDriveDirectory(this, null, task.Result);
        }
        public /*override*/ Common.File _GetFile(string path)
        {
            Initialize();

            if (!Path.StartsWith("/") || !IsPathValid(Path) || Path.EndsWith("/"))
                throw new Exception("The specified path is not valid");

            path = Path.Trim('/') + "/" + path.Trim('/');

            Task<Item> task = Client.Drive.Root.ItemWithPath(path).Request().GetAsync();
            task.Wait();

            // TODO: Rebuild file hierarchy

            return new OneDriveFile(this, null, task.Result);
        }

        public /*override*/ IEnumerable<Directory> _GetAllDirectories(string[] exclusions = null)
        {
            Initialize();

            Task<IItemSearchCollectionPage> task = Client.Drive.Items[root.Id].Search("Excel").Request().GetAsync();
            task.Wait();

            foreach (Item item in task.Result)
            {
                item.ToString();
            }

            yield break;
        }

        public override string ToString()
        {
            return string.Format("OneDrive {{ Path: {0} }}", Path);
        }
    }
}