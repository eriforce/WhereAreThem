using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PureLib.Common;
using WhereAreThem.Model.Models;
using ServiceStack.Text;

namespace WhereAreThem.Model.Persistences {
    public class JsvPersistence : PersistenceBase {
        public override void Save(Folder folder, Stream stream) {
            using (StreamWriter sw = new StreamWriter(stream)) {
                TypeSerializer.SerializeToWriter(folder, sw);
            }
        }

        public override Folder Load(Stream stream) {
            using (StreamReader sr = new StreamReader(stream)) {
                return TypeSerializer.DeserializeFromReader<Folder>(sr);
            }
        }
    }
}
