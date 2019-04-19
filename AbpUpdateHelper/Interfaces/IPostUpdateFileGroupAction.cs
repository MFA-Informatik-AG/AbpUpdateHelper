namespace AbpUpdateHelper
{
    public interface IPostUpdateFileGroupAction
    {
        void Run(FileGroup fileGroup, string destinationFolder);
    }
}