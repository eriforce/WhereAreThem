using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PureLib.Common;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;
using WhereAreThem.WebViewer.Models;

namespace WhereAreThem.WebViewer.Controllers {
    [Authorize]
    public class HomeController : Controller {
        public ViewResult Index() {
            return View(List.MachineNames);
        }

        public RedirectResult ChangeView(ExplorerView view) {
            Session[Extensions.ExplorerViewSessionName] = view;
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ViewResult Explorer(string machineName, string path, string searchPattern) {
            List<Folder> stack = null;
            Folder folder = GetFolder(machineName, path, out stack);

            if (searchPattern.IsNullOrEmpty()) {
                if (stack != null)
                    ViewBag.Stack = stack;
                return (folder == null) ? null : View(folder);
            }
            else {
                Dictionary<FileSystemItem, string> result = new Dictionary<FileSystemItem, string>();
                stack.Remove(stack.Last());
                SearchInFolder(folder, stack, searchPattern.WildcardToRegex(), result);
                return View(Extensions.ActionSearchResult, result);
            }
        }

        public JsonResult GetProperties(string machineName, string path, string[] selectedItems) {
            List<Folder> stack = null;
            Folder parent = GetFolder(machineName, path, out stack);
            return Json(new PropertyInfo(parent, selectedItems));
        }

        private Folder GetFolder(string machineName, string path, out List<Folder> stack) {
            if (!List.MachineNames.Any(n => n == machineName))
                throw new ArgumentException("Machine name '{0}' cannot be found.".FormatWith(machineName));

            ViewBag.MachineName = machineName;
            return List.GetFolder(machineName, path, out stack);
        }

        private void SearchInFolder(Folder folder, List<Folder> folderStack, string pattern, Dictionary<FileSystemItem, string> result) {
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
