using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using RefactoringEssentials.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RefactoringEssentials.VsExtension
{
    static class CodeConversion
    {
        public static readonly string CSToVBConversionTitle = "Convert C# to VB:";

        public static void PerformCSToVBConversion(IServiceProvider serviceProvider, string inputCode)
        {
            string convertedText = null;
            try
            {
                if (!TryConvertingCSToVBCode(inputCode, out convertedText))
                {
                    VsShellUtilities.ShowMessageBox(
                        serviceProvider,
                        "Selected C# code seems to have errors or to be incomplete.",
                        CSToVBConversionTitle,
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }
            }
            catch (Exception ex)
            {
                VisualStudioInteraction.ShowException(serviceProvider, CSToVBConversionTitle, ex);
                return;
            }

            // Direct output for debugging
            //string message = convertedText;
            //VsShellUtilities.ShowMessageBox(
            //    serviceProvider,
            //    message,
            //    CSToVBConversionTitle,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            WriteStatusBarText(serviceProvider, "Copied converted VB code to clipboard.");

            Clipboard.SetText(convertedText);
        }

        static bool TryConvertingCSToVBCode(string csCode, out string vbCode)
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

        static void WriteStatusBarText(IServiceProvider serviceProvider, string text)
        {
            IVsStatusbar statusBar = (IVsStatusbar)serviceProvider.GetService(typeof(SVsStatusbar));
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

        static IWpfTextViewHost GetCurrentCSViewHost(IServiceProvider serviceProvider)
        {
            IWpfTextViewHost viewHost = VisualStudioInteraction.GetCurrentViewHost(serviceProvider);
            if (viewHost == null)
                return null;

            ITextDocument textDocument = viewHost.GetTextDocument();
            if ((textDocument == null) || !IsCSFileName(textDocument.FilePath))
                return null;

            return viewHost;
        }

        public static bool IsCSFileName(string fileName)
        {
            return fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
        }

        public static ITextSelection GetCSSelectionInCurrentView(IServiceProvider serviceProvider)
        {
            IWpfTextViewHost viewHost = GetCurrentCSViewHost(serviceProvider);
            if (viewHost == null)
                return null;

            return viewHost.TextView.Selection;
        }
    }
}
