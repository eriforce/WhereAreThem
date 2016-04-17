using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public class CompressedPersistence<T> : IPersistence where T : IStreamPersistence {
        private T _streamPersistence = Activator.CreateInstance<T>();

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void Save(Folder folder, string path) {
            using (MemoryStream ms = new MemoryStream()) {
                _streamPersistence.Save(folder, ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (FileStream fs = new FileStream(path, FileMode.Create)) {
                    using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress)) {
                        ms.CopyTo(gzipStream);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public Folder Load(string path) {
            using (MemoryStream ms = new MemoryStream()) {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress)) {
                        gzipStream.CopyTo(ms);
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);
                return _streamPersistence.Load(ms);
            }
        }
    }
}
