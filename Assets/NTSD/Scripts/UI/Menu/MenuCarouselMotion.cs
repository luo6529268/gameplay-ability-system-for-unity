using System;

namespace NTSD.UI.Menu
{
    /// <summary>Continuous item coordinates; positive motion selects the next item.</summary>
    public sealed class MenuCarouselMotion
    {
        public int Count { get; private set; }
        public double Position { get; private set; }
        public double Target { get; private set; }
        public bool IsDragging { get; private set; }
        public bool IsSettled => !IsDragging && Math.Abs(Target - Position) < 0.0001;
        public int SelectedIndex => WrapIndex((int)Math.Floor(Target + 0.5), Count);

        public void Reset(int count, int index)
        {
            Count = Math.Max(0, count);
            Position = Target = WrapIndex(index, Count);
            IsDragging = false;
        }

        public void Move(int steps)
        {
            if (Count < 2 || IsDragging) return;
            Target += steps;
            Normalize();
        }

        public void Select(int index)
        {
            if (Count == 0 || IsDragging) return;
            Target += WrappedOffset(WrapIndex(index, Count), Target, Count);
            Target = Math.Floor(Target + 0.5);
            Normalize();
        }

        public void BeginDrag()
        {
            IsDragging = true;
            Target = Position;
        }

        public void Drag(double itemDelta)
        {
            if (!IsDragging || Count < 2 || double.IsNaN(itemDelta) || double.IsInfinity(itemDelta)) return;
            Position += itemDelta;
            Target = Position;
            Normalize();
        }

        public void EndDrag()
        {
            IsDragging = false;
            Target = Math.Floor(Position + 0.5);
            Normalize();
        }

        public void Advance(double seconds, double snapSeconds)
        {
            if (IsDragging || Count == 0 || seconds <= 0) return;
            double blend = 1.0 - Math.Exp(-seconds / Math.Max(0.001, snapSeconds));
            Position += (Target - Position) * blend;
            if (Math.Abs(Target - Position) < 0.0001) Position = Target;
            Normalize();
        }

        public void FinishSnap()
        {
            if (!IsDragging) Position = Target;
            Normalize();
        }

        public double Offset(int index) => WrappedOffset(index, Position, Count);

        public static int WrapIndex(int index, int count)
        {
            if (count <= 0) return 0;
            int value = index % count;
            return value < 0 ? value + count : value;
        }

        public static double WrappedOffset(double index, double position, int count)
        {
            if (count <= 0) return 0;
            double delta = index - position;
            return delta - Math.Floor((delta + count * 0.5) / count) * count;
        }

        private void Normalize()
        {
            if (Count <= 0) return;
            double turns = Math.Floor(Position / Count);
            Position -= turns * Count;
            Target -= turns * Count;
            // Bound queued keyboard motion without losing its selected index.
            if (Math.Abs(Target - Position) > Count * 2)
                Target = Position + WrappedOffset(Target, Position, Count);
        }
    }
}
