<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-HIDAN-NATURAL-WARP-001
status: VERIFIED
change-kind: Q09_P20_HIDAN_NATURAL_PAIRED_WARP_VISUAL_WITNESS
code-path: Tools/NTSD28Q09Diagnostics/hidan_natural_frame430_warp_probe.cpp
authority: formal root NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and matching playable source render_snapshot.cpp plus dat_parser.cpp cumulative ranges
evidence: compile/run exit0, natural tick16 frame430 hid6 source rect, paired WARP 1995 changed pixels; scoped acceptance in artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-NATURAL-WARP-001/ACCEPTANCE.md
-->

# NTSD28-Q09-P20-HIDAN-NATURAL-WARP-001

Pre-code status `IN_PROGRESS`. [Task](../TASKS/NTSD28-Q09-P20-HIDAN-NATURAL-WARP-001.md).

Current producer: existing `hidan_natural_frame430_probe.cpp` generates the 30-tick ordinary-input LFR, but emits no sprite source-sheet details or body pixels. Existing `hun_outofimage_warp_probe.cpp` demonstrates paired D3D11 WARP on/off of one body command. The new main will combine the proven input sequence and same-snapshot WARP technique, without changing formal source or the Unity project. Expected side effects are only a new Tools diagnostic source, executable and PNG/metadata artifacts. Exact file and symbols, build/run output, hashes, pixel result and limits are pending.

Actual code: added only `Tools/NTSD28Q09Diagnostics/hidan_natural_frame430_warp_probe.cpp`. It starts from ordinary Hidan action0 without action/MP/facing override, feeds the already verified 16-tick input schedule, requires frame430/MP150, checks exactly one slot0 body sprite command from cumulative hid6 range117–128 at rect(722,0,360,289), copies that snapshot, removes only the slot0 sprite command in the copy and renders both via paired D3D11 WARP. Existing formal source/EXE, Unity runtime, DAT, images and scenes are untouched. Compile/run/independent pixel result pending.

Final status `VERIFIED`, limited to the formal paired-source GPU witness. Formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; diagnostic source SHA-256 `8D242B7D0FD7EA69FB33A6749605B33D1376025DA2A8D5189838CCF6270ED89E`. The reused 55-argument playable/core/D3D11 build exited 0 with empty log; diagnostic run exited 0 and reported natural tick16 action430/pic119/MP150, `hid6.png` cumulative 117–128 and rect(722,0,360,289). Same-snapshot on/off WARP 1333x730 images differ by 1,995 pixels in `[471,451,546,520)`; PNG hashes and independent Pillow analysis are in [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-NATURAL-WARP-001/ACCEPTANCE.md). The root EXE's own GPU, actual Unity Battle sheet binding/visible pixels, other P-20 branches, Q09/BATCH-05 and Q07 remain unverified. No Unity Editor run or Unity script change in this package. Rollback is limited to this newly created diagnostic source and its new artifact folder, subject to explicit deletion authorization; existing work must stay intact.
