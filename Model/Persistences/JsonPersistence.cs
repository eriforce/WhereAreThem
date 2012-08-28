using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PureLib.Common;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public class JsonPersistence : PersistenceBase {
        private const SerializationFormat format = SerializationFormat.Json;

        public override void Save(Folder folder, Stream stream) {
            stream.WriteDataContract(folder, format);
        }

        public override Folder Load(Stream stream) {
            return stream.ReadDataContract<Folder>(format);
        }
    }
}
