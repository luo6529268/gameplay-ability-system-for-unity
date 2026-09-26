# Lee J→L OID204 birth-to-despawn trace reconciliation

Status: `VERIFIED / SCOPED_NO_TARGET_CHILD_BIRTH_TO_DESPAWN_TRACE`. This closes the **snapshot/entity lifetime** question for one ordinary, no-target Lee J→L sequence. Q07/D-024/R17/R18 and full battle alignment remain open.

The root formal EXE SHA-256 was freshly checked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. This audit reused its saved LFR release trace from `NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001/run4` and the original Unity Editor complete-Driver `original-editor-run2.raw.jsonl` plus `domain.jsonl` from `NTSD28-Q07-LEE-JL-UNITY-RAW-FIRST-DIFF-001`; it did **not** restart Unity or the EXE. The native file contains initial tick0 and an additional tick46; the equal comparison window is completed ticks1–45.

| Observation | Formal EXE trace | Unity original-Editor raw/domain trace |
|---|---|---|
| OID204/ownerLee birth | Tick6, slots51–55, five native `spawn` events (`sourceLine=1065`) | Tick6, slots51–55, five snapshot-derived `birth` deltas |
| Alive interval | Each slot present contiguously tick6–13 (8 completed ticks) | The same five slots and interval |
| Per-slot action trajectory | `20,20,21,21,22,22,23,23` | Identical for each of five slots |
| Removal | Tick14, five native `lifecycle/despawned` events, terminal code1000 from action23 | Tick14, five snapshot-derived `death` deltas and no occupied OID204 slots |
| Remainder through tick45 | No OID204 in the no-target scene | No OID204 in the no-target scene |

The independent reread compared child slot sets at all 45 completed ticks and 16 state fields for each of the 40 child-tick observations: **640/640 equal, zero first differences**. Fields were slot, OID, action, state, frame counter, facing, X/Y/Z, Vx/Vy/Vz, HP, MP, team and owner. Birth and death slot sets are all `[51,52,53,54,55]` in both traces. The machine-readable [lifecycle-summary.json](lifecycle-summary.json) has SHA-256 `C36C96647E37CE52254B0CD2773486682ACFF68CCA82EF810FECA53A1FF9C112`. Inputs are pinned there by SHA-256: native trace `33FE38881ADB397F0D4667EE76D8FBA1C449FB231B285601BD5160915D1B5A8F`, Unity raw `62BC27E269948033A3A6C6CB4F661CB42A9C64206B5393EA0B75719C082B538D`, and Unity domain `4FE4DAA76355F353A436CCAC7E5CC686A1D800ABD3A857AF4266CFF622BAC753`.

This is stronger than the previous birth-and-camera witness: the five children are observed from birth to terminal absence in both complete-Driver traces, so that **particular** no-target lifecycle does not need an extended 45+ tick replay. It does not prove identical internal cleanup cause or pool behavior at the instant of despawn, collision/target interaction, physical-key host timing, all OID204 branches, or formal/Unity pixel identity. The separate saved Battle Scene Play probes proved natural birth, isolated/composed camera pixels and ordered shutdown/borrowers0, but did not themselves record every child alive tick. The root LFR playback's CRT seed differs from the source/Unity fixture; no CRT draw is observed in this 45-tick window, but this report does not claim full RNG-state identity.

No project source, DAT/image bytes, Scene, ProjectSettings, old asset or nonbattle file was edited for this audit. It reuses accepted existing traces and narrows the remaining Q07 work to cases with a target/collision and other representative lifecycle classes, plus the previously recorded broader phase exits.

Governance check after documentation updates: `Tools/Validate-ChangeLedger.ps1` exited 0 with 849 Records and five currently governed code diffs covered; `git diff --check` exited 0. Alignment, handoff and STATE remained NUL-free. These checks do not substitute for a new target-hit runtime witness.
