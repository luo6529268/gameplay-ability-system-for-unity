#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q04MassFrictionGateEditorTests
    {
        [TestCase(0f)]
        [TestCase(-2f)]
        [TestCase(1f)]
        public void CoreMatchesNativeRegardlessOfReservedMass(float mass)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../artifacts/diagnostics/NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001/native.tsv"));
            string[] rows = File.ReadAllLines(path);
            Assert.That(rows.Length, Is.EqualTo(9));
            foreach (string row in rows)
            {
                string[] values = row.Split('\t');
                if (values[0] == "id") continue;
                string id = values[0];
                var runtime = new NTSDEntityRuntime();
                double y = id == "airborne" ? -5 : id == "landing" ? -1 :
                    id == "negative_floor" ? -10 : id == "integer_snapshot" ? -0.5 : 0;
                runtime.CollisionYReference = id == "negative_floor" ? -10 : 0;
                runtime.SetPosition(0, y, 0);
                runtime.SetVelocity(id == "reverse" ? -3 : id == "small" ? 0.25 : 5,
                    id == "landing" ? 2 : 0,
                    id == "reverse" ? 3 : id == "small" ? -0.25 : -5);
                runtime.SyncIntegerPosition();
                runtime.XBoundPositive = id == "blocked";
                runtime.ZBoundNegative = id == "blocked";
                var context = new CharacterMechanicsContext(runtime, null, 0, mass, 0, 1.7);
                new CharacterMechanics().StepBattleLogic(context);
                double[] actual = { runtime.X, runtime.Y, runtime.Z, runtime.Vx, runtime.Vy, runtime.Vz };
                for (int field = 0; field < actual.Length; field++)
                {
                    // Native step also resolves landing; Unity does that in its caller.
                    // This kernel's just-landed step must preserve pre-resolution Vx/Vy.
                    if (id == "landing" && (field == 3 || field == 4))
                    {
                        Assert.That(actual[field], Is.EqualTo(field == 3 ? 5.0 : 2.0), id);
                        continue;
                    }
                    Assert.That(actual[field], Is.EqualTo(double.Parse(values[field + 1], CultureInfo.InvariantCulture)),
                        id + "/mass=" + mass + "/field=" + field);
                }
                Assert.That(runtime.XBoundPositive || runtime.ZBoundNegative, Is.False, id);
            }
        }

        [TestCase(0f, false)]
        [TestCase(-2f, false)]
        [TestCase(1f, false)]
        [TestCase(0f, true)]
        [TestCase(-2f, true)]
        [TestCase(1f, true)]
        public void RealCharacterAndEcsUseSameGroundedRule(float mass, bool ecs)
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(world, 0);
            try
            {
                typeof(LF2Character).GetField("_mass", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(character, mass);
                character.Runtime.SetPosition(0, 0, 0);
                character.Runtime.SetVelocity(5, 0, -5);
                character.Runtime.SyncIntegerPosition();
                if (ecs)
                {
                    var pass = new BattleEcsCharacterFrameAdvancePass(world);
                    Assert.That(pass.TryExecute(character, 1), Is.True);
                    Assert.That(pass.Diagnostics.ExactCharacterCount, Is.EqualTo(1));
                }
                else character.ApplyDynamics();
                Assert.That((character.Runtime.X, character.Runtime.Z), Is.EqualTo((5.0, -5.0)));
                Assert.That((character.Runtime.Vx, character.Runtime.Vz), Is.EqualTo((4.0, -4.0)));
                Assert.That(character.MassForFrameAdvance, Is.EqualTo(mass), "Q05 carrier must remain in this package");
            }
            finally { world.Unregister(character); }
        }

        internal static LF2Character CreateCharacter(SimulationWorld world, int slot)
        {
            var frame = new LF2FrameData { frameId = 0, state = 0, wait = 100, next = 0 };
            var data = new LF2CharacterData
            {
                name = "Q04Mass", type_sub = 0, frames = new List<LF2FrameData> { frame }
            };
            var character = new LF2Character { Name = data.name, ObjectId = 0 };
            character.ModuleInitialize();
            character.FrameCache.Load(new LF2CharacterDataWrapper(0, data));
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.D = frame;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            character.FrameDelay = 0;
            return character;
        }
    }
}
#endif
