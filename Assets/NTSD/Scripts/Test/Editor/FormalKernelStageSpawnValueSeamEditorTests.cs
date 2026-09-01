#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FormalKernelStageSpawnValueSeam")]
    public sealed class FormalKernelStageSpawnValueSeamEditorTests
    {
        [Test]
        public void ValuePreservesExactEightScalarsAndIsImmutable()
        {
            var value = new BattleStageSpawnValue(
                id: 122,
                act: 7,
                hp: 345,
                times: 6,
                x: -1000,
                y: -20,
                ratio: 1.75,
                join: 9);

            Assert.That(value.Id, Is.EqualTo(122));
            Assert.That(value.Act, Is.EqualTo(7));
            Assert.That(value.Hp, Is.EqualTo(345));
            Assert.That(value.Times, Is.EqualTo(6));
            Assert.That(value.X, Is.EqualTo(-1000));
            Assert.That(value.Y, Is.EqualTo(-20));
            Assert.That(value.Ratio, Is.EqualTo(1.75));
            Assert.That(value.Join, Is.EqualTo(9));

            PropertyInfo[] properties = typeof(BattleStageSpawnValue).GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(properties, Has.Length.EqualTo(8));
            for (int index = 0; index < properties.Length; index++)
                Assert.That(properties[index].CanWrite, Is.False, properties[index].Name);
        }

        [Test]
        public void LoaderDtoProjectionPreservesExactScalars()
        {
            var source = new BattleStageSpawnData
            {
                Id = 123,
                Act = 4,
                Hp = 501,
                Times = 3,
                X = 88,
                Y = -33,
                Ratio = 2.5,
                Join = 1,
            };

            BattleStageSpawnValue value = source.ToValue();

            Assert.That(value, Is.EqualTo(new BattleStageSpawnValue(
                123,
                4,
                501,
                3,
                88,
                -33,
                2.5,
                1)));
        }

        [Test]
        public void TaskMappingFromValueIsExactAndWarmedAllocationFree()
        {
            var configurator = new StageSpawnTaskConfigurator();
            var task = new OPointCreateTask();
            var value = new BattleStageSpawnValue(124, 11, 700, 2, 5, 6, 0.0, 3);

            configurator.Configure(
                task,
                value,
                spawnX: 101,
                spawnY: -21,
                spawnZ: 222,
                facingDir: "left",
                requiredRuntimeSlot: 37);

            Assert.That(task.opoint.oid, Is.EqualTo(124));
            Assert.That(task.opoint.action, Is.EqualTo(11));
            Assert.That(task.opoint.x, Is.EqualTo(101));
            Assert.That(task.opoint.y, Is.EqualTo(-21));
            Assert.That(task.z, Is.EqualTo(222));
            Assert.That(task.dir, Is.EqualTo("left"));
            Assert.That(task.requiredRuntimeSlot, Is.EqualTo(37));

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                configurator.Configure(
                    task,
                    value,
                    101,
                    -21,
                    222,
                    "left",
                    37);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void StageWaveModuleHasNoMutableSpawnDtoScratch()
        {
            Type moduleType = typeof(SimulationWorld).Assembly.GetType(
                "NTSD.Simulation.SimulationStageWaveModule",
                throwOnError: true);
            FieldInfo[] fields = moduleType.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            for (int index = 0; index < fields.Length; index++)
            {
                Assert.That(fields[index].FieldType,
                    Is.Not.EqualTo(typeof(BattleStageSpawnData)),
                    fields[index].Name);
            }
        }
    }
}
#endif
