# Dat Skill Flow Web — Implementation Plan

## Scope and non-negotiables

Build `Tools/DatSkillFlowWeb` as a fresh, standalone, zero-dependency Node application. `package.json` must constrain Node to `>=24.11.1 <25` and declare no runtime or development dependencies. Author browser code in TypeScript where useful, emit JavaScript with Node's `stripTypeScriptTypes`, and use native HTML, CSS, and Canvas rather than a browser framework or bundler. Unit and integration tests use only built-in `node:test` and `node:assert`. The browser application is served only by a Node 24 native-HTTP process bound to `127.0.0.1`; it has no Unity dependency and no LAN listener.

If `vite.config.ts`, any Vitest configuration, or `playwright.config.ts` already exists, preserve it unchanged as a legacy optional artifact: never delete it and never make Vite, Vitest, or Playwright a required script, dependency, build input, or acceptance condition. Playwright may be used only as an optional, system-provided E2E supplement; its absence must not prevent install, build, test, or acceptance.

The existing repository `README` files and data/C++ runtime materials are retained. They can supply paths, fixtures, and context, but README text is never behavioral authority. Gameplay behavior is authoritative only when traceable to `J:\QQFile\NTSD2.4\ntsd_cpp`. The optional `NTSD_DAT_CORPUS_ROOT` environment variable may select a read-only external corpus fixture source; its absence must not prevent the app or its synthetic test fixtures from running.

Do not implement menu/HUD flow, networking, full AI, Unity integration, destructive cleanup, or silent DAT normalization.

## Target layout and ownership partitions

```text
Tools/DatSkillFlowWeb/
  package.json                         # Node >=24.11.1 <25 scripts; zero dependencies
  package-lock.json                    # lockfile with zero dependency packages
  vite.config.ts                        # retained legacy optional artifact if already present
  vitest.config.*                       # retained legacy optional artifact if already present
  playwright.config.ts                  # retained legacy optional artifact if already present
  index.html
  src/
    client/                             # DOM/canvas UI and presentation interpolation only
    syntax/                             # byte-preserving DAT/data.txt lexer, CST, diagnostics, serializer
    model/                              # structured document/frame facade and edit commands
    project/                            # selected-root/session metadata and corpus discovery
    assets/                             # BMP discovery, decoding metadata, sheet/frame lookup
    server/                             # Node HTTP API, loopback binding, filesystem/save policy
      windows-replace-adapter.ts        # PowerShell/ReplaceFileW overwrite adapter and Win32 error mapping
    sim/                                # pure headless FRAME_MS=33 simulation, scheduler, replay, snapshots
    authority/                          # C++ authority ledger and unsupported/provisional status
    trace/                              # machine-readable parse/save and per-tick trace schema/writer
  tests/
    fixtures/                           # synthetic data.txt/DAT/BMP fixtures; no external corpus required
    unit/                               # syntax/model/sim/authority/trace tests
    integration/                        # server, filesystem safety, and deterministic replay tests
    system-e2e/                         # optional system-provided browser workflows; not acceptance
  scripts/
    build.mjs                           # Node stripTypeScriptTypes build and build-ID manifest writer
    windows-replace-file.ps1            # Add-Type/PInvoke kernel32 ReplaceFileW bridge
  dist/
    manifest.json                       # build ID and exact server allowlist for this output
```

The module dependency direction is: `syntax -> model -> project/assets`; `sim` remains independent of Node, browser, canvas, and filesystem; `authority` and `trace` are shared contracts; `server` adapts filesystem work; `client` invokes server APIs and renders snapshots. Do not let `client` mutate canonical simulation state except by submitting explicit commands/actions.

## Gate 0 — recovery, audit, and zero-dependency foundation

