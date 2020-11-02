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
    public class InterfaceFanuc : InterfaceCNCBase
    {
        #region Propery
        private const short EW_OK = (short)Focas1.focas_ret.EW_OK;          // 라이브러리 ReturnCode (EW_OK)
        private const short EW_BUFFER = (short)Focas1.focas_ret.EW_BUFFER;  // 라이브러리 ReturnCode (EW_BUFFER)
        private const short EW_SOCKET = (short)Focas1.focas_ret.EW_SOCKET;  // 라이브러리 ReturnCode (EW_SOCKET)
        private ushort fanucHandle;                                         // 라이브러리 핸들
        private short focasReturn;                                          // 라이브러리 ReturnCode 변수
        private int machineType;                                            // 컨트롤러 버전
        private int machineAxesCount;                                       // 설비 축 개수
        #endregion

        #region Constructor
        public InterfaceFanuc(CommInfo commInfo)
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
                base.SPINDLE_LOAD = new Dictionary<object, object>();
                base.MODAL_VALUE = new Dictionary<object, object>();
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
                    focasReturn = Focas1.cnc_allclibhndl3(commInfo.machineIp, commInfo.machinePort, 5, out fanucHandle);

                    if (focasReturn == EW_OK)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Success", this.ToString()));

                        base.isConnect = true;
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));

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
                    focasReturn = Focas1.cnc_freelibhndl(fanucHandle);

                    if (focasReturn == EW_OK)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Success", this.ToString()));

                        base.isConnect = false;
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));

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
                CallSysInfo();
                CallStatInfo();
                CallRdAlmMsg();
                CallActF();
                CallActS();
                CallRdSpMeter();
                CallRdPrgNum();
                CallExePrgName();
                for (short i = 0; i < machineAxesCount; i++)
                {
                    CallAbsolute(i);
                }
                CallRdSvMeter((short)machineAxesCount);
                CallModal(106);
                CallModal(104);
                CallRdParam(6750);
                CallRdParam(6752);
                CallRdParam(6754);
                CallRdParam(6758);
                CallRdParam(6712);

                base.MAIN_PRG_NO_NCDATA = ReadNCData(base.MAIN_PRG_NO);
                base.CURRENT_PRG_NO_NCDATA = ReadNCData(base.CURRENT_PRG_NO);
                base.CURRENT_PRG_NAME_NCDATA = ReadNCData(base.CURRENT_PRG_NAME);

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
            try
            {
                base.M_CODE = Convert.ToInt32(base.MODAL_VALUE[106]);
                base.T_CODE = Convert.ToInt32(base.MODAL_VALUE[106]);
                base.POWER_T = Convert.ToInt32(base.PARAM_VALUE[6750]);
                base.OPERATION_T = Convert.ToInt32(base.PARAM_VALUE[6752]);
                base.CUTTING_T = Convert.ToInt32(base.PARAM_VALUE[6754]);
                base.CYCLE_T = Convert.ToInt32(base.PARAM_VALUE[6758]);
                base.PART_CNT = Convert.ToInt32(base.PARAM_VALUE[6712]);

                LogHandler.WriteLog(base.division, string.Format("{0} :: Parsing() Success", this.ToString()));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: Parsing() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method (Library)
        /// <summary>
        /// Read Machine Info Data (MachineType, MachineAxesCount)
        /// </summary>
        private void CallSysInfo()
        {
            try
            {
                Focas1.ODBSYS odbSys = new Focas1.ODBSYS();

                focasReturn = Focas1.cnc_sysinfo(fanucHandle, odbSys);

                if (focasReturn == EW_OK)
                {
                    machineType = Convert.ToInt32(new string(odbSys.cnc_type));
                    machineAxesCount = Convert.ToInt32(new string(odbSys.axes));

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallSysInfo() Success :: MACHINE TYPE = {1}", this.ToString(), machineType));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallSysInfo() Success :: AXES COUNT = {1}", this.ToString(), machineAxesCount));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallSysInfo() Fail :: SocketError", this.ToString()));

                    isConnect = false;
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallSysInfo() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallSysInfo() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Status Data (MachineMode, MachineStatus, MachineAlarmStatus)
        /// </summary>
        private void CallStatInfo()
        {
            try
            {
                Focas1.ODBST odbSt = new Focas1.ODBST();

                focasReturn = Focas1.cnc_statinfo(fanucHandle, odbSt);

                if (focasReturn == EW_OK)
                {
                    base.MODE = GetDescription(machineType, "MODE", odbSt.aut.ToString());
                    base.STATUS = GetDescription(machineType, "STATUS", odbSt.run.ToString());
                    base.ALARM = GetDescription(machineType, "ALARM", odbSt.alarm.ToString());

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallStatInfo() Success :: MODE = {1}", this.ToString(), base.MODE));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallStatInfo() Success :: STATUS = {1}", this.ToString(), base.STATUS));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallStatInfo() Success :: ALARM = {1}", this.ToString(), base.ALARM));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallStatInfo() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallStatInfo() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallStatInfo() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Alarm Info Data (MachineAlarmType, MachineAlarmNo)
        /// </summary>
        private void CallRdAlmMsg()
        {
            try
            {
                Focas1.ODBALMMSG odbAlmMsg = new Focas1.ODBALMMSG();
                short alarmCount = 1;

                if (base.ALARM.Equals("ALarM"))
                {
                    focasReturn = Focas1.cnc_rdalmmsg(fanucHandle, -1, ref alarmCount, odbAlmMsg);

                    if (focasReturn == EW_OK)
                    {
                        base.ALARM_TYPE = GetDescription(machineType, "ALARM_TYPE", odbAlmMsg.msg1.type.ToString());
                        base.ALARM_NO = odbAlmMsg.msg1.alm_no;

                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdAlmMsg() Success :: ALARM TYPE = {1}", this.ToString(), base.ALARM_TYPE));
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdAlmMsg() Success :: ALARM NO = {1}", this.ToString(), base.ALARM_NO));
                    }
                    else if (focasReturn == EW_SOCKET)
                    {
                        base.isConnect = false;
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdAlmMsg() Fail :: SocketError", this.ToString()));
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdAlmMsg() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                    }
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdAlmMsg() Success :: ALARM = No Alarm", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdAlmMsg() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Feedrate Data (MachineFeed)
        /// </summary>
        private void CallActF()
        {
            try
            {
                Focas1.ODBACT odbAct = new Focas1.ODBACT();

                focasReturn = Focas1.cnc_actf(fanucHandle, odbAct);

                if (focasReturn == EW_OK)
                {
                    base.FEED = odbAct.data;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallActF() Success :: FEED = {1}", this.ToString(), base.FEED));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallActF() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallActF() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallActF() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Spindle RPM Data (MachineSpindle)
        /// </summary>
        private void CallActS()
        {
            try
            {
                Focas1.ODBACT odbAct = new Focas1.ODBACT();

                focasReturn = Focas1.cnc_acts(fanucHandle, odbAct);

                if (focasReturn == EW_OK)
                {
                    base.SetDictionary(base.SPINDLE, 0, odbAct.data);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallActS() Success :: STATUS = {1}", this.ToString(), base.SPINDLE[0]));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallActS() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallActS() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallActS() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Modal Data (M Code, T Code)
        /// </summary>
        /// <param name="typeNo">Modal Type No</param>
        private void CallModal(short typeNo)
        {
            try
            {
                Focas1.ODBMDL odbMdl = new Focas1.ODBMDL();

                focasReturn = Focas1.cnc_modal(fanucHandle, typeNo, 0, odbMdl);

                if (focasReturn == EW_OK)
                {
                    base.SetDictionary(base.MODAL_VALUE, typeNo, odbMdl.modal.aux.aux_data);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallModal(TypeNo = {2}) Success :: VALUE = {1}", this.ToString(), base.MODAL_VALUE[typeNo], typeNo));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallModal(TypeNo = {1}) Fail :: SocketError", this.ToString(), typeNo));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallModal(TypeNo = {2}) Fail :: ReturnCode = {1}", this.ToString(), focasReturn, typeNo));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallModal(TypeNo = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, typeNo));
            }
        }

        /// <summary>
        /// Read Machine Position Data (MachineAbsolutePosition)
        /// </summary>
        /// <param name="axisNo">Axis No</param>
        private void CallAbsolute(short axisNo)
        {
            try
            {
                Focas1.ODBAXIS odbAxis = new Focas1.ODBAXIS();
                short pAxisNo = (short)(axisNo + 1);

                focasReturn = Focas1.cnc_absolute2(fanucHandle, pAxisNo, 8, odbAxis);

                if (focasReturn == EW_OK)
                {
                    base.SetDictionary(base.AXIS_ABS, axisNo, Convert.ToDouble(odbAxis.data[0]) / 1000);
                    
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAbsolute(AxisNo = {2}) Success :: ABS = {1}", this.ToString(), base.AXIS_ABS[axisNo], axisNo));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAbsolute(AxisNo = {1}) Fail :: SocketError", this.ToString(), axisNo));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAbsolute(AxisNo = {2}) Fail :: ReturnCode = {1}", this.ToString(), focasReturn, axisNo));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallAbsolute(AxisNo = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, axisNo));
            }
        }

        /// <summary>
        /// Read Machine Axis Loadmeter Data (MachineAxisLoad)
        /// </summary>
        /// <param name="axisCount">Axis Count</param>
        private void CallRdSvMeter(short axisCount)
        {
            try
            {
                Focas1.ODBSVLOAD odbSvLoad = new Focas1.ODBSVLOAD();
                List<int> totalMachineLoad = new List<int>();

                focasReturn = Focas1.cnc_rdsvmeter(fanucHandle, ref axisCount, odbSvLoad);

                if (focasReturn == EW_OK)
                {
                    totalMachineLoad.Add(odbSvLoad.svload1.data);
                    totalMachineLoad.Add(odbSvLoad.svload2.data);
                    totalMachineLoad.Add(odbSvLoad.svload3.data);
                    totalMachineLoad.Add(odbSvLoad.svload4.data);
                    totalMachineLoad.Add(odbSvLoad.svload5.data);
                    totalMachineLoad.Add(odbSvLoad.svload6.data);
                    totalMachineLoad.Add(odbSvLoad.svload7.data);
                    totalMachineLoad.Add(odbSvLoad.svload8.data);

                    for(int i = 0; i < axisCount; i++)
                    {
                        base.SetDictionary(base.AXIS_LOAD, i, totalMachineLoad[i]);

                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSvMeter(AxisNo = {2}) Success :: AXIS LOAD = {1}", this.ToString(), base.AXIS_LOAD[i], i));
                    }
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSvMeter() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSvMeter() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSvMeter() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Spindle Loadmeter Data (MachineSpindleLoad)
        /// </summary>
        private void CallRdSpMeter()
        {
            try
            {
                Focas1.ODBSPLOAD odbSpLoad = new Focas1.ODBSPLOAD();
                short spindleCount = 1;

                focasReturn = Focas1.cnc_rdspmeter(fanucHandle, 0, ref spindleCount, odbSpLoad);

                if (focasReturn == EW_OK)
                {
                    base.SetDictionary(base.SPINDLE_LOAD, 0, odbSpLoad.spload1.spload.data);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSpMeter() Success :: SPINDLE LOAD = {1}", this.ToString(), base.SPINDLE_LOAD[0]));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSpMeter() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSpMeter() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdSpMeter() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Parameter Data
        /// </summary>
        /// <param name="paramNo">Parameter No</param>
        private void CallRdParam(short paramNo)
        {
            try
            {
                Focas1.IODBPSD_1 iOdbPsd = new Focas1.IODBPSD_1();

                focasReturn = Focas1.cnc_rdparam(fanucHandle, paramNo, 0, 8, iOdbPsd);

                if (focasReturn == EW_OK)
                {
                    base.SetDictionary(base.PARAM_VALUE, paramNo, iOdbPsd.ldata);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdParam(ParamNo = {2}) Success :: VALUE = {1}", this.ToString(), base.PARAM_VALUE[paramNo], paramNo));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdParam(ParamNo = {1}) Fail :: SocketError", this.ToString(), paramNo));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdParam(ParamNo = {2}) Fail :: ReturnCode = {1}", this.ToString(), focasReturn, paramNo));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdParam(ParamNo = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, paramNo));
            }
        }

        /// <summary>
        /// Read Machine System Macro Data
        /// </summary>
        /// <param name="macroNo">System Macro No</param>
        private void CallRdMacro(short macroNo)
        {
            try
            {
                Focas1.ODBM odbM = new Focas1.ODBM();

                focasReturn = Focas1.cnc_rdmacro(fanucHandle, macroNo, 10, odbM);

                if (focasReturn == EW_OK)
                {
                    base.SetDictionary(base.MACRO_VALUE, macroNo, odbM.mcr_val / Math.Pow((double)10, (double)odbM.dec_val));

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdMacro(MacroNo = {2}) Success :: VALUE = {1}", this.ToString(), base.MACRO_VALUE[macroNo], macroNo));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdMacro(MacroNo = {1}) Fail :: SocketError", this.ToString(), macroNo));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdMacro(MacroNo = {2}) Fail :: ReturnCode = {1}", this.ToString(), focasReturn, macroNo));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdMacro(MacroNo = {2}) Exception :: Message = {1}", this.ToString(), ex.Message, macroNo));
            }
        }

        /// <summary>
        /// Read Machine Program No Data (MachineMainProgramNo, MachineCurrentProgramNo)
        /// </summary>
        private void CallRdPrgNum()
        {
            try
            {
                Focas1.ODBPRO odbPro = new Focas1.ODBPRO();

                focasReturn = Focas1.cnc_rdprgnum(fanucHandle, odbPro);

                if (focasReturn == EW_OK)
                {
                    base.MAIN_PRG_NO = odbPro.mdata;
                    base.CURRENT_PRG_NO = odbPro.data;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdPrgNum() Success :: MAIN PRG NO = {1}", this.ToString(), base.MAIN_PRG_NO));
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdPrgNum() Success :: CURRENT PRG NO = {1}", this.ToString(), base.CURRENT_PRG_NO));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdPrgNum() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdPrgNum() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallRdPrgNum() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine Program Name Data (MachineCurrentProgramName)
        /// </summary>
        private void CallExePrgName()
        {
            try
            {
                Focas1.ODBEXEPRG odbExePrg = new Focas1.ODBEXEPRG();

                focasReturn = Focas1.cnc_exeprgname(fanucHandle, odbExePrg);

                if (focasReturn == EW_OK)
                {
                    base.CURRENT_PRG_NAME = new string(odbExePrg.name);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallExePrgName() Success :: CURRENT PRG NAME = {1}", this.ToString(), base.CURRENT_PRG_NAME));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallExePrgName() Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallExePrgName() Fail :: ReturnCode = {1}", this.ToString(), focasReturn));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallExePrgName() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Read Machine NC Data Using NC Program No
        /// </summary>
        /// <param name="programNo">NC Program No</param>
        /// <returns></returns>
        private string ReadNCData(int programNo)
        {
            string strNcData = string.Empty;

            focasReturn = Focas1.cnc_upstart3(fanucHandle, 0, programNo, programNo);

            if (focasReturn == EW_OK)
            {
                char[] ncData = new char[2048];
                int ncDataLength = ncData.Length;

                do
                {
                    focasReturn = Focas1.cnc_upload3(fanucHandle, ref ncDataLength, ncData);
                } while (focasReturn == EW_BUFFER);

                if (focasReturn == EW_OK)
                {
                    strNcData = new string(ncData);
                    strNcData = strNcData.Replace("\u0000", string.Empty);
                    strNcData = strNcData.Replace("\r\n", "\n");
                    strNcData = strNcData.Replace("\n", string.Empty);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Success :: MAIN PRG NC DATA = {1}", this.ToString(), strNcData));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: Upload3 Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: Upload3 Fail :: ErrorCode = {1}", this.ToString(), focasReturn));
                }

                focasReturn = Focas1.cnc_upend3(fanucHandle);

                if (focasReturn == EW_OK)
                {
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadEnd3 Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadEnd3 Fail :: ErrorCode = {1}", this.ToString(), focasReturn));
                }
            }
            else if (focasReturn == EW_SOCKET)
            {
                base.isConnect = false;
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadStart3 Fail :: SocketError", this.ToString()));
            }
            else
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadStart3 Fail :: ErrorCode = {1}", this.ToString(), focasReturn));
            }

            return strNcData;
        }

        /// <summary>
        /// Read Machine NC Data Using NC Program Name (Only MachineType = 0, 30, 31, 32)
        /// </summary>
        /// <param name="programName">NC Program Name</param>
        /// <returns>NC Program</returns>
        private string ReadNCData(string programName)
        {
            string strNcData = string.Empty;

            focasReturn = Focas1.cnc_upstart4(fanucHandle, 0, programName);

            if (focasReturn == EW_OK)
            {
                char[] ncData = new char[2048];
                int ncDataLength = ncData.Length;

                do
                {
                    focasReturn = Focas1.cnc_upload4(fanucHandle, ref ncDataLength, ncData);
                } while (focasReturn == EW_BUFFER);

                if (focasReturn == EW_OK)
                {
                    strNcData = new string(ncData);
                    strNcData = strNcData.Replace("\u0000", string.Empty);
                    strNcData = strNcData.Replace("\r\n", "\n");
                    strNcData = strNcData.Replace("\n", string.Empty);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Success :: CURRENT PRG NC DATA = {1}", this.ToString(), strNcData));
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: Upload4 Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: Upload4 Fail :: ErrorCode = {1}", this.ToString(), focasReturn));
                }

                focasReturn = Focas1.cnc_upend4(fanucHandle);

                if (focasReturn == EW_OK)
                {
                }
                else if (focasReturn == EW_SOCKET)
                {
                    base.isConnect = false;
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadEnd4 Fail :: SocketError", this.ToString()));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadEnd4 Fail :: ErrorCode = {1}", this.ToString(), focasReturn));
                }
            }
            else if (focasReturn == EW_SOCKET)
            {
                base.isConnect = false;
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadStart4 Fail :: SocketError", this.ToString()));
            }
            else
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Fail :: UploadStart4 Fail :: ErrorCode = {1}", this.ToString(), focasReturn));
            }

            return strNcData;
        }
        #endregion

        #region Method
        /// <summary>
        /// Convert Code To Description
        /// </summary>
        /// <param name="cncType">MachineType</param>
        /// <param name="type">CodeType</param>
        /// <param name="value">Code</param>
        /// <returns>Description</returns>
        private string GetDescription(int cncType, string type, string value)
        {
            string description = string.Empty;

            if (!value.Equals(string.Empty))
            {
                if (type.Equals("MODE"))
                {
                    if (cncType == 15)
                    {
                        switch (value)
                        {
                            case "0":
                                description = "****(No selection)";
                                break;
                            case "1":
                                description = "MDI";
                                break;
                            case "2":
                                description = "DNC";
                                break;
                            case "3":
                                description = "MEMory";
                                break;
                            case "4":
                                description = "EDIT";
                                break;
                            case "5":
                                description = "TeacH IN";
                                break;
                        }
                    }
                    else
                    {
                        switch (value)
                        {
                            case "0":
                                description = "MDI";
                                break;
                            case "1":
                                description = "MEMory";
                                break;
                            case "2":
                                description = "****";
                                break;
                            case "3":
                                description = "EDIT";
                                break;
                            case "4":
                                description = "HaNDle";
                                break;
                            case "5":
                                description = "JOG";
                                break;
                            case "6":
                                description = "Teach in JOG";
                                break;
                            case "7":
                                description = "Teach in HaNDle";
                                break;
                            case "8":
                                description = "INC·feed";
                                break;
                            case "9":
                                description = "REFerence";
                                break;
                            case "10":
                                description = "ReMoTe";
                                break;
                        }
                    }
                }

                else if (type.Equals("STATUS"))
                {
                    if (cncType == 15)
                    {
                        switch (value)
                        {
                            case "0":
                                description = "STOP";
                                break;
                            case "1":
                                description = "HOLD";
                                break;
                            case "2":
                                description = "STaRT";
                                break;
                            case "3":
                                description = "MSTR";
                                break;
                            case "4":
                                description = "ReSTaRt";
                                break;
                            case "5":
                                description = "PRSR";
                                break;
                            case "6":
                                description = "NSRC";
                                break;
                            case "7":
                                description = "ReSTaRt";
                                break;
                            case "8":
                                description = "ReSET";
                                break;
                            case "13":
                                description = "HPCC";
                                break;
                        }
                    }
                    else
                    {
                        switch (value)
                        {
                            case "0":
                                description = "****(reset)";
                                break;
                            case "1":
                                description = "STOP";
                                break;
                            case "2":
                                description = "HOLD";
                                break;
                            case "3":
                                description = "STaRT";
                                break;
                            case "4":
                                description = "MSTR";
                                break;
                        }
                    }
                }

                else if (type.Equals("ALARM"))
                {
                    switch (value)
                    {
                        case "0":
                            description = "(No alarm)";
                            break;
                        case "1":
                            description = "ALarM";
                            break;
                    }
                }

                else if (type.Equals("ALARM_TYPE"))
                {
                    if (cncType == 15)
                    {
                        switch (value)
                        {
                            case "0":
                                description = "BG";
                                break;
                            case "1":
                                description = "PS";
                                break;
                            case "2":
                                description = "OH";
                                break;
                            case "3":
                                description = "SB";
                                break;
                            case "4":
                                description = "SN";
                                break;
                            case "5":
                                description = "SW";
                                break;
                            case "6":
                                description = "OT";
                                break;
                            case "7":
                                description = "PC";
                                break;
                            case "8":
                                description = "EX";
                                break;
                            case "10":
                                description = "SR";
                                break;
                            case "12":
                                description = "SV";
                                break;
                            case "13":
                                description = "IO";
                                break;
                            case "14":
                                description = "PW";
                                break;
                            case "16":
                                description = "EX";
                                break;
                            case "17":
                                description = "EX";
                                break;
                            case "18":
                                description = "EX";
                                break;
                            case "19":
                                description = "MC";
                                break;
                            case "20":
                                description = "SP";
                                break;
                        }
                    }
                    else if (cncType == 16 || cncType == 18 || cncType == 21)
                    {
                        switch (value)
                        {
                            case "0":
                                description = "P/S100";
                                break;
                            case "1":
                                description = "P/S000";
                                break;
                            case "2":
                                description = "P/S101";
                                break;
                            case "3":
                                description = "P/S";
                                break;
                            case "4":
                                description = "Overtravel alarm ";
                                break;
                            case "5":
                                description = "Overheat alarm";
                                break;
                            case "6":
                                description = "Servo alarm";
                                break;
                            case "8":
                                description = "APC alarm";
                                break;
                            case "9":
                                description = "Spindle alarm";
                                break;
                            case "10":
                                description = "P/S alarm";
                                break;
                            case "11":
                                description = "Laser alarm";
                                break;
                            case "13":
                                description = "Rigid tap alarm";
                                break;
                            case "15":
                                description = "External alarm message";
                                break;
                        }
                    }
                    else
                    {
                        switch (value)
                        {
                            case "0":
                                description = "SW";
                                break;
                            case "1":
                                description = "PW";
                                break;
                            case "2":
                                description = "IO";
                                break;
                            case "3":
                                description = "PS";
                                break;
                            case "4":
                                description = "OT";
                                break;
                            case "5":
                                description = "OH";
                                break;
                            case "6":
                                description = "SV";
                                break;
                            case "7":
                                description = "SR";
                                break;
                            case "8":
                                description = "MC";
                                break;
                            case "9":
                                description = "SP";
                                break;
                            case "10":
                                description = "DS";
                                break;
                            case "11":
                                description = "IE";
                                break;
                            case "12":
                                description = "BG";
                                break;
                            case "13":
                                description = "SN";
                                break;
                            case "15":
                                description = "EX";
                                break;
                            case "19":
                                description = "PC";
                                break;
                        }
                    }
                }
            }

            return description;
        }
        #endregion
    }
}
