<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-NATURAL-UNITY-DRIVER-001
status: VERIFIED
change-kind: Q07_HIDAN_NATURAL_INPUT_UNITY_DRIVER_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal root NTSD2.8-Logan.exe paired playable GameSession/input_routing and Hidan OID24 hid.dat
evidence: NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001 natural action0 source/root EXE 40-tick X580/X1200 traces
-->

# NTSD28-Q07-HIDAN-NATURAL-UNITY-DRIVER-001

Before change: Unity strict Hidan scenario covers controlled action236/no input for 24 ticks. The formal source/root EXE have since shown ordinary attack→jump from action0 reaches action249/236, X580 catches/injures and X1200 misses. Unity same-input full-driver first difference remains unknown.

Exact code path: append strict `ntsd28-q07-hidan-catch-natural/1.0` validation in `NTSD28UnityRawCaptureEditor.cs`, preserving previous schema; fixed 40 ticks, mode0/stage23/difficulty0, seed `0x28A55A5A`, OID24 actor action0 X500 and target action0 X580/X1200, HP500/PP300, zero-based J rows0–1/K rows2–3 only. Existing exporter carries the input into production `SimulationTickDriver`; no new driver or production rule.

Expected effect: two original Editor raw traces for direct selected-field comparison. No DAT, Scene, config, image/audio, nonbattle or framework edit. Acceptance and rollback in Task Contract. Status PLANNED before code.

Actual script: only the declared exporter was extended with a second strict Hidan natural-input schema, 40-tick/formal identity/difficulty0 contract, exact X580/X1200 action0 participants, and four zero-based J/J/K/K carrier rows. Existing controlled action236 schema remains unchanged. Two new JSON fixtures were written. Original Editor compile/capture and first-difference comparison pending.

Final scoped status `VERIFIED`: original Editor recompiled the exporter, had no checked C# Console error, and both existing raw-capture requests returned PASS with 40 completed rows. Against root formal EXE natural input traces, the 11 shared action/PP/HP/XYZ fields matched 440/440 for X580 and 440/440 for X1200, first difference none. The X580 observed action249→236 and tick13 resource/HP values match; X1200 remains a miss. The raw schema does not expose actor CaughtSlotIndex or target CatchSourceSlot90, so their direct Unity full-driver comparison is a separate focused test. Raw header remains `certificateEligible:false`; no physical-key Battle Play/all-state/pixel certification. See same-ID ACCEPTANCE.md.

Final repository gates after the Hidan diagnostic chain: Change Ledger validator PASS, 877 records/11 governed code paths; `git diff --check` exit0; protected Scene/config SHA-256 values unchanged. These gates do not certify Battle Play.
