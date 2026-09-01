# BATTLE-RUNTIME-ORDERED-SHUTDOWN-001 Task Contract

## Objective

Implement the exact ordered shutdown transaction defined by `Assets/NTSD/Docs/battle-runtime-ordered-shutdown-contract.md`, without changing Running-state battle behavior.

## Authorized scope

- Lifecycle state and shutdown diagnostics.
- Tick/input/worker/spawn gates.
- Allocation unseal, presentation reset, pending OPoint discard/recycle.
- Renderer, logic World, pool and runtime boundary cleanup in the frozen order.
- Focused Editor tests, SelfCheck coverage and live Play enter/exit/re-enter validation.

## Forbidden expansion

- No battle-rule/pass/tick/checksum changes.
- No Scene, DAT, Prefab, URP, Input Actions, Server or C++ modifications.
- No whole-project Mono/Core split, asmdef migration or directory move.
- No destructive Git operation or cleanup of unrelated dirty files.

## Exit criteria

The runtime reaches `Stopped` only after all hard postconditions pass; two live Play teardown cycles produce no cleanup warning or residual runtime object; compile, focused tests, SelfCheck and governance validation are honestly recorded.
