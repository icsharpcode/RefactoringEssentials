namespace RefactoringEssentials.VsExtension
{
    partial class GeneralOptionsDialogPageControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.hideConverterContextMenuCheckBox = new System.Windows.Forms.CheckBox();
            this.generalGroup = new System.Windows.Forms.GroupBox();
            this.versionInfo = new System.Windows.Forms.Label();
            this.generalGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // hideConverterContextMenuCheckBox
            // 
            this.hideConverterContextMenuCheckBox.AutoSize = true;
            this.hideConverterContextMenuCheckBox.Location = new System.Drawing.Point(6, 19);
            this.hideConverterContextMenuCheckBox.Name = "hideConverterContextMenuCheckBox";
            this.hideConverterContextMenuCheckBox.Size = new System.Drawing.Size(215, 17);
            this.hideConverterContextMenuCheckBox.TabIndex = 2;
            this.hideConverterContextMenuCheckBox.Text = "Hide Code Converter from context menu";
            this.hideConverterContextMenuCheckBox.UseVisualStyleBackColor = true;
            this.hideConverterContextMenuCheckBox.CheckedChanged += new System.EventHandler(this.hideConverterContextMenuCheckBox_CheckedChanged);
            // 
            // generalGroup
            // 
            this.generalGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.generalGroup.Controls.Add(this.hideConverterContextMenuCheckBox);
            this.generalGroup.Location = new System.Drawing.Point(6, 38);
            this.generalGroup.Name = "generalGroup";
            this.generalGroup.Size = new System.Drawing.Size(382, 45);
            this.generalGroup.TabIndex = 1;
            this.generalGroup.TabStop = false;
            this.generalGroup.Text = "Code Converter options";
            // 
            // versionInfo
            // 
            this.versionInfo.AutoSize = true;
            this.versionInfo.Location = new System.Drawing.Point(3, 9);
            this.versionInfo.Name = "versionInfo";
            this.versionInfo.Size = new System.Drawing.Size(89, 13);
            this.versionInfo.TabIndex = 1;
            this.versionInfo.Text = "Detected Roslyn-";
            // 
            // GeneralOptionsDialogPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.versionInfo);
            this.Controls.Add(this.generalGroup);
            this.Name = "GeneralOptionsDialogPageControl";
            this.Size = new System.Drawing.Size(388, 122);
            this.generalGroup.ResumeLayout(false);
            this.generalGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox hideConverterContextMenuCheckBox;
        private System.Windows.Forms.GroupBox generalGroup;
        private System.Windows.Forms.Label versionInfo;
    }
}
