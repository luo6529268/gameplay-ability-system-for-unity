#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattlePresentationCommandWriterEditorTests
    {
        private static readonly MethodInfo ResetFrameMethod =
            typeof(BattlePresentationFrame).GetMethod(
                "Reset",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo CopyFrameMethod =
            typeof(BattlePresentationFrame).GetMethod(
                "CopyFrom",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo ModeOverlayGateProperty =
            typeof(BattlePresentationFrame).GetProperty(
                "SelectedModeReviveLivesGate54",
                BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo ModeEtcProperty =
            typeof(BattlePresentationFrame).GetProperty(
                "SelectedModeEtcMode",
                BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo AddEntityMethod =
            typeof(BattlePresentationFrame).GetMethod(
                "AddEntity",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo SlotLabelCharsField =
            typeof(BattlePresentationFrame).GetField(
                "slotLabelChars",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo SlotLabelStateField =
            typeof(BattlePresentationFrame).GetField(
                "slotLabelState",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo BoundCatalogField =
            typeof(BattlePresentationFrame).GetField(
                "boundCatalog",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly ConstructorInfo BindingConstructor =
            typeof(BattleCommonVisualBinding).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleVisualResourceKey),
                    typeof(Sprite),
                    typeof(Texture2D),
                    typeof(Material),
                    typeof(Rect),
                    typeof(Rect),
                    typeof(Vector2),
                    typeof(Vector2),
                    typeof(BattleSpriteRenderState),
                },
                null);
        private static readonly ConstructorInfo CatalogConstructor =
            typeof(BattleCommonVisualCatalog).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleCommonVisualBinding),
                    typeof(BattleCommonVisualBinding[]),
                    typeof(Texture2D[]),
                    typeof(BattleCommonVisualBinding[][]),
                    typeof(string),
                },
                null);
        private static readonly ConstructorInfo CatalogWithSpecialConstructor =
            typeof(BattleCommonVisualCatalog).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleCommonVisualBinding),
                    typeof(BattleCommonVisualBinding[]),
                    typeof(Texture2D[]),
                    typeof(BattleCommonVisualBinding[][]),
                    typeof(BattleCommonVisualBinding),
                    typeof(string),
                },
                null);
        private static readonly ConstructorInfo CatalogWithComLabelsConstructor =
            typeof(BattleCommonVisualCatalog).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleCommonVisualBinding),
                    typeof(BattleCommonVisualBinding[]),
                    typeof(Texture2D[]),
                    typeof(BattleCommonVisualBinding[][]),
                    typeof(BattleCommonVisualBinding[]),
                    typeof(string),
                    typeof(bool),
                },
                null);

        [Test]
        public void State9997BodyPlacement_UsesOwnerOnlyForSelectedModeAndValidOwner()
        {
            Vector2 ordinary = new Vector2(485f, 123f);
            Vector3 owner = new Vector3(500f, 10f, 350f);
            Vector2 ownerPivot = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                9997, 1, 8, true, ordinary, true, 100f, 80f,
                40f, 20f, 1.5f, owner, 5f, -3.25f, 2044.75f,
                out bool ownerFlip);
            Assert.That(ownerFlip, Is.False);
            Assert.That(ownerPivot.x, Is.EqualTo(515f).Within(0.0001f));
            Assert.That(ownerPivot.y, Is.EqualTo(457.5f).Within(0.0001f));

            foreach (int selectedMode in new[] { 0, 2 })
            {
                Vector2 fallback = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                    9997, selectedMode, 8, true, ordinary, true, 100f, 80f,
                    40f, 20f, 1.5f, owner, 5f, -3.25f, 2044.75f,
                    out bool fallbackFlip);
                Assert.That(fallbackFlip, Is.True);
                Assert.That(fallback, Is.EqualTo(ordinary));
            }

            foreach (int ownerSlot in new[] { -1, 9 })
            {
                Vector2 fallback = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                    9997, 1, ownerSlot, true, ordinary, true, 100f, 80f,
                    40f, 20f, 1.5f, owner, 5f, -3.25f, 2044.75f,
                    out bool fallbackFlip);
                Assert.That(fallbackFlip, Is.True);
                Assert.That(fallback, Is.EqualTo(ordinary));
            }

            Vector2 missingOwner = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                9997, 1, 8, false, ordinary, true, 100f, 80f,
                40f, 20f, 1.5f, owner, 5f, -3.25f, 2044.75f,
                out bool missingOwnerFlip);
            Assert.That(missingOwnerFlip, Is.True);
            Assert.That(missingOwner, Is.EqualTo(ordinary));
        }

        [Test]
        public void State9997BodyPlacement_ClampsToLiveCameraAndLeavesOtherStatesUnchanged()
        {
            Vector2 ordinary = new Vector2(2200f, 200f);
            Vector3 owner = new Vector3(2100f, 0f, 350f);
            Vector2 ownerPivot = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                9997, 1, 0, true, ordinary, true, 100f, 80f,
                40f, 20f, 1.5f, owner, 0f, -3.25f, 2044.75f,
                out bool ownerFlip);
            Assert.That(ownerFlip, Is.False);
            Assert.That(ownerPivot.x, Is.EqualTo(1969.75f).Within(0.0001f));

            Vector2 fallback = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                9997, 0, 0, true, ordinary, true, 100f, 80f,
                40f, 20f, 1.5f, owner, 0f, -3.25f, 2044.75f,
                out bool fallbackFlip);
            Assert.That(fallbackFlip, Is.True);
            Assert.That(fallback.x, Is.EqualTo(1968.75f).Within(0.0001f));
            Assert.That(fallback.y, Is.EqualTo(ordinary.y));

            Vector2 otherState = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                0, 1, 0, true, ordinary, true, 100f, 80f,
                40f, 20f, 1.5f, owner, 0f, -3.25f, 2044.75f,
                out bool otherFlip);
            Assert.That(otherFlip, Is.True);
            Assert.That(otherState, Is.EqualTo(ordinary));

            Vector2 leftEdge = LF2ObjectRenderer.ResolveState9997BodyPivotPixels(
                9997, 1, 0, true, ordinary, true, 100f, 80f,
                40f, 20f, 1.5f, new Vector3(0f, 0f, 350f),
                0f, -3.25f, 2044.75f, out _);
            Assert.That(leftEdge.x, Is.EqualTo(71.75f).Within(0.0001f));
        }

        [Test]
        public void State9997ModeGate_FrozenFrameCopyAndResetPreserveBoundary()
        {
            var source = new BattlePresentationFrame();
            Reset(source, BattleCommonVisualCatalog.Empty);
            ModeEtcProperty.SetValue(source, 1);
            var frozen = new BattlePresentationFrame();
            CopyFrameMethod.Invoke(frozen, new object[] { source, null });
            Assert.That(ModeEtcProperty.GetValue(frozen), Is.EqualTo(1));
            Reset(frozen, BattleCommonVisualCatalog.Empty);
            Assert.That(ModeEtcProperty.GetValue(frozen), Is.EqualTo(0));
        }

        [Test]
        public void LowHpBPoint_CentralCommandFollowsBodyAndCurrentContentDefaults()
        {
            var points = new BattleBloodPointCatalog(new[]
            {
                new BattleBloodPointValue(7, 9),
                new BattleBloodPointValue(11, 13),
            });
            var coordinator = new BattlePresentationCoordinator();
            coordinator.SetMode(BattlePresentationBackendMode.CentralOnly);

            foreach (bool facingLeft in new[] { false, true })
            {
                var frame = new BattlePresentationFrame();
                Reset(frame, BattleCommonVisualCatalog.Empty);
                BattlePresentationEntitySnapshot entity = CreateBleedEntity(
                    points, facingLeft, 33, true, 0);
                Assert.That(entity.WithPresentationBaseOrder(104).BloodPoints,
                    Is.SameAs(points));
                Assert.That(entity.WithResolvedSprite(80f, 80f, Rect.zero,
                    new Vector2(0.5f, 0f), true, default, null).BloodPoints,
                    Is.SameAs(points));
                AddEntity(frame, entity);
                coordinator.BuildCommandsForSelfCheck(frame);

                int bodyIndex = -1;
                int markCount = 0;
                BattleRenderCommand body = default;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type == BattleRenderCommandType.Entity)
                    {
                        bodyIndex = index;
                        body = command;
                    }
                    if (command.Type != BattleRenderCommandType.BleedMark)
                        continue;

                    Assert.That(index, Is.GreaterThan(bodyIndex));
                    Assert.That(command.SortOrder, Is.EqualTo(body.SortOrder + 1));
                    Assert.That(command.Size, Is.EqualTo(new Vector2(1f, 3f)));
                    Assert.That(command.Color, Is.EqualTo(new Color32(255, 0, 0, 255)));
                    Assert.That(command.SpriteDescriptor.LogicalResourceKey,
                        Is.EqualTo(BattleVisualResourceKey.CommonSolid));
                    int pointX = markCount == 0 ? 7 : 11;
                    int pointY = markCount == 0 ? 9 : 13;
                    float width = body.Size.x * NTSDRenderSpace.BattleVisualScale *
                                  NTSDRenderSpace.UnitsPerPixelX;
                    float height = body.Size.y * NTSDRenderSpace.BattleVisualScale *
                                   NTSDRenderSpace.UnitsPerPixelY;
                    float left = body.Position.x - body.Pivot.x * width;
                    float top = body.Position.y + (1f - body.Pivot.y) * height;
                    float expectedX = left + (facingLeft ? 1 - pointX : pointX) *
                                      NTSDRenderSpace.BattleVisualScale *
                                      NTSDRenderSpace.UnitsPerPixelX;
                    float expectedY = top - pointY * NTSDRenderSpace.BattleVisualScale *
                                      NTSDRenderSpace.UnitsPerPixelY;
                    Assert.That(command.Position.x, Is.EqualTo(expectedX).Within(0.0001f));
                    Assert.That(command.Position.y, Is.EqualTo(expectedY).Within(0.0001f));
                    markCount++;
                }
                Assert.That(bodyIndex, Is.GreaterThanOrEqualTo(0));
                Assert.That(markCount, Is.EqualTo(2));
            }

            foreach (var sample in new[]
            {
                (health: 34, visible: true, action: 0),
                (health: 33, visible: false, action: 0),
                (health: 33, visible: true, action: 1000),
            })
            {
                var frame = new BattlePresentationFrame();
                Reset(frame, BattleCommonVisualCatalog.Empty);
                AddEntity(frame, CreateBleedEntity(points, false, sample.health,
                    sample.visible, sample.action));
                coordinator.BuildCommandsForSelfCheck(frame);
                for (int index = 0; index < frame.CommandCount; index++)
                    Assert.That(frame.GetCommand(index).Type,
                        Is.Not.EqualTo(BattleRenderCommandType.BleedMark));
            }

            var belowThreshold = new BattlePresentationFrame();
            Reset(belowThreshold, BattleCommonVisualCatalog.Empty);
            AddEntity(belowThreshold, CreateBleedEntity(points, false, 32, true, 0));
            coordinator.BuildCommandsForSelfCheck(belowThreshold);
            int belowMarkCount = 0;
            for (int index = 0; index < belowThreshold.CommandCount; index++)
            {
                if (belowThreshold.GetCommand(index).Type == BattleRenderCommandType.BleedMark)
                    belowMarkCount++;
            }
            Assert.That(belowMarkCount, Is.EqualTo(2));
        }

        [Test]
        public void LowHpBPoint_LegacyCommandFollowsBodyWithoutChangingShadowBuild()
        {
            var points = new BattleBloodPointCatalog(new[]
            {
                new BattleBloodPointValue(7, 9),
            });
            foreach (BattlePresentationBackendMode mode in new[]
            {
                BattlePresentationBackendMode.LegacyOnly,
                BattlePresentationBackendMode.CentralShadowBuild,
            })
            {
                var coordinator = new BattlePresentationCoordinator();
                coordinator.SetMode(mode);
                var frame = new BattlePresentationFrame();
                Reset(frame, BattleCommonVisualCatalog.Empty);
                AddEntity(frame, CreateBleedEntity(points, false, 33, true, 0));
                coordinator.BuildCommandsForSelfCheck(frame);

                int bodyIndex = -1;
                int markIndex = -1;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type == BattleRenderCommandType.Entity)
                        bodyIndex = index;
                    if (command.Type == BattleRenderCommandType.BleedMark)
                        markIndex = index;
                }

                Assert.That(bodyIndex, Is.GreaterThanOrEqualTo(0));
                if (mode == BattlePresentationBackendMode.LegacyOnly)
                {
                    Assert.That(markIndex, Is.GreaterThan(bodyIndex));
                    Assert.That(frame.GetCommand(markIndex).SortOrder,
                        Is.EqualTo(frame.GetCommand(bodyIndex).SortOrder + 1));
                }
                else
                {
                    Assert.That(markIndex, Is.EqualTo(-1));
                }
            }
        }

        [Test]
        public void LegacyBleedOwner_PreparesSolidMarkersAndClearsIdempotently()
        {
            GameObject host = new GameObject("LegacyBleedOwnerTest");
            try
            {
                BattleEntityOverlayRenderer owner =
                    host.AddComponent<BattleEntityOverlayRenderer>();
                owner.PrepareBattleCapacity(3);
                SpriteRenderer[] markers =
                    host.GetComponentsInChildren<SpriteRenderer>(true);
                Assert.That(markers.Length, Is.EqualTo(3));
                foreach (SpriteRenderer marker in markers)
                {
                    Assert.That(marker.sprite, Is.Not.Null);
                    Assert.That(marker.sprite.texture,
                        Is.SameAs(Texture2D.whiteTexture));
                    Assert.That(marker.gameObject.activeSelf, Is.False);
                }

                owner.PrepareBattleCapacity(3);
                Assert.That(host.GetComponentsInChildren<SpriteRenderer>(true).Length,
                    Is.EqualTo(3));
                owner.StopForBattleShutdown();
                owner.StopForBattleShutdown();
                Assert.That(owner.RejectedBleedMarkCountForDiagnostics,
                    Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [TestCase(false, 23)]
        [TestCase(true, -50)]
        public void PlatformShadowOffset_MovesShadowOnlyAndSurvivesSnapshotCopies(
            bool fixedView, int sourceOffset)
        {
            BattleSpatialProjection projection = fixedView
                ? BattleSpatialProjection.FromReferenceViewport(2048, 1152)
                : BattleSpatialProjection.Identity;
            BattleCommonVisualCatalog catalog = CreateCatalog(0, includeShadow: true);
            var frame = new BattlePresentationFrame();
            Reset(frame, catalog);
            frame.SpatialProjection = projection;
            BattlePresentationEntitySnapshot entity = CreatePlatformShadowEntity(sourceOffset);
            Assert.That(entity.WithPresentationBaseOrder(104).RenderShadowOffset10C,
                Is.EqualTo(sourceOffset));
            Assert.That(entity.WithResolvedSprite(1f, 1f, Rect.zero, Vector2.zero,
                false, default, null).RenderShadowOffset10C, Is.EqualTo(sourceOffset));
            AddEntity(frame, entity);

            var coordinator = new BattlePresentationCoordinator();
            coordinator.BuildCommandsForSelfCheck(frame);
            var baseline = new BattlePresentationFrame();
            Reset(baseline, catalog);
            baseline.SpatialProjection = projection;
            AddEntity(baseline, CreatePlatformShadowEntity(0));
            coordinator.BuildCommandsForSelfCheck(baseline);

            Assert.That(frame.CommandCount, Is.EqualTo(baseline.CommandCount));
            int shadowCount = 0;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand actual = frame.GetCommand(index);
                BattleRenderCommand original = baseline.GetCommand(index);
                Assert.That(actual.Type, Is.EqualTo(original.Type));
                Assert.That(actual.ZInt, Is.EqualTo(original.ZInt));
                Assert.That(actual.SortOrder, Is.EqualTo(original.SortOrder));
                if (actual.Type == BattleRenderCommandType.Shadow)
                {
                    shadowCount++;
                    Assert.That(actual.Position, Is.EqualTo(
                        NTSDRenderSpace.CaptureViewportTransform().ScreenPixelToWorld(
                            120, 180f + (float)(sourceOffset *
                                (fixedView ? 1152.0 / 730.0 : 1.0)), 0f)));
                }
                else if (actual.Type != BattleRenderCommandType.OverlayGlyph)
                {
                    Assert.That(actual.Position, Is.EqualTo(original.Position));
                }
            }
            Assert.That(shadowCount, Is.EqualTo(1));
        }

        [TestCase(false, 23)]
        [TestCase(true, -50)]
        public void PlatformShadowOffset_LegacyRendererUsesSameVisualOffset(
            bool fixedView, int sourceOffset)
        {
            var shadowObject = new GameObject("Platform Shadow Offset Test");
            var actor = new LF2Character();
            var world = new SimulationWorld();
            try
            {
                if (fixedView)
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                actor.SetRequiredRuntimeSlot(0);
                world.Register(actor);
                SpriteRenderer renderer = shadowObject.AddComponent<SpriteRenderer>();
                float worldDepth = renderer.transform.position.z;
                actor.ObjectId = 2;
                actor.Frame.D = new LF2FrameData { state = 0 };
                actor.Runtime.LinkState = 0;
                actor.Runtime.HitStop = 0;
                actor.Runtime.XInt = 120;
                actor.Runtime.ZInt = 180;
                actor.Runtime.RenderShadowOffset10C = sourceOffset;
                actor.SetShadowRenderer(renderer);

                actor.UpdateShadow();
                Assert.That(renderer.enabled, Is.True);
                Assert.That(renderer.transform.position, Is.EqualTo(
                    NTSDRenderSpace.SnapPresentationWorldPosition(
                        NTSDRenderSpace.ScreenPixelToPresentationWorld(
                            120f, 180f + (float)(sourceOffset *
                                (fixedView ? 1152.0 / 730.0 : 1.0)), worldDepth))));

                actor.Runtime.RenderShadowOffset10C = 0;
                actor.UpdateShadow();
                Assert.That(renderer.transform.position, Is.EqualTo(
                    NTSDRenderSpace.SnapPresentationWorldPosition(
                        NTSDRenderSpace.ScreenPixelToPresentationWorld(
                            120f, 180f, worldDepth))));
                Assert.That(actor.Runtime.ZInt, Is.EqualTo(180));
            }
            finally
            {
                actor.SetShadowRenderer(null);
                world.Unregister(actor);
                UnityEngine.Object.DestroyImmediate(shadowObject);
            }
        }

        [Test]
        public void OptimizedWriter_MatchesReferenceForWords5ComCounterAndBracket()
        {
            BattleCommonVisualCatalog catalog = CreateCatalog(0);
            var frame = new BattlePresentationFrame();
            Reset(frame, catalog);
            char[,] labels = GetLabels(frame);
            int[] labelState = GetLabelState(frame);
            labels[0, 0] = 'L';
            labels[0, 1] = 'F';
            labelState[0] = -1;

            AddEntity(frame, CreateOverlayEntity(
                new RuntimeEntityHandle(20, 7),
                100,
                20,
                1,
                5,
                0,
                1,
                120,
                180));
            AddEntity(frame, CreateOverlayEntity(
                new RuntimeEntityHandle(0, 9),
                101,
                0,
                12,
                2,
                0,
                31,
                240,
                210));

            List<BattleRenderCommand> expected = BuildReference(frame, catalog);
            var coordinator = new BattlePresentationCoordinator();
            coordinator.BuildCommandsForSelfCheck(frame);

            AssertFrameEquals(expected, frame);
            Assert.That(frame.CommandCount, Is.EqualTo(3 + 3 + 4));
            Assert.That(frame.GetCommand(0).EffectivePic, Is.EqualTo('C'));
            Assert.That(frame.GetCommand(1).EffectivePic, Is.EqualTo('o'));
            Assert.That(frame.GetCommand(2).EffectivePic, Is.EqualTo('m'));
            Assert.That(frame.GetCommand(3).EffectivePic, Is.EqualTo('x'));
            Assert.That(frame.GetCommand(6).EffectivePic, Is.EqualTo('['));
            Assert.That(frame.GetCommand(9).EffectivePic, Is.EqualTo(']'));
        }

        [Test]
        public void GlyphTemplates_AllKeysMatchBindingsAndCatalogReplacementInvalidatesEpoch()
        {
            BattleCommonVisualCatalog first = CreateCatalog(0);
            BattleCommonVisualCatalog replacement = CreateCatalog(17);
            var coordinator = new BattlePresentationCoordinator();
            var handle = new RuntimeEntityHandle(42, 3);

            for (int sheet = 0; sheet < BattleCommonVisualCatalog.WordSheetCount; sheet++)
            {
                for (int code = 0; code < BattleCommonVisualCatalog.WordGlyphsPerSheet; code++)
                {
                    Assert.That(
                        coordinator.TryCreateWordGlyphCommandForSelfCheck(
                            first,
                            sheet,
                            code,
                            handle,
                            9001,
                            222,
                            42,
                            102,
                            code,
                            new Vector3(code, sheet, 0f),
                            out BattleRenderCommand actual),
                        Is.True);
                    Assert.That(first.TryGetWordGlyph(sheet, code, out BattleCommonVisualBinding binding),
                        Is.True);
                    AssertStaticTemplateFields(binding, sheet, code, actual);
                }
            }

            Assert.That(
                coordinator.TryCreateWordGlyphCommandForSelfCheck(
                    replacement,
                    5,
                    'C',
                    new RuntimeEntityHandle(42, 4),
                    9002,
                    333,
                    42,
                    202,
                    11,
                    new Vector3(3f, 4f, 0f),
                    out BattleRenderCommand replaced),
                Is.True);
            Assert.That(replacement.TryGetWordGlyph(5, 'C', out BattleCommonVisualBinding replacementBinding),
                Is.True);
            AssertStaticTemplateFields(replacementBinding, 5, 'C', replaced);
            Assert.That(replaced.SpriteDescriptor.PixelRect,
                Is.Not.EqualTo(first.TryGetWordGlyph(5, 'C', out BattleCommonVisualBinding oldBinding)
                    ? oldBinding.PixelRect
                    : Rect.zero));
            Assert.That(replaced.Handle.Generation, Is.EqualTo(4));
            Assert.That(replaced.StableId, Is.EqualTo(9002));
        }

        [Test]
        public void Writer_ReservesProvenCapacityAndWarmedBuildAllocatesZeroBytes()
        {
            BattleCommonVisualCatalog catalog = CreateCatalog(0);
            var frame = new BattlePresentationFrame();
            Reset(frame, catalog);
            const int entityCount = 1000;
            for (int index = 0; index < entityCount; index++)
            {
                AddEntity(frame, CreateOverlayEntity(
                    new RuntimeEntityHandle(20 + index, (uint)(index + 1)),
                    10000 + index,
                    20 + index,
                    1,
                    5,
                    0,
                    1,
                    100 + index,
                    180));
            }

            var coordinator = new BattlePresentationCoordinator();
            coordinator.BuildCommandsForSelfCheck(frame);
            Assert.That(frame.CommandCount, Is.EqualTo(entityCount * 3));
            Assert.That(
                frame.CommandCapacity,
                Is.GreaterThanOrEqualTo(
                    entityCount * (2 + BattleEntityOverlayLayout.MaximumGlyphCount)));

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 16; iteration++)
                coordinator.BuildCommandsForSelfCheck(frame);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(frame.CommandCount, Is.EqualTo(entityCount * 3));
        }

        [Test]
        public void CentralOnly_SpecialComUsesOneCompositeCommand_LegacyKeepsThreeGlyphs()
        {
            BattleCommonVisualCatalog catalog = CreateCatalog(0, includeSpecialCom: true);
            var frame = new BattlePresentationFrame();
            Reset(frame, catalog);
            AddEntity(frame, CreateOverlayEntity(
                new RuntimeEntityHandle(20, 7),
                100,
                20,
                1,
                5,
                0,
                1,
                120,
                180));

            var centralCoordinator = new BattlePresentationCoordinator();
            centralCoordinator.SetMode(BattlePresentationBackendMode.CentralOnly);
            centralCoordinator.BuildCommandsForSelfCheck(frame);

            Assert.That(frame.CommandCount, Is.EqualTo(1));
            BattleRenderCommand composite = frame.GetCommand(0);
            Assert.That(composite.Type, Is.EqualTo(BattleRenderCommandType.OverlayGlyph));
            Assert.That(composite.SpriteDescriptor.LogicalResourceKey,
                Is.EqualTo(BattleVisualResourceKey.CommonSpecialCom));
            Assert.That(composite.Size,
                Is.EqualTo(new Vector2(
                    BattleCommonVisualCatalog.SpecialComWidth,
                    BattleCommonVisualCatalog.SpecialComHeight)));
            Assert.That(composite.Pivot,
                Is.EqualTo(BattleCommonVisualCatalog.GetSpecialComPivotNormalized()));

            var runtime = new BattleEntityOverlayRuntimeSlot(
                20,
                1,
                5,
                0,
                1,
                0,
                120,
                0,
                180,
                0,
                0,
                0);
            Assert.That(
                BattleEntityOverlayLayout.TryGetSpecialComLayout(
                    in runtime,
                    out int labelX,
                    out int labelY,
                    out _),
                Is.True);
            Assert.That(composite.Position,
                Is.EqualTo(NTSDRenderSpace.ScreenPixelToWorld(labelX, labelY, 0f)));

            var legacyCoordinator = new BattlePresentationCoordinator();
            legacyCoordinator.BuildCommandsForSelfCheck(frame);
            Assert.That(frame.CommandCount, Is.EqualTo(3));
            Assert.That(frame.GetCommand(0).EffectivePic, Is.EqualTo('C'));
            Assert.That(frame.GetCommand(1).EffectivePic, Is.EqualTo('o'));
            Assert.That(frame.GetCommand(2).EffectivePic, Is.EqualTo('m'));
        }

        [Test]
        public void CentralOnly_GenericComUsesRelationSheetComposite_LegacyKeepsThreeGlyphs()
        {
            const int relationSheet = 2;
            BattleCommonVisualCatalog catalog = CreateCatalog(0, includeAllComLabels: true);
            var frame = new BattlePresentationFrame();
            Reset(frame, catalog);
            AddEntity(frame, CreateOverlayEntity(
                new RuntimeEntityHandle(20, 8),
                101,
                20,
                1,
                relationSheet,
                0,
                31,
                240,
                210));

            var centralCoordinator = new BattlePresentationCoordinator();
            centralCoordinator.SetMode(BattlePresentationBackendMode.CentralOnly);
            centralCoordinator.BuildCommandsForSelfCheck(frame);

            Assert.That(frame.CommandCount, Is.EqualTo(1));
            BattleRenderCommand composite = frame.GetCommand(0);
            Assert.That(composite.Type, Is.EqualTo(BattleRenderCommandType.OverlayGlyph));
            Assert.That(composite.SpriteDescriptor.LogicalResourceKey,
                Is.EqualTo(BattleVisualResourceKey.CommonComLabel(relationSheet)));
            Assert.That(composite.Size,
                Is.EqualTo(new Vector2(
                    BattleCommonVisualCatalog.SpecialComWidth,
                    BattleCommonVisualCatalog.SpecialComHeight)));
            Assert.That(composite.Pivot,
                Is.EqualTo(BattleCommonVisualCatalog.GetSpecialComPivotNormalized()));

            var runtime = new BattleEntityOverlayRuntimeSlot(
                20,
                1,
                relationSheet,
                0,
                31,
                0,
                240,
                0,
                210,
                0,
                0,
                0);
            var glyphs = new BattleEntityOverlayGlyph[BattleEntityOverlayLayout.MaximumGlyphCount];
            Assert.That(
                BattleEntityOverlayLayout.TryBuild(
                    in runtime,
                    GetLabels(frame),
                    GetLabelState(frame),
                    glyphs,
                    out int glyphCount),
                Is.True);
            Assert.That(glyphCount, Is.EqualTo(3));
            Assert.That(composite.Position,
                Is.EqualTo(NTSDRenderSpace.ScreenPixelToWorld(
                    glyphs[0].PixelX,
                    glyphs[0].PixelY,
                    0f)));

            var legacyCoordinator = new BattlePresentationCoordinator();
            legacyCoordinator.BuildCommandsForSelfCheck(frame);
            Assert.That(frame.CommandCount, Is.EqualTo(3));
            Assert.That(frame.GetCommand(0).EffectivePic, Is.EqualTo('C'));
            Assert.That(frame.GetCommand(1).EffectivePic, Is.EqualTo('o'));
            Assert.That(frame.GetCommand(2).EffectivePic, Is.EqualTo('m'));
        }

        [Test]
        public void FifthBattleGroupNameplate_UsesWords5WhileReviveCounterUsesWords0()
        {
            var labels = new char[
                BattleEntityOverlayLayout.SlotCount,
                BattleEntityOverlayLayout.SlotLabelCharacterCapacity];
            labels[0, 0] = 'A';
            var states = new int[BattleEntityOverlayLayout.SlotCount];
            var glyphs = new BattleEntityOverlayGlyph[
                BattleEntityOverlayLayout.MaximumGlyphCount];
            var slot = new BattleEntityOverlayRuntimeSlot(
                0, 2, 5, 0, 2, 0, 200, 0, 300, 0, 0, 0);

            Assert.That(BattleEntityOverlayLayout.TryBuild(
                in slot, labels, states, glyphs, out int count), Is.True);
            Assert.That(count, Is.EqualTo(3));
            Assert.That(glyphs[0].CharCode, Is.EqualTo('x'));
            Assert.That(glyphs[0].SheetIndex, Is.EqualTo(0));
            Assert.That(glyphs[1].CharCode, Is.EqualTo('2'));
            Assert.That(glyphs[1].SheetIndex, Is.EqualTo(0));
            Assert.That(glyphs[2].CharCode, Is.EqualTo('A'));
            Assert.That(glyphs[2].SheetIndex, Is.EqualTo(5));
        }

        [Test]
        public void OverlayLayoutSelfCheck_UsesFormalInclusiveRightEdge()
        {
            MethodInfo check = typeof(BattleRuntimeSelfCheck).GetMethod(
                "CheckBattleEntityOverlayLayoutContracts",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(check, Is.Not.Null);
            Assert.DoesNotThrow(() => check.Invoke(null, null));
        }

        [Test]
        public void ReviveLivesCounter_UsesVisibleCameraLeftMarginWithoutHidingNameplate()
        {
            var labels = new char[
                BattleEntityOverlayLayout.SlotCount,
                BattleEntityOverlayLayout.SlotLabelCharacterCapacity];
            labels[0, 0] = 'P';
            var labelState = new int[BattleEntityOverlayLayout.SlotCount];
            var glyphs = new BattleEntityOverlayGlyph[
                BattleEntityOverlayLayout.MaximumGlyphCount];
            var beyondLeft = new BattleEntityOverlayRuntimeSlot(
                0, 2, 1, 0, 2, 0, 105, 0, 287, 0, 20, 0,
                visibleLeftPixel: 100, visibleRightPixel: 500);
            var atMargin = new BattleEntityOverlayRuntimeSlot(
                0, 2, 1, 0, 2, 0, 106, 0, 287, 0, 20, 0,
                visibleLeftPixel: 100, visibleRightPixel: 500);
            var suppressedByMode = new BattleEntityOverlayRuntimeSlot(
                0, 2, 1, 0, 2, 0, 106, 0, 287, 0, 20, 0,
                visibleLeftPixel: 100, visibleRightPixel: 500,
                selectedModeReviveLivesGate54: 3);

            Assert.That(BattleEntityOverlayLayout.TryBuild(
                in beyondLeft, labels, labelState, glyphs, out int beyondCount),
                Is.True);
            Assert.That(beyondCount, Is.EqualTo(1));
            Assert.That(glyphs[0].Type, Is.EqualTo(BattleEntityOverlayGlyphType.Label));

            Assert.That(BattleEntityOverlayLayout.TryBuild(
                in atMargin, labels, labelState, glyphs, out int marginCount),
                Is.True);
            Assert.That(marginCount, Is.EqualTo(3));
            Assert.That(glyphs[0].Type, Is.EqualTo(BattleEntityOverlayGlyphType.Counter));
            Assert.That(glyphs[0].PixelX, Is.EqualTo(73));
            Assert.That(glyphs[2].Type, Is.EqualTo(BattleEntityOverlayGlyphType.Label));

            Assert.That(BattleEntityOverlayLayout.TryBuild(
                in suppressedByMode, labels, labelState, glyphs,
                out int suppressedCount), Is.True);
            Assert.That(suppressedCount, Is.EqualTo(1));
            Assert.That(glyphs[0].Type, Is.EqualTo(BattleEntityOverlayGlyphType.Label));
        }

        [Test]
        public void CentralNameplate_StaysWithVisibleBodyBeyondLegacyViewportEdge()
        {
            var cameraObject = new GameObject("Nameplate Viewport Clamp Test Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.76f;
            camera.aspect = 16f / 9f;
            camera.transform.position = new Vector3(-1.79f, -4.8f, -10f);
            NTSDRenderSpace.BindWorldCamera(camera);
            try
            {
                var frame = new BattlePresentationFrame();
                Reset(frame, CreateCatalog(0));
                char[,] labels = GetLabels(frame);
                const string name = "Remie";
                for (int index = 0; index < name.Length; index++)
                    labels[0, index] = name[index];
                AddEntity(frame, CreateOverlayEntity(
                    new RuntimeEntityHandle(0, 9), 101, 0, 1, 1, 0, 2,
                    1000, 287));

                var coordinator = new BattlePresentationCoordinator();
                coordinator.SetMode(BattlePresentationBackendMode.CentralOnly);
                coordinator.BuildCommandsForSelfCheck(frame);

                Assert.That(frame.CommandCount, Is.EqualTo(name.Length));
                Assert.That(frame.GetCommand(0).Type,
                    Is.EqualTo(BattleRenderCommandType.OverlayGlyph));
                Assert.That(frame.GetCommand(0).Position,
                    Is.EqualTo(NTSDRenderSpace.CaptureViewportTransform()
                        .ScreenPixelToWorld(978, 290, 0f)));

                NTSDRenderSpace.ViewportTransformSnapshot viewport =
                    NTSDRenderSpace.CaptureViewportTransform();
                float halfWidth = camera.orthographicSize * camera.aspect;
                int visibleLeft = Mathf.CeilToInt(
                    (camera.transform.position.x - halfWidth - viewport.Left) /
                    viewport.UnitsPerPixelX);
                int visibleRight = Mathf.FloorToInt(
                    (camera.transform.position.x + halfWidth - viewport.Left) /
                    viewport.UnitsPerPixelX);

                var leftFrame = new BattlePresentationFrame();
                Reset(leftFrame, CreateCatalog(0));
                char[,] leftLabels = GetLabels(leftFrame);
                for (int index = 0; index < name.Length; index++)
                    leftLabels[0, index] = name[index];
                int leftBodyX = visibleLeft + 1;
                AddEntity(leftFrame, CreateOverlayEntity(
                    new RuntimeEntityHandle(0, 10), 102, 0, 2, 1, 0, 2,
                    leftBodyX, 287));
                coordinator.BuildCommandsForSelfCheck(leftFrame);
                Assert.That(leftFrame.CommandCount, Is.EqualTo(name.Length + 2));
                Assert.That(leftFrame.GetCommand(0).Position,
                    Is.EqualTo(viewport.ScreenPixelToWorld(leftBodyX - 13, 280, 0f)),
                    "revive counter must stay at its source position");
                Assert.That(leftFrame.GetCommand(2).Position,
                    Is.EqualTo(viewport.ScreenPixelToWorld(visibleLeft, 290, 0f)),
                    "nameplate must clamp to the visible left edge");

                var rightFrame = new BattlePresentationFrame();
                Reset(rightFrame, CreateCatalog(0));
                char[,] rightLabels = GetLabels(rightFrame);
                for (int index = 0; index < name.Length; index++)
                    rightLabels[0, index] = name[index];
                AddEntity(rightFrame, CreateOverlayEntity(
                    new RuntimeEntityHandle(0, 11), 103, 0, 1, 1, 0, 2,
                    visibleRight - 1, 287));
                coordinator.BuildCommandsForSelfCheck(rightFrame);
                Assert.That(rightFrame.CommandCount, Is.EqualTo(name.Length));
                Assert.That(rightFrame.GetCommand(0).Position,
                    Is.EqualTo(viewport.ScreenPixelToWorld(
                        visibleRight - name.Length *
                        BattleEntityOverlayLayout.GlyphAdvance - 1,
                        290, 0f)),
                    "nameplate must clamp to the visible right edge");
            }
            finally
            {
                NTSDRenderSpace.ClearBoundWorldCamera(camera);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void CentralNameplate_UsesCapturedPlatformHeight()
        {
            BattleCommonVisualCatalog catalog = CreateCatalog(0, includeAllComLabels: true);
            int[] offsets = { 0, -5 };
            for (int index = 0; index < offsets.Length; index++)
            {
                var frame = new BattlePresentationFrame();
                Reset(frame, catalog);
                AddEntity(frame, CreateOverlayEntity(
                    new RuntimeEntityHandle(51, 8), 101, 51, 1, 2, 0, 2,
                    205, 287, offsets[index]));
                var coordinator = new BattlePresentationCoordinator();
                coordinator.SetMode(BattlePresentationBackendMode.CentralOnly);
                coordinator.BuildCommandsForSelfCheck(frame);

                Assert.That(frame.CommandCount, Is.EqualTo(1));
                Assert.That(frame.GetCommand(0).Type,
                    Is.EqualTo(BattleRenderCommandType.OverlayGlyph));
                Assert.That(frame.GetCommand(0).Position,
                    Is.EqualTo(NTSDRenderSpace.ScreenPixelToWorld(
                        205 - (3 * BattleEntityOverlayLayout.GlyphAdvance / 2),
                        287 + offsets[index] + 3, 0f)));
            }

            var counterAndLabel = new BattleEntityOverlayRuntimeSlot(
                0, 2, 2, 0, 2, 0, 205, 0, 287, 0, 0, 0, -5);
            var glyphs = new BattleEntityOverlayGlyph[
                BattleEntityOverlayLayout.MaximumGlyphCount];
            var labels = new char[
                BattleEntityOverlayLayout.SlotCount,
                BattleEntityOverlayLayout.SlotLabelCharacterCapacity];
            labels[0, 0] = 'P';
            Assert.That(BattleEntityOverlayLayout.TryBuild(
                in counterAndLabel,
                labels,
                new int[BattleEntityOverlayLayout.SlotCount],
                glyphs,
                out int glyphCount), Is.True);
            Assert.That(glyphCount, Is.EqualTo(3));
            Assert.That(glyphs[0].PixelY, Is.EqualTo(287 - 7));
            Assert.That(glyphs[2].PixelY, Is.EqualTo(287 - 5 + 3));
        }

        [Test]
        public void SelectedModeGate_ControlsCentralLivesAndNameplateCommands()
        {
            int[] gates = { 0, 1, 2, 3, 4 };
            int[] ordinaryCounts = { 3, 3, 2, 1, 0 };
            int[] highSlotCounts = { 5, 2, 2, 0, 0 };
            var coordinator = new BattlePresentationCoordinator();
            coordinator.SetMode(BattlePresentationBackendMode.CentralOnly);
            for (int index = 0; index < gates.Length; index++)
            {
                var ordinaryFrame = new BattlePresentationFrame();
                Reset(ordinaryFrame, CreateCatalog(0));
                SetModeOverlayGate(ordinaryFrame, gates[index]);
                GetLabels(ordinaryFrame)[0, 0] = 'P';
                AddEntity(ordinaryFrame, CreateOverlayEntity(
                    new RuntimeEntityHandle(0, (uint)(80 + index)), 201 + index,
                    0, 2, 1, 0, 2, 205, 287));
                coordinator.BuildCommandsForSelfCheck(ordinaryFrame);
                Assert.That(ordinaryFrame.CommandCount, Is.EqualTo(ordinaryCounts[index]),
                    $"ordinary slot, gate {gates[index]}");

                var highFrame = new BattlePresentationFrame();
                Reset(highFrame, CreateCatalog(0));
                SetModeOverlayGate(highFrame, gates[index]);
                AddEntity(highFrame, CreateOverlayEntity(
                    new RuntimeEntityHandle(20, (uint)(90 + index)), 211 + index,
                    20, 2, 2, 0, 2, 205, 287));
                coordinator.BuildCommandsForSelfCheck(highFrame);
                Assert.That(highFrame.CommandCount, Is.EqualTo(highSlotCounts[index]),
                    $"high slot, gate {gates[index]}");

                var compositeFrame = new BattlePresentationFrame();
                Reset(compositeFrame, CreateCatalog(0, includeAllComLabels: true));
                SetModeOverlayGate(compositeFrame, gates[index]);
                AddEntity(compositeFrame, CreateOverlayEntity(
                    new RuntimeEntityHandle(20, (uint)(100 + index)), 221 + index,
                    20, 1, 2, 0, 2, 205, 287));
                coordinator.BuildCommandsForSelfCheck(compositeFrame);
                Assert.That(compositeFrame.CommandCount,
                    Is.EqualTo(gates[index] == 0 ? 1 : 0),
                    $"composite high slot, gate {gates[index]}");

                var specialFrame = new BattlePresentationFrame();
                Reset(specialFrame, CreateCatalog(0, includeSpecialCom: true));
                SetModeOverlayGate(specialFrame, gates[index]);
                AddEntity(specialFrame, CreateOverlayEntity(
                    new RuntimeEntityHandle(20, (uint)(110 + index)), 231 + index,
                    20, 1, 5, 0, 2, 205, 287));
                coordinator.BuildCommandsForSelfCheck(specialFrame);
                Assert.That(specialFrame.CommandCount,
                    Is.EqualTo(gates[index] == 0 ? 1 : 0),
                    $"special Com high slot, gate {gates[index]}");
            }
        }

        [Test]
        public void SelectedModeGate_FrozenFrameCopyAndResetDoNotRetainStaleValue()
        {
            var source = new BattlePresentationFrame();
            Reset(source, BattleCommonVisualCatalog.Empty);
            SetModeOverlayGate(source, 4);
            var frozen = new BattlePresentationFrame();
            Assert.That(CopyFrameMethod, Is.Not.Null);
            CopyFrameMethod.Invoke(frozen, new object[] { source, null });
            Assert.That(ModeOverlayGateProperty.GetValue(frozen), Is.EqualTo(4));
            Reset(frozen, BattleCommonVisualCatalog.Empty);
            Assert.That(ModeOverlayGateProperty.GetValue(frozen), Is.Zero);
        }

        [Test]
        public void DeferredSpriteMaterialization_BuildsCommandWithoutMutatingFrozenSnapshot()
        {
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                sprite = Sprite.Create(
                    texture,
                    new Rect(2f, 3f, 8f, 10f),
                    new Vector2(0.5f, 0f));
                var builder = new BattleSpriteCatalogBuilder();
                builder.Add(
                    901,
                    2,
                    "deferred-test.bmp",
                    texture,
                    new Rect(2f, 3f, 8f, 10f),
                    sprite);
                BattleSpriteCatalog spriteCatalog = builder.Publish();

                var frame = new BattlePresentationFrame();
                Reset(frame, CreateCatalog(0));
                Assert.That(BoundCatalogField, Is.Not.Null);
                BoundCatalogField.SetValue(frame, spriteCatalog);
                AddEntity(frame, new BattlePresentationEntitySnapshot(
                    new RuntimeEntityHandle(20, 7),
                    100,
                    901,
                    901,
                    2,
                    180,
                    20,
                    80,
                    0,
                    false,
                    0,
                    0,
                    100,
                    0,
                    0,
                    120,
                    0,
                    180f,
                    0f,
                    0,
                    0,
                    4f,
                    10f,
                    0f,
                    0f,
                    Vector2.zero,
                    Rect.zero,
                    Vector2.zero,
                    false,
                    false,
                    default,
                    0,
                    0,
                    true,
                    false));
                BattlePresentationEntitySnapshot frozenBefore = frame.GetEntity(0);
                var coordinator = new BattlePresentationCoordinator();
                coordinator.SetMode(BattlePresentationBackendMode.CentralOnly);

                coordinator.MaterializeCommands(frame, null);

                BattlePresentationEntitySnapshot frozenAfter = frame.GetEntity(0);
                Assert.That(frozenAfter.PixelWidth, Is.EqualTo(frozenBefore.PixelWidth));
                Assert.That(frozenAfter.PixelHeight, Is.EqualTo(frozenBefore.PixelHeight));
                Assert.That(frozenAfter.NormalizedUv, Is.EqualTo(frozenBefore.NormalizedUv));
                Assert.That(frozenAfter.Pivot, Is.EqualTo(frozenBefore.Pivot));
                Assert.That(frozenAfter.HasCatalogKey, Is.EqualTo(frozenBefore.HasCatalogKey));
                Assert.That(frame.CommandCount, Is.EqualTo(1));
                BattleRenderCommand command = frame.GetCommand(0);
                Assert.That(command.Type, Is.EqualTo(BattleRenderCommandType.Entity));
                Assert.That(command.Size, Is.EqualTo(new Vector2(8f, 10f)));
                Assert.That(command.NormalizedUv,
                    Is.EqualTo(new Rect(2f / 16f, 3f / 16f, 8f / 16f, 10f / 16f)));
                Assert.That(command.Pivot, Is.EqualTo(new Vector2(0.5f, 0f)));
                Assert.That(command.SpriteDescriptor.LogicalResourceKey,
                    Is.EqualTo(BattleVisualResourceKey.FromEntity(
                        new BattleSpriteKey(901, 2))));
                Assert.That(frame.RequiresCatalogPublicationBinding, Is.True);
            }
            finally
            {
                if (sprite != null)
                    UnityEngine.Object.DestroyImmediate(sprite);
                if (texture != null)
                    UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static List<BattleRenderCommand> BuildReference(
            BattlePresentationFrame frame,
            BattleCommonVisualCatalog catalog)
        {
            var result = new List<BattleRenderCommand>();
            var scratch = new BattleEntityOverlayGlyph[BattleEntityOverlayLayout.MaximumGlyphCount];
            for (int rank = 0; rank < frame.EntityCount; rank++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(rank);
                var runtime = new BattleEntityOverlayRuntimeSlot(
                    entity.RuntimeSlot,
                    entity.HP2Orig,
                    entity.RelationTeam,
                    entity.CurrentDatObjType,
                    entity.CurrentDatObjectId,
                    entity.HitStop,
                    entity.XInt,
                    entity.YInt,
                    entity.ZInt,
                    (int)entity.RenderOffsetX,
                    entity.CameraX,
                    (int)entity.CenterY,
                    entity.RenderShadowOffset10C);
                Assert.That(
                    BattleEntityOverlayLayout.TryBuild(
                        in runtime,
                        GetLabels(frame),
                        GetLabelState(frame),
                        scratch,
                        out int glyphCount),
                    Is.True);
                int localSequence = 0;
                for (int glyphIndex = 0; glyphIndex < glyphCount; glyphIndex++)
                {
                    BattleEntityOverlayGlyph glyph = scratch[glyphIndex];
                    if (!catalog.TryGetWordGlyph(
                            glyph.SheetIndex,
                            glyph.CharCode,
                            out BattleCommonVisualBinding binding))
                    {
                        continue;
                    }

                    result.Add(new BattleRenderCommand(
                        BattleRenderCommandType.OverlayGlyph,
                        entity.Handle,
                        entity.StableId,
                        glyph.SheetIndex,
                        glyph.CharCode,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        entity.PresentationBaseOrder + 2,
                        SortingLayer.NameToID("Object"),
                        localSequence++,
                        NTSDRenderSpace.ScreenPixelToWorld(glyph.PixelX, glyph.PixelY, 0f),
                        binding.PixelSize,
                        binding.Pivot,
                        binding.NormalizedUv,
                        binding.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            binding.SpriteInstanceId,
                            binding.TextureInstanceId,
                            binding.MaterialInstanceId,
                            binding.PixelRect,
                            binding.Pivot,
                            binding.Key),
                        motionAnchor: glyph.Type == BattleEntityOverlayGlyphType.Counter
                            ? BattlePresentationMotionAnchor.Body
                            : BattlePresentationMotionAnchor.Ground));
                }
            }

            return result;
        }

        private static BattlePresentationEntitySnapshot CreateOverlayEntity(
            RuntimeEntityHandle handle,
            int stableId,
            int runtimeSlot,
            int hp2Orig,
            int relationTeam,
            int objType,
            int oid,
            int x,
            int z,
            int renderShadowOffset10C = 0)
        {
            return new BattlePresentationEntitySnapshot(
                handle,
                stableId,
                oid,
                oid,
                999,
                z,
                runtimeSlot,
                1000 + runtimeSlot * 4,
                0,
                true,
                3005,
                0,
                hp2Orig,
                relationTeam,
                objType,
                x,
                0,
                z,
                0f,
                0,
                0,
                0f,
                0f,
                1f,
                1f,
                Vector2.zero,
                Rect.zero,
                Vector2.zero,
                false,
                false,
                default,
                0,
                0,
                false,
                false,
                renderShadowOffset10C: renderShadowOffset10C);
        }

        private static BattlePresentationEntitySnapshot CreatePlatformShadowEntity(int offset)
        {
            return new BattlePresentationEntitySnapshot(
                new RuntimeEntityHandle(20, 7), 100, 1, 1, 999,
                180, 20, 100, 0, true, 0, 0, 500, 1, 0,
                120, 0, 180f, 0f, 0, 0, 0f, 0f, 1f, 1f,
                Vector2.zero, Rect.zero, Vector2.zero, false, false,
                default, 0, 0, entityVisible: false, shadowVisible: true,
                renderShadowOffset10C: offset);
        }

        private static BattlePresentationEntitySnapshot CreateBleedEntity(
            BattleBloodPointCatalog points,
            bool facingLeft,
            int health,
            bool visible,
            int frameId)
        {
            return new BattlePresentationEntitySnapshot(
                new RuntimeEntityHandle(5, 1), 5, 9, 9, 0,
                180, 5, 100, 0, true, 0, 0, 500, 1, 0,
                120, 0, 180f, 0f, 0, 0, 40f, 30f, 80f, 80f,
                Vector2.zero, new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0f), facingLeft, true,
                default, 0, 0, entityVisible: visible, shadowVisible: false,
                frameId: frameId, currentHealth: health, maximumHealth: 99,
                bloodPoints: points);
        }

        private static BattleCommonVisualCatalog CreateCatalog(
            int variant,
            bool includeSpecialCom = false,
            bool includeAllComLabels = false,
            bool includeShadow = false)
        {
            Assert.That(BindingConstructor, Is.Not.Null);
            Assert.That(CatalogConstructor, Is.Not.Null);
            var glyphs = new BattleCommonVisualBinding[BattleCommonVisualCatalog.WordSheetCount][];
            for (int sheet = 0; sheet < glyphs.Length; sheet++)
            {
                glyphs[sheet] =
                    new BattleCommonVisualBinding[BattleCommonVisualCatalog.WordGlyphsPerSheet];
                for (int code = 0; code < glyphs[sheet].Length; code++)
                {
                    Rect rect = BattleCommonVisualCatalog.GetWordGlyphPixelRect(code);
                    rect.x += variant;
                    Rect uv = new Rect(
                        (rect.x + sheet) / 4096f,
                        rect.y / 4096f,
                        rect.width / 4096f,
                        rect.height / 4096f);
                    var state = new BattleSpriteRenderState(
                        new Color32(
                            (byte)(255 - variant),
                            (byte)(240 + sheet),
                            (byte)(200 + code % 55),
                            255),
                        false,
                        false,
                        SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
                    glyphs[sheet][code] = (BattleCommonVisualBinding)BindingConstructor.Invoke(
                        new object[]
                        {
                            BattleVisualResourceKey.CommonWordGlyph(sheet, code),
                            null,
                            null,
                            null,
                            rect,
                            uv,
                            rect.size,
                            BattleCommonVisualCatalog.GetWordGlyphPivotNormalized(),
                            state,
                        });
                }
            }

            BattleCommonVisualBinding shadow = includeShadow
                ? (BattleCommonVisualBinding)BindingConstructor.Invoke(new object[]
                {
                    BattleVisualResourceKey.CommonShadow,
                    null,
                    null,
                    null,
                    new Rect(0f, 0f, 32f, 16f),
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(32f, 16f),
                    new Vector2(0.5f, 0.5f),
                    BattleSpriteRenderState.Default(),
                })
                : null;

            if (!includeSpecialCom && !includeAllComLabels)
            {
                return (BattleCommonVisualCatalog)CatalogConstructor.Invoke(
                    new object[]
                    {
                        shadow,
                        Array.Empty<BattleCommonVisualBinding>(),
                        Array.Empty<Texture2D>(),
                        glyphs,
                        string.Empty,
                    });
            }


            if (includeAllComLabels)
            {
                Assert.That(CatalogWithComLabelsConstructor, Is.Not.Null);
                var comLabels = new BattleCommonVisualBinding[BattleCommonVisualCatalog.WordSheetCount];
                for (int sheetIndex = 0; sheetIndex < comLabels.Length; sheetIndex++)
                {
                    Rect rect = BattleCommonVisualCatalog.GetComLabelPixelRect(sheetIndex);
                    comLabels[sheetIndex] = (BattleCommonVisualBinding)BindingConstructor.Invoke(
                        new object[]
                        {
                            BattleVisualResourceKey.CommonComLabel(sheetIndex),
                            null,
                            null,
                            null,
                            rect,
                            new Rect(
                                0f,
                                rect.y / BattleCommonVisualCatalog.ComLabelsTextureHeight,
                                1f,
                                rect.height / BattleCommonVisualCatalog.ComLabelsTextureHeight),
                            rect.size,
                            BattleCommonVisualCatalog.GetSpecialComPivotNormalized(),
                            BattleSpriteRenderState.Default(),
                        });
                }

                return (BattleCommonVisualCatalog)CatalogWithComLabelsConstructor.Invoke(
                    new object[]
                    {
                        null,
                        Array.Empty<BattleCommonVisualBinding>(),
                        Array.Empty<Texture2D>(),
                        glyphs,
                        comLabels,
                        string.Empty,
                        false,
                    });
            }

            Assert.That(CatalogWithSpecialConstructor, Is.Not.Null);
            var specialRect = new Rect(
                0f,
                0f,
                BattleCommonVisualCatalog.SpecialComWidth,
                BattleCommonVisualCatalog.SpecialComHeight);
            var specialBinding = (BattleCommonVisualBinding)BindingConstructor.Invoke(
                new object[]
                {
                    BattleVisualResourceKey.CommonSpecialCom,
                    null,
                    null,
                    null,
                    specialRect,
                    new Rect(0f, 0f, 1f, 1f),
                    specialRect.size,
                    BattleCommonVisualCatalog.GetSpecialComPivotNormalized(),
                    BattleSpriteRenderState.Default(),
                });
            return (BattleCommonVisualCatalog)CatalogWithSpecialConstructor.Invoke(
                new object[]
                {
                    null,
                    Array.Empty<BattleCommonVisualBinding>(),
                    Array.Empty<Texture2D>(),
                    glyphs,
                    specialBinding,
                    string.Empty,
                });
        }

        private static void Reset(
            BattlePresentationFrame frame,
            BattleCommonVisualCatalog catalog)
        {
            Assert.That(ResetFrameMethod, Is.Not.Null);
            ResetFrameMethod.Invoke(frame, new object[] { 1, catalog });
        }

        private static void SetModeOverlayGate(BattlePresentationFrame frame, int gate)
        {
            Assert.That(ModeOverlayGateProperty, Is.Not.Null);
            ModeOverlayGateProperty.SetValue(frame, gate);
        }

        private static void AddEntity(
            BattlePresentationFrame frame,
            BattlePresentationEntitySnapshot entity)
        {
            Assert.That(AddEntityMethod, Is.Not.Null);
            AddEntityMethod.Invoke(frame, new object[] { entity });
        }

        private static char[,] GetLabels(BattlePresentationFrame frame)
        {
            Assert.That(SlotLabelCharsField, Is.Not.Null);
            return (char[,])SlotLabelCharsField.GetValue(frame);
        }

        private static int[] GetLabelState(BattlePresentationFrame frame)
        {
            Assert.That(SlotLabelStateField, Is.Not.Null);
            return (int[])SlotLabelStateField.GetValue(frame);
        }

        private static void AssertFrameEquals(
            IReadOnlyList<BattleRenderCommand> expected,
            BattlePresentationFrame actual)
        {
            Assert.That(actual.CommandCount, Is.EqualTo(expected.Count));
            for (int index = 0; index < expected.Count; index++)
                AssertCommandEquals(expected[index], actual.GetCommand(index));
        }

        private static void AssertStaticTemplateFields(
            BattleCommonVisualBinding binding,
            int sheet,
            int code,
            in BattleRenderCommand actual)
        {
            Assert.That(actual.Type, Is.EqualTo(BattleRenderCommandType.OverlayGlyph));
            Assert.That(actual.VisualDataId, Is.EqualTo(sheet));
            Assert.That(actual.EffectivePic, Is.EqualTo(code));
            Assert.That(actual.Size, Is.EqualTo(binding.PixelSize));
            Assert.That(actual.Pivot, Is.EqualTo(binding.Pivot));
            Assert.That(actual.NormalizedUv, Is.EqualTo(binding.NormalizedUv));
            Assert.That(actual.RenderState.Color, Is.EqualTo(binding.RenderState.Color));
            Assert.That(actual.RenderState.FlipX, Is.EqualTo(binding.RenderState.FlipX));
            Assert.That(actual.RenderState.FlipY, Is.EqualTo(binding.RenderState.FlipY));
            Assert.That(actual.RenderState.MaskInteraction,
                Is.EqualTo(binding.RenderState.MaskInteraction));
            Assert.That(actual.RenderState.MaterialSemantic,
                Is.EqualTo(binding.RenderState.MaterialSemantic));
            Assert.That(actual.SpriteDescriptor.LogicalResourceKey, Is.EqualTo(binding.Key));
            Assert.That(actual.SpriteDescriptor.PixelRect, Is.EqualTo(binding.PixelRect));
            Assert.That(actual.SpriteDescriptor.PivotNormalized, Is.EqualTo(binding.Pivot));
        }

        private static void AssertCommandEquals(
            in BattleRenderCommand expected,
            in BattleRenderCommand actual)
        {
            Assert.That(actual.Type, Is.EqualTo(expected.Type));
            Assert.That(actual.MotionAnchor, Is.EqualTo(expected.MotionAnchor));
            Assert.That(actual.Handle, Is.EqualTo(expected.Handle));
            Assert.That(actual.StableId, Is.EqualTo(expected.StableId));
            Assert.That(actual.VisualDataId, Is.EqualTo(expected.VisualDataId));
            Assert.That(actual.EffectivePic, Is.EqualTo(expected.EffectivePic));
            Assert.That(actual.ZInt, Is.EqualTo(expected.ZInt));
            Assert.That(actual.RuntimeSlot, Is.EqualTo(expected.RuntimeSlot));
            Assert.That(actual.SortOrder, Is.EqualTo(expected.SortOrder));
            Assert.That(actual.SortingLayerId, Is.EqualTo(expected.SortingLayerId));
            Assert.That(actual.LocalSequence, Is.EqualTo(expected.LocalSequence));
            Assert.That(actual.Position, Is.EqualTo(expected.Position));
            Assert.That(actual.Size, Is.EqualTo(expected.Size));
            Assert.That(actual.Pivot, Is.EqualTo(expected.Pivot));
            Assert.That(actual.NormalizedUv, Is.EqualTo(expected.NormalizedUv));
            Assert.That(actual.RenderState.Color, Is.EqualTo(expected.RenderState.Color));
            Assert.That(actual.RenderState.FlipX, Is.EqualTo(expected.RenderState.FlipX));
            Assert.That(actual.RenderState.FlipY, Is.EqualTo(expected.RenderState.FlipY));
            Assert.That(actual.RenderState.MaskInteraction,
                Is.EqualTo(expected.RenderState.MaskInteraction));
            Assert.That(actual.RenderState.MaterialSemantic,
                Is.EqualTo(expected.RenderState.MaterialSemantic));
            Assert.That(actual.SpriteDescriptor.RequiresSprite,
                Is.EqualTo(expected.SpriteDescriptor.RequiresSprite));
            Assert.That(actual.SpriteDescriptor.HasSprite,
                Is.EqualTo(expected.SpriteDescriptor.HasSprite));
            Assert.That(actual.SpriteDescriptor.SpriteInstanceId,
                Is.EqualTo(expected.SpriteDescriptor.SpriteInstanceId));
            Assert.That(actual.SpriteDescriptor.TextureInstanceId,
                Is.EqualTo(expected.SpriteDescriptor.TextureInstanceId));
            Assert.That(actual.SpriteDescriptor.MaterialInstanceId,
                Is.EqualTo(expected.SpriteDescriptor.MaterialInstanceId));
            Assert.That(actual.SpriteDescriptor.PixelRect,
                Is.EqualTo(expected.SpriteDescriptor.PixelRect));
            Assert.That(actual.SpriteDescriptor.PivotNormalized,
                Is.EqualTo(expected.SpriteDescriptor.PivotNormalized));
            Assert.That(actual.SpriteDescriptor.HasLogicalResourceKey,
                Is.EqualTo(expected.SpriteDescriptor.HasLogicalResourceKey));
            Assert.That(actual.SpriteDescriptor.LogicalResourceKey,
                Is.EqualTo(expected.SpriteDescriptor.LogicalResourceKey));
        }
    }
}
#endif
