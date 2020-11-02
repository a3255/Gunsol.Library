using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Model;
using Gunsol.Common.Protocol;
using Gunsol.Interface.Model;

namespace Gunsol.Interface.Interface.IO
{
    public class InterfaceArduino : InterfaceIOBase
    {
        #region Propery
        private byte[] SEND_PACKET = Encoding.UTF8.GetBytes("GET"); // Send Packet
        private SerialHandler arduinoHandle;                        // Serial 핸들
        #endregion

        #region Constructor
        public InterfaceArduino(CommInfo commInfo)
        {
            try
            {
                base.division = "CNC";
                base.commInfo = commInfo;
                base.isConnect = false;
                //base.sendData = new Dictionary<string, object>();
                //base.receiveData = new Dictionary<string, object>();

                SerialInfo serialInfo = new SerialInfo();
                serialInfo.portName = commInfo.machineSerialPort;
                serialInfo.baudRate = 57400;
                serialInfo.parity = System.IO.Ports.Parity.None;
                serialInfo.dataBits = 8;
                serialInfo.stopBits = System.IO.Ports.StopBits.One;

                arduinoHandle = new SerialHandler(serialInfo);
            }
            catch (Exception ex)
            {
                LogHandler.PrintLog(string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method (Override)
        public override void Connect()
        {
            try
            {
                if (arduinoHandle.Connect())
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Success", this.ToString()));

                    base.isConnect = true;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Fail", this.ToString()));

                    base.isConnect = false;
                }
            }
            catch(Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));

                base.isConnect = false;
            }
        }

        public override void DisConnect()
        {
            try
            {
                arduinoHandle.DisConnect();

                LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Success", this.ToString()));

                base.isConnect = false;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));

                base.isConnect = true;
            }
        }

        public override void Receive()
        {
            try
            {
                if (arduinoHandle.Send(SEND_PACKET))
                {
                    List<byte> receivePacket = new List<byte>();

                    if (arduinoHandle.Receive(receivePacket))
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Success", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.PrintLog(string.Format("{0} :: Receive() Fail :: Send Fail", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        public override void Send()
        {
            base.Send();
        }
        #endregion
    }
}
