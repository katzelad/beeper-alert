namespace BMGR.Globals
{
    public class InputSource
    {
        private int _id;
        private string _name;
        private string _station;
        private string _address;
        private int _port;
        private int _baudRate;
        private int _dataBits;
        private int _parity;
        private int _stopBits;

        // constructor
        //public InputSource()
        //{
        //}
        public InputSource(System.Data.DataRow row)
        {
            ID = (int)row["ID"];
            Name = row["Name"].ToString();
            Station = row["Station"].ToString();
            Address = row["Address"].ToString();
            Port = row.IsNull("Port") ? 0 : (int)row["Port"];
            BaudRate = row.IsNull("BaudRate") ? 0 : (int)row["BaudRate"];//(int?)row["BaudRate"];
            DataBits = row.IsNull("DataBits") ? 0 : (int)row["DataBits"]; //(int?)row["DataBits"];
            Parity = row.IsNull("Parity") ? -1 : (int)row["Parity"]; //(int?)row["Parity"];
            StopBits = row.IsNull("StopBits") ? -1 : (int)row["StopBits"]; //(int?)row["StopBits"];
        }

        public int ID { get { return _id; } set { _id = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Station { get { return _station; } set { _station = value; } }
        public string Address { get { return _address; } set { _address = value; } }
        public int Port { get { return _port; } set { _port = value; } }
        public int BaudRate { get { return _baudRate; } set { _baudRate = value; } }
        public int DataBits { get { return _dataBits; } set { _dataBits = value; } }
        public int Parity { get { return _parity; } set { _parity = value; } }
        public int StopBits { get { return _stopBits; } set { _stopBits = value; } }

    }
}
