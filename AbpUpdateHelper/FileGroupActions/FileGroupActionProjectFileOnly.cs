using System.IO;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper.FileGroupActions
{
    public class FileGroupActionProjectFileOnly : IFileGroupAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, fileGroup.ProjectFile.RelativeDirectory);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var destinationFileName = destination + "\\" + fileGroup.ProjectFile.File.Name;

            File.Copy(fileGroup.ProjectFile.File.FullName, destinationFileName, true);
        }

        public bool Match(FileGroup fileGroup)
        {
            if (fileGroup.NewAbpFile == null && fileGroup.CurrentAbpFile == null && fileGroup.ProjectFile != null)
            {
                return true;
            }

            if (fileGroup.NewAbpFile != null && fileGroup.CurrentAbpFile != null)
            {
                if (AbpFileHelper.FilesAreEqual(fileGroup.NewAbpFile.File, fileGroup.CurrentAbpFile.File))
                {
                    return true;
                }
            }

            return false;
        }
    }
}