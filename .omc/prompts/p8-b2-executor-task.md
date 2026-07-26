# P8-B2 implementation task

You own only the new Editor diagnostic/export tool and narrowly necessary tests. Implement P8-B2 from Assets/NTSD/Docs/central-battle-render-system-plan.md.

Requirements:
- Add an Editor-only tool under Assets/NTSD/Scripts/Animation/Rendering/Editor/ that lets a developer query the current SimulationWorld by runtime slot and BattleRenderCommandType, and export both the per-entity BattleCentralEntityDiagnostic and current BattleRenderingDiagnosticReport as deterministic JSON.
- Reuse BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot and CaptureDiagnosticReport; do not duplicate production render traversal.
- Do not make runtime state, Transform, SpriteRenderer, or GameObject the truth.
- The tool should be useful from menu/Editor UI or a request file, and must fail clearly when no current world exists or the slot is invalid.
- Avoid allocations in the normal tick/render hot path; allocations are allowed only when explicitly invoked.
- Add focused self-checks only if Editor-independent; otherwise add EditMode-safe deterministic serialization checks.
- Do not touch T8, Android validation, DAT, battle logic, scaling, prefabs, or unrelated dirty files.
- You are not alone in the codebase. Preserve all existing edits and do not revert others.

Run dotnet build and git diff --check on owned changes. Report files, behavior, and remaining Unity runtime verification needed.
