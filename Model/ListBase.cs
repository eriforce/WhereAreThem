using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public abstract class ListBase {
        protected string _outputPath;
        protected IPersistence _persistence;

        public event StringEventHandler PrintLine;

        public ListBase(string outputPath, IPersistence persistence) {
            _outputPath = outputPath;
            _persistence = persistence;
        }

        protected void OnPrintLine(string s) {
            if (PrintLine != null)
                PrintLine(this, new StringEventArgs() { String = s });
        }

        protected string GetListPath(string machineName, string driveLetter, DriveType driveType) {
            return Path.Combine(_outputPath, machineName, string.Join(".", driveLetter, driveType, Constant.ListExt));
        }
    }
}
