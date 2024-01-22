﻿using CORE_VS_PLUGIN.GENERATOR;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CORE_VS_PLUGIN.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CMD_Generate_Query
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4131;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("1d0d17d4-abaf-4a9e-b385-1b0b6ceb2670");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CMD_Generate_Query"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CMD_Generate_Query(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CMD_Generate_Query Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CMD_Generate_Query's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CMD_Generate_Query(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var message = "Successfully generated code!";
            var title = nameof(CORE_VS_PLUGIN);
            var messageType = OLEMSGICON.OLEMSGICON_INFO;

            var selectedItems = ((UIHierarchy)((DTE2)ServiceProvider.GetServiceAsync(typeof(DTE)).Result).Windows.Item("{3AE79031-E1BC-11D0-8F78-00A0C9110057}").Object).SelectedItems as object[];

            var filesPaths = new List<string>();

            foreach (var item in selectedItems)
            {
                if ((item as UIHierarchyItem)?.Object is ProjectItem)
                {
                    var fileInfo = (ProjectItem)(((UIHierarchyItem)item).Object);

                    if (!string.IsNullOrEmpty(fileInfo.Name) && fileInfo.Name.ToLower().EndsWith("xml"))
                    {
                        filesPaths.Add(fileInfo.FileNames[1]);
                    }
                }
            }

            if (!filesPaths.Any())
            {
                title = "Warning!";
                message = "No valid XML files were selected for code generating!";

                messageType = OLEMSGICON.OLEMSGICON_WARNING;
            }
            else
            {
                var application = (DTE2)ServiceProvider.GetServiceAsync(typeof(DTE)).Result;

                var selectedSolutionItems = GetSelectedSolutionItems(application, ".xml");

                foreach (var selectedItem in selectedSolutionItems)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        ["Project_Path"] = selectedItem.ContainingProjectPath,
                        ["File_Path"] = selectedItem.SelectedFilePath
                    };

                    var content = File.ReadAllText(selectedItem.SelectedFilePath);
                    var serializer = new XmlSerializer(typeof(CORE_DB_QUERY_XML_Template));

                    CORE_DB_QUERY_XML_Template xmlTemplate = null;

                    using (var reader = new StringReader(content))
                    {
                        xmlTemplate = (CORE_DB_QUERY_XML_Template)serializer.Deserialize(reader);
                    }

                    if (xmlTemplate != null)
                    {
                        CORE_DB_Query_Generator.GenerateQuery(selectedItem.Item.ContainingProject, xmlTemplate, parameters);
                    }
                }
            }

            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                messageType,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public List<SelectedSolutionItem> GetSelectedSolutionItems(DTE2 application, string extensionFilter = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<SelectedSolutionItem> solutionItems = new List<SelectedSolutionItem>();

            UIHierarchy solutionExplorer = application.ToolWindows.SolutionExplorer;
            var items = solutionExplorer.SelectedItems as Array;

            foreach (UIHierarchyItem item in items)
            {
                if (item.Object is ProjectItem)
                {
                    ProjectItem projectItem = item.Object as ProjectItem;
                    SelectedSolutionItem solutionItem = new SelectedSolutionItem()
                    {
                        ContainingProjectPath = projectItem.ContainingProject.Properties.Item("FullPath").Value.ToString(),
                        SelectedFilePath = projectItem.Properties.Item("FullPath").Value.ToString(),
                        Item = projectItem
                    };

                    if (extensionFilter == null || solutionItem.SelectedFilePath.EndsWith(extensionFilter))
                    {
                        solutionItems.Add(solutionItem);
                    }
                }
            }

            return solutionItems;
        }
    }

    class SelectedSolutionItem
    {
        public ProjectItem Item { get; set; }
        public string SelectedFilePath { get; set; }
        public string ContainingProjectPath { get; set; }
    }
}