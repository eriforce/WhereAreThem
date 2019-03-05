using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using PureLib.Common;
using WhereAreThem.Model.Models;
using IO = System.IO;

namespace WhereAreThem.WinViewer.Model {
    public static class Helper {
        public static Computer GetComputer(this List<Folder> stack) {
            return (Computer)stack[0];
        }

        public static DriveModel GetDrive(this List<Folder> stack) {
            return (stack.Count > 1) ? (DriveModel)stack[1] : null;
        }

        public static Folder GetParent(this List<Folder> stack) {
            return (stack.Count > 1) ? stack[stack.Count - 2] : stack[0];
        }

        public static IEnumerable<Folder> GetParentStack(this List<Folder> stack) {
            return stack.Take(stack.Count - 1);
        }

        public static void LocateOnDisk(this FileSystemItem item, List<Folder> stack, Window owner) {
            string path = IO.Path.Combine(stack.Select(f => f.Name).Concat(new[] { item.Name }).ToArray());
            bool itemExists = ((item is File) && IO.File.Exists(path))
                || ((item is Folder) && IO.Directory.Exists(path));
            if (itemExists)
                Process.Start("explorer.exe", @"/select,{0}".FormatWith(path));
            else
                MessageBox.Show(owner, "{0} doesn't exist on your disk.".FormatWith(path));
        }
    }
}
