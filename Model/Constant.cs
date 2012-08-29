using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public static class Constant {
        public const string ListExt = "wat";

        private static IPersistence _persistence;
        public static IPersistence Persistence {
            get {
                if (_persistence == null) {
                    Type type = LoadType("persistence");
                    if (type.IsGenericType)
                        type = type.MakeGenericType(LoadType("persistenceArgs"));
                    _persistence = Activator.CreateInstance(type) as IPersistence;
                }
                return _persistence;
            }
        }

        private static Type LoadType(string key) {
            string typeName = ConfigurationManager.AppSettings[key];
            Type type = Type.GetType(typeName);
            if (type == null)
                throw new ApplicationException("Cannot load {0}.".FormatWith(typeName));
            return type;
        }
    }
}
