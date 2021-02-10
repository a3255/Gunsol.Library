using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

//using Gunsol.Common.File;
using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;
//using Gunsol.Common.Protocol;

namespace Gunsol.Common.File
{
    /// <summary>
    /// FTP 다운로드/업로드/조회 기능을 제공하는 Class
    /// </summary>
    public class FtpHandler
    {
        #region Property
        /// <summary>
        /// FTP 계정 정보
        /// </summary>
        public NetworkCredential ftpUserInfo { get; set; }

        /// <summary>
        /// FTP 접속 ID
        /// </summary>
        public string ftpUserId
        {
            get
            {
                if (ftpUserInfo != null)
                {
                    return ftpUserInfo.UserName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// FTP 접속 PW
        /// </summary>
        public string ftpUserPw
        {
            get
            {
                if (ftpUserInfo != null)
                {
                    return ftpUserInfo.Password;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="ftpUserInfo">FTP 계정 정보 (생략할 경우 null 할당)</param>
        public FtpHandler(NetworkCredential ftpUserInfo = null)
        {
            if (ftpUserInfo == null)
            {
                this.ftpUserInfo = new NetworkCredential();
            }
            else
            {
                this.ftpUserInfo = ftpUserInfo;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 지정 FTP 경로의 파일 다운로드
        /// </summary>
        /// <param name="remotePath">원격 파일 경로</param>
        /// <param name="localPath">로컬 다운로드 경로</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Download(string remotePath, string localPath)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();
            Exception funcException = null;
            bool isSuccess = false;

            WebClient ftpClient = new WebClient();
            FileStream fileStream = null;

            stopWatch.Start();

            try
            {
                if (this.ftpUserId.Equals(string.Empty) || this.ftpUserPw.Equals(string.Empty))
                {
                    isSuccess = false;
                }
                else
                {
                    ftpClient.Credentials = ftpUserInfo;

                    byte[] ftpData = ftpClient.DownloadData(remotePath);

                    fileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fileStream.Write(ftpData, 0, ftpData.Length);
                    fileStream.Flush();

                    isSuccess = true;
                }

                funcException = null;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                funcException = ex;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }

            stopWatch.Stop();

            result.isSuccess = isSuccess;
            result.funcException = funcException;
            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 FTP 경로에 파일 업로드
        /// </summary>
        /// <param name="remotePath">업로드 경로</param>
        /// <param name="localPath">로컬 파일 경로</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Upload(string remotePath, string localPath)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();
            Exception funcException = null;
            bool isSuccess = false;

            WebClient ftpClient = new WebClient();
            FileStream fileStream = null;

            //stopWatch.Start();

            try
            {
                if (this.ftpUserId.Equals(string.Empty) || this.ftpUserPw.Equals(string.Empty))
                {
                    isSuccess = false;
                }
                else
                {
                    ftpClient.Credentials = ftpUserInfo;

                    FileInfo fileInfo = new FileInfo(localPath);
                    byte[] localFileData = new byte[fileInfo.Length];

                    fileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fileStream.Read(localFileData, 0, localFileData.Length);

                    isSuccess = true;
                }

                funcException = null;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                funcException = ex;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }

            //stopWatch.Stop();

            result.isSuccess = isSuccess;
            result.funcException = funcException;
            //result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            //stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 FTP 경로의 파일 읽기
        /// </summary>
        /// <param name="remotePath">원격 파일 경로</param>
        /// <returns>함수 실행 결과 (FileResult 객체)</returns>
        public CommonStruct.FileResult GetFileData(string remotePath)
        {
            CommonStruct.FileResult result = new CommonStruct.FileResult();
            Exception funcException = null;
            string resultString = string.Empty;
            bool isSuccess = false;

            WebClient ftpClient = new WebClient();

            stopWatch.Start();

            try
            {
                if (ftpUserId.Equals(string.Empty) || ftpUserPw.Equals(string.Empty))
                {
                    resultString = string.Empty;
                    isSuccess = false;                    
                }
                else
                {
                    if (ftpClient.Credentials == null)
                    {
                        ftpClient.Credentials = ftpUserInfo;
                    }

                    resultString = Encoding.UTF8.GetString(ftpClient.DownloadData(remotePath));
                    isSuccess = true;
                }

                funcException = null;
            }
            catch (Exception ex)
            {
                resultString = string.Empty;
                isSuccess = false;
                funcException = ex;
            }

            stopWatch.Stop();

            result.funcResult.isSuccess = isSuccess;
            result.funcResult.funcException = funcException;
            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;
            result.resultFiles = null;
            result.resultString = resultString;

            stopWatch.Reset();

            return result;
        }
        #endregion
    }
}
