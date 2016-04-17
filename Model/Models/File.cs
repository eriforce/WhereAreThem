using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WhereAreThem.Model.Models {
    [DataContract]
    public class File : FileSystemItem {
        [DataMember]
        public long FileSize { get; set; }
        [DataMember]
        public DateTime ModifiedDateUtc { get; set; }
        [DataMember]
        public string Description { get; set; }

        public DateTime ModifiedDate {
            get { return ModifiedDateUtc.ToLocalTime(); }
        }
        public string Extension {
            get { return Path.GetExtension(Name); }
        }
        public override long Size {
            get { return FileSize; }
        }
    }
}
