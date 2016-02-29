using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using System.Windows;
using Microsoft.VisualStudio.Text;
using RefactoringEssentials.Converter;

namespace RefactoringEssentials.VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConvertCSToVBCommand
    {
        public const int MainMenuCommandId = 0x0100;
        public const int CtxMenuCommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a3378a21-e939-40c9-9e4b-eb0cec7b7854");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertCSToVBCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ConvertCSToVBCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                // Command in main menu
                var menuCommandID = new CommandID(CommandSet, MainMenuCommandId);
                var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                commandService.AddCommand(menuItem);

                // Command in code editor's context menu
                var ctxMenuCommandID = new CommandID(CommandSet, CtxMenuCommandId);
                var ctxMenuItem = new OleMenuCommand(this.MenuItemCallback, ctxMenuCommandID);
                ctxMenuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                commandService.AddCommand(ctxMenuItem);
            }
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = !GetCSSelectionInCurrentView()?.StreamSelectionSpan.IsEmpty ?? false;
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConvertCSToVBCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
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
        public static void Initialize(Package package)
        {
            Instance = new ConvertCSToVBCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            string title = "Convert C# to VB:";

            string selectedText = GetCSSelectionInCurrentView()?.StreamSelectionSpan.GetText();
            string convertedText = null;
            try
            {
                if (!TryConvertingCSToVBCode(selectedText, out convertedText))
                {
                    VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        "Selected C# code seems to have errors or to be incomplete.",
                        title,
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }
            }
            catch (Exception ex)
            {
                // Show error
                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    $"An error has occured during conversion: {ex.ToString()}",
                    title,
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return;
            }

            string message = convertedText;

            // Direct output for debugging
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            WriteStatusBarText("Copied converted VB code to clipboard.");

            Clipboard.SetText(convertedText);
        }

        IWpfTextViewHost GetCurrentCSViewHost()
        {
            IVsTextManager txtMgr = (IVsTextManager)ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView vTextView = null;
            int mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
            IVsUserData userData = vTextView as IVsUserData;
            if (userData == null)
                return null;

            IWpfTextViewHost viewHost;
            object holder;
            Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            userData.GetData(ref guidViewHost, out holder);
            viewHost = (IWpfTextViewHost)holder;

            if (viewHost == null)
                return null;

            ITextDocument textDocument;
            viewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            if ((textDocument == null) || !textDocument.FilePath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
                return null;

            return viewHost;
        }

        ITextSelection GetCSSelectionInCurrentView()
        {
            IWpfTextViewHost viewHost = GetCurrentCSViewHost();
            if (viewHost == null)
                return null;

            return viewHost.TextView.Selection;
        }

        bool TryConvertingCSToVBCode(string csCode, out string vbCode)
        {
            vbCode = null;

            var codeWithOptions = new CodeWithOptions(csCode)
                .WithDefaultReferences();
            var result = CodeConverter.Convert(codeWithOptions);

            if (result.Success)
            {
                vbCode = result.ConvertedCode;
                return true;
            }

            return false;
        }

        void WriteStatusBarText(string text)
        {
            IVsStatusbar statusBar = (IVsStatusbar)ServiceProvider.GetService(typeof(SVsStatusbar));
            if (statusBar == null)
                return;

            int frozen;
            statusBar.IsFrozen(out frozen);
            if (frozen != 0)
            {
                statusBar.FreezeOutput(0);
            }

            statusBar.SetText(text);

            statusBar.FreezeOutput(1);
        }
    }
}
