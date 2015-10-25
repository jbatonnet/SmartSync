using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Storage left = new BasicStorage(@"D:\Temp\Left");
            Storage right = new BasicStorage(@"D:\Temp\Left");

            string[] exclusions = new string[]
            {
                "/Excel file.xlsx",
                "/Directory/SubDirectory 2"
            };

            // Left directories
            Directory leftRoot = left.Root;
            List<Directory> leftDirectories = new List<Directory>() { leftRoot };
            leftDirectories.AddRange(GetSubDirectories(leftRoot, exclusions));

            // Left files
            List<File> leftFiles = leftDirectories.SelectMany(d => d.Files)
                                                  .Where(f => !exclusions.Any(e => MatchPattern(f.Path, e)))
                                                  .ToList();

            // Right directories
            Directory rightRoot = right.Root;
            List<Directory> rightDirectories = new List<Directory>() { rightRoot };
            rightDirectories.AddRange(GetSubDirectories(rightRoot, exclusions));

            // Right files
            List<File> rightFiles = rightDirectories.SelectMany(d => d.Files)
                                                    .Where(f => !exclusions.Any(e => MatchPattern(f.Path, e)))
                                                    .ToList();

            // Find differences

        }

        public static IEnumerable<Directory> GetSubDirectories(Directory directory, string[] exclusions = null)
        {
            foreach (Directory subDirectory in directory.Directories)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(subDirectory.Path, e)))
                    continue;

                yield return subDirectory;

                foreach (Directory subSubDirectory in GetSubDirectories(subDirectory))
                    yield return subSubDirectory;
            }
        }

        public static bool MatchPattern(string path, string pattern)
        {
            if (path == pattern)
                return true;

            // Escape characters
            pattern = pattern.Replace(@"\", @"\\");
            pattern = pattern.Replace(".", @"\.");

            // Replace tokens
            pattern = pattern.Replace("**", ".+");
            pattern = pattern.Replace("*", @"[^\\/]+");

            return Regex.IsMatch(path, pattern);
        }
    }
}