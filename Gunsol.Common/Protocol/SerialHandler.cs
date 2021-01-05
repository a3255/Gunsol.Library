using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO.Ports;
using System.Text;

using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;

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
        /// Serial 연결 상태
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

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="serialInfo">Serial 통신 정보 객체 (생략할 경우 이후 초기화 필요)</param>
        public SerialHandler(SerialInfo serialInfo = null)
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
        #endregion

        #region Method
        /// <summary>
        /// Serial Port에 접속
        /// </summary>
        /// <param name="serialInfo">Serial 통신 정보 객체</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Connect(SerialInfo serialInfo = null)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (serialInfo == null && this.serialInfo == null)
                {
                    result.isSuccess = false;
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
                    result.isSuccess = true;
                }
                else
                {
                    result.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.funcException = ex;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// Serial Port 접속 해제
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult DisConnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (serialHandle != null)
                {
                    if (serialHandle.IsOpen)
                    {
                        serialHandle.Close();
                    }

                    result.isSuccess = true;
                }
                else
                {
                    result.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.funcException = ex;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// Serial Port에 데이터 송신
        /// </summary>
        /// <param name="sendBytes">송신 데이터</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Send(byte[] sendBytes)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

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
                        result.isSuccess = true;
                    }
                    else
                    {
                        result.isSuccess = false;
                    }
                }
                else
                {
                    result.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.funcException = ex;
            }

            return result;
        }

        /// <summary>
        /// Serial Port로부터 데이터 수신
        /// </summary>
        /// <returns>함수 실행 결과 (ProtocolResult 객체)</returns>
        public CommonStruct.ProtocolResult Receive()
        {
            CommonStruct.ProtocolResult result = new CommonStruct.ProtocolResult();
            List<byte> receiveBytes = new List<byte>();

            stopWatch.Start();

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
                            result.funcResult.isSuccess = true;
                        }
                        else
                        {
                            receiveBytes = null;
                            result.funcResult.isSuccess = false;
                        }
                    }
                    else
                    {
                        receiveBytes = null;
                        result.funcResult.isSuccess = false;
                    }
                }
                else
                {
                    receiveBytes = null;
                    result.funcResult.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                receiveBytes = null;
                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;
            result.receiveData = receiveBytes;

            stopWatch.Reset();

            return result;
        }
        #endregion
    }
}
