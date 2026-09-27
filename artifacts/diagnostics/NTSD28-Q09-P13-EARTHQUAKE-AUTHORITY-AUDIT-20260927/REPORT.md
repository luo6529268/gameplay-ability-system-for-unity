# Q09/P-13 earthquake 权威纠偏（2026-09-27）

状态：`FORMAL_BACKGROUND_ONLY_CONFIRMED / UNITY_BACKGROUND_CONSUMER_PENDING / RUNTIME_PENDING`。这是静态调用链与项目接线审计，不是 Q09 或 P-13 的运行验收。

## 新鲜正式源码测试

2026-09-27 用 `ntsd28_core/scripts/build.ps1` 中列出的 28 个 core `.cpp` 编译单元与原 `tests/render_snapshot_tests.cpp`，通过本机 `g++ 15.1.0`、C++17、`-O0` 编译为本目录 `render_snapshot_tests_focused.exe`，没有运行会写入原版 `source/build` 的脚本。可执行文件 SHA-256 `AE3E0FDB2C72E63041F0CF2ED640D9CC8F25C03607E4C064B1FA68C5966BE8FD`。在正式根的父目录（含测试所需 `reextracted_v2_需要保留`）运行，退出码0，输出 `render_snapshot_tests: PASS`；日志 `render_snapshot_tests_focused.log` SHA-256 `B69178691FA12B9C83BB2EFE58E5B3ECBE10BF829F01E9C689DAA3DCA488BBD0`。根目录正式 EXE SHA-256 当轮重核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。

这个测试文件内的 earthquake 两例实际覆盖 owner 锁定/转移、55052的(+2,0)、55050的(0,0)、离开5类状态时偏移不自动重置、snapshot转交，以及测试使用的 `reextracted_v2_需要保留` 330对象库仅有8个相关帧且状态集合为 `{55050,55052}`。该库是测试夹具，不作为当前正式 `resources/runtime` 内容权威；正式Han帧的证据是下方对当前 decoded DAT 的独立只读检查。整个测试二进制通过不等于正式根 EXE 自然 Han 技能、D3D11 同帧画面或 Unity Play 验收。

## 结论与依据

- 正式 playable 构建闭包在 `source/ntsd28_playable/scripts/build.ps1:77` 包含 `render_snapshot.cpp`。`GameSession28::step()` 在 `game_session.cpp:3305-3306` 先 `earthquake_.step(*world_)`、再推进 camera；`build_render_snapshot()` 在 `:3352` 发布状态。
- `source/ntsd28_core/src/rendering/render_snapshot.cpp:1060-1097` 每 tick 按活动槽升序扫描当前帧 state。现 owner 离开 `[50000,60000)` 时只释放 owner，不自动清零偏移；5XXYY 写入 `(encoded % 100 - 50, encoded / 100 - 50)` 并锁定 owner。`render_snapshot.h:574-582` 将其明确限定为 background-only。
- `source/ntsd28_playable/src/d3d11_renderer.cpp:1555-1560,1573-1577` 仅背景 layer `draw_quad` 坐标读取 `snapshot.earthquake.background_offset_x/y`。搜索该正式 renderer/core 对此字段的消费，没有实体 draw 路径。因而旧总表“背景和实体展示均受 earthquake 影响”与当前正式 playable 路径矛盾。
- 正式 decoded `data/data.txt:150-151` 将 OID726/727 指向 `c/han/han.dat` 和 `smo.dat`；两者帧150/265为 `state:55052`，相邻帧151/266为 `state:55050`，编码分别产生 `(+2,0)`、`(0,0)`。这证明所选 DAT 中有具体触发帧，但本审计未证明自然按键能进入这些帧或正式 EXE 同帧视觉输出。
- `c/han/han.dat:815,836-845` 给出候选链：帧147的抓取 `catchingact:149`，帧149 `next:150`；`:1450-1477` 给出另一路：帧264→268→269，帧269的 `hit_g:265`，随后帧265→266。它们只界定下一次可达性检查的入口，不能把 DAT 边写成已通过的自然技能或原 EXE 画面证据。
- Unity `Assets/NTSD/Scripts/Simulation/Core/NTSD28BattlePassOrder.cs:98,356` 只列 `SessionEarthquake` pass；对 `Assets/NTSD/Scripts/Simulation`、`App`、`Animation` 搜索未见 earthquake 输出消费者。`NTSD_Battle.unity:137,143-205,210-223,2038` 的项目 `Map` SpriteRenderer 已绑定 `BattleBootstrap.backgroundRenderer` 和 `BattleBackgroundPlatformPresentation`；后者 `TryApplyWorldCameraFrame()` 由 Sprite bounds 计算固定取景。任何后续背景视觉偏移都须保持项目相机/边界与实体逻辑真值不动，并尊重用户排除原版背景 DAT/图片。

## 下一精确出口

从 Han 帧147抓取→149→150 或 264→268→269→265 的候选链中，先核正式 root EXE 自然可达 5XXYY 的动作及同 tick 可观察输出，再在原 Unity Battle Scene 确认背景与实体的相对像素差异。若差异成立，先建立独立 Task/Change，限定为项目背景的表现快照/视觉绘制出口，并对 owner 切换、55052→55050、背景/实体隔离、固定相机、逻辑 checksum 和有序退出做聚焦验证。当前没有生产脚本、DAT、图片、Scene、相机或非战斗修改；也没有新 Unity 编译、SelfCheck 或 Play 结果。
