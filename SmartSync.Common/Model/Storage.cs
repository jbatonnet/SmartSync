using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    [Serializable]
    public abstract class Storage : IDisposable
    {
        private static Dictionary<string, Predicate<string>> predicateCache = new Dictionary<string, Predicate<string>>();
        private static Dictionary<string, Regex> regexCache = new Dictionary<string, Regex>();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string Name { get; set; }
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string Description { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract Directory Root { get; }

        public virtual Directory GetDirectory(string path)
        {
            if (path[0] != '/')
                return null;
            if (path == "/")
                return Root;

            return Root.GetDirectory(path.Substring(1));
        }
        public virtual File GetFile(string path)
        {
            if (path[0] != '/')
                return null;

            return Root.GetFile(path.Substring(1));
        }

        public virtual IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            yield return Root;

            foreach (Directory directory in GetSubDirectories(Root, exclusions))
                yield return directory;
        }
        public virtual IEnumerable<File> GetAllFiles(string[] exclusions = null)
        {
            foreach (Directory directory in GetAllDirectories(exclusions))
                foreach (File file in directory.Files)
                {
                    if (exclusions != null && exclusions.Any(e => MatchPattern(file.Path, e)))
                        continue;

                    yield return file;
                }
        }

        public virtual void Dispose() { }

        public static bool IsPathValid(string path)
        {
            if (path.Contains("//"))
                return false;

            return Regex.IsMatch(path, @"[a-zA-Z0-9_\-\.#$~ \/]+");
        }
        public static bool IsNameValid(string name)
        {
            return Regex.IsMatch(name, @"[a-zA-Z0-9_\-\.#$~ ]+");
        }

        protected static IEnumerable<Directory> GetSubDirectories(Directory directory, string[] exclusions = null)
        {
            foreach (Directory subDirectory in directory.Directories)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(subDirectory.Path + "/", e)))
                    continue;

                yield return subDirectory;

                foreach (Directory subSubDirectory in GetSubDirectories(subDirectory, exclusions))
                    yield return subSubDirectory;
            }
        }
        public static bool MatchPattern(string path, string pattern)
        {
            if (path == pattern)
                return true;

            // Used cached predicate
            Predicate<string> predicate;
            if (predicateCache.TryGetValue(pattern, out predicate))
                return predicate(path);

            // Fast pattern matching
            int tokenPosition = pattern.IndexOf("**");
            if (tokenPosition >= 0)
            {
                // **XX**
                if (tokenPosition == 0 && pattern.EndsWith("**") && pattern.IndexOf('*', 2) == pattern.Length - 2)
                {
                    string middle = pattern.Substring(2, pattern.Length - 4);

                    predicate = p => p.Contains(middle);
                    predicateCache.Add(pattern, predicate);
                    return predicate(path);
                }

                // AA**BB
                if (pattern.IndexOf('*') == tokenPosition && pattern.IndexOf('*', tokenPosition + 2) == -1)
                {
                    string left = pattern.Substring(0, tokenPosition);
                    string right = pattern.Substring(tokenPosition + 2);

                    predicate = p => p.StartsWith(left) && p.EndsWith(right);
                    predicateCache.Add(pattern, predicate);
                    return predicate(path);
                }
            }

            // Default regex matching
            Regex regex;
            if (!regexCache.TryGetValue(pattern, out regex))
            {
                string regexPattern = pattern;

                // Escape characters
                regexPattern = regexPattern.Replace(@"\", @"\\");
                regexPattern = regexPattern.Replace(".", @"\.");

                // Replace tokens
                regexPattern = regexPattern.Replace("**", ".#"); // TODO: Find a better way to do this
                regexPattern = regexPattern.Replace("*", @"[^\\/]*");
                regexPattern = regexPattern.Replace(".#", ".*");

                regex = new Regex(regexPattern, RegexOptions.Compiled);
                regexCache.Add(pattern, regex);
            }

            predicate = p => regex.IsMatch(p);
            predicateCache.Add(pattern, predicate);
            return predicate(path);
        }
    }
}