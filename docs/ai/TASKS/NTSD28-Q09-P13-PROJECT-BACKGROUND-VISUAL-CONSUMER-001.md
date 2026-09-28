# NTSD28-Q09-P13-PROJECT-BACKGROUND-VISUAL-CONSUMER-001

Status: `RUNTIME_PENDING` after scoped original-Editor GPU acceptance, 2026-09-27. This Task was created as `PLANNED` before script/shader edits. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-13`. Q07 is the earliest aggregate open group; this is an independently executable Q09 presentation package.

Authority and current state: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Paired playable `NativeEarthquake28::step` publishes offsets, and `d3d11_renderer.cpp` adds them only to background-layer screen draw coordinates. The prior `NTSD28-Q09-P13-EARTHQUAKE-STATE-PUBLICATION-001` places offsets in the frozen Unity frame but has no project-map consumer. Unity `BattleBackgroundPlatformPresentation.TryApplyWorldCameraFrame` derives the user's fixed full-background camera from the Map `SpriteRenderer.bounds`; moving the Map Transform would change that frame and is not this contract. Native screen Y grows down; Unity world Y grows up.

Exact ownership:

- `Assets/NTSD/Scripts/App/BattleBackgroundPlatformPresentation.cs`: main-thread, world-camera-only draw-time consumer of the latest frozen frame; scoped runtime Material lifecycle and reset, no logic writes or camera/frame refactor.
- `Assets/NTSD/Resources/BattleEarthquakeBackground.shader` and `.meta`: URP SpriteRenderer-compatible transparent shader translating only the project-map sprite's drawn vertices by frozen screen-pixel offsets converted through its pixels-per-unit, leaving renderer bounds and Transform unchanged.
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeBackgroundEditorTests.cs` and `.meta`: focused visual, coordinate-sign, reset and isolation validation. A later real Battle Play and formal-root pixel A/B remain explicitly separate exits.

Invariants: preserve the current project Map Sprite, fixed whole-background camera, mobile bottom overlay, Map Transform/bounds, entity/weapon/shadow logic and presentation, DAT data, excluded native background/mode DAT, Scene/Prefab/ProjectSettings, and nonbattle behavior. No new manager, queue, worker, prefab or scene object. Runtime material exists only while needed and is destroyed/restored through the existing component lifecycle; the renderer's preexisting material must be restored when offsets reach zero or the component stops. Background offset is its own formal draw domain, not the D-024 entity movement rule.

Validation: write a focused failing visual/consumer test before production edits if the existing Editor route is available, then verify URP shader import and original project Editor exact test. Compare zero-offset baseline pixels to current SpriteRenderer output, then one known nonzero offset including Y sign; confirm camera, Map Transform/bounds and unrelated sprite pixels do not move, and repeat reset/disable/scene-exit. Run generated runtime/editor builds, Ledger validator, `git diff --check` and both Scene disk SHA checks. If GPU/Play cannot run, report `RUNTIME_PENDING`, not P-13 completion. Actual Han natural Battle Play and formal root same-frame pixel comparison remain Q09 exits.

Rollback: review only this declared code/shader/test diff; any destructive restore requires explicit authorization. Preserve all pre-existing dirty files and user work.
