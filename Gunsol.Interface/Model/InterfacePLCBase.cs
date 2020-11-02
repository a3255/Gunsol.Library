using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Gunsol.Interface.Model
{
    public class InterfacePLCBase : InterfaceBase
    {
        public string STATUS { get; set; }
        public Dictionary<object, object> PLC_VALUE { get; set; }
    }
}
