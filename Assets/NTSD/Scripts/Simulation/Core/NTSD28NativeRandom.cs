using System;

namespace NTSD.Simulation
{
    public sealed class NTSD28SynchronizedRandomState
    {
        public const int TableSize = 3000;
        public const int SerializedTableSize = 3001;

        public int Counter { get; set; }
        public int Index { get; set; }
        public byte[] Table { get; }
        public ulong Calls { get; set; }
        public uint LastCallSite { get; set; }

        public NTSD28SynchronizedRandomState()
        {
            Table = new byte[SerializedTableSize];
        }

        internal NTSD28SynchronizedRandomState(
            NTSD28SynchronizedRandomState source)
            : this()
        {
            if (source == null)
            {
                return;
            }

            Counter = source.Counter;
            Index = source.Index;
            Calls = source.Calls;
            LastCallSite = source.LastCallSite;
            Array.Copy(source.Table, Table, SerializedTableSize);
        }

        internal NTSD28SynchronizedRandomState Clone()
        {
            return new NTSD28SynchronizedRandomState(this);
        }
    }

    public sealed class NTSD28NativeRandomState
    {
        public uint CrtState { get; }
        public ulong CrtCalls { get; }
        public NTSD28SynchronizedRandomState Synchronized { get; }

        public NTSD28NativeRandomState(
            uint crtState,
            ulong crtCalls,
            NTSD28SynchronizedRandomState synchronized)
        {
            CrtState = crtState;
            CrtCalls = crtCalls;
            Synchronized = new NTSD28SynchronizedRandomState(synchronized);
        }
    }

    public sealed class NTSD28Msvcr80Random
    {
        public uint State { get; private set; }
        public ulong Calls { get; private set; }

        public NTSD28Msvcr80Random(uint seed = 1u)
        {
            Seed(seed);
        }

        public void Seed(uint value)
        {
            State = value;
            Calls = 0;
        }

        public void Restore(uint state, ulong calls)
        {
            State = state;
            Calls = calls;
        }

        public uint Next()
        {
            unchecked
            {
                State = State * 214013u + 2531011u;
                Calls++;
            }

            return (State >> 16) & 0x7FFFu;
        }
    }

    public readonly struct NTSD28NativeRandomScalarState
    {
        public NTSD28NativeRandomScalarState(
            uint crtState,
            ulong crtCalls,
            uint tableSeed,
            int synchronizedCounter,
            int synchronizedIndex,
            ulong synchronizedCalls,
            uint lastSynchronizedCallSite,
            ulong synchronizedTableHash,
            uint synchronizedGeneration)
        {
            CrtState = crtState;
            CrtCalls = crtCalls;
            TableSeed = tableSeed;
            SynchronizedCounter = synchronizedCounter;
            SynchronizedIndex = synchronizedIndex;
            SynchronizedCalls = synchronizedCalls;
            LastSynchronizedCallSite = lastSynchronizedCallSite;
            SynchronizedTableHash = synchronizedTableHash;
            SynchronizedGeneration = synchronizedGeneration;
        }

        public uint CrtState { get; }
        public ulong CrtCalls { get; }
        public uint TableSeed { get; }
        public int SynchronizedCounter { get; }
        public int SynchronizedIndex { get; }
        public ulong SynchronizedCalls { get; }
        public uint LastSynchronizedCallSite { get; }
        public ulong SynchronizedTableHash { get; }
        public uint SynchronizedGeneration { get; }
    }

    internal readonly struct NTSD28NativeCrtCall
    {
        internal NTSD28NativeCrtCall(
            uint result,
            uint stateAfter,
            ulong totalCalls)
        {
            Result = result;
            StateAfter = stateAfter;
            TotalCalls = totalCalls;
        }

        internal uint Result { get; }
        internal uint StateAfter { get; }
        internal ulong TotalCalls { get; }
    }

    internal readonly struct NTSD28NativeSynchronizedCall
    {
        internal NTSD28NativeSynchronizedCall(
            uint callSite,
            int upperBound,
            int result,
            int counterAfter,
            int indexAfter,
            ulong totalCalls)
        {
            CallSite = callSite;
            UpperBound = upperBound;
            Result = result;
            CounterAfter = counterAfter;
            IndexAfter = indexAfter;
            TotalCalls = totalCalls;
        }

        internal uint CallSite { get; }
        internal int UpperBound { get; }
        internal int Result { get; }
        internal int CounterAfter { get; }
        internal int IndexAfter { get; }
        internal ulong TotalCalls { get; }
    }

