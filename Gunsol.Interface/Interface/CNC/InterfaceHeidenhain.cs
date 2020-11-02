using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using HeidenhainDNCLib;
using Gunsol.Common;
using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Protocol;
using Gunsol.Interface.Model;


namespace Gunsol.Interface.Interface.CNC
{
    public class InterfaceHeidenhain : InterfaceCNCBase
    {
        #region Propery
        private JHMachine heidenhainHandle = null;  // 라이브러리 핸들
        private int machineType;                    // 컨트롤러 버전
        private int machineAxesCount;               // 전체 축의 개수
        #endregion

        #region Constructor
        /// <summary>
        /// Property 초기화
        /// </summary>
        /// <param name="commInfo">통신 정보를 저장하는 CommInfo Object</param>
        public InterfaceHeidenhain(CommInfo commInfo)
        {
            try
            {
                base.division = "CNC";
                base.commInfo = commInfo;
                base.isConnect = false;
                //base.sendData = new Dictionary<string, object>();
                //base.receiveData = new Dictionary<string, object>();

                base.SPINDLE = new Dictionary<object, object>();
                base.SPINDLE_LOAD = new Dictionary<object, object>();
                base.AXIS_ABS = new Dictionary<object, object>();
                base.AXIS_LOAD = new Dictionary<object, object>();                
                base.MODAL_VALUE = new Dictionary<object, object>();
                base.PARAM_VALUE = new Dictionary<object, object>();
                base.MACRO_VALUE = new Dictionary<object, object>();
                base.PLC_VALUE = new Dictionary<object, object>();

                this.heidenhainHandle = new JHMachine();
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
                    if (CallAddConnection())
                    {
                        heidenhainHandle.Connect(commInfo.machineCode);

                        if (heidenhainHandle.connected)
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
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Fail :: Add Connection List Fail", this.ToString()));

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
        /// DisConnect Machine
        /// </summary>
        public override void DisConnect()
        {
            try
            {
                if (base.isConnect)
                {
                    heidenhainHandle.Disconnect();

                    LogHandler.WriteLog(base.division, string.Format("{0} :: DisConnect() Success", this.ToString()));

                    base.isConnect = false;
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
                CallAutomaticData();
                CallProcessData();
                CallErrorData();
                CallFileSystemData();
                CallDataAccess(@"\PLC\memory\W\14");
                CallDataAccess(@"\PLC\memory\W\260");
                CallDataAccess(@"\PLC\memory\W\322");
                CallDataAccess(@"\PLC\memory\W\7382");
                CallDataAccess(@"\PLC\memory\W\7380");
                for (int i = 0; i < machineAxesCount; i++)
                {
                    CallDataAccess(string.Format(@"\PLC\memory\W\{0}", 7384 + (i * 2)));
                }

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
            try
            {
                base.M_CODE = Convert.ToInt32(PLC_VALUE[@"\PLC\memory\W\260"]);
                base.T_CODE = Convert.ToInt32(PLC_VALUE[@"\PLC\memory\W\14"]);

                base.SetDictionary(base.SPINDLE, 0, PLC_VALUE[@"\PLC\memory\W\322"]);
                base.SetDictionary(base.SPINDLE_LOAD, 0, PLC_VALUE[@"\PLC\memory\W\7382"]);

                for (int i = 0; i < machineAxesCount; i++)
                {
                    base.SetDictionary(base.AXIS_LOAD, i, PLC_VALUE[string.Format(@"\PLC\memory\W\{0}", 7384 + (i * 2))]);
                }

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
        /// Add Connection Object To IJHConnectionList
        /// </summary>
        /// <returns>Is Success</returns>
        private bool CallAddConnection()
        {
            IJHConnectionList ijhConnectionList = null;
            IJHConnection ijhConnection = null;
            bool isSuccess = false;

            try
            {
                ijhConnectionList = (IJHConnectionList)heidenhainHandle.ListConnections();

                if (ijhConnectionList != null)
                {
                    List<IJHConnection> heidenhainCastList = ijhConnectionList.Cast<IJHConnection>().ToList();

                    ijhConnection = ijhConnectionList.NewConnection(DNC_CNC_TYPE.DNC_CNC_TYPE_ITNC, DNC_PROTOCOL.DNC_PROT_TCPIP);

                    if (ijhConnection != null)
                    {
                        ijhConnection.name = commInfo.machineCode;
                        ijhConnection.ConnectionProperty[DNC_CONNECTION_PROPERTY.DNC_CP_HOST].value = commInfo.machineIp;
                        ijhConnection.ConnectionProperty[DNC_CONNECTION_PROPERTY.DNC_CP_PORT].value = commInfo.machinePort;

                        if (!heidenhainCastList.Exists(p => p.name.Equals(ijhConnection.name)))
                        {
                            ijhConnectionList.AddConnection(ijhConnection);

                            LogHandler.WriteLog(base.division, string.Format("{0} :: AddConnection() Success :: NAME = {1}", this.ToString(), ijhConnection.name));

                            isSuccess = true;
                        }
                        else
                        {
                            LogHandler.WriteLog(base.division, string.Format("{0} :: AddConnection() Success :: Already Exists", this.ToString()));

                            isSuccess = true;
                        }
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: AddConnection() Fail :: Create IJHConnection Fail", this.ToString()));

                        isSuccess = false;
                    }
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: AddConnection() Fail :: Create IJHConnectionList Fail", this.ToString()));

                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: AddConnection() Exception :: Message = {1}", this.ToString(), ex.Message));

                isSuccess = false;
            }
            finally
            {
                if (ijhConnection != null)
                {
                    Marshal.ReleaseComObject(ijhConnection);
                }

                if (ijhConnectionList != null)
                {
                    Marshal.ReleaseComObject(ijhConnectionList);
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Read Machine Automatic Data (MachineMode, MachineStatus, MachineCurrentPosition, MachineMainProgramPath/Name)
        /// </summary>
        private void CallAutomaticData()
        {
            JHAutomatic jhAutomatic = null;
            JHCutterLocationList jhCutterLocationList = null;

            try
            {
                jhAutomatic = (JHAutomatic)heidenhainHandle.GetInterface(DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAUTOMATIC);

                if (jhAutomatic != null)
                {
                    object selectedProgram = null;

                    base.MODE = jhAutomatic.GetExecutionMode().ToString();
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData.GetExecutionMode() Success :: MODE = {1}", this.ToString(), base.MODE));

                    base.STATUS = jhAutomatic.GetProgramStatus().ToString();
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData.GetProgramStatus() Success :: STATUS = {1}", this.ToString(), base.STATUS));

                    jhCutterLocationList = jhAutomatic.GetCutterLocation(0);
                    machineAxesCount = jhCutterLocationList.Count;

                    for (int i = 0; i < jhCutterLocationList.Count; i++)
                    {
                        base.SetDictionary(base.AXIS_ABS, i, jhCutterLocationList[i].dPosition);
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData.GetCutterLocation(AxisNo = {1}) Success :: ABS = {2}", this.ToString(), i, base.AXIS_ABS[i]));
                    }

                    jhAutomatic.GetExecutionPoint(ref selectedProgram);
                    base.MAIN_PRG_PATH = selectedProgram.ToString();
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData.GetExecutionPoint() Success :: MAIN PRG PATH = {1}", this.ToString(), base.MAIN_PRG_PATH));

                    if (base.MAIN_PRG_PATH.IndexOf("\\") >= 0)
                    {
                        base.MAIN_PRG_NAME = base.MAIN_PRG_PATH.Split('\\')[base.MAIN_PRG_PATH.Split('\\').Length - 1];
                    }
                    else
                    {
                        base.MAIN_PRG_NAME = base.MAIN_PRG_PATH;
                    }

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData.GetExecutionPoint() Success :: MAIN PRG NAME = {1}", this.ToString(), base.MAIN_PRG_NAME));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData() Fail :: Create JHAutomatic Fail", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallAutomaticData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
            finally
            {
                if (jhCutterLocationList != null)
                {
                    Marshal.ReleaseComObject(jhCutterLocationList);
                }

                if (jhAutomatic != null)
                {
                    Marshal.ReleaseComObject(jhAutomatic);
                }
            }
        }

        /// <summary>
        /// Read Machine Process Data (MachineNCUpTime(Min), MachineUpTime, MachineRunningTime, MachineSpindleRunningTime)
        /// </summary>
        private void CallProcessData()
        {
            JHProcessData jhProcessData = null;

            try
            {
                jhProcessData = (JHProcessData)heidenhainHandle.GetInterface(DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHPROCESSDATA);

                if (jhProcessData != null)
                {
                    object pHours = null;
                    object pMinutes = null;

                    jhProcessData.GetNcUpTime(ref pHours, ref pMinutes);
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData.GetNcUpTime() Success :: NC UP TIME = {1}", this.ToString(), Convert.ToInt32(pHours) * 60 + Convert.ToInt32(pMinutes)));

                    jhProcessData.GetMachineUpTime(ref pHours, ref pMinutes);
                    base.POWER_T = Convert.ToInt32(pHours) * 60 + Convert.ToInt32(pMinutes);
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData.GetMachineUpTime() Success :: MACHINE UP TIME = {1}", this.ToString(), base.POWER_T));

                    jhProcessData.GetMachineRunningTime(ref pHours, ref pMinutes);
                    base.OPERATION_T = Convert.ToInt32(pHours) * 60 + Convert.ToInt32(pMinutes);
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData.GetMachineRunningTime() Success :: MACHINE RUNNING TIME = {1}", this.ToString(), base.OPERATION_T));

                    jhProcessData.GetSpindleRunningTime(0, ref pHours, ref pMinutes);
                    base.CUTTING_T = Convert.ToInt32(pHours) * 60 + Convert.ToInt32(pMinutes);
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData.GetSpindleRunningTime() Success :: SPINDLE RUNNING TIME = {1}", this.ToString(), base.CUTTING_T));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData() Fail :: Create JHProcessData Fail", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
            finally
            {
                if (jhProcessData != null)
                {
                    Marshal.ReleaseComObject(jhProcessData);
                }
            }
        }

        /// <summary>
        /// Read Machine Error Data (MachineAlarmStatus, MachineAlarmType, MachineAlarmNo, MachineAlarmMsg)
        /// </summary>
        private void CallErrorData()
        {
            JHError jhError = null;

            try
            {
                jhError = (JHError)heidenhainHandle.GetInterface(DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHERROR);

                if (jhError != null)
                {
                    object pErrorGroup = null;
                    object pErrorNumber = null;
                    object pErrorClass = null;
                    object pErrorText = null;
                    object pErrorChannel = null;

                    jhError.GetFirstError(ref pErrorGroup, ref pErrorNumber, ref pErrorClass, ref pErrorText, ref pErrorChannel);

                    if (pErrorText.ToString().IndexOf("<No error available>") < 0 && !pErrorText.ToString().Equals(string.Empty))
                    {
                        base.ALARM = "ALarM";
                        base.ALARM_TYPE = pErrorGroup.ToString();
                        base.ALARM_NO = Convert.ToInt32(pErrorNumber);
                        base.ALARM_MSG = pErrorText.ToString();

                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallErrorData() Success :: ALARM = {1}", this.ToString(), base.ALARM));
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallErrorData() Success :: ALARM TYPE = {1}", this.ToString(), base.ALARM_TYPE));
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallErrorData() Success :: ALARM NO = {1}", this.ToString(), base.ALARM_NO));
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallErrorData() Success :: ALARM MSG = {1}", this.ToString(), base.ALARM_MSG));
                    }
                    else
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: CallErrorData() Success :: ALARM = No Alarm", this.ToString()));
                    }
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallErrorData() Fail :: Create JHError Fail", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallProcessData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
            finally
            {
                if (jhError != null)
                {
                    Marshal.ReleaseComObject(jhError);
                }
            }
        }

        /// <summary>
        /// Read Machine File System Data (Download MachineMainNCData)
        /// </summary>
        private void CallFileSystemData()
        {
            JHFileSystem jhFileSystem = null;

            try
            {
                jhFileSystem = (JHFileSystem)heidenhainHandle.GetInterface(DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHFILESYSTEM);

                if (jhFileSystem != null)
                {
                    string localPath = string.Format(@"{0}\{1}", System.Windows.Forms.Application.StartupPath, base.MAIN_PRG_PATH);

                    jhFileSystem.SetAccessMode(DNC_ACCESS_MODE.DNC_ACCESS_MODE_DEFAULT, "");
                    jhFileSystem.ReceiveFile(base.MAIN_PRG_PATH, localPath);

                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallFileSystemData() Success :: PATH = {1}", this.ToString(), localPath));
                }
                else
                {
                    LogHandler.WriteLog(base.division, string.Format("{0} :: CallFileSystemData() Fail :: Create JHFileSystem Fail", this.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallFileSystemData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
            finally
            {
                if (jhFileSystem != null)
                {
                    Marshal.ReleaseComObject(jhFileSystem);
                }
            }
        }

        /// <summary>
        /// Read Machine PLC/Parameter Data
        /// </summary>
        /// <param name="dataPath">PLC/Parameter Path</param>
        private void CallDataAccess(string dataPath)
        {
            IJHDataAccess2 ijhDataAccess2 = null;
            IJHDataEntryPropertyList ijhDataEntryPropertyList = null;
            IJHDataEntry ijhDataEntry = null;

            try
            {
                ijhDataAccess2 = (IJHDataAccess2)heidenhainHandle.GetInterface(DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHDATAACCESS);

                if (ijhDataAccess2 != null)
                {
                    ijhDataEntry = ijhDataAccess2.GetDataEntry(dataPath);
                    ijhDataEntryPropertyList = ijhDataEntry.propertyList;

                    foreach (IJHDataEntryProperty p in ijhDataEntryPropertyList)
                    {
                        if (p.kind == DNC_DATAENTRY_PROPKIND.DNC_DATAENTRY_PROPKIND_DATA)
                        {
                            base.SetDictionary(base.PLC_VALUE, dataPath, p.varValue);
                            LogHandler.WriteLog(base.division, string.Format("{0} :: CallDataAccess(Path = {1}) Success :: VALUE = {2}", this.ToString(), dataPath, p.varValue));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: CallDataAccess() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
            finally
            {
                if (ijhDataEntry != null)
                {
                    Marshal.ReleaseComObject(ijhDataEntry);
                }

                if (ijhDataEntryPropertyList != null)
                {
                    Marshal.ReleaseComObject(ijhDataEntryPropertyList);
                }

                if (ijhDataAccess2 != null)
                {
                    Marshal.ReleaseComObject(ijhDataAccess2);
                }
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Read Machine NC Data Using Download NC Program File
        /// </summary>
        /// <param name="ncDataPath">NC Program File Local Path</param>
        /// <returns></returns>
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
            catch(Exception ex)
            {
                LogHandler.WriteLog(base.division, string.Format("{0} :: ReadNCData() Exception :: Message = {1}", this.ToString(), ex.Message));
            }

            return strNcData;
        }
        #endregion
    }
}
