using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    internal static class Bootstrap
    {
        static Bootstrap()
        {
            string[] assemblyNames = new[]
            {
                "SmartSync.Common.Extern.Renci.SshNet.dll"
            };

            foreach (string assemblyName in assemblyNames)
            {
                using (Stream assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName))
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