using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Gunsol.Interface.Model
{
    public class InterfaceIOBase : InterfaceBase
    {
        public string STATUS { get; set; }
        public int SIGNAL_1 { get; set; }
        public int SIGNAL_2 { get; set; }
        public int SIGNAL_3 { get; set; }
        public int SIGNAL_4 { get; set; }
    }
}
