using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace WhereAreThem.Model {
    public class CompressedPersistence<T> : IPersistence where T : IStreamPersistence {
        private T _streamPersistence = Activator.CreateInstance<T>();

        public void Save(Folder folder, string path) {
            using (MemoryStream ms = new MemoryStream()) {
                _streamPersistence.Save(folder, ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (FileStream fs = new FileStream(path, FileMode.Create)) {
                    GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress);
                    ms.CopyTo(gzipStream);
                }
            }
        }

        public Folder Load(string path) {
            using (MemoryStream ms = new MemoryStream()) {
                using (FileStream fs = new FileStream(path, FileMode.Open)) {
                    GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress);
                    gzipStream.CopyTo(ms);
                }
                ms.Seek(0, SeekOrigin.Begin);
                return _streamPersistence.Load(ms);
            }
        }
    }
}
