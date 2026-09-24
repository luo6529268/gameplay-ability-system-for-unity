<!-- CHANGE-RECORD
id: NTSD28-USER-OPOINT-BIRTH-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_LATE_OPOINT_BIRTH_OFFSET_RATIO
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OpointDepthLivesEditorTests.cs
authority: D-024 user all-entity screen-travel-fraction decision and current playable ObjectSpawnPlanner28::plan_frame
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md
-->

# NTSD28-USER-OPOINT-BIRTH-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Created before any script edit. Exact Task Contract: `docs/ai/TASKS/NTSD28-USER-OPOINT-BIRTH-RATIO-001.md`.

Before: both production late-OPoint owners (`BattleLogicObjectPointRuntime` and component `LF2ObjectPointFactory`) add unscaled DAT/frame-center X/Z offsets to the parent's current integer position. The current D-024 core movement slice has already scaled parent travel, so a child offset can occupy a smaller fraction of the fixed Unity view than its formal counterpart. Formal child placement is parent-relative and launch motion is separate.

Intended after: scale only the late child-minus-parent X and Z birth deltas in both producers using the World factors, including native depth `+1`; preserve parent's absolute coordinate, raw launch motion and Y for the later vertical contract. Write the resulting precise and integer task positions coherently. Keep the default-factor-one branch behavior identical. No DAT, Scene, other spawn route or nonbattle change.

Expected side effects: child actual world birth coordinates, hitbox origin and later stage/collision interaction shift under the approved fixed-camera exception. No change to frame/pass timing, object slot or pooling. Validate both original Editor late-OPoint owners in focused right/left configured cases and default-scale depth/source cases after the current broad test job terminates, plus ledger/DAT/Scene checks. Real Battle Scene/EXE visual comparison and other entity spawn categories remain open. Roll back by inverse patch on the three declared script files only, after reviewing concurrent work.

Resume amendment before any script edit: original Editor PID288224 returned a fresh idle/EditMode state with no active test or compile job at 2026-09-23 15:04 UTC. The two production files and declared focused-test file have no pre-existing Git diff. Run configured/default-scale, right/left tests through both actual late materializers RED before production edits; change only relative X/Z child birth offsets, preserving parent absolute coordinate, raw launch velocity, Y and DAT.

RED evidence before production edit: original Editor compiled the new eight-case actual-materializer test with Tundra success. Exact run_tests job e2884de02c7d4b249c2abf2b4e7adca1 completed eight selected cases: four default-factor cases passed and all four configured 2048x1152 right/left World/component cases failed on child precise X (expected 215.36384096024005 or 184.63615903975995, actual 210 or 190). This is a measured screen-ratio first difference, not just a lexical candidate. Next change only the two declared production formulas; no DAT edit.

First GREEN attempt after both producer edits compiled (Tundra success) but configured four tests still failed only on precise X: expected fractional 215.36384/184.63616, observed 215/184. The downstream existing BattleNativeOpointBirthWriter.InitializeBirth intentionally writes kind-1 precise X/Y/Z from the initial integer coordinates after its random-offset phase, matching the formal integer birth model. This is a test expectation error, not evidence that the scaled producer was bypassed: the observed integer changed from 210/190 to 215/184. Before the next test-only edit, correct kind-1 expected precise coordinates to the truncating integer result and keep assertions on XInt/ZInt, raw Vx, both materializers and default scale. Kind-2 fractional behavior may be a separate focused extension if necessary.

Scoped GREEN job 81966f932ad4451cb680d58d28272860 passed all eight kind-1 cases after correcting the diagnostic precision contract: World/component, right/left, configured/default. Before the next declared test edit, extend the same test parameter with kind-2 OPoint cases because both producers admit kinds 1 and 2 but the downstream kind-2 birth path does not use the kind-1 precise-from-integer synchronization. Assert fractional precise X/Z for kind2, truncating XInt/ZInt, and unchanged authored launch velocity. This is still the declared test file and existing two producers only.

Final scoped implementation and checks: two declared late-OPoint producers now multiply only the parent-relative X offset and `(opoint.Z+1)` depth offset by the configured World view factors; parent integer absolute coordinates, raw launch motion and Y stay unchanged. Original Editor Tundra build succeeded with no `error CS` in the filtered build tail. New exact kind1/kind2 × World/component × right/left × configured/default cases passed 16/16 in job 548b31f660d94cc29fdbcf9e28ecb110. Existing default native-depth cases passed 6/6 in job 307cd676d02541d98949ce23381890e2; two existing source materializer cases passed 2/2 in job 34530ec61cb14b98ba02c3bd465fff92. Kind1 precise position is intentionally integer-synced by the existing birth writer; kind2 retains fractional precise values. Battle Scene SHA remained 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0; DAT/Scene Git status empty, scoped diff --check and Change Ledger validation passed (717 Records, 3 governed code files). No real Play or formal EXE visible ratio was run; other spawn writers and all-entity D-024 remain open.