1. Audit `Tools/DatSkillFlowWeb` using source files and configuration, not README claims. Before any scaffold action, write a preservation manifest covering every existing file below that directory: relative path, byte size, and SHA-256. Write a second manifest after scaffolding and prove that every entry from the first still exists with the same size and SHA-256; this includes pre-existing, ignored, and untracked files, so Git status/diff is not evidence for preservation. Record what is retained and why a minimal new skeleton is required if recovery is unsuccessful.
2. Scaffold a native HTML/CSS/Canvas client, a Node 24 native `http` server, and built-in `node:test`/`node:assert` test harness. `package.json` must enforce `engines.node: >=24.11.1 <25`, contain zero runtime and development dependencies, and produce a zero-dependency `package-lock.json`; `npm ci --offline` must succeed from that lockfile. The server must listen with host `127.0.0.1` (never an unspecified host or LAN interface), publish the selected ephemeral/fixed local port safely, and serve the built/browser assets and API under the same loopback origin.
3. Implement `scripts/build.mjs` using Node's `stripTypeScriptTypes` to emit JavaScript. Each build creates a fresh build ID and a manifest that names exactly the generated files the production server may serve. The server must load the current build manifest and reject every path not in its allowlist, so a no-clean build cannot expose stale files from an earlier output.
4. Add empty but executable contracts for `authority` ledger entries and structured trace events. Establish a test harness that can run without an NTSD corpus selected.
5. Define error/diagnostic envelopes early so parse failure, missing asset, unsupported rule, and unsafe-save conditions reach both tests and browser UI without auto-repair.

Files primarily introduced: zero-dependency root tooling/configuration and lockfile, `scripts/build.mjs`, `src/server`, `src/client`, `src/authority`, `src/trace`, and baseline tests/fixtures. Any existing Vite/Vitest/Playwright configuration remains untouched and optional.

Acceptance evidence:

- `npm ci --offline`
- `npm run dev:server` starts a loopback-only server; a test verifies its address is `127.0.0.1`.
- `npm run test` (implemented with `node --test` and `node:assert`) executes the empty authority-ledger/trace baseline and native browser-asset smoke coverage.
- `npm run build` proves `scripts/build.mjs` uses `stripTypeScriptTypes`, emits JavaScript, creates a build ID/manifest, and the production server serves only that manifest's exact allowlist even after a no-clean rebuild.
- Before/after preservation-manifest test proves every pre-existing `Tools/DatSkillFlowWeb` file has the same relative path, size, and SHA-256. The test creates an untracked fixture before the baseline scan to prove that Git tracking status is irrelevant.

## Gate 1 — lossless syntax/model pipeline and safe save

1. Implement a token/CST representation that retains original bytes/trivia wherever possible: line endings, encoding/BOM information, comments, whitespace, token ordering, duplicate keys, unknown sections/fields, and malformed-but-preserved text. Keep structured frame access in `model` as a non-destructive view over syntax nodes.
2. Load selected `data.txt`, DAT files, and BMP fixtures through `project`/`assets`, reporting missing or unreadable resources explicitly. External corpus discovery through the optional, read-only `NTSD_DAT_CORPUS_ROOT` environment variable is optional; synthetic fixtures cover the test baseline.
3. Model each edit as a named atomic command. The serializer replaces only the affected syntax regions and emits exact diagnostics/trace records explaining changed ranges. Unchanged inputs must reproduce byte-for-byte whenever preservation is possible.
4. Canonicalize the selected corpus root with `realpath` and treat it as the filesystem safety boundary. Canonicalize the candidate target/parent and require `path.relative(canonicalRoot, canonicalTarget)` to be non-empty/non-absolute and not begin with `..`; reject traversal, absolute input, cross-drive resolution, and every symlink escape. Apply this validation before load, Save As, backup, temp-file creation, and overwrite.
5. Make **Save As** the ordinary save route. Serialize all writes for each canonical target through a per-canonical-target lock. After canonical-root validation and while holding that lock, create a new destination directly with `fs.open(finalPath, 'wx')`, write the complete content, call `FileHandle.sync()`, and close it. Do not use a check-then-rename sequence or a temporary-file-plus-rename path for a new target. If `open(..., 'wx')` reports `EEXIST` (or the destination is otherwise known to exist), reject the new-target save and transition to the explicit overwrite protocol: confirmation plus an immediate external-change check.
6. Implement overwrite with a tested Windows `ReplaceFileW` adapter and this recoverable sequence:

   1. Under the target lock, re-stat/re-hash the existing canonical target immediately before commit and reject a conflict if it differs from the loaded version.
   2. Select a unique, nonexistent sibling backup path. Do **not** pre-create it, copy the old target to it, or otherwise pre-copy a backup: the single `ReplaceFileW` call creates the backup while replacing the target.
   3. Create a same-directory unique temporary file with `wx`, write the complete content, call `FileHandle.sync()`, and close it.
   4. Call the adapter's `ReplaceFileW(target, closedTemp, uniqueNonexistentBackup, ...)` path; it is the only overwrite commit operation. Do not call `unlink`, pre-delete the destination, or use Node `rename` over an existing target.

   Add `scripts/windows-replace-file.ps1`, which uses `Add-Type`/PInvoke to call `kernel32!ReplaceFileW`, and `src/server/windows-replace-adapter.ts`, which invokes that script through `child_process.execFile` or `spawn` using an argument array containing `-NoProfile`, `-NonInteractive`, and `-File`. Never invoke PowerShell through a shell or string concatenation. The adapter maps Win32 errors into save diagnostics. In particular, for official `ReplaceFileW` errors 1175, 1176, and 1177, diagnostics and tests must separately record and assert the actual post-call existence/state of target, temporary replacement, and backup. Do not make the vague or false claim that every failure preserves both the old target and the backup; recovery guidance must use the observed three-path state. The Windows adapter and PowerShell helper must have Windows integration tests, including failure/lock injection where supported, and the distribution/packaging plan must include the `.ps1` helper as a runtime asset.

