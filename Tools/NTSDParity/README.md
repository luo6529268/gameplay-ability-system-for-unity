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

## Authority400 witness runner

`authority400-witness-manifest.v1.json` freezes the Slice 0 witness ledger.
W01 (dense input/RNG) and W02 (frame wait/immediate-frame reset) remain on the
unchanged v3 trace schema. W03 (live scan plus free/reuse lifecycle), W04
(allocator start bands), and W07 (positive-link validation) use the additive
v4 structural event domain. W05, W06, and W08 remain
`requires-v4-structural-events/source-callchain-plus-focused-test`.

W06 is backed by `W06CollisionHitResolveWitnessEditorTests`. The focused tests
execute Unity's real `NTSDBattleTickSystem.RunInteractionPhase` and production
collision candidate/character-hit consume path. They lock the authority order
`CollectCandidates -> ResolveCharacterHits -> natural random weapon drop ->
ResolveObjectHits` from `GameTick.cs`, including the RNG-call boundary around
the random-drop gate. They also prove that the frozen candidate carrier is
consumed in ascending runtime-slot order without re-filtering later geometry or
team mutations, matching `HitResolve.ResolveCandidates`. This closes the local
focused-test requirement, but W06 remains non-runnable until the corresponding
v4 cross-runtime structural events exist; it is not a parity certificate.

Every v4 structural record has the canonical fields `tick`, `pass`, `action`,
`cursorSlot`, `actorSlot`, `slot`, `searchStart`, `searchEndExclusive`,
`before`, `after`, `lifecycleEpoch`, and `sourceKind`. `lifecycleEpoch` is
derived independently by each exporter from allocations observed for the same
slot. Unity's internal `RuntimeSlotTable.Generation` is intentionally never
serialized or compared across runtimes. The comparer rejects slot fields above
399 and search ends above 400.

W07 `link-validation` records additionally contain canonical forward fields
`beforeLinkState`, `beforeTargetSlot`, `beforeHeldWeaponSlot`,
`afterLinkState`, `afterTargetSlot`, and `afterHeldWeaponSlot`; observation
fields `targetActive` and `observedHolderSlot`; `outcome` and `reason`; and
target reverse fields `targetBeforeHolderSlot`, `targetBeforeLinkState`,
`targetAfterHolderSlot`, and `targetAfterLinkState`. These are runtime slot
indices only; Unity stable-id terminology is not serialized into the canonical
event contract.

W03/W04 are diagnostic source-callchain witnesses, not production parity
certificates. The Unity W03 fixture executes the real `LateEntityUpdateAll`
live pass and real registry mutation boundaries. The authority W03 exporter
uses the real fixed `Objects` table plus `SpawnAt`/`FreeEntity`, but its cursor
loop is an exporter fixture matching `GameTick.cs`; it does not instrument an
actual `GameTick.Run` pass. W04 similarly exercises real registry operations
while its 0/20/50 start searches are explicit exporter fixtures anchored to
`SimulationWorld.Registry.cs`, `GameTick.cs`, and `FrameTick.cs`. Consequently
their manifest coverage is `diagnostic-source-callchain`, and runner summaries
remain `certificateEligible: false`.

W07 is `diagnostic-source-callchain/partial`, also never certificate eligible.
The authority method `ValidatePositiveLinks` is private, so its fixture invokes
the real accessible `GameTick.Run` path and observes state through the adjacent
`beforeCollectCandidates` and `afterCollectCandidates` hooks. The Unity fixture
invokes the real `ValidateHeldLinksAll`. It proves the focused reciprocal-link
keep and holder-mismatch clear cases, not every positive-link branch or the
entire production tick. Structural sinks are null by default; diagnostic maps,
context writes, source classification, and event construction are guarded and
do not execute on the normal production path.

The focused comparer self-test also requires missing structural fields,
changed field values, event deletion, and event reordering to fail. Cross-tick
`lifecycleEpoch` monotonicity remains exporter-derived but is not yet enforced
as a whole-trace comparer invariant.

The checked-in scenarios use `${AUTHORITY_GAME_ROOT}` rather than a machine
specific `J:` path. The runner materializes that token only into its output
directory; it never changes the checked-in scenario, exporter, trace schema,
or an expected trace.

Validate the manifest and portable scenarios before execution. On Windows
PowerShell (available on this machine):

```powershell
powershell -ExecutionPolicy Bypass -File Tools/NTSDParity/Invoke-Authority400Witness.ps1 -ValidateOnly
```

PowerShell 7 users can equivalently run `pwsh Tools/NTSDParity/Invoke-Authority400Witness.ps1 -ValidateOnly`.

Run all currently executable v3/v4 witnesses (one Unity batch process at a time):

```powershell
$env:UNITY_EXE = 'C:\Program Files\Unity\Hub\Editor\2022.3.4f1c1\Editor\Unity.exe'
$env:NTSD_AUTHORITY_GAME_ROOT = 'J:\QQFile\NTSD2.4'
powershell -ExecutionPolicy Bypass -File Tools/NTSDParity/Invoke-Authority400Witness.ps1 -ExecutableOnly
```

Use `-WitnessId W01`, `-UnityExe`, `-ProjectPath`, `-AuthorityAssembly`,
`-AuthorityGameRoot`, and `-OutputRoot` for focused or CI runs. `-DryRun`
validates selection without starting processes. Normal artifacts are retained
under `.omc/validation/authority400-witness/<witness-id>/`, including the
resolved scenario, authority trace, Unity trace, comparison report, Unity log,
and an aggregate `summary.json`; this avoids Unity's exit-time `Temp` cleanup.
The runner refuses to launch while any `Unity.exe` process exists, preventing a
second Editor from contending for the project Library.

The runner intentionally uses the `authority-dat-diagnostic` fixture and
passes `--allow-diagnostic` to the comparer. Summaries are always marked
`certificateEligible: false` with evidence class `diagnostic-witness-only`;
`completed` only means that the selected diagnostic witness comparison ran
successfully, never that it produced a production parity certificate.
