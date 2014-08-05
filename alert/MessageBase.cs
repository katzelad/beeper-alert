using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BMGR.Globals
{
    public enum MessageType
    {
        Unknown,
        Text
    }

    public enum MessageStatus
    {
        Unknown = -1,
        New = 0,
        Assigned = 1,
        Received = 10,
        Sent = 11,
        Scheduled = 12,
        Failed = 13
    }

    public class MessageBase:IComparable<MessageBase>
    {

        #region Private Members
        private MessageType _type;
        private MessageStatus _status;
        private int _sourceID;
        private object _tag;
        private string _text; 
        private bool _readed;
        private DateTime _received;
        private long _id;
        private long _originalMessageId;

        #endregion

        #region Public Properties
        public MessageType Type 
        {
            get { return _type; }
            set { _type = value; }
        }
        public MessageStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }
        public int SourceID
        {
            get { return _sourceID; }
            set { _sourceID = value; }
        }
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        public long ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public DateTime Received
        {
            get { return _received; }
            set { _received = value; }
        }
        public long Number
        {
            get { return _originalMessageId; }
            set { _originalMessageId = value; }
        }
        public bool Readed
        {
            get { return _readed; }
            set 
            { 
                _readed = value; 

            }
        }

        public MessageBase FormattedText { get { return this; } }


        #endregion

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType().Equals(GetType()))
            {
                MessageBase other = obj as MessageBase;
                if (other != null)
                {
                    return other.ID == ID;
                }
            }

            return base.Equals(obj);
        }



        public virtual string GetDelimitedString(string delimiter)
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
            return sb.ToString();
        }

        #region IComparable Members

        public int CompareTo(MessageBase other)
        {
            return ID.CompareTo(other.ID);
        }

        #endregion
    }

    public class MessageFormatter : IFormatProvider, ICustomFormatter
    {
        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {
            return formatType == typeof (ICustomFormatter) ? this : null;
        }

        #endregion

        #region ICustomFormatter Members

        public string Format(string formatString, object argToBeFormatted, IFormatProvider formatProvider)
        {
            // If no format string is provided or the format string cannot 
            // be handled, use IFormattable or standard string processing.
            if (formatString == null ||
                string.IsNullOrEmpty(formatString))
            {
                if (argToBeFormatted is IFormattable)
                    return ((IFormattable)argToBeFormatted).ToString(formatString, formatProvider);

                return argToBeFormatted.ToString();
            }

            throw new NotImplementedException("Custom Format not implemented");
        }

        #endregion
    }
}
