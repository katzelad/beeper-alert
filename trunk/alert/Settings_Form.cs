using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace alert
{
    public partial class Settings_Form : Form
    {
        private JSONParser JSONdata = JSONParser.get();
        private string[] areas;
        private string[] selectedAreas;
        public static Settings settings = Settings.Instance;

        public Settings_Form()
        {
            InitializeComponent();
            areas = JSONdata.getGroupNames();
            chkAreaList.DataSource = areas;
            // settings.loadSettings();

            if (settings.areas != null)
            {
                foreach (string strArea in settings.areas)
                {
                    chkAreaList.SetItemChecked(chkAreaList.FindStringExact(strArea), true);
                }
            }
            port.Items.AddRange(SerialPort.GetPortNames());
            if (port.Items.Count > 0)
                port.SelectedIndex = 0;
            // portCOM.Value = Settings.Instance.port == null ? 5 : Int32.Parse(Regex.Match(Settings.Instance.port, @"\d+").Value);

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            selectedAreas = chkAreaList.CheckedItems.OfType<string>().ToArray();
            settings.areas = selectedAreas;
            // settings.port = "COM" + portCOM.Value;
            settings.port = port.SelectedItem.ToString();
            settings.alerts = new List<Alert>();

            //create selected areas list 
            for (int i = 0; i < settings.areas.Length; i++)
            {
                //extract id from area
                int areaID = Int32.Parse(Regex.Match(settings.areas[i], @"\d+").Value);

                //get alert by id
                Alert newAlert = JSONdata.getAlert(areaID);

                //add to alerts list in settings
                settings.alerts.Add(newAlert);
            }
            //settings.saveSettings();

            if (!Bepper_Alert.listenToBeeper(settings.port))
                return;

            this.Close();
        }

    }
}
