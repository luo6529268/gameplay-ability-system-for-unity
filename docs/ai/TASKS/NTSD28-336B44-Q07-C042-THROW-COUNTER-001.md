# NTSD28-336B44-Q07-C042-THROW-COUNTER-001

状态：`RUNTIME_PENDING / FOCUSED_TEST_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C042/R12。当前唯一规则权威是正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable 构建闭包；参见 `docs/ai/CURRENT-AUTHORITY.md`。用户限定战斗场景与 runtime，不改 DAT 值、Scene、项目框架或非战斗功能。

## 首差与准确范围

当前正式 `source/ntsd28_core/src/simulation/battle_world.cpp` 的 `BattleWorld28::advance_catch_relations()` 在非零 `throw_vx` 分支只清抓取者 `frame.frame_counter`。同源 `ntsd28_core/tests/battle_world_tests.cpp::test_native_throw_preserves_victim_frame_counter()` 用正式抓取帧/被抓帧及初始被投者计数7证明投掷后仍为7。现有项目 `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs::ApplyThrow` 在选择被投者动作后额外将 `victim.AttackingCounter=0`；`ApplyAction` 在输入选招阶段另有被投者清零，不能顺带删除。

旧 Q06 392 例源见证是 B1E13 权威，全部被投者计数8→0。现已用**未改动的既有诊断 runner**重新链接当前 336B44 Core/Playable：两次各392行、SHA-256同为 `2c14dde683fb8832084bc0190013d2c17a677d07686d80369e4217e1aee69ffc`，196例无选招计数8→8、196例有选招8→0，全部投掷1。原始输出保存在 `artifacts/diagnostics/NTSD28-336B44-Q07-C042-THROW-COUNTER-001/`。这是当前源码的受控机制证据，不冒充正式根 EXE 或自然 Play。

此次允许改动的脚本仅：`Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointThrowRawBindingEditorTests.cs` 的源见证入口常量，使既有测试读取当前版本的另存392行；`Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs::ApplyThrow` 中被投者动作计数的额外清零。先跑当前源见证的原 Editor `ImmediateThrowMatchesNativeRawBinding` 两 profile 建 RED；再只删除投掷分支的额外清零，复跑两 profile 与相邻 CPOINT 测试。若新当前源其它字段产生首差，保持失败并诊断，不扩充生产改动。`LF2Entity.ApplyCpointThrowStep10` 与非正式平行路径只读确认正式调用归属，未证生产调用不顺手修改。

验收：当前源码 witness 双跑字节一致、196/196 分支矩阵；Unity 原 Editor 编译0错，测试先 RED 后目标 GREEN，选招清零与无选招保留各有断言；定向原 Battle Scene 生产 Driver 与正式根同输入可达投掷及后继 tick 另证。正式根 LFR 初态没有 frame counter override，受控初态不能冒充根证书。必要时寻自然投掷入口，保留 `RUNTIME_PENDING`。修改后运行 `Tools/Validate-ChangeLedger.ps1`、`git diff --check`、四个受保护 Scene/config SHA。回滚仅按此 ID 的准确代码 hunks 经审阅处理，不用 blanket Git 命令。

当前验收：原 Editor 编译0错；先 RED 两 profile，后当前源码392例×两 profile GREEN 2/2，相邻选招/生产路由3/3 PASS。详[验收记录](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-THROW-COUNTER-001/ACCEPTANCE.md)。正式根/原 Battle Scene 自然投掷及后继完整 tick 未验，不关闭 C042 的运行时出口或 Q07。

