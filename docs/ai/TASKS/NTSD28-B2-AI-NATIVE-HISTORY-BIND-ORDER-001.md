# Task Contract — NTSD28-B2-AI-NATIVE-HISTORY-BIND-ORDER-001

> 状态：`FOCUSED_TEST_PASS / NATIVE_HISTORY_PRE_BIND_READY / AI_TICK1_EXACT_EQUAL / NEXT_FIRST_DIFFERENCE_TICK2_PREVIOUS_MASK / FORMAL_EXE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / AI-EXACT-INPUT`  
> 建立日期：2026-09-04

## 目标

关闭 AI v3 joint 在 completed tick 1 的新首差：authority `keyHistory[0] == -1`，Unity 为0。让成功注册的
2.8 native entity先建立权威五项 `-1` history，再由 generation-owned `BattleCharacterInputStore` 捕获初值；
注册失败、Legacy profile和snapshot restore语义不扩改。

## Authority 与当前首差

- `input_state.h` 的 `EntityInputState28::key_history` 初值固定为 `{-1,-1,-1,-1,-1}`。
- `input_routing.cpp::push_history` 左移五项并把新edge code写入末尾；AI tick1左方向edge后应为
  `{-1,-1,-1,-1,4}`。
- Unity runtime在注册成功后才调用 `InitializeNativeHistory`，但 `SimulationRegistryModule` 已先
  `CharacterInputWriter.Bind` 并捕获了全0 history；首个AI commit把这份旧0值写回runtime。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅同源断言如实际失败）
- 本 Task、Change Record、Ledger、STATE、handoff与总表
- `Tools/NTSD28AuthorityTrace/README.md`（仅更新当前first-difference说明）

禁止修改 AI decision算法、native combo/history推进算法、RNG、pass顺序、snapshot restore、Config/DAT、
Scene/Prefab、ProjectSettings、Packages、权威源码或正式 EXE。

## 不变量

- 只在runtime slot成功提交、entity已OnAdded且writer尚未Bind的窗口初始化native history。
- 注册失败不得因本修复新增history副作用；Legacy profile不初始化`-1`。
- snapshot restore保留快照里的history，不重置为`-1`。
- common/standing human fixture与AI RNG逐次结果必须保持相等；本包关闭首history差或冻结下一个真实首差。

## 验收

- test-first先加入AI tick1 `[-1,-1,-1,-1,4]`断言并确认修复前精确red；
- Unity compile0；exporter、registry/input/AI/lockstep focused通过；
- common/standing保持equal；AI tick1 history相等，joint首差下移或整体equal；
- full SelfCheck、Console0、validator通过。

## 回滚

恢复成功注册后的历史初始化位置与本包断言；不回退first-tick readiness、accepted RNG trace或此前B2包。

## 完成证据

- red `c9956771...`；compile0；exporter `d7fa2ffe...` 11/11；broad `f6cfd497...` 183/183。
- common/standing v3仍equal；AI tick1 exact input及RNG全equal。
- 新首差下移为tick2 slot1 `previousMask` authority0 / Unity2。
- 18:50:38 full SelfCheck PASS；预期负向日志清除后Console0。
