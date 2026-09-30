<!-- CHANGE-RECORD
id: NTSD28-Q07-CHIYO-TICK54-RNG-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ChiyoNaturalBattlePlayProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable Chiyo ordinary standing attack 0x82 route
evidence: artifacts/diagnostics/NTSD28-Q07-CHIYO-TICK54-RNG-PLAY-001/ACCEPTANCE-20260929.md
-->

# NTSD28-Q07-CHIYO-TICK54-RNG-PLAY-001

Before script edit: existing direct-canonical Chiyo probe matched the formal relative-tick phase, OID419/OID854 counts and puppet action 120/120, but Chiyo action differed only at tick54 (formal60/Unity65). The formal root trace and playable source prove formal `0x82` standing attack RNG at that tick; Unity's Play report omitted the RNG cursor/call. This package adds only opt-in diagnostic snapshots to the existing Editor probe, never changes production combat, content, Scene or input schedule. Exact symbols, expected fields, verification, side effects, risk and rollback are frozen in the Task. Output is a new non-overwriting JSON report. Q07/BATCH-04, D-024, and the total goal remain open.

After script edit: only `NTSD28Q07ChiyoNaturalBattlePlayProbeEditor.cs` changed. `Request`/`Report` gained opt-in `rngAudit`; `TryStartRequest` rejects audit without direct-canonical; `AdvanceAndObserve` captures pre/post scalar synchronized RNG state and Chiyo LinkState/HitConfirmEa for the unchanged measured tick, then records the end-of-tick `0x82` modulo-2 result when present. The existing 120-tick input, neutral step, natural407 candidate check and shutdown are unchanged. `git diff --check` for the script passed. Original Editor compile, one scoped Play, formal comparison, shutdown, protected hashes and ledger validator remain pending. Do not claim a production first difference from unequal initial RNG states.

Final scoped result: original Editor compiled the opt-in probe (newer assembly, MCP filtered errors0), and exactly one non-overwriting original Battle direct-canonical 120-tick Play ran. Formal/Unity synchronized calls agree as 0 through relative tick19; first call-count difference is tick20 formal7/Unity6, both end at callsite0x38. Their offset varies thereafter; at tick54 it is22 calls, both end at0x82 with Unity selection1/action65 versus formal selection0/action60. Previous phase/birth/puppet selected matches are unchanged, with Chiyo action119/120. Ordered shutdown, zero objects/slots/borrowers, Scene clean and four protected hashes passed. Production, DAT, Scene and nonbattle code did not change. This Change ID closes only the scoped first-difference observation; Q07, D-024 and natural child/AI cause remain open. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q07-CHIYO-TICK54-RNG-PLAY-001/ACCEPTANCE-20260929.md).

Final checks: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exit0/PASSED (985 Records, 18 governed diff code files), output `Temp/NTSD28_Q07_ChiyoRng_ledger-validation.txt`; `git diff --check` exit0. Full SelfCheck, other characters, formal same-World replay and final Q07/Q12 gates were not run by this diagnostic-only package.
