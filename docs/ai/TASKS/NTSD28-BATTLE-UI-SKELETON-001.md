# NTSD28-BATTLE-UI-SKELETON-001

Status: IN_PROGRESS

## User request

为 `NTSD_Battle` 准备战斗 UI 骨架，后续由用户在 Unity 中创建界面并完成具体的 Inspector 节点绑定。战斗 UI 的长期窗口生命周期预留给 TEngine `UIModule`，当前仓库不直接复制整套 TEngine 运行时。

## Bounded scope

- Add a project-owned battle UI view contract under `Assets/NTSD/Scripts/UI/Battle/`.
- Provide reusable role-slot binding and snapshot refresh methods for up to eight battle slots.
- Keep the view presentation-only: no direct character mutation, no raw key polling, and no Canvas ownership change in `BattleBootstrap`.
- Do not modify `NTSD_Battle.unity`, existing UI assets, input actions, DAT/content, battle logic, or other scenes in this skeleton cut.
- Leave the TEngine `UIWindow`/`UIModule` call site as a documented integration seam until the user creates the `BattleMainUI` Prefab and supplies its visual bindings.

## Acceptance

- New UI scripts compile in the current project source model when the existing project dependencies are available.
- The root view accepts a `BattleUiContext`, reuses one snapshot buffer, applies static and dynamic state to assigned slot views, and clears bindings explicitly.
- Each slot view tolerates missing optional visual references without mutating battle state.
- The user receives exact Unity hierarchy and Inspector binding steps for creating the `BattleMainUI` Prefab.
- `Tools/Validate-ChangeLedger.ps1` and `git diff --check` pass for this change.

## Rollback

Review and remove only the files listed in the Change Record. Preserve all pre-existing worktree modifications and do not reset, clean, or restore unrelated files.
