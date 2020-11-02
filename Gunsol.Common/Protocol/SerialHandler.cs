using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text;

using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Model;

namespace Gunsol.Common.Protocol
{
    /// <summary>
    /// Serial 통신 기능을 제공하는 Class
    /// </summary>
    public class SerialHandler
    {
        #region Property
        /// <summary>
        /// Serial Client 객체
        /// </summary>
        private SerialPort serialHandle;

        /// <summary>
        /// Serial 통신 정보 객체
        /// </summary>
        public SerialInfo serialInfo { get; set; }

        /// <summary>
        /// 연결 여부
        /// </summary>
        public bool isConnect
        {
            get
            {
                if (serialHandle == null)
                {
                    return false;
                }
                else
                {
                    return serialHandle.IsOpen;
                }
            }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="serialInfo">Serial 통신 정보 객체 (생략할 경우, 이후 초기화 필요)</param>
        public SerialHandler(SerialInfo serialInfo = null)
        {
            try
            {
                this.serialHandle = new SerialPort();

                if (serialInfo != null)
                {
                    this.serialInfo = serialInfo;
                    this.serialHandle.PortName = serialInfo.portName;
                    this.serialHandle.BaudRate = serialInfo.baudRate;
                    this.serialHandle.Parity = serialInfo.parity;
                    this.serialHandle.DataBits = serialInfo.dataBits;
                    this.serialHandle.StopBits = serialInfo.stopBits;
                }
                else
                {
                    this.serialInfo = new SerialInfo();
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
        /// Serial Port에 접속
        /// </summary>
        /// <param name="serialInfo">Serial 통신 정보 객체</param>
        public void Connect(SerialInfo serialInfo = null)
        {
            try
            {
                if (serialInfo == null && this.serialInfo == null)
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: Serial Info Not Initialized", this.ToString()));
                }
                else
                {
                    if (serialHandle == null)
                    {
                        serialHandle = new SerialPort();
                    }

                    if (serialInfo != null)
                    {
                        serialHandle.PortName = serialInfo.portName;
                        serialHandle.BaudRate = serialInfo.baudRate;
                        serialHandle.Parity = serialInfo.parity;
                        serialHandle.DataBits = serialInfo.dataBits;
                        serialHandle.StopBits = serialInfo.stopBits;

                        serialHandle.Open();

                        this.serialInfo = serialInfo;
                    }
                    else
                    {
                        serialHandle.Open();
                    }
                }

                if (serialHandle.IsOpen)
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Serial Port 접속 해제
        /// </summary>
        public void DisConnect()
        {
            try
            {
                if (serialHandle != null)
                {
                    if (serialHandle.IsOpen)
                    {
                        serialHandle.Close();

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: Not Conneted", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: Serial Handle Not Initialized", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Serial Port에 데이터 송신
        /// </summary>
        /// <param name="sendBytes">송신 데이터</param>
        /// <returns>성공 여부</returns>
        public bool Send(byte[] sendBytes)
        {
            bool isSuccess = false;

            try
            {
                if (serialHandle.IsOpen)
                {
                    int sendPacketLength = 0;

                    serialHandle.Write(sendBytes, 0, sendBytes.Length);

                    System.Threading.Thread.Sleep(100);

                    sendPacketLength = serialHandle.BytesToWrite;

                    if (sendPacketLength > 0)
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Send() Success", this.ToString()));

                        isSuccess = true;
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Send() Fail :: No Data Send", this.ToString()));

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

            return isSuccess;
        }

        /// <summary>
        /// Serial Port로부터 데이터 수신
        /// </summary>
        /// <param name="receiveBytes">수신 데이터</param>
        /// <returns>성공 여부</returns>
        public bool Receive(List<byte> receiveBytes)
        {
            bool isSuccess = false;

            try
            {
                if (serialHandle.IsOpen)
                {
                    byte[] tempReceiveByte = new byte[1];

                    if (serialHandle.BytesToRead > 0)
                    {
                        do
                        {
                            serialHandle.Read(tempReceiveByte, 0, tempReceiveByte.Length);
                            receiveBytes.AddRange(tempReceiveByte);
                        } while (serialHandle.BytesToRead > 0);

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
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Fail :: Stream Is Empty", this.ToString()));

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

            return isSuccess;
        }
        #endregion
    }
}
