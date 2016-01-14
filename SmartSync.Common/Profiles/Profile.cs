using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public enum DiffType
    {
        /// <summary>
        /// Compare file paths only
        /// </summary>
        Paths,

        /// <summary>
        /// Compare file sizes to detect differences
        /// </summary>
        Sizes,

        /// <summary>
        /// Compare file write dates to detect differences
        /// </summary>
        Dates,

        /// <summary>
        /// Compare file hashes to detect differences
        /// </summary>
        Hashes,
    }

    public enum SyncType
    {
        /// <summary>
        /// Add left content to right content, update files if necessary
        /// </summary>
        Backup,

        /// <summary>
        /// Replace all right content with left content, remove files if necessary
        /// </summary>
        Clone,

        /// <summary>
        /// Sync all files between left and right storage, keep newest files on both sides
        /// </summary>
        Sync,
    }

    public abstract class Profile : IDisposable
    {
        public virtual DiffType DiffType { get; } = DiffType.Dates;
        public virtual SyncType SyncType { get; } = SyncType.Backup;

        public abstract IEnumerable<string> Exclusions { get; }

        public abstract Storage Left { get; }
        public abstract Storage Right { get; }

        public virtual IEnumerable<Diff> GetDifferences()
        {
            string[] exclusions = Exclusions.ToArray();

            // Directories
            Directory[] leftDirectories = Left.GetAllDirectories(exclusions).ToArray();
            Directory[] rightDirectories = Right.GetAllDirectories(exclusions).ToArray();

            // Compute directory differences
            IEnumerable<DirectoryDiff> directoryDiffs = FullOuterJoin(leftDirectories, rightDirectories, l => l.Path, r => r.Path, (l, r, p) => new DirectoryDiff(Left, l, Right, r))
                                                            .Where(d => d.Left == null || d.Right == null);
            foreach (DirectoryDiff directoryDiff in directoryDiffs)
                yield return directoryDiff;

            // Files
            File[] leftFiles = Left.GetAllFiles(exclusions).ToArray();
            File[] rightFiles = Right.GetAllFiles(exclusions).ToArray();

            // Compute file differences
            FileComparer comparer = new FileComparer(DiffType);
            IEnumerable<FileDiff> fileDiffs = FullOuterJoin(leftFiles, rightFiles, l => l, r => r, (l, r, p) => new FileDiff(Left, l, Right, r, comparer), keyComparer: new FileComparer(DiffType.Paths))
                                                  .Where(d => d.Left == null || d.Right == null || !comparer.Equals(d.Left, d.Right));
            foreach (FileDiff fileDiff in fileDiffs)
                yield return fileDiff;
        }

        public virtual void Dispose()
        {
            Left?.Dispose();
            Right?.Dispose();
        }

        protected static IEnumerable<Directory> GetSubDirectories(Directory directory, string[] exclusions = null)
        {
            foreach (Directory subDirectory in directory.Directories)
            {
                if (exclusions != null && exclusions.Any(e => Storage.MatchPattern(subDirectory.Path, e)))
                    continue;

                yield return subDirectory;

                foreach (Directory subSubDirectory in GetSubDirectories(subDirectory, exclusions))
                    yield return subSubDirectory;
            }
        }
        protected static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TKey, TResult> projection, TLeft leftDefault = default(TLeft), TRight rightDefault = default(TRight), IEqualityComparer<TKey> keyComparer = null)
        {
            keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            ILookup<TKey, TLeft> leftLookup = left.ToLookup(leftKeySelector, keyComparer);
            ILookup<TKey, TRight> rightLookup = right.ToLookup(rightKeySelector, keyComparer);

            HashSet<TKey> keys = new HashSet<TKey>(leftLookup.Select(p => p.Key), keyComparer);
            keys.UnionWith(rightLookup.Select(p => p.Key));

            return from key in keys
                   from leftValue in leftLookup[key].DefaultIfEmpty(leftDefault)
                   from rightValue in rightLookup[key].DefaultIfEmpty(rightDefault)
                   select projection(leftValue, rightValue, key);
        }
    }
}