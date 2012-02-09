using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public interface IPersistence {
        void Save(Folder folder, string path);
        Folder Load(string path);
    }
}
