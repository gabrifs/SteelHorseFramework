namespace SteelHorse.Framework.Services.Database
{
    public interface IDatabaseService
    {
        void Setup();

        TDatabase Get<TDatabase>() where TDatabase : SteelHorse.Framework.Database.Database;
        bool TryGet<TDatabase>(out TDatabase database) where TDatabase : SteelHorse.Framework.Database.Database;
    }
}
