# R4-HIT-05 — current-DAT target dispatch versus CLR shell preflight

> 日期：2026-08-22  
> 状态：`INFERRED / NO GAMEPLAY CHANGE`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；C++目录只读。

## 问题

C++ release 的 Entity 是统一运行时对象，collision/hurt 分支读取当前 `char_data->obj_type`，不具备 Unity 的
`LF2Weapon` / `LF2SpecialAttack` CLR subclass 概念。Unity 大多数 target dispatch 已先按 current DAT type判断，
但两个路径仍在 character target 分支以外按 CLR 壳优先分发：

- `LF2CharacterDatInteractionResolver.DispatchInteractionByKind:95-110`；
- `LF2SpecialAttack.TryApplyHit:532-557`。

若 target 是 CLR `LF2WeaponBase`、但当前 DAT 已变为 type3 或 type5，则它们可直接调用 `weapon.Hit` →
`ApplyWeaponDamage`，而不是按 C++ 当前 `char_data->obj_type` 的 type3/type5 route处理。

## C++ source contract

- `collision.cpp:559-632` 在 normal hurt 后依据 `vic_core.char_data->obj_type` 进入type1/type4/6/type2/type3
  tail；
- `hit.cpp:86,181,200,206,232,235,278,368,479`反复从当前 `char_data->obj_type` 决定damage、fall、sound、
  knockback和attacker response；
- `game_tick.cpp:1227-1243` 的live path直接替换统一Entity的`char_data`并传播给子对象，说明运行时DAT身份不是
  CLR 壳的固定属性；
- `Makefile`已列入上述 `game_tick.cpp`、`collision.cpp`、`hit.cpp`。

## Unity crosswalk

正确的 current-DAT-first target dispatch 已存在于：

- `LF2CharacterInteractionResolver:67-92`；
- `LF2WeaponBase.TryApplyHit:642-670`。

风险路径则为：

- shared Character-DAT attacker 的 `LF2CharacterDatInteractionResolver:98-110`：type0先特判后，无论target DAT
  type如何，先以 CLR `LF2WeaponBase` / `LF2SpecialAttack` 分支处理；
- `LF2SpecialAttack.TryApplyHit:536-552` 同样先处理type0，之后按 CLR weapon/special branch；
- Unity `LF2Entity.ApplyStateDataTransform:4309-4325` 和 `TryApplyRuntimeIdentity:4358-4377` 可以在不改变CLR壳的
  前提下载入新的 wrapper；现有self-check也明确构造过“weapon CLR shell + current type3 DAT”的物理路径
  (`BattleRuntimeSelfCheck.cs:24720-24759`)。

## 证据边界

- **VERIFIED（source）**：C++ target behavior由当前DAT type决定；Unity存在两个CLR-first dispatch路径，且工程
  支持 CLR shell/current-DAT 不一致；
- **INFERRED**：若武器CLR壳在实际战斗中经identity/state/child propagation成为type3/type5，当前dispatch将走错
  writer；
- **UNKNOWN**：正式NTSD_Battle资产中是否存在可达的“weapon CLR shell → non-weapon current DAT → attack candidate”
  sequence，以及其最小输入/slot witness；没有C++ runtime trace，不得假称已发生；
- **NO GAMEPLAY CHANGE**：不能仅给`weapon.Hit`加type gate。那会让type3 current-DAT weapon shell失去正确的
  generic type3 writer；正确修复需要一个按current DAT type路由、可接收通用`LF2Entity` target的damage adapter，
  涉及至少shared character interaction、special attack interaction与damage-writer contract，已超出R4-HIT-004。

## 后续必要条件

如要实施，必须另建独立 Work Package，先回答：

1. type3/type5 current-DAT shell的generic target damage/response writer应复用或拆出哪些 C++ fields；
2. 所有 CLR attacker（character/shared character/weapon/special）对同一 target current DAT的统一route；
3. 变换、candidate、lifecycle和render identity的联合fixture；
4. production asset reachability与target Play Mode证据。

在这些条件闭合前，保持 `INFERRED / no gameplay change`，不把它混入R4-HIT-004或R5。
