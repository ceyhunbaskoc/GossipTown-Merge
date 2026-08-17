using System;

namespace Core.GridSystem
{
    public readonly struct ItemIdentifier : IEquatable<ItemIdentifier>
    {
        public readonly string Id;
        public readonly int Level;

        public ItemIdentifier(string id, int level)
        {
            Id = id;
            Level = level;
        }
        public bool Equals(ItemIdentifier other)
        {
            return Id == other.Id && Level == other.Level;
        }

        public override bool Equals(object obj) => obj is ItemIdentifier other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Level);
        }
    }
}