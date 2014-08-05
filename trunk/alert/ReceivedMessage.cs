using System;
using System.Text;
using System.Data; 

namespace BMGR.Globals
{
    public class ReceivedMessage : MessageBase,IFormattable 
    {

        private string _addressSlot;
        private bool _encrypted;
        private InputSource  _inputSource;

        public InputSource  InputSource
        {
            get { return _inputSource; }
            set { _inputSource = value; }
        }
	
        public string AddressSlot
        {
            get { return _addressSlot; }
            set { _addressSlot = value; }
        }
        public bool Encrypted
        {
            get { return _encrypted; }
            set { _encrypted = value; }
        }

        public ReceivedMessage()
        { }
        public ReceivedMessage(DataRow row)
        { 
            ID = (long)row["ID"];
	        Text = row["Text"].ToString();
	        Status = (MessageStatus)row["StatusID"];
	        SourceID = (int) row["SourceID"];
	        Received = DateTime.Parse(row["ReceivedTime"].ToString());
	        Readed = Convert.ToBoolean(row["Read_flg"]);
            Number = row.IsNull("OriginalMessageID") ? -1 : (long)row["OriginalMessageID"];
            Encrypted = row.IsNull("Encrypted")?false:Convert.ToBoolean(row["Encrypted"]);
            AddressSlot = row["AddressSlot"].ToString();
        }


        public override string ToString()
        {
            return Text;
        }

        public override string GetDelimitedString(string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ID); sb.Append(delimiter);
            sb.Append(Received); sb.Append(delimiter);
            sb.Append(Number); sb.Append(delimiter);
            switch (delimiter)
            {
                case ",":
                    sb.Append(Text.Replace(",", ";"));
                    break;
                default:
                    sb.Append(Text);
                    break;
            }
            sb.Append(delimiter);
            sb.Append(Encrypted ? "*" : ""); 
            return sb.ToString();
        } 


        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return Text;

            StringBuilder sb = new StringBuilder();
            if (format.Contains("e"))
            {
                if (Encrypted)
                    sb.Append("*");
            }
            if (format.Contains("n"))
            {
                sb.Append(Number);
                sb.Append(") ");
            }
            sb.Append(Text);
            return sb.ToString();
        }

        #endregion
    }

}
