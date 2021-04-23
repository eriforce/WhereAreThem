using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PureLib.Common;
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
            Folder folder = GetFolder(machineName, path, out List<Folder> stack);

            if (searchPattern.IsNullOrEmpty()) {
                if (stack != null)
                    ViewBag.Stack = stack;
                return (folder == null) ? null : View(folder);
            }
            else {
                stack.RemoveAt(stack.Count - 1);
                return View(Extensions.ActionSearchResult, folder.Search(stack, searchPattern, true, true));
            }
        }

        public JsonResult GetProperties(string machineName, string path, string[] selectedItems) {
            Folder parent = GetFolder(machineName, path, out List<Folder> stack);
            IEnumerable<FileSystemItem> items = (parent.Files ?? new List<File>())
                .Cast<FileSystemItem>()
                .Concat(parent.Folders ?? new List<Folder>())
                .Where(i => selectedItems.Contains(i.Name));

            return Json(new PropertyInfo(items));
        }

        private Folder GetFolder(string machineName, string path, out List<Folder> stack) {
            if (!List.MachineNames.Any(n => n == machineName))
                throw new ArgumentException("Machine name '{0}' cannot be found.".FormatWith(machineName));

            ViewBag.MachineName = machineName;
            return List.GetFolder(machineName, path, out stack);
        }
    }
}
