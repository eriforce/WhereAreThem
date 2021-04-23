namespace WhereAreThem.Model.Plugins {
    public interface IPlugin {
        bool Loaded { get; }
        string[] Extensions { get; }
        string GetDescription(string path);
    }
}
