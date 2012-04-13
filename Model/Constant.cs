using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public static class Constant {
        public const string ListExt = "wat";

        public static IPersistence GetPersistence() {
            Type type = Type.GetType(ConfigurationManager.AppSettings["persistence"]);
            return Activator.CreateInstance(type) as IPersistence;
        }
    }
}
