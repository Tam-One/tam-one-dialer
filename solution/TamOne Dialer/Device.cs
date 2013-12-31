using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TamOne_Dialer
{
    public class Device
    {
        private string id;
        private string name;
        public string Id
        {
            get
            {
                return id;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }

        public Device(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    public class Prefix : Device
    {
        public Prefix(string id, string name)
            : base(id, name)
        {

        }
    }
}
