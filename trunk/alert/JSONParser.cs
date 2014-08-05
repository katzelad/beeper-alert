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

        public JSONParser(string jsonFileName)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(jsonFileName)));
            data = (RootObject)new DataContractJsonSerializer(typeof(RootObject)).ReadObject(stream);
        }

        public List<string> getGroupNames()
        {
            return data.features.Select(element => element.attributes.MIGUN_GROUP_NAME).ToList();
        }
    }
}
