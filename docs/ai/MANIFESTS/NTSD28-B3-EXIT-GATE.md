# NTSD 2.8-Logan B3 exit gate

> Change ID：`NTSD28-B3-EXIT-GATE-AUDIT-001`  
> 状态：`VERIFIED / B3_PLACEMENT_EXIT_READY / FULL_CLOSE_DEFERRED`

## 1. 已满足的进入B4条件

- current Authority 57-checkpoint immutable contract已重建。
- C00～C24 production placement、双次clamp/refill、type0/drop/non-type0顺序、C23/C24 clock已具名接入。
- C25为C24后的单一dynamic ascending live-slot入口；C25a/b、f/h/i/j、g scoped core、l/m scoped order、p owner已进入同一slot transaction。
- normal presentation从completed world生成，不再早于C25/session tails/results。

## 2. 阻止B3完全关闭的残留

| 生产残留 | 当前实际职责 | 下游owner/删除门 |
|---|---|---|
| C25后`SerialTickAll` | `LF2SpecialAttack.RunPostNativePhysicsSerialForWorldPass`的state entry、state15、death；runtime snapshot；global state9998 cleanup | B4 frame/state、B5 interaction、B7 lifecycle接管后删除caller；再做pass trace |
| `HandleFrameTickExit`在C25k前 | 11xx/12xx reset及terminal free过早，无formal pending/code | B7 terminal carrier+C25g producer+C25k/o联合 |
| C25n缺weapon pieces | broken sound/pending-only | B7+B10+B11/H |
| `EntityPostFrameTailAll` | F7 full stats、HitConfirm2/transient cleanup、snapshot | B8/session与独立cleanup owner接管后缩减或删除 |
| `Mode2RandomWeaponDropTailAll` | 当前随机掉武器 | 用户批准例外；保持隔离并披露 |

## 3. 行为包路由

- B4：frame motion、physics、bounds、special state entry/state15及C25g剩余frame语义。
- B5：候选、命中、伤害及special state interaction依赖。
- B7：spawn、terminal pending/lifecycle、C25k/n/o、state9998 cleanup。
- B8：stage/session/function-key与global post-tail剩余owner。
- B10：声音；B11/H：DAT schema/content/resource；B12：full trace/长时/展示campaign。

## 4. 状态口径

`B3_PLACEMENT_EXIT_READY`只授权依赖顺序进入B4；在临时serial/global残留删除且full pass trace无首差前，禁止写`B3_ALIGNED`或`B3_COMPLETE`。
