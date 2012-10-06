﻿using System;
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
                node.ImageIndex = 0;
                localTreeView.Nodes.Add(node);
                PopulateTree(drive, node);
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
                    TreeNode node = new TreeNode(remote);
                    node.Tag = remote;
                    node.ImageIndex = 0;
                    remoteTreeView.Nodes.Add(node);
                }
                ftpClient = null;
            }
        }


    }
}
