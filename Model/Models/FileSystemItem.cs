using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WhereAreThem.Model.Models {
    public abstract class FileSystemItem : IComparable<FileSystemItem> {
        public string Name { get; set; }
        public DateTime CreatedDateUtc { get; set; }

        public DateTime CreatedDate => CreatedDateUtc.ToLocalTime();
        public abstract long Size { get; }

        public bool NameEquals(string name) {
            return Name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(FileSystemItem other) {
            if (double.TryParse(Name, out double xValue) && double.TryParse(other.Name, out double yValue))
                return xValue.CompareTo(yValue);
            else
                return Name.CompareTo(other.Name);
        }

        public override string ToString() {
            return Name;
        }
    }
}
