using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPClient.Dialogs
{
    public partial class CreateRemoteDirectoryForm : Form
    {
        public CreateRemoteDirectoryForm()
        {
            InitializeComponent();
        }

        public string newDirectoryName
        {
            get { return newDirectoryTxtBox.Text; }
        }

        private void AcceptButton_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
