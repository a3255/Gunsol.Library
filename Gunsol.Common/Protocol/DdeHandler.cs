using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using NDde.Client;
using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Model;

namespace Gunsol.Common.Protocol
{
    /// <summary>
    /// DDE 통신 기능을 제공하는 Class
    /// </summary>
    public class DdeHandler
    {
        #region Property
        /// <summary>
        /// DDE Client 객체
        /// </summary>
        private DdeClient ddeHandle;

        /// <summary>
        /// DDE 통신 정보 객체
        /// </summary>
        public DdeInfo ddeInfo { get; set; }

        /// <summary>
        /// 연결 여부
        /// </summary>
        public bool isConnect
        {
            get
            {
                if (ddeHandle == null)
                {
                    return false;
                }
                else
                {
                    return ddeHandle.IsConnected;
                }
            }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="ddeInfo">DDE 통신 정보 객체</param>
        public DdeHandler(DdeInfo ddeInfo)
        {
            try
            {
                this.ddeHandle = new DdeClient(ddeInfo.ddeService, ddeInfo.ddeTopic);
                this.ddeInfo = ddeInfo;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// DDE Server에 접속
        /// </summary>
        /// <param name="ddeInfo">DDE 통신 정보 객체 (생략할 경우, 생성자 호출시 사용한 Paramter를 이용)</param>
        public void Connect(DdeInfo ddeInfo = null)
        {
            try
            {
                if (ddeInfo == null && this.ddeInfo == null)
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: TCP Info Not Initialized", this.ToString()));
                }
                else
                {
                    if (ddeHandle == null)
                    {
                        if (ddeInfo == null)
                        {
                            ddeHandle = new DdeClient(this.ddeInfo.ddeService, this.ddeInfo.ddeTopic);
                        }
                        else
                        {
                            ddeHandle = new DdeClient(ddeInfo.ddeService, ddeInfo.ddeTopic);
                            this.ddeInfo = ddeInfo;
                        }
                    }

                    ddeHandle.Connect();

                    if (ddeHandle.IsConnected)
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success", this.ToString()));
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail", this.ToString()));
                    }
                }                
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// DDE Server 접속 해제
        /// </summary>
        public void DisConnect()
        {
            try
            {
                if (ddeHandle != null)
                {
                    if (ddeHandle.IsConnected)
                    {
                        ddeHandle.Disconnect();

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));

                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: Not Conneted", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: DDE Client Not Initialized", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// DDE Server에 Request
        /// </summary>
        /// <param name="ddeItem">Request Item</param>
        /// <param name="itemValue">Item 값</param>
        /// <returns>성공 여부</returns>
        public bool Request(string ddeItem, ref string itemValue)
        {
            bool isSuccess = false;

            try
            {
                itemValue = ddeHandle.Request(ddeItem, 1);

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Request(Item = {1}) Success :: Value = {2}", this.ToString(), ddeItem, itemValue));

                isSuccess = true;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Request(Item = {1}) Exception :: Message = {2}", this.ToString(), ddeItem, ex.Message));
                
                itemValue = string.Empty;

                isSuccess = false;
            }

            return isSuccess;
        }
        #endregion
    }
}
