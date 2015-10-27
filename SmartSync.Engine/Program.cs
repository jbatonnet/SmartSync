using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using SmartSync.Common;
using Action = SmartSync.Common.Action;

namespace SmartSync.Engine
{
    class Program
    {
        public static Dictionary<string, string> Parameters { get; private set; }

        public static void Main(string[] args)
        {
            Parameters = Environment.GetCommandLineArgs()
                                    .Where(p => p.StartsWith("/"))
                                    .Select(p => p.TrimStart('/'))
                                    .Select(p => new { Parameter = p.Trim(), Separator = p.Trim().IndexOf(':') })
                                    .ToDictionary(p => p.Separator == -1 ? p.Parameter : p.Parameter.Substring(0, p.Separator).ToLower(), p => p.Separator == -1 ? null : p.Parameter.Substring(p.Separator + 1));

            string profilePath = Environment.GetCommandLineArgs().Skip(1).LastOrDefault();
            if (profilePath == null)
            {
                Log.Error("You must specify a profile to run");
                Exit();
            }

            FileInfo profileInfo = new FileInfo(profilePath);
            if (!profileInfo.Exists)
            {
                Log.Error("Could not find the specified profile");
                Exit();
            }

            Profile profile = null;

            try
            {
                switch (profileInfo.Extension)
                {
                    case ".xsync": profile = XProfile.Load(XDocument.Load(profileInfo.FullName)); break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while loading the specified profile: " + e.Message);
            }

            if (profile == null)
            {
                Log.Error("Could not load the specified profile");
                Exit();
            }

            // Compute differences and actions
            Diff[] differences = profile.GetDifferences().ToArray();
            Action[] actions = differences.Select(d => d.GetAction(profile.SyncType)).ToArray();

            if (actions.Length > 0)
            {
                // Process actions
                for (int i = 0; i < actions.Length; i++)
                {
                    Log.Info("{1} % - {0} ...", actions[i], i * 100 / actions.Length);
                    actions[i].Process();
                }

                Log.Info("Flushing data to storage...");
            }

            profile.Dispose();

            Log.Info("Everything is in sync. {0} actions processed.", actions.Length);
            Exit();
        }

        public static void Exit()
        {
            if (Parameters.ContainsKey("pause"))
            {
                Console.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey(true);
            }

            Environment.Exit(0);
        }
    }
}