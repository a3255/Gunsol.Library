using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Model;

namespace Gunsol.Common.Protocol
{
    /// <summary>
    /// Socket 통신 기능을 제공하는 Class
    /// </summary>
    public class TcpHandler
    {
        #region Property
        /// <summary>
        /// TCP Client 객체
        /// </summary>
        private TcpClient tcpHandle; 

        /// <summary>
        /// TCP 통신 정보 객체
        /// </summary>
        public TcpInfo tcpInfo { get; set; }

        /// <summary>
        /// 연결 여부
        /// </summary>
        public bool isConnect
        {
            get
            {
                if (tcpHandle == null)
                {
                    return false;
                }
                else
                {
                    return tcpHandle.Connected;
                }
            }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="tcpInfo">TCP 통신 정보 객체 (생략할 경우, 이후 초기화 필요)</param>
        public TcpHandler(TcpInfo tcpInfo = null)
        {
            try
            {
                this.tcpHandle = new TcpClient();

                if (tcpInfo != null)
                {
                    this.tcpInfo = tcpInfo;
                }
                else
                {
                    this.tcpInfo = new TcpInfo();
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// TCP Server/Client에 접속
        /// </summary>
        /// <param name="tcpInfo">TCP 통신 정보 객체</param>
        public void Connect(TcpInfo tcpInfo = null)
        {
            try
            {
                if (tcpInfo == null && this.tcpInfo == null)
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: TCP Info Not Initialized", this.ToString()));
                }
                else
                {
                    if (tcpHandle == null)
                    {
                        tcpHandle = new TcpClient();
                    }

                    if (tcpInfo != null)
                    {
                        tcpHandle.Connect(tcpInfo.ipAddress, tcpInfo.portNo);

                        this.tcpInfo = tcpInfo;
                    }
                    else
                    {
                        tcpHandle.Connect(this.tcpInfo.ipAddress, this.tcpInfo.portNo);
                    }

                    if (tcpHandle.Connected)
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success", this.ToString()));
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail", this.ToString()));
                    }
                }
            }
            catch(Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// TCP Server/Client 접속 해제
        /// </summary>
        public void DisConnect()
        {
            try
            {
                if (tcpHandle != null)
                {
                    if (tcpHandle.Connected)
                    {
                        tcpHandle.Close();

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: Not Conneted", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: TCP Not Initialized", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// TCP Server/Client에 데이터 송신
        /// </summary>
        /// <param name="sendBytes">송신 데이터</param>
        /// <returns>성공 여부</returns>
        public bool Send(byte[] sendBytes)
        {
            NetworkStream tcpStream = null;
            bool isSuccess = false;

            try
            {
                if (tcpHandle.Connected)
                {
                    tcpStream = tcpHandle.GetStream();

                    if (tcpStream.CanWrite)
                    {
                        tcpStream.Write(sendBytes, 0, sendBytes.Length);
                        tcpStream.Flush();

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Send() Success", this.ToString()));

                        isSuccess = true;
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Send() Fail :: Stream Can't Write", this.ToString()));

                        isSuccess = false;
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Send() Fail :: Not Connected", this.ToString()));

                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Send() Exception :: Message = {1}", this.ToString(), ex.Message));

                isSuccess = false;
            }
            finally
            {
                if (tcpStream != null)
                {
                    tcpStream.Close();
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// TCP Server/Client으로부터 데이터 수신
        /// </summary>
        /// <param name="receiveBytes">수신 데이터</param>
        /// <returns>성공 여부</returns>
        public bool Receive(List<byte> receiveBytes)
        {
            NetworkStream tcpStream = null;
            bool isSuccess = false;

            try
            {
                if (tcpHandle.Connected)
                {
                    byte[] tempReceiveByte = new byte[1];

                    tcpStream = tcpHandle.GetStream();

                    if (tcpStream.CanRead)
                    {
                        do
                        {
                            tcpStream.Read(tempReceiveByte, 0, tempReceiveByte.Length);
                            receiveBytes.AddRange(tempReceiveByte);
                        } while (tcpStream.DataAvailable);

                        if (receiveBytes.Count > 0)
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Success :: Count = {1}", this.ToString(), receiveBytes.Count));

                            isSuccess = true;
                        }
                        else
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Fail :: No Data Received", this.ToString()));

                            isSuccess = false;
                        }
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Fail :: Stream Can't Read", this.ToString()));

                        isSuccess = false;
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Fail :: Not Connected", this.ToString()));

                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Exception :: Message = {1}", this.ToString(), ex.Message));

                isSuccess = false;
            }
            finally
            {
                if (tcpStream != null)
                {
                    tcpStream.Close();
                }
            }

            return isSuccess;
        }
        #endregion
    }
}
