using System.IO;

namespace AbpUpdateHelper.FileGroupActions
{
    public class FileGroupActionAddAbpFile : IFileGroupAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, fileGroup.NewAbpFile.RelativeDirectory);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var destinationFileName = destination + "\\" + fileGroup.NewAbpFile.File.Name;

            File.Copy(fileGroup.NewAbpFile.File.FullName, destinationFileName, true);

        }

        public bool Match(FileGroup fileGroup)
        {
            if (fileGroup.NewAbpFile != null && fileGroup.CurrentAbpFile == null && fileGroup.ProjectFile == null)
            {
                return true;
            }

            return false;
        }
    }
}