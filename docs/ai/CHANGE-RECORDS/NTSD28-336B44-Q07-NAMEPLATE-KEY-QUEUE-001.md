<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-NAMEPLATE-KEY-QUEUE-001
status: ROLLED_BACK
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs
authority: observed 336B44 Unity Menu-to-Battle input trace and existing normal-update physical movement probe pattern; no production rule change
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-NAMEPLATE-KEY-QUEUE-001.md
-->

# Q07 natural nameplate probe key queue timing

Created before script edit. Current Editor diagnostic calls `InputSystem.Update()` immediately after `QueueStateEvent` in `EditorApplication.update`; first-difference trace observes keyboard D pressed but MoveAction/canonical input zero at ticks4–7. A normal player-update queue is a hypothesis to test, not yet established cause or a production battle-rule defect.

Planned one-line behavior edit: in `QueueKey` retain the state event but remove the immediate manual InputSystem update, matching the repository's default physical movement probe pattern. Keep cleanup release update, all assertions, output schema and production unchanged. Expected side effect: diagnostic D may enter MoveAction on next normal player update. Accept with generated and original Editor compile zero errors; one new request shows action/canonical state and reports source/projection motion, clean exit and stable four SHA values. On failure, preserve artifacts and leave production untouched. Rollback is a later audited restore of the removed line if needed. Status `IN_PROGRESS`; Play evidence pending.

2026-10-03 first control: generated Editor compiled zero errors; original Editor compiled and ran `q01-words-20261003-04`, which returned to sole clean Menu/four SHA stable. Ticks5–7 still show keyboard D pressed while action/canonical zero and X620 stationary, so removing immediate manual update alone falsifies the first test-harness hypothesis. Original Editor MCP state reports unfocused. Existing physical probes temporarily set `backgroundBehavior=IgnoreFocus` and `editorInputBehaviorInPlayMode=AllDeviceInputAlwaysGoesToGameView`, restoring both afterward. Before the next edit, Task scope is amended to apply/restore those settings solely while this Editor nameplate probe runs and record settings/device state in its bounded trace. This remains a diagnostic environment change, not a production input fix. Status `IN_PROGRESS`; focus-policy control/cleanup pending.

2026-10-03 second control and rollback: `q01-words-20261003-05` exited to sole clean Menu/four SHA stable. While policy was 2/2, observed ticks5/6/8 had keyboard D false, MoveAction/canonical zero and X620 stationary. Probe reported both `focusPolicyAdjusted` and `focusPolicyRestored` true, returning original policy 0/0. Both hypotheses failed as independent controls. With a targeted source patch, removed the transient policy code and reinstated the original `QueueKey` immediate update; no `git restore`, reset or file deletion was used. Final source diff from baseline contains no changed queue/focus behavior; the prior Q01 visible-only variant and Q07 bounded read-only trace remain. Generated Editor final build exited0/zero errors and original Editor refresh was requested after rollback. No new Play after restoring the original diagnostic behavior because the initial trace run already exercised it; final recompilation is checked separately. Status `ROLLED_BACK` for this unsuccessful test-environment experiment, with raw failures retained. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-NAMEPLATE-KEY-QUEUE-001/REPORT.md). Q07 and goal open.
