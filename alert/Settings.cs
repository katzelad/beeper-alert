using System;
using System.Collections.Generic;
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
   public string port = "COM5";

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
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        string areasString = String.Join(",", areas);

        localSettings.Values["port"] = port;
        localSettings.Values["areas"] = areasString;

   }

    public void loadSettings()
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        Object portVal = localSettings.Values["port"];
        port = portVal.ToString();
        Object areasVal = localSettings.Values["areas"];
        string areasString = areasVal.ToString();
        areas = areasString.Split(',');

    }
   
  }
}