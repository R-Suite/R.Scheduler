using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Common.Logging;

namespace R.Scheduler.Ftp
{
    public class FtpLibrary : IFtpLibrary
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _requestUri;
        private int _serverPort;
        private string _userName;
        private string _password;
        private const int BufferSize = 2048;

        public void Connect(string host, int serverPort, string userName, string password, string remoteDirectoryPath = null)
        {
            _requestUri = host;
            if (!string.IsNullOrEmpty(remoteDirectoryPath))
            {
                _requestUri += "/" + remoteDirectoryPath;
            }

            _serverPort = serverPort;
            _userName = userName;
            _password = password;
        }

        public IList<string> GetDirectoryItems()
        {
            IList<string> retval = new List<string>();
            try
            {
                var ftpRequest = (FtpWebRequest)WebRequest.Create(_requestUri);
                ftpRequest.Credentials = new NetworkCredential(_userName, _password);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                // Specify the Type of FTP Request
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                WebResponse response = ftpRequest.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());

                string line = reader.ReadLine();
                while (line != null)
                {
                    retval.Add(line);

                    line = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

            return retval;
        }

        public void GetFiles(string fileExtension, string localDir, string cutOff)
        {
            TimeSpan cutOffTimeSpan;
            if (!string.IsNullOrEmpty(cutOff))
            {
                if (!TimeSpan.TryParse(cutOff, out cutOffTimeSpan))
                {
                    throw new ArgumentException(string.Format("Invalid cutOffTimeSpan format [{0}] specified.", cutOffTimeSpan));
                }

                var dirItems = GetDirectoryItems();

                foreach (var dirItem in dirItems)
                {
                    DateTime dateCreated = DateTime.MinValue;
                    bool dateCreatedAssigned = false;
                    for (int i = 0; i < dirItem.Length; i++)
                    {
                        for (int j = 8; j < 30; j++)
                        {
                            string dateTest = dirItem.Substring(i, j);
                            if (DateTime.TryParse(dateTest, out dateCreated))
                            {
                                dateCreatedAssigned = true;
                                break;
                            }
                        }

                        if (dateCreatedAssigned)
                        {
                            break;
                        }
                    }

                    if (dateCreatedAssigned && dateCreated > DateTime.UtcNow.Subtract(cutOffTimeSpan))
                    {
                        IList<string> fileNames = new List<string>();
                        var dirItemArr = dirItem.Split(' ');
                        foreach (var fileNameTest in dirItemArr)
                        {
                            if (fileNameTest.EndsWith(fileExtension))
                            {
                                fileNames.Add(fileNameTest);
                                break;
                            }
                        }
                    }

                    //todo: get files
                }
            }
        }

        public void GetFile(string filename, string target)
        {
            try
            {
                var ftpRequest = (FtpWebRequest)WebRequest.Create(_requestUri + "/" + filename);
                ftpRequest.Credentials = new NetworkCredential(_userName, _password);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                // Specify the Type of FTP Request
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                // Establish Return Communication with the FTP Server
                var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                // Get the FTP Server's Response Stream
                Stream ftpStream = ftpResponse.GetResponseStream();
                // Open a File Stream to Write the Downloaded File
                var localFileStream = new FileStream(target, FileMode.Create);
                // Buffer for the Downloaded Data
                var byteBuffer = new byte[BufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, BufferSize);
                // Download the File by Writing the Buffered Data Until the Transfer is Complete
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, BufferSize);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            
                // Resource Cleanup
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        public void PutFile(string filename, string source)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
