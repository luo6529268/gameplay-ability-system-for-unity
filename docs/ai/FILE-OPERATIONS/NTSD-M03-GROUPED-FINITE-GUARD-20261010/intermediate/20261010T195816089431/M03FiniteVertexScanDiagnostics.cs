#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using NTSD.Animation.Rendering;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    // Editor-only mirrors; this tool does not replace the production Upload guard.
    public sealed class M03FiniteVertexScanDiagnostics
    {
        private const string Root = "artifacts/diagnostics/NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-20261010";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
            public Vector4 UvSampleBounds;
        }

        [Serializable]
        private sealed class SemanticReport
        {
            public string status;
            public int actualBuilds;
            public int actualQuads;
            public int nonfiniteBuilds;
            public int generatedRectangles;
            public bool arbitraryCorruptionNegativeControl;
            public int stride;
        }

        [Serializable]
        private sealed class Window
        {
            public int quads;
            public bool grouped;
            public int order;
            public int warm = 8;
            public long[] ticks = new long[32];
            public BattleScopedGcAllocationRecorder.ScopeResult[] gc = new BattleScopedGcAllocationRecorder.ScopeResult[32];
            public double meanMs;
            public double p95Ms;
            public double p99Ms;
            public bool bufferUnchanged;
        }

        [Serializable]
        private sealed class CostReport
        {
            public string status = "NOT_COMPLETED";
            public string scope = "Editor local mirror scan only; one scan per sample; no Build or native upload gain claim";
            public long frequency;
            public string unit;
            public BattleScopedGcAllocationRecorder.CalibrationResult before;
            public BattleScopedGcAllocationRecorder.CalibrationResult after;
            public Window[] windows = new Window[8];
            public int completed;
        }

        private delegate bool Scan(Vertex[] vertices, int activeVertices);

        private static bool Original(Vertex[] vertices, int activeVertices)
        {
            for (int vertexIndex = 0; vertexIndex < activeVertices; vertexIndex++)
            {
                Vector3 position = vertices[vertexIndex].Position;
                if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
                    float.IsNaN(position.y) || float.IsInfinity(position.y) ||
                    float.IsNaN(position.z) || float.IsInfinity(position.z))
                    return false;
            }
            return true;
        }

        private static bool Grouped(Vertex[] vertices, int activeVertices)
        {
            for (int vertexIndex = 0; vertexIndex < activeVertices; vertexIndex += 4)
            {
                Vector3 position = vertices[vertexIndex].Position;
                float top = vertices[vertexIndex + 1].Position.y;
                float right = vertices[vertexIndex + 2].Position.x;
                if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
                    float.IsNaN(position.y) || float.IsInfinity(position.y) ||
                    float.IsNaN(position.z) || float.IsInfinity(position.z) ||
                    float.IsNaN(top) || float.IsInfinity(top) ||
                    float.IsNaN(right) || float.IsInfinity(right))
                    return false;
            }
            return true;
        }

        private static void Rectangle(Vertex[] vertices, float left, float right, float bottom, float top, float z)
        {
            vertices[0].Position = new Vector3(left, bottom, z);
            vertices[1].Position = new Vector3(left, top, z);
            vertices[2].Position = new Vector3(right, bottom, z);
            vertices[3].Position = new Vector3(right, top, z);
        }

        private static Vertex[] ReadCpuChunk(BattleDynamicMeshBackend backend, int chunkIndex)
        {
            var chunks = (Array)typeof(BattleDynamicMeshBackend).GetField("chunks", PrivateInstance).GetValue(backend);
            object chunk = chunks.GetValue(chunkIndex);
            var source = (Array)chunk.GetType().GetField("vertices", PrivateInstance).GetValue(chunk);
            Assert.That(Marshal.SizeOf(source.GetType().GetElementType()), Is.EqualTo(44));
            var result = new Vertex[source.Length];
            for (int i = 0; i < result.Length; i++)
            {
                object value = source.GetValue(i);
                Type type = value.GetType();
                result[i] = new Vertex
                {
                    Position = (Vector3)type.GetField("Position").GetValue(value),
                    Color = (Color32)type.GetField("Color").GetValue(value),
                    Uv = (Vector2)type.GetField("Uv").GetValue(value),
                    AtlasSlice = (float)type.GetField("AtlasSlice").GetValue(value),
                    UvSampleBounds = (Vector4)type.GetField("UvSampleBounds").GetValue(value)
                };
            }
            return result;
        }

        private static void SameBits(float a, float b)
        {
            Assert.That(BitConverter.SingleToInt32Bits(a), Is.EqualTo(BitConverter.SingleToInt32Bits(b)));
        }

        private static void CheckRectangleInvariant(Vertex[] vertices, int count)
        {
            Assert.That(count % 4, Is.Zero);
            for (int i = 0; i < count; i += 4)
            {
                SameBits(vertices[i].Position.x, vertices[i + 1].Position.x);
                SameBits(vertices[i + 2].Position.x, vertices[i + 3].Position.x);
                SameBits(vertices[i].Position.y, vertices[i + 2].Position.y);
                SameBits(vertices[i + 1].Position.y, vertices[i + 3].Position.y);
                for (int j = 1; j < 4; j++) SameBits(vertices[i].Position.z, vertices[i + j].Position.z);
            }
            Assert.That(Grouped(vertices, count), Is.EqualTo(Original(vertices, count)));
        }

        private static string Fingerprint(Vertex[] vertices)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            using (var sha = SHA256.Create())
            {
                foreach (Vertex v in vertices)
                {
                    writer.Write(v.Position.x); writer.Write(v.Position.y); writer.Write(v.Position.z);
                    writer.Write(v.Color.r); writer.Write(v.Color.g); writer.Write(v.Color.b); writer.Write(v.Color.a);
                    writer.Write(v.Uv.x); writer.Write(v.Uv.y); writer.Write(v.AtlasSlice);
                    writer.Write(v.UvSampleBounds.x); writer.Write(v.UvSampleBounds.y);
                    writer.Write(v.UvSampleBounds.z); writer.Write(v.UvSampleBounds.w);
                }
                return BitConverter.ToString(sha.ComputeHash(stream.ToArray()));
            }
        }

        [Test]
        public void ActualWriteQuadAndIeeeRectangles_PreserveFiniteDecision()
        {
            var report = new SemanticReport { stride = Marshal.SizeOf<Vertex>() };
            Assert.That(report.stride, Is.EqualTo(44));
            try
            {
                for (int mode = 0; mode < 3; mode++)
                using (var fixture = new BattleCatalogCentralResourceResolverEditorTests.SegmentUvFixture(mode, true))
                using (var backend = new BattleDynamicMeshBackend())
                {
                    backend.PrepareCapacity(4097);
                    foreach (int count in new[] { 0, 1, 4096, 4097, 1, 0, 4097 })
                    {
                        var frame = fixture.Frame(count);
                        string before = BattleCatalogCentralResourceResolverEditorTests.CaptureFields(frame);
                        backend.Build(frame, fixture.Resolver, BattleCentralDrawMode.OrderedChunks);
                        for (int chunk = 0; chunk < backend.ActiveChunkCount; chunk++)
                        {
                            int active = backend.GetChunkActiveQuadCount(chunk) * 4;
                            Vertex[] vertices = ReadCpuChunk(backend, chunk);
                            CheckRectangleInvariant(vertices, active);
                            Assert.That(Original(vertices, active), Is.True);
                            using (var data = Mesh.AcquireReadOnlyMeshData(backend.GetChunkMesh(chunk)))
                            {
                                var uploaded = data[0].GetVertexData<Vertex>(0);
                                for (int i = 0; i < active; i++)
                                {
                                    SameBits(uploaded[i].Position.x, vertices[i].Position.x);
                                    SameBits(uploaded[i].Position.y, vertices[i].Position.y);
                                    SameBits(uploaded[i].Position.z, vertices[i].Position.z);
                                }
                            }
                            report.actualQuads += active / 4;
                        }
                        Assert.That(BattleCatalogCentralResourceResolverEditorTests.CaptureFields(frame), Is.EqualTo(before));
                        report.actualBuilds++;
                    }
                    foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                    for (int axis = 0; axis < 3; axis++)
                    {
                        backend.Build(fixture.Frame(1), fixture.Resolver, BattleCentralDrawMode.OrderedChunks);
                        Vertex[] nativeBefore;
                        using (var data = Mesh.AcquireReadOnlyMeshData(backend.GetChunkMesh(0)))
                            nativeBefore = data[0].GetVertexData<Vertex>(0).ToArray();
                        Vector3 position = Vector3.zero;
                        position[axis] = bad;
                        var frame = new BattlePresentationFrame();
                        frame.Reset(88);
                        frame.AddCommand(fixture.Command(0, 0, position));
                        var exception = Assert.Throws<ArgumentException>(() => backend.Build(frame, fixture.Resolver, BattleCentralDrawMode.OrderedChunks));
                        Assert.That(exception.Message, Is.EqualTo("Battle mesh vertex positions must be finite."));
                        Vertex[] vertices = ReadCpuChunk(backend, 0);
                        CheckRectangleInvariant(vertices, 4);
                        Assert.That(Original(vertices, 4), Is.False);
                        using (var data = Mesh.AcquireReadOnlyMeshData(backend.GetChunkMesh(0)))
                            Assert.That(Fingerprint(data[0].GetVertexData<Vertex>(0).ToArray()), Is.EqualTo(Fingerprint(nativeBefore)));
                        report.nonfiniteBuilds++;
                        backend.Clear();
                    }
                }
                var rectangle = new Vertex[8];
                float[] values = { 0f, -0f, 1f, -1f, float.Epsilon, -float.Epsilon, float.MaxValue,
                    float.MinValue, float.NaN, float.PositiveInfinity, float.NegativeInfinity };
                foreach (float x in values)
                foreach (float size in values)
                foreach (float pivot in values)
                foreach (float z in values)
                {
                    float left = x - pivot * size;
                    float right = left + size;
                    float bottom = size - pivot * x;
                    float top = bottom + x;
                    Rectangle(rectangle, left, right, bottom, top, z);
                    CheckRectangleInvariant(rectangle, 4);
                    report.generatedRectangles++;
                }
                Rectangle(rectangle, 0f, 1f, 0f, 1f, 0f);
                rectangle[4].Position.x = float.NaN;
                Assert.That(Original(rectangle, 4) && Grouped(rectangle, 4), Is.True, "Inactive tail excluded");
                rectangle[3].Position.z = float.NaN;
                Assert.That(Original(rectangle, 4), Is.False);
                Assert.That(Grouped(rectangle, 4), Is.True, "Negative control: arbitrary buffer corruption is outside writer invariant");
                report.arbitraryCorruptionNegativeControl = true;
                report.status = "SCOPED_EQUIVALENCE_PASS";
            }
            catch { report.status = "FAILED"; throw; }
            finally { Save("semantic.json", report); }
        }

        [Test, Explicit("One authorized local scan group after semantic prerequisites")]
        public void FiniteScan_OneAbbaGroupPerActualArraySize()
        {
            Assert.That(File.ReadAllText(Path.Combine(Root, "semantic.json")), Does.Contain("SCOPED_EQUIVALENCE_PASS"));
            Assert.That(File.Exists(Path.Combine(Root, "local-cost.json")), Is.False, "Do not rerun an existing group");
            var report = new CostReport { frequency = Stopwatch.Frequency };
            try
            {
                using (var fixture = new BattleCatalogCentralResourceResolverEditorTests.SegmentUvFixture(1))
                using (var backend = new BattleDynamicMeshBackend())
                using (var recorder = new BattleScopedGcAllocationRecorder())
                {
                    backend.PrepareCapacity(4096);
                    report.unit = recorder.Unit;
                    report.before = recorder.Calibrate();
                    Assert.That(report.before.passed, Is.True);
                    foreach (int count in new[] { 2000, 4096 })
                    {
                        backend.Build(fixture.Frame(count), fixture.Resolver, BattleCentralDrawMode.OrderedChunks);
                        Vertex[] vertices = ReadCpuChunk(backend, 0);
                        CheckRectangleInvariant(vertices, count * 4);
                        string hash = Fingerprint(vertices);
                        Scan original = Original;
                        Scan grouped = Grouped;
                        for (int order = 0; order < 4; order++)
                        {
                            bool enabled = order == 1 || order == 2;
                            Scan scan = enabled ? grouped : original;
                            var window = new Window { quads = count, grouped = enabled, order = order };
                            report.windows[report.completed] = window;
                            for (int i = 0; i < window.warm; i++) Assert.That(scan(vertices, count * 4), Is.True);
                            for (int i = 0; i < window.ticks.Length; i++)
                            {
                                bool began = recorder.Begin();
                                bool finite = false;
                                try
                                {
                                    long start = Stopwatch.GetTimestamp();
                                    finite = scan(vertices, count * 4);
                                    window.ticks[i] = Stopwatch.GetTimestamp() - start;
                                }
                                finally { window.gc[i] = recorder.End(); }
                                Assert.That(began && finite, Is.True);
                                var gc = window.gc[i];
                                Assert.That(gc.valid && gc.calibratedBefore && !gc.wrapped && !gc.saturated, Is.True);
                                Assert.That(gc.allocationEvents, Is.Zero);
                            }
                            window.bufferUnchanged = Fingerprint(vertices) == hash;
                            Assert.That(window.bufferUnchanged, Is.True);
                            var sorted = (long[])window.ticks.Clone();
                            Array.Sort(sorted);
                            double sum = 0;
                            foreach (long ticks in sorted) sum += ticks;
                            window.meanMs = sum / sorted.Length * 1000.0 / report.frequency;
                            window.p95Ms = sorted[(int)Math.Ceiling(0.95 * sorted.Length) - 1] * 1000.0 / report.frequency;
                            window.p99Ms = sorted[(int)Math.Ceiling(0.99 * sorted.Length) - 1] * 1000.0 / report.frequency;
                            report.completed++;
                        }
                    }
                    report.after = recorder.Calibrate();
                    Assert.That(report.after.passed, Is.True);
                    bool gain = true;
                    for (int offset = 0; offset < 8; offset += 4)
                    {
                        Window a = report.windows[offset], b = report.windows[offset + 1],
                            c = report.windows[offset + 2], d = report.windows[offset + 3];
                        gain &= Math.Max(b.meanMs, c.meanMs) < Math.Min(a.meanMs, d.meanMs) &&
                            Math.Max(b.p95Ms, c.p95Ms) <= Math.Max(a.p95Ms, d.p95Ms) &&
                            Math.Max(b.p99Ms, c.p99Ms) <= Math.Max(a.p99Ms, d.p99Ms);
                    }
                    report.status = gain ? "LOCAL_SCAN_GAIN_SIGNAL_ONLY" : "INCONCLUSIVE_NO_DISCERNIBLE_LOCAL_GAIN";
                }
            }
            catch { report.status = "FAILED_STOPPED"; throw; }
            finally { Save("local-cost.json", report); }
        }

        private static void Save(string name, object report)
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, name), JsonUtility.ToJson(report, true));
        }
    }
}
#endif