    internal interface INTSD28NativeRandomCallObserver
    {
        void OnCrtNext(NTSD28NativeCrtCall call);

        void OnSynchronizedNext(NTSD28NativeSynchronizedCall call);
    }

    internal struct NTSD28SynchronizedRandomCursor
    {
        private readonly byte[] table;
        private readonly uint generation;
        private readonly int originCounter;
        private readonly int originIndex;
        private readonly ulong originCalls;
        private readonly uint originLastCallSite;

        internal NTSD28SynchronizedRandomCursor(
            byte[] table,
            uint generation,
            int counter,
            int index,
            ulong calls,
            uint lastCallSite)
        {
            this.table = table;
            this.generation = generation;
            originCounter = counter;
            originIndex = index;
            originCalls = calls;
            originLastCallSite = lastCallSite;
            Counter = counter;
            Index = index;
            Calls = calls;
            LastCallSite = lastCallSite;
        }

        internal int Counter { get; private set; }
        internal int Index { get; private set; }
        internal ulong Calls { get; private set; }
        internal uint LastCallSite { get; private set; }

        internal int Next(uint callSite, int upperBound)
        {
            return Next(callSite, upperBound, out _);
        }

        internal int Next(
            uint callSite,
            int upperBound,
            out int rawValue)
        {
            if (upperBound < 1)
            {
                rawValue = 0;
                return 0;
            }

            Counter = (Counter + 1) % 1234;
            Index = (Index + 1) % NTSD28SynchronizedRandomState.TableSize;
            Calls++;
            LastCallSite = callSite;
            rawValue = table[Index] + Counter;
            return (int)((uint)rawValue % (uint)upperBound);
        }

        internal bool BelongsTo(byte[] candidateTable, uint candidateGeneration)
        {
            return table != null &&
                   ReferenceEquals(table, candidateTable) &&
                   generation == candidateGeneration;
        }

        internal bool StartsAt(
            int counter,
            int index,
            ulong calls,
            uint lastCallSite)
        {
            return originCounter == counter &&
                   originIndex == index &&
                   originCalls == calls &&
                   originLastCallSite == lastCallSite;
        }
    }

    public sealed class NTSD28NativeRandom
    {
        private const ulong HashOffset = 14695981039346656037UL;
        private const ulong HashPrime = 1099511628211UL;
        internal const uint DirectBattleRandomBgmCallSite = 0x004021E0u;

        private readonly NTSD28Msvcr80Random crt = new NTSD28Msvcr80Random();
        private NTSD28SynchronizedRandomState synchronized;
        private uint tableSeed;
        private uint synchronizedGeneration;
        private INTSD28NativeRandomCallObserver diagnosticCallObserver;

        public NTSD28NativeRandom()
        {
            ResetFromSeed(1u);
        }

        public NTSD28NativeRandom(uint seed)
        {
            ResetFromSeed(seed);
        }

        public void ResetFromSeed(uint seed, int synchronizedIndex = 0)
        {
            tableSeed = seed;
            crt.Seed(seed);
            synchronized ??= new NTSD28SynchronizedRandomState();
            synchronized.Counter = 0;
            synchronized.Index = PositiveModulo(
                synchronizedIndex,
                NTSD28SynchronizedRandomState.TableSize);
            synchronized.Calls = 0;
            synchronized.LastCallSite = 0;

            // Alignment contract: NTSD28-B2-NATIVE-DUAL-RNG-PRIMITIVE-001.
            for (int index = 0;
                 index < NTSD28SynchronizedRandomState.TableSize;
                 index++)
            {
                synchronized.Table[index] =
                    (byte)((crt.Next() % 255u) + 1u);
            }

            synchronized.Table[NTSD28SynchronizedRandomState.TableSize] = 0;
            AdvanceSynchronizedGeneration();
        }

        internal void ResetForDirectBattle(uint seed)
        {
            // Alignment contract: NTSD28-B2-NATIVE-RNG-DIRECT-BATTLE-BOOTSTRAP-001.
            ResetFromSeed(seed);
            SynchronizedNext(DirectBattleRandomBgmCallSite, 1);
        }

        public void Restore(NTSD28NativeRandomState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            crt.Restore(state.CrtState, state.CrtCalls);
            RestoreSynchronized(state.Synchronized);
        }

