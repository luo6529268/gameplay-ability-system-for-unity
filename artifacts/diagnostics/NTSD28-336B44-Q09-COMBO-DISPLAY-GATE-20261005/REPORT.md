# Q09 连击图层的正式运行门槛复核（2026-10-05）

状态：`FORMAL_DEFAULT_DISPLAY_DISABLED / NO_BATTLE_REPAIR_TASK`。本次只读复核正式根 EXE、当前 playable 源码、项目模式 Asset 与图片在位状态；没有修改脚本、DAT、图片或 Scene，也没有运行 Unity Play。

正式根 `NTSD2.8-Logan.exe` 本次重新计算的 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 `data/mode/ntsd.dat` 确有 `<combo>` 记录，指向 `sprite/combo_hits.png`。正式 `GameSession28` 仍可生产连击计数并把记录交给 render snapshot；但 `game_session.h` 中 `native_combo_runtime_display_enabled_49fd8` 的普通会话默认值是 `false`，旁注明确对应发行 EXE 的进程级显示锁存初值 0。`render_snapshot.cpp::project_native_combo_command` 在读取计数或图集前先检查这个锁存；所检 playable 源码中仅 `offscreen_gate_main.cpp` 的显式诊断入口会改它，正式根 `main.cpp` 的普通 GUI 初始化没有开启入口。故当前正式普通战斗不会发布这个可选连击图层；这不表示连击计数和过期规则被关闭。

Unity 的 `ProjectBattleModeConfig.asset` 中 `combo.enabled: 1` 对应战斗计数/规则 tuple，并不是正式版可选图层的显示锁存。当前 Unity 生产脚本仍无 `NativeComboHitCount1E0` 的战斗表现读取者，`LoganRuntime/vfs/sprite/combo_hits.png` 也已不在磁盘；正式根对应图片仍在。由于正式普通 GUI 的图层门槛为关，这两个事实**不构成已证非例外画面首差**，不应从 2026-09-22 的历史“缺显示消费链”报告直接派生 Q09 生产修复、恢复用户删除的图或 Play 矩阵。若以后用户明确启用该图层或当前正式可达普通路径出现实际可见命令，再以同初态图层首差回访；战斗连击逻辑本身仍按 Q06/Q07 的既有证据与触发门处理。

证据入口：正式 `source/ntsd28_playable/include/ntsd28_playable/game_session.h` 的 `BattleConfig28::native_combo_runtime_display_enabled_49fd8`；`source/ntsd28_core/src/rendering/render_snapshot.cpp::project_native_combo_command`；`source/ntsd28_playable/src/{main.cpp,offscreen_gate_main.cpp,game_session.cpp}`；正式 `resources/runtime/decoded_dat/data/mode/ntsd.dat`；Unity `Assets/NTSD/Resources/ProjectBattleModeConfig.asset` 和 `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleNativeComboExpiryModule.cs`。此为源码/配置边界证据，不替代正式 EXE GPU 或 Unity 同帧 Play 证书。
