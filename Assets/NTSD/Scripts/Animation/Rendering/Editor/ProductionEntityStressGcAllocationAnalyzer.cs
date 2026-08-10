#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    internal static class ProductionEntityStressGcAllocationAnalyzer
    {
        private const int MaximumFrames = 10000;
        private const int MaximumRows = 100;
        private const string OutputPath = "Temp/NTSD_ProductionEntityStress.gc-callstacks.txt";
        private const string SavedProfilePath = "Temp/NTSD_1000AI_p57-gc-path.raw";
        private const string TickMarkerName =
            "NTSD.ProductionEntityStress.Driver.StepOneTick";
        private const string ReportWriteMarkerName =
            "NTSD.ProductionEntityStress.WriteReport";

        [MenuItem("NTSD/Battle Diagnostics/Start Live GC Alloc Capture")]
        private static void StartLiveGcAllocCapture()
        {
            ProfilerDriver.ClearAllFrames();
            ProfilerDriver.profileEditor = true;
            ProfilerDriver.enabled = true;
            UnityEngine.Profiling.Profiler.enableAllocationCallstacks = true;
            Debug.Log("[ProductionEntityStress] Live GC.Alloc capture started.");
        }

        [MenuItem("NTSD/Battle Diagnostics/Stop Live GC Alloc Capture and Dump")]
        private static void StopLiveGcAllocCaptureAndDump()
        {
            ProfilerDriver.enabled = false;
            DumpRecentGcAllocCallstacks();
            UnityEngine.Profiling.Profiler.enableAllocationCallstacks = false;
        }

        private sealed class AllocationAggregate
        {
            internal string Key;
            internal long Bytes;
            internal int Count;
        }

        private readonly struct SampleScope
        {
            internal SampleScope(int endSampleIndex, string name)
            {
                EndSampleIndex = endSampleIndex;
                Name = name;
            }

            internal int EndSampleIndex { get; }
            internal string Name { get; }
        }

        [MenuItem("NTSD/Battle Diagnostics/Dump Recent GC Alloc Callstacks")]
        private static void DumpRecentGcAllocCallstacks()
        {
            int firstFrame = ProfilerDriver.firstFrameIndex;
            int lastFrame = ProfilerDriver.lastFrameIndex;
            if (firstFrame < 0 || lastFrame < firstFrame)
            {
                Debug.LogWarning("[ProductionEntityStress] No Profiler frames are available.");
                return;
            }

            int scanFirstFrame = Math.Max(firstFrame, lastFrame - MaximumFrames + 1);
            var aggregates = new Dictionary<string, AllocationAggregate>(StringComparer.Ordinal);
            var callstack = new List<ulong>(32);
            var sampleScopes = new List<SampleScope>(32);
            long totalBytes = 0;
            int totalCount = 0;
            int scannedFrames = 0;
            int tickFrames = 0;
            int reportWriteFramesExcluded = 0;

            for (int frameIndex = scanFirstFrame; frameIndex <= lastFrame; frameIndex++)
            {
                using (RawFrameDataView frame = ProfilerDriver.GetRawFrameDataView(frameIndex, 0))
                {
                    if (frame == null || !frame.valid || frame.sampleCount <= 0)
                        continue;

                    bool containsTick = false;
                    bool containsReportWrite = false;
                    for (int sampleIndex = 0; sampleIndex < frame.sampleCount; sampleIndex++)
                    {
                        string sampleName = frame.GetSampleName(sampleIndex);
                        if (string.Equals(sampleName, TickMarkerName, StringComparison.Ordinal))
                            containsTick = true;
                        if (string.Equals(
                                sampleName,
                                ReportWriteMarkerName,
                                StringComparison.Ordinal))
                        {
                            containsReportWrite = true;
                        }
                    }

                    if (!containsTick)
                        continue;
                    if (containsReportWrite)
                    {
                        reportWriteFramesExcluded++;
                        continue;
                    }

                    scannedFrames++;
                    tickFrames++;
                    sampleScopes.Clear();
                    for (int sampleIndex = 0; sampleIndex < frame.sampleCount; sampleIndex++)
                    {
                        while (sampleScopes.Count > 0 &&
                               sampleIndex > sampleScopes[sampleScopes.Count - 1].EndSampleIndex)
                        {
                            sampleScopes.RemoveAt(sampleScopes.Count - 1);
                        }

                        string sampleName = frame.GetSampleName(sampleIndex);
                        if (string.Equals(sampleName, "GC.Alloc", StringComparison.Ordinal))
                        {
                            long bytes = ReadAllocatedBytes(frame, sampleIndex);
                            string key = ResolveProjectCallsite(frame, sampleIndex, callstack);
                            if (string.Equals(
                                    key,
                                    "<no managed allocation callstack>",
                                    StringComparison.Ordinal))
                            {
                                key = ResolveSampleHierarchyCallsite(sampleScopes);
                            }

                            if (!aggregates.TryGetValue(key, out AllocationAggregate aggregate))
                            {
                                aggregate = new AllocationAggregate { Key = key };
                                aggregates.Add(key, aggregate);
                            }

                            aggregate.Bytes += bytes;
                            aggregate.Count++;
                            totalBytes += bytes;
                            totalCount++;
                        }

                        int descendantCount = frame.GetSampleChildrenCountRecursive(sampleIndex);
                        if (descendantCount > 0)
                        {
                            sampleScopes.Add(
                                new SampleScope(sampleIndex + descendantCount, sampleName));
                        }
                    }
                }
            }

            var rows = new List<AllocationAggregate>(aggregates.Values);
            rows.Sort(CompareAggregates);
            var builder = new StringBuilder(8192);
            builder.AppendLine("NTSD Production Entity Stress - Recent GC.Alloc Callstacks");
            builder.Append("frames=").Append(scannedFrames)
                .Append(" tickFrames=").Append(tickFrames)
                .Append(" reportWriteFramesExcluded=")
                .Append(reportWriteFramesExcluded)
                .Append(" range=").Append(scanFirstFrame).Append("..").Append(lastFrame)
                .Append(" allocations=").Append(totalCount)
                .Append(" bytes=").Append(totalBytes)
                .AppendLine();
            builder.AppendLine("bytes\tcount\tcallsite");
            int rowCount = Math.Min(MaximumRows, rows.Count);
            for (int index = 0; index < rowCount; index++)
            {
                AllocationAggregate row = rows[index];
                builder.Append(row.Bytes).Append('\t')
                    .Append(row.Count).Append('\t')
                    .AppendLine(row.Key);
            }

            string absolutePath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", OutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, builder.ToString(), new UTF8Encoding(false));
            Debug.Log($"[ProductionEntityStress] GC allocation callstacks written to {absolutePath}");
        }

        [MenuItem("NTSD/Battle Diagnostics/Load Saved DataOriented GC Profile and Dump")]
        private static void LoadSavedDataOrientedGcProfileAndDump()
        {
            string absolutePath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", SavedProfilePath));
            if (!File.Exists(absolutePath))
            {
                Debug.LogWarning($"[ProductionEntityStress] Saved Profiler capture was not found: {absolutePath}");
                return;
            }

            ProfilerDriver.LoadProfile(absolutePath, false);
            EditorApplication.delayCall += DumpRecentGcAllocCallstacks;
        }

        [MenuItem("NTSD/Battle Diagnostics/Clear Loaded Profiler Frames")]
        private static void ClearLoadedProfilerFrames()
        {
            ProfilerDriver.ClearAllFrames();
            Debug.Log("[ProductionEntityStress] Loaded Profiler frames cleared.");
        }

        private static int CompareAggregates(AllocationAggregate left, AllocationAggregate right)
        {
            int bytes = right.Bytes.CompareTo(left.Bytes);
            if (bytes != 0)
                return bytes;
            int count = right.Count.CompareTo(left.Count);
            return count != 0
                ? count
                : string.CompareOrdinal(left.Key, right.Key);
        }

        private static long ReadAllocatedBytes(RawFrameDataView frame, int sampleIndex)
        {
            if (frame.GetSampleMetadataCount(sampleIndex) <= 0)
                return 0;
            try
            {
                return frame.GetSampleMetadataAsLong(sampleIndex, 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static string ResolveProjectCallsite(
            RawFrameDataView frame,
            int sampleIndex,
            List<ulong> callstack)
        {
            callstack.Clear();
            frame.GetSampleCallstack(sampleIndex, callstack);
            string firstResolved = null;
            for (int index = 0; index < callstack.Count; index++)
            {
                FrameDataView.MethodInfo method = frame.ResolveMethodInfo(callstack[index]);
                if (string.IsNullOrEmpty(method.methodName))
                    continue;

                string resolved = FormatMethod(method);
                if (firstResolved == null)
                    firstResolved = resolved;
                if (!string.IsNullOrEmpty(method.sourceFileName) &&
                    method.sourceFileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return resolved;
                }
            }

            return firstResolved ?? "<no managed allocation callstack>";
        }

        private static string FormatMethod(FrameDataView.MethodInfo method)
        {
            if (string.IsNullOrEmpty(method.sourceFileName))
                return method.methodName;
            return method.methodName + " (" + method.sourceFileName + ":" + method.sourceFileLine + ")";
        }

        private static string ResolveSampleHierarchyCallsite(List<SampleScope> sampleScopes)
        {
            if (sampleScopes.Count == 0)
                return "<no managed allocation callstack or sample parent>";

            var builder = new StringBuilder(256);
            builder.Append("<sample hierarchy> ");
            int first = Math.Max(0, sampleScopes.Count - 8);
            for (int index = first; index < sampleScopes.Count; index++)
            {
                if (index > first)
                    builder.Append(" > ");
                builder.Append(sampleScopes[index].Name);
            }

            return builder.ToString();
        }
    }
}
#endif
