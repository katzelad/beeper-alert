using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace alert
{
public class Settings
{ 
   private static Settings instance;

   public string[] areas { get; set; }

   public List<Alert> alerts = new List<Alert>();

   public Alert myAlert;
   public string port;

   private Settings() { }

   public static Settings Instance
   {
      get 
      {
         if (instance == null)
         {
             instance = new Settings();
         }
         return instance;
      }
   }

    public void saveSettings()
    {
        string areasString = String.Join(",", areas);

        Properties.Settings.Default.Port = port;
        Properties.Settings.Default.Areas = areasString;

        try
        {
            StreamWriter sw = new StreamWriter("../../properties.txt");
            
            //clear file
            sw.Write("");
            //Write port
            sw.WriteLine(port);
            //Write areas
            sw.WriteLine(areasString);
            //Close the file
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block.");
        }
   }

    public void loadSettings()
    {
        /*if (Properties.Settings.Default.Port != "")
        {
            port = Properties.Settings.Default.Port;
        }
        if (Properties.Settings.Default.Areas.Length > 1)
        {
            areas = Properties.Settings.Default.Areas.Split(',');
        }*/

        StreamReader sr = new StreamReader("../../properties.txt");

        port = sr.ReadLine();
        if (sr.ReadLine() != null)
            areas = sr.ReadLine().Split(',');
    }
   
  }
}