﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WebViewer.Models {
    public static class Extensions {
        public const string ControllerHome = "Home";
        public const string ActionIndex = "Index";
        public const string ActionExplorer = "Explorer";
        public const string ActionSearchResult = "SearchResult";
        public const string ExplorerViewSessionName = "ExplorerView";

        public static string ToExplorerString(this DateTime time) => time.ToString("yyyy-MM-dd HH:mm:ss");

        public static string GetTitle(this string titleName, string path, string machineName) => $"{titleName} - {path} | {machineName}";

        public static ExplorerView GetExplorerView(this HttpSessionStateBase session) {
            return (ExplorerView)((session[ExplorerViewSessionName] == null)
                ? Enum.Parse(typeof(ExplorerView), ConfigurationManager.AppSettings["explorerView"])
                : session[ExplorerViewSessionName]);
        }

        public static string GetFullPath(this Folder folder, List<Folder> stack) {
            if (stack.Contains(folder))
                stack = stack.Take(stack.LastIndexOf(folder)).ToList();
            List<string> parts = stack.Select(f => f.Name).ToList();
            parts.Add(folder.Name);
            return Path.Combine(parts.ToArray());
        }
    }
}