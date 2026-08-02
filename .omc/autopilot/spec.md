# Dat Skill Flow Web — Executable Specification

## 1. Objective and boundaries

Create a standalone, local Windows web tool at `Tools/DatSkillFlowWeb`. It is independent of Unity: no Unity project references, editor integration, menu/HUD implementation, networking, or complete AI are in scope.

The tool edits and previews NTSD character skill/frame data using real `data.txt`, DAT, and BMP assets. It runs as a loopback-only Node service with a browser UI. If recovery/audit finds no usable existing web source, create a new minimal skeleton rather than inheriting behavior from any README.

### Authority

- **Behavior authority:** `J:\QQFile\NTSD2.4\ntsd_cpp`.
- **UX-only references:** `CharacterFramePreviewWindow` and `LF2.IDE`; their gameplay behavior is not authoritative.
- Every implemented simulation rule must have an authority-ledger entry pointing to the C++ source location, or be explicitly marked as an unimplemented/temporary non-authoritative UX behavior.

## 2. Product requirements

### Local execution and file safety

- Bind the Node server to loopback only (`127.0.0.1`/`localhost`); do not expose a LAN listener.
- Open only user-selected project/data directories. Canonicalize each allowed root and requested path with Windows realpath/handle-backed resolution, reject `..` traversal and any symlink/junction/reparse-point escape, and revalidate containment immediately before every read or write. Preserve source file encoding, line endings, comments, whitespace, ordering, and unrecognized tokens wherever feasible.
- `Save As` is the default write path. Direct overwrite requires an explicit confirmation.
- Serialize saves through a per-canonical-target lock. Record the target hash at load, then recompute and compare it again while holding the lock immediately before commit; abort on external modification.
- Implement Windows overwrite publication in the bundled `scripts/windows-replace-file.ps1`. The script must use PowerShell `Add-Type` to P/Invoke `kernel32!ReplaceFileW`, return a structured result, and map Win32 error codes. Node must invoke it with `spawnFile`/`execFile` and an argument array (shell disabled); never build or concatenate a command string.
- For overwrite, create an exclusively named temporary replacement in the target directory, write and sync it, then close it. Reserve a unique backup pathname that does not exist, but do **not** copy or create the backup beforehand. Pass that path as `ReplaceFileW(lpBackupFileName)` so Windows creates the backup in the same replacement operation. Never pre-delete the target, replacement, or backup path, and never downgrade overwrite to rename, copy/delete, or best-effort publication.
- A failed `ReplaceFileW` call does not permit a blanket guarantee about which file remains where. Map and report `ERROR_UNABLE_TO_REMOVE_REPLACED` (1175: target and replacement are expected at their original target/temp paths), `ERROR_UNABLE_TO_MOVE_REPLACEMENT` (1176: replacement remains at temp and the original target is expected restored at target), and `ERROR_UNABLE_TO_MOVE_REPLACEMENT_2` (1177: replacement remains at temp and the original target may remain at the backup path while target may be absent). After any failure, probe target, replacement, and backup paths without modifying them; report their existence and hashes alongside the error and preserve all recovery paths/instructions. Never silently clean up a failed publication.
- For `Save As` to a new file, open the final destination directly with Node `fs.open(path, 'wx')`, write, sync, and close it. Reject `EEXIST`; do not pre-check existence and do not publish through rename.
- Never delete an existing user file as part of save, overwrite, migration, or cleanup.

### Data fidelity

- Load real `data.txt`, character/skill DAT files, and BMP sheets.
- Parse into a lossless token/CST representation: unchanged content must round-trip byte-for-byte when possible; changed regions must retain all unrelated original trivia/tokens.
- Expose structured frame editing without dropping unknown sections, fields, duplicate keys, comments, or malformed-but-preserved content.
- Trace every parser/serializer transformation; round-trip tests compare original and emitted files and report intentional edits precisely.

### Deterministic preview simulation

- Use the authoritative fixed main-loop quantum `FRAME_MS = 33` milliseconds (approximately 30.303 Hz, nominal 30 fps). Canonical simulation advances by integer ticks, never by accumulated floating-point seconds; do not describe it as exact `1/30` second or infer that 300 ticks equal 10 seconds. UI labels may say `30 fps nominal / 33 ms`. Rendering must not alter simulation state.
- Implement deterministic integer-tick scheduling and a controllable playback timeline (play, pause, single step, seek, loop range, frame/tick display).
- Support `wait`/`next` frame progression and state transitions based on C++ authority. Preserve the late-pass sentinel ordering: `next=1000` makes `frame_tick` leave frame 1000, runs collision once, then ordinary late range cleanup frees the entity. `next=1280` makes `frame_tick` leave frame 1280 and runs collision once, but late processing first matches the `frame / 100 == 12` special branch; it sets the relevant related objects' and the entity's `hit_stop` to `1100 - 1280 = -180`, resets the entity frame to 0, and keeps it active. Frame 1280 must never fall through to ordinary out-of-range cleanup/free.
- Model and visualize all ten hit fields: `hit_a`, `hit_d`, `hit_j`, `hit_Fj`, `hit_Fa`, `hit_Da`, `hit_Ua`, `hit_ja`, `hit_Dj`, and `hit_Uj`.
- Provide dynamic collision/hit slots, cross-DAT `opoint` spawning, `wpoint` held-object synchronization, XYZ movement, jumping, Z sorting, clone/duplicate behavior, and camera behavior, all only to the extent traceable to the authority.
- Determinism tests must replay identical input/scripted actions and assert identical state snapshots and trace output.

