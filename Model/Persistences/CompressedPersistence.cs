using System;
using System.IO;
using System.IO.Compression;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public class CompressedPersistence<T> : IPersistence where T : IFormatProvider {
        private readonly T _streamPersistence = Activator.CreateInstance<T>();

        public void Save(Folder folder, string path) {
            using (FileStream fs = new(path, FileMode.Create))
            using (GZipStream gzipStream = new(fs, CompressionMode.Compress)) {
                _streamPersistence.Save(folder, gzipStream);
            }
        }

        public Folder Load(string path) {
            using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
            using (GZipStream gzipStream = new(fs, CompressionMode.Decompress)) {
                return _streamPersistence.Load(gzipStream);
            }
        }
    }
}
