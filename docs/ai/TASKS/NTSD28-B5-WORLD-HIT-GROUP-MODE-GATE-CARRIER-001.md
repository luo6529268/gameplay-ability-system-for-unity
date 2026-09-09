# Task Contract — NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_AND_CONTENT_UNCONNECTED`
> 依赖：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001 / VERIFIED`

## 目标

为Authority selected background-mode record `+0x18`建立Unity确定性world scalar
`ActiveModeHitGroupGate18`，闭合default/reset、snapshot/restore、checksum、full parity和schema，供后续
hit-group resolver消费。

## 修改边界

- production：`BattleRuntimeState.NativeHitResourceRules`、core scalar snapshot、full restore、lockstep checksum、
  full parity。
- tests：新增专属carrier test，并机械更新依赖精确schema版本的既有测试。
- schema：core scalar `8→9`、battle state `17→18`、checksum `20→21`；entity runtime保持11。
- 不接candidate或consumer行为，不装载background content，不实现C25 full-restore规则，不改Scene/Prefab/
  Config/ProjectSettings/Authority。

## 验收

- RED证明字段/restore/schema缺失；实现后focused carrier与snapshot restore通过。
- 每个值改变checksum，并出现在full parity；warm scalar capture/checksum 0 allocation。
- B5与完整Unity侧NTSD28自动组、编译、SelfCheck、Console、Scene unchanged及Ledger通过。

## 回滚

移除该world field及其snapshot/checksum/parity投影，恢复三个schema版本及相应测试期望；不涉及content或Scene。

## 验证结论

- RED：新专属test触发9个预期编译错误，准确证明field、4-arg restore及snapshot缺失。
- focused：`19/19`、schema兼容批次`12/12`与`21/21`；warm capture/checksum 0 allocation。
- broad：B5 `529/529`（job `d9350d30c2384dd0847679f1d4ba994f`）；完整Unity侧NTSD28
  `1087/1087`（job `6754731d4b5345bea9b37ab2e6a46d37`）。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly`：0 error、129既有warning。
- `Temp/NTSD_BattleRuntimeSelfCheck.result`：2026-09-06 20:27:08 +08:00，`PASS`；随后Console清空后0 error。
- 当前Scene `NTSD_Battle`：dirty=false、root=13、SHA-256
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`；未进入Play。
- Change Ledger：332 records / 285 governed code files，PASS。
