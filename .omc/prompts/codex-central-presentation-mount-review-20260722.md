# Architect Review: Mountable Central Battle Presentation Component

Review the current uncommitted implementation of the mountable central presentation contract. This is a read-only architecture/code review. Do not edit files.

Scope:

- `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralPresentationMount.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralPresentationMountRegistry.cs`
- integrations in `LF2ObjectRenderer.cs`, `BattleCentralRenderSystem.cs`, and `SimulationWorld.Registry.partial.cs`
- focused checks in `BattleRuntimeSelfCheck.cs`

Required contract:

- The public MonoBehaviour is a mountable prefab declaration for EntityModel/EntitySprite or Shadow/CommonShadow.
- It must not draw, load resources, update per frame, store transform/pixel/frame/HP truth, or alter battle/presentation frame commands.
- Registration is idempotent through OnEnable/OnDisable; runtime binding uses generation-aware RuntimeEntityHandle.
- EntityModel owner is LF2ObjectRenderer on the same GameObject. Shadow owner is the sibling EntityModel LF2ObjectRenderer under the same direct entity root. No name-string dependency.
- Invalid role/purpose/owner/node configurations must remain unbound and be diagnosable. Duplicate owner+purpose must be diagnosed.
- All runtime-slot release/reset paths must clear the binding before release; failed release may restore the current valid handle.
- Ordinary characters that receive their slot after SetLogicObject must bind after SimulationWorld.Register.
- No prefab is modified in this batch. Legacy SpriteRenderer remains until the user mounts and confirms the component.

Fresh evidence already obtained by the primary agent:

- `dotnet build Assembly-CSharp.csproj --no-restore /m:1`: 0 errors, 42 existing warnings.
- Unity MCP refresh produced a DLL newer than all changed sources.
- `Temp/NTSD_BattleRuntimeSelfCheck.result`: PASS at 2026-07-22 11:00:57 local time.
- After clearing Console and rerunning, one existing intentional self-check LogError was emitted by `CheckProductionRuntimeRestStoreLifecycleContracts` while exercising mismatched rest-release rollback; there were no compile errors and the full result remained PASS.
- `git diff --check` passes.

Please report findings first, severity P0-P3 with exact file/line citations. Explicitly assess lifecycle correctness, generation safety, invalid configuration behavior, pending-unregister rollback, unwanted hot-path/per-frame overhead, and whether the self-check genuinely covers the contract. End with PASS only if there are no P0-P2 issues; otherwise FAIL and state required fixes.
