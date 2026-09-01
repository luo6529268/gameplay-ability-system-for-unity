# Task Contract — S0-WITNESS-001

> 状态：`FOCUSED_TEST_PASS / COMPILE_PASS / FRESH_SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / CLIENT_S0_WITNESS_READY / S0_NOT_VERIFIED`
> 所属阶段：服务器帧同步 `S0`  
> 前置：`S0-INPROC-AUTHORITY-001` validation-only evidence complete to current scope

## 1. 目标

让同进程 S0 authority session 在首次 Server/Client aggregate mismatch 后生成可审计的十 checksum 值 witness，并在真实 test-only entity 的 server + two-client world journal 下验证正常一致路径；不改变任何战斗规则或正常 tick 行为。

## 2. 允许文件

- `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepAuthoritySession.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepChecksumWitness.cs`（新增）
- `Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs`
- 此 Task、`S0-WITNESS-001` Change Record、Ledger、State 与 handoff/progress 文档。

任何其他 Client authored script 需要新的范围确认后才能修改。

## 3. 不做

- 不改 C++ authority、battle simulation/tick/pass、FrameInputSet 语义、30 Hz、Scene、资源、DAT、配置、UI、renderer 或 input config；
- 不进入 S1 的协议/deadline/ACK/Jitter，也不做 Socket、transport、database、ServerHost、公网或控制面；
- 不用 snapshot/state overwrite 掩盖 mismatch；
- 不把 focused evidence 写为 S0 `VERIFIED`。

## 4. 验收

1. 正常一致帧继续只走 aggregate checksum fast path；
2. 首次 mismatch 后得到固定域序、RNG、slot/generation 与双方 structured snapshots 的 witness；
3. witness 后会话维持 fail closed，后续帧不覆盖首差；
4. real-entity 的 server + 2 client worlds 在相同 seed/barrier/journal 下连续 N 帧一致；
5. 当前 7/7 S0 focused、9/9 existing lockstep 与 self-check 按实际运行结果记录；历史截图只有扩展前5项，不能替代当前7项；
6. Change Ledger/Record/State/Handoff 与实际 diff 一致。

## 5. 当前限制

- 当前 Unity Editor 持有项目；不得启动第二个 Editor 写同一 Library。
- 用户只授权本合同列出的 S0 runtime/test 范围；若需要实体 asset/scene 或任何 gameplay 修复，停止并请求新授权。
- C++ full trace 仍为独立 evidence boundary；本包不改变其状态。

## 6. 关闭证据（2026-08-30）

- Unity MCP 连接唯一实例 `gameplay-ability-system-for-unity@b1b02287`（Unity 2022.3.62f3，端口 6401）。
- EditMode job `2a52bf503c784bf3a4e8642a089b8838`：当前 `InProcessLockstepAuthoritySessionEditorTests` `7/7 passed / 0 failed / 0 skipped`，`resultState=Passed`。
- EditMode job `6811ad1b4c56427bbcd085aa8f107d05`：`BattleLockstepSessionEditorTests` `9/9 passed / 0 failed / 0 skipped`，`resultState=Passed`。
- `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`；MCP Console 精确过滤 `error CS` 返回 0 条。
- 本轮没有新增 Client 源码修改；Scene、资源、Input Actions、battle rules、30 Hz、transport 与 recovery 均未触碰。
- 本 Task 的 witness/fixture 验收已闭合；formal shared-Kernel/C++ domain mapping 属于后续具名包，因此 S0 阶段仍为 `NOT_VERIFIED`。
