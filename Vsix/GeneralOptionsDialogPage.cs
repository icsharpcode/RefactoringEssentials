using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace RefactoringEssentials.VsExtension
{
    [Guid("81091F34-2F71-4A54-B5EF-29EBEDAE8BB2")]
    public class GeneralOptionsDialogPage : DialogPage
    {
        private bool disableConverterInContextMenu = false;

        public bool HideConverterFromContextMenu
        {
            get
            {
                return disableConverterInContextMenu;
            }
            set
            {
                disableConverterInContextMenu = value;
            }
        }

        protected override IWin32Window Window
        {
            get
            {
                GeneralOptionsDialogPageControl pageControl = new GeneralOptionsDialogPageControl();
                pageControl.optionsPage = this;
                pageControl.Initialize();
                return pageControl;
            }
        }
    }
}
