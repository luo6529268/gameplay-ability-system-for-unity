#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Input;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28AiSampleProxyTwoPassEditorTests
    {
        [Test]
        public void CharacterInputAll_CompletesEveryProducerBeforeAnyRouting()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 100);
            RegisterCharacter(world, 1, 101);
            var order = new List<string>(4);
            world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(
                (_, entity) => order.Add($"P{entity.Runtime.SlotIndex}"));
            world.SetCharacterInputPassMutationOverrideForSelfCheck(
                (_, entity) => order.Add($"R{entity.Runtime.SlotIndex}"));

            world.CharacterInputAll(2);

            Assert.That(order, Is.EqualTo(new[] { "P0", "P1", "R0", "R1" }));
            Assert.That(world.LastNTSD28InputProducerFreezeCountForDiagnostics,
                Is.EqualTo(2));
        }

        [Test]
        public void LowerProxy_ConsumesHigherAiSameTickProducerAndRoutesMappedAction()
        {
            var world = new SimulationWorld();
            LF2Character target = RegisterCharacter(world, 0, 200, hitFa: 1);
            LF2Character source = RegisterCharacter(world, 1, 201);
            source.AiControlled = true;
            target.Runtime.InputProxyCounter14C = 2;
            target.Runtime.InputProxySourceSlot178 = 1;
            target.Runtime.InputProxyEnabled17C = 1;
            world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(
                (_, entity) =>
                {
                    if (!ReferenceEquals(entity, source))
                        return;

                    entity.Runtime.KeyRight = 1;
                    entity.Runtime.CdRight = 5;
                    entity.Runtime.ComboDra = 3;
                    entity.Runtime.NativeInputProxy.ProxyTail = 0x5A;
                });

            world.CharacterInputAll(2);

            Assert.That(target.Frame.N, Is.EqualTo(1));
            Assert.That(target.Runtime.Frame, Is.EqualTo(1));
            Assert.That(target.Runtime.NativeInputProxy.Current[3], Is.EqualTo(1));
            Assert.That(target.Runtime.NativeInputProxy.ProxyTail, Is.EqualTo(0x5A));
            Assert.That(world.LastNTSD28InputProxyCopyCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void ExactProxyCopy_CopiesAll33BytesWithoutControlFields()
        {
            var world = new SimulationWorld();
            LF2Character target = RegisterCharacter(world, 0, 300);
            LF2Character source = RegisterCharacter(world, 1, 301);
            target.Runtime.InputProxyCounter14C = 9;
            target.Runtime.InputProxySourceSlot178 = 1;
            target.Runtime.InputProxyEnabled17C = 1;
            world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(
                (_, entity) =>
                {
                    if (!ReferenceEquals(entity, source))
                        return;

                    FillLegacyProducer(entity.Runtime);
                    entity.Runtime.NativeInputProxy.ProxyTail = 0x6A;
                    entity.Runtime.InputProxyCounter14C = 4;
                    entity.Runtime.InputProxySourceSlot178 = 17;
                    entity.Runtime.InputProxyEnabled17C = 2;
                });

            world.CharacterInputAll(2);

            Assert.That(Serialize(target.Runtime.NativeInputProxy),
                Is.EqualTo(Serialize(source.Runtime.NativeInputProxy)));
            Assert.That(target.Runtime.InputProxyCounter14C, Is.EqualTo(9));
            Assert.That(target.Runtime.InputProxySourceSlot178, Is.EqualTo(1));
            Assert.That(target.Runtime.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [TestCase(false, 500, 0)]
        [TestCase(true, 0, 0)]
        [TestCase(true, 500, 1)]
        public void InvalidSource_DoesNotOverwriteTargetExactState(
            bool sourceExists,
            int sourceHp,
            int sourceType)
        {
            var world = new SimulationWorld();
            LF2Character target = RegisterCharacter(world, 0, 400);
            target.Runtime.NativeInputProxy.ProxyTail = 0x31;
            target.Runtime.InputProxyCounter14C = 3;
            target.Runtime.InputProxySourceSlot178 = 1;
            target.Runtime.InputProxyEnabled17C = 1;
            if (sourceExists)
            {
                LF2Character source = RegisterCharacter(world, 1, 401);
                source.Runtime.HP = sourceHp;
                source.Runtime.ObjType = sourceType;
                source.Runtime.NativeInputProxy.ProxyTail = 0x72;
            }

            world.CharacterInputAll(2);

            Assert.That(target.Runtime.NativeInputProxy.ProxyTail, Is.EqualTo(0x31));
            Assert.That(world.LastNTSD28InputProxyRejectCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void NestedProxy_UsesAscendingSecondPassVisibility()
        {
            var world = new SimulationWorld();
            LF2Character first = RegisterCharacter(world, 0, 500);
            LF2Character second = RegisterCharacter(world, 1, 501);
            LF2Character source = RegisterCharacter(world, 2, 502);
            first.Runtime.NativeInputProxy.ProxyTail = 0x10;
            second.Runtime.NativeInputProxy.ProxyTail = 0x11;
            source.Runtime.NativeInputProxy.ProxyTail = 0x22;
            ArmProxy(first.Runtime, 1);
            ArmProxy(second.Runtime, 2);

            world.CharacterInputAll(2);

            Assert.That(first.Runtime.NativeInputProxy.ProxyTail, Is.EqualTo(0x11));
            Assert.That(second.Runtime.NativeInputProxy.ProxyTail, Is.EqualTo(0x22));
            Assert.That(world.LastNTSD28InputProxyCopyCountForDiagnostics,
                Is.EqualTo(2));
        }

        private static void ArmProxy(NTSDEntityRuntime runtime, int sourceSlot)
        {
            runtime.InputProxyCounter14C = 2;
            runtime.InputProxySourceSlot178 = sourceSlot;
            runtime.InputProxyEnabled17C = 1;
        }

        private static void FillLegacyProducer(NTSDEntityRuntime runtime)
        {
            runtime.KeyUp = 1;
            runtime.KeyLeft = 1;
            runtime.KeyJump = 1;
            runtime.KeyAttack = 1;
            runtime.PrevDown = 1;
            runtime.PrevRight = 1;
            runtime.PrevDefend = 1;
            runtime.CdAttack = 10;
            runtime.CdJump = 11;
            runtime.CdDefend = 12;
            runtime.CdRight = 13;
            runtime.CdLeft = 14;
            runtime.CdUp = 15;
            runtime.CdDown = 16;
            runtime.CdDefendLock = 17;
            runtime.ComboDra = 3;
            runtime.ComboDuj = 3;
            runtime.ComboDda = 3;
            runtime.ComboDja = 3;
            runtime.NativeInputProxy.ComboState[6] = 1;
            runtime.NativeInputProxy.ComboState[7] = 1;
            runtime.NativeInputProxy.ComboState[9] = 1;
        }

        private static byte[] Serialize(NTSD28InputProxyBlock block)
        {
            var result = new byte[NTSD28InputProxyBlock.SerializedByteCount];
            block.WriteSerialized(result);
            return result;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int hitFa = 0)
        {
            LF2Character character = CreateCharacter(slot, objectId, hitFa);
            world.Register(character);
            return character;
        }

        private static LF2Character CreateCharacter(
            int slot,
            int objectId,
            int hitFa)
        {
            var root = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
                hit_Fa = hitFa,
            };
            var target = new LF2FrameData
            {
                frameId = 1,
                state = 3,
                wait = 100,
                next = 1,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"NTSD28TwoPass_{slot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { root, target },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(slot);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            return character;
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
