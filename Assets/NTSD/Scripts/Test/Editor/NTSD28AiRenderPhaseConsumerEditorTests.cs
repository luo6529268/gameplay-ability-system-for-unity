#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;

using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28AiRenderPhaseConsumerEditorTests
    {
        [Test]
        public void Nearest_StateNineUsesHitStopInsteadOfPhysicalY()
        {
            AiSensingSnapshot rows = Snapshot(2);
            SetCharacter(rows, 0, team: 1, x: 0, state: 9, y: 0, hitStop: 0);
            SetCharacter(rows, 1, team: 2, x: 10, state: 0, y: 100, hitStop: 0);

            Assert.That(
                AiSensingKernel.TryFindNearest(rows, 0, 2, out AiSensingNearestResult result),
                Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(1));

            rows.Y[1] = 0;
            rows.HitStop[1] = 5;
            Assert.That(
                AiSensingKernel.TryFindNearest(rows, 0, 2, out result),
                Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(-1));
        }

        [Test]
        public void SoARoleMembershipUsesHitStopAndIgnoresPhysicalY()
        {
            var rows = new SimulationAiSensingModule.AiSoASensingRows(1);
            rows.Reset(7);
            SetCharacter(rows, 0, team: 1, x: 0, state: 0, y: 100, hitStop: 0);

            Assert.That(
                SimulationAiSensingModule.IsGroundRoleMember(rows, 0),
                Is.True);
            Assert.That(
                SimulationAiSensingModule.IsAirRoleMember(rows, 0),
                Is.False);

            rows.Y[0] = 0;
            rows.HitStop[0] = -5;
            Assert.That(
                SimulationAiSensingModule.IsGroundRoleMember(rows, 0),
                Is.False);
            Assert.That(
                SimulationAiSensingModule.IsAirRoleMember(rows, 0),
                Is.True);
        }

        [Test]
        public void SourceConsumersUseHitStopWhilePhysicalHeightBranchesRemainY()
        {
            string root = Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath,
                ".."));
            string kernel = File.ReadAllText(Path.Combine(
                root,
                "Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs"));
            string sensing = File.ReadAllText(Path.Combine(
                root,
                "Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs"));

            Assert.That(kernel, Does.Not.Contain("Abs(rows.Y[target]) > 2"));
            Assert.That(kernel, Does.Not.Contain("Abs(rows.Y[target]) <= 2"));
            Assert.That(kernel, Does.Not.Contain("Abs(rows.Y[slot]) > 2"));
            Assert.That(kernel, Does.Not.Contain("rows.Y[self] != 0"));
            Assert.That(sensing, Does.Not.Contain("Abs(rows.Y[slot]) <= 2"));
            Assert.That(sensing, Does.Not.Contain("Abs(rows.Y[slot]) > 2"));

            Assert.That(kernel, Does.Contain("rows.Y[target] == 0"));
            Assert.That(kernel, Does.Contain("rows.Y[target] < 0"));
            Assert.That(kernel, Does.Contain("rows.Y[target] >= 0"));
        }

        private static AiSensingSnapshot Snapshot(int capacity)
        {
            var rows = new AiSensingSnapshot(capacity);
            rows.Reset(1);
            return rows;
        }

        private static void SetCharacter(
            AiSensingSnapshot rows,
            int slot,
            int team,
            int x,
            int state,
            int y,
            int hitStop)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = (uint)(slot + 1);
            rows.Identity[slot] = 1000 + slot;
            rows.DataObjectType[slot] = 0;
            rows.Team[slot] = team;
            rows.X[slot] = x;
            rows.Y[slot] = y;
            rows.Z[slot] = 0;
            rows.State[slot] = state;
            rows.Hp[slot] = 500;
            rows.HitStop[slot] = hitStop;
            rows.OwnerSlot[slot] = -1;
            rows.CoordinateTargetX[slot] = -1000;
        }
    }
}
#endif
