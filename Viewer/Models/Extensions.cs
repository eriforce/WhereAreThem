using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using PureLib.Common;
using WhereAreThem.Model;

namespace WhereAreThem.Viewer.Models {
    public static class Extensions {
        public const string ControllerHome = "Home";
        public const string ActionIndex = "Index";
        public const string ActionExplorer = "Explorer";
        public const string ActionSearchResult = "SearchResult";
        public const string DetailsViewSessionName = "DetailsView";

        public static string ToLocalTimeString(this DateTime utcTime) {
            DateTimeOffset offset = new DateTimeOffset(utcTime, TimeZoneInfo.Utc.GetUtcOffset(utcTime));
            return offset.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetFullPath(this Folder folder, List<Folder> stack) {
            if (stack.Contains(folder))
                stack = stack.Take(stack.IndexOf(folder)).ToList();
            List<string> parts = stack.Select(f => f.Name).ToList();
            parts.Add(folder.Name);
            return Path.Combine(parts.ToArray());
        }

        public static string WildcardToRegex(this string pattern) {
            return Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
        }

        public static string GetTitle(this string titleName, string path, string machineName) {
            return "{0} - {1} | {2}".FormatWith(titleName, path, machineName);
        }
    }
}