using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gunsol.Common.File
{
    public class LogHandler
    {
        #region Method (Static)
        /// <summary>
        /// Print Log Message To Console
        /// </summary>
        /// <param name="log">Log Message</param>
        public static void PrintLog(string log)
        {
            Console.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"), log));
        }

        /// <summary>
        /// Write Log Message To Text File (Also Print Console)
        /// </summary>
        /// <param name="machineType">Machine Type (To Divide Log File)</param>
        /// <param name="log">Log Message</param>
        public static void WriteLog(string machineType, string log)
        {
            FileStream fileStream = null;
            StreamWriter fileWriter = null;
            string iniPath = string.Format(@"{0}\Config.ini", Application.StartupPath);
            string logDate = string.Format("{0:0000}-{1:00}-{2:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            string logDirectory = string.Format(@"{0}\Log", Application.StartupPath);
            string logPath = string.Format(@"{0}\{1}_{2}.log", logDirectory, logDate, machineType);
            string logLine = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"), log);

            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                fileStream = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fileWriter = new StreamWriter(fileStream);

                fileWriter.BaseStream.Seek(0, SeekOrigin.End);
                fileWriter.Write(logLine);

                fileWriter.Flush();

                Console.Write(logLine);
            }
            catch (Exception e)
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

            DeleteLog();
        }

        /// <summary>
        /// Delete Log File After 7 Days
        /// </summary>
        public static void DeleteLog()
        {
            string iniPath = string.Format(@"{0}\Config.ini", Application.StartupPath);
            string logDirectory = string.Format(@"{0}\Log", Application.StartupPath);
            string logSaveDay = "7";
            
            try
            {
                if (Directory.Exists(logDirectory))
                {
                    DirectoryInfo logDirectoryInfo = new DirectoryInfo(logDirectory);
                    DateTime today = DateTime.Today;
                    DateTime deleteDay = today.AddDays(Convert.ToInt32(logSaveDay) * (-1));

                    if (logDirectoryInfo.GetFiles().ToList().Exists(p => p.CreationTime <= deleteDay))
                    {
                        FileInfo[] deleteFiles = logDirectoryInfo.GetFiles().Where(p => p.CreationTime <= deleteDay).ToArray();

                        foreach (FileInfo f in deleteFiles)
                        {
                            f.Delete();
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        #endregion
    }
}