### Editing workflow

- Provide frame/timeline editing with undo and redo. Each mutation is atomic, named, and reversible; undo/redo must restore both model state and selection/timeline context where applicable.
- Surface parse errors, missing assets, unsupported authority rules, and unsafe save conflicts clearly without silently repairing or discarding data.

## 3. Architecture constraints

- Keep browser UI, Node filesystem/service layer, DAT lossless syntax/model layer, simulation core, and authority/trace infrastructure as separate modules.
- Require Node.js 24.11 or newer and zero third-party runtime, development, build, or test dependencies. `package-lock.json` must resolve zero dependency packages; Vite, Vitest, and Playwright must not be required dependencies.
- Use `node:test` for the required automated suite. Build TypeScript sources with `node:module` `stripTypeScriptTypes` into emitted JavaScript, and run/test the emitted JavaScript rather than relying on direct TypeScript execution.
- Implement the browser UI with native HTML, CSS, JavaScript, and Canvas APIs. Playwright is optional only as an externally available system-browser acceptance aid; its absence must not block any required build, test, or gate.
- Every build must generate a unique build ID, stamp emitted assets with it, and publish a manifest enumerating the exact current outputs. The server must load the manifest and refuse to serve missing, unlisted, or build-ID-mismatched `dist` artifacts, so stale output can never be served even when the build runs without cleaning `dist` first.
- The simulation core must be runnable headlessly for deterministic tests; it must not depend on browser timing, canvas state, or Node file I/O.
- Browser animation may interpolate presentation only; canonical state advances exclusively through integer scheduler ticks of `FRAME_MS = 33`.
- Use explicit stable IDs for dynamic objects/slots so collision, hit, opoint, held links, trace records, and replay snapshots remain auditable.

## 4. Authority ledger and trace contract

From Phase 1 onward, every behavior-facing feature ships with:

1. an authority-ledger record (C++ source file, function/region, rule summary, implementation status);
2. a machine-readable per-tick trace with inputs, frame transitions, slot allocation/release, collision candidates/results, hits, opoint/wpoint effects, and save/parse diagnostics as applicable;
3. a focused test that cites the ledger rule and asserts observable outcomes.

Do not represent unverified behavior as parity. The UI must label rules without C++ backing as unsupported or provisional.

## 5. Delivery gates

### Gate 0 — Recovery and foundation

- Audit `Tools/DatSkillFlowWeb` for recoverable source without trusting README behavior.
- Record the audit result and preserve all current repository files.
- If no usable source exists, scaffold the loopback-only Node + browser application, test runner, and module boundaries.
- Acceptance: app starts locally, binds only to loopback, no existing file is removed, and the authority ledger/trace test harness has a runnable empty baseline.

### Gate 1 — Lossless data pipeline and safe save

- Load `data.txt`, DAT, and BMP fixtures; implement lossless token/CST parse/serialize and structured frame access.
- Add direct-exclusive Save As, canonical-root containment checks, per-target serialization, external-change detection, `scripts/windows-replace-file.ps1`, the Node `ReplaceFileW` adapter, and explicit overwrite confirmation.
- Acceptance: unchanged fixtures round-trip losslessly; intentional edits preserve unrelated syntax; traversal and symlink/junction escape tests fail closed; concurrent writers to one canonical target serialize and recheck hashes; temp and not-yet-created backup pathnames are exclusive; `Save As` uses final-path `fs.open(..., 'wx')` and rejects an existing destination without a check/rename race; tests cover both the PowerShell script and Node adapter, safe argument-array invocation, mapped 1175/1176/1177 failures, and post-failure target/replacement/backup location-plus-hash recovery reports; no overwrite pre-copies a backup, pre-deletes a path, or degrades to best-effort replacement.

### Gate 2 — Core deterministic preview

- Implement the headless integer-tick `FRAME_MS = 33` scheduler, frame `wait`/`next`, timeline playback/loop/seek, and browser preview synchronized to canonical ticks.
- Add initial ledger entries and traces for each implemented frame rule.
- Acceptance: deterministic replay tests pass; timeline controls produce the same final snapshot as equivalent single stepping; rendering-rate changes do not change simulation results; timing tests assert 33 ms integer ticks rather than exact `1/30`; ordered trace fixtures prove `next=1000` receives one collision before ordinary late free, while `next=1280` receives one collision, takes the frame-group-12 branch, applies `hit_stop=-180`, resets to frame 0, and remains active without ordinary late free.

### Gate 3 — Combat-object mechanics

- Add all ten hit fields, dynamic collision/hit slots, `opoint` cross-DAT spawning, and `wpoint` held synchronization under authority-ledger coverage.
- Acceptance: fixture scenarios validate slot lifecycle, hit outcomes, spawned-object DAT/frame selection, held synchronization, and complete per-tick traces.

### Gate 4 — Spatial mechanics and editor completion

- Add XYZ/jump/Z sort, clone behavior, camera behavior, undo/redo, and polished diagnostics/playback workflow.
- Acceptance: authority-backed spatial replay scenarios are deterministic; undo/redo restores edited documents and UI context; all supported behaviors are ledgered, traced, and tested.

## 6. Explicit exclusions

- Unity integration or any dependency on Unity runtime/editor.
- Game menus, HUD, multiplayer/networking, and full AI reproduction.
- Treating README text, `CharacterFramePreviewWindow`, or `LF2.IDE` as gameplay authority.
- Any destructive cleanup, replacement strategy that first deletes a user file, or silent lossy DAT normalization.
