using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;

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
        /// TCP 연결 상태
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

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="tcpInfo">TCP 통신 정보 객체 (생략할 경우 이후 초기화 필요)</param>
        public TcpHandler(TcpInfo tcpInfo = null)
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
        #endregion

        #region Method
        /// <summary>
        /// TCP Server/Client에 접속
        /// </summary>
        /// <param name="tcpInfo">TCP 통신 정보 객체</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Connect(TcpInfo tcpInfo = null)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (tcpInfo == null && this.tcpInfo == null)
                {
                    result.isSuccess = false;
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
                        result.isSuccess = true;
                    }
                    else
                    {
                        result.isSuccess = false;
                    }
                }
            }
            catch(Exception ex)
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
        /// TCP Server/Client 접속 해제
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult DisConnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (tcpHandle != null)
                {
                    if (tcpHandle.Connected)
                    {
                        tcpHandle.Close();
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
                result.isSuccess = true;
                result.funcException = null;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// TCP Server/Client에 데이터 송신
        /// </summary>
        /// <param name="sendBytes">송신 데이터</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Send(byte[] sendBytes)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            NetworkStream tcpStream = null;

            stopWatch.Start();

            try
            {
                if (tcpHandle.Connected)
                {
                    tcpStream = tcpHandle.GetStream();

                    if (tcpStream.CanWrite)
                    {
                        tcpStream.Write(sendBytes, 0, sendBytes.Length);
                        tcpStream.Flush();

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
            finally
            {
                if (tcpStream != null)
                {
                    tcpStream.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// TCP Server/Client으로부터 데이터 수신
        /// </summary>
        /// <returns>함수 실행 결과 (ProtocolResult 객체)</returns>
        public CommonStruct.ProtocolResult Receive()
        {
            CommonStruct.ProtocolResult result = new CommonStruct.ProtocolResult();
            List<byte> receiveBytes = new List<byte>();

            NetworkStream tcpStream = null;

            stopWatch.Start();

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
            finally
            {
                if (tcpStream != null)
                {
                    tcpStream.Close();
                }
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
