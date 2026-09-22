#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test
{
    internal sealed class NTSD28Q07WindowsNaturalSkillProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public string configuredRoot;
            public string fingerprint;
            public int characterId;
            public int startTick;
            public int step1Tick;
            public int step2Tick;
            public int targetTick;
            public int firstCloneTick;
            public int hiddenTick;
            public int visibleTick;
            public int peakCloneCount;
            public string shutdownStage;
            public string lifecycleAfterShutdown;
            public int activePoolBorrowers;
        }

        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB";
        private const int TimeoutTicks = 90;
        private const int HoldTicks = 2;
        private const int MaxPressAttempts = 8;
        private static readonly FieldInfo FirstPlayerField =
            typeof(BattleTestBootstrap).GetField(
                "firstPlayerLf2", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly List<LF2Entity> observedEntities = new List<LF2Entity>(16);
        private readonly Report report = new Report
        {
            status = "RUNNING",
            step1Tick = -1,
            step2Tick = -1,
            targetTick = -1,
            firstCloneTick = -1,
            hiddenTick = -1,
            visibleTick = -1,
        };
        private static string reportPath;
        private SimulationTickDriver driver;
        private LF2Character character;
        private Keyboard keyboard;
        private Key directionKey;
        private byte directionCombo;
        private int lastObservedTick;
        private int lastPulseTick;
        private int pressAttempts;
        private bool releaseQueued;
        private bool active;
        private bool finishing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Environment.GetCommandLineArgs().Contains("-ntsd-q07-natural-player-probe"))
                return;
            reportPath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "q07-natural-player.json");
            GameObject owner = new GameObject("NTSD28_Q07_WindowsNaturalSkillProbe");
            DontDestroyOnLoad(owner);
            owner.AddComponent<NTSD28Q07WindowsNaturalSkillProbe>();
        }

        private IEnumerator Start()
        {
            WriteReport();
            float deadline = Time.realtimeSinceStartup + 180f;
            while (Time.realtimeSinceStartup < deadline)
            {
                driver = SimulationTickDriver.Instance;
                BattleTestBootstrap bootstrap = FindObjectOfType<BattleTestBootstrap>();
                character = bootstrap == null
                    ? null
                    : FirstPlayerField?.GetValue(bootstrap) as LF2Character;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World?.ObjectCount > 0 && character?.Frame?.D != null &&
                    character.Runtime?.NativeInputProxy != null)
                    break;
                yield return null;
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
            report.configuredRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            report.fingerprint = manager?.PublishedLoganContentIdentity?.SemanticFingerprint;
            report.characterId = character == null ? -1 : character.ObjectId;
            if (driver == null || driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                driver.World?.ObjectCount <= 0 || character?.Frame?.D == null ||
                character.Runtime?.NativeInputProxy == null ||
                report.configuredRoot != FormalRoot ||
                report.fingerprint != FormalFingerprint ||
                report.characterId != 2 || character.Frame.D.hit_Fa != 285)
            {
                BeginFinish(false, "Formal Player Naruto battle preflight failed.");
                yield break;
            }

            keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
            directionKey = string.Equals(character.Runtime.Dir, "left",
                StringComparison.OrdinalIgnoreCase) ? Key.A : Key.D;
            directionCombo = directionKey == Key.A ? (byte)3 : (byte)2;
            QueueKeys();
            yield return null;
            yield return null;
            report.startTick = driver.CurrentTickIndex;
            lastObservedTick = report.startTick;
            lastPulseTick = report.startTick;
            pressAttempts = 1;
            active = true;
            QueueKeys(Key.L);
            WriteReport();
        }

        private void Update()
        {
            if (!active || finishing)
                return;
            if (driver?.World == null || character?.Runtime?.NativeInputProxy == null)
            {
                BeginFinish(false, "Battle runtime ended during the physical combo.");
                return;
            }

            int tick = driver.CurrentTickIndex;
            if (tick <= lastObservedTick)
                return;
            lastObservedTick = tick;
            int frame = character.Frame?.N ?? -1;
            byte combo = character.Runtime.NativeInputProxy.ComboState[0];

            observedEntities.Clear();
            driver.World.GetAllEntities(observedEntities);
            int cloneCount = 0;
            foreach (LF2Entity entity in observedEntities)
            {
                if (entity == null || entity.ObjectId != 33)
                    continue;
                cloneCount++;
                if (report.firstCloneTick < 0)
                    report.firstCloneTick = tick;
                int cloneFrame = entity.Frame?.N ?? -1;
                int pic = entity.GetRenderPicIndex();
                bool resolved = entity.TryResolveCurrentSpriteEntry(
                    out BattleSpriteEntry entry);
                if (report.hiddenTick < 0 &&
                    (cloneFrame == 240 || cloneFrame == 241) && pic == 999 && !resolved)
                    report.hiddenTick = tick;
                if (report.visibleTick < 0 && cloneFrame == 242 && pic == 1 &&
                    resolved && entry != null && entry.Key.VisualDataId == 33 &&
                    entry.Key.EffectivePic == 1 &&
                    entry.SourceSheetPath.Replace('\\', '/').EndsWith(
                        "c/nar/ncl.png", StringComparison.OrdinalIgnoreCase) &&
                    Math.Abs(entry.PixelWidth - 79f) < 0.01f &&
                    Math.Abs(entry.PixelHeight - 79f) < 0.01f &&
                    entry.CentralBinding.IsValid)
                    report.visibleTick = tick;
            }
            report.peakCloneCount = Math.Max(report.peakCloneCount, cloneCount);

            if (report.step1Tick < 0 && combo == 1)
            {
                report.step1Tick = tick;
                QueueKeys(directionKey);
                ResetPulse(tick);
            }
            else if (report.step1Tick >= 0 && report.step2Tick < 0 &&
                     combo == directionCombo)
            {
                report.step2Tick = tick;
                QueueKeys(Key.J);
                ResetPulse(tick);
            }
            if (report.step2Tick >= 0 && report.targetTick < 0 && frame == 285)
            {
                report.targetTick = tick;
                QueueKeys();
            }

            if (report.targetTick < 0 && !PulsePendingKey(tick))
            {
                BeginFinish(false, "Physical L/D/J did not cross the canonical input poll.");
                return;
            }
            if (report.targetTick >= 0 && tick >= report.targetTick + 18)
            {
                bool passed = report.step1Tick >= 0 && report.step2Tick >= 0 &&
                    report.peakCloneCount > 0 && report.hiddenTick >= 0 &&
                    report.visibleTick > report.hiddenTick;
                BeginFinish(passed, passed
                    ? "Built Player physical Naruto L/D/J spawned and displayed formal OID33."
                    : "Target frame did not yield the complete formal OID33 sprite sequence.");
                return;
            }
            if (tick > report.startTick + TimeoutTicks)
                BeginFinish(false, "Natural skill did not complete within 90 logic ticks.");
        }

        private void ResetPulse(int tick)
        {
            lastPulseTick = tick;
            pressAttempts = 1;
            releaseQueued = false;
        }

        private bool PulsePendingKey(int tick)
        {
            if (tick - lastPulseTick < HoldTicks)
                return true;
            Key pending = report.step1Tick < 0 ? Key.L :
                report.step2Tick < 0 ? directionKey : Key.J;
            if (!releaseQueued)
            {
                if (pressAttempts >= MaxPressAttempts)
                    return false;
                QueueKeys();
                releaseQueued = true;
            }
            else
            {
                QueueKeys(pending);
                pressAttempts++;
                releaseQueued = false;
            }
            lastPulseTick = tick;
            return true;
        }

        private void QueueKeys(params Key[] keys)
        {
            if (keyboard != null)
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private void BeginFinish(bool passed, string message)
        {
            if (finishing)
                return;
            finishing = true;
            active = false;
            QueueKeys();
            StartCoroutine(Finish(passed, message));
        }

        private IEnumerator Finish(bool passed, string message)
        {
            AppManager app = AppManager.Instance;
            BattleRuntimeShutdownReport shutdown = default;
            bool shutdownSucceeded = app != null &&
                app.TryShutdownBattleRuntimeBeforeSceneDestroy(
                    out shutdown);
            report.shutdownStage = shutdownSucceeded
                ? shutdown.CompletedStage.ToString() : "FAILED";
            yield return null;
            yield return null;
            report.lifecycleAfterShutdown = driver == null
                ? "MISSING" : driver.LifecycleState.ToString();
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            report.activePoolBorrowers = pool == null
                ? -1 : pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
            bool stopped = shutdownSucceeded && driver != null &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                driver.World == null && pool != null &&
                report.activePoolBorrowers == 0 && pool.IsQuiescedForDiagnostics;
            report.status = passed && stopped ? "PASS" : "FAIL";
            report.message = stopped ? message : message + " Ordered shutdown failed.";
            WriteReport();
            Application.Quit(report.status == "PASS" ? 0 : 1);
        }

        private void WriteReport()
        {
            if (reportPath != null)
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        }
    }
}
#endif
