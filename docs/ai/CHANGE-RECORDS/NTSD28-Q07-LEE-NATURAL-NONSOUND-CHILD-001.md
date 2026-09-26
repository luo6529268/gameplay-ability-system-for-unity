<!-- CHANGE-RECORD
id: NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001
status: VERIFIED
change-kind: Q07_LEE_NATURAL_NONSOUND_CHILD_FORMAL_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/lee_natural_child_lfr_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable input/frame/OPoint chain
evidence: docs/ai/TASKS/NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001.md; artifacts/diagnostics/NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001/ACCEPTANCE.md
-->

# NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001

Before edit: the original Unity Editor has a controlled Lee frame22→OID706 full-Driver child witness, but its natural 600-tick AI route only visited sound-only OID814 OPoints and did not establish a naturally selected persistent child. Formal Lee DAT also has frame164→OID204/action20, with an authored frame145→146→164 chain and `hit_ad:145` on reachable attack frames. The paired input router maps `hit_ad` to J,L key history. No natural Lee J→L source or root-EXE child playback has yet been observed. Existing diagnostics must remain unchanged.

Declared edit: add only `Tools/NTSD28Q07Diagnostics/lee_natural_child_lfr_probe.cpp` to search a bounded ordinary sampled-input schedule, emit explicit initial conditions and per-tick trace, and encode an LFR only after the production input/frame/OPoint chain creates a persistent OID204. Source compilation and root-EXE playback are focused acceptance; no Unity production or original authority write. Risks: 2tu sampling, initial mode/team/HitStop, and frame wait can make a naive J→L attempt miss the combo. Preserve all negative attempts and report the actual input history/action before adjusting the diagnostic; never add a special-case game rule. The exact Task defines rollback and later Unity gate.

After edit: only the declared diagnostic C++ file was added. It compiled from the paired playable/core source with g++ exit 0; the runnable binary SHA-256 is `8A933BF70A999155771CB5CF29389BB6A3EDF2815C6F0FF0F047390165B8BA44`. Ordinary sampled J at tick 2 followed by L at ticks 3–4 reached Lee actions 145/146/164 on ticks 4/5/6 and spawned five owned OID204/action20 children on tick 6. The unchanged root formal EXE replayed the generated 45-tick LFR with exit 0, passed true/failureCode 0. Source versus release ticks 1–45 matched input phase, actor action, MP and owned-child count, 180/180 comparisons. Exact artifacts and SHA values, including preserved negative setup attempts, are in the linked ACCEPTANCE report.

Verification scope: formal source/release diagnostic exit satisfied. `nativeParityClaim=false` in the playback report and the selected-field comparison do not imply all-state or visual parity. Original Unity Editor same-input full Driver, physical key input, child source coordinates, pixels and lifecycle remain pending under Q07/D-024. No DAT, production Unity script, Scene, resource, formal EXE or existing diagnostic was edited. This Record's `VERIFIED` status applies only to the bounded formal source/release diagnostic.

Governance after report/progress update: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 with 845 Records and all three then-current governed code diffs covered, including this C++ diagnostic; its many historical non-diff-path warnings were nonfatal. `git diff --check` exited 0. The alignment, handoff and STATE documents remained NUL-free. No Unity test was run for this package.
