#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07HeldWeaponDualDomainEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string Root =
            "artifacts/diagnostics/NTSD28-Q07-HELD-WEAPON-DUAL-DOMAIN-FULL-TICK-001";
        private const double XFactor = 2048.0 / 1333.0;
        private const double ZFactor = 1152.0 / 730.0;

        [TestCase(false)]
        [TestCase(true)]
        public void NaturalPickupHeldWeapon_ViewWPointsCoincideAfterProjection(bool configuredView)
        {
            NativeRow expected = ReadNativeRows()[1];
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, Root + "/naruto-held-weapon-24.json",
                BattleRuntimeProfile.Authority400, 24,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(
                        configuredView ? 2048 : 1333, configuredView ? 1152 : 730);
                    LF2Entity holder = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity opponent = world.FindEntityByRuntimeSlotForQuery(1);
                    holder.Runtime.SetSourceRulePosition(holder.Runtime.X, holder.Runtime.Z);
                    holder.Runtime.SyncSourceRuleIntegerPosition();
                    opponent.Runtime.SetSourceRulePosition(opponent.Runtime.X, opponent.Runtime.Z);
                    opponent.Runtime.SyncSourceRuleIntegerPosition();
                    var weaponData = world.RuntimeCharacterConfigs.Resolve(120);
                    Assert.That(weaponData?.characterData, Is.Not.Null);
                    var weapon = new LF2Weapon();
                    weapon.ObjectId = 120;
                    weapon.SetWeaponType(1);
                    weapon.FrameCache.Load(weaponData);
                    weapon.SetRequiredRuntimeSlot(50);
                    world.Register(weapon);
                    weapon.ImmediateFrame(64);
                    weapon.Health.HP = 100;
                    weapon.Runtime.SetPosition(190, 0, 542);
                    weapon.Runtime.SyncIntegerPosition();
                    weapon.Runtime.SetSourceRulePosition(190, 542);
                    weapon.Runtime.SyncSourceRuleIntegerPosition();
                    for (int tick = 1; tick <= 2; tick++)
                        Assert.That(driver.StepOneTick(inputs[tick - 1], true, true), Is.True);

                    Assert.That(holder.Frame.D.frameId, Is.EqualTo(expected.ActorAction));
                    Assert.That(weapon.Frame.D.frameId, Is.EqualTo(expected.WeaponAction));
                    Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(50));
                    Assert.That(weapon.Runtime.LinkState, Is.EqualTo(-1));
                    Assert.That(weapon.Runtime.HolderStableId, Is.EqualTo(0));
                    Assert.That(holder.Runtime.SourceRuleXInt, Is.EqualTo(expected.ActorX));
                    Assert.That(holder.Runtime.SourceRuleZInt, Is.EqualTo(expected.ActorZ));
                    Assert.That(weapon.Runtime.SourceRuleXInt, Is.EqualTo(expected.WeaponX));
                    Assert.That(weapon.Runtime.SourceRuleZInt, Is.EqualTo(expected.WeaponZ));

                    Vector2 offset = LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels(
                        weapon.Runtime, weapon.Frame.D, holder, NTSDRenderSpace.BattleVisualScale);
                    Vector2 holderPoint = RenderedWeaponPoint(holder, world.SpatialProjection, 2);
                    Vector2 weaponPoint = RenderedWeaponPoint(weapon, world.SpatialProjection, 2) + offset;
                    Vector2 difference = weaponPoint - holderPoint;
                    string directory = Path.GetFullPath(
                        "artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005");
                    Directory.CreateDirectory(directory);
                    string output = Path.Combine(directory, "pickup-anchor-" +
                        (configuredView ? "project-" : "identity-") +
                        DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) +
                        "-" + Guid.NewGuid().ToString("N") + ".csv");
                    using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("tick,configured_view,holder_action,weapon_action,holder_source_x,weapon_source_x,holder_y,weapon_y,holder_view_x,weapon_view_x,holder_view_z,weapon_view_z,holder_center_x,holder_center_y,holder_wpoint_x,holder_wpoint_y,weapon_center_x,weapon_center_y,weapon_wpoint_x,weapon_wpoint_y,offset_x,offset_y,holder_point_x,holder_point_y,weapon_point_x,weapon_point_y,difference_x,difference_y");
                        writer.WriteLine(string.Join(",", new double[]
                        {
                            2, configuredView ? 1 : 0, holder.Frame.D.frameId, weapon.Frame.D.frameId,
                            holder.Runtime.SourceRuleXInt, weapon.Runtime.SourceRuleXInt,
                            holder.Runtime.YInt, weapon.Runtime.YInt,
                            holder.Runtime.XInt, weapon.Runtime.XInt,
                            holder.GetDisplayZ(), weapon.GetDisplayZ(),
                            holder.Frame.D.centerx, holder.Frame.D.centery,
                            holder.Frame.D.PrimaryWeaponPoint.X, holder.Frame.D.PrimaryWeaponPoint.Y,
                            weapon.Frame.D.centerx, weapon.Frame.D.centery,
                            weapon.Frame.D.PrimaryWeaponPoint.X, weapon.Frame.D.PrimaryWeaponPoint.Y,
                            offset.x, offset.y, holderPoint.x, holderPoint.y,
                            weaponPoint.x, weaponPoint.y, difference.x, difference.y
                        }.Select(value => value.ToString("R", CultureInfo.InvariantCulture))));
                    }
                    TestContext.Progress.WriteLine("D024 held WPoint geometry: " + output);
                    Assert.That(difference.x, Is.EqualTo(0.0).Within(0.0003), "held visual WPoint X");
                    Assert.That(difference.y, Is.EqualTo(0.0).Within(0.0003), "held visual WPoint Y");
                }, useProjectMode: true);
        }

        private static Vector2 RenderedWeaponPoint(
            LF2Entity entity, BattleSpatialProjection projection, int tick)
        {
            LF2FrameData frame = entity.Frame.D;
            float width = entity.GetSpriteWidthPxForRender();
            float height = entity.GetSpriteHeightPxForRender();
            float scale = NTSDRenderSpace.BattleVisualScale;
            bool facingLeft = entity.Runtime.Dir == "left";
            Vector2 pivot = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                entity.Runtime.XInt, entity.Runtime.YInt, entity.GetDisplayZ(),
                entity.GetRenderOffsetX(), entity.Match.ReleaseCameraX,
                entity.FrameDelay, tick, facingLeft, width, height,
                frame.centerx, frame.centery, scale, projection.VerticalScale);
            float pointX = facingLeft ? width - frame.PrimaryWeaponPoint.X :
                frame.PrimaryWeaponPoint.X;
            return pivot + new Vector2(
                scale * (pointX - width * 0.5f),
                scale * (frame.PrimaryWeaponPoint.Y - height));
        }

        [Test]
        public void NaturalPickupMovingHeldWeapon_KeepsRuleAndScaledViewDomains()
        {
            NativeRow[] native = ReadNativeRows();
            var observed = new List<string>(25)
            {
                "tick,actor_action,actor_link,actor_child_slot,actor_source_x,actor_source_z," +
                "actor_source_x_int,actor_source_z_int,actor_view_x,actor_view_z," +
                "weapon_action,weapon_link,weapon_parent_slot,weapon_source_x,weapon_source_z," +
                "weapon_source_x_int,weapon_source_z_int,weapon_view_x,weapon_view_z"
            };
            int movingX = 0;
            int movingZ = 0;
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, Root + "/naruto-held-weapon-24.json",
                BattleRuntimeProfile.Authority400, 24,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    LF2Entity actor = world.FindEntityByRuntimeSlotForQuery(0);
                    Assert.That(actor, Is.Not.Null);
                    actor.Runtime.SetSourceRulePosition(
                        actor.Runtime.X, actor.Runtime.Z);
                    actor.Runtime.SyncSourceRuleIntegerPosition();
                    LF2Entity opponent = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(opponent, Is.Not.Null);
                    opponent.Runtime.SetSourceRulePosition(
                        opponent.Runtime.X, opponent.Runtime.Z);
                    opponent.Runtime.SyncSourceRuleIntegerPosition();

                    var weaponData = world.RuntimeCharacterConfigs.Resolve(120);
                    Assert.That(weaponData?.characterData, Is.Not.Null,
                        "formal OID120 configuration");
                    var weapon = new LF2Weapon();
                    weapon.ObjectId = 120;
                    weapon.SetWeaponType(1);
                    weapon.FrameCache.Load(weaponData);
                    weapon.SetRequiredRuntimeSlot(50);
                    world.Register(weapon);
                    weapon.ImmediateFrame(64);
                    weapon.Health.HP = 100;
                    weapon.Runtime.SetPosition(190, 0, 542);
                    weapon.Runtime.SyncIntegerPosition();
                    weapon.Runtime.SetSourceRulePosition(190, 542);
                    weapon.Runtime.SyncSourceRuleIntegerPosition();

                    double birthActorViewX = 0;
                    double birthActorViewZ = 0;
                    int previousActorX = 0;
                    int previousActorZ = 0;
                    int previousWeaponX = 0;
                    int previousWeaponZ = 0;
                    try
                    {
                        for (int tick = 1; tick <= 24; tick++)
                        {
                            bool captureMotionPair = tick == 1 || tick == 2 ||
                                tick == 8 || tick == 9 || tick == 16 || tick == 17;
                            Assert.That(driver.StepOneTick(
                                    inputs[tick - 1], true, captureMotionPair),
                                Is.True, "complete Driver tick=" + tick);
                            LF2Entity currentWeapon =
                                world.FindEntityByRuntimeSlotForQuery(50);
                            Assert.That(currentWeapon, Is.Not.Null,
                                "formal OID120 present tick=" + tick);
                            NativeRow expected = native[tick - 1];
                            var a = actor.Runtime;
                            var w = currentWeapon.Runtime;
                            observed.Add(string.Join(",", new[]
                            {
                                tick.ToString(CultureInfo.InvariantCulture),
                                (actor.Frame.D?.frameId ?? -1).ToString(CultureInfo.InvariantCulture),
                                a.LinkState.ToString(CultureInfo.InvariantCulture),
                                a.TargetSlotIndex.ToString(CultureInfo.InvariantCulture),
                                a.SourceRuleX.ToString("R", CultureInfo.InvariantCulture),
                                a.SourceRuleZ.ToString("R", CultureInfo.InvariantCulture),
                                a.SourceRuleXInt.ToString(CultureInfo.InvariantCulture),
                                a.SourceRuleZInt.ToString(CultureInfo.InvariantCulture),
                                a.X.ToString("R", CultureInfo.InvariantCulture),
                                a.Z.ToString("R", CultureInfo.InvariantCulture),
                                (currentWeapon.Frame.D?.frameId ?? -1).ToString(CultureInfo.InvariantCulture),
                                w.LinkState.ToString(CultureInfo.InvariantCulture),
                                w.HolderStableId.ToString(CultureInfo.InvariantCulture),
                                w.SourceRuleX.ToString("R", CultureInfo.InvariantCulture),
                                w.SourceRuleZ.ToString("R", CultureInfo.InvariantCulture),
                                w.SourceRuleXInt.ToString(CultureInfo.InvariantCulture),
                                w.SourceRuleZInt.ToString(CultureInfo.InvariantCulture),
                                w.X.ToString("R", CultureInfo.InvariantCulture),
                                w.Z.ToString("R", CultureInfo.InvariantCulture)
                            }));

                            Assert.That(expected.Tick, Is.EqualTo(tick));
                            Assert.That(actor.Frame.D?.frameId ?? -1,
                                Is.EqualTo(expected.ActorAction), "actor action tick=" + tick);
                            Assert.That(a.LinkState, Is.EqualTo(expected.ActorLink),
                                "actor relation tick=" + tick);
                            if (expected.ActorLink % 100 == 1)
                                Assert.That(a.TargetSlotIndex, Is.EqualTo(expected.ActorChildSlot),
                                    "held slot tick=" + tick);
                            Assert.That(a.SourceRuleX,
                                Is.EqualTo(expected.ActorPreciseX).Within(1e-6),
                                "actor source X tick=" + tick);
                            Assert.That(a.SourceRuleZ,
                                Is.EqualTo(expected.ActorPreciseZ).Within(1e-6),
                                "actor source Z tick=" + tick);
                            Assert.That(a.SourceRuleXInt, Is.EqualTo(expected.ActorX),
                                "actor rule X tick=" + tick);
                            Assert.That(a.SourceRuleZInt, Is.EqualTo(expected.ActorZ),
                                "actor rule Z tick=" + tick);
                            Assert.That(currentWeapon.Frame.D?.frameId ?? -1,
                                Is.EqualTo(expected.WeaponAction),
                                "weapon action tick=" + tick);
                            Assert.That(w.LinkState, Is.EqualTo(expected.WeaponLink),
                                "weapon relation tick=" + tick);
                            if (expected.WeaponLink == -1)
                                Assert.That(w.HolderStableId,
                                    Is.EqualTo(expected.WeaponParentSlot),
                                    "holder slot tick=" + tick);
                            Assert.That(w.SourceRuleX,
                                Is.EqualTo(expected.WeaponPreciseX).Within(1e-6),
                                "weapon source X tick=" + tick);
                            Assert.That(w.SourceRuleZ,
                                Is.EqualTo(expected.WeaponPreciseZ).Within(1e-6),
                                "weapon source Z tick=" + tick);
                            Assert.That(w.SourceRuleXInt, Is.EqualTo(expected.WeaponX),
                                "weapon rule X tick=" + tick);
                            Assert.That(w.SourceRuleZInt, Is.EqualTo(expected.WeaponZ),
                                "weapon rule Z tick=" + tick);

                            if (tick == 1)
                            {
                                birthActorViewX = a.X;
                                birthActorViewZ = a.Z;
                            }
                            else
                            {
                                Assert.That(a.X - birthActorViewX,
                                    Is.EqualTo((expected.ActorPreciseX - native[0].ActorPreciseX) * XFactor)
                                        .Within(1e-6), "actor view X tick=" + tick);
                                Assert.That(a.Z - birthActorViewZ,
                                    Is.EqualTo((expected.ActorPreciseZ - native[0].ActorPreciseZ) * ZFactor)
                                        .Within(1e-6), "actor view Z tick=" + tick);
                            }
                            if (tick >= 2 && expected.ActorLink % 100 == 1)
                            {
                                Assert.That(w.X - a.X,
                                    Is.EqualTo((expected.WeaponX - expected.ActorX) * XFactor)
                                        .Within(1.0),
                                    "scaled WPoint X offset tick=" + tick);
                                Assert.That(w.Z - a.Z,
                                    Is.EqualTo((expected.WeaponZ - expected.ActorZ) * ZFactor)
                                        .Within(1.0),
                                    "scaled WPoint Z offset tick=" + tick);
                                if (tick > 2)
                                {
                                    if (a.XInt != previousActorX && w.XInt != previousWeaponX)
                                        movingX++;
                                    if (a.ZInt != previousActorZ && w.ZInt != previousWeaponZ)
                                        movingZ++;
                                }
                            }
                            if (tick == 2 || tick == 9 || tick == 17)
                            {
                                BattlePresentationFrame frame =
                                    world.BattlePresentation.PublishedFrame;
                                Assert.That(frame, Is.Not.Null,
                                    "held motion publication tick=" + tick);
                                Assert.That(frame.TickIndex, Is.EqualTo(tick));
                                Assert.That(frame.PreviousMotionTickIndex,
                                    Is.EqualTo(tick - 1));
                                Assert.That(TryFindMotionState(frame, 50, true,
                                    out BattlePresentationMotionState previousMotion),
                                    Is.True, "previous weapon motion tick=" + tick);
                                Assert.That(TryFindMotionState(frame, 50, false,
                                    out BattlePresentationMotionState currentMotion),
                                    Is.True, "current weapon motion tick=" + tick);

                                string checksumBefore =
                                    world.CaptureParityFrameSnapshot(tick).OverallChecksum;
                                BattlePresentationMotionSampleStatus status =
                                    BattlePresentationMotionSampler.Sample(
                                        previousMotion, currentMotion, tick - 1, tick,
                                        0.5, XFactor, ZFactor,
                                        out BattlePresentationMotionDelta delta);
                                var display = new BattlePresentationDisplayMotion();
                                display.Prepare(frame, 0.5, XFactor, ZFactor);
                                bool sampledWeapon = display.TryGet(
                                    currentMotion.Handle,
                                    out BattlePresentationMotionDelta publishedDelta);
                                string checksumAfter =
                                    world.CaptureParityFrameSnapshot(tick).OverallChecksum;
                                Assert.That(checksumAfter, Is.EqualTo(checksumBefore),
                                    "held display sampling changed logic tick=" + tick);

                                if (tick == 2)
                                {
                                    Assert.That(status,
                                        Is.EqualTo(BattlePresentationMotionSampleStatus.RelationChanged));
                                    Assert.That(sampledWeapon, Is.False,
                                        "pickup relation must break interpolation");
                                }
                                else
                                {
                                    Assert.That(status,
                                        Is.EqualTo(BattlePresentationMotionSampleStatus.Sampled));
                                    Assert.That(sampledWeapon, Is.True,
                                        "held weapon motion must remain visible to the sampler");
                                    Assert.That(publishedDelta.ViewX,
                                        Is.EqualTo(delta.ViewX).Within(1e-9));
                                    Assert.That(publishedDelta.ViewZ,
                                        Is.EqualTo(delta.ViewZ).Within(1e-9));
                                    if (tick == 9)
                                        Assert.That(delta.ViewX,
                                            Is.EqualTo(-2.0 * XFactor).Within(1e-9));
                                    if (tick == 17)
                                        Assert.That(delta.ViewZ,
                                            Is.EqualTo(-1.0 * ZFactor).Within(1e-9));
                                }
                                TestContext.Progress.WriteLine(
                                    $"Q09 held motion tick={tick} status={status} " +
                                    $"viewDelta={delta.ViewX:R}/{delta.ViewZ:R} " +
                                    $"checksum={checksumAfter}");
                            }
                            previousActorX = a.XInt;
                            previousActorZ = a.ZInt;
                            previousWeaponX = w.XInt;
                            previousWeaponZ = w.ZInt;
                        }
                        Assert.That(movingX, Is.GreaterThanOrEqualTo(3));
                        Assert.That(movingZ, Is.GreaterThanOrEqualTo(3));
                    }
                    finally
                    {
                        string directory = Path.GetFullPath(Root + "/unity");
                        Directory.CreateDirectory(directory);
                        string output = Path.Combine(directory,
                            "held-weapon-" + DateTime.UtcNow.ToString(
                                "yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) +
                            "-" + Guid.NewGuid().ToString("N") + ".csv");
                        File.WriteAllLines(output, observed);
                        TestContext.Progress.WriteLine("Q07 held weapon raw: " + output);
                    }
                }, useProjectMode: true);
            Assert.That(observed.Count, Is.EqualTo(25));
        }

        private static NativeRow[] ReadNativeRows()
        {
            string[] lines = File.ReadAllLines(Path.GetFullPath(
                "artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/native-336b44-24.csv"));
            Assert.That(lines.Length, Is.EqualTo(25));
            var rows = new NativeRow[24];
            for (int index = 1; index < lines.Length; index++)
            {
                string[] fields = lines[index].Split(',');
                Assert.That(fields.Length, Is.EqualTo(18));
                rows[index - 1] = new NativeRow
                {
                    Tick = ParseInt(fields[0]),
                    ActorAction = ParseInt(fields[3]),
                    ActorLink = ParseInt(fields[4]),
                    ActorChildSlot = ParseInt(fields[5]),
                    ActorX = ParseInt(fields[6]),
                    ActorZ = ParseInt(fields[7]),
                    ActorPreciseX = ParseDouble(fields[8]),
                    ActorPreciseZ = ParseDouble(fields[9]),
                    WeaponAction = ParseInt(fields[11]),
                    WeaponLink = ParseInt(fields[12]),
                    WeaponParentSlot = ParseInt(fields[13]),
                    WeaponX = ParseInt(fields[14]),
                    WeaponZ = ParseInt(fields[15]),
                    WeaponPreciseX = ParseDouble(fields[16]),
                    WeaponPreciseZ = ParseDouble(fields[17]),
                };
                Assert.That(ParseInt(fields[10]), Is.EqualTo(1),
                    "formal weapon present tick=" + index);
            }
            return rows;
        }

        private static bool TryFindMotionState(
            BattlePresentationFrame frame,
            int slot,
            bool previous,
            out BattlePresentationMotionState state)
        {
            int count = previous
                ? frame.PreviousMotionStateCount
                : frame.MotionStateCount;
            for (int index = 0; index < count; index++)
            {
                BattlePresentationMotionState candidate = previous
                    ? frame.GetPreviousMotionState(index)
                    : frame.GetMotionState(index);
                if (candidate.Handle.Slot != slot)
                    continue;
                state = candidate;
                return true;
            }

            state = default;
            return false;
        }

        private static int ParseInt(string textValue) =>
            int.Parse(textValue, CultureInfo.InvariantCulture);

        private static double ParseDouble(string textValue) =>
            double.Parse(textValue, CultureInfo.InvariantCulture);

        private sealed class NativeRow
        {
            public int Tick;
            public int ActorAction;
            public int ActorLink;
            public int ActorChildSlot;
            public int ActorX;
            public int ActorZ;
            public double ActorPreciseX;
            public double ActorPreciseZ;
            public int WeaponAction;
            public int WeaponLink;
            public int WeaponParentSlot;
            public int WeaponX;
            public int WeaponZ;
            public double WeaponPreciseX;
            public double WeaponPreciseZ;
        }
    }
}
#endif
