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
    public class InterfaceSollae : InterfaceIOBase
    {
        #region Propery
        private ModbusTcpHandler sollaeHandle;  // ModbusTCP 핸들
        #endregion

        #region Constructor
        public InterfaceSollae(CommInfo commInfo)
        {
            try
            {
                TcpInfo tcpInfo = new TcpInfo(commInfo.machineIp, commInfo.machinePort);

                base.division = "IO";
                base.commInfo = commInfo;
                base.isConnect = false;
                //base.sendData = new Dictionary<string, object>();
                //base.receiveData = new Dictionary<string, object>();

                sollaeHandle = new ModbusTcpHandler(tcpInfo);
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method (Override)
        public override void Connect()
        {
            try
            {
                if (sollaeHandle.Connect(commInfo.machineIp, commInfo.machinePort))
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
                if (base.isConnect)
                {
                    sollaeHandle.DisConnect();

                    base.isConnect = false;
                }

                LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Success", this.ToString()));
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
                base.SIGNAL_1 = sollaeHandle.Read(0x02, 0, 1);
                base.SIGNAL_2 = sollaeHandle.Read(0x02, 1, 1);
                base.SIGNAL_3 = sollaeHandle.Read(0x02, 2, 1);
                base.SIGNAL_4 = sollaeHandle.Read(0x02, 3, 1);

                LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Success", this.ToString()));
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
