# R6-PRES-07 — fixed-world camera / presentation camera adapter certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（no-code adapter certification）  
> 对应登记：`A-RENDER-003`

## 1. Authority / approved boundary

- C++ release `src/entity/game_tick.cpp:2026-2059`在render callback前由存活character派生并平滑
  `camera_x/camera_vel`；
- C++ release `src/render/renderer.cpp:460-505`、`517-575`、`687-716`让background、shadow、body、
  spark消费同一`camera_x`；`render_offset_x`是renderer-derived perspective显示字段；
- 用户明确批准Unity保持fixed-world logic camera：不得把C++ camera_x恢复为Unity战斗逻辑真值；
  `BattleCameraSafeArea`只属于presentation camera。

## 2. Unity source mapping / writer inventory

| 合同 | Unity source | 结论 |
|---|---|---|
| tick清零 | `SimulationWorld.StageRender.partial.cs:263-266,828-843` | PreFrame bounds后清`_cameraX/_cameraVel`及所有active entity `RenderOffsetX`。 |
| scalar owner | `SimulationWorld.cs:1355-1359` | production reset只写0；world reset同样清0。 |
| snapshot restore | `SimulationWorld.cs:533-540`、`Lockstep/BattleStateSnapshotRestore.cs:623-627` | restore可恢复已捕获scalar；正常fixed-world tick边界捕获值应为0，下一PreFrame仍强制清0。不是safe-area输入。 |
| command snapshot | `BattlePresentationShadowBuild.cs:1925-1926,2125-2126` | entity/shadow/spark读取同一已清零runtime camera/offset。 |
| safe-area display | `BattleCameraSafeArea.cs:231-270`、`NTSDRenderSpace.cs:122-140,202-212` | 只移动Unity Camera并登记presentation offset；没有写`NTSDEntityRuntime.X/Y/Z`、`_cameraX`或`RenderOffsetX`。 |

全仓production writer inventory中，非测试的`RenderOffsetX`赋值只存在runtime copy/reset、entity pool reset和
fixed-world清零；`_cameraX/_cameraVel`非零来源只有snapshot restore。没有发现角色移动直接写另一个实体或
阴影逻辑位置的路径。

## 3. Existing acceptance evidence

- `BattleRuntimeSelfCheck.CheckUnityBattleCameraRemainsDisabled`人为注入非零camera/velocity与两个实体的
  RenderOffsetX，调用production reset后断言全为0；随后移动/翻转另一个角色并改变stage snapshot，再次
  注入后确认stationary entity和shadow X不变；
- `CheckEntityAndShadowRenderPositionFormula`确认body/shadow共享同一整数offset/camera输入语义；
- `CheckRenderSpaceHorizontalOriginContracts`确认boundary viewport与fixed 794×550 fallback不会让1.5 scale
  进入逻辑坐标换算；
- 上述检查均在fresh 19:49:12 full self-check中实际执行并`PASS`，当前Editor.log `error CS=0`。

## 4. Decision

`A-RENDER-003`在source + fresh full self-check层认证为当前一致的Unity adapter；本包不修改脚本。
C++ camera链的像素表现不要求逐像素复制，且不得反写逻辑。真实`BattleCameraSafeArea`、URP world camera、
scene边缘与stationary body/shadow仍需Play Mode观察，因此状态保持`RUNTIME_PENDING`。

## 5. Residual risk / reopen

- snapshot restore后若允许在下一个PreFrame前直接发布presentation，可能观察到非零旧scalar；当前没有
  建立该调度可达性差异，标为`UNKNOWN`，后续lockstep restore fixture若证明可达再独立建包；
- safe-area脚本或其它camera follower新增runtime/entity writer；
- Play Mode再次出现一个角色移动导致其它实体/阴影跟随、或活动边界左右偏移；
- 任何修复试图移动全局场景根节点来模拟角色移动。

