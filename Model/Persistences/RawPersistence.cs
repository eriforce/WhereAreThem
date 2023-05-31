using System;
using System.IO;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public sealed class RawPersistence<T> : IPersistence where T : IFormatProvider {
        private readonly T _streamPersistence = Activator.CreateInstance<T>();

        public void Save(Folder folder, string path) {
            using (FileStream stream = new(path, FileMode.Create)) {
                _streamPersistence.Save(folder, stream);
            }
        }

        public Folder Load(string path) {
            using (FileStream stream = new(path, FileMode.Open, FileAccess.Read)) {
                return _streamPersistence.Load(stream);
            }
        }
    }
}
