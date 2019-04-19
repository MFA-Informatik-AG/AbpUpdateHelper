using System.Globalization;
using System.IO;

namespace AbpUpdateHelper
{
    public class FileGroup
    {
        public SingleFile CurrentAbpFile { get; set; }

        public SingleFile NewAbpFile { get; set; }

        public SingleFile ProjectFile { get; set; }

        public bool OutputFileExists(string destinationFolder)
        {
            if (NewAbpFile == null)
            {
                return false;
            }

            var destination = Path.Combine(destinationFolder, NewAbpFile.RelativeDirectory, NewAbpFile.File.Name);

            return File.Exists(destination);
        }
        public void DeleteCurrentAbpFile(string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, CurrentAbpFile.RelativeDirectory);

            var destinationFileName = destination + "\\" + CurrentAbpFile.File.Name;

            if (File.Exists(destinationFileName))
            {
                File.Delete(destinationFileName);
            }
        }

        public void CopyNewAbpFile(string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, NewAbpFile.RelativeDirectory);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var destinationFileName = destination + "\\" + NewAbpFile.File.Name;

            File.Copy(NewAbpFile.File.FullName, destinationFileName, true);
        }

        public void CopyProjectFile(string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, ProjectFile.RelativeDirectory);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var destinationFileName = destination + "\\" + ProjectFile.File.Name;

            File.Copy(ProjectFile.File.FullName, destinationFileName, true);
        }

        public bool ExistsAbpFile(string fileName)
        {
            if (NewAbpFile != null && NewAbpFile.File.Name.EndsWith(fileName))
            {
                return true;
            }

            return false;
        }

        public string ReadTextLoadAbpFile(string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, NewAbpFile.RelativeDirectory);

            var destinationFileName = destination + "\\" + NewAbpFile.File.Name;

            if (File.Exists(destinationFileName))
            {
                return File.ReadAllText(destinationFileName);
            }

            return null;
        }

        public void WriteTextAbpFile(string text, string destinationFolder)
        {
            var destination = Path.Combine(destinationFolder, NewAbpFile.RelativeDirectory);

            var destinationFileName = destination + "\\" + NewAbpFile.File.Name;

            File.WriteAllText(text, destinationFileName);
        }
    }
}