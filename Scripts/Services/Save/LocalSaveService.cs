using System;
using System.IO;
using UnityEngine;

namespace SteelHorse.Framework.Services.Save
{
    public static class LocalSaveService<T> where T : new()
    {
        private static T _current;

        public static T Current
        {
            get
            {
                if (_current == null)
                    Load();
                return _current;
            }
        }

        public static void Load(string fileName = "save.json")
        {
            string path = GetPath(fileName);

            if (!File.Exists(path))
            {
                _current = new T();
                return;
            }

            try
            {
                var raw = File.ReadAllText(path);
                _current = JsonUtility.FromJson<T>(SaveEncryption.Decrypt(raw));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LocalSaveService<{typeof(T).Name}>] Failed to load '{fileName}': {e.Message}. Using defaults.");
                _current = new T();
            }
        }

        public static void Save(string fileName = "save.json")
        {
            try
            {
                var json = JsonUtility.ToJson(_current);
                File.WriteAllText(GetPath(fileName), SaveEncryption.Encrypt(json));
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveService<{typeof(T).Name}>] Failed to save '{fileName}': {e.Message}");
            }
        }

        // For data types with no public mutable fields (e.g. constructed once via a
        // parameterized constructor rather than tweaked via `Current.Field = x`), saving a
        // freshly-built instance directly is simpler than reflecting it into `_current`.
        public static void Save(T data, string fileName = "save.json")
        {
            _current = data;
            Save(fileName);
        }

        public static void Delete(string fileName)
        {
            string path = GetPath(fileName);

            if (File.Exists(path))
                File.Delete(path);
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

        private static string GetPath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName);
    }
}
