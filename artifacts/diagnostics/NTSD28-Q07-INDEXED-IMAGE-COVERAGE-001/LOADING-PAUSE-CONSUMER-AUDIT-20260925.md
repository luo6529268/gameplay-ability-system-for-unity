# Q07 missing PNG consumer follow-up: loading and pause (2026-09-25)

Read-only scope: the formal root `NTSD2.8-Logan.exe` SHA-256 was freshly verified as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The inspected `source/ntsd28_playable/src` is the declared playable source. This audit did not run Unity or the formal EXE and did not alter DAT, PNG, code, Scene, or ProjectSettings.

| Missing formal VFS PNG | Formal producer and consumer | Q07 disposition |
| --- | --- | --- |
| `sprite/Loading/menu_wait.png` | `data/system.dat` `<menu_wait>` is resolved by `game_session.cpp`'s `load_native_loading_config28` into `LoadingRenderSnapshot28.background_source_path`; `main.cpp` dispatches the `FrontendScene28::loading` snapshot to `D3D11Renderer28::render_loading`. | Confirmed pre-battle loading presentation owner, outside the battle simulation/image publication gate. Preserve the project's own loading flow; no automatic copy. |
| `sprite/Loading/loading.png` | `data/resource.dat` index 47 is resolved as the loading atlas by the same config; the same frontend loading dispatch renders it. | Same pre-battle loading owner; no automatic copy. |
| `sprite/UI/PAUSE.png` | Declared at zero-based `data/resource.dat` index 22. The inspected playable C++ has no `PAUSE.png` literal or `NativeResourceCatalog28::resolved_path(..., 22)` consumer. In `d3d11_renderer.cpp`, the paused battle frame draws a solid 360×12 bar at (620,16), rather than reading an image path. | No selected pause-image consumer proved in the inspected live path. Do not treat the absent PNG as an observed battle pixel defect. An indirect/dynamic consumer or independent formal GUI pixel result remains unverified. |

The two Loading PNGs are absent from `Assets/NTSD/Content/LoganRuntime/vfs/sprite/Loading`; the full-tree audit already counted them among 114 absent `sprite/*` files. This narrows **two** paths to the loading owner and **one** path to `NO_SELECTED_CONSUMER_PROVED`, leaving **111** of the 114 without this particular refinement; it does not certify the remaining 111 as battle or nonbattle. The prior 1,255/1,031/224 counts and 1031/1031 common-path byte equality are unchanged. Battle WORDS0..5 and SPARK remain the 7/7 in-place directly selected `resource.dat` images. Q07/Q09/R17 and final visual acceptance stay open.

Source anchors: `game_session.cpp:377-425,1250-1256,3048-3066,3784-3808`; `main.cpp:2658-2682`; `d3d11_renderer.cpp:1465-1510,2292-2297`; formal `resources/runtime/decoded_dat/data/resource.dat` indices 22 and 47, and `data/system.dat` `<menu_wait>`.
