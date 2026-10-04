<!-- CHANGE-RECORD
id: NTSD-BUTTON-PRESS-CONSISTENCY-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/NTSDButton.cs
authority: User requests necessary fixes; installed uGUI1.0.0 Selectable/Button left-pointer and InstantClearState contract
evidence: docs/ai/FILE-OPERATIONS/NTSD-BUTTON-PRESS-CONSISTENCY-001/RECORD.md
-->
# NTSD-BUTTON-PRESS-CONSISTENCY-001
Before: custom pressed state accepts right/middle down/up although base ignores them; Selectable.OnApplicationFocus clears base pressed visual through virtual InstantClearState but custom state remains true.
Change: match left-button filtering on down/up; override InstantClearState, call base then SetPressed(false) so built-in focus/disable clearing and custom state agree. Keep OnDisable fallback, PointerExit release, OnPointerClick/OnSubmit/UnityEvent untouched. No fields/serialization changes.
Only known subscriber BattleControlsView; no derived NTSDButton classes or serialized GUID instances found under Assets. No automatic scene wiring. Multiple-pointer aggregation/ownership, navigation Cancel semantics and application pause policy lack established contract and remain unchanged; normal pointer-up/exit and existing disable cleanup retained.
Acceptance: independent runtime compile, actual source with temporary dependency-stub tests, diff/ledger checks. No Editor/real touch claims. Existing shutdown disable hook remains, no manager/queue/stage changes. Rollback exact before-0.txt and audit.

Actual changes: left-button guard before base OnPointerDown/Up; InstantClearState override calls base then SetPressed(false). Normal exit release and OnDisable fallback preserved. No new serialized fields, callbacks, pointer ownership or click policy.
Local harness9/9 PASS using actual NTSDButton source with a dependency stub matching examined uGUI branches: non-left down, non-left up during left hold, duplicate down, left release, exit dedupe, base focus clearing, disable/clear/up dedupe, inherited click/submit, hook/event counts. This proves local logic only, not Unity message dispatch or real multi-touch.
Current code usage scan: only BattleControlsView subscriber; no derived classes or serialized NTSDButton GUID references found across Assets. Existing scene setup is not changed. InputSystem navigation cancel targets current selected object and is not equivalent to every touch cancellation; adding ICancelHandler is deferred. Same-button multi-touch boolean aggregation remains limited: any pointer-up/exit may release. First-owner versus any-held policy is not selected without user direction. Application pause without focus loss/disable has no explicit hook, unchanged. PointerExit custom release versus inherited reentry visual semantics preserved; no broad Button visual rewrite.
Validation receipts: compile.txt independent runtime compilation; harness-results.txt; ledger.txt. No Editor refresh/Play; runtime acceptance pending.
