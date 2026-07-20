# NTSD C# / Unity battle parity tools

`NTSDParity` references the formal C# authority project at
`J:\QQFile\NTSD2.4\ntsd_release_C#`. It reads that project through its public
runtime and DAT APIs; it never modifies authority source files.

Build:

```powershell
dotnet build Tools/NTSDParity/NTSDParity.csproj
```

## Resolved DAT audit

Both sides are parsed by the authority `DatLoader`. The comparison includes
all parsed combat fields, sprite dimensions/ranges, sound cues, and the
resolved frame table. Duplicate frame ids follow the actual runtime rule:
the last parsed frame wins. Pure sprite asset paths are normalized across the
two repository layouts; sound cues stay in the comparison under their logical
basename (`snddata_1877.wav` and `1877.wav` are the same cue).
Per-OID duplicate frame-id lists remain in the report for diagnosis even
though only each side's last resolved definition participates in comparison.

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- data-audit `
  --output Temp/NTSDParity/data-audit-full.json
```

Use repeatable `--oid <id>` options for a focused audit. The report records
`missing-unity-file` separately from an index mismatch or parser error, and
contains stable authority and Unity SHA-256 normalized manifest hashes. Full,
battle-logic, and presentation manifests are reported separately.

Add `--require-equal` for a certificate gate. It exits nonzero when any OID is
missing or different, parsing fails, or the normalized manifests differ. A
normal audit keeps exit code zero when differences are successfully reported.

## Deterministic authority trace

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- trace-authority `
  --scenario Tools/NTSDParity/scenario.sample.json `
  --output Temp/NTSDParity/authority-trace.jsonl
```

The v3 header embeds only the normalized battle-logic DAT manifest hash. Frame
centers remain in that manifest because they affect collision and held/opoint
coordinates. Sound definitions stay in the presentation manifest while the
sounds actually emitted each tick are deterministic `events`, so sound timing
and cue identity are still compared without treating deployment paths as
battle state. Each tick has independent
hashes for input, RNG, world state, all fixed runtime slots, ARest, VRest,
stats, and events, plus an overall hash. Compact output writes only active or
non-default slots and carries a SHA-256 commitment for every fixed slot. Use
`--detail full` to open all 400 slot commitments and emit full rest matrices.

`stage.dat` is not loaded by default. A scenario may opt in with an explicit
`stageFixture` path; its payload hash is then written to the trace header.

Button masks are `Right=1`, `Left=2`, `Up=4`, `Down=8`, `Attack=16`,
`Jump=32`, and `Defend=64`.

## Streaming trace comparison

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- compare `
  --authority Temp/NTSDParity/authority-trace.jsonl `
  --unity Temp/NTSDParity/unity-trace.jsonl `
  --output Temp/NTSDParity/first-difference.json `
  --profile fixed-world-camera
```

The comparator reads one line from each trace at a time. It validates the full
header contract, requires contiguous ticks `1..scenario.ticks`, reconstructs
the canonical body for every domain, recomputes every domain hash and the
overall hash, then compares the two streams. It rejects truncated and extra
traces. `--detail full` on the compare command includes up to 512 field-level
differences from the first divergent line.

Only full/full comparison returns certificate-eligible `equal`. A comparison
involving compact data returns `equal-commitments`: all 400 slot commitments
match and every opened slot is verified, but omitted slot bodies are not a
field-level certificate.

`--profile fixed-world-camera` normalizes only `cameraX` and `cameraVel` in
the top-level world and `runtime.stage` domains after each producer's original
body hashes have been verified. Entity positions, render offsets, bounds, and
all other world fields remain strict. This profile records its name in the
report and does not restore character-driven camera behavior in Unity.
An eligible full/full result uses certificate class
`profiled-fixed-world-camera-v1`; only strict full/full production comparison
uses `strict-production-v1`.

Headers identify `production` versus `authority-dat-diagnostic` data. A
diagnostic comparison requires explicit `--allow-diagnostic`, reports
`equal-diagnostic` at best, and is never certificate eligible. Production
manifest mismatch remains a hard failure in every mode.
Use `--require-certificate` in CI so `equal-diagnostic` and compact
`equal-commitments` still exit nonzero.

Run malicious trace regression tests:

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- self-test `
  --output Temp/NTSDParity/trace-compare-self-test.json
```

The suite covers empty/header-only traces, skipped and missing ticks, stale or
forged hashes, changed bodies, and manifest mismatches.
