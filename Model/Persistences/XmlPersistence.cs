using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PureLib.Common;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public class XmlPersistence : PersistenceBase {
        public override void Save(Folder folder, Stream stream) {
            XmlSerializer serializer = new XmlSerializer(typeof(Folder));
            using (XmlWriter writer = XmlWriter.Create(stream)) {
                serializer.Serialize(writer, folder);
            }
        }

        public override Folder Load(Stream stream) {
            using (XmlReader reader = XmlReader.Create(stream)) {
                XmlSerializer serializer = new XmlSerializer(typeof(Folder));
                return (Folder)serializer.Deserialize(reader);
            }
        }
    }
}
