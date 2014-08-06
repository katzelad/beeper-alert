using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace alert
{
    public class Alert
    {
        public Alert(string iName,int iTTL)
        {
            Name = iName;
            TTL = iTTL;
            ID = int.Parse(Regex.Match(iName, @"\d+").Value);
        }
        public int ID;
        public string Name;
        public int TTL;
    }
}
