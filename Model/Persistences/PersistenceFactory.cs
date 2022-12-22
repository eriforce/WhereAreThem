using System;
using System.Collections.Generic;
using System.Configuration;

namespace WhereAreThem.Model.Persistences {
    public class PersistenceFactory {
        private static readonly Dictionary<string, Type> persistenceTypes = new(StringComparer.OrdinalIgnoreCase) {
            { PersistenceType.Bin.ToString(), typeof(BinaryProvider) },
        };

        public static IPersistence Persistence { get; private set; }

        static PersistenceFactory() {
            Type formaterType = persistenceTypes[ConfigurationManager.AppSettings["format"]];
            Type persistenceType = typeof(CompressedPersistence<>).MakeGenericType(formaterType);

            Persistence = Activator.CreateInstance(persistenceType) as IPersistence;
        }
    }
}
