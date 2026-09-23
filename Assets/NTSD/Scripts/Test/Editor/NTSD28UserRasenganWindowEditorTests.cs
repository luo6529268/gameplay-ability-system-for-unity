#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.Animation;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28UserRasenganWindowEditorTests
    {
        private const string RuntimeRoot =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string DiagnosticDirectory =
            "Temp/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001";

        [TestCase(23, 2)]
        [TestCase(25, 2)]
        [TestCase(26, 2)]
        public void FullRasenganWindowRecordsPhysicalAttackBoundary(
            int attackStartScenarioTick, int heldTicks)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string directory = Path.Combine(projectRoot, DiagnosticDirectory);
            string seedScenario = Path.Combine(
                directory, "rasengan-unity-seed-3tick.scenario.json");
            string outputPath = Path.Combine(
                directory,
                $"rasengan-unity-t{attackStartScenarioTick}-h{heldTicks}.raw.jsonl");
            Assert.That(File.Exists(seedScenario), Is.True, seedScenario);
            Directory.CreateDirectory(directory);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                RuntimeRoot, seedScenario, BattleRuntimeProfile.Authority400, 32,
                (driver, unusedFrames, identity) =>
                {
                    LF2ObjectPool pool = LF2ObjectPool.Instance;
                    if (!pool.IsRuntimeStateValidForAcceptance)
                    {
                        typeof(LF2ObjectPool).GetMethod(
                            "Awake",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.Invoke(pool, null);
                    }
                    Assert.That(pool.IsRuntimeStateValidForAcceptance, Is.True);
                    LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
                    pool.BeginBattlePreparation();
                    factory.BeginBattlePreparation();
                    // The shared diagnostic helper materializes its roster before capturing service owners.
                    typeof(SimulationTickDriver).GetField(
                        "_battleObjectPool",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(driver, pool);
                    typeof(SimulationTickDriver).GetField(
                        "_battleObjectPointFactory",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(driver, factory);
                    driver.World.SetLogicOnlyEntityMaterialization(true);
                    using var writer = new StreamWriter(
                        outputPath, false, new UTF8Encoding(false));
                    for (int scenarioTick = 0; scenarioTick < 32; scenarioTick++)
                    {
                        SimulationInputButtons attack =
                            scenarioTick >= attackStartScenarioTick &&
                            scenarioTick < attackStartScenarioTick + heldTicks
                                ? SimulationInputButtons.Jump
                                : SimulationInputButtons.None;
                        var input = new FrameInputSet(
                            scenarioTick + 1,
                            new[]
                            {
                                new SimulationPlayerInput(0, attack),
                                new SimulationPlayerInput(1, SimulationInputButtons.None),
                            });
                        try
                        {
                            Assert.That(driver.StepOneTick(input, true, false), Is.True,
                                "completed tick " + (scenarioTick + 1));
                        }
                        catch (Exception exception)
                        {
                            File.WriteAllText(
                                outputPath + ".error.txt",
                                "completed tick " + (scenarioTick + 1) +
                                Environment.NewLine + exception,
                                new UTF8Encoding(false));
                            throw;
                        }
                        writer.WriteLine(NTSD28UnityEntityRawCapture.CaptureTickJson(
                            driver.World, scenarioTick + 1));
                    }
                    Assert.That(driver.CurrentTickIndex, Is.EqualTo(32));
                    BattleRuntimeShutdownReport shutdown =
                        driver.ShutdownBattleRuntime();
                    File.WriteAllText(
                        outputPath + ".shutdown.txt",
                        "status=" + shutdown.Status + Environment.NewLine +
                        "stage=" + shutdown.CompletedStage + Environment.NewLine +
                        "reason=" + shutdown.FailureReason + Environment.NewLine +
                        "objects=" + shutdown.RemainingWorldObjects + Environment.NewLine +
                        "slots=" + shutdown.RemainingRuntimeSlots + Environment.NewLine +
                        "borrowers=" + shutdown.RemainingPoolBorrowers,
                        new UTF8Encoding(false));
                });

            Assert.That(new FileInfo(outputPath).Length, Is.GreaterThan(0));
        }
    }
}
#endif
