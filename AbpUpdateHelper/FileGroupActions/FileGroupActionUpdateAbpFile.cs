﻿using System.IO;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper.FileGroupActions
{
    public class FileGroupActionUpdateAbpFile : IFileGroupAction
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

            if (fileGroup.NewAbpFile != null && fileGroup.CurrentAbpFile != null && fileGroup.ProjectFile != null)
            {
                if (fileGroup.NewAbpFile.IsAutoGenerated())
                {
                    return true;
                }

                //
                // the new and the current abp file must not match (an update would not give sense in this case)
                //
                if (AbpFileHelper.FilesAreEqual(fileGroup.NewAbpFile.File, fileGroup.CurrentAbpFile.File))
                {
                    return false;
                }

                //
                // if the current abp and the project file matches the project file can be replaced through the new abp file
                //
                if (AbpFileHelper.FilesAreEqual(fileGroup.CurrentAbpFile.File, fileGroup.ProjectFile.File))
                {
                    return true;
                }
            }

            return false;
        }
    }
}