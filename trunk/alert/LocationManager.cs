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
        private GeoCoordinate current;

        public void GetLocationEvent()
        {
            this.watcher = new GeoCoordinateWatcher();
            this.current = new GeoCoordinate();
            this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            bool started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!started)
            {
                Console.WriteLine("GeoCoordinateWatcher timed out on start.");
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            PrintPosition(e.Position.Location.Latitude, e.Position.Location.Longitude);
            current.Latitude = e.Position.Location.Latitude;
            current.Longitude = e.Position.Location.Longitude;
        }

        void PrintPosition(double Latitude, double Longitude)
        {
            Console.WriteLine("Latitude: {0}, Longitude {1}", Latitude, Longitude);
        }

        double getLatitude()
        {
            return current.Latitude;
        }
        double getLongitude()
        {
            return current.Longitude;
        }
    }

}
