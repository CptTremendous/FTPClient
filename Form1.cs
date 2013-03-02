using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FTPClient.Dialogs;

namespace FTPClient
{
    public partial class Form1 : Form
    {
        public string strHost = null;
        public string strUser = null;
        public string strPass = null;
        public string strLocalFilePath = @"C:";
        public string strRemoteFilePath = "/";
        private System.ComponentModel.BackgroundWorker uploadBackgroundWorker;
        private System.ComponentModel.BackgroundWorker downloadBackgroundWorker;

        public Form1()
        {
            InitializeComponent();
            InitializeUploadBackgroundWorker();
            InitializeDownloadBackgroundWorker();
            localPathLabel.Text = strLocalFilePath;
            remotePathLabel.Text = "Not Connected";
            DisplayLocalFileTree();
        }

        private void InitializeDownloadBackgroundWorker()
        {
            downloadBackgroundWorker.WorkerReportsProgress = true;
            downloadBackgroundWorker.DoWork += new DoWorkEventHandler(downloadBackgroundWorker_DoWork);
            downloadBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadBackgroundWorker_RunWorkerCompleted);
            downloadBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(downloadBackgroundWorker_ProgressChanged);
        }

        private void downloadBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        private void downloadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("File Download Complete");
            progressBar1.Value = 0;
        }

        private void downloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string[] args = e.Argument as string[];
            string remoteFile = args[0];
            string localFile = args[1];

            Ftp ftp = new Ftp(strHost, strUser, strPass);
            ftp.download(remoteFile, localFile, worker, e);
            ftp = null;
        }

        private void InitializeUploadBackgroundWorker()
        {
            uploadBackgroundWorker.WorkerReportsProgress = true;
            uploadBackgroundWorker.DoWork += new DoWorkEventHandler(uploadBackgroundWorker_DoWork);
            uploadBackgroundWorker.RunWorkerCompleted +=  new RunWorkerCompletedEventHandler(uploadBackgroundWorker_RunWorkerCompleted);
            uploadBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(uploadBackgroundWorker_ProgressChanged);
        }

        private void uploadBackgroundWorker_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        private void uploadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("File Upload Complete");
            progressBar1.Value = 0;
            populateRemoteNode(remoteTreeView.SelectedNode.Parent);
            remoteTreeView.SelectedNode.Parent.Expand();
            remoteTreeView.Refresh();
        }

        private void uploadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation 
            // to the Result property of the DoWorkEventArgs 
            // object. This is will be available to the  
            // RunWorkerCompleted eventhandler.
            string[] args = e.Argument as string[];
            string remoteFile = args[0];
            string localFile = args[1];

            Ftp ftp = new Ftp(strHost,strUser,strPass);
            ftp.upload(remoteFile,localFile, worker, e);
            ftp = null;
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
                try
                {
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
                            if (!checkIsFile(node.Text))
                            {
                                node.ImageIndex = -1;
                                node.SelectedImageIndex = 1;
                            }
                            else
                            {
                                node.ImageIndex = 2;
                            }
                            remoteTreeView.Nodes.Add(node);
                        }
                        
                    }
                    remotePathLabel.Text = "/";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Retrieving Remote Files.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                ftpClient = null;
            }
            Cursor.Current = Cursors.Default;
        }

        private void remoteTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = remoteTreeView.SelectedNode;

            if (checkIsFile(node.Text))
                downloadFile(node.FullPath, node.Text);
            else
                populateRemoteNode(node);
        }

        private bool checkIsFile(string nodeText)
        {
            string ext = System.IO.Path.GetExtension(nodeText);

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
            try
            {
                Ftp ftpClient = new Ftp(strHost, strUser, strPass);

                string[] remoteContents = ftpClient.directoryList("/" + node.FullPath + "/");
                node.Nodes.Clear();
                foreach (string strRemote in remoteContents)
                {
                    if (strRemote == "")
                    {
                        break;
                    }
                    else
                    {
                        node.Nodes.Add(strRemote);
                    }
                }
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (!checkIsFile(childNode.Text))
                    {
                        childNode.ImageIndex = -1;
                        childNode.SelectedImageIndex = 1;
                    }
                    else
                    {
                        childNode.ImageIndex = 2;
                        childNode.SelectedImageIndex = 2;
                    }
                }
                node.Expand();

                ftpClient = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Retrieving Remote Contents\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (checkIsFile(localTreeView.SelectedNode.Text))
                strLocalFilePath = localTreeView.SelectedNode.Parent.FullPath;
            else
                strLocalFilePath = localTreeView.SelectedNode.FullPath;

            localPathLabel.Text = strLocalFilePath.Replace("\\",@"\");
        }

        private void downloadFile(string strRemoteFilePath,string strLocalFileName)
        { 
            strLocalFilePath += @"\" + strLocalFileName;

            strLocalFilePath.Replace("\\\\", "\\");
            strRemoteFilePath.Replace("\\\\", "\\");
            string[] files = { strRemoteFilePath, strLocalFilePath };

            downloadBackgroundWorker.RunWorkerAsync(files);
        }
        
        private void uploadFile(string strLocalFile, string strFileName)
        {
            //call run async
            strRemoteFilePath += "/" + strFileName;
            string[] files = { strRemoteFilePath, strLocalFile };

            uploadBackgroundWorker.RunWorkerAsync(files);
        }

        private void remoteTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (checkIsFile(remoteTreeView.SelectedNode.Text))
                strRemoteFilePath = remoteTreeView.SelectedNode.Parent.FullPath;
            else
                strRemoteFilePath = remoteTreeView.SelectedNode.FullPath;

            remotePathLabel.Text = strRemoteFilePath;
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

                if (!checkIsFile(nodeUnderMouse.Text))
                {
                    ToolStripItem createDirectoryItem = cm.Items.Add("Create Directory");
                    createDirectoryItem.Click += new EventHandler(createDirectoryItem_Click);
                }
                else
                {
                    ToolStripItem fileInfoItem = cm.Items.Add("Get File Info");
                    fileInfoItem.Click += new EventHandler(fileInfoItem_Click);
                }

                renameItem.Click += new EventHandler(renameItem_Click);
                deleteItem.Click += new EventHandler(deleteItem_Click);

                cm.Show(remoteTreeView, e.Location);
            }
            
        }

        void createDirectoryItem_Click(object sender, EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            string newDirectory = remoteTreeView.SelectedNode.FullPath;

            if (clickedItem.Text.Equals("Create Directory"))
            { 
                using(CreateRemoteDirectoryForm newDirectoryForm = new CreateRemoteDirectoryForm())
                {
                    if(newDirectoryForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            string strNewDirectoryName = newDirectoryForm.newDirectoryName;
                            newDirectory += "/" + strNewDirectoryName;

                            Ftp ftpClient = new Ftp(strHost, strUser, strPass);

                            ftpClient.createRemoteDirectory(newDirectory);

                            ftpClient = null;
                            populateRemoteNode(remoteTreeView.SelectedNode);
                            remoteTreeView.SelectedNode.Expand();
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Directory Creation Successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        catch(Exception ex)
                        {
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Directory Creation Unsuccessful\n" + ex.Message, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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

                    if (checkIsFile(remoteTreeView.SelectedNode.Text))
                        ftpClient.delete(strRemotePath);
                    else
                        ftpClient.deleteDirectory(strRemotePath);

                    populateRemoteNode(remoteTreeView.SelectedNode.Parent);
                    remoteTreeView.SelectedNode.Parent.Expand();
                    ftpClient = null;
                    remoteTreeView.Refresh();
                }
            }
        }

        void fileInfoItem_Click(object sender, EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            string currentFile = remoteTreeView.SelectedNode.FullPath;
            string[] fileInfo;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                Ftp ftpClient = new Ftp(strHost, strUser, strPass);

                fileInfo = ftpClient.getFileSize(currentFile);

                ftpClient = null;
                Cursor.Current = Cursors.Default;
                MessageBox.Show("File Size: " + fileInfo[0] + "\nDate Last Modified: " + fileInfo[1], "File Information");
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Failure to retrieve File Information\n" + ex.Message, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
