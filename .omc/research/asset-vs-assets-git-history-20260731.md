# Asset / Assets Git-history recovery audit — 2026-07-31

## Scope and method

Read-only audit only. Neither `Asset` nor `Assets` was copied, deleted, reset, restored, checked out, or otherwise modified.

- Recovered source: `Asset`
- Clean baseline: `Assets`, verified from Git commit `ee3fc7597e77b3c89531108ea25288522e7b1421` (2026-07-26 09:32:16 +08:00).
- Full history source: `I:\GitHub\Unity_GAS\_git_recovery_full_20260731` (730 reachable commits).
- Reflog evidence records a later local commit `ae8fb0c62d3ff6f23d544294bbfa58291a5d9d5c` (`111`) around 2026-07-27 09:09 +08:00, but its object is unavailable. No result below asserts that a file belongs to that commit.
- Every recovered file was converted to a Git blob ID and tested against all reachable history. Content hashes, not modification times, determine `history` status. Modification times are used only to rank *unreachable* candidates.

## Critical structural finding

`Asset` is **not** a usable replacement tree for `Assets`:

- Raw same-relative-path comparison: 14 identical, 12 different, 6,305 Asset-only, 7,717 Assets-only.
- The recovered NTSD payload is split between two recovery containers:
  - `Asset/NTSD/$Folder100002ECA/` maps to `Assets/NTSD/`.
  - `Asset/NTSD/00040000001DF94F27659311/` maps to `Assets/NTSD/Sprite/`.
- Under the combined inferred mapping: 6,305 recovered files are compared with 6,602 baseline files; 4,696 are byte-identical, 1,580 differ, 29 exist only in recovered, and 326 exist only in baseline.
- The second container contributes 688 mapped files: 320 identical and 368 different. All 368 recovered-file modification times precede HEAD, so it adds no post-HEAD candidate; this timestamp fact alone does not prove those files are older or incorrect.

Therefore copying the whole `Asset` directory would preserve invalid recovery-container paths, omit 326 baseline files, and overwrite 1,580 current files without file-level validation. It is not safe.

## Evidence-based classification (inferred NTSD mapping)

| Classification | Files | Decision |
|---|---:|---|
| Recovered differs but its blob exists in reachable Git history | 949 | Proven historical content, not the clean HEAD bytes. Keep `Assets` by default. |
| Recovered differs, unreachable blob, mtime not later than HEAD | 231 | Cannot prove newer. Keep `Assets`; retain recovered only as an archive/candidate. |
| Recovered differs, unreachable blob, timestamp between HEAD and lost commit | 29 | Potential local work after HEAD; candidate for a later file-by-file semantic merge, not an automatic restore. |
| Recovered differs, unreachable blob, timestamp after lost commit | 3 | Strongest candidate uncommitted work; must be reviewed file by file. |
| Recovered-only, unreachable blob, timestamp between HEAD and lost commit | 22 | Potential new local files; candidate only. |
| Recovered-only, unreachable blob, mtime not later than HEAD | 7 | Unresolved; do not restore automatically. |
| Recovered-only blob present in reachable history | 0 | No such files in the inferred NTSD mapping. |
| Second-container differences with timestamps not later than HEAD | 368 | No post-HEAD evidence; timestamp alone does not establish age or correctness. Keep the baseline pending any explicit semantic review. |

## Strongest recovered candidates (not yet adopted)

These three files are not in reachable history and have timestamps **after** the reflog-only lost commit. This makes them plausible later uncommitted edits, but it does not establish correctness or provenance:

| Recovered source | Likely destination | mtime |
|---|---|---|
| `Asset/NTSD/$Folder100002ECA/Scripts/Test/BattleRuntimeSelfCheck.cs` | `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 2026-07-27 10:12:43 |
| `Asset/NTSD/$Folder100002ECA/Scripts/Test/Editor/BattleParityTraceEditor.cs` | `Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs` | 2026-07-27 10:32:08 |
| `Asset/NTSD/$Folder100002ECA/Scripts/Simulation/BattleParitySnapshot.cs` | `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs` | 2026-07-27 10:50:32 |

The 29 pre-lost-commit candidate differences include central rendering, broadphase/AI, object-point, renderer, and focused Editor-test work. The 22 recovered-only candidates are predominantly focused Editor tests plus a small number of diagnostics/temporary or duplicate assets. Together with the three after-lost-commit differences, there are 54 post-HEAD candidates. The second recovery container adds none. All 54 require semantic review and Unity compilation before any selective merge.

## Rationale and recommendation

`Assets` is the only complete, Git-verified, internally laid-out baseline. `Asset` preserves a mixture of:

1. 949 differing files whose recovered blobs are reachable historical Git content,
2. 231 divergent files that are not in reachable history and do not have post-HEAD timestamps,
3. 368 second-container differences with no post-HEAD timestamp evidence,
4. a bounded set of 54 potentially newer local changes, and
5. a malformed path topology produced by recovery.

Recommendation: **retain `Assets` as the active tree; do not use `Asset` wholesale.** Next, if requested, create a separate file-by-file candidate merge list for all 54 post-HEAD paths (29 differing paths and 22 recovered-only paths between HEAD/lost commit, plus three differing paths after the lost commit), then review diffs and validate them before adopting any individual file.
