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

        private static string GetPath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName);
    }
}
