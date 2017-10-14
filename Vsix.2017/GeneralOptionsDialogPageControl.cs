using System;
using System.Windows.Forms;

namespace RefactoringEssentials.VsExtension
{
	public partial class GeneralOptionsDialogPageControl : UserControl
    {
        public GeneralOptionsDialogPageControl()
        {
            InitializeComponent();
        }

        internal GeneralOptionsDialogPage optionsPage;

        public void Initialize()
        {
            hideConverterContextMenuCheckBox.Checked = optionsPage.HideConverterFromContextMenu;
            versionInfo.Text += ReflectionNamespaces.VersionInfo;
        }

        private void hideConverterContextMenuCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            optionsPage.HideConverterFromContextMenu = hideConverterContextMenuCheckBox.Checked;
        }
    }
}
