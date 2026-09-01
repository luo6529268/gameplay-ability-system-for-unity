#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("RuntimeRestStateSeam")]
    public sealed class RuntimeRestStateSeamEditorTests
    {
        private const string ExpectedCorpusSha256 =
            "E10CF6D96104F69F574AA73503AFF9F03C0AD85633E66AE02054A435D86434E8";

        private const string GoldenCorpus =
            "schema=ntsd-rest-state-cross-consumer-v1\n" +
            "scope=authority|case=A|step=00|op=reset-world|p=-1|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=A|step=01|op=set-a|p=5|q=-1|value=3|result=1|aCount=1|vCount=0|vRows=0|hash=D7A80DE075F77CC6\n" +
            "scope=authority|case=A|step=02|op=set-v|p=9|q=5|value=2|result=1|aCount=1|vCount=1|vRows=1|hash=7FEBA6B29646ADBC\n" +
            "scope=authority|case=A|step=03|op=set-v|p=5|q=9|value=4|result=1|aCount=1|vCount=2|vRows=2|hash=FC9E3E2117565CC0\n" +
            "scope=authority|case=A|step=04|op=tick-a|p=5|q=-1|value=0|result=1|aCount=1|vCount=2|vRows=2|hash=23EEB70C8432BEAA\n" +
            "scope=authority|case=A|step=05|op=tick-pair|p=5|q=9|value=0|result=1|aCount=1|vCount=2|vRows=2|hash=FCF9EC7B4F1F7EBB\n" +
            "scope=authority|case=A|step=06|op=tick-pair|p=5|q=9|value=0|result=1|aCount=1|vCount=1|vRows=1|hash=A0A8CF74B0D57AE5\n" +
            "scope=authority|case=A|step=07|op=set-a|p=5|q=-1|value=-4|result=1|aCount=0|vCount=1|vRows=1|hash=985CC8B8DFF7C14B\n" +
            "scope=authority|case=A|step=08|op=set-v|p=5|q=9|value=-1|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=A|step=09|op=read-a|p=5|q=-1|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=A|step=10|op=read-v|p=9|q=5|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=00|op=reset-world|p=-1|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=01|op=set-a|p=20|q=-1|value=7|result=1|aCount=1|vCount=0|vRows=0|hash=070D987291607883\n" +
            "scope=authority|case=S|step=02|op=set-v|p=20|q=21|value=3|result=1|aCount=1|vCount=1|vRows=1|hash=56273B4F9E3EC5D3\n" +
            "scope=authority|case=S|step=03|op=set-v|p=21|q=20|value=4|result=1|aCount=1|vCount=2|vRows=2|hash=BFA889760F71E74E\n" +
            "scope=authority|case=S|step=04|op=set-v|p=22|q=20|value=5|result=1|aCount=1|vCount=3|vRows=3|hash=DA0514246F63C7C9\n" +
            "scope=authority|case=S|step=05|op=set-v|p=20|q=20|value=6|result=1|aCount=1|vCount=4|vRows=3|hash=7FAAB0073C20A0DE\n" +
            "scope=authority|case=S|step=06|op=reset-slot|p=20|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=07|op=read-a|p=20|q=-1|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=08|op=read-v|p=20|q=21|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=09|op=read-v|p=21|q=20|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=10|op=read-v|p=22|q=20|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=S|step=11|op=read-v|p=20|q=20|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=W|step=00|op=reset-world|p=-1|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=W|step=01|op=set-a|p=0|q=-1|value=2|result=1|aCount=1|vCount=0|vRows=0|hash=92F63CA57B752ABF\n" +
            "scope=authority|case=W|step=02|op=set-a|p=399|q=-1|value=1|result=1|aCount=2|vCount=0|vRows=0|hash=D345525801DD2322\n" +
            "scope=authority|case=W|step=03|op=set-v|p=399|q=0|value=3|result=1|aCount=2|vCount=1|vRows=1|hash=9C028057E2A566D4\n" +
            "scope=authority|case=W|step=04|op=set-v|p=0|q=399|value=4|result=1|aCount=2|vCount=2|vRows=2|hash=E7192F6BCA9937DC\n" +
            "scope=authority|case=W|step=05|op=reset-world|p=-1|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=W|step=06|op=set-a|p=400|q=-1|value=9|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=W|step=07|op=set-v|p=-1|q=0|value=9|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=W|step=08|op=reset-slot|p=400|q=-1|value=0|result=0|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=O1|step=00|op=reset-world|p=-1|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=O1|step=01|op=set-a|p=7|q=-1|value=5|result=1|aCount=1|vCount=0|vRows=0|hash=763D3B46EFC9C703\n" +
            "scope=authority|case=O1|step=02|op=set-v|p=3|q=9|value=4|result=1|aCount=1|vCount=1|vRows=1|hash=492D97165C089CD1\n" +
            "scope=authority|case=O1|step=03|op=set-v|p=1|q=8|value=6|result=1|aCount=1|vCount=2|vRows=2|hash=F5D11428A6530702\n" +
            "scope=authority|case=O2|step=00|op=reset-world|p=-1|q=-1|value=0|result=1|aCount=0|vCount=0|vRows=0|hash=2226BC7D98803B17\n" +
            "scope=authority|case=O2|step=01|op=set-v|p=1|q=8|value=6|result=1|aCount=0|vCount=1|vRows=1|hash=0FA510C7F3561E42\n" +
            "scope=authority|case=O2|step=02|op=set-v|p=3|q=9|value=4|result=1|aCount=0|vCount=2|vRows=2|hash=633E106A61030BD0\n" +
            "scope=authority|case=O2|step=03|op=set-a|p=7|q=-1|value=5|result=1|aCount=1|vCount=2|vRows=2|hash=F5D11428A6530702\n" +
            "scope=unity|case=P|step=00|op=prepare|capacity=400|storage=dense|sparseCapacity=0\n" +
            "scope=unity|case=P|step=01|op=prepare|capacity=1050|storage=dense|sparseCapacity=0\n" +
            "scope=unity|case=P|step=02|op=prepare|capacity=2048|storage=dense|sparseCapacity=0\n" +
            "scope=unity|case=P|step=03|op=prepare|capacity=2049|storage=sparse|sparseCapacity=65568\n" +
            "scope=unity|case=L|step=00|op=reset-world|slot=20|result=1|bound=0|a=0|row=0|column=0\n" +
            "scope=unity|case=L|step=01|op=seed-rest|slot=20|result=1|bound=0|a=7|row=3|column=5\n" +
            "scope=unity|case=L|step=02|op=acquire|slot=20|result=1|bound=1|a=7|row=3|column=5\n" +
            "scope=unity|case=L|step=03|op=try-reset-acquire-conflict|slot=20|result=0|bound=1|a=7|row=3|column=5\n" +
            "scope=unity|case=L|step=04|op=release|slot=20|result=1|bound=0|a=7|row=3|column=5\n" +
            "scope=unity|case=L|step=05|op=try-reset-acquire|slot=20|result=1|bound=1|a=0|row=0|column=0\n" +
            "scope=unity|case=L|step=06|op=release|slot=20|result=1|bound=0|a=0|row=0|column=0\n" +
            "scope=unity|case=L|step=07|op=seed-rest|slot=20|result=1|bound=0|a=2|row=4|column=6\n" +
            "scope=unity|case=L|step=08|op=reset-slot|slot=20|result=1|bound=0|a=0|row=0|column=0\n" +
            "scope=unity|case=L|step=09|op=acquire|slot=20|result=1|bound=1|a=0|row=0|column=0\n" +
            "scope=unity|case=L|step=10|op=seed-rest|slot=20|result=1|bound=1|a=2|row=4|column=6\n" +
            "scope=unity|case=L|step=11|op=reset-world|slot=20|result=1|bound=0|a=0|row=0|column=0\n";

        [Test]
        public void GoldenCorpus_ReplaysAllFrozenRowsAndHashes()
        {
            byte[] payload = Encoding.UTF8.GetBytes(GoldenCorpus);
            using var sha = SHA256.Create();
            string digest = BitConverter.ToString(sha.ComputeHash(payload))
                .Replace("-", string.Empty);
            Assert.That(digest, Is.EqualTo(ExpectedCorpusSha256));

            string[] lines = GoldenCorpus.TrimEnd('\n').Split('\n');
            Assert.That(lines, Has.Length.EqualTo(57));
            ReplayAuthorityRows(lines);
            ReplayProfileRows(lines);
            ReplayLeaseRows(lines);
        }

        [TestCase(64)]
        [TestCase(2049)]
        public void CanonicalTraversal_IsOrderedForDenseAndSparseStorage(int capacity)
        {
            var store = new RuntimeRestStore(capacity);
            store.PrepareForBattle();
            Assert.That(store.SetARest(9, 1), Is.True);
            Assert.That(store.SetARest(2, 3), Is.True);
            Assert.That(store.SetVRest(9, 7, 4), Is.True);
            Assert.That(store.SetVRest(1, 8, 5), Is.True);
            Assert.That(store.SetVRest(1, 3, 6), Is.True);

            var aEntries = new List<RuntimeRestStore.ARestEntry>();
            foreach (RuntimeRestStore.ARestEntry entry in
                     store.EnumerateCanonicalARestEntries())
            {
                aEntries.Add(entry);
            }

            var vEntries = new List<RuntimeRestStore.VRestEntry>();
            foreach (RuntimeRestStore.VRestEntry entry in
                     store.EnumerateCanonicalVRestEntries())
            {
                vEntries.Add(entry);
            }

            Assert.That(aEntries.Count, Is.EqualTo(2));
            Assert.That(aEntries[0].AttackerSlot, Is.EqualTo(2));
            Assert.That(aEntries[1].AttackerSlot, Is.EqualTo(9));
            Assert.That(vEntries.Count, Is.EqualTo(3));
            Assert.That((vEntries[0].VictimSlot, vEntries[0].AttackerSlot),
                Is.EqualTo((1, 3)));
            Assert.That((vEntries[1].VictimSlot, vEntries[1].AttackerSlot),
                Is.EqualTo((1, 8)));
            Assert.That((vEntries[2].VictimSlot, vEntries[2].AttackerSlot),
                Is.EqualTo((9, 7)));
        }

        [TestCase(400)]
        [TestCase(2049)]
        public void WarmedCanonicalTraversalAndChecksum_DoNotAllocate(int capacity)
        {
            var store = new RuntimeRestStore(capacity);
            store.PrepareForBattle();
            Assert.That(store.SetARest(7, 5), Is.True);
            Assert.That(store.SetVRest(3, 9, 4), Is.True);
            Assert.That(store.SetVRest(1, 8, 6), Is.True);

            long sink = TraverseAndHash(store, 1);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            sink ^= TraverseAndHash(store, 256);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(sink, Is.Not.Zero);
        }

        private static long TraverseAndHash(RuntimeRestStore store, int iterations)
        {
            long sink = 0;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                foreach (RuntimeRestStore.ARestEntry entry in
                         store.EnumerateCanonicalARestEntries())
                {
                    sink += entry.AttackerSlot + entry.Value;
                }

                foreach (RuntimeRestStore.VRestEntry entry in
                         store.EnumerateCanonicalVRestEntries())
                {
                    sink += entry.VictimSlot + entry.AttackerSlot + entry.Value;
                }

                sink ^= unchecked((long)
                    BattleLockstepChecksumModule.CaptureRestProjectionChecksum(store));
            }

            return sink;
        }

        private static void ReplayAuthorityRows(string[] lines)
        {
            var store = new RuntimeRestStore(400);
            store.PrepareForBattle();
            for (int index = 1; index < lines.Length; index++)
            {
                string line = lines[index];
                if (!line.StartsWith("scope=authority|", StringComparison.Ordinal))
                    continue;

                string operation = Field(line, "op");
                int p = IntField(line, "p");
                int q = IntField(line, "q");
                int value = IntField(line, "value");
                int result;
                switch (operation)
                {
                    case "reset-world":
                        store.ResetWorld();
                        result = 1;
                        break;
                    case "set-a":
                        result = store.SetARest(p, value) ? 1 : 0;
                        break;
                    case "set-v":
                        result = store.SetVRest(p, q, value) ? 1 : 0;
                        break;
                    case "tick-a":
                        result = store.TickARest(p) ? 1 : 0;
                        break;
                    case "tick-pair":
                        result = store.TickCollisionPairVRest(new[] { p, q }) ? 1 : 0;
                        break;
                    case "reset-slot":
                        result = store.ResetSlot(p) ? 1 : 0;
                        break;
                    case "read-a":
                        result = store.IsAddressable(p) ? store.GetARest(p) : -1;
                        break;
                    case "read-v":
                        result = store.IsAddressable(p) && store.IsAddressable(q)
                            ? store.GetVRest(p, q)
                            : -1;
                        break;
                    default:
                        Assert.Fail($"Unknown authority operation: {operation}");
                        return;
                }

                Assert.That(result, Is.EqualTo(IntField(line, "result")), line);
                Assert.That(store.ARestEntryCount,
                    Is.EqualTo(IntField(line, "aCount")), line);
                Assert.That(store.VRestEntryCount,
                    Is.EqualTo(IntField(line, "vCount")), line);
                Assert.That(store.VRestRowCount,
                    Is.EqualTo(IntField(line, "vRows")), line);
                Assert.That(
                    BattleLockstepChecksumModule.CaptureRestProjectionChecksum(store)
                        .ToString("X16", CultureInfo.InvariantCulture),
                    Is.EqualTo(Field(line, "hash")),
                    line);
            }
        }

        private static void ReplayProfileRows(string[] lines)
        {
            for (int index = 1; index < lines.Length; index++)
            {
                string line = lines[index];
                if (!line.StartsWith("scope=unity|case=P|", StringComparison.Ordinal))
                    continue;

                int capacity = IntField(line, "capacity");
                var store = new RuntimeRestStore(capacity);
                store.PrepareForBattle();
                Assert.That(store.UsesDenseBattleStorage,
                    Is.EqualTo(Field(line, "storage") == "dense"), line);
                Assert.That(store.PreparedSparseVRestEntryCapacity,
                    Is.EqualTo(IntField(line, "sparseCapacity")), line);
            }
        }

        private static void ReplayLeaseRows(string[] lines)
        {
            var store = new RuntimeRestStore(64);
            RuntimeRestBindingHandle lease = default;
            for (int index = 1; index < lines.Length; index++)
            {
                string line = lines[index];
                if (!line.StartsWith("scope=unity|case=L|", StringComparison.Ordinal))
                    continue;

                int slot = IntField(line, "slot");
                int result;
                switch (Field(line, "op"))
                {
                    case "reset-world":
                        store.ResetWorld();
                        result = 1;
                        break;
                    case "seed-rest":
                    {
                        int a = IntField(line, "a");
                        int row = IntField(line, "row");
                        int column = IntField(line, "column");
                        result = store.SetARest(slot, a) &&
                                 store.SetVRest(slot, slot + 1, row) &&
                                 store.SetVRest(slot + 2, slot, column)
                            ? 1
                            : 0;
                        break;
                    }
                    case "acquire":
                        result = store.TryAcquireBinding(slot, out lease) ? 1 : 0;
                        break;
                    case "try-reset-acquire-conflict":
                        result = store.TryResetSlotAndAcquireBinding(slot, out _)
                            ? 1
                            : 0;
                        break;
                    case "release":
                        result = store.ReleaseBinding(lease) ? 1 : 0;
                        lease = default;
                        break;
                    case "try-reset-acquire":
                        result = store.TryResetSlotAndAcquireBinding(slot, out lease)
                            ? 1
                            : 0;
                        break;
                    case "reset-slot":
                        result = store.ResetSlot(slot) ? 1 : 0;
                        break;
                    default:
                        Assert.Fail($"Unknown lease operation: {Field(line, "op")}");
                        return;
                }

                Assert.That(result, Is.EqualTo(IntField(line, "result")), line);
                Assert.That(store.IsBindingValid(lease) ? 1 : 0,
                    Is.EqualTo(IntField(line, "bound")), line);
                Assert.That(store.GetARest(slot), Is.EqualTo(IntField(line, "a")), line);
                Assert.That(store.GetVRest(slot, slot + 1),
                    Is.EqualTo(IntField(line, "row")), line);
                Assert.That(store.GetVRest(slot + 2, slot),
                    Is.EqualTo(IntField(line, "column")), line);
            }
        }

        private static int IntField(string line, string key)
        {
            return int.Parse(Field(line, key), CultureInfo.InvariantCulture);
        }

        private static string Field(string line, string key)
        {
            string prefix = key + "=";
            string[] fields = line.Split('|');
            for (int index = 0; index < fields.Length; index++)
            {
                if (fields[index].StartsWith(prefix, StringComparison.Ordinal))
                    return fields[index].Substring(prefix.Length);
            }

            Assert.Fail($"Missing field '{key}' in: {line}");
            return string.Empty;
        }
    }
}
#endif
