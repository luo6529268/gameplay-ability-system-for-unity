using System;

namespace NTSD.Simulation
{
    internal struct AiDecisionRandomStream
    {
        internal const ulong HashOffset = 1469598103934665603UL;
        private const ulong HashPrime = 1099511628211UL;

        internal AiDecisionRandomStream(
            uint state,
            ulong calls,
            bool captureTrace = false,
            int[] moduli = null,
            int[] rawValues = null,
            int[] values = null)
        {
            State = state;
            Calls = calls;
            OrderHash = HashOffset;
            DrawCount = 0;
            CaptureTrace = captureTrace;
            Moduli = moduli;
            RawValues = rawValues;
            Values = values;
            TraceOverflow = false;
        }

        internal uint State;
        internal ulong Calls;
        internal ulong OrderHash;
        internal int DrawCount;
        internal bool CaptureTrace;
        internal int[] Moduli;
        internal int[] RawValues;
        internal int[] Values;
        internal bool TraceOverflow;

        internal int Rand(int modulus)
        {
            unchecked
            {
                State = State * 0x343FDu + 0x269EC3u;
                Calls++;
            }

            int raw = (int)((State >> 16) & 0x7FFFu);
            int normalizedModulus = Math.Max(1, modulus);
            int value = raw % normalizedModulus;
            if (CaptureTrace &&
                Moduli != null &&
                RawValues != null &&
                Values != null &&
                DrawCount < Moduli.Length &&
                DrawCount < RawValues.Length &&
                DrawCount < Values.Length)
            {
                Moduli[DrawCount] = modulus;
                RawValues[DrawCount] = raw;
                Values[DrawCount] = value;
            }
            else if (CaptureTrace)
            {
                TraceOverflow = true;
            }

            if (CaptureTrace)
            {
                unchecked
                {
                    OrderHash ^= (uint)modulus;
                    OrderHash *= HashPrime;
                    OrderHash ^= (uint)raw;
                    OrderHash *= HashPrime;
                    OrderHash ^= (uint)value;
                    OrderHash *= HashPrime;
                }
            }

            DrawCount++;
            return value;
        }
    }
}
