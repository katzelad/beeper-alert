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
    public partial class Settings_Form : Form
    {
        private JSONParser JSONdata = new JSONParser("../../MIGUN_GROUPS.json");
        private List<string> areas = new List<string>();
        private string[] selectedAreas;
        private string comPort;
        public static Settings settings = Settings.Instance;

        public Settings_Form()
        {
            InitializeComponent();
            areas = JSONdata.getGroupNames();
            chkAreaList.DataSource = areas;
            settings.loadSettings();

            foreach (string strArea in settings.areas)
            {
                chkAreaList.SetItemChecked(chkAreaList.FindStringExact(strArea), true);
            }
 
        }
        
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            selectedAreas = chkAreaList.CheckedItems.OfType<string>().ToArray();
            comPort = txtCOM.Text;
            settings.areas = selectedAreas;
            settings.port = comPort;
            settings.saveSettings();
        }
    }
}
