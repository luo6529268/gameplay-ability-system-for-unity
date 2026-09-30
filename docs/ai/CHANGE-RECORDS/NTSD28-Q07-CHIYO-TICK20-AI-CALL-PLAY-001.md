<!-- CHANGE-RECORD
id: NTSD28-Q07-CHIYO-TICK20-AI-CALL-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ChiyoNaturalBattlePlayProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable NativeAi28 tick20 call accounting
evidence: artifacts/diagnostics/NTSD28-Q07-CHIYO-TICK20-AI-OWNER-AUDIT-20260929/REPORT.md
-->

# NTSD28-Q07-CHIYO-TICK20-AI-CALL-PLAY-001

Pre-edit record: formal root trace attributes all seven relative-tick20 synchronized calls to OID850/slot51 native AI; Unity prior report has six total, with no per-entity or per-call trace. The existing Editor-only probe can use the production Driver and native random diagnostic observer plus accepted AI trace observer. Add only opt-in tick20 observation to that probe, preserve input and protected work, and end after 20 measured ticks. The Task freezes symbols, expected side effects, acceptance, risks and rollback. This is diagnosis, not authorization to alter a combat rule, DAT, Scene or nonbattle behavior. Q07/BATCH-04 remains open.

Post-edit record: only `NTSD28Q07ChiyoNaturalBattlePlayProbeEditor.cs` was changed. `Request`/`Report` gained an opt-in `aiTick20Audit`; the probe records the unique OID850 runtime slot, action, target, AI flag, relation team and source/physical coordinates after measured ticks19/20. During measured tick20, it attaches the existing direct native random observer and accepted AI trace observer, records each call's origin/site/modulus/result/counter/index/total, and detaches both in a `finally` before the existing ordered shutdown. The same input schedule remains; this mode stops after 20 measured ticks and does not assert later natural407. Production rule, DAT, image, Scene and nonbattle code are untouched. The Task's observer lifetime/duplicate-origin caveat remains.

Pending: original Editor compilation and single Battle Play report, formal comparison, protected hashes, zero-residue exit, validator and `git diff --check`. Status remains `CODE_WRITTEN` until verification proves more.

First Play result and bounded follow-up: original Editor compiled zero probe errors; `chiyo-ai-tick20-20260929-01.json` SHA-256 `74F02B49C1B56F51C22AA6059FEDEA226B7FB4E03E5B780BD18523CD18631C49` recorded 20 samples and six accepted AI calls (`0x14,0x3c,0x1c,0x1e,0x1f,0x38`), direct calls zero, OID850 Unity slot50/action310→311. Ordered shutdown, zero residues, clean Scene and four protected hashes passed; generated build and validator passed. Formal trace has OID419 slot50 at tick18 and OID850 slot51/action311 at tick19, so the next first-difference question is Unity dynamic slot occupancy/ascending-tail timing, not the final `0x38` call. The Task now declares one opt-in live-slot log for ticks17–20 and one new 20-tick report only; no production rule edit.

Follow-up script edit: the same Editor-only probe now appends `earlyDynamicEntities` entries for live runtime slots >=50 after measured ticks17–20, including slot/OID/action/AI flag. This observes the existing World without changing input, frame stepping, spawn or cleanup. The previous immutable report remains intact; second original-Editor compile/Play and formal slot comparison are pending.

Final scoped result: both original Editor 20-tick Play reports passed their diagnostic scope, with zero-residue ordered exit and four protected hashes stable. The second report establishes formal OID419 slot50 versus Unity slot51 at tick18, followed by formal OID850 slot51/action311 versus Unity slot50/action310 at tick19; tick20 formal/Unity synchronized calls7/6. The original Editor assembly compiled the extension with filtered probe errors0. This record closes only the bounded observer behavior and first observed slot difference; allocator search/claim/release provenance and any production fix remain separate open Q07 work. Evidence: [Acceptance](../../../artifacts/diagnostics/NTSD28-Q07-CHIYO-TICK20-AI-CALL-PLAY-001/ACCEPTANCE-20260929.md).
