using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PureLib.Common;

namespace WhereAreThem.Viewer.Models {
    public class PropertyInfo {
        public int FolderCount { get; set; }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }

        public string TotalSizeInByte {
            get {
                return TotalSize.ToString("n0");
            }
        }
        public string TotalSizeString {
            get {
                return Utility.ToFriendlyString(TotalSize);
            }
        }
    }
}