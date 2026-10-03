<!-- CHANGE-RECORD
id: NTSD-MENU-CAROUSEL-VISUAL-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Menu/MenuLoopCarousel.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuOptionBase.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuCarouselTextEffect.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuCarouselStyle.cs
asset-path: Assets/NTSD/Resources/UI/MenuCarouselText.shader
code-path: Assets/NTSD/Scripts/Test/Editor/MenuCarouselVisualEditorTests.cs
authority: User 2026-10-03 requests reference-image centered white/red/black selected text, distance size/color/soft edges and an infinite loop mode selector
evidence: artifacts/diagnostics/NTSD-MENU-CAROUSEL-VISUAL-001/REPORT.md
-->

# NTSD-MENU-CAROUSEL-VISUAL-001

Pre-change: existing uncommitted NTSD-MENU-LOOP-CAROUSEL-001 implements cyclic seven-item motion and input; its record explicitly leaves reference visuals pending. TMP labels already use user-edited fonts/materials. This task inspected both supplied images. Current Menu disk SHA is 5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D; use current bytes as the protected baseline, without restoring the historical scene.

Scope: add a lightweight TMP presentation component, attach it to the existing carousel's labels at runtime, blend selected scale/white face/red outline/black underlay into gray smaller softer distant text, fade at viewport edges. Keep cyclic phase/input implementation and option identities. No font, scene, package, DAT or mode callback remapping.

Side effects: per-label private runtime material copies prevent changing shared font materials. On release/disable restore original color, material and rect state; destroy only owned in-memory materials at lifecycle cleanup. Carousel still owns and restores layout only while active. No persistent scene objects or extra mode events. No battle shutdown order or simulation changes.

Acceptance: compile; focused tests for center vs edge style, material isolation including fallback atlases, restoration/no growth after reopening and wrap continuity; existing motion tests; original clean Menu Play plus captured screenshot if editor channel is available. Distinguish event-interface input from physical mouse/device verification.

Rollback: forward edit only this task's declared integration hunks using hash-checked preimages in FILE-OPERATIONS/NTSD-MENU-CAROUSEL-VISUAL-001-PREPARE; retain all pre-existing edits and historical evidence. No irreversible operations authorized.

Validation and final state will be appended after execution.

Validator scope clarification: the existing validator governs only Assets/NTSD/Scripts and Tools, so the authored Resources shader is declared as asset-path with manual diff/compile checks, rather than an invalid code-path. This does not omit its source, implementation ownership or audit; no validator rule is changed.

Play05 found a local TMP TryAddCharacters append-batch duplicate glyph exception. Carousel now gathers all seven current labels once per opening and passes the complete string to fallback creation; shared borrowers never repopulate the atlas. Keep Play05 failure XML. No TMP/Plugin source patch.

Play06 reached both screenshots, completed full wrap and close/reopen identity assertions; its last Scene guard failed because the fixture's static baseline became null on ExitPlayMode domain reload, not because disk SHA changed. Preserve play-results-06.xml and both original screenshots. Replace only baseline storage with UnityEditor.SessionState, and re-run the original Menu fixture. Actual hash stayed 5D79DBB7...0052D in the failure output.

Pre-change scope addition: MenuOptionBase gains a runtime external-highlight ownership flag, set/reset only by carousel acquisition/release. The current MenuOptionShowHide infers child0 as highlightObject, but TMP fallback glyphs are also child objects; allowing it to toggle them conflicts with carousel presentation. The flag bypasses that legacy visual policy while keeping selection index/sound/events. Preimage preserved in option-base-before.json.

Screenshot-driven addition: center-vs.png proves existing brush font has no fallback and lacks 决; some TMP fallback submesh GameObjects remain inactive from legacy ShowHide. Add MenuCarouselStyle.cs and new Resources/UI/MenuCarouselStyle.asset referencing existing SOURCEHANSERIFCN-BOLD SDF. A private transient font copy appends that fallback only for carousel labels; restore source font/material on release. Enable owned fallback render children while active and restore their prior active state. No source font or TMP settings file edits. New asset creation through native manage_scriptable_object(create, overwrite=false) with the existing font reference; not a scene save. Current test preimages and failures are preserved. Near-white post-snap comparison changes to tolerance because exponential snap produces subpixel float differences.

Follow-up runtime glyph witness: Play04 failed the explicit missing-glyph assertion for 闯. This project's TMP automatic dynamic fallback population is commented out, and the existing SourceHan atlas has only a limited glyph subset. MenuCarouselStyle therefore creates one private runtime font atlas from the existing SourceHan sourceFontFile, explicitly adds only the seven labels' characters, and reference-counts that temporary font/atlas/material across the labels. Last release destroys only these owned in-memory resources. Existing fonts/atlases remain untouched. Tests must verify populated glyphs and no shared-fallback retention across reopen.
First focus run 10/11: center alpha 0.16 exposed an incorrect SmoothStep edge interval. Forward-fixed effect; second original Editor run 11/11 PASS (focus-result-02.json/XML), independent outline/shadow shader supported/no errors. First original Menu visual test reached runtime checks but failed in screenshot fixture: EditMode UnityTest cannot yield WaitForEndOfFrame; CaptureScreenshotAsTexture consequently failed before frame end. Preserved play-results-01.xml and pre-fix test source; changed only screenshot fixture to scheduled CaptureScreenshot plus bounded null-yield wait. No screenshot/Play completion claim from the first attempt.

Final 2026-10-03: declared six authored sources plus new profile/meta implemented. Final generated Editor compile0error (final-compile-04.txt); original Editor focused12/12 PASS (focus-result-04.json/XML), original clean Menu Play08 1/1 PASS (play-results-08.xml), including all seven glyphs, distinct shader materials, one full wrap, second selected screenshot, close/reopen object identities and Scene protection after ExitPlayMode. Final real screenshots center-vs-235046841.png and center-tournament-235049058.png. Menu/Battle/manifest SHA match this task's starting bytes. Physical mouse/touch/Player and user art acceptance remain pending, hence RUNTIME_PENDING; this is not a battle-rule alignment claim. Full failure history, path responsibilities, commands, warning counts, protected hashes and rollback are in REPORT.md.
