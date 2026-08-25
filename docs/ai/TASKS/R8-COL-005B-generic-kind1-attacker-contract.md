# R8-COL-005B — generic kind1 attacker and weapon dispatch contract

> 日期：2026-08-23  
> 状态：`IN_PROGRESS`  
> D-ID：`D-COL-005B`  
> Change ID：`R8-COL-005B-001`

## Goal

按C++ Release统一Entity合同修正Unity non-character kind1路径：selector从通用runtime key读取，weapon
attacker的kind1消费进入generic grab writer；pickup继续只属于kind2/7。不得加入角色、OID或技能特判。

## Scope

- `BruteForceSceneQuery`：kind1 left/right读取通用`LF2Entity.Runtime`；
- `LF2WeaponInteractionResolver`：case1调用`BattleInteractionWriter.TryApplyGrab(..., kind:1)`；
- `BattleRuntimeSelfCheck`：新增真实weapon CLR attacker→character target的collect→object consume矩阵；
- 记录当前全部已部署DAT的结构化扫描结果：`itr kind1 = 0`，故current asset route不可达但代码合同仍需闭合。

## Authority / Evidence

- C++ `collision_collect.cpp:200-220`直接读取通用Entity `key_right/key_left`，没有CLR/type gate；
- C++ `collision.cpp:921-993` case1是generic Entity grab；
- C++ pickup位于case2/7；
- Unity selector仅接受CLR `LF2Character`，weapon case1错误进入pickup helper；
- Unity全DAT block-aware inventory确认`itr kind1=0`，所有文本`kind:1`均为其他block或非ITR。

## Files likely involved

- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponInteractionResolver.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- Task/Change/Ledger/STATE/register/plan/handoff

## Deliverables

1. generic runtime-key selector；
2. weapon-attacker kind1 generic grab dispatch；
3. collect→object consume→frame/link/duration/fall矩阵；
4. compile、full self-check、ledger/diff证据；
5. current asset不可达与future/mod DAT合同边界。

## Verification

- actual weapon CLR attacker携带manual runtime KeyRight时可按C++ selector收集injured2 target；
- consume后attacker/victim raw frame、caught/catcher、duration、fall与generic writer一致；
- target不被错误当作picker、weapon不进入held/pickup关系；
- current existing character-attacker/non-character-target kind1和kind3矩阵继续通过；
- compile0、full self-check、validator/diff PASS。

## Stop conditions

- 修复需要改kind2/7、held/pool、input producer、candidate order或pass order；
- generic writer缺少C++字段而必须扩大到D-HIT/D-LIFE；
- current DAT结构化扫描发现正式itr kind1且真实行为需要独立Play场景。

## Out of scope

kind2/7重构、held/pickup完整重写、D-HIT-005、D-LIFE-001、F1/F2 debug、C++ executable。

