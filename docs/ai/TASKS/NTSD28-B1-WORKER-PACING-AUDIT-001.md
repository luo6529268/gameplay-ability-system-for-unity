# Task Contract — NTSD28-B1-WORKER-PACING-AUDIT-001

> 状态：`FOCUSED_TEST_PASS / REAL_REASON_CAPTURED / CURRENT_PATH_INACTIVE / B9_REVALIDATION_TRIGGER`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

闭合B1剩余dedicated worker边界：读取真实`NTSD_Battle`的worker ineligibility reason，核对
single-in-flight提交、publication acknowledgement和LocalFreeRun two-interval drain的实际交互，判断
当前inactive是否为正式生产路径，或是否存在Fast3ms默认分支差异。只在证据需要时再拆生产修复包。

## 允许文件

- `Assets/NTSD/Scripts/Test/Editor/BattleHostControlPlayModeProbeEditor.cs`
- 本Task/Change、Ledger、STATE、handoff、总表

默认不改Driver、worker、Scene、Prefab或ProjectSettings；如需改生产，必须另建Change。

## 不变量与验收

- 报告必须记录worker active、ineligibility reason、failure和last submission failure。
- 读取当前Scene序列化`useDedicatedSimulationWorker=1`，但不得仅凭配置推断运行路径。
- 审计worker在同一Unity Update是否可能接受第二tick；不得把fallback Play结果外推到worker。
- 若worker因当前正式presentation binding而ineligible，记录后续B9重新启用时的B1复验触发器。
- 若worker本应active却失败，另建实现包，不能通过禁用配置掩盖。
- fresh compile0、Host相关tests、Play report和Ledger/diff证据齐全。

## 回滚

移除probe中的worker诊断字段；保留此前fallback runtime报告，不修改生产配置。

## 实际验收

- Scene静态配置`useDedicatedSimulationWorker=1`；真实Play报告却为inactive，exact reason为
  `unity-presentation-bindings-are-still-attached`，worker failure与submission failure均为空。
- `ResolveDedicatedSimulationWorkerIneligibilityReason()`在存在Unity presentation bindings时fail-closed；
  当前正式运行因此使用已通过32.719/3.888ms trace的inline fallback，不存在当前默认worker差异。
- source审计确认worker为single-in-flight：首次submit后`CanAdvanceTick`因in-flight/awaiting ack返回false，
  因而尚不能在同一Unity Update排空第二interval。
- B9若移除Unity presentation bindings并可能使worker eligible，必须先建新的B1 worker cadence包，
  完成two-interval/33/3ms/worker publication真实trace；在此前不得把worker提升为正式默认路径。
- worker audit report SHA-256：`DAC9D1D1D8CE14E8D850B032FC16E929C09E319AD49FB1F7B8381DD11EC5C0ED`。
