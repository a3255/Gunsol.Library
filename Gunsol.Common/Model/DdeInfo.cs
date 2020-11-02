using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunsol.Common.Model
{
    /// <summary>
    /// DDE 통신 정보 Class
    /// </summary>
    public class DdeInfo
    {
        #region Property
        /// <summary>
        /// DDE Service
        /// </summary>
        public string ddeService { get; set; }

        /// <summary>
        /// DDE Topic
        /// </summary>
        public string ddeTopic { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 빈 값으로 Propery 초기화 (이후 Property 설정 필요)
        /// </summary>
        public DdeInfo()
        {
            this.ddeService = string.Empty;
            this.ddeTopic = string.Empty;
        }

        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="ddeService">DDE Service</param>
        /// <param name="ddeTopic">DDE Topic</param>
        public DdeInfo(string ddeService, string ddeTopic)
        {
            this.ddeService = ddeService;
            this.ddeTopic = ddeTopic;
        }
        #endregion
    }
}
