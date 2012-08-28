using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model {
    public interface ILoader {
        string[] MachineNames { get; }
        List<Folder> GetDrives(string machineName);
        Folder GetDrive(string machineName, string drive);
    }
}
