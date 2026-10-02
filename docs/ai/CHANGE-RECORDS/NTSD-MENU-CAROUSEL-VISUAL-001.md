<!-- CHANGE-RECORD
id: NTSD-MENU-CAROUSEL-VISUAL-001
status: IN_PROGRESS
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Menu/MenuLoopCarousel.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuCarouselTextEffect.cs
code-path: Assets/NTSD/Resources/UI/MenuCarouselText.shader
code-path: Assets/NTSD/Scripts/Test/Editor/MenuCarouselVisualEditorTests.cs
authority: User 2026-10-03 requests reference-image centered white/red/black selected text, distance size/color/soft edges and an infinite loop mode selector
evidence: docs/ai/TASKS/NTSD-MENU-CAROUSEL-VISUAL-001.md
-->

# NTSD-MENU-CAROUSEL-VISUAL-001

Pre-change: existing uncommitted NTSD-MENU-LOOP-CAROUSEL-001 implements cyclic seven-item motion and input; its record explicitly leaves reference visuals pending. TMP labels already use user-edited fonts/materials. This task inspected both supplied images. Current Menu disk SHA is 5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D; use current bytes as the protected baseline, without restoring the historical scene.

Scope: add a lightweight TMP presentation component, attach it to the existing carousel's labels at runtime, blend selected scale/white face/red outline/black underlay into gray smaller softer distant text, fade at viewport edges. Keep cyclic phase/input implementation and option identities. No font, scene, package, DAT or mode callback remapping.

Side effects: per-label private runtime material copies prevent changing shared font materials. On release/disable restore original color, material and rect state; destroy only owned in-memory materials at lifecycle cleanup. Carousel still owns and restores layout only while active. No persistent scene objects or extra mode events. No battle shutdown order or simulation changes.

Acceptance: compile; focused tests for center vs edge style, material isolation including fallback atlases, restoration/no growth after reopening and wrap continuity; existing motion tests; original clean Menu Play plus captured screenshot if editor channel is available. Distinguish event-interface input from physical mouse/device verification.

Rollback: forward edit only this task's declared integration hunks using hash-checked preimages in FILE-OPERATIONS/NTSD-MENU-CAROUSEL-VISUAL-001-PREPARE; retain all pre-existing edits and historical evidence. No irreversible operations authorized.

Validation and final state will be appended after execution.
