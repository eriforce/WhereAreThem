using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public static class Helper {
        public static Computer GetComputer(this List<Folder> stack) {
            return (Computer)stack[0];
        }

        public static DriveModel GetDrive(this List<Folder> stack) {
            return (DriveModel)stack[1];
        }

        public static Folder GetParent(this List<Folder> stack) {
            return (stack.Count > 1) ? stack[stack.Count - 2] : stack[0];
        }

        public static IEnumerable<Folder> GetParentStack(this List<Folder> stack) {
            return stack.Take(stack.Count - 1);
        }
    }
}
