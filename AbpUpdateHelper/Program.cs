using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AbpUpdateHelper.FileGroupActions;
using AbpUpdateHelper.PostUpdateActions;
using Microsoft.Extensions.CommandLineUtils;

namespace AbpUpdateHelper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.Name = "ASP.NET Zero Update Helper";
            app.Description = "ASP.NET Zero Framework Update Helper";
            app.HelpOption("-?|-h|--help");
            app.ExtendedHelpText = "The update helper provides some support updating an existing ASP.NET Zero project to a newer version of the framework.";

            app.VersionOption("-v|--version", () => $"Version {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");

            app.OnExecute(() =>
            {
                app.ShowHelp();

                return 0;
            });

            app.Command("coreMvc", (command) =>
            {
                command.ExtendedHelpText = "Update an ASP.NET Zero 'ASP.NET CORE MVC & jQuery' project";
                command.Description = "Helps update an ASP.NET Zero 'ASP.NET CORE MVC & jQuery' project";
                command.HelpOption("-?|-h|--help");

                var abpProjectNameOption = command.Option("-z|--zero <projectname>",
                    "Name of the ASP.NET Zero Project (as from the project download option)",
                    CommandOptionType.SingleValue);

                var abpNewVersionDirectoryOption = command.Option("-n|--new <folder>",
                    "Folder with the new ASP.NET Zero 'ASP.NET CORE MVC & jQuery' version",
                    CommandOptionType.SingleValue);

                var abpCurrentVersionDirectoryOption = command.Option("-c|--current <folder>",
                    "Folder with the current ASP.NET Zero 'ASP.NET CORE MVC & jQuery' version",
                    CommandOptionType.SingleValue);

                var projectDirectoryOption = command.Option("-p|--project <folder>",
                    "Folder with your 'ASP.NET CORE MVC & jQuery' project to be updated",
                    CommandOptionType.SingleValue);

                var outputDirectoryOption = command.Option("-o|--output <folder>",
                    "Output folder with the updated files",
                    CommandOptionType.SingleValue);

                var skipExistingOutputFilesOption = command.Option("-s|--skipexisting",
                    "Existing files in the output folder are not overwritten",
                    CommandOptionType.NoValue);

                command.OnExecute(() =>
                {
                    if (!abpProjectNameOption.HasValue()
                        || !abpNewVersionDirectoryOption.HasValue()
                        || !abpCurrentVersionDirectoryOption.HasValue()
                        || !projectDirectoryOption.HasValue()
                        || !outputDirectoryOption.HasValue()
                    )
                    {
                        command.ShowHelp();

                        return -1;
                    }

                    var abpProjectName = abpProjectNameOption.Value();
                    var abpNewVersionDirectory = abpNewVersionDirectoryOption.Value();
                    var abpCurrentVersionDirectory = abpCurrentVersionDirectoryOption.Value();
                    var projectDirectory = projectDirectoryOption.Value();
                    var outputDirectory = outputDirectoryOption.Value();
                    var skipExistingOutputFiles = skipExistingOutputFilesOption.HasValue();

                    var mergeActions = new List<IMergeAction>
                    {
                        new SemanticMergeMergeAction(),
                        new CodeCompareMergeAction(),
                        new WinMergeAction()
                    };

                    var fileActions = new List<IFileGroupAction>
                    {
                        new FileGroupActionAddAbpFile(),
                        new FileGroupActionMergeAbpFile(mergeActions),
                        new FileGroupActionProjectFileOnly(),
                        new FileGroupActionRemoveAbpFile(),
                        new FileGroupActionUpdateAbpFile(),
                        new FileGroupActionSkipOldAbpFile()
                    };

                    var postUpdateFileGroupActions = new List<IPostUpdateFileGroupAction>
                    {
                        new ModifyPackageJson()
                    };

                    var controller = new AbpUpdateController(
                        fileActions,
                        postUpdateFileGroupActions
                        );

                    var copyAllways = new List<string>
                    {
                        @"(\.css)",
                        @"(\.less)",
                        @"(\\nvs\\)",
                    };

                    controller.UpdateAbpVersion(
                        abpProjectName, 
                        abpNewVersionDirectory, 
                        abpCurrentVersionDirectory, 
                        projectDirectory, 
                        outputDirectory, 
                        skipExistingOutputFiles,
                        copyAllways
                        );

                    CreateAbpUpdateBatch(outputDirectory);

                    Console.WriteLine("Manual post update actions:");
                    Console.WriteLine(" -Check that the environment is up-to-date (VS, yarn, etc.)");
                    Console.WriteLine(" -Update the netcore version in the projects if/where required (don't forget the test projects)");
                    Console.WriteLine(" -Update the nuget packages");
                    Console.WriteLine(" -Double-check missing dependencies and update the package.json file accordingly");
                    Console.WriteLine(" -Run the Database Migration tool");
                    Console.WriteLine(" -Global npm packages might cause issues (you find them in the AppData\\Roaming folder");
                    Console.WriteLine(" -Double-check bundling and minifing errors. It might be that charts.js conflicts with 'use strict'");

                    return 0;
                });
            });

            try
            {
                Console.WriteLine(app.Name);

                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
        }

        private static void CreateAbpUpdateBatch(string outputDirectory)
        {
            var abpUpdateBatch = new StringBuilder();

            abpUpdateBatch.AppendLine("rd /S /Q node_modules");
            abpUpdateBatch.AppendLine(@"rd /S /Q wwwroot\lib");
            abpUpdateBatch.AppendLine(@"rd /S /Q wwwroot\assets\jcrop\src");
            abpUpdateBatch.AppendLine(@"rd /S /Q wwwroot\view - resources\Areas\App\Views\_Bundles");
            abpUpdateBatch.AppendLine("del /Q package -lock.json");
            abpUpdateBatch.AppendLine("del /Q yarn.lock");

            abpUpdateBatch.AppendLine("call dotnet restore");
                
            abpUpdateBatch.AppendLine("call yarn add--dev webpack-cli");
            abpUpdateBatch.AppendLine("call yarn add--dev webpack");
            abpUpdateBatch.AppendLine("call yarn");
                
            abpUpdateBatch.AppendLine("npx webpack --progress--profile--mode = development");
            abpUpdateBatch.AppendLine("npx webpack --mode = production");

            var batchFile = outputDirectory + "\\abpupdate.bat";

            File.WriteAllText(batchFile, abpUpdateBatch.ToString());
        }
    }
}