using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SmartSync.GoogleDrive
{
    internal static class ModuleInitializer
    {
        private const string resourcePrefix = "SmartSync.GoogleDrive.Extern";
        private static string[] assemblyNames =
        {
            "BouncyCastle.Crypto.dll",
            "Google.Apis.Auth.dll",
            "Google.Apis.Auth.PlatformServices.dll",
            "Google.Apis.Core.dll",
            "Google.Apis.dll",
            "Google.Apis.Drive.v2.dll",
            "Google.Apis.PlatformServices.dll",
            "log4net.dll",
            "Microsoft.Threading.Tasks.dll",
            "Microsoft.Threading.Tasks.Extensions.dll",
            "Microsoft.Threading.Tasks.Extensions.Desktop.dll",
            "Newtonsoft.Json.dll",
            "System.Net.Http.Extensions.dll",
            "System.Net.Http.Primitives.dll",
            "Zlib.Portable.dll",
        };

        internal static void Run()
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
            Assembly assembly = null;

            // Check already loaded assemblies
            assembly = assembly ?? AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);

            // Try to load full assembly name
            try
            {
                assembly = assembly ?? Assembly.Load(new AssemblyName(args.Name));
            }
            catch { }

            return assembly;
        }
    }
}