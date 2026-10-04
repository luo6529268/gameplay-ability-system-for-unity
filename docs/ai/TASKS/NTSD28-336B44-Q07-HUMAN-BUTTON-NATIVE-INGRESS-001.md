# NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001

状态：`SUPERSEDED / HISTORICAL_DIRECT_INPUT_EVIDENCE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07；Q09/P-08 是首差见证，不把血点表现结果当输入规则来源。

2026-10-04 Q12 自然物理键揭示本 Task 前提漏掉 `CharacterInputModule` 既有 J/K/L→旧字段交叉映射；其下游再轮换已由[精确更正 Task](NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001.md)撤去。以下原始计划、测试和 22tick 结果均保留为发生过的事实，但不再作为玩家三键生产修复证书，也不在当前队列单独执行。

正式权威：根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable `input_state.h`、`input_routing.cpp::route_native_standing_attack`。正式输入索引 4/5/6 为 Attack/Jump/Defend；普通站立 Attack 在此初态经同步 RNG site `0x82` 选动作 60。

实际首差：原 Battle Scene 以正式 LoganRuntime、鸣人 OID2/鼬 OID9、mode0/难度0、源 X500/540、Z650、HP500/30、RNG seed0、同一前两 tick Attack 的 22 tick 生产 Driver Play，正式根 tick2 鸣人动作60，Unity tick2 动作110；Unity 后续目标 HP30、血点0，正式 tick8 HP10、tick22 有 1×3 血点。Unity 探针输入确认为 `SimulationInputButtons.Attack=16`，初态 CRT `3374725112/3000`。前轮错误 Jump 注入及其 130/132 部分吻合不作生产证书；本轮修正输入原件保留。场景退出 clean，Battle/Menu/GameConfig/ProjectBattleModeConfig 四 SHA 本轮不变。

代码路径和预期边界：当前 `SimulationFrameInputModule.ApplyFrameInputSet` 将语义 Attack 转 `FuncKeyMask.att`，人类本地输入模块 `NTSDInputStateModule` 将其写入旧 `KeyAttack`；`NTSD28InputTwoPassModule.FreezeProducerState` 按历史 C# 字段布局把旧 `KeyAttack/KeyJump/KeyDefend` 投影至正式 Defend/Attack/Jump。这在上述真实 Scene 产生错误动作。只在现有 `NTSDInputStateModule` 的**人类且正式 native pipeline**入口做语义到旧载体的适配，并保持本地 held/previous 语义连续；不改已证的 AI 冻结顺序、正式索引、DAT、Scene、资源、非战斗输入或用户 UI。若定位表明还需 `NTSD28InputTwoPassModule` 的状态回读小改，限于人类本地状态，不改 AI。

脚本前计划路径：`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputProducerMigrationEditorTests.cs` 写 test-first；生产候选 `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs` 与必要时 `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`。验收先让语义 Attack/Jump/Defend 的三组 one-hot 经过现有输入缓冲→人类 producer→正式 proxy 的定向测试 RED→GREEN，包含 previous/释放和 AI 旧布局/非 native profile 相邻不变；再编译、原 Editor 定向测试、原 Battle Scene 同一 22 tick Attack 复验，逐 tick 动作/HP/RNG/血点命令与当前根对照，清洁退出及四 SHA 稳定。仅发现跨 pass 副作用时扩检 SelfCheck。真实人手键与 Q12 整场仍另验。

风险与回滚：三键入口是共享战斗路径，误改会影响连招和 AI。先以完整 tick 首差与聚焦红例裁决，不用把多个入口各加特判；若测试显示初态/探针而非生产映射问题，停止生产修改并更正本记录。回滚只逆向撤销本 Change 的精确代码 hunk，保留原始诊断及已有用户/并行修改，不使用会覆盖其它工作的 Git restore。
2026-10-04 队列重整前测试结果：原项目 Editor 新程序集导入后只运行 `DataOrientedHumanSemanticButtons_ReachFormalThreeButtonIndices` 一项，job `26f844e9164348649265f0ed7aeeb133`，1/1 按预期失败：语义 `att` 应进正式 index4=1，实际为0。原件在 `artifacts/diagnostics/NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001/editmode-red-20261004.json`。这证实测试覆盖的共用人类/native 投影错误；生产文件仍未修改。用户要求先完成新版总表去重，本 Task 是整理后唯一优先恢复的规则首差；P-08 血点与螺旋丸后续键窗均先等此入口。
2026-10-04 文档队列重整后按顺位恢复：人类/native 三键语义在本地输入共用入口一次投影到旧载体，native 第二 pass 人类回读做逆投影；AI、非 native 和正式冻结索引不变。聚焦测试已扩充按住/释放，生成 Editor 工程编译 exit0、0 error/332 warning，见同 ID Change Record。原 Editor GREEN、相邻用例、原 Battle Scene 和四保护 SHA 仍待，当前只报 `COMPILE_PASS`。
2026-10-04 原 Editor 精确筛选五项 EditMode GREEN：job `dabf1b5adc334ff1bd737a1ee71cea72`，5/5 PASS，含新增正式三键及 held/release、AI、legacy/non-native 相邻边界；原始 JSON 见同 ID Change Record。下一仅做原 Battle Scene 的既有 22 tick Attack 同初态复验；真实物理键与 Q12 整场仍待。
2026-10-04 复验准备：旧忽略请求文件含第二轮 runId，按文件操作审计合同保留其字节。Task 追加现有 `NTSD28Q09P08SameStateBattlePlayProbeEditor.cs` 为**测试诊断路径**，只增加一个优先读取的新请求路径，以新 runId 触发第三轮；结果写新文件，不覆盖第二轮。风险是 Editor 初始化探针误启动；用唯一新请求/结果、不重复请求、进入 Play 前 clean/空闲检查和输出四 SHA 限界。回滚仅撤销诊断中新增的优先路径 hunk，保留旧路径和所有证据。
2026-10-04 原 Scene 第三轮已完成、前后四保护 SHA 完全相等；初态CRT匹配、同步随机表不匹配，故动作60/65不是可裁决的生产差异。输入修复后不再进动作110，22tick 中伤害HP10、血点1×3已出现，六字段130/132同。原始JSON、机械配对及表哈希见同 ID Change Record。此包保留 `RUNTIME_PENDING` 给真实键/同表或Q12代表性验收，不再为抬状态重复原22tick。
