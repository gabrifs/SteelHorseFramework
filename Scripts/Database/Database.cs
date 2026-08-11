using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteelHorse.Framework.Database
{
    public abstract class Database : ScriptableObject
    {
    }

    public abstract class Database<TEntry> : Database
    {
        public IReadOnlyList<TEntry> Entries { get { return _entries; } }

        // Formerly "_tags" on TagDatabase before it became a Database<TEntry> subclass — keeps existing
        // Tag Database.asset entries intact across the migration.
        [FormerlySerializedAs("_tags")]
        [SerializeField] protected List<TEntry> _entries = new List<TEntry>();
    }

    // Specialization of Database<TEntry> for TEntry types that carry a DatabaseEntry.Key, adding a
    // generic key lookup + duplicate-key validation once instead of every concrete database
    // reimplementing both. Database<TEntry> itself stays unconstrained so it can still back a
    // database of plain [Serializable] entries that have no Key at all.
    public abstract class KeyedDatabase<TEntry> : Database<TEntry> where TEntry : DatabaseEntry
    {
        public bool TryGetByKey(string key, out TEntry entry)
        {
            entry = _entries.Find(e => e != null && e.Key == key);
            return entry != null;
        }

#if UNITY_EDITOR
        // Unity's list "+" button duplicates the previous element's reference, so warn early rather
        // than let the same entry asset (or two with a hand-typed matching key) end up twice in the list.
        private void OnValidate()
        {
            HashSet<string> seenKeys = new HashSet<string>();
            foreach (TEntry entry in _entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key))
                    continue;

                if (!seenKeys.Add(entry.Key))
                    Debug.LogWarning($"[{GetType().Name}] Duplicate key '{entry.Key}' in '{name}' — keys must be unique.", this);
            }
        }
#endif
    }
}
