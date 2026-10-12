# M12 single test helper output correction proposal — NOT IMPLEMENTED

M12 task/change unchanged. This proposal does not resume H08 optimization or extend production shutdown scope.

Observed failure: Q05 reused NTSD28B11AtomicPublicationEditorTests.NativeCoordinatorOn_MultiDefinitionSource2DAndAtlasPixelsRemainEquivalent; its first Source2D draw helper calls H08ResidentVisualMemoryDiagnostics.VerifySharedPublicationDraw and SaveCandidate. SaveCandidate uses fixed CandidateOutput=artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-20261010 and FileMode.CreateNew. Existing overlap-Source2D.json caused IOException. All remaining test/ABBA work stopped. This is an incomplete test, not a pixel mismatch PASS. Q05 final XML/JSON preserved.

Need one additional TEST file (outside current seven-script M12 Change ownership):
Assets/NTSD/Scripts/Test/Editor/H08ResidentVisualMemoryDiagnostics.cs
Current SHA256 32647ef6e46eb4b8b6d1ff9ecdadc0eb28e2f723fa50ab3b5d9c22de81846ef5.
Only symbols SaveCandidate (lines57–63) and VerifySharedPublicationDraw signature(line421)/its SaveCandidate call(line519).

Proposed exact behavior: add optional per-call evidenceDirectory parameter; null keeps CandidateOutput byte-for-byte existing routing. SaveCandidate resolves a local directory from the parameter, still CreateNew, never deletes/overwrites. VerifySharedPublicationDraw forwards that optional parameter only to SaveCandidate. No mutable global override, pixel/render oracle or tolerance change.

Within already approved files, Atomic fixture adds optional DrawEvidenceDirectory (default null), forwards it at its two existing draw calls. New M12 fixture assigns a unique implementation follow-up directory plus run GUID. No diagnostic label path tricks, CWD changes, skipping draw assertions, old output relocation or cleanup. SourceCache helper only if its actual call chain uses the same draw forwarding; no other assertions/logic edited.

After explicit scope approval: first append exact code-path to same Task/Change and create true current-byte Operation backups; implement optional output routing and compile once. Reuse the eight passed unchanged cases with exact source/hash evidence. Resume Q05 only with a NEW evidence version/output, then currently NOT_RUN Q06 public cache/caller, Q06B six-sheet ON/OFF pixels, and finally the first and only Q07 ABBA if all pass. No old failed result becomes PASS. First further failure stops. Existing production code, C/E/defaultOFF, shutdown contract and all holds stay fixed unless a newly found defect requires separate review.

Existing file protection: overlap-Source2D.json SHA25640e1951441196b2584c8f3a79f4c28f7398c3663439266deb171ff7c4c14c7e8; tracked and git status clean after failure. No existing file was removed or overwritten by this failed CreateNew. The extra helper file has not been modified.
