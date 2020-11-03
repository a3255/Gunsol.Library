using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunsol.Common.Model.Class
{
    /// <summary>
    /// TCP 통신 정보 Class
    /// </summary>
    public class TcpInfo
    {
        #region Property
        /// <summary>
        /// IP 주소
        /// </summary>
        public string ipAddress { get; set; }

        /// <summary>
        /// Port 번호
        /// </summary>
        public int portNo { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 빈 값으로 Propery 초기화
        /// </summary>
        public TcpInfo()
        {
            this.ipAddress = string.Empty;
            this.portNo = 0;
        }

        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="ipAddress">IP 주소</param>
        /// <param name="portNo">Port 번호</param>
        public TcpInfo(string ipAddress, int portNo)
        {
            this.ipAddress = ipAddress;
            this.portNo = portNo;
        }
        #endregion
    }
}
