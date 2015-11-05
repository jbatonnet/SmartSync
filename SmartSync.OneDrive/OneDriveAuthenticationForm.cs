using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.OneDrive.Sdk;

namespace SmartSync.OneDrive
{
    public partial class OneDriveAuthenticationForm : Form, IWebAuthenticationUi
    {
        public OneDriveAuthenticationForm()
        {
            InitializeComponent();
        }

        public Task<IDictionary<string, string>> AuthenticateAsync(Uri requestUri, Uri callbackUri)
        {
            throw new NotImplementedException();
        }
    }
}
