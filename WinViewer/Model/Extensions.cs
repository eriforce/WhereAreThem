using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using WhereAreThem.Model.Models;
using IO = System.IO;

namespace WhereAreThem.WinViewer.Model {
    public static class Extensions {
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

        public static bool Exists(this List<Folder> stack) {
            if (!stack.Any())
                return false;
            if (stack.Count == 1)
                return true;

            for (int i = stack.Count - 1; i > 0; i--) {
                if (!stack[i - 1].Folders.Contains(stack[i]))
                    return false;
            }
            return true;
        }

        public static void LocateOnDisk(this FileSystemItem item, List<Folder> stack, Window owner) {
            string path = IO.Path.Combine(stack.Select(f => f.Name).Concat(new[] { item.Name }).ToArray());
            bool itemExists = ((item is File) && IO.File.Exists(path))
                || ((item is Folder) && IO.Directory.Exists(path));
            if (itemExists)
                Process.Start("explorer.exe", $"/select,{path}");
            else
                MessageBox.Show(owner, $"{path} doesn't exist on your disk.");
        }

        public static string GetExceptionText(this Exception ex) {
            StringBuilder sb = new();
            while (ex != null) {
                if (sb.Length > 0)
                    sb.AppendLine($"{Environment.NewLine}-: Inner Exception :-");
                sb.AppendLine(ex.GetType().AssemblyQualifiedName);
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);

                ex = ex.InnerException;
            }
            return sb.ToString();
        }
    }
}
