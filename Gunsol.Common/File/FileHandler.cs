using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

//using Gunsol.Common.File;
using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;
//using Gunsol.Common.Protocol;

namespace Gunsol.Common.File
{
    /// <summary>
    /// 파일 생성/삭제, 복사/검색, 읽기/쓰기 기능을 제공하는 Class
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

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="filePath">파일 경로 (생략할 경우 빈 문자열 할당)</param>
        public FileHandler(string filePath = null)
        {
            if (filePath == null)
            {
                this.filePath = string.Empty;
            }
            else
            {
                this.filePath = filePath;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 지정 경로에 파일 생성
        /// </summary>
        /// <param name="filePath">생성 파일 경로 (생략할 경우 filePath Property 사용)</param>
        /// <param name="isOverwrite">덮어쓰기 여부 (생략할 경우 전체 검색)</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult FileCreate(string filePath = null, bool isOverwrite = false)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (filePath != null)
                {
                    this.filePath = filePath;
                }

                if (isExist)
                {
                    if (isOverwrite)
                    {
                        System.IO.File.Delete(this.filePath);
                        System.IO.File.Create(this.filePath);

                        //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success(Overwrite)", this.ToString(), this.filePath));
                    }
                    else
                    {
                        //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Fail :: File Already Exist", this.ToString(), this.filePath));
                    }
                }
                else
                {
                    System.IO.File.Create(filePath);

                    //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), this.filePath));
                }

                result.isSuccess = true;
                result.funcException = null;

                #region Comment
                //if (filePath != null)
                //{
                //    if (System.IO.File.Exists(filePath))
                //    {
                //        if (isOverwrite)
                //        {
                //            System.IO.File.Delete(filePath);
                //            System.IO.File.Create(filePath);

                //            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), filePath));
                //        }
                //        else
                //        {
                //            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Fail :: File Already Exist", this.ToString(), filePath));
                //        }
                //    }
                //    else
                //    {
                //        System.IO.File.Create(filePath);

                //        LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), filePath));
                //    }
                //}
                //else
                //{
                //    if (isExist)
                //    {
                //        if (isOverwrite)
                //        {
                //            System.IO.File.Delete(this.filePath);
                //            System.IO.File.Create(this.filePath);

                //            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), this.filePath));
                //        }
                //        else
                //        {
                //            LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Fail :: File Already Exist", this.ToString(), this.filePath));
                //        }
                //    }
                //    else
                //    {
                //        System.IO.File.Create(filePath);

                //        LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {1}) Success", this.ToString(), this.filePath));
                //    }
                //}    
                #endregion
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileCreate(Path = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, this.filePath));

                result.isSuccess = false;
                result.funcException = ex;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 경로의 파일 삭제
        /// </summary>
        /// <param name="filePath">삭제 파일 경로 (생략할 경우 filePath Property 사용)</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult FileDelete(string filePath = null)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (filePath != null)
                {
                    this.filePath = filePath;
                }

                if (isExist)
                {
                    System.IO.File.Delete(this.filePath);

                    result.isSuccess = true;
                }
                else
                {
                    result.isSuccess = false;
                }

                result.funcException = null;
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.funcException = ex;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 경로의 파일 쓰기
        /// </summary>
        /// <param name="fileContents">파일 내용</param>
        /// <param name="filePath">읽을 파일 경로 (생략할 경우 filePath Property 사용)</param>
        /// <param name="isContinue">이어 쓰기 여부 (생략할 경우 기존 파일 내용에 이어 쓰기)</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult FileWrite(string fileContents, string filePath = null, bool isContinue = true)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();
            StreamWriter fileWriter = null;
            FileStream fileStream = null;
            Exception funcException = null;
            bool isSuccess = false;

            stopWatch.Start();

            try
            {
                if (filePath == null)
                {
                    if (this.filePath.Equals(string.Empty))
                    {
                        isSuccess = false;
                    }
                }
                else
                {
                    this.filePath = filePath;
                }

                if (isExist)
                {
                    fileStream = new FileStream(this.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fileWriter = new StreamWriter(fileStream);

                    if (!isContinue)
                    {
                        System.IO.File.WriteAllText(this.filePath, string.Empty);
                    }

                    fileWriter.BaseStream.Seek(0, SeekOrigin.End);
                    fileWriter.Write(fileContents);
                    fileWriter.Flush();

                    isSuccess = true;
                }
                else
                {
                    isSuccess = false;
                }

                result.funcException = null;
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

                if (fileWriter != null)
                {
                    fileWriter.Close();
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
        /// 지정 경로의 파일 읽기
        /// </summary>
        /// <param name="filePath">읽을 파일 경로 (생략할 경우 filePath Property 사용)</param>
        /// <param name="readLines">읽을 Line 수 (생략할 경우 전체 내용 읽기)</param>
        /// <returns>함수 실행 결과 (FileResult 객체)</returns>
        public CommonStruct.FileResult FileRead(string filePath = null, ushort readLines = 0)
        {
            CommonStruct.FileResult result = new CommonStruct.FileResult();
            StreamReader fileReader = null;
            FileStream fileStream = null;
            string fileContents = string.Empty;

            stopWatch.Start();

            try
            {
                if (filePath == null)
                {
                    if (this.filePath.Equals(string.Empty))
                    {
                        //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Fail :: File Path Not Initialize", this.ToString()));
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

                    //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Success :: FileContents = {1}", this.ToString(), fileContents));

                    result.funcResult.isSuccess = true;
                }
                else
                {
                    //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Fail :: File Not Exist (Path = {1})", this.ToString(), this.filePath));
                    fileContents = string.Empty;

                    result.funcResult.isSuccess = false;
                }

                result.funcResult.funcException = null;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: FileRead() Exception :: Message = {1}", this.ToString(), ex.Message));
                fileContents = string.Empty;

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
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

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;
            result.resultFiles = null;
            result.resultString = fileContents;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 경로의 파일 검색
        /// </summary>
        /// <param name="directoryPath">검색 대상 폴더 경로 (생략할 경우 directoryPath Property 사용)</param>
        /// <param name="searchPattern">검색 패턴 (생략할 경우 전체 검색)</param>
        /// <returns>함수 실행 결과 (FileResult 객체)</returns>
        public CommonStruct.FileResult FileSearch(string directoryPath = null, string searchPattern = "*.*")
        {
            CommonStruct.FileResult result = new CommonStruct.FileResult();
            bool isSuccess = false;
            Exception funcException = null;
            FileInfo[] resultFiles = null;
            string resultString = null;
            string pDirectoryPath = string.Empty;

            stopWatch.Start();

            try
            {
                if (directoryPath == null)
                {
                    pDirectoryPath = this.directoryPath;
                }
                else
                {
                    pDirectoryPath = directoryPath;
                }

                if (Directory.Exists(pDirectoryPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(pDirectoryPath);

                    isSuccess = true;
                    resultFiles = directoryInfo.GetFiles(searchPattern);                    
                }
                else
                {
                    isSuccess = false;
                    resultFiles = null;
                }

                funcException = null;
                resultString = null;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                funcException = ex;
                resultString = null;
            }

            stopWatch.Stop();

            result.funcResult.isSuccess = isSuccess;
            result.funcResult.funcException = funcException;
            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;
            result.resultFiles = resultFiles;
            result.resultString = resultString;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 경로의 파일 복사
        /// </summary>
        /// <param name="destinationPath">복사 파일 경로 (생략할 경우 filePath Property 사용)</param>
        /// <param name="filePath">원본 파일 경로</param>
        /// <param name="isOverwrite">덮어쓰기 여부 (생략할 경우 전체 검색)</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult FileCopy(string destinationPath, string filePath = null, bool isOverwrite = false)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();
            bool isSuccess = false;
            Exception funcException = null;

            stopWatch.Start();

            try
            {
                if (filePath != null)
                {
                    this.filePath = filePath;
                }

                if (isExist)
                {
                    System.IO.File.Copy(this.filePath, destinationPath, isOverwrite);

                    isSuccess = true;
                }
                else
                {
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                funcException = ex;
            }


            stopWatch.Stop();

            result.isSuccess = isSuccess;
            result.funcException = funcException;
            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }
        #endregion
    }
}
