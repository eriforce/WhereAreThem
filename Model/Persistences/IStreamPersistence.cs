using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public interface IStreamPersistence {
        void Save(Folder folder, Stream stream);
        Folder Load(Stream stream);
    }
}
