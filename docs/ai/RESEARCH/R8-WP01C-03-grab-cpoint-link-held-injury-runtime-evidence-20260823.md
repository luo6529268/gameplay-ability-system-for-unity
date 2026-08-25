# R8-WP01C-03 — grab / CPoint / link / held injury runtime evidence

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY PRODUCTION PLAY S4 ONLY`  
> Change ID：`R8-GRABPLAY-001`

## Scope conclusion

在当前国际版Unity 2022.3.62f3、`NTSD_Battle`、production world与dedicated worker启用配置下，
R8-WP01C-03要求的live joint matrix全部PASS。没有发现新的production first-difference，production代码0改动。

该结论只把R5-CPT-001～005、R5-LINK-001～002和相关scheduler边界推进到Unity S4；C++ executable/full
trace继续由R1-WP02 BLOCKED，不得把本文扩展为C++ runtime完整认证或整个战斗对齐。

## Authority

- `game_tick.cpp:1441-1643`：first negative-held；
- `cpoint.cpp:23-190`：prev-frame kind1 relation/decrease/action/throw/dircontrol；
- `weapon.cpp:13-107`：current-frame weapon-sync vaction/injury/global stat/position；
- `game_tick.cpp:1827-1846`：positive-link validation；
- `game_tick.cpp:1860-2018`：second negative-held；
- Makefile纳入上述live source；authority全程只读，未运行、构建、修改或写入。

## Final Play report

文件：`Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json`

- start/end tick：16→17；dedicated worker active；
- objects：4→4；claimed slots：2→2；object pool：2→2；reference pool：2→2；
- global KillStats/DamageStats恢复：true；cleanup：true；clean Play Console error：0。

### Valid grab / weapon-sync owner

- production kind3 grab accepted，catcher/victim reciprocal slots建立；
- first-held：duration=300、HP=20、stats=0，证明没有提前执行CPoint/injury；
- PreInteraction：duration 300→299，HP 20→-10，HPBound 100→90，victim combo=30；
- holder kill/combo=1/30，world kill/damage delta=1/30；
- expected/actual held position均116/19/201；FWC保持；
- positive-link和second-held后上述值不再变化，证明held injury/stat只在current weapon-sync执行一次。

### Reciprocal mismatch / throw tail

- active victim reciprocal mismatch；action/decrease跳过；
- catcher frame/prev2=110/110，victim frame/prev2=132/132；caught duration保持9；
- fallback frame0 geometry位置140/30；velocity=8/-4/-3；throw tail执行且FWC保持。

### Negative escape / dircontrol / postprocess

- valid relation duration 2 + decrease -5 → -3；catcher frame0、victim181；
- action跳过，dircontrol令catcher转right；
- immediate victim hit_count=1、knockback=4/-3；
- FramePostProcess后hit_count=0、runtime velocity=4/-3；FWC保持。

### Link residue

- invalid positive：holder link→0；TargetSlotIndex=3、HeldWeaponStableId=3、target reverse holder=-1、
  target link=-5均按positive-validation观察点保留；
- invalid negative：first-held child link→0但HolderStableId=4保留；second-held后仍0/4。

## Other verification

- source 12:22:01 < `Assembly-CSharp-Editor.dll` 12:22:18；C# compiler error=0；
- positive-link job `2e1446b473a64aef81ca80fd9b69d30d`：8/8 PASS；
- negative-link job `aa8d155711ac4ee5a9fc48862bf2fe42`：2/2 PASS；
- 首次组合class filter job `f75fa220c274452787c1ac109e02ae33`返回0 tests，作废且不作为证据；
- 首次clean Play行为断言PASS，但JSON在后续观察点读取已清字段；仅修probe取样时点并clean Play复跑；
- 2026-08-23 12:23:59 full `BattleRuntimeSelfCheck=PASS`；
- ledger validator：68 records / 67 governed code files PASS。

## Remaining boundaries

- C++ full trace / S5仍BLOCKED；
- 真实玩家手动抓取具体角色手感不是本通用fixture的覆盖范围；
- collision/hit/damage general consume归WP01C-04；
- death/respawn、random/late effect、synthesis、1000 entity、Player均未由本文关闭。
