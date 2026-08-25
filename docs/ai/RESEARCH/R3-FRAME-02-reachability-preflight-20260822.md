# R3-FRAME-02 — D-MOV-004 / D-MOV-005 reachability preflight

> 日期：2026-08-22  
> 类型：只读 C++ release source / Unity source / 当前 Unity DAT inventory。  
> C++ authority：`J:\QQFile\NTSD2.4\ntsd_release`；本调查未运行、修改、构建或写入该目录。

## 结论

| 差异 | 判定 | 后续动作 |
|---|---|---|
| `D-MOV-004` — Unity `ThrowFrameGuard` | **VERIFIED static extra gate，且 normal production nonnegative writer不存在。** C++ release source只有 field declaration/reset `-1`，没有任何 conditional read或非负 writer。 | 建立 `R3-FRAME-02A / R3-FRAME-002A-001`，只移除 Unity F03/F07 reader，并加 exact/shared fixture。 |
| `D-MOV-005` — state2000 facing | **INFERRED not reachable through current exact-character route。** C++ F07 state2000 facing行为存在；Unity fallback已经实现同一 facing。当前 `Assets/NTSD/Config` 内所有 literal `state: 2000` DAT均为 type2/type4 weapon，而 exact ECS FrameTick只接收 CLR exact `LF2Character` + current type0 character-DAT，故会落入 fallback。 | 本批不改脚本。保留 asset-reachability watch：若未来 type0 character DAT或 exact eligibility可达 state2000，须新建 Task Contract后处理。 |

## Authority evidence

### C++ release build participation

`Makefile:16-17,32` 将 `src/entity/physics.cpp`、`src/entity/frame_advance.cpp`、
`src/entity/game_tick.cpp` 编入 release source list。

### D-MOV-004

- `include/game_world.h:70,233` 定义和 reset `throw_frame_guard = -1`；
- `src/entity/game_tick.cpp:133-138`，`src/entity/weapon.cpp:117-131` 仅在 held release / invalidation
  相关路径写 `-1`；
- 对 release `src/` + `include/` 的 literal field inventory共有九处，五处是 `= -1`，零处是 nonnegative
  write，零处是 conditional read；
- `frame_advance.cpp:25-48` 的 F03 gates只有 delay、negative link、cpoint.kind2；
  `frame_advance.cpp:824-906` 的 late frame_tick path也没有该 field read。

因此任何 Unity `ThrowFrameGuard == Frame.N` early return都不是当前 C++ release source定义的 battle rule。

### D-MOV-005

- C++ `frame_advance.cpp:884-887`：late frame_tick在 state2000无条件写
  `facing = (vx > 0.0) ? 0 : 1`；该 source branch是 `VERIFIED`。
- Unity `LF2Entity.RunCommonFrameTick` 在 `LF2Entity.cs:5820-5821` 对
  `LF2States.HeavyWeaponInSky` 调用相同方向映射；`BattleEcsCharacterFrameTickPass.ExecuteExactCharacter`
  尚未包含这一 branch。
- 当前 Unity DAT inventory中 literal `state: 2000` 只出现在：
  - `Config/chars/weapon1.dat` — `data.txt` id150/type2；
  - `Config/chars/log.dat` — id151/type2；
  - `Config/chars/weapon10.dat` — id217/type2；
  - `Config/chars/weapon11.dat` — id218/type2；
  - `Config/chars/weapon9.dat` — id124/type4。
- `BattleEcsCharacterFrameTickPass.TryExecute` 只对 exact `LF2Character` 且
  `GetCurrentDataObjectTypeForSimulation() == Character (type0)` 接管；上述 type2/type4均走
  `SimFrameTick → RunCommonFrameTick` fallback。
- 既有 `BattleRuntimeSelfCheck` transformed heavy landing case已经断言“late state2000 face final vx”。

这里的“不存在 current exact reachability”来自项目的当前文本 DAT inventory与 gate crosswalk，因此标记为
`INFERRED`，不是 C++ runtime trace的替代物。

## Unity reader / writer crosswalk

| Unity location | 当前作用 | 处理 |
|---|---|---|
| `LF2Entity.TryEnterReleaseFrameAdvanceAfterDelay` | F03 先按 `ThrowFrameGuard == Frame.N` return。 | R3-FRAME-02A移除。 |
| `BattleEcsCharacterFrameTickPass.ExecuteExactCharacter` | exact F07 先按同一 guard return。 | R3-FRAME-02A移除。 |
| `LF2Entity.RunCommonFrameTick` | fallback/shared F07 先按同一 guard return。 | R3-FRAME-02A移除。 |
| `LF2CharacterWeaponLinkResolver`、`LF2WeaponHeldStateResolver`、`LF2WeaponReleaseFlowResolver`、`BattleHeldObjectWriter` | 都只清为 `-1`。 | 不改 writer；C++也有 cleanup/reset语义。 |
| `BattleRuntimeSelfCheck` | 仅 test fixture写29/31的非负值。 | 新 test可以显式使用该 test-only异常值证明不再成为 runtime gate。 |

## 约束

- 不能因 current asset reachability暂不可达而删除 C++ state2000规则或改变 fallback；
- 不修改 `D-MOV-005` 任何脚本；
- 不运行 C++ executable、不开 C++ build、不给 authority目录写 trace；
- `R3-FRAME-02A` 只处理 D-MOV-004，不能带入 held/link、weapon release、CPoint、opoint、R4或 renderer。
