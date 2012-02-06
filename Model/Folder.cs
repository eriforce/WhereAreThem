using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WhereIsThem.Model {
    [Serializable]
    public class Folder {
        private const string folderFormat = "{1}{0}{2}";

        public string Name { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public List<Folder> Folders { get; set; }
        public List<File> Files { get; set; }

        public override string ToString() {
            return string.Format(folderFormat, Constant.ColumnSeparator,
                Name, CreatedDateUtc.Ticks);
        }
    }
}
