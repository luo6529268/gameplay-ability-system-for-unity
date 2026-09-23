# NTSD28-Q07-PRODUCTION-SCENE-LIST-001

Status: FOCUSED_TEST_PASS (2026-09-23). The user confirmed `NTSD_Menu` first and `NTSD_Battle` second in the Q07 first-Scene question. This task is limited to the original Unity project at `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`.

Current pre-change `ProjectSettings/EditorBuildSettings.asset` SHA-256: `424A5EDABB5F42E30840B5600F0BF7F0E1058C37D645DC722D3074388E151841`; `m_Scenes: []`. The existing Menu Scene SHA-256 is `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1`; the existing Battle Scene SHA-256 is `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. Preserve both Scene files and the `m_configObjects` mapping.

Exact change: insert two enabled `m_Scenes` entries only: `Assets/NTSD/Scene/NTSD_Menu.unity` GUID `132c934aff207b048833ab33c1961278` at index 0; `Assets/NTSD/Scene/NTSD_Battle.unity` GUID `9a044b4462e171548a6466aaa64f92e5` at index 1. Do not edit the Scene files, UI/App scripts, GameConfig, old content or the separately open `I:\UnityPreject\test` project.

Why: the actual saved Menu callback reaches Fight, but `AppManager.LoadBattleAdditive()` fails because the Battle Scene is absent from enabled Build Settings. The first-Scene choice required by `Assets/NTSD/Docs/android-mobile-readiness/h-02-android-build-scene-closure.md` has now been supplied by the user.

Acceptance: inspect exact setting diff and two enabled GUID/path entries; verify both Scene SHA values unchanged; use the existing original Editor and Q07 Menu callback Play probe to check BattleRunning, selected character and formal content, ordered UnloadBattle and Menu return. A Player cold-start remains a separate closure gate. Rollback would restore only this task's two-entry `m_Scenes` change to the recorded empty list, subject to the repository's explicit approval rule for `git restore` or destructive operations.
