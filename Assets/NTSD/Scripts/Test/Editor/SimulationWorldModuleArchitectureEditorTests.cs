#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class SimulationWorldModuleArchitectureEditorTests
    {
        private static readonly string[] PartialDeclarationAllowlist =
            Array.Empty<string>();

        private static readonly Regex PartialDeclarationPattern = new Regex(
            @"\bpartial\s+class\s+SimulationWorld\b",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex HistoricalPartialFilePattern = new Regex(
            @"^SimulationWorld(?:\..+)?\.partial\.cs$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly (string TypeName, string FileName)[]
            PhysicalModuleContracts =
            {
                (nameof(SimulationRegistryModule), "SimulationRegistryModule.cs"),
                (nameof(SimulationAiRuntime), "SimulationAiRuntime.cs"),
                (nameof(SimulationAiInputModule), "SimulationAiInputModule.cs"),
                (nameof(SimulationAiSensingModule), "SimulationAiSensingModule.cs"),
                (nameof(SimulationAiDecisionModule), "SimulationAiDecisionModule.cs"),
                (nameof(SimulationStageWaveModule), "SimulationStageWaveModule.cs"),
                (nameof(SimulationStageRenderModule), "SimulationStageRenderModule.cs"),
                (nameof(BattleOid5152RuntimeModule), "BattleOid5152RuntimeModule.cs"),
                (nameof(BattleRespawnModule), "BattleRespawnModule.cs"),
                (nameof(BattleEarlyFrameAdvanceModule), "BattleEarlyFrameAdvanceModule.cs"),
                (nameof(BattleLateEntityLifecycleModule), "BattleLateEntityLifecycleModule.cs"),
                (nameof(BattleInteractionPipeline), "BattleInteractionPipeline.cs"),
                (nameof(BattleRandomWeaponDropModule), "BattleRandomWeaponDropModule.cs"),
                (nameof(SimulationPassPipeline), "SimulationPassPipeline.cs"),
            };

        [Test]
        public void SimulationWorldPartialDeclarations_MatchShrinkingMigrationAllowlist()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string scriptsRoot = Path.Combine(
                projectRoot,
                "Assets",
                "NTSD",
                "Scripts");
            var actual = new List<string>();

            string[] files = Directory.GetFiles(
                scriptsRoot,
                "*.cs",
                SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string file = files[index];
                if (!PartialDeclarationPattern.IsMatch(File.ReadAllText(file)))
                    continue;

                actual.Add(NormalizeProjectPath(projectRoot, file));
            }

            actual.Sort(StringComparer.Ordinal);
            string[] expected = PartialDeclarationAllowlist
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(actual, Is.EqualTo(expected),
                "SimulationWorld partial declarations are migration debt. " +
                "The allowlist may only shrink; new partial files are forbidden.");
        }

        [Test]
        public void SimulationWorldHistoricalPartialFiles_AreRemoved()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string simulationRoot = Path.Combine(
                projectRoot,
                "Assets",
                "NTSD",
                "Scripts",
                "Simulation");

            string[] historicalPartialFiles = Directory.GetFiles(
                    simulationRoot,
                    "*.cs",
                    SearchOption.TopDirectoryOnly)
                .Where(path => HistoricalPartialFilePattern.IsMatch(
                    Path.GetFileName(path)))
                .Select(path => NormalizeProjectPath(projectRoot, path))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            Assert.That(historicalPartialFiles, Is.Empty,
                "SimulationWorld historical partial filenames must not be reintroduced.");
        }

        [Test]
        public void SimulationWorldChildModules_HaveDedicatedPhysicalFiles()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string simulationRoot = Path.Combine(
                projectRoot,
                "Assets",
                "NTSD",
                "Scripts",
                "Simulation");
            string worldPath = Path.Combine(simulationRoot, "SimulationWorld.cs");
            string worldSource = File.ReadAllText(worldPath);

            for (int index = 0; index < PhysicalModuleContracts.Length; index++)
            {
                (string typeName, string fileName) = PhysicalModuleContracts[index];
                string modulePath = Path.Combine(simulationRoot, fileName);
                Assert.That(File.Exists(modulePath), Is.True,
                    $"Module {typeName} must have dedicated file {fileName}.");

                string moduleSource = File.ReadAllText(modulePath);
                StringAssert.Contains($"class {typeName}", moduleSource,
                    $"Dedicated file {fileName} must declare {typeName}.");
                Assert.That(
                    Regex.IsMatch(
                        worldSource,
                        $@"\bclass\s+{Regex.Escape(typeName)}\b",
                        RegexOptions.CultureInvariant),
                    Is.False,
                    $"SimulationWorld.cs must not declare child module {typeName}.");
            }
        }

        [Test]
        public void SimulationWorldComposition_OwnsExistingExtractedModules()
        {
            AssertPrivateReadonlyField(
                "entityTraversal",
                typeof(SimulationEntityTraversal));
            AssertPrivateReadonlyField(
                "queryAndLinkModule",
                typeof(SimulationQueryAndLinkModule));
            AssertPrivateReadonlyField(
                "frameInputModule",
                typeof(SimulationFrameInputModule));
            AssertPrivateReadonlyField(
                "registryModule",
                typeof(SimulationRegistryModule));
            AssertPrivateReadonlyField(
                "aiRuntime",
                typeof(SimulationAiRuntime));
            AssertPrivateReadonlyField(
                "passPipeline",
                typeof(SimulationPassPipeline));
            AssertPrivateReadonlyField(
                typeof(SimulationPassPipeline),
                "oid5152RuntimeModule",
                typeof(BattleOid5152RuntimeModule));
            AssertPrivateReadonlyField(
                typeof(SimulationPassPipeline),
                "respawnModule",
                typeof(BattleRespawnModule));
            AssertPrivateReadonlyField(
                typeof(SimulationPassPipeline),
                "earlyFrameAdvanceModule",
                typeof(BattleEarlyFrameAdvanceModule));
            AssertPrivateReadonlyField(
                typeof(SimulationPassPipeline),
                "lateEntityLifecycleModule",
                typeof(BattleLateEntityLifecycleModule));
            AssertPrivateReadonlyField(
                typeof(SimulationPassPipeline),
                "interactionPipeline",
                typeof(BattleInteractionPipeline));
            AssertPrivateReadonlyField(
                typeof(SimulationPassPipeline),
                "randomWeaponDropModule",
                typeof(BattleRandomWeaponDropModule));
            AssertPrivateReadonlyField(
                "stageWaveModule",
                typeof(SimulationStageWaveModule));
            AssertPrivateReadonlyField(
                "stageRenderModule",
                typeof(SimulationStageRenderModule));
        }

        private static void AssertPrivateReadonlyField(
            string fieldName,
            Type expectedType)
        {
            AssertPrivateReadonlyField(
                typeof(SimulationWorld),
                fieldName,
                expectedType);
        }

        private static void AssertPrivateReadonlyField(
            Type ownerType,
            string fieldName,
            Type expectedType)
        {
            FieldInfo field = ownerType.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"{ownerType.Name} must own the extracted {fieldName} module.");
            Assert.That(field.FieldType, Is.EqualTo(expectedType));
            Assert.That(field.IsInitOnly, Is.True,
                $"{ownerType.Name} module field {fieldName} must be readonly.");
        }

        private static string NormalizeProjectPath(
            string projectRoot,
            string absolutePath)
        {
            string relative = absolutePath.Substring(projectRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return relative.Replace('\\', '/');
        }
    }
}
#endif
