using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

using NDde.Client;
using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;

namespace Gunsol.Common.Protocol
{
    /// <summary>
    /// DDE 통신 기능을 제공하는 Class
    /// </summary>
    public class DdeHandler
    {
        #region Property
        /// <summary>
        /// DDE Client 객체
        /// </summary>
        private DdeClient ddeHandle;

        /// <summary>
        /// DDE 통신 정보 객체
        /// </summary>
        public DdeInfo ddeInfo { get; set; }

        /// <summary>
        /// DDE 연결 상태
        /// </summary>
        public bool isConnect
        {
            get
            {
                if (ddeHandle == null)
                {
                    return false;
                }
                else
                {
                    return ddeHandle.IsConnected;
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
        /// <param name="ddeInfo">DDE 통신 정보 객체</param>
        public DdeHandler(DdeInfo ddeInfo)
        {
            this.ddeHandle = new DdeClient(ddeInfo.ddeService, ddeInfo.ddeTopic);
            this.ddeInfo = ddeInfo;
        }
        #endregion

        #region Method
        /// <summary>
        /// DDE Server에 접속
        /// </summary>
        /// <param name="ddeInfo">DDE 통신 정보 객체 (생략할 경우, 생성자 호출시 사용한 Paramter를 이용)</param>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Connect(DdeInfo ddeInfo = null)
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (ddeInfo == null && this.ddeInfo == null)
                {
                    result.isSuccess = false;
                }
                else
                {
                    if (ddeHandle == null)
                    {
                        if (ddeInfo == null)
                        {
                            ddeHandle = new DdeClient(this.ddeInfo.ddeService, this.ddeInfo.ddeTopic);
                        }
                        else
                        {
                            ddeHandle = new DdeClient(ddeInfo.ddeService, ddeInfo.ddeTopic);
                            this.ddeInfo = ddeInfo;
                        }
                    }

                    ddeHandle.Connect();

                    if (ddeHandle.IsConnected)
                    {
                        result.isSuccess = true;
                    }
                    else
                    {
                        result.isSuccess = false;
                    }
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
        /// DDE Server 접속 해제
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult DisConnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (ddeHandle != null)
                {
                    if (ddeHandle.IsConnected)
                    {
                        ddeHandle.Disconnect();

                        result.isSuccess = true;
                    }
                    else
                    {
                        result.isSuccess = true;
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

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// DDE Server에 Request
        /// </summary>
        /// <param name="ddeItem">Request Item</param>
        /// <returns>함수 실행 결과 (ProtocolResult 객체)</returns>
        public CommonStruct.ProtocolResult Request(string ddeItem)
        {
            CommonStruct.ProtocolResult result = new CommonStruct.ProtocolResult();
            string receiveValue = string.Empty;

            stopWatch.Start();

            try
            {
                receiveValue = ddeHandle.Request(ddeItem, 1);
                result.funcResult.isSuccess = true;
            }
            catch (Exception ex)
            {
                receiveValue = string.Empty;
                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;
            result.receiveData = receiveValue;

            stopWatch.Reset();

            return result;
        }
        #endregion
    }
}
