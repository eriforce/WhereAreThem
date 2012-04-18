using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PureLib.Common;

namespace WhereAreThem.Model {
    public static class Constant {
        public const string ListExt = "wat";

        private static IPersistence _persistence;
        public static IPersistence Persistence {
            get {
                if (_persistence == null) {
                    string persistenceType = ConfigurationManager.AppSettings["persistence"];
                    string typeArgs = ConfigurationManager.AppSettings["persistenceArgs"];

                    Type type = Type.GetType(persistenceType);
                    if (!typeArgs.IsNullOrEmpty())
                        type = type.MakeGenericType(Type.GetType(typeArgs));
                    _persistence = Activator.CreateInstance(type) as IPersistence;
                }
                return _persistence;
            }
        }
    }
}
