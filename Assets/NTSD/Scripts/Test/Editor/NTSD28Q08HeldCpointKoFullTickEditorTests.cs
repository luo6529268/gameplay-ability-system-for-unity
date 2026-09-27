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
    public sealed class NTSD28Q08HeldCpointKoFullTickEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string Root =
            "artifacts/diagnostics/NTSD28-Q08-HELD-CPOINT-KO-FULL-TICK-001";

        [Test]
        public void HeldCpointKnockoutAndNonlethalControlMatchFormalFullTicks()
        {
            RunCase(30, "positive", "held-cpoint-hp30.json");
            RunCase(100, "control", "held-cpoint-hp100.json");
        }

        private static void RunCase(int initialHp, string label, string scenario)
        {
            string[] expectedLines = File.ReadAllLines(Path.GetFullPath(
                Root + "/native-run-01/" + label + ".csv"));
            Assert.That(expectedLines.Length, Is.EqualTo(4), label + " native rows");
            var observed = new List<string>(4)
            {
                "tick,initial_hp,catcher_action,victim_action,victim_hp," +
                "victim_effective_hp,catch_target,catch_source,catcher_counter," +
                "catch_timeout,catcher_score,catcher_knockouts,event_count," +
                "event_time,event_type,event_victim,event_source,event_credit," +
                "event_four_owner,result_timer"
            };
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    FormalRuntime, Root + "/" + scenario,
                    BattleRuntimeProfile.Authority400, 3,
                    (driver, inputs, _) =>
                    {
                        SimulationWorld world = driver.World;
                        LF2Entity catcher = world.FindEntityByRuntimeSlotForQuery(0);
                        LF2Entity victim = world.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(catcher, Is.Not.Null);
                        Assert.That(victim, Is.Not.Null);
                        Assert.That(catcher.Frame.D?.frameId, Is.EqualTo(260));
                        Assert.That(victim.Frame.D?.frameId, Is.EqualTo(132));
                        catcher.CaughtSlotIndex = 1;
                        catcher.Runtime.CaughtDuration = 100;
                        catcher.AttackingCounter = 0;
                        catcher.Runtime.OwnerSlotIndex = -1;
                        victim.Runtime.CatchSourceSlot90 = 0;
                        victim.Runtime.CatcherSlotIndex = 0;
                        victim.Health.HP = initialHp;
                        victim.Runtime.HPBound = initialHp;
                        victim.Runtime.IncomingDamageScale340 = 100;
                        victim.Runtime.OrdinaryCreditGate2F4 = -1;

                        for (int tick = 1; tick <= 3; tick++)
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
                                initialHp.ToString(CultureInfo.InvariantCulture),
                                (catcher.Frame.D?.frameId ?? -1).ToString(CultureInfo.InvariantCulture),
                                (victim.Frame.D?.frameId ?? -1).ToString(CultureInfo.InvariantCulture),
                                victim.Health.HP.ToString(CultureInfo.InvariantCulture),
                                victim.Runtime.HPBound.ToString(CultureInfo.InvariantCulture),
                                catcher.CaughtSlotIndex.ToString(CultureInfo.InvariantCulture),
                                victim.Runtime.CatchSourceSlot90.ToString(CultureInfo.InvariantCulture),
                                catcher.AttackingCounter.ToString(CultureInfo.InvariantCulture),
                                catcher.Runtime.CaughtDuration.ToString(CultureInfo.InvariantCulture),
                                catcher.Runtime.InputScoreTotal348.ToString(CultureInfo.InvariantCulture),
                                catcher.Runtime.KnockoutCount358.ToString(CultureInfo.InvariantCulture),
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
                string directory = Path.GetFullPath(Root + "/unity");
                Directory.CreateDirectory(directory);
                string output = Path.Combine(directory, label + "-" +
                    DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff",
                        CultureInfo.InvariantCulture) + "-" +
                    Guid.NewGuid().ToString("N") + ".csv");
                File.WriteAllLines(output, observed);
                TestContext.Progress.WriteLine("Q08 held CPoint raw: " + output);
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
