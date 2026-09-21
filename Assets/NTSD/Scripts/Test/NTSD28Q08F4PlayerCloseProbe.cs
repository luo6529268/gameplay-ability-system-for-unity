#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NTSD.Animation;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test
{
    internal sealed class NTSD28Q08F4PlayerCloseProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public bool f4Queued;
            public int tickBeforeF4;
            public int tickAtQuit;
            public string lifecycleAtQuit;
            public int activePoolBorrowersAtQuit;
            public bool poolQuiescedAtQuit;
        }

        private static string reportPath;
        private readonly Report report = new Report { status = "RUNNING" };
        private SimulationTickDriver driver;
        private Keyboard keyboard;
        private bool timedOut;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Environment.GetCommandLineArgs().Contains("-ntsd-q08-f4-close-probe"))
                return;

            reportPath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "q08-f4-player-close.json");
            GameObject owner = new GameObject("NTSD28_Q08_F4_PlayerCloseProbe");
            DontDestroyOnLoad(owner);
            owner.AddComponent<NTSD28Q08F4PlayerCloseProbe>();
        }

        private IEnumerator Start()
        {
            Write();
            float deadline = Time.realtimeSinceStartup + 180f;
            while (Time.realtimeSinceStartup < deadline)
            {
                driver = SimulationTickDriver.Instance;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World?.ObjectCount > 0)
                    break;
                yield return null;
            }

            if (driver == null ||
                driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                driver.World?.ObjectCount <= 0)
            {
                Fail("Battle did not reach a Running world before F4.");
                yield break;
            }

            keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            yield return null;
            report.tickBeforeF4 = driver.CurrentTickIndex;
            report.f4Queued = true;
            Write();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F4));

            deadline = Time.realtimeSinceStartup + 20f;
            while (Time.realtimeSinceStartup < deadline)
                yield return null;
            Fail("Accepted physical F4 did not close the Player within 20 seconds.");
        }

        private void OnApplicationQuit()
        {
            if (reportPath == null)
                return;

            report.tickAtQuit = driver == null ? -1 : driver.CurrentTickIndex;
            report.lifecycleAtQuit = driver == null
                ? "MISSING"
                : driver.LifecycleState.ToString();
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            report.activePoolBorrowersAtQuit = pool == null
                ? -1
                : pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
            report.poolQuiescedAtQuit = pool != null && pool.IsQuiescedForDiagnostics;
            if (!timedOut && report.f4Queued &&
                report.tickAtQuit == report.tickBeforeF4 &&
                report.lifecycleAtQuit == BattleRuntimeLifecycleState.Stopped.ToString() &&
                report.activePoolBorrowersAtQuit == 0 && report.poolQuiescedAtQuit)
            {
                report.status = "PASS";
                report.message = "Physical F4 closed the Player after ordered battle shutdown.";
            }
            else
            {
                report.status = "FAIL";
                if (string.IsNullOrEmpty(report.message))
                    report.message = "Player quit without the expected F4 shutdown state.";
            }
            Write();
        }

        private void Fail(string message)
        {
            timedOut = true;
            report.status = "FAIL";
            report.message = message;
            Write();
            Application.Quit(2);
        }

        private void Write()
        {
            if (reportPath != null)
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        }
    }
}
#endif
