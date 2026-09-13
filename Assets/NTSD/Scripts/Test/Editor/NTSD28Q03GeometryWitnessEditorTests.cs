#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q03GeometryWitnessEditorTests
    {
        [Test]
        public void CaptureSharedDatThroughProductionCollectors()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../artifacts/diagnostics/NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001"));
            string fixtures = Path.Combine(root, "fixtures");
            string[] rows = File.ReadAllLines(Path.Combine(fixtures, "cases.tsv"));
            var output = new StringBuilder("id\tmode\tcandidates\n");
            int captured = 0;
            foreach (string row in rows)
            {
                if (row.StartsWith("id\t", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(row))
                    continue;
                string[] values = row.Split('\t');
                Assert.That(values.Length, Is.EqualTo(3));
                string id = values[0];
                foreach (CollisionFormalCollectorMode mode in new[]
                {
                    CollisionFormalCollectorMode.ForceBruteForce,
                    CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                    CollisionFormalCollectorMode.ForceRoleAware
                })
                {
                    var world = new SimulationWorld();
                    LF2Character attacker = null;
                    LF2Character target = null;
                    try
                    {
                        attacker = Create(Path.Combine(fixtures, id, "attacker.dat"), 8900);
                        target = Create(Path.Combine(fixtures, id, "target.dat"), 8901);
                        Register(world, attacker, 0, 1, 0, 0);
                        Register(world, target, 1, 2, int.Parse(values[1]), int.Parse(values[2]));
                        var query = (BruteForceSceneQuery)world.SceneQuery;
                        query.FormalCollectorMode = mode;
                        query.ForceRoleAwareDirectForDiagnostics = true;
                        world.Rng.Seed(0x2847u);
                        world.CaptureCollisionFrameSnapshotsAll();
                        world.CollectCollisionCandidatesAll();
                        Assert.That(query.TryGetCollisionCandidateSequence(attacker,
                            out List<SceneQueryHit> sequence), Is.True, id + "/" + mode);
                        output.Append(id).Append('\t').Append(mode).Append('\t')
                            .Append(sequence.Count).Append('\n');
                        captured++;
                    }
                    finally
                    {
                        world.EndCollisionCandidateConsumption();
                        if (target != null) world.Unregister(target);
                        if (attacker != null) world.Unregister(attacker);
                    }
                }
            }
            Assert.That(captured, Is.GreaterThan(0));
            File.WriteAllText(Path.Combine(root, "unity.tsv"), output.ToString());
            // A successful capture is not a parity assertion: compare native.tsv separately.
        }

        private static LF2Character Create(string path, int objectId)
        {
            Lf2DatFile document = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(path), path);
            Assert.That(document.Frames.Count, Is.EqualTo(1));
            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(document.Frames[0]);
            var data = new LF2CharacterData
            {
                name = "Q03Geometry" + objectId,
                type_sub = 0,
                frames = new List<LF2FrameData> { frame }
            };
            var entity = new TestCharacter { Name = data.name, ObjectId = objectId };
            entity.ModuleInitialize();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Initialize(500, 500);
            entity.FrameDelay = 0;
            return entity;
        }

        private static void Register(SimulationWorld world, LF2Character entity,
            int slot, int team, int x, int z)
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
            entity.Runtime.SetPosition(x, 0, z);
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
