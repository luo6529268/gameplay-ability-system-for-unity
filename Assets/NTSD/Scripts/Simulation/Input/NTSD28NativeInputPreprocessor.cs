namespace NTSD.Simulation
{
    internal enum NTSD28NativeInputRemapResult : byte
    {
        NotRequested = 0,
        Applied = 1,
        Invalid = 2,
    }

    internal static class NTSD28NativeInputPreprocessor
    {
        internal static NTSD28NativeInputRemapResult ApplyCurrentButtonRemap(
            NTSDEntityRuntime runtime)
        {
            if (runtime == null || runtime.InputRemapState138 <= 0)
                return NTSD28NativeInputRemapResult.NotRequested;

            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            byte[] remap = runtime.InputRemapIndices13C;
            if (input == null ||
                !input.HasCanonicalStorage ||
                remap == null ||
                remap.Length != NTSDEntityRuntime.NativeInputRemapCount)
            {
                return NTSD28NativeInputRemapResult.Invalid;
            }

            for (int index = 0; index < remap.Length; index++)
            {
                if (remap[index] >= NTSDEntityRuntime.NativeInputRemapCount)
                    return NTSD28NativeInputRemapResult.Invalid;
            }

            byte source0 = input.Current[0];
            byte source1 = input.Current[1];
            byte source2 = input.Current[2];
            byte source3 = input.Current[3];
            byte source4 = input.Current[4];
            byte source5 = input.Current[5];
            byte source6 = input.Current[6];
            for (int index = 0; index < input.Current.Length; index++)
                input.Current[index] = 0;
            input.Current[remap[0]] = source0;
            input.Current[remap[1]] = source1;
            input.Current[remap[2]] = source2;
            input.Current[remap[3]] = source3;
            input.Current[remap[4]] = source4;
            input.Current[remap[5]] = source5;
            input.Current[remap[6]] = source6;
            return NTSD28NativeInputRemapResult.Applied;
        }

        internal static bool ShouldSuppressJumpEdge(NTSDEntityRuntime runtime)
        {
            return runtime != null &&
                   runtime.BoundState198 > 0 &&
                   runtime.InputGlobalRecordState20 != 1 &&
                   runtime.InputGlobalRecordState20 != 3;
        }
    }
}