Files primarily introduced: `src/syntax`, `src/model`, `src/project`, `src/assets`, server save routes, and fixture-heavy unit/integration tests.

Acceptance evidence:

- `npm run test -- syntax model` proves unchanged synthetic fixtures round-trip byte-for-byte and intentional edits preserve unrelated trivia/tokens.
- `npm run test -- save` proves direct Save As creation with `fs.open(finalPath, 'wx') -> write -> FileHandle.sync() -> close`, `EEXIST` transition to explicit overwrite, existing-target overwrite gating, per-canonical-target serialization, immediate pre-commit re-hash conflict rejection, selection of a unique nonexistent backup path without pre-copying it, `wx` temp creation, sync/close-before-`ReplaceFileW` ordering, and no-deletion behavior.
- Filesystem-boundary tests reject `..` traversal, absolute paths, cross-drive paths, and symlink escapes for load, Save As, temporary, backup, and overwrite paths; normal in-root paths pass.
- Windows integration tests prove the PowerShell `Add-Type`/PInvoke helper and `ReplaceFileW` adapter are used for overwrite (never Node rename), verify safe PowerShell argument-array invocation, and verify the helper is included by production distribution/packaging. For each official error 1175, 1176, and 1177, tests capture and assert the actual, separate post-call states of target, temporary replacement, and backup; no universal preservation assertion substitutes for those observations.

## Gate 2 — headless deterministic preview and timeline

1. Implement a pure `sim` core with `FRAME_MS = 33` timing (nominal 30 fps). It owns stable IDs, canonical state, frame scheduler, snapshots, scripted inputs, replay, and deterministic trace generation; it must import neither browser timing/canvas nor Node filesystem APIs. The deterministic timing contract is integer milliseconds: 300 ticks must equal 9900 ms.
2. Add authority-ledger records for each implemented `wait`, `next`, and state-transition rule, citing concrete file/function/region in `J:\QQFile\NTSD2.4\ntsd_cpp`. A rule without a citation stays unsupported/provisional in both trace and UI. Create separate authority-ledger entries and separate focused tests for the following frame-transition contracts: `next=1000` enters frame 1000, executes entity collision exactly once, then ordinary late cleanup frees the entity; `next=1280` enters frame 1280, executes entity collision exactly once, then takes the 12xx frame-group branch, sets `hit_stop = -180`, resets the frame to 0, and keeps the entity active. Neither may resolve directly to frame 0. Document these alongside the `next=999` transition context where applicable.
3. Implement browser playback controls—play, pause, one tick, seek, loop range, and frame/tick display—as command adapters over the headless core. Rendering can interpolate between snapshots but may never advance or modify canonical state.
4. Emit a per-tick trace containing input, frame transition, dynamic-slot lifecycle (even when no slots are yet allocated), rule identifiers, and snapshot digest.

Files primarily introduced: `src/sim`, expanded `src/authority`/`src/trace`, timeline pieces in `src/client`, and replay tests.

Acceptance evidence:

