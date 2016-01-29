using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SmartSync.Engine
{
    internal static class ModuleInitializer
    {
        internal static void Run()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //DetachFromExe();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        private static void DetachFromExe()
        {
            if (Debugger.IsAttached)
                return;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 0 && args[0] == "-")
                return;

            string sourcePath = Assembly.GetExecutingAssembly().Location;
            string destinationPath = Path.GetTempFileName();

            File.Copy(sourcePath, destinationPath, true);
            Process.Start(destinationPath, "- " + string.Join(" ", args));
        }
    }
}