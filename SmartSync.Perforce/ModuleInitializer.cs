using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SmartSync.Sftp
{
    internal static class ModuleInitializer
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        private const string resourcePrefix = "SmartSync.Perforce.Extern.p4api.net";
        private static string[] assemblyNames =
        {
            "p4api.net.dll",
        };
        private static string[] libraryNames =
        {
            "p4bridge.dll",
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

            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

            foreach (string libraryName in libraryNames)
            {
                string libraryPath = Path.Combine(tempPath, libraryName);

                using (Stream libraryStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePrefix + "." + libraryName))
                using (Stream fileStream = File.Open(libraryPath, FileMode.Create))
                    libraryStream.CopyTo(fileStream);

                LoadLibrary(libraryPath);
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