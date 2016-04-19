using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        private void hideConverterContextMenuCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            optionsPage.HideConverterFromContextMenu = hideConverterContextMenuCheckBox.Checked;
        }
    }
}
