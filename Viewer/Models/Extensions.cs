using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WhereAreThem.Model;
using System.Text.RegularExpressions;

namespace WhereAreThem.Viewer.Models {
    public static class Extensions {
        public const string Home = "Home";
        public const string Explorer = "Explorer";
        public const string SearchResult = "SearchResult";

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
    }
}