using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.GoogleDrive
{
    internal static class Bootstrap
    {
        private const string resourcePrefix = "SmartSync.GoogleDrive.External";
        private static string[] assemblyNames =
        {
            "Google.Apis.dll",
            "Google.Apis.Auth.dll",
            "Google.Apis.Core.dll",
            "Google.Apis.Drive.v2.dll",
        };

        static Bootstrap()
        {
            foreach (string assemblyName in assemblyNames)
            {
                using (Stream assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePrefix + "." + assemblyName))
                {
                    // Read assembly
                    byte[] assemblyBytes = new byte[assemblyStream.Length];
                    assemblyStream.Read(assemblyBytes, 0, assemblyBytes.Length);

                    // Load assembly
                    Assembly.Load(assemblyBytes);
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                                          .FirstOrDefault(a => a.FullName == args.Name);
        }

        public static void Initialize() { }
    }
}