# R4-HIT-03 — normal weapon vital/stat versus raw durability preflight

> 调查日期：2026-08-22  
> 状态：`VERIFIED source contract / Unity compile+self-check PASS / runtime pending`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:559-585`、`src\entity\hit.cpp:107-167`；二者由release `Makefile:19,22`编译。  
> C++ 边界：仅只读源码；未运行、构建、修改、复制或向authority写入任何内容。

## C++ live-source contract

normal kind0的weapon-like victim首先由`collision.cpp`决定调用方式：

| current DAT type | C++ entry | vital/stat | `unk_31C` durability |
|---|---|---|---|
| type1 / type2 / type4 | `apply_hurt(..., apply_damage=true)` | 依`FallDamageDiv`缩放后写HP、HP max、victim combo、DamageStats | 紧随vital/stat后使用**raw**`itr.injury`扣减；`bdefend==100`改为`-1` |
| type6 | `apply_hurt_reaction(..., apply_damage=false)` | 不写上述vital/stat | 同样使用raw `itr.injury`与`bdefend==100`规则 |

对于type1/2/4，`hit.cpp:111-155`的精确合同为：

1. `adjusted = rawInjury * 100 / FallDamageDiv`，当`FallDamageDiv>0`，否则rawInjury；
2. `HP -= adjusted`、`HPBound -= adjusted / 3`、`ComboCountVic += adjusted`；不钳制负值；
3. `DamageStats[Unk344] += adjusted`只在`Unk344`为1或2时发生；
4. kill stat与holder `ComboCountAtk`只对type0 victim，**不能**带到type1/2/4；
5. 之后`unk_31C`（Unity映射为`Runtime.WeaponFlightCounter`）只减rawInjury，并保留`bdefend==100 → -1`。

## Unity current first difference

`BattleDamageWriter.ApplyWeaponDamage`目前对所有damageable weapon先写`HitConfirm2`与
`Runtime.WeaponFlightCounter`，但不写Health/HPBound/ComboCountVic/DamageStats，也没有对vital路径应用
`FallDamageDiv`。这导致type1/2/4遗漏完整vital/stat子合同；type6的“只耐久、不vital”反而必须保持。

另发现Unity将`HitConfirm2`与`RelationTeam`提前写在common weapon writer，而C++在`apply_hurt`返回后由
`collision.cpp:587-632`的type tail写入。这是独立的时序差异，已登记为`D-HIT-004`，不在本包修改。

## 允许的最小实现方向

1. 仅在`ApplyWeaponDamage`中，将normal type1/2/4的专用vital/stat helper放在C++相对位置：damage effect之后、
   raw durability之前；
2. helper独立计算adjusted injury，不复用含type0-only kill/holder score的standard helper；
3. type6不进入该helper；`WeaponFlightCounter`仍用raw injury；
4. 不动现有HitConfirm2/RelationTeam时序（移交`D-HIT-004`）、raw frame、CPoint/held/link、RNG、candidate、scheduler、input、AI、render、DAT或C++ authority。

## 必需 fixture

- type1/type2/type4 nonlethal：scaled vital/stat与raw durability同时正确；
- type2 lethal with active holder：HP可到0，但不写KillStats、holder KillStat或holder ComboCountAtk；
- type4 `bdefend==100`：vital仍正常、durability为-1；
- type6 reaction control：vital/stat均不变、durability仍按raw injury处理；
- 所有夹具走真实`LF2Weapon.Hit → ApplyWeaponDamage`，而非私有helper反射。

## 实施与验证证据（2026-08-22）

- `ApplyWeaponDamage`现按C++相对顺序执行damage effect、type1/2/4专用scaled vital/stat、raw durability；
  type6仍跳过vital helper；
- `ApplyWeaponNormalVitalAndStatWrites`仅写HP、HPBound、ComboCountVic与DamageStats 1/2，不调用任何type0
  kill/holder score路径；
- `CheckWeaponVitalAndDurabilityContract`的type1/2/4 nonlethal、type2 lethal with holder、type4 bdefend100、
  type6 reaction矩阵随full self-check执行；UnityMCP compile `error CS`=0，结果文件于2026-08-22 06:50:08 +08:00写入`PASS`；
- self-check后Console读取到的仅是两个既有rest-binding negative-control日志。一次后续按文本过滤的MCP socket查询发生了
  瞬时transport错误，但不影响已写入的`PASS`、compile查询或成功的全量Console读取。

## 未关闭 / 停止边界

- C++ runtime trace仍`R1-WP02 BLOCKED`，真实Play Mode未做；
- 若实现要求改type0 standard writer、D-HIT-004、global helper、CPoint/held/link、RNG或scope外模块，停止本包；
- Play Mode与C++ trace仍未取得；本包状态只能为`RUNTIME_PENDING`，不得扩大为完整对齐。
