using System.IO;

namespace AbpUpdateHelper.FileGroupActions
{
    public class FileGroupActionAddAbpFile : IFileGroupAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            fileGroup.CopyNewAbpFile(destinationFolder);
        }

        public bool Match(FileGroup fileGroup)
        {
            if (fileGroup.NewAbpFile != null && fileGroup.ProjectFile == null)
            {
                return true;
            }

            return false;
        }
    }
}