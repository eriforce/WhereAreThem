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

        public static string WatRootPath => ConfigurationManager.AppSettings["path"].MakeFullPath();
    }
}

