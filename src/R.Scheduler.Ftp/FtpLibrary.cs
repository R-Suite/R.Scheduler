using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Common.Logging;

namespace R.Scheduler.Ftp
{
    public struct DirItemStruct
    {
        public string Flags;
        public string Owner;
        public string Group;
        public bool IsDirectory;
        public DateTime CreateTime;
        public string Name;
    }

    public enum DirItemListStyle
    {
        UnixStyle,
        WindowsStyle,
        Unknown
    }

    /// <summary>
    /// Implementation of IFtpLibrary using FtpWebRequest and FtpWebResponse classes.
    /// </summary>
    public class FtpLibrary : IFtpLibrary
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _requestUri;
        private string _userName;
        private string _password;
        private const int BufferSize = 2048;

        /// <summary>
        /// Only builds Request Uri but does not create physical connection to ftp server.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="serverPort">not in use</param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void Connect(string host, int serverPort, string userName, string password = null)
        {
            _requestUri = host;
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// Get all the files with specified <paramref name="fileExtensions" /> no older than <paramref name="cutOff" /> timespan.
        /// Matched files are downloaded into <paramref name="localDir" />
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="localDir"></param>
        /// <param name="fileExtensions"></param>
        /// <param name="cutOff"></param>
        public void GetFiles(string remotePath, string localDir, string fileExtensions, TimeSpan cutOff)
        {
            // Build list of file extensions
            var fileExtensionList = new List<string>();
            if (fileExtensions.Contains(","))
            {
                fileExtensionList = fileExtensions.Split(',').Select(s => s.Trim()).ToList();
            }
            else
            {
                fileExtensionList.Add(fileExtensions);
            }

            // If remotePath specified, adjust _requestUri
            if (!string.IsNullOrEmpty(remotePath))
            {
                _requestUri += "/" + remotePath;
            }

            // Get all directory items metadata 
            var dirItemsRaw = GetDirectoryItems() as IList<string>;
            IEnumerable<DirItemStruct> dirItemsParsed = GetDirItemsParsed(dirItemsRaw);

            // Download directory items with matching filetype/extension
            // and no older than a specified cut-off timespan
            foreach (var dirItem in dirItemsParsed)
            {
                if (!dirItem.IsDirectory &&
                    !string.IsNullOrEmpty(dirItem.Name) &&
                    fileExtensionList.Any(e => dirItem.Name.ToLower().EndsWith(e.ToLower())) &&
                    dirItem.CreateTime > DateTime.UtcNow.Subtract(cutOff))
                {
                    GetFile(dirItem.Name, Path.Combine(localDir, dirItem.Name));
                }
            }
        }

        /// <summary>
        /// Not relevant to this implementation
        /// </summary>
        public void Dispose()
        {}

        #region Private Methods

        /// <summary>
        /// Get detailed listing of files on ftp server.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetDirectoryItems()
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

        /// <summary>
        /// Get specified file into target file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="target"></param>
        private void GetFile(string filename, string target)
        {
            Logger.InfoFormat("Getting file {0} into {1}", filename, target);

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
                    Logger.ErrorFormat("Error writing buffered data. {0}", ex.ToString(), ex);
                }
            
                // Resource Cleanup
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error getting file. {0}", ex.ToString(), ex);
            }
        }


        /* 
         * Code for parsing FtpwebRequest response for ListDirectoryDetails 
         * http://blogs.msdn.com/b/adarshk/archive/2004/09/15/sample-code-for-parsing-ftpwebrequest-response-for-listdirectorydetails.aspx
         */

        /// <summary>
        /// Parse raw output from <see cref="WebRequestMethods.Ftp.ListDirectoryDetails"/> 
        /// into <see cref="DirItemStruct"/>
        /// </summary>
        /// <param name="dirItems"></param>
        /// <returns></returns>
        private IEnumerable<DirItemStruct> GetDirItemsParsed(IList<string> dirItems)
        {
            var retval = new List<DirItemStruct>();
            DirItemListStyle directoryListStyle = GuessFileListStyle(dirItems);
            foreach (string s in dirItems)
            {
                if (directoryListStyle != DirItemListStyle.Unknown && s != "")
                {
                    var f = new DirItemStruct {Name = ".."};
                    switch (directoryListStyle)
                    {
                        case DirItemListStyle.UnixStyle:
                            f = ParseFileStructFromUnixStyleRecord(s);
                            break;
                        case DirItemListStyle.WindowsStyle:
                            f = ParseFileStructFromWindowsStyleRecord(s);
                            break;
                    }
                    if (!(f.Name == "." || f.Name == ".."))
                    {
                        retval.Add(f);
                    }
                }
            }
            return retval.ToArray(); ;
        }

        /// <summary>
        /// Decide Windows-Style, Unixs-Style or unknown
        /// </summary>
        /// <param name="recordList"></param>
        /// <returns></returns>
        private DirItemListStyle GuessFileListStyle(IEnumerable<string> recordList)
        {
            foreach (string s in recordList)
            {
                if (s.Length > 10
                 && Regex.IsMatch(s.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return DirItemListStyle.UnixStyle;
                }
                if (s.Length > 8
                    && Regex.IsMatch(s.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return DirItemListStyle.WindowsStyle;
                }
            }
            return DirItemListStyle.Unknown;
        }

        /// <summary>
        /// Parse directory item from Windows-Style record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private DirItemStruct ParseFileStructFromWindowsStyleRecord(string record)
        {
            // Assuming the record style as 
            // 02-03-04  07:46PM       <DIR>          Append
            var f = new DirItemStruct();
            string processstr = record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            f.CreateTime = DateTime.ParseExact(dateStr + " " + timeStr, "MM-dd-yy h:mmtt", CultureInfo.InvariantCulture);
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                f.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                string[] strs = processstr.Split(new[] {' '}, StringSplitOptions.None);
                processstr = strs[1].Trim();
                f.IsDirectory = false;
            }
            f.Name = processstr;  //Rest is name   
            return f;
        }

        /// <summary>
        /// Parse directory item from Unix-Style record.
        /// Not currently supported.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private DirItemStruct ParseFileStructFromUnixStyleRecord(string record)
        {
            throw new NotImplementedException("Parsing from Unix-Style record is currently not supported.");

            /*
            // Assuming record style as
            // dr-xr-xr-x   1 owner    group               0 Nov 25  2002 bussys
            var f = new DirItemStruct();
            string processstr = record.Trim();
            f.Flags = processstr.Substring(0, 9);
            f.IsDirectory = (f.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //skip one part
            f.Owner = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            f.Group = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //skip one part
            f.CreateTime = DateTime.Parse(_cutSubstringFromStringWithTrim(ref processstr, ' ', 8));
            f.Name = processstr;   //Rest of the part is name
            return f;
            */
        }

        /*
        private string _cutSubstringFromStringWithTrim(ref string s, char c, int startIndex)
        {
            int pos1 = s.IndexOf(c, startIndex);
            string retString = s.Substring(0, pos1);
            s = (s.Substring(pos1)).Trim();
            return retString;
        }
        */

        #endregion
    }
}
