using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PureLib.Common;

namespace WhereAreThem.WebViewer.Models {
    public class PropertyInfo {
        public int FolderCount { get; set; }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }

        public string FolderCountString {
            get {
                return ToNumber(FolderCount);
            }
        }
        public string FileCountString {
            get {
                return ToNumber(FileCount);
            }
        }
        public string TotalSizeString {
            get {
                return ToNumber(TotalSize);
            }
        }
        public string TotalSizeFriendlyString {
            get {
                return Utility.ToFriendlyString(TotalSize);
            }
        }

        private string ToNumber(long value) {
            return value.ToString("n0");
        }
    }
}