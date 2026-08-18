namespace SteelHorse.Framework.Services.Save
{
    // Implement on a LocalSaveService<T> data type to opt into schema migration on Load.
    // See LocalSaveService<T>.RegisterMigration.
    public interface IVersionedSaveData
    {
        int SchemaVersion { get; }
    }
}
