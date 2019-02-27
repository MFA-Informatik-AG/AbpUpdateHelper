using System.Diagnostics;
using System.IO;

namespace AbpUpdateHelper
{
    public abstract class ExternalMergeAction : MergeActionBase, IMergeAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, fileGroup.ProjectFile.RelativeDirectory);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var mergeResult = Merge(fileGroup);

            var destinationPath = destination + "\\" + fileGroup.NewAbpFile.File.Name;

            File.WriteAllText(destinationPath, mergeResult);

            RunComparer(fileGroup, destinationPath);
        }

        protected abstract void RunComparer(FileGroup fileGroup, string destinationPath);
    }
}