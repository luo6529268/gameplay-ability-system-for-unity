# R8-WP01G-R05 candidate / PreInteraction joint evidence

> 日期：2026-08-23  
> 结论：`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`  
> D-ID：`D-SCHED-007`、`D-PERF-001`

## 1. 结论

本包没有发现candidate、consume或PreInteraction gameplay first-difference。Unity为性能增加的
CollisionSnapshot、PairVRest、role-aware/store adapter和PreInteraction no-op proof，在本包可执行的同seed、
同配置A/B与live Play矩阵中没有改变最终战斗状态；warmed tick为0 B且未触发GC。

本结论限定为Unity当前可取得的S4证据。R1-WP02的C++ executable/full trace仍BLOCKED，因此不能写成
“C++ runtime trace VERIFIED”，也不能扩大成整个战斗系统完全对齐。

## 2. C++ Release只读source合同

- `Makefile`实际包含`src/entity/game_tick.cpp`与`src/entity/collision_collect.cpp`；
- `game_tick(...)`在active slot写`prev_frame2`后调用`collision_collect_candidates(...)`；
- 随后依次执行type0 character consume、random-weapon边界、type>0 object consume、CPoint与held weapon sync；
- collector按固定slot/itr/body顺序建立最多20个candidate；nearest/kind1等距分支按source消费RNG；
- vrest、selection、HitConfirm2/abort与candidate carrier均属于同一frozen tick消费合同。

Unity额外pass可以保留，但必须在上述两段consume可观察结果上等价；PreInteraction proof只能跳过当前点可证明
neutral的CPoint/mismatch/held sync，不能复用旧slot generation或跨中间writer缓存结论。

## 3. Fresh EditMode证据

| Job | 覆盖 | 结果 |
|---|---|---|
| `0e36158fb2164915b3f01fba688b152b` | broadphase shadow基础 | 9/9 PASS |
| `bd8d5ba246264be1a0ce08b3797b18f4` | formal brute/role/direct/tree/fallback、order/cap/RNG/generation/store | 58/58 PASS |
| `2bcdc7341ae44e39b6c91cb4cb175a55` | character/object consume、abort、damage/stat/vrest与0 B | 185/185 PASS |
| `5192097f253e4175938c10b76dcd0af6` | PreInteraction neutral/non-neutral/frame/CPoint/link/holder/generation/0 B | 15/15 PASS |
| `3b4be88f297a4e419262c219e4f4a858` | 修正后candidate+PreInteraction+validator focused | 84/84 PASS |
| `e63da64f8b714b29b1b9c998c2b1bc0e` | 完整stress Editor测试 | 256/256 PASS |

覆盖重点：20-cap、nearest/tie RNG、invalid generation、role-aware direct/tree、degenerate/fallback、异常后
RNG与candidate恢复、StoreOnly fail-closed、same-slot generation复用、neutral whole-pass、kind1/kind2、
stale held/link、frame/wait/position变化与warmed 0 B。

## 4. Fresh live Play证据

### 4.1 定向candidate / consume

`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`：`PASS`。

- 真实production world冻结10个candidate；
- 顺序为collect → character consume → object consume；
- character、weapon、special的HP/HPBound/combo/stat/vrest/durability一致；
- HitConfirm2、caught skip、effect21 abort与raw-frame分支通过；
- RNG、rests、stats、sound queue、pool、slot与world计数恢复。

### 4.2 定向PreInteraction

`Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json`：`PASS`。

- valid grab与reciprocal link；
- CPoint/weapon-sync injury、统计、位置挂点；
- mismatch throw、escape与positive/negative link residue；
- world、slot、pool、stats全部恢复。

### 4.3 同seed current / forced-legacy A/B

报告：

- `Temp/NTSD_R8_WP01G_R05_Current.report.json`：`SmokePassed`；
- `Temp/NTSD_R8_WP01G_R05_Legacy.report.json`：`SmokePassed`。

固定条件：50 AI作为相同输入负载、seed `0x4E545344`、30 warmup + 30 steady samples。AI算法本身已由用户
排除出对齐backlog；这里仅把相同AI输出当作可重复战斗负载，不认证AI parity。

current侧：

- store authority 35/35 tick applied；
- legacy oracle 35/35 tick sampled；
- shadow mismatch=0、invalid=0、legacy fallback=0；
- PreInteraction whole-pass proof成功3 tick，proof skip 5021；
- zero-GC gate PASS，cleanup exception=0，teardown restored=true。

legacy侧强制legacy candidate carrier与PreInteraction full path；zero-GC和teardown同样PASS。双方以下20项hash
全部相等：parity与lockstep各自的input、RNG、metadata、world、slots、arest、vrest、stats、events、overall。

- parity overall：`fdf240f5a312b910bd71b00f561438bf7c2d9da08840b829c09427a32a19bef1`；
- lockstep overall：`a386d02379f8a66fe86f1ddef30998a1df93e09f2812f1d028ef3e9bf7f4c6a7`。

## 5. 诊断误报与最小修正

首次current运行虽然store/oracle mismatch为0且最终hash与legacy相同，但stress validator误报：它要求
`EntryReadCount == CaptureProductionCounters()`在两段consume之后观察到的carrier candidate sum。candidate可能在
consume中被清理、替换或因abort提前终止，因此post-tick carrier只可能作为entry reads下界。

`R8-CANDSTORE-DIAG-001`仅把关系改为`entryReads >= postTickCandidateSum`，保留tick cadence、authority applied、
oracle/store-only cadence、range read coverage、fallback、hard failure、shadow mismatch/invalid与restore硬门；并新增
extra/equal/below三段回归。修正后相同current运行转为`SmokePassed`。没有修改production gameplay。

## 6. 最终验证

- fresh Unity refresh/compile完成，脚本编译0 error；
- full `BattleRuntimeSelfCheck`：2026-08-23 18:35:05 PASS；
- self-check预期负路径日志与MCP disposed-stream工具噪声清理后，Console error=0；
- `Tools/Validate-ChangeLedger.ps1`：PASS，80 records / 94 governed code files；
- scoped `git diff --check`：PASS（仅行尾转换warning）。

## 7. 剩余限制

- C++ executable/full trace仍由B-R1-WP02-01..04阻塞；
- 本包不认证已被用户排除的AI算法；
- 本包不处理P1/P2、negative-link输入、OID51 merge/split、central render writeback、exact DAT不可达分支、
  T8、IL2CPP、Android或服务器。

