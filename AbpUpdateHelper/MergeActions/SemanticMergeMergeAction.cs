using System;
using System.Diagnostics;
using System.IO;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper
{
    public class SemanticMergeMergeAction : ExternalMergeActionBase
    {
        protected override void RunComparer(FileGroup fileGroup, string destinationPath)
        {

            var merge = new Process
            {
                StartInfo =
                {
                    FileName = GetSmartMergeFile().Item2,
                    Arguments = $" -s \"{fileGroup.NewAbpFile.File.FullName}\" -d \"{fileGroup.ProjectFile.File.FullName}\" -b \"{fileGroup.CurrentAbpFile.File.FullName}\" -r \"{destinationPath}\" -a --silent --nolangwarn"
                }
            };

            merge.Start();

            merge.WaitForExit();

            if (merge.ExitCode != 0)
            {
                merge.StartInfo.Arguments = $" -s \"{fileGroup.NewAbpFile.File.FullName}\" -d \"{fileGroup.ProjectFile.File.FullName}\" -b \"{fileGroup.CurrentAbpFile.File.FullName}\" -r \"{destinationPath}\" -a --nolangwarn";
                merge.Start();

                merge.WaitForExit();
            }
        }

        private Tuple<bool, string> GetSmartMergeFile()
        {
            var semanticMergePath = $"{AbpFileHelper.GetUserLocalAppDataPath()}\\semanticmerge\\semanticmergetool.exe";

            return new Tuple<bool, string>(File.Exists(semanticMergePath), semanticMergePath);
        }

        public override bool Match(FileGroup fileGroup)
        {
            if (!GetSmartMergeFile().Item1)
            {
                return false;
            }

            return base.Match(fileGroup);
        }
    }
}