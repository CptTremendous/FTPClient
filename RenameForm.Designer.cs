namespace FTPClient
{
    partial class RenameForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameForm));
            this.renameCancelButton = new System.Windows.Forms.Button();
            this.renameAcceptButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.newNameTxtBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // renameCancelButton
            // 
            this.renameCancelButton.Location = new System.Drawing.Point(220, 61);
            this.renameCancelButton.Name = "renameCancelButton";
            this.renameCancelButton.Size = new System.Drawing.Size(75, 23);
            this.renameCancelButton.TabIndex = 0;
            this.renameCancelButton.Text = "Cancel";
            this.renameCancelButton.UseVisualStyleBackColor = true;
            // 
            // renameAcceptButton
            // 
            this.renameAcceptButton.Location = new System.Drawing.Point(139, 61);
            this.renameAcceptButton.Name = "renameAcceptButton";
            this.renameAcceptButton.Size = new System.Drawing.Size(75, 23);
            this.renameAcceptButton.TabIndex = 1;
            this.renameAcceptButton.Text = "Accept";
            this.renameAcceptButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "New File Name:";
            // 
            // newNameTxtBox
            // 
            this.newNameTxtBox.Location = new System.Drawing.Point(13, 30);
            this.newNameTxtBox.Name = "newNameTxtBox";
            this.newNameTxtBox.Size = new System.Drawing.Size(282, 20);
            this.newNameTxtBox.TabIndex = 3;
            // 
            // RenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(307, 98);
            this.Controls.Add(this.newNameTxtBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.renameAcceptButton);
            this.Controls.Add(this.renameCancelButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rename";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button renameCancelButton;
        private System.Windows.Forms.Button renameAcceptButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox newNameTxtBox;
    }
}