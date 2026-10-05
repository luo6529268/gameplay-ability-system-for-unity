<!-- CHANGE-RECORD
id: NTSD-ROLE-SELECTION-UI-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/SelectRoleItem.cs
code-path: Assets/NTSD/Scripts/UI/CharacterSelectionBoard.cs
code-path: Assets/NTSD/Scripts/UI/CharacterChoiceItem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleSelectionSceneValidationTemporary.cs
authority: User explicit role selection join / portrait / confirmation cancellation / five teams request
evidence: artifacts/diagnostics/NTSD-ROLE-SELECTION-UI-001/before.json
-->
# NTSD-ROLE-SELECTION-UI-001
Current: SelectRoleItem has old4stage flow and idle blinking, does not toggle authored Join/portrait/name. Saved Menu has one player slot, one unbound CharItem Image template with Select/TeamIndexTxt, shared5existing team Buttons. Head getter currently returns SmallSprite; new portrait path explicitly uses CharacterUISprites.HeadSprite without changing global resource service/HUD. Player names from GameLocalSettings, Attack InputAction from assigned player map; team UI indices0..4 follow actual config order (legacy final team mapping retained, no battle launch).

Plan: explicit Idle/SelectingCharacter/SelectingTeam (character confirmed) flow, event-driven board and pooled template instances, no new input polling. SelectingTeam shows existing5team options and no second ready confirmation. Public existing legacy methods retained where possible; new normal attack cannot advance to old confirmed/countdown. Initial onlyJoin per player; candidate selection updates head, character confirmed card shows Cancel text; cancel clears team and selection marker state. Shared board selects one explicit player owner, keyboard per-player independent. Re-enable resets state/listeners, pool cells returned inactive. Existing scene transforms/fonts untouched.

Exact click semantics question pending before dependent implementation: firstclick preview/secondsame confirm/thirdsame cancel, versus immediateconfirm. Initial display/input/team resource work independent. Acceptance fresh compile, real authored UI events and current InputAction, repeatedconfirmcancel,5teams, enable/disable/re-entry and multiplayer isolation; original Editor scene clean. No fullbattle alignment claim or deleted tests restoration. Rollback backed-up bytes/newresources only with prior removal audit.

Independent script stage written: SelectRoleItem explicit join/visibility/player name/HeadSprite/direct select/team index API, event notification, disable/re-enable reset, removed idle Update polling. Added CharacterChoiceItem pooled display/click adapter without assigning ambiguous click state transition yet. Initial independent compile0errors (37warnings, one unused legacy countdown field subsequently removed). Existing Menu/GameConfig bytes unchanged even though user committed baseline while task running; no conflicting content overwrite.

Normal Attack while character-confirmed now cancels back to character choice rather than invoking legacy ready/countdown, so this task does not start later flow. Existing explicit OnConfirmTeam API remains for legacy callers outside this UI task. Isolated test setup first lacked TMP package cache and stopped before test source creation; empty batch returned1 without touching main Editor. Revised fixture loads current compiled TMP runtime through existing dependencies.

Independent stage: compile0errors/36warnings; Unity focused66assertions PASS, actual sprite bytes/current action maps with controlled catalog/UI fixture. Original Editor compiler error query0; original Scene SHA unchanged, no Scene Play run. Full report artifacts/diagnostics/NTSD-ROLE-SELECTION-UI-001/REPORT.md. Required pointer click semantics unanswered, so Board/Scene wiring/full real-scene acceptance remain undone. This is not delivered-complete.

User clarification A received via parent: first different candidate click previews, second same confirms, third same cancels; other candidates ignored while confirmed. Continue Board/pool/existing5buttons and scene binding. New current original Editor observation: NTSD_Battle dirty=true/nonplaying, so no scene switching/writing until user saves/permits; independent code/test proceeds. Shared board follows last explicit slot input/focus, state remains per player.

A flow and pooled shared Board implemented; runtime compile0errors/36warnings and isolated Unity115assertions PASS including actual candidate pointer and existing Unity Button event chains, pooled re-entry reuse, P1/P2 focus and isolated selections. Preparing temporary Editor wiring/validation probe declared above; no original Scene modifications until current dirty Battle scene confirmation.

Final independent-stage handoff: NTSD-ROLE-SELECTION-UI-001 remains FOCUSED_TEST_PASS, not original Scene integrated. A semantics resolved; Board/Choice/SelectRoleItem compiled and independent Unity115assertions PASS. NTSD_Battle remains dirty, so Menu wiring/Play/screenshots wait for saved-scene confirmation. Prepared exact hookup and guarded compiled probe retained under artifacts/diagnostics/NTSD-ROLE-SELECTION-UI-001; probe source/meta audited and removed from Assets. REPORT.md is current.

NTSD-ROLE-SELECTION-UI-001 / VERIFIED (scoped): original Unity Menu formal prewarm succeeded with deleted WORDS absent; original Play73assertions PASS, Scene clean after exit. Role independent115assertions retained (P1/P2 fixture); original Scene only one configured player. Full battle SelfCheck/native overlay visual parity not claimed. Audited temporary probe removed; evidence artifacts/diagnostics/NTSD-ROLE-SELECTION-UI-001/scene-result.json and screenshots.
