using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Gunsol.Common.Model.Class
{
    /// <summary>
    /// Serial 통신 정보 Class
    /// </summary>
    public class SerialInfo
    {
        #region Property
        /// <summary>
        /// COM Port
        /// </summary>
        public string portName { get; set; }

        /// <summary>
        /// 통신 속도
        /// </summary>
        public int baudRate { get; set; }

        /// <summary>
        /// Parity
        /// </summary>
        public Parity parity { get; set; }

        /// <summary>
        /// Data Bits
        /// </summary>
        public int dataBits { get; set; }

        /// <summary>
        /// Stop Bits
        /// </summary>
        public StopBits stopBits { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 빈 값으로 Propery 초기화
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
        /// <param name="portName">COM Port</param>
        /// <param name="baudRate">통신 속도</param>
        /// <param name="parity">Parity</param>
        /// <param name="dataBits">Data Bits</param>
        /// <param name="stopBits">Stop Bits</param>
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
