# Q06 L-02–L-04 formal source witness

Status: `SOURCE4_PASS / UNITY_JOINT_PENDING`. The current formal EXE SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; builder manifest source SHA-256 is `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`. New diagnostic source: `Tools/NTSD28AuthorityTrace/opoint_zero_frame_slot_visibility_witness.cpp`. Builder exit 0 and both independent full-driver captures have the same SHA-256 `2DA583167F96523BF227DE0BB4388702AD7D09E02202FD044FECEE802E58CB01`. Four explicit PowerShell assertions passed. The immutable build manifest and two captures are in this directory.

| Case | Observed source result |
|---|---|
| `repeat_zero_frame`, parent slot20, `wait:0,next:0` | One new child each of three ticks, occupying 50, 51, 52; parent frame counter remains zero. Prior children advance counters 1→2→3 as their slots are scanned. |
| `unscanned_high_slot`, parent slot50 | Child takes slot51 and its frame counter becomes 1 in the same tick. |
| `already_scanned_low_slot`, parent slot60 | Child takes slot50 and remains at counter0 that tick; on the following tick slot50 advances to counter1. A new child then takes slot51 and remains at counter0. |
| `surviving_motion_hold`, parent slot20, initial hold2 | The first tick leaves hold1 and frame counter0 yet still emits one child into slot50. The second tick emits again. This establishes a concrete control for the Unity `FrameDelay` gate; it does not by itself prove Unity differs. |

These cases use synthetic source-matched DAT, zero input, seed42, stage bounds800/180/350 and actual `SimulationTickDriver28::step`. They establish the source rule and slot ordering, not Unity parity or formal-content natural skill behavior. Next is a declared Unity full-driver fixture matching the same DAT, parent slot, hold and tick count, with initial and per-tick assertions. Production remains unchanged until that comparison shows a first difference. Q06/BATCH-03 remains HOLD; Q07 has not started.
