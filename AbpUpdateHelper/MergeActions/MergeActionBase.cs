using System;
using System.Collections.Generic;
using System.Linq;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper
{
    public abstract class MergeActionBase
    {
        public virtual bool Match(FileGroup fileGroup)
        {
            var noMatchList = new[]
            {
                ".dll",
                ".png",
                ".gif",
                ".svg",
                ".eot",
                ".ttf",
                ".woff",
                ".woff2",
                ".ico",
                ".lock"
            };

            var fileExtension = fileGroup.ProjectFile.File.Extension.ToLower();

            return !noMatchList.Any(pr => pr.Equals(fileExtension));
        }

        protected string Merge(FileGroup fileGroup)
        {
            var diffPatch = new diff_match_patch();

            var diff = diffPatch.diff_main(fileGroup.NewAbpFile.FileContent, fileGroup.CurrentAbpFile.FileContent, false);

            diffPatch.diff_cleanupSemantic(diff);

            var patchList = diffPatch.patch_make(fileGroup.NewAbpFile.FileContent, fileGroup.CurrentAbpFile.FileContent, diff);

            var results = diffPatch.patch_apply(patchList, fileGroup.ProjectFile.FileContent);

            return results[0].ToString();
        }

        protected string MergeComplex(FileGroup fileGroup)
        {
            var result = new List<string>();

            var projectFile = fileGroup.ProjectFile.FileContentLines;

            var diff3Merges = Diff3.diff3_merge(fileGroup.NewAbpFile.FileContentLines, projectFile, fileGroup.CurrentAbpFile.FileContentLines, true);

            foreach (var diff3Merge in diff3Merges)
            {
                if (diff3Merge is Diff3.MergeOKResultBlock okBlock)
                {
                    result.AddRange(okBlock.ContentLines);
                }

                if (diff3Merge is Diff3.MergeConflictResultBlock conflictBlock)
                {
                    var mergeResult = MergeBlock(conflictBlock);

                    result.AddRange(mergeResult);
                }
            }

            return string.Join('\n', result);
        }

        private bool EqualLineCount(string[] a, string[] b, string[] c)
        {
            return a.Length == b.Length && b.Length == c.Length;
        }

        private IEnumerable<string> MergeBlock(Diff3.MergeConflictResultBlock conflictBlock)
        {
            var result = new List<string>();

            //
            // O block was added
            //
            if (conflictBlock.LeftLines.Length == 0 && conflictBlock.RightLines.Length == 0)
            {
                return conflictBlock.OldLines.ToList();
            }

            //
            // L block was added
            //
            if (conflictBlock.LeftLines.Length >= 1 && conflictBlock.OldLines.Length == 0)
            {
                return conflictBlock.LeftLines.ToList();
            }

            var diffLr = Diff3.diff_comm(conflictBlock.LeftLines, conflictBlock.RightLines);

            //
            // no difference between L and R, change only in O
            //
            if (diffLr.All(pr => pr.common != null))
            {
                return conflictBlock.OldLines.ToList();
            }

            //
            // no existing lines in R, merge L and O
            //
            if (conflictBlock.RightLines.Length == 0)
            {
                var merge = Diff3.diff_merge_keepall(conflictBlock.LeftLines, conflictBlock.OldLines);

                result.AddRange(merge);

                return result;
            }

            //
            // no existing lines in L, potentially added in O only
            //
            if (conflictBlock.LeftLines.Length == 0)
            {
                return result;
            }

            //
            // completely missmatch of the lines
            //
            if (EqualLineCount(conflictBlock.OldLines, conflictBlock.LeftLines, conflictBlock.RightLines))
            {
                result.AddRange(conflictBlock.OldLines);

                return result;
            }

            //
            // wild guess, assume O >= L == R 
            //
            if (conflictBlock.LeftLines.Length == conflictBlock.RightLines.Length && conflictBlock.OldLines.Length >= conflictBlock.LeftLines.Length)
            {
                result.AddRange(conflictBlock.OldLines);

                return result;
            }

            //
            // Update O = R with L
            //
            for (var old = 0; old < conflictBlock.OldLines.Length; old++)
            {
                var currentOldLine = conflictBlock.OldLines[old];

                var index = Search(conflictBlock.RightLines, currentOldLine);

                if (index < 0)
                {
                    continue;
                }

                var rightLine = conflictBlock.RightLines[index];

                if (conflictBlock.LeftLines.Length <= index)
                {
                    continue;
                }

                var leftLine = conflictBlock.LeftLines[index];

                if (rightLine.Equals(currentOldLine, StringComparison.OrdinalIgnoreCase))
                {
                    conflictBlock.OldLines[old] = leftLine;
                }
            }

            //
            // rematch O with L
            //
            var fullMerge = Diff3.diff_merge_keepall(conflictBlock.LeftLines, conflictBlock.OldLines);

            foreach (var line in fullMerge)
            {
                if (result.Any(pr => pr.Equals(line, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                result.Add(line);
            }

            return result;
        }

        private int Search(string[] stringArray, string searchFor)
        {
            for (var i = 0; i < stringArray.Length; i++)
            {
                if (stringArray[i].Equals(searchFor, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}