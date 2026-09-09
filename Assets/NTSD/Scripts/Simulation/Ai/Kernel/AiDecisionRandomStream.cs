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
            UsesSynchronizedCursor = false;
            synchronizedCursor = default;
            OrderHash = HashOffset;
            DrawCount = 0;
            CaptureTrace = captureTrace;
            CallSites = null;
            Moduli = moduli;
            RawValues = rawValues;
            Values = values;
            TraceOverflow = false;
        }

        internal AiDecisionRandomStream(
            NTSD28SynchronizedRandomCursor cursor,
            bool captureTrace = false,
            uint[] callSites = null,
            int[] moduli = null,
            int[] rawValues = null,
            int[] values = null)
        {
            State = 0;
            Calls = cursor.Calls;
            UsesSynchronizedCursor = true;
            synchronizedCursor = cursor;
            OrderHash = HashOffset;
            DrawCount = 0;
            CaptureTrace = captureTrace;
            CallSites = callSites;
            Moduli = moduli;
            RawValues = rawValues;
            Values = values;
            TraceOverflow = false;
        }

        internal uint State;
        internal ulong Calls;
        internal bool UsesSynchronizedCursor { get; private set; }
        internal ulong OrderHash;
        internal int DrawCount;
        internal bool CaptureTrace;
        internal uint[] CallSites;
        internal int[] Moduli;
        internal int[] RawValues;
        internal int[] Values;
        internal bool TraceOverflow;

        private NTSD28SynchronizedRandomCursor synchronizedCursor;

        internal int SynchronizedCounter => synchronizedCursor.Counter;

        internal int SynchronizedIndex => synchronizedCursor.Index;

        internal uint LastCallSite => synchronizedCursor.LastCallSite;

        internal int Rand(int modulus)
        {
            if (UsesSynchronizedCursor)
            {
                throw new InvalidOperationException(
                    "Synchronized AI RNG draws require an explicit call-site ID.");
            }

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

        internal int Rand(uint callSite, int modulus)
        {
            if (!UsesSynchronizedCursor)
            {
                throw new InvalidOperationException(
                    "Call-site-aware AI RNG draws require a synchronized cursor.");
            }

            if (modulus < 1)
                return 0;

            int value = synchronizedCursor.Next(
                callSite,
                modulus,
                out int raw);
            Calls = synchronizedCursor.Calls;
            if (CaptureTrace &&
                CallSites != null &&
                Moduli != null &&
                RawValues != null &&
                Values != null &&
                DrawCount < CallSites.Length &&
                DrawCount < Moduli.Length &&
                DrawCount < RawValues.Length &&
                DrawCount < Values.Length)
            {
                CallSites[DrawCount] = callSite;
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
                    OrderHash ^= callSite;
                    OrderHash *= HashPrime;
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

        internal NTSD28SynchronizedRandomCursor CaptureSynchronizedCursor()
        {
            if (!UsesSynchronizedCursor)
            {
                throw new InvalidOperationException(
                    "Legacy CRT AI RNG streams do not own a synchronized cursor.");
            }

            return synchronizedCursor;
        }
    }
}
