﻿using CORE_VS_PLUGIN.Utils;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace CORE_VS_PLUGIN.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CMD_GIT_PullAll
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4131;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e674492e-ff4e-4d34-b495-2f81eb5a86bf");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CMD_GIT_PullAll"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CMD_GIT_PullAll(AsyncPackage package, OleMenuCommandService commandService)
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
        public static CMD_GIT_PullAll Instance
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
            // Switch to the main thread - the call to AddCommand in CMD_GIT_PullAll's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CMD_GIT_PullAll(package, commandService);
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

            string title = "GIT execution";
            string message = "GIT repositories pull finished successfully!";
            var messageType = OLEMSGICON.OLEMSGICON_INFO;

            var svc = ServiceProvider.GetServiceAsync(typeof(EnvDTE.DTE)).Result;

            var dte = svc as EnvDTE.DTE;

            try
            {
                var projectFilesInfos = CORE_VS_PLUGIN_HELPER._GetProjectsFileInfos(dte);

                foreach (var projectFileInfo in projectFilesInfos)
                {
                    var commandoutput = CORE_VS_PLUGIN_HELPER.CommandOutput($"cd {projectFileInfo.DirectoryName} & git pull");
                    
                    CORE_VS_PLUGIN_HELPER.GIT_WriteToConsole(dte, $"{projectFileInfo.Name} - {commandoutput}");
                }
            }
            catch (Exception ex)
            {
                title = "Error!";
                message = "Failed to execute GIT pull!";

                messageType = OLEMSGICON.OLEMSGICON_CRITICAL;

                CORE_VS_PLUGIN_HELPER.GIT_WriteToConsole(dte, ex.Message);
                CORE_VS_PLUGIN_HELPER.GIT_WriteToConsole(dte, ex.StackTrace);

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    CORE_VS_PLUGIN_HELPER.GIT_WriteToConsole(dte, ex.InnerException.Message);
                }
            }
            finally
            {
                CORE_VS_PLUGIN_HELPER.GIT_WriteToConsole(dte, "Finished pulling all GIT repositories!");

                VsShellUtilities.ShowMessageBox(
                package,
                message,
                title,
                messageType,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}