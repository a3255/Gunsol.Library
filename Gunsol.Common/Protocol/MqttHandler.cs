using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using uPLibrary.Networking.M2Mqtt;
using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Model;

namespace Gunsol.Common.Protocol
{
    /// <summary>
    /// MQTT 프로토콜을 이용한 통신 기능을 제공하는 Class
    /// </summary>
    public class MqttHandler
    {
        #region Property
        /// <summary>
        /// MQTT Client 객체
        /// </summary>
        private MqttClient mqttHandle;

        /// <summary>
        /// 연결 여부
        /// </summary>
        public bool isConnect
        {
            get
            {
                if (mqttHandle == null)
                {
                    return false;
                }
                else
                {
                    return mqttHandle.IsConnected;
                }
            }
        }

        /// <summary>
        /// 수신 메시지
        /// </summary>
        public Dictionary<string, string> subMessage { get; set; }
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="hostName">MQTT Broker IP</param>
        public MqttHandler(string hostName)
        {
            try
            {
                mqttHandle = new MqttClient(hostName);
                subMessage = new Dictionary<string, string>();

                mqttHandle.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// MQTT Broker에 접속
        /// </summary>
        public void Connect()
        {
            try
            {
                mqttHandle.Connect(Guid.NewGuid().ToString());

                if (isConnect)
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
        /// MQTT Broker 접속 해제
        /// </summary>
        public void DisConnect()
        {
            try
            {
                if (mqttHandle == null)
                {
                    if (isConnect)
                    {
                        subMessage.Clear();

                        mqttHandle.Disconnect();

                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: Not Conneted", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Fail :: MQTT Not Initialized", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// MQTT Broker에 Topic 발행 (송신)
        /// </summary>
        /// <param name="mqttTopic">Topic 명</param>
        /// <param name="topicMessage">메시지</param>
        public void Publish(string mqttTopic, string topicMessage)
        {
            try
            {
                byte[] sendBytes = Encoding.UTF8.GetBytes(topicMessage);
                ushort publishResult = mqttHandle.Publish(mqttTopic, sendBytes);

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Publish(Topic = {1}) Success", this.ToString(), mqttTopic));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Publish(Topic = {1}) Exception :: Message = {2}", this.ToString(), mqttTopic, ex.Message));
            }
        }

        /// <summary>
        /// MQTT Broker Topic 구독
        /// </summary>
        /// <param name="mqttTopics">Topic 명</param>
        /// <param name="qosLevels">QoS</param>
        public void Subscribe(List<string> mqttTopics, List<byte> qosLevels)
        {
            try
            {
                ushort subscribeResult = mqttHandle.Subscribe(mqttTopics.ToArray(), qosLevels.ToArray());

                foreach (string t in mqttTopics)
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Subscribe(Topic = {1}) Success", this.ToString(), t));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Subscribe() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// 구독한 Topic의 메시지가 수신되었을 때 수신 메시지 Property에 저장
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Argument</param>
        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                string pubTopic = e.Topic;
                string pubMsg = Encoding.Default.GetString(e.Message);
                int pubQosLevel = Convert.ToInt32(e.QosLevel);
                bool isRetain = e.Retain;
                bool isDup = e.DupFlag;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive(Topic = {1}) Success :: {1} = {2}", this.ToString(), pubTopic, pubMsg));

                if (subMessage.Keys.Contains(pubTopic))
                {
                    subMessage[pubTopic] = pubMsg;
                }
                else
                {
                    subMessage.Add(pubTopic, pubMsg);
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Receive() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion
    }
}
