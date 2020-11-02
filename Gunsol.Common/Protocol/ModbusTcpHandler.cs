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
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="tcpInfo">Socket 통신 정보 객체</param>
        public ModbusTcpHandler(TcpInfo tcpInfo)
        {
            try
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
            catch(Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// TCP Server/Client에 접속
        /// </summary>
        /// <param name="slaveIp"></param>
        /// <param name="slavePort"></param>
        public void Connect(string slaveIp, int slavePort)
        {
            try
            {
                tcpHandler.Connect();
            }
            catch (Exception ex)
            {
                LogHandler.PrintLog(string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// TCP Server/Client 접속 해제
        /// </summary>
        public void DisConnect()
        {
            try
            {
                if (tcpHandler != null)
                {
                    tcpHandler.DisConnect();

                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: SerialHandler Not Initialized", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Modbus TCP 프로토콜을 사용하여 데이터 읽기
        /// </summary>
        /// <param name="funcCode">Function Code</param>
        /// <param name="beginAddress">시작 주소</param>
        /// <param name="readCount">데이터 수</param>
        /// <returns></returns>
        public ushort Read(byte funcCode, ushort beginAddress, ushort readCount)
        {
            ushort data = 0;

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

                if (tcpHandler.Send(sendData.ToArray()))
                {
                    System.Threading.Thread.Sleep(10);

                    if (tcpHandler.Receive(recvData))
                    {
                        if (recvData.Count > 0)
                        {
                            readDataArray[0] = recvData[recvData.Count - 1];
                            readDataArray[1] = recvData[recvData.Count - 2];

                            data = BitConverter.ToUInt16(readDataArray, 0);

                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: Read(Address = {1}) Success :: Data = {2}", this.ToString(), beginAddress, data));
                        }
                        else
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: Read(Address = {1}) Fail :: Receive Response Fail", this.ToString(), beginAddress));
                        }
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Read(Address = {1}) Fail :: Receive Response Fail", this.ToString(), beginAddress));
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Read(Address = {1}) Fail :: Send Request Fail", this.ToString(), beginAddress));
                }
            }
            catch (Exception ex)
            {
                LogHandler.PrintLog(string.Format("{0} :: Read(Address = {1}) Exception :: Message = {2}", this.ToString(), beginAddress, ex.Message));
            }

            return data;
        }
        #endregion
    }
}
