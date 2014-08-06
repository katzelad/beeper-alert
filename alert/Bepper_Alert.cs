using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using BMGR.Daemon;

namespace alert
{
    public partial class Bepper_Alert : Form
    {
        public Bepper_Alert()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            ContextMenu notifyMenu = new System.Windows.Forms.ContextMenu();
#if DEBUG
            notifyMenu.MenuItems.Add("DUMMY", new EventHandler(notifyMenu_dummy));
#endif
            notifyMenu.MenuItems.Add("הגדרות", new EventHandler(notifyMenu_Settings));
            notifyMenu.MenuItems.Add("יציאה", new EventHandler(notifyMenu_exit));
            notifyIcon1.ContextMenu = notifyMenu;

            if (Settings.Instance.port != null)
                listenToBeeper(Settings.Instance.port);

            Alert_Form Alert = new Alert_Form();
            Alert.Show();

        }

        static SerialBeeper a;
        public static bool listenToBeeper(string port)
        {
            if (a != null)
            {
                a.Close();
                a.Dispose();
            }
            try
            {
                a = new SerialBeeper(port, 4800, Parity.None, 8, StopBits.One, 1);
                a.Open();
                a.OnPagerMessageReceived += new SerialBeeper.PagerMessageReceived(OnPagerMessageReceived);
            }
            catch (System.IO.IOException)
            {
                return false;
            }
            return true;
        }

        private void notifyMenu_exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyMenu_Settings(object sender, EventArgs e)
        {
            Settings_Form Form = new Settings_Form();
            Form.ShowDialog();
        }

        private void notifyMenu_dummy(object sender, EventArgs e)
        {
            OnPagerStringReceived("fk311kk");
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        static void OnPagerMessageReceived(PagerMessage nan)
        {

            OnPagerStringReceived(nan.Text);

        }

        static void OnPagerStringReceived(string nan)
        {

            //extract id from area
            int areaID = Int32.Parse(Regex.Match(nan, @"\d+").Value);

            foreach (Alert al in Settings.Instance.alerts)
            {
                if (al.ID == areaID)
                {
                    Alert_Form.arrAlert.Add(new Alert(al));
                }
            }

            if (Settings.Instance.myAlert != null && Settings.Instance.myAlert.ID == areaID)
            {
                Alert_Form.arrAlert.Add(new Alert(Settings.Instance.myAlert));
            }


        }

        private void Bepper_Alert_Load(object sender, EventArgs e)
        {

        }
    }
}
