# Q12 后续方向输入写者影响复核（2026-10-04）

结论：`NO_OBSERVED_NEW_FIRST_DIFFERENCE / PREVIOUS_SCOPED_EVIDENCE_RETAINED`。`NTSD-BATTLE-DIRECTION-HYBRID-001` 的共享输入改动晚于 Q12 鸣人首 253 合成键正例，因此旧正例不能冒称“改动后同案例重跑通过”。本次逐分支源码核查显示：没有 UI 方向输入时，设备方向阈值、四方向边沿入缓冲、Attack/Jump/Defend 映射和下游正式 proxy 路由保持原语义。该 UI 改动自身的原 Battle Scene 生产 Driver 验证为 259 项断言通过，涵盖设备/UI 方向合并及方向配三种动作；没有出现新的可裁决战斗首差。按当前总表的“共享写者改动才做相邻回归”规则，以这项相邻验证和源码等价核查复用证据，不再重复完整鸣人技能 Play。

证据与边界：

- Q12 首 253 的原 Battle Scene 正例在 `NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001/REPORT.md`：防御消费后动作、PP、combo1 和输入相位 132/132 同；它早于本次 UI 方向改动。故此处只保留原正例的版本边界。
- 当前工作树的 `InputModule.cs` 只加 `TrySetMoveInput` 到既有注册玩家 sink；`CharacterInputModule.cs` 把设备和 UI 方向分别保存，以同一个阈值生成位掩码并取 OR。UI 源为零时，新方向掩码等于原设备掩码，`CurrentMoveInput` 仍取原设备向量；既有 `CheckAndEnqueueDirectionChange` 及 Attack/Jump/Defend 三个 `SetEffectiveActionPressed` 调用未改。设备 Move canceled 时，UI 源为零的情况下仍释放原设备四方向。并发 UI 源非零的行为由下条验证限定。
- 相邻任务 `NTSD-BATTLE-DIRECTION-HYBRID-001/REPORT.md` 记录原 Battle Scene Play 的 259 项断言通过：真实组件/EventSystem 到生产 Driver，设备/UI 方向 OR、方向与 Attack/Jump/Defend 同帧及方向释放；另有独立 115 项断言。该报告明确不证明正式 EXE 对齐，也没有重跑鸣人防→前→跳→Attack 的完整序列。
- 当前共享源文件写入时间：`CharacterInputModule.cs` 2026-10-04 12:32:57 UTC，`InputModule.cs` 12:32:57 UTC，新增 `BattleDirectionControl.cs` 12:34:16 UTC；原 Editor 当前 `Assembly-CSharp.dll` 为 13:06:03 UTC、SHA-256 `E1855B12488A274981DCF19BC031AD588D11DC6C8796D3B454CB3E3FD08D5743`。这与 Q12 两轮重进见证的程序集身份相同，因此重进已在该共享写者版本运行。重进只证生产 World/tick/退出残留，不证组合技。
- 并行 `NTSDInputConfig.inputactions` 的 P2 三个键由 numpad1/2/3 调为 v/b/n，P1 鸣人键位未动。本复核不替代 P2 实际设备操作验收。

未知：共享写者改动后，完整鸣人组合序列未在真实物理键或合成键下再次跑；正式根 EXE 与 Unity 的同帧 Present 像素、真人按键时差和 UI 触控设备仍未比较。若玩家在当前版本复现非例外首差，按首个不同 tick/字段回到共用输入 owner 开最小包。此次只读复核不改生产代码、DAT、Scene 或输入配置。
