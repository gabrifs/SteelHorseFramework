using SteelHorse.Framework.Tags;
using UnityEditor;

namespace SteelHorse.Framework.Editor
{
    // Multiple TagDatabase assets can exist in a project (testing, backups, etc.) — this tracks
    // which single one is "active" via EditorBuildSettings' named config-object slot, the same
    // mechanism Unity packages like Addressables use to point at a singleton settings asset.
    internal static class TagDatabaseLocator
    {
        private const string ActiveConfigName = "SteelHorse.Framework.Tags.ActiveTagDatabase";

        public static TagDatabase Find()
        {
            EditorBuildSettings.TryGetConfigObject(ActiveConfigName, out TagDatabase active);
            return active;
        }

        public static bool IsActive(TagDatabase database)
        {
            return database != null && Find() == database;
        }

        public static void SetActive(TagDatabase database)
        {
            EditorBuildSettings.AddConfigObject(ActiveConfigName, database, true);
        }
    }
}
