#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class M12DecodeUploadBackpressureEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-20261011/implementation-01/review-02/focused-01";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private NTSD28B11AtomicPublicationEditorTests fixture;
        private CharacterAnimtorManager Manager => fixture.M12Manager;
        private string runId;

        [SetUp]
        public void SetUp()
        {
            runId = Guid.NewGuid().ToString("N");
            fixture = new NTSD28B11AtomicPublicationEditorTests
            {
                EvidenceDirectory = Output,
                DrawEvidenceDirectory = Path.Combine(Output, "draw", runId)
            };
            fixture.SetUp();
        }

        [TearDown]
        public void TearDown()
        {
            Manager.NativeDecodeStartedForDiagnostics = null;
            Manager.NativeBeforeUploadForDiagnostics = null;
            Assert.That(Manager.HasPendingNativePrewarmWork, Is.False, "No live source worker may escape this fixture.");
            fixture.TearDown();
        }

        private void Save(string name, object value)
        {
            Directory.CreateDirectory(Output);
            using (var stream = new FileStream(Path.Combine(Output, name + "-" + runId + ".json"), FileMode.CreateNew))
            using (var writer = new StreamWriter(stream))
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
        }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        private static byte[] Png(int width, int height, int offset = 0)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[width * height];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32((byte)(i + offset), (byte)(i / width), (byte)(i * 13), (byte)(i % 3 == 0 ? 0 : i % 3 == 1 ? 128 : 255));
                texture.SetPixels32(pixels);
                texture.Apply();
                return texture.EncodeToPNG();
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private string NewFile(string name, byte[] bytes)
        {
            string directory = Path.GetFullPath(Path.Combine(Output, "fixtures", runId));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, name);
            using (var stream = new FileStream(path, FileMode.CreateNew)) stream.Write(bytes, 0, bytes.Length);
            return path;
        }

        private void ReplaceOwnedFixture(string path, byte[] bytes)
        {
            string preimage = path + "." + Guid.NewGuid().ToString("N") + ".before";
            File.Copy(path, preimage, false);
            Save("fixture-operation-" + Guid.NewGuid().ToString("N"), new
            {
                path, preimage, before = Hash(File.ReadAllBytes(path)), after = Hash(bytes),
                reason = "M12 controlled same-owner TOCTOU negative fixture", operation = "replace exact owned fixture"
            });
            File.WriteAllBytes(path, bytes);
        }

        [Test]
        public void R02_AdmissionStopPrecedesFailureReturn()
        {
            var gate = new CharacterAnimtorManager.NativeDecodeAdmission(100, 200);
            var failed = gate.AcquireAsync(60).GetAwaiter().GetResult();
            var other = gate.AcquireAsync(40).GetAwaiter().GetResult();
            var waiter = gate.AcquireAsync(60);
            try
            {
                Assert.That(gate.Waiting, Is.EqualTo(1));
                Assert.That(waiter.Status, Is.EqualTo(UniTaskStatus.Pending));
                gate.Stop();
                failed.Dispose();
                Save("r02-stop-before-return", new { waiter = waiter.Status.ToString(), gate.Reserved, gate.Active, gate.Waiting });
                Assert.Throws<OperationCanceledException>(() => waiter.GetAwaiter().GetResult());
                Assert.That(gate.Reserved, Is.EqualTo(40));
                Assert.That(gate.Active, Is.EqualTo(1));
                failed.Dispose();
                Assert.That(gate.Reserved, Is.EqualTo(40));
                other.Dispose();
                other.Dispose();
                Assert.That(gate.Reserved + gate.Active + gate.Waiting, Is.Zero);
                Assert.Throws<OperationCanceledException>(() => gate.AcquireAsync(1));
            }
            finally { gate.Stop(); failed.Dispose(); other.Dispose(); }
        }

        [UnityTest]
        public IEnumerator R02_RealWorkerFailureCancelsOrdinaryWaiter()
        {
            return UniTask.ToCoroutine(async () =>
            {
                byte[] bytes = Png(16, 16);
                string path = NewFile("r02-worker.png", bytes);
                string hash = Hash(bytes);
                var layout = BMPLoader.ReadVerifiedImageLayout(path, hash);
                long weight = CharacterAnimtorManager.NativeDecodeAdmission.Weight(layout, 64);
                Type managerType = typeof(CharacterAnimtorManager);
                Type nativeType = managerType.GetNestedType("NativePrewarmOperation", BindingFlags.NonPublic);
                Type requestType = managerType.GetNestedType("NativeBodyDecodeRequest", BindingFlags.NonPublic);
                Type groupType = managerType.GetNestedType("NativeBodySheetGroup", BindingFlags.NonPublic);
                var fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                foreach (bool shared in new[] { true, false })
                foreach (bool throwDecode in new[] { true, false })
                {
                    var gate = new CharacterAnimtorManager.NativeDecodeAdmission(2 * weight, 2 * weight);
                    var failed = await gate.AcquireAsync(weight);
                    var other = await gate.AcquireAsync(weight);
                    var waiter = gate.AcquireAsync(weight);
                    int thirdDecode = 0, actualDecode = 0;
                    async UniTask<bool> ObserveWaiter()
                    {
                        try
                        {
                            using (await waiter)
                            {
                                await UniTask.RunOnThreadPool(() =>
                                {
                                    Interlocked.Increment(ref thirdDecode);
                                    return BMPLoader.LoadVerifiedImageData(path, layout);
                                });
                            }
                            return true;
                        }
                        catch (OperationCanceledException) { return false; }
                    }
                    var observed = ObserveWaiter().Preserve();
                    var sprites = new HashSet<Sprite>();
                    var textures = new HashSet<Texture2D>();
                    var atlas = new List<BattleAtlasSourcePixels>();
                    var spriteLists = new Dictionary<int, List<Sprite>> { [56] = new List<Sprite> { null } };
                    var clamped = new Dictionary<string, Sprite>();
                    var clampedPaths = new Dictionary<Sprite, string>();
                    var info = new SpriteFileInfo { filePath = path, width = 8, height = 8,
                        row = throwDecode ? 1 : 0, col = 1, startFrame = 0, endFrame = 0 };
                    var ownedPics = new HashSet<int> { 0 };
                    object native = Activator.CreateInstance(nativeType, true);
                    object request = Activator.CreateInstance(requestType, true);
                    nativeType.GetField("Admission", fields).SetValue(native, gate);
                    nativeType.GetField("DecodeStarted", fields).SetValue(native, new Action<string>(_ =>
                    {
                        Interlocked.Increment(ref actualDecode);
                        if (throwDecode) throw new InvalidDataException("M12 R02 injected worker fault");
                    }));
                    requestType.GetField("Layout", fields).SetValue(request, layout);
                    requestType.GetField("Reservation", fields).SetValue(request, failed);
                    using (var cpu = new SemaphoreSlim(0, 1))
                    using (var upload = new SemaphoreSlim(1, 1))
                    {
                        UniTask<int> worker = default;
                        bool started = false;
                        try
                        {
                            Assert.That(gate.Waiting, Is.EqualTo(1));
                            Assert.That(gate.Reserved, Is.EqualTo(2 * weight));
                            Assert.That(observed.Status, Is.EqualTo(UniTaskStatus.Pending));
                            if (throwDecode)
                                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("处理文件失败:[\\s\\S]*M12 R02 injected worker fault"));
                            var source = BattleContentSource.ForLoganRuntime(Path.GetDirectoryName(path));
                            if (shared)
                            {
                                object group = Activator.CreateInstance(groupType, true);
                                groupType.GetField("ExpectedSha256", fields).SetValue(group, hash);
                                ((IList)groupType.GetField("Members", fields).GetValue(group)).Add((56, info, ownedPics));
                                var result = (UniTask<(int created, int failed)>)managerType.GetMethod(
                                    "ProcessNativeBodySheetGroupAsync", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,
                                    new object[] { group, spriteLists, sprites, textures, atlas, clamped, clampedPaths,
                                        null, cpu, upload, source, new Func<bool>(() => true), native, request });
                                worker = result.ContinueWith(value => value.failed).Preserve();
                            }
                            else
                            {
                                worker = ((UniTask<int>)managerType.GetMethod("ProcessAndCreateSpritesForCandidateAsync",
                                    BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,
                                    new object[] { 56, info, ownedPics, spriteLists, sprites, textures, atlas, clamped, clampedPaths,
                                        null, cpu, upload, source, hash, new Func<bool>(() => true), native, request })).Preserve();
                            }
                            started = true;
                            int value = await worker.Timeout(TimeSpan.FromSeconds(5));
                            bool granted = await observed.Timeout(TimeSpan.FromSeconds(5));
                            Save("r02-worker-" + shared + "-" + throwDecode, new
                            {
                                shared, throwDecode, value, granted, thirdDecode, actualDecode, weight,
                                gate.Reserved, gate.Active, gate.Waiting, gate.PendingPixels,
                                sprites = sprites.Count, textures = textures.Count
                            });
                            Assert.That(value, Is.EqualTo(shared ? 1 : -1));
                            Assert.That(granted, Is.False);
                            Assert.That(thirdDecode, Is.Zero);
                            Assert.That(actualDecode, Is.EqualTo(1));
                            Assert.That(gate.Reserved, Is.EqualTo(weight));
                            Assert.That(gate.Active, Is.EqualTo(1));
                            Assert.That(gate.Waiting + gate.PendingPixels, Is.Zero);
                            Assert.That(sprites.Count + textures.Count, Is.Zero);
                            other.Dispose();
                            other.Dispose();
                            Assert.That(gate.Reserved + gate.Active + gate.Waiting + gate.PendingPixels, Is.Zero);
                        }
                        finally
                        {
                            gate.Stop();
                            if (started) await worker;
                            ((IDisposable)request).Dispose();
                            failed.Dispose();
                            other.Dispose();
                            await observed;
                            CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures);
                            Save("r02-worker-cleanup-" + shared + "-" + throwDecode, new
                                { gate.Reserved, gate.Active, gate.Waiting, gate.PendingPixels, actualDecode, thirdDecode });
                        }
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator Q01_DescriptorIdentityDimensionsAndOriginalPixels()
        {
            return UniTask.ToCoroutine(async () =>
            {
                byte[] bytes = Png(8, 4);
                string path = NewFile("guard.png", bytes);
                var layout = BMPLoader.ReadVerifiedImageLayout(path, Hash(bytes));
                Assert.That(layout.IsPng && layout.Width == 8 && layout.Height == 4, Is.True);
                var oldPixels = await UniTask.RunOnThreadPool(() => BMPLoader.LoadVerifiedImageData(path, Hash(bytes)));
                var newPixels = await UniTask.RunOnThreadPool(() => BMPLoader.LoadVerifiedImageData(path, layout));
                CollectionAssert.AreEqual(oldPixels.Pixels, newPixels.Pixels);
                byte[] bmp = new byte[70];
                using (var writer = new BinaryWriter(new MemoryStream(bmp)))
                {
                    writer.Write((byte)'B'); writer.Write((byte)'M'); writer.Write(70); writer.Write(0); writer.Write(54);
                    writer.Write(40); writer.Write(2); writer.Write(2); writer.Write((short)1); writer.Write((short)24);
                    writer.Write(0); writer.Write(16); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
                    writer.Write(new byte[] { 0, 0, 255, 0, 255, 0, 0, 0, 255, 0, 0, 255, 255, 255, 0, 0 });
                }
                string bmpPath = NewFile("legacy.bmp", bmp);
                Assert.That(BMPLoader.ReadVerifiedImageLayout(bmpPath, Hash(bmp)).IsPng, Is.False);
                var legacy = await UniTask.RunOnThreadPool(() => BMPLoader.LoadVerifiedImageData(bmpPath, Hash(bmp)));
                Assert.That(legacy.Width == 2 && legacy.Height == 2 && !legacy.IsPng, Is.True);
                ReplaceOwnedFixture(path, bytes.Concat(new byte[] { 0 }).ToArray());
                Assert.Throws<InvalidDataException>(() => BMPLoader.LoadVerifiedImageData(path, layout));
                ReplaceOwnedFixture(path, bytes.Take(bytes.Length - 1).ToArray());
                Assert.Throws<InvalidDataException>(() => BMPLoader.LoadVerifiedImageData(path, layout));
                byte[] changed = (byte[])bytes.Clone();
                changed[changed.Length - 1] ^= 1;
                ReplaceOwnedFixture(path, changed);
                Assert.Throws<InvalidDataException>(() => BMPLoader.LoadVerifiedImageData(path, layout));
                foreach (int width in new[] { 0, -1, 33554433 })
                {
                    changed = (byte[])bytes.Clone();
                    changed[16] = (byte)(width >> 24); changed[17] = (byte)(width >> 16);
                    changed[18] = (byte)(width >> 8); changed[19] = (byte)width;
                    string invalid = NewFile("dimension-" + width + ".png", changed);
                    Assert.Throws<InvalidDataException>(() => BMPLoader.ReadVerifiedImageLayout(invalid, Hash(changed)));
                }
                string truncated = NewFile("short.png", bytes.Take(20).ToArray());
                Assert.Throws<InvalidDataException>(() => BMPLoader.ReadVerifiedImageLayout(truncated, Hash(File.ReadAllBytes(truncated))));
                Assert.Throws<OverflowException>(() => CharacterAnimtorManager.NativeDecodeAdmission.Weight(
                    new BMPLoader.VerifiedImageLayout(path, Hash(bytes), bytes.Length, int.MaxValue, int.MaxValue, true), 0));
                Save("descriptor", new { pass = true, equalPixels = oldPixels.Pixels.Length, grow = true, truncate = true, sameLengthMutation = true, dimensionsRejected = 3 });
            });
        }

        [Test]
        public void Q02_ReservationFifoOversizeCancellationAndMoreThanSixLeases()
        {
            var gate = new CharacterAnimtorManager.NativeDecodeAdmission(100, 200);
            var leases = new List<CharacterAnimtorManager.NativeDecodeAdmission.Reservation>();
            for (int i = 0; i < 9; i++) leases.Add(gate.AcquireAsync(10).GetAwaiter().GetResult());
            Assert.That(gate.Active, Is.EqualTo(9));
            var waiting = gate.AcquireAsync(101);
            Assert.That(waiting.Status, Is.EqualTo(UniTaskStatus.Pending));
            Assert.Throws<InvalidOperationException>(() => gate.AcquireAsync(1));
            foreach (var lease in leases) { lease.Dispose(); lease.Dispose(); }
            var oversized = waiting.GetAwaiter().GetResult();
            Assert.That(gate.Reserved, Is.EqualTo(101));
            var cancelled = gate.AcquireAsync(1);
            gate.Stop();
            Assert.Throws<OperationCanceledException>(() => cancelled.GetAwaiter().GetResult());
            oversized.Dispose();
            Assert.That(gate.Reserved + gate.Active + gate.Waiting, Is.Zero);
            var limits = new CharacterAnimtorManager.NativeDecodeAdmission(100, 200);
            limits.AcquireAsync(100).GetAwaiter().GetResult().Dispose();
            limits.AcquireAsync(200).GetAwaiter().GetResult().Dispose();
            Assert.Throws<InvalidDataException>(() => limits.AcquireAsync(201));
            Assert.That(limits.Reserved, Is.Zero);
            Save("reservation", new { gate.PeakActive, gate.PeakReserved, gate.Reserved, gate.Active, gate.Waiting, pass = true });
        }

        [UnityTest]
        public IEnumerator Q02B_OverCeilingRejectsBeforeDecodePreservingPublication()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var previous = fixture.M12Candidate("Prior");
                Assert.That(await fixture.M12Load(previous), Is.True);
                Manager.ConfigureNativeDecodeAdmission(true, 100, 200);
                int decoded = 0;
                Manager.NativeDecodeStartedForDiagnostics = _ => Interlocked.Increment(ref decoded);
                bool rejected = false;
                try { await fixture.M12Load(fixture.M12Candidate("OverCeiling")); }
                catch (InvalidDataException) { rejected = true; }
                Assert.That(rejected, Is.True);
                Assert.That(decoded, Is.Zero);
                Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(previous.SourceCacheKey));
                Assert.That(Manager.LastNativeDecodeAdmission.Reserved, Is.Zero);
                Save("ceiling-reject", new { rejected, decoded, preserved = true });
            });
        }

        [UnityTest]
        public IEnumerator Q03_RealDecodeShutdownPendingResumeAndStickyFailure()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var managerField = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object previous = managerField.GetValue(null);
                var driver = fixture.M12NewObject("M12ShutdownDriver").AddComponent<SimulationTickDriver>();
                var world = new SimulationWorld();
                typeof(SimulationTickDriver).GetField("_world", PrivateInstance).SetValue(driver, world);
                using (var entered = new ManualResetEventSlim())
                using (var release = new ManualResetEventSlim())
                {
                    UniTask<bool> load = default;
                    bool started = false;
                    managerField.SetValue(null, Manager);
                    try
                    {
                        Manager.ConfigureNativeDecodeAdmission(true);
                        Manager.NativeDecodeStartedForDiagnostics = _ =>
                        {
                            entered.Set();
                            if (!release.Wait(10000)) throw new TimeoutException("Fixture decode release was not signalled.");
                        };
                        load = fixture.M12Load(fixture.M12Candidate("Shutdown")).Preserve();
                        started = true;
                        await UniTask.WaitUntil(() => entered.IsSet).Timeout(TimeSpan.FromSeconds(5));
                        var pending = driver.ShutdownBattleRuntime();
                        Assert.That(pending.Status, Is.EqualTo(BattleRuntimeShutdownStatus.AlreadyStopping));
                        Assert.That(pending.CompletedStage, Is.EqualTo(BattleRuntimeShutdownStage.TickAndInputClosed));
                        Assert.That(driver.World, Is.SameAs(world));
                        Assert.That(driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true).Status, Is.EqualTo(BattleRuntimeShutdownStatus.AlreadyStopping));
                        Assert.Throws<InvalidOperationException>(() => driver.RecreateWorld());
                        var first = driver.ShutdownBattleRuntimeAsync();
                        var second = driver.ShutdownBattleRuntimeAsync();
                        release.Set();
                        Assert.That(await load.Timeout(TimeSpan.FromSeconds(5)), Is.False);
                        var finished = await first;
                        Assert.That((await second).CompletedStage, Is.EqualTo(finished.CompletedStage));
                        Assert.That(finished.RuntimeStagesCompleted, Is.True);
                        Assert.That(Manager.HasPendingNativePrewarmWork, Is.False);
                        var metrics = Manager.LastNativeDecodeAdmission;
                        Assert.That(metrics.Reserved + metrics.Active + metrics.Waiting + metrics.PendingPixels, Is.Zero);
                        var failed = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(false);
                        Assert.That(failed.Status, Is.EqualTo(BattleRuntimeShutdownStatus.Failed));
                        Assert.That(driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true).FailureReason, Is.EqualTo(failed.FailureReason));
                        Assert.That(driver.ShutdownBattleRuntime().Status, Is.EqualTo(BattleRuntimeShutdownStatus.Failed));
                        Save("shutdown", new { pending.CompletedStage, afterDrain = finished.CompletedStage, failed.FailureReason, noLatePublication = !Manager.IsPrewarmCompleted, metrics.Reserved, metrics.PendingPixels });
                    }
                    finally
                    {
                        release.Set();
                        Manager.CancelNativeContentPrewarm();
                        if (started) await load;
                        managerField.SetValue(null, previous);
                        Manager.NativeDecodeStartedForDiagnostics = null;
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator Q03B_CompetingGenerationsWaitForOldWorker()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var old = fixture.M12Candidate("OldGeneration");
                var middle = fixture.M12Candidate("MiddleGeneration");
                var latest = fixture.M12Candidate("LatestGeneration");
                Manager.ConfigureNativeDecodeAdmission(true);
                using (var entered = new ManualResetEventSlim())
                using (var release = new ManualResetEventSlim())
                {
                    int decoded = 0;
                    Manager.NativeDecodeStartedForDiagnostics = _ =>
                    {
                        if (Interlocked.Increment(ref decoded) != 1) return;
                        entered.Set();
                        if (!release.Wait(10000)) throw new TimeoutException("Old generation was not released.");
                    };
                    var a = fixture.M12Load(old).Preserve();
                    UniTask<bool> b = default, c = default;
                    bool submitted = false;
                    try
                    {
                        await UniTask.WaitUntil(() => entered.IsSet).Timeout(TimeSpan.FromSeconds(5));
                        b = fixture.M12Load(middle).Preserve();
                        c = fixture.M12Load(latest).Preserve();
                        submitted = true;
                        await UniTask.Yield();
                        Assert.That(decoded, Is.EqualTo(1));
                        Assert.That(b.Status, Is.EqualTo(UniTaskStatus.Pending));
                        Assert.That(c.Status, Is.EqualTo(UniTaskStatus.Pending));
                        release.Set();
                        Assert.That(await a, Is.False);
                        Assert.That(await b, Is.False);
                        Assert.That(await c.Timeout(TimeSpan.FromSeconds(10)), Is.True);
                        Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(latest.SourceCacheKey));
                        Assert.That(decoded, Is.EqualTo(2));
                        Save("generations", new { decoded, finalKey = Manager.PublishedVisualContentKey, oldAndMiddleCancelled = true });
                    }
                    finally
                    {
                        release.Set();
                        await a;
                        if (submitted) { await b; await c; }
                        Manager.NativeDecodeStartedForDiagnostics = null;
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator Q03C_FaultCannotFinishWhileAnotherWorkerIsBlocked()
        {
            return UniTask.ToCoroutine(async () =>
            {
                Manager.ConfigureNativeDecodeAdmission(true, 8L * 1024 * 1024);
                Manager.ReuseNativeBodySheetsForDiagnostics = true;
                var baseline = fixture.M12Candidate("Preserved");
                Assert.That(await fixture.M12Load(baseline), Is.True);
                var candidate = SixSheets();
                using (var blocked = new ManualResetEventSlim())
                using (var faulted = new ManualResetEventSlim())
                using (var release = new ManualResetEventSlim())
                {
                    int started = 0;
                    Manager.NativeDecodeStartedForDiagnostics = _ =>
                    {
                        int number = Interlocked.Increment(ref started);
                        if (number == 1)
                        {
                            blocked.Set();
                            if (!release.Wait(10000)) throw new TimeoutException("Fault fixture was not released.");
                        }
                        else if (number == 2)
                        {
                            faulted.Set();
                            throw new InvalidDataException("M12 injected worker fault");
                        }
                    };
                    LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("处理文件失败:[\\s\\S]*M12 injected worker fault"));
                    var load = fixture.M12Load(candidate).Preserve();
                    try
                    {
                        await UniTask.WaitUntil(() => blocked.IsSet && faulted.IsSet).Timeout(TimeSpan.FromSeconds(5));
                        await UniTask.Yield();
                        Assert.That(load.Status, Is.EqualTo(UniTaskStatus.Pending));
                        Assert.That(Manager.HasPendingNativePrewarmWork, Is.True);
                        Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(baseline.SourceCacheKey));
                        release.Set();
                        bool loaded = await load.Timeout(TimeSpan.FromSeconds(10));
                        Assert.That(loaded, Is.False);
                        var metrics = Manager.LastNativeDecodeAdmission;
                        Assert.That(metrics.Reserved + metrics.PendingPixels + metrics.Active + metrics.Waiting, Is.Zero);
                        Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(baseline.SourceCacheKey));
                        Save("fault-drain", new { started, loaded, preserved = true, metrics.Reserved, metrics.PendingPixels });
                    }
                    finally { release.Set(); await load; Manager.NativeDecodeStartedForDiagnostics = null; }
                }
            });
        }

        [UnityTest]
        public IEnumerator Q03D_AppAsyncCloseSharesPendingOwner()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var managerField = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                var driverField = typeof(NTSD.Tools.SingletonBehaviour<SimulationTickDriver>)
                    .GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
                object oldManager = managerField.GetValue(null), oldDriver = driverField.GetValue(null);
                var driver = fixture.M12NewObject("M12AppDriver").AddComponent<SimulationTickDriver>();
                var app = fixture.M12NewObject("M12AppClose").AddComponent<AppManager>();
                typeof(AppManager).GetField("battleSceneName", PrivateInstance).SetValue(app, "M12Absent-" + runId);
                using (var entered = new ManualResetEventSlim())
                using (var release = new ManualResetEventSlim())
                {
                    Manager.ConfigureNativeDecodeAdmission(true);
                    managerField.SetValue(null, Manager);
                    driverField.SetValue(null, driver);
                    Manager.NativeDecodeStartedForDiagnostics = _ =>
                    {
                        entered.Set();
                        if (!release.Wait(10000)) throw new TimeoutException("App fixture release timeout.");
                    };
                    var load = fixture.M12Load(fixture.M12Candidate("AppClose")).Preserve();
                    try
                    {
                        await UniTask.WaitUntil(() => entered.IsSet).Timeout(TimeSpan.FromSeconds(5));
                        var first = app.TryShutdownBattleRuntimeBeforeSceneDestroyAsync();
                        var second = app.TryShutdownBattleRuntimeBeforeSceneDestroyAsync();
                        Assert.That(first.Status, Is.EqualTo(UniTaskStatus.Pending));
                        Assert.That(driver.LifecycleState, Is.EqualTo(BattleRuntimeLifecycleState.Stopping));
                        release.Set();
                        Assert.That(await load, Is.False);
                        Assert.That(await first, Is.True);
                        Assert.That(await second, Is.True);
                        Assert.That(driver.LifecycleState, Is.EqualTo(BattleRuntimeLifecycleState.Stopped));
                        Save("app-async-close", new { acceptedTwice = true, completedOnce = true, stages = 11, actualSceneUnload = "NOT_RUN_EMPTY_MAP_BOUNDARY" });
                    }
                    finally
                    {
                        release.Set();
                        await load;
                        UnityEngine.Object.DestroyImmediate(app.gameObject);
                        driverField.SetValue(null, oldDriver);
                        managerField.SetValue(null, oldManager);
                        Manager.NativeDecodeStartedForDiagnostics = null;
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator Q04_ExistingSynchronousShutdownAndPixels()
        {
            Manager.ConfigureNativeDecodeAdmission(true);
            return fixture.NativeCoordinatorOn_DriverShutdownRecyclesGroupedUnpublishedImages();
        }

        [UnityTest]
        public IEnumerator Q05_ExistingSourceAndAtlasPixelOracle()
        {
            Manager.ConfigureNativeDecodeAdmission(true);
            return fixture.NativeCoordinatorOn_MultiDefinitionSource2DAndAtlasPixelsRemainEquivalent();
        }

        [UnityTest]
        public IEnumerator Q06_PublicConfiguredOwnerAndFallback()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var caller = new NTSD28B11SourceCacheCallerEditorTests();
                caller.SetUp();
                try
                {
                    caller.ConfigureM12(Output, true);
                    await caller.NativeDefaultAdmission_ConfiguredEntryPreservesFallbackAndRebuildsOwners().ToUniTask();
                }
                finally { caller.TearDown(); }
            });
        }

        private LoganVisualContentCandidate SixSheets()
        {
            string root = Path.GetFullPath(Path.Combine(Output, "fixtures", runId, "six"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            string catalog = "registry_section,registry_index,id,type,source_path,published_folder\n";
            int[] sizes = { 256, 384, 512, 256, 384, 512 };
            for (int i = 0; i < sizes.Length; i++)
            {
                string relative = "c/sheet" + i + ".png";
                using (var f = new FileStream(Path.Combine(root, "vfs", relative), FileMode.CreateNew))
                {
                    byte[] bytes = Png(sizes[i], sizes[i], i);
                    f.Write(bytes, 0, bytes.Length);
                }
                catalog += "object," + (7 + i) + "," + (56 + i) + ",0,a" + i + ".dat,missing\n";
                File.WriteAllText(Path.Combine(root, "decoded_dat/a" + i + ".dat"),
                    "<bmp_begin>\nname: M12Six" + i + "\nfile(20-19): " + relative + " w: " + sizes[i] + " h: " + sizes[i] +
                    " row: 1 col: 1\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n");
            }
            File.WriteAllText(Path.Combine(root, "catalog.csv"), catalog);
            return LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
        }

        [UnityTest]
        public IEnumerator Q06B_SixRealSheetsGateSharedAndOffPixels()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = SixSheets();
                Manager.ConfigureNativeDecodeAdmission(true, 8L * 1024 * 1024);
                GameConfig.Instance.BattleAtlasModeName = "SourceTexture2D";
                Manager.NativeBeforeUploadForDiagnostics = async () => { for (int i = 0; i < 5; i++) await UniTask.Yield(); };
                var readback = typeof(NTSD28B11AtomicPublicationEditorTests).GetMethod("ReadbackTexturePixels", BindingFlags.Static | BindingFlags.NonPublic);
                var evidence = new List<object>();
                foreach (bool shared in new[] { true, false })
                {
                    Manager.ReuseNativeBodySheetsForDiagnostics = shared;
                    Assert.That(await fixture.M12Load(candidate).Timeout(TimeSpan.FromSeconds(30)), Is.True);
                    for (int i = 0; i < 6; i++)
                    {
                        Assert.That(Manager.SpriteCatalog.TryGet(56 + i, 0, out var entry), Is.True);
                        string path = Path.Combine(candidate.Catalog.Source.RuntimeRoot, "vfs/c/sheet" + i + ".png");
                        var original = await UniTask.RunOnThreadPool(() => BMPLoader.LoadVerifiedImageData(path, candidate.GetImageSha256(path)));
                        Color32[] expected = original.Pixels.Select(p => (Color32)p).ToArray();
                        Color32[] actual = (Color32[])readback.Invoke(null, new object[] { entry.SharedTexture });
                        CollectionAssert.AreEqual(expected, actual, "Six-sheet independent original loader oracle " + i);
                        Assert.That(entry.PixelRect, Is.EqualTo(new Rect(0, 0, original.Width, original.Height)));
                    }
                    var m = Manager.LastNativeDecodeAdmission;
                    Assert.That(m.Reserved + m.PendingPixels + m.Active + m.Waiting, Is.Zero);
                    evidence.Add(new { shared, m.PeakReserved, m.PeakPendingPixels, m.RetainedPixels });
                }
                Save("six-sheet-correctness", evidence);
            });
        }

        [UnityTest]
        public IEnumerator Q07_OnlyOnceSameInputAbba()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = SixSheets();
                var results = new List<object>();
                var peaks = new List<long>();
                var retained = new List<long>();
                var durations = new List<double>();
                var identities = new List<string>();
                var total = Stopwatch.StartNew();
                Manager.ReuseNativeBodySheetsForDiagnostics = true;
                Manager.NativeBeforeUploadForDiagnostics = async () => { for (int i = 0; i < 5; i++) await UniTask.Yield(); };
                try
                {
                    foreach (bool enabled in new[] { false, true, true, false })
                    {
                        Manager.ConfigureNativeDecodeAdmission(enabled, 8L * 1024 * 1024);
                        var watch = Stopwatch.StartNew();
                        bool loaded = await fixture.M12Load(candidate).Timeout(TimeSpan.FromSeconds(30));
                        watch.Stop();
                        var m = Manager.LastNativeDecodeAdmission;
                        peaks.Add(m.PeakPendingPixels); retained.Add(m.RetainedPixels); durations.Add(watch.Elapsed.TotalMilliseconds);
                        identities.Add(Manager.PublishedVisualContentKey);
                        results.Add(new { enabled, loaded, milliseconds = watch.Elapsed.TotalMilliseconds,
                            m.PeakPendingPixels, m.RetainedPixels, m.PeakReserved, m.PeakActive, m.Reserved, m.PendingPixels });
                        Save("abba-leg-" + results.Count, results.Last());
                        Assert.That(loaded, Is.True);
                        Assert.That(m.Reserved + m.Active + m.Waiting + m.PendingPixels, Is.Zero);
                        Assert.That(total.Elapsed.TotalSeconds, Is.LessThan(180));
                    }
                    double baselinePeak = (peaks[0] + peaks[3]) / 2.0;
                    double candidatePeak = (peaks[1] + peaks[2]) / 2.0;
                    double ratio = (durations[1] + durations[2]) / (durations[0] + durations[3]);
                    Save("abba-result", new { results, baselinePeak, candidatePeak, reduction = 1 - candidatePeak / baselinePeak, costRatio = ratio,
                        scope = "managed float/processed source arrays held by body workers; retained arrays reported separately; not whole-process or Android memory" });
                    Assert.That(identities.Distinct().Count(), Is.EqualTo(1));
                    Assert.That(retained.Distinct().Count(), Is.EqualTo(1));
                    Assert.That(candidatePeak, Is.LessThanOrEqualTo(baselinePeak * 0.75));
                    Assert.That(ratio, Is.LessThanOrEqualTo(1.2));
                }
                finally { Manager.NativeBeforeUploadForDiagnostics = null; }
            });
        }
    }
}
#endif


