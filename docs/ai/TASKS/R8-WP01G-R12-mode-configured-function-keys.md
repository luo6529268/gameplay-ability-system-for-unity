# R8-WP01G-R12 — mode-configured F7/F8/F9 fixed-tick battle commands

> 建立日期：2026-08-24  
> 状态：`COMPLETE / VERIFIED`  
> Change ID：`R8-FUNCTIONKEYMODE-001`

## Goal

在不实现F1/F2步进、不过度扩展调试系统的前提下，为Unity补齐C++ Release正常战斗功能键F7/F8/F9，并由
`GameConfig.asset`按`gameModeId + battleGameModeId`显式控制。物理按键只在Unity外层捕获边沿；所有战斗写入只在
30Hz逻辑tick边界消费。

## Exact behavior

- F7：本tick postframe将所有active实体`HP3/HPBound/HP/PP`写500，并清`BattleExitCountdown`；
- F8：设置mode2 request1，复用既有正式`SpawnMode2RandomWeapons()`全武器生成链；
- F9：设置mode2 request2，复用既有正式weapon `WeaponFlightCounter=-1`清理链；
- tick尾在mode2与entity postframe消费后清request；
- 同一渲染帧F7→F8→F9固定顺序，F9覆盖F8；跨渲染帧则最后一个F8/F9边沿获胜；F7按边沿奇偶折叠。

## Mode / lockstep policy

- 规则列表存于`GameConfig.asset`，无匹配规则默认deny；
- 初始只为标准本地战斗`gameModeId=0 / battleGameModeId=1`显式启用；
- 仅`LocalFreeRun`捕获物理F7/F8/F9；`LockstepBuffered`和`Manual`默认禁止，避免未journal命令造成帧同步分歧；
- 后续若服务器需要这些命令，必须作为独立deterministic command journal设计，不自动复用本地键盘。

## Scope

### 允许

- 新增非partial、无分配的功能键规则/边沿latch类；
- `GameConfig`增加序列化规则；
- `SimulationTickDriver`捕获并在可推进tick前消费；
- Flow增加`InitStatsRequest`并完整纳入checksum/parity/snapshot/restore；
- postframe按C++时点应用F7并与Mode2一起清理；
- focused EditMode与production Play验收。

### 禁止

- 不实现F1/F2、A→B→C、overlay、F3～F6；
- 不在`Update()`直接修改实体；
- 不把功能键塞入玩家`FrameInputSet`；
- 不允许所有模式无条件生效；
- 不改既有mode2生成/清理算法、RNG顺序、slot策略、DAT或战斗pass顺序；
- 不修改C++、AI、T8、服务器、Android或IL2CPP。

## Authority / Evidence

- `src/core/main.cpp:157-205`：F7/F8/F9 edge effects；
- `src/entity/game_tick.cpp:223+ / 310+ / 2086-2089`：mode2、init-stats、postframe与清flag时点；
- Unity现有`Mode2Request`、`Mode2RandomWeaponDropTailAll`与postframe顺序已经具备F8/F9核心。

## Files likely involved

- `Assets/NTSD/Scripts/App/GameConfig.cs`；
- 新`BattleFunctionKeyModeRule.cs`、`BattleFunctionKeyInputLatch.cs`；
- `SimulationTickDriver.cs`、`BattleRuntimeState.cs`、`SimulationWorld.Registry.partial.cs`、`SimulationWorld.Passes.partial.cs`、`NTSDBattleTickSystem.cs`；
- checksum/parity/snapshot/restore；
- focused tests与Play probe；
- `Assets/NTSD/Config/GameConfig/GameConfig.asset`。

## Deliverables / Verification

1. mode exact-match/default-deny/非LocalFreeRun禁用；
2. 边沿latch无GC、F7 parity、F8/F9 latest-wins；
3. F7正式postframe字段与clear时点；
4. F8/F9复用既有mode2生产链且tick尾清零；
5. snapshot/checksum/restore合同；
6. fresh compile、focused tests、full self-check、定向Play、validator均PASS。

## Stop conditions

- 需要改变pass order、FrameInputSet、RNG、对象池或mode2核心算法；
- 需要让联机/Manual模式接受未journal物理命令；
- 首个production Play mismatch指向scope外模块；
- 用户改变范围。

## Out of scope

F1/F2、A→B→C、其他功能键、网络命令同步、AI、T8、R1-WP02 full trace、Android、服务器、IL2CPP。

## Final evidence — 2026-08-24

- 默认只为标准本地战斗`0/1`显式启用；其他mode exact mismatch继续deny；
- LocalFreeRun物理edge经无分配latch进入tick边界；Manual/LockstepBuffered不捕获；
- focused job `7c4e0d2675f74d12aacca145f75aa302`：4/4 PASS；
- snapshot/checksum/restore job `dca455601f2a4997be98eae4baaa7db8`：18/18 PASS；
- production Play `Temp/NTSD_R8_WP01G_R12_FunctionKeys.result.json = PASS`：F7 tick1581、F8 tick1582、F9 tick1583；
- F7四项500与request/exit clear通过；F8正式生成9个武器；F9按C++ tail当时资格清理7个，2个已在tail前转换出候选类型；
- cleanup恢复world、slot、object pool、logic pool基线；
- full self-check于12:28:41 PASS。
