using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PureLib.Common;
using WhereAreThem.Model;
using WhereAreThem.Viewer.Models;

namespace WhereAreThem.Viewer.Controllers {
    [Authorize]
    public class HomeController : Controller {
        public ViewResult Index() {
            return View(List.MachineNames);
        }

        public RedirectResult ToggleView() {
            if (Session[Extensions.DetailsViewSessionName] == null)
                Session[Extensions.DetailsViewSessionName] = new object();
            else
                Session[Extensions.DetailsViewSessionName] = null;
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
                SearchInFolder(folder, stack, searchPattern.WildcardToRegex(), result);
                return View(Extensions.ActionSearchResult, result);
            }
        }

        public JsonResult GetProperties(string machineName, string path, string[] selectedItems) {
            List<Folder> stack = null;
            Folder folder = GetFolder(machineName, path, out stack);

            List<File> files = folder.Files == null ? new List<File>() :
                folder.Files.Where(i => selectedItems.Contains(i.Name)).ToList();
            List<Folder> folders = folder.Folders == null ? new List<Folder>() :
                folder.Folders.Where(i => selectedItems.Contains(i.Name)).ToList();
            PropertyInfo info = new PropertyInfo() {
                FileCount = folders.Sum(f => GetFileCount(f)) + files.Count,
                FolderCount = folders.Sum(f => GetFolderCount(f)),
                TotalSize = files.Sum(f => f.Size) + folders.Sum(f => f.Size)
            };
            if (selectedItems.Length > 1)
                info.FolderCount += folders.Count;
            return Json(info);
        }

        private int GetFileCount(Folder folder) {
            int count = folder.Files == null ? 0 : folder.Files.Count;
            if (folder.Folders != null)
                count += folder.Folders.Sum(f => GetFileCount(f));
            return count;
        }

        private int GetFolderCount(Folder folder) {
            return folder.Folders == null ? 0 : (folder.Folders.Count + folder.Folders.Sum(f => GetFolderCount(f)));
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
