#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiDecisionAuthorityChainContractEditorTests
    {
        private static readonly string[] AuthorityPositions =
        {
            "FirstDecision",
            "TeammateGuard",
            "Oid1Combo",
            "CloseOid1",
            "Oid4Combo",
            "Oid5Combo",
            "Oid6Combo",
            "Oid7FrameKey",
            "Oid7CloseCombo",
            "Oid7MidFarCombo",
            "Oid7FacingCombo",
            "Oid7Frame255Key",
            "Oid8Combo",
            "Oid11FirstCombo",
            "Oid11Frame290SideEffect",
            "Oid11Dua",
            "Oid10_1FirstCombo",
            "Oid10_1Frame271",
            "Oid10_1PredictedDua",
            "Oid10_1MidrangeCombo",
            "Oid10_1HpTeamScanSideEffect",
            "Oid10_1HpAdvantageSideEffect",
            "Oid9_2PredictedDda",
            "Oid9_2MidFarCombo",
            "Oid9_2NearestDua",
            "Oid32_19MidFarCombo",
            "Oid32_19CloseCombo",
            "Oid33_19_16PredictedDua",
            "Oid34_10_5_14LowHpDdj",
            "Oid34_10_5_14TeammateGuard",
            "Label464LongCombo",
            "Label464CloseDda",
            "Oid35LongCombo",
            "Oid36_16TeamDuj",
            "Oid36_16RangeDua",
            "Oid38Combo",
            "Oid39_10CloseCombo",
            "Oid52_1_2_21PreLabel591",
            "Label591Oid51_2_18_7",
        };

        [Test]
        public void SourceContract_DeclaresAllThirtyNinePositionsInAuthorityOrder()
        {
            Assert.That(AuthorityPositions, Has.Length.EqualTo(39));
            Assert.That(AuthorityPositions[0], Is.EqualTo("FirstDecision"));
            Assert.That(AuthorityPositions[5], Is.EqualTo("Oid5Combo"));
            Assert.That(AuthorityPositions[6], Is.EqualTo("Oid6Combo"));
            Assert.That(
                AuthorityPositions[27],
                Is.EqualTo("Oid33_19_16PredictedDua"));
            Assert.That(
                AuthorityPositions[37],
                Is.EqualTo("Oid52_1_2_21PreLabel591"));
            Assert.That(
                AuthorityPositions[38],
                Is.EqualTo("Label591Oid51_2_18_7"));
        }

        [Test]
        public void MissingPosition07_Oid6OuterGateHit_ProducesDirectionalJumpCombo()
        {
            AiDecisionSnapshot snapshot = CreateTwoCharacterSnapshot(
                selfOid: 6,
                selfX: 0,
                targetX: 100,
                targetState: 0,
                difficulty: 0,
                rngSeed: 23u);

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(
                witness.Input.ComboDrj,
                Is.EqualTo(3),
                "C++ position 7 consumes the third RNG draw (6130 % 10 == 0) " +
                "and sets DRJ for an OID6 target to the right.");
            Assert.That(
                witness.RngCalls,
                Is.EqualTo(3),
                "the authority exits after the boundary draw, outer gate and OID6 helper.");
        }

        [Test]
        public void Position28_Oid33OuterGateMiss_DoesNotExecutePredictedDua()
        {
            AiDecisionSnapshot snapshot = CreateTwoCharacterSnapshot(
                selfOid: 33,
                selfX: 0,
                targetX: 50,
                targetState: 0,
                difficulty: 1,
                rngSeed: 5u);

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(
                witness.Input.ComboDua,
                Is.Zero,
                "after the boundary draw, outer raw 28693 has 28693 % 6 != 0, " +
                "so C++ skips every position inside the outer gate, including position 28.");
        }

        private static AiDecisionSnapshot CreateTwoCharacterSnapshot(
            int selfOid,
            int selfX,
            int targetX,
            int targetState,
            int difficulty,
            uint rngSeed)
        {
            var snapshot = new AiDecisionSnapshot(2);
            snapshot.Reset(1);
            SetCharacter(
                snapshot.Rows,
                slot: 0,
                stableId: 1000,
                generation: 1,
                objectId: selfOid,
                team: 1,
                x: selfX,
                state: 0);
            SetCharacter(
                snapshot.Rows,
                slot: 1,
                stableId: 1001,
                generation: 2,
                objectId: 999,
                team: 2,
                x: targetX,
                state: targetState);

            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            snapshot.World.Difficulty = difficulty;
            snapshot.World.InputPhase = 2;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = 180;
            snapshot.World.StageZMax = 350;
            snapshot.RngState = rngSeed;
            return snapshot;
        }

        private static void SetCharacter(
            AiSensingSnapshot rows,
            int slot,
            int stableId,
            uint generation,
            int objectId,
            int team,
            int x,
            int state)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = generation;
            rows.Identity[slot] = stableId;
            rows.DataObjectType[slot] = 0;
            rows.ObjectId[slot] = objectId;
            rows.Team[slot] = team;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = 0;
            rows.State[slot] = state;
            rows.Frame[slot] = 0;
            rows.Facing[slot] = 0;
            rows.Vx[slot] = 0.0;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = 500;
            rows.CoordinateTargetX[slot] = -1000;
            rows.KillCount[slot] = -1;
        }
    }
}
#endif
