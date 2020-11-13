using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Gunsol.Common.Model.Struct
{
    /// <summary>
    /// 공통 구조체 정의 Class
    /// </summary>
    public class CommonStruct
    {
        /// <summary>
        /// 함수 실행 결과
        /// </summary>
        public struct FuncResult
        {
            /// <summary>
            /// 성공 여부
            /// </summary>
            public bool isSuccess;

            /// <summary>
            /// 실행 시간(ms)
            /// </summary>
            public long totalMilliseconds;

            /// <summary>
            /// 예외 객체
            /// </summary>
            public Exception funcException;
        }

        /// <summary>
        /// DBMS 함수 실행 결과
        /// </summary>
        public struct DbmsResult
        {
            /// <summary>
            /// 함수 실행 결과
            /// </summary>
            public FuncResult funcResult;

            /// <summary>
            /// 결과 테이블 (결과가 없을 경우 null)
            /// </summary>
            public DataTable resultTable;

            /// <summary>
            /// 결과 영향 받은 RowCount (Select일 경우 Select한 RowCount)
            /// </summary>
            //public int resultRowCount;
        }

        /// <summary>
        /// File 함수 실행 결과
        /// </summary>
        public struct FileResult
        {
            /// <summary>
            /// 함수 실행 결과
            /// </summary>
            public FuncResult funcResult;

            /// <summary>
            /// 파일 검색 결과 (결과가 없을 경우 null)
            /// </summary>
            public System.IO.FileInfo[] resultFiles;

            /// <summary>
            /// 파일 내용 (결과가 없을 경우 null)
            /// </summary>
            public string resultString;
        }
    }
}
