using System;
using System.Collections.Generic;
using System.Configuration;

namespace WhereAreThem.Model.Persistences {
    public static class PersistenceFactory {
        private static readonly Dictionary<string, Type> persistenceTypes = new(StringComparer.OrdinalIgnoreCase) {
            { PersistenceType.Gzip.ToString(), typeof(GzipPersistence<>).MakeGenericType(typeof(BinaryProvider)) },
            { PersistenceType.Zstd.ToString(), typeof(ZstdPersistence<>).MakeGenericType(typeof(BinaryProvider)) },
        };

        public static IPersistence Persistence { get; }

        static PersistenceFactory() {
            Type persistenceType = persistenceTypes[ConfigurationManager.AppSettings["format"]];
            Persistence = (IPersistence)Activator.CreateInstance(persistenceType);
        }
    }
}
