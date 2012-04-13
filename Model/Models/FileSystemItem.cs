using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public abstract class FileSystemItem : IComparable<FileSystemItem> {
        public string Name { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public DateTime CreatedDate {
            get { return CreatedDateUtc.ToLocalTime(); }
        }
        public abstract long Size { get; }

        public int CompareTo(FileSystemItem other) {
            double xValue;
            double yValue;
            if (double.TryParse(Name, out xValue) && double.TryParse(other.Name, out yValue))
                return xValue.CompareTo(yValue);
            else
                return Name.CompareTo(other.Name);
        }

        public override string ToString() {
            return Name;
        }
    }
}
