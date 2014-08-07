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
    class CLocation
    {
        private GeoCoordinateWatcher watcher;

        public void GetLocationEvent()
        {
            this.watcher = new GeoCoordinateWatcher();
            this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            bool started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!started)
            {
                System.Windows.Forms.MessageBox.Show("GeoCoordinateWatcher timed out on start.");
            }
        }

        static void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            changePosition(e.Position.Location.Latitude, e.Position.Location.Longitude);
        }

        public static void changePosition(double x, double y)
        {
            // PrintPosition(x, y);
            Settings.Instance.myAlert = JSONParser.get().getPolygon(x, y);
        }

        static void PrintPosition(double Latitude, double Longitude)
        {
            System.Windows.Forms.MessageBox.Show("Latitude: " + Latitude + ", Longitude " + Longitude);
        }
    }

}
