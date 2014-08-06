using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alert
{
    public class Alert
    {
        static const char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        public Alert(string iName,int iTTL)
        {
            Name = iName;
            TTL = iTTL;
            ID = int.Parse(new string(iName.Skip(iName.LastIndexOfAny(digits)).ToArray()));
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public int TTL { get; set; }
    }
}
