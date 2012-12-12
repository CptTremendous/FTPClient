using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FTPClient
{
    public partial class Form1 : Form
    {
        public string strHost = null;
        public string strUser = null;
        public string strPass = null;
        public string strLocalFilePath = @"C:";
        public string strRemoteFilePath = "/";

        public Form1()
        {
            InitializeComponent();

            DisplayLocalFileTree();
        }


        private void DisplayLocalFileTree()
        {
            string[] drives = Environment.GetLogicalDrives();

            foreach (string drive in drives)
            {
                TreeNode node = new TreeNode(drive);
                node.Tag = drive;
                node.ImageIndex = -1;
                localTreeView.Nodes.Add(node);
            }
        }

        public void PopulateTree(string strDirectory, TreeNode node)
        {
            // get the information of the directory
            DirectoryInfo directory = new DirectoryInfo(strDirectory);
            // loop through each subdirectory
            try
            {
                foreach (DirectoryInfo d in directory.GetDirectories())
                {
                    // create a new node
                    TreeNode t = new TreeNode(d.Name);
                    // populate the new node recursively
                    PopulateTree(d.FullName, t);
                    node.Nodes.Add(t); // add the node to the "master" node
                }
            
                // lastly, loop through each file in the directory, and add these as nodes
                foreach (FileInfo f in directory.GetFiles())
                {
                    // create a new node
                    TreeNode t = new TreeNode(f.Name);
                    // add it to the "master"
                    node.Nodes.Add(t);
                }
            }
            catch (Exception ex) { }
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (hostNameTxtBox.Text == "" || userNameTxtBox.Text == "" || passwrdTxtBox.Text == "")
            {
                MessageBox.Show("Please Enter Connection Details");
                return;
            }
            else
            {
                if(portTextBox.Text == "")
                    strHost = "ftp://" + hostNameTxtBox.Text;
                else
                    strHost = "ftp://" + hostNameTxtBox.Text + ":" + portTextBox.Text;

                strUser = userNameTxtBox.Text;
                strPass= passwrdTxtBox.Text;

                Ftp ftpClient = new Ftp(strHost, strUser, strPass);

                remoteTreeView.Nodes.Clear();
                string[] remoteContents = ftpClient.directoryList("/");


                foreach (string strRemote in remoteContents)
                {
                    if (strRemote == "")
                    {
                        break;
                    }
                    else
                    {
                        TreeNode node = new TreeNode(strRemote);
                        node.Tag = strRemote;
                        node.ImageIndex = 0;
                        remoteTreeView.Nodes.Add(node);
                    }
                }
                ftpClient = null;
            }
            Cursor.Current = Cursors.Default;
        }

        private void remoteTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = remoteTreeView.SelectedNode;

            if (checkIsFile(node))
                downloadFile(node.FullPath, node.Text);
            else
                populateRemoteNode(node);
        }

        private bool checkIsFile(TreeNode node)
        {
            string ext = System.IO.Path.GetExtension(node.Text);

            if (ext == "")
                return false;
            else
                return true;
        }

        private void localTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = localTreeView.SelectedNode;
            strLocalFilePath = node.FullPath;

            node.Nodes.Clear();
            FileAttributes attr = File.GetAttributes(node.FullPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                populateNode(node);
            else
                uploadFile(node.FullPath, node.Text);

        }

        private void populateRemoteNode(TreeNode node)
        {
            Ftp ftpClient = new Ftp(strHost, strUser, strPass);

            string[] remoteContents = ftpClient.directoryList("/" + node.FullPath + "/");
            node.Nodes.Clear();
            foreach (string strRemote in remoteContents)
            {
                if (strRemote == "")
                    break;
                else
                    node.Nodes.Add(strRemote);
            }

            node.Expand();

            ftpClient = null;
        }

        private void populateNode(TreeNode node)
        {
            try
            {
                string strDir = node.FullPath;
                Console.WriteLine("Node: {0}", strDir);
                DirectoryInfo directory = new DirectoryInfo(strDir);

                foreach (DirectoryInfo d in directory.GetDirectories())
                {
                    node.Nodes.Add(d.Name);
                }

                foreach (FileInfo f in directory.GetFiles())
                {
                    TreeNode file = new TreeNode(f.Name);
                    file.Tag = f.Name;
                    file.ImageIndex = 2;
                    file.SelectedImageIndex = 2;
                    node.Nodes.Add(file);
                }

                node.Expand();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void localTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (checkIsFile(localTreeView.SelectedNode))
                strLocalFilePath = localTreeView.SelectedNode.Parent.FullPath;
            else
                strLocalFilePath = localTreeView.SelectedNode.FullPath;
        }

        private void downloadFile(string strRemoteFilePath,string strLocalFileName)
        {
            Ftp ftpClient = new Ftp(strHost, strUser, strPass);

            strLocalFilePath += @"\" + strLocalFileName;

            strLocalFilePath.Replace("\\\\", "\\");
            strRemoteFilePath.Replace("\\\\", "\\");

            ftpClient.download(strRemoteFilePath,strLocalFilePath);

            MessageBox.Show("Transfer of " + strLocalFileName + " Complete!");
            strLocalFilePath = localTreeView.SelectedNode.FullPath;
            ftpClient = null;
        }
        
        private void uploadFile(string strLocalFile, string strFileName)
        {

            Ftp ftpClient = new Ftp(strHost, strUser, strPass);

            strRemoteFilePath += "/" + strFileName;
            ftpClient.upload(strRemoteFilePath, strLocalFile);
            ftpClient = null;
            MessageBox.Show("Successful upload of " + strFileName);
        }

        private void remoteTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (checkIsFile(remoteTreeView.SelectedNode))
                strRemoteFilePath = remoteTreeView.SelectedNode.Parent.FullPath;
            else
                strRemoteFilePath = remoteTreeView.SelectedNode.FullPath;
        }

        private void remoteTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode nodeUnderMouse = remoteTreeView.GetNodeAt(e.X, e.Y);
                string strNodePath = nodeUnderMouse.FullPath;

                ContextMenuStrip cm = new ContextMenuStrip();

                ToolStripItem renameItem = cm.Items.Add("Rename");
                ToolStripItem deleteItem = cm.Items.Add("Delete");
                renameItem.Click += new EventHandler(renameItem_Click);
                deleteItem.Click += new EventHandler(deleteItem_Click);

                cm.Show(remoteTreeView, e.Location);
            }
            
        }

        void renameItem_Click(object sender, EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            string strRemotePath ="";

            if (clickedItem.Text.Equals("Rename"))
            {
                strRemotePath = remoteTreeView.SelectedNode.FullPath;
                using (RenameForm renameForm = new RenameForm())
                {
                    if (renameForm.ShowDialog() == DialogResult.OK)
                    {

                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            string strNewFileName = renameForm.newFileName;
                            Ftp ftpClient = new Ftp(strHost, strUser, strPass);
                            ftpClient.rename(strRemotePath, strNewFileName);
                            ftpClient = null;
                            populateRemoteNode(remoteTreeView.SelectedNode.Parent);
                            remoteTreeView.SelectedNode.Parent.Expand();
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("File Rename Successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        catch (Exception ex)
                        {
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("File Rename Unsuccessful\n" + ex.Message, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        void deleteItem_Click(object sender, EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            string strRemotePath = "";

            if (clickedItem.Text.Equals("Delete"))
            {
                strRemotePath = remoteTreeView.SelectedNode.FullPath;
                DialogResult deleteResult = MessageBox.Show("Delete will be Permanent. Continue?","Please Confirm Delete",
                    MessageBoxButtons.YesNo,MessageBoxIcon.Warning);

                if (deleteResult == DialogResult.Yes)
                {
                    Ftp ftpClient = new Ftp(strHost, strUser, strPass);
                    ftpClient.delete(strRemotePath);
                    ftpClient = null;
                    remoteTreeView.Refresh();
                }
            }
        }
    }
}
