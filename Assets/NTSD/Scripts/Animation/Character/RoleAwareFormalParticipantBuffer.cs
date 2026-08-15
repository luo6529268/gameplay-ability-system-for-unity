using System;

namespace NTSD.Animation
{
    /// <summary>
    /// Reusable canonical participant storage for the formal collision pass.
    /// It supports ref reads without copying the large participant struct into a
    /// second array. A new build overwrites the active prefix and only clears a
    /// stale tail when the participant count shrinks.
    /// </summary>
    internal sealed class RoleAwareFormalParticipantBuffer
    {
        private RoleAwareFormalParticipant[] items;
        private int previousBuildCount;

        internal RoleAwareFormalParticipantBuffer(int initialCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));

            items = new RoleAwareFormalParticipant[initialCapacity];
        }

        internal int Count { get; private set; }

        internal ref RoleAwareFormalParticipant this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ref items[index];
            }
        }

        internal void EnsureCapacity(int capacity)
        {
            if (capacity <= items.Length)
                return;

            int doubled = items.Length == 0 ? 4 : checked(items.Length * 2);
            Array.Resize(ref items, Math.Max(capacity, doubled));
        }

        internal void BeginBuild()
        {
            previousBuildCount = Count;
            Count = 0;
        }

        internal void Add(in RoleAwareFormalParticipant participant)
        {
            if (Count == items.Length)
                EnsureCapacity(checked(Count + 1));

            items[Count++] = participant;
        }

        internal void CompleteBuild()
        {
            if (Count < previousBuildCount)
                Array.Clear(items, Count, previousBuildCount - Count);

            previousBuildCount = Count;
        }
    }
}
