#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q04OscillatePlayProbeEditor
    {
        [MenuItem("NTSD/Battle Diagnostics/Q04/Run Oscillate Consumer Play Probe")]
        public static void Run()
        {
            var report = new Report();
            SimulationTickDriver driver = UnityEngine.Object.FindObjectOfType<SimulationTickDriver>();
            SimulationWorld world = driver?.World;
            if (!EditorApplication.isPlaying || world == null)
            {
                report.message = "Existing Play battle world required";
                Write(report);
                return;
            }
            LF2Character entity = null;
            SpriteRenderer renderer = null;
            FieldInfo rendererField = typeof(LF2Sprite).GetField("_renderer", BindingFlags.Instance | BindingFlags.NonPublic);
            for (int slot = 0; slot < 1000; slot++)
            {
                if (world.FindEntityByRuntimeSlotForQuery(slot) is LF2Character candidate && candidate.Sprite != null)
                {
                    renderer = rendererField.GetValue(candidate.Sprite) as SpriteRenderer;
                    entity = candidate;
                    break;
                }
            }
            if (entity == null)
            {
                report.message = "No registered character with actual LF2Sprite";
                Write(report);
                return;
            }
            bool paused = driver.IsPaused;
            LF2Sprite sprite = entity.Sprite;
            Vector2 originalOffset = sprite.LocalOffsetPixels;
            Vector3 originalPosition = renderer != null ? renderer.transform.localPosition : Vector3.zero;
            bool originalVisible = sprite.EntityVisible;
            bool originalEnabled = renderer != null && renderer.enabled;
            double vx = entity.Runtime.Vx, vy = entity.Runtime.Vy;
            int objects = world.ObjectCount;
            var saved = new Dictionary<PropertyInfo, object>();
            foreach (PropertyInfo property in typeof(LF2EffectState).GetProperties())
                if (property.CanRead && property.CanWrite) saved.Add(property, property.GetValue(entity.Effect));
            MethodInfo process = typeof(LF2LivingObject).GetMethod("ProcessEffects", BindingFlags.Instance | BindingFlags.NonPublic);
            var capture = new BattlePresentationCoordinator();
            capture.SetMode(BattlePresentationBackendMode.CentralOnly);
            capture.PrepareCapacity(world.RuntimeSlotCapacityForDiagnostics);
            try
            {
                driver.SetPaused(true);
                report.slot = entity.Runtime.SlotIndex;
                report.backend = world.BattlePresentation.Mode.ToString();
                report.hasSpriteRenderer = renderer != null;
                sprite.SetXY(11, 9);
                Vector3 testPosition = renderer != null ? renderer.transform.localPosition : Vector3.zero;
                entity.Effect.TimeIn = -1;
                entity.Effect.TimeOut = 9;
                entity.Effect.Blink = false;
                process.Invoke(entity, null);
                Require(sprite.LocalOffsetPixels == new Vector2(11, 9) &&
                    (renderer == null || renderer.transform.localPosition == testPosition),
                    "Effect processing moved the explicit sprite offset");
                CheckSnapshot(capture, world, driver.CurrentTickIndex, entity, new Vector2(11, 9), sprite.EntityVisible);
                report.offsetPreserved = true;
                entity.Effect.Blink = true;
                entity.Effect.BlinkCounter = 0;
                process.Invoke(entity, null);
                Require(!sprite.EntityVisible && (renderer == null || !renderer.enabled) && entity.Effect.BlinkCounter == 1,
                    "Effect processing suppressed Blink");
                CheckSnapshot(capture, world, driver.CurrentTickIndex, entity, new Vector2(11, 9), false);
                report.blinkPassed = true;
                entity.Effect.TimeOut = 0;
                entity.Effect.Stuck = true;
                entity.Effect.Super = true;
                process.Invoke(entity, null);
                Require(entity.Effect.Num == -99 && !entity.Effect.Stuck && !entity.Effect.Super &&
                    !entity.Effect.Blink && sprite.EntityVisible && entity.Effect.BlinkCounter == 0 &&
                    (renderer == null || renderer.transform.localPosition == testPosition), "Timeout changed unrelated behavior or offset");
                CheckSnapshot(capture, world, driver.CurrentTickIndex, entity, new Vector2(11, 9), true);
                report.productionSnapshotPassed = true;
                report.timeoutPassed = true;
                entity.Effect.Dvx = 1.5f;
                entity.Effect.Dvy = -2.25f;
                process.Invoke(entity, null);
                Require(entity.Runtime.Vx == 1.5 && entity.Runtime.Vy == -2.25 &&
                    entity.Effect.Dvx == 0 && entity.Effect.Dvy == 0, "Deferred motion changed");
                report.deferredMotionPassed = true;
                report.status = "PASS";
                report.message = "Existing Play character after retired effect carrier removal; production ProcessEffects and presentation snapshot capture verified; renderer checks when bound.";
            }
            catch (Exception error) { report.status = "FAIL"; report.message = error.ToString(); }
            finally
            {
                capture.Reset();
                foreach (var item in saved) item.Key.SetValue(entity.Effect, item.Value);
                entity.Runtime.Vx = vx;
                entity.Runtime.Vy = vy;
                sprite.SetXY(originalOffset.x, originalOffset.y);
                if (originalVisible) sprite.Show(); else sprite.Hide();
                if (renderer != null)
                {
                    renderer.transform.localPosition = originalPosition;
                    renderer.enabled = originalEnabled;
                }
                driver.SetPaused(paused);
                report.cleanupPassed = world.ObjectCount == objects && entity.Runtime.Vx == vx &&
                    entity.Runtime.Vy == vy && sprite.LocalOffsetPixels == originalOffset &&
                    sprite.EntityVisible == originalVisible &&
                    (renderer == null || (renderer.transform.localPosition == originalPosition && renderer.enabled == originalEnabled)) &&
                    driver.IsPaused == paused;
                foreach (var item in saved)
                    report.cleanupPassed &= Equals(item.Key.GetValue(entity.Effect), item.Value);
                if (!report.cleanupPassed) { report.status = "FAIL"; report.message += " Restore failed."; }
                Write(report);
            }
        }

        private static void CheckSnapshot(BattlePresentationCoordinator capture, SimulationWorld world,
            int tick, LF2Character entity, Vector2 offset, bool visible)
        {
            capture.BeginSimulationWorkerFrame(world, tick);
            BattlePresentationFrame frame = capture.PublishedFrame;
            Require(frame != null, "Presentation capture missing");
            for (int i = 0; i < frame.EntityCount; i++)
            {
                BattlePresentationEntitySnapshot row = frame.GetEntity(i);
                if (row.RuntimeSlot != entity.Runtime.SlotIndex) continue;
                Require(row.LocalOffsetPixels == offset && row.EntityVisible == visible,
                    "Production presentation snapshot disagrees with effect state");
                return;
            }
            throw new InvalidOperationException("Character absent from presentation capture");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
        private static void Write(Report report) => File.WriteAllText(
            "Temp/NTSD28_Q04_Oscillate.result.json", JsonUtility.ToJson(report, true));

        [Serializable] private sealed class Report
        {
            public string status = "FAIL";
            public string message;
            public string backend;
            public int slot;
            public bool hasSpriteRenderer, productionSnapshotPassed;
            public bool offsetPreserved, blinkPassed, timeoutPassed, deferredMotionPassed, cleanupPassed;
        }
    }
}
#endif
