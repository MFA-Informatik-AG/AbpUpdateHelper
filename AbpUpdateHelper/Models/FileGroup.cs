using System.IO;

namespace AbpUpdateHelper
{
    public class FileGroup
    {
        public SingleFile CurrentAbpFile { get; set; }

        public SingleFile NewAbpFile { get; set; }

        public SingleFile ProjectFile { get; set; }

        public bool OutputFileExisis(string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, NewAbpFile.RelativeDirectory, NewAbpFile.File.Name);

            return File.Exists(destination);
        }
    }
}