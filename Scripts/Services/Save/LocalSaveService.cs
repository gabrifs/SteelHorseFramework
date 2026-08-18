using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SteelHorse.Framework.Services.Save
{
    public static class LocalSaveService<T> where T : new()
    {
        private static readonly Dictionary<string, T> _cache = new();
        private static readonly Dictionary<int, Func<T, T>> _migrations = new();

        // Sugar for the common single-slot case - equivalent to Get("save.json").
        public static T Current => Get("save.json");

        // Returns the cached instance for this file, lazily loading (and migrating) it from
        // disk on first access. Each file name gets its own independent cache slot, unlike
        // the old single `_current` field shared by every file of type T.
        public static T Get(string fileName = "save.json")
        {
            if (!_cache.TryGetValue(fileName, out T data))
                data = LoadFromDisk(fileName);

            return data;
        }

        public static void Load(string fileName = "save.json")
        {
            LoadFromDisk(fileName);
        }

        public static void Save(string fileName = "save.json")
        {
            SaveToDisk(Get(fileName), fileName);
        }

        // For data types with no public mutable fields (e.g. constructed once via a
        // parameterized constructor rather than tweaked via `Current.Field = x`), saving a
        // freshly-built instance directly is simpler than reflecting it into the cache.
        public static void Save(T data, string fileName = "save.json")
        {
            _cache[fileName] = data;
            SaveToDisk(data, fileName);
        }

        // Registers a migration step run on Load when a loaded IVersionedSaveData instance's
        // SchemaVersion equals fromVersion. Chained automatically until no further migration
        // is registered for the resulting version. JsonUtility deserializes old JSON directly
        // into the current shape of T before this runs, so a migration can only work with
        // fields that still exist on T by name - it can't recover a renamed/removed field.
        public static void RegisterMigration(int fromVersion, Func<T, T> migrate)
        {
            _migrations[fromVersion] = migrate;
        }

        public static void Delete(string fileName)
        {
            string path = GetPath(fileName);

            if (File.Exists(path))
                File.Delete(path);

            _cache.Remove(fileName);
        }

        public static string[] ListFiles(string searchPattern = "*.json")
        {
            if (!Directory.Exists(Application.persistentDataPath))
                return new string[0];

            string[] paths = Directory.GetFiles(Application.persistentDataPath, searchPattern);

            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = Path.GetFileName(paths[i]);
            }

            return paths;
        }

        private static T LoadFromDisk(string fileName)
        {
            string path = GetPath(fileName);
            T data;

            if (!File.Exists(path))
            {
                data = new T();
            }
            else
            {
                try
                {
                    var raw = File.ReadAllText(path);
                    data = JsonUtility.FromJson<T>(SaveEncryption.Decrypt(raw));
                    data = Migrate(data, fileName);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LocalSaveService<{typeof(T).Name}>] Failed to load '{fileName}': {e.Message}. Using defaults.");
                    data = new T();
                }
            }

            _cache[fileName] = data;
            return data;
        }

        private static T Migrate(T data, string fileName)
        {
            if (data is not IVersionedSaveData versioned)
                return data;

            while (_migrations.TryGetValue(versioned.SchemaVersion, out Func<T, T> migrate))
            {
                int fromVersion = versioned.SchemaVersion;
                data = migrate(data);
                versioned = data as IVersionedSaveData;

                if (versioned == null || versioned.SchemaVersion == fromVersion)
                {
                    Debug.LogError($"[LocalSaveService<{typeof(T).Name}>] Migration from schema version {fromVersion} for '{fileName}' did not advance the schema version. Aborting further migration.");
                    break;
                }
            }

            return data;
        }

        private static void SaveToDisk(T data, string fileName)
        {
            try
            {
                var json = JsonUtility.ToJson(data);
                File.WriteAllText(GetPath(fileName), SaveEncryption.Encrypt(json));
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveService<{typeof(T).Name}>] Failed to save '{fileName}': {e.Message}");
            }
        }

        private static string GetPath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName);
    }
}
