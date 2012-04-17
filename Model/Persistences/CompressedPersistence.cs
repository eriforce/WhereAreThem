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
            using (MemoryStream stream = new MemoryStream()) {
                _streamPersistence.Save(folder, stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (FileStream fs = new FileStream(path, FileMode.Create)) {
                    Compress(stream, fs);
                }
            }
        }

        public Folder Load(string path) {
            using (MemoryStream stream = new MemoryStream()) {
                using (FileStream fs = new FileStream(path, FileMode.Open)) {
                    Decompress(fs, stream);
                }
                stream.Seek(0, SeekOrigin.Begin);
                return _streamPersistence.Load(stream);
            }
        }

        private void Compress(Stream inStream, Stream outStream) {
            using (GZipStream gzipStream = new GZipStream(outStream, CompressionMode.Compress)) {
                inStream.CopyTo(gzipStream);
            }
        }

        private void Decompress(Stream inStream, Stream outStream) {
            using (GZipStream gzipStream = new GZipStream(inStream, CompressionMode.Decompress)) {
                gzipStream.CopyTo(outStream);
            }
        }
    }
}
