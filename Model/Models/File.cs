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
        public Dictionary<string, string> Data { get; set; }

        public DateTime ModifiedDate
            => ModifiedDateUtc.ToLocalTime();
        public string Extension
            => Path.GetExtension(Name);
        public override long Size
            => FileSize;
    }
}
