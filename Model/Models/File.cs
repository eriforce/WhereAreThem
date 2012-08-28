using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WhereAreThem.Model.Models {
    public class File : FileSystemItem {
        [DataMember]
        public long FileSize { get; set; }
        [DataMember]
        public DateTime ModifiedDateUtc { get; set; }
        public DateTime ModifiedDate {
            get { return ModifiedDateUtc.ToLocalTime(); }
        }

        public override long Size {
            get {
                return FileSize;
            }
        }
    }
}
