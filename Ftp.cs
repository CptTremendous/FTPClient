using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace FTPClient
{
    class Ftp
    {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        // Constructor
        public Ftp(string hostAddress, string userName, string password)
        {
            host = hostAddress;
            user = userName;
            pass = password;
        }

        // Download
        public void download(string remoteFile, string localFile)
        {
            try
            {
                // Create Request
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                // Log In
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                // Request Type
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                
                // Get Response
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                // Get server response stream 
                ftpStream = ftpResponse.GetResponseStream();

                FileStream localFileStream = new FileStream(localFile, FileMode.Create);

                // Buffer for downloaded data
                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);

                // Download File
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex) { }

                // Housekeeping
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { }
            return;
        }

        // Upload File 
        public void upload(string remoteFile, string localFile)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);

                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                ftpStream = ftpRequest.GetRequestStream();

                FileStream localFileStream = new FileStream(localFile, FileMode.Open);

                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);

                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex) 
                { }
                // Housekeeping
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }


        // Delete
        public void delete(string filePath)
        {
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + filePath);

                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            { }
            return;
        }

        // Rename
        public void rename(string currentPath, string newName)
        {
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentPath);

                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                ftpRequest.RenameTo = newName;

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                ftpResponse.Close();
                ftpRequest = null;
                MessageBox.Show("File Rename Successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Rename Unsuccessful\n" + ex.Message, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        // Create Directory

        // Get Directory Contents
        public string[] directoryList(string directory)
        {
            try
            {
                // Create an FTP Request 
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                ftpStream = ftpResponse.GetResponseStream();

                StreamReader ftpReader = new StreamReader(ftpStream);

                string directoryRaw = null;

                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                
                // Housekeeping
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;

                try 
                { 
                    string[] directoryList = directoryRaw.Split("|".ToCharArray()); 
                    return directoryList; 
                }
                catch (Exception ex) { }
            }
            catch (Exception ex) { }
            // Return an Empty string Array if an Exception Occurs
            return new string[] { "" };
        }
    }
}
