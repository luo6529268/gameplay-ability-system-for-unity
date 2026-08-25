# HANDOFF — R8-WP01G-R02 normal combat four-difference closure

> 日期：2026-08-23  
> 状态：`COMPLETE TO AVAILABLE EVIDENCE / RUNTIME PENDING`

## User authorization

用户明确要求先处理以下四项，再讨论后续工作：

1. `D-MOV-005` — state2000 facing；
2. `D-COL-005B` — non-character kind1 / pickup / generic grab；
3. `D-HIT-005` — current-DAT target dispatch；
4. `D-LIFE-001` — oid7/8 merge dormant、slot、split与cleanup。

## Scope decision

F1/F2 battle debug step、A→B→C unlock与其debug-only candidate-tail差异已由用户明确排除；见`D-015`。
`R2-CANDIDATE-TAIL-01`和`R3-STEP-01`不执行。

## Current action

按`R8-WP01G-R02-normal-combat-four-difference-closure.md`顺序执行四项source/reachability closure。
本包已按独立Change Record修改三项Unity脚本；C++始终只读。每项均先登记Ledger/STATE/本handoff，再执行
最小Unity修改，并保留不可取得的production Play/C++ trace边界。

### Active Change

`R8-MOV-005-001 / RUNTIME_PENDING`：exact character state2000 facing已补齐；fresh compile0、focused1/1、
16:14:46 self-check与74/74 validator PASS。正式type0 state2000 Play不可达、C++ trace BLOCKED。

`R8-COL-005B-001 / RUNTIME_PENDING`：generic key selector与weapon case1 grab已修；fresh compile0、
16:21:57 self-check及75/75 validator PASS。current DAT `itr kind1=0`，production Play不可得、C++ trace BLOCKED。

`R8-HIT-005-001 / RUNTIME_PENDING`：统一dispatcher、generic weapon/type3/type5 writer与mismatch matrix已写；
fresh compile0、第三次full self-check PASS、focused job `9411895645354ca4a241d2a84d8525a5`为178/178 PASS。
两次旧断言失败与source更正均已留痕；production mismatch Play不可得、C++ trace BLOCKED。

`R8-LIFE-001 / RUNTIME_PENDING / APPROVED UNITY ADAPTER`：C++ live merge/split、battle-time slot域与Unity
dormant/slot/reset/query/presentation crosswalk已重新闭合，未发现production差异、脚本修改0；focused
job `04ddfe7fa44b4f92beb0618d0f269a13`为32/32 PASS，同代码状态full self-check PASS并执行七组OID5152矩阵。

四项均已处理到当前可取得的最高证据层；按用户范围要求，本包在此停止，不自动进入后续D-ID。
global Change Ledger validator被任务外`WEB-CADENCE-001`的non-governed/unrecorded diff阻塞，本包未修改该工作。

## Protected boundaries

- C++ Release authority只读，不运行、构建、修改、复制或写入；
- 不改CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×、fixed camera与扩展容量；
- 不处理T8、IL2CPP、Android、服务器或四项以外D-ID；
- 任何完成结论必须区分source、compile、focused/self-check与Play证据。
