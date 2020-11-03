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
        /// Query 실행 결과
        /// </summary>
        public struct CallResult
        {
            /// <summary>
            /// 성공 여부
            /// </summary>
            public bool isSuccess;

            /// <summary>
            /// 결과 테이블 (결과가 없을 경우 null)
            /// </summary>
            public DataTable resultTable;

            /// <summary>
            /// 결과 영향 받은 RowCount (Select일 경우 Select한 RowCount)
            /// </summary>
            public int resultRowCount;

            /// <summary>
            /// 실행 시간(ms)
            /// </summary>
            public double resultTime;
        }
    }
}
