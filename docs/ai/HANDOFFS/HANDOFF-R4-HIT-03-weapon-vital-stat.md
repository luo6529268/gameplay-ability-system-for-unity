# HANDOFF — R4-HIT-03 normal weapon vital/stat and raw durability

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-003`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改、复制或写入C++ authority。

## 已确认的 C++ / Unity first difference

- type1/type2/type4的normal kind0 route必须用FallDamageDiv调整的injury写HP/HPBound/ComboCountVic/DamageStats，
  随后才以raw injury写WeaponFlightCounter；
- type6调用reaction，不写vital/stat，但仍按raw injury改durability；
- weapon victim即使lethal也不属于type0，所以不能写KillStats、holder KillStat或holder ComboCountAtk；
- Unity当前只写durability/reaction字段，遗漏上述type1/2/4 vital/stat；
- Unity的early HitConfirm2/RelationTeam又是独立`D-HIT-004`，本包不触碰。

## 已写入的最小改动

- `ApplyWeaponDamage`现依C++相对顺序调用damage effect、type1/2/4专用vital/stat、raw durability；
- `ApplyWeaponNormalVitalAndStatWrites`按FallDamageDiv调整HP/HPBound/Combo/DamageStats（只限index1/2），不写type0-only
  kill/holder score；type6不进入该helper；
- 新增真实`LF2Weapon.Hit`的type1/2/4 scaled nonlethal、type2 lethal with holder、type4 bdefend100、type6 reaction fixture。
- UnityMCP刷新脚本后`error CS`=0；full `BattleRuntimeSelfCheck`在2026-08-22 06:50:08 +08:00写入`PASS`；
- self-check后的全量Console读取只保留两个既有rest-binding negative-control error-level日志。一次后续filter MCP socket
  重连错误为tool transport，并非Unity compiler或fixture失败。

## 禁止扩大 / 未验证

- Play Mode尚未运行；
- 不改type0/type3 writer、D-HIT-004、global helper、frame/response、CPoint/held/link、RNG、candidate/scheduler/input/AI/render/DAT；
- C++ trace仍BLOCKED，真实Play Mode仍待补；`RUNTIME_PENDING`不表示任何完整对齐结论。
