using System;
using System.Collections.Generic;
using System.Configuration;

namespace WhereAreThem.Model.Persistences {
    public class PersistenceFactory {
        private static readonly Dictionary<string, Type> persistenceTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) {
            { PersistenceType.Bin.ToString(), typeof(BinaryProvider) },
        };

        public static IPersistence Persistence { get; private set; }

        static PersistenceFactory() {
            Type formaterType = persistenceTypes[ConfigurationManager.AppSettings["format"]];
            bool enableCompression = bool.Parse(ConfigurationManager.AppSettings["enableCompression"]);
            Type persistenceType = (enableCompression ? typeof(CompressedPersistence<>) : typeof(PlainPersistence<>)).MakeGenericType(formaterType);

            Persistence = Activator.CreateInstance(persistenceType) as IPersistence;
        }
    }
}
