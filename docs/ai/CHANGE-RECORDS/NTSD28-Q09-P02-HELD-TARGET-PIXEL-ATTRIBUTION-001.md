<!-- CHANGE-RECORD
id: NTSD28-Q09-P02-HELD-TARGET-PIXEL-ATTRIBUTION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor.cs
authority: formal playable presentation_interpolation.cpp and original Battle Scene q09-held-unsat-20260928-b pixel ownership gap
evidence: artifacts/diagnostics/NTSD28-Q09-P02-HELD-TARGET-PIXEL-ATTRIBUTION-001/ACCEPTANCE-20260928.md
-->

# NTSD28-Q09-P02-HELD-TARGET-PIXEL-ATTRIBUTION-001

Before: original Battle Scene proves same-tick held OID120 central command X moves at two unsaturated display phases, but the full-camera pixel differences overlap other command bounds and cannot be attributed to the weapon. After: an opt-in Editor diagnostic renders the same frozen central command frame with and without only OID120 at each phase, records GPU target contribution and compares its masks. Source rule, gameplay World and published presentation are read-only. Exact scope, acceptance, rollback and limits are in the Task.

Expected side effects: four new diagnostic PNGs plus JSON in a new Q09 artifact folder; transient scratch backends and GPU targets only during the opt-in Play. No production, DAT value, character image, Scene, input setting, mode Asset or nonbattle change. Build, original Editor Play, checksum/shutdown/hash, ledger and diff checks are required. The existing `-a/-b` raw failures remain unchanged.

Actual code: only the declared Editor probe gained `captureHeldAttribution`, a separate result root, two temporary all/without-weapon command frames per display phase, production-resource `BattleDynamicMeshBackend` GPU rendering, four PNGs, target-contribution masks and an early/later mask-difference guard. The live published frame and World are never edited. Generated Editor build: 0 errors/191 warnings. Original Editor in-place script compile requested; Play, shutdown, four SHA and ledger checks pending. No old raw report was changed.

Original Editor Play `q09-held-attrib-20260928-a` raw `PASS_SCOPED_PHYSICAL_CHAIN`: physical pickup6/held-air20, motion19→20; actual camera alpha0.2048696970→0.7049303036, weapon command X -10.0540142059→-9.8850116730. At both phases all commands resolved5/5 and without only weapon resolved4/4; GPU differential weapon pixels11 early,6 later, mask symmetric difference17. Independent Pillow decoded four saved scratch PNGs and matched 11/6/17 plus bounds. World checksum before/after all captures identical `e54a96d35593761bc585fd9bf8418b1b0507a20e78b75dded3dd080ebec2bf0b`; target submitted, zero shutdown residue, focus restored, Scene clean. Original Editor PID11944 idle/nonPlay/noncompiling. Battle SHA `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; InputSystem `8616688648A47C7928692C8F8BF72D897331DE7B61FF017C942DB02F8F45E42E`; mode Asset `0AD22411DC42C899094A69F3D13BE9503E2524F6286B636E02C9DF95ABCD7061`; all match pre-run. The result proves central weapon-command GPU contribution only, not full composite/formal same-view/Q09 aggregate. Audit validator and diff check pending final run.

Final audit: `Tools/Validate-ChangeLedger.ps1` returned `Change ledger validation PASSED` with 949 Records and 17 governed code files in diff; its many warnings about historical records declaring paths absent from the current diff do not fail the gate. `git -c core.safecrlf=false diff --check` returned 0. The generated Editor build was 0 errors/191 warnings and original Editor reloaded the changed assembly before this single Play. No new production, DAT, Scene, input/mode Asset or nonbattle diff belongs to this package.

Visual QA correction: direct inspection showed scratch CommandBuffer PNGs vertically inverted relative to the true World camera PNG, so scratch raw Y=227..233/228..233 is not screen Y. A read-only Pillow vertical inversion maps target contribution to actual camera Y306..312/306..311, inside the phase-specific projected weapon bounds. Every 11/11 and 6/6 target pixel's full-command scratch RGBA matches the same true camera pixel exactly; no-target scratch differs. Raw JSON/PNGs remain unchanged. The final claim is the selected weapon's true camera-visible pixel contribution, not whole-frame or formal EXE parity.
