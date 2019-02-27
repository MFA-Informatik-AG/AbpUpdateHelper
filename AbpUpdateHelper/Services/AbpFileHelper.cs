using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AbpUpdateHelper.Services
{
    public static class AbpFileHelper
    {
        public static IEnumerable<SingleFile> ReadAbpFiles(string directoryPath, string abpProjectName, List<string> excludeDirectories, List<string> excludeFiles) 
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            var allFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList();

            var filterFiles = allFiles.Where(pr => !IsFileOrDirectoryExcluded(pr, excludeDirectories, excludeFiles));

            return filterFiles.Select(abpFile => new SingleFile(abpFile, abpProjectName)).ToList();
        }

        private static bool IsFileOrDirectoryExcluded(FileInfo fileInfo, List<string> excludeDirectories, List<string> excludeFiles)
        {
            if (fileInfo.DirectoryName == null || excludeDirectories.Any(pr => Regex.IsMatch(fileInfo.DirectoryName, pr, RegexOptions.IgnoreCase)))
            {
                return true;
            }

            if (excludeFiles.Any(pr => Regex.IsMatch(fileInfo.Name, pr, RegexOptions.IgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            const int bytesToRead = 64;

            if (first.Length != second.Length)
            {
                return false;
            }

            var iterations = (int)Math.Ceiling((double)first.Length / bytesToRead);

            using (var fs1 = first.OpenRead())
            {
                using (var fs2 = second.OpenRead())
                {
                    var one = new byte[bytesToRead];
                    var two = new byte[bytesToRead];

                    for (var i = 0; i < iterations; i++)
                    {
                        fs1.Read(one, 0, bytesToRead);
                        fs2.Read(two, 0, bytesToRead);

                        if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static string GetProgramFilesPath()
        {
            return Environment.ExpandEnvironmentVariables("%ProgramW6432%");
        }

        public static string GetProgramFilesX86Path()
        {
            return Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
        }
    }
}