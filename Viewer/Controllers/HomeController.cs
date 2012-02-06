using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PureLib.Common;
using WhereIsThem.Model;
using WhereIsThem.Viewer.Models;

namespace WhereIsThem.Viewer.Controllers {
    public class HomeController : Controller {
        public ViewResult Index() {
            return View(List.MachineNames);
        }

        public ViewResult Explorer(string machineName, string path) {
            ViewBag.MachineName = machineName;
            string machine = List.MachineNames.SingleOrDefault(n => n == machineName);
            if (machine.IsNullOrEmpty())
                return null;

            Folder folder = null;
            if (path.IsNullOrEmpty())
                folder = new Folder() {
                    Name = machineName,
                    Folders = List.GetDrives(machineName)
                };
            else {
                string[] parts = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                Folder drive = List.GetDrive(machineName, "{0}\\".FormatWith(parts.First()));
                if (drive == null)
                    return null;

                List<Folder> stack = new List<Folder>();
                stack.Add(drive);
                for (int i = 1; i < parts.Length; i++) {
                    stack.Add(stack.Last().Folders.Single(f => f.Name == parts[i]));
                }
                ViewBag.Stack = stack;
                folder = stack.Last();
            }
            return View(folder);
        }
    }
}
