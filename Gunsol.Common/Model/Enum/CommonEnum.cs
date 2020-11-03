using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunsol.Common.Model.Enum
{
    /// <summary>
    /// 공통 열거형 정의 Class
    /// </summary>
    public class CommonEnum
    {
        /// <summary>
        /// Procedure/Query 의 실행 타입
        /// </summary>
        public enum ExecuteType
        {
            /// <summary>
            /// SELECT Procedure/Query
            /// </summary>
            SELECT = 0,

            /// <summary>
            /// No Select Procedure/Query
            /// </summary>
            NOSELECT = 1
        }
    }
}
