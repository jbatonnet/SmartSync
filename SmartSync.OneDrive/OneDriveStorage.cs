using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.OneDrive.Sdk;

namespace SmartSync.OneDrive
{
    public class Toto : IWebAuthenticationUi
    {
        public Task<IDictionary<string, string>> AuthenticateAsync(Uri requestUri, Uri callbackUri)
        {
            throw new NotImplementedException();
        }
    }

    public class OneDriveStorage
    {
        public OneDriveStorage()
        {
            var oneDriveClient = OneDriveClient.GetMicrosoftAccountClient("000000004416B479", "https://login.live.com/oauth20_desktop.srf", new[] { "wl.signin", "wl.offline_access", "onedrive.readwrite" }, "GYdcxFCOMNAdwA2Ha5PIHFX-Xs0klVex");

            Task<AccountSession> oneDriveSessionTask = oneDriveClient.AuthenticateAsync();
            oneDriveSessionTask.Wait();

            AccountSession oneDriveSession = oneDriveSessionTask.Result;
        }
    }
}