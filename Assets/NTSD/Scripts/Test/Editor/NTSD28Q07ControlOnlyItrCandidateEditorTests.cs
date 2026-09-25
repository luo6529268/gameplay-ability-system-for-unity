#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28Q07ControlOnlyItrCandidate")]
    public sealed class NTSD28Q07ControlOnlyItrCandidateEditorTests
    {
        [TestCase("c/chi/pup5.dat", "E2483952B8C4390A93A957533382948FEA5DB9952D430AF849BCA6EE615581B4", 407, 400, 300, 361, 270, 1)]
        [TestCase("c/kon/ang.dat", "CD5380DD4934FB51BE095841BADF7C218181D2D3547414C65DAF4409938E9F29", 11, 400, 300, 504, 400, 0)]
        public void FormalControlOnlyItrDoesNotEnterCandidateRow(
            string relativePath,
            string expectedSha,
            int frameId,
            int attackerX,
            int attackerY,
            int targetX,
            int targetY,
            int controlOrdinal)
        {
            LF2FrameData frame = ReadFormalFrame(relativePath, expectedSha, frameId);
            Assert.That(frame.itrs, Has.Count.EqualTo(controlOrdinal + 1));
            Assert.That(frame.itrs[controlOrdinal].kind, Is.EqualTo(100100));
            Assert.That(frame.itrs[controlOrdinal].hasGeometry, Is.False);

            var candidateCounts = new List<int>();
            var observedItrOrdinals = new List<int>();
            foreach (CollisionFormalCollectorMode mode in new[]
            {
                CollisionFormalCollectorMode.ForceBruteForce,
                CollisionFormalCollectorMode.ForceRoleAware
            })
            {
                var world = new SimulationWorld();
                LF2Character attacker = null;
                LF2Character target = null;
                try
                {
                    attacker = Create(frame, frameId, 8900);
                    target = Create(frame, frameId, 8901);
                    Register(world, attacker, 0, 1, attackerX, attackerY);
                    Register(world, target, 1, 2, targetX, targetY);

                    var query = (BruteForceSceneQuery)world.SceneQuery;
                    query.FormalCollectorMode = mode;
                    query.ForceRoleAwareDirectForDiagnostics = true;
                    query.CollisionCandidateStoreAuthorityEnabled = true;
                    world.Rng.Seed(0x2847u);
                    world.CaptureCollisionFrameSnapshotsAll();
                    world.CollectCollisionCandidatesAll();

                    Assert.That(query.TryGetCollisionCandidateSequence(attacker,
                        out List<SceneQueryHit> sequence), Is.True, relativePath + "/" + mode);
                    Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                        attacker.Runtime.SlotIndex, attacker,
                        out RuntimeEntityHandle handle), Is.True);
                    Assert.That(query.TryGetCollisionCandidateStoreRowForSelfCheck(
                        handle, out int storeCount), Is.True);
                    Assert.That(storeCount, Is.EqualTo(sequence.Count));
                    candidateCounts.Add(sequence.Count);
                    for (int index = 0; index < sequence.Count; index++)
                        observedItrOrdinals.Add(sequence[index].ItrIndex);
                }
                finally
                {
                    world.EndCollisionCandidateConsumption();
                    if (target != null) world.Unregister(target);
                    if (attacker != null) world.Unregister(attacker);
                }
            }
            Assert.That(candidateCounts, Is.EqualTo(new[] { 0, 0 }),
                relativePath + ": control-only ordinal " + controlOrdinal +
                ", observed ITR ordinals=" + string.Join(",", observedItrOrdinals));
        }

        [Test]
        public void CompleteDegenerateGeometryRemainsAdmissible()
        {
            MethodInfo predicate = typeof(BruteForceSceneQuery).GetMethod(
                "IsReleaseItrGeometry", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(predicate, Is.Not.Null);
            var completeZeroWidth = new InteractionArea
            {
                kind = 0,
                x = 0,
                y = 0,
                w = 0,
                h = 79,
                hasGeometry = true
            };
            var completeControlKind = new InteractionArea
            {
                kind = 100100,
                x = 0,
                y = 0,
                w = 1,
                h = 1,
                hasGeometry = true
            };
            Assert.That(predicate.Invoke(null, new object[] { completeZeroWidth }), Is.EqualTo(true));
            Assert.That(predicate.Invoke(null, new object[] { completeControlKind }), Is.EqualTo(true));
        }

        [TestCase("CheckZeroDimensionCollisionGeometry")]
        [TestCase("CheckNegativeHeightCollisionGeometry")]
        public void CompleteSyntheticGeometrySelfCheckRemainsValid(string methodName)
        {
            MethodInfo check = typeof(BattleRuntimeSelfCheck).GetMethod(
                methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(check, Is.Not.Null);
            Assert.DoesNotThrow(() => check.Invoke(null, null), methodName);
        }

        private static LF2FrameData ReadFormalFrame(string relativePath, string expectedSha, int frameId)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath,
                "NTSD/Content/LoganRuntime/decoded_dat", relativePath));
            byte[] bytes = File.ReadAllBytes(path);
            using (SHA256 sha = SHA256.Create())
            {
                string actualSha = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
                Assert.That(actualSha, Is.EqualTo(expectedSha), relativePath);
            }

            Lf2DatFile document = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(path), path);
            foreach (Lf2FrameBlock block in document.Frames)
            {
                if (block.FrameIndex == frameId)
                    return Lf2DatConverter.ConvertLoganFrameData(block);
            }
            Assert.Fail(relativePath + " missing frame " + frameId);
            return null;
        }

        private static LF2Character Create(LF2FrameData frame, int frameId, int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "Q07ControlItr" + objectId,
                type_sub = 0,
                frames = new List<LF2FrameData> { frame }
            };
            var entity = new TestCharacter { Name = data.name, ObjectId = objectId };
            entity.ModuleInitialize();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(frameId);
            entity.Frame.N = frameId;
            entity.Frame.PN = frameId;
            entity.Initialize(500, 500);
            entity.FrameDelay = 0;
            return entity;
        }

        private static void Register(SimulationWorld world, LF2Character entity,
            int slot, int team, int x, int y)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.Dir = "right";
            entity.Runtime.SetPosition(x, y, 250);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class TestCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }
    }
}
#endif
