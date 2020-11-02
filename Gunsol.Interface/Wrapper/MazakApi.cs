using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public class MazakApi
{
    #region API Const
    public const int MAZ_AXISMAX = 16;
    public const int MAZ_ALMMAX = 24;
    #endregion

    #region API Enum
    public enum MazError
    {
        MAZERR_OK = 0,

        MAZERR_SOCK = -10,

        MAZERR_HNDL = -20,
        MAZERR_CLIMAX = -21,
        MAZERR_SERVERMAX = -22,

        MAZERR_BUSY = -40,

        MAZERR_RUNNING = -50,
        MAZERR_OVER = -51,
        MAZERR_NONE = -52,
        MAZERR_TYPE = -53,
        MAZERR_EDIT = -54,
        MAZERR_PROSIZE = -55,
        MAZERR_PRONUM = -56,
        MAZERR_RESTARTSEACH = -57,
        MAZERR_RUNMODE = -58,
        MAZERR_DISPLAY = -59,

        MAZERR_ARG = -60,
        MAZERR_VALUE = -61,
        MAZERR_OPTION = -62,
        MAZERR_INTERFERING = -63,
        MAZERR_SET_TDATA = -64,
        MAZERR_MEM_CAP_EXC = -65,
        MAZERR_FILE_SIZE = -66,

        MAZERR_SYS = -70,

        MAZERR_FUNC = -80,

        MAZERR_TIMEOUT = -90,

        MAZERR_AXIS = -100,

        MAZERR_CERTIFY = -120
    }

    public enum MazMode
    {
        //_     : Space
        //__    : /

        No_operation_mode = 0,
        Automatic_operation_mode = 1,
        Tape_operation_mode = 2,
        Rapid_traverse_mode = 3,
        Home_return_mode = 4,
        Cutting_feed__manual_pulse_mode = 5,
        MDI_operation_mode = 6,
    }

    public enum MazStatus
    {
        //_     : Space
        //__    : /

        Automatic_operation_has_not_started = 0,
        Running_in_automatic_operation = 1,
        Stopping_due_to_an_alarm = 2,
        Stopping_in_automatic_operation = 3
    }

    public enum MazAlarm
    {
        //_     : Space
        //__    : /

        Warning = 0,
        Alarm = 1
    }

    public enum MazProg
    {
        //_     : Space
        //__    : /

        Not_selected = 0,
        EIA__ISO = 1,
        MAZATROL = 2
    }
    #endregion

    #region API Structure (Related To Connection)
    public struct MAZ_CERTIFY
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string vendorCode;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string dummy;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string dummy2;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string userPasswd;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string dummy3;
    }
    #endregion

    #region API Structure (Related To Position)
    public struct MAZ_AXISNAME
    {
        public char[] status;
        public string[] axisname;
    }

    public struct MAZ_NCPOS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAZ_AXISMAX)]
        public byte[] status;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAZ_AXISMAX)]
        public int[] data;
    }

    public struct MAZ_ONE_POS_INFO
    {
        public char status;
        public char[] dummy;
        public long curPos;
        public long macPos;
        public long buffer;
        public long remain;
    }

    public struct MAZ_POSITION_INFO
    {
        public MAZ_ONE_POS_INFO[] data;
    }

    public struct MAZ_PROINFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
        public char[] wno;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public char[] dummy;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 49)]
        public char[] comment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public char[] dummy2;
        public char type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public char[] dummy3;
    }

    public struct MAZ_PROINFO2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
        public char[] wno;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public char[] dummy;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 49)]
        public char[] comment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public char[] dummy2;
        public char type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public char[] dummy3;
        public long uno;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy4;
        public long sno;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy5;
        public long bno;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy6;
    }

    public struct MAZ_MODAL_INFO
    {
        public char sts;
        public char dummy1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public char[] aux;
        public long modalS;
        public long modalF;
        public long modalM;
        public long modalAux;
        public long dummy2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public ushort[] modalG;
    }

    public struct MAZ_WNOTIME
    {
        public char dataver;
        public string dummy1;
        public string wno;
        public string dummy2;
        public short[] starttime;
        public ushort wnotime;
        public ushort g0time;
        public ushort g1time;
        public long[] dummy3;
    }

    public struct MAZ_WNOTIME_ALL
    {
        public MAZ_WNOTIME[] one_progtime;
    }

    public struct MAZ_AXISLOAD
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAZ_AXISMAX)]
        public char[] status;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAZ_AXISMAX)]
        public ushort[] data;
    }

    public struct MAZ_FEED
    {
        public long fmin;
        public long frev;
    }

    public struct MAZ_TOOLINFO
    {
        public ushort tno;
        public byte suf;
        public byte sufatr;
        public byte name;
        public byte part;
        public char sts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public char[] dummy;
        public long yob;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy2;
    }
    #endregion

    #region API Structure (Related To Diagnosis)
    public struct MAZ_ALARM
    {
        public short eno;
        public byte sts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public char[] dummy;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] pa1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] pa2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] pa3;
        public byte mon;
        public byte day;
        public byte hor;
        public byte min;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] extmes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] head;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] message;
    }

    public struct MAZ_ALARM_ALL
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAZ_ALMMAX)]
        public MAZ_ALARM[] alarm;
    }

    public struct MAZ_NCTIME
    {
        public uint hor;
        public uint min;
        public uint sec;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] dummy;
    }

    public struct MAZ_ACCUM_TIME
    {
        public MAZ_NCTIME power_on;
        public MAZ_NCTIME auto_ope;
        public MAZ_NCTIME auto_cut;
        public MAZ_NCTIME total_cut;
        public MAZ_NCTIME total_time;
    }
    #endregion


    #region API Function (Related To Connection)
    [DllImport("NTIFDLL.dll", EntryPoint = "MazConnect_s", CallingConvention = CallingConvention.Cdecl)]
    extern public static int MazConnect_s(ref ushort hndl, string ipaddress, ushort port, ushort timeout, MAZ_CERTIFY certify);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazDisconnect")]
    extern public static int MazDisconnect(ushort hndl);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazSetTimeout")]
    extern public static int MazSetTimeout(ushort hndl, ushort timeout);
    #endregion

    #region API Function (Related To Position)
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetAxisName")]
    extern public static int MazGetAxisName(ushort hndl, ref MAZ_AXISNAME alldata);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetMachineUnit")]
    extern public static int MazGetMachineUnit(ushort hndl, ref short num);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetCurrentPos")]
    extern public static int MazGetCurrentPos(ushort hndl, ref MAZ_NCPOS alldata);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRemain")]
    extern public static int MazGetRemain(ushort hndl, ref MAZ_NCPOS alldata);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetMachinePos")]
    extern public static int MazGetMachinePos(ushort hndl, ref MAZ_NCPOS alldata);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetPositionInfo")]
    extern public static int MazGetPositionInfo(ushort hndl, ref MAZ_POSITION_INFO alldata);

    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRunMode")]
    extern public static int MazGetRunMode(ushort hndl, ushort head, ref short num);

    /// <summary>
    /// Obtain the main program information selected on the position display
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="pro">Store the main program information in MAZ_PROINFO type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetMainPro")]
    extern public static int MazGetMainPro(ushort hndl, ushort head, ref MAZ_PROINFO pro);

    /// <summary>
    /// Set the Wno program specified in the standard program as the main program selected on the position display
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="wno">Specify the program WNo</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazSetMainPro")]
    extern public static int MazSetMainPro(ushort hndl, ushort head, ref string wno);

    /// <summary>
    /// Obtain the program information currently being operated
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="pro">Store the program information currently being operated in MAZ_PROINFO type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRunningPro")]
    extern public static int MazGetRunningPro(ushort hndl, ushort head, ref MAZ_PROINFO pro);

    /// <summary>
    /// Obtain the program name, unit number, sequence number and block number currently being operated
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="pro">Store the program information currently being operated in MAZ_PROINFO2 type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRunningPro2")]
    extern public static int MazGetRunningPro2(ushort hndl, ushort head, ref MAZ_PROINFO2 pro);

    /// <summary>
    /// Obtain the modal information (G, M, S, F, Second auxiliary code)
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="modal">Store the modal information currently in MAZ_MODAL_INFO type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetModal")]
    extern public static int MazGetModal(ushort hndl, ushort head, ref MAZ_MODAL_INFO modal);

    /// <summary>
    /// Obtain the machining time information of the completed program according to the system
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="data">Store the machining time information of the latest 16 programs in order of completion (from latest to oldest) in MAZ_WNOTIME_ALL type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetProTime")]
    extern public static int MazGetProTime(ushort hndl, ushort head, ref MAZ_WNOTIME_ALL data);

    /// <summary>
    /// Obtain the spindle load (%)
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="spindle">Specify the spindle</param>
    /// <param name="data">Store the spindle load in unsigned short type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetSpindleLoad")]
    extern public static int MazGetSpindleLoad(ushort hndl, ushort spindle, ref ushort data);

    /// <summary>
    /// Obtain the servo axis load (%)
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="alldata">Store the servo axis load in MAZ_AXISLOAD type in the same order as the axis name obtained by MazGetAxisName</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetAxisLoad")]
    extern public static int MazGetAxisLoad(ushort hndl, ref MAZ_AXISLOAD alldata);


    /// <summary>
    /// Obtain the current operation status
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="num">Store the current operation status in short type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRunningSts")]
    extern public static int MazGetRunningSts(ushort hndl, ushort head, ref short num);

    /// <summary>
    /// Obtain the feedrate per minute and the feedrate per revolution in the resultant velocity
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="data">Store the federate in MAZ_FEED type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetFeed")]
    extern public static int MazGetFeed(ushort hndl, ushort head, ref MAZ_FEED data);

    /// <summary>
    /// Obtain the current spindle speed
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="spindle">Specify the spindle</param>
    /// <param name="data">Store the current spindle speed in long type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetCurrentSpindleRev")]
    extern public static int MazGetCurrentSpindleRev(ushort hndl, ushort spindle, ref int data);

    /// <summary>
    /// Obtain the spindle tool information
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="tool">Store the spindle tool information in MAZ_ TOOLINFO type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetCurrentTool")]
    extern public static int MazGetCurrentTool(ushort hndl, ushort head, ref MAZ_TOOLINFO tool);

    /// <summary>
    /// Obtain the actual quantity of machined workpieces (Parts count)
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="num">Store the actual quantity of machined workpieces (Parts count) in long type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetPartsCount")]
    extern public static int MazGetPartsCount(ushort hndl, ushort head, ref int num);
    #endregion

    #region API Function (Related To Diagnosis)
    /// <summary>
    /// Obtain the occurring alarm
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="alarm">Store the occurring alarm information (alarm for all system) in ALARMALL type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetAlarm")]
    extern public static int MazGetAlarm(ushort hndl, ref MAZ_ALARM_ALL alarm);

    /// <summary>
    /// Obtain the automatic operation time since the last time the NC power was turned on
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="time">Store the automatic operation time since the last time the NC power was turned on in MAZ_NCTIME type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRunningTime")]
    extern public static int MazGetRunningTime(ushort hndl, ushort head, ref MAZ_NCTIME time);

    /// <summary>
    /// Obtain the power ON time, automatic operation time, automatic cutting time, automatic turning time, and accumulated time
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="time">Store the each accumulated time in MAZ_ACCUM_TIME type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetAccumulatedTime")]
    extern public static int MazGetAccumulatedTime(ushort hndl, ushort head, ref MAZ_ACCUM_TIME time);
    #endregion

    #region API Function (Related To Program)
    /// <summary>
    /// Download the program of specified WNo in the standard area to the specified directory
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="wno">Specify the program WNo</param>
    /// <param name="dir">Specify the program data destination directory by absolute path</param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazReceiveProgram")]
    extern public static int MazReceiveProgram(ushort hndl, char[] wno, char[] dir, short overwrite);

    /// <summary>
    /// Download the program of specified WNo in the standard area to the specified directory
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="wno">Specify the program WNo</param>
    /// <param name="dir">Specify the program data destination directory by absolute path</param>
    /// <param name="overwrite"></param>
    /// <param name="area"></param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazReceiveProgram2")]
    extern public static int MazReceiveProgram2(ushort hndl, char[] wno, char[] dir, short overwrite, short area);
    #endregion

    #region API Function (Related To Setup)
    /// <summary>
    /// Obtain a batch of selected macro variable (Common variable for automatic operation) for specified data area
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="cmnVarTop">Specify the first number of the common variable to be obtained</param>
    /// <param name="cmnVarEnd">Specify the final number of the common variable to be obtained</param>
    /// <param name="mcrData">Store the obtained common variable in double type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazGetRangedCmnVar")]
    extern public static int MazGetRangedCmnVar(ushort hndl, ushort head, short cmnVarTop, short cmnVarEnd, ref double mcrData);

    /// <summary>
    /// Obtain a batch of selected macro variable (Common variable for automatic operation) for specified data area
    /// </summary>
    /// <param name="hndl">Specify the library handle</param>
    /// <param name="head">Specify the system number</param>
    /// <param name="cmnVarTop">Specify the first number of the common variable to be obtained</param>
    /// <param name="cmnVarEnd">Specify the final number of the common variable to be obtained</param>
    /// <param name="mcrData">Store the common variable to be set in double type</param>
    /// <returns></returns>
    [DllImport("NTIFDLL.dll", EntryPoint = "MazSetRangedCmnVar")]
    extern public static int MazSetRangedCmnVar(ushort hndl, ushort head, short cmnVarTop, short cmnVarEnd, ref double mcrData);
    #endregion
}