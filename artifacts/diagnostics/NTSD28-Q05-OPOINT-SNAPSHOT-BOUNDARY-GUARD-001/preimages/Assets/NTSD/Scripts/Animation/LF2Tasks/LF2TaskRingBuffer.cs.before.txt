using System;

namespace NTSD.Animation.LF2Tasks
{
    /// <summary>
    /// Preallocated FIFO used by the battle opoint boundary. It preserves enqueue
    /// order without allocating one managed node per task.
    /// </summary>
    public sealed class LF2TaskRingBuffer
    {
        private LF2TaskBase[] items;
        private int head;
        private int tail;
        private int count;
        private bool capacitySealed;
        private long rejectedEnqueueCount;

        public LF2TaskRingBuffer(int initialCapacity)
        {
            if (initialCapacity < 1)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));

            items = new LF2TaskBase[initialCapacity];
        }

        public int Count => count;
        public int Capacity => items.Length;
        public bool CapacitySealed => capacitySealed;
        public long RejectedEnqueueCount => rejectedEnqueueCount;

        public void EnsureCapacity(int requiredCapacity)
        {
            if (requiredCapacity <= items.Length || capacitySealed)
                return;

            Resize(requiredCapacity);
        }

        public void SealCapacity()
        {
            capacitySealed = true;
        }

        public void UnsealCapacity()
        {
            capacitySealed = false;
        }

        public bool TryEnqueue(LF2TaskBase task)
        {
            if (task == null)
                return false;

            if (count == items.Length)
            {
                if (capacitySealed)
                {
                    rejectedEnqueueCount++;
                    return false;
                }

                Resize(checked(items.Length * 2));
            }

            items[tail] = task;
            tail++;
            if (tail == items.Length)
                tail = 0;
            count++;
            return true;
        }

        public bool TryDequeue(out LF2TaskBase task)
        {
            if (count == 0)
            {
                task = null;
                return false;
            }

            task = items[head];
            items[head] = null;
            head++;
            if (head == items.Length)
                head = 0;
            count--;
            return true;
        }

        private void Resize(int nextCapacity)
        {
            var next = new LF2TaskBase[nextCapacity];
            for (int i = 0; i < count; i++)
                next[i] = items[(head + i) % items.Length];

            items = next;
            head = 0;
            tail = count;
        }
    }
}
