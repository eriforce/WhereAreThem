using System.IO;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public interface IFormatProvider {
        void Save(Folder folder, Stream stream);
        Folder Load(Stream stream);
    }
}
