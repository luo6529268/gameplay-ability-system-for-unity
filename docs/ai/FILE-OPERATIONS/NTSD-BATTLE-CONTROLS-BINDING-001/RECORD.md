# NTSD-BATTLE-CONTROLS-BINDING-001
Overwrite only3declared scripts, no deletion. User bugfix authorization; Codex executor; per-file prehash/backup/git status in diagnostics before.json/git-before.txt.
Requirement: repair actual BattleControlsView button-to-character path. Current saved Scene has valid three NTSDButton references, but production has no BindPlayer caller. Existing CharacterInputModule registers input sink and preserves device/UI OR. UI observes selected-human HUD immutable publication; extend that publication with actual Roster InputId (not PlayerIndex+1), preserve session/handle to release on identity changes. No mutable World reads in UI, no first-AI/default Player1 fallback. OnEnable subscribe/replay current snapshot, OnDisable/Destroy release/unbind, invisible/stale events safe; native sampling/mapping/data unchanged. AppManager and BattleTestBootstrap both NotifyBattleHudParticipantReady only after valid human/slot registration, so single producer seam covers both. No new lifecycle manager. Stop UI ingress and release holds on invalid publication/disable; do not alter shutdown order. Validate fresh runtime/editor compilation; actual authored UI NTSDButton pointer events through sink, sample and native actions; 3actions/up/down/multitouch/keyboardOR/disable/re-enable/rebind/stale. OriginalEditor Play only after current idle/saved confirmation. Preserve Combo pendingbinding/scene/HUD PP/ripple. Rollback exact backed up bytes with subsequent changes preserved, no commit/push/deleted tests restoration.

Own isolated helper correction: Temp NTSDControlsValidation-001/Assets/Editor/ComboValidation.cs backed up probe-first.cs.txt; change generic ExecuteEvents conditional to two typed calls. No production change.

Create temporary Assets/NTSD/Scripts/Test/Editor/ControlsSceneValidationTemporary.cs and Unity-generated meta for authorized originalScene verification. No preexisting files at path; later removal requires exact backup/hash record.

Own isolated helper/result overwrite backed up probe-second*, set explicit Dynamic InputSystem update for Editor-update-driven device simulation; production input unchanged.

Probe-only third correction after unsupported public Update overload compile failure; preserve probe-third.*; configure isolated InputSettings focus routing instead, no project settings edits.

Temporary originalScene probe overwrite backed up original-probe-first.cs.txt; fix JSON Vector2 self-referencing serialization by recording scalar x/y, ensure ExitPlaymode in finally. No production change; first run not counted PASS.

Probe/result overwrite backed up original-scene-unfocused-result.json/original-probe-unfocused.cs.txt. Add GameView focus to probe Begin; button intentionally rejects unfocused app. Preserve production focus guard.

Before deletion: remove only our temporary ControlsSceneValidationTemporary.cs/.meta; exact paths/hashes/backups in diagnostics/probe-removal.json. OriginalScene PASS137, editor exited. User authorized diagnostic work; no prior user file at these paths.
