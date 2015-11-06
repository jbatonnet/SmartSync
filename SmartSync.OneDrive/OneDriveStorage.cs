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

namespace SmartSync.OneDrive
{
    public class OneDriveCredentialCache : CredentialCache
    {
        public OneDriveCredentialCache()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Credentials))
                InitializeCacheFromBlob(JsonConvert.DeserializeObject<byte[]>(Settings.Default.Credentials));

            BeforeAccess = CredentialCache_BeforeAccess;
            AfterAccess = CredentialCache_AfterAccess;
        }

        private void CredentialCache_BeforeAccess(CredentialCacheNotificationArgs args)
        {
            if (!string.IsNullOrEmpty(Settings.Default.Credentials))
                args.CredentialCache.InitializeCacheFromBlob(JsonConvert.DeserializeObject<byte[]>(Settings.Default.Credentials));
        }
        private void CredentialCache_AfterAccess(CredentialCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                Settings.Default.Credentials = JsonConvert.SerializeObject(GetCacheBlob());
                Settings.Default.Save();

                HasStateChanged = false;
            }
        }
    }

    public class OneDriveStorage : Storage
    {
        private const string returnUrl = "https://login.live.com/oauth20_desktop.srf";
        private static string[] scopes = new[] { "wl.signin", "wl.offline_access", "onedrive.readwrite" };

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

            // Authenticate with OneDrive
            Client = OneDriveClient.GetMicrosoftAccountClient("000000004416B479", returnUrl, scopes, credentialCache: new OneDriveCredentialCache(), webAuthenticationUi: new FormsWebAuthenticationUi());

            Task<AccountSession> oneDriveSessionTask = Client.AuthenticateAsync();
            oneDriveSessionTask.Wait();

            Session = oneDriveSessionTask.Result;

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

        public override string ToString()
        {
            return string.Format("OneDrive {{ Path: {0} }}", Path);
        }
    }
}