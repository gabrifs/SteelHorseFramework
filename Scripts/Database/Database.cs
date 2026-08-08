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
}
