# R8-WP01G-R06 — G2 input residual preflight

> 日期：2026-08-23  
> 类型：只读source/reachability审计  
> D-ID：`D-INP-001`、`D-INP-004`

## 1. 结论

G2不能作为一个实现包直接执行，必须拆分：

1. `D-INP-001`不再是当前非AI Play backlog；
2. `D-INP-004`发现新的source-confirmed physical-source差异，应成为下一独立实施包。

本次没有修改任何脚本、InputAction asset、场景或C++ authority。

## 2. D-INP-001 negative-link input reachability

### 已确认事实

- C++ `main.cpp`在`world.game_tick > 1`后对所有active current character DAT调用`apply_input`，caller不按
  `link_state < 0`整段跳过；
- Unity原有整体negative-link input return已由`R3-HOLD-INP-001`移除，self-check证明手工构造的
  negative-link current character DAT仍能进入direct input resolve；
- C++正式pickup writer只为current DAT type 1/2/4/6写negative link；这些对象不属于current character DAT
  input participant；
- C++ opoint kind2可以生成current type0 child并写`link_state=-1`，但同一正式writer会对type0 child明确写
  `ai_controlled=true`；Unity factory保持同类parent/child relation。

### 当前范围决定

自然可达的`negative-link + current character DAT`分支属于AI-controlled opoint child。用户已明确把C++ AI
sensing/decision/action parity移出当前对齐backlog，未来改用Unity状态树或行为树。因此：

- 不再为`D-INP-001`新增非AI physical Play探针；
- 不伪造一个玩家角色的negative-link关系来制造S4；
- 现有source-correct eligibility代码和self-check保留，不回退；
- 若未来Unity AI实现需要negative-link child输入，仍必须通过canonical `FrameInputSet`/固定tick边界进入，
  但不要求复刻C++ AI决策。

状态建议：`SOURCE-CLOSED / NATURAL TYPE0 PATH IS AI-DEFERRED / NO NON-AI PLAY BACKLOG`。

## 3. D-INP-004 P1/P2 physical source first difference

### C++ Release authority

- `main.cpp:2379-2380`固定P1/P2为runtime slot0/1；
- `main.cpp:4607-4608`按P1后P2顺序poll；
- `include/input_handler.h:15-17`：
  - P1：D/A/W/S + L/J/K；
  - P2：方向键 + numpad3/numpad1/numpad2；
- `InputHandler::poll`把cfg.attack/jump/defend分别写key_attack/key_jump/key_defend。

### Unity current state

- `SimulationFrameInputModule`和既有self-check已证明：一旦canonical packet存在，roster player slot0/1可稳定
  路由到runtime slot0/1，不串键；
- `CharacterInputModule`会从`Player_<inputId>` action map查找`Move/Attack/Jump/Defend`；
- `NTSDInputConfig.inputactions`的`Player_1`具备四个action；
- `Player_2`只有`Move`和方向键binding，没有`Attack`、`Jump`、`Defend` action或numpad binding；
- 因此P2的三个action lookup均为null，正式InputSystem无法产生P2攻击、跳跃、防御或组合键packet。

这是生产physical source差异。旧`R3-INP-004-001`手工构造`FrameInputSet`只证明packet之后的routing，不能
覆盖缺失的packet producer。

状态：`SOURCE-CONFIRMED DIFFERENCE / UNFIXED`。

## 4. 推荐下一包

`R8-WP01G-R06 — P1/P2 physical input source and roster joint runtime`

只处理`D-INP-004`：

- 为`Player_2`补齐Attack/Jump/Defend action；
- 按C++ P2键位与Unity既有crossed action adapter绑定：
  - numpad1 → Unity `Attack` → canonical `Jump` → C++ key_jump；
  - numpad2 → Unity `Jump` → canonical `Defend` → C++ key_defend；
  - numpad3 → Unity `Defend` → canonical `Attack` → C++ key_attack；
- 通过真实InputSystem device state、两个production human roster slot、同tick `FrameInputSet`与最终runtime
  证明P1/P2不串键；
- 3～8 local player仍是Unity扩展，不因C++只有P1/P2而回退。

