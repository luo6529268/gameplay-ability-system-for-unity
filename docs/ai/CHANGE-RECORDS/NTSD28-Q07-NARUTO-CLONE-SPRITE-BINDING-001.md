<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: formal OID33 ncl.dat frames240-242 and formal ncl.png; existing Q07 Naruto physical input
evidence: q07-naruto-clone-2 actual Play PASS; initial FAIL retained; ACCEPTANCE.md; Scene SHA unchanged
-->

# NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001

Before: prior physical Naruto probe recorded frame285 and OID33 count but no clone frame/pic or formal sprite catalog binding. OID33 source file and image SHA, authored hidden/visible sequence and test boundaries are stated in the Task. Existing script is already modified by the previously verified `NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001`; its working copy and old report must remain intact.

Planned exact edit: only extend that Editor test script with an optional Q07 clone-sprite request mode, unique result root and per-tick clone entry fields. Require authored pic999 no-entry then pic1 formal catalog entry/79×79. Keep old Q07 and legacy request paths unchanged and preserve their outputs. No production battle/render script, Scene, prefab, formal content, old asset, nonbattle or ProjectSettings edit. On a first difference, retain result and inspect actual content/consumer before any production fix.

Rollback: revert only this test-only extension to the existing probe while preserving earlier probe code and output; do not reset the whole file. Verify Unity compile, one target Play, request reset/no overwrite, Scene SHA and Change Ledger; append exact result and limits before status advancement.

Written: added `cloneSpriteAudit` to the existing Q07 JSON request and a separate result root; prior Q07/legacy paths remain. Per-tick OID33 fields now include frame, effective render pic, catalog entry key, source sheet, 79×79 dimensions and central binding validity. The clone-audit PASS additionally requires frame240/pic999 without entry and frame242/pic1 bound to formal `c\nar\ncl.png`. No production code/content/Scene was modified. Unity compile and target Play pending.

First actual Play `q07-naruto-clone-1.json`: report write initially raised `DirectoryNotFoundException` because the new artifact directory had not been created, repeating from `Observe`. Creating only that declared diagnostics directory allowed the same run to write its original result and exit Play. The original result reports FAIL because the test required observing transient frame240 exactly. Its trace actually first sees OID33 at tick11 frame241/pic999/no sprite entry, then tick12 frame242/pic1 with key(33,1), formal ncl.png, 79×79 and valid central binding. This is test-condition overspecification, not a confirmed production visual difference. The source authorizes both frame240 and frame241 pic999. Test-only correction now accepts either hidden authored frame and creates the result directory before writes; write cleanup runs in `finally` so failed I/O cannot leave the observer looping. Original FAIL report remains unchanged; fresh compile and same-case rerun pending.

Verified: fresh Editor import/compile had no new `error CS` in the current log tail; same natural L/D/J case `q07-naruto-clone-2.json` PASS with physical input ticks2/4/6, clone frame241/pic999/no entry tick11 and frame242/pic1/key(33,1)/formal ncl.png/79×79/valid central binding tick12. Editor idle/outside Play on original Battle Scene, request false, both Scene SHA values unchanged. No production code/Scene/resource change. This Change ID closes only logical frame-to-formal sprite catalog binding for one clone sequence; actual draw/pixel/order/shadow/whole-skill remains Q09/Q12/R17/R18. Details in `artifacts/diagnostics/NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001/ACCEPTANCE.md`.
