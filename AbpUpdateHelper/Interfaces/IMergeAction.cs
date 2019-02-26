namespace AbpUpdateHelper
{
    public interface IMergeAction
    {
        void Run(FileGroup fileGroup, string destinationFolder);

        bool Match(FileGroup fileGroup);
    }
}