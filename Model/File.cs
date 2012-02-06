using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WhereIsThem.Model {
    [Serializable]
    public class File {
        private const string fileFormat = "{1}{0}{2}{0}{3}{0}{4}";

        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public DateTime ModifiedDateUtc { get; set; }

        public override string ToString() {
            return string.Format(fileFormat, Constant.ColumnSeparator,
                Name, Size, CreatedDateUtc.Ticks, ModifiedDateUtc.Ticks);
        }
    }
}
