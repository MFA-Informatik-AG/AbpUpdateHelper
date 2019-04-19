using System;
using System.Collections.Generic;
using System.Text;

namespace AbpUpdateHelper.PostUpdateActions
{
    public class ModifyPackageJson : IPostUpdateFileGroupAction
    {
        public void Run(FileGroup fileGroup, string destinationFolder)
        {
            if (fileGroup.ExistsAbpFile("package.json"))
            {
                var packageJson = fileGroup.ReadTextLoadAbpFile(destinationFolder);

                packageJson = packageJson.Replace(
                    @"""webpack --progress --profile --watch --mode=development""",
                    @"""npx webpack --progress --profile --mode=development"""
                );

                packageJson = packageJson.Replace(
                    @"""webpack --mode=production""",
                    @"""npx webpack --mode=production"""
                );

                fileGroup.WriteTextAbpFile(packageJson, destinationFolder);
            }
        }
    }
}
