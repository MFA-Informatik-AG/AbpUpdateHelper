using System;
using System.Collections.Generic;
using System.Reflection;
using AbpUpdateHelper.FileGroupActions;
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
            app.ExtendedHelpText = "The update helper provides (hopefully) some support updating an existing ASP.NET Zero project to a newer version of the framework.";

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
                        new SmartMergeMergeAction(),
                        new CodeCompareMergeAction(),
                        new WinMergeAction()
                    };

                    var fileActions = new List<IFileGroupAction>
                    {
                        new FileGroupActionAddAbpFile(),
                        new FileGroupActionMergeAbpFile(mergeActions),
                        new FileGroupActionProjectFileOnly(),
                        new FileGroupActionRemoveAbpFile(),
                        new FileGroupActionUpdateAbpFile()
                    };

                    var controller = new AbpUpdateController(fileActions);

                    controller.UpdateAbpVersion(abpProjectName, abpNewVersionDirectory, abpCurrentVersionDirectory, projectDirectory, outputDirectory, skipExistingOutputFiles);

                    Console.WriteLine("Manual post update actions:");
                    Console.WriteLine(" -Check that the environment is up-to-date (VS, yarn, etc.)");
                    Console.WriteLine(" -Delete node_modules folder");
                    Console.WriteLine(" -Delete wwwroot\\lib folder");
                    Console.WriteLine(" -Delete wwwroot\\assets\\jcrop\\src folder");
                    Console.WriteLine(" -Run donet restore in the .web.mvc folder");
                    Console.WriteLine(" -Run yarn install and yarn upgrade in the .web.mvc folder");
                    Console.WriteLine(" -Update the netcore version in the projects if/where required (don't forget the test projects)");
                    Console.WriteLine(" -Update the nuget packages");
                    Console.WriteLine(" -Double-check missing dependencies and update the package.json file accordingly");
                    Console.WriteLine(" -Run the Database Migration tool");

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
    }
}