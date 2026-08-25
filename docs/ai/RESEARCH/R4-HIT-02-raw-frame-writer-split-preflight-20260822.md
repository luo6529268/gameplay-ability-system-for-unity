# R4-HIT-02 — raw-frame writer split preflight

> 调查日期：2026-08-22  
> 状态：`VERIFIED source contract / implementation split planned`  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> C++ 边界：本调查只读源码；未运行、构建、修改、复制或向 authority 写入任何内容。

## 1. 结论

`D-HIT-002` 不能以“将所有 `ImmediateFrame` 替换为 raw writer”的方式一次完成。
C++ 的四组 direct `frame` 写入拥有不同的显式后续字段写入；Unity 的
`ImmediateFrame` / `SetFrameDirect` 会额外改写 `Frame.PN`、`AttackingCounter` 或 wait/next
同步状态。为避免把一个已知差异修成另一个差异，后续实施拆为四个最小包：

| 子包 | C++ authority | Unity 当前 writer | 最小目标 | 状态 |
|---|---|---|---|---|
| `R4-HIT-02A` | `collision.cpp:1193-1237` kind10/11 character | 两个 `ApplyFluteCharacterForce` | raw `frame=182` 不改 PN、attacking、wait counter | `RUNTIME_PENDING` |
| `R4-HIT-02B` | `hit.cpp:664-793` kind16 character | `BattleDamageWriter.ApplyKind16` | raw `frame=200`，仅保留 C++ 显式 `attacking=0` | `RUNTIME_PENDING` |
| `R4-HIT-02C` | `collision.cpp:583-632` normal weapon victim | `ApplyWeaponDamage` + `ApplyKind0WeaponVictimTail` | raw knockdown一处与tail四处均不附带攻击标记/wait重置 | `RUNTIME_PENDING` |
| `R4-HIT-02D` | `hit.cpp:342-361,465-482` normal weapon attacker | `ApplyWeaponDamage`内两个局部attacker writer | state3000 pre-knockdown raw write/skip-reset及state1002 later raw write的顺序与显式副作用 | `RUNTIME_PENDING` |

抓取、拾取、CPoint、held/link 和 opoint 的 raw-frame 不是本差异的同一 writer family，仍按
`R1-SOURCE-005` 交由各自的 future work package 关闭。

## 2. C++ live-source 证据

### 2.1 kind10 / kind11 character

`src/entity/collision.cpp:1193-1237`：当 victim `obj_type == 0` 时，case10/11依次写
`weapon_count=-20`、条件 combo/damage stat、速度阻尼，然后直接：

```cpp
vic_core_local.frame = 182;
```

紧接着只处理 Y/Vy。该 case 内没有 general frame transition、没有 `prev_frame` 写入、没有
`attacking` 清零、没有 wait-counter reset。故该组的 contract 是 **raw current-frame write**。

### 2.2 kind16 character

`src/entity/hit.cpp:664-793`：kind16的normal stat处理后直接 `victim_core.frame = 200`，并在下一句
显式 `victim_special.attacking = 0`。因此 raw frame和 attacking reset必须区分；不能让一个泛化helper
隐式完成二者。

### 2.3 normal weapon victim

`src/entity/collision.cpp:583-632`：normal kind0后，type1、type4/type6、type2武器分别直接写
`rand()%16`、`rand()%16`、`20`或`rand()%6`。这些写入没有通用 attacking reset。

### 2.4 normal weapon attacker

`src/entity/hit.cpp:342-361,465-482`：state3000在common victim knockdown前读取attacker当前frame，先按
`oid209 + non-character Karasu / oid209 frame40`例外决定是否skip，再raw写`frame=10`、显式
`attacking=0`、`vx=0`、`vz=frame10.dvz`（不写`vy`）；state1002在frame-delay/vrest/holder处理之后、
outer weapon-victim tail之前raw写random frame后才写`vx/vy`及type4 knockback。二者同样不能借一个会隐式写
PN / attacking的 helper合并，也不能颠倒state3000/state1002的C++检查时点。

