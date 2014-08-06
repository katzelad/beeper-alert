using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace alert
{
    class JSONParser
    {
        [DataContract]
        class FieldAliases
        {
            [DataMember]
            public string MIGUN_GROUP_NAME { get; set; }
        }

        [DataContract]
        class SpatialReference
        {
            [DataMember]
            public string wkt { get; set; }
        }

        [DataContract]
        class Field
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string alias { get; set; }
            [DataMember]
            public int length { get; set; }
        }

        [DataContract]
        class Attributes
        {
            [DataMember]
            public string MIGUN_GROUP_NAME { get; set; }
        }

        [DataContract]
        class Geometry
        {
            [DataMember]
            public List<List<List<double>>> rings { get; set; }
        }

        [DataContract]
        class Feature
        {
            [DataMember]
            public Attributes attributes { get; set; }
            [DataMember]
            public Geometry geometry { get; set; }
        }

        [DataContract]
        class RootObject
        {
            [DataMember]
            public string displayFieldName { get; set; }
            [DataMember]
            public FieldAliases fieldAliases { get; set; }
            [DataMember]
            public string geometryType { get; set; }
            [DataMember]
            public SpatialReference spatialReference { get; set; }
            [DataMember]
            public List<Field> fields { get; set; }
            [DataMember]
            public List<Feature> features { get; set; }
        }

        RootObject data;
        Dictionary<string, Alert> zoneData;
        Dictionary<int, Alert> zoneByID;

        private JSONParser()
        {

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText("../../MIGUN_GROUPS.json")));
            data = (RootObject)new DataContractJsonSerializer(typeof(RootObject)).ReadObject(stream);

            zoneData = File.ReadAllLines("../../בסיס נתונים התרעה.csv", Encoding.GetEncoding("iso-8859-8"))
                .Skip(3)
                .Select(line => line.Split(','))
                .ToDictionary(row => row[4], row => new Alert(row[5], int.Parse(row[10])));

            foreach (Alert a in zoneData.Values)
                if (a.Name[0] == '"' && a.Name[a.Name.Length - 1] == '"')
                    a.Name = a.Name.Substring(1, a.Name.Length - 2);

            zoneByID = zoneData.Values.ToDictionary(element => element.ID);

        }

        static JSONParser instance;

        public static JSONParser get()
        {
            if (instance == null)
                instance = new JSONParser();
            return instance;
        }

        public string[] getGroupNames()
        {
            return zoneData.Select(element => element.Value.Name).ToArray();
        }

        public Alert getAlert(int id)
        {
            return zoneByID[id];
        }

        class Point
        {

            public readonly double X, Y;

            public Point(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

        }

        bool pointInPolygon(Point[] polygon, Point point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        string currentZone = "";

        public Alert getPolygon(double x, double y)
        {
            Feature zone = data.features.Find(feature => feature.attributes.MIGUN_GROUP_NAME.Equals(currentZone));
            if (currentZone.Equals("") || !pointInPolygon(zone.geometry.rings[0].Select(p => new Point(p[0], p[1])).ToArray(), new Point(x, y)))
            {
                Feature zoneOrNull = data.features.Find(feature =>
                    pointInPolygon(feature.geometry.rings[0].Select(p =>
                        new Point(p[0], p[1])
                    ).ToArray(), new Point(x, y))
                );
                if (zoneOrNull == null)
                    return null;
                currentZone = zoneOrNull.attributes.MIGUN_GROUP_NAME;
            }
            return zoneData[currentZone];
        }

    }
}
