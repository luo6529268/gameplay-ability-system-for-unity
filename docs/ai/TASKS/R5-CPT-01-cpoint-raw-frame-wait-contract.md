# R5-CPT-01 — CPoint raw frame / wait-state preservation contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 已完成合同内 writer replacement 与 Unity code-level validation；C++ trace / Play Mode 待验。  
> 对应差异：`D-CPT-001`  
> Change ID：`R5-CPT-001`  
> Authority：`J:/QQFile/NTSD2.4/ntsd_release` 的 `src/entity/cpoint.cpp:35-124`、
> `src/entity/weapon.cpp:42-48`、`src/entity/game_tick.cpp:659-664`。

## Goal

使 Unity CPoint relation fallback、decrease escape、action selection 与 held current-frame vaction 的
frame write 不再额外清 `Runtime.FrameWaitCounter`；保留 C++ source 所定义的 frame、facing、attacking、
position、velocity、hit 与 link 结果。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

允许的 production change 仅为把下列七个 CPoint callsite 改用已有、支持 missing raw frame且不清 FWC 的
direct writer：

1. broken caught slot frame0；
2. reciprocal/kind2 failure frame0；
3. decrease escape attacker frame0；
4. decrease escape victim frame181；
5. signed action attacker frame；
6. action victim vaction；
7. held current-frame victim vaction。

## Required behavior

1. 每个指定 CPoint frame write 保持调用前的 nonzero `Runtime.FrameWaitCounter`；
2. frame id、负 action 的 facing flip、合法 missing positive frame、`Frame.D`、Trans wait mirror 与 collision
   `Prev2` 保持各自原有合同；
3. action selection 继续只显式清双方 `AttackingCounter`；
4. decrease escape 继续写 hit counter 与 knockback velocity，且仍在 throw / dircontrol 前结束；
5. held vaction=0 继续不换 frame，vaction=-131 的 raw negative-frame/facing 语义保持；
6. 不改 CPoint injury global stat（`D-CPT-002`）、reciprocal mismatch control flow（`D-CPT-003`）、
   cpoint/weapon-sync pass order、kind2 validation、throw、held/link、opoint、input、collision、render、
   slot/generation、ECS capacity、DAT、scene 或 C++ authority。

## Authority / Evidence

| 证据 | 等级 | 用途 |
|---|---|---|
| C++ `Makefile:20-21` | VERIFIED | cpoint / weapon source 参与 release build。 |
| C++ `game_tick.cpp:659-664` | VERIFIED | step10 CPoint → weapon sync 调用顺序。 |
| C++ `cpoint.cpp:35-124`、`weapon.cpp:42-48` | VERIFIED | 指定分支只写 frame / explicit fields，没有 wait write。 |
| Unity `BattleCpointWriter` / `LF2Entity` helper | VERIFIED | immediate helper会清 FWC；CPoint专用helper会拒绝missing positive frame，raw direct helper保留FWC且匹配旧writer的missing-frame边界。 |
| C++ runtime trace / same-scene Play Mode | BLOCKED / PENDING | 不是本包完成前可声称取得的证据。 |

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | 重读上述 C++ 分支、Makefile、Unity seven callsite 与 raw helper。 |
| S1 focused fixture | 扩展 existing CPoint self-check matrix，用 FWC sentinel 验证 seven callsite；保留非本包字段断言。 |
| S2 governance | 更新 Change Record、ledger、STATE、full diff、主计划和 handoff；运行 `Tools/Validate-ChangeLedger.ps1` 与本包 scoped diff check。 |
| S3 Unity | 当前已打开 Unity 的 scripts refresh 后 `error CS=0`；执行 full `BattleRuntimeSelfCheck` 并读取结果文件。 |
| S4 honesty | 最高为 `RUNTIME_PENDING`；C++ trace、joint first-difference 与 Play Mode 不得伪称完成。 |

## Stop conditions

- 需要改已有 raw helper、`LF2Entity`、CPoint pass order、injury stats、mismatch early return、throw、held/link、
  input、collision、render 或任何未列出文件；
- source 发现 C++ 指定分支实际还写了 wait state；
- Unity compile / fixture / full self-check 失败且无法在本合同范围内修复；
- 需要运行、修改、构建、复制或向 C++ authority 写入任何内容。

发生任一项时，停止本包、记录证据，并为新差异建立独立合同。
