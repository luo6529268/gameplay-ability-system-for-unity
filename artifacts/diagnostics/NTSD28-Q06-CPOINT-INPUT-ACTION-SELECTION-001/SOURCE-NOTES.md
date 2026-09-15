# CPoint input action selection source witness

Status: source code written only; no build, run, or Unity implementation by delegated writer. Root reports formal330 primary-kind1 inventory1709 with zero nonzero selector fields, corroborated by wider405 content scan. Therefore this source rule witness is retained as synthetic/source-only and must not outrank a formally reachable kind8 task. That inventory was reported by root, not independently rerun here.

Runner: Tools/NTSD28AuthorityTrace/cpoint_input_action_selection_witness.cpp. Expected370 rows: binding240, priority32, sample_gate80, victim_binding18. No throw matrix is repeated (throwvx0/decrease0).

params.actions order is aaction,taction,daction,uzaction,dzaction,faction,baction,jaction. params.current/previous/edge are objects keyed attack,jump,defend,right,left,up,down; all source assignments use explicit InputKey28 enums, not Unity indexes or mislabeled KeyJump properties. Other fields: group,selector,variant,facing,victimAction,victimDeclared,selectedCpoint,declared(list of catcher selected frames),seed42.

Binding group: each8 selector ×2 facing ×9 raw targets0/99/-99/900/-900/999/-999/1000/-1000; additionally declared magnitude99/900/999 for both signs. No intMin case is introduced because native signed negation requires its own contract investigation. Initial source frames100/130 are independent of every target.

Priority group: activate selectors0..last in order with distinct actions201..208, both faces; normal and last-field0. Taction0 changes the A/T eligibility condition and may restore A, so zeroT is not blindly expected to cancel all prior input. Later matched D/Uz/Dz/F/B/J zero values really overwrite the selection to0. Only the final requested action is written once.

Sample gate group:8 selectors ×2 faces ×5 variants: swapped Current/Previous; all samples absent; active edge set0; edge1; edge255. Correct sample source differs between action keys(Current) and directional selectors(Previous). Front/back key mapping follows exact current facing.

Victim binding group: source900 declared; victim vaction0/131/-131/999/1000 implicit, declaration variants only0/131/999; both faces. Additional selected-frame-without-cpoint case perface. Full advance_catch_relations also executes slot1 orphan-kind2 validation if its current selected frame exposes kind2; this is part of actual pass output, not a bypassed helper.

Each row outputs catcherDat/victimDat, config(stage800/180/350 and formal mode defaults), before/after/following(raw, descriptor/snapshot, catch fields, links, B2 and RNG), pass/followingPass full counters/diagnostics and calls/followingCalls. Both spawns complete before reciprocal catch fields and independent input samples are assigned. Initial X300/350,Z250,Y0,counter7/8,latch11/12,prev/snapshot100/130; timeout300; no random pre-advancement. Full following tick has empty/default controls and may produce additional action/AI/lifecycle work, which must be compared separately.

No success claim is made: expected matrix count must be checked by actual compilation/execution and any future independent validator. Current formal-content zero domain means no production selector rewrite is authorized merely by this synthetic witness.
