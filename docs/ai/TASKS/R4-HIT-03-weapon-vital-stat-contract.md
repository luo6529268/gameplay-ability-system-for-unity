# R4-HIT-03 — normal weapon vital/stat and raw durability contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — Unity compile与full self-check已通过；C++ trace / Play Mode未关闭。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:559-585`、`src\entity\hit.cpp:107-167`。  
> 对应差异：`D-HIT-003`；关联 Change ID：`R4-HIT-003`。

## Goal

补齐normal kind0 type1/type2/type4 weapon victim的C++ vital/stat子合同，同时保持raw durability和type6 reaction例外；
不得把type0-only score写入weapon。

## Scope

仅允许修改`Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs`的`ApplyWeaponDamage`及其专用vital/stat helper，
以及`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`中的focused fixture。

禁止修改type0 standard/type3 writer、D-HIT-004 hit-confirm/relation时序、global helper、raw frame、weapon
attacker/victim response、CPoint/held/link、RNG、candidate、scheduler、input、AI、render、DAT、C++ authority或其他R4包。

## Required behavior

1. type1/type2/type4仅：`FallDamageDiv`调整的HP、HPBound、ComboCountVic、DamageStats（索引1/2）在raw durability前写入；
2. `WeaponFlightCounter`对type1/type2/type4/type6仍使用raw injury；`bdefend==100`仍为-1；
3. type6不得写normal vital/stat；
4. weapon lethal不得写world KillStats、holder KillStat或holder ComboCountAtk；
5. 不新增RNG/alloc，不改变02C/02D的frame/RNG/response合同。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | 只读复核C++ `collision.cpp:559-585`、`hit.cpp:107-167`及Makefile参与性。 |
| S1 | 真实weapon hit的type1/2/4 nonlethal、type2 lethal、type4 bdefend100、type6 reaction fixture。 |
| S2 | Unity compile=0 error、full self-check PASS、ledger/diff check通过。 |
| S3 | 最高`RUNTIME_PENDING`；C++ trace / Play Mode仍待补。 |

## 本次实际验证

- S0：`PASS`，只读复核C++ `collision.cpp:559-585`、`hit.cpp:107-167`及release `Makefile:19,22`；未运行、构建、复制或写入authority；
- S1：`PASS`，新增真实`LF2Weapon.Hit → ApplyWeaponDamage`fixture覆盖type1/2/4 scaled nonlethal、type2 lethal with holder、
  type4 bdefend100与type6 reaction control；
- S2：`PASS`，已打开Unity Editor通过UnityMCP刷新脚本后`error CS`=0；
  `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 06:50:08 +08:00为`PASS`；
  `Tools/Validate-ChangeLedger.ps1`与`git diff --check`在写后审计中通过（后者仅既有CRLF提示）；
- Console：full self-check后仅有两条既有rest-binding negative-control日志；一次后续filter查询的MCP socket重连错误已作为
  tool transport记录，不是Unity compiler或fixture错误。

## Stop conditions / Out of scope

若需改type0/type3 standard writer、D-HIT-004、global helper、CPoint/held/link、RNG engine或scope外模块，停止并记录；
R5～R8、T8、C++ executable、Unity Play Mode、性能和服务器不在范围内。
