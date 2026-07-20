---
provider: "codex"
agent_role: "architect"
model: "gpt-5.6-sol"
prompt_id: "2ee28c78"
timestamp: "2026-07-17T03:40:09.877Z"
---

I’m activating the repository’s architect-review workflow. I’ll split the evidence gathering between the Unity/tooling implementation and the formal C# authority, then have an independent reviewer synthesize and verify a severity-ordered certificate assessment; the final report will be written under `.omc/research/` because no explicit output path was supplied.
The first structural issue is already clear from the supplied skeleton: the authority side emits a complete JSONL trace, while the Unity side currently exposes only an in-memory per-tick snapshot and compact `ToJson()` path. I’m tracing whether a production Unity runner/header writer exists elsewhere before treating that as a blocking gap.
The review is treating “end-of-tick equality” and “full certificate” as different claims. Even a perfect final-state hash can miss transient pass-order errors, same-tick candidate consumption, and queued lifecycle differences, so the certificate needs explicit pass-boundary evidence in addition to the 30 Hz terminal snapshot.
Evidence gathering is complete. The highest-risk false positives are concrete: header-only traces can compare equal, tick bodies are not rehashed, checksum capture mutates roster bindings, gameplay-significant DAT centers are excluded from the battle manifest, and requested fighters can silently fail to bootstrap into an otherwise “valid” scenario.
The report is being written as a scoped certificate review, not a generic bug list. It will separate deterministic simulation obligations from Unity-native presentation proof, and it will preserve T8’s deferral by defining a non-stage certificate profile rather than weakening the stage-independent logic gates.
The delegated writer hit a sandbox path-creation defect even though `.omc/research` already exists. I’m creating only the requested report placeholder in the permitted `.omc/**` area so the writer can populate it; no source files are being changed.
The repository’s managed Windows sandbox is denying all writes, including `apply_patch` to the repo root, `Temp/NTSDParity`, `.omc/research`, and the explicitly writable `I:\tmp`. I’ll still complete the independent review and fresh architect verification, but unless the ACL clears, the report must be delivered in the response with the intended output path recorded as a write blocker.
# NTSD Full Battle Parity Certificate Architecture Review

**Verdict: NOT READY.** The current implementation is a useful differential-tracing skeleton, but it can produce false-positive equality and cannot yet generate a reproducible Unity-side certificate.

The intended output file was `.omc/research/ntsd-full-battle-parity-certificate-architecture-review.md`. The managed Windows sandbox denied every `apply_patch` write, including the repo root, `.omc/research`, `Temp/NTSDParity`, and `I:\tmp`, so no file was created. The complete review follows.

## Critical Findings

1. **The comparator can certify empty or fabricated traces.** Simultaneous EOF after the headers returns `equal`, without verifying the scenario’s tick count ([TraceCompareCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/TraceCompareCommand.cs:95)). Tick indexes only need to match each other, not start at 1 or remain consecutive. Most importantly, the comparator trusts supplied hash strings and never recomputes domain or overall hashes from the serialized bodies ([TraceCompareCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/TraceCompareCommand.cs:116)). Copied authority hashes with unrelated Unity payloads can pass.

2. **There is no Unity trace runner.** The CLI only implements `data-audit`, `trace-authority`, and `compare` ([Program.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/Program.cs:15)). The README assumes a `unity-trace.jsonl` without explaining how it is produced. Unity retains only the most recent in-memory snapshot ([SimulationTickDriver.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs:222)); it has no shared scenario parser, deterministic bootstrap barrier, JSONL/header writer, or build-artifact provenance.

3. **The capture boundaries differ.** Unity captures before `AfterSimTick` ([SimulationTickDriver.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs:204)), while authority tracing occurs after `StepOneTick` returns ([AuthorityTraceCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/AuthorityTraceCommand.cs:90)). The hooks may currently be inert, but the certificate must align the boundaries or enforce and test that they are pure.

