using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public class CompressedPersistence<T> : IPersistence where T : IFormatProvider {
        private readonly T _streamPersistence = Activator.CreateInstance<T>();

        public void Save(Folder folder, string path) {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress)) {
                _streamPersistence.Save(folder, gzipStream);
            }
        }

        public Folder Load(string path) {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress)) {
                return _streamPersistence.Load(gzipStream);
            }
        }
    }
}
