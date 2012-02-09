using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WhereAreThem.Model {
    [Serializable]
    public class Folder {
        public string Name { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public List<Folder> Folders { get; set; }
        public List<File> Files { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
