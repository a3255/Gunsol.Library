using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Gunsol.Interface.Model
{
    public class InterfaceCNCBase : InterfaceBase
    {
        public string MODE { get; set; }
        public string STATUS { get; set; }
        public string ALARM { get; set; }
        public string ALARM_TYPE { get; set; }
        public int ALARM_NO { get; set; }
        public string ALARM_MSG { get; set; }

        public int FEED { get; set; }
        public Dictionary<object, object> SPINDLE { get; set; }
        public int M_CODE { get; set; }
        public int T_CODE { get; set; }
        public Dictionary<object, object> AXIS_ABS { get; set; }
        public Dictionary<object, object> AXIS_LOAD { get; set; }
        public Dictionary<object, object> SPINDLE_LOAD { get; set; }
        public int PART_CNT { get; set; }

        public int POWER_T { get; set; }
        public int OPERATION_T { get; set; }
        public int CUTTING_T { get; set; }
        public int CYCLE_T { get; set; }

        public Dictionary<object, object> MODAL_VALUE { get; set; }
        public Dictionary<object, object> PARAM_VALUE { get; set; }
        public Dictionary<object, object> MACRO_VALUE { get; set; }
        public Dictionary<object, object> PLC_VALUE { get; set; }

        public int MAIN_PRG_NO { get; set; }
        public int CURRENT_PRG_NO { get; set; }
        public string MAIN_PRG_NAME { get; set; }
        public string CURRENT_PRG_NAME { get; set; }
        public string MAIN_PRG_PATH { get; set; }
        public string CURRENT_PRG_PATH { get; set; }

        public string MAIN_PRG_NO_NCDATA { get; set; }
        public string MAIN_PRG_NAME_NCDATA { get; set; }
        public string CURRENT_PRG_NO_NCDATA { get; set; }
        public string CURRENT_PRG_NAME_NCDATA { get; set; }
    }
}
