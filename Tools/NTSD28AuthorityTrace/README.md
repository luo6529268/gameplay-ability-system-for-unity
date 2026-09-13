# NTSD28AuthorityTrace

The v2 source wrapper hashes the actual catalog and referenced DAT bytes using
the same frozen BinaryWriter/semantic-byte contract as Unity. It rechecks inputs
after initialization and after simulation. The content object records raw and
semantic identity, scope/profile and joint schemas; it does not accept a caller
provided content digest. Windows system BCrypt supplies SHA-256.
`trace_binding_witness.cpp` is a separate unit witness for the actual output
helper (-1/0/37 versus owner19), not a gameplay capture or formal EXE certificate.


This workspace-owned diagnostic runner compiles the unchanged NTSD 2.8-Logan
playable/core sources and emits the 50-field raw entity capture consumed by
`NTSD28Parity`. With `--domain-output`, the same run also emits applied input,
the two source-native authority RNG streams, physical slot/allocation-epoch
snapshots, and snapshot-derived lifecycle deltas. It never builds into or
writes under the authority root.

With `--b2-input-rng-output`, the runner emits the separate versioned B2
completed-tick stream for exact entity input proxy/history/combo state,
input-update phase, native CRT/synchronized scalar state, and direct native
RNG calls. The build uses GNU linker wrapping for the two original authority
symbols; it does not patch or write the authority source. The per-call window
starts after session initialization, so table construction and the initial BGM
draw remain scalar-only. v3 additionally accepts Unity AI cursor calls only
after the canonical commit succeeds; rejected/speculative cursors are excluded.
Formal-EXE observation remains a separate evidence boundary.

`Scenarios/input-standing-attack-rng.json` exercises one synchronized direct
call at completed tick 2: call-site `0x82`, upper bound `2`.

Historical pre-D-023 content comparison: `Scenarios/input-ai-one-entity-rng.json` enables one low-slot native AI through
`nativeComputerState1b8=3`. The first-tick readiness and host-pending projection
fixes now make Unity and authority consume the same 6/7/8 synchronized calls
across ticks 1/2/3, including call sites, bounds, results, and after-state.
Tick 1 and tick 2 exact input now match after the post-route canonical-store
roundtrip. The next comparison difference is tick 3 slot 1 `currentMask`
(`18` authority versus `3` Unity), downstream of the already-observed tick-2
content action split (`650` authority versus `9` under current Unity Direction
B content). It is therefore tracked under B11 rather than treated as another
B2 input implementation difference. Formal-EXE observation remains pending.

The output is `SOURCE_MODEL_DIAGNOSTIC_ONLY` and is not a formal-executable
runtime trace or parity certificate. Its header deliberately separates the
formal EXE SHA, authority source manifest SHA, capture-runner source SHA, and
the exact capture binary SHA, and the legacy ScenarioLoader reference SHA.

```powershell
pwsh -NoProfile -File Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1
```

Read the generated `Temp/NTSD28AuthorityTrace/build/build-manifest.json`, then
run the executable with the manifest hashes, the checked-in scenario, and the
authority `resources/runtime` path as both resource roots. Validate the result
with:

```powershell
dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  validate-authority-capture --capture <raw.jsonl> --output <report.json>

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  validate-b0-domain-raw --capture <domain.raw.jsonl> --output <report.json>

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  validate-b2-input-rng-raw --capture <b2.raw.jsonl> --output <report.json>
```
