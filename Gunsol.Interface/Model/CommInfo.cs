using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunsol.Interface.Model
{
    public class CommInfo
    {
        public string machineCode { get; set; }
        public string machineName { get; set; }
        public string machineType { get; set; }
        public string machineIp { get; set; }
        public ushort machinePort { get; set; }
        public string machineSerialPort { get; set; }        
        public string machineAddressPath { get; set; }
        public string machineTableName { get; set; }
        public bool machineUseYn { get; set; }
    }
}
