# Q07 Menu-to-Battle Scene closure proposal (2026-09-22)

Status: `PROPOSAL_ONLY / USER_DIRECTION_REQUIRED`. No Scene, ProjectSettings, script or resource was changed.

The current `ProjectSettings/EditorBuildSettings.asset` SHA-256 is `424A5EDABB5F42E30840B5600F0BF7F0E1058C37D645DC722D3074388E151841` and contains `m_Scenes: []`. The actual saved Menu Scene flow completed formal-content prewarm, VS/Naruto selection and Fight, then failed at `AppManager.LoadBattleAdditive()` because `NTSD_Battle` is not in Build Settings; see `NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001/RESULT.md`. `AppManager` names `NTSD_Menu` and `NTSD_Battle`, loads the latter additively, and requires both loaded for its result return-to-selection path. The saved Menu Scene serializes those same names.

The smallest production closure consistent with that route is two enabled scenes, in this order:

| Build index | Scene | Existing GUID | Role |
|---:|---|---|---|
| 0 | `Assets/NTSD/Scene/NTSD_Menu.unity` | `132c934aff207b048833ab33c1961278` | Initial menu and selection host |
| 1 | `Assets/NTSD/Scene/NTSD_Battle.unity` | `9a044b4462e171548a6466aaa64f92e5` | Additive battle scene |

`NTSD_Test.unity` (`26a6ddd32f9892749bf7b2794e9ef90b`) stays out of the production list. No Scene object, GameConfig, input or battle rule needs changing merely to register the list. The exact proposed `m_Scenes` value is:

```yaml
  m_Scenes:
  - enabled: 1
    path: Assets/NTSD/Scene/NTSD_Menu.unity
    guid: 132c934aff207b048833ab33c1961278
  - enabled: 1
    path: Assets/NTSD/Scene/NTSD_Battle.unity
    guid: 9a044b4462e171548a6466aaa64f92e5
```

This is a change to global build/startup configuration outside the already approved battle code and resource migration. `AGENTS.md` section 13.2 reserves new direction for Scene changes, and `Assets/NTSD/Docs/android-mobile-readiness/h-02-android-build-scene-closure.md` explicitly requires the user to choose the formal first Scene before its build closure. Therefore implementation waits for that direction. If approved, create an exact Task/Change, review current dirty state again, edit only `EditorBuildSettings.asset`, then validate a fresh original-project import, the saved Menu callback flow into Battle and return, focused battle shutdown/re-entry, and a Windows Player build using the declared list. Preserve old failure evidence. If validation fails, revert only the two proposed entries after reviewing the diff; do not reset or clean the workspace.