        public void RestoreSynchronized(
            NTSD28SynchronizedRandomState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            synchronized = state.Clone();
            synchronized.Index = PositiveModulo(
                synchronized.Index,
                NTSD28SynchronizedRandomState.TableSize);
            synchronized.Counter = PositiveModulo(
                synchronized.Counter,
                1234);
            AdvanceSynchronizedGeneration();
        }

        public uint CrtNext()
        {
            uint result = crt.Next();
            diagnosticCallObserver?.OnCrtNext(
                new NTSD28NativeCrtCall(result, crt.State, crt.Calls));
            return result;
        }

        public int SynchronizedNext(uint callSite, int upperBound)
        {
            if (upperBound < 1)
            {
                return 0;
            }

            synchronized.Counter = (synchronized.Counter + 1) % 1234;
            synchronized.Index =
                (synchronized.Index + 1) %
                NTSD28SynchronizedRandomState.TableSize;
            synchronized.Calls++;
            synchronized.LastCallSite = callSite;
            int result = (int)(
                ((uint)synchronized.Table[synchronized.Index] +
                 (uint)synchronized.Counter) %
                (uint)upperBound);
            diagnosticCallObserver?.OnSynchronizedNext(
                new NTSD28NativeSynchronizedCall(
                    callSite,
                    upperBound,
                    result,
                    synchronized.Counter,
                    synchronized.Index,
                    synchronized.Calls));
            return result;
        }

        internal void SetDiagnosticCallObserver(
            INTSD28NativeRandomCallObserver observer)
        {
            // Alignment contract: NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001.
            diagnosticCallObserver = observer;
        }

        public NTSD28NativeRandomState CaptureState()
        {
            return new NTSD28NativeRandomState(
                crt.State,
                crt.Calls,
                synchronized);
        }

        public NTSD28SynchronizedRandomState CaptureSynchronizedState()
        {
            return synchronized.Clone();
        }

        internal NTSD28SynchronizedRandomCursor CaptureSynchronizedCursor()
        {
            return new NTSD28SynchronizedRandomCursor(
                synchronized.Table,
                synchronizedGeneration,
                synchronized.Counter,
                synchronized.Index,
                synchronized.Calls,
                synchronized.LastCallSite);
        }

        internal bool TryCommitSynchronizedCursor(
            NTSD28SynchronizedRandomCursor cursor)
        {
            if (!CanCommitSynchronizedCursor(cursor))
                return false;

            synchronized.Counter = cursor.Counter;
            synchronized.Index = cursor.Index;
            synchronized.Calls = cursor.Calls;
            synchronized.LastCallSite = cursor.LastCallSite;
            return true;
        }

        internal bool CanCommitSynchronizedCursor(
            NTSD28SynchronizedRandomCursor cursor)
        {
            return cursor.BelongsTo(
                       synchronized.Table,
                       synchronizedGeneration) &&
                   cursor.StartsAt(
                       synchronized.Counter,
                       synchronized.Index,
                       synchronized.Calls,
                       synchronized.LastCallSite);
        }

        public NTSD28NativeRandomScalarState CaptureScalarState()
        {
            return new NTSD28NativeRandomScalarState(
                crt.State,
                crt.Calls,
                tableSeed,
                synchronized.Counter,
                synchronized.Index,
                synchronized.Calls,
                synchronized.LastCallSite,
                SynchronizedTableHash(),
                synchronizedGeneration);
        }

        public bool TryRestoreScalarState(NTSD28NativeRandomScalarState state)
        {
            ResetFromSeed(state.TableSeed);
            if (SynchronizedTableHash() != state.SynchronizedTableHash)
            {
                return false;
            }

            crt.Restore(state.CrtState, state.CrtCalls);
            synchronized.Counter = PositiveModulo(
                state.SynchronizedCounter,
                1234);
            synchronized.Index = PositiveModulo(
                state.SynchronizedIndex,
                NTSD28SynchronizedRandomState.TableSize);
            synchronized.Calls = state.SynchronizedCalls;
            synchronized.LastCallSite = state.LastSynchronizedCallSite;
            return true;
        }

        public ulong SynchronizedTableHash()
        {
            ulong value = HashOffset;
            unchecked
            {
                for (int index = 0; index < synchronized.Table.Length; index++)
                {
                    value ^= synchronized.Table[index];
                    value *= HashPrime;
                }
            }

            return value;
        }

        private static int PositiveModulo(int value, int modulus)
        {
            int remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        private void AdvanceSynchronizedGeneration()
        {
            unchecked
            {
                synchronizedGeneration++;
                if (synchronizedGeneration == 0)
                    synchronizedGeneration = 1;
            }
        }
    }
}
