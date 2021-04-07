using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public interface IPersistence {
        void Save(Folder folder, string path);
        Folder Load(string path);
    }
}
