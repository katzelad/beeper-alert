using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace alert
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            BMGR.Daemon.SerialBeeper a = new BMGR.Daemon.SerialBeeper("COM5", 4800, Parity.None, 8, StopBits.One, 1);
            a.Open();
            a.OnPagerMessageReceived += new BMGR.Daemon.SerialBeeper.PagerMessageReceived(OnPagerMessageReceived);
        }

        void OnPagerMessageReceived(BMGR.Daemon.PagerMessage nan)
        {
            label1.Text = nan.Number + ":" + nan.Text + ":" + nan.Status;
        }
    }
}
