using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhereAreThem.Model.Persistences {
    public class PersistenceFactory {
        private static readonly Dictionary<string, Type> persistenceTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) {
            { PersistenceType.Bin.ToString(), typeof(BinaryProvider) },
        };

        public static IPersistence Persistence { get; private set; }

        static PersistenceFactory() {
            Type providerType = persistenceTypes[ConfigurationManager.AppSettings["persistenceType"]];
            bool enableCompression = bool.Parse(ConfigurationManager.AppSettings["enableCompression"]);
            Type persistenceType = (enableCompression ? typeof(CompressedPersistence<>) : typeof(PlainPersistence<>)).MakeGenericType(providerType);

            Persistence = Activator.CreateInstance(persistenceType) as IPersistence;
        }
    }
}
