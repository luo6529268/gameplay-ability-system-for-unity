<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: formal Logan EXE and playable input_routing.cpp; formal Naruto DAT hit_Fa 285 and OPoint OID33
evidence: q07-naruto-fa-1 actual Unity Play PASS; ACCEPTANCE.md; protected Scene SHA unchanged
-->

# NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001

Before: formal content is selected by the production GameConfig and a built Player can load it, but Q07 has no fresh natural key-to-skill/spawn acceptance. Existing `BattleComboPlayModeProbeEditor` sends real Input System keyboard states and tracks combo/frame on the live battle Scene, yet its requests/results use fixed legacy paths and cleanup that could overwrite previous evidence; it does not identify the formal source or OID33 spawn.

Planned exact edit: extend only that Editor test probe with a separate Q07 request/result branch, formal root/fingerprint/Naruto preflight and per-tick OID33 count. Keep prior menu/legacy behavior unchanged; for the Q07 branch mark request `requested=false` instead of deleting it and reject an existing result path. No production logic or assets touched. The Task gives inputs, output, validation and rollback. Append actual implementation/results and first differences before advancing status.

Written checkpoint: the declared Editor probe accepted `Temp/NTSD28_Q07_NarutoForwardAttack.request.json`, entered Play if needed, validated a unique run ID/output, marked its request false without deletion and reused the existing physical forward-attack sequence. Before queuing keys it required the formal GameConfig root/fingerprint, Naruto OID2 and authored hit_Fa285; it recorded per-tick OID33 count, peak and source key, and Q07 PASS required the target frame and an actual OID33 occurrence. Legacy menu items/request paths stayed on their prior branch. Compile and actual Play validation were pending at this checkpoint; the result follows below.

Verified: the existing Editor compiled the changed probe and actual `NTSD_Battle` Play produced `q07-naruto-fa-1.json` with `PASS`. Physical L/D/J steps were observed at ticks 2/4/6, authored frame 285 at tick 6 and frame 286 plus one OID33 at tick 11. Formal root/fingerprint was true; objects rose 4 to 7. The request now reads `requested:false`; Editor reports idle/outside Play. Protected Scene SHA is `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6`, unchanged; GameConfig SHA is `C4DB45C7426045D09FB481B2FC1D3FF27089CAF902A7AFE0CA413DE1795DF4B2`. This verifies only one authored natural physical-input-to-spawn path; original EXE whole-tick trace, hits, lifetime, pixels and all-character R18 remain unverified. Evidence and commands are in `artifacts/diagnostics/NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001/ACCEPTANCE.md`.
