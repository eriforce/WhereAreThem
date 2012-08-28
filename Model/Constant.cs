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
                if (_persistence == null)
                    LoadPersistence();
                return _persistence;
            }
        }

        private static void LoadPersistence() {
            string persistenceType = ConfigurationManager.AppSettings["persistence"];
            string typeArgs = ConfigurationManager.AppSettings["persistenceArgs"];

            Type type = Type.GetType(persistenceType);
            if (type == null)
                throw new ApplicationException("Cannot load {0}.".FormatWith(persistenceType));
            if (!typeArgs.IsNullOrEmpty())
                type = type.MakeGenericType(Type.GetType(typeArgs));
            _persistence = Activator.CreateInstance(type) as IPersistence;
        }
    }
}
