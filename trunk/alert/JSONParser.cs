using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

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
        Dictionary<string, Tuple<string, int>> zoneData;

        public JSONParser(string jsonFileName, string tableFileName)
        {

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(jsonFileName)));
            data = (RootObject)new DataContractJsonSerializer(typeof(RootObject)).ReadObject(stream);

            SpreadsheetDocument table = SpreadsheetDocument.Open(tableFileName, false);
            Func<Row, int, string> getCell = (row, i) => row.Elements<Cell>().ElementAt<Cell>(i).CellValue.Text;
            zoneData = table.WorkbookPart.WorksheetParts.First().Worksheet.Elements<SheetData>().First().Elements<Row>().Skip(1)
                .ToDictionary(
                    row => getCell(row, 1),
                    row => new Tuple<string, int>(getCell(row, 2), int.Parse(getCell(row, 9)))
                );
            table.Close();

        }

        public List<string> getGroupNames()
        {
            return data.features.Select(element => element.attributes.MIGUN_GROUP_NAME).ToList();
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

        public Tuple<string, int> getPolygon(double x, double y)
        {
            Feature zone = data.features.Find(feature => feature.attributes.MIGUN_GROUP_NAME.Equals(currentZone));
            if (currentZone.Equals("") || !pointInPolygon(zone.geometry.rings[0].Select(p => new Point(p[0], p[1])).ToArray(), new Point(x, y)))
                currentZone = data.features.Find(feature =>
                    pointInPolygon(feature.geometry.rings[0].Select(p =>
                        new Point(p[0], p[1])
                    ).ToArray(), new Point(x, y))
                ).attributes.MIGUN_GROUP_NAME;
            return zoneData[currentZone];
        }

    }
}
