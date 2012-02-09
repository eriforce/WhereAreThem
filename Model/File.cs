using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WhereAreThem.Model {
    [Serializable]
    public class File {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public DateTime ModifiedDateUtc { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
