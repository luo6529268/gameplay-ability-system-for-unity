#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_Q07")]
    public sealed class NTSD28Q07KnockoutFeedProductionRowEditorTests
    {
        private const string Scenario =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-PUNCH-UNITY-RAW-FIRST-DIFF-001/naruto-punch-formal-30.json";
        private const string Output =
            "artifacts/diagnostics/NTSD28-Q07-KO-FEED-PRODUCTION-ROW-WITNESS-001/unity-row-38.csv";

        private readonly struct RowObservation
        {
            public RowObservation(int tick, int frameTick, int eventCount,
                int firstEventTime, int nativeRecordCount, int rowCount,
                int firstRowTime)
            {
                Tick = tick;
                FrameTick = frameTick;
                EventCount = eventCount;
                FirstEventTime = firstEventTime;
                NativeRecordCount = nativeRecordCount;
                RowCount = rowCount;
                FirstRowTime = firstRowTime;
            }

            public int Tick { get; }
            public int FrameTick { get; }
            public int EventCount { get; }
            public int FirstEventTime { get; }
            public int NativeRecordCount { get; }
            public int RowCount { get; }
            public int FirstRowTime { get; }

            public override string ToString() =>
                $"{Tick},{FrameTick},{EventCount},{FirstEventTime}," +
                $"{NativeRecordCount},{RowCount},{FirstRowTime}";
        }

        [Test]
        public void NarutoPunchPublishesKoFeedThroughNativeThirtyTickBoundary()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string runtimeRoot = Path.Combine(projectRoot,
                "Assets/NTSD/Content/LoganRuntime");
            var observations = new List<RowObservation>(38);
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    runtimeRoot, Scenario, BattleRuntimeProfile.Authority400, 38,
                    (driver, inputs, _) =>
                    {
                        SimulationWorld world = driver.World;
                        for (int tick = 1; tick <= inputs.Length; tick++)
                        {
                            Assert.That(driver.StepOneTick(inputs[tick - 1],
                                    buildPresentation: true),
                                Is.True, "tick=" + tick);
                            BattlePresentationFrame frame =
                                world.BattlePresentation.PublishedFrame;
                            int eventCount = world.NativeKnockoutEvents.Count;
                            int rowCount = frame?.KnockoutFeedRowCount ?? -1;
                            observations.Add(new RowObservation(tick,
                                frame?.TickIndex ?? -1,
                                eventCount,
                                eventCount > 0
                                    ? world.NativeKnockoutEvents[0].BattleTimeTick : -1,
                                frame?.KnockoutFeedNativeRecordCount ?? -1,
                                rowCount,
                                rowCount > 0
                                    ? frame.GetKnockoutFeedRow(0).BattleTimeTick : -1));
                        }
                    },
                    useProjectMode: true);
            }
            finally
            {
                string path = Path.Combine(projectRoot, Output);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var rows = new List<string>(observations.Count + 1)
                {
                    "tick,frame_tick,event_count,first_event_time,native_record_count,row_count,first_row_time",
                };
                foreach (RowObservation observation in observations)
                    rows.Add(observation.ToString());
                File.WriteAllLines(path, rows);
            }

            Assert.That(observations.Count, Is.EqualTo(38));
            AssertAt(observations, 7, 0, 0, 0);
            AssertAt(observations, 8, 1, 1, 7);
            AssertAt(observations, 36, 1, 1, 7);
            AssertAt(observations, 37, 1, 0, 7);
            AssertAt(observations, 38, 1, 0, 7);
        }

        private static void AssertAt(IReadOnlyList<RowObservation> observations,
            int tick, int nativeRecordCount, int rowCount, int eventTime)
        {
            RowObservation value = observations[tick - 1];
            Assert.That(value.FrameTick, Is.EqualTo(tick), "frame tick=" + tick);
            Assert.That(value.EventCount, Is.EqualTo(nativeRecordCount),
                "event count tick=" + tick);
            Assert.That(value.NativeRecordCount, Is.EqualTo(nativeRecordCount),
                "native record count tick=" + tick);
            Assert.That(value.RowCount, Is.EqualTo(rowCount),
                "published row count tick=" + tick);
            if (nativeRecordCount > 0)
                Assert.That(value.FirstEventTime, Is.EqualTo(eventTime),
                    "event time tick=" + tick);
            if (rowCount > 0)
                Assert.That(value.FirstRowTime, Is.EqualTo(eventTime),
                    "row time tick=" + tick);
        }
    }
}
#endif
