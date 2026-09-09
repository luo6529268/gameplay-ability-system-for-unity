using System;

namespace NTSD.Simulation
{
    public sealed class NTSD28InputProxyBlock
    {
        public const int InputKeyCount = 7;
        public const int ComboStateCount = 10;
        public const int SerializedByteCount = 0x21;

        public NTSD28InputProxyBlock()
        {
            EdgeWindow = new byte[InputKeyCount];
            Previous = new byte[InputKeyCount];
            Current = new byte[InputKeyCount];
            ComboState = new byte[ComboStateCount];
        }

        public byte[] EdgeWindow { get; }
        public byte DefendReentryCooldown { get; set; }
        public byte[] Previous { get; }
        public byte[] Current { get; }
        public byte[] ComboState { get; }
        public byte ProxyTail { get; set; }

        internal bool HasCanonicalStorage =>
            EdgeWindow != null && EdgeWindow.Length == InputKeyCount &&
            Previous != null && Previous.Length == InputKeyCount &&
            Current != null && Current.Length == InputKeyCount &&
            ComboState != null && ComboState.Length == ComboStateCount;

        public void Clear()
        {
            Array.Clear(EdgeWindow, 0, EdgeWindow.Length);
            DefendReentryCooldown = 0;
            Array.Clear(Previous, 0, Previous.Length);
            Array.Clear(Current, 0, Current.Length);
            Array.Clear(ComboState, 0, ComboState.Length);
            ProxyTail = 0;
        }

        public void CopyFrom(NTSD28InputProxyBlock source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Array.Copy(source.EdgeWindow, EdgeWindow, InputKeyCount);
            DefendReentryCooldown = source.DefendReentryCooldown;
            Array.Copy(source.Previous, Previous, InputKeyCount);
            Array.Copy(source.Current, Current, InputKeyCount);
            Array.Copy(source.ComboState, ComboState, ComboStateCount);
            ProxyTail = source.ProxyTail;
        }

        public void WriteSerialized(byte[] destination, int offset = 0)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (offset < 0 || offset > destination.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (destination.Length - offset < SerializedByteCount)
            {
                throw new ArgumentException(
                    "The destination does not have room for the 0x21-byte input proxy block.",
                    nameof(destination));
            }

            // Alignment contract: NTSD28-B2-INPUT-PROXY-BLOCK-001.
            destination[offset] = EdgeWindow[0];
            destination[offset + 1] = EdgeWindow[1];
            destination[offset + 2] = EdgeWindow[2];
            destination[offset + 3] = DefendReentryCooldown;
            destination[offset + 4] = EdgeWindow[3];
            destination[offset + 5] = EdgeWindow[4];
            destination[offset + 6] = EdgeWindow[5];
            destination[offset + 7] = EdgeWindow[6];
            Array.Copy(Previous, 0, destination, offset + 8, InputKeyCount);
            Array.Copy(Current, 0, destination, offset + 15, InputKeyCount);
            Array.Copy(ComboState, 0, destination, offset + 22, ComboStateCount);
            destination[offset + 32] = ProxyTail;
        }
    }
}
