<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-F02-FORMAL-OFFSCREEN-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp
authority: 336B44 playable F02 GameSession snapshot and D3D11 renderer, with root LFR and original Unity Game View controls
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-F02-FORMAL-OFFSCREEN-001.md
-->

# F02 current-playable offscreen frame witness

Created before modifying the declared diagnostic C++ file. Existing tool already reproduces the current-source F02 Naruto/Tayuya/OID600 kind10 chain and writes CSV/LFR; it has no GPU output. Proposed single opt-in renders only relative tick39 with the current playable D3D11 renderer via WARP and stores one PNG plus sprite geometry in a new output directory. This is an authority-source diagnostic, not a formal root-EXE screenshot. No production, DAT/image, Unity Scene/camera/framework or nonbattle change. Acceptance and forward-only rollback are in the Task; actual diff, build, run and limitations will be appended.

2026-10-03 first code/build/run: the existing C++ probe accepts opt-in `--offscreen-tick39` for `kind10`, writes five-sprite geometry and uses `D3D11Renderer28` WARP at 1333×730. New g++ build against the current playable closure plus `d3d11_renderer.cpp` exited 0 with no output. First unique run `source-bg1-z400-20261003-01` emitted a nonempty PNG and five geometry rows (Naruto pic1, OID219 pic3/8 included), then process exited `0xC0000005` before finishing CSV/LFR. This is a diagnostic process failure, not an observed battle-rule difference; all partial original outputs are preserved. The renderer local currently outlives `CoUninitialize`, unlike the repository's successful offscreen probe pattern. Correct only object lifetime by destructing the renderer before `CoUninitialize`, rebuild, and rerun under a new unique directory. Do not promote the partial PNG as an accepted formal output.

2026-10-03 final source build/run: nested the renderer's scope so it destructs before `CoUninitialize`, with no other behavioral code change. New unique g++ build `build-20261003-02` exited0, stdout/stderr empty. New `kind10 530 400 1 --offscreen-tick39` source run exit0, produced valid 1333×730 PNG SHA `437E550EB87F575140085D5C7E503CEE51B721FAB4C59B4676D2BC6F5EA09123`, geometry 5/5 expected visible identities, and black connected component `(148,350)..(237,437)` area7,680 in a bounded image region. Current formal root trace tick39 matches all five OID/pic pairs. New no-flag invocation independently exited0. Each new run's summary/source/opponent/relation/LFR five files equals the pre-change BG1/Z400 baseline byte-for-byte. Only the declared diagnostic C++ source changed; no formal source/EXE/resource, Unity production/Scene/camera/nonbattle modification. This Record is VERIFIED only for current playable source WARP offscreen output. The root EXE actual GPU/Present screenshot and full Q09/F02/Q12/goal stay open. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-F02-FORMAL-OFFSCREEN-001/REPORT.md).
