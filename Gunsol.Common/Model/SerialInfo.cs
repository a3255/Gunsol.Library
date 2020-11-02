using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Gunsol.Common.Model
{
    /// <summary>
    /// Serial 통신 정보 Class
    /// </summary>
    public class SerialInfo
    {
        #region Property
        public string portName { get; set; }        // ComPort 명
        public int baudRate { get; set; }           // 통신 속도 (BaudRate)
        public Parity parity { get; set; }          // Parity
        public int dataBits { get; set; }           // DataBits
        public StopBits stopBits { get; set; }      // StopBits
        #endregion

        #region Constructor
        /// <summary>
        /// 빈 값으로 Propery 초기화 (이후 Property 설정 필요)
        /// </summary>
        public SerialInfo()
        {
            this.portName = string.Empty;
            this.baudRate = 0;
            this.parity = Parity.None;
            this.dataBits = 0;
            this.stopBits = StopBits.None;
        }

        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="portName">ComPort 명</param>
        /// <param name="baudRate">통신 속도 (BaudRate)</param>
        /// <param name="parity">Parity</param>
        /// <param name="dataBits">DataBits</param>
        /// <param name="stopBits">StopBits</param>
        public SerialInfo(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            this.portName = portName;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
        }
        #endregion
    }
}
