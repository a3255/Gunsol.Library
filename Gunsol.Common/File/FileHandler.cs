using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gunsol.Common.File
{
    /// <summary>
    /// 로컬 파일 생성/삭제, 복사/검색, 읽기/쓰기 기능을 제공하는 Class
    /// </summary>
    public class FileHandler
    {
        #region Property
        /// <summary>
        /// 파일 경로
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// 파일명
        /// </summary>
        public string fileName
        {
            get
            {
                if (filePath.Equals(string.Empty))
                {
                    return string.Empty;
                }
                else if (filePath.IndexOf(@"\") >= 0)
                {
                    return filePath.Split('\\')[filePath.Split('\\').Length - 1];
                }
                else
                {
                    return filePath;
                }
            }
        }

        /// <summary>
        /// 폴더 경로
        /// </summary>
        public string directoryPath
        {
            get
            {
                if (filePath.Equals(string.Empty))
                {
                    return string.Empty;
                }
                else if (filePath.IndexOf(@"\") >= 0)
                {
                    return filePath.Replace(string.Format(@"\{0}", filePath.Split('\\')[filePath.Split('\\').Length - 1]), string.Empty);
                }
                else
                {
                    return System.Windows.Forms.Application.StartupPath;
                }
            }
        }

        /// <summary>
        /// 파일 유무
        /// </summary>
        public bool isExist
        {
            get
            {
                if (filePath.Equals(string.Empty))
                {
                    return false;
                }
                else
                {
                    return System.IO.File.Exists(filePath);
                }
            }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// 빈 값으로 Propery 초기화
        /// </summary>
        public FileHandler()
        {
            try
            {
                this.filePath = string.Empty;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        public FileHandler(string filePath)
        {
            try
            {
                this.filePath = filePath;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 로컬에 빈 파일 생성
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isClear"></param>
        public void FileCreate(string filePath = null, bool isClear = false)
        {
            try
            {
                if (filePath != null)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        if (isClear)
                        {
                            System.IO.File.Delete(filePath);
                            System.IO.File.Create(filePath);

                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), filePath));
                        }
                        else
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Fail :: File Already Exist", this.ToString(), filePath));
                        }
                    }
                    else
                    {
                        System.IO.File.Create(filePath);

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), filePath));
                    }
                }
                else
                {
                    if (isExist)
                    {
                        if (isClear)
                        {
                            System.IO.File.Delete(this.filePath);
                            System.IO.File.Create(this.filePath);

                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), this.filePath));
                        }
                        else
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Fail :: File Already Exist", this.ToString(), this.filePath));
                        }
                    }
                    else
                    {
                        System.IO.File.Create(filePath);

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), this.filePath));
                    }
                }                
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, this.filePath));
            }
        }

        /// <summary>
        /// 로컬 파일 읽기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="readLines">읽을 Line 수</param>
        /// <returns>파일 내용</returns>
        public string FileRead(string filePath = null, int readLines = 0)
        {
            StreamReader fileReader = null;
            FileStream fileStream = null;
            string fileContents = string.Empty;

            try
            {
                if (filePath == null)
                {
                    if (this.filePath.Equals(string.Empty))
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Fail :: File Path Not Initialize", this.ToString()));
                        fileContents = string.Empty;
                    }
                }
                else
                {
                    this.filePath = filePath;                    
                }

                if (isExist)
                {
                    fileStream = new FileStream(this.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fileReader = new StreamReader(fileStream);

                    if (readLines == 0)
                    {
                        fileContents = fileReader.ReadToEnd();
                        fileContents = fileContents.Replace("\r\n", "\n");
                    }
                    else
                    {
                        for (int i = 0; i < readLines; i++)
                        {
                            string currentLine = fileReader.ReadLine();

                            if (currentLine == null)
                            {
                                currentLine = currentLine.Replace("\r\n", "\n");

                                fileContents += string.Format("{0}\n", currentLine);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Success :: FileContents = {1}", this.ToString(), fileContents));
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Fail :: File Not Exist (Path = {1})", this.ToString(), this.filePath));
                    fileContents = string.Empty;
                }
            }
            catch(Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Exception :: Message = {1}", this.ToString(), ex.Message));
                fileContents = string.Empty;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                if (fileReader != null)
                {
                    fileReader.Close();
                }
            }

            return fileContents;
        }

        /// <summary>
        /// 로컬 파일 검색
        /// </summary>
        /// <param name="searchPattern">검색 패턴</param>
        public void FileSearch(string searchPattern)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileSearch() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// 로컬 파일 복사
        /// </summary>
        public void FileCopy()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCopy() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion
    }
}
