using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;

namespace Gunsol.Common.File
{
    /// <summary>
    /// 로그 출력/저장/삭제 기능을 제공하는 Class
    /// </summary>
    public class LogHandler
    {
        #region Method
        /// <summary>
        /// 로그 내용을 콘솔 창에 출력
        /// </summary>
        /// <param name="log">로그 내용</param>
        public static void PrintLog(string log)
        {
            Console.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"), log));
        }

        /// <summary>
        /// 로그 내용을 텍스트 파일에 기록
        /// </summary>
        /// <param name="logDirectoryPath">로그 파일 저장 폴더 경로</param>
        /// <param name="log">로그 내용</param>
        public static void WriteLog(string logDirectoryPath, string log)
        {
            FileStream fileStream = null;
            StreamWriter fileWriter = null;
            string logFileName = string.Format("{0:0000}-{1:00}-{2:00}.log", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            string logFilePath = string.Format(@"{0}\{1}", logDirectoryPath, logFileName);
            string logLine = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"), log);

            try
            {
                if (!Directory.Exists(logDirectoryPath))
                {
                    Directory.CreateDirectory(logDirectoryPath);
                }

                fileStream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fileWriter = new StreamWriter(fileStream);

                fileWriter.BaseStream.Seek(0, SeekOrigin.End);
                fileWriter.Write(logLine);

                fileWriter.Flush();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (fileWriter != null)
                {
                    fileWriter.Close();
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        /// <summary>
        /// 저장 일자 기준으로 저장된 로그 파일을 삭제
        /// </summary>
        /// <param name="logDirectoryPath">로그 파일 저장 폴더 경로</param>
        /// <param name="logSaveDay">로그 파일 저장 일</param>
        public static void DeleteLog(string logDirectoryPath, int logSaveDay)
        {
            try
            {
                if (Directory.Exists(logDirectoryPath))
                {
                    CommonStruct.FileResult result = new CommonStruct.FileResult();
                    DateTime deleteDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(logSaveDay * (-1));

                    FileHandler fileHandle = new FileHandler();
                    result = fileHandle.SearchFile(logDirectoryPath, "*.txt");

                    FileInfo[] deleteFiles = result.resultFiles.Where(p => p.CreationTime < deleteDate).ToArray();

                    foreach(FileInfo f in deleteFiles)
                    {
                        f.Delete();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
