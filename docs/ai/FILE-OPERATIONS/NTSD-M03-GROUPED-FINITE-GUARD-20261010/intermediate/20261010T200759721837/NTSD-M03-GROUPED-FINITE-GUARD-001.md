<!-- CHANGE-RECORD
id: NTSD-M03-GROUPED-FINITE-GUARD-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M03FiniteVertexScanDiagnostics.cs
authority: Human-authorized bounded M03 grouped finite guard handoff via discussion thread 01a11fe2-bfaa-72f3-a9ed-c8260f63b193; exact contract artifacts/diagnostics/NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-20261010/execution-handoff.txt.
evidence: docs/ai/TASKS/NTSD-M03-GROUPED-FINITE-GUARD-20261010.md
-->
# NTSD-M03-GROUPED-FINITE-GUARD-001
Original Upload checks all12 coordinate scalars per private WriteQuad rectangle; ON checks stored v0.xyz/v1.y/v2.x at same Upload entry before EnsureMesh/ActiveQuadCount/native mutation. Same ArgumentException/message and chunk order/partial-success API counters. OFF retains original loop. Only Build passes bool to Upload; cold instance internal property defaultfalse, Clear/Dispose disable. No writer/UV/resolver/segment/upload/stride44/index/capacity changes.
Owner: backend existing lifecycle; one capacity1 bool, disabled by stage5 existing Clear and Dispose; no resource/cache/drain or new stage dependency. Candidate scans existing active staging only, all locals call-owned. Tests own temporary fixtures and always Dispose, no shared resources.
Minimum: RED missingflag, actual compile, OFF/ON prepared3bindings full44byte/index/submesh/segment/bounds/diagnostics/publication,0/1/4096/4097 Ordered/Strict/shrink/empty/native recovery/seal. NaN/+Inf/-Inf axes and generated size/pivot overflow, invalid0/4096 same exception/API counters/native unchanged failedchunk and existing Clear recovery. Reuse unchanged pixels and old mirror IEEE negativecontrol with explicit private writer dependency.
One fullBuild array2000/4097 ABBA each8warm+32sample, oldUV OFF, no fine timers/counters, calibrated currentthread GC0event validscope. First hardfail stops; no JIT/warmchange/Profiler/retry/localmirror rerun. BothONmean<bothOFFmean and maxON P95/P99<=maxOFF perlayout. Even PASS signal only/no default.
Risks: future nonrectangle writer violates invariant, missed written arithmetic overflow, guard moved past native mutation, noisy cost. Rollback only reviewed own delta from true dirty backups after audit, never HEAD restore/unrelated content. No irreversible battle-rule changes.

Symbols: GroupedFinitePositionValidationEnabled/Build/BattleMeshChunk.Upload/Clear/Dispose; M03FiniteVertexScanDiagnostics actual candidate methods and explicit fullBuild cost.

CODE_WRITTEN: RED17683785fdeb4f2eb5b0a8c635617381 expectedmissingflag. Exact two scripts: private instanceflag/coldproperty, Build passesbool, same Upload site ON grouped5storedscalars/OFF unchanged12loop, Clear/Dispose disable. Writer/UV/nativeAPI unchanged. Added actual prepared payload/lifecycle/axis/partial native/finiteinput arithmetic overflow tests; pending compile.

Checkpoint: actual candidate GREEN job0be8757fdc58452fa8f4b7e056bdbb02 13/13 PASS; red missingflag failure preserved. Added exactly one Explicit complete Build ABBA entry in same declared diagnostics path, fixed8warm/32sample, two layouts2000/4097, reliable existing recorder, firsthardfail stop, create-new sentinel/raw, no local-cost rerun. Post calibration attempted once and preserved even on failure. Cost compile/run pending.
