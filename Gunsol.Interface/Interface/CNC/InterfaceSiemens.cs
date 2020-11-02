using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Gunsol.Common.DBMS;
using Gunsol.Common.File;
using Gunsol.Common.Model;
using Gunsol.Common.Protocol;
using Gunsol.Interface.Model;


namespace Gunsol.Interface.Interface.CNC
{
    public class InterfaceSiemens : InterfaceCNCBase
    {
        #region Propery
        private const string DDE_SERVER_PATH_1 = @"C:\Siemens\Sinumerik\StartUp-Tool\mmc2\ncdde.exe";       // DDE Server 경로 1
        private const string DDE_SERVER_PATH_2 = @"C:\Siemens\Sinumerik\StartUp-Tool\mmc2\cp_840di.exe";    // DDE Server 경로 2
        private const string DDE_SERVICE = "ncdde";                                                         // DDE Service
        private const string DDE_TOPIC = "NCU840D";                                                         // DDE Topic
        private DdeHandler siemensHandle;                                                                   // 라이브러리 핸들
        private int machineType;                                                                            // 컨트롤러 버전
        private int machineAxesCount;                                                                       // 설비 축 개수
        #endregion

        #region Constructor
        public InterfaceSiemens(CommInfo commInfo)
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

                if (CallDDEServer())
                {
                    DdeInfo ddeInfo = new DdeInfo(DDE_SERVICE, DDE_TOPIC);

                    siemensHandle = new DdeHandler(ddeInfo);
                }
                else
                {
                    siemensHandle = null;
                }
            }
            catch (Exception ex)
            {
                LogHandler.PrintLog(string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
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
                    if (siemensHandle == null)
                    {
                        LogHandler.WriteLog(base.division, string.Format("{0} :: Connect() Fail :: DDE Server Not Execute", this.ToString()));

                        base.isConnect = false;
                    }
                    else
                    {
                        if (siemensHandle.Connect())
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
                    siemensHandle.DisConnect();

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
                string ddeValue = string.Empty;

                if (siemensHandle.Request("/Channel/Configuration/numActAxes", ref ddeValue))
                {
                    machineAxesCount = Convert.ToInt32(ddeValue.Equals(string.Empty) ? "0" : ddeValue);
                }

                //base.MODE = siemensHandle.Request("/Bag/State/opMode");
                //base.STATUS = siemensHandle.Request("/Channel/State/progStatus");
                //base.ALARM = siemensHandle.Request("/Channel/State/chanAlarm");
                //base.ALARM_NO = Convert.ToInt32(siemensHandle.Request("/Nck/SequencedAlarms/textIndex").Equals(string.Empty) ? "0" : siemensHandle.Request("/Nck/SequencedAlarms/textIndex"));
                //base.FEED = Convert.ToInt32(siemensHandle.Request("/Channel/State/actFeedRateIpo").Equals(string.Empty) ? "0" : siemensHandle.Request("/Channel/State/actFeedRateIpo"));
                //base.SPINDLE.Add(0, Convert.ToInt32(siemensHandle.Request("/Nck/Spindle/actSpeed").Equals(string.Empty) ? "0" : siemensHandle.Request("/Nck/Spindle/actSpeed")));
                //base.M_CODE = Convert.ToInt32(siemensHandle.Request("/Channel/SelectedFunctions/Mval").Equals(string.Empty) ? "0" : siemensHandle.Request("/Channel/SelectedFunctions/Mval"));
                //base.T_CODE = Convert.ToInt32(siemensHandle.Request("/Channel/State/actTNumber").Equals(string.Empty) ? "0" : siemensHandle.Request("/Channel/State/actTNumber"));
                //base.PART_CNT = Convert.ToInt32(siemensHandle.Request("/Channel/State/totalParts").Equals(string.Empty) ? "0" : siemensHandle.Request("/Channel/State/totalParts"));
                //for (int i = 0; i < machineAxesCount; i++)
                //{
                //    base.AXIS_ABS.Add(i, siemensHandle.Request(string.Format("/Channel/MachineAxis/actToolBasePos[u1, {0}]", i)));
                //    base.AXIS_LOAD.Add(i, siemensHandle.Request(string.Format("/Channel/MachineAxis/vaLoad[u1, {0}]", i)));
                //}
                //base.SPINDLE_LOAD.Add(0, Convert.ToInt32(siemensHandle.Request("/Channel/Spindle/driveLoad").Equals(string.Empty) ? "0" : siemensHandle.Request("/Channel/Spindle/driveLoad")));
                //base.CURRENT_PRG_NAME = siemensHandle.Request("/Channel/ProgramInfo/progName");
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

        /// <summary>
        /// Call DDE Server
        /// </summary>
        /// <returns>IsSuccess</returns>
        private bool CallDDEServer()
        {
            bool isSuccess = false;

            try
            {
                if (CallProcess(DDE_SERVER_PATH_1))
                {
                    LogHandler.PrintLog(string.Format("{0} :: CallDDEServer() Success :: Path = {1}", this.ToString(), DDE_SERVER_PATH_1));

                    if (CallProcess(DDE_SERVER_PATH_2))
                    {
                        LogHandler.PrintLog(string.Format("{0} :: CallDDEServer() Success :: Path = {1}", this.ToString(), DDE_SERVER_PATH_2));

                        isSuccess = true;
                    }
                    else
                    {
                        LogHandler.PrintLog(string.Format("{0} :: CallDDEServer() Fail :: Path = {1}", this.ToString(), DDE_SERVER_PATH_2));

                        isSuccess = false;
                    }
                }
                else
                {
                    LogHandler.PrintLog(string.Format("{0} :: CallDDEServer() Fail :: Path = {1}", this.ToString(), DDE_SERVER_PATH_1));

                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LogHandler.PrintLog(string.Format("{0} :: CallDDEServer() Exception :: Message = {1}", this.ToString(), ex.Message));

                isSuccess = false;
            }

            return isSuccess;
        }

        /// <summary>
        /// Call External Process
        /// </summary>
        /// <param name="processPath">Process Path</param>
        /// <returns>IsSuccess</returns>
        private bool CallProcess(string processPath)
        {
            bool isSuccess = false;

            try
            {
                List<Process> currentProcessList = Process.GetProcesses().ToList();

                if (System.IO.File.Exists(processPath))
                {
                    if (!currentProcessList.Exists(p => p.StartInfo.FileName.Equals(processPath)))
                    {
                        ProcessStartInfo startInfo1 = new ProcessStartInfo();
                        startInfo1.FileName = processPath;

                        Process.Start(startInfo1);

                        LogHandler.PrintLog(string.Format("{0} :: CallProcess(ProcessPath = {1}) Success", this.ToString(), processPath));
                    }
                    else
                    {
                        LogHandler.PrintLog(string.Format("{0} :: CallProcess(ProcessPath = {1}) Already Executing", this.ToString(), processPath));
                    }

                    isSuccess = true;
                }
                else
                {
                    LogHandler.PrintLog(string.Format("{0} :: CallProcess(ProcessPath = {1}) Fail :: File Not Exist", this.ToString(), processPath));

                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LogHandler.PrintLog(string.Format("{0} :: CallProcess(ProcessPath = {1}) Exception :: Message = {2}", this.ToString(), processPath, ex.Message));

                isSuccess = false;
            }

            return isSuccess;
        }
        #endregion
    }
}
