using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PureLib.Common;

namespace WhereAreThem.Model {
    public static class Constant {
        public const string ListExt = "wat";

        public static IPersistence GetPersistence() {
            string persistenceType = ConfigurationManager.AppSettings["persistence"];
            string typeArgs = ConfigurationManager.AppSettings["persistenceArgs"];

            Type type = Type.GetType(persistenceType);
            if (!typeArgs.IsNullOrEmpty())
                type = type.MakeGenericType(Type.GetType(typeArgs));
            return Activator.CreateInstance(type) as IPersistence;
        }
    }
}
