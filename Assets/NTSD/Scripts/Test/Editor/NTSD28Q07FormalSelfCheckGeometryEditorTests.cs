#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28Q07FormalSelfCheckGeometry")]
    public sealed class NTSD28Q07FormalSelfCheckGeometryEditorTests
    {
        private const string FormalRoot =
            @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime";

        [Test]
        public void FormalAndStagedResolvedGeometryMatchCurrentContentContract()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            Assert.That(Directory.Exists(FormalRoot), Is.True, FormalRoot);
            Assert.That(Directory.Exists(stagedRoot), Is.True, stagedRoot);

            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            GeometryCount formal = Measure(FormalRoot, mode);
            GeometryCount staged = Measure(stagedRoot, mode);
            Assert.That(staged, Is.EqualTo(formal), "staged and formal resolved geometry differ");
            Assert.That(formal.Objects, Is.EqualTo(330));
            Assert.That(formal.AuthoredFrames, Is.EqualTo(55348));
            Assert.That(formal.OutOfRangeFrames, Is.Zero);
            Assert.That(formal.ResolvedVisits, Is.EqualTo(282810));
            Assert.That(formal.Itrs, Is.EqualTo(19461));
            Assert.That(formal.Bodies, Is.EqualTo(86383));
            Assert.That(formal.CompleteZeroWidthItrs, Is.EqualTo(27));
            Assert.That(formal.ControlOnlyItrs, Is.EqualTo(82));
            Assert.That(formal.OtherNonPositiveItrs, Is.Zero);
            Assert.That(formal.NonPositiveBodies, Is.Zero);
        }

        [Test]
        public void FormalGeometrySelfCheckAcceptsStagedContent()
        {
            MethodInfo check = typeof(BattleRuntimeSelfCheck).GetMethod(
                "CheckDeployableResolvedGeometryRisks", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(BattleContentSource), typeof(ProjectBattleModeConfig.Snapshot) }, null);
            Assert.That(check, Is.Not.Null);
            string stagedRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            var source = BattleContentSource.ForLoganRuntime(stagedRoot);
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            Assert.DoesNotThrow(() => check.Invoke(null, new object[] { source, mode }));
        }

        private static GeometryCount Measure(string root, ProjectBattleModeConfig.Snapshot mode)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(root), mode);
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            var result = new GeometryCount { Objects = catalog.Entries.Count };
            Assert.That(configs.Count, Is.EqualTo(result.Objects), root);

            foreach (LoganObjectCatalog.Entry entry in catalog.Entries)
            {
                Assert.That(configs.TryGetValue(entry.Id, out LF2CharacterDataWrapper wrapper),
                    Is.True, "missing oid " + entry.Id);
                var cache = new LF2FrameCache();
                cache.Load(wrapper);
                foreach (LF2FrameData authored in wrapper.characterData.frames)
                {
                    if (authored.frameId < 0 || authored.frameId >= LF2FrameCache.MaxFrameIdExclusive)
                        result.OutOfRangeFrames++;
                }
                for (int frameId = 0; frameId < LF2FrameCache.MaxFrameIdExclusive; frameId++)
                {
                    LF2FrameData frame = cache.GetFrameDataById(frameId);
                    Assert.That(frame, Is.Not.Null, "resolved frame " + entry.Id + "/" + frameId);
                    result.ResolvedVisits++;
                    if (!cache.HasFrame(frameId))
                        continue;
                    result.AuthoredFrames++;
                    foreach (InteractionArea itr in frame.itrs)
                    {
                        result.Itrs++;
                        if (itr.w > 0 && itr.h > 0)
                            continue;
                        if (itr.kind == 0 && itr.w == 0 && itr.h == 79 && itr.hasGeometry)
                            result.CompleteZeroWidthItrs++;
                        else if (itr.kind == 100100 && itr.w == 0 && itr.h == 0 && !itr.hasGeometry)
                            result.ControlOnlyItrs++;
                        else
                            result.OtherNonPositiveItrs++;
                    }
                    foreach (BattleBodyBoxValue body in frame.bodies)
                    {
                        result.Bodies++;
                        if (body.W <= 0 || body.H <= 0)
                            result.NonPositiveBodies++;
                    }
                }
            }
            return result;
        }

        private sealed class GeometryCount : IEquatable<GeometryCount>
        {
            public int Objects;
            public int AuthoredFrames;
            public int OutOfRangeFrames;
            public int ResolvedVisits;
            public int Itrs;
            public int Bodies;
            public int CompleteZeroWidthItrs;
            public int ControlOnlyItrs;
            public int OtherNonPositiveItrs;
            public int NonPositiveBodies;

            public bool Equals(GeometryCount other)
            {
                return other != null && Objects == other.Objects &&
                    AuthoredFrames == other.AuthoredFrames &&
                    OutOfRangeFrames == other.OutOfRangeFrames &&
                    ResolvedVisits == other.ResolvedVisits && Itrs == other.Itrs &&
                    Bodies == other.Bodies &&
                    CompleteZeroWidthItrs == other.CompleteZeroWidthItrs &&
                    ControlOnlyItrs == other.ControlOnlyItrs &&
                    OtherNonPositiveItrs == other.OtherNonPositiveItrs &&
                    NonPositiveBodies == other.NonPositiveBodies;
            }

            public override bool Equals(object other) => Equals(other as GeometryCount);

            public override int GetHashCode() => Objects;

            public override string ToString()
            {
                return $"objects={Objects}, authored={AuthoredFrames}, outOfRange={OutOfRangeFrames}, " +
                    $"visits={ResolvedVisits}, itr={Itrs}, bdy={Bodies}, " +
                    $"zeroWidth={CompleteZeroWidthItrs}, controlOnly={ControlOnlyItrs}, " +
                    $"otherNonPositiveItr={OtherNonPositiveItrs}, nonPositiveBdy={NonPositiveBodies}";
            }
        }
    }
}
#endif
