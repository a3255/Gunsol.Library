using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gunsol.Common;
using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Protocol;
using Gunsol.Interface.Model;

namespace Gunsol.Interface.Interface.ROBOT
{
    public class InterfaceDoosan : InterfaceROBOTBase
    {
        #region Propery
        private ModbusTcpHandler doosanHandle;  // ModbusTCP 핸들
        #endregion

        #region Constructor
        public InterfaceDoosan(CommInfo commInfo)
        {
            base.division = "IO";
            base.commInfo = commInfo;
            base.isConnect = false;
            base.SEND_DATA = new Dictionary<object, object>();
            //base.sendData = new Dictionary<string, object>();
            //base.receiveData = new Dictionary<string, object>();

            doosanHandle = new ModbusTcpHandler();
        }
        #endregion

        #region Method (Override)
        /// <summary>
        /// Connect To Machine Using Library Method
        /// </summary>
        public override void Connect()
        {
            try
            {
                if (!base.isConnect)
                {
                    if (doosanHandle.Connect(commInfo.machineIp, commInfo.machinePort))
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
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));

                base.isConnect = false;
            }
        }

        /// <summary>
        /// DisConnect Machine Using Library Method
        /// </summary>
        public override void DisConnect()
        {
            try
            {
                if (base.isConnect)
                {
                    doosanHandle.DisConnect();

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

        /// <summary>
        /// Receive Machine Data Using Library Method
        /// </summary>
        public override void Receive()
        {
            try
            {
                base.RBT_PWR = doosanHandle.Read(0x03, 264, 1);

                LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Success", this.ToString()));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Send Data To Machine Using Library Method
        /// </summary>
        public override void Send()
        {
            try
            {
                base.Send();
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Send() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Parsing Machine Data To Base Property
        /// </summary>
        public override void Parsing()
        {
            try
            {
                base.Parsing();
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Parsing() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion
    }
}
