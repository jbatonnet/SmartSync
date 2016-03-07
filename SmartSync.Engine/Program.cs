using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static Dictionary<string, string> Options { get; private set; }
        public static List<string> Parameters { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            Options = args.Where(a => a.StartsWith("/"))
                          .Select(a => a.TrimStart('/'))
                          .Select(a => new { Parameter = a.Trim(), Separator = a.Trim().IndexOf(':') })
                          .ToDictionary(a => a.Separator == -1 ? a.Parameter : a.Parameter.Substring(0, a.Separator).ToLower(), a => a.Separator == -1 ? null : a.Parameter.Substring(a.Separator + 1));
            Parameters = args.Where(a => !a.StartsWith("/"))
                             .ToList();

            string profilePath = Parameters.FirstOrDefault();
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

            Environment.CurrentDirectory = profileInfo.DirectoryName;

            Profile profile = null;

            // Load the profile
            try
            {
                Log.Debug("Loading profile {0} ...", profileInfo.FullName);

                switch (profileInfo.Extension)
                {
                    case ".xsync": profile = XProfile.Load(XDocument.Load(profileInfo.FullName)); break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while loading the specified profile: {0}", e.Message);
            }

            if (profile == null)
            {
                Log.Error("Could not load the specified profile");
                Exit();
            }
            else
            {
                Log.Debug("Profile loaded successfully");
                Log.Debug("");
            }

            if (Options.ContainsKey("reverse"))
                profile = profile.Reverse();

            // Display some profile informations
            Log.Info("Left  : {0}", profile.Left);
            Log.Info("Right : {0}", profile.Right);
            Log.Info("Sync  : {0}", profile.SyncType);
            Log.Info("Diff  : {0}", profile.DiffType);
            Log.Info("");

            // Compute differences and actions
            Diff[] differences;
            Action[] actions;

            try
            {
                Log.Info("Computing storage differences ...");
                differences = profile.GetDifferences().ToArray();

                Log.Info("Computing actions to perform ...");
                actions = differences.Select(d => d.GetAction(profile.SyncType)).ToArray();

                Log.Info("");
            }
            catch (Exception e)
            {
                Log.Error("Error while computing differences: {0}", e.Message);
                Exit();
                return;
            }

            if (actions.Length > 0)
            {
                // Show actions to validate sync
                if (!Options.ContainsKey("novalidate"))
                {
                    Log.Info("{0} actions to process ...", actions.Length);

                    for (int i = 0; i < actions.Length; i++)
                        Log.Info(actions[i].ToString());

                    string input = "";
                    string[] answers =
                    {
                        "o", "y", "oui", "ok", "yes", "yep", "yeah",
                        "n", "no", "non", "nope", "nop", "nah"
                    };
                    while (!answers.Contains(input.ToLower()))
                    {
                        Console.Write("Do you want to process these actions ? ");
                        input = Console.ReadLine();
                    }

                    if (input[0] == 'n')
                    {
                        Log.Info("Sync cancelled by user");
                        Exit();
                    }
                }

                Log.Info("Processing {0} actions ...", actions.Length);

                // Process actions
                for (int i = 0; i < actions.Length; i++)
                {
                    Log.Info("{1} % - {0} ...", actions[i], i * 100 / actions.Length);
                    actions[i].Process();
                }

                Log.Info("Flushing data to storage ...");
                Log.Info("");
            }

            profile.Dispose();

            Log.Info("Everything is in sync. {0} actions processed.", actions.Length);
            Exit();
        }

        public static void Exit()
        {
            if (Debugger.IsAttached || Options.ContainsKey("pause"))
            {
                Console.WriteLine();
                Console.Write("Press any key to exit ...");
                Console.ReadKey(true);
            }

            Environment.Exit(0);
        }
    }
}