using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public abstract class PersistenceBase : IPersistence, IStreamPersistence {
        public virtual void Save(Folder folder, string path) {
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                Save(folder, stream);
            }
        }

        public virtual Folder Load(string path) {
            using (FileStream stream = new FileStream(path, FileMode.Open)) {
                return Load(stream);
            }
        }

        public abstract void Save(Folder folder, Stream stream);
        public abstract Folder Load(Stream stream);
    }
}
