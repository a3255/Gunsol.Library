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
    public class InterfaceNeuromeka : InterfaceROBOTBase
    {
        #region Propery
        public MqttHandler neuromekaHandle; // MQTT 핸들
        public List<string> subTopics;      // 구독 Topic 리스트
        public List<byte> subQosLevels;     // 구독 QoS 리스트
        #endregion

        #region Constructor
        public InterfaceNeuromeka(CommInfo commInfo)
        {
            base.division = "ROBOT";
            base.commInfo = commInfo;
            base.isConnect = false;
            base.SEND_DATA = new Dictionary<object, object>();
            //base.sendData = new Dictionary<string, object>();
            //base.receiveData = new Dictionary<string, object>();

            neuromekaHandle = new MqttHandler(commInfo.machineIp);
            subTopics = new List<string>();
            subQosLevels = new List<byte>();
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
                    if (neuromekaHandle.Connect())
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
                    subTopics.Clear();
                    subQosLevels.Clear();

                    neuromekaHandle.DisConnect();
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
        /// Send Data To Machine Using Library Method
        /// </summary>
        public override void Send()
        {
            try
            {
                foreach (KeyValuePair<object, object> d in base.SEND_DATA)
                {
                    neuromekaHandle.Publish(d.Key.ToString(), d.Value.ToString());

                    LogHandler.WriteLog(base.division, string.Format("{0} :: Send(Topic = {1}) Success :: VALUE = {2}", this.ToString(), d.Key, d.Value));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Send() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Receive Machine Data Using Library Method
        /// </summary>
        public override void Receive()
        {
            try
            {
                if (subTopics.Count == 0)
                {
                    subTopics.Add("RBT_GRP");
                    subTopics.Add("RBT_BGN");
                    subTopics.Add("RBT_END");
                    subTopics.Add("RBT_ERR");
                    subTopics.Add("RBT_VSN");

                    subQosLevels.Add(0x00);
                    subQosLevels.Add(0x00);
                    subQosLevels.Add(0x00);
                    subQosLevels.Add(0x00);
                    subQosLevels.Add(0x00);

                    neuromekaHandle.Subscribe(subTopics, subQosLevels);
                }

                if (neuromekaHandle.subMessage.Count > 0)
                {
                    foreach (KeyValuePair<string, string> m in neuromekaHandle.subMessage)
                    {
                        switch(m.Key)
                        {
                            case "RBT_GRP": base.RBT_GRP = Convert.ToInt32(m.Value); break;
                            case "RBT_BGN": base.RBT_BGN = m.Value; break;
                            case "RBT_END": base.RBT_END = m.Value; break;
                            case "RBT_ERR": base.RBT_ERR = m.Value; break;
                            case "RBT_VSN": base.RBT_VSN = Convert.ToInt32(m.Value); break;
                        }

                        LogHandler.WriteLog(base.division, string.Format("{0} :: Receive(Topic = {1}) Success :: DATA = {2}", this.ToString(), m.Key, m.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion
    }
}
