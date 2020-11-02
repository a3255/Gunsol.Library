using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Gunsol.Interface.Model
{
    public class InterfaceROBOTBase : InterfaceBase
    {
        public int RBT_PWR { get; set; }
        public int RBT_GRP { get; set; }
        public string RBT_BGN { get; set; }
        public string RBT_END { get; set; }
        public string RBT_ERR { get; set; }
        public int RBT_VSN { get; set; }
        public Dictionary<object, object> SEND_DATA;
    }
}
