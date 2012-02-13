using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhereAreThem.Viewer.Models {
    public class PropertyInfo {
        public int FolderCount { get; set; }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
    }
}