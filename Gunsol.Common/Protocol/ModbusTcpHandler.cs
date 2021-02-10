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
    /// Modbus TCP 프로토콜을 이용한 통신 기능을 제공하는 Class
    /// </summary>
    public class ModbusTcpHandler
    {
        #region Property
        /// <summary>
        /// TcpHandler 객체
        /// </summary>
        public TcpHandler tcpHandler;

        /// <summary>
        /// 연결 여부
        /// </summary>
        public bool isConnect
        {
            get
            {
                if (this.tcpHandler == null)
                {
                    return false;
                }
                else
                {
                    return this.tcpHandler.isConnect;
                }
            }
        }

        /// <summary>
        /// Modbus TCP Header Packet
        /// </summary>
        private List<byte> hDataUnit;

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="tcpInfo">Socket 통신 정보 객체</param>
        public ModbusTcpHandler(TcpInfo tcpInfo)
        {
            byte[] TRANSACTION_ID = new byte[] { 0x00, 0x00 };
            byte[] PROTOCOL_ID = new byte[] { 0x00, 0x00 };
            byte[] LENGTH = new byte[] { 0x00, 0x06 };
            byte[] UNIT_ID = new byte[] { 0x01 };

            tcpHandler = new TcpHandler(tcpInfo);

            hDataUnit = new List<byte>();
            hDataUnit.AddRange(TRANSACTION_ID);
            hDataUnit.AddRange(PROTOCOL_ID);
            hDataUnit.AddRange(LENGTH);
            hDataUnit.AddRange(UNIT_ID);
        }
        #endregion

        #region Method
        /// <summary>
        /// Modbus TCP Slave에 접속
        /// </summary>
        /// <param name="slaveIp">Modbus Slave Ip</param>
        /// <param name="slavePort">Modbus Slave Port</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Connect(string slaveIp, int slavePort)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                tcpHandler.Connect();

                if (tcpHandler.isConnect)
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
        /// Modbus TCP Slave에 접속 해제
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult DisConnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (tcpHandler != null)
                {
                    if (tcpHandler.isConnect)
                    {
                        tcpHandler.DisConnect();
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
        /// Modbus TCP 프로토콜을 사용하여 데이터 읽기
        /// </summary>
        /// <param name="funcCode">함수 Code</param>
        /// <param name="beginAddress">시작 주소</param>
        /// <param name="readCount">데이터 수</param>
        /// <returns>함수 실행 결과 (ProtocolResult 객체)</returns>
        public CommonStruct.ProtocolResult Read(byte funcCode, ushort beginAddress, ushort readCount)
        {
            CommonStruct.ProtocolResult result = new CommonStruct.ProtocolResult();

            stopWatch.Start();

            try
            {
                List<byte> sendData = new List<byte>();
                List<byte> recvData = new List<byte>();
                byte[] beginAddressArray = BitConverter.GetBytes(beginAddress).Reverse().ToArray();
                byte[] readCountArray = BitConverter.GetBytes(readCount).Reverse().ToArray();
                byte[] readDataArray = new byte[2];

                sendData.AddRange(hDataUnit);
                sendData.Add(funcCode);
                sendData.AddRange(beginAddressArray);
                sendData.AddRange(readCountArray);

                CommonStruct.FuncResult sendResult = tcpHandler.Send(sendData.ToArray());

                if (sendResult.isSuccess)
                {
                    System.Threading.Thread.Sleep(10);

                    CommonStruct.ProtocolResult receiveResult = tcpHandler.Receive();

                    if (receiveResult.funcResult.isSuccess)
                    {
                        recvData = (List<byte>)receiveResult.receiveData;

                        if (recvData.Count > 0)
                        {
                            readDataArray[0] = recvData[recvData.Count - 1];
                            readDataArray[1] = recvData[recvData.Count - 2];

                            result.receiveData = BitConverter.ToUInt16(readDataArray, 0);
                            result.funcResult.isSuccess = true;
                        }
                        else
                        {
                            result.receiveData = null;
                            result.funcResult.isSuccess = false;
                        }
                    }
                    else
                    {
                        result.receiveData = null;
                        result.funcResult.isSuccess = false;
                    }
                }
                else
                {
                    result.receiveData = null;
                    result.funcResult.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                result.receiveData = null;
                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }
        #endregion
    }
}
