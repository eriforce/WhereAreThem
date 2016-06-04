using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;
using Newtonsoft.Json;

namespace WhereAreThem.Model.Persistences {
    public class JsonNetPersistence : PersistenceBase {
        public override void Save(Folder folder, Stream stream) {
            StreamWriter sw = new StreamWriter(stream);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(sw, folder);
            sw.Flush();
            stream.Flush();
        }

        public override Folder Load(Stream stream) {
            StreamReader sr = new StreamReader(stream);
            JsonSerializer serializer = new JsonSerializer();
            return (Folder)serializer.Deserialize(sr, typeof(Folder));
        }
    }
}
