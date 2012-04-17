using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;

namespace WhereAreThem.Model {
    public class StringPersistenceWithCompression : CompressedPersistenceBase, IPersistence {
        private StringPersistence _stringPersistence = new StringPersistence();

        public void Save(Folder folder, string path) {
            using (MemoryStream stream = new MemoryStream()) {
                _stringPersistence.Save(folder, new StreamWriter(stream));
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
                return _stringPersistence.Load(new StreamReader(stream));
            }
        }
    }
}
