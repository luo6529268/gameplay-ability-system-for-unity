using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleRuntimeStructureGuardEditorTests
    {
        private static readonly HashSet<string> RemainingSimulationWorldPartialFiles =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "SimulationWorld.AiDecisionShadow.partial.cs",
                "SimulationWorld.AiInput.partial.cs",
                "SimulationWorld.AiSoaShadow.partial.cs",
                "SimulationWorld.cs",
                "SimulationWorld.Passes.partial.cs",
                "SimulationWorld.Registry.partial.cs",
            };

        [Test]
        public void PartialDeclarations_AreLimitedToTheShrinkingMigrationAllowlist()
        {
            string scriptsRoot = Path.Combine(Application.dataPath, "NTSD", "Scripts");
            string[] sourceFiles = Directory.GetFiles(
                scriptsRoot,
                "*.cs",
                SearchOption.AllDirectories);
            var actualFiles = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < sourceFiles.Length; index++)
            {
                string source = File.ReadAllText(sourceFiles[index]);
                if (!Regex.IsMatch(source, @"\bpartial\s+(?:class|struct)\b"))
                    continue;

                string fileName = Path.GetFileName(sourceFiles[index]);
                actualFiles.Add(fileName);
            }

            CollectionAssert.AreEquivalent(
                RemainingSimulationWorldPartialFiles,
                actualFiles,
                "Do not add partial declarations. Remove an allowlist entry whenever a " +
                "SimulationWorld responsibility is moved behind an owned module.");
        }

        [Test]
        public void SimulationRuntime_DoesNotIntroduceMutableStaticFields()
        {
            string simulationRoot = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Scripts",
                "Simulation");
            string[] sourceFiles = Directory.GetFiles(
                simulationRoot,
                "*.cs",
                SearchOption.AllDirectories);
            var violations = new List<string>();
            var mutableStaticField = new Regex(
                @"^\s*(?:public|internal|protected|private)?\s*static\s+" +
                @"(?!readonly\b|class\b)[^\r\n\(=;]+\s+" +
                @"[A-Za-z_][A-Za-z0-9_]*\s*(?:=(?!>)|;)",
                RegexOptions.Multiline);

            for (int index = 0; index < sourceFiles.Length; index++)
            {
                string source = File.ReadAllText(sourceFiles[index]);
                MatchCollection matches = mutableStaticField.Matches(source);
                for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                {
                    string declaration = matches[matchIndex].Value.Trim();
                    if (declaration.Contains(" operator ", StringComparison.Ordinal))
                        continue;

                    string relativePath = sourceFiles[index]
                        .Replace('\\', '/')
                        .Replace(Application.dataPath.Replace('\\', '/') + "/", string.Empty);
                    violations.Add(relativePath + ": " + declaration);
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Simulation state must belong to a world/module instance. Static constants, " +
                "readonly tables and stateless methods remain allowed.");
        }

        [Test]
        public void BattleRuntimeCallbacks_DoNotUseAsyncVoidFrameMethods()
        {
            string scriptsRoot = Path.Combine(Application.dataPath, "NTSD", "Scripts");
            string[] sourceFiles = Directory.GetFiles(
                scriptsRoot,
                "*.cs",
                SearchOption.AllDirectories);
            var violations = new List<string>();
            var callbackPattern = new Regex(
                @"\basync\s+void\s+(?:Update|LateUpdate|FixedUpdate)\s*\(",
                RegexOptions.Multiline);

            for (int index = 0; index < sourceFiles.Length; index++)
            {
                string normalizedPath = sourceFiles[index].Replace('\\', '/');
                if (normalizedPath.Contains("/Editor/", StringComparison.Ordinal) ||
                    normalizedPath.Contains("/Test/", StringComparison.Ordinal) ||
                    normalizedPath.Contains("/Gen/", StringComparison.Ordinal))
                {
                    continue;
                }

                string source = File.ReadAllText(sourceFiles[index]);
                if (!callbackPattern.IsMatch(source))
                    continue;

                string relativePath = normalizedPath.Replace(
                    Application.dataPath.Replace('\\', '/') + "/",
                    string.Empty);
                violations.Add(relativePath);
            }

            Assert.That(
                violations,
                Is.Empty,
                "Per-frame async void callbacks hide state-machine allocations and exceptions. " +
                "Use a synchronous no-work fast path and explicitly own any pending UniTask.");
        }
    }
}
