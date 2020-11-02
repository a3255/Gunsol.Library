using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gunsol.Common;
using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Protocol;
using Gunsol.Interface.Model;


namespace Gunsol.Interface.Interface.CNC
{
    public class InterfaceMazak : InterfaceCNCBase
    {
        #region Propery
        private const short MAZERR_OK = (short)MazakApi.MazError.MAZERR_OK;             // 라이브러리 ReturnCode (MAZERR_OK)
        private const short MAZERR_TIMEOUT = (short)MazakApi.MazError.MAZERR_TIMEOUT;   // 라이브러리 ReturnCode (MAZERR_TIMEOUT)
        private const short MAZERR_NONE = (short)MazakApi.MazError.MAZERR_NONE;         // 라이브러리 ReturnCode (MAZERR_NONE)
        private const short MAZERR_SOCK = (short)MazakApi.MazError.MAZERR_SOCK;         // 라이브러리 ReturnCode (MAZERR_SOCK)
        private ushort mazakHandle;                                                     // 라이브러리 핸들
        private int mazakReturn;                                                        // 라이브러리 ReturnCode 변수
        private int machineType;                                                        // 컨트롤러 버전
        private int machineAxesCount;                                                   // 설비 축 개수
        #endregion

        #region Constructor
        public InterfaceMazak(CommInfo commInfo)
        {
            try
            {
                base.division = "CNC";
                base.commInfo = commInfo;
                base.isConnect = false;
                //base.sendData = new Dictionary<string, object>();
                //base.receiveData = new Dictionary<string, object>();

                base.SPINDLE = new Dictionary<object, object>();
                base.AXIS_ABS = new Dictionary<object, object>();
                base.AXIS_LOAD = new Dictionary<object, object>();
                base.MODAL_VALUE = new Dictionary<object, object>();
                base.SPINDLE_LOAD = new Dictionary<object, object>();
                base.PARAM_VALUE = new Dictionary<object, object>();
                base.MACRO_VALUE = new Dictionary<object, object>();
                base.PLC_VALUE = new Dictionary<object, object>();
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
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
                    MazakApi.MAZ_CERTIFY certify = new MazakApi.MAZ_CERTIFY();
                    certify.vendorCode = "JCLWTD0GsDXpOAWZywZzeArRD6eTwaya";
                    certify.appName = "MAZAK API AGENT";
                    certify.userPasswd = "1234";

                    mazakReturn = MazakApi.MazConnect_s(ref mazakHandle, commInfo.machineIp, commInfo.machinePort, 5, certify);

                    if (mazakReturn == MAZERR_OK)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Success", this.ToString()));

                        base.isConnect = true;
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));

