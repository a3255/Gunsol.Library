using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Gunsol.Common;
using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Protocol;
using Gunsol.Interface.Model;


namespace Gunsol.Interface.Interface.PLC
{
    public class InterfaceLs : InterfacePLCBase
    {
        #region Propery
        private TcpClient lsHandle;     // TCP 핸들
        private List<string> addrList;  // 읽으려는 PLC Address 리스트
        #endregion

        #region Constructor
        public InterfaceLs(CommInfo commInfo)
        {
            base.division = "PLC";
            base.commInfo = commInfo;
            base.isConnect = false;
            //base.sendData = new Dictionary<string, object>();
            //base.receiveData = new Dictionary<string, object>();
            base.PLC_VALUE = new Dictionary<object, object>();

            lsHandle = new TcpClient();
            addrList = GetAddresses();
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
                    lsHandle.Connect(commInfo.machineIp, commInfo.machinePort);

                    if (lsHandle.Connected)
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
                    lsHandle.Close();
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
                if (addrList.Count > 0)
                {
                    foreach (string a in addrList)
                    {
                        int singleData = ReadPlc(a);
                        base.SetDictionary(base.PLC_VALUE, a, singleData);

                        LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Success", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Fail :: Address Is Empty", this.ToString()));
                }
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

        #region Method
        /// <summary>
        /// Read PLC Address Data
        /// </summary>
        /// <param name="address">PLC Address</param>
        /// <returns>Data</returns>
        private int ReadPlc(string address)
        {
            int readData = 0;

            try
            {
                NetworkStream plcStream = lsHandle.GetStream();
                List<byte> sendPacketList = new List<byte>();
                List<byte> recvPacketList = new List<byte>();
                byte[] sendPacket;
                byte[] recvPacket;
                byte[] tempPacket = new byte[1];
                byte[] dataPacket = CreateDataPacket(address);
                byte[] headerPacket = CreateHeaderPacket(dataPacket.Length);
                int totalRecvCount = 0;
                int recvCount = 0;

                sendPacketList.AddRange(headerPacket);
                sendPacketList.AddRange(dataPacket);
                sendPacket = sendPacketList.ToArray();

                plcStream.WriteTimeout = 5000;
                plcStream.ReadTimeout = 5000;

                plcStream.Write(sendPacket, 0, sendPacket.Length);
                plcStream.Flush();

                System.Threading.Thread.Sleep(10);

                do
                {
                    recvCount = plcStream.Read(tempPacket, 0, tempPacket.Length);

                    if (recvCount > 0)
                    {
                        recvPacketList.AddRange(tempPacket);

                        totalRecvCount += recvCount;
                    }
                } while (plcStream.DataAvailable);

                if (totalRecvCount > 0)
                {
                    recvPacket = Encoding.UTF8.GetBytes(Encoding.Default.GetString(recvPacketList.ToArray()).Substring(0, totalRecvCount));

                    if (address.IndexOf("X") >= 0)
                    {
                        readData = Convert.ToInt32((recvPacket[recvPacket.Length - 1]).ToString());
                    }
                    else
                    {
                        readData = Convert.ToInt32((recvPacket[recvPacket.Length - 1] << 8 | recvPacket[recvPacket.Length - 2]).ToString());
                    }

                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadPlc(Address = {1}) Success :: DATA = {2}", this.ToString(), address, readData));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadPlc(Address = {1}) Fail", this.ToString(), address));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadPlc(Address = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, address));
            }

            return readData;
        }

        /// <summary>
        /// Write PLC Address Data
        /// </summary>
        /// <param name="address">PLC Address</param>
        private void WritePlc(string address)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: WritePlc(Address = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, address));
            }
            finally
            {

            }
        }

        /// <summary>
        /// Get PLC Address From Text File
        /// </summary>
        /// <returns>Address List</returns>
        private List<string> GetAddresses()
        {
            List<string> addresses = new List<string>();

            try
            {
                if (File.Exists(commInfo.machineAddressPath))
                {
                    FileStream fileStream = new FileStream(commInfo.machineAddressPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader fileReader = new StreamReader(fileStream);
                    string fileContents = fileReader.ReadToEnd();

                    fileContents = fileContents.Replace("\r\n", "\n");

                    if (fileContents.IndexOf("\n") >= 0)
                    {
                        addresses = fileContents.Split('\n').ToList();
                    }
                    else if (fileContents.Equals(string.Empty))
                    {
                        addresses.Add(fileContents);
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: GetAddresses() Fail :: Address File Is Empty", this.ToString()));
                    }

                    LogHandler.WriteLog(base.division, string.Format("{0} :: GetAddresses() Success :: COUNT = {1}", this.ToString(), addresses.Count));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: GetAddresses() Exception :: Message = {1}", this.ToString(), ex.Message));
            }

            return addresses;
        }

        /// <summary>
        /// Create LS Private Communication Header Packet
        /// </summary>
        /// <param name="dataPacketLen">LS Private Communication Data Packet Length</param>
        /// <returns>LS Private Communication Header Packet</returns>
        private byte[] CreateHeaderPacket(int dataPacketLen)
        {
            byte[] retPacket = new byte[20];

            retPacket[0] = 0x4C;
            retPacket[1] = 0x53;
            retPacket[2] = 0x49;
            retPacket[3] = 0x53;
            retPacket[4] = 0x2D;
            retPacket[5] = 0x58;
            retPacket[6] = 0x47;
            retPacket[7] = 0x54;
            retPacket[8] = 0x00;
            retPacket[9] = 0x00;
            retPacket[10] = 0x00;
            retPacket[11] = 0x00;
            retPacket[12] = 0xA0;
            retPacket[13] = 0x33;
            retPacket[14] = 0x02;
            retPacket[15] = 0x00;
            retPacket[16] = Convert.ToByte(dataPacketLen);
            retPacket[17] = 0x00;
            retPacket[18] = 0x03;
            retPacket[19] = 0x40;

            return retPacket;
        }

        /// <summary>
        /// Create LS Private Communication Data Packet
        /// </summary>
        /// <param name="address">PLC Address</param>
        /// <returns>LS Private Communication Data Packet</returns>
        private byte[] CreateDataPacket(string address)
        {
            byte[] retPacket = new byte[10 + address.Length];

            retPacket[0] = 0x54;
            retPacket[1] = 0x00;
            retPacket[2] = 0x02;
            retPacket[3] = 0x00;
            retPacket[4] = 0x00;
            retPacket[5] = 0x00;
            retPacket[6] = 0x01;
            retPacket[7] = 0x00;
            retPacket[8] = Convert.ToByte(address.Length);
            retPacket[9] = 0x00;

            for (int i = 10; i < retPacket.Length; i++)
            {
                retPacket[i] = Encoding.UTF8.GetBytes(address)[i - 10];
            }

            return retPacket;
        }
        #endregion
    }
}
