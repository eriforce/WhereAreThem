using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using PureLib.Common;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WebViewer.Models {
    public static class List {
        private static Loader _loader = new Loader(
            ConfigurationManager.AppSettings["path"].WrapPath(), Constant.Persistence);
  
        public static string[] MachineNames {
            get {
                return _loader.MachineNames;
            }
        }

        public static Folder GetFolder(string machineName, string path, out List<Folder> stack) {
            stack = null;
            if (path.IsNullOrEmpty())
                return new Folder() {
                    Name = machineName,
                    Folders = new List<Folder>(_loader.GetDrives(machineName))
                };
            else {
                string[] parts = path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                Folder drive = _loader.GetDrive(machineName, parts.First());
                if (drive == null)
                    return null;

                stack = new List<Folder>();
                stack.Add(drive);
                for (int i = 1; i < parts.Length; i++) {
                    stack.Add(stack.Last().Folders.Single(f => f.NameEquals(parts[i])));
                }
                return stack.Last();
            }
        }
    }
}