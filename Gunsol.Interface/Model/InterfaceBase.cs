using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunsol.Interface.Model
{
    public class InterfaceBase
    {
        public CommInfo commInfo { get; set; }
        public string division { get; set; }
        public bool isConnect { get; set; }

        //public Dictionary<string, object> receiveData { get; set; }
        //public Dictionary<string, object> sendData { get; set; }

        public virtual void Connect() { }
        public virtual void DisConnect() { }
        public virtual void Send() { }
        public virtual void Receive() { }
        public virtual void Parsing() { }

        //public virtual void SetDictionary(bool isReceive, string key, object value)
        //{
        //    if (isReceive)
        //    {
        //        if (receiveData.Keys.Contains(key))
        //        {
        //            receiveData[key] = value;
        //        }
        //        else
        //        {
        //            receiveData.Add(key, value);
        //        }
        //    }
        //    else
        //    {
        //        if (sendData.Keys.Contains(key))
        //        {
        //            sendData[key] = value;
        //        }
        //        else
        //        {
        //            sendData.Add(key, value);
        //        }
        //    }
        //}

        public virtual void SetDictionary(Dictionary<object, object> data, object key, object value)
        {
            if (data.Keys.Contains(key))
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }
        }
        //public virtual void SetDictionary(Dictionary<string, object> data, string key, object value)
        //{
        //    if (data.Keys.Contains(key))
        //    {
        //        data[key] = value;
        //    }
        //    else
        //    {
        //        data.Add(key, value);
        //    }
        //}
        //public virtual void SetDictionary(Dictionary<int, object> data, int key, object value)
        //{
        //    if (data.Keys.Contains(key))
        //    {
        //        data[key] = value;
        //    }
        //    else
        //    {
        //        data.Add(key, value);
        //    }
        //}
    }
}
