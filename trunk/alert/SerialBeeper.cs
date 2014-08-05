using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using BMGR.Globals;

namespace BMGR.Daemon
{
    public class SerialBeeper:IDisposable 
    {
        public const char STX = (char)2; //Start of text
        public const char ETX = (char)3; //End of text
        public const char EOT = (char)4; //End of transmission

        private byte[] buffer;
        private int pos = -1;

        private SerialPort _port;
        private int _sourceID=-1;
        private InputSource _src;
        public delegate void PagerMessageReceived(PagerMessage pagerMessage);
        public PagerMessageReceived OnPagerMessageReceived;

        public SerialBeeper(InputSource src)
        {
            _src = src;
            _sourceID = src.ID;
            _port = new SerialPort("COM" + src.Port, src.BaudRate ,
                (System.IO.Ports.Parity)src.Parity , src.DataBits , 
                (System.IO.Ports.StopBits)src.StopBits);
            _port.DtrEnable = true;
            _port.DiscardNull = true;
            _port.ReceivedBytesThreshold = 1;
            _port.RtsEnable = false;
            _port.Handshake = Handshake.None;
            //_port.Encoding = Encoding.GetEncoding(862);
             // Attach a method to be called when there      
            // is data waiting in the port's buffer
            _port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }
        
        public SerialBeeper(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits,int SourceID)
        {
            _sourceID = SourceID;
            _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _port.DtrEnable = true;
            _port.DiscardNull = true;
            _port.ReceivedBytesThreshold = 1;
            _port.RtsEnable = false;
            _port.Handshake = Handshake.None;
            _port.Encoding = Encoding.GetEncoding(862);
            // Attach a method to be called when there      
            // is data waiting in the port's buffer
            _port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        [STAThread]
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] bttmp= new byte[_port.ReadBufferSize];
            int i = _port.Read(bttmp, 0, _port.ReadBufferSize);

            if (i > 0)
            {
                PagerMessage pm;
                //since the event is fired n number of times, each time with different
                //message block, we'll buffer it until we get the last chunk of the data
                if (bttmp[0] == STX)
                {
                    buffer = new byte[_port.ReadBufferSize];
                    pos = -1;
                    AppendToBuffer(buffer, bttmp);
                }
                else
                {
                    AppendToBuffer(buffer, bttmp);
                    if (Array.IndexOf(buffer, (byte)EOT) > 0)
                    {
                        pm = new PagerMessage(buffer);
                        pm.SourceID = _sourceID;
                        pm.InputSource = _src;
                        if (OnPagerMessageReceived != null)
                            OnPagerMessageReceived.BeginInvoke(pm, null, null);
                    }
                }
            }
        }

        private int AppendToBuffer(byte[] target,byte[] source)
        {
            foreach (byte bt in source)
            {
                if (bt == 0) break;
                pos++;
                target[pos] = bt;
            }
            return pos;
        }

        public void WakeUp()
        {
            if (_port.IsOpen)
            {
                _port.DtrEnable = false;
                _port.DtrEnable = true;
            }
        }

        public void Open()
        {
            if (!_port.IsOpen) 
                _port.Open();
        }
        
        public void Close()
        {
            if (_port.IsOpen)
                _port.Close();
        
        }
        
        #region IDisposable Members

        public void Dispose()
        {
            if (_port != null)
            {
                this.Close();
                _port.DataReceived -= port_DataReceived;
                _port.Dispose();
            }
        }

        #endregion
    }
}
