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
        private const SerializationFormat format = SerializationFormat.Xml;

        public override void Save(Folder folder, Stream stream) {
            stream.WriteDataContract(folder, format);
        }

        public override Folder Load(Stream stream) {
            return stream.ReadDataContract<Folder>(format);
        }
    }
}
