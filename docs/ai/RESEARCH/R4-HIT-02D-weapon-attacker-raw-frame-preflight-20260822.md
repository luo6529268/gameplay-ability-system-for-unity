# R4-HIT-02D — normal weapon-attacker raw-frame / ordering preflight

> 调查日期：2026-08-22  
> 状态：`VERIFIED source contract / Unity compile+self-check PASS / runtime pending`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\hit.cpp:342-361,465-482`，且`hit.cpp`由release `Makefile:19`编译。  
> C++ 边界：仅只读源码；未运行、构建、修改、复制或向authority写入任何内容。

## C++ live-source contract

在normal kind0 common hurt中，两个attacker branch不是一个可交换的tail：

| C++顺序 | 条件 / 写入 | 明确未写入 |
|---|---|---|
| `hit.cpp:342-361` | generic victim knockdown前：state3000先检查；若非skip，raw `frame=10`，再`attacking=0`、`vx=0`、`vz=frame10.dvz` | 不写PN、wait、prev或`vy` |
| `hit.cpp:465-482` | frame-delay、arest/vrest、held-holder处理之后、outer weapon-victim tail之前：state1002 raw `frame=rand()%16`，再`vx=victim.knockback_vx*-0.5`、`vy=-4`；双方type4再写attacker knockback | 不清attacking、不写PN、wait、prev |

state3000 skipReset仅在victim current DAT不是Character、attacker current oid为209且victim current oid为
200/203/205/206/207/215/216，或victim current oid为209且**该检查时的frame为40**时成立。因为检查发生在
generic victim frame write之前，不能把它移到victim已经进入180/186或tail final frame之后。

## Unity current first differences

`BattleDamageWriter.ApplyWeaponDamage`目前在generic weapon-victim knockdown与arest/vrest处理完成后才调用
`ApplyWeaponAttackerResponse`。该helper：

1. 先以`ImmediateFrame(random16)`处理state1002；
2. 再以当前Unity frame重读state3000并`ImmediateFrame(10)`；
3. 没有weapon-victim的oid209 skipReset；
4. 两个`ImmediateFrame`都额外改变PN、attacking、wait/transistor。

其中state1002的位置大致正确，但state3000的检查过晚；`oid209 + victim oid209/frame40`会在当前Unity通用
weapon frame write后丢失frame40，属于可观察的source first-difference，而不是单纯helper副作用。

## 允许的最小实现方向

仅在`BattleDamageWriter`内拆分当前helper：

1. `state3000`专用writer插入generic victim knockdown之前，完整复制C++ skipReset条件，使用
   `DirectWriteRawFramePreserveWaitCounter(10)`并保留紧随其后的显式`AttackingCounter=0`、`Vx=0`和frame10 `Vz`；
2. `state1002`专用writer保留在arest/vrest/holder之后、weapon-victim tail之前，使用raw random16，不清
   attacking，并保留C++速度/type4 knockback；
3. 不改global helper、02C weapon-victim tail、vital/stat、candidate、CPoint/held/link、RNG engine、scheduler、input、AI、render、DAT或C++ authority。

## 必需的 focused fixture

- state1002：raw random16后保留PN/attacking/wait，验证Vx/Vy和type4-to-type4 knockback；
- state3000 normal：raw10后保留PN/wait、显式清attacking，Vx=0、Vz=frame10.dvz、Vy不变；
- state3000 order witness：将frame10设为state1002，验证C++的pre-state3000→later-state1002顺序；
- oid209 skip：Karasu oid与oid209/frame40两类均保留attacker状态，证明skip在victim generic frame写入前判定；
- 夹具必须走真实`LF2Weapon.Hit → ApplyWeaponDamage`，并锁定RNG总数（每个accepted hit已有两笔hit-record RNG）。

## 实施与验证证据（2026-08-22）

- `ApplyWeaponDamage`现在在generic victim knockdown前调用state3000 local writer，在arest/vrest/holder之后、
  weapon-victim tail之前调用state1002 local writer；未改变global tick/pass顺序；
- state3000 local writer复用`ResolveCurrentDataObjectId`与既有`IsKarasuOid`，只为non-character weapon victim
  执行C++ oid209 skipReset；其normal分支raw写10后保留显式attacking/Vx/Vz；
- state1002 local writerraw写random16后保留C++ Vx/Vy/type4 knockback，不隐式清attacking；
- `CheckWeaponAttackerRawFrameAndOrderingContract`的五类required category（含type4 state1002 subcase）随full
  `BattleRuntimeSelfCheck`运行：UnityMCP compile `error CS`=0，结果文件于2026-08-22 06:36:40 +08:00写入`PASS`；
- self-check后Console仅有两个既有rest-binding negative-control日志，未见C# compiler error或02D fixture失败。

## 未关闭 / 停止边界

- C++ runtime trace仍为`R1-WP02 BLOCKED`，真实Play Mode未做；
- 若实现需要改global writer、02C、weapon vital/stat、CPoint/held/link或任何scope外模块，停止02D；
- Play Mode与C++ trace仍未取得；本包状态只能为`RUNTIME_PENDING`，不得扩大为完整对齐。