## 3. Unity crosswalk

| Unity 位置 | 当前问题 | 归属 |
|---|---|---|
| `LF2CharacterHitResolver.ApplyFluteCharacterForce` | `ImmediateFrame(182)` 会写 `Frame.PN`、清 `AttackingCounter`、以 target wait 覆盖 current wait | `02A` |
| `LF2CharacterDatHitResolver.ApplyFluteCharacterForce` | 同上，shared character-DAT route | `02A` |
| `BattleDamageWriter.ApplyKind16` | `ImmediateFrame(MpDrain)` 额外改 PN；C++只有后续显式 attacking reset | `02B` |
| `BattleDamageWriter.ApplyKind0WeaponVictimTail` | `SetFrameDirect` 清 `AttackingCounter` | `02C` |
| `BattleDamageWriter.ApplyWeaponAttackerResponse` | 两个`ImmediateFrame`会写PN/清attacking/重置wait；state3000位置在generic victim write之后、且缺少oid209 skipReset | `02D` |

`LF2Entity.DirectWriteRawFramePreserveWaitCounter` 是已经存在的 Unity 适配 writer：它更新 Unity 的
`Frame.N`/`Runtime.Frame`/`Frame.D` mirror，以便后续 Unity frame data consumer可以读到新 frame；但不改
`Frame.PN`、不清 `AttackingCounter`，并保留 `Trans.WaitCounter`。这不是声称 Unity 内部结构等同 C++，而是
在 Unity 必须维护 data mirror 的前提下保留 C++ raw-write 的可观察副作用边界。

## 4. 02A 的独立验收合同

在 exact `LF2Character.Hit` 和 shared `LF2CharacterDatHitResolver.TryResolveHit` 两条 route中，kind10与kind11
均应：

1. 保持现有 `WeaponCount=-20`、条件 combo/damage stat、Vx/Vz阻尼、Y/Vy air step；
2. 将 current logical frame写为182；
3. 保留 fixture 预置的 `Frame.PN`、`AttackingCounter` 与 `Trans.WaitCounter`；
4. 不改 scheduler、candidate、RNG、ITR、DAT、C++、CPoint、held/link、weapon writer或 render。

## 5. 未关闭项

- C++ runtime trace仍受 `R1-WP02=BLOCKED` 限制；
- 真实Play Mode flute/技能行为未运行；
- raw writer之后的跨tick frame advance、motion、presentation、other type和任何 asset reachability均不在
  `02A` 验收内；
- 02D已取得独立Unity compile/full self-check证据，但C++ trace、真实Play Mode和跨tick presentation仍未关闭，故不证明完整weapon/R4对齐。

## 6. 02A 实施证据（2026-08-22）

- exact/shared两处callsite都已改为`DirectWriteRawFramePreserveWaitCounter(182)`；没有修改全局helper；
- `BattleRuntimeSelfCheck.CheckKind10And11CharacterStatsWithoutDamage` 现在执行 exact/shared × kind10/11
  四个组合，并断言current frame182、Frame.Data mirror、PN、attacking和wait counter；
- 现有 Unity `2022.3.62f3` / UnityMCP port 6401 scripts refresh后，filtered `error CS`返回0；
- 菜单 `NTSD/验证/运行战斗运行时自检` 运行后，`Temp/NTSD_BattleRuntimeSelfCheck.result` 于
  2026-08-22 05:43:54 +08:00 写入 `PASS`；
- full self-check后的四条 error-level console entries是两条MCP domain-reload disposed-connection提示和两条
  self-check故意验证runtime-rest绑定拒绝的negative control；没有C# compiler error或此fixture failure。

因此 `02A` 只提升为 `RUNTIME_PENDING`；C++ trace和真实Play Mode仍未取得。下一项是独立建立
`R4-HIT-02B` 的kind16 raw-frame合同，不能直接把02A证据扩展到它。
