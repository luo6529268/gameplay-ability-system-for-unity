# NTSD28Parity

## B0 input / RNG / slot raw contract

`b0-domain-contract` emits the independent completed-tick raw contract used before
the full v3 trace can be populated. It records applied seven-action held input,
three source-native RNG streams (`authorityCrt`, `authoritySynchronized`, and
`unityDeterministic`), slot occupants/allocation epochs, and snapshot-derived
birth/death/reuse deltas. The producer availability matrix is strict: Unity's
single deterministic stream is not presented as either authority stream, and
currently unavailable per-call logs remain JSON `null`, never synthetic zeroes.

```powershell
dotnet run --project Tools/NTSD28Parity -- b0-domain-contract --output Temp/NTSD28Parity/b0-domain-contract.json
dotnet run --project Tools/NTSD28Parity -- self-test-b0-domain-raw --output Temp/NTSD28Parity/b0-domain-self-test.json
dotnet run --project Tools/NTSD28Parity -- validate-b0-domain-raw --capture <capture.jsonl> --output <report.json>
dotnet run --project Tools/NTSD28Parity -- compare-b0-domain-raw --authority <authority.jsonl> --unity <unity.jsonl> --output <report.json>
dotnet run --project Tools/NTSD28Parity -- self-test-b0-domain-comparator --output Temp/NTSD28Parity/b0-domain-comparator-self-test.json
```

## B2 exact input / native dual-RNG raw

The B2 stream is separate from the B0 entity/domain schemas. It compares the
completed-tick input phase, both native RNG scalar states, and each entity's
exact input proxy, history, combo bank, remap/bound controls, run accumulator,
and last routed action. Per-call RNG logs remain explicitly unavailable in this
version, so scalar equality is diagnostic evidence rather than a formal runtime
certificate.

```powershell
dotnet run --project Tools/NTSD28Parity -- validate-b2-input-rng-raw --capture <capture.jsonl> --output <report.json>
dotnet run --project Tools/NTSD28Parity -- compare-b2-input-rng-raw --authority <authority.jsonl> --unity <unity.jsonl> --output <report.json>
dotnet run --project Tools/NTSD28Parity -- self-test-b2-input-rng-raw --output Temp/NTSD28Parity/b2-input-rng-self-test.json
```

The v3 B2 raw contract requires completed-tick call arrays for both native
streams. Each array is checked against `tickCallCount`, monotonically increasing
call ordinals, and the tick's final scalar state. Initialization is excluded;
Unity synchronized-cursor calls are published only after an accepted canonical
AI commit. Captures remain diagnostic and are never formal-EXE certificates.

The domain comparator validates both inputs first. It compares applied input,
slot occupants/allocation epochs, and lifecycle deltas strictly. Only the slot
capacity number is excluded under the user-approved Unity capacity exception.
Different source-native RNG stream sets are reported as
`STREAM_TOPOLOGY_DIFFERENCE`; the tool preserves each stream's call-count
vector and never invents a Unity-to-authority stream mapping.

`NTSD28Parity` is the independent B0 trace-contract consumer for the current
NTSD 2.8-Logan authority. It does not reference the superseded NTSD 2.4 C#
project, Unity assemblies, or the legacy `Tools/NTSDParity` schema.
It targets the repository host's installed .NET 10 SDK and has no external
package dependencies.

Current schema package: `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001`; the v1 envelope
package `NTSD28-B0-TRACE-CONTRACT-001` remains its historical foundation.

## Scope

The tool freezes and validates:

- the fixed authority executable identity;
- the completed-tick JSONL envelope;
- separate CRT and synchronized RNG streams;
- input, world, entities, relations, rests, events, and presentation domains;
- per-domain and overall SHA-256 commitments;
- the approved Unity exceptions and user-excluded features;
- actual object-DAT content identity, decoder semantics and joint-schema compatibility under D-023;
- streaming first-difference order.
- a v3 exact-property core entity schema with 50 typed fields and explicit
  authority/Unity binding maturity (`VERIFIED`, `CANDIDATE`, or `MISSING`).

This package does not include an NTSD 2.8 C++ exporter or a Unity exporter.
Every result therefore has `certificateEligible: false`, including synthetic
equal traces.

## Commands

```powershell
dotnet build Tools/NTSD28Parity/NTSD28Parity.csproj -c Release

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  contract --output Temp/NTSD28Parity/contract.json

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  validate --trace <trace.jsonl> --output Temp/NTSD28Parity/validation.json

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  compare-raw-entities --authority <authority.raw.jsonl> `
  --unity <unity.raw.jsonl> --output Temp/NTSD28Parity/raw-difference.json

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  self-test-raw-entities --output Temp/NTSD28Parity/raw-self-test.json

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  compare --authority <authority.jsonl> --unity <unity.jsonl> `
  --output Temp/NTSD28Parity/first-difference.json

dotnet run --project Tools/NTSD28Parity/NTSD28Parity.csproj -c Release -- `
  self-test --output Temp/NTSD28Parity/self-test.json
```

`slotCapacity` is recorded in each producer header but is not an equality
condition because the user selected the Unity capacity model. Slot identity,
allocation epoch, event order, and entity state remain strict domains. Each
producer must still keep every emitted entity slot inside its own declared
capacity.

Approved exceptions are recorded but are not silently normalized by the v3
comparator. Future exporter/projection packages must either select scenarios
that do not activate a gameplay exception or introduce an explicitly
versioned projection. This prevents an exception from hiding unrelated state
or RNG drift.

Content headers bind the actual raw definition digest, decoder tag, semantic SHA-256,
little-endian 64-bit projection and the 13/21/24/2/2 joint schema set.
A content/profile/schema mismatch fails before entity comparison and produces a
nonzero CLI exit; the old `content-strategy-pending` success state is retired.
The identity scope is catalog object DAT definitions, not every visual/audio/stage
asset. Source-native and Unity assembly hashes remain separate provenance.
Unity legacy captures identify their actual DAT files with a separate profile.
All captures remain diagnostics, never parity certificates.

The entity binding status is not a comparison waiver. `CANDIDATE` and
`MISSING` fields remain strict trace fields so exporters and later runtime
packages must close the binding or expose an honest first difference. Input,
relations, rests, events, and presentation have separate domain contracts and
are not duplicated inside the entity object.
