using System;
using System.IO;
using WhereAreThem.Model.Models;
using ZstdSharp;

namespace WhereAreThem.Model.Persistences {
    public sealed class ZstdPersistence<T> : IPersistence where T : IFormatProvider {
        private readonly T _streamPersistence = Activator.CreateInstance<T>();

        public void Save(Folder folder, string path) {
            using (FileStream fs = new(path, FileMode.Create))
            using (CompressionStream zstdStream = new(fs)) {
                _streamPersistence.Save(folder, zstdStream);
            }
        }

        public Folder Load(string path) {
            using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
            using (DecompressionStream zstdStream = new(fs)) {
                return _streamPersistence.Load(zstdStream);
            }
        }
    }
}
