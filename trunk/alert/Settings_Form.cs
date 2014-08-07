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

            if (settings.areas != null )
            {
                foreach (string strArea in settings.areas)
                {
                    chkAreaList.SetItemChecked(chkAreaList.FindStringExact(strArea), true);
                }
            }
            port.Items.AddRange(SerialPort.GetPortNames());
            if (port.Items.Count > 0)
                port.SelectedIndex = 0;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            selectedAreas = chkAreaList.CheckedItems.OfType<string>().ToArray();
            settings.areas = selectedAreas;
            settings.port = port.Text;
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
            settings.saveSettings();

            if (!Bepper_Alert.listenToBeeper(settings.port))
            {
                MessageBox.Show("פורט COM שגוי.", "VEN", 0, MessageBoxIcon.Error, 0, MessageBoxOptions.RtlReading);
                return;
            }

            this.Close();
        }

    }
}
