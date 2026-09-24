<!-- CHANGE-RECORD
id: NTSD28-BATTLE-UI-HUD-REWORK-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleUiContracts.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleHudView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleComboView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleControlsView.cs
authority: Current user correction after reviewing the NTSD_Battle HUD; current scene hierarchy is the binding target
evidence: docs/ai/TASKS/NTSD28-BATTLE-UI-HUD-REWORK-001.md
-->

# NTSD28-BATTLE-UI-HUD-REWORK-001

Pre-change: the previous package added a generic eight-slot `BattleMainUIView` and `BattleCharacterSlotView`. The user's current `NTSD_Battle` scene now has one `HUD/HUDBg` resource panel, one `ComboPanel`, and one `BattleControls` region, so the slot-list root does not represent the actual UI. The current scene contains the obsolete `BattleMainUIView` component on `HUD` with an empty `characterSlots` array.

Required change: supersede the eight-slot code with small, explicit views for the current hierarchy. Keep the snapshot boundary presentation-only and independent from `BattleBootstrap`; do not discover HP/MP or input state from the UI layer in this package.

Expected side effects: the obsolete `BattleMainUIView` component is removed from `HUD`. The scene layout, sprites, anchors, camera, and existing user changes remain untouched. New views have unassigned serialized references until the user binds the actual nodes.

Non-goals: adding eight slots, adding a prefab, importing TEngine, implementing a TEngine `UIWindow`, wiring Button events into `CharacterInputModule`, changing battle state ownership, or making current HUD values a runtime parity claim.

Acceptance: source review, static C# check, no eight-slot references, focused scene component diff, `git diff --check`, and `Tools/Validate-ChangeLedger.ps1`. Unity compile, Inspector binding, UIModule loading, and Play Mode are pending.

## Actual change and validation

- Retired the previous `BattleMainUIView` and `BattleCharacterSlotView` source files and their metadata from the active implementation.
- Reworked `BattleUiContracts.cs` into a single-character HUD state, combo state, action-control state, and one reusable `BattleUiSnapshot`; no slot array remains.
- Added `BattleHudView.cs` for `HeadImg`, `HP`, optional `HpXu` preview, `MP`, and optional character-name text.
- Added `BattleComboView.cs` for `ComboBg`, `ComboTemp`, `ComboArraw`, and optional TMP text.
- Added `BattleControlsView.cs` for `AttackBtn`, `JumpBtn`, and `DefendBtn`, with optional pressed overlays and presentation-only color feedback.
- Removed only the obsolete `BattleMainUIView` MonoBehaviour component from the current `HUD` GameObject. The current HUD hierarchy and serialized visual layout were not reformatted or rearranged.
- Temporary Unity/TMP/uGUI stub compile passed as `STATIC_CSHARP_HUD_SYNTAX_PASS`; this is a syntax/member-shape check, not a Unity assembly compile.
- `Tools/Validate-ChangeLedger.ps1` passed with all four current UI source files covered by this Record.
- New UI files and metadata passed the focused whitespace check. The global `git diff --check` is not clean because the user-modified scene contains Unity serialized empty fields with trailing spaces; those lines were left unchanged.
- Unity assembly compile, Inspector binding, TEngine UIModule loading, actual snapshot source, and Battle Scene Play verification remain pending.

Current status: `CODE_WRITTEN` / waiting for the user's Inspector bindings before the TEngine window and Runtime source integration.
