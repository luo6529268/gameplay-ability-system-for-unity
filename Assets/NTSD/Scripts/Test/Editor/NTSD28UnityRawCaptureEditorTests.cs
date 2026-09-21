using System;
using System.IO;
using System.Linq;
using NTSD.Animation;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.EditorTools.Tests
{
    public sealed class NTSD28UnityRawCaptureEditorTests
    {
        private const string OutputRoot =
            "Temp/NTSD28UnityTrace/FocusedTests";

        [NUnit.Framework.Test]
        public void OriginalAuthorityScenarioFailsClosedForMissingUnityOid99()
        {
            string output = ProjectPath(
                $"{OutputRoot}/missing-oid99.raw.jsonl");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                    NTSD28UnityRawCaptureEditor.OriginalAuthorityScenario,
                    output));

            StringAssert.Contains(
                "missing required object ids: 99",
                exception.Message);
            Assert.That(File.Exists(output), Is.False);
        }

        [NUnit.Framework.Test]
        public void CommonScenarioWritesThreeCompletedTicksWithTwoRealEntities()
        {
            string output = ProjectPath($"{OutputRoot}/common.raw.jsonl");
            int driverCount = UnityEngine.Object.FindObjectsOfType<
                SimulationTickDriver>(true).Length;
            int dataManagerCount = UnityEngine.Object.FindObjectsOfType<
                GameDataManager>(true).Length;
            int animationManagerCount = UnityEngine.Object.FindObjectsOfType<
                CharacterAnimtorManager>(true).Length;

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.DefaultScenario,
                output);

            string[] lines = File.ReadAllLines(output);
            Assert.That(lines, Has.Length.EqualTo(4));
            StringAssert.Contains(
                $"\"schema\":\"{NTSD28UnityRawCaptureEditor.CaptureSchema}\"",
                lines[0]);
            StringAssert.Contains("\"certificateEligible\":false", lines[0]);
            StringAssert.Contains(
                "\"formalAuthorityExeSha256\":\"B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033\"",
                lines[0]);
            StringAssert.Contains(
                $"\"evidenceClass\":\"{NTSD28UnityRawCaptureEditor.EvidenceClass}\"",
                lines[0]);
            StringAssert.Contains("\"verifiedCount\":50", lines[0]);
            StringAssert.Contains("\"candidateCount\":0", lines[0]);
            StringAssert.Contains("\"missingCount\":0", lines[0]);
            StringAssert.Contains("\"reviveLives\":1", lines[1]);
            StringAssert.Contains("\"reviveNextLives\":0", lines[1]);
            StringAssert.Contains("\"reviveNextHp\":0", lines[1]);
            StringAssert.Contains("\"hitReactionTimer\":0", lines[1]);
            StringAssert.Contains("\"attackerRest\":0", lines[1]);
            StringAssert.Contains("\"motionHoldTimer\":0", lines[1]);
            StringAssert.Contains("\"weaponHp\":0", lines[1]);

            for (int tick = 1; tick <= 3; tick++)
            {
                TickEnvelope envelope =
                    JsonUtility.FromJson<TickEnvelope>(lines[tick]);
                Assert.That(envelope.completedTick, Is.EqualTo(tick));
                Assert.That(envelope.entities, Has.Length.EqualTo(2));
                Assert.That(
                    envelope.entities.Select(entity => entity.slot).ToArray(),
                    Is.EqualTo(new[] { 0, 1 }));
                Assert.That(
                    envelope.entities.Select(entity => entity.identity.objectId)
                        .ToArray(),
                    Is.EqualTo(new[] { 2, 7 }));
                Assert.That(
                    envelope.entities.Select(entity => entity.frame.frameCounter)
                        .ToArray(),
                    Is.EqualTo(new[] { tick, tick }));
            }

            Assert.That(
                UnityEngine.Object.FindObjectsOfType<SimulationTickDriver>(true)
                    .Length,
                Is.EqualTo(driverCount));
            Assert.That(
                UnityEngine.Object.FindObjectsOfType<GameDataManager>(true).Length,
                Is.EqualTo(dataManagerCount));
            Assert.That(
                UnityEngine.Object.FindObjectsOfType<CharacterAnimtorManager>(true)
                    .Length,
                Is.EqualTo(animationManagerCount));
        }

        [NUnit.Framework.Test]
        public void RenderPhaseScenario_ProjectsHitStopIndependentlyFromY()
        {
            string output = ProjectPath(
                $"{OutputRoot}/render-phase.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.RenderPhaseScenario,
                output);

            string[] lines = File.ReadAllLines(output);
            Assert.That(lines, Has.Length.EqualTo(4));
            int[][] expectedPhases =
            {
                new[] { 4, -4 },
                new[] { 3, -3 },
                new[] { 2, -2 },
            };
            for (int tick = 1; tick <= 3; tick++)
            {
                TickEnvelope envelope =
                    JsonUtility.FromJson<TickEnvelope>(lines[tick]);
                Assert.That(
                    envelope.entities.Select(entity => entity.combat.renderPhase)
                        .ToArray(),
                    Is.EqualTo(expectedPhases[tick - 1]));
            }

            TickEnvelope first = JsonUtility.FromJson<TickEnvelope>(lines[1]);
            Assert.That(first.entities[0].position.y, Is.Zero);
            Assert.That(first.entities[0].combat.renderPhase, Is.EqualTo(4));
            Assert.That(first.entities[1].position.y, Is.EqualTo(-11));
            Assert.That(first.entities[1].combat.renderPhase, Is.EqualTo(-4));
        }

        [NUnit.Framework.Test]
        public void CommonScenarioProducesDeterministicCanonicalRawCapture()
        {
            string first = ProjectPath($"{OutputRoot}/deterministic-a.raw.jsonl");
            string second = ProjectPath($"{OutputRoot}/deterministic-b.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.DefaultScenario,
                first);
            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.DefaultScenario,
                second);

            Assert.That(
                File.ReadAllText(second),
                Is.EqualTo(File.ReadAllText(first)));
        }

        [NUnit.Framework.Test]
        public void InputScenarioWritesExactAppliedMasksAndHonestRngAvailability()
        {
            string entityOutput = ProjectPath(
                $"{OutputRoot}/input-common.raw.jsonl");
            string domainOutput = ProjectPath(
                $"{OutputRoot}/input-common.domain.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.InputScenario,
                entityOutput,
                domainOutput);

            string[] entityLines = File.ReadAllLines(entityOutput);
            string[] domainLines = File.ReadAllLines(domainOutput);
            Assert.That(entityLines, Has.Length.EqualTo(4));
            Assert.That(domainLines, Has.Length.EqualTo(4));
            StringAssert.Contains(
                $"\"schema\":\"{NTSD28UnityRawCaptureEditor.DomainCaptureSchema}\"",
                domainLines[0]);
            StringAssert.Contains(
                "\"authorityCrt\":\"missing\"",
                domainLines[0]);
            StringAssert.Contains(
                "\"authoritySynchronized\":\"missing\"",
                domainLines[0]);
            StringAssert.Contains(
                "\"unityDeterministic\":\"available\"",
                domainLines[0]);
            StringAssert.Contains(
                "\"players\":[{\"heldMask\":17,\"playerSlot\":0}," +
                "{\"heldMask\":2,\"playerSlot\":1}]",
                domainLines[1]);
            StringAssert.Contains(
                "\"players\":[{\"heldMask\":1,\"playerSlot\":0}," +
                "{\"heldMask\":96,\"playerSlot\":1}]",
                domainLines[2]);
            StringAssert.Contains(
                "\"players\":[{\"heldMask\":0,\"playerSlot\":0}," +
                "{\"heldMask\":12,\"playerSlot\":1}]",
                domainLines[3]);
            StringAssert.Contains(
                "\"authorityCrt\":{\"availability\":\"missing\"," +
                "\"calls\":null,\"state\":null,\"tickCallCount\":null," +
                "\"totalCalls\":null}",
                domainLines[1]);
            StringAssert.Contains(
                "\"unityDeterministic\":{\"availability\":\"available\"," +
                "\"calls\":null",
                domainLines[1]);
            StringAssert.Contains(
                "\"occupants\":[{\"allocationEpoch\":1,\"objectId\":2," +
                "\"slot\":0},{\"allocationEpoch\":1,\"objectId\":7," +
                "\"slot\":1}]",
                domainLines[1]);
            StringAssert.Contains("\"events\":[]", domainLines[1]);
        }

        [NUnit.Framework.Test]
        public void InputScenarioWritesVersionedExactInputAndNativeRngRaw()
        {
            string entityOutput = ProjectPath(
                $"{OutputRoot}/input-b2-entity.raw.jsonl");
            string domainOutput = ProjectPath(
                $"{OutputRoot}/input-b2-domain.raw.jsonl");
            string inputRngOutput = ProjectPath(
                $"{OutputRoot}/input-b2-exact.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.InputScenario,
                entityOutput,
                domainOutput,
                inputRngOutput);

            string[] lines = File.ReadAllLines(inputRngOutput);
            Assert.That(lines, Has.Length.EqualTo(4));
            StringAssert.Contains(
                $"\"schema\":\"{NTSD28UnityRawCaptureEditor.InputRngCaptureSchema}\"",
                lines[0]);
            StringAssert.Contains("\"producer\":\"unity-diagnostic\"", lines[0]);
            StringAssert.Contains(
                "\"crt\":\"available-completed-ticks\"",
                lines[0]);
            StringAssert.Contains(
                "\"synchronized\":\"available-completed-ticks\"",
                lines[0]);
            StringAssert.Contains("\"aiCursorExcluded\":false", lines[0]);
            StringAssert.Contains("\"initializationExcluded\":true", lines[0]);
            StringAssert.Contains("\"state\":1758127634", lines[0]);
            StringAssert.Contains(
                "\"tableHash64\":\"A1BA1B90EA55796D\"",
                lines[0]);
            StringAssert.Contains("\"counter\":1", lines[0]);
            StringAssert.Contains("\"index\":1", lines[0]);
            StringAssert.Contains("\"lastCallSite\":4202976", lines[0]);
            StringAssert.Contains("\"totalCalls\":1", lines[0]);
            StringAssert.Contains("\"inputPhase\":1", lines[1]);
            StringAssert.Contains("\"currentMask\":0", lines[1]);
            StringAssert.Contains("\"inputPhase\":0", lines[2]);
            StringAssert.Contains("\"currentMask\":1", lines[2]);
            StringAssert.Contains("\"currentMask\":96", lines[2]);
            StringAssert.Contains(
                "\"remapIndices\":[0,1,2,3,4,5,6]",
                lines[1]);
            StringAssert.Contains("\"comboState\":[", lines[1]);
            StringAssert.Contains("\"keyHistory\":[", lines[1]);
            StringAssert.Contains("\"tableHash64\":", lines[1]);
            StringAssert.Contains("\"calls\":[]", lines[1]);
        }

        [NUnit.Framework.Test]
        public void StandingAttackScenarioWritesExactDirectSite82Call()
        {
            string entityOutput = ProjectPath(
                $"{OutputRoot}/standing-attack-entity.raw.jsonl");
            string domainOutput = ProjectPath(
                $"{OutputRoot}/standing-attack-domain.raw.jsonl");
            string inputRngOutput = ProjectPath(
                $"{OutputRoot}/standing-attack-exact.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.StandingAttackRngScenario,
                entityOutput,
                domainOutput,
                inputRngOutput);

            string[] lines = File.ReadAllLines(inputRngOutput);
            Assert.That(lines, Has.Length.EqualTo(4));
            StringAssert.Contains("\"tickCallCount\":0", lines[1]);
            StringAssert.Contains("\"tickCallCount\":1", lines[2]);
            StringAssert.Contains("\"callSite\":130", lines[2]);
            StringAssert.Contains("\"counterAfter\":2", lines[2]);
            StringAssert.Contains("\"indexAfter\":2", lines[2]);
            StringAssert.Contains("\"totalCalls\":2", lines[2]);
            StringAssert.Contains("\"upperBound\":2", lines[2]);
            StringAssert.Contains("\"tickCallCount\":0", lines[3]);
        }

        [NUnit.Framework.Test]
        public void AiScenarioWritesAuthorityAcceptedCallsFromFirstCompletedTick()
        {
            string entityOutput = ProjectPath(
                $"{OutputRoot}/ai-input-entity.raw.jsonl");
            string domainOutput = ProjectPath(
                $"{OutputRoot}/ai-input-domain.raw.jsonl");
            string inputRngOutput = ProjectPath(
                $"{OutputRoot}/ai-input-exact.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.AiRngScenario,
                entityOutput,
                domainOutput,
                inputRngOutput);

            string[] lines = File.ReadAllLines(inputRngOutput);
            Assert.That(lines, Has.Length.EqualTo(4));
            StringAssert.Contains("\"aiCursorExcluded\":false", lines[0]);
            StringAssert.Contains("\"tickCallCount\":6", lines[1]);
            StringAssert.Contains(
                "\"keyHistory\":[-1,-1,-1,-1,4]",
                lines[1]);
            StringAssert.Contains("\"callSite\":20", lines[1]);
            StringAssert.Contains("\"callSite\":60", lines[1]);
            StringAssert.Contains("\"callSite\":29", lines[1]);
            StringAssert.Contains("\"callSite\":30", lines[1]);
            StringAssert.Contains("\"callSite\":31", lines[1]);
            StringAssert.Contains("\"callSite\":56", lines[1]);
            StringAssert.Contains("\"tickCallCount\":7", lines[2]);
            Assert.That(
                System.Text.RegularExpressions.Regex.Matches(
                    lines[2],
                    "\"previousMask\":0").Count,
                Is.EqualTo(2));
            Assert.That(
                System.Text.RegularExpressions.Regex.Matches(
                    lines[2],
                    "\"runAccumulator\":0").Count,
                Is.EqualTo(2));
            StringAssert.Contains(
                "\"keyHistory\":[-1,-1,-1,4,4]",
                lines[2]);
            StringAssert.Contains("\"tickCallCount\":8", lines[3]);
        }

        [NUnit.Framework.Test]
        public void InputScenarioProducesDeterministicEntityAndDomainRaw()
        {
            string firstEntity = ProjectPath(
                $"{OutputRoot}/input-deterministic-a.raw.jsonl");
            string firstDomain = ProjectPath(
                $"{OutputRoot}/input-deterministic-a.domain.raw.jsonl");
            string secondEntity = ProjectPath(
                $"{OutputRoot}/input-deterministic-b.raw.jsonl");
            string secondDomain = ProjectPath(
                $"{OutputRoot}/input-deterministic-b.domain.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.InputScenario,
                firstEntity,
                firstDomain);
            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.InputScenario,
                secondEntity,
                secondDomain);

            Assert.That(
                File.ReadAllText(secondEntity),
                Is.EqualTo(File.ReadAllText(firstEntity)));
            Assert.That(
                File.ReadAllText(secondDomain),
                Is.EqualTo(File.ReadAllText(firstDomain)));
        }

        [NUnit.Framework.Test]
        public void PhysicalActionKeysEnterExistingCrossedFrameInputContract()
        {
            Assert.That(
                NTSD28UnityRawCaptureEditor.ParsePhysicalInputKeyForTests("J"),
                Is.EqualTo(SimulationInputButtons.Jump));
            Assert.That(
                NTSD28UnityRawCaptureEditor.ParsePhysicalInputKeyForTests("K"),
                Is.EqualTo(SimulationInputButtons.Defend));
            Assert.That(
                NTSD28UnityRawCaptureEditor.ParsePhysicalInputKeyForTests("L"),
                Is.EqualTo(SimulationInputButtons.Attack));
        }

        [NUnit.Framework.Test]
        public void InputScenarioPhysicalJumpDefendMatchesAuthorityDefendAction()
        {
            string entityOutput = ProjectPath(
                $"{OutputRoot}/input-physical-kl.raw.jsonl");

            NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                NTSD28UnityRawCaptureEditor.InputScenario,
                entityOutput);

            string[] lines = File.ReadAllLines(entityOutput);
            TickEnvelope completedTick2 =
                JsonUtility.FromJson<TickEnvelope>(lines[2]);
            EntityEnvelope slot1 = completedTick2.entities.Single(
                entity => entity.slot == 1);
            Assert.That(slot1.frame.action, Is.EqualTo(110));
            Assert.That(slot1.frame.frameState, Is.EqualTo(7));
        }

        [NUnit.Framework.Test]
        public void UnknownInputKeyFailsClosedBeforeWritingOutputs()
        {
            string scenario = ProjectPath(
                $"{OutputRoot}/invalid-input-key.scenario.json");
            string entityOutput = ProjectPath(
                $"{OutputRoot}/invalid-input-key.raw.jsonl");
            string domainOutput = ProjectPath(
                $"{OutputRoot}/invalid-input-key.domain.raw.jsonl");
            Directory.CreateDirectory(Path.GetDirectoryName(scenario));
            File.WriteAllText(
                scenario,
                File.ReadAllText(ProjectPath(
                    NTSD28UnityRawCaptureEditor.InputScenario))
                    .Replace("\"D\", \"J\"", "\"Q\", \"J\""));
            if (File.Exists(entityOutput))
                File.Delete(entityOutput);
            if (File.Exists(domainOutput))
                File.Delete(domainOutput);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => NTSD28UnityRawCaptureEditor.RunScenarioForTests(
                    scenario,
                    entityOutput,
                    domainOutput));

            StringAssert.Contains("unknown key: Q", exception.Message);
            Assert.That(File.Exists(entityOutput), Is.False);
            Assert.That(File.Exists(domainOutput), Is.False);
        }

        private static string ProjectPath(string path)
        {
            return Path.GetFullPath(
                Path.Combine(Environment.CurrentDirectory, path));
        }

        [Serializable]
        private sealed class TickEnvelope
        {
            public int completedTick;
            public EntityEnvelope[] entities;
        }

        [Serializable]
        private sealed class EntityEnvelope
        {
            public int slot;
            public IdentityEnvelope identity;
            public FrameEnvelope frame;
            public CombatEnvelope combat;
            public PositionEnvelope position;
        }

        [Serializable]
        private sealed class IdentityEnvelope
        {
            public int objectId;
        }

        [Serializable]
        private sealed class FrameEnvelope
        {
            public int frameCounter;
            public int action;
            public int frameState;
        }

        [Serializable]
        private sealed class CombatEnvelope
        {
            public int renderPhase;
        }

        [Serializable]
        private sealed class PositionEnvelope
        {
            public int y;
        }
    }
}
