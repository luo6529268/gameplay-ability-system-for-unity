#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeType0BuiltinProductionIntegrationEditorTests
    {
        [Test]
        public void NativeSecondPass_RoutesThreeButtonThenState5BuiltinBeforeProjection()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2CharacterData data = Data(
                Frame(0, 0, hitAttack: 5),
                Frame(5, 5), Frame(40, 3), Frame(90, 3, 25),
                Frame(213, 5), Frame(214, 5),
                Frame(216, 5), Frame(217, 5), Frame(305, 3));
            LF2CharacterData linkedData = Data(Frame(0, 0));
            linkedData.jump_attack = 305;
            LF2Character character = CreateCharacter(data, 0, 860);
            LF2Character linked = CreateCharacter(linkedData, 1, 861);
            world.Register(character);
            world.Register(linked);
            character.Runtime.LinkState = 101;
            character.Runtime.TargetSlotIndex = 1;
            character.SwitchDir("right");
            character.Runtime.Vx = 5.0;
            character.Runtime.Vy = -2.0;
            character.Runtime.AnimSub = 17;
            character.Health.PP = 200;
            character.Runtime.KeyJump = 1;

            world.CharacterInputAll(2);

            Assert.That(character.Frame.N, Is.EqualTo(305));
            Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(5));
            Assert.That(character.Runtime.Vy, Is.EqualTo(-3.0));
            Assert.That(character.Runtime.AnimSub, Is.EqualTo(16));
            Assert.That(character.Runtime.CdAttack, Is.Zero);
            world.Unregister(linked);
            world.Unregister(character);
        }

        [Test]
        public void NativeSecondPass_Action215ReentersState5WithoutLegacyAnimReset()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2CharacterData data = Data(
                Frame(102, 3), Frame(213, 5), Frame(214, 5),
                Frame(215, 15), Frame(216, 5), Frame(217, 5));
            data.dash_distance = 15f;
            data.dash_height = -13f;
            data.dash_distancez = 3.75f;
            LF2Character character = CreateCharacter(data, 0, 862, 215);
            world.Register(character);
            character.SwitchDir("right");
            character.Runtime.AnimSub = 17;
            character.Runtime.AttackingCounter = 9;
            character.Runtime.InputLastAction144 = 77;
            character.Runtime.KeyRight = 1;
            character.Runtime.KeyDefend = 1;

            world.CharacterInputAll(2);

            Assert.That(character.Frame.N, Is.EqualTo(213));
            Assert.That(character.Runtime.Vx, Is.EqualTo(15.0));
            Assert.That(character.Runtime.Vy, Is.EqualTo(-13.0));
            Assert.That(character.Runtime.Vz, Is.EqualTo(0.0));
            Assert.That(character.Runtime.AnimSub, Is.EqualTo(16));
            Assert.That(character.Runtime.AttackingCounter, Is.EqualTo(9));
            Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(77));
            world.Unregister(character);
        }

        [Test]
        public void NativeSecondPass_RowingUsesRawCostAndHonorsEnvironmentGate()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2CharacterData data = Data(
                Frame(100, 6, 25), Frame(108, 6, 30),
                Frame(182, 12), Frame(188, 12));
            data.rowing_height = -2f;
            data.rowing_distance = 30f;
            LF2Character character = CreateCharacter(data, 0, 863, 182);
            world.Register(character);
            character.SwitchDir("right");
            character.Runtime.Vx = 0.5;
            character.Runtime.Vy = 3.0;
            character.Runtime.EnvironmentState320 = 0;
            character.Health.PP = 200;
            character.Runtime.KeyDefend = 1;

            world.CharacterInputAll(2);

            Assert.That(character.Frame.N, Is.EqualTo(108));
            Assert.That(character.Health.PP, Is.EqualTo(175));
            Assert.That(character.Runtime.InputMpConsumedTotal350, Is.Zero);
            Assert.That(character.Runtime.Vx, Is.EqualTo(-30.0));
            Assert.That(character.Runtime.Vy, Is.EqualTo(-2.0));

            SetFrame(character, 182);
            character.Runtime.PrevDefend = 0;
            character.Runtime.KeyDefend = 1;
            character.Runtime.EnvironmentState320 = -1;
            character.Runtime.Vx = 0.5;
            character.Runtime.Vy = 3.0;
            character.Runtime.AttackingCounter = 9;
            character.Health.PP = 200;

            world.CharacterInputAll(3);

            Assert.That(character.Frame.N, Is.EqualTo(182));
            Assert.That(character.Health.PP, Is.EqualTo(200));
            Assert.That(character.Runtime.AttackingCounter, Is.EqualTo(9));
            world.Unregister(character);
        }

        [Test]
        public void NativeSecondPass_DeadType0StopsBeforeBuiltinRouting()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = CreateCharacter(
                Data(Frame(0, 0), Frame(60, 3), Frame(65, 3)),
                0,
                869);
            world.Register(character);
            character.Runtime.HP = 0;
            character.Health.HP = 0;
            character.Runtime.AnimSub = 17;
            character.Runtime.KeyJump = 1;

            world.CharacterInputAll(2);

            Assert.That(character.Frame.N, Is.Zero);
            Assert.That(character.Runtime.AnimSub, Is.EqualTo(17));
            Assert.That(character.Runtime.KeyJump, Is.Zero);
            Assert.That(character.Runtime.CdAttack, Is.Zero);
            world.Unregister(character);
        }

        [Test]
        public void NativeProfile_CharacterResolverDoesNotRunLegacyRelease()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = CreateCharacter(
                Data(Frame(0, 0), Frame(60, 3), Frame(65, 3)),
                0,
                864);
            world.Register(character);
            character.Runtime.KeyJump = 1;
            character.Runtime.CdAttack = 5;
            character.InputState.SyncFromRuntime(character.Runtime);

            bool resolved = world.CharacterInputActionResolver
                .ApplyFrameInputFromRuntimeProgress(
                    character,
                    world.CharacterInputWriter);

            Assert.That(resolved, Is.False);
            Assert.That(character.Frame.N, Is.Zero);
            world.Unregister(character);
        }

        [Test]
        public void NativeProfile_SharedCharacterDatShellDoesNotRunLegacyActions()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2OtherObject shell = CreateSharedShell(
                Data(Frame(0, 0), Frame(60, 3), Frame(65, 3)),
                0,
                865);
            world.Register(shell);
            shell.Runtime.KeyJump = 1;
            shell.Runtime.CdAttack = 5;

            shell.RunCharacterInputRoutingPhaseForKnownCharacterDat(2);

            Assert.That(shell.Frame.N, Is.Zero);
            world.Unregister(shell);
        }

        [Test]
        public void LegacyProfile_RetainsCharacterAndSharedShellReleasePaths()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.LegacyCanonical);
            LF2CharacterData data = Data(
                Frame(0, 0), Frame(60, 3), Frame(65, 3));
            LF2Character character = CreateCharacter(data, 0, 866);
            LF2OtherObject shell = CreateSharedShell(data, 1, 867);
            world.Register(character);
            world.Register(shell);
            character.Runtime.KeyJump = 1;
            character.Runtime.CdAttack = 5;
            character.InputState.SyncFromRuntime(character.Runtime);
            shell.Runtime.KeyJump = 1;
            shell.Runtime.CdAttack = 5;

            bool resolved = world.CharacterInputActionResolver
                .ApplyFrameInputFromRuntimeProgress(
                    character,
                    world.CharacterInputWriter);
            shell.RunCharacterInputRoutingPhaseForKnownCharacterDat(2);

            Assert.That(resolved, Is.True);
            Assert.That(
                character.Frame.N,
                Is.EqualTo(60).Or.EqualTo(65),
                "Legacy LF2Character release path");
            Assert.That(
                shell.Frame.N,
                Is.EqualTo(60).Or.EqualTo(65),
                "Legacy shared Character-DAT shell path");
            world.Unregister(shell);
            world.Unregister(character);
        }

        [Test]
        public void WarmNativeProductionInputRoute_AllocatesZeroManagedBytes()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = CreateCharacter(
                Data(Frame(0, 86)),
                0,
                868);
            world.Register(character);
            character.Runtime.AnimSub = 17;
            for (int tick = 0; tick < 128; tick++)
                world.CharacterInputAll(tick + 2);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 0; tick < 512; tick++)
                world.CharacterInputAll(tick + 130);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(character.Frame.N, Is.Zero);
            world.Unregister(character);
        }

        private static LF2CharacterData Data(params LF2FrameData[] frames)
        {
            return new LF2CharacterData
            {
                name = "NativeBuiltinProductionIntegration",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>(frames),
            };
        }

        private static LF2FrameData Frame(
            int id,
            int state,
            int mp = 0,
            int hitAttack = 0)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
                mp = mp,
                hit_a = hitAttack,
            };
        }

        private static LF2Character CreateCharacter(
            LF2CharacterData data,
            int slot,
            int objectId,
            int initialFrame = -1)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            int frameId = initialFrame >= 0
                ? initialFrame
                : data.frames[0].frameId;
            character.WriteCurrentFrameId(frameId);
            character.Frame.D = character.FrameCache.GetFrameDataById(frameId);
            character.Frame.PN = frameId;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(slot);
            character.Team = slot + 1;
            character.RelationTeam = slot + 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            character.PS.groundY = 0f;
            return character;
        }

        private static LF2OtherObject CreateSharedShell(
            LF2CharacterData data,
            int slot,
            int objectId)
        {
            var shell = new SharedCharacterDatShell
            {
                Name = data.name,
                ObjectId = objectId,
            };
            shell.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            shell.WriteCurrentFrameId(data.frames[0].frameId);
            shell.Frame.D = data.frames[0];
            shell.Frame.PN = data.frames[0].frameId;
            shell.SetRequiredRuntimeSlot(slot);
            shell.Team = slot + 1;
            shell.RelationTeam = slot + 1;
            shell.Runtime.ObjType = 0;
            shell.Runtime.HP = 500;
            shell.Runtime.HP3 = 500;
            shell.Runtime.HPBound = 500;
            shell.Health.HP = 500;
            shell.Health.HP3 = 500;
            shell.Health.HPBound = 500;
            shell.Health.PP = 500;
            return shell;
        }

        private static void SetFrame(LF2Character character, int frameId)
        {
            Assert.That(character.WriteNativeInputActionUnchecked(frameId), Is.True);
            Assert.That(character.Frame.D, Is.Not.Null);
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

        private sealed class SharedCharacterDatShell : LF2OtherObject
        {
            public override int ReleaseEntityType =>
                (int)LF2ObjectType.Character;
        }
    }
}
#endif
