using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhereAreThem.Model.Persistences {
    public class PersistenceFactory {
        private static readonly Dictionary<string, Type> persistenceTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) {
            { PersistenceType.Bin.ToString(), typeof(BinaryPersistence) },
            { PersistenceType.Txt.ToString(), typeof(TextPersistence) },
            { PersistenceType.JsonNet.ToString(), typeof(JsonNetPersistence) },
        };

        public static IPersistence Persistence { get; private set; }

        static PersistenceFactory() {
            bool enableCompression = bool.Parse(ConfigurationManager.AppSettings["enableCompression"]);
            Type type = persistenceTypes[ConfigurationManager.AppSettings["persistenceType"]];
            if (enableCompression)
                type = typeof(CompressedPersistence<>).MakeGenericType(type);
            Persistence = Activator.CreateInstance(type) as IPersistence;
        }
    }
}
