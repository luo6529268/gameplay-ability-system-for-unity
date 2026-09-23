<!-- CHANGE-RECORD
id: NTSD28-Q07-PRODUCTION-SCENE-LIST-001
status: FOCUSED_TEST_PASS
change-kind: GOVERNANCE_ONLY
code-path: none
authority: user-confirmed Menu-first Battle-second order; Q07 actual Menu callback failure; H-02 first-Scene decision requirement
evidence: exact EditorBuildSettings m_Scenes diff; q07-menu-scene-closure-2.json PASS; Menu and Battle Scene SHA unchanged
-->

# NTSD28-Q07-PRODUCTION-SCENE-LIST-001

The ledger validator has no metadata category for project settings: `change-kind: GOVERNANCE_ONLY` and `code-path: none` declare that no governed script was changed. The actual non-script edit is exactly `ProjectSettings/EditorBuildSettings.asset::m_Scenes`, tracked by this record and the Task; the metadata does not claim a documentation-only edit.

Before: `EditorBuildSettings.asset::m_Scenes` is empty. The saved Menu Scene's actual callback reaches Fight, but the Battle Scene cannot load additively. The original Unity Editor is open on this repository; the unrelated `I:\UnityPreject\test` Editor predates this NTSD work and is outside this change.

Planned after: only `m_Scenes` becomes the user-confirmed enabled Menu index 0 / Battle index 1 list using the existing Scene GUIDs. Preserve `m_configObjects`, both serialized Scene files and all nonbattle assets. No battle rules or scripts change.

Expected effect: normal Editor Build Settings and default Player entry resolve the Menu and its additive Battle dependency. This does not by itself prove the full Menu, battle, Android or Player runtime flow.

Validation: exact setting diff, current GUID/path and Scene hash check, original Editor's existing Q07 Menu callback Play probe, then focused status and Change Ledger validation. Any failure remains recorded; no second Unity Editor and no computer-use.

Rollback: reverse only the two-entry `m_Scenes` change after checking then-current contents and obtaining any approval required by `AGENTS.md`; do not use `git restore` or overwrite other work.

Written: only `EditorBuildSettings.asset::m_Scenes` changed to the two enabled existing Scene paths/GUIDs. The resulting file SHA-256 is `8D621A077642B5154BA305861CA57DFB549FF4FDE5BC58F3E284536A5982F01E`; `m_configObjects` remained unchanged. Menu and Battle Scene SHA-256 still equal the pre-change values in the Task. The first probe `q07-menu-scene-closure-1.json` failed at edit preflight because the already-open Editor had not reloaded the external Build Settings edit. A `refresh_unity(mode=force, scope=all, compile=none)` request through the existing original Editor bridge returned success/idle. The second probe `q07-menu-scene-closure-2` passed edit preflight and entered actual Menu Play; final outcome is pending.

Final focused outcome: `q07-menu-scene-closure-2.json` is PASS in the original Editor. Actual saved Menu component callbacks prewarmed the formal root, selected Naruto OID2, loaded Battle Additive, reached BattleRunning with two World objects and three matching formal publication keys, then unloaded to Menu with `Stopped` and zero pool borrowers. The request was cleared (`requested=false`, `restorePending=false`). Menu/Battle Scene SHA-256 remained `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1` / `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. No physical pointer/keyboard or visual UX, Player cold-start, Android build/device, or formal EXE equivalence is claimed. Q07 remains open.
