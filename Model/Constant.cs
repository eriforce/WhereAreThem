using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public static class Constant {
        private static readonly Dictionary<string, Type> persistenceTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) {
            { PersistenceType.Bin.ToString(), typeof(BinaryPersistence) },
            { PersistenceType.Txt.ToString(), typeof(TextPersistence) },
            { PersistenceType.Json.ToString(), typeof(JsonPersistence) },
            { PersistenceType.Xml.ToString(), typeof(XmlPersistence) },
            { PersistenceType.JsonNet.ToString(), typeof(JsonNetPersistence) },
        };
        private static readonly bool enableCompression = bool.Parse(ConfigurationManager.AppSettings["enableCompression"]);

        public const string ListExt = "wat";

        public static IPersistence Persistence { get; private set; }
        public static string Path {
            get { return ConfigurationManager.AppSettings["path"].MakeFullPath(); }
        }

        static Constant() {
            bool enableCompression = bool.Parse(ConfigurationManager.AppSettings["enableCompression"]);
            Type type = persistenceTypes[ConfigurationManager.AppSettings["persistenceType"]];
            if (enableCompression)
                type = typeof(CompressedPersistence<>).MakeGenericType(type);
            Persistence = Activator.CreateInstance(type) as IPersistence;
        }
    }
}
