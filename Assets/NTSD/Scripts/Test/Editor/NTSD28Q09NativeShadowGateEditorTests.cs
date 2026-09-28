#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q09NativeShadowGateEditorTests
    {
        [TestCase(1, 0, false)]
        [TestCase(0, 1, false)]
        [TestCase(0, 0, true)]
        [TestCase(2, 2, true)]
        public void FormalShadowFieldsGateBothPresentationStates(
            int bmpShadow, int frameShadow, bool expectedVisible)
        {
            string text = "<bmp_begin>\nshadow: " + bmpShadow + "\n<bmp_end>\n" +
                "<frame> 1 fly\npic: 3 state: 3000 wait: 1 next: 1 shadow: " +
                frameShadow + "\n<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var frame = Lf2DatConverter.ConvertLoganFrameData(parsed.Frames[0]);
            var data = new LF2CharacterData
            {
                NativeMetadata = new LoganDefinitionMetadata(
                    LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties),
                    LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            data.frames.Add(frame);

            var actor = new LF2Character();
            var shadowObject = new GameObject("Q09 native shadow gate");
            try
            {
                actor.ObjectId = 518;
                actor.FrameCache.Load(new LF2CharacterDataWrapper(518, data));
                actor.Frame.D = frame;
                actor.Runtime.LinkState = 0;
                actor.Runtime.HitStop = 0;
                SpriteRenderer renderer = shadowObject.AddComponent<SpriteRenderer>();
                actor.SetShadowRenderer(renderer);

                var world = new SimulationWorld();
                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                actor.SetRequiredRuntimeSlot(50);
                world.Register(actor);
                world.BattlePresentation.BeginFrame(world, 1);
                Assert.That(world.BattlePresentation.PublishedFrame.EntityCount, Is.EqualTo(1));
                Assert.That(world.BattlePresentation.PublishedFrame.GetEntity(0).ShadowVisible,
                    Is.EqualTo(expectedVisible));

                actor.UpdateShadow();
                Assert.That(renderer.enabled, Is.EqualTo(expectedVisible));
                Assert.That(actor.Sprite.ShadowVisible, Is.EqualTo(expectedVisible));

                actor.UpdateShadowManagedState();
                Assert.That(actor.Sprite.ShadowVisible, Is.EqualTo(expectedVisible));
            }
            finally
            {
                actor.SetShadowRenderer(null);
                Object.DestroyImmediate(shadowObject);
            }
        }
    }
}
#endif
