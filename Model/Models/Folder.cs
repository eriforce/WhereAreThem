using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public class Folder : FileSystemItem {
        public List<Folder> Folders { get; set; }
        public List<File> Files { get; set; }

        public override long Size {
            get {
                long size = 0;
                if (Files != null)
                    size += Files.Sum(f => f.Size);
                if (Folders != null)
                    size += Folders.Sum(f => f.Size);
                return size;
            }
        }
    }
}
