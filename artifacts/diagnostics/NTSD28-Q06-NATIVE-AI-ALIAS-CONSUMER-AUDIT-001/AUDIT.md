# Q06 NativeAI 持久别名消费审计

2026-09-21。READ_ONLY_SOURCE_AUDIT_COMPLETE；未修改生产或测试脚本，未运行 Unity 测试。不是运行时对齐证书。

## 结论与权威

持久字段 NativeAiProfileObjectId 已存在，但 AiSensingSnapshot 没有对应行字段；当前同步 RNG 决策仍用 ObjectId 判断三个别名匹配分支，且 special-profile 仅在成功设置 combo 时返回，遗漏正式源码的 unsupported-custom-profile 提前结束普通决策路径。

权威根为 J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan；正式身份继续遵循 CURRENT-AUTHORITY.md。source/ntsd28_playable/scripts/build.ps1:63 明确包含 src/simulation/native_ai.cpp。以下均为当前源码静态证据，尚无本包 source witness 或 Unity RED。

## 必须区分的源码语义

source/ntsd28_core/src/simulation/native_ai.cpp:

- 49–73：classifier 只承认 alias 2/4/6/7/8/9/10/11/33/34；其他值回退 actual object_id。不是任何非负 alias 都替代 ID。
- 75–80：match(value) 要求 alias >= 0，且 alias 或 actual ID 任一个匹配。负 alias 即使 actual ID 相同也不匹配。
- 1484/1524/1534：预测分支分别使用 classifier、match(34)、match(1)；其后的 actual object_id == 1 追击分支仍只读实际 ID。
- 1564–1628：先适用性检查，再消耗 0x3c；正结果直接返回，不检查 alias。通过 gate 后 match(33) 且有效目标才消耗 0x6c；命中距离、MP、朝向等条件后设置 combo_state[2]=3 并成功返回。未成功时 alias != 0 返回 unsupported_custom_profile，包含 -1。后面的旧注释与实际 -1 分支不一致，以可执行代码为准。
- 1650–1657：成功或 unsupported 都停止普通 AI 后续处理。2031–2034 把 unsupported 传到外层结果；simulation_tick_driver.cpp:489 记录诊断。停止的是该 AI 决策路径，不是停止世界 tick 或抛出异常；InputRouter sample 仍执行。

## Unity 对应路径与最小实施边界

全部路径相对 Assets/NTSD/Scripts/：

| 文件 | 已确认位置与下一实施要求 |
|---|---|
| Simulation/Ai/Snapshots/AiSensingSnapshot.cs | 构造36、字段82、CopyTo146附近：独立派生 alias 数组及增长复制；不能覆盖 ObjectId。 |
| Simulation/Ai/Runtime/SimulationAiSensingModule.cs | TryCaptureRow 1647附近从 runtime 采集；内部 AiSoASensingRows.Grow 使用继承 CopyTo。 |
| Simulation/Ai/Runtime/SimulationAiDecisionModule.cs | canonical row 1467、普通 capture 2542、stale检查1884、全行/单行比较3567/3707均需要 alias；refresh 1973/2929等复用 TryCaptureRow，实际 ID 的特殊成员判断保持。 |
| Simulation/Ai/Runtime/SimulationAiDecisionTypes.cs | AiUnifiedSnapshotField 当前末项 HitJ=61；如增加诊断字段，应追加而非重排既有数值。 |
| Simulation/Ai/Kernel/AiDecisionKernel.cs | 996 classifier，1030 match34，1037 match1，1046 actual ID；1064 special-profile 及546 caller必须一起核对提前返回契约。 |

BattleAiUnifiedRowPublisher 当前只发布明确的 pending 变化字段，不承载 ObjectId；不能因为名称是 publisher 就机械增加写职责。新 alias 在出生/融合更新，具体快照创建/刷新时点应通过生产入口测试证实。若发现 pass 内有新的 alias writer，先补所有权与准确 Task 路径。

保留 SimulationAiInputModule 特殊对象扫描、目标身份与旧非同步分支的 actual ID 消费。Unity 历史 KeyAttack/KeyDefend/KeyJump 与 native 物理按键有既有轮换映射，不能按名称直接改键。ApplyProducerInputEdges:1149 在同步路径不应用旧边沿；最终边沿需继续通过实际 input producer/sampler 验证。

NativeAiProfileObjectId 已属于持久 runtime/snapshot/checksum 合同。本次预期只是派生 AI 行接线，不因新增缓存数组自动升级 entity/schema 或提升 raw47/3。

## 按分支代表验证，不遍历全角色

下一包先建立准确 Task/Change，创建调用真实 NativeAi28 API 的源见证，再编写 Unity 定向 RED。建议等价类：

1. recognized alias 与 actual ID 不同；unrecognized positive alias 的 classifier fallback。
2. actual ID 34 或1配负 alias：不走对应 match；actual ID1追击仍可走，证明两种判断未混同。
3. 非33 actual ID配 alias33，及 actual33配 alias0：0x6c/组合技可达；actual33配负 alias不可达。
4. 0x3c 正值：任意 alias 均不进入 unsupported，继续 ordinary。
5. 0x3c 零值且特殊动作未成功：alias0继续，负值/非零值停止；比较后续输入和 RNG sites/cursor，而非只比较 bool。
6. 派生数组增长、三条实际 row producer、stale/比较发现 alias-only 变化、snapshot restore 后重建行。
7. 一个生产 AI 入口代表，确认同步采样/返回后不会额外执行 ordinary tail。

已有 NTSD28AiSpecialProfileRandomEditorTests 可复用，但其手工行未声明 alias；新增字段默认值不能被当成正式 runtime 出生值的证明。先记录原失败，再对照当前权威明确 fixture 意图。

源见证不是正式 EXE 录像。局部修复只运行本包/受影响 AI 分支；稳定后按实际风险选联合回归，不重跑融合、无关角色或全场景。编译与源见证未执行前不得标 VERIFIED。

## 独立复核

effect_fall_review 只读复核确认上述三种消费语义及停止边界。精确方法名为 step_profiled_combat（native_ai.cpp:1448），不是历史上下文中的 step_predicted_combat。建议增加 actual77/alias1 与 actual1/alias0 的对照，直接证明 alias1 不触发 actual1 专属 chase。完整入口的采样与后继处理必须由真实 tick 见证，单独 helper 通过不能关闭该要求。未运行测试，未修改脚本。

## 恢复点

Q06 未完成，Q07 正式 DAT/角色图片迁移未开始，总目标 ACTIVE。融合四包已验职责复用；非战斗、Scene、资源、框架及已批准例外保持。本审计改变下一行动：必须覆盖 alias 的决策终止语义，不能只加字段或替换 ID getter。下一步建立 source witness Task/Change，先取得 source 输出和 Unity RED，再按上述准确路径实施。
