using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28UnityEntityRawCaptureEditorTests
    {
        [NUnit.Framework.Test]
        public void CaptureTickJson_ProjectsBoundValuesAndKeepsMissingNull()
        {
            var world = new SimulationWorld();
            var entity = new LF2OtherObject();
            entity.SetRequiredRuntimeSlot(0);
            world.Register(entity);

            NTSDEntityRuntime runtime = entity.Runtime;
            runtime.ObjectId = 99;
            runtime.ObjType = 0;
            runtime.Team = 11;
            runtime.RelationTeam = 13;
            runtime.OwnerSlotIndex = 17;
            runtime.OwnerStableId = 23;
            runtime.SpawnerSlotIndex = 19;
            runtime.RelationOwnerSlotIndex = 21;
            runtime.Unk344 = 4;
            runtime.AnimCounter = 5;
            runtime.Frame = 7;
            entity.Frame.Prev = 9;
            runtime.WaitCounter = 8;
            runtime.PrevFrame2 = 6;
            runtime.FrameWaitCounter = 93;
            runtime.AttackingCounter = 3;
            runtime.Dir = "left";
            runtime.SetPosition(560.25, -2.5, 650.75);
            runtime.SyncIntegerPosition();
            runtime.SetVelocity(1.25, -0.5, 0.75);
            runtime.HP = 450;
            runtime.HPBound = 480;
            runtime.HP3 = 500;
            runtime.MP = 200;
            runtime.PP = 173;
            runtime.MPMax = 500;
            runtime.HP2Orig = 4;
            runtime.HPOrig = 7;
            runtime.RespawnCount = 320;
            runtime.AttackExempt = 5;
            runtime.HitStop = 4;
            runtime.Fall = 60;
            runtime.Bdefend = 9;
            runtime.FrameDelay = 3;
            runtime.WeaponFlightCounter = 37;
            runtime.SpecialHitLatch0EB = true;
            runtime.Unk328 = -3;
            runtime.PendingFlushDestroy = true;

            string first = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);
            string second = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(first, Does.Contain("\"fieldCount\":49"));
            Assert.That(first, Does.Contain("\"verifiedCount\":43"));
            Assert.That(first, Does.Contain("\"candidateCount\":0"));
            Assert.That(first, Does.Contain("\"missingCount\":6"));
            Assert.That(first, Does.Contain("\"allocationEpoch\":1"));
            Assert.That(first, Does.Contain("\"objectId\":99"));
            Assert.That(first, Does.Contain("\"battleGroup\":13"));
            Assert.That(first, Does.Not.Contain("\"battleGroup\":11"));
            Assert.That(first, Does.Contain("\"participantClass\":4"));
            Assert.That(first, Does.Contain("\"ownerSlot\":17"));
            Assert.That(first, Does.Not.Contain("\"ownerSlot\":19"));
            Assert.That(first, Does.Not.Contain("\"ownerSlot\":21"));
            Assert.That(first, Does.Not.Contain("\"ownerSlot\":23"));
            Assert.That(first, Does.Contain("\"controlSlot\":5"));
            Assert.That(first, Does.Contain("\"action\":7"));
            Assert.That(first, Does.Contain("\"actionLatch\":8"));
            Assert.That(first, Does.Contain("\"previousAction\":9"));
            Assert.That(first, Does.Contain("\"tickActionSnapshot\":6"));
            Assert.That(first, Does.Contain("\"frameCounter\":3"));
            Assert.That(first, Does.Not.Contain("\"frameCounter\":93"));
            Assert.That(first, Does.Contain("\"preciseX\":560.25"));
            Assert.That(first, Does.Contain("\"currentHp\":450"));
            Assert.That(first, Does.Contain("\"effectiveMaxHp\":480"));
            Assert.That(first, Does.Contain("\"baseMaxHp\":500"));
            Assert.That(first, Does.Contain("\"currentMp\":173"));
            Assert.That(first, Does.Not.Contain("\"currentMp\":200"));
            Assert.That(first, Does.Contain("\"reviveLives\":4"));
            Assert.That(first, Does.Contain("\"reviveNextLives\":7"));
            Assert.That(first, Does.Contain("\"reviveNextHp\":320"));
            Assert.That(first, Does.Contain("\"hitReactionTimer\":60"));
            Assert.That(first, Does.Not.Contain("\"hitReactionTimer\":4"));
            Assert.That(first, Does.Contain("\"renderPhase\":4"));
            Assert.That(first, Does.Not.Contain("\"renderPhase\":-2"));
            Assert.That(first, Does.Contain("\"attackerRest\":5"));
            Assert.That(first, Does.Contain("\"motionHoldTimer\":3"));
            Assert.That(first, Does.Contain("\"weaponHp\":37"));
            Assert.That(first, Does.Contain("\"specialHitLatch0eb\":true"));
            Assert.That(first, Does.Contain("\"runtimeArmorHp\":0"));
            Assert.That(first, Does.Contain("\"armorRecoveryTimer\":-1"));
            Assert.That(first, Does.Contain("\"environmentState\":null"));
            Assert.That(first, Does.Not.Contain("\"environmentState\":-3"));
            Assert.That(first, Does.Contain("\"resolutionPending\":null"));
            Assert.That(first, Does.Not.Contain("\"resolutionPending\":true"));
            Assert.That(first, Does.Contain("\"runtimeStateCode\":null"));
            Assert.That(first, Does.Contain(
                "\"evidenceClass\":\"UNITY_RAW_BINDING_DIAGNOSTIC_ONLY\""));
            Assert.That(first, Does.Contain("\"certificateEligible\":false"));

            runtime.WeaponFlightCounter = -1;
            runtime.FrameDelay = -5;
            string depleted = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);
            Assert.That(depleted, Does.Contain("\"motionHoldTimer\":-5"));
            Assert.That(depleted, Does.Contain("\"weaponHp\":-1"));
        }

        [NUnit.Framework.Test]
        public void CaptureTickJson_RejectsInvalidBoundaryArguments()
        {
            Assert.That(
                () => NTSD28UnityEntityRawCapture.CaptureTickJson(null, 1),
                Throws.ArgumentNullException);
            Assert.That(
                () => NTSD28UnityEntityRawCapture.CaptureTickJson(
                    new SimulationWorld(),
                    0),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void BindingManifest_MatchesFrozenMaturityCounts()
        {
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Has.Length.EqualTo(0));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Has.Length.EqualTo(6));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("combat.collisionYReference"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("combat.runtimeArmorHp"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("combat.armorRecoveryTimer"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("combat.weaponHp"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("combat.motionHoldTimer"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("combat.specialHitLatch0eb"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("vitals.reviveLives"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("vitals.reviveNextLives"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("vitals.reviveNextHp"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("combat.hitReactionTimer"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("combat.attackerRest"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("vitals.effectiveMaxHp"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("vitals.baseMaxHp"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("identity.battleGroup"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("identity.participantClass"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("identity.ownerSlot"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("combat.environmentState"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Contain("combat.environmentState"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("lifecycle.resolutionPending"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Contain("lifecycle.resolutionPending"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Is.Empty);
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("frame.frameCounter"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("identity.controlSlot"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("frame.actionLatch"));
            Assert.That(
                NTSD28UnityEntityRawCapture.CandidateBindings,
                Does.Not.Contain("frame.previousAction"));
            Assert.That(
                NTSD28UnityEntityRawCapture.MissingBindings,
                Does.Not.Contain("frame.tickActionSnapshot"));
        }
    }
}
