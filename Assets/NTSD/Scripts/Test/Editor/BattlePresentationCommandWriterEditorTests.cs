#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
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
                },
                null);

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
                    (int)entity.CenterY);
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
                            binding.Key)));
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
            int z)
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
                false);
        }

        private static BattleCommonVisualCatalog CreateCatalog(
            int variant,
            bool includeSpecialCom = false,
            bool includeAllComLabels = false)
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

            if (!includeSpecialCom && !includeAllComLabels)
            {
                return (BattleCommonVisualCatalog)CatalogConstructor.Invoke(
                    new object[]
                    {
                        null,
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
