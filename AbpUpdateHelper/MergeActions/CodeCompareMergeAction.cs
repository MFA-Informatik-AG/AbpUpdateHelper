using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper
{
    public class CodeCompareMergeAction : ExternalMergeAction
    {
        protected override void RunComparer(FileGroup fileGroup, string destinationPath)
        {
            var winMerge = new Process();

            winMerge.StartInfo.FileName = GetCodeCompareFile().Item2;
            winMerge.StartInfo.Arguments = $" /w \"{fileGroup.NewAbpFile.File.FullName}\" \"{destinationPath}\"";
            winMerge.Start();

            winMerge.WaitForExit();
        }

        private Tuple<bool, string> GetCodeCompareFile()
        {
            var pathList = new List<string>
            {
                $"{AbpFileHelper.GetProgramFilesPath()}\\Devart\\Code Compare\\CodeCompare.exe",
                $"{AbpFileHelper.GetProgramFilesX86Path()}\\Devart\\Code Compare\\CodeCompare.exe"
            };

            foreach (var path in pathList)
            {
                if (File.Exists(path))
                {
                    return new Tuple<bool, string>(true, path);
                }
                
            }
            return new Tuple<bool, string>(false,null);
        }

        public override bool Match(FileGroup fileGroup)
        {
            if (!GetCodeCompareFile().Item1)
            {
                return false;
            }

            return base.Match(fileGroup);
        }
    }
}