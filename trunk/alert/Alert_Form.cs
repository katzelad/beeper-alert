using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace alert
{
    public partial class Alert_Form : Form
    {

        public static List<Alert> arrAlert;
        public Alert_Form()
        {
            InitializeComponent();
            arrAlert = new List<Alert>();
        }

        private void Alert_Form_Load(object sender, EventArgs e)
        {
            listBox1.DataSource = arrAlert;
            listBox1.DisplayMember = "Name";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (arrAlert.Count == 0)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            else
            {                
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                listBox1.DataSource = arrAlert;

                arrAlert = arrAlert.Where(a => a.TTL != 0).ToList<Alert>();

                foreach (var item in arrAlert)
                {
                    item.TTL--;
                }
            }
        }
    }
}
