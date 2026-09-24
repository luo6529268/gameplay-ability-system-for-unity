# NTSD28-BATTLE-UI-HUD-REWORK-001

status: CODE_WRITTEN

## User requirement

The user reviewed the first UI skeleton and clarified that `NTSD_Battle` does not use eight character slots. The current HUD has one character resource panel, one combo panel, and one action-control panel. Replace the slot-list abstraction with small views matching the current hierarchy.

## Bounded scope

- Supersede the unused eight-slot `BattleMainUIView` / `BattleCharacterSlotView` skeleton from `NTSD28-BATTLE-UI-SKELETON-001`.
- Keep the user's current `NTSD_Battle` layout and only remove the obsolete `BattleMainUIView` component that was added by the previous UI package.
- Add three small presentation-only views: `BattleHudView`, `BattleComboView`, and `BattleControlsView`.
- Keep a reusable snapshot contract for a single HUD character, combo state, and action-button held state.
- Do not change battle Runtime, `CharacterInputModule`, DAT/content, camera, Canvas layout, `BattleBootstrap`, or other scenes.
- Do not add TEngine source or a TEngine `UIWindow` implementation while the repository has no TEngine dependency.

## Acceptance

- Current scene hierarchy is preserved after removing only the obsolete UI component.
- New code maps directly to the current HUD regions and has no eight-slot array.
- Static C# check and Change Ledger validation pass.
- Unity assembly compile, Inspector binding, TEngine open/close, and Play Mode remain explicitly pending.

## Current evidence

- Static C# compile with local Unity/TMP/uGUI type stubs passed.
- `Tools/Validate-ChangeLedger.ps1` passed and covers all four current UI source files.
- The obsolete scene component marker and old Slot/MainView source references are absent.
- The current scene still has user-owned serialized layout changes; global `git diff --check` reports Unity's empty serialized fields in that scene, so no whitespace cleanup was applied to it.

## Rollback

Review the scoped scene component removal and the new/retired files only. Preserve all other dirty scene, diagnostics, and source changes in the worktree.
