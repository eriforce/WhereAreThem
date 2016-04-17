using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public interface IStreamPersistence {
        void Save(Folder folder, Stream stream);
        Folder Load(Stream stream);
    }
}
