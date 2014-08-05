using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
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
            var watcher = new GeoCoordinateWatcher();

            watcher.TryStart(false, TimeSpan.FromMilliseconds(5000));

            var coord = watcher.Position.Location;


            if (coord.IsUnknown != true)

                label1.Text = coord.Longitude + "," + coord.Latitude;
            else
               label1.Text = "Can't locate";
        }
    }
}
