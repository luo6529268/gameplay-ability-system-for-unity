# Q03 geometry witness

Change `NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001`. Diagnostic only; no battle rules are replaced.

Run `pwsh -File Tools/NTSD28Q03Geometry/Build-And-Capture.ps1`, then run the Unity EditMode test `NTSD.Test.Editor.NTSD28Q03GeometryWitnessEditorTests`. Both read the same DAT fixtures under the corresponding artifacts directory. Native calls the actual playable `HitCandidateBuilder28::append_pair_geometry`; Unity calls ParserV2/Converter and three production candidate collectors.

Compare native.tsv with unity.tsv per case. Passing the test means capture completed, not parity. The native executable is a source-linked witness, not the formal EXE. Ordinary type0 geometry does not certify held weapon strength selection, full tick ordering, damage, Play scenes, or final alignment. Build identity and fixture hashes must accompany results.
