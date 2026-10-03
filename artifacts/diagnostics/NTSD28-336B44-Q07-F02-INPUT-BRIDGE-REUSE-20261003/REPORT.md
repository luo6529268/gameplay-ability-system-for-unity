# Q07/F02 共用物理按键桥复用核查（2026-10-03）

结论限定为 `SHARED_J_BRIDGE_REUSED / F02_SPECIFIC_PHYSICAL_PLAY_NOT_RUN`。本次只读，不修改脚本或 DAT，不启动 Play。F02 run-07 的完整 Driver 通过显式 `FrameInputSet` 送入 P1 按键；正式配对中的拾取与轻投输入分别是相对 tick18–19 和 tick24 的同一个 `SimulationInputButtons.Jump` 位（值32）。此旧名位由项目物理 J 的 Attack Action 经现有 `CharacterInputModule.CaptureHeldSimulationButtons` 映射而来，正式输入语义是 attack，而不是玩家按 K 跳跃。

现存**336B44 内容下**的原 Battle Scene 物理设备探针 `naruto-physical-x190-336b44-phasepair-source-01.json` 直接记录 J 在两次分离的按键段 tick6–7 和20–21 均产生 P1 canonical `Jump`；两次起点的 `pressed=Jump`，次 tick 按住时 `pressed=None`，最终 `PASS_SCOPED_PHYSICAL_CHAIN`、有序关闭 World/槽/池借用均0、退出干净。其对应旧 `ACCEPTANCE-20260926.md` 使用 B1E13 根身份，只作为历史索引；这里仅引用 336B44 命名的原始 Unity 结果，不能把旧报告的正式根结论升级为新版权威。

调用链静态复核：`SimulationTickDriver.StepOneTick(FrameInputSet,...)` 和生产输入提供者的 `StepOneTick(bool,...)` 都进入同一个 `StepOneTickInternal(FrameInputSet,...)`，后者调用 `SimulationWorld.ApplyFrameInputSet`。`SimulationFrameInputModule.ApplyFrameInputSet` 对每个按键只看 `playerInput.Buttons` 并向角色共用 `InputBuffer.EnqueueCompletePacketKeyForTick` 写入 down；不按角色、持武器或 F02 分出独立物理桥。生产提供者额外计算 `Pressed/Released`，但此模块的逐键消费只读 `Buttons`，从相邻 tick 的 down 变化形成输入边沿。F02 的两次攻击间已有明确中性间隔，故这条桥接输入的重复角色级映射测试没有新信息。

证据边界：F02 run-07 三实体同条件规则/事件/关闭已分别直接验证；上述物理设备探针证明**共用 J 映射和分离重按**，不等于玩家实际对三实体连续操作的 F02 画面或硬件键盘自然场景已实测。F02 专项不再为同一通用输入桥重复跑角色案例；若之后 Q12 整场自然操作或输入桥代码变化出现首差，再按同种子/输入/tick 做最窄物理键重放。Game View 像素另归 Q09/Q12，不由此核查裁决。

主要原始证据：

- `artifacts/diagnostics/NTSD28-Q07-NARUTO-PHYSICAL-PICKUP-PLAY-001/naruto-physical-x190-336b44-phasepair-source-01.json`：两段 J 与 canonical/pressed、场景退出。
- `artifacts/diagnostics/NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-scene-20261003-07/00056.json`、`paired-shutdown-20261003-07.json`：F02 两段输入及正式规则/事件/关闭配对。
- `artifacts/diagnostics/NTSD28-336B44-Q07-NATIVE-THREE-KEY-ORDER-001/STATIC-CONTRACT-CORRECTION.md`：正式 attack4 与 Unity 旧按键位交叉映射的静态合同；该报告本身不证明 F02 物理场景。
