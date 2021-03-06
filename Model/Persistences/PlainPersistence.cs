﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public class PlainPersistence<T> : IPersistence where T : IFormatProvider {
        private readonly T _streamPersistence = Activator.CreateInstance<T>();

        public void Save(Folder folder, string path) {
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                _streamPersistence.Save(folder, stream);
            }
        }

        public Folder Load(string path) {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                return _streamPersistence.Load(stream);
            }
        }
    }
}
