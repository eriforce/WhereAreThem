using System.IO;
using PureLib.Common;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public abstract class ListBase {
        protected const string SharedMachineName = "[Shared]";

        protected IPersistence Persistence { get; private set; }
        protected string OutputPath { get; private set; }
        protected string SharedPath { get; private set; }

        public ListBase(string outputPath, IPersistence persistence) {
            OutputPath = outputPath;
            Persistence = persistence;

            SharedPath = Path.Combine(OutputPath, SharedMachineName);

            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);
            if (!Directory.Exists(SharedPath))
                Directory.CreateDirectory(SharedPath);
        }

        protected string GetListPath(string machineName, string driveLetter, DriveType driveType) {
            var folderName = IsShared(driveType) ? SharedMachineName : machineName;
            return Path.Combine(OutputPath, folderName, "{0}.{1}.{2}".FormatWith(driveLetter, driveType, Constant.ListExt));
        }

        private bool IsShared(DriveType driveType) {
            return driveType == DriveType.Network;
        }
    }
}
