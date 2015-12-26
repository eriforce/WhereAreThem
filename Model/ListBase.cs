using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PureLib.Common;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public abstract class ListBase {
        protected string _outputPath;
        protected IPersistence _persistence;

        public ListBase(string outputPath, IPersistence persistence) {
            _outputPath = outputPath;
            _persistence = persistence;
        }

        protected string GetListPath(string machineName, string driveLetter, DriveType driveType) {
            if (driveType == DriveType.Network)
                return Path.Combine(_outputPath, "{0}.{1}.{2}".FormatWith(driveLetter, driveType, Constant.ListExt));
            return Path.Combine(_outputPath, machineName, "{0}.{1}.{2}".FormatWith(driveLetter, driveType, Constant.ListExt));
        }
    }
}
