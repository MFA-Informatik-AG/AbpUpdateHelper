using System;
using System.Diagnostics;
using System.IO;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper
{
    public class SmartMergeMergeAction : ExternalMergeActionBase
    {
        protected override void RunComparer(FileGroup fileGroup, string destinationPath)
        {

            var winMerge = new Process();

            winMerge.StartInfo.FileName = GetSmartMergeFile().Item2;
            winMerge.StartInfo.Arguments = $" -s \"{fileGroup.NewAbpFile.File.FullName}\" -d \"{fileGroup.ProjectFile.File.FullName}\" -b \"{fileGroup.CurrentAbpFile.File.FullName}\" -r \"{destinationPath}\" --automatic --silent --nolangwarn";
            winMerge.Start();

            winMerge.WaitForExit();

            if (winMerge.ExitCode != 0)
            {
                winMerge.StartInfo.Arguments = $" -s \"{fileGroup.NewAbpFile.File.FullName}\" -d \"{fileGroup.ProjectFile.File.FullName}\" -b \"{fileGroup.CurrentAbpFile.File.FullName}\" -r \"{destinationPath}\" --automatic --nolangwarn";
                winMerge.Start();

                winMerge.WaitForExit();
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