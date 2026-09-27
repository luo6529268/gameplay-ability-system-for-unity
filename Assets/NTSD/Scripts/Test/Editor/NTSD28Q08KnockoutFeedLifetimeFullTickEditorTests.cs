#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q08KnockoutFeedLifetimeFullTickEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string ScenarioRoot =
            "artifacts/diagnostics/NTSD28-Q08-NEGATIVE-ENV-KO-FULL-TICK-001";
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD28-Q08-KNOCKOUT-FEED-LIFETIME-FULL-TICK-001";

        [Test]
        public void Time12KnockoutTailExpiresAtFormalFullTickBoundary()
        {
            RunCase(5, "positive", "negative-environment-hp5.json");
            RunCase(50, "control", "negative-environment-hp50.json");
        }

        private static void RunCase(int initialHp, string label, string scenario)
        {
            string[] expectedLines = File.ReadAllLines(Path.GetFullPath(
                OutputRoot + "/native-run-01/" + label + ".csv"));
            Assert.That(expectedLines.Length, Is.EqualTo(85), label + " native rows");
            var observed = new List<string>(85)
            {
                "tick,derived_phase12,initial_hp,action,hp,effective_hp," +
                "environment_state,credit_score,credit_knockouts,event_count," +
                "event_time,event_type,event_victim,event_source,event_credit," +
                "event_four_owner,result_timer"
            };
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    FormalRuntime, ScenarioRoot + "/" + scenario,
                    BattleRuntimeProfile.Authority400, 84,
                    (driver, inputs, _) =>
                    {
                        SimulationWorld world = driver.World;
                        LF2Entity victim = world.FindEntityByRuntimeSlotForQuery(0);
                        LF2Entity credit = world.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(victim, Is.Not.Null);
                        Assert.That(credit, Is.Not.Null);
                        Assert.That(world.Runtime.NativeKnockoutFeed.RecordPresent, Is.True);
                        Assert.That(world.Runtime.NativeKnockoutFeed.LifetimeTicks, Is.EqualTo(70));
                        victim.Health.HP = initialHp;
                        victim.Runtime.HPBound = initialHp;
                        victim.Runtime.EnvironmentState320 = -1;
                        victim.Runtime.CatchSourceSlot90 = 1;
                        victim.Runtime.ImpactSourceSlot164 = -1;

                        for (int tick = 1; tick <= 84; tick++)
                        {
                            Assert.That(driver.StepOneTick(inputs[tick - 1], true, false),
                                Is.True, label + " full Driver tick=" + tick);
                            IReadOnlyList<NativeKnockoutEvent> events =
                                world.NativeKnockoutEvents;
                            NativeKnockoutEvent last = events.Count == 0
                                ? default
                                : events[events.Count - 1];
                            observed.Add(string.Join(",", new[]
                            {
                                tick.ToString(CultureInfo.InvariantCulture),
                                world.NativeResourcePhase12.ToString(CultureInfo.InvariantCulture),
                                initialHp.ToString(CultureInfo.InvariantCulture),
                                (victim.Frame.D?.frameId ?? -1).ToString(CultureInfo.InvariantCulture),
                                victim.Health.HP.ToString(CultureInfo.InvariantCulture),
                                victim.Runtime.HPBound.ToString(CultureInfo.InvariantCulture),
                                victim.Runtime.EnvironmentState320.ToString(CultureInfo.InvariantCulture),
                                credit.Runtime.InputScoreTotal348.ToString(CultureInfo.InvariantCulture),
                                credit.Runtime.KnockoutCount358.ToString(CultureInfo.InvariantCulture),
                                events.Count.ToString(CultureInfo.InvariantCulture),
                                (events.Count == 0 ? -1 : last.BattleTimeTick)
                                    .ToString(CultureInfo.InvariantCulture),
                                (events.Count == 0 ? -1 : last.SourceObjectType)
                                    .ToString(CultureInfo.InvariantCulture),
                                (events.Count == 0 ? -1 : last.VictimSlot)
                                    .ToString(CultureInfo.InvariantCulture),
                                (events.Count == 0 ? -1 : last.SourceSlot)
                                    .ToString(CultureInfo.InvariantCulture),
                                (events.Count == 0 ? -1 : last.CreditSlot)
                                    .ToString(CultureInfo.InvariantCulture),
                                (events.Count == 0 ? -1 : last.FourOwnerSlot)
                                    .ToString(CultureInfo.InvariantCulture),
                                world.Runtime.Results.NativeResultTimer
                                    .ToString(CultureInfo.InvariantCulture)
                            }));
                        }
                    }, useProjectMode: true);
            }
            finally
            {
                string directory = Path.GetFullPath(OutputRoot + "/unity");
                Directory.CreateDirectory(directory);
                string output = Path.Combine(directory, label + "-" +
                    DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff",
                        CultureInfo.InvariantCulture) + "-" +
                    Guid.NewGuid().ToString("N") + ".csv");
                File.WriteAllLines(output, observed);
                TestContext.Progress.WriteLine("Q08 knockout lifetime raw: " + output);
            }

            Assert.That(observed.Count, Is.EqualTo(expectedLines.Length), label + " count");
            for (int index = 1; index < expectedLines.Length; index++)
            {
                Assert.That(observed[index], Is.EqualTo(expectedLines[index]),
                    label + " completed tick=" + index);
            }
        }
    }
}
#endif
