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
        public string host = null;
        public string user = null;
        public string pass = null;
        public string localFilePath = @"C:";
        public string remoteFilePath = "/";

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
                //PopulateTree(drive, node);
            }
        }

        public void PopulateTree(string dir, TreeNode node)
        {
            // get the information of the directory
            DirectoryInfo directory = new DirectoryInfo(dir);
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
            if (hostNameTxtBox.Text == "" || userNameTxtBox.Text == "" || passwrdTxtBox.Text == "")
            {
                MessageBox.Show("Please Enter Connection Details");
                return;
            }
            else
            {
                host = "ftp://" + hostNameTxtBox.Text;
                user = userNameTxtBox.Text;
                pass= passwrdTxtBox.Text;

                Ftp ftpClient = new Ftp(host, user, pass);

                remoteTreeView.Nodes.Clear();
                string[] remoteContents = ftpClient.directoryList("/");


                foreach (string remote in remoteContents)
                {
                    if (remote == "")
                    {
                        break;
                    }
                    else
                    {
                        TreeNode node = new TreeNode(remote);
                        node.Tag = remote;
                        node.ImageIndex = 0;
                        remoteTreeView.Nodes.Add(node);
                    }
                }
                //ftpClient.upload("webspace/httpdocs/screen.png", @"C:\SimpleFTP\screen.png");
                ftpClient = null;
            }
        }

        private void remoteTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = remoteTreeView.SelectedNode;
            string ext = System.IO.Path.GetExtension(node.Text);

            if (ext == "")
                populateRemoteNode(node);
            else
                downloadFile(node.FullPath,node.Text);
        }

        private void localTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = localTreeView.SelectedNode;
            localFilePath = node.FullPath;

            node.Nodes.Clear();
            FileAttributes attr = File.GetAttributes(node.FullPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                populateNode(node);
            else
                //MessageBox.Show("Its a file");
                uploadFile(node.FullPath, node.Text);

        }

        private void populateRemoteNode(TreeNode node)
        {
            Ftp ftpClient = new Ftp(host, user, pass);

            string[] remoteContents = ftpClient.directoryList("/" + node.FullPath + "/");
            node.Nodes.Clear();
            foreach (string remote in remoteContents)
            {
                if (remote == "")
                    break;
                else
                    node.Nodes.Add(remote);
            }

            node.Expand();

            ftpClient = null;
        }

        private void populateNode(TreeNode node)
        {
            try
            {
                string dir = node.FullPath;
                Console.WriteLine("Node: {0}", dir);
                DirectoryInfo directory = new DirectoryInfo(dir);

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
            localFilePath = localTreeView.SelectedNode.FullPath;
        }

        private void downloadFile(string remoteFilePath,string localFileName)
        {
            Ftp ftpClient = new Ftp(host, user, pass);

            localFilePath += @"\" + localFileName;

            localFilePath.Replace("\\\\", "\\");
            remoteFilePath.Replace("\\\\", "\\");

            ftpClient.download(remoteFilePath,localFilePath);

            MessageBox.Show("Transfer of " + localFileName + " Complete!");
            localFilePath = localTreeView.SelectedNode.FullPath;
            ftpClient = null;
        }
        
        private void uploadFile(string localFile, string fileName)
        {

            Ftp ftpClient = new Ftp(host, user, pass);

            remoteFilePath += "/" + fileName;
            ftpClient.upload(remoteFilePath, localFile);
            ftpClient = null;
            MessageBox.Show("Successful upload of " + fileName);
        }

        private void remoteTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            remoteFilePath = remoteTreeView.SelectedNode.FullPath;
        }
    }
}