                        base.isConnect = false;
                    }
                }
            }
            catch(Exception ex)
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
                    mazakReturn = MazakApi.MazDisconnect(mazakHandle);

                    if (mazakReturn == MAZERR_OK)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Success", this.ToString()));

                        base.isConnect = false;
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));

                        base.isConnect = true;
                    }
                }
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
                CallGetRunMode();
                CallGetRunningSts();
                CallGetAlarm();
                CallGetFeed();
                CallGetSpindle();
                CallGetModal();
                CallGetCurrentTool();
                CallGetCurrentPos();
                CallGetAxisLoad();
                CallGetSpindleLoad();
                CallGetPartsCount();
                CallGetAccumulatedTime();
                CallGetMainPro();
                CallReceiveProgram();

                base.MAIN_PRG_NAME_NCDATA = ReadNCData(string.Format(@"{0}\{1}", System.Windows.Forms.Application.StartupPath, base.MAIN_PRG_NAME));

                LogHandler.WriteLog(base.division, string.Format("{0} :: Receive() Success", this.ToString()));
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
            base.Send();
        }

        /// <summary>
        /// Parsing Machine Data To Base Property
        /// </summary>
        public override void Parsing()
        {
            base.Parsing();
        }
        #endregion

        #region Method (Library)
        /// <summary>
        /// Read Machine Mode Data (MachineMode)
        /// </summary>
        private void CallGetRunMode()
        {
            try
            {
                short modeNo = -1;

                mazakReturn = MazakApi.MazGetRunMode(mazakHandle, 0, ref modeNo);

                if (mazakReturn == MAZERR_OK)
                {
                    base.MODE = GetDescription("MODE", modeNo);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunMode() Success :: MODE = {1}", this.ToString(), base.MODE));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunMode() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunMode() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunMode() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Status Data (MachineStatus)
        /// </summary>
        private void CallGetRunningSts()
        {
            try
            {
                short statusNo = -1;

                mazakReturn = MazakApi.MazGetRunningSts(mazakHandle, 0, ref statusNo);

                if (mazakReturn == MAZERR_OK)
                {
                    base.STATUS = GetDescription("STATUS", statusNo);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunningSts() Success :: STATUS = {1}", this.ToString(), base.STATUS));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunningSts() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunningSts() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRunningSts() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Alarm Info Data (MachineAlarmStatus, MachineAlarmNo, MachineAlarmMsg)
        /// </summary>
        private void CallGetAlarm()
        {
            try
            {
                MazakApi.MAZ_ALARM_ALL mazAlarmAll = new MazakApi.MAZ_ALARM_ALL();

                mazakReturn = MazakApi.MazGetAlarm(mazakHandle, ref mazAlarmAll);

                if (mazakReturn == MAZERR_OK)
                {
                    for (int i = 0; i < mazAlarmAll.alarm.Length; i++)
                    {
                        char[] binStatus = Convert.ToString(mazAlarmAll.alarm[i].sts, 2).PadLeft(8, '0').ToCharArray();

                        if (binStatus[7] == '1')
                        {
                            base.ALARM = GetDescription("ALARM", Convert.ToInt16(binStatus[6].ToString()));
                            base.ALARM_NO = (int)mazAlarmAll.alarm[i].eno;
                            base.ALARM_MSG = new string(mazAlarmAll.alarm[i].message).Replace("\u0000", string.Empty);
                        }
                    }

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAlarm() Success :: ALARM = {1}", this.ToString(), base.ALARM));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAlarm() Success :: ALARM_NO = {1}", this.ToString(), base.ALARM_NO));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAlarm() Success :: ALARM_MSG = {1}", this.ToString(), base.ALARM_MSG));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAlarm() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAlarm() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAlarm() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Feedrate Data (MachineFeed)
        /// </summary>
        private void CallGetFeed()
        {
            try
            {
                MazakApi.MAZ_FEED mazFeed = new MazakApi.MAZ_FEED();

                mazakReturn = MazakApi.MazGetFeed(mazakHandle, 0, ref mazFeed);

                if (mazakReturn == MAZERR_OK)
                {
                    base.FEED = (int)mazFeed.fmin;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetFeed() Success :: FEED = {1}", this.ToString(), base.FEED));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetFeed() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetFeed() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetFeed() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Spindle RPM Data (MachineSpindle)
        /// </summary>
        private void CallGetSpindle()
        {
            try
            {
                int spindle = 0;

                mazakReturn = MazakApi.MazGetCurrentSpindleRev(mazakHandle, 0, ref spindle);

                if (mazakReturn == MAZERR_OK)
                {
                    base.SetDictionary(base.SPINDLE, 0, spindle);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindle() Success :: SPINDLE = {1}", this.ToString(), base.SPINDLE[0]));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindle() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindle() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindle() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Modal Data (M Code)
        /// </summary>
        private void CallGetModal()
        {
            try
            {
                MazakApi.MAZ_MODAL_INFO mazModalInfo = new MazakApi.MAZ_MODAL_INFO();

                mazakReturn = MazakApi.MazGetModal(mazakHandle, 0, ref mazModalInfo);

                if (mazakReturn == MAZERR_OK)
                {
                    base.M_CODE = (int)mazModalInfo.modalM;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetModal() Success :: VALUE = {1}", this.ToString(), base.M_CODE));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetModal() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetModal() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetModal() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Current Tool No Data (T Code)
        /// </summary>
        private void CallGetCurrentTool()
        {
            try
            {
                MazakApi.MAZ_TOOLINFO mazToolInfo = new MazakApi.MAZ_TOOLINFO();

                mazakReturn = MazakApi.MazGetCurrentTool(mazakHandle, 0, ref mazToolInfo);

                if (mazakReturn == MAZERR_OK)
                {
                    base.T_CODE = mazToolInfo.tno;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentTool() Success :: T CODE = {1}", this.ToString(), base.T_CODE));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentTool() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentTool() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentTool() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Position Data (MachineCurrentPosition)
        /// </summary>
        /// <param name="axisNo">Axis No</param>
        private void CallGetCurrentPos()
        {
            try
            {
                MazakApi.MAZ_NCPOS mazNcPos = new MazakApi.MAZ_NCPOS();
                int axisNo = 0;

                mazakReturn = MazakApi.MazGetCurrentPos(mazakHandle, ref mazNcPos);                

                if (mazakReturn == MAZERR_OK)
                {
                    for (int i = 0; i < mazNcPos.status.Length; i++)
                    {
                        char[] binStatus = Convert.ToString(mazNcPos.status[i], 2).PadLeft(8, '0').ToCharArray();

                        if (binStatus[7] == '1')
                        {
                            base.SetDictionary(base.AXIS_ABS, axisNo, Convert.ToDouble(mazNcPos.data[i]) / 10000);

                            LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentPos(AxisNo = {1}) Success :: ABS = {2}", this.ToString(), axisNo, base.AXIS_ABS[axisNo]));

                            axisNo++;
                        }
                    }
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentPos() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentPos() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetCurrentPos() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Axis Loadmeter Data (MachineAxisLoad)
        /// </summary>
        private void CallGetAxisLoad()
        {
            try
            {
                MazakApi.MAZ_AXISLOAD mazAxisLoad = new MazakApi.MAZ_AXISLOAD();
                int axisNo = 0;

                mazakReturn = MazakApi.MazGetAxisLoad(mazakHandle, ref mazAxisLoad);

                if (mazakReturn == MAZERR_OK)
                {
                    for (int i = 0; i < mazAxisLoad.status.Length; i++)
                    {
                        char[] binStatus = Convert.ToString(mazAxisLoad.status[i], 2).PadLeft(8, '0').ToCharArray();

                        if (binStatus[7] == '1')
                        {
                            base.SetDictionary(base.AXIS_LOAD, axisNo, mazAxisLoad.data[i]);

                            LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAxisLoad(AxisNo = {1}) Success :: AXIS LOAD = {2}", this.ToString(), axisNo, base.AXIS_LOAD[axisNo]));

                            axisNo++;
                        }
                    }
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAxisLoad() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAxisLoad() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAxisLoad() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Spindle Loadmeter Data (MachineSpindleLoad)
        /// </summary>
        private void CallGetSpindleLoad()
        {
            try
            {
                ushort spindleLoad = 0;

                mazakReturn = MazakApi.MazGetSpindleLoad(mazakHandle, 0, ref spindleLoad);

                if (mazakReturn == MAZERR_OK)
                {
                    base.SetDictionary(base.SPINDLE_LOAD, 0, spindleLoad);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindleLoad() Success :: SPINDLE LOAD = {1}", this.ToString(), base.SPINDLE_LOAD[0]));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindleLoad() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindleLoad() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetSpindleLoad() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Part Count Data (MachinePartCount)
        /// </summary>
        private void CallGetPartsCount()
        {
            try
            {
                int partCount = 0;

                mazakReturn = MazakApi.MazGetPartsCount(mazakHandle, 0, ref partCount);

                if (mazakReturn == MAZERR_OK)
                {
                    base.PART_CNT = partCount;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetPartsCount() Success :: PART COUNT = {1}", this.ToString(), base.PART_CNT));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetPartsCount() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetPartsCount() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetPartsCount() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Time Data (MachinePowerTime(Min), MachineOperationTime(Min), MachineCuttingTime(Min))
        /// </summary>
        private void CallGetAccumulatedTime()
        {
            try
            {
                MazakApi.MAZ_ACCUM_TIME mazAccumTime = new MazakApi.MAZ_ACCUM_TIME();

                mazakReturn = MazakApi.MazGetAccumulatedTime(mazakHandle, 0, ref mazAccumTime);

                if (mazakReturn == MAZERR_OK)
                {
                    int powerOnTime = (int)((mazAccumTime.power_on.hor * 60) + mazAccumTime.power_on.min);
                    int operationTime = (int)((mazAccumTime.auto_ope.hor * 60) + mazAccumTime.auto_ope.min);
                    int cuttingTime = (int)((mazAccumTime.auto_cut.hor * 60) + mazAccumTime.auto_cut.min);

                    base.POWER_T = powerOnTime;
                    base.OPERATION_T = operationTime;
                    base.CUTTING_T = cuttingTime;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAccumulatedTime() Success :: POWER TIME = {1}", this.ToString(), base.POWER_T));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAccumulatedTime() Success :: OPERATION TIME = {1}", this.ToString(), base.OPERATION_T));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAccumulatedTime() Success :: CUTTING TIME = {1}", this.ToString(), base.CUTTING_T));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAccumulatedTime() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAccumulatedTime() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetAccumulatedTime() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine System Macro Data
        /// </summary>
        /// <param name="macroNo">System Macro No</param>
        private void CallGetRangedCmnVar(short macroNo)
        {
            try
            {
                double macroValue = 0;

                mazakReturn = MazakApi.MazGetRangedCmnVar(mazakHandle, 0, macroNo, macroNo, ref macroValue);

                if (mazakReturn == MAZERR_OK)
                {
                    base.SetDictionary(base.MACRO_VALUE, macroNo, macroValue);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRangedCmnVar(MacroNo = {1}) Success :: VALUE = {2}", this.ToString(), macroNo, base.MACRO_VALUE[macroNo]));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRangedCmnVar(MacroNo = {1}) Fail :: SocketError", this.ToString(), macroNo));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRangedCmnVar(MacroNo = {1}) Fail :: ReturnCode = {2}", this.ToString(), macroNo, mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetRangedCmnVar(MacroNo = {1}) Exception :: Message = {2}", this.ToString(), macroNo, ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Program Name Data (MachineCurrentProgramName)
        /// </summary>
        private void CallGetMainPro()
        {
            try
            {
                MazakApi.MAZ_PROINFO mazProInfo = new MazakApi.MAZ_PROINFO();

                mazakReturn = MazakApi.MazGetMainPro(mazakHandle, 0, ref mazProInfo);

                if (mazakReturn == MAZERR_OK)
                {
                    base.MAIN_PRG_NAME = new string(mazProInfo.wno).Replace("\u0000", string.Empty);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetMainPro() Success :: MAIN PROGRAM NAME = {1}", this.ToString(), base.MAIN_PRG_NAME));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetMainPro() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetMainPro() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallGetMainPro() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine File System Data (Download MachineMainNCData)
        /// </summary>
        private void CallReceiveProgram()
        {
            try
            {
                char[] wno = base.MAIN_PRG_NAME.ToCharArray();
                char[] path = string.Format(@"{0}", System.Windows.Forms.Application.StartupPath).ToCharArray();
                short overwrite = 1;
                short area = 1;

                mazakReturn = MazakApi.MazReceiveProgram(mazakHandle, wno, path, overwrite);

                if (mazakReturn == MAZERR_OK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram() Success :: PATH = {1}", this.ToString(), new string(path)));
                }
                else if (mazakReturn == MAZERR_SOCK)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram() Fail :: SocketError", this.ToString()));

                    base.isConnect = false;
                }
                else if (mazakReturn == MAZERR_NONE)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram() Fail :: No NC File", this.ToString()));

                    mazakReturn = MazakApi.MazReceiveProgram2(mazakHandle, wno, path, overwrite, area);

                    if (mazakReturn == MAZERR_OK)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram2() Success :: PATH = {1}", this.ToString(), new string(path)));
                    }
                    else if (mazakReturn == MAZERR_SOCK)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram2() Fail :: SocketError", this.ToString()));

                        base.isConnect = false;
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram2() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                    }
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram.MazReceiveProgram() Fail :: ReturnCode = {1}", this.ToString(), mazakReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallReceiveProgram() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Convert Code To Description
        /// </summary>
        /// <param name="type">CodeType</param>
        /// <param name="value">Code</param>
        /// <returns>Description</returns>
        private string GetDescription(string type, short value)
        {
            string description = string.Empty;

            if (type.Equals("MODE"))
            {
                description = ((MazakApi.MazMode)value).ToString().Replace("__", "/").Replace("_", " ").ToUpper();
            }
            else if (type.Equals("STATUS"))
            {
                description = ((MazakApi.MazStatus)value).ToString().Replace("__", "/").Replace("_", " ").ToUpper();
            }
            else if (type.Equals("ALARM"))
            {
                description = ((MazakApi.MazAlarm)value).ToString().Replace("__", "/").Replace("_", " ").ToUpper();
            }
            else if (type.Equals("PROG"))
            {
                description = ((MazakApi.MazProg)value).ToString().Replace("__", "/").Replace("_", " ").ToUpper();
            }
            else
            {
                description = value.ToString();
            }
            return description;
        }

        /// <summary>
        /// Read Machine NC Data Using Download NC Program File
        /// </summary>
        /// <param name="ncDataPath">NC Program File Local Path</param>
        /// <returns>NC Program</returns>
        private string ReadNCData(string ncDataPath)
        {
            string strNcData = string.Empty;

            try
            {
                FileHandler fileHandle = new FileHandler(ncDataPath);

                if (fileHandle.isExist)
                {
                    strNcData = fileHandle.FileRead();

                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Success :: CONTENTS = {1}", this.ToString(), strNcData));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: File Not Exists (PATH = {1})", this.ToString(), ncDataPath));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }

            return strNcData;
        }
        #endregion
    }
}
