﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PureLib.Common;
using WhereAreThem.Model.Models;
using ServiceStack.Text;

namespace WhereAreThem.Model.Persistences {
    public class JsonPersistence : PersistenceBase {
        public override void Save(Folder folder, Stream stream) {
            using (StreamWriter sw = new StreamWriter(stream)) {
                JsonSerializer.SerializeToWriter(folder, sw);
            }
        }

        public override Folder Load(Stream stream) {
            using (StreamReader sr = new StreamReader(stream)) {
                return JsonSerializer.DeserializeFromReader<Folder>(sr);
            }
        }
    }
}
