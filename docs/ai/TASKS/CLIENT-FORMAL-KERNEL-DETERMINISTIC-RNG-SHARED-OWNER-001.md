# Task Contract — CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001

> 状态：`FOCUSED_TEST_PASS / SHARED_RNG_OWNER_READY / GOVERNANCE_CLOSED / S0_NOT_VERIFIED`
> 阶段：Formal S0 shared-owner Cut A

## 1. 目标

把当前唯一的 `NTSD.Simulation.DeterministicRng` 源码移动到 Server 仓库拥有的 `com.ntsd.battle-kernel` 包，让 Unity 通过本地 UPM 依赖继续使用同一命名空间/类型/API，并运行授权的编译、focused RNG、SelfCheck 与 S0 回归。

## 2. 允许范围

- 删除原 `Assets/NTSD/Scripts/Simulation/DeterministicRng.cs` 并把其源码和 `.meta` GUID移动到共享包。
- 修改 `Packages/manifest.json` 与 `Packages/packages-lock.json` 接入本地包。
- 修改本 Task/Record、Client Ledger/State/Handoff、Server progress 与 S0/S5阶段文档。
- 运行 Unity import/compile、package RNG focused tests、`BattleRuntimeSelfCheck`、S0 8/8 与 existing lockstep 9/9。

## 3. 不允许

不改battle rules、30 Hz tick、Scene、资源、Input Actions、transport、database、公网、snapshot/recovery、S1 protocol或formal marker；不开始FrameInput Cut B/formal AI。

## 4. 验收

- Existing Client call sites无需逻辑改写，seed与输出不变。
- moved source GUID保持 `86598656af70f284a91f23c18b720ef9`。
- Unity package focused tests复现冻结vector/digest。
- compile、SelfCheck、S0与existing lockstep均有fresh通过证据。
- S0仍只可记为NOT_VERIFIED。

## 5. 回滚

把同一源码/GUID恢复到原Client路径并删除本地package引用；不得回滚其他用户改动。

## 6. 最终结果

- 原Client RNG源码与`.meta`已移动到Server-owned `packages/com.ntsd.battle-kernel`，未保留第二份production源码；GUID仍为`86598656af70f284a91f23c18b720ef9`。
- Unity通过`file:../../NTSD_Server/packages/com.ntsd.battle-kernel`消费`NTSD.Battle.Kernel` asmdef；现有调用者保持`NTSD.Simulation.DeterministicRng`身份，无需逻辑改写。
- 冻结60-line vector/digest、.NET direct/artifact consumers、Unity RNG focused 1/1、S0 8/8、existing lockstep 9/9、fresh SelfCheck与`error CS=0`均通过。
- 本包只关闭Cut A；formal marker仍为false，S0/S5仍非VERIFIED。FrameInput Cut B/formal AI需要新的独立授权。