- `npm run test -- sim` replays identical scripted inputs twice and asserts byte-identical snapshots/deterministic trace output.
- Deterministic timing tests assert `FRAME_MS = 33`, nominal 30 fps, and exactly 9900 ms after 300 ticks.
- Timeline tests prove play/seek/loop outcomes equal the same number of `step` operations.
- A rendering-rate test drives presentation at different rates while canonical final snapshots remain identical.
- Transition tests separately prove `next=1000` enters frame 1000, runs one collision pass, and is freed by ordinary late cleanup; and `next=1280` enters frame 1280, runs one collision pass, then takes the 12xx branch to set `hit_stop = -180`, reset frame 0, and remain active.

## Gate 3 — authority-backed combat-object mechanics

1. Extend the model and preview renderer for all ten hit fields: `hit_a`, `hit_d`, `hit_j`, `hit_Fj`, `hit_Fa`, `hit_Da`, `hit_Ua`, `hit_ja`, `hit_Dj`, and `hit_Uj`.
2. Add dynamic collision/hit slots with explicit stable IDs, allocation/release events, collision candidate/result traces, and tests for slot lifecycle.
3. Implement cross-DAT `opoint` spawn selection and `wpoint` held-object synchronization only for authority-ledgered C++ rules. A requested but not yet verified variant must report unsupported/provisional status instead of being presented as parity.
4. Ensure trace records capture hits, selected DAT/frame, object slot allocation/release, opoint effects, and held-link synchronization.

Files primarily extended: `src/sim`, `src/model`, `src/client`, `src/authority`, `src/trace`, and deterministic scenario fixtures/tests.

Acceptance evidence:

- `npm run test -- combat` validates every hit field, collision slot lifecycle, hit outcome, `opoint` DAT/frame selection, `wpoint` synchronization, and complete tick traces.
- Every scenario test names the ledger rule(s) it verifies.

## Gate 4 — spatial mechanics and completed editor workflow

1. Add authority-ledgered XYZ movement, jumping, Z sorting, clone/duplicate behavior, and camera behavior. Keep camera/presentation data separate from the canonical rule state unless a C++ rule requires otherwise.
2. Complete frame/timeline editing UX with named undo/redo command stacks. Restore both document state and associated selection/timeline context on undo and redo.
3. Surface parse errors, missing BMP/data assets, authority gaps, save conflicts, backup locations, and trace diagnostics in a clear browser diagnostics panel. Never silently repair data.
4. Mark the final supported surface explicitly from ledger status: authoritative implementations, provisional UX-only behaviors, and unsupported rules must remain distinguishable.

Files primarily extended: `src/sim`, `src/client`, `src/model`, `src/authority`, `src/trace`, and E2E/authority replay fixtures.

Acceptance evidence:

- `npm run test` completes all unit and integration suites.
- Required `node:test` browser-workflow coverage validates editing, diagnostics, playback, and undo/redo context restoration through native loopback requests and deterministic client contracts. Playwright system E2E may supplement this when already available, but is never required.
- Spatial replay tests prove deterministic results and cite their authority ledger records.
- `npm run build` succeeds, followed by a local production-server smoke test bound to `127.0.0.1`.

## Final verification matrix

Run these from `Tools/DatSkillFlowWeb` once the corresponding gate is implemented:

```powershell
node --version                 # require >=24.11.1 and <25
npm ci --offline
npm run lint
npm run typecheck
npm run test
npm run build
npm run start:server
```

Add dedicated tests where the generic scripts cannot prove the contract: server-address assertion, no-file-removal audit, zero-dependency lockfile and offline install, build-ID/manifest allowlist enforcement after no-clean builds, byte-level round trip, save conflict/backup/temp ordering, direct Save As `fs.open(..., 'wx')` no-rename behavior, closed-temp-before-`ReplaceFileW` ordering, deterministic replay/trace equality, `FRAME_MS = 33` timing (300 ticks = 9900 ms), separate `next=1000` and `next=1280` transition contracts, authority-ledger citation coverage, and UI labels for unsupported/provisional behavior. Treat all real corpus tests as opt-in additions; the synthetic fixtures are the required reproducible baseline. Optional system-provided Playwright E2E may run separately but is not an acceptance requirement.
