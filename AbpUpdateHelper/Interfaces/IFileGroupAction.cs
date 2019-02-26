namespace AbpUpdateHelper
{
    public interface IFileGroupAction
    {
        void Run(FileGroup fileGroup, string destinationFolder);

        bool Match(FileGroup fileGroup);
    }
}