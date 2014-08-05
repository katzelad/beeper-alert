using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alert
{
    public class Alert
    {
        public Alert(string iName,int iTTL)
        {
            Name = iName;
            TTL = iTTL;
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public int TTL { get; set; }
    }
}
