using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AbpUpdateHelper.Services;

namespace AbpUpdateHelper
{
    public class AbpUpdateController
    {
        private readonly List<IFileGroupAction> _fileActions;
        private readonly List<IPostUpdateFileGroupAction> _postUpdateFileGroupActions;

        public AbpUpdateController(
            List<IFileGroupAction> fileActions, 
            List<IPostUpdateFileGroupAction> postUpdateFileGroupActions
            )
        {
            _fileActions = fileActions;
            _postUpdateFileGroupActions = postUpdateFileGroupActions;
        }

        public void UpdateAbpVersion(
            string abpProjectName, 
            string pathToNewAbpVersion, 
            string pathToCurrentAbpVersion, 
            string pathToProject, 
            string pathToOutputFolder, 
            bool skipExistingOutputFiles,
            List<string> copyAllways
            )
        {
            var fileGroups = CreateFileGroups(abpProjectName, pathToNewAbpVersion, pathToCurrentAbpVersion, pathToProject);

            foreach (var fileGroup in fileGroups)
            {
                try
                {
                    if (skipExistingOutputFiles && fileGroup.OutputFileExists(pathToOutputFolder))
                    {
                        continue;
                    }

                    if (IsInList(fileGroup.NewAbpFile, copyAllways))
                    {
                        fileGroup.CopyNewAbpFile(pathToOutputFolder);
                    }

                    var fileAction = _fileActions.Single(pr => pr.Match(fileGroup));

                    fileAction.Run(fileGroup, pathToOutputFolder);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    throw;
                }
            }

            foreach (var fileGroup in fileGroups)
            {
                _postUpdateFileGroupActions.ForEach(pr => pr.Run(fileGroup, pathToOutputFolder));
            }

        }

        private bool IsInList(SingleFile file, IEnumerable<string> copyAllways)
        {
            if (file == null)
            {
                return false;
            }

            return copyAllways.Any(pr => Regex.IsMatch(file.File.FullName, pr, RegexOptions.IgnoreCase));
        }

        private List<FileGroup> CreateFileGroups(string abpProjectName, string pathToNewAbpVersion, string pathToCurrentAbpVersion, string pathToProject)
        {
            var filterDirectories = new List<string>
            {
                @"\\bin",
                @"\\obj",
                @"\\node_modules",
                @"\\jcrop\\src",
            };

            var filterFiles = new List<string>
            {
                "package-lock.json",
                "yarn.lock"
            };

            var newAbpVersionFiles = AbpFileHelper.ReadAbpFiles(pathToNewAbpVersion, abpProjectName, filterDirectories, filterFiles);
            var currentAbpVersionFiles = AbpFileHelper.ReadAbpFiles(pathToCurrentAbpVersion, abpProjectName, filterDirectories, filterFiles);
            var projectFiles = AbpFileHelper.ReadAbpFiles(pathToProject, abpProjectName, filterDirectories, filterFiles);

            var fileGroups = new Dictionary<string, FileGroup>();

            ReadFileGroups(newAbpVersionFiles, fileGroups, (group, file) => group.NewAbpFile = file);
            ReadFileGroups(currentAbpVersionFiles, fileGroups, (group, file) => group.CurrentAbpFile = file);
            ReadFileGroups(projectFiles, fileGroups, (group, file) => group.ProjectFile = file);

            return fileGroups.Values.ToList();
        }


        private void ReadFileGroups(IEnumerable<SingleFile> singleFiles, Dictionary<string, FileGroup> fileGroups, Action<FileGroup, SingleFile> storeFile)
        {
            foreach (var singleFile in singleFiles)
            {
                var relativePath = singleFile.RelativePath;

                if (fileGroups.ContainsKey(relativePath))
                {
                    storeFile.Invoke(fileGroups[relativePath], singleFile);
                }
                else
                {
                    var addFileGroup = new FileGroup();

                    storeFile.Invoke(addFileGroup, singleFile);

                    fileGroups.Add(relativePath, addFileGroup);
                }
            }
        }
    }
}