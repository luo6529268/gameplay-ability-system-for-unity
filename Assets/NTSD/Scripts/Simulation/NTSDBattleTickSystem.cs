namespace NTSD.Simulation
{
    /// <summary>
    /// Unity NTSD 战斗 tick 调度器。
    /// pass 顺序以 C++ release 工程为基准；实体专属行为保留在 LF2Entity 子类中，
    /// 本类只负责集中维护这些 pass 的执行时机。
    /// </summary>
    public sealed class NTSDBattleTickSystem
    {
        private readonly SimulationWorld world;

        public NTSDBattleTickSystem(SimulationWorld world)
        {
            this.world = world;
        }

        public void RunReleaseTick(int tickIndex)
        {
            if (world == null) return;

            world.AdvanceInputPhase();
            RunFrameAdvancePhase(tickIndex);
            RunInteractionPhase(tickIndex);
            RunPresentationAndCleanupPhase(tickIndex);
        }

        private void RunFrameAdvancePhase(int tickIndex)
        {
            TickCooldowns(tickIndex);
            PostCooldownHumanInput(tickIndex);
            Oid5152RuntimeMaintenance(tickIndex);
            AiInputAndCombo(tickIndex);
            EarlyFrameAdvanceSpecials(tickIndex);
            FrameLogicBeforeAdvance(tickIndex);
            FrameAdvanceAll(tickIndex);
            PostFrameAdvanceDeathCleanup(tickIndex);
            ProcessHeldObjects(tickIndex);
            CaptureCollisionFrameSnapshots();
            CollectCollisionCandidates();
        }

        private void RunInteractionPhase(int tickIndex)
        {
            ResolvePostInteractions(tickIndex);
            RandomWeaponDrop(tickIndex);
            ResolveObjectInteractions(tickIndex);
            EndCollisionCandidateConsumption();
            ResolvePreInteractions(tickIndex);
            ValidateHeldLinks(tickIndex);
            ProcessHeldObjects(tickIndex);
        }

        private void RunPresentationAndCleanupPhase(int tickIndex)
        {
            ClampCharacterZToStageBounds();
            ValidateHeldObjectsAfterClamp(tickIndex);
            PreFrameBounds();
            CurrentWaveStage(tickIndex);
            RenderDispatch(tickIndex);
            FramePostProcess();
            LateEntityUpdate(tickIndex);
            Mode2RandomWeaponDropTail(tickIndex);
            EntityPostFrameTail(tickIndex);
        }

        private void TickCooldowns(int tickIndex)
        {
            world.VrestTickAll(tickIndex);
        }

        private void PostCooldownHumanInput(int tickIndex)
        {
            world.PostCooldownHumanInputAll(tickIndex);
        }

        private void AiInputAndCombo(int tickIndex)
        {
            world.AiInputAndComboAll(tickIndex);
        }

        private void ProcessHeldObjects(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
        }

        private void Oid5152RuntimeMaintenance(int tickIndex)
        {
            world.Oid5152RuntimeMaintenanceAll(tickIndex);
        }

        private void CaptureCollisionFrameSnapshots()
        {
            world.CaptureCollisionFrameSnapshotsAll();
        }

        private void CollectCollisionCandidates()
        {
            world.CollectCollisionCandidatesAll();
        }

        private void EndCollisionCandidateConsumption()
        {
            world.EndCollisionCandidateConsumption();
        }

        private void FrameLogicBeforeAdvance(int tickIndex)
        {
            world.FrameLogicBeforeAdvanceAll(tickIndex);
        }

        private void EarlyFrameAdvanceSpecials(int tickIndex)
        {
            world.EarlyFrameAdvanceSpecialsAll(tickIndex);
        }

        private void ResolvePreInteractions(int tickIndex)
        {
            world.PreInteractionTickAll(tickIndex);
        }

        private void FrameAdvanceAll(int tickIndex)
        {
            world.SerialTickAll(tickIndex);
        }

        private void PostFrameAdvanceDeathCleanup(int tickIndex)
        {
            world.PostFrameAdvanceDeathCleanupAll(tickIndex);
        }

        private void RandomWeaponDrop(int tickIndex)
        {
            world.RandomWeaponDropTickAll(tickIndex);
        }

        private void ResolvePostInteractions(int tickIndex)
        {
            world.PostInteractionTickAll(tickIndex);
        }

        private void ResolveObjectInteractions(int tickIndex)
        {
            world.ObjectInteractionTickAll(tickIndex);
        }

        private void ValidateHeldLinks(int tickIndex)
        {
            world.ValidateHeldLinksAll(tickIndex);
        }

        private void ClampCharacterZToStageBounds()
        {
            world.ClampCharacterZToStageBoundsAll();
        }

        private void ValidateHeldObjectsAfterClamp(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
        }

        private void FramePostProcess()
        {
            world.FramePostProcessAll();
        }

        private void CurrentWaveStage(int tickIndex)
        {
            world.CurrentWaveStageTickAll();
        }

        private void RenderDispatch(int tickIndex)
        {
            world.RenderDispatchAll(tickIndex);
        }

        private void PreFrameBounds()
        {
            world.ApplyPreFrameBoundsAll();
        }

        private void LateEntityUpdate(int tickIndex)
        {
            world.LateEntityUpdateAll(tickIndex);
        }

        private void Mode2RandomWeaponDropTail(int tickIndex)
        {
            world.Mode2RandomWeaponDropTailAll(tickIndex);
        }

        private void EntityPostFrameTail(int tickIndex)
        {
            world.EntityPostFrameTailAll(tickIndex);
        }
    }
}
