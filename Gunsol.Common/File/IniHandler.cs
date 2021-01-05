using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;

namespace Gunsol.Common.File
{
    /// <summary>
    /// Initialize 파일 읽기/쓰기 기능을 제공하는 Class
    /// </summary>
    public class IniHandler
    {
        #region Property
        /// <summary>
        /// Initialize 파일의 설정 값 읽기 (외부 DLL 함수 Import)
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder val, int size, string filePath);

        /// <summary>
        /// Initialize 파일의 설정 값 쓰기 (외부 DLL 함수 Import)
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private static Stopwatch stopWatch = new Stopwatch();
        #endregion

        #region Method
        /// <summary>
        /// Initialize 파일의 설정 값 읽기
        /// </summary>
        /// <param name="iniPath">Initialize 파일 경로</param>
        /// <param name="section">Initialize Section</param>
        /// <param name="key">Initialize Key</param>
        /// <returns>함수 실행 결과 (FileResult 객체)</returns>
        public static CommonStruct.FileResult GetConfig(string iniPath, string section, string key)
        {
            CommonStruct.FileResult result = new CommonStruct.FileResult();
            Exception funcException = null;
            string resultString = string.Empty;
            bool isSuccess = false;

            StringBuilder temp = new StringBuilder(255);

            stopWatch.Start();

            try
            {
                GetPrivateProfileString(section, key, string.Empty, temp, 255, iniPath);

                resultString = temp.ToString();
                isSuccess = true;
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

        /// <summary>
        /// Initialize 파일의 설정 값 쓰기
        /// </summary>
        /// <param name="iniPath">Initialize 파일 경로</param>
        /// <param name="section">Initialize Section</param>
        /// <param name="key">Initialize Key</param>
        /// <param name="value">설정 값</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public static CommonStruct.FuncResult SetConfig(string iniPath, string section, string key, string value)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();
            Exception funcException = null;
            string resultString = string.Empty;
            bool isSuccess = false;

            stopWatch.Start();

            try
            {
                if (!System.IO.File.Exists(iniPath))
                {
                    System.IO.File.Create(iniPath);
                }

                WritePrivateProfileString(section, key, value, iniPath);

                resultString = string.Empty;
                isSuccess = true;
                funcException = null;
            }
            catch (Exception ex)
            {
                resultString = string.Empty;
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
