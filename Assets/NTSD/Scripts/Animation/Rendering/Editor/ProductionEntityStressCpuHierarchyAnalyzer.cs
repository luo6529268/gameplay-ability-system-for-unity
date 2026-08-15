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
    [InitializeOnLoad]
    internal static class ProductionEntityStressCpuHierarchyAnalyzer
    {
        private const int MaximumFrames = 10000;
        private const int MaximumRows = 160;
        private const int MaximumAllocationRows = 80;
        private const string OutputPath = "Temp/NTSD_ProductionEntityStress.cpu-hierarchy.txt";
        private const string SavedProfilePath = "Temp/NTSD_1000AI_p93-steady-cpu.raw";
        private const string TickMarkerName = "NTSD.ProductionEntityStress.Driver.StepOneTick";
        private const string AllocationMarkerName = "GC.Alloc";
        private const string ReportWriteMarkerName =
            "NTSD.ProductionEntityStress.WriteReport";
        private const string StopAndDumpSessionKey =
            "NTSD.ProductionEntityStress.StopAndDumpAtSampleCompletion";
        private const string RequestPath =
            "Temp/NTSD_ProductionEntityStress.profiler-analysis.request";
        private static readonly string AbsoluteRequestPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", RequestPath));
        private static readonly string AbsoluteSavedProfilePath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", SavedProfilePath));

        static ProductionEntityStressCpuHierarchyAnalyzer()
        {
            ProductionEntityStressEditorBridge
                .StopAndDumpIfArmedAtSampleCompletionAction =
                StopAndDumpIfArmedAtSampleCompletion;
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!File.Exists(AbsoluteRequestPath))
                return;

            string request;
            try
            {
                request = File.ReadAllText(AbsoluteRequestPath).Trim();
                File.Delete(AbsoluteRequestPath);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[ProductionEntityStress] Profiler analysis request failed: {exception}");
                return;
            }

            switch (request)
            {
                case "cpu-p56":
                    LoadSavedProfileAndDump();
                    break;
                case "gc-p57":
                    EditorApplication.ExecuteMenuItem(
                        "NTSD/Battle Diagnostics/Load Saved DataOriented GC Profile and Dump");
                    break;
                case "stop-play":
                    EditorApplication.isPlaying = false;
                    break;
                default:
                    Debug.LogWarning(
                        $"[ProductionEntityStress] Unknown profiler analysis request: {request}");
                    break;
            }
        }

        private sealed class SampleAggregate
        {
            internal string Name;
            internal double InclusiveMilliseconds;
            internal double SelfMilliseconds;
            internal int Calls;
            internal int Frames;
        }

        private readonly struct SampleRange
        {
            internal SampleRange(string name, int endExclusive)
            {
                Name = name;
                EndExclusive = endExclusive;
            }

            internal string Name { get; }
            internal int EndExclusive { get; }
        }

        [MenuItem("NTSD/Battle Diagnostics/Load 1000 AI CPU Profile and Dump")]
        private static void LoadSavedProfileAndDump()
        {
            if (!File.Exists(AbsoluteSavedProfilePath))
            {
                Debug.LogWarning(
                    $"[ProductionEntityStress] Saved Profiler capture was not found: {AbsoluteSavedProfilePath}");
                return;
            }

            ProfilerDriver.LoadProfile(AbsoluteSavedProfilePath, false);
            EditorApplication.delayCall += DumpLoadedProfileAndClear;
        }

        private static void DumpLoadedProfileAndClear()
        {
            try
            {
                DumpLoadedProfile();
            }
            finally
            {
                ProfilerDriver.ClearAllFrames();
            }
        }

        [MenuItem("NTSD/Battle Diagnostics/Dump Loaded 1000 AI CPU Hierarchy")]
        private static void DumpLoadedProfile()
        {
            int firstFrame = ProfilerDriver.firstFrameIndex;
            int lastFrame = ProfilerDriver.lastFrameIndex;
            if (firstFrame < 0 || lastFrame < firstFrame)
            {
                Debug.LogWarning("[ProductionEntityStress] No Profiler frames are available.");
                return;
            }

            int scanFirstFrame = Math.Max(firstFrame, lastFrame - MaximumFrames + 1);
            var aggregates = new Dictionary<string, SampleAggregate>(StringComparer.Ordinal);
            var frameAggregates = new Dictionary<string, SampleAggregate>(StringComparer.Ordinal);
            var allocationPathAggregates =
                new Dictionary<string, SampleAggregate>(StringComparer.Ordinal);
            var frameAllocationPathAggregates =
                new Dictionary<string, SampleAggregate>(StringComparer.Ordinal);
            var sampleRanges = new List<SampleRange>(32);
            int scannedFrames = 0;
            int tickFrames = 0;
            int reportWriteFramesExcluded = 0;
            double mainThreadMilliseconds = 0.0;

            for (int frameIndex = scanFirstFrame; frameIndex <= lastFrame; frameIndex++)
            {
                using (RawFrameDataView frame = ProfilerDriver.GetRawFrameDataView(frameIndex, 0))
                {
                    if (frame == null || !frame.valid || frame.sampleCount <= 0)
                        continue;

                    frameAggregates.Clear();
                    frameAllocationPathAggregates.Clear();
                    sampleRanges.Clear();
                    bool containsTick = false;
                    bool containsReportWrite = false;
                    for (int sampleIndex = 0; sampleIndex < frame.sampleCount; sampleIndex++)
                    {
                        while (sampleRanges.Count > 0 &&
                               sampleRanges[sampleRanges.Count - 1].EndExclusive <= sampleIndex)
                        {
                            sampleRanges.RemoveAt(sampleRanges.Count - 1);
                        }

                        string sampleName = frame.GetSampleName(sampleIndex);
                        if (string.IsNullOrEmpty(sampleName))
                            continue;

                        float inclusiveMilliseconds = frame.GetSampleTimeMs(sampleIndex);
                        double selfMilliseconds = CalculateSelfMilliseconds(frame, sampleIndex, inclusiveMilliseconds);
                        if (!frameAggregates.TryGetValue(sampleName, out SampleAggregate aggregate))
                        {
                            aggregate = new SampleAggregate { Name = sampleName };
                            frameAggregates.Add(sampleName, aggregate);
                        }

                        aggregate.InclusiveMilliseconds += inclusiveMilliseconds;
                        aggregate.SelfMilliseconds += selfMilliseconds;
                        aggregate.Calls++;
                        if (string.Equals(sampleName, TickMarkerName, StringComparison.Ordinal))
                            containsTick = true;
                        if (string.Equals(
                                sampleName,
                                ReportWriteMarkerName,
                                StringComparison.Ordinal))
                        {
                            containsReportWrite = true;
                        }
                        if (string.Equals(
                                sampleName,
                                AllocationMarkerName,
                                StringComparison.Ordinal))
                        {
                            string allocationPath = BuildAllocationPath(sampleRanges);
                            if (!frameAllocationPathAggregates.TryGetValue(
                                    allocationPath,
                                    out SampleAggregate allocationAggregate))
                            {
                                allocationAggregate = new SampleAggregate
                                {
                                    Name = allocationPath,
                                };
                                frameAllocationPathAggregates.Add(
                                    allocationPath,
                                    allocationAggregate);
                            }

                            allocationAggregate.InclusiveMilliseconds +=
                                inclusiveMilliseconds;
                            allocationAggregate.SelfMilliseconds += selfMilliseconds;
                            allocationAggregate.Calls++;
                        }

                        int recursiveChildCount =
                            frame.GetSampleChildrenCountRecursive(sampleIndex);
                        if (recursiveChildCount > 0)
                        {
                            sampleRanges.Add(new SampleRange(
                                sampleName,
                                sampleIndex + recursiveChildCount + 1));
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
                    mainThreadMilliseconds += frame.GetSampleTimeMs(0);
                    foreach (KeyValuePair<string, SampleAggregate> pair in frameAggregates)
                    {
                        SampleAggregate frameAggregate = pair.Value;
                        if (!aggregates.TryGetValue(pair.Key, out SampleAggregate aggregate))
                        {
                            aggregate = new SampleAggregate { Name = pair.Key };
                            aggregates.Add(pair.Key, aggregate);
                        }

                        aggregate.InclusiveMilliseconds += frameAggregate.InclusiveMilliseconds;
                        aggregate.SelfMilliseconds += frameAggregate.SelfMilliseconds;
                        aggregate.Calls += frameAggregate.Calls;
                        aggregate.Frames++;
                    }
                    foreach (KeyValuePair<string, SampleAggregate> pair in
                             frameAllocationPathAggregates)
                    {
                        SampleAggregate frameAggregate = pair.Value;
                        if (!allocationPathAggregates.TryGetValue(
                                pair.Key,
                                out SampleAggregate aggregate))
                        {
                            aggregate = new SampleAggregate { Name = pair.Key };
                            allocationPathAggregates.Add(pair.Key, aggregate);
                        }

                        aggregate.InclusiveMilliseconds +=
                            frameAggregate.InclusiveMilliseconds;
                        aggregate.SelfMilliseconds += frameAggregate.SelfMilliseconds;
                        aggregate.Calls += frameAggregate.Calls;
                        aggregate.Frames++;
                    }
                }
            }

            var rows = new List<SampleAggregate>(aggregates.Values);
            rows.Sort(CompareBySelfTime);
            var builder = new StringBuilder(32768);
            builder.AppendLine("NTSD Production Entity Stress - CPU Hierarchy");
            builder.Append("profile=currently-loaded")
                .Append(" frameRange=").Append(scanFirstFrame).Append("..").Append(lastFrame)
                .Append(" tickFrames=").Append(tickFrames)
                .Append(" scannedFrames=").Append(scannedFrames)
                .Append(" reportWriteFramesExcluded=")
                .Append(reportWriteFramesExcluded)
                .AppendLine();
            if (tickFrames > 0)
            {
                builder.Append("averageMainThreadFrameMs=")
                    .Append((mainThreadMilliseconds / tickFrames).ToString("F4"))
                    .AppendLine();
            }

            builder.AppendLine("avgSelfMs\tavgInclusiveMs\tavgCallsPerTickFrame\tframes\tname");
            int rowCount = Math.Min(MaximumRows, rows.Count);
            for (int index = 0; index < rowCount; index++)
            {
                SampleAggregate row = rows[index];
                builder.Append((row.SelfMilliseconds / tickFrames).ToString("F4")).Append('\t')
                    .Append((row.InclusiveMilliseconds / tickFrames).ToString("F4")).Append('\t')
                    .Append(((double)row.Calls / tickFrames).ToString("F3")).Append('\t')
                    .Append(row.Frames).Append('\t')
                    .AppendLine(row.Name);
            }

            var allocationRows =
                new List<SampleAggregate>(allocationPathAggregates.Values);
            allocationRows.Sort(CompareByCalls);
            builder.AppendLine();
            builder.AppendLine(
                "GC.Alloc parent paths (callsPerTickFrame, avgSelfMs, frames, path)");
            int allocationRowCount = Math.Min(
                MaximumAllocationRows,
                allocationRows.Count);
            for (int index = 0; index < allocationRowCount; index++)
            {
                SampleAggregate row = allocationRows[index];
                builder.Append(((double)row.Calls / tickFrames).ToString("F3"))
                    .Append('\t')
                    .Append((row.SelfMilliseconds / tickFrames).ToString("F4"))
                    .Append('\t')
                    .Append(row.Frames).Append('\t')
                    .AppendLine(row.Name);
            }

            string absoluteOutputPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", OutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutputPath));
            File.WriteAllText(absoluteOutputPath, builder.ToString(), new UTF8Encoding(false));
            Debug.Log($"[ProductionEntityStress] CPU hierarchy written to {absoluteOutputPath}");
        }

        [MenuItem("NTSD/Battle Diagnostics/Stop Profiler and Dump Current 1000 AI CPU Hierarchy")]
        private static void StopProfilerAndDumpCurrentProfile()
        {
            ProfilerDriver.enabled = false;
            DumpLoadedProfile();
        }

        [MenuItem("NTSD/Battle Diagnostics/Arm CPU Hierarchy Dump at Stress Sample Completion")]
        private static void ArmDumpAtStressSampleCompletion()
        {
            SessionState.SetBool(StopAndDumpSessionKey, true);
            ProfilerDriver.enabled = true;
            Debug.Log(
                "[ProductionEntityStress] CPU hierarchy capture armed for the next completed stress sample.");
        }

        internal static void StopAndDumpIfArmedAtSampleCompletion()
        {
            if (!SessionState.GetBool(StopAndDumpSessionKey, false))
                return;

            SessionState.SetBool(StopAndDumpSessionKey, false);
            ProfilerDriver.enabled = false;
            EditorApplication.delayCall += DumpLoadedProfile;
        }

        private static double CalculateSelfMilliseconds(
            RawFrameDataView frame,
            int sampleIndex,
            float inclusiveMilliseconds)
        {
            int directChildCount = frame.GetSampleChildrenCount(sampleIndex);
            if (directChildCount <= 0)
                return inclusiveMilliseconds;

            double childMilliseconds = 0.0;
            int childIndex = sampleIndex + 1;
            for (int child = 0; child < directChildCount && childIndex < frame.sampleCount; child++)
            {
                childMilliseconds += frame.GetSampleTimeMs(childIndex);
                childIndex += frame.GetSampleChildrenCountRecursive(childIndex) + 1;
            }

            return Math.Max(0.0, inclusiveMilliseconds - childMilliseconds);
        }

        private static int CompareBySelfTime(SampleAggregate left, SampleAggregate right)
        {
            int self = right.SelfMilliseconds.CompareTo(left.SelfMilliseconds);
            if (self != 0)
                return self;
            int inclusive = right.InclusiveMilliseconds.CompareTo(left.InclusiveMilliseconds);
            return inclusive != 0
                ? inclusive
                : string.CompareOrdinal(left.Name, right.Name);
        }

        private static int CompareByCalls(SampleAggregate left, SampleAggregate right)
        {
            int calls = right.Calls.CompareTo(left.Calls);
            if (calls != 0)
                return calls;
            return CompareBySelfTime(left, right);
        }

        private static string BuildAllocationPath(List<SampleRange> sampleRanges)
        {
            const int maximumAncestors = 8;
            int start = Math.Max(0, sampleRanges.Count - maximumAncestors);
            var builder = new StringBuilder(256);
            for (int index = start; index < sampleRanges.Count; index++)
            {
                if (builder.Length > 0)
                    builder.Append(" > ");
                builder.Append(sampleRanges[index].Name);
            }
            if (builder.Length > 0)
                builder.Append(" > ");
            builder.Append(AllocationMarkerName);
            return builder.ToString();
        }
    }
}
#endif
