using System.IO;

namespace AbpUpdateHelper.FileGroupActions
{
    public class FileGroupActionRemoveAbpFile : IFileGroupAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, fileGroup.CurrentAbpFile.RelativeDirectory);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var destinationFileName = destination + "\\" + fileGroup.CurrentAbpFile.File.Name;

            if (File.Exists(destinationFileName))
            {
                File.Delete(destinationFileName);
            }
        }

        public bool Match(FileGroup fileGroup)
        {
            if (fileGroup.NewAbpFile == null && fileGroup.CurrentAbpFile != null && fileGroup.ProjectFile != null)
            {
                return true;
            }

            return false;
        }
    }
}