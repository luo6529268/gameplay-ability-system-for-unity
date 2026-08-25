# R8-WP01C-03 — grab / CPoint / link / held injury execution

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY PRODUCTION PLAY S4`  
> Change ID：`R8-GRABPLAY-001`

## Goal

在真实`NTSD_Battle` Play world和production pass中联合认证抓取双向关系、CPoint/WeaponSync、held injury/
global stats、escape/throw/dircontrol tail、positive/negative link residue，以及
`first-held → CPoint/weapon sync → positive-link validation → second-held`边界。

## Scope

- 新增一个Editor-only显式Play probe；
- 在production world idle边界注册通用source-derived character fixtures，不依赖角色/技能/OID特判；
- 使用真实`SimulationWorld.HeldObjectProcessAll`、`PreInteractionTickAll`、`ValidateHeldLinksAll`和
  `RunBattleEcsFramePostProcessPass`；
- 输出逐pass字段表与结构化JSON；
- 每个场景后注销probe实体并恢复world global KillStats/DamageStats、driver pause和池计数；
- 若发现first-difference，只登记通用repair，不在本认证包修改production gameplay。

## Authority / Evidence

- C++ Release只读source：
  - `game_tick.cpp:1441-1643` first negative-held；
  - `cpoint.cpp:23-190` prev-frame kind1 relation/decrease/action/throw/dircontrol；
  - `weapon.cpp:13-107` current-frame weapon-sync vaction/injury/stat/position；
  - `game_tick.cpp:1827-1846` positive-link validation；
  - `game_tick.cpp:1860-2018` second negative-held；
  - Makefile纳入`game_tick.cpp`、`cpoint.cpp`、`weapon.cpp`；
- Unity existing repairs：`R5-CPT-001～005`、`R5-LINK-001～002`均为`RUNTIME_PENDING`，已有source/
  compile/self-check但缺真实Play joint S4；
- 前置`R8-WP01C-01/02`均已在各自S4范围`VERIFIED`。

## Required matrix

1. **valid grab + weapon-sync injury/stat owner**：valid reciprocal kind1/kind2；first-held无damage；
   PreInteraction一次性完成decrease、vaction、lethal injury、holder-local kill/combo与world kill/damage stats、
   held position；positive validation/second-held不得重复伤害；raw frame保持FWC。
2. **reciprocal mismatch + throw tail**：active victim reciprocal不匹配时，action/decrease跳过，但frame0 fallback
   geometry/next、vaction/prev2、velocity继续执行；missing victim immediate return不混入本case。
3. **valid duration-negative escape + dircontrol**：negative duration写attacker0/victim181、hit/knockback，跳过
   actions但继续dircontrol；随后FramePostProcess消费hit count。
4. **invalid positive link residue**：只清holder LinkState，保留TargetSlotIndex/HeldWeaponStableId和target reverse字段。
5. **invalid negative link residue**：first-held只清child LinkState，保留HolderStableId；second-held不得再次扩大清理。
6. **pass boundary table**：至少为正向grab记录first-held、PreInteraction、positive-link、second-held四个观察点。

## Deliverables

1. `Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json`；
2. 正向、mismatch、escape、positive/negative residue与pass boundary evidence rows；
3. cleanup/pause/global-stat restore；
4. fresh compile、focused tests、full self-check、validator结果；
5. persistent runtime evidence与D-ID状态更新。

## Verification

1. fresh Unity compile 0 error；
2. existing CPoint/link focused Editor suites PASS；
3. explicit clean Play probe全部matrix PASS，Console无非预期error；
4. world object/claimed、object/reference pool、global stats和pause状态恢复；
5. full `BattleRuntimeSelfCheck`与ledger validator PASS。

## Stop conditions

- 任一matrix出现production first-difference；
- 需要修改`BattleCpointWriter`、link/held writer、scheduler、DAT/scene或已批准adapter；
- 需要角色/技能/OID专项分支；
- 需要运行、构建、修改或写入C++ authority；
- 02的对象生命周期前置回归失败。

命中后只保存最短witness、建立独立production repair Change Record并停止相应场景；不得在probe中修行为。

## Out of scope

- collision/hit/damage通用消费（WP01C-04）；
- death/respawn（05）、random/late effect（06）、synthesis（07）；
- render图片/SceneView（WP01D）、1000实体（E）、Player（F）；
- C++ full trace、T8、Android、服务器。

## Authorization

用户于2026-08-23明确回复：`批准执行 R8-WP01C-03，恢复目标`。

## Result

- fresh compile 0 error；
- positive-link focused 8/8、negative-link focused 2/2；
- final clean Play在worker active配置下required matrix全部PASS；
- objects/claimed/object pool/reference pool/global stats/pause全部恢复，Play Console 0 error；
- 12:23:59 full self-check PASS；
- persistent evidence：`RESEARCH/R8-WP01C-03-grab-cpoint-link-held-injury-runtime-evidence-20260823.md`；
- production gameplay、scheduler、DAT/scene、render与C++均0改动。

本Task只关闭03的Unity S4；C++ full trace和WP01C-04～07继续独立。
