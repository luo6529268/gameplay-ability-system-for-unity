# R4-HIT-02C — normal weapon-victim raw-frame preflight

> 调查日期：2026-08-22  
> 状态：`VERIFIED source contract / task split pending`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:583-632`。  
> C++ 边界：只读；未运行、构建、修改、复制或写入 authority。

## C++ contract

normal kind0 hit的weapon victim tail在`collision.cpp:583-632`于normal/reaction writer之后执行：

| target DAT type | C++ raw-frame write | 相关同段副作用 |
|---|---|---|
| type1 | `frame = rand()%16` | `hit_confirm2=1`、`unk_364=attacker.unk_364` |
| type4/type6 | `frame = rand()%16` | 先写self vrest=30、`hit_confirm2=1`、`unk_364` copy |
| type2 | ground/low-fall/effect!=4时`frame=20`，否则`frame=rand()%6` | `hit_confirm2=1`、relation-dependent vrest、facing copy、`unk_364` copy |

这四种frame写入都没有通用attacking清零、PN写入或wait reset。type1/type4/type6各一次`rand()%16`；type2只有
random branch才一次`rand()%6`，frame20 branch不消耗该随机数。

## Unity crosswalk / confirmed difference

`BattleDamageWriter.ApplyKind0WeaponVictimTail`对应三族target，但四个callsite均使用
`LF2WeaponBase.SetFrameDirect`。更关键的是，`ApplyWeaponDamage` 的knockdown branch先使用另一处
`SetFrameDirect(hitFrame)`，然后所有damageable weapon才进入上述tail。C++ `apply_hurt` 对 knockback也先
raw-write 180/186，随后weapon-tail再raw-write final frame；因此这个**两次frame写入的顺序必须保留**，但Unity
两处writer都不能继续隐式清`AttackingCounter`或以目标frame覆盖wait。Unity的`RelationTeam=attacker.RelationTeam`
是现有`unk_364` adapter，不在本包改动。

## Planned split before code

`R4-HIT-02C` 已建立独立Task Contract/Change Record。它必须覆盖同一normal weapon-writer family的
**五处**frame callsite（knockdown前写 + tail四处），fixture必须区分：

1. light(type1)与throw/drink(type4/type6)先raw knockdown frame、后raw random16：PN、attacking、wait保留；随机数恰好一次；现有
   vrest/hit-confirm/team contract不变；
2. heavy(type2)先raw knockdown frame、后ground frame20：零frame-random；PN/attacking/wait保留；
3. heavy(type2)先raw knockdown frame、后airborne/effect4 random6：恰好一次random6，且同样保留raw-frame side effects。

不允许改`SetFrameDirect`全局实现、attacker tail、kind10/11、kind16、weapon vital/stat、CPoint/held/link、
candidate、RNG engine、scheduler或render。

## Current state

`R4-HIT-02A/B`的character writers均已`RUNTIME_PENDING`；02C已建立Change Record、未改脚本。新发现的two-write
contract已写入本调查；下一步只可在此基础上完成focused fixture与五处raw-writer最小替换。
