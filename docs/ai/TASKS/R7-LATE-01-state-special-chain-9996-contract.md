# R7-LATE-01 — late state-special reload chain / state9996 structural contract

## Goal

将Unity late state-special恢复为C++ release同一次调用的9995→4000→8000顺序reload，并在最终
current state9996、current character DAT、attacking==1时，以world-owned structural writer按最低空槽
生成4×OID217+1×OID218，保持C++ RNG调用顺序、初值与slot cursor语义。

## Scope

- `LF2Entity.RunStateSpecialPreCollision`：
  - 去掉9995/4000 transform后的提前return；
  - 每次transform后从current DAT frame0 reload state，再进入后续两个独立`if`；
  - missing target DAT仍按C++写frame0；8000分支无论target DAT是否存在都写HitStun=140。
- `SimulationWorld.LateEntityUpdateAll`：
  - state-special segment调用实体reload chain后，由world执行9996 structural writer；
  - exact-character no-op gate将符合条件的9996纳入participant；
  - 每轮从slot50起重新取最低空槽；无空槽break，缺217/218 DAT continue且不耗RNG；
  - 成功child严格按X、Y、Vy、条件Vz、Vx、frame、facing消费RNG并写C++ reset/init字段。
- `BattleRuntimeSelfCheck`：supersede旧GT-11零spawn断言，覆盖full success、missing217、missing218、
  no-slot、transform chain、slot/generation与高/低cursor same-pass可见性。

## Authority / Evidence

- C++ release `Makefile:11-35`；
- `src/entity/game_tick.cpp:352-428,577-584,687-692`；
- `include/game_world.h:23-258`的`Entity::reset`默认值；
- `include/ntsd_types.h:137-146`的LCG与`ntsd_rand()`；
- `RESEARCH/R7-LATE-01-state-special-chain-9996-preflight-20260822.md`。

## Files likely involved

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`；
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

## Unknowns

- 正式DAT中触发9995→4000→8000→9996完整链的角色/技能与真实输入可达性；留R8 Play Mode确认；
- R1-WP02 full C++ runtime trace仍BLOCKED；
- Unity对象/任务pool若在已seal战斗中容量不足属于capacity fault，本包不调整pool sizing策略。

## Deliverables

- production同调用reload chain；
- world-owned 9996五轮structural writer；
- deterministic RNG/slot/resource/cursor focused matrix；
- ledger/STATE/register/plan/handoff同步。

## Verification

1. 两份生成C#工程窄编译0 error；
2. fresh Unity compile且`error CS=0`；
3. focused GT-10/GT-11 self-check matrix；
4. full `BattleRuntimeSelfCheck`；
5. warmed no-slot state9996路径0 B；
6. validator与scoped diff check；
7. Play Mode/C++ runtime trace缺失时最高`RUNTIME_PENDING`。

## Stop conditions

- 需要改变通用opoint API、slot allocator、pool ownership/RNG abstraction或late pass order；
- 需要修改C++、DAT、scene、CentralOnly、capacity、30Hz或FrameInputSet；
- 生成child无法在不继承parent relation/team/owner的情况下复用现有factory；
- focused matrix揭示first difference属于frame_tick、weapon cleanup或其它独立模块。

## Out of scope

普通opoint、N30、broken fragment、collision/hit、AI/Frame SoA、broadphase、render worker、R8真实操作、
T8、服务器与C++ trace获取。

