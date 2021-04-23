using System.Configuration;
using PureLib.Common;

namespace WhereAreThem.Model {
    public static class Constant {
        public const string ListExt = "wat";

        public static string WatRootPath => ConfigurationManager.AppSettings["path"].MakeFullPath();
    }
}

