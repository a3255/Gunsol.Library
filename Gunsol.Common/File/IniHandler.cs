using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Gunsol.Common.File
{
    /// <summary>
    /// 
    /// </summary>
    public class IniHandler
    {
        #region Property
        [DllImport("kernel32.dll")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder val, int size, string filePath);
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        #endregion

        #region Method
        /// <summary>
        /// Get Value To Initialize File
        /// </summary>
        /// <param name="iniPath">Ini 파일 경로</param>
        /// <param name="section">Ini Section</param>
        /// <param name="key">Ini Key</param>
        /// <returns>Configuration Value</returns>
        public static string GetConfig(string iniPath, string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);

            GetPrivateProfileString(section, key, string.Empty, temp, 255, iniPath);

            return temp.ToString();
        }

        /// <summary>
        /// Set Value To Initialize File
        /// </summary>
        /// <param name="iniPath">Ini 파일 경로</param>
        /// <param name="section">Ini Section</param>
        /// <param name="key">Ini Key</param>
        /// <param name="value">Value</param>
        public static void SetConfig(string iniPath, string section, string key, string value)
        {
            if (!System.IO.File.Exists(iniPath))
            {
                System.IO.File.Create(iniPath);
            }

            WritePrivateProfileString(section, key, value, iniPath);
        }
        #endregion
    }
}
