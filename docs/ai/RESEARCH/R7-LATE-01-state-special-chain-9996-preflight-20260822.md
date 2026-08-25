# R7-LATE-01 — late state-special chain / state9996 source preflight

> 日期：2026-08-22  
> 状态：`SOURCE_CONFIRMED_DIFFERENCE / IMPLEMENTATION_DEFERRED`  
> 差异 ID：`D-LATE-001`  
> 预定 Change ID：`R7-LATE-001`（尚未创建 Change Record，禁止修改脚本）  
> 发现入口：R7 `LateEntityUpdate exact-character skip` 重新认证。

> 2026-08-22实施更新：本预检的后续Task/Change Record已建立并执行，当前状态为
> `R7-LATE-001 / RUNTIME_PENDING`；fresh compile、GT-11 matrix、warmed no-slot 0 B与full self-check已通过。
> 本段是追加更正，保留上方文字作为实施前历史状态。

## 1. 结论

Unity late state-special不仅把C++ `state9996`误判为exact-character no-op，底层
`LF2Entity.RunStateSpecialPreCollision`也没有实现9996 writer，并在9995/4000 transform后提前return，
无法执行C++同一次调用中的链式reload。现有GT-11 self-check明确断言9996不耗RNG、不spawn，是已被新唯一
C++ authority否定的旧C#结论，必须在未来实施包中修正，不能继续作为回归标准。

该项不是“把skip gate增加一个state判断”即可关闭：必须先建立world-owned结构生成合同、RNG顺序、最低空槽
和transform chain验收，然后整体落地。

## 2. C++ Release authority

- `Makefile:11-35`把`game_tick.cpp`列入release build；
- `game_tick.cpp:355-383` 的`run_state_special_pre_collision`依序处理：
  1. current state9995 + character DAT → data identity 50 / frame0，然后reload frame/state；
  2. reload state4000..4999 → 对应OID / frame0，然后reload；
  3. reload state8000..8999 → 对应OID / frame0 / hit-stop140，然后reload；
  4. reload state9996 + character DAT + `attacking==1` → state9996 structural writer；
- `game_tick.cpp:385-428`：9996 writer按`v415=0..4`，每轮重新寻找slot50起最低空槽；前四个OID217，
  第五个OID218。找不到空槽即break；对应DAT缺失则continue且不消费RNG；
- 每个成功child写spawner slot、identity、整数/双精度位置、`z=parent.z+1`、Vy、attack_exempt=6、
  Vx/Vz、frame和facing，再增加object_count并reset cooldown；
- 全部五个成功时，前四个child各消费7次RNG，第五个消费6次，总计34次。调用顺序严格为X、Y、Vy、
  条件Vz、Vx、frame、facing。

## 3. Unity current difference

- `SimulationWorld.Passes.partial.cs:1517-1530` 的`CanSkipExactCharacterLateStateSpecial`只将9995、
  4000..4999、8000..8999视为participant，state9996被直接skip；
- `LF2Entity.cs:2604-2624`只实现9995/4000/8000 transform，没有9996 writer；9995和4000分支return，
  不reload新DAT frame0并继续后续chain；
- 全仓脚本检索只有`BattleRuntimeSelfCheck.cs:24126-24147`的GT-11使用9996；该测试断言RNG和对象数
  完全不变，与C++ source相反；
- Unity `Assets/NTSD/Config/data.txt:55-56`已配置OID217/218生产对象，故resource IDs并非不存在；当前
  DAT资产是否含state9996 source帧不影响runtime规则合同，但真实DAT reachability仍需单独记录为待测。

## 4. Future implementation contract outline

`R7-LATE-001`必须整体覆盖：

1. 在world-owned structural writer中实现C++9996五轮最低空槽生成，兼容Authority400与已批准
   MobileExtended/DesktopExtended（C++起点50保留，但不能恢复400 production cap）；
2. 精确复现missing slot/missing OID、34-call RNG顺序和所有child初值；不得复用会继承team/owner或改变
   RNG顺序的普通opoint helper，除非逐字段证明等价；
3. 让9995→4000→8000→9996在同一state-special调用中按C++ reload继续，同时防止无界循环；C++只有
   这三个顺序单次if，不是while；
4. `CanSkipExactCharacterLateStateSpecial`必须只在完整writer落地后把9996列为participant；
5. supersede GT-11旧零spawn断言，加入：5-child full success、missing217/218、capacity exhaustion、
   transform-to-9996 chain、slot/generation/newborn visibility和next-slot same-pass可见性矩阵。

## 5. Stop / evidence boundary

脚本实施前必须新建Task Contract与`R7-LATE-001` Change Record。若需要改变通用opoint API、slot allocator、
pool ownership、RNG abstraction或late pass order，应停止并拆分架构包，不在本项顺手重构。

R6-PRES-005 fresh验收前只允许记录。后续即使compile/self-check通过，没有真实Play Mode/C++ trace时最高
仍为`RUNTIME_PENDING`。不得修改C++、DAT、scene、CentralOnly、1.5scale、camera、capacity、30Hz、
FrameInputSet、worker、SoA/ECS、pool或T8。
