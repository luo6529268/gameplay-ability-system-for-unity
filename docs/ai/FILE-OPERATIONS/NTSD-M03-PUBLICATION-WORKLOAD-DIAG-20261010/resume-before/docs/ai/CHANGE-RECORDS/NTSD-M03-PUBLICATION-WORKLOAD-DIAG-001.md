<!-- CHANGE-RECORD
id: NTSD-M03-PUBLICATION-WORKLOAD-DIAG-001
status: BLOCKED
code-path: Assets/NTSD/Scripts/Test/Editor/M03PublicationWorkloadDiagnostics.cs
authority: User requests continued M03 verification before production repair
evidence: artifacts/diagnostics/NTSD-M03-PUBLICATION-WORKLOAD-DIAG-20261010/REPORT.md
-->
# M03 finite publication workload observer

Original: existing production counters already classify publication/alpha/repeated builds, but recent combat1000 report omits them. New Editor observer copies these counters at end-camera with fixed capacity and exports only after terminal. No production changes, new cache, profiler, business calls or per-command timing.

Symbols: M03PublicationWorkloadDiagnostics.Begin/Update/Observe/Finish plus Row/Evidence and focused classification tests. Owner is one SessionState-selected request and static Editor hooks, default inactive; hooks removed at terminal/Play exit/reload. Request processor owns Play setup and ordered cleanup. No Scene/settings save. Existing shared Temp request/result operations backed up under matching Operation; no unrelated content removal.

Validation: compile, minimal classification consistency/fixed-capacity checks, one fixed 120+180 workload; current counters and actual bindings, no CPU-gain/0GC certification. Failure retained, no repeated workload. Source/config fingerprints, clean scene restoration and ChangeLedger validation required. Rollback: new file remains inert; deletion needs user approval and operation record. Existing gameplay untouched.


Correction 2026-10-10: attempt-01 actually completed 120+180 logic ticks, but observer rejected its own absolute report path against a relative string and recorded zero camera rows (FAILED). This is diagnostic implementation failure, not strategy or gain failure. Original report/observation retained. Ordered shutdown calls ResetRuntime and clears cumulative counters, so missing camera attribution cannot be recovered after cleanup. Under the user request to continue verification, fix only the same Editor observer and validate exact ProductionEntityStressConfig.FromRequest output ownership before ONE replacement acquisition at attempt-02. This explicitly supersedes the prior single-run/no-retry limit only for this tool defect: two workload runs total, at most one valid acquisition; no candidate/CPU-cost experiment, no altered warm/sample, no rerun based on outcome. Any further invalid acquisition ends INCONCLUSIVE. Prior source saved as observer-before-owner-path-fix.cs.txt.

Fresh verification: existing counter contract tests 20/20; new actual-config ownership test 1/1 (job58702071e50048daa617b8bba15d9cfc), imported and compiled in original Editor. Attempt-02 started only after ownership pass. Runtime collection pending. Production unchanged.


Attempt-02 terminal: runner completed 120+180 and ordered cleanup, but observer stopped PARTIAL at its first second Game camera. Only one tick0/steady0 row exists, submission unavailable, so ZERO valid steady classification rows. No frequency/binding/gain conclusion. Second defect is observer camera scope (Game != world render camera). Retain all raw files and stop bounded experiment; no attempt-03. Same-file correction reuses production CanRenderCamera(world/base) before counting, with temporary in-memory Camera fixtures disposed in finally, no scene save or production change. This correction gets compile/focused only, NOT live acceptance. Final diagnostic state must remain INCONCLUSIVE/BLOCKED.

Final: BLOCKED on valid live acquisition, no active run. Existing counter20/20; final observer2/2 pass (job32ce7b201fdb451c8632fcff1cfdd86b), but two actual workloads produced zero valid steady camera rows. Camera-fixed source is compiled/focused only, no third live run. No production handoff. See final REPORT and analysis-summary.

Final audit: original Editor idle/Menu clean8 after final focused2/2; 157 named protected files unchanged, 8 original backups valid, HEAD/staged unchanged; scoped git diff --check exit0; Tools/Validate-ChangeLedger.ps1 exit0 with historical path warnings. New observer/meta only, production unchanged, no active run or third acquisition. Evidence: artifacts/diagnostics/NTSD-M03-PUBLICATION-WORKLOAD-DIAG-20261010/audit-final.json and validator-final.log. Task stopped INCONCLUSIVE; Change BLOCKED awaiting valid live data, no handoff.
