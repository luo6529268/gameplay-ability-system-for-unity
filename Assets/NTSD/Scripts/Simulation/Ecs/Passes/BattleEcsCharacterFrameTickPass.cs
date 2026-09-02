using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterFrameTickPassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    public readonly struct BattleEcsCharacterFrameTickPassDiagnostics
    {
        internal BattleEcsCharacterFrameTickPassDiagnostics(
            BattleEcsCharacterFrameTickPassMode mode,
            long runCount,
            long exactCharacterCount,
            long compatibilityFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            ExactCharacterCount = exactCharacterCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
        }

        public BattleEcsCharacterFrameTickPassMode Mode { get; }
        public long RunCount { get; }
        public long ExactCharacterCount { get; }
        public long CompatibilityFallbackCount { get; }
    }

    /// <summary>
    /// Owns the authority-ordered FrameTick orchestration for exact characters.
    /// Unknown derived entities and non-character DAT shells retain the virtual
    /// compatibility path.
    /// </summary>
    internal sealed class BattleEcsCharacterFrameTickPass
    {
        private BattleEcsCharacterFrameTickPassMode mode =
            BattleEcsCharacterFrameTickPassMode.DataOriented;
        private long runCount;
        private long exactCharacterCount;
        private long compatibilityFallbackCount;

        internal BattleEcsCharacterFrameTickPassMode Mode => mode;

        internal BattleEcsCharacterFrameTickPassDiagnostics Diagnostics =>
            new BattleEcsCharacterFrameTickPassDiagnostics(
                mode,
                runCount,
                exactCharacterCount,
                compatibilityFallbackCount);

        internal void SetMode(BattleEcsCharacterFrameTickPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        internal void Reset()
        {
            ResetDiagnostics();
        }

        internal bool TryExecute(LF2Entity entity)
        {
            runCount++;
            if (mode == BattleEcsCharacterFrameTickPassMode.Legacy ||
                entity == null ||
                entity.GetType() != typeof(LF2Character))
            {
                compatibilityFallbackCount++;
                return false;
            }

            var character = (LF2Character)entity;
            if (character.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                compatibilityFallbackCount++;
                return false;
            }

            exactCharacterCount++;
            ExecuteExactCharacter(character);
            return true;
        }

        private static void ExecuteExactCharacter(LF2Character character)
        {
            if (character.FrameDelay != 0)
                return;

            if (character.AttackExempt > 0)
                character.AttackExempt--;

            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime == null || runtime.LinkState < 0)
                return;

            LF2FrameData frame = character.Frame?.D;
            if (frame == null ||
                (frame.HasPrimaryCatchPoint &&
                 frame.PrimaryCatchPoint.Kind == 2))
                return;

            character.RunReleaseFrameTickCounters();

            int waitCounter = character.Trans?.WaitCounter ?? 0;
            if ((character.Frame?.N ?? 0) != waitCounter)
            {
                character.OnFrameTickFrameChangedFromWaitCounter();
                character.AttackingCounter = 0;
            }

            character.AttackingCounter++;

            int state = frame.state;
            bool suppressJumpInit = false;
            if (state == 0 && runtime.YInt < 0)
            {
                character.SetFrameTickImmediateRawDirect(212);
                suppressJumpInit = true;
                frame = character.Frame?.D;
                if (frame == null)
                    return;
                state = frame.state;
            }

            if (state == LF2States.Lying &&
                character.Health != null &&
                character.Health.HP <= 0)
            {
                bool canArmHitStop =
                    character.KillCount >= 0 ||
                    character.RelationTeam == 5 ||
                    runtime.SlotIndex >= 20;
                if (canArmHitStop && character.HitStun <= 0)
                    character.HitStun = 30;
                character.AttackingCounter = 0;
            }

            // Alignment contract: R8-MOV-005-001
            if (state == LF2States.HeavyWeaponInSky)
                character.SwitchDir(runtime.Vx > 0.0 ? "right" : "left");

            int wait = character.Trans?.Wait ?? frame.wait;
            if (character.AttackingCounter > wait)
            {
                int next = character.Trans?.Next ?? frame.next;
                character.AttackingCounter = 0;
                if (next != 0)
                {
                    bool allowJumpInit = true;
                    int targetFrame = next;
                    if (targetFrame == 999)
                    {
                        bool to212 = runtime.YInt != 0;
                        targetFrame = to212 ? 212 : 0;
                        suppressJumpInit = to212;
                        allowJumpInit = false;
                    }
                    else if (targetFrame < 0)
                    {
                        targetFrame = -targetFrame;
                        character.SwitchDir(
                            runtime.Dir == "left" ? "right" : "left");
                    }

                    int previousFrame = waitCounter;
                    character.SetFrameTickImmediateRawDirect(targetFrame);
                    int frameAfterTransit = character.Frame?.N ?? targetFrame;
                    if (frameAfterTransit < 0 ||
                        frameAfterTransit >= LF2FrameCache.MaxFrameIdExclusive ||
                        character.Frame?.D == null)
                    {
                        return;
                    }

                    character.ApplyCaughtExitHitStopForWorldPass(previousFrame);
                    if (frameAfterTransit == 212 &&
                        allowJumpInit &&
                        !suppressJumpInit)
                    {
                        character.ApplyFrame212JumpInitForWorldPass();
                    }

                    character.ApplyFrameTickPpDisplayForWorldPass();
                }
            }

            int currentFrame = character.Frame?.N ?? -1;
            if (currentFrame == 110 || currentFrame == 114)
            {
                SimulationWorld world = character.RegisteredWorldForSimulation;
                if (world != null)
                    world.CharacterInputWriter.SetDefendLock(runtime, 3);
                else
                    runtime.CdDefendLock = 3;
            }

            if (currentFrame == 202)
                character.HitStun = 20;

            LF2FrameData currentData = character.Frame?.D;
            if (currentData != null)
                character.Trans?.SyncWaitCounterFrame(currentFrame);
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            exactCharacterCount = 0;
            compatibilityFallbackCount = 0;
        }
    }
}
