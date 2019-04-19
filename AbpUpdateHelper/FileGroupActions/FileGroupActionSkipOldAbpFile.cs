namespace AbpUpdateHelper.FileGroupActions
{
    public class FileGroupActionSkipOldAbpFile : IFileGroupAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            
        }

        public bool Match(FileGroup fileGroup)
        {
            if (fileGroup.CurrentAbpFile != null && fileGroup.NewAbpFile == null && fileGroup.ProjectFile == null)
            {
                return true;
            }

            return false;
        }
    }
}