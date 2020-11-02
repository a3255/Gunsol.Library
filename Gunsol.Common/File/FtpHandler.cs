using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gunsol.Common.File
{
    /// <summary>
    /// FTP 다운로드/업로드 기능을 제공하는 Class
    /// </summary>
    public class FtpHandler
    {
        #region Property
        public string ftpId { get; set; }   // FTP 접속 ID
        public string ftpPw { get; set; }   // FTP 접속 PW
        private WebClient ftpClient;        // WebClient 객체
        #endregion

        #region Contructor
        /// <summary>
        /// Constructor To Initialize Property Using Empty Value
        /// </summary>
        public FtpHandler()
        {
            try
            {
                this.ftpClient = new WebClient();
                this.ftpId = string.Empty;
                this.ftpPw = string.Empty;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Constructor To Initialize Property Using Parameters
        /// </summary>
        /// <param name="ftpId">FTP 접속 ID</param>
        /// <param name="ftpPw">FTP 접속 PW</param>
        public FtpHandler(string ftpId, string ftpPw)
        {
            try
            {
                this.ftpClient = new WebClient();
                this.ftpClient.Credentials = new NetworkCredential(ftpId, ftpPw);
                this.ftpId = ftpId;
                this.ftpPw = ftpPw;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Download File From FTP Server
        /// </summary>
        /// <param name="remotePath">Remote File Path</param>
        /// <param name="localPath">Local File Path</param>
        public void Download(string remotePath, string localPath)
        {
            try
            {
                if (ftpId.Equals(string.Empty) || ftpPw.Equals(string.Empty))
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Download() Fail :: Credential Info Not Initialize", this.ToString()));
                }
                else
                {
                    if (ftpClient.Credentials == null)
                    {
                        ftpClient.Credentials = new NetworkCredential(ftpId, ftpPw);
                    }

                    byte[] ftpData = ftpClient.DownloadData(remotePath);

                    FileStream localFileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    localFileStream.Write(ftpData, 0, ftpData.Length);
                    localFileStream.Flush();
                    localFileStream.Close();

                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Download() Success :: Path = {1}", this.ToString(), localPath));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Download() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Upload File To FTP Server
        /// </summary>
        /// <param name="remotePath">Remote File Path</param>
        /// <param name="localPath">Local File Path</param>
        public void Upload(string remotePath, string localPath)
        {
            try
            {
                if (ftpId.Equals(string.Empty) || ftpPw.Equals(string.Empty))
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Upload() Fail :: Credential Info Not Initialize", this.ToString()));
                }
                else
                {
                    if (ftpClient.Credentials == null)
                    {
                        ftpClient.Credentials = new NetworkCredential(ftpId, ftpPw);
                    }

                    FileInfo fileInfo = new FileInfo(localPath);
                    FileStream localFileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    byte[] localFileData = new byte[fileInfo.Length];
                       
                    localFileStream.Read(localFileData, 0, localFileData.Length);
                    ftpClient.UploadData(remotePath, localFileData);
                    localFileStream.Close();

                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Upload() Success :: Path = {1}", this.ToString(), remotePath));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Upload() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Get File Contents From FTP Server
        /// </summary>
        /// <param name="remotePath">Remote File Path</param>
        /// <returns>File Contents</returns>
        public string GetFileData(string remotePath)
        {
            string ftpData = string.Empty;

            try
            {
                if (ftpId.Equals(string.Empty) || ftpPw.Equals(string.Empty))
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: GetFileData() Fail :: Credential Info Not Initialize", this.ToString()));
                }
                else
                {
                    if (ftpClient.Credentials == null)
                    {
                        ftpClient.Credentials = new NetworkCredential(ftpId, ftpPw);
                    }

                    ftpData = Encoding.UTF8.GetString(ftpClient.DownloadData(remotePath));

                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: GetFileData() Success :: FileContents = {1}", this.ToString(), ftpData));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: GetFileData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }

            return ftpData;
        }
        #endregion
    }
}
