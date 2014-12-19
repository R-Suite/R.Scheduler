using System;
using System.Collections.Generic;

namespace R.Scheduler.Ftp
{
    public interface IFtpLibrary :IDisposable
    {
        /// <summary>
        /// Binds the library to the specified host and port
        /// </summary>
        /// <param name="host">The address of the target ftp site</param>
        /// <param name="serverPort">port</param>
        /// <param name="userName">The username</param>
        /// <param name="password">The password</param>
        void Connect(string host, int serverPort, string userName, string password, string remoteDirectoryPath = null);

        /// <summary>
        /// Downloads the specified file into the target file
        /// </summary>
        /// <param name="filename">The file being downloaded</param>
        /// <param name="target">The local filename teh contents are written into</param>
        void GetFile(string filename, string target);

        /// <summary>
        /// Uploads the contents of source file to the ftp site with the name provided
        /// </summary>
        /// <param name="filename">The name of the file being uploaded</param>
        /// <param name="source">The full path to the local file being uploaded</param>
        void PutFile(string filename, string source);

        /// <summary>
        /// Returns the list of items in the current directory
        /// </summary>
        /// <returns></returns>
        IList<string> GetDirectoryItems();

        /// <summary>
        /// Downloads the files with specified extension into the local directory
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <param name="localDir"></param>
        /// <param name="cutOff"></param>
        void GetFiles(string fileExtension, string localDir, string cutOff = null);
    }
}
