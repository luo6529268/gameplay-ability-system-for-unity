#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("StageCampaignParserDefaultsAlignment")]
    public sealed class BattleStageCampaignLoaderDefaultsEditorTests
    {
        [Test]
        public void MissingAndInvalidStageIdPreserveMinusOneAndSourceOrder()
        {
            const string text =
                "<stage> #missing\n<stage_end>\n" +
                "<stage> id: invalid #invalid\n<stage_end>\n" +
                "<stage> id: 7 #valid\n<stage_end>\n";

            List<BattleStageCampaignData> campaigns =
                BattleStageCampaignLoader.ParseText(text);

            Assert.That(campaigns, Has.Count.EqualTo(3));
            Assert.That(campaigns[0].Id, Is.EqualTo(-1));
            Assert.That(campaigns[0].Comment, Is.EqualTo("missing"));
            Assert.That(campaigns[1].Id, Is.EqualTo(-1));
            Assert.That(campaigns[1].Comment, Is.EqualTo("invalid"));
            Assert.That(campaigns[2].Id, Is.EqualTo(7));
        }

        [Test]
        public void MissingAndInvalidSpawnTimesPreserveOne()
        {
            const string text =
                "<stage> id: 3\n" +
                "<phase>\n" +
                "id: 10 hp: 50\n" +
                "id: 11 times: invalid\n" +
                "<phase_end>\n<stage_end>\n";

            List<BattleStageCampaignData> campaigns =
                BattleStageCampaignLoader.ParseText(text);
            List<BattleStageSpawnData> spawns = campaigns[0].Phases[0].Spawns;

            Assert.That(spawns, Has.Count.EqualTo(2));
            Assert.That(spawns[0].Times, Is.EqualTo(1));
            Assert.That(spawns[1].Times, Is.EqualTo(1));
        }

        [Test]
        public void MissingAndInvalidRequiredSpawnIdRowsRemainOmitted()
        {
            const string text =
                "<stage> id: 3\n" +
                "<phase>\n" +
                "times: 9 hp: 50\n" +
                "id: invalid times: 8\n" +
                "id: 12 times: 7\n" +
                "<phase_end>\n<stage_end>\n";

            List<BattleStageCampaignData> campaigns =
                BattleStageCampaignLoader.ParseText(text);
            List<BattleStageSpawnData> spawns = campaigns[0].Phases[0].Spawns;

            Assert.That(spawns, Has.Count.EqualTo(1));
            Assert.That(spawns[0].Id, Is.EqualTo(12));
            Assert.That(spawns[0].Times, Is.EqualTo(7));
        }

        [Test]
        public void ValidFieldsCommentsAndDuplicateCampaignsRemainSourceOrdered()
        {
            const string text =
                "<stage> id: 4 #first\n" +
                "<phase> bound: 900\n" +
                "id: 20 act: 2 hp: 300 times: 5 x: -10 y: -20 ratio: 1.5 join: 8\n" +
                "<phase_end>\n<stage_end>\n" +
                "<stage> id: 4 #second\n<stage_end>\n";

            List<BattleStageCampaignData> campaigns =
                BattleStageCampaignLoader.ParseText(text);

            Assert.That(campaigns, Has.Count.EqualTo(2));
            Assert.That(campaigns[0].Id, Is.EqualTo(4));
            Assert.That(campaigns[0].Comment, Is.EqualTo("first"));
            Assert.That(campaigns[1].Id, Is.EqualTo(4));
            Assert.That(campaigns[1].Comment, Is.EqualTo("second"));
            BattleStagePhaseData phase = campaigns[0].Phases[0];
            BattleStageSpawnData spawn = phase.Spawns[0];
            Assert.That(phase.Bound, Is.EqualTo(900));
            Assert.That(spawn.Id, Is.EqualTo(20));
            Assert.That(spawn.Act, Is.EqualTo(2));
            Assert.That(spawn.Hp, Is.EqualTo(300));
            Assert.That(spawn.Times, Is.EqualTo(5));
            Assert.That(spawn.X, Is.EqualTo(-10));
            Assert.That(spawn.Y, Is.EqualTo(-20));
            Assert.That(spawn.Ratio, Is.EqualTo(1.5));
            Assert.That(spawn.Join, Is.EqualTo(8));
        }
    }
}
#endif