#if UNITY_EDITOR
namespace NTSD.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NTSD.Animation;
    using NTSD.App;
    using NTSD.Simulation;
    using NTSD.Simulation.Presentation;
    using NTSD.UI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class M12RealSceneCloseDiagnostics
    {
        public const string RunGuid = "64935977fe0c414d8d0b7bafcf2a6e8b";
        private const string Project = "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity";
        private const string Base = Project + "/artifacts/diagnostics/NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-20261011/implementation-01/review-02";
        public const string RunRoot = Base + "/play-01/" + RunGuid;
        public const string InterfaceRoot = Base + "/diagnostic-interface-01";
        private const string RevisionRoot = InterfaceRoot + "/revision-01";
        private const string FrozenInputSha256 = "7f0f9fae4d31d2447ec7041c898dfd28cd7752397727db4b3b8ac55743f2cf15";
        public const string Schema = "ntsd.m12.real-scene-close.interface.v1";
        private const string Source = "Assets/NTSD/Scripts/Test/Editor/M12DecodeUploadBackpressureEditorTests.cs";
        private const string ConfigPath = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuPrefix = "Tools/NTSD/Diagnostics/M12/64935977/";
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static RunOwner owner;

        [MenuItem(MenuPrefix + "Read Static Plan")]
        private static void MenuReadStaticPlan() { ReadStaticPlan(); }
        [MenuItem(MenuPrefix + "Prepare")]
        private static void MenuPrepare() { Prepare(); }
        [MenuItem(MenuPrefix + "Enter Play")]
        private static void MenuEnter() { Enter(); }
        [MenuItem(MenuPrefix + "Run Two Cycles")]
        private static void MenuRun() { Run(); }
        [MenuItem(MenuPrefix + "Read Status")]
        private static void MenuStatus() { UnityEngine.Debug.Log(ReadStatus()); }
        [MenuItem(MenuPrefix + "Cleanup Only")]
        private static void MenuCleanup() { CleanupOnly(); }
        [MenuItem(MenuPrefix + "Exit If Safe")]
        private static void MenuExit() { ExitIfSafe(); }
        [MenuItem(MenuPrefix + "Restore After Exit")]
        private static void MenuRestore() { RestoreAfterExit(); }

        public static string ReadStaticPlan()
        {
            CheckProject();
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling &&
                !EditorApplication.isUpdating && owner == null, "Cold interface requires idle Edit mode.");
            Require(!Directory.Exists(RunRoot), "Future play root must not exist during cold qualification.");
            var contract = ReadJson(InterfaceRoot + "/source-contract.json");
            var compiled = ReadJson(InterfaceRoot + "/compile-identity.json");
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            Require(config != null, "Original GameConfig asset is missing.");
            var settings = BattleRuntimeProfileProductionSource.Resolve(config);
            var backend = BattlePresentationBackendResolver.Resolve(config);
            string sourceHash = HashFile(Project + "/" + Source);
            string runtimeHash = HashFile(Project + "/Library/ScriptAssemblies/Assembly-CSharp.dll");
            string editorHash = HashFile(Project + "/Library/ScriptAssemblies/Assembly-CSharp-Editor.dll");
            var errors = new List<string>();
            if (sourceHash != (string)contract["sourceSha256"]) errors.Add("Source identity differs from registered candidate.");
            if (runtimeHash != (string)compiled["runtimeSha256"]) errors.Add("Runtime assembly identity changed.");
            if (editorHash != (string)compiled["editorSha256"]) errors.Add("Editor assembly identity changed.");
            if (settings.Profile != BattleRuntimeProfile.Authority400 ||
                settings.InitialRuntimeSlotCapacity != 400 || settings.MaxActiveRuntimeEntities != 400)
                errors.Add("Final Resolve profile/capacity differs from frozen expectation.");
            if (backend != BattlePresentationBackendMode.CentralOnly)
                errors.Add("Final Resolve backend differs from frozen expectation.");
            if (AssetDatabase.AssetPathToGUID(ConfigPath) != "513170c32731baa449d4d0e9abf8c827")
                errors.Add("Original configuration GUID changed.");
            if (AssetDatabase.GetAssetPath(config.LF2ObjectPrefab) != "Assets/NTSD/Prefabs/Common/EntityObject.prefab" ||
                AssetDatabase.GetAssetPath(config.ShadowPrefab) != "Assets/NTSD/Prefabs/Common/Shadow.prefab")
                errors.Add("Original prefab references differ.");
            Scene active = SceneManager.GetActiveScene();
            if (active.path != MenuScene || active.isDirty || active.rootCount != 8 ||
                SceneManager.sceneCount != 1) errors.Add("Original clean Menu scene setup differs.");
            var result = new
            {
                schema = Schema, runGuid = RunGuid, status = errors.Count == 0 ? "PASS" : "FAIL",
                utc = DateTime.UtcNow.ToString("O"),
                editorPid = System.Diagnostics.Process.GetCurrentProcess().Id,
                project = Path.GetFullPath(Application.dataPath + "/..").Replace('\\', '/'),
                sourceSha256 = sourceHash, runtimeSha256 = runtimeHash, editorSha256 = editorHash,
                loadedType = typeof(M12RealSceneCloseDiagnostics).AssemblyQualifiedName,
                loadedAssemblyLocation = typeof(M12RealSceneCloseDiagnostics).Assembly.Location,
                profile = settings.Profile.ToString(), capacity = settings.InitialRuntimeSlotCapacity,
                maximumActive = settings.MaxActiveRuntimeEntities, backend = backend.ToString(),
                configuredAi = config.BattleAiExecutionProfileName,
                originalRoot = config.BattleContentRuntimeRoot,
                prefab = AssetDatabase.GetAssetPath(config.LF2ObjectPrefab),
                shadow = AssetDatabase.GetAssetPath(config.ShadowPrefab),
                configGuid = AssetDatabase.AssetPathToGUID(ConfigPath),
                scenes = CaptureScenes(), protectedFiles = CaptureFiles(),
                runtimeOwnerCreated = false, futureRunRootExists = Directory.Exists(RunRoot),
                contentPreparationRun = false, audioPreparationRun = false,
                parameterCollection = false, errors
            };
            SaveNew(InterfaceRoot + "/interface-readstaticplan.json", result);
            Require(errors.Count == 0, "M12 cold interface qualification failed; see original receipt.");
            return JsonConvert.SerializeObject(result);
        }

        public static string Prepare()
        {
            CheckProject();
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling &&
                !EditorApplication.isUpdating && owner == null, "Prepare requires idle Edit mode.");
            Require(!Directory.Exists(RunRoot), "Frozen run root already exists; no replacement or retry.");
            VerifyRevisionIdentity();
            Scene active = SceneManager.GetActiveScene();
            Require(active.path == MenuScene && !active.isDirty && active.rootCount == 8 &&
                SceneManager.sceneCount == 1, "Original clean Menu setup required.");
            string manifestPath = RevisionRoot + "/frozen-input-manifest.json";
            Require(HashFile(manifestPath) == FrozenInputSha256, "Frozen input manifest changed.");
            var manifest = ReadJson(manifestPath);
            var inputs = (JArray)manifest["files"];
            Require((string)manifest["runGuid"] == RunGuid && inputs.Count == 24 &&
                (long)manifest["bytes"] == 232571, "Expected the frozen six fixtures plus eighteen WAVs.");
            var destinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            long bytes = 0;
            foreach (JObject input in inputs)
            {
                string source = Path.GetFullPath(Path.Combine(Project, (string)input["source"]));
                string destination = InputDestination(input);
                Require(source.StartsWith(Path.GetFullPath(Project + "/"), StringComparison.OrdinalIgnoreCase) &&
                    destinations.Add(destination), "Input escaped project or repeated destination.");
                Require(new FileInfo(source).Length == (long)input["bytes"] &&
                    HashFile(source) == (string)input["sha256"], "Frozen source bytes changed.");
                bytes += (long)input["bytes"];
            }
            Require(bytes == 232571, "Frozen input byte total differs.");
            var protection = CaptureFiles();
            var setup = CaptureScenes();
            Directory.CreateDirectory(RunRoot);
            SaveNew(RunRoot + "/prepare.json", new
            {
                schema = Schema, runGuid = RunGuid, editorPid = System.Diagnostics.Process.GetCurrentProcess().Id,
                utc = DateTime.UtcNow.ToString("O"), scenes = setup, protectedFiles = protection,
                sourceSha256 = HashFile(Project + "/" + Source),
                revisionContractSha256 = HashFile(RevisionRoot + "/revision-contract.json"),
                compiledIdentitySha256 = HashFile(RevisionRoot + "/compile-identity.json"),
                inputManifestSha256 = FrozenInputSha256
            });
            foreach (JObject input in inputs)
            {
                string destination = InputDestination(input);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(Path.Combine(Project, (string)input["source"]), destination, false);
                Require(HashFile(destination) == (string)input["sha256"], "Copied input changed.");
            }
            SaveNew(RunRoot + "/prepared.json", new { runGuid = RunGuid, inputCount = 24, inputBytes = bytes });
            return "PREPARED";
        }

        private static string InputDestination(JObject input)
        {
            string destination = Path.GetFullPath(RunRoot + "/runtime/" + (string)input["relative"]);
            Require(destination.StartsWith(Path.GetFullPath(RunRoot + "/runtime/"),
                StringComparison.OrdinalIgnoreCase), "Fixture destination escaped run root.");
            return destination;
        }

        private static void VerifyRevisionIdentity()
        {
            CheckProject();
            var contract = ReadJson(RevisionRoot + "/revision-contract.json");
            var compiled = ReadJson(RevisionRoot + "/compile-identity.json");
            Require((string)contract["revision"] == "revision-01" && (string)contract["runGuid"] == RunGuid &&
                (string)contract["project"] == Project &&
                (int)contract["editorPid"] == System.Diagnostics.Process.GetCurrentProcess().Id,
                "Revision project/PID/run identity differs.");
            Require((string)compiled["status"] == "COMPILE_PASS" &&
                (string)compiled["runGuid"] == RunGuid &&
                (int)compiled["editorPid"] == System.Diagnostics.Process.GetCurrentProcess().Id &&
                (string)compiled["contractSha256"] == HashFile(RevisionRoot + "/revision-contract.json"),
                "Revision compiled identity is missing or mismatched.");
            Require((string)contract["sourceSha256"] == HashFile(Project + "/" + Source) &&
                (string)compiled["sourceSha256"] == (string)contract["sourceSha256"] &&
                (string)compiled["runtimeSha256"] == HashFile(Project + "/Library/ScriptAssemblies/Assembly-CSharp.dll") &&
                (string)compiled["runtimeSha256"] == (string)contract["runtimeSha256"] &&
                (string)compiled["editorSha256"] == HashFile(Project + "/Library/ScriptAssemblies/Assembly-CSharp-Editor.dll"),
                "Current source or compiled assembly differs from this revision.");
            Require(string.Equals(Path.GetFullPath(typeof(M12RealSceneCloseDiagnostics).Assembly.Location),
                Path.GetFullPath(Project + "/Library/ScriptAssemblies/Assembly-CSharp-Editor.dll"),
                StringComparison.OrdinalIgnoreCase), "Diagnostic loaded from another assembly path.");
            Require((string)contract["inputManifestSha256"] == FrozenInputSha256 &&
                HashFile(RevisionRoot + "/frozen-input-manifest.json") == FrozenInputSha256 &&
                HashFile(InterfaceRoot + "/interface-readstaticplan.json") ==
                    "c2eabab457f8897aa07eb8810a08c7aae1a31ca6e107553551cb60a776b2fb09",
                "Frozen inputs or original cold evidence changed.");
            VerifyFiles(contract);
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            var references = contract["references"];
            Require(config != null && AssetDatabase.AssetPathToGUID(ConfigPath) == (string)references["configGuid"] &&
                AssetDatabase.GetAssetPath(config.LF2ObjectPrefab) == (string)references["prefab"] &&
                AssetDatabase.GetAssetPath(config.ShadowPrefab) == (string)references["shadow"] &&
                config.BattleContentRuntimeRoot == (string)references["originalRoot"],
                "Original configuration references differ.");
        }

        public static string Enter()
        {
            CheckProject();
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && owner == null &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating, "Enter requires idle Edit mode.");
            Require(File.Exists(RunRoot + "/prepared.json") && !File.Exists(RunRoot + "/enter-issued.json"),
                "Frozen run has not prepared or Enter was already issued.");
            var preparation = ReadJson(RunRoot + "/prepare.json");
            VerifyRevisionIdentity();
            VerifyFiles(preparation);
            Require((int)preparation["editorPid"] == System.Diagnostics.Process.GetCurrentProcess().Id,
                "Editor process changed.");
            SceneReceipt[] saved = preparation["scenes"].ToObject<SceneReceipt[]>();
            SceneReceipt[] current = CaptureScenes();
            Require(current.Length == saved.Length && current.Zip(saved, (a, b) =>
                a.path == b.path && a.loaded == b.loaded && a.active == b.active &&
                !a.dirty && a.roots == b.roots).All(x => x),
                "Original prepared scene setup changed; do not replace user work.");
            SaveNew(RunRoot + "/enter-issued.json", new
            {
                runGuid = RunGuid, utc = DateTime.UtcNow.ToString("O"),
                editorPid = System.Diagnostics.Process.GetCurrentProcess().Id
            });
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.isPlaying = true;
            return "ENTER_REQUESTED";
        }

        public static string Run()
        {
            CheckProject();
            Require(EditorApplication.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Run requires the expected completed Enter reload.");
            VerifyRevisionIdentity();
            Require(owner == null && File.Exists(RunRoot + "/enter-issued.json") &&
                !File.Exists(RunRoot + "/play-start.json"), "Owner exists, run already started, or owner was lost.");
            var preparation = ReadJson(RunRoot + "/prepare.json");
            Require((int)preparation["editorPid"] == System.Diagnostics.Process.GetCurrentProcess().Id,
                "Editor process changed.");
            VerifyFiles(preparation);
            Require(SceneManager.sceneCount == 1 && SceneManager.GetActiveScene().rootCount == 0,
                "Expected empty unsaved host.");
            Require(AppManager.Instance == null && GameConfig.Instance == null,
                "Old App/config owner remains after Enter reload.");
            owner = new RunOwner(DateTime.Parse((string)ReadJson(RunRoot + "/enter-issued.json")["utc"],
                null, System.Globalization.DateTimeStyles.RoundtripKind));
            SaveNew(RunRoot + "/play-start.json", new { runGuid = RunGuid, utc = DateTime.UtcNow.ToString("O") });
            owner.Start();
            return "RUN_ACCEPTED";
        }

        public static string ReadStatus()
        {
            CheckProject();
            return owner == null
                ? JsonConvert.SerializeObject(new
                {
                    runGuid = RunGuid, hotOwner = false,
                    state = File.Exists(RunRoot + "/play-start.json") &&
                        !File.Exists(RunRoot + "/exit-issued.json") ? "OWNER_LOST" : "NO_HOT_OWNER",
                    playing = EditorApplication.isPlaying
                })
                : owner.Status();
        }

        public static string CleanupOnly()
        {
            Require(owner != null && EditorApplication.isPlaying, "No surviving hot owner; cannot recreate cleanup.");
            return owner.StartCleanup();
        }

        public static string ExitIfSafe()
        {
            Require(owner != null && EditorApplication.isPlaying, "No surviving Play owner.");
            Require(!File.Exists(RunRoot + "/exit-issued.json"), "Exit was already issued.");
            owner.CheckSafeExit();
            owner.CaptureWarmupExit();
            owner.ReleaseOwnedResourcesForExit();
            SaveNew(RunRoot + "/exit-issued.json", new
            {
                runGuid = RunGuid, utc = DateTime.UtcNow.ToString("O"),
                state = owner.State, firstFailure = owner.FirstFailure,
                ownedResourcesReleased = owner.OwnedResourcesReleased,
                sourceSha256 = HashFile(Project + "/" + Source)
            });
            owner.CheckSafeExit();
            owner.PrepareForExpectedExit();
            EditorApplication.isPlaying = false;
            return "EXIT_REQUESTED";
        }

        public static string RestoreAfterExit()
        {
            CheckProject();
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling &&
                !EditorApplication.isUpdating, "Expected Exit reload has not finished.");
            Require(File.Exists(RunRoot + "/exit-issued.json") &&
                File.Exists(RunRoot + "/expected-exit-reload.json") &&
                !File.Exists(RunRoot + "/external-interruption.json") && !File.Exists(RunRoot + "/result.json"),
                "No safe Exit receipt or result already exists.");
            var preparation = ReadJson(RunRoot + "/prepare.json");
            Require((int)preparation["editorPid"] == System.Diagnostics.Process.GetCurrentProcess().Id,
                "Editor process changed.");
            VerifyFiles(preparation);
            Require(CaptureScenes().All(x => !x.dirty), "Do not replace dirty post-Exit scenes.");
            var cloneRelease = ReadJson(RunRoot + "/owned-resources-released.json");
            Require((string)cloneRelease["runGuid"] == RunGuid &&
                (bool)cloneRelease["cloneAbsentAfterRelease"] && (bool)cloneRelease["handlesDisposed"],
                "No qualified hot-owner release evidence; never reconstruct ownership from instance IDs.");
            int releasedCloneId = (int)cloneRelease["cloneId"];
            bool cloneNeverCreated = cloneRelease["cloneCreated"]?.Type == JTokenType.Boolean && !(bool)cloneRelease["cloneCreated"];
            Require(cloneNeverCreated ? releasedCloneId == 0 : releasedCloneId != 0, "Clone identity was not recorded.");
            bool cloneAbsent = cloneNeverCreated || EditorUtility.InstanceIDToObject(releasedCloneId) == null;
            var warmup = ReadJson(RunRoot + "/warmup-before-exit.json");
            int textureId = (int)warmup["textureId"], spriteId = (int)warmup["spriteId"];
            bool warmupNotReached = (string)warmup["status"] == "NOT_REACHED" &&
                warmup["menuLoadRequested"]?.Type == JTokenType.Boolean && !(bool)warmup["menuLoadRequested"] &&
                warmup["contentRequested"]?.Type == JTokenType.Boolean && !(bool)warmup["contentRequested"] &&
                textureId == 0 && spriteId == 0;
            Require(warmupNotReached || (textureId != 0 && spriteId != 0), "Warmup identity is absent from evidence.");
            bool absent = !warmupNotReached && EditorUtility.InstanceIDToObject(textureId) == null &&
                EditorUtility.InstanceIDToObject(spriteId) == null;
            SceneReceipt[] saved = preparation["scenes"].ToObject<SceneReceipt[]>();
            var setup = saved.Select(x => new SceneSetup
            {
                path = x.path, isLoaded = x.loaded, isActive = x.active
            }).ToArray();
            EditorSceneManager.RestoreSceneManagerSetup(setup);
            SceneReceipt[] restored = CaptureScenes();
            Require(restored.Length == saved.Length && restored.Zip(saved, (a, b) =>
                a.path == b.path && a.loaded == b.loaded && a.active == b.active &&
                !a.dirty && a.roots == b.roots).All(x => x), "Scene restoration differs.");
            VerifyFiles(preparation);
            var exited = ReadJson(RunRoot + "/exit-issued.json");
            bool noFailure = string.IsNullOrEmpty((string)exited["firstFailure"]);
            var reload = ReadJson(RunRoot + "/expected-exit-reload.json");
            bool logComplete = (int)reload["errors"] == 1 && (int)reload["acceptedError"] == 1 &&
                (int)reload["logIoFailure"] == 0 && (bool)reload["logPersistenceComplete"] &&
                !File.Exists(RunRoot + "/log-extra.json") && !File.Exists(RunRoot + "/log-after-seal.json");
            var result = new
            {
                runGuid = RunGuid, status = absent && cloneAbsent && noFailure && logComplete ? "RESTORED_REQUIRES_POST_EXIT_LOG_REVIEW" : "FAIL",
                scope = "Two real Scene lifecycle cycles only; no performance/GC/device certificate",
                warmup = warmupNotReached ? "NOT_REACHED" : absent ? "NOT_PRESENT_AFTER_EXIT" : "RETAINED_OR_ID_REUSED_UNKNOWN",
                clone = cloneAbsent ? "OWNED_RELEASE_AND_ABSENCE_OBSERVED" : "PRESENT_OR_ID_REUSED_UNKNOWN",
                cloneId = releasedCloneId, cloneType = (string)cloneRelease["cloneType"],
                scenes = restored, firstFailure = (string)exited["firstFailure"],
                postExitLogs = "EXTERNAL_ORIGINAL_EDITOR_CONSOLE_REVIEW_REQUIRED",
                previousNative = "UNKNOWN_UNTOUCHED", utc = DateTime.UtcNow.ToString("O")
            };
            SaveNew(RunRoot + "/result.json", result);
            return JsonConvert.SerializeObject(result);
        }

        private static void CheckProject()
        {
            Require(string.Equals(Path.GetFullPath(Application.dataPath + "/..").TrimEnd('\\', '/'),
                Path.GetFullPath(Project).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase),
                "Wrong project; frozen run cannot be redirected.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static JObject ReadJson(string path)
        {
            return JObject.Parse(File.ReadAllText(path));
        }

        private static void SaveNew(string path, object value)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)))
                writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private static string HashFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private static FileReceipt[] CaptureFiles()
        {
            return new[]
            {
                ConfigPath, MenuScene, BattleScene, "ProjectSettings/EditorSettings.asset",
                "Assets/NTSD/Prefabs/Common/EntityObject.prefab", "Assets/NTSD/Prefabs/Common/Shadow.prefab",
                "Assets/NTSD/Map/BattleMapCatalog.asset", "Assets/NTSD/Map/SunagakureMap.asset",
                "Assets/NTSD/Sprite/OtherBg/Sunagakure.png", Source,
                "Library/ScriptAssemblies/Assembly-CSharp.dll", "Library/ScriptAssemblies/Assembly-CSharp-Editor.dll"
            }.Select(x => new FileReceipt { path = x, sha256 = HashFile(Project + "/" + x) }).ToArray();
        }

        private static void VerifyFiles(JObject receipt)
        {
            foreach (var file in receipt["protectedFiles"].ToObject<FileReceipt[]>())
                Require(HashFile(Project + "/" + file.path) == file.sha256, "Protected file changed: " + file.path);
        }

        private static SceneReceipt[] CaptureScenes()
        {
            var result = new List<SceneReceipt>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                result.Add(new SceneReceipt
                {
                    path = scene.path, loaded = scene.isLoaded, active = scene == SceneManager.GetActiveScene(),
                    dirty = scene.isDirty, roots = scene.rootCount
                });
            }
            return result.ToArray();
        }

        private static T Field<T>(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(name, Fields);
            Require(field != null, "Diagnostic field missing: " + name);
            return (T)field.GetValue(value);
        }

        private static T SceneObject<T>() where T : Component
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>()
                .Where(x => x != null && x.gameObject.scene.IsValid() && x.gameObject.scene.isLoaded).ToArray();
            Require(objects.Length == 1, "Expected one live " + typeof(T).Name + ", got " + objects.Length);
            return objects[0];
        }

        [Serializable]
        private sealed class FileReceipt
        {
            public string path;
            public string sha256;
        }

        [Serializable]
        private sealed class SceneReceipt
        {
            public string path;
            public bool loaded;
            public bool active;
            public bool dirty;
            public int roots;
        }

        private sealed class ErrorReceipt
        {
            public string condition, stack, type, utc;
            public int closeAccepted, thread;
        }

        private sealed class RunOwner
        {
            public string State { get; private set; } = "STARTING";
            public string FirstFailure { get; private set; }
            private readonly DateTime enteredUtc;
            private readonly ManualResetEventSlim release = new ManualResetEventSlim();
            private readonly ManualResetEventSlim decoded = new ManualResetEventSlim();
            private CancellationTokenSource stop = new CancellationTokenSource();
            private GameConfig clone, previousConfig;
            private readonly string evidenceRoot;
            private bool previousConfigCaptured, cloneCreated, cloneInstalled, menuLoadRequested;
            private bool externalExitingObserved, externalEnteredEditObserved;
            private AppManager app;
            private CharacterAnimtorManager manager;
            private LoadingPrewarmController loader;
            private SimulationTickDriver driver;
            private SimulationWorld firstWorld;
            private MenuUIController menu;
            private UniTask pipeline;
            private UniTask loading;
            private UniTask drain;
            private UniTask<AsyncOperation> closing;
            private bool hasLoading, hasDrain, hasClosing, pumping, sceneHandler, expectedExit;
            private AsyncOperation loadOperation, unloadOperation;
            private int generation, closeAccepted, bodyStarts, barrierTimeout, errors, logIoFailure, acceptedError;
            private ErrorReceipt firstError;
            private int phaseIndex, textureId, spriteId;
            private HashSet<int> beforeTextures, beforeSprites;
            private DateTime phaseDeadline;
            private string phase;
            private bool cleanup;
            private Action<string> previousDecodeHook;
            private bool shutdownWitness;
            private bool cleanupAccepted, handlesDisposed, cloneReleased, startCatchAndFollowingFrameObserved;
            private bool attributionExamined, attributionMatched;
            private int cloneId, decodeCallbacks, logWriters, persistedLogs, lateLogs;
            private string cloneType;
            private readonly object logGate = new object();
            private bool logsSealed;
            public bool OwnedResourcesReleased => handlesDisposed && cloneReleased;

            public RunOwner(DateTime entered, string outputRoot = RunRoot)
            {
                enteredUtc = entered;
                evidenceRoot = outputRoot;
            }

            public void Start()
            {
                Application.logMessageReceivedThreaded += OnLog;
                AssemblyReloadEvents.beforeAssemblyReload += OnReload;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                SceneManager.sceneLoaded += OnSceneLoaded;
                sceneHandler = true;
                pumping = true;
                EditorApplication.update += Tick;
                pipeline = RunAsync().Preserve();
            }

            private void Begin(string next)
            {
                Require(++phaseIndex <= 32, "Diagnostic phase evidence capacity exhausted.");
                phase = next;
                phaseDeadline = DateTime.UtcNow.AddSeconds(30);
                SaveNew(evidenceRoot + "/phase-" + phaseIndex.ToString("D2") + ".json", new
                {
                    phase = next, state = State, utc = DateTime.UtcNow.ToString("O"),
                    managerPending = manager != null && manager.HasPendingNativePrewarmWork,
                    driverState = ReferenceEquals(driver, null) ? null : driver.LifecycleState.ToString()
                });
            }

            private async UniTask Wait(Func<bool> predicate)
            {
                while (!predicate())
                {
                    stop.Token.ThrowIfCancellationRequested();
                    await UniTask.Yield(PlayerLoopTiming.Update, stop.Token);
                }
            }

            private async UniTask RunAsync()
            {
                try
                {
                    Begin("menu_load");
                    var original = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
                    CreateOwnedClone(original);
                    SaveNew(evidenceRoot + "/owned-clone-created.json", new
                    {
                        runGuid = RunGuid, cloneId, cloneType, cloneName = clone.name,
                        originalAssetId = original.GetInstanceID(), nonAsset = true,
                        owner = "M12RealSceneCloseDiagnostics.RunOwner", liveReferenceHeld = true
                    });
                    clone.BattleContentRuntimeRoot = evidenceRoot + "/runtime";
                    var a = JObject.Parse(EditorJsonUtility.ToJson(original));
                    var b = JObject.Parse(EditorJsonUtility.ToJson(clone));
                    JObject difference = M12CloneComparison.Compare(a, b, clone.BattleContentRuntimeRoot);
                    M12CloneComparison.SaveNew(evidenceRoot + "/clone-comparison.json", new
                    {
                        original = a, clone = b, firstDifference = difference,
                        originalReferences = M12CloneComparison.References(original),
                        cloneReferences = M12CloneComparison.References(clone)
                    });
                    Require(difference == null, "Clone changed fields other than runtime root: " + difference);
                    GameConfig.Instance = clone;
                    cloneInstalled = ReferenceEquals(GameConfig.Instance, clone);
                    Require(cloneInstalled, "Clone installation failed.");
                    menuLoadRequested = true;
                    loadOperation = SceneManager.LoadSceneAsync(MenuScene, LoadSceneMode.Single);
                    Require(loadOperation != null, "Menu load did not start.");
                    await Wait(() => loadOperation.isDone);
                    app = SceneObject<AppManager>();
                    loader = SceneObject<LoadingPrewarmController>();
                    menu = SceneObject<MenuUIController>();
                    manager = CharacterAnimtorManager.Instance;
                    Require(ReferenceEquals(GameConfig.Instance, clone), "Menu Awake replaced clone.");
                    Require(!loader.gameObject.activeSelf && Field<bool>(loader, "runOnStart") &&
                        loader.LoadingResourceTxt != null, "Real Loading controller is not cold/complete.");
                    Require(Field<Sprite[]>(loader, "loadingSprites").Length != 0 &&
                        Field<UnityEngine.UI.Image>(loader, "RandomImg") != null, "Loading image references missing.");
                    var settings = BattleRuntimeProfileProductionSource.Resolve(clone);
                    Require(settings.Profile == BattleRuntimeProfile.Authority400 &&
                        settings.InitialRuntimeSlotCapacity == 400 &&
                        BattlePresentationBackendResolver.Resolve(clone) == BattlePresentationBackendMode.CentralOnly,
                        "Runtime configuration changed after cold qualification.");
                    Require(manager.ReuseNativeBodySheetsForDiagnostics && !manager.HasPendingNativePrewarmWork,
                        "Shared default/idle manager required.");
                    previousDecodeHook = manager.NativeDecodeStartedForDiagnostics;
                    Require(previousDecodeHook == null && manager.NativeBeforeUploadForDiagnostics == null,
                        "Another diagnostic owns image hooks.");
                    manager.ConfigureNativeDecodeAdmission(true);
                    manager.NativeDecodeStartedForDiagnostics = OnDecode;
                    beforeTextures = new HashSet<int>(Resources.FindObjectsOfTypeAll<Texture2D>().Select(x => x.GetInstanceID()));
                    beforeSprites = new HashSet<int>(Resources.FindObjectsOfTypeAll<Sprite>().Select(x => x.GetInstanceID()));
                    Begin("cycle1_start_loading");
                    menu.ShowLoading();
                    await Wait(() => decoded.IsSet);
                    Require(manager.HasPendingNativePrewarmWork && Volatile.Read(ref bodyStarts) == 1,
                        "No unique actual body worker pending.");
                    generation = Field<int>(loader, "prewarmGeneration");
                    loading = loader.PrewarmOnceAsync().Preserve();
                    hasLoading = true;
                    Require(Field<bool>(loader, "prewarmRunning") &&
                        Field<int>(loader, "prewarmGeneration") == generation,
                        "Captured Loading task must be the already running generation.");
                    drain = manager.WaitForNativePrewarmDrainAsync().Preserve();
                    hasDrain = true;
                    CaptureWarmup();
                    Begin("cycle1_load_and_pending_close");
                    loadOperation = app.LoadBattleAdditive();
                    Require(loadOperation != null, "First real Battle load did not start.");
                    await Wait(() => Volatile.Read(ref closeAccepted) == 1 && hasClosing);
                    stop.Token.ThrowIfCancellationRequested();
                    Require(manager.HasPendingNativePrewarmWork, "Image hard-gate witness was lost.");
                    SignalRelease();
                    Begin("cycle1_join_close");
                    unloadOperation = await closing.AttachExternalCancellation(stop.Token);
                    CaptureShutdown("cycle1");
                    await Wait(() => loadOperation.isDone && unloadOperation.isDone);
                    await drain.AttachExternalCancellation(stop.Token);
                    await Wait(() => loading.Status != UniTaskStatus.Pending);
                    bool cancelled = false;
                    try { loading.GetAwaiter().GetResult(); }
                    catch (OperationCanceledException) { cancelled = true; }
                    Require(cancelled && !Field<bool>(loader, "prewarmRunning"),
                        "First full Loading task did not cancel and execute its finally.");
                    await Wait(() => { CheckLogs(); return acceptedError == 1; });
                    await UniTask.Yield(PlayerLoopTiming.Update, stop.Token);
                    startCatchAndFollowingFrameObserved = true;
                    SaveNew(evidenceRoot + "/cycle1-loading-witness.json", new
                    {
                        coreStatus = loading.Status.ToString(), generation,
                        asyncVoidStartTaskHandle = false, startCatchAndFollowingFrameObserved,
                        meaning = "Canceled Core and owned Start catch plus a later frame observed; not a Start UniTask join."
                    });
                    Require(app.State == AppFlowState.MenuMain, "Ordinary close did not return to main menu.");
                    CheckImageDrained();
                    RestoreHook();

                    Begin("cycle2_explicit_core_pool_key_text");
                    Require(ReferenceEquals(SceneObject<LoadingPrewarmController>(), loader) &&
                        ReferenceEquals(SceneObject<AppManager>(), app), "Menu owner changed between cycles.");
                    hasClosing = false;
                    unloadOperation = null;
                    menu.ShowLoading();
                    loading = loader.PrewarmOnceAsync().Preserve();
                    Require(Field<int>(loader, "prewarmGeneration") > generation, "Second Loading generation did not start.");
                    await loading.AttachExternalCancellation(stop.Token);
                    await Wait(() => loader.IsPrewarmed &&
                        Field<Queue<string>>(loader, "pendingTexts").Count == 0);
                    Require(loader.LoadingResourceTxt != null && !Field<bool>(loader, "prewarmRunning"),
                        "Second full Loading/UI task did not finish.");
                    CheckImageDrained();
                    CaptureBorrowedMaterials();

                    Begin("cycle2_real_battle");
                    app.SetMatchConfig(new MatchConfig { seed = 2833 });
                    loadOperation = app.LoadBattleAdditive();
                    Require(loadOperation != null, "Second Battle load did not start.");
                    await Wait(() => loadOperation.isDone && app.State == AppFlowState.BattleRunning);
                    driver = SceneObject<SimulationTickDriver>();
                    shutdownWitness = false;
                    Require(driver.World != null && !ReferenceEquals(driver.World, firstWorld) &&
                        driver.LifecycleState == BattleRuntimeLifecycleState.Running && driver.World.ObjectCount == 0,
                        "Second Battle must be a new, actually running, empty configured World.");
                    SaveNew(evidenceRoot + "/cycle2-running.json", new
                    {
                        lifecycle = driver.LifecycleState.ToString(), driver.World.ObjectCount,
                        newWorld = !ReferenceEquals(driver.World, firstWorld),
                        appState = app.State.ToString(), emptyPlayers = app.CurrentMatchConfig.players.Count
                    });
                    Begin("cycle2_return_to_selection");
                    Require(app.TryReturnToCharacterSelectionFromBattleResult(), "Public selection close was rejected.");
                    closing = app.UnloadBattleAfterDrainAsync(true).Preserve();
                    hasClosing = true;
                    unloadOperation = await closing.AttachExternalCancellation(stop.Token);
                    CaptureShutdown("cycle2");
                    await Wait(() => unloadOperation.isDone);
                    Require(app.State == AppFlowState.MenuSelectCharacter,
                        "Selection state changed before/without actual unload completion.");
                    CheckImageDrained();
                    CheckLogs();
                    VerifyFiles(ReadJson(evidenceRoot + "/prepare.json"));
                    State = "READY_TO_EXIT";
                    pumping = false;
                    EditorApplication.update -= Tick;
                    DetachSceneHandler();
                    SaveNew(evidenceRoot + "/ready-to-exit.json", new
                    {
                        State, FirstFailure, cycles = 2, errors = Volatile.Read(ref errors), acceptedError,
                        nativePending = manager.HasPendingNativePrewarmWork,
                        loadingStatus = loading.Status.ToString(), closeStatus = closing.Status.ToString(),
                        warmupTexture = textureId, warmupSprite = spriteId,
                        resourceScope = "Observed identities only; unrelated/native/cache residency not certified."
                    });
                }
                catch (Exception ex)
                {
                    Hold(ex);
                }
            }

            private void RestoreHook()
            {
                if (manager != null && manager.NativeDecodeStartedForDiagnostics == OnDecode)
                    manager.NativeDecodeStartedForDiagnostics = previousDecodeHook;
            }

            private void CaptureWarmup()
            {
                Sprite[] matches = Resources.FindObjectsOfTypeAll<Sprite>().Where(x =>
                    !beforeSprites.Contains(x.GetInstanceID()) && x.texture != null &&
                    !beforeTextures.Contains(x.texture.GetInstanceID()) &&
                    x.texture.width == 2 && x.texture.height == 2 && x.texture.format == TextureFormat.RGBA32 &&
                    x.rect.width == 2 && x.rect.height == 2).ToArray();
                Require(matches.Length == 1, "Warmup Sprite/Texture identity is ambiguous or not observed.");
                spriteId = matches[0].GetInstanceID();
                textureId = matches[0].texture.GetInstanceID();
                SaveNew(evidenceRoot + "/warmup-observed.json", new
                {
                    spriteId, textureId, spriteName = matches[0].name, textureName = matches[0].texture.name,
                    format = matches[0].texture.format.ToString(), width = 2, height = 2,
                    ownership = "Observed candidate pair; diagnostic does not destroy production Warmup resources."
                });
            }

            private void CheckImageDrained()
            {
                Require(manager != null && !manager.HasPendingNativePrewarmWork, "Native work is still pending.");
                var metrics = manager.LastNativeDecodeAdmission;
                Require(metrics != null && metrics.Reserved == 0 && metrics.Active == 0 &&
                    metrics.Waiting == 0 && metrics.PendingPixels == 0, "Image admission still owns payload/work.");
            }

            private void CaptureShutdown(string cycle)
            {
                Require(unloadOperation != null, "No actual Scene unload operation.");
                Require(driver != null && !unloadOperation.isDone,
                    "Missed live pre-destroy shutdown witness; managed leftovers are not sufficient.");
                Require(driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                    driver.ShutdownStageForDiagnostics == BattleRuntimeShutdownStage.RuntimeMapCleared &&
                    driver.World == null, "Driver shutdown did not finish all eleven phases.");
                BattleBootstrap bootstrap = SceneObject<BattleBootstrap>();
                Require(bootstrap.IsRuntimeMapCleared, "Runtime map still has a carrier.");
                LF2ObjectPool[] pools = Resources.FindObjectsOfTypeAll<LF2ObjectPool>()
                    .Where(x => x != null && x.gameObject.scene.IsValid() && x.gameObject.scene.isLoaded).ToArray();
                Require(pools.Length <= 1 && pools.All(x => x.IsQuiescedForDiagnostics), "Pool is not quiesced.");
                CheckImageDrained();
                shutdownWitness = true;
                SaveNew(evidenceRoot + "/" + cycle + "-shutdown.json", new
                {
                    lifecycle = driver.LifecycleState.ToString(), stage = driver.ShutdownStageForDiagnostics.ToString(),
                    worldUnbound = driver.World == null, bootstrap.IsRuntimeMapCleared,
                    pools = pools.Length, poolsQuiesced = pools.All(x => x.IsQuiescedForDiagnostics),
                    sceneStillLoaded = SceneManager.GetSceneByPath(BattleScene).isLoaded,
                    actualUnloadCompleted = unloadOperation.isDone
                });
            }

            private void CaptureBorrowedMaterials()
            {
                var materials = Resources.FindObjectsOfTypeAll<SpriteRenderer>()
                    .Where(x => x != null && x.gameObject.scene.IsValid() && x.gameObject.scene.isLoaded)
                    .Select(x => x.sharedMaterial).Where(x => x != null).Distinct().Select(x => new
                    {
                        id = x.GetInstanceID(), x.name, assetPath = AssetDatabase.GetAssetPath(x),
                        shader = x.shader == null ? null : x.shader.name
                    }).ToArray();
                SaveNew(evidenceRoot + "/borrowed-materials.json", new
                {
                    materials, ownership = "Borrowed; no material/shader destruction or mutation by diagnostic."
                });
            }

            private void OnDecode(string path)
            {
                Interlocked.Increment(ref decodeCallbacks);
                try
                {
                    if (!string.Equals(Path.GetFullPath(path), Path.GetFullPath(evidenceRoot + "/runtime/vfs/c/body.png"),
                        StringComparison.OrdinalIgnoreCase)) return;
                    if (Interlocked.Increment(ref bodyStarts) != 1) return;
                    decoded.Set();
                    if (!release.Wait(TimeSpan.FromSeconds(5))) Interlocked.Exchange(ref barrierTimeout, 1);
                }
                finally { Interlocked.Decrement(ref decodeCallbacks); }
            }

            private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.path != BattleScene || Volatile.Read(ref closeAccepted) != 0) return;
                DetachSceneHandler();
                try
                {
                    driver = SceneObject<SimulationTickDriver>();
                    shutdownWitness = false;
                    Require(driver.gameObject.scene == scene && driver.World != null &&
                        driver.LifecycleState == BattleRuntimeLifecycleState.Preparing, "Missing real preparing Driver/World.");
                    firstWorld = driver.World;
                    Interlocked.Exchange(ref closeAccepted, 1);
                    Require(app.UnloadBattle() == null && app.UnloadBattle() == null,
                        "Pending ordinary close did not return null twice.");
                    closing = app.UnloadBattleAfterDrainAsync(false).Preserve();
                    hasClosing = true;
                    Require(scene.isLoaded && driver.LifecycleState == BattleRuntimeLifecycleState.Stopping &&
                        ReferenceEquals(driver.World, firstWorld) && manager.HasPendingNativePrewarmWork,
                        "World/Scene crossed image hard gate early.");
                }
                catch (Exception ex) { Hold(ex); }
            }

            private void OnLog(string condition, string stack, LogType type)
            {
                if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                Interlocked.Increment(ref logWriters);
                try
                {
                    lock (logGate)
                    {
                        var item = new ErrorReceipt
                        {
                            condition = condition, stack = stack, type = type.ToString(),
                            utc = DateTime.UtcNow.ToString("O"), closeAccepted = Volatile.Read(ref closeAccepted),
                            thread = Thread.CurrentThread.ManagedThreadId
                        };
                        int count = Interlocked.Increment(ref errors);
                        if (logsSealed)
                        {
                            if (Interlocked.Increment(ref lateLogs) == 1)
                                SaveNew(evidenceRoot + "/log-after-seal.json", item);
                            return;
                        }
                        if (count > 2) return;
                        SaveNew(evidenceRoot + (count == 1 ? "/log-first.json" : "/log-extra.json"), item);
                        Interlocked.Increment(ref persistedLogs);
                        if (count == 1) Volatile.Write(ref firstError, item);
                    }
                }
                catch { Interlocked.Exchange(ref logIoFailure, 1); }
                finally { Interlocked.Decrement(ref logWriters); }
            }

            private void CheckLogs()
            {
                if (Volatile.Read(ref logIoFailure) != 0) throw new IOException("Diagnostic error evidence could not be saved.");
                if (Volatile.Read(ref errors) > 1 || Volatile.Read(ref lateLogs) != 0)
                    throw new InvalidOperationException("Unexpected, repeated or late error log.");
                if (Volatile.Read(ref logWriters) != 0) return;
                ErrorReceipt error = Volatile.Read(ref firstError);
                if (error == null || acceptedError != 0) return;
                Require(Volatile.Read(ref persistedLogs) == 1, "Cancellation raw record is not durable.");
                Require(error.type == LogType.Error.ToString() && error.closeAccepted == 1 &&
                    hasLoading && loading.Status != UniTaskStatus.Pending && loader != null &&
                    Field<int>(loader, "prewarmGeneration") == generation &&
                    ReferenceEquals(GameConfig.Instance, clone), "Error is not owned completed Loading cancellation.");
                Require(ReferenceEquals(SceneObject<LoadingPrewarmController>(), loader) &&
                    ReferenceEquals(SceneObject<CharacterAnimtorManager>(), manager), "Error owner changed.");
                if (!attributionExamined)
                {
                    attributionExamined = true;
                    OperationCanceledException cancelled = null;
                    try { loading.GetAwaiter().GetResult(); }
                    catch (OperationCanceledException ex) { cancelled = ex; }
                    string expected = cancelled == null ? null :
                        "[LoadingPrewarmController] Start failed with exception: " +
                        cancelled.Message + "\n" + cancelled.StackTrace;
                    attributionMatched = cancelled != null && !string.IsNullOrEmpty(cancelled.StackTrace) &&
                        string.Equals(error.condition, expected, StringComparison.Ordinal) &&
                        error.stack.Contains("LoadingPrewarmController") &&
                        (error.stack.Contains("Start") || error.stack.Contains("<Start>"));
                    SaveNew(evidenceRoot + "/cancellation-attribution.json", new
                    {
                        generation, coreStatus = loading.Status.ToString(),
                        controllerId = loader.GetInstanceID(), managerId = manager.GetInstanceID(),
                        observedExceptionType = cancelled?.GetType().AssemblyQualifiedName,
                        actualCondition = error.condition, expectedCondition = expected,
                        actualCallerStack = error.stack,
                        comparison = "Ordinal complete condition including observed Core exception stack; no normalization",
                        attribution = attributionMatched ? "MATCH" : "UNKNOWN_FIRST_DIFFERENCE",
                        asyncVoidStartTaskHandle = false
                    });
                }
                Require(attributionMatched,
                    "UNKNOWN cancellation attribution: full condition differs; no consumer-stack normalization is authorized.");
                acceptedError = 1;
            }

            private void RequireLogsDurable()
            {
                bool entered = Monitor.TryEnter(logGate);
                Require(entered, "Log writer still owns persistence; do not wait or exit.");
                try
                {
                    Require(!logsSealed && Volatile.Read(ref logWriters) == 0 &&
                        Volatile.Read(ref errors) == Volatile.Read(ref persistedLogs) &&
                        Volatile.Read(ref logIoFailure) == 0 && Volatile.Read(ref lateLogs) == 0,
                        "Observed log persistence is incomplete or uncertain.");
                }
                finally { Monitor.Exit(logGate); }
            }

            private void Tick()
            {
                if (!pumping) return;
                try
                {
                    if (!cleanup) CheckLogs();
                    if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
                        throw new InvalidOperationException("Unexpected Editor lifecycle interruption.");
                    if (Volatile.Read(ref barrierTimeout) != 0)
                        throw new TimeoutException("Owned decode barrier exceeded five seconds.");
                    if (!cleanup && (DateTime.UtcNow - enteredUtc).TotalSeconds >= 240)
                        throw new TimeoutException("Single Play budget exhausted.");
                    if (DateTime.UtcNow >= phaseDeadline)
                        throw new TimeoutException("Phase exceeded deadline: " + phase);
                }
                catch (Exception ex) { Hold(ex); }
            }

            private void DetachSceneHandler()
            {
                if (!sceneHandler) return;
                SceneManager.sceneLoaded -= OnSceneLoaded;
                sceneHandler = false;
            }

            private void RememberFailure(Exception exception)
            {
                if (FirstFailure == null)
                {
                    FirstFailure = exception.ToString();
                    try { SaveNew(evidenceRoot + "/first-failure.json", new { phase, error = FirstFailure }); }
                    catch { Interlocked.Exchange(ref logIoFailure, 1); }
                }
            }

            private void Hold(Exception exception)
            {
                RememberFailure(exception);
                State = "HOLD";
                SignalRelease();
                DetachSceneHandler();
                RestoreHook();
                pumping = false;
                EditorApplication.update -= Tick;
                if (!handlesDisposed && !stop.IsCancellationRequested) stop.Cancel();
            }

            public string StartCleanup()
            {
                if (cleanupAccepted)
                    return "CLEANUP_ALREADY_ACCEPTED:" + State + ":" + pipeline.Status;
                Require(!handlesDisposed && State == "HOLD" && pipeline.Status != UniTaskStatus.Pending,
                    "Driver task must finish before the one cleanup transaction.");
                cleanupAccepted = true;
                SaveNew(evidenceRoot + "/cleanup-accepted.json", new
                {
                    runGuid = RunGuid, transaction = 1, State, FirstFailure,
                    utc = DateTime.UtcNow.ToString("O")
                });
                stop.Dispose();
                stop = new CancellationTokenSource();
                cleanup = true;
                SignalRelease();
                State = "FAILED_CLEANUP_PENDING";
                pumping = true;
                EditorApplication.update += Tick;
                pipeline = CleanupAsync().Preserve();
                return "CLEANUP_ONLY_ACCEPTED";
            }

            private async UniTask CleanupAsync()
            {
                try
                {
                    Begin("failed_cleanup_only");
                    if (hasLoading)
                    {
                        await Wait(() => loading.Status != UniTaskStatus.Pending);
                        try { loading.GetAwaiter().GetResult(); }
                        catch (Exception) { }
                    }
                    if (manager != null)
                    {
                        drain = manager.WaitForNativePrewarmDrainAsync().Preserve();
                        hasDrain = true;
                        await drain.AttachExternalCancellation(stop.Token);
                    }
                    if (loadOperation != null) await Wait(() => loadOperation.isDone);
                    if (SceneManager.GetSceneByPath(BattleScene).isLoaded)
                    {
                        Require(app != null, "No original App for cleanup.");
                        if (!hasClosing)
                        {
                            closing = app.UnloadBattleAfterDrainAsync(false).Preserve();
                            hasClosing = true;
                        }
                        unloadOperation = await closing.AttachExternalCancellation(stop.Token);
                        Require(unloadOperation != null, "Cleanup has no real unload operation.");
                        if (!shutdownWitness) CaptureShutdown("failed-cleanup");
                        await Wait(() => unloadOperation.isDone);
                    }
                    if (manager != null) CheckImageDrained();
                    State = "FAILED_CLEAN_SAFE";
                    pumping = false;
                    EditorApplication.update -= Tick;
                }
                catch (Exception ex) { Hold(ex); }
            }

            public void CheckSafeExit()
            {
                Require(State == "READY_TO_EXIT" || State == "FAILED_CLEAN_SAFE", "Owner is not safe to exit.");
                Require(pipeline.Status != UniTaskStatus.Pending && (!hasLoading || loading.Status != UniTaskStatus.Pending) &&
                    (!hasDrain || drain.Status != UniTaskStatus.Pending) && (!hasClosing || closing.Status != UniTaskStatus.Pending),
                    "Actual task still pending.");
                Require(!externalExitingObserved && !externalEnteredEditObserved,
                    "External exit boundary is not a qualified live release path.");
                if (State == "READY_TO_EXIT")
                {
                    try { CheckLogs(); }
                    catch (Exception exception)
                    {
                        RememberFailure(exception);
                        State = "FAILED_CLEAN_SAFE";
                    }
                }
                Require(State != "FAILED_CLEAN_SAFE" || FirstFailure != null, "Failed verdict must be durable.");
                RequireLogsDurable();
                if (manager != null) CheckImageDrained();
                Require(ReferenceEquals(driver, null) || shutdownWitness, "No live shutdown witness captured.");
                Require(!SceneManager.GetSceneByPath(BattleScene).isLoaded &&
                    (loadOperation == null || loadOperation.isDone) && (unloadOperation == null || unloadOperation.isDone),
                    "Scene transaction is incomplete.");
                Require(ReferenceEquals(driver, null) || (driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                    driver.World == null), "Driver hard shutdown conditions have not completed.");
                Require(loader == null || !Field<bool>(loader, "prewarmRunning"), "Loading Core still running.");
                Require(Volatile.Read(ref decodeCallbacks) == 0, "Owned decode callback still uses a barrier.");
                Require(State != "READY_TO_EXIT" || startCatchAndFollowingFrameObserved,
                    "The first async-void Start catch and later frame were not observed.");
            }

            private void SignalRelease()
            {
                if (!handlesDisposed) release.Set();
            }

            public void ReleaseOwnedResourcesForExit()
            {
                if (OwnedResourcesReleased) return;
                CheckSafeExit();
                pumping = false;
                EditorApplication.update -= Tick;
                DetachSceneHandler();
                RestoreHook();
                Require(Volatile.Read(ref decodeCallbacks) == 0 &&
                    (manager == null || manager.NativeDecodeStartedForDiagnostics != OnDecode),
                    "Callbacks still reference owned synchronization handles.");
                if (cloneCreated)
                {
                    var original = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
                    Require(!ReferenceEquals(clone, null) && clone != null && cloneId != 0 &&
                        clone.GetInstanceID() == cloneId && clone.GetType().AssemblyQualifiedName == cloneType &&
                        clone.name == "M12-Owned-GameConfig-" + RunGuid &&
                        !ReferenceEquals(clone, original) && !AssetDatabase.Contains(clone) &&
                        string.IsNullOrEmpty(AssetDatabase.GetAssetPath(clone)) && previousConfigCaptured &&
                        ReferenceEquals(GameConfig.Instance, cloneInstalled ? clone : previousConfig),
                        "Clone ownership cannot be proved from this live owner; do not destroy.");
                    UnityEngine.Object.DestroyImmediate(clone, false);
                    Require(clone == null && EditorUtility.InstanceIDToObject(cloneId) == null,
                        "Owned clone release/absence was not observed.");
                    if (cloneInstalled) GameConfig.Instance = previousConfig;
                    Require(ReferenceEquals(GameConfig.Instance, previousConfig), "Borrowed singleton changed.");
                }
                else
                {
                    Require(ReferenceEquals(clone, null) && cloneId == 0 && !cloneInstalled,
                        "Clone creation ownership is uncertain.");
                }
                cloneReleased = true;
                SignalRelease();
                release.Dispose();
                decoded.Dispose();
                stop.Dispose();
                handlesDisposed = true;
                SaveNew(evidenceRoot + "/owned-resources-released.json", new
                {
                    runGuid = RunGuid, cloneId, cloneType, owner = "M12RealSceneCloseDiagnostics.RunOwner",
                    cloneCreated, cloneInstalled, menuLoadRequested,
                    cloneAbsentAfterRelease = !cloneCreated || (clone == null && EditorUtility.InstanceIDToObject(cloneId) == null),
                    previousSingletonRestored = !previousConfigCaptured || ReferenceEquals(GameConfig.Instance, previousConfig),
                    handlesDisposed, callbacks = Volatile.Read(ref decodeCallbacks),
                    actualTasksTerminal = true, noAssetDestruction = true,
                    originalConfigSha256 = HashFile(Project + "/" + ConfigPath)
                });
                RequireLogsDurable();
            }

            public void CaptureWarmupExit()
            {
                bool notReached = !menuLoadRequested && loadOperation == null && !hasLoading &&
                    app == null && loader == null && manager == null && ReferenceEquals(driver, null) &&
                    textureId == 0 && spriteId == 0;
                Require(notReached || (textureId != 0 && spriteId != 0), "Warmup identity was never observed after Menu request.");
                SaveNew(evidenceRoot + "/warmup-before-exit.json", new
                {
                    status = notReached ? "NOT_REACHED" : "OBSERVED", textureId, spriteId,
                    menuLoadRequested, contentRequested = hasLoading
                });
            }

            public void PrepareForExpectedExit()
            {
                expectedExit = true;
                pumping = false;
                EditorApplication.update -= Tick;
                DetachSceneHandler();
                SignalRelease();
                if (manager != null && manager.NativeDecodeStartedForDiagnostics == OnDecode)
                    manager.NativeDecodeStartedForDiagnostics = previousDecodeHook;
            }


            private void OnPlayModeChanged(PlayModeStateChange transition)
            {
                if (expectedExit || (transition != PlayModeStateChange.ExitingPlayMode &&
                    transition != PlayModeStateChange.EnteredEditMode)) return;
                bool enteringEdit = transition == PlayModeStateChange.EnteredEditMode;
                if (enteringEdit ? externalEnteredEditObserved : externalExitingObserved) return;
                if (enteringEdit) externalEnteredEditObserved = true;
                else externalExitingObserved = true;
                RememberFailure(new InvalidOperationException("External Play exit; owned Dispose remains unproved."));
                State = "EXTERNAL_INTERRUPTED";
                pumping = false;
                EditorApplication.update -= Tick;
                bool gateEntered = Monitor.TryEnter(logGate);
                try
                {
                    SaveNew(evidenceRoot + (enteringEdit ? "/external-entered-edit.json" : "/external-exiting-play.json"), new
                    {
                        runGuid = RunGuid, transition = transition.ToString(), State, FirstFailure,
                        ownedResourcesReleased = OwnedResourcesReleased,
                        dispose = OwnedResourcesReleased ? "PREVIOUSLY_OBSERVED" : "UNKNOWN",
                        logPersistenceComplete = gateEntered && !logsSealed && Volatile.Read(ref logWriters) == 0 &&
                            Volatile.Read(ref errors) == Volatile.Read(ref persistedLogs) &&
                            Volatile.Read(ref lateLogs) == 0 && Volatile.Read(ref logIoFailure) == 0,
                        status = Status()
                    });
                }
                catch { Interlocked.Exchange(ref logIoFailure, 1); }
                finally { if (gateEntered) Monitor.Exit(logGate); }
                if (enteringEdit) EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            }

            private void CreateOwnedClone(GameConfig original)
            {
                Require(!cloneCreated && ReferenceEquals(clone, null), "Clone creation already attempted.");
                previousConfig = GameConfig.Instance;
                previousConfigCaptured = true;
                clone = UnityEngine.Object.Instantiate(original);
                cloneCreated = true;
                clone.name = "M12-Owned-GameConfig-" + RunGuid;
                cloneId = clone.GetInstanceID();
                cloneType = clone.GetType().AssemblyQualifiedName;
                Require(cloneId != original.GetInstanceID() && !AssetDatabase.Contains(clone) &&
                    string.IsNullOrEmpty(AssetDatabase.GetAssetPath(clone)), "Clone must be this owner's nonasset instance.");
            }

            private void OnReload()
            {
                bool gateEntered = Monitor.TryEnter(logGate);
                bool durable = false;
                try
                {
                    if (gateEntered)
                    {
                        logsSealed = true;
                        durable = Volatile.Read(ref logWriters) == 0 &&
                            Volatile.Read(ref persistedLogs) == Volatile.Read(ref errors) &&
                            Volatile.Read(ref lateLogs) == 0 && Volatile.Read(ref logIoFailure) == 0;
                    }
                    // A busy writer is an UNKNOWN boundary, never a synchronous join or a successful seal.
                    Application.logMessageReceivedThreaded -= OnLog;
                    SaveNew(evidenceRoot + (expectedExit ? "/expected-exit-reload.json" : "/external-interruption.json"),
                        new
                        {
                            State, FirstFailure, errors = Volatile.Read(ref errors), acceptedError,
                            logIoFailure = Volatile.Read(ref logIoFailure), phase,
                            persistedLogs = Volatile.Read(ref persistedLogs), logWriters = Volatile.Read(ref logWriters),
                            lateLogs = Volatile.Read(ref lateLogs), logPersistenceComplete = gateEntered && durable,
                            ownedResourcesReleased = OwnedResourcesReleased,
                            observationBoundary = "Before expected domain reload; post-Exit Console still requires external review."
                        });
                }
                catch { Interlocked.Exchange(ref logIoFailure, 1); }
                finally
                {
                    if (gateEntered) Monitor.Exit(logGate);
                    Application.logMessageReceivedThreaded -= OnLog;
                    AssemblyReloadEvents.beforeAssemblyReload -= OnReload;
                    EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                    EditorApplication.update -= Tick;
                    DetachSceneHandler();
                    SignalRelease();
                }
                if (!expectedExit)
                    Hold(new InvalidOperationException("Unexpected assembly reload; hot owner and release ownership remain UNKNOWN."));
            }

            public string Status()
            {
                return JsonConvert.SerializeObject(new
                {
                    runGuid = RunGuid, state = State, phase, firstFailure = FirstFailure,
                    playing = EditorApplication.isPlaying, nativePending = manager != null && manager.HasPendingNativePrewarmWork,
                    loadingPending = hasLoading && loading.Status == UniTaskStatus.Pending,
                    closePending = hasClosing && closing.Status == UniTaskStatus.Pending,
                    errors = Volatile.Read(ref errors), acceptedError, textureId, spriteId,
                    cleanupAccepted, handlesDisposed, cloneReleased, cloneId, cloneType,
                    logWriters = Volatile.Read(ref logWriters), persistedLogs = Volatile.Read(ref persistedLogs),
                    lateLogs = Volatile.Read(ref lateLogs), attributionExamined, attributionMatched,
                    logIoFailure = Volatile.Read(ref logIoFailure), decodeCallbacks = Volatile.Read(ref decodeCallbacks),
                    pipeline = pipeline.Status.ToString(), drainPending = hasDrain && drain.Status == UniTaskStatus.Pending,
                    sceneLoadPending = loadOperation != null && !loadOperation.isDone,
                    sceneUnloadPending = unloadOperation != null && !unloadOperation.isDone,
                    cloneCreated, cloneInstalled, menuLoadRequested, externalExitingObserved, externalEnteredEditObserved,
                    cloneLive = !ReferenceEquals(clone, null) && clone != null,
                    liveCloneId = clone == null ? 0 : clone.GetInstanceID(),
                    liveCloneName = clone == null ? null : clone.name,
                    liveCloneType = clone == null ? null : clone.GetType().AssemblyQualifiedName,
                    cloneIsAsset = clone != null && AssetDatabase.Contains(clone),
                    singletonIsClone = !ReferenceEquals(clone, null) && ReferenceEquals(GameConfig.Instance, clone),
                    singletonIsPrevious = previousConfigCaptured && ReferenceEquals(GameConfig.Instance, previousConfig)
                });
            }
        }
    }

    internal static class M12CloneComparison
    {
        internal static JObject FirstDifference(JToken original, JToken clone, string path = "$")
        {
            if (JToken.DeepEquals(original, clone)) return null;
            if (original == null || clone == null || original.Type != clone.Type)
                return Difference(path, original, clone);
            if (original is JObject a && clone is JObject b)
            {
                foreach (string key in a.Properties().Select(x => x.Name).Concat(b.Properties().Select(x => x.Name))
                    .Distinct().OrderBy(x => x, StringComparer.Ordinal))
                {
                    JObject difference = FirstDifference(a[key], b[key], path + "." + key);
                    if (difference != null) return difference;
                }
            }
            if (original is JArray aa && clone is JArray bb)
            {
                if (aa.Count != bb.Count) return Difference(path + ".Length", new JValue(aa.Count), new JValue(bb.Count));
                for (int i = 0; i < aa.Count; i++)
                {
                    JObject difference = FirstDifference(aa[i], bb[i], path + "[" + i + "]");
                    if (difference != null) return difference;
                }
            }
            return Difference(path, original, clone);
        }

        private static JObject Difference(string path, JToken original, JToken clone)
        {
            return new JObject
            {
                ["path"] = path, ["originalType"] = original?.Type.ToString() ?? "MISSING",
                ["cloneType"] = clone?.Type.ToString() ?? "MISSING",
                ["original"] = original?.DeepClone(), ["clone"] = clone?.DeepClone()
            };
        }

        internal static JObject Compare(JObject original, JObject clone, string expectedRoot)
        {
            string root = nameof(GameConfig.BattleContentRuntimeRoot);
            if (original.Count != 1 || clone.Count != 1 ||
                !(original["MonoBehaviour"] is JObject originalBody) ||
                !(clone["MonoBehaviour"] is JObject cloneBody))
                return Difference("$.MonoBehaviour", original, clone);
            if (originalBody[root]?.Type != JTokenType.String || cloneBody[root]?.Type != JTokenType.String)
                return Difference("$.MonoBehaviour." + root, originalBody[root], cloneBody[root]);
            if ((string)cloneBody[root] != expectedRoot)
                return Difference("$.MonoBehaviour." + root, new JValue(expectedRoot), cloneBody[root]);
            if (originalBody["m_Name"]?.Type != JTokenType.String || cloneBody["m_Name"]?.Type != JTokenType.String)
                return Difference("$.MonoBehaviour.m_Name", originalBody["m_Name"], cloneBody["m_Name"]);
            var a = (JObject)original.DeepClone();
            var b = (JObject)clone.DeepClone();
            ((JObject)a["MonoBehaviour"]).Remove("m_Name");
            ((JObject)b["MonoBehaviour"]).Remove("m_Name");
            a["MonoBehaviour"][root] = expectedRoot;
            return FirstDifference(a, b);
        }

        internal static void SaveNew(string path, object value)
        {
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            if (System.Text.Encoding.UTF8.GetByteCount(json) > 1024 * 1024)
                throw new InvalidOperationException("Clone evidence exceeds one MiB; no truncated PASS.");
            using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(file, new System.Text.UTF8Encoding(false)))
                writer.Write(json);
        }

        internal static object[] References(GameConfig config)
        {
            var rows = new List<object>();
            using (var serialized = new SerializedObject(config))
            {
                SerializedProperty property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                    UnityEngine.Object value = property.objectReferenceValue;
                    string guid = null;
                    long localId = 0;
                    bool persistent = value != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out guid, out localId);
                    rows.Add(new
                    {
                        path = property.propertyPath, instanceId = property.objectReferenceInstanceIDValue,
                        type = value == null ? null : value.GetType().AssemblyQualifiedName,
                        name = value == null ? null : value.name, persistent, guid, localId
                    });
                }
            }
            return rows.ToArray();
        }
    }

    public sealed class M12DiagnosticRepairTests
    {
        internal const string Evidence = M12RealSceneCloseDiagnostics.InterfaceRoot + "/post-external-exit-repair-01";
        private const string Config = "Assets/NTSD/Config/GameConfig/GameConfig.asset";

        [Test]
        public void R01_ColdCloneFirstDifference()
        {
            Assert.That(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating, Is.False);
            Assert.That((bool)JObject.Parse(M12RealSceneCloseDiagnostics.ReadStatus())["hotOwner"], Is.False);
            var previous = GameConfig.Instance;
            var original = AssetDatabase.LoadAssetAtPath<GameConfig>(Config);
            Assert.That(original, Is.Not.Null);
            GameConfig clone = null;
            int cloneId = 0;
            JObject difference = null;
            try
            {
                clone = UnityEngine.Object.Instantiate(original);
                clone.name = "M12-Repair-Cold-Clone";
                cloneId = clone.GetInstanceID();
                Assert.That(AssetDatabase.Contains(clone), Is.False);
                Assert.That(cloneId, Is.Not.EqualTo(original.GetInstanceID()));
                clone.BattleContentRuntimeRoot = M12RealSceneCloseDiagnostics.RunRoot + "/runtime";
                var a = JObject.Parse(EditorJsonUtility.ToJson(original));
                var b = JObject.Parse(EditorJsonUtility.ToJson(clone));
                difference = M12CloneComparison.Compare(a, b, clone.BattleContentRuntimeRoot);
                M12CloneComparison.SaveNew(Evidence + "/cold-clone-comparison.json", new
                {
                    original = a, clone = b, firstDifference = difference,
                    originalId = original.GetInstanceID(), cloneId,
                    originalReferences = M12CloneComparison.References(original),
                    cloneReferences = M12CloneComparison.References(clone),
                    singletonUnchanged = ReferenceEquals(GameConfig.Instance, previous),
                    installed = false, menuOrContentRequested = false
                });
            }
            finally
            {
                if (clone != null && !ReferenceEquals(clone, original) && !AssetDatabase.Contains(clone))
                    UnityEngine.Object.DestroyImmediate(clone, false);
                bool absent = !ReferenceEquals(clone, null) && clone == null &&
                    cloneId != 0 && EditorUtility.InstanceIDToObject(cloneId) == null;
                M12CloneComparison.SaveNew(Evidence + "/cold-clone-release.json", new
                {
                    cloneId, absent, singletonUnchanged = ReferenceEquals(GameConfig.Instance, previous),
                    originalStillAsset = original != null && AssetDatabase.Contains(original)
                });
                Assert.That(absent, Is.True);
                Assert.That(ReferenceEquals(GameConfig.Instance, previous), Is.True);
            }
            Assert.That(difference, Is.Null, difference?.ToString());
        }

        private static readonly Type OwnerType = typeof(M12RealSceneCloseDiagnostics).GetNestedType("RunOwner", BindingFlags.NonPublic);
        private const BindingFlags OwnerMembers = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static object Get(object owner, string name) => OwnerType.GetField(name, OwnerMembers).GetValue(owner);
        private static void Set(object owner, string name, object value) => OwnerType.GetField(name, OwnerMembers).SetValue(owner, value);
        private static object Call(object owner, string name, params object[] args)
        {
            try { return OwnerType.GetMethod(name, OwnerMembers).Invoke(owner, args); }
            catch (TargetInvocationException exception) { throw exception.InnerException; }
        }

        private static void State(object owner, string state) => OwnerType.GetProperty("State", OwnerMembers).SetValue(owner, state);
        private static object NewOwner(string test, out string output)
        {
            Assert.That(EditorApplication.isPlayingOrWillChangePlaymode, Is.False);
            output = Evidence + "/" + test;
            Assert.That(Directory.Exists(output), Is.False, "Do not rerun into existing evidence.");
            Directory.CreateDirectory(output);
            var owner = Activator.CreateInstance(OwnerType, new object[] { DateTime.UtcNow, output });
            Call(owner, "CreateOwnedClone", AssetDatabase.LoadAssetAtPath<GameConfig>(Config));
            Call(owner, "RememberFailure", new InvalidOperationException("Expected diagnostic negative-control failure."));
            State(owner, "FAILED_CLEAN_SAFE");
            return owner;
        }

        // These owners never Start: no subscribed callback, Scene, content, task or worker is created.
        private static void DisposeTestFixture(object owner, string output)
        {
            var clone = (GameConfig)Get(owner, "clone");
            var previous = (GameConfig)Get(owner, "previousConfig");
            if (clone != null)
            {
                Assert.That(AssetDatabase.Contains(clone), Is.False);
                Assert.That(clone.GetInstanceID(), Is.EqualTo((int)Get(owner, "cloneId")));
                Assert.That(clone.name, Is.EqualTo("M12-Owned-GameConfig-" + M12RealSceneCloseDiagnostics.RunGuid));
                Assert.That(ReferenceEquals(GameConfig.Instance, clone) || ReferenceEquals(GameConfig.Instance, previous), Is.True);
                UnityEngine.Object.DestroyImmediate(clone, false);
                if (ReferenceEquals(GameConfig.Instance, clone)) GameConfig.Instance = previous;
            }
            if (!(bool)Get(owner, "handlesDisposed"))
            {
                ((ManualResetEventSlim)Get(owner, "release")).Dispose();
                ((ManualResetEventSlim)Get(owner, "decoded")).Dispose();
                ((CancellationTokenSource)Get(owner, "stop")).Dispose();
            }
            M12CloneComparison.SaveNew(output + "/fixture-finally.json", new
            {
                cloneAbsent = clone == null, singletonUnchanged = ReferenceEquals(GameConfig.Instance, previous),
                scope = "Unstarted cold test fixture only; never old run Dispose evidence"
            });
        }

        [Test]
        public void R02_RecordedCloneAndStrictNegativeControls()
        {
            var evidence = JObject.Parse(File.ReadAllText(Evidence + "/cold-clone-comparison.json"));
            var original = (JObject)evidence["original"];
            var clone = (JObject)evidence["clone"];
            string root = (string)clone["MonoBehaviour"][nameof(GameConfig.BattleContentRuntimeRoot)];
            Assert.That(M12CloneComparison.Compare(original, clone, root), Is.Null);
            Assert.That(JToken.DeepEquals(evidence["originalReferences"], evidence["cloneReferences"]), Is.True);
            var mutations = new Action<JObject>[]
            {
                x => x["MonoBehaviour"]["DesktopInitialRuntimeSlotCapacity"] = 1024,
                x => x["MonoBehaviour"]["BattleRuntimeProfileName"] = "DesktopExtended",
                x => x["MonoBehaviour"]["BattleAiExecutionProfileName"] = "Other",
                x => x["MonoBehaviour"]["LF2ObjectPrefab"]["guid"] = "different",
                x => ((JArray)x["MonoBehaviour"]["FootMarkerAnimationFrames"]).RemoveAt(0),
                x => ((JObject)x["MonoBehaviour"]).Remove(nameof(GameConfig.BattleContentRuntimeRoot)),
                x => x["MonoBehaviour"][nameof(GameConfig.BattleContentRuntimeRoot)] = 1,
                x => x["MonoBehaviour"][nameof(GameConfig.BattleContentRuntimeRoot)] = "wrong-root",
                x => x["MonoBehaviour"]["m_UnknownMetadata"] = 1,
                x => x["unexpectedEnvelopeField"] = 1,
                x => ((JObject)x["MonoBehaviour"]).Remove("m_Name"),
                x => x["MonoBehaviour"] = 1
            };
            var differences = new List<JObject>();
            foreach (var mutation in mutations)
            {
                var changed = (JObject)clone.DeepClone();
                mutation(changed);
                JObject difference = M12CloneComparison.Compare(original, changed, root);
                Assert.That(difference, Is.Not.Null);
                differences.Add(difference);
            }
            Assert.That((string)differences[3]["path"], Is.EqualTo("$.MonoBehaviour.LF2ObjectPrefab.guid"));
            string collision = Evidence + "/negative-controls.json";
            M12CloneComparison.SaveNew(collision, new { recordedPairMatches = true, differences });
            Assert.Throws<IOException>(() => M12CloneComparison.SaveNew(collision, new { overwrite = true }));
            Assert.Throws<InvalidOperationException>(() => M12CloneComparison.SaveNew(
                Evidence + "/overflow-must-not-exist.json", new { data = new string('x', 1024 * 1024) }));
            Assert.That(File.Exists(Evidence + "/overflow-must-not-exist.json"), Is.False);
        }

        [Test]
        public void R03_UninstalledCloneAndNeverReachedWarmupRelease()
        {
            object owner = NewOwner("u3", out string output);
            try
            {
                var previous = GameConfig.Instance;
                Call(owner, "CaptureWarmupExit");
                Assert.That((string)JObject.Parse(File.ReadAllText(output + "/warmup-before-exit.json"))["status"], Is.EqualTo("NOT_REACHED"));
                Call(owner, "ReleaseOwnedResourcesForExit");
                byte[] receipt = File.ReadAllBytes(output + "/owned-resources-released.json");
                Call(owner, "ReleaseOwnedResourcesForExit");
                CollectionAssert.AreEqual(receipt, File.ReadAllBytes(output + "/owned-resources-released.json"));
                Assert.That((bool)Get(owner, "handlesDisposed"), Is.True);
                Assert.That((bool)Get(owner, "cloneReleased"), Is.True);
                Assert.That(ReferenceEquals(GameConfig.Instance, previous), Is.True);
                Assert.Throws<ObjectDisposedException>(() => ((ManualResetEventSlim)Get(owner, "release")).Wait(0));
                Assert.Throws<ObjectDisposedException>(() => ((ManualResetEventSlim)Get(owner, "decoded")).Wait(0));
                Assert.Throws<ObjectDisposedException>(() => { var token = ((CancellationTokenSource)Get(owner, "stop")).Token; });
            }
            finally { DisposeTestFixture(owner, output); }
        }

        [Test]
        public void R04_InstalledCloneRestoresSingleton()
        {
            Assert.That(GameConfig.Instance == null, Is.True, "Never replace a live borrowed singleton.");
            object owner = NewOwner("installed", out string output);
            try
            {
                var clone = (GameConfig)Get(owner, "clone");
                GameConfig.Instance = clone;
                Assert.That(ReferenceEquals(GameConfig.Instance, clone), Is.True);
                Set(owner, "cloneInstalled", true);
                Call(owner, "ReleaseOwnedResourcesForExit");
                Assert.That(ReferenceEquals(GameConfig.Instance, Get(owner, "previousConfig")), Is.True);
                Assert.That(clone == null, Is.True);
            }
            finally { DisposeTestFixture(owner, output); }
        }

        [Test]
        public void R05_OwnershipPendingAndWarmupGuards()
        {
            object owner = NewOwner("guards", out string output);
            try
            {
                var clone = (GameConfig)Get(owner, "clone");
                var original = AssetDatabase.LoadAssetAtPath<GameConfig>(Config);
                try
                {
                    Set(owner, "clone", original);
                    Assert.Throws<InvalidOperationException>(() => Call(owner, "ReleaseOwnedResourcesForExit"));
                    Assert.That(AssetDatabase.Contains(original), Is.True);
                }
                finally { Set(owner, "clone", clone); }
                string name = clone.name;
                try
                {
                    clone.name = "Foreign-name";
                    Assert.Throws<InvalidOperationException>(() => Call(owner, "ReleaseOwnedResourcesForExit"));
                    Assert.That(clone != null, Is.True);
                }
                finally { clone.name = name; }
                var pending = new UniTaskCompletionSource();
                try
                {
                    Set(owner, "pipeline", pending.Task.Preserve());
                    Assert.Throws<InvalidOperationException>(() => Call(owner, "ReleaseOwnedResourcesForExit"));
                }
                finally { pending.TrySetResult(); }
                try
                {
                    Set(owner, "decodeCallbacks", 1);
                    Assert.Throws<InvalidOperationException>(() => Call(owner, "ReleaseOwnedResourcesForExit"));
                }
                finally { Set(owner, "decodeCallbacks", 0); }
                try
                {
                    Set(owner, "menuLoadRequested", true);
                    Assert.Throws<InvalidOperationException>(() => Call(owner, "CaptureWarmupExit"));
                    Assert.That(File.Exists(output + "/warmup-before-exit.json"), Is.False);
                }
                finally { Set(owner, "menuLoadRequested", false); }
                Call(owner, "ReleaseOwnedResourcesForExit");
            }
            finally { DisposeTestFixture(owner, output); }
        }

        [Test]
        public void R06_UnknownErrorStaysFailedWhileDurableSafeReleaseCompletes()
        {
            object owner = NewOwner("logs", out string output);
            try
            {
                foreach (string field in new[] { "logIoFailure", "logWriters", "lateLogs" })
                {
                    try
                    {
                        Set(owner, field, 1);
                        Assert.Throws<InvalidOperationException>(() => Call(owner, "ReleaseOwnedResourcesForExit"));
                        Assert.That((bool)Get(owner, "handlesDisposed"), Is.False);
                    }
                    finally { Set(owner, field, 0); }
                }
                Call(owner, "OnLog", "Synthetic unknown exception retained", "Exact synthetic stack", LogType.Exception);
                State(owner, "READY_TO_EXIT");
                Call(owner, "ReleaseOwnedResourcesForExit");
                Assert.That((string)OwnerType.GetProperty("State").GetValue(owner), Is.EqualTo("FAILED_CLEAN_SAFE"));
                Assert.That((int)Get(owner, "errors"), Is.EqualTo(1));
                Assert.That((int)Get(owner, "persistedLogs"), Is.EqualTo(1));
                Assert.That((int)Get(owner, "acceptedError"), Is.EqualTo(0));
                var log = JObject.Parse(File.ReadAllText(output + "/log-first.json"));
                Assert.That((string)log["condition"], Is.EqualTo("Synthetic unknown exception retained"));
                Assert.That((bool)Get(owner, "handlesDisposed"), Is.True);
            }
            finally { DisposeTestFixture(owner, output); }
        }

        [Test]
        public void R07_ExternalBoundaryIsBoundedAndNeverDisposes()
        {
            object owner = NewOwner("external", out string output);
            try
            {
                Call(owner, "OnPlayModeChanged", PlayModeStateChange.ExitingPlayMode);
                byte[] first = File.ReadAllBytes(output + "/external-exiting-play.json");
                Call(owner, "OnPlayModeChanged", PlayModeStateChange.ExitingPlayMode);
                CollectionAssert.AreEqual(first, File.ReadAllBytes(output + "/external-exiting-play.json"));
                Call(owner, "OnPlayModeChanged", PlayModeStateChange.EnteredEditMode);
                Call(owner, "OnPlayModeChanged", PlayModeStateChange.EnteredEditMode);
                Assert.That((bool)Get(owner, "handlesDisposed"), Is.False);
                Assert.That((bool)Get(owner, "cloneReleased"), Is.False);
                Assert.That((int)Get(owner, "logIoFailure"), Is.EqualTo(0));
                Assert.Throws<InvalidOperationException>(() => Call(owner, "ReleaseOwnedResourcesForExit"));
                Assert.That(File.Exists(output + "/expected-exit-reload.json"), Is.False);
                Assert.That((string)JObject.Parse(File.ReadAllText(output + "/external-entered-edit.json"))["dispose"], Is.EqualTo("UNKNOWN"));
                Assert.That((bool)JObject.Parse(M12RealSceneCloseDiagnostics.ReadStatus())["hotOwner"], Is.False);
            }
            finally { DisposeTestFixture(owner, output); }
        }
    }

}
#endif


