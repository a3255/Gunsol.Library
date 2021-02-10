using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

using uPLibrary.Networking.M2Mqtt;
using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;

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

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="hostName">MQTT Broker IP</param>
        public MqttHandler(string hostName)
        {
            mqttHandle = new MqttClient(hostName);
            subMessage = new Dictionary<string, string>();

            mqttHandle.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
        }
        #endregion

        #region Method
        /// <summary>
        /// MQTT Broker에 접속
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Connect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                mqttHandle.Connect(Guid.NewGuid().ToString());

                if (mqttHandle.IsConnected)
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
        /// MQTT Broker 접속 해제
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult DisConnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (mqttHandle == null)
                {
                    if (isConnect)
                    {
                        subMessage.Clear();

                        mqttHandle.Disconnect();
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
        /// MQTT Broker에 Topic 발행 (송신)
        /// </summary>
        /// <param name="mqttTopic">Topic 명</param>
        /// <param name="topicMessage">메시지</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Publish(string mqttTopic, string topicMessage)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            try
            {
                if (mqttHandle.IsConnected)
                {
                    byte[] sendBytes = Encoding.UTF8.GetBytes(topicMessage);
                    ushort publishResult = mqttHandle.Publish(mqttTopic, sendBytes);

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

            return result;
        }

        /// <summary>
        /// MQTT Broker Topic 구독
        /// </summary>
        /// <param name="mqttTopics">Topic 명</param>
        /// <param name="qosLevels">QoS</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Subscribe(List<string> mqttTopics, List<byte> qosLevels)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            try
            {
                ushort subscribeResult = mqttHandle.Subscribe(mqttTopics.ToArray(), qosLevels.ToArray());

                result.isSuccess = true;
                result.funcException = null;
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.funcException = ex;
            }

            return result;
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

            }
        }
        #endregion
    }
}