4. **Snapshot capture mutates the state it measures.** World projection calls `TryResolveRosterEntity` ([BattleParitySnapshot.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:549)), which writes `RuntimeSlotIndex` and `StableId` into roster state ([SimulationWorld.FrameInput.partial.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs:85)). Enabling checksums can therefore alter later simulation. The observer must be pure, with tracing-on versus tracing-off equivalence tests.

5. **Unity does not project the full entity runtime.** Authority serializes the complete formal `entity.Runtime` for every slot ([AuthorityTraceCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/AuthorityTraceCommand.cs:330)). Unity manually maps a subset and emits constants for candidate arrays, jump flags, pickup state and `abortRemainingHitPairs` ([BattleParitySnapshot.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:397)). Omitted live fields include stable and transform identity, owner relationships, pass suppression, `NextFrame`, candidate distances, dormant/pending-destroy state, weapon-flight state, `HPLost`, and MP/PP bounds ([NTSDEntityRuntime.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs:103)). A versioned field ledger must map every formal field to live Unity state, a proved derivation, or a formally justified exclusion.

6. **The battle DAT manifest excludes combat inputs.** `FrameData.CenterX/CenterY` are excluded from `battleLogicSha256` ([DataAuditCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/DataAuditCommand.cs:19)), although the formal authority uses them for collision geometry ([CollisionCollect.cs](J:/QQFile/NTSD2.4/ntsd_release_C#/src/BattleCore/Interaction/CollisionCollect.cs:485)), cpoint placement, hit alignment, opoint placement and held attachment. Sound fields are also excluded even though cue emission is deterministic behavior.

7. **The data audit is not a certification gate.** Missing manifest files are skipped ([DataAuditCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/DataAuditCommand.cs:316)), and the command exits successfully whenever parser errors are zero, even with missing OIDs, field differences or unequal manifests ([DataAuditCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/DataAuditCommand.cs:136)). Required data differences, missing files and unresolved duplicate OIDs must make certification fail.

## High Findings

8. **Fixed 400-slot identity and lifecycle are not proven.** Producing a 400-element view does not detect duplicate occupants, stale bucket membership, allocator disagreement, pending unregisters, queued spawns, pending destroys or same-tick reuse. Unity’s stable generation identity is also omitted. Certification needs an occupancy bitmap, exactly-one-occupant invariants, entity-only count, queue state and ordered allocate/publish/free/reuse events.

9. **Only tick-end state is compared.** Candidate arrays and abort state may be cleared before capture, while spawn-then-destroy or compensating pass-order errors can leave identical final state. Required checkpoints include post-input, post-frame-advance, post-cpoint/held/link, post-collect, post-character-consume, post-object-consume, post-spawn-publication, late update and lifecycle flush. Candidate records need attacker slot, victim slot, itr index, sequence and abort result.

10. **Input semantics are ambiguous.** `FrameInputSet` permits sparse and duplicate records ([FrameInputSet.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs:33)). Unity describes packets as complete held-state snapshots but leaves unlisted players unchanged ([SimulationWorld.FrameInput.partial.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs:19)). Validation does not reject duplicate players, unknown mask bits, inactive/AI input or missing active-human records. Use one sorted held mask per active human per tick and disable local InputSystem callbacks during certificate runs.

11. **Bootstrap validity is not enforced.** Authority scenario validation checks only basic tick and slot ranges ([AuthorityTraceCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/AuthorityTraceCommand.cs:120)); formal bootstrap may skip an unavailable OID or failed spawn and continue. Unity has no authority-equivalent certificate bootstrap. A tick-zero gate must assert every requested active slot produced exactly one matching entity with the expected OID, team, AI state, runtime slot and initial state.

12. **RNG/header provenance is incomplete.** Per-tick RNG state and call count are useful, but the comparator ignores `rngAfterBootstrap`, `loadedChars`, `maxRuntimeSlots`, button mapping and detail mode emitted in the header ([AuthorityTraceCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/AuthorityTraceCommand.cs:194)). Record and compare authority revision, Unity revision/build hash, Unity version, tool and canonicalizer version, scenario hash and tick-zero RNG state.

13. **World coverage uses placeholders.** Unity hardcodes several flow, reserve and result values instead of reading live state ([BattleParitySnapshot.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:571)). Flow toggles, bounds, roster identity, results and later-pass gates require live mappings. `LoadedOidOrder` and `RuntimeStageCount` are excluded without a certificate-level justification; formal C# uses loaded OID order in weapon-selection paths and stage count in stage cycling.

14. **ARest/VRest, statistics and relationships are only tail evidence.** Their current hashes are useful, but they do not prove intermediate writes, directionality, overwrite order or lifecycle cleanup. Full checkpoints must cover all 400 ARest values, the 400x400 attacker/victim VRest mapping, kill/damage arrays, and every owner/link/target/holder/catcher/picker reference before and after consumption and slot reuse.

15. **Event evidence is incomplete.** End-of-tick pending sounds are hashed, but Unity hashes raw cue text while authority normalizes logical cue paths. Spawn, destroy, hit, spark, pool activation and publication timing are not journaled. Cue emission belongs in deterministic trace evidence; actual playback belongs in Play Mode evidence.

16. **Scenario coverage is far below a full certificate.** The only sample runs six ticks with two humans and a left press/hold/release ([scenario.sample.json](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/scenario.sample.json:9)). There is no coverage ledger for AI, itr kinds, catch/held/throw, opoints, transforms, death/respawn, bounds, RNG branches, candidate ties/cap, slot exhaustion/reuse or long-running timers.

## Logic vs Presentation

| Deterministic certificate | Separate Play Mode evidence |
|---|---|
| Full 400-slot runtime, world flow, input state/history, RNG, rests, stats and relationships | Actual `GameObject` and renderer existence/activation |
| Candidate sequences, pass checkpoints and lifecycle queues | Sprite/pic selection, imported texture slicing and pivots |
| Logical position, facing, frame, attachment pose, visibility/sort intent | Post-`LateUpdate` transform, parenting, sorting layer/order and visibility |
| Ordered spawn/destroy/hit/spark/sound-emission events | Shadow/spark pixels and audible playback timing |
| Behavior-relevant DAT fields and logical arena bounds | Asset bytes, importer settings and presentation quality |

T8’s deferred default `stage.dat` blocks only a stage-campaign certificate. A non-stage certificate must explicitly record `stageFixture.loaded=false`, while still comparing background width/Z bounds, background index count, random-stage inputs and all non-stage combat behavior.

## Minimal Sufficient Gate

1. Fresh Unity compile and full self-check, with immutable authority/Unity/tool/build identifiers.
2. Strict resolved-DAT and logical-arena audit: zero missing, different or unresolved entries.
3. Automated authority and Unity runners consuming the same validated dense scenario in Manual 30 Hz mode.
4. Tick-zero bootstrap equality followed by exactly ticks `1..N`.
5. Pure, versioned schema covering all 400 slots, complete runtime/world state, RNG, rests, stats, links, candidates, queues, events and pass checkpoints.
6. Comparator validates exact schema/header/scope/tick sequence and independently recomputes every domain and overall hash.
7. Coverage ledger maps every in-scope formal C# pass and branch to differential scenarios, with seed sweeps, long soaks and 400-slot churn.
8. Separate post-`LateUpdate` Play Mode matrix proves attachments, sorting, visibility, sprite/pic, shadows, sparks and sound playback.
9. Stage and non-stage certificates remain distinct.

The alignment document is regression evidence, not a parity certificate; it already records that complete tick-by-tick comparison and RISK-4 remain open ([csharp-vs-unity-battle-alignment.md](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md:573)).

Fresh independent architect verification returned **PASS** after incorporating the capture-boundary and non-stage bounds corrections. No source files were modified.