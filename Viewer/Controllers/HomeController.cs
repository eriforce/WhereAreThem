using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PureLib.Common;
using WhereAreThem.Model;
using WhereAreThem.Viewer.Models;
using System.Text.RegularExpressions;

namespace WhereAreThem.Viewer.Controllers {
    [Authorize]
    public class HomeController : Controller {
        public ViewResult Index(string id) {
            return View(List.MachineNames);
        }

        public ViewResult Explorer(string machineName, string path, string searchPattern) {
            if (!List.MachineNames.Any(n => n == machineName))
                return null;

            ViewBag.MachineName = machineName;
            List<Folder> stack = null;
            Folder folder = List.GetFolder(machineName, path, out stack);
            if (searchPattern.IsNullOrEmpty()) {
                if (stack != null)
                    ViewBag.Stack = stack;
                return (folder == null) ? null : View(folder);
            }
            else {
                Dictionary<object, string> result = new Dictionary<object, string>();
                SearchInFolder(folder, stack, searchPattern.WildcardToRegex(), result);
                return View(Extensions.SearchResult, result);
            }
        }

        private void SearchInFolder(Folder folder, List<Folder> folderStack, string pattern, Dictionary<object, string> result) {
            if (folder.Files != null)
                foreach (File f in folder.Files) {
                    if (Regex.IsMatch(f.Name, pattern, RegexOptions.IgnoreCase))
                        result.Add(f, folder.GetFullPath(folderStack));
                }
            if (folder.Folders != null) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (Folder f in folder.Folders) {
                    if (Regex.IsMatch(f.Name, pattern, RegexOptions.IgnoreCase))
                        result.Add(f, folder.GetFullPath(folderStack));
                    SearchInFolder(f, stack, pattern, result);
                }
            }
        }
    }
}
