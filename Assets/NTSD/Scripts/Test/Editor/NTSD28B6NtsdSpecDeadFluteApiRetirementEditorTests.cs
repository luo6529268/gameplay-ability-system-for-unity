#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6NtsdSpecDeadFluteApiRetirementEditorTests
    {
        private const string RetiredApiName = "FluteForce";
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [Test]
        public void LF2Entity_DoesNotExposeRetiredFluteForceMethod()
        {
            MethodInfo method = typeof(LF2Entity).GetMethod(
                RetiredApiName,
                InstanceMembers);

            Assert.That(
                method,
                Is.Null,
                "LF2Entity must not expose the retired NTSDSpec FluteForce API.");
        }

        [Test]
        public void LF2WeaponBase_DoesNotExposeRetiredFluteForceMethod()
        {
            MethodInfo method = typeof(LF2WeaponBase).GetMethod(
                RetiredApiName,
                InstanceMembers);

            Assert.That(
                method,
                Is.Null,
                "LF2WeaponBase must not expose the retired FluteForce API, " +
                "including an inherited base method.");
        }

        [Test]
        public void ProductionSources_ContainNoRetiredFluteForceReference()
        {
            string scriptsRoot = ProjectPath("Assets/NTSD/Scripts");
            string testRoot = Path.Combine(scriptsRoot, "Test") +
                Path.DirectorySeparatorChar;
            var offenders = new List<string>();

            foreach (string path in Directory.EnumerateFiles(
                         scriptsRoot,
                         "*.cs",
                         SearchOption.AllDirectories))
            {
                if (path.StartsWith(testRoot, StringComparison.OrdinalIgnoreCase))
                    continue;

                string source = File.ReadAllText(path);
                if (source.IndexOf(RetiredApiName, StringComparison.Ordinal) >= 0)
                    offenders.Add(RelativePath(scriptsRoot, path));
            }

            Assert.That(
                offenders,
                Is.Empty,
                "Production source still references the retired API: " +
                string.Join(", ", offenders));

            string entity = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs");
            string character = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs");

            Assert.That(entity, Does.Not.Contain("NTSDSpec.GetMassOrDefault"));
            Assert.That(
                Count(character, "NTSDSpec.GetMassOrDefault"),
                Is.Zero,
                "Q05 removed the retired mass carrier and initialization query.");
        }

        [Test]
        public void ImpactLivePaths_RemainIndependentOfRetiredFluteForceApi()
        {
            AssertImpactPath(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs",
                "DamageWriter.TryApplyNativeImpact",
                "DamageWriter.ApplyStandardCharacterDamage");
            AssertImpactPath(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs",
                "DamageWriter.TryApplyNativeImpact",
                "DamageWriter.ApplyStandardCharacterDamage");
            AssertImpactPath(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs",
                "DamageWriter.ApplyWeaponDamage");
            AssertImpactPath(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs",
                "CreateNativeImpactPlan");
            AssertImpactPath(
                "Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs",
                "ProjectNativeImpactWriterEffect");

            string weaponBase = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs");
            string weaponHit = Slice(
                weaponBase,
                "internal bool TryApplyHit(InteractionArea itr, LF2Entity target)",
                "internal bool HandlePreInteractionKind1(");
            Assert.That(weaponHit, Does.Not.Contain(RetiredApiName));
            Assert.That(
                weaponHit,
                Does.Contain("DamageWriter.TryApplyCurrentDatTargetHit"));
        }

        private static void AssertImpactPath(
            string relativePath,
            params string[] livePathAnchors)
        {
            string source = Source(relativePath);
            Assert.That(
                source,
                Does.Not.Contain(RetiredApiName),
                relativePath + " must not dispatch the retired API.");

            foreach (string anchor in livePathAnchors)
            {
                Assert.That(
                    source,
                    Does.Contain(anchor),
                    relativePath + " lost live impact owner anchor " + anchor);
            }
        }

        private static string Source(string relativePath)
        {
            return File.ReadAllText(ProjectPath(relativePath));
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }

        private static string RelativePath(string root, string path)
        {
            return path.Substring(root.Length + 1).Replace('\\', '/');
        }

        private static string Slice(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start, StringComparison.Ordinal);
            int endIndex = source.IndexOf(
                end,
                startIndex < 0 ? 0 : startIndex + start.Length,
                StringComparison.Ordinal);

            Assert.That(startIndex, Is.GreaterThanOrEqualTo(0), start);
            Assert.That(endIndex, Is.GreaterThan(startIndex), end);
            return source.Substring(startIndex, endIndex - startIndex);
        }

        private static int Count(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(
                       value,
                       offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }

            return count;
        }
    }
}
#endif
