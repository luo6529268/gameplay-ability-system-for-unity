#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NTSD.Animation.LF2Objects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test
{
    public sealed class H07InputFacingCostDiagnosticsEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/input-facing-cost-diag/result.json";

        [Test]
        [Explicit("Finite H07 facing diagnostic; preserve original output and never run in a general regression suite.")]
        public void RunFiniteFacingDiagnostic()
        {
            Assert.IsFalse(System.IO.File.Exists(Output), "Do not overwrite or repeat a finite group.");
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating);
            var windows = new List<object>();
            var checks = new List<string>();
            string status = "FAILED";
            string error = null;
            string before = CaptureScenes();
            Scene preview = EditorSceneManager.NewPreviewScene();
            var objects = new List<GameObject>(1000);
            var sprites = new LF2Sprite[1000];
            var renderers = new SpriteRenderer[1000];
            Texture2D texture = null;
            Sprite image = null;
            try
            {
                texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                image = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
                image.hideFlags = HideFlags.HideAndDontSave;
                MethodInfo method = typeof(LF2Sprite).GetMethod("SwitchLRManagedOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(method);
                var managed = (Action<LF2Sprite, string>)Delegate.CreateDelegate(typeof(Action<LF2Sprite, string>), method);
                Action<LF2Sprite, string> original = (sprite, direction) => sprite.SwitchLR(direction);
                for (int i = 0; i < sprites.Length; i++)
                {
                    var go = new GameObject("H07FacingDiagnostic") { hideFlags = HideFlags.HideAndDontSave };
                    objects.Add(go);
                    SceneManager.MoveGameObjectToScene(go, preview);
                    renderers[i] = go.AddComponent<SpriteRenderer>();
                    sprites[i] = new LF2Sprite();
                    sprites[i].Initialize(renderers[i], null);
                    renderers[i].sprite = image;
                    sprites[i].SetLegacyRendererSuppressed(true);
                }

                LF2Sprite probe = sprites[0];
                SpriteRenderer renderer = renderers[0];
                probe.SwitchLR("left");
                Assert.IsTrue(renderer.flipX);
                Assert.IsFalse(renderer.enabled);
                checks.Add("Original SwitchLR writes native flip while legacy suppressed.");
                managed(probe, "right");
                Assert.AreEqual("right", probe.Dir);
                Assert.IsTrue(renderer.flipX);
                probe.SetLegacyRendererSuppressed(false);
                Assert.IsTrue(renderer.enabled);
                Assert.IsTrue(renderer.flipX);
                checks.Add("Existing unsuppression alone does not repair stale flip: a future skip needs restoration.");
                probe.SetLegacyRendererSuppressed(true);
                renderer.flipX = probe.Dir == "left";
                probe.SetLegacyRendererSuppressed(false);
                Assert.IsTrue(renderer.enabled);
                Assert.IsFalse(renderer.flipX);
                checks.Add("Explicit facing restoration before visibility gives correct legacy facing.");
                renderer.flipX = true;
                probe.SwitchLR("right");
                Assert.IsFalse(renderer.flipX);
                checks.Add("Legacy original same-direction call repairs an externally changed flip; no blanket direction cache.");
                probe.Initialize(renderer, null);
                Assert.AreEqual("right", probe.Dir);
                Assert.IsFalse(renderer.flipX);
                renderer.sprite = image;
                probe.SetLegacyRendererSuppressed(true);
                checks.Add("Initialize resets managed/native facing and suppression lifecycle.");

                bool[] controls = { false, true, true, false };
                for (int scenario = 0; scenario < 2; scenario++)
                {
                    for (int window = 0; window < controls.Length; window++)
                    {
                        bool control = controls[window];
                        Action<LF2Sprite, string> operation = control ? managed : original;
                        long[] elapsed = new long[32];
                        long[] allocated = new long[32];
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            sprites[i].SwitchLR("right");
                            Assert.IsFalse(renderers[i].enabled);
                        }
                        for (int round = 0; round < 40; round++)
                        {
                            string direction = scenario == 0 || (round & 1) == 0 ? "left" : "right";
                            long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                            long begin = System.Diagnostics.Stopwatch.GetTimestamp();
                            for (int i = 0; i < sprites.Length; i++)
                                operation(sprites[i], direction);
                            long end = System.Diagnostics.Stopwatch.GetTimestamp();
                            long bytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                            for (int i = 0; i < sprites.Length; i++)
                            {
                                Assert.AreEqual(direction, sprites[i].Dir);
                                Assert.IsFalse(renderers[i].enabled);
                                if (!control)
                                    Assert.AreEqual(direction == "left", renderers[i].flipX);
                            }
                            if (round >= 8)
                            {
                                elapsed[round - 8] = end - begin;
                                allocated[round - 8] = bytes;
                                Assert.AreEqual(0, bytes, "Complete measured loop must allocate zero bytes.");
                            }
                        }
                        windows.Add(new { scenario = scenario == 0 ? "same-facing" : "alternating-facing", window, mode = control ? "MANAGED_ONLY_CONTROL" : "ORIGINAL", callsPerRound = 1000, warm = 8, sample = 32, elapsed, allocated });
                    }
                }
                status = "FINITE_DIAGNOSTIC_COMPLETE";
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                throw;
            }
            finally
            {
                for (int i = 0; i < sprites.Length; i++)
                    sprites[i]?.Reset();
                foreach (GameObject go in objects)
                    if (go != null) UnityEngine.Object.DestroyImmediate(go);
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                EditorSceneManager.ClosePreviewScene(preview);
                string after = CaptureScenes();
                bool sceneUnchanged = before == after;
                var result = new { status, error, scope = "Isolated 1000-renderer API control, not production candidate, real input frequency, Driver or Player benefit", frequency = System.Diagnostics.Stopwatch.Frequency, checks, windows, sceneBefore = before, sceneAfter = after, sceneUnchanged, remainingObjects = objects.Count(go => go != null) };
                System.IO.File.WriteAllText(Output, Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented));
                Assert.IsTrue(sceneUnchanged, "Original Scene state must remain unchanged.");
            }
        }

        private static string CaptureScenes()
        {
            return string.Join("|", Enumerable.Range(0, SceneManager.sceneCount).Select(i =>
            {
                Scene scene = SceneManager.GetSceneAt(i);
                return scene.path + ":" + scene.isDirty + ":" + scene.rootCount;
            }));
        }
    }
}
#endif
