---
provider: "codex"
agent_role: "code-reviewer"
model: "gpt-5.6-sol"
files:
  - ".omc/research/game-tick-physics-audit-20260717.md"
  - "Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs"
  - "Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs"
  - "Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs"
  - "Assets/NTSD/Scripts/Simulation/NTSDGlobal.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs"
  - "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs"
  - "Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs"
timestamp: "2026-07-17T03:54:50.823Z"
---

--- File: .omc/research/game-tick-physics-audit-20260717.md ---
# GameTick / Physics 全量静态对齐审计（2026-07-17）

## 1. 口径与结论

- 唯一权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`。
- Unity 对照范围：`Assets/NTSD/Scripts/Simulation/`、`Assets/NTSD/Scripts/Animation/Character/`、`Assets/NTSD/Scripts/Animation/LF2Objects/`。
- 禁止并且未读取 C++、反编译、伪代码或历史旧实现作为结论依据。
- 本报告是只读静态审计；没有修改生产代码或主进度文档。
- T8 默认 `stage.dat` 部署未计为差异。stage 逻辑本身仍以现有结构化测试夹具做了静态比较。
- F8、Mode2 随机武器、step-wait 是调试/特殊模式，单列为排除项，不计正式对局确认差异。
- 结算菜单 `RunResultsTick` 属于默认战斗范围外；只确认 Unity 正式战斗 tick 未实现该菜单流，不计入本批战斗逻辑差异。

审计覆盖：

- `Simulation/GameTick.cs`：`Run` 正式对局主干、输入/掉落、early state、冷却、preframe、late update、postframe、stage wave 的全部正式分支，覆盖 100%。
- `Frame/Physics.cs`：`Update` 到 `ResetWeaponCountOutsideState12` 的全部水平、深度、摩擦、重力、空中选帧、角色/四类武器/oid999 落地和整数同步分支，覆盖 100%。
- 为防止脱离调用者误判，额外追踪 `Frame/FrameAdvance.cs:13-47`、Unity `LF2Entity`/`LF2Character`/`LF2WeaponBase` 的实际 frame-advance 入口。

本分区共确认 **21 个差异簇**：GameTick/stage 15 个，Physics 6 个。另有 3 个需要对拍或数据契约补齐后才能关闭的风险。此前已知的 `NeedClearInput`、frame advance 前清键和自然武器掉落差异也在本报告中重新实锤，不算“已修复”。

## 2. GameTick 确认差异（15）

| ID | 权威 C# | Unity | 前置条件与差异 | 顺序/字段/RNG 后果 |
|---|---|---|---|---|
| GT-01 | `GameTick.Run:66-72`、`ClearBattleEntryInput:608` | `NTSDBattleTickSystem.RunReleaseTick:17-26`；无 `NeedClearInput` 生产字段/分支 | 进入战斗首 tick 或明确要求清输入 | C# 清 current/previous/runtime input 后整 tick 提前返回；Unity继续输入、frame、碰撞。**确认差异**。 |
| GT-02 | `GameTick.Run:99-101` | `NTSDBattleTickSystem.RunFrameAdvancePhase:28-45`、`SimulationWorld.SerialTickAll:327` | 每个 active slot 开始 frame advance 前 | C# 先清 runtime action/directional keys；Unity未在该边界清键。frame 内读取旧键时会不同。**确认差异**。 |
| GT-03 | `RunNaturalRandomWeaponDrop:636-697`、`SpawnWeaponDrop:793` | `SimulationWorld.RandomWeaponDropTickAll:1099-1191` | weaponCount<4 且 RNG gate=0 | C# 计数所有 current-DAT non-character；按 `LoadedOidOrder` 枚举；122/123 先消耗特殊 RNG 且 game mode 1..4 排除；先找 free slot，再选候选/位置；初始 frame=0，oid122 HP=200。Unity只计 `LF2WeaponBase` type1/2/4/6，按 OID 100..199，漏 game-mode gate，在耗完后续 RNG 后才由 factory 尝试分槽，扫描“飞行帧”作为 action，未显式落实 oid122 HP=200，并以 scene stage snapshot 替代 C# `XMaxOverride/Bg`。**确认差异簇**。 |
| GT-04 | `RunState501Pass:1049-1090` | `SimulationWorld.RunEarlyState501Specials:594-644` | state501，transform target有效；child `KillCount` 关联 | C# child 只接受 `KillCount == source runtime slot`，并同步 CharData/CharId/ObjType/EntityType；Unity额外接受 StableId，并只换 `FrameCache/ObjectId`，依赖稍后 snapshot 派生 type。StableId!=slot 时会误变身无关 child；变身当拍字段契约不同。**确认差异**。 |
| GT-05 | `ApplyPreframeBounds:1301-1398`、`ClampCharactersToStageZ:1961` | `SimulationWorld.StageRender.ApplyPreFrameBoundsAll:69-97`、`ClampCharacterZToStageBoundsAll:50-67`；`LF2Entity.ApplyPreFrameZBounds:2078` 空实现；仅 `LF2Character:1592` override | 任意 weapon/special/other，或 current DAT 与 CLR shell 不同 | C# 全部 active slot 按 current CharData type 做 Z clamp（type3 用 logic-Z，其他 nonchar 为 zmin-1..zmax+1，char为 zmin..zmax）并写 ZInt。Unity只有 CLR `LF2Character` 做 char clamp；所有非角色无 preframe Z；character shell 变成 nonchar 仍被当 char，nonchar shell 变成 char则不 clamp。**确认差异**。 |
| GT-06 | `RegeneratePreCollisionStats:1474-1519`，由 `RunLateEntityUpdate:1539` 按 current CharData character 调用 | 仅 `LF2Character.RunPreCollisionRecoveryPhase:1600` override；基类 `LF2Entity:2175` 空实现 | state transform 造成 current DAT/CLR shell 交叉 | C# 恢复 HP/负 WeaponCount 伤害/PP 的资格由 current DAT type 决定；Unity由 CLR shell 决定。会漏跑或多跑恢复。**确认差异**。 |
| GT-07 | `RunLateEntityUpdate:1558-1583` 的 character death/drop/bounce 与 weapon `Unk31C<0` cleanup | 仅 `LF2Character.RunLateDeathOpointPreCleanupPhase:1346` 和 `LF2WeaponBase.TryRunLatePostOpointCleanupPhase:539` override；基类 `LF2Entity:3481-3483` 为空 | current DAT 与 CLR shell 交叉 | C# 按 current DAT 类型；Unity按 CLR shell。变身后会漏掉死亡放武器/弹地或武器破碎清理，也可能对已变成非角色的 character shell 多跑。**确认差异**。 |
| GT-08 | `RunLateEntityUpdate:1556-1568` | `SimulationWorld.HandleLateFrameTickExit:807-851` | late frame tick 得到 frame 1100..1299 | C# 先识别 frame/100==11或12，广播 `HitStop=1100-frame`，自身 frame=0 并保留实体；Unity在 `:813` 先用 `frameId>=400` 直接 Free，后面的 relay 分支 `:826` 永远不可达。**确认差异**。 |
| GT-09 | `RunState9998Cleanup:825` 只位于 frame advance 后；late tick 后没有第二次 state9998 cleanup | `HandleLateFrameTickExit:819-824` | late frame tick 本拍新进入一个合法 frameId、其 state=9998 | C# 实体保留到下一 tick 的 post-frame-advance cleanup；Unity本 tick late 立即 Free，生命周期少一拍。**确认差异**。 |
| GT-10 | `RunStateSpecialPreCollision:1615-1662`：变更 identity 后直接 `entity.Frame=0`，8000 分支另写 HitStop=140 | `LF2Entity.ApplyStateDataTransform:3670-3690` 调 `ImmediateFrame(0)` | state9995、4000..4999、8000..8999 | Unity `ImmediateFrame` 还写 PN、清 Attacking、同步 transistor，并可能触发表现；C# 这里只写 Frame（外加 identity/Unk31C/可选 HitStop）。**确认差异**。 |
| GT-11 | 正式 C# `GameTick.cs`/调用链无 state9996 分支 | `LF2Character.RunState9996SpecialPreCollision:1280-1336`，由 `RunStateSpecialPreCollision:744-750` 调用 | CLR character，state9996 且 Attacking==1 | Unity额外生成 5 个 oid217/218，消耗多次全局 RNG，并写 attackExempt/速度/位置；该逻辑只引用被禁止作为权威的旧来源，正式 C# 无对应行为。**确认多余逻辑**。 |
| GT-12 | `SpawnTransitionEffectBranch1:1804-1840`、Branch2`:1842-1871` 全程 double | `LF2Entity.SpawnTransitionEffectBranch1:3535-3558`、Branch2`:3560-3582` 将 X/Y/Vx/Vy 强转 float | state13/200 退出或 state18/19 碎片 | RNG 次数/顺序现已相同，但 Unity在写 runtime 前丢失 double 精度，逐 tick trace 和后续位置可能分叉。**确认差异**。 |
| GT-13 | `RunEntityPostframeTail:1897-1959` 只做 Mode2、InitStats、heal/catch、state1700、candidate clear | `SimulationWorld.EntityPostFrameTailAll:854-906` 之后额外 `RunReleaseEntityCleanupTail:908-939` | 任意 current-DAT non-character HP<=0，或 dead character state14 等待超阈值 | 正式 C# 无这个通用尾部销毁 pass；Unity会额外 Free 非角色和部分死亡角色。会改变实体寿命、槽位复用和后续 RNG/碰撞。**确认多余逻辑**。 |
| GT-14 | `SpawnStageImmediateEntrySlot:2079-2148` 先取首个 free slot `20..399`，无槽则不消耗生成 RNG | `SimulationWorld.StageWave.SpawnStageImmediateEntrySlot:436-484` 先算随机 X/Z，factory/registry 动态槽从 `DynamicRuntimeSlotStart=50` 分配 | stage fixture 有 spawn；不涉及默认 stage.dat 部署 | Unity固定槽编号与 C# 不同，且满槽时 RNG 调用边界不同。slot 会进入 holder/owner/vrest/checksum。**确认差异**。 |
| GT-15 | `StageSpawnEntryFactor:2153-2171` 只计 active slots 0..19 | `SimulationWorld.StageWave.StageSpawnEntryFactor:139-165` 用 `GetAllEntities`，未按 `IsActiveForCurrentPass` 排除 `OidMergeDormant/PendingFlushDestroy` | oid7/8 合体或固定槽有 dormant/pending 实体，同时 stage 正比例 spawn | C# 合体只计 active oid51（权重2）；Unity还会计 dormant partner，放大 entryCount/targetTotal。**确认差异**。 |

## 3. Physics 确认差异（6）

| ID | 权威 C# | Unity | 前置条件与差异 | 可观察后果 |
|---|---|---|---|---|
| PH-01 | `Physics.UpdateHorizontal/UpdateDepth:33-60` 只按 block flags 决定 X/Z 位移 | `CharacterMechanics.Step:156-232` 在位移后额外调用 scene `IsMovementWalkable`，失败就回滚 X/Z；native/shared character 均注入该 callback | 场景存在 BoundaryWall 且新点不在 polygon | C# 不做该 point-polygon 回滚，边界由既定 pass 处理；Unity运动轨迹被额外改变。**确认多余逻辑**。 |
| PH-02 | `UpdateVertical:124`、`ApplyGroundResolve:220` 对 `-0.0001/0.0001` 使用 double 且 `(0,0.0001]` 不 clamp；IronBall 只看 newY>0.0001 | `CharacterMechanics.Step:201-216`、`WeaponDynamics:268-272` 用 float epsilon/`Y>0` 即 clamp，统一 `crossedGround` 还要求 oldVy>0 | 极小 Y 边界、外部/opoint 生成 below-ground Y、IronBall oldVy<=0 | 空中重力、Y 真值和 IronBall landing 分支不同。**确认差异簇**。特别地，grounded state2000 stationary 的正确 C# 结果是保持 frame，不是 frame20：newY=0 在 `Physics.cs:230` 已 return。 |
| PH-03 | `Physics.cs:44,46` 用 double `0.2`；`:242,397,415` 用 `0.3333333333333333` | `NTSDGlobal.WeaponExtraVxFactor` 是 `0.2f`，`LF2Entity:4755/4757`、`LF2Weapon:170/172` 乘 float；character landing `LF2Entity:4960/4988/5012` 用 `1f/3f` | oid120/101/type4 横移，或角色落地衰减 Vx | double runtime 被 float 常量污染，第一拍即可产生位级不同，随后 X/碰撞分叉。**确认差异**。 |
| PH-04 | state12/18 landing `Physics.cs:387-389` 直接 `Hp -= damage; HpMax -= damage`；state13 高速落地也不 clamp HP | `LF2Entity.ApplySharedCharacterDatLandingWeaponCountDamage:5026-5042` 和 state13 `:4993-4999` 额外把 HP/HPBound clamp 到0 | 落地伤害超过当前 HP 或 HPBound | C# 允许负值留到后续死亡/结果链；Unity提前归零，checksum/分支不同。**确认差异**。 |
| PH-05 | oid999 分支位于 `ApplyGroundResolve:367`，受开头 `newY<=0.0001 return` 约束 | `LF2Entity.ApplyCurrentDatNonCharacterLanding:4905-4912` 条件为 `Runtime.Y > -0.0001`，即 grounded Y=0 每拍也触发 | oid999 在地面、Vy=0 | C#不切 frame101；Unity强制 frame101 并清 Vx/Vy/Attacking。**确认差异**。 |
| PH-06 | `Physics.ApplyGroundResolve:255-356` 只更新 frame、速度、Unk31C、Facing、Attacking；不会写 `Residual.WeaponState` | `LF2Entity.ApplyCurrentDatNonCharacterLanding:4800-4900` 每个 light/heavy/throw/drink landing 分支额外写 `Runtime.WeaponState=1000/1003/1004/2000/2004` | 任意武器落地/反弹 | C# 的 WeaponState 只在 `FrameAdvance.FrameLogic:66-80` 走 1002→2000→3000 内部链；Unity将它改成 frame-state，下一拍 boomerang/减速/持有判定可能不同。**确认差异**。 |

## 4. 已核对等价的主分支

以下分支在当前静态代码中未发现新的字段/顺序差异（不代表已有 focused test 自动证明全部输入空间）：

- GameTick tick-head `GameTick/InputPhase/FrameMod12/FrameToggle` 的推进。
- state400/401 目标选择、距离严格比较、无目标清速度及位置偏移主逻辑。
- state500 reset gate 主判断。
- oid7/8→51 合体与51拆分的主要 gate、HP/HPBound、位置、计时器和正式 reset 字段；dormant 适配自身能模拟 Active=false，但 stage factor 未排除 dormant，已列 GT-15。
- cooldown `ARest` decrement 与当前 frame itr/holder-wpoint 对 AttackExempt 的清除。
- frame postprocess 的 hitCount>0 速度平均和 knockback 清零（正常生产写入只有0/1）。
- held positive-link validation 的 target slot/反向 holder 检查。
- character state12/18/13 与普通落地主分支的帧号和阈值；差异仅为 PH-02/03/04。
- light/heavy/throw/drink 的主要落地阈值、反弹速度、Vx系数、声音和帧号；差异仅为 PH-02/06。
- type3 visual-Z 的加法和 logic-Z 概念；preframe Z 分派缺失另列 GT-05。
- `SyncIntegers` 使用 `(int)double` 截断，当前 Unity `Runtime.SyncIntegerPosition` 等价。
- `ResetWeaponCountOutsideState12` 主条件等价。
- stage wave 的 phase advance、ratio/times 计算、40上限、bound 写入、refill/producer gate 主体；槽位/RNG与 dormant factor 例外已列 GT-14/15。

## 5. 待运行时/数据契约证实的风险（3）

| ID | 风险 | 需要的证据 |
|---|---|---|
| R-GP-01 | C# 有独立 `FrameWaitCounter`，`FrameRuntime.SetFrameImmediate` 会清0；Unity `NTSDEntityRuntime` 没有独立字段。新 `BattleParitySnapshot.cs:403,410` 暂时把 `frameWaitCounter` 和 `waitCounter` 都投影成同一个 `runtime.WaitCounter`，因此 comparator 无法发现这类差异。 | 补独立 runtime 字段、所有写入/重置方和 schema 后再对拍。当前不能签发 full certificate。 |
| R-GP-02 | `CharacterMechanics.Step` 的 friction 受 `mass>0` gate，C# `Physics.ApplyGroundFriction` 不看 mass。当前 `NTSDSpec` 已部署条目均为正数/默认1，尚未找到 production mass=0 角色。 | 全量资产/spec 审计或构造 future mass=0 fixture；若可达则升级为确认差异。 |
| R-GP-03 | Unity 用 `OidMergeDormant` 模拟 C# `Active=false + CharData=null`。多数 query 已正确排除，但 `GetAllEntities` 使用点不是统一 active 过滤，GT-15 已实锤一个消费者；仍可能有其他消费者读取 dormant 的 ObjectId/current DAT。 | 全 repo `GetAllEntities` consumer audit 与 merge→多 tick trace。 |

## 6. 明确排除项

- `RunF8WeaponDrop`：调试功能，不计正式战斗差异。
- `RunMode2RandomWeaponDrop` / `InitStats`：特殊调试模式，不计正式战斗差异。
- step-wait/overlay early-return：调试单步，不计正式战斗差异。
- `RunResultsTick` 的结算菜单交互：默认战斗场景范围外；若未来把“battle-end flow”纳入逐帧证书，需要单独实现/对拍。
- camera_x 表现链：按用户明确要求，Unity 固定世界相机，不恢复角色驱动 camera；不把该表现适配计为战斗逻辑差异。背景 layer `AnimCounter` 也未作为本批战斗状态证书字段。
- 默认 `stage.dat` 未部署：继续暂缓，不计差异；GT-14/15 是独立于默认资产部署的 stage runtime 逻辑问题。

## 7. 建议修复顺序

1. 先修会让首个正式 tick 立即分叉的 GT-01/02、PH-03/06。
2. 再修生命周期/槽位：GT-08/09/13/14/15、PH-05。
3. 中央化 current-DAT 分派：GT-04/05/06/07/10，避免继续按 CLR shell 漏分支。
4. 清除正式 C# 不存在的 GT-11，并修 GT-12 double 链。
5. 修自然掉落 GT-03 和 BoundaryWall 额外逻辑 PH-01。
6. 补 `FrameWaitCounter` 契约后，运行同 seed 双端逐 tick trace；以首个 domain/slot/field 差异继续收敛。

在上述确认差异和风险关闭前，不能声明 GameTick/Physics 或全战斗逐帧等价。


--- File: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs ---
using System;
using System.Collections.Generic;
using NTSD.App;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// 对齐 C++ GameWorld 的战斗配置快照。
    /// 这里只保存 battle runtime 需要长期持有的配置真相，不混 UI 光标或场景对象引用。
    /// </summary>
    [Serializable]
    public sealed class BattleMatchRuntimeState
    {
        public int LocalGameModeId;
        public int BattleGameModeId;
        public int BackgroundId = -1;
        public int Difficulty = 2;
        public int Seed;

        public void Reset()
        {
            LocalGameModeId = 0;
            BattleGameModeId = 0;
            BackgroundId = -1;
            Difficulty = 2;
            Seed = 0;
        }
    }

    /// <summary>
    /// 对齐 C++ GameWorld 里的 stage / boundary 运行态。
    /// Unity 场景对象只是来源；真正运行时以这里的快照为准。
    /// </summary>
    [Serializable]
    public sealed class BattleStageRuntimeState
    {
        public int BaseStageWidthPx = 800;
        public int StageWidthPx = 800;
        public int ZMin = 180;
        public int ZMax = 350;
        public int PerspectiveNear;
        public int PerspectiveFar;
        public int XMaxOverride;
        public int CameraMaxOverride;

        public void Reset()
        {
            BaseStageWidthPx = 800;
            StageWidthPx = 800;
            ZMin = 180;
            ZMax = 350;
            PerspectiveNear = 0;
            PerspectiveFar = 0;
            XMaxOverride = 0;
            CameraMaxOverride = 0;
        }

        public void SetSceneSnapshot(int stageWidthPx, int zMin, int zMax, int perspectiveNear, int perspectiveFar)
        {
            BaseStageWidthPx = Mathf.Max(stageWidthPx, 1);
            ZMin = zMin;
            ZMax = Mathf.Max(zMax, zMin + 1);
            PerspectiveNear = perspectiveNear;
            PerspectiveFar = perspectiveFar;
            RebuildActiveStageBounds();
        }

        public void ApplyPhaseBound(int bound)
        {
            if (bound > 0)
            {
                XMaxOverride = Mathf.Max(bound, 1);
                CameraMaxOverride = XMaxOverride - 794;
            }
            else
            {
                XMaxOverride = 0;
                CameraMaxOverride = 0;
            }

            RebuildActiveStageBounds();
        }

        public void ClearPhaseBound()
        {
            XMaxOverride = 0;
            CameraMaxOverride = 0;
            RebuildActiveStageBounds();
        }

        private void RebuildActiveStageBounds()
        {
            StageWidthPx = XMaxOverride > 0
                ? Mathf.Max(XMaxOverride, 1)
                : Mathf.Max(BaseStageWidthPx, 1);
        }
    }

    [Serializable]
    public sealed class BattleStageSpawnData
    {
        public int Id = -1;
        public int Act;
        public int Hp;
        public int Times = 1;
        public int X;
        public int Y;
        public double Ratio;
        public int Join;
    }

    [Serializable]
    public sealed class BattleStagePhaseData
    {
        public int Bound;
        public List<BattleStageSpawnData> Spawns = new List<BattleStageSpawnData>();
    }

    [Serializable]
    public sealed class BattleStageCampaignData
    {
        public int Id = -1;
        public string Comment = string.Empty;
        public List<BattleStagePhaseData> Phases = new List<BattleStagePhaseData>();
    }

    [Serializable]
    public sealed class BattleStageProgressionState
    {
        public int StageSeriesIdx;
        public int WaveIdx = -1;
        public int Round;
        public int RoundMax;

        public void Reset()
        {
            StageSeriesIdx = 0;
            WaveIdx = -1;
            Round = 0;
            RoundMax = 0;
        }
    }

    /// <summary>
    /// 对齐 C++ battle slot / reserve 前置编排信息。
    /// 当前先落主 slot 信息；reserve/result 细节后续继续迁移到这里。
    /// </summary>
    [Serializable]
    public sealed class BattleSlotRuntimeState
    {
        public bool Active;
        public bool IsHuman;
        public int CharacterId = -1;
        public int Team;
        public int InputId;
        public int AiId = -1;
        public int RuntimeSlotIndex = -1;
        public int StableId = -1;

        public void Reset()
        {
            Active = false;
            IsHuman = false;
            CharacterId = -1;
            Team = 0;
            InputId = 0;
            AiId = -1;
            RuntimeSlotIndex = -1;
            StableId = -1;
        }
    }

    [Serializable]
    public sealed class BattleRosterRuntimeState
    {
        public BattleSlotRuntimeState[] Slots = CreateSlots();
        public int ActiveSlotCount;

        private static BattleSlotRuntimeState[] CreateSlots()
        {
            var slots = new BattleSlotRuntimeState[8];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new BattleSlotRuntimeState();
            return slots;
        }

        public void Reset()
        {
            if (Slots == null || Slots.Length != 8)
                Slots = CreateSlots();

            for (int i = 0; i < Slots.Length; i++)
                Slots[i].Reset();

            ActiveSlotCount = 0;
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            Reset();
            if (config?.players == null)
                return;

            int writeIndex = 0;
            for (int i = 0; i < config.players.Count && writeIndex < Slots.Length; i++)
            {
                PlayerSlotConfig player = config.players[i];
                if (player == null || !player.use)
                    continue;

                BattleSlotRuntimeState slot = Slots[writeIndex];
                slot.Active = true;
                slot.IsHuman = player.isHuman;
                slot.CharacterId = player.characterId;
                slot.Team = player.team;
                slot.InputId = player.inputId;
                slot.AiId = player.aiId;
                writeIndex++;
            }

            ActiveSlotCount = writeIndex;
        }
    }

    /// <summary>
    /// 对齐 C++ GameWorld / battle globals 的流程态。
    /// 这里只收全局 tick / gate / route 标记，不混表现层字段。
    /// </summary>
    [Serializable]
    public sealed class BattleFlowRuntimeState
    {
        public int CurrentTickIndex;
        public int SparkRenderFrame;
        public int AiPhaseGate;
        public int InputPhase;
        public int FrameMod12;
        public int FrameToggle;
        public int AiDifficulty;
        public int AiRand3;
        public int AiRand5;
        public int AiRand15;
        public int AiRand20;
        public int AiMoveMode;
        public int AiStageTargetX;
        public int BattleExitCountdown;
        public int RouteOutRequest;
        public int Mode2Request;
        public int BattleStepMode;
        public int BattleStepGate;
        public int DjaGuardGlobal44F224;
        public bool NeedClearInput;

        public void Reset()
        {
            CurrentTickIndex = 0;
            SparkRenderFrame = 0;
            AiPhaseGate = 0;
            InputPhase = 0;
            FrameMod12 = 0;
            FrameToggle = 0;
            AiDifficulty = 0;
            AiRand3 = 0;
            AiRand5 = 0;
            AiRand15 = 0;
            AiRand20 = 0;
            AiMoveMode = 0;
            AiStageTargetX = 0;
            BattleExitCountdown = 0;
            RouteOutRequest = 0;
            Mode2Request = 0;
            BattleStepMode = 0;
            BattleStepGate = 0;
            DjaGuardGlobal44F224 = 0;
            NeedClearInput = false;
        }
    }

    /// <summary>
    /// Unity 侧的战斗唯一运行态根节点。
    /// 让 SimulationWorld 对齐 C++ GameWorld 的“职责中心”，但避免重新长成一个巨型类。
    /// </summary>
    [Serializable]
    public sealed class BattleRuntimeState
    {
        private const int BattleStatSlotCount = 3;

        public BattleMatchRuntimeState Match = new BattleMatchRuntimeState();
        public BattleStageRuntimeState Stage = new BattleStageRuntimeState();
        public List<BattleStageCampaignData> StageCampaigns = new List<BattleStageCampaignData>();
        public BattleStageProgressionState StageProgression = new BattleStageProgressionState();
        public bool StageProgressionValid;
        public int StageSpawnWaveApplied = -1;
        public int StageSpawnWaveDeferredEntryApplied = -1;
        public int StageSpawnRuntimeWave = -1;
        public List<int> StageSpawnRuntimeTargetTotal = new List<int>();
        public List<int> StageSpawnRuntimeEntryCount = new List<int>();
        public List<int> StageSpawnRuntimeSpawnedTotal = new List<int>();
        public List<int[]> StageSpawnRuntimeSlots = new List<int[]>();
        public BattleRosterRuntimeState Roster = new BattleRosterRuntimeState();
        public BattleFlowRuntimeState Flow = new BattleFlowRuntimeState();
        public int[] KillStats = new int[BattleStatSlotCount];
        public int[] DamageStats = new int[BattleStatSlotCount];

        public void Reset()
        {
            Match?.Reset();
            Stage?.Reset();
            StageProgression?.Reset();
            StageProgressionValid = StageCampaigns != null && StageCampaigns.Count > 0;
            StageSpawnWaveApplied = -1;
            StageSpawnWaveDeferredEntryApplied = -1;
            StageSpawnRuntimeWave = -1;
            StageSpawnRuntimeTargetTotal?.Clear();
            StageSpawnRuntimeEntryCount?.Clear();
            StageSpawnRuntimeSpawnedTotal?.Clear();
            StageSpawnRuntimeSlots?.Clear();
            Roster?.Reset();
            Flow?.Reset();
            ResetStatArray(ref KillStats);
            ResetStatArray(ref DamageStats);
        }

        private static void ResetStatArray(ref int[] stats)
        {
            if (stats == null || stats.Length != BattleStatSlotCount)
            {
                stats = new int[BattleStatSlotCount];
                return;
            }

            Array.Clear(stats, 0, stats.Length);
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs ---
namespace NTSD.Simulation
{
    /// <summary>
    /// Unity NTSD 战斗 tick 调度器。
    /// pass 顺序以 C++ release 工程为基准；实体专属行为保留在 LF2Entity 子类中，
    /// 本类只负责集中维护这些 pass 的执行时机。
    /// </summary>
    public sealed class NTSDBattleTickSystem
    {
        private readonly SimulationWorld world;

        public NTSDBattleTickSystem(SimulationWorld world)
        {
            this.world = world;
        }

        public void RunReleaseTick(int tickIndex)
        {
            if (world == null) return;

            world.PendingSounds.Clear();
            world.AdvanceBattleFlowTick(tickIndex);
            if (!RunFrameAdvancePhase(tickIndex))
                return;
            RunInteractionPhase(tickIndex);
            RunPresentationAndCleanupPhase(tickIndex);
        }

        private bool RunFrameAdvancePhase(int tickIndex)
        {
            TickCooldowns(tickIndex);
            PostCooldownHumanInput(tickIndex);
            AiInputAndCombo(tickIndex);
            Oid5152RuntimeMaintenance(tickIndex);
            if (world.NeedClearInput)
            {
                world.SetNeedClearInput(false);
                world.ClearBattleEntryInputAll();
                return false;
            }

            EarlyFrameAdvanceSpecials(tickIndex);
            FrameLogicBeforeAdvance(tickIndex);
            FrameAdvanceAll(tickIndex);
            PostFrameAdvanceDeathCleanup(tickIndex);
            ClampCharacterZToStageBounds();
            ResolvePreInteractions(tickIndex);
            ValidateHeldLinks(tickIndex);
            ClampCharacterZToStageBounds();
            ProcessHeldObjects(tickIndex);
            CaptureCollisionFrameSnapshots();
            CollectCollisionCandidates();
            return true;
        }

        private void RunInteractionPhase(int tickIndex)
        {
            ResolvePostInteractions(tickIndex);
            RandomWeaponDrop(tickIndex);
            ResolveObjectInteractions(tickIndex);
            EndCollisionCandidateConsumption();
        }

        private void RunPresentationAndCleanupPhase(int tickIndex)
        {
            PreFrameBounds();
            CurrentWaveStage(tickIndex);
            RenderDispatch(tickIndex);
            FramePostProcess();
            LateEntityUpdate(tickIndex);
            Mode2RandomWeaponDropTail(tickIndex);
            EntityPostFrameTail(tickIndex);
        }

        private void TickCooldowns(int tickIndex)
        {
            world.VrestTickAll(tickIndex);
        }

        private void PostCooldownHumanInput(int tickIndex)
        {
            world.PostCooldownHumanInputAll(tickIndex);
        }

        private void AiInputAndCombo(int tickIndex)
        {
            world.AiInputAndComboAll(tickIndex);
        }

        private void ProcessHeldObjects(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
        }

        private void Oid5152RuntimeMaintenance(int tickIndex)
        {
            world.Oid5152RuntimeMaintenanceAll(tickIndex);
        }

        private void CaptureCollisionFrameSnapshots()
        {
            world.CaptureCollisionFrameSnapshotsAll();
        }

        private void CollectCollisionCandidates()
        {
            world.CollectCollisionCandidatesAll();
        }

        private void EndCollisionCandidateConsumption()
        {
            world.EndCollisionCandidateConsumption();
        }

        private void FrameLogicBeforeAdvance(int tickIndex)
        {
            world.FrameLogicBeforeAdvanceAll(tickIndex);
        }

        private void EarlyFrameAdvanceSpecials(int tickIndex)
        {
            world.EarlyFrameAdvanceSpecialsAll(tickIndex);
        }

        private void ResolvePreInteractions(int tickIndex)
        {
            world.PreInteractionTickAll(tickIndex);
        }

        private void FrameAdvanceAll(int tickIndex)
        {
            world.SerialTickAll(tickIndex);
        }

        private void PostFrameAdvanceDeathCleanup(int tickIndex)
        {
            world.PostFrameAdvanceDeathCleanupAll(tickIndex);
        }

        private void RandomWeaponDrop(int tickIndex)
        {
            world.RandomWeaponDropTickAll(tickIndex);
        }

        private void ResolvePostInteractions(int tickIndex)
        {
            world.PostInteractionTickAll(tickIndex);
        }

        private void ResolveObjectInteractions(int tickIndex)
        {
            world.ObjectInteractionTickAll(tickIndex);
        }

        private void ValidateHeldLinks(int tickIndex)
        {
            world.ValidateHeldLinksAll(tickIndex);
        }

        private void ClampCharacterZToStageBounds()
        {
            world.ClampCharacterZToStageBoundsAll();
        }

        private void FramePostProcess()
        {
            world.FramePostProcessAll();
        }

        private void CurrentWaveStage(int tickIndex)
        {
            world.CurrentWaveStageTickAll();
        }

        private void RenderDispatch(int tickIndex)
        {
            world.RenderDispatchAll(tickIndex);
        }

        private void PreFrameBounds()
        {
            world.ApplyPreFrameBoundsAll();
        }

        private void LateEntityUpdate(int tickIndex)
        {
            world.LateEntityUpdateAll(tickIndex);
        }

        private void Mode2RandomWeaponDropTail(int tickIndex)
        {
            world.Mode2RandomWeaponDropTailAll(tickIndex);
        }

        private void EntityPostFrameTail(int tickIndex)
        {
            world.EntityPostFrameTailAll(tickIndex);
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs ---
using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// 所有战斗实体共享的运行时字段。
    /// 这里按语义镜像 C++ release 实体布局；Unity 的渲染、对象池和组件引用不写入战斗真相状态。
    /// </summary>
    [Serializable]
    public sealed class NTSDEntityRuntime
    {
        public int SlotIndex = -1;
        public int StableId;
        public int ObjectId;

        public int ObjType;
        public int EntityType;
        public int TransformOriginalObjectId = -1;
        public int TransformTargetObjectId = -1;

        public int Team;
        public int RelationTeam;
        public int OwnerSlotIndex = -1;
        public int OwnerStableId = -1;
        public int RelationOwnerSlotIndex = -1;
        public int SpawnerSlotIndex = -1;
        public int GrabbedBy;
        public int LinkState;
        public int TargetSlotIndex = -1;
        public int CaughtSlotIndex = -1;
        public int CatcherSlotIndex = -1;
        public int HeldWeaponStableId = -1;
        public int ThrowFrameGuard = -1;
        public int CaughtDuration;
        public int CaughtFrontFlag = 1;
        public int CatchingStateTU;
        public int JumpAttackLock;
        public int AnimCounter;
        public int AnimSub;
        public int LateSpecialTargetX;
        public int LateSpecialTargetZ;
        public int[] InputHistory = new int[6];
        public byte CdAttack;
        public byte CdJump;
        public byte CdDefend;
        public byte CdDefendLock;
        public byte CdRight;
        public byte CdLeft;
        public byte CdUp;
        public byte CdDown;
        public byte ComboDra;
        public byte ComboDla;
        public byte ComboDua;
        public byte ComboDda;
        public byte ComboDrj;
        public byte ComboDlj;
        public byte ComboDuj;
        public byte ComboDdj;
        public byte ComboDja;
        public byte PrevUp;
        public byte PrevDown;
        public byte PrevLeft;
        public byte PrevRight;
        public byte PrevJump;
        public byte PrevDefend;
        public byte PrevAttack;
        public byte KeyUp;
        public byte KeyDown;
        public byte KeyLeft;
        public byte KeyRight;
        public byte KeyAttack;
        public byte KeyJump;
        public byte KeyDefend;
        public int HolderStableId = -1;
        public int HolderCopySlotIndex = -1;
        public int PickerStableId = -1;
        public int TrackerFlag;
        public bool AiControlled;

        public double X;
        public double Y;
        public double Z;
        public int XInt;
        public int YInt;
        public int ZInt;
        public double Vx;
        public double Vy;
        public double Vz;
        public float SpriteX;
        public float SpriteY;
        public float SpriteZ;
        public double Type3VisualZOffset;
        public float RenderOffsetX;
        public string Dir = "right";
        public float Zz;
        public bool XBoundPositive;
        public bool XBoundNegative;
        public bool ZBoundPositive;
        public bool ZBoundNegative;

        public int Frame;
        public int PrevFrame2;
        public int FirstPresentationTick;
        public int SpawnSemantic;
        public int SuppressFrameTickUntilTick;
        public int SuppressLateFrameTickUntilTick;
        public int SuppressPostInteractionUntilTick;
        public int SuppressObjectInteractionUntilTick;
        public int SuppressPreInteractionUntilTick;
        public int SuppressCollisionCandidateUntilTick;
        public int RenderPicOffset;
        public int WaitCounter;
        public int NextFrame;
        public int AttackingCounter;
        public int FrameDelay;
        public int HitStop;
        public double KnockbackVx;
        public double KnockbackVy;
        public double KnockbackVz;
        public int ShakeTimer;
        public int AttackExempt;
        public int HitStateCount;
        public int Fall;
        public int Bdefend;
        public int HitCount;
        public int HitConfirmEa;
        public int HitConfirm2;
        public int HealTimer;
        public int CatchTimer;
        public int KillCount = -1;
        public int ComboCountVic;
        public int ComboCountAtk;
        public int KillStat;
        public int Unk328 = -1;
        public int Unk32C = -1;
        public int Unk330;
        public int Unk334;
        public int Unk338;
        public int Unk344;
        public int Unk360 = -1;
        public int Unk3FC = -1000;
        public int Unk400 = -1000;
        public int ShotCount;
        public int WeaponCount;
        public int FallDamageDiv;
        public int WeaponFlightCounter;
        public int WeaponDropHurt;
        public int WeaponState;
        public int Blink;
        public int HitCandidateCount;
        public int HitCandidateNearestDistance = 1000;
        public int HitCandidateKind1Distance = 1000;
        public int HitCandidateExtraDistance = 1000;
        public bool OidMergeDormant;
        public bool PendingFlushDestroy;

        public int HP = 500;
        public int HPBound = 500;
        public int HP3 = 500;
        public int HPOrig;
        public int HP2Orig;
        public int RespawnCount;
        public int HPLost;
        public int MP = 500;
        public int MPMax = 500;
        public int PP = 500;
        public int PPMax = 500;
        public int PPBound = 500;
        public int PpDisplay;

        public void SetPosition(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void SetVelocity(double vx, double vy, double vz)
        {
            Vx = vx;
            Vy = vy;
            Vz = vz;
        }

        public void SyncIntegerPosition()
        {
            XInt = (int)X;
            YInt = (int)Y;
            ZInt = (int)Z;
        }

        public void UpdateSpriteOrigin(int centerx, int centery, float spriteWidthPx)
        {
            SpriteX = (float)(Dir == "right"
                ? X - centerx
                : X + centerx - spriteWidthPx);
            SpriteY = (float)(Y + Z - centery);
            SpriteZ = (float)Z;
        }

        public void ClearBounds()
        {
            XBoundPositive = false;
            XBoundNegative = false;
            ZBoundPositive = false;
            ZBoundNegative = false;
        }

        public int ResolveActiveHeldSlotIndex()
        {
            return LinkState > 0 ? TargetSlotIndex : -1;
        }

        public int ResolveActiveHolderSlotIndex()
        {
            return LinkState < 0 ? HolderStableId : -1;
        }

        public bool IsActivelyHeldBySlot(int holderSlotIndex)
        {
            return LinkState < 0 && HolderStableId == holderSlotIndex;
        }

        public void RollInputFromCurrent()
        {
            PrevUp = KeyUp;
            PrevDown = KeyDown;
            PrevLeft = KeyLeft;
            PrevRight = KeyRight;
            PrevJump = KeyJump;
            PrevDefend = KeyDefend;
            PrevAttack = KeyAttack;
        }

        public bool HasInputHistoryGate()
        {
            EnsureInputHistory();
            return InputHistory[0] != 0;
        }

        public void ClearDirectionalInputKeys()
        {
            KeyUp = KeyDown = KeyLeft = KeyRight = 0;
        }

        public void ClearActionInputKeys()
        {
            KeyAttack = KeyJump = KeyDefend = 0;
        }

        public void ResetInputState()
        {
            CdAttack = CdJump = CdDefend = CdDefendLock = CdRight = CdLeft = CdUp = CdDown = 0;
            ComboDra = ComboDla = ComboDua = ComboDda = ComboDrj = ComboDlj = ComboDuj = ComboDdj = ComboDja = 0;
            EnsureInputHistory();
            Array.Clear(InputHistory, 0, InputHistory.Length);
            PrevUp = PrevDown = PrevLeft = PrevRight = PrevJump = PrevDefend = PrevAttack = 0;
            ClearDirectionalInputKeys();
            ClearActionInputKeys();
        }

        public void ApplyInputEdges()
        {
            if (PrevRight == 0 && KeyRight == 1) { CdRight = 5; PushInputHistory(6); }
            if (PrevLeft == 0 && KeyLeft == 1) { CdLeft = 5; PushInputHistory(4); }
            if (PrevUp == 0 && KeyUp == 1) { CdUp = 5; PushInputHistory(8); }
            if (PrevDown == 0 && KeyDown == 1) { CdDown = 5; PushInputHistory(2); }
            if (PrevAttack == 0 && KeyAttack == 1) { CdDefend = 5; PushInputHistory(9); }
            if (PrevDefend == 0 && KeyDefend == 1) { CdJump = 5; PushInputHistory(0); }
            if (PrevJump == 0 && KeyJump == 1) { CdAttack = 5; PushInputHistory(5); }
        }

        public void PushInputHistory(int keyNum)
        {
            EnsureInputHistory();
            InputHistory[1] = InputHistory[2];
            InputHistory[2] = InputHistory[3];
            InputHistory[3] = InputHistory[4];
            InputHistory[4] = InputHistory[5];
            InputHistory[5] = keyNum;
        }

        public void SetInputHistoryGate(bool enabled)
        {
            EnsureInputHistory();
            InputHistory[0] = enabled ? 1 : 0;
        }

        public void ClearInputHistoryTail()
        {
            EnsureInputHistory();
            Array.Clear(InputHistory, 1, InputHistory.Length - 1);
        }

        public void TickInputCooldowns()
        {
            if (CdRight > 0) CdRight--;
            if (CdLeft > 0) CdLeft--;
            if (CdUp > 0) CdUp--;
            if (CdDown > 0) CdDown--;
            if (CdJump > 0) CdJump--;
            if (CdAttack > 0) CdAttack--;
            if (CdDefend > 0) CdDefend--;
        }

        private void EnsureInputHistory()
        {
            if (InputHistory == null || InputHistory.Length != 6)
                InputHistory = new int[6];
        }

        internal void TickDefendLockCooldown()
        {
            if (CdDefendLock > 0)
                CdDefendLock--;
        }

        public void Reset()
        {
            SlotIndex = -1;
            StableId = 0;
            ObjectId = 0;
            ObjType = 0;
            EntityType = 0;
            TransformOriginalObjectId = -1;
            TransformTargetObjectId = -1;
            Team = 0;
            RelationTeam = 0;
            OwnerSlotIndex = -1;
            OwnerStableId = -1;
            RelationOwnerSlotIndex = -1;
            SpawnerSlotIndex = -1;
            GrabbedBy = 0;
            LinkState = 0;
            TargetSlotIndex = -1;
            CaughtSlotIndex = -1;
            CatcherSlotIndex = -1;
            HeldWeaponStableId = -1;
            ThrowFrameGuard = -1;
            CaughtDuration = 0;
            CaughtFrontFlag = 1;
            CatchingStateTU = 0;
            JumpAttackLock = 0;
            AnimCounter = 0;
            AnimSub = 0;
            LateSpecialTargetX = 0;
            LateSpecialTargetZ = 0;
            EnsureInputHistory();
            Array.Clear(InputHistory, 0, InputHistory.Length);
            CdAttack = 0;
            CdJump = 0;
            CdDefend = 0;
            CdDefendLock = 0;
            CdRight = 0;
            CdLeft = 0;
            CdUp = 0;
            CdDown = 0;
            ComboDra = 0;
            ComboDla = 0;
            ComboDua = 0;
            ComboDda = 0;
            ComboDrj = 0;
            ComboDlj = 0;
            ComboDuj = 0;
            ComboDdj = 0;
            ComboDja = 0;
            PrevUp = 0;
            PrevDown = 0;
            PrevLeft = 0;
            PrevRight = 0;
            PrevJump = 0;
            PrevDefend = 0;
            PrevAttack = 0;
            KeyUp = 0;
            KeyDown = 0;
            KeyLeft = 0;
            KeyRight = 0;
            KeyAttack = 0;
            KeyJump = 0;
            KeyDefend = 0;
            HolderStableId = -1;
            HolderCopySlotIndex = -1;
            PickerStableId = -1;
            TrackerFlag = 0;
            AiControlled = false;
            X = 0f;
            Y = 0f;
            Z = 0f;
            XInt = 0;
            YInt = 0;
            ZInt = 0;
            Vx = 0f;
            Vy = 0f;
            Vz = 0f;
            SpriteX = 0f;
            SpriteY = 0f;
            SpriteZ = 0f;
            Type3VisualZOffset = 0.0;
            RenderOffsetX = 0f;
            Dir = "right";
            Zz = 0f;
            ClearBounds();
            Frame = 0;
            PrevFrame2 = 0;
            FirstPresentationTick = 0;
            SpawnSemantic = 0;
            SuppressFrameTickUntilTick = 0;
            SuppressLateFrameTickUntilTick = 0;
            SuppressPostInteractionUntilTick = 0;
            SuppressObjectInteractionUntilTick = 0;
            SuppressPreInteractionUntilTick = 0;
            SuppressCollisionCandidateUntilTick = 0;
            RenderPicOffset = 0;
            WaitCounter = 0;
            NextFrame = 0;
            AttackingCounter = 0;
            FrameDelay = 0;
            HitStop = 0;
            KnockbackVx = 0.0;
            KnockbackVy = 0.0;
            KnockbackVz = 0.0;
            ShakeTimer = 0;
            AttackExempt = 0;
            HitStateCount = 0;
            Fall = 0;
            Bdefend = 0;
            HitCount = 0;
            HitConfirmEa = 0;
            HitConfirm2 = 0;
            HealTimer = 0;
            CatchTimer = 0;
            KillCount = -1;
            ComboCountVic = 0;
            ComboCountAtk = 0;
            KillStat = 0;
            Unk328 = -1;
            Unk32C = -1;
            Unk330 = 0;
            Unk334 = 0;
            Unk338 = 0;
            Unk344 = 0;
            Unk360 = -1;
            Unk3FC = -1000;
            Unk400 = -1000;
            ShotCount = 0;
            WeaponCount = 0;
            FallDamageDiv = 0;
            WeaponFlightCounter = 0;
            WeaponDropHurt = 0;
            WeaponState = 0;
            Blink = 0;
            HitCandidateCount = 0;
            HitCandidateNearestDistance = 1000;
            HitCandidateKind1Distance = 1000;
            HitCandidateExtraDistance = 1000;
            OidMergeDormant = false;
            PendingFlushDestroy = false;
            HP = 500;
            HPBound = 500;
            HP3 = 500;
            HPOrig = 0;
            HP2Orig = 0;
            RespawnCount = 0;
            HPLost = 0;
            MP = 500;
            MPMax = 500;
            PP = 500;
            PPMax = 500;
            PPBound = 500;
            PpDisplay = 0;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/NTSDGlobal.cs ---
﻿using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// NTSD 战斗全局常量。
    /// 复刻基准以 C++ release 工程为准；保留的 LF2 名称仅用于说明 DAT 语义。
    /// </summary>
    public static class NTSDGlobal
    {
        public static class Gameplay
        {
            public const int Framerate = 30;

            // 基础物理常量。
            public const float MinSpeed = 1f;
            public const double Gravity = 1.7; // P0-f-2a: double sim gravity (baseline GravityDefault=1.7)

            // 倒地摩擦查表。正式版等价逻辑仍使用按阈值取值的语义。
            // 注意：LookupAbs 依赖 key 升序遍历。
            public static readonly IReadOnlyDictionary<int, float> FrictionFell = new Dictionary<int, float>
            {
                { 2, 0f },
                { 3, 1f },
                { 5, 2f },
                { 6, 4f },
                { 9, 5f },
                { 13, 7f },
                { 25, 9f },
            };

            // 武器命中后弹起参数。
            public const float WeaponBounceupSpeedX = 3f;
            public const float WeaponBounceupSpeedZ = 1f;

            // C++ release Entity_FrameAdvance / physics_update 常量。
            // state=1002 投射物重力分级（按 chardata.type_sub / DAT 对象类型）。
            // P0-f-2a: double sim gravity, baseline full-precision literals (NtsdConstants.cs).
            public const double WeaponGravityTypeSub7C  = 0.17;                // type_sub=0x7C：极轻对象 (baseline oid124=0.17)
            public const double WeaponGravityTypeSub78  = 0.425;               // type_sub=0x78：轻对象 (baseline oid120=0.425)
            public const double WeaponGravityTypeSub65  = 1.1333333333333333;  // type_sub=0x65：中等对象 (baseline GravityType6)
            public const double WeaponGravityDefault1002 = 0.5666666666666667; // state=1002 默认重力 (baseline)
            public const double WeaponGravityDefault    = 1.7;                 // 非 state=1002 默认重力 (baseline GravityDefault)

            // type=4 / type_sub=0x78 额外 X 速度位置修正。
            public const double WeaponExtraVxFactor = 0.2;

            // 武器落地反弹参数。P0-f-2b B1: float→double，对齐 baseline Physics.cs 全 double 落地反弹链。
            public const double WeaponType1BigBounceThreshold = 9.9;
            public const double WeaponType1BigBounceVy = -8.0;
            public const double WeaponType1VxFactor    = 0.5;

            public const double WeaponType2BigBounceThreshold = 9.0;
            public const double WeaponType2BigBounceVy = -5.0;
            public const double WeaponType2VxFactor    = 0.5;

            public const double WeaponType46BigBounceThreshold = 8.5;
            public const double WeaponType46BigBounceVyFactor  = -0.7;
            public const double WeaponType46BigBounceVyClamp   = -10.0;
            public const double WeaponType46VxFactor  = 0.7;

            // 回旋镖 vx 上下限 clamp。
            public const float WeaponBoomerangVxMax  = 9.0f;
            public const float WeaponBoomerangVxMin  = -9.0f;

            // C++ release AI_Process2：饮料/食物恢复 PP 上限，0x1F4 = 500。
            public const int DrinkPPCap = 500;

            public const float WeaponHitVx = 3f;
            public const float WeaponHitVy = -3f;

            public const float WeaponReverseFactorVx = -0.4f;
            public const float WeaponReverseFactorVy = 0.8f;
            public const float WeaponReverseFactorVz = 0.8f;

            public const float WeaponSoftBounceupSpeedY = -3f;

            public const int DefendBreakLimit = 60;
            public const float DefendInjuryFactor = 0.5f;

            // 防御吸收 lookup_abs 表：key=|ef_dvx| 阈值，value=吸收量。
            public static readonly IReadOnlyDictionary<int, float> DefendAbsorb = new Dictionary<int, float>
            {
                { 15, 5f },
            };

            public const int EffectDuration = 20;

            public const int FallKO = 60;

            // 倒地等待查表：State 12 frame 事件中按 effect.dvy 计算帧 180 的等待时间。
            public static readonly IReadOnlyDictionary<int, float> FallWait180 = new Dictionary<int, float>
            {
                { 7,  1f },
                { 9,  2f },
                { 11, 3f },
                { 13, 4f },
            };

            // 角色落地弹起参数。
            public const float CharBounceupLimitXY = 9.9f;
            public const float CharBounceupLimitY  = 11f;
            public const float CharBounceupY       = 8.5f;
            public static readonly IReadOnlyDictionary<int, float> CharBounceupAbsorb = new Dictionary<int, float>
            {
                { 9,  1f  },
                { 14, 4f  },
                { 20, 10f },
                { 40, 20f },
                { 60, 30f },
            };

            public const int EffectNumToId = 300;

            // fall/bdefend 每 TU 自然恢复量，负数表示减少累计值。
            public const float RecoverFall    = -0.45f;
            public const float RecoverBdefend = -0.5f;

            // C++ release regenerate_pre_collision_stats。
            public const int HpRecoverPeriod = 12;
            public const int PpRecoverPeriod = 3;
            public const int PpRecoverCap = 500;
            public const int PpRecoverLowLimit = 150;
            public const int PpRecoverHpRateDivisor = 100;
            public const int NegativeWeaponCountInjury = 9;
            public const int NegativeWeaponCountScaledInjury = 900;
            public const int NegativeWeaponCountHpBoundDivisor = 3;
            public const int FluteCharacterWeaponCount = -20;

            // C++ release Entity_FrameLogic：角色互撞时的速度处理。
            public const float CharCollisionVxPush = 0.85f;
            public const float CharCollisionVzDecay = 5f / 7f;
        }

        public static class Default
        {
            public static class Health
            {
                public const int HpFull = 500;
                public const int MpFull = 500;
                public const int MpStart = 200;
            }

            public static class Itr
            {
                public const float ZWidth = 12f;
                public const int HitStop = 3;
                public const int ThrowInjury = 10;
            }

            public static class CPoint
            {
                public const int Hurtable = 0;
                public const int Cover = 0;
                public const int VAction = 135;
            }

            public static class WPoint
            {
                public const int Cover = 0;
            }

            public static class Effect
            {
                public const int Num = 0;
            }

            public static class Fall
            {
                public const int Value = 20;
                public const float Dvy = -6.9f;
            }

            public static class Weapon
            {
                // C++ release GameMode_Process：普通命中 vrest 默认 10 帧。
                public const int VRest = 10;
            }

            public static class Character
            {
                public const int ARest = 7;
            }

            public static class Machanics
            {
                public const float Mass = 1f;
            }
        }

        public static class Combo
        {
           public const int Timeout = 10; // 连招超时时间。
        }

        /// <summary>
        /// 全局 MP 消耗开关，对应 C++ release 的 g_pp_mode / dword_446970。
        /// true 表示启用 MP/PP 消耗；false 表示跳过消耗逻辑。
        /// </summary>
        public static bool MPEnabled = true;

        public static class Sound
        {
            public const string DefendGuard = "Battle/Defend/Guard";
            public const string FireBurn    = "Battle/Fire/Burn";
            public const string IceFreeze   = "Battle/Ice/Freeze";
            public const string IceShatter  = "Battle/Ice/Shatter";
            public const string FallLand    = "Battle/Fall/Land";
            public const string HitNormal    = "Battle/Hit/Normal";    // 001.wav
            public const string HitKnockdown = "Battle/Hit/Knockdown"; // 006.wav
        }

        /// <summary>
        /// 按绝对值查表：
        /// 取 abs(x)，返回第一个 key >= abs(x) 的 value；
        /// 若 abs(x) 大于所有 key，则返回最后一个 key 的 value。
        /// </summary>
        public static float LookupAbs(IReadOnlyDictionary<int, float> table, float x)
        {
            if (table == null || table.Count == 0)
                return 0f;

            if (x < 0f) x = -x;

            int? lastKey = null;
            foreach (var kv in table)
            {
                lastKey = kv.Key;
                if (x <= kv.Key)
                {
                    return kv.Value;
                }
            }

            return lastKey.HasValue ? table[lastKey.Value] : 0f;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs ---
﻿using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Simulation
{
    public enum SimulationDriveMode
    {
        LocalFreeRun,
        LockstepBuffered,
        Manual
    }

    /// <summary>
    /// 战斗逻辑帧配置。
    /// 逻辑帧长度固定使用 SimulationConstants.SIM_DT；这里的配置只决定外层驱动、追帧和联机预留策略。
    /// </summary>
    [System.Serializable]
    public sealed class LockstepSimulationSettings
    {
        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多追多少个逻辑帧。正式 NTSD 以 30Hz 逐帧呈现，默认不在一个渲染帧内连续追多个逻辑帧。")]
        public int maxCatchUpTicksPerFrame = 1;

        [Tooltip("最多保留多少个逻辑帧的时间积压，超过后丢弃外层积压但不改变单个逻辑帧步长。")]
        public int maxBacklogTicks = 8;

        [Tooltip("联机帧同步预留：本地输入写入未来第 N 帧。当前单机可保持 0。")]
        public int inputDelayTicks = 0;

        [Tooltip("联机帧同步预留：推进前是否要求该逻辑帧的输入已经准备好。")]
        public bool requireInputFrameReady = false;

        [Tooltip("在每个逻辑 tick 尾部生成 canonical battle snapshot 和分域 checksum。")]
        public bool enableFrameChecksum = false;

        public void Normalize()
        {
            if (maxCatchUpTicksPerFrame < 1) maxCatchUpTicksPerFrame = 1;
            if (maxBacklogTicks < maxCatchUpTicksPerFrame) maxBacklogTicks = maxCatchUpTicksPerFrame;
            if (inputDelayTicks < 0) inputDelayTicks = 0;
        }
    }

    /// <summary>
    /// 逻辑帧输入源预留接口。
    /// 当前单机输入仍由角色自己的 SimInputBuffer 消费；后续联机可在这里接入输入收齐、预测、回滚和重放。
    /// </summary>
    public interface ISimulationFrameInputProvider
    {
        bool IsFrameInputReady(int tickIndex);
        FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
        void BeforeSimTick(int tickIndex) { }
        void AfterSimTick(int tickIndex) { }
        void Reset() { }
    }

    public sealed class LocalSimulationFrameInputProvider : ISimulationFrameInputProvider
    {
        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
    }

    /// <summary>
    /// 战斗场景模拟时钟。
    /// 负责固定 30Hz 逻辑 tick，并把 C# 权威工程的 pass 顺序交给 NTSDBattleTickSystem。
    /// Unity 的 Update/LateUpdate 只作为外层驱动和表现刷新；战斗逻辑内部不能依赖 deltaTime。
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("记录每个模拟 tick 的开始和结束。")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("启动时暂停，直到 BattleBootstrap 恢复模拟。")]
        [SerializeField] private bool startPaused = true;

        [Header("帧同步时钟")]
        [SerializeField] private LockstepSimulationSettings lockstepSettings = new LockstepSimulationSettings();

        [Header("调试信息（只读）")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;
        [SerializeField][MMReadOnly] private float renderAlpha = 0f;
        [SerializeField][MMReadOnly] private int backlogTickCount = 0;
        [SerializeField][MMReadOnly] private string lastFrameChecksum = string.Empty;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();

            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void Update()
        {
            if (paused || _world == null || lockstepSettings.driveMode == SimulationDriveMode.Manual)
            {
                RefreshInspectorState();
                return;
            }

            float delta = lockstepSettings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeAccumulator += delta;

            int maxBacklogTicks = Mathf.Max(lockstepSettings.maxBacklogTicks, lockstepSettings.maxCatchUpTicksPerFrame);
            float maxAccumulator = SimulationConstants.SIM_DT * maxBacklogTicks;
            if (_timeAccumulator > maxAccumulator)
                _timeAccumulator = maxAccumulator;

            int catchUpTicks = 0;
            while (_timeAccumulator >= SimulationConstants.SIM_DT &&
                   catchUpTicks < lockstepSettings.maxCatchUpTicksPerFrame)
            {
                int nextTickIndex = _tickIndex + 1;
                if (!CanAdvanceTick(nextTickIndex))
                    break;

                _timeAccumulator -= SimulationConstants.SIM_DT;
                StepOneTickInternal(nextTickIndex);
                catchUpTicks++;
            }

            RefreshInspectorState();
        }

        private void FixedUpdate()
        {
            // 帧同步逻辑不依赖 Unity FixedUpdate。Unity 物理循环只作为引擎外层回调存在。
        }

        private void LateUpdate()
        {
            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }

            _sparkRenderer.RenderAll(_world);
        }

        private bool CanAdvanceTick(int tickIndex)
        {
            if (lockstepSettings.driveMode != SimulationDriveMode.LockstepBuffered &&
                !lockstepSettings.requireInputFrameReady)
            {
                return true;
            }

            return _frameInputProvider == null || _frameInputProvider.IsFrameInputReady(tickIndex);
        }

        private bool StepOneTickInternal(int tickIndex)
        {
            if (_world == null || !CanAdvanceTick(tickIndex))
                return false;

            _tickIndex = tickIndex;
            _sparkRenderFrame = tickIndex;
            if (_world.Runtime?.Flow != null)
            {
                _world.Runtime.Flow.SparkRenderFrame = _sparkRenderFrame;
            }

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            _frameInputProvider?.BeforeSimTick(tickIndex);
            FrameInputSet frameInput = _frameInputProvider?.GetFrameInput(tickIndex) ??
                                       FrameInputSet.Empty(tickIndex);
            if (frameInput.TickIndex != tickIndex)
                frameInput = FrameInputSet.Empty(tickIndex);

            _lastAppliedFrameInput = frameInput;
            _world.ApplyFrameInputSet(frameInput);
            _battleTickSystem?.RunReleaseTick(tickIndex);
            CaptureFrameChecksumIfNeeded(tickIndex, frameInput);
            _frameInputProvider?.AfterSimTick(tickIndex);

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");

            return true;
        }

        private void CaptureFrameChecksumIfNeeded(int tickIndex, FrameInputSet frameInput)
        {
            if (!lockstepSettings.enableFrameChecksum)
            {
                _lastFrameSnapshot = null;
                lastFrameChecksum = string.Empty;
                return;
            }

            _lastFrameSnapshot = _world.CaptureParityFrameSnapshot(tickIndex, frameInput);
            lastFrameChecksum = _lastFrameSnapshot?.Hashes?.Overall ?? string.Empty;
        }

        private void RefreshInspectorState()
        {
            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world?.ObjectCount ?? 0;
            renderAlpha = Mathf.Clamp01(_timeAccumulator / SimulationConstants.SIM_DT);
            backlogTickCount = Mathf.FloorToInt(_timeAccumulator / SimulationConstants.SIM_DT);
        }

        public SimulationWorld World => _world;
        public int SparkRenderFrame => _sparkRenderFrame;
        public int CurrentTickIndex => _tickIndex;
        public FrameInputSet LastAppliedFrameInput => _lastAppliedFrameInput;
        public BattleParityFrameSnapshot LastFrameSnapshot => _lastFrameSnapshot;
        public bool HasFrameChecksum => _lastFrameSnapshot != null;
        public string LastFrameChecksum => lastFrameChecksum;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void ApplySettings(LockstepSimulationSettings settings)
        {
            if (settings == null)
                return;

            lockstepSettings = settings;
            lockstepSettings.Normalize();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            if (_world == null)
                return;

            _world.ResetRuntimeState();

            BattleMatchRuntimeState matchState = _world.Runtime?.Match;
            if (matchState != null)
            {
                matchState.LocalGameModeId = config?.gameMode?.gameModeId ?? 0;
                matchState.BattleGameModeId = config?.gameMode?.battleGameModeId ?? 1;
                matchState.BackgroundId = config?.backgroundId ?? -1;
                matchState.Difficulty = config?.difficulty ?? 2;
                matchState.Seed = config?.seed ?? 0;
            }

            _world.Rng?.Seed((uint)(config?.seed ?? 0));
            _world.Runtime?.Roster?.ApplyMatchConfig(config);
            _world.SetNeedClearInput(true);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            _world.ConfigureStageCampaigns(stageCampaigns, config?.stageSeriesId ?? 0, -1);
            if (matchState != null &&
                (matchState.BattleGameModeId == 1 || matchState.BattleGameModeId == 2))
            {
                _world.StartInitialStageWave();
            }

            _world.SetAiPhaseGate(matchState != null && matchState.BattleGameModeId == 2 ? 1 : 0);
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            _frameInputProvider = provider ?? new LocalSimulationFrameInputProvider();
            _frameInputProvider.Reset();
            _lastAppliedFrameInput = FrameInputSet.Empty(_tickIndex);
        }

        public bool StepOneTick(bool ignorePaused = false)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);
            _tickIndex = 0;
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            _lastAppliedFrameInput = FrameInputSet.Empty(0);
            _lastFrameSnapshot = null;
            lastFrameChecksum = string.Empty;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        protected override void OnSingletonDestroyed()
        {
            _world = null;
            _battleTickSystem = null;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的正式版战斗 pass 执行入口。
    /// </summary>
    public partial class SimulationWorld
    {
        internal static System.Func<SimulationWorld, LF2Entity, LF2Entity> RespawnEffectSpawnOverride;

        private void RunDeferredMutationEntityPass(System.Action<LF2Entity> action)
        {
            if (action == null)
                return;

            _ticking = true;
            try
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (entity == null || !IsActiveForCurrentPass(entity))
                        return;

                    action(entity);
                });
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        public void PostCooldownInputAll(int tickIndex)
        {
            PostCooldownHumanInputAll(tickIndex);
            AiInputAndComboAll(tickIndex);
        }

        public void FlushQueuedObjectPointTasks()
        {
            LF2ObjectPointFactory.Instance?.FlushTasks();
        }

        public void PostCooldownHumanInputAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (entity.AiControlled)
                    return;
                entity.RunPostCooldownInputPhase(tickIndex);
                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            });
        }

        public void ClearBattleEntryInputAll()
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    return;

                entity.Runtime?.ResetInputState();
                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            });
        }

        public void AiInputAndComboAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            BuildAiInputSlotSnapshot();
            try
            {
                RunDeferredMutationEntityPass(entity =>
                {
                    if (!entity.AiControlled || entity.GetCurrentDataObjectTypeForSimulation() != 0)
                        return;
                    entity.RunPostCooldownInputPhase(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        public void Oid5152RuntimeMaintenanceAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < 20; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                    if (obj == null || !IsActiveForCurrentPass(obj))
                        continue;

                    if (obj.Runtime.Unk338 > 0)
                    {
                        obj.Runtime.Unk338--;
                        RefreshRuntimeSnapshot(obj);
                    }

                    if (obj.ObjectId == 51)
                    {
                        TrySplitOid51BackToPair(obj);
                    }
                    else if (obj.ObjectId == 7 || obj.ObjectId == 8)
                    {
                        TryMergeOid7Or8Into51(obj);
                    }
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private bool TryMergeOid7Or8Into51(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;

            int selfSlot = self.Runtime.SlotIndex;
            LF2FrameData selfFrame = self.Frame?.D;
            if (selfSlot < 0 || selfSlot >= 10 || selfFrame == null || selfFrame.state != 2)
                return false;
            if (self.Health.HP <= 0 || self.Runtime.Unk338 != 0)
                return false;
            if (!PassesOid5152HpGate(self))
                return false;

            LF2CharacterDataWrapper oid51Wrapper = LF2Entity.ResolveRuntimeCharacterConfig(51);
            if (oid51Wrapper == null)
                return false;

            int selfX = self.GetRuntimeXInt();
            int selfZ = self.GetRenderZInt();
            int selfRelationTeam = ResolveOid5152RelationTeam(self);
            int partnerOid = 15 - self.ObjectId;

            for (int partnerSlot = 0; partnerSlot < 20; partnerSlot++)
            {
                if (partnerSlot == selfSlot)
                    continue;

                LF2Entity partner = FindEntityByRuntimeSlotForQuery(partnerSlot);
                if (partner?.Runtime == null || partner.Health == null)
                    continue;
                if (partner.ObjectId != partnerOid || partner.Health.HP <= 0 || partner.Runtime.Unk338 != 0)
                    continue;
                if (!PassesOid5152HpGate(partner))
                    continue;
                if (ResolveOid5152RelationTeam(partner) != selfRelationTeam)
                    continue;

                LF2FrameData partnerFrame = partner.Frame?.D;
                int partnerFrameId = partner.Frame?.N ?? -1;
                if (partnerFrame == null || partnerFrameId < 0 || partnerFrameId >= 400)
                    continue;
                if (partnerFrame.state == 14)
                    continue;
                if (partnerFrame.state != 2 && (partner.GetRuntimeYInt() != 0 || partnerSlot <= 9))
                    continue;

                int partnerX = partner.GetRuntimeXInt();
                int partnerZ = partner.GetRenderZInt();
                if (Mathf.Abs(selfX - partnerX) >= 50 || Mathf.Abs(selfZ - partnerZ) >= 8)
                    continue;
                if (partnerSlot <= 9 && selfX <= partnerX)
                    continue;

                int mergedHpBound = self.Health.HPBound + partner.Health.HPBound;
                if (mergedHpBound > self.Health.HP3)
                    mergedHpBound = self.Health.HP3;

                int mergedHp = self.Health.HP + partner.Health.HP;
                if (mergedHp > mergedHpBound)
                    mergedHp = mergedHpBound;

                int midpointX = (selfX + partnerX) / 2;
                int midpointZ = (selfZ + partnerZ) / 2;
                int originalSelfOid = self.ObjectId;

                self.Runtime.Unk328 = 1;
                self.Runtime.Unk32C = partnerSlot;
                self.Runtime.Unk330 = originalSelfOid;
                self.Runtime.Unk334 = partner.ObjectId;
                self.Runtime.Unk338 = 4500;
                self.Health.HPBound = mergedHpBound;
                self.Health.HP = mergedHp;
                self.Runtime.Vx = 0f;
                self.Runtime.X = midpointX;
                self.Runtime.Z = midpointZ;
                self.Runtime.XInt = midpointX;
                self.Runtime.ZInt = midpointZ;

                partner.Runtime.Vy = 0f;
                partner.Runtime.OidMergeDormant = true;

                self.TryApplyRuntimeIdentity(51, 290, false, out _);
                self.Health.PP = 500;
                self.RefreshRuntimeSnapshot();
                partner.RefreshRuntimeSnapshot();
                return true;
            }

            return false;
        }

        private bool TrySplitOid51BackToPair(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;
            if (self.ObjectId != 51 || self.Runtime.Unk328 != 1 || self.Runtime.Unk338 > 0)
                return false;

            int currentFrameId = self.Frame?.N ?? -1;
            if (currentFrameId >= 9 && currentFrameId <= 260)
                return false;

            int originalOid = self.Runtime.Unk330;
            if (LF2Entity.ResolveRuntimeCharacterConfig(originalOid) == null)
                return false;

            int aggregateHp = self.Health.HP;
            int aggregateHpBound = self.Health.HPBound;
            int partnerSlot = self.Runtime.Unk32C;
            int partnerOid = self.Runtime.Unk334;
            double splitX = self.Runtime.X;
            double splitZ = self.Runtime.Z;
            int splitXInt = self.GetRuntimeXInt();
            int splitZInt = self.GetRenderZInt();
            double preservedVy = self.Runtime.Vy;
            double preservedVz = self.Runtime.Vz;
            string preservedDir = self.Runtime.Dir;

            self.TryApplyRuntimeIdentity(originalOid, currentFrameId, false, out _);
            self.Runtime.Unk328 = -1;
            self.Runtime.Unk338 = 900;
            self.RefreshRuntimeSnapshot();

            if (partnerSlot < 0)
                return true;

            LF2Entity partner = FindEntityByRuntimeSlotIncludingDormant(partnerSlot);
            if (partner == null || LF2Entity.ResolveRuntimeCharacterConfig(partnerOid) == null)
                return true;

            int halfHp = aggregateHp / 2;
            int halfHpBound = aggregateHpBound / 2;
            int partnerStableId = partner.Runtime.StableId;
            int partnerRuntimeSlot = partner.Runtime.SlotIndex;
            LF2ItrRestTracker.StateSnapshot partnerRestState = partner.ItrRest?.CaptureState();

            self.TryApplyRuntimeIdentity(originalOid, 112, false, out _);
            self.Health.HP = halfHp;
            self.Health.HPBound = halfHpBound;
            self.Health.PP = 0;
            self.Runtime.Y = 0f;
            self.Runtime.YInt = 0;
            self.Runtime.Vx = 0f;
            self.Runtime.Vy = preservedVy;
            self.Runtime.Vz = preservedVz;
            self.Runtime.Dir = preservedDir;
            self.RefreshRuntimeSnapshot();

            partner.Reset();
            // LF2Character.Reset has pool-specific defaults that differ from formal Entity::reset.
            partner.FrameDelay = 0;
            partner.KnockbackVx = 0.1;
            partner.KnockbackVy = 0.1;
            partner.KnockbackVz = 0.1;
            partner.HolderCopySlot = 99;
            partner.Effect?.Reset();
            if (partner is LF2Character partnerCharacter)
                partnerCharacter.DeadBlinkCountInternal = -1;
            if (partner.Frame != null)
            {
                partner.Frame.PN = 0;
                partner.Frame.Prev = 0;
                partner.Frame.Prev2 = 0;
                partner.Frame.Prev2D = null;
            }
            partner.ItrRest?.RestoreState(partnerRestState);
            partner.Runtime.StableId = partnerStableId;
            partner.SetRuntimeSlotIndex(partnerRuntimeSlot);
            partner.Runtime.OidMergeDormant = false;
            partner.TryApplyRuntimeIdentity(partnerOid, 112, true, out _);
            partner.Health.HP = halfHp;
            partner.Health.HPBound = halfHpBound;
            partner.Health.PP = 0;
            partner.RelationTeam = self.RelationTeam;
            partner.Runtime.X = splitX;
            partner.Runtime.Y = 0f;
            partner.Runtime.Z = splitZ;
            partner.Runtime.XInt = splitXInt;
            partner.Runtime.YInt = 0;
            partner.Runtime.ZInt = splitZInt;
            partner.Runtime.Vx = 0f;
            partner.Runtime.Vy = 0f;
            partner.Runtime.Vz = 0f;
            partner.SwitchDir(preservedDir == "right" ? "left" : "right");
            partner.RefreshRuntimeSnapshot();
            return true;
        }

        private bool PassesOid5152HpGate(LF2Entity entity)
        {
            if (entity?.Health == null || entity.Health.HP <= 0)
                return false;

            return BattleGameModeId == 1 || entity.Health.HP < 177;
        }

        private static int ResolveOid5152RelationTeam(LF2Entity entity)
        {
            return entity?.RelationTeam ?? 0;
        }

        public void SerialTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                // C++ frame_advance scans objects[0..399] and completes one entity before
                // advancing to the next slot. The dynamic scan lets a flushed producer in a
                // later slot participate this tick; a reused lower slot waits until next tick.
                ForEachEntityByRuntimeSlot(entity =>
                {
                    entity.Runtime?.ClearActionInputKeys();
                    entity.Runtime?.ClearDirectionalInputKeys();
                    entity.SimTransit(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;

                    entity.SimTU(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });

                CleanupState9998Entities();
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private void CleanupState9998Entities()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null || frame.state != 9998) continue;
                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void PostFrameAdvanceDeathCleanupAll(int tickIndex)
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                entity?.Runtime?.SyncIntegerPosition();
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (!PassesRespawnGate(entity))
                    continue;

                if (entity.RespawnCount <= 0)
                {
                    ApplyRespawnWithoutStoredCount(entity);
                }
                else
                {
                    ApplyRespawnFromStoredCount(entity);
                }

                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            }

            _entityScratch.Clear();
        }

        private bool PassesRespawnGate(LF2Entity entity)
        {
            if (entity?.Health == null || !IsActiveForCurrentPass(entity))
                return false;

            LF2FrameData frame = entity.Frame?.D;
            if (frame == null || frame.state != LF2States.Lying || entity.Health.HP > 0)
                return false;

            int slotIndex = entity.Runtime?.SlotIndex ?? -1;
            if (slotIndex < 20 && entity.KillCount < 0 && entity.RelationTeam != 5)
                return false;

            int hitStop = entity.HitStun;
            return hitStop > 0 && hitStop < 5;
        }

        private void ApplyRespawnWithoutStoredCount(LF2Entity entity)
        {
            int hp2 = entity.HP2Orig;
            if (hp2 < 2)
            {
                entity.FreeEntityLikeExe();
                return;
            }

            entity.HP2Orig = hp2 - 1;

            int relationTeam = entity.RelationTeam;
            int sumX = 0;
            int sumZ = 0;
            int count = 0;

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity other = _entityScratch[i];
                if (other == null || other == entity || other.Health == null)
                    continue;

                if (other.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;

                if (other.RelationTeam != relationTeam)
                    continue;

                sumX += other.Runtime.XInt;
                sumZ += other.Runtime.ZInt;
                count++;
            }

            if (count > 0)
            {
                int avgX = sumX / count;
                int avgZ = sumZ / count;
                entity.Runtime.X = avgX + entity.BattleRandInt(0, 51) - 26.0;
                entity.Runtime.XInt = (int)entity.Runtime.X;
                entity.Runtime.Z = avgZ + entity.BattleRandInt(0, 31) - 16.0;
                entity.Runtime.ZInt = (int)entity.Runtime.Z;
                entity.PS.x = entity.Runtime.X;
                entity.PS.z = entity.Runtime.Z;
            }

            entity.Health.PP = 500;
            entity.Health.PPBound = entity.Health.MaxPP;
            entity.Health.HPBound = entity.Health.HP3;
            entity.Health.HP = entity.Health.HPBound;
            entity.HitStun = 20;
            entity.DirectWriteFramePreserveWaitCounter(212);
            entity.PS.y = -300.0;
            entity.PS.vy = 0.0;
            entity.Runtime.Y = -300.0;
            entity.Runtime.Vy = 0.0;
            entity.Runtime.SyncIntegerPosition();
        }

        private void ApplyRespawnFromStoredCount(LF2Entity entity)
        {
            entity.HP2Orig = entity.HPOrig;
            entity.Health.PP = 0;
            entity.Health.HPBound = entity.RespawnCount;
            entity.Health.HP3 = entity.Health.HPBound;
            entity.Health.HP = entity.Health.HP3;
            entity.RespawnCount = 0;
            entity.HPOrig = 0;
            entity.RelationTeam = 1;

            if (entity.ObjectId >= 0x1E && entity.ObjectId <= 0x24)
                entity.Runtime.RenderPicOffset = 0x8C;

            entity.DirectWriteFramePreserveWaitCounter(0xDB);
            entity.AttackingCounter = 0;
            entity.FrameDelay = 0xA;

            TrySpawnRespawnEffect(entity);
        }

        private LF2Entity TrySpawnRespawnEffect(LF2Entity entity)
        {
            if (entity == null)
                return null;

            LF2Entity overrideSpawned = RespawnEffectSpawnOverride?.Invoke(this, entity);
            if (overrideSpawned != null)
                return overrideSpawned;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return null;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = 6, facing = 0 };
            task.parent = null;
            task.team = 0;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = entity.RelationTeam;
            task.holderCopySlot = -1;
            task.spawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            task.pos = new Vector3(entity.GetRuntimeXInt(), entity.GetRuntimeYInt(), entity.GetRenderZInt());
            task.z = entity.GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = entity.GetRuntimeXInt();
            task.initialRuntimeY = entity.GetRuntimeYInt();
            task.initialRuntimeZ = entity.GetRenderZInt() + 1;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            LF2Entity spawned = factory.CreateObjectImmediate(task);
            if (spawned == null)
                return null;

            spawned.RelationTeam = entity.RelationTeam;
            spawned.SpawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            spawned.RefreshRuntimeSnapshot();
            return spawned;
        }

        public void EarlyFrameAdvanceSpecialsAll(int tickIndex)
        {
            bool teleportGate = FrameToggle != 0;

            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                entity.RunEarlyTeleportSpecialsPhase(_entityScratch, teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                RefreshRuntimeSnapshot(entity);
            }

            RunEarlyState500Specials(_entityScratch);
            RunEarlyState501Specials(_entityScratch);
            _entityScratch.Clear();
        }

        private void RunEarlyState500Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null)
                    continue;

                if (frame.state != 500)
                    continue;

                if (entity.TransformTargetObjectId == -1 || entity.TransformOriginalObjectId >= 0)
                {
                    // BMD-023: state=500 reset branch must mirror baseline SetFrameImmediate:
                    // write Frame + FrameWaitCounter only, never Attacking. Unity's
                    // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                    entity.DirectWriteFramePreserveWaitCounter(0);
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        private void RunEarlyState501Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null)
                    continue;

                if (frame.state != 501 || entity.TransformTargetObjectId <= -1)
                    continue;

                var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(entity.TransformTargetObjectId);
                if (wrapper == null)
                    continue;

                entity.TransformOriginalObjectId = entity.ObjectId;
                entity.FrameCache.Load(wrapper);
                entity.ObjectId = entity.TransformTargetObjectId;
                // BMD-023: state=501 transform branch must mirror baseline SetFrameImmediate:
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);

                int ownerStableId = entity.StableId;
                int ownerSlotIndex = entity.Runtime?.SlotIndex ?? ownerStableId;

                for (int j = 0; j < entities.Count; j++)
                {
                    LF2Entity child = entities[j];
                    if (child == null || child == entity)
                        continue;
                    if (child.KillCount != ownerStableId && child.KillCount != ownerSlotIndex)
                        continue;
                    if (child.Health != null && child.Health.HP <= 0)
                        continue;

                    child.FrameCache.Load(wrapper);
                    child.ObjectId = entity.ObjectId;
                    // BMD-023: state=501 child-transform branch must mirror baseline SetFrameImmediate.
                    // Same Y<0→212 / Y≥0→0 split as LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance:
                    // write Frame + FrameWaitCounter only, never Attacking. Unity's
                    // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                    child.DirectWriteFramePreserveWaitCounter(child.PS != null && child.PS.y < 0f ? 212 : 0);
                    RefreshRuntimeSnapshot(child);
                }
            }
        }

        public void FrameLogicBeforeAdvanceAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                LF2FrameData frame = entity.Frame?.D;
                if (!entity.SupportsFrameLogicBeforeAdvancePhase(frame))
                    return;

                entity.RunFrameLogicBeforeAdvance();
                FlushQueuedObjectPointTasks();
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void CaptureCollisionFrameSnapshotsAll()
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (entity.Runtime != null && entity.Runtime.SuppressCollisionCandidateUntilTick > 0)
                {
                    int currentTick = CurrentTickIndex;
                    if (currentTick < entity.Runtime.SuppressCollisionCandidateUntilTick)
                        return;
                }

                entity.CaptureCollisionFrameSnapshot();
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void CollectCollisionCandidatesAll()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.CollectCollisionCandidates();
        }

        public void EndCollisionCandidateConsumption()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.EndCollisionCandidateConsumption();
        }

        public void LateEntityUpdateAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotCurrent(runtimeSlot);

                    if (obj == null)
                        continue;
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    obj.RunStateSpecialPreCollision();
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    obj.RunPreCollisionRecoveryPhase(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    int frameBeforeLateTick = obj.Frame?.N ?? -1;
                    if (obj.Runtime != null && tickIndex < obj.Runtime.SuppressLateFrameTickUntilTick)
                    {
                        RefreshRuntimeSnapshot(obj);
                    }
                    else
                    {
                        obj.SimFrameTick(tickIndex);
                    }
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    if ((obj.Frame?.N ?? -1) != frameBeforeLateTick)
                        SyncHeldPoseAfterLateHolderFrameChange(obj);
                    RefreshRuntimeSnapshot(obj);

                    obj.SimEntityCollision(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    bool exitedLateFrameTick = HandleLateFrameTickExit(obj);
                    if (exitedLateFrameTick)
                    {
                        if (obj is LF2SpecialAttack)
                            FlushQueuedObjectPointTasks();
                        continue;
                    }
                    RefreshRuntimeSnapshot(obj);

                    obj.RunLateDeathOpointPreCleanupPhase();
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    var opointFactory = LF2ObjectPointFactory.Instance;
                    if (opointFactory != null)
                        opointFactory.ProcessOpointSpawnAlignedToCpp(obj);
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    bool completedLateCleanup = obj.TryRunLatePostOpointCleanupPhase();
                    if (completedLateCleanup)
                    {
                        FlushQueuedObjectPointTasks();
                        RefreshRuntimeSnapshot(obj);
                        continue;
                    }

                    obj.RunLateTailBeforePrevFrame();
                    FlushQueuedObjectPointTasks();
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    RefreshRuntimeSnapshot(obj);
                    obj.MirrorLatePrevFrame();
                    RefreshRuntimeSnapshot(obj);
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private void SyncHeldPoseAfterLateHolderFrameChange(LF2Entity holder)
        {
            if (holder?.Runtime == null || holder.Runtime.LinkState <= 0)
                return;

            int holderSlot = GetRuntimeSlotOrder(holder);
            int heldSlot = holder.Runtime.TargetSlotIndex;
            if (holderSlot < 0 || heldSlot < 0 || heldSlot >= MaxRuntimeSlots)
                return;

            LF2Entity held = FindEntityByRuntimeSlotCurrent(heldSlot);
            if (held == null ||
                !IsActiveForCurrentPass(held) ||
                held.Runtime == null ||
                held.Runtime.LinkState >= 0 ||
                held.Runtime.HolderStableId != holderSlot)
            {
                return;
            }

            if (!LF2HeldObjectRuntime.SyncHeldPose(holder, held))
                return;

            RefreshRuntimeSnapshot(held);
            holder.Renderer?.ForceRefreshPresentation();
            held.Renderer?.ForceRefreshPresentation();
        }

        private bool HandleLateFrameTickExit(LF2Entity entity)
        {
            if (entity?.Frame == null)
                return false;

            int frameId = entity.Frame.N;
            if (frameId < 0 || frameId >= 400)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            LF2FrameData frameData = entity.Frame.D;
            if (frameData != null && frameData.state == 9998)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            int frameGroup = frameId / 100;
            if (frameGroup == 11 || frameGroup == 12)
            {
                int ownerSlot = GetRuntimeSlotOrder(entity);
                GetAllEntities(_entityScratch);
                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity other = _entityScratch[i];
                    if (other != null && other.KillCount == ownerSlot)
                        other.HitStun = 1100 - frameId;
                }

                _entityScratch.Clear();
                entity.HitStun = 1100 - frameId;
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);
                return true;
            }

            if (frameId < 0 || frameId >= 400)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            return false;
        }

        public void EntityPostFrameTailAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity == null || entity.Health == null)
                    return;

                if (entity.HealTimer / 1000 == 1 && entity.Health.HP > 0)
                {
                    entity.HealTimer--;
                    if (entity.HealTimer % 8 == 0)
                    {
                        if (entity.Health.HP < entity.Health.HPBound)
                        {
                            entity.Health.HP += 8;
                            if (entity.Health.HP > entity.Health.HPBound)
                                entity.Health.HP = entity.Health.HPBound;
                        }
                        else
                        {
                            entity.HealTimer = 0;
                        }
                    }

                    if (entity.HealTimer % 1000 == 0)
                        entity.HealTimer = 0;
                }

                if (entity.CatchTimer > 0 && entity.Health.HP > 0)
                {
                    entity.CatchTimer--;
                    if (entity.CatchTimer % 8 == 0 && entity.Health.HP < entity.Health.HPBound)
                    {
                        entity.Health.HP += 8;
                        if (entity.Health.HP > entity.Health.HPBound)
                        {
                            entity.Health.HP = entity.Health.HPBound;
                            entity.CatchTimer = 0;
                        }
                    }
                }

                LF2FrameData frame = entity.Frame?.D;
                if (frame != null && frame.state == 1700)
                    entity.HealTimer = 1100;

                entity.ClearHitCandidateCarriers();

                RefreshRuntimeSnapshot(entity);
            });

            RunReleaseEntityCleanupTail();
        }

        private void RunReleaseEntityCleanupTail()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null || entity.Health == null)
                    continue;

                LF2FrameData frame = entity.Frame?.D;
                int dataType = entity.GetCurrentDataObjectTypeForSimulation();

                if (dataType == (int)LF2ObjectType.Character)
                {
                    if (frame != null &&
                        entity.Health.HP <= 0 &&
                        frame.state == 14 &&
                        entity.FrameDelay <= 0 &&
                        entity.Runtime != null &&
                        entity.Runtime.WaitCounter > frame.wait * 3)
                    {
                        entity.FreeEntityLikeExe();
                    }

                    continue;
                }

                if (entity.Health.HP <= 0)
                    entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void FramePostProcessAll()
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity.FrameDelay != 0) return;

                if (entity.HitCount > 0)
                {
                    float denom = entity.HitCount + 1;
                    entity.PS.vx = entity.KnockbackVx * 2f / denom;
                    entity.PS.vy = entity.KnockbackVy * 2f / denom;
                    entity.PS.vz = entity.KnockbackVz * 2f / denom;
                }
                entity.KnockbackVx = 0f;
                entity.KnockbackVy = 0f;
                entity.KnockbackVz = 0f;
                entity.HitCount = 0;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void VrestTickAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                entity.ItrRest?.TickArest();
                entity.Runtime?.TickDefendLockCooldown();
                ClearAttackExemptIfCurrentFrameCannotHit(entity);
                RefreshRuntimeSnapshot(entity);
            });
        }

        private void ClearAttackExemptIfCurrentFrameCannotHit(LF2Entity entity)
        {
            if (entity == null || entity.AttackExempt <= 0)
                return;

            LF2CharacterData entityData = (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity.FrameCache?.Wrapper?.characterData;
            if (entityData == null)
                return;

            LF2FrameData frame = entity.Frame?.D;
            bool clear = frame?.itrs == null || frame.itrs.Count == 0;
            if (!clear &&
                frame.state == LF2States.WeaponOnHand &&
                entity.Runtime != null)
            {
                int holderSlot = entity.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                LF2CharacterData holderData = (holder as LF2LivingObject)?._FrameDataWrapper?.characterData
                    ?? holder?.FrameCache?.Wrapper?.characterData;
                if (holder != null && holderData != null)
                {
                    LF2FrameData holderFrame = holder.Frame?.D;
                    clear = holderFrame?.wpoints == null ||
                            holderFrame.wpoints.Count == 0 ||
                            holderFrame.wpoints[0].attacking == 0;
                }
            }

            if (clear)
                entity.AttackExempt = 0;
        }

        public void PostInteractionTickAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (!entity.SupportsPostInteractionPhase()) return;
                if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPostInteractionUntilTick)
                    return;
                entity.SimPostInteraction(tickIndex);
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void ObjectInteractionTickAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (!entity.SupportsObjectInteractionPhase()) return;
                if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressObjectInteractionUntilTick)
                    return;
                entity.SimObjectInteraction(tickIndex);
                if (entity is LF2SpecialAttack)
                    FlushQueuedObjectPointTasks();
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void PreInteractionTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                if (_entityScratch.Count == 0) return;

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.RunCpointCheckStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.RunCpointMismatchTailStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                _entityScratch.Clear();

                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        return;
                    if (!IsActiveForCurrentPass(entity))
                        return;

                    entity.RunWeaponSyncHeldStep10();
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });
            }
            finally
            {
                _entityScratch.Clear();
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        public void RandomWeaponDropTickAll(int tickIndex)
        {
            int weaponCount = 0;
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is LF2Entity entity && entity.CountsAsRandomWeaponDropCandidate())
                        weaponCount++;
                }
            }
            if (weaponCount >= 4) return;
            if (Rng.NextInt(0, 200) != 0) return;

            var manager = CharacterAnimtorManager.Instance;
            if (manager == null) return;

            var candidates = new System.Collections.Generic.List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null) continue;
                if (oid == 122 || oid == 123)
                {
                    if (Rng.NextInt(0, 2) == 0) continue;
                }
                candidates.Add(oid);
            }
            if (candidates.Count == 0) return;

            int selectedOid = candidates[Rng.NextInt(0, candidates.Count)];

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(selectedOid);
            int flyFrame = -1;
            int minFrame = int.MaxValue;
            if (charData?.frames != null)
            {
                foreach (var f in charData.frames)
                {
                    if (f == null) continue;
                    if (f.frameId > 0 && f.frameId < minFrame) minFrame = f.frameId;
                    if (flyFrame < 0 && f.frameId > 0 && (
                        f.state == LF2States.WeaponInSky ||
                        f.state == LF2States.WeaponThrowing ||
                        f.state == LF2States.HeavyWeaponInSky))
                        flyFrame = f.frameId;
                }
            }
            if (flyFrame < 0) flyFrame = minFrame != int.MaxValue ? minFrame : 0;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out _, out _);
            if (stageWidth <= 60 || zMax - zMin <= 60) return;

            int r1 = Rng.NextInt(0, 30);
            int r2 = Rng.NextInt(0, 30);
            int r3 = Rng.NextInt(0, 30);
            int r4 = Rng.NextInt(0, 30);
            float lf2X = r1 * ((stageWidth - 60) / 30) + r2 + 30;
            float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
            const float lf2Y = -500f;

            var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();

            spawnTask.opoint = new ObjectPoint
            {
                oid = selectedOid,
                kind = 0,
                action = flyFrame,
                x = Mathf.RoundToInt(lf2X),
                y = Mathf.RoundToInt(lf2Y),
                dvx = 0,
                dvy = 0,
                facing = 0,
            };
            spawnTask.parent = null; spawnTask.team = 0;
            spawnTask.pos = new UnityEngine.Vector3(lf2X, lf2Y, 0);
            spawnTask.z = lf2Z; spawnTask.dir = "right"; spawnTask.dvz = 0;
            factory.CreateObjectImmediate(spawnTask);
        }

        public void Mode2RandomWeaponDropTailAll(int tickIndex)
        {
            int mode2Request = Mode2Request;
            if (mode2Request == 0)
                return;

            if (mode2Request == 1)
            {
                SpawnMode2RandomWeapons();
            }
            else if (mode2Request == 2)
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (!entity.CountsAsRandomWeaponDropCandidate())
                        return;

                    entity.Runtime.WeaponFlightCounter = -1;
                    RefreshRuntimeSnapshot(entity);
                });
            }

            SetMode2Request(0);
        }

        private void SpawnMode2RandomWeapons()
        {
            var manager = CharacterAnimtorManager.Instance;
            if (manager == null)
                return;

            var candidates = new List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null)
                    continue;

                if (oid == 122 && Rng.NextInt(0, 2) == 0)
                    continue;

                candidates.Add(oid);
            }

            if (candidates.Count == 0)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out _, out _);
            if (stageWidth <= 60 || zMax - zMin <= 60)
                return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            for (int chooseIndex = 0; chooseIndex < candidates.Count; chooseIndex++)
            {
                int oid = candidates[chooseIndex];

                bool hasFreeSlot = false;
                for (int slot = DynamicRuntimeSlotStart; slot < MaxRuntimeSlots; slot++)
                {
                    if (!_runtimeSlotUsed[slot])
                    {
                        hasFreeSlot = true;
                        break;
                    }
                }

                if (!hasFreeSlot)
                    break;

                int r1 = Rng.NextInt(0, 30);
                int r2 = Rng.NextInt(0, 30);
                int r3 = Rng.NextInt(0, 30);
                int r4 = Rng.NextInt(0, 30);
                float lf2X = r1 * ((stageWidth - 60) / 30) + r2 + 30;
                float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
                const float lf2Y = -500f;

                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
                int flyFrame = -1;
                int minFrame = int.MaxValue;
                if (charData?.frames != null)
                {
                    foreach (var f in charData.frames)
                    {
                        if (f == null)
                            continue;
                        if (f.frameId > 0 && f.frameId < minFrame)
                            minFrame = f.frameId;
                        if (flyFrame < 0 && f.frameId > 0 &&
                            (f.state == LF2States.WeaponInSky ||
                             f.state == LF2States.WeaponThrowing ||
                             f.state == LF2States.HeavyWeaponInSky))
                        {
                            flyFrame = f.frameId;
                        }
                    }
                }

                if (flyFrame < 0)
                    flyFrame = minFrame != int.MaxValue ? minFrame : 0;

                var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                spawnTask.opoint = new ObjectPoint
                {
                    oid = oid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
                };
                spawnTask.parent = null;
                spawnTask.team = 0;
                spawnTask.pos = new Vector3(lf2X, lf2Y, 0f);
                spawnTask.z = lf2Z;
                spawnTask.dir = "right";
                spawnTask.dvz = 0f;
                factory.CreateObjectImmediate(spawnTask);
            }
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 注册、运行时槽位和基础上下文。
    /// </summary>
    public partial class SimulationWorld
    {
        /// <summary>同一 SimOrder 的对象桶；只有桶内容变化后才延迟重新排序。</summary>
        private class Bucket
        {
            public List<ISimObject> items = new List<ISimObject>();
            public bool dirty = false;

            public void EnsureSorted(System.Func<ISimObject, int> stableIdSelector)
            {
                if (dirty)
                {
                    items = items.OrderBy(stableIdSelector).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>按 SimOrder 建立的模拟桶，SortedDictionary 保证 pass 顺序。</summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();
        /// <summary>注册对象时注入的模拟上下文。</summary>
        private SimContext _context;
        /// <summary>给没有显式运行时 ID 的对象自动分配 StableId。</summary>
        private int _nextAutoStableId = 100;
        private const int MaxRuntimeSlots = 400;
        private const int DynamicRuntimeSlotStart = 50;
        private readonly bool[] _runtimeSlotUsed = new bool[MaxRuntimeSlots];
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;
        private readonly List<LF2Entity> _entityScratch = new List<LF2Entity>(128);
        private int _cameraX;
        private int _cameraVel;

        public int ReleaseCameraX => _cameraX;
        internal bool IsUnityFixedWorldCameraStateClear => _cameraX == 0 && _cameraVel == 0;
        internal int MaxRuntimeSlotsForServices => MaxRuntimeSlots;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;

        private int GetRuntimeStableId(ISimObject obj)
        {
            return obj is LF2Entity entity ? entity.Runtime.StableId : obj.StableId;
        }

        private int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null) return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }

        private int CompareRuntimeSlotOrder(LF2Entity a, LF2Entity b)
        {
            int cmp = GetRuntimeSlotOrder(a).CompareTo(GetRuntimeSlotOrder(b));
            if (cmp != 0) return cmp;
            return (a?.StableId ?? int.MaxValue).CompareTo(b?.StableId ?? int.MaxValue);
        }

        private void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        private List<int> GetBucketKeySnapshot()
        {
            return _buckets.Count > 0 ? new List<int>(_buckets.Keys) : null;
        }

        public ILF2SceneQuery SceneQuery { get; private set; }
        public INTSDItrKindService ItrKindService { get; private set; }
        public DeterministicRng Rng { get; private set; }
        public BattleRuntimeState Runtime { get; private set; }
        public int[] KillStats => Runtime.KillStats;
        public int[] DamageStats => Runtime.DamageStats;

        public SimulationWorld()
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        public void ResetRuntimeState()
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Reset();
            Rng?.Seed(0x4E545344u);
            PendingSounds.Clear();
        }

        public int CurrentTickIndex => Runtime?.Flow?.CurrentTickIndex ?? 0;
        public int SparkRenderFrame => Runtime?.Flow?.SparkRenderFrame ?? 0;
        public int BattleGameModeId => Runtime?.Match?.BattleGameModeId ?? 0;
        public int LocalGameModeId => Runtime?.Match?.LocalGameModeId ?? 0;
        public int Difficulty => Runtime?.Match?.Difficulty ?? 2;
        public int BackgroundId => Runtime?.Match?.BackgroundId ?? -1;
        public int MatchSeed => Runtime?.Match?.Seed ?? 0;
        public int AiPhaseGate => Runtime?.Flow?.AiPhaseGate ?? 0;
        public int InputPhase => Runtime?.Flow?.InputPhase ?? 0;
        public int FrameMod12 => Runtime?.Flow?.FrameMod12 ?? 0;
        public int FrameToggle => Runtime?.Flow?.FrameToggle ?? 0;
        public int BattleExitCountdown => Runtime?.Flow?.BattleExitCountdown ?? 0;
        public int RouteOutRequest => Runtime?.Flow?.RouteOutRequest ?? 0;
        public int Mode2Request => Runtime?.Flow?.Mode2Request ?? 0;
        public bool NeedClearInput => Runtime?.Flow?.NeedClearInput ?? false;
        public List<BattleStageCampaignData> StageCampaigns => Runtime?.StageCampaigns;
        public BattleStageProgressionState StageProgression => Runtime?.StageProgression;
        public bool StageProgressionValid => Runtime?.StageProgressionValid ?? false;
        public int StageSpawnWaveApplied => Runtime?.StageSpawnWaveApplied ?? -1;
        public int StageSpawnWaveDeferredEntryApplied => Runtime?.StageSpawnWaveDeferredEntryApplied ?? -1;
        public int StageSpawnRuntimeWave => Runtime?.StageSpawnRuntimeWave ?? -1;
        public List<int> StageSpawnRuntimeTargetTotal => Runtime?.StageSpawnRuntimeTargetTotal;
        public List<int> StageSpawnRuntimeEntryCount => Runtime?.StageSpawnRuntimeEntryCount;
        public List<int> StageSpawnRuntimeSpawnedTotal => Runtime?.StageSpawnRuntimeSpawnedTotal;
        public List<int[]> StageSpawnRuntimeSlots => Runtime?.StageSpawnRuntimeSlots;

        public void SetAiPhaseGate(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.AiPhaseGate = value;
        }

        public void SetBattleExitCountdown(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.BattleExitCountdown = value;
        }

        public void SetRouteOutRequest(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.RouteOutRequest = value;
        }

        public void SetMode2Request(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.Mode2Request = value;
        }

        public void SetNeedClearInput(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.NeedClearInput = value;
        }

        public void AdvanceBattleFlowTick(int tickIndex)
        {
            if (Runtime?.Flow == null)
                return;

            Runtime.Flow.CurrentTickIndex = tickIndex;
            Runtime.Flow.InputPhase = (Runtime.Flow.InputPhase + 1) & 1;
            Runtime.Flow.FrameMod12 = tickIndex % 12;
            Runtime.Flow.FrameToggle = 1 - Runtime.Flow.FrameToggle;
        }

        public void SetStageProgressionValid(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageProgressionValid = value;
        }

        public void SetStageSpawnWaveApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveApplied = value;
        }

        public void SetStageSpawnWaveDeferredEntryApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveDeferredEntryApplied = value;
        }

        public void SetStageSpawnRuntimeWave(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnRuntimeWave = value;
        }

        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            // A pooled instance can be reused during the same dynamic late-slot scan.
            // Finalize its queued old lifecycle before registering the new one, and
            // remove the pending entry so the pass-finally flush cannot delete it.
            if (_pendingUnregister.Remove(obj))
                UnregisterImmediate(obj);

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                bucket = new Bucket();
                _buckets[simOrder] = bucket;
            }

            if (bucket.items.Contains(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object already registered: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity registeredEntity)
            {
                int runtimeSlot = AllocateRuntimeSlot(registeredEntity);
                registeredEntity.SetRuntimeSlotIndex(runtimeSlot);
                if (runtimeSlot < 0)
                {
                    if (bucket.items.Count == 0)
                        _buckets.Remove(simOrder);
                    Debug.LogWarning(
                        $"[SimulationWorld] Runtime slot exhausted; registration rejected: " +
                        $"StableId={registeredEntity.StableId}, Type={registeredEntity.GetType().Name}");
                    return;
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();
            }

            bucket.items.Add(obj);
            bucket.dirty = true;
            obj.OnAdded(_context);
            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                if (obj is LF2Entity pendingEntity)
                    ReleaseRuntimeSlot(pendingEntity);
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                return;
            }

            UnregisterImmediate(obj);
        }

        private void UnregisterImmediate(ISimObject obj)
        {
            int bucketKey = obj.SimOrder;
            _buckets.TryGetValue(bucketKey, out Bucket bucket);
            if (bucket == null || !bucket.items.Contains(obj))
            {
                bucket = null;
                List<int> bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys != null)
                {
                    for (int i = 0; i < bucketKeys.Count; i++)
                    {
                        int candidateKey = bucketKeys[i];
                        if (!_buckets.TryGetValue(candidateKey, out Bucket candidateBucket) ||
                            !candidateBucket.items.Contains(obj))
                        {
                            continue;
                        }

                        bucketKey = candidateKey;
                        bucket = candidateBucket;
                        break;
                    }
                }
            }

            if (bucket == null || !bucket.items.Remove(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            if (obj is LF2Entity entity)
                ReleaseRuntimeSlot(entity);
            obj.OnRemoved(_context);

            if (bucket.items.Count == 0)
                _buckets.Remove(bucketKey);

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={bucketKey}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        private void FlushPendingUnregister()
        {
            if (_pendingUnregister.Count == 0) return;
            foreach (var obj in _pendingUnregister)
                UnregisterImmediate(obj);
            _pendingUnregister.Clear();
        }

        private void FlushPendingEntityDestroy()
        {
            // Pending entities are deliberately hidden from active pass queries. Scan the
            // runtime registry directly so C++ free_entity-at-late-tail still finalizes them.
            _entityScratch.Clear();
            for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (entity?.Runtime != null && entity.Runtime.PendingFlushDestroy)
                    _entityScratch.Add(entity);
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                entity.Runtime.PendingFlushDestroy = false;
                entity.OnTransitDestroy();
            }

            _entityScratch.Clear();
        }

        private bool IsActiveForCurrentPass(ISimObject obj)
        {
            if (obj == null || _pendingUnregister.Contains(obj))
                return false;

            if (obj is LF2Entity entity && entity.Runtime != null)
            {
                if (entity.Runtime.OidMergeDormant)
                    return false;

                if (entity.Runtime.PendingFlushDestroy)
                    return false;
            }

            return true;
        }

        internal bool IsActiveForCurrentPassInternal(ISimObject obj)
        {
            return IsActiveForCurrentPass(obj);
        }

        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        private int AllocateRuntimeSlot(LF2Entity entity)
        {
            bool requiresDynamicSlot = entity.UsesDynamicRuntimeSlot();
            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 && existingSlot < MaxRuntimeSlots;
            bool existingSlotInAllowedRange = !requiresDynamicSlot || existingSlot >= DynamicRuntimeSlotStart;
            if (existingSlotInRange && existingSlotInAllowedRange && !_runtimeSlotUsed[existingSlot])
            {
                _runtimeSlotUsed[existingSlot] = true;
                return existingSlot;
            }

            int startSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            int slot = FindFreeRuntimeSlot(startSlot);
            if (slot >= 0)
                return slot;

            return -1;
        }

        private int FindFreeRuntimeSlot(int startSlot)
        {
            for (int i = Mathf.Max(0, startSlot); i < MaxRuntimeSlots; i++)
            {
                if (_runtimeSlotUsed[i]) continue;
                _runtimeSlotUsed[i] = true;
                return i;
            }

            return -1;
        }

        private void ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 && slot < MaxRuntimeSlots)
                _runtimeSlotUsed[slot] = false;

            entity.SetRuntimeSlotIndex(-1);
        }

        public int ObjectCount
        {
            get
            {
                int count = 0;
                var bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys == null) return 0;

                foreach (int simOrder in bucketKeys)
                {
                    if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                    for (int i = 0; i < bucket.items.Count; i++)
                    {
                        ISimObject obj = bucket.items[i];
                        if (obj is LF2Entity entity)
                        {
                            if (_pendingUnregister.Contains(entity))
                                continue;

                            if (entity.Runtime != null &&
                                (entity.Runtime.OidMergeDormant || entity.Runtime.PendingFlushDestroy))
                                continue;
                        }

                        count++;
                    }
                }
                return count;
            }
        }

        public SimContext Context => _context;
    }
}


--- File: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Input;
using NTSD.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 所有战斗实体的最底层公共基类。
    /// 
    /// 你可以把它理解成“所有战斗对象共享的骨架”：
    /// 1. 统一持有 Runtime、Frame、Effect、Renderer 等核心数据。
    /// 2. 定义所有实体都可能会参与的生命周期入口。
    /// 3. 让角色、武器、技能体、特效体可以共享同一套基础框架。
    /// 
    /// 简单理解项目分层：
    /// - LF2Entity：最底层实体框架
    /// - LF2LivingObject：更像战斗单位的公共能力
    /// - LF2Character / LF2WeaponBase / LF2SpecialAttack：具体对象类型
    /// </summary>
    public abstract class LF2Entity : ILF2Entity
    {
        public const int OverlaySortingOrderOffset = 10000;
        protected static readonly List<LF2Entity> N30HistoryGateScratch = new List<LF2Entity>(32);
        private readonly NTSDInputStateModule sharedCharacterDatInputModule = new NTSDInputStateModule();
        internal static System.Func<int, LF2CharacterDataWrapper> RuntimeCharacterConfigResolverOverride;


        /// <summary>对象名称。</summary>
        public string Name { get; set; }

        /// <summary>实体稳定 ID。</summary>
        public int StableId
        {
            get => Runtime.StableId;
            protected set => Runtime.StableId = value;
        }

        /// <summary>对象 ID。</summary>
        public int ObjectId
        {
            get => Runtime.ObjectId;
            set => Runtime.ObjectId = value;
        }

        /// <summary>队伍 ID。</summary>
        public int Team
        {
            get => Runtime.Team;
            set => Runtime.Team = value;
        }

        public virtual int RelationTeam
        {
            get => Runtime.RelationTeam;
            set => Runtime.RelationTeam = value;
        }

        /// <summary>生成者 StableId；-1 表示无生成者。</summary>
        public int OwnerId
        {
            get => Runtime.OwnerStableId;
            set => Runtime.OwnerStableId = value;
        }

        /// <summary>被抓取状态。</summary>
        public int GrabbedBy
        {
            get => Runtime.GrabbedBy;
            set => Runtime.GrabbedBy = value;
        }

        /// <summary>kind==2 的 tracker 标记。</summary>
        public int TrackerFlag
        {
            get => Runtime.TrackerFlag;
            set => Runtime.TrackerFlag = value;
        }

        /// <summary>kind==2 的 tracker 父对象引用。</summary>
        public LF2Entity TrackerParent { get; set; }

        /// <summary>当前命中的 itr 槽位索引，用于 spark 计时。</summary>
        public int CurrentItrIndex { get; set; }

        /// <summary>对象类型整数值，由子类 ObjectTypeEnum 决定。</summary>
        public int ObjectType => (int)ObjectTypeEnum;

        /// <summary>对象类型枚举，由子类实现。</summary>
        public abstract LF2ObjectType ObjectTypeEnum { get; }

        /// <summary>
        /// 逻辑真值运行时。
        /// 大部分真正参与战斗结算的位置、速度、状态字段，最终都应该落在这里。
        /// </summary>
        public NTSDEntityRuntime Runtime { get; } = new NTSDEntityRuntime();

        public PhysicsState PS { get; protected set; } = new PhysicsState();

        private static readonly DeterministicRng FallbackRng = new DeterministicRng(0x4E545344u);

        /// <summary>C++ release 实体类型值。</summary>
        public virtual int ReleaseEntityType => ObjectType;

        public virtual bool CountsAsRandomWeaponDropCandidate() => false;

        /// <summary>当前对象正在执行哪一帧逻辑，以及上一帧/碰撞快照帧等辅助信息。</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>当前对象对应的 DAT 帧数据缓存。</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>帧切换控制器。负责 wait/next/frame jump 等帧推进细节。</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>效果状态。</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 资源引用。</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>渲染器引用。</summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>当前实体所在的战斗世界。大多数情况下通过单例 Driver 反查。</summary>
        private SimulationWorld registeredWorld;

        public SimulationWorld Match => registeredWorld ?? SimulationTickDriver.Instance?.World;



        /// <summary>帧延迟计数器。大于 0 或小于 0 时，都会影响本帧是否真正推进。</summary>
        public int FrameDelay
        {
            get => Runtime.FrameDelay;
            set => Runtime.FrameDelay = value;
        }

        /// <summary>投掷后的同帧保护帧号，命中当前 frame 时直接跳过 frame advance / frame tick。</summary>
        public int ThrowFrameGuard
        {
            get => Runtime.ThrowFrameGuard;
            set => Runtime.ThrowFrameGuard = value;
        }

        /// <summary>C++ release Entity::attacking，帧等待/攻击状态计数器。</summary>
        public int AttackingCounter
        {
            get => Runtime.AttackingCounter;
            set => Runtime.AttackingCounter = value;
        }

        /// <summary>命中停帧/锁定计数。可以理解成“这一小段时间内对象被短暂停住”。</summary>
        public int HitStun
        {
            get => Runtime.HitStop;
            set => Runtime.HitStop = value;
        }

        /// <summary>累计击退 X 速度。</summary>
        public double KnockbackVx
        {
            get => Runtime.KnockbackVx;
            set => Runtime.KnockbackVx = value;
        }

        /// <summary>累计击退 Y 速度。</summary>
        public double KnockbackVy
        {
            get => Runtime.KnockbackVy;
            set => Runtime.KnockbackVy = value;
        }

        /// <summary>累计击退 Z 速度。</summary>
        public double KnockbackVz
        {
            get => Runtime.KnockbackVz;
            set => Runtime.KnockbackVz = value;
        }

        /// <summary>震屏计时器。</summary>
        public int ShakeTimer
        {
            get => Runtime.ShakeTimer;
            set => Runtime.ShakeTimer = value;
        }

        /// <summary>攻击豁免计数器；角色类改用 HitCounters 存储。</summary>
        public virtual int AttackExempt
        {
            get => Runtime.AttackExempt;
            set => Runtime.AttackExempt = value;
        }

        public virtual int HitStateCount
        {
            get => Runtime.HitStateCount;
            set => Runtime.HitStateCount = value;
        }

        public virtual int HitConfirmCounter
        {
            get => Runtime.HitConfirmEa;
            set => Runtime.HitConfirmEa = value;
        }

        /// <summary>生成者实体索引，opoint 生成时写入。</summary>
        public int OwnerEntityIndex
        {
            get => Runtime.OwnerSlotIndex;
            set => Runtime.OwnerSlotIndex = value;
        }

        /// <summary>发射/生成计数。</summary>
        public int ShotCount
        {
            get => Runtime.ShotCount;
            set => Runtime.ShotCount = value;
        }

        /// <summary>C++ release ai_controlled 标记；角色生成后由输入准备阶段消费。</summary>
        public bool AiControlled
        {
            get => Runtime.AiControlled;
            set => Runtime.AiControlled = value;
        }

        /// <summary>itr 攻击冷却跟踪器。</summary>
        public virtual LF2ItrRestTracker ItrRest { get; protected set; } = null;

        /// <summary>生命和资源状态。</summary>
        public virtual LF2Health Health { get; protected set; } = null;

        /// <summary>HP 恢复计时器。</summary>
        public virtual int HealTimer
        {
            get => Runtime.HealTimer;
            set => Runtime.HealTimer = value;
        }

        public virtual int CatchTimer
        {
            get => Runtime.CatchTimer;
            set => Runtime.CatchTimer = value;
        }

        /// <summary>C++ release kill_count；-1 表示普通实体，&gt;=0 表示关联的生成者/归属槽。</summary>
        public int KillCount
        {
            get => Runtime.KillCount;
            set => Runtime.KillCount = value;
        }

        /// <summary>C++ release combo_count_vic；累计承受的连击伤害统计。</summary>
        public int ComboCountVic
        {
            get => Runtime.ComboCountVic;
            set => Runtime.ComboCountVic = value;
        }

        /// <summary>C++ release combo_count_atk；累计造成的连击伤害统计。</summary>
        public int ComboCountAtk
        {
            get => Runtime.ComboCountAtk;
            set => Runtime.ComboCountAtk = value;
        }

        /// <summary>C++ release kill_stat；击杀统计。</summary>
        public int KillStat
        {
            get => Runtime.KillStat;
            set => Runtime.KillStat = value;
        }

        /// <summary>C# authority Entity.Unk344；索引 1..2 指向全局击杀/伤害统计槽。</summary>
        public int Unk344
        {
            get => Runtime.Unk344;
            set => Runtime.Unk344 = value;
        }

        /// <summary>C++ release weapon_count；角色受笛子命中时可为负，武器侧用于飞行/笛子累计。</summary>
        public int WeaponCount
        {
            get => Runtime.WeaponCount;
            set => Runtime.WeaponCount = value;
        }

        /// <summary>C++ release fall_damage_div；落地持续扣血分支的伤害缩放除数。</summary>
        public int FallDamageDiv
        {
            get => Runtime.FallDamageDiv;
            set => Runtime.FallDamageDiv = value;
        }

        /// <summary>C++ release 原始 HP 备份字段。</summary>
        public int HPOrig
        {
            get => Runtime.HPOrig;
            set => Runtime.HPOrig = value;
        }

        /// <summary>C++ release 原始 HP2/残机备份字段。</summary>
        public int HP2Orig
        {
            get => Runtime.HP2Orig;
            set => Runtime.HP2Orig = value;
        }

        /// <summary>C++ release 复活血量配置字段；0 表示走普通复活次数路径。</summary>
        public int RespawnCount
        {
            get => Runtime.RespawnCount;
            set => Runtime.RespawnCount = value;
        }

        /// <summary>C# 基线 presentation `PpDisplay`；输入扣费与帧推进回退维护的 PP 表现层累计面。</summary>
        public int PpDisplay
        {
            get => Runtime.PpDisplay;
            set => Runtime.PpDisplay = value;
        }

        protected bool IsPpModeEnabled()
        {
            return Match?.PpMode ?? NTSDGlobal.MPEnabled;
        }

        public int HitCount
        {
            get => Runtime.HitCount;
            set => Runtime.HitCount = value;
        }

        public int HitConfirm2
        {
            get => Runtime.HitConfirm2;
            set => Runtime.HitConfirm2 = value;
        }

        public virtual int FallCounter
        {
            get => Runtime.Fall;
            set => Runtime.Fall = value;
        }

        public int TransformOriginalObjectId
        {
            get => Runtime.TransformOriginalObjectId;
            set => Runtime.TransformOriginalObjectId = value;
        }

        public int TransformTargetObjectId
        {
            get => Runtime.TransformTargetObjectId;
            set => Runtime.TransformTargetObjectId = value;
        }

        public int CaughtSlotIndex
        {
            get => Runtime.CaughtSlotIndex;
            set => Runtime.CaughtSlotIndex = value;
        }

        public int CatcherSlotIndex
        {
            get => Runtime.CatcherSlotIndex;
            set => Runtime.CatcherSlotIndex = value;
        }

        public int HolderCopySlot
        {
            get => Runtime.HolderCopySlotIndex;
            set => Runtime.HolderCopySlotIndex = value;
        }

        public int RelationOwnerSlot
        {
            get => Runtime.RelationOwnerSlotIndex;
            set => Runtime.RelationOwnerSlotIndex = value;
        }

        public int SpawnerEntityIndex
        {
            get => Runtime.SpawnerSlotIndex;
            set => Runtime.SpawnerSlotIndex = value;
        }

        private bool _hasForcedRuntimeIntPosition;



        /// <summary>阴影 SpriteRenderer，由渲染器注入。</summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>注入阴影渲染器引用。</summary>
        public void SetShadowRenderer(SpriteRenderer sr) => ShadowRenderer = sr;

        /// <summary>更新阴影位置和显示状态。</summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (ShadowRenderer == null || Runtime == null) return;

            int state = Frame?.D?.state ?? -1;
            int oid = ObjectId;
            bool hide = state == 3005
                     || state == 9997
                     || (Runtime?.LinkState ?? 0) < 0
                     || oid == 223
                     || oid == 224;

            ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                var t = ShadowRenderer.transform;
                Sprite shadowSprite = ShadowRenderer.sprite;
                float shadowWidth = shadowSprite != null ? shadowSprite.rect.width : 0f;
                float shadowHeight = shadowSprite != null ? shadowSprite.rect.height : 0f;

                // C# 基准工程先计算阴影绘制矩形：
                // left = x + renderOffsetX - cameraX - shadowW / 2
                // top  = z - shadowH / 2
                // Unity Sprite 默认中心 pivot，这里把矩形换算回中心点。
                int cameraX = Match?.ReleaseCameraX ?? 0;
                int renderOffsetX = (int)GetRenderOffsetX();
                float shadowLeft = GetRuntimeXInt() + renderOffsetX - cameraX - shadowWidth * 0.5f;
                float shadowTop = GetRenderZInt() - shadowHeight * 0.5f;
                float shadowCenterX = shadowLeft + shadowWidth * 0.5f;
                float shadowCenterY = shadowTop + shadowHeight * 0.5f;
                Vector3 worldPos = NTSDRenderSpace.ScreenPixelToWorld(shadowCenterX, shadowCenterY, t.position.z);
                t.position = NTSDRenderSpace.SnapWorldPosition(worldPos);
            }
        }



        /// <summary>命中记录数量，对齐 C# 基线 Entity.HitRecordCount。</summary>
        public int HitRecordCount { get; private set; } = 0;

        /// <summary>最大命中记录数量，对齐 C# 基线的 10 槽。</summary>
        public const int MaxHitRecordSlots = 10;

        private readonly int[] _hitRecordDamage = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordX = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordZ = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordLastAdvanceTick = new int[MaxHitRecordSlots];

        /// <summary>追加一条命中记录，供 SparkRenderer 按 C# 基线渲染。</summary>
        public void AddHitRecord(int age, int anchorX, int anchorZ)
        {
            if (HitRecordCount >= MaxHitRecordSlots)
                return;

            int slot = HitRecordCount++;
            _hitRecordDamage[slot] = age;
            _hitRecordX[slot] = anchorX;
            _hitRecordZ[slot] = anchorZ;
            _hitRecordLastAdvanceTick[slot] = int.MinValue;
        }

        /// <summary>记录一次 kind 0 命中；由受击对象调用。</summary>
        internal void RecordKind0Hit(LF2Entity attacker, InteractionArea itr)
        {
            if (attacker == null || itr == null)
                return;

            int attackerZ = attacker.Runtime.ZInt;
            int victimZ = Runtime.ZInt;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int victimSlot = Runtime.SlotIndex;
            LF2Entity recordOwner = attackerZ > victimZ ||
                                    (attackerZ == victimZ && attackerSlot > victimSlot)
                ? attacker
                : this;

            if (recordOwner.HitRecordCount >= MaxHitRecordSlots)
                return;

            int sparkPhase = itr.effect == 1 ? 1 : 0;
            int timer = itr.fall > 60 ? sparkPhase * 20 : sparkPhase * 20 + 10;
            LF2FrameData attackerFrame = attacker.GetFrameDataById(attacker.Frame?.N ?? 0) ?? attacker.Frame?.D;
            int attackerCenterX = attackerFrame?.centerx ?? 0;
            int attackerCenterY = attackerFrame?.centery ?? 0;
            int attackerX = attacker.Runtime.XInt;
            int attackerY = attacker.Runtime.YInt;
            int victimX = Runtime.XInt;
            int victimY = Runtime.YInt;

            int hitX;
            if (attacker.Dirh() > 0)
            {
                hitX = attackerX - attackerCenterX + itr.x + itr.w;
                if (hitX > victimX)
                    hitX = victimX;
            }
            else
            {
                hitX = attackerX + attackerCenterX - itr.x - itr.w;
                if (hitX < victimX)
                    hitX = victimX;
            }

            int hitYOffset = attackerY + (itr.h / 2) + itr.y - attackerCenterY;
            int lowerY = victimY - attackerCenterY;
            if (hitYOffset < lowerY)
                hitYOffset = (lowerY + hitYOffset) >> 1;
            else if (hitYOffset > victimY)
                hitYOffset = (victimY + hitYOffset) >> 1;

            int hitZ = attackerZ + hitYOffset + BattleRandInt(0, 9) - 4;
            hitX += BattleRandInt(0, 9) - 4;
            recordOwner.AddHitRecord(timer, hitX, hitZ);
        }

        /// <summary>读取指定命中记录年龄。</summary>
        public int GetHitRecordAge(int slotIndex) => _hitRecordDamage[slotIndex];

        /// <summary>读取指定命中记录 X 锚点。</summary>
        public int GetHitRecordX(int slotIndex) => _hitRecordX[slotIndex];

        /// <summary>读取指定命中记录 Z 锚点。</summary>
        public int GetHitRecordZ(int slotIndex) => _hitRecordZ[slotIndex];

        /// <summary>命中记录成功渲染后推进年龄。</summary>
        public void AdvanceHitRecord(int slotIndex, int tickIndex)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount)
                return;

            if (_hitRecordLastAdvanceTick[slotIndex] == tickIndex)
                return;

            _hitRecordDamage[slotIndex]++;
            _hitRecordLastAdvanceTick[slotIndex] = tickIndex;
        }

        /// <summary>仅当该记录位于尾槽时移除，对齐 C# 基线尾槽回收规则。</summary>
        public bool RemoveHitRecordIfTail(int slotIndex)
        {
            if (slotIndex != HitRecordCount - 1)
                return false;

            RemoveHitRecord(slotIndex);
            return true;
        }

        private void RemoveHitRecord(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount)
                return;

            int tail = HitRecordCount - 1;
            if (slotIndex < tail)
            {
                System.Array.Copy(_hitRecordDamage, slotIndex + 1, _hitRecordDamage, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordX, slotIndex + 1, _hitRecordX, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordZ, slotIndex + 1, _hitRecordZ, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordLastAdvanceTick, slotIndex + 1, _hitRecordLastAdvanceTick, slotIndex, tail - slotIndex);
            }

            _hitRecordDamage[tail] = 0;
            _hitRecordX[tail] = 0;
            _hitRecordZ[tail] = 0;
            _hitRecordLastAdvanceTick[tail] = 0;
            HitRecordCount--;
        }

        protected void ResetSpark()
        {
            HitRecordCount = 0;
            System.Array.Clear(_hitRecordDamage, 0, _hitRecordDamage.Length);
            System.Array.Clear(_hitRecordX, 0, _hitRecordX.Length);
            System.Array.Clear(_hitRecordZ, 0, _hitRecordZ.Length);
            System.Array.Clear(_hitRecordLastAdvanceTick, 0, _hitRecordLastAdvanceTick.Length);
        }



        /// <summary>Unity 保留的状态事件入口；具体行为以 C++ release 运行时为准。</summary>
        protected virtual bool StateExitEvent() => false;
        protected virtual bool StateEntryEvent() => false;
        protected virtual bool FrameEvent() => false;
        protected virtual bool TransitEvent() => false;
        protected virtual bool TUEvent() => false;
        protected virtual bool DieEvent() => false;
        protected virtual bool DestroyEvent() => false;

        /// <summary>获取当前状态。</summary>
        public virtual int GetState() => Frame.D?.state ?? 0;

        public virtual void SwitchDir(string dir)
        {
            string nextDir = dir == "left" ? "left" : "right";
            Runtime.Dir = nextDir;
            if (PS != null)
                PS.dir = nextDir;
            Sprite?.SwitchLR(nextDir);
        }

        public virtual int Dirh() => Runtime.Dir == "left" ? -1 : 1;

        public virtual int Dirv() => 1;

        protected virtual string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return parentDir == "right" ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }



        /// <summary>受到 itr kind=10/11 时的受力处理，角色和武器共用。</summary>
        public virtual void FluteForce()
        {
            if (Runtime == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            float lowLevel = -140f;
            float midLevel = -160f;
            float highLevel = -180f;

            Effect.Super = true;
            Runtime.Vx = 0;
            Runtime.Vz = 0;

            if (Runtime.Y > lowLevel)
                Runtime.Vy = (Runtime.Vy <= 0) ? -7.5f : -Runtime.Vy / 2f;
            else if (Runtime.Y <= lowLevel && Runtime.Y > midLevel)
                Runtime.Vy -= mass / 2f;
            else if (Runtime.Y <= midLevel && Runtime.Y > highLevel)
                Runtime.Vy += mass / 2f;

            switch ((LF2ObjectType)GetCurrentDataObjectType())
            {
                case LF2ObjectType.Character:
                    if (Frame.N >= 55) ImmediateFrame(40);
                    break;
                case LF2ObjectType.HeavyWeapon:
                    if (Frame.N >= 5) ImmediateFrame(1);
                    break;
            }
        }



        /// <summary>写入实体位置。</summary>
        public void SetPos(float x, float y, float z)
        {
            Runtime.SetPosition(x, y, z);
        }

        /// <summary>创建武器破碎碎片特效。</summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
            SpawnBrokenWeaponFragments(id);
        }

        protected void SpawnBrokenWeaponFragments(int sourceOid)
        {
            int count = BrokenWeaponFragmentCount(sourceOid);
            if (count <= 0 || Runtime == null) return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            for (int i = 0; i < count; i++)
            {
                int x = (int)Runtime.X + RandInt(0, 7) - 3;
                int y = (int)Runtime.Y + RandInt(0, 7) - 3;
                float vx = RandInt(0, 11) - 5f;
                float vy = BrokenWeaponFragmentVy(sourceOid, i);
                int frame = BrokenWeaponFragmentFrame(sourceOid, i);

                var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 999,
                    kind = 0,
                    action = frame,
                    facing = Runtime.Dir == "right" ? 0 : 1,
                    x = 0,
                    y = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0
                };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(x, y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.useDirectVelocity = true;
                task.directVx = vx;
                task.directVy = vy;
                task.directVz = 0f;
                task.releaseSpawnSemantic = LF2Tasks.ReleaseSpawnSemantic.BrokenFragment;
                factory.EnqueueCreateObject(task);
            }
        }

        private int BrokenWeaponFragmentCount(int oid)
        {
            if (oid == 101 || oid == 218) return 7;
            if (oid == 100 || oid == 213 || oid == 217) return 5;
            if (oid == 201 || oid == 120 || oid == 124) return 3;
            if (oid == 150) return 13;
            if (oid == 151) return 15;
            if (oid == 121) return 4;
            if (oid == 122 || oid == 123) return 9;
            return 0;
        }

        private float BrokenWeaponFragmentVy(int oid, int fragmentIndex)
        {
            if (oid == 150 || oid == 151 || oid == 213)
                return -(RandInt(0, 20) / 2f) - 8f;

            if (oid == 100 || oid == 101 || oid == 201 || oid == 120 || oid == 121 ||
                oid == 122 || oid == 123 || oid == 124 || oid == 217 || oid == 218)
            {
                if ((oid == 122 || oid == 123) && fragmentIndex >= 3)
                    return -(RandInt(0, 18) / 2f) - 4f;

                return -(RandInt(0, 8) / 2f) - 6f;
            }

            return 0f;
        }

        private int BrokenWeaponFragmentFrame(int oid, int fragmentIndex)
        {
            if (oid == 150) return RandInt(0, 4) + (fragmentIndex < 5 ? 0 : 4);
            if (oid == 100) return RandInt(0, 4) + (fragmentIndex < 2 ? 10 : 14);
            if (oid == 213) return RandInt(0, 4) + (fragmentIndex < 2 ? 150 : 154);
            if (oid == 101)
            {
                if (fragmentIndex < 5) return RandInt(0, 2) * 4 + RandInt(0, 4) + 20;
                return RandInt(0, 4) + 30;
            }
            if (oid == 151)
            {
                if (fragmentIndex < 2) return RandInt(0, 4) + 40;
                if (fragmentIndex < 5) return RandInt(0, 4) + 44;
                if (fragmentIndex < 8) return RandInt(0, 4) + 50;
                return RandInt(0, 4) + 54;
            }
            if (oid == 120) return RandInt(0, 4) + (fragmentIndex < 2 ? 54 : 30);
            if (oid == 124) return RandInt(0, 4) + 170;
            if (oid == 121) return RandInt(0, 4) + 60;
            if (oid == 122)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 70;
                if (fragmentIndex < 3) return RandInt(0, 4) + 80;
                return RandInt(0, 4) + 74;
            }
            if (oid == 123)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 160;
                if (fragmentIndex < 3) return RandInt(0, 4) + 164;
                return RandInt(0, 4) + 74;
            }
            if (oid == 217 || oid == 218) return RandInt(0, 4) + 174;
            return 0;
        }

        /// <summary>正式战斗随机数入口，对应 C++ release 的 ntsd_rand()。</summary>
        public int BattleRandInt(int minInclusive, int maxExclusive)
            => RandInt(minInclusive, maxExclusive);

        protected int RandInt(int minInclusive, int maxExclusive)
        {
            var rng = Match?.Rng;
            if (rng != null) return rng.NextInt(minInclusive, maxExclusive);
            return FallbackRng.NextInt(minInclusive, maxExclusive);
        }

        /// <summary>检查 itr arest 冷却是否允许攻击。</summary>
        public bool ItrArestTest() => ItrRest == null || ItrRest.Arest <= 0;

        internal static int ResolveArestCooldown(int arest, int vrest)
        {
            return arest < 4 && vrest == 0 ? 4 : arest;
        }

        /// <summary>命中后更新 arest 冷却。</summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;
            if (itr == null || SuppressesGenericArest(itr.kind)) return;

            ItrRest.Arest = ResolveArestCooldown(itr.arest, itr.vrest);
        }

        /// <summary>检查指定攻击者的 vrest 冷却是否结束。</summary>
        public bool ItrVrestTest(int uid) => ItrRest == null || !ItrRest.HasVrest(uid);

        /// <summary>更新指定攻击者的 vrest 冷却。</summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            if (SuppressesGenericVrest(itr.kind)) return;
            if (itr.vrest > 0)
                ItrRest.SetVrest(attackerUid, itr.vrest);
        }

        /// <summary>更新击飞路径的 vrest 冷却，固定写 45。</summary>
        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr)
        {
            ItrVrestUpdate(attackerUid, itr);
        }

        private static bool SuppressesGenericArest(int kind)
        {
            return kind == 8 || kind == 10 || kind == 11 || kind == 14 || kind == 15 || kind == 16;
        }

        private static bool SuppressesGenericVrest(int kind)
        {
            return kind == 8 || kind == 10 || kind == 11 || kind == 14 || kind == 15;
        }

        public bool ItrVrestTest(int uid, bool releaseRuntimeSlot) => ItrVrestTest(uid);

        public void ItrVrestUpdate(int attackerUid, InteractionArea itr, bool releaseRuntimeSlot)
            => ItrVrestUpdate(attackerUid, itr);

        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr, bool releaseRuntimeSlot)
            => ItrVrestUpdateKnockdown(attackerUid, itr);

        protected bool TryApplyKind6HitConfirm(InteractionArea itr, LF2Entity target)
        {
            if (itr?.kind != 6 || target == null || target == this)
                return false;
            if (target.Runtime == null || target.Frame?.D == null)
                return false;
            if (target.Health != null && target.Health.HP <= 0)
                return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr))
                return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target))
                return false;
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(this, itr, target))
                return false;

            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true))
                return false;

            target.HitConfirmCounter = 3;
            return true;
        }

        internal bool TryApplyKind6HitConfirmForCharacterDatInteraction(InteractionArea itr, LF2Entity target)
            => TryApplyKind6HitConfirm(itr, target);

        protected void ApplyKind14DirectionalBlockFrom(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || Runtime == null)
                return;

            double attackerX = attacker.Runtime.X;
            double attackerZ = attacker.Runtime.Z;
            double victimX = Runtime.X;
            double victimZ = Runtime.Z;

            if (attackerX > victimX + 5f && (Runtime.Vx > 0f || KnockbackVx > 0f))
                Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5f && (Runtime.Vx < 0f || KnockbackVx < 0f))
                Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2f && (Runtime.Vz > 0f || KnockbackVz > 0f))
                Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2f && (Runtime.Vz < 0f || KnockbackVz < 0f))
                Runtime.ZBoundNegative = true;
        }

        /// <summary>立即写入指定帧，绕过 wait 推进。</summary>
        // 这是最直接的硬切帧入口：
        // 当前帧会立刻变成目标帧，不等待 FrameTransistor 下一拍再处理。
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = targetFrame;
            AttackingCounter = 0;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        /// <summary>按帧 ID 获取帧数据。</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
            => FrameCache?.GetFrameDataById(frameId);

        /// <summary>请求跳转到指定帧。</summary>
        // 对外的标准跳帧入口，默认 wait=0。
        public virtual void TransitionToFrame(int frameId)
            => TransitionToFrame(frameId, 0);

        /// <summary>请求跳转到指定帧。</summary>
        // 和 ImmediateFrame 的区别在于：这里是把请求交给 FrameTransistor，
        // 让它按正式 frame_tick 顺序在后续推进里消费。
        public virtual void TransitionToFrame(int frameId, int wait = 0)
        {
            if (Trans == null)
                return;

            Trans.SetNext(frameId);
            Trans.SetWait(wait);
        }

        /// <summary>获取碰撞用 sprite 宽度，单位为像素。</summary>
        public virtual float GetSpriteWidthPxForCollision() => 0f;



        public abstract void Reset();
        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>从 SimulationWorld 注销自身。</summary>
        public virtual void UnregisterFromWorld()
        {
            Match?.Unregister(this);
        }

        /// <summary>销毁当前对象的可视表现。</summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>FrameTransistor 检测到 next=1000 时调用，子类可实现销毁逻辑。</summary>
        public virtual void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            else
            {
                UnregisterFromWorld();
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        // FrameTransistor 真正执行换帧时，会先走到这里。
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans, Trans?.WaitCounter ?? 0);
        }

        /// <summary>帧转换回调，子类实现具体帧切换逻辑。</summary>
        // 需要额外参考 oldLock 或保留更细对齐语义时，子类实现这个重载。
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) { }



        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx)
        {
            registeredWorld = ctx?.World;
            RefreshRuntimeSnapshot();
        }

        public virtual void OnRemoved(SimContext ctx)
        {
            if (ReferenceEquals(registeredWorld, ctx?.World))
                registeredWorld = null;
            TrackerParent = null;
            Runtime.SlotIndex = -1;
        }

        internal LF2Entity ResolveTrackerParentFromRuntime()
        {
            int selfSlot = Runtime?.SlotIndex ?? -1;
            int parentSlot = Runtime?.HolderStableId ?? -1;
            if ((Runtime?.LinkState ?? 0) >= 0 || selfSlot < 0 || parentSlot < 0)
            {
                TrackerParent = null;
                return null;
            }

            LF2Entity parent = Match?.FindEntityByRuntimeSlotForQuery(parentSlot);
            if (parent == null && (TrackerParent?.Runtime?.SlotIndex ?? -1) == parentSlot)
                parent = TrackerParent;

            if (parent?.Runtime == null || parent.Runtime.LinkState <= 0 ||
                parent.Runtime.TargetSlotIndex != selfSlot)
            {
                TrackerParent = null;
                return null;
            }

            TrackerParent = parent;
            return parent;
        }

        public virtual void SimTransit(int tickIndex) { }
        public virtual void SimTU(int tickIndex) { }
        public virtual void SimPostInteraction(int tickIndex)
        {
            if (!UsesCharacterDatInteractionPhase())
                return;

            LF2CharacterDatInteractionResolver.TryConsumeUnifiedStep7CandidateSequence(this);
        }
        public virtual void SimObjectInteraction(int tickIndex) { }
        public virtual void SimPreInteraction(int tickIndex) { }
        public virtual void SimEntityCollision(int tickIndex) { }
        public virtual void SimFrameTick(int tickIndex) { }

        /// <summary>模拟后期更新，默认刷新渲染深度。</summary>
        public virtual void SimLateTick(int tickIndex)
        {
            Sprite?.SetZ(GetRenderSortingOrder());
        }

        public virtual void RunFrameLogicBeforeAdvance()
        {
            RunCurrentDatFrameLogicBeforeAdvance();
        }

        private void RunCurrentDatFrameLogicBeforeAdvance()
        {
            int hitFa = Frame?.D?.hit_Fa ?? 0;
            if (Runtime == null || (hitFa != 1 && hitFa != 2 && hitFa != 3 && hitFa != 4 && hitFa != 5 && hitFa != 6 && hitFa != 7 && hitFa != 8 && hitFa != 9 && hitFa != 10 && hitFa != 11 && hitFa != 12 && hitFa != 13 && hitFa != 14))
                return;

            if (hitFa == 1)
            {
                RunHitFa1FrameLogic();
                return;
            }

            if (hitFa == 3)
            {
                RunHitFa3FrameLogic();
                return;
            }

            if (hitFa == 2 || hitFa == 4 || hitFa == 12 || hitFa == 14)
            {
                RunHitFa2Or4Or12Or14FrameLogic(hitFa);
                return;
            }

            if (hitFa == 10)
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 1.1f;
                else
                    Runtime.Vx += 1.1f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -30.0, 30.0);
                if (Runtime.Y > 3f)
                    Runtime.Y = 3f;

                SwitchDir(Runtime.Vx > 0f ? "right" : "left");
                Runtime.YInt = (int)Runtime.Y;
                return;
            }

            if (hitFa == 6 || hitFa == 9)
            {
                RunHitFa6Or9FrameLogic(hitFa);
                return;
            }

            if (hitFa == 8)
            {
                RunHitFa8FrameLogic();
                return;
            }

            if (hitFa == 11)
            {
                RunHitFa11FrameLogic();
                return;
            }

            if (hitFa == 13)
            {
                RunHitFa13FrameLogic();
                return;
            }

            if (hitFa == 5)
            {
                RunHitFa5FrameLogic();
                return;
            }

            RunHitFa7FrameLogic();
        }

        private void RunHitFa1FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(1);
            if (target == null || target.Health == null || target.Health.HP <= 0)
            {
                if (Health != null)
                    Health.HP = 0;
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 1);
            int selfZ = GetFrameLogicTargetZInt(this, 1);

            if (targetX > selfX)
                Runtime.Vx += 0.85f;
            if (targetX < selfX)
                Runtime.Vx -= 0.85f;
            if (targetZ > selfZ + 7)
                Runtime.Vz += 0.3f;
            if (targetZ < selfZ - 7)
                Runtime.Vz -= 0.3f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f鈫?.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 10f < target.Runtime.Y)
                    Runtime.Y += 1.2f;
                if (Runtime.Y + 10f > target.Runtime.Y)
                    Runtime.Y -= 1.2f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -13.0, 13.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.0, 2.0);
            if (Runtime.Y > 1f)
                Runtime.Y = 1f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private void RunHitFa3FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(3);
            if (target == null)
            {
                if (Health != null)
                    Health.HP = 0;

                return;
            }

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa3NoTargetDrift();
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 3);
            int selfZ = GetFrameLogicTargetZInt(this, 3);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 10)
                Runtime.Vz += 0.17f;
            if (targetZ < selfZ - 10)
                Runtime.Vz -= 0.17f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -16.0, 16.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.4, 2.4);
        }

        private void RunHitFa8FrameLogic()
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            var allObjects = new List<LF2Entity>(16);
            Match?.GetAllEntities(allObjects);

            var enemies = new List<int>(8);
            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int i = 0; i < allObjects.Count; i++)
            {
                LF2Entity obj = allObjects[i];
                if (IsDeadLikeFrameLogicTarget(obj))
                    continue;
                if (!IsCharacterFrameLogicTarget(obj))
                    continue;
                if (ResolveFrameLogicRelationIdentity(obj) == selfTeam)
                    continue;

                int enemySlot = GetRuntimeSlotOrNegative(obj);
                if (enemySlot < 0)
                    continue;

                enemies.Add(enemySlot);
            }

            int count = 3;
            if (enemies.Count > 4)
                count = (enemies.Count - 3) / 2 + 3;

            int facing = Runtime.Dir == "right" ? 0 : 1;
            for (int i = 0; i < count; i++)
            {
                float directVx = RandInt(0, 21) - 11;
                float directVy = 3.0f - RandInt(0, 24) * 0.25f;
                float directVz = 3.0f - RandInt(0, 24) * 0.25f;
                int ownerSlot = enemies.Count > 0
                    ? enemies[RandInt(0, enemies.Count)]
                    : GetRuntimeSlotOrNegative(this);

                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 225,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = facing,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.dvz = 0f;
                task.useDirectVelocity = true;
                task.directVx = directVx;
                task.directVy = directVy;
                task.directVz = directVz;
                task.ownerEntityIndex = ownerSlot;
                FillHitFa8SpawnTask(task);
                factory.EnqueueCreateObject(task);
            }

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa6Or9FrameLogic(int hitFa)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            var allObjects = new List<LF2Entity>(16);
            Match?.GetAllEntities(allObjects);

            int selfTeam = ResolveFrameLogicRelationIdentity();
            int max = hitFa == 9 ? 10 : 7;
            int maxPerLaterPass = hitFa == 9 ? 4 : 0;
            int spawnCount = 0;
            int loopCount = 0;
            bool spawnedThisLoop;

            do
            {
                spawnedThisLoop = false;
                for (int i = 0; i < allObjects.Count && spawnCount < max; i++)
                {
                    LF2Entity obj = allObjects[i];
                    if (IsDeadLikeFrameLogicTarget(obj))
                        continue;
                    if (!IsCharacterFrameLogicTarget(obj))
                        continue;
                    if (ResolveFrameLogicRelationIdentity(obj) == selfTeam)
                        continue;
                    if (!(spawnCount < maxPerLaterPass || loopCount == 0))
                        continue;

                    int enemySlot = GetRuntimeSlotOrNegative(obj);
                    if (enemySlot < 0)
                        continue;

                    int oid;
                    float vx;
                    float vy;
                    if (hitFa == 6)
                    {
                        oid = 220;
                        vx = (float)((obj.Runtime.X - Runtime.X) / 50.0f);
                        vy = -(4 + RandInt(0, 4));
                    }
                    else
                    {
                        oid = RandInt(0, 2) + 221;
                        vx = RandInt(0, 21) - 11;
                        vy = -2.0f - RandInt(0, 40) * (1.0f / 6.0f);
                    }

                    OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                    task.opoint = new ObjectPoint
                    {
                        oid = oid,
                        kind = 0,
                        action = 0,
                        dvx = 0,
                        dvy = 0,
                        dvz = 0,
                        facing = Runtime.Dir == "right" ? 0 : 1,
                    };
                    task.parent = this;
                    task.team = Team;
                    task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                    task.z = (float)Runtime.Z;
                    task.dir = Runtime.Dir;
                    task.dvz = 0f;
                    task.useDirectVelocity = true;
                    task.directVx = vx;
                    task.directVy = vy;
                    task.directVz = 0f;
                    task.ownerEntityIndex = enemySlot;
                    FillHitFa8SpawnTask(task);
                    factory.EnqueueCreateObject(task);

                    spawnCount++;
                    spawnedThisLoop = true;
                }

                loopCount++;
            } while (hitFa == 9 &&
                     spawnCount < maxPerLaterPass &&
                     spawnCount > 0 &&
                     spawnedThisLoop &&
                     spawnCount < max);

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa2Or4Or12Or14FrameLogic(int hitFa)
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(hitFa);
            bool rawSlotTarget = hitFa == 4 && target == null && IsReferenceRuntimeSlot(OwnerEntityIndex);

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            bool targetHasHp = target != null
                ? target.Health != null && target.Health.HP > 0
                : rawSlotTarget;
            if (hitFa == 4 && targetHasHp)
            {
                int dx = (target?.GetRuntimeXInt() ?? 0) - GetRuntimeXInt();
                int dy = (target?.GetRuntimeYInt() ?? 0) - GetRuntimeYInt();
                int dz = (target != null ? GetFrameLogicZInt(target) : 0) - GetFrameLogicZInt(this);
                if (dx > -30 && dx < 30 && dy > 0 && dy < 80 && dz > -10 && dz < 10)
                {
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                    SetFrameTickDirect(60);
                    if (target != null)
                        target.CatchTimer = 100;
                    return;
                }
            }

            if (target == null && !rawSlotTarget)
            {
                if (hitFa != 4 && Health != null)
                {
                    Health.HP = 0;
                    return;
                }

                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            int targetX = target?.GetRuntimeXInt() ?? 0;
            int selfX = GetRuntimeXInt();
            int targetZ = target != null ? GetFrameLogicTargetZInt(target, hitFa) : 0;
            int selfZ = GetFrameLogicTargetZInt(this, hitFa);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 5)
                Runtime.Vz += 0.4f;
            if (targetZ < selfZ - 5)
                Runtime.Vz -= 0.4f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f鈫?.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (target != null && IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 40f < target.Runtime.Y)
                    Runtime.Y += 1f;
                if (Runtime.Y + 40f > target.Runtime.Y)
                    Runtime.Y -= 1f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            if (hitFa == 14)
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -1.5, 1.5);
            else
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();

            if (hitFa == 14)
            {
                double absVx = System.Math.Abs(Runtime.Vx);
                int curFrame = Frame?.N ?? -1;
                if (absVx >= 8f)
                {
                    if (curFrame > 40)
                        SetFrameTickDirect(curFrame - 50);
                }
                else if (curFrame < 10)
                {
                    SetFrameTickDirect(curFrame + 50);
                }
            }
        }

        private void RunHitFa7FrameLogic()
        {
            if (Match != null)
                SpawnHitFa7Clone();

            LF2Entity target = null;
            int targetSlot = Runtime.OwnerSlotIndex;
            if (Match != null && targetSlot >= 0)
                target = Match.FindEntityByRuntimeSlotForQuery(targetSlot) ??
                         Match.FindEntityByRuntimeSlotIncludingPending(targetSlot);

            bool rawSlotTarget = target == null && IsReferenceRuntimeSlot(targetSlot);
            bool valid = (target != null || rawSlotTarget) && Health != null && Health.HP > 0;
            if (valid)
            {
                int targetX = target?.GetRuntimeXInt() ?? 0;
                if (targetX > GetRuntimeXInt())
                {
                    Runtime.Vx += 0.7f;
                    Runtime.Vx += 0.7f;
                }
                else if (targetX < GetRuntimeXInt())
                {
                    Runtime.Vx -= 0.7f;
                    Runtime.Vx -= 0.7f;
                }

                int targetZ = target?.Runtime?.ZInt ?? 0;
                int selfZ = Runtime.ZInt;
                if (targetZ > selfZ + 5)
                    Runtime.Vz += 0.4f;
                if (targetZ < selfZ - 5)
                    Runtime.Vz -= 0.4f;

                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
                if (Runtime.Y > 1.4f)
                    Runtime.Y = 1.4f;
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);
            }
            else
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 2f;
                else
                    Runtime.Vx += 2f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.YInt = -25;
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }
            }

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private bool IsReferenceRuntimeSlot(int runtimeSlot)
        {
            return Match != null &&
                   runtimeSlot >= 0 &&
                   runtimeSlot < Match.MaxRuntimeSlotsForServices;
        }

        private void RunHitFa13FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            var allObjects = new List<LF2Entity>(16);
            Match?.GetAllEntities(allObjects);

            var enemies = new List<int>(8);
            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int i = 0; i < allObjects.Count; i++)
            {
                LF2Entity obj = allObjects[i];
                if (IsDeadLikeFrameLogicTarget(obj))
                    continue;
                if (!IsCharacterFrameLogicTarget(obj))
                    continue;
                if (ResolveFrameLogicRelationIdentity(obj) == selfTeam)
                    continue;

                int enemySlot = GetRuntimeSlotOrNegative(obj);
                if (enemySlot < 0)
                    continue;

                enemies.Add(enemySlot);
            }

            int freeSlot = -1;
            for (int slot = 50; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null &&
                    Match.FindEntityByRuntimeSlotIncludingPending(slot) == null)
                {
                    freeSlot = slot;
                    break;
                }
            }

            if (freeSlot < 0)
            {
                Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int spawnOid = 228;
            if (CharacterAnimtorManager.Instance?.GetCharacterData(spawnOid) == null)
            {
                Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int chosenTarget = enemies.Count == 0
                ? GetRuntimeSlotOrNegative(this)
                : enemies[RandInt(0, enemies.Count)];

            float spawnY = (float)(Runtime.Y + RandInt(0, 7) - 3);
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = spawnOid,
                kind = 0,
                action = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = Runtime.Dir == "right" ? 0 : 1,
            };
            task.parent = this;
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, spawnY, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.dvz = 0f;
            task.useDirectVelocity = true;
            task.directVx = (float)Runtime.Vx;
            task.directVy = 0.1f;
            task.directVz = (float)(3.0f - RandInt(0, 24) * 0.25f + Runtime.Vz);
            task.ownerEntityIndex = chosenTarget;
            FillHitFa13SpawnTask(task);
            factory.EnqueueCreateObject(task);

            Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa5FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            if (CharacterAnimtorManager.Instance?.GetCharacterConfig(219) == null)
            {
                if (Health != null)
                    Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            var allObjects = new List<LF2Entity>(16);
            Match.GetAllEntities(allObjects);

            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int i = 0; i < allObjects.Count; i++)
            {
                LF2Entity ally = allObjects[i];
                if (IsDeadLikeFrameLogicTarget(ally))
                    continue;
                if (!IsCharacterFrameLogicTarget(ally))
                    continue;
                if (ResolveFrameLogicRelationIdentity(ally) != selfTeam)
                    continue;

                int allySlot = GetRuntimeSlotOrNegative(ally);
                if (allySlot < 0)
                    continue;

                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 219,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = "right";
                task.dvz = 0f;
                task.useDirectVelocity = true;
                task.directVx = (float)((ally.Runtime.X - Runtime.X) / 50.0f);
                task.directVy = 0f;
                task.directVz = 0f;
                task.ownerEntityIndex = allySlot;
                FillHitFa13SpawnTask(task);
                factory.EnqueueCreateObject(task);
            }

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa11FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            (int oid, int frameId, float xOff, float yOff, float zOff, float vzDelta, string dir)[] spawns =
            {
                (211, 109,    0f,    0f,  0f,  0f, Runtime.Dir),
                (221,  81,    0f, -100f,  0f,  0f, Runtime.Dir),
                (212, 100,   80f,   -3f,  0f, -7f, "right"),
                (212, 100,  100f,   -3f,  0f,  0f, "right"),
                (212, 100,   80f,   -3f,  0f,  7f, "right"),
                (212, 100,  -80f,   -3f,  0f, -7f, "left"),
                (212, 100, -100f,   -3f,  0f,  0f, "left"),
                (212, 100,  -80f,   -3f,  0f,  7f, "left"),
                (211,  50,  -30f,   -1f, -5f,  0f, "left"),
                (211,  50,   30f,   -1f, -5f,  0f, "left"),
                (211,  50,  -30f,   -1f,  2f,  0f, "right"),
                (211,  50,   30f,   -1f,  2f,  0f, "right"),
                (211,  50,    0f,   -1f, -9f,  0f, "left"),
                (211,  50,    0f,   -1f,  6f,  0f, "right"),
            };

            for (int i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];
                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = spawn.oid,
                    kind = 0,
                    action = spawn.frameId,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = spawn.dir == "right" ? 0 : 1,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)(Runtime.X + spawn.xOff), (float)(Runtime.Y + spawn.yOff), (float)(Runtime.Z + spawn.zOff));
                task.z = (float)(Runtime.Z + spawn.zOff);
                task.dir = spawn.dir;
                task.dvz = 0f;
                task.useDirectVelocity = true;
                task.directVx = (float)Runtime.Vx;
                task.directVy = (float)Runtime.Vy;
                task.directVz = (float)(Runtime.Vz + spawn.vzDelta);
                FillHitFa13SpawnTask(task);
                factory.EnqueueCreateObject(task);
            }

            ResolveFrameLogicTargetByHitFa(11);

            if (OwnerEntityIndex < 0)
            {
                if (Health != null)
                    Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void SpawnHitFa7Clone()
        {
            if (Match == null || FrameCache?.Wrapper?.characterData == null)
                return;

            int freeSlot = -1;
            for (int slot = Match.DynamicRuntimeSlotStartForServices; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null &&
                    Match.FindEntityByRuntimeSlotIncludingPending(slot) == null)
                {
                    freeSlot = slot;
                    break;
                }
            }

            if (freeSlot < 0)
                return;

            int cloneOid = FrameCache.Wrapper.characterId;
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null || CharacterAnimtorManager.Instance?.GetCharacterConfig(cloneOid) == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = cloneOid,
                kind = 0,
                action = 40,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = 0,
            };
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            FillHitFa13SpawnTask(task);
            factory.EnqueueCreateObject(task);
        }

        private void FillHitFa13SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = (int)task.pos.x;
            task.initialRuntimeY = (int)task.pos.y;
            task.initialRuntimeZ = (int)task.pos.z;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
        }

        private void FillHitFa8SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = (int)task.pos.x;
            task.initialRuntimeY = (int)task.pos.y;
            task.initialRuntimeZ = (int)task.pos.z;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
        }

        private LF2Entity ResolveFrameLogicTargetByHitFa(int hitFa)
        {
            if (Match == null)
                return null;

            if (hitFa == 4)
            {
                return OwnerEntityIndex >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(OwnerEntityIndex) ??
                      Match.FindEntityByRuntimeSlotIncludingPending(OwnerEntityIndex)
                    : null;
            }

            int selfTeam = ResolveFrameLogicRelationIdentity();
            int holderTeam = -1;
            if (SpawnerEntityIndex >= 0)
            {
                LF2Entity spawner = Match.FindEntityByRuntimeSlotForQuery(SpawnerEntityIndex) ??
                                    Match.FindEntityByRuntimeSlotIncludingPending(SpawnerEntityIndex);
                if (spawner != null)
                    holderTeam = ResolveFrameLogicRelationIdentity(spawner);
            }

            int currentTargetSlot = OwnerEntityIndex;
            bool needScan = true;
            LF2Entity target = currentTargetSlot >= 0
                ? Match.FindEntityByRuntimeSlotForQuery(currentTargetSlot) ??
                  Match.FindEntityByRuntimeSlotIncludingPending(currentTargetSlot)
                : null;

            if (target != null)
            {
                bool valid = !IsDeadLikeFrameLogicTarget(target) &&
                             IsCharacterFrameLogicTarget(target) &&
                             target.GetState() != LF2States.Lying &&
                             Mathf.Abs(target.HitStun) <= 2f &&
                             ResolveFrameLogicRelationIdentity(target) != selfTeam;
                if (valid && holderTeam != ResolveFrameLogicRelationIdentity(target))
                    needScan = false;
                if (!valid)
                    target = null;
            }

            if (needScan)
            {
                var allObjects = new List<LF2Entity>(16);
                Match.GetAllEntities(allObjects);

                int bestDist = 10000;
                int bestSlot = -1;
                for (int i = 0; i < allObjects.Count; i++)
                {
                    LF2Entity obj = allObjects[i];
                    if (obj == null || ReferenceEquals(obj, this))
                        continue;
                    if (IsDeadLikeFrameLogicTarget(obj))
                        continue;
                    if (!IsCharacterFrameLogicTarget(obj))
                        continue;

                    int objTeam = ResolveFrameLogicRelationIdentity(obj);
                    if (objTeam == selfTeam)
                        continue;
                    if (holderTeam >= 0 && objTeam == holderTeam)
                        continue;
                    if ((obj.GetState() == LF2States.Lying || Mathf.Abs(obj.HitStun) > 2f) && currentTargetSlot != -1)
                        continue;

                    int dist = Mathf.Abs(obj.GetRuntimeXInt() - GetRuntimeXInt()) +
                               Mathf.Abs(GetFrameLogicTargetZInt(obj, hitFa) - GetFrameLogicTargetZInt(this, hitFa));
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestSlot = GetRuntimeSlotOrNegative(obj);
                    }
                }

                OwnerEntityIndex = bestSlot;
                target = bestSlot >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(bestSlot) ??
                      Match.FindEntityByRuntimeSlotIncludingPending(bestSlot)
                    : null;
            }

            return target;
        }

        private int ResolveFrameLogicRelationIdentity()
        {
            return ResolveFrameLogicRelationIdentity(this);
        }

        private static int ResolveFrameLogicRelationIdentity(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        private static bool IsCharacterFrameLogicTarget(LF2Entity entity)
        {
            return entity?.GetCurrentDataObjectType() == (int)LF2ObjectType.Character;
        }

        private static bool IsDeadLikeFrameLogicTarget(LF2Entity entity)
        {
            if (entity == null)
                return true;
            if (entity is LF2LivingObject living && living.Dead)
                return true;

            return entity.Health == null || entity.Health.HP <= 0;
        }

        private static int GetRuntimeSlotOrNegative(LF2Entity entity)
        {
            return entity?.Runtime?.SlotIndex ?? -1;
        }

        private void ApplyHitFa2Or4Or12Or14NoTargetCatch(int hitFa)
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();
        }

        private void ApplyHitFa3NoTargetDrift()
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
        }

        private void ApplyHitFa2FrameSelection()
        {
            double absVx = System.Math.Abs(Runtime.Vx);
            int curFrame = Frame?.N ?? -1;
            if (absVx > 14f)
            {
                if (curFrame != 5 && curFrame != 6)
                    SetFrameTickDirect(5);
            }
            else if (absVx > 7f)
            {
                if (curFrame != 3 && curFrame != 4)
                    SetFrameTickDirect(3);
            }
            else
            {
                if (curFrame != 1 && curFrame != 2)
                    SetFrameTickDirect(1);
            }
        }

        private static int GetFrameLogicZInt(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            if (entity.GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                entity.Runtime != null &&
                System.Math.Abs(entity.Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (int)(entity.Runtime.Z - entity.Runtime.Type3VisualZOffset);
            }

            return entity.Runtime?.ZInt ?? 0;
        }

        private static int GetFrameLogicTargetZInt(LF2Entity entity, int hitFa)
        {
            if (hitFa == 1 || hitFa == 3 || hitFa == 7 || hitFa == 12 || hitFa == 14)
                return entity?.Runtime?.ZInt ?? 0;

            return GetFrameLogicZInt(entity);
        }

        internal virtual bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
        {
            return frame != null &&
                   frame.hit_Fa > 0 &&
                   GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character;
        }

        internal bool SupportsPostInteractionPhase() => UsesCharacterDatInteractionPhase();

        internal bool SupportsObjectInteractionPhase() => !UsesCharacterDatInteractionPhase();

        protected bool UsesCharacterDatInteractionPhase()
            => GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;

        internal virtual bool UsesDynamicRuntimeSlot() => false;

        internal virtual bool IsStageBoundedCharacter() => false;

        internal virtual bool ShouldContributeToReleaseCamera() => false;

        internal virtual void ApplyPreFrameZBounds(float zMin, float zMax) { }

        // C++ PreFrame keeps the background width separate from the phase-only character override.
        internal virtual bool ApplyPreFrameXBounds(float baseStageWidth, int xMaxOverride)
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (currentDataType == (int)LF2ObjectType.SpecialAttack)
            {
                if (Runtime.X < -300f || Runtime.X > baseStageWidth + 300f)
                {
                    FreeEntityLikeExe();
                    return true;
                }
            }
            else if (currentDataType == (int)LF2ObjectType.Character)
            {
                int slotIndex = Runtime?.SlotIndex ?? StableId;
                if (slotIndex >= 20)
                {
                    if (Runtime.X < -100f)
                        Runtime.X = -100f;
                    if (Runtime.X > baseStageWidth + 100f)
                        Runtime.X = baseStageWidth + 100f;
                }
                else
                {
                    if (RelationTeam == 5)
                    {
                        if (Runtime.X < -300f)
                            Runtime.X = -300f;
                    }
                    else if (Runtime.X < 0f)
                    {
                        Runtime.X = 0f;
                    }

                    if (Runtime.X > baseStageWidth)
                        Runtime.X = baseStageWidth;

                    if (xMaxOverride > 0 &&
                        Runtime.X > xMaxOverride &&
                        RelationTeam != 5 &&
                        HitStun == 0)
                    {
                        Runtime.X = xMaxOverride;
                    }
                }
            }
            else if ((ObjectId == 122 || ObjectId == 123) && Unk344 > 0)
            {
                if (Runtime.X < 10f)
                    Runtime.X = 10f;
                if (Runtime.X > baseStageWidth - 10f)
                    Runtime.X = baseStageWidth - 10f;
            }
            else if (Runtime.YInt == 0 && (Runtime.X < 0f || Runtime.X > baseStageWidth))
            {
                FreeEntityLikeExe();
                return true;
            }

            Runtime.XInt = (int)Runtime.X;
            return false;
        }

        /// <summary>
        /// pre-collision 阶段的公共 state 特判。
        /// 对齐参考 C# `RunStateSpecialPreCollision`：
        /// - state 4000..4999：切换到 `state - 4000` 对应对象并进入 frame 0
        /// - state 8000..8999：切换到 `state - 8000` 对应对象并进入 frame 0，同时写入 140 hit stop
        /// 
        /// 这里仍然保持 Unity 当前架构边界：
        /// 只切换 `ObjectId + FrameCache`，不在这里改运行时 C# 实例类型。
        /// </summary>
        public virtual void RunStateSpecialPreCollision()
        {
            LF2FrameData frameData = Frame?.D;
            if (frameData == null)
                return;

            int state = frameData.state;
            if (state == 9995 && GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
            {
                ApplyStateDataTransform(50, false);
                return;
            }

            if (state >= 4000 && state < 5000)
            {
                ApplyStateDataTransform(state - 4000, false);
                return;
            }

            if (state >= 8000 && state < 9000)
                ApplyStateDataTransform(state - 8000, true);
        }

        internal virtual void RunPreCollisionRecoveryPhase(int tickIndex) { }

        /// <summary>
        /// 冷却递减后的输入消费阶段。
        /// 参考 C# 基准工程这里按当前 DAT `ObjType == 0` 分发角色输入；
        /// Unity 当前由 `LF2Character` 覆盖完整角色输入链；
        /// 对于“当前 DAT 已是 Character，但 CLR 运行时实例不是 LF2Character”的实体，
        /// 这里至少要补齐共享输入快照、基础 combo/direct frame jump，
        /// 以及不依赖完整角色 resolver 的 standing/walking 三个基础动作入口。
        /// </summary>
        internal virtual void RunPostCooldownInputPhase(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;

            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            if (AiControlled)
            {
                Match?.PrepareAiInputBasic(this, tickIndex);
            }
            else
            {
                UpdateSharedRuntimeInputSnapshotForSimulation(tickIndex);
            }

            if (this is LF2Character)
                return;

            RunSharedCharacterDatFrameJumpInputPhase();
            RunSharedCharacterDatStandingActionInputPhase();
            ApplyNonCharacterFrameVelocityForFrameAdvance();
        }

        internal virtual void RunCharacterInputPhase(int tickIndex)
        {
            RunPostCooldownInputPhase(tickIndex);
        }

        protected bool UsesSharedCharacterDatShellRouting()
        {
            return Runtime != null &&
                   this is not LF2Character &&
                   GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        /// <summary>
        /// 按当前运行时壳类型解析共享输入控制器。
        /// 这里不要求一定是 `LF2Character`，因为 transform 后的 current DAT character
        /// 仍然可能挂在 `LF2OtherObject` / `LF2SpecialAttack` / `LF2WeaponBase` 壳上。
        /// </summary>
        internal bool TryGetSharedInputControllerForSimulation(out ILF2Controller controller)
        {
            controller = null;

            if (this is LF2LivingObject living)
                controller = living.Controller;
            else if (this is LF2WeaponBase weapon)
                controller = weapon.Controller;

            return controller?.InputBuffer != null;
        }

        internal virtual void EnsureSharedCharacterDatControllerForSimulation()
        {
        }

        /// <summary>
        /// 把共享 controller 的输入缓冲滚入运行时输入快照。
        /// 结果菜单、battle-entry 清输入后的重新采样、post-cooldown 输入消费都可以复用这条入口。
        /// </summary>
        internal void UpdateSharedRuntimeInputSnapshotForSimulation(int tickIndex)
        {
            Runtime.RollInputFromCurrent();
            Runtime.TickInputCooldowns();

            if (!TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                return;

            UpdateSharedRuntimeInputSnapshotFromBuffer(controller.InputBuffer, tickIndex);
        }

        private void UpdateSharedRuntimeInputSnapshotFromBuffer(SimInputBuffer inputBuffer, int tickIndex)
        {
            if (inputBuffer == null || !inputBuffer.TryDequeueAll(tickIndex, out System.Collections.Generic.List<SimInputEvent> events))
                return;

            for (int i = 0; i < events.Count; i++)
                ApplySharedRuntimeInputEvent(events[i].key, events[i].down);
        }

        private void RunSharedCharacterDatFrameJumpInputPhase()
        {
            if (Runtime == null)
                return;

            sharedCharacterDatInputModule.SyncFromRuntime(Runtime);
            sharedCharacterDatInputModule.ApplyFrameInput(this);
        }

        /// <summary>
        /// shared character-DAT 的最小 standing/walking 动作桥。
        /// 这里只补不依赖 `LF2CharacterActionResolver` 的基础 walk-run/attack/jump/defend 入口，
        /// 不扩到 running/dash/catching/held-weapon/release 全动作解析。
        /// </summary>
        private void RunSharedCharacterDatStandingActionInputPhase()
        {
            if (Runtime == null || this is LF2Character)
                return;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            ApplySharedCharacterDatSpecialStateLaneControl();

            if (TryRunSharedCharacterDatJumpAttackInputPhase())
                return;
            if (TryRunSharedCharacterDatCrouchInputPhase())
                return;
            if (TryRunSharedCharacterDatDefensiveRecoveryInputPhase())
                return;
            if (TryRunSharedCharacterDatRunningInputPhase())
                return;
            if (TryRunSharedCharacterDatDashAttackInputPhase())
                return;

            if ((Frame?.N ?? -1) == LF2StandardFrames.Defend)
            {
                // 参考 C# `ApplyCharacterInput(...)`：
                // frame 110 会先按左右输入刷新 facing，然后再继续走 standing-like 输入消费。
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("right");
                else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                    SwitchDir("left");
            }

            int state = Frame?.D?.state ?? -1;
            if (state != LF2States.Standing && state != LF2States.Walking)
                return;

            if (TryRunSharedCharacterDatHeavyWalkInputPhase())
                return;

            ApplySharedCharacterDatWalkRunMovement();

            if (TryRunSharedCharacterDatStandingAttackAction())
                return;
            if (TryRunSharedCharacterDatStandingJumpAction())
                return;

            TryRunSharedCharacterDatStandingDefendAction();
        }

        private bool TryRunSharedCharacterDatStandingAttackAction()
        {
            if (!IsSharedCharacterDatAttackInputReadyInternal())
                return false;

            int linkState = Runtime?.LinkState ?? 0;
            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            if (HitConfirmCounter > 0 &&
                linkState == 0 &&
                FrameCache?.HasFrame(LF2StandardFrames.SuperPunch) == true &&
                TryCharacterDatInputFrameJump(LF2StandardFrames.SuperPunch))
            {
                return true;
            }

            if (linkState == 0)
            {
                bool usePunch = BattleRandInt(0, 2) == 0;
                int primary = usePunch ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                int fallback = usePunch ? LF2StandardFrames.Punch4 : LF2StandardFrames.Punch;
                return TryRunSharedCharacterDatStandingActionFrame(primary, fallback);
            }

            if (linkState == 101)
            {
                int primary = HasAnyDirectionInputForSharedCharacterDat()
                    ? LF2StandardFrames.LightWeaponThw
                    : RandomSharedCharacterDatWeaponAttackFrame();
                int fallback = primary == LF2StandardFrames.LightWeaponThw
                    ? 0
                    : LF2StandardFrames.LightWeaponThw;
                return TryRunSharedCharacterDatStandingActionFrame(primary, fallback);
            }

            if (linkState == 2)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.HeavyWeaponThw);

            if (linkState % 100 == 1)
                return TryRunSharedCharacterDatStandingActionFrame(RandomSharedCharacterDatWeaponAttackFrame());

            if (linkState == 4)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.LightWeaponThw);

            if (linkState == 6)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.SkyLgtWpThw);

            return false;
        }

        private bool TryRunSharedCharacterDatStandingJumpAction()
        {
            if (!IsSharedCharacterDatJumpInputReadyInternal())
                return false;

            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Jumping);
        }

        private bool TryRunSharedCharacterDatStandingDefendAction()
        {
            if (!IsSharedCharacterDatDefendInputReadyInternal(requireDefendLockOpen: true))
                return false;

            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Defend);
        }

        private bool TryRunSharedCharacterDatHeavyWalkInputPhase()
        {
            if (Runtime == null)
                return false;

            int state = Frame?.D?.state ?? -1;
            if (Runtime.LinkState != 2 || (state != LF2States.Standing && state != LF2States.Walking))
                return false;

            ApplySharedCharacterDatHeavyWalkMovement();

            if (IsSharedCharacterDatAttackInputReadyInternal() &&
                FrameCache?.HasFrame(LF2StandardFrames.HeavyWeaponThw) == true)
            {
                Runtime.AnimSub = 0;
                AttackingCounter = 0;
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.HeavyWeaponThw);
            }

            return true;
        }

        private void ApplySharedCharacterDatWalkRunMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.YInt != 0)
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = Runtime.AnimSub;
            if (animSub > 0)
                Runtime.AnimSub--;
            else if (animSub < 0)
                Runtime.AnimSub++;

            bool handled = false;
            bool vxSet = false;
            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
            {
                handled = true;
                if (Runtime.Dir == "left")
                    Runtime.AnimSub = 0;

                SwitchDir("right");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);
                Runtime.Vx = characterData.walking_speed;
                vxSet = true;

                if (Runtime.PrevRight == 0)
                    Runtime.AnimSub += 10;
                if (Runtime.AnimSub >= 11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.RunningStart);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (!handled && Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
            {
                if (Runtime.Dir == "right")
                    Runtime.AnimSub = 0;

                SwitchDir("left");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);
                Runtime.Vx = -characterData.walking_speed;
                vxSet = true;

                if (Runtime.PrevLeft == 0)
                    Runtime.AnimSub -= 10;
                if (Runtime.AnimSub <= -11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.RunningStart);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
            {
                if (!vxSet)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);

                Runtime.Vz = -characterData.walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
            else if (Runtime.KeyDown != 0 && Runtime.KeyUp == 0)
            {
                if (!vxSet)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);

                Runtime.Vz = characterData.walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        private void ApplySharedCharacterDatHeavyWalkMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.YInt != 0)
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = Runtime.AnimSub;
            if (animSub > 0)
                Runtime.AnimSub--;
            else if (animSub < 0)
                Runtime.AnimSub++;

            if ((Frame?.N ?? -1) < LF2StandardFrames.HeavyObjWalk0)
                SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);

            bool hasHorizontalMove = false;
            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
            {
                hasHorizontalMove = true;
                if (Runtime.Dir == "left")
                    Runtime.AnimSub = 0;

                SwitchDir("right");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);
                Runtime.Vx = characterData.heavy_walking_speed;

                if (Runtime.PrevRight == 0)
                    Runtime.AnimSub += 10;
                if (Runtime.AnimSub >= 11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjRun);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
            {
                hasHorizontalMove = true;
                if (Runtime.Dir == "right")
                    Runtime.AnimSub = 0;

                SwitchDir("left");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);
                Runtime.Vx = -characterData.heavy_walking_speed;

                if (Runtime.PrevLeft == 0)
                    Runtime.AnimSub -= 10;
                if (Runtime.AnimSub <= -11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjRun);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
            {
                if (!hasHorizontalMove)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);

                Runtime.Vz = -characterData.heavy_walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
            else if (Runtime.KeyDown != 0 && Runtime.KeyUp == 0)
            {
                if (!hasHorizontalMove)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);

                Runtime.Vz = characterData.heavy_walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        private bool TryRunSharedCharacterDatStandingActionFrame(int primaryFrameId, int fallbackFrameId = 0)
        {
            if (TryCharacterDatInputFrameJump(primaryFrameId))
                return true;

            if (fallbackFrameId > 0)
                return TryCharacterDatInputFrameJump(fallbackFrameId);

            return false;
        }

        /// <summary>
        /// shared character-DAT 的最小 jump attack 输入桥。
        /// 参考正式 C++ release `state_jumping`，这里只补无持有态空中 `key_jump -> frame 80`。
        /// </summary>
        private bool TryRunSharedCharacterDatJumpAttackInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Jump || Runtime.YInt >= 0)
                return false;
            if (Runtime.KeyJump == 0)
                return false;

            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                SwitchDir("right");
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                SwitchDir("left");

            int linkState = Runtime.LinkState;
            if (linkState == 0)
            {
                AttackingCounter = 0;
                if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.JumpAttack, clampOnOverdraw: true))
                    return false;

                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.JumpAttack);
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if (linkState % 100 == 1)
            {
                AttackingCounter = 0;
                SetSharedCharacterDatInputFrameDirect(
                    hasDirection ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
                return true;
            }

            if (linkState == 4 || linkState == 6)
            {
                SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.SkyLgtWpThw);
                return true;
            }

            return false;
        }

        /// <summary>
        /// shared character-DAT 的最小 running 输入桥。
        /// 当前补 stop-running、run attack、running defend、running jump，
        /// 以及 release 风格的共享 held running 分支。
        /// </summary>
        private bool TryRunSharedCharacterDatRunningInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Running)
                return false;
            if (Runtime.LinkState == 2)
            {
                ApplySharedCharacterDatHeavyRunningMovement();

                if (IsSharedCharacterDatAttackInputReadyInternal())
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.HeavyWeaponThw);

                return true;
            }

            ApplySharedCharacterDatRunningMovement();

            if (IsSharedCharacterDatAttackInputReadyInternal())
            {
                int linkState = Runtime.LinkState;
                bool hasDirection = HasAnyDirectionInputForSharedCharacterDat();

                if (linkState % 100 == 1)
                {
                    SetSharedCharacterDatInputFrameDirect(
                        hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.RunWeaponAtck);
                }
                else if (linkState == 4)
                {
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    SetSharedCharacterDatInputFrameDirect(
                        hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.SkyLgtWpThw);
                }
                else if (TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.RunAttack))
                {
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.RunAttack);
                }
            }

            if (IsSharedCharacterDatDefendInputReadyInternal())
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.Rowing2);

            if (IsSharedCharacterDatJumpInputReadyInternal())
            {
                LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
                if (characterData == null)
                    return true;

                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashForward);
                Runtime.AnimSub = 0;
                Runtime.Vx = Runtime.Dir == "right"
                    ? characterData.dash_distance
                    : -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
            }

            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 normal running 基线移动。
        /// 这里只补跑动帧推进、速度写入、斜向 lane 速度和反向 stop-running 前置帧维护，
        /// 不覆盖后续的 stop-running / dash / run-attack 分支。
        /// </summary>
        private void ApplySharedCharacterDatRunningMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null)
                return;

            AttackingCounter = 0;

            int rate = characterData.running_frame_rate;
            if (rate < 1)
                rate = 1;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 4);
            Runtime.AnimCounter = animCounter;

            int frameId = LF2StandardFrames.RunningStart + (animCounter / rate);
            if ((animCounter / rate) >= 3)
                frameId = LF2StandardFrames.Running1;

            if (Runtime.Dir == "right")
            {
                Runtime.Vx = characterData.running_speed;
                if (Runtime.KeyLeft != 0)
                    frameId = LF2StandardFrames.StopRunning;
            }
            else
            {
                Runtime.Vx = -characterData.running_speed;
                if (Runtime.KeyRight != 0)
                    frameId = LF2StandardFrames.StopRunning;
            }

            ApplySharedCharacterDatRunLane(characterData.running_speedz);
            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void ApplySharedCharacterDatHeavyRunningMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null)
                return;

            AttackingCounter = 0;

            int rate = characterData.running_frame_rate;
            if (rate < 1)
                rate = 1;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 4);
            Runtime.AnimCounter = animCounter;

            int frameId = LF2StandardFrames.HeavyObjRun + (animCounter / rate);
            if ((animCounter / rate) >= 3)
                frameId = LF2StandardFrames.TreeJump0;

            if (Runtime.Dir == "right")
            {
                Runtime.Vx = characterData.heavy_running_speed;
                if (Runtime.KeyLeft != 0)
                    frameId = LF2StandardFrames.TreeJump2;
            }
            else
            {
                Runtime.Vx = -characterData.heavy_running_speed;
                if (Runtime.KeyRight != 0)
                    frameId = LF2StandardFrames.TreeJump2;
            }

            bool upPressed = Runtime.KeyUp != 0 && Runtime.KeyDown == 0;
            bool downPressed = Runtime.KeyDown != 0 && Runtime.KeyUp == 0;
            if (upPressed)
            {
                Runtime.Vz = -characterData.heavy_running_speedz;
                Runtime.Vx *= 5.0 / 6.0;
            }
            else if (downPressed)
            {
                Runtime.Vz = characterData.heavy_running_speedz;
                Runtime.Vx *= 5.0 / 6.0;
            }

            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void ApplySharedCharacterDatRunLane(float speedZ)
        {
            if (Runtime == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
            {
                Runtime.Vz = -speedZ;
                Runtime.Vx *= 5.0 / 6.0;
            }
            else if (downPressed && !upPressed)
            {
                Runtime.Vz = speedZ;
                Runtime.Vx *= 5.0 / 6.0;
            }
        }

        /// <summary>
        /// shared character-DAT 的最小 crouch 输入桥。
        /// 这里只补 `frame 215` 的 defend / crouch-dash 分支。
        /// release `ApplyFrame215Landing(...)` 的 dash branch 没有 `LinkState` gate，
        /// 所以 transformed character-DAT 的 non-LF2Character shell 在 held 路径下也必须能进 dash。
        /// </summary>
        private bool TryRunSharedCharacterDatCrouchInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.N ?? -1) != LF2StandardFrames.Crouch)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return false;

            bool handled = false;
            if (IsSharedCharacterDatDefendInputReadyInternal())
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.Rowing2);
                handled = true;
            }

            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();
            bool rightPressed = Runtime.KeyRight != 0;
            bool leftPressed = Runtime.KeyLeft != 0;

            if ((rightPressed || Runtime.Vx > 0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(
                    Runtime.Dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                Runtime.AnimSub = 0;
                Runtime.Vx = characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }
            else if ((leftPressed || Runtime.Vx < -0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(
                    Runtime.Dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                Runtime.AnimSub = 0;
                Runtime.Vx = -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }

            ApplySharedCharacterDatDashLane(characterData.dash_distancez);

            return handled;
        }

        /// <summary>
        /// shared character-DAT 的最小倒地 recovery 输入桥。
        /// 这里只补 `FallingFront2/FallingBack2 + KeyDefend + CdJump` 的 recovery 分支。
        /// </summary>
        private bool TryRunSharedCharacterDatDefensiveRecoveryInputPhase()
        {
            if (Runtime == null)
                return false;

            int frameId = Frame?.N ?? -1;
            if (frameId != LF2StandardFrames.FallingFront2 && frameId != LF2StandardFrames.FallingBack2)
                return false;
            if (WeaponCount < 0 || !IsSharedCharacterDatJumpInputReadyInternal() || Health?.HP <= 0)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            bool backward = Runtime.Dir == "right" ? Runtime.Vx <= 0f : Runtime.Vx >= 0f;
            SetSharedCharacterDatInputFrameDirect(
                backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
            AttackingCounter = 0;

            if (characterData == null)
                return true;

            if (Runtime.Vy > characterData.rowing_height)
                Runtime.Vy = characterData.rowing_height;

            float rowingDistance = characterData.rowing_distance;
            if (Runtime.Vx > -1f && Runtime.Vx < 1f)
                Runtime.Vx = Runtime.Dir == "left" ? rowingDistance : -rowingDistance;
            else
                Runtime.Vx = Runtime.Vx > 0f ? rowingDistance : -rowingDistance;

            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 dash attack 输入桥。
        /// 这里按正式 C++ release `state_dash` 只补已确认的最小 held 分支：
        /// 无持有态 `DashAttack`、`linkState % 100 == 1 -> DashWeaponAtck`、
        /// `linkState == 4/6 && hasDirection -> SkyLgtWpThw`。
        /// </summary>
        private bool TryRunSharedCharacterDatDashAttackInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Dash)
                return false;

            ApplySharedCharacterDatDashFrameMaintenance();

            if (Runtime.KeyJump == 0)
                return false;

            bool dashForward = (Runtime.Dir == "right" && Runtime.Vx > 0f) ||
                               (Runtime.Dir == "left" && Runtime.Vx < 0f);
            if (!dashForward)
                return false;

            int linkState = Runtime.LinkState;
            if (linkState == 0)
            {
                if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.DashAttack))
                    return false;

                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashAttack);
                return true;
            }

            if (linkState % 100 == 1)
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashWeaponAtck);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if ((linkState == 4 || linkState == 6) && hasDirection)
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.SkyLgtWpThw);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            return false;
        }

        private void ApplySharedCharacterDatDashFrameMaintenance()
        {
            if (Runtime == null)
                return;

            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                SwitchDir("right");
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                SwitchDir("left");

            bool facingRight = Runtime.Dir == "right";
            if (facingRight)
            {
                if (Frame.N != LF2StandardFrames.DashBack2 && Runtime.Vx < 0f)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (Runtime.Vx > 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward);
            }
            else
            {
                if (Runtime.Vx > 0f && Frame.N != LF2StandardFrames.DashBack2)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (Runtime.Vx < 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward);
            }
        }

        private bool HasAnyDirectionInputForSharedCharacterDat()
        {
            return Runtime != null &&
                   (Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0);
        }

        private int RandomSharedCharacterDatWeaponAttackFrame()
        {
            return BattleRandInt(0, 2) == 0
                ? LF2StandardFrames.NormalWeaponAtck
                : LF2StandardFrames.NormalWeaponAtck2;
        }

        private void StepSharedCharacterDatWalkAnimation(int rate, int frameBase)
        {
            if (Runtime == null)
                return;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 6);
            Runtime.AnimCounter = animCounter;

            int fi = animCounter / rate;
            int frameId = fi < 4 ? frameBase + fi : frameBase + (6 - fi);
            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void SetSharedCharacterDatMoveFrameDirect(int frameId)
        {
            if (Frame == null || FrameCache == null || Runtime == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return;

            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            Runtime.NextFrame = Frame.D.next;
        }

        private bool SetSharedCharacterDatInputFrameDirect(int frameId)
        {
            if (Frame == null || FrameCache == null || Runtime == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            Runtime.NextFrame = targetFrame.next;
            return true;
        }

        private void ApplySharedCharacterDatSpecialStateLaneControl()
        {
            if (Runtime == null || GetRuntimeYInt() != 0)
                return;

            int state = Frame?.D?.state ?? -1;
            if (state != LF2States.DeepSpecific && state != LF2States.FirenSpecific)
                return;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -characterData.running_speedz;
            else if (downPressed && !upPressed)
                Runtime.Vz = characterData.running_speedz;
        }

        private void ApplySharedCharacterDatDashLane(float dashDistanceZ)
        {
            if (Runtime == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -dashDistanceZ;
            else if (downPressed && !upPressed)
                Runtime.Vz = dashDistanceZ;
        }

        protected bool TrySpendSharedCharacterDatFramePpCost(int frameId, bool clampOnOverdraw = false)
        {
            if (!IsPpModeEnabled() || Health == null)
                return true;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            int ppCost = targetFrame.mp;
            if (!clampOnOverdraw && Health.PP < ppCost)
                return false;

            Health.PP -= ppCost;
            if (Health.PP >= 0)
            {
                SpendPpDisplay(ppCost);
            }
            else
            {
                Health.PP = 0;
            }

            return true;
        }

        private void ApplySharedRuntimeInputEvent(FuncKeyMask key, bool down, bool forceFreshEdge = false)
        {
            if (forceFreshEdge && down)
                ForceSharedRuntimePreviousState(key);

            switch (key)
            {
                case FuncKeyMask.right: Runtime.KeyRight = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.left: Runtime.KeyLeft = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.up: Runtime.KeyUp = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.down: Runtime.KeyDown = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.att: Runtime.KeyAttack = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.jump: Runtime.KeyJump = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.def: Runtime.KeyDefend = down ? (byte)1 : (byte)0; break;
            }

            if (!down)
                return;

            // shared character-DAT 输入镜像也要保持 reference 的交叉 cooldown 语义：
            // attack -> CdDefend, defend -> CdJump, jump -> CdAttack。
            switch (key)
            {
                case FuncKeyMask.right:
                    if (Runtime.PrevRight == 0)
                    {
                        Runtime.CdRight = 5;
                        Runtime.PushInputHistory(6);
                    }
                    break;
                case FuncKeyMask.left:
                    if (Runtime.PrevLeft == 0)
                    {
                        Runtime.CdLeft = 5;
                        Runtime.PushInputHistory(4);
                    }
                    break;
                case FuncKeyMask.up:
                    if (Runtime.PrevUp == 0)
                    {
                        Runtime.CdUp = 5;
                        Runtime.PushInputHistory(8);
                    }
                    break;
                case FuncKeyMask.down:
                    if (Runtime.PrevDown == 0)
                    {
                        Runtime.CdDown = 5;
                        Runtime.PushInputHistory(2);
                    }
                    break;
                case FuncKeyMask.att:
                    if (Runtime.PrevAttack == 0)
                    {
                        Runtime.CdDefend = 5;
                        Runtime.PushInputHistory(9);
                    }
                    break;
                case FuncKeyMask.jump:
                    if (Runtime.PrevJump == 0)
                    {
                        Runtime.CdAttack = 5;
                        Runtime.PushInputHistory(5);
                    }
                    break;
                case FuncKeyMask.def:
                    if (Runtime.PrevDefend == 0)
                    {
                        Runtime.CdJump = 5;
                        Runtime.PushInputHistory(0);
                    }
                    break;
            }
        }

        private void ForceSharedRuntimePreviousState(FuncKeyMask key)
        {
            switch (key)
            {
                case FuncKeyMask.right: Runtime.PrevRight = 0; break;
                case FuncKeyMask.left: Runtime.PrevLeft = 0; break;
                case FuncKeyMask.up: Runtime.PrevUp = 0; break;
                case FuncKeyMask.down: Runtime.PrevDown = 0; break;
                case FuncKeyMask.att: Runtime.PrevAttack = 0; break;
                case FuncKeyMask.jump: Runtime.PrevJump = 0; break;
                case FuncKeyMask.def: Runtime.PrevDefend = 0; break;
            }
        }

        /// <summary>
        /// 供“当前 DAT 是 Character”的通用输入消费链使用的 DJA guard。
        /// 这层判断只依赖共享 runtime / frame 数据，不要求 CLR 类型真的是 LF2Character。
        /// </summary>
        internal bool ShouldHoldCharacterDatDjaInputGuard(int targetFrame)
        {
            if (ObjectId != 6 || targetFrame != 300 || Health == null || Health.HP <= 177)
                return false;

            return Match?.Runtime?.Flow?.DjaGuardGlobal44F224 == 0;
        }

        internal bool CanEnterCharacterDatInputFrameJump()
        {
            return TransformOriginalObjectId == -1 && Runtime.LinkState != 2;
        }

        /// <summary>
        /// 通用输入跳帧入口。
        /// 参考 C# `DoFrameJump(...)`，用于当前 DAT 已经是 Character 的任意实体。
        /// </summary>
        internal bool TryCharacterDatInputFrameJump(int frameId)
        {
            bool flipFacing = false;
            if (frameId < 0)
            {
                frameId = -frameId;
                flipFacing = true;
            }

            if (frameId == 999)
                frameId = 0;

            if (FrameCache?.HasFrame(frameId) != true || Health == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            bool ppMode = IsPpModeEnabled();
            if (ppMode)
            {
                int ppCost = targetFrame.mp % 1000;
                int hpCost = (targetFrame.mp / 1000) * 10;
                if (Health.PP < ppCost || Health.HP <= hpCost)
                    return false;

                Health.HP -= hpCost;
                Health.PP -= ppCost;
                ComboCountVic += hpCost;
                SpendPpDisplay(ppCost);
            }

            if (flipFacing && ppMode)
                SwitchDir(Runtime.Dir == "right" ? "left" : "right");

            return SetSharedCharacterDatInputFrameDirect(frameId);
        }

        /// <summary>
        /// 判断当前实体是否满足 N30 晚阶段输入触发条件。
        /// 这里按“当前 DAT 是否还是角色”判断，而不是按 CLR 子类判断。
        /// </summary>
        internal bool TryResolveLateN30InputTriggerCode(out int frameVal)
        {
            frameVal = 0;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            if (slotIndex < 0 || slotIndex >= 10)
                return false;
            if (Health == null || Health.HP <= 0)
                return false;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;

            int[] history = Runtime?.InputHistory;
            if (history == null || history.Length < 6)
                return false;

            int a = history[2];
            int b = history[3];
            int c = history[4];
            int d = history[5];
            if (a == 9 && b == 0 && c == 9 && d == 0) frameVal = 100;
            else if (a == 9 && b == 9 && c == 9 && d == 9) frameVal = 102;
            else if (a == 9 && b == 5 && c == 9 && d == 5) frameVal = 104;

            return frameVal != 0;
        }

        /// <summary>
        /// 处理当前 DAT 仍是角色对象时的晚阶段 N30 输入触发。
        /// 参考实现按 slot + 当前 DAT 类型参与，所以不能只挂在 LF2Character 上。
        /// </summary>
        private void RunLateCharacterDatInputTrigger()
        {
            if (!TryResolveLateN30InputTriggerCode(out int frameVal))
                return;

            Runtime?.ClearInputHistoryTail();

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            ConfigureLateN30SpawnTask(task, slotIndex, frameVal);

            LF2Entity spawned = factory.CreateObjectImmediate(task);
            if (spawned == null)
                return;

            ApplyLateN30HistoryGateBroadcast(frameVal);
        }

        /// <summary>
        /// 统一写入晚阶段 N30 触发生成 998 效果时的运行时身份。
        /// Unity 侧同阵营筛选已经以 `RelationTeam -> Team` 作为当前真值，
        /// 所以这里的 effect 任务也必须沿用同一套来源，不能继续把 `team` 留成 0。
        /// </summary>
        private void ApplyLateN30SpawnIdentity(OPointCreateTask task, int slotIndex)
        {
            if (task == null)
                return;

            int sourceTeam = ResolveN30HistoryGateTeam(this);
            task.team = sourceTeam;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = sourceTeam;
            task.holderCopySlot = -1;
            task.spawnerEntityIndex = slotIndex;
        }

        /// <summary>
        /// 晚阶段 N30 生成的 `oid=998` 属于立即特效路径。
        /// 这类 task 的 `z` 已经直接编码了参考实现最终可见 Z，
        /// 不能再吃工厂通用的 post-init `Z+1` 抬高。
        /// </summary>
        private void ConfigureLateN30SpawnTask(OPointCreateTask task, int slotIndex, int frameVal)
        {
            if (task == null)
                return;

            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = frameVal, facing = 0 };
            task.parent = null;
            ApplyLateN30SpawnIdentity(task, slotIndex);
            task.pos = new Vector3(GetRuntimeXInt(), 0f, GetRenderZInt());
            task.z = GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = GetRuntimeXInt();
            task.initialRuntimeY = 0;
            task.initialRuntimeZ = GetRenderZInt();
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
        }

        /// <summary>
        /// N30 晚阶段除了生成 998 效果外，
        /// 102 还要给同阵营角色打开 history gate，104 要关闭 gate。
        /// </summary>
        private void ApplyLateN30HistoryGateBroadcast(int frameVal)
        {
            if (frameVal != 102 && frameVal != 104)
                return;

            SimulationWorld world = Match;
            if (world == null)
                return;

            int sourceTeam = ResolveN30HistoryGateTeam(this);
            if (sourceTeam == 0)
                return;

            bool enabled = frameVal == 102;
            N30HistoryGateScratch.Clear();
            world.GetAllEntities(N30HistoryGateScratch);

            try
            {
                for (int i = 0; i < N30HistoryGateScratch.Count; i++)
                {
                    LF2Entity teammate = N30HistoryGateScratch[i];
                    if (teammate == null || teammate.Runtime == null || teammate.Health == null)
                        continue;
                    if (teammate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                        continue;
                    if (teammate.Health.HP <= 0)
                        continue;
                    if (ResolveN30HistoryGateTeam(teammate) != sourceTeam)
                        continue;

                    teammate.Runtime.SetInputHistoryGate(enabled);
                }
            }
            finally
            {
                N30HistoryGateScratch.Clear();
            }
        }

        private static int ResolveN30HistoryGateTeam(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        /// <summary>
        /// 早期 state 400/401 传送特判入口。
        /// C++ release 只要求 source active 且有当前 frame；候选 target 才要求 Character DAT。
        /// source 不能按 CLR 类型或当前 DAT 类型提前排除。
        /// </summary>
        internal virtual void RunEarlyTeleportSpecialsPhase(System.Collections.Generic.List<LF2Entity> entities, bool frameToggleGate)
        {
            if (frameToggleGate || entities == null || Health == null)
                return;

            int state = Frame?.D?.state ?? -1;
            bool toEnemy = state == LF2States.TeleportToEnemy;
            bool toTeammate = state == LF2States.TeleportToTeammate;
            if (!toEnemy && !toTeammate)
                return;

            LF2Entity best = null;
            int bestDistance = toEnemy ? 10000 : -1;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity target = entities[i];
                if (target == null || target.Health == null)
                    continue;
                if (target.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;
                if (target.Health.HP <= 0)
                    continue;
                if (toEnemy && target.RelationTeam == RelationTeam)
                    continue;
                if (toTeammate && target.RelationTeam != RelationTeam)
                    continue;

                int distance = Mathf.Abs(target.GetRenderZInt() - GetRenderZInt()) +
                               Mathf.Abs(target.GetRuntimeXInt() - GetRuntimeXInt());
                if (toEnemy && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
                else if (toTeammate && distance > bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            if (best == null)
            {
                Runtime.Y = 0f;
                Runtime.YInt = 0;
                Runtime.Vx = 0f;
                Runtime.Vy = 0f;
                Runtime.Vz = 0f;
                return;
            }

            int offset = toEnemy ? 120 : 60;
            int nextZ = best.GetRenderZInt() + 1;
            int nextX = Runtime.Dir == "right"
                ? best.GetRuntimeXInt() - offset
                : best.GetRuntimeXInt() + offset;

            Runtime.Z = nextZ;
            Runtime.ZInt = nextZ;
            Runtime.X = nextX;
            Runtime.XInt = nextX;
            Runtime.Y = 0f;
            Runtime.YInt = 0;
            Runtime.Vx = 0f;
            Runtime.Vy = 0f;
            Runtime.Vz = 0f;
        }

        internal virtual void RunLateDeathOpointPreCleanupPhase() { }

        internal virtual bool TryRunLatePostOpointCleanupPhase() => false;

        internal virtual void RunLateTailBeforePrevFrame()
        {
            RunLateCharacterDatInputTrigger();
            SpawnLateTransitionEffects();
        }

        public virtual void MirrorLatePrevFrame()
        {
            if (Frame != null)
                Frame.Prev = Frame.N;
        }

        private void SpawnLateTransitionEffects()
        {
            LF2FrameData prevFrame = GetFrameDataById(Frame?.Prev ?? 0);
            LF2FrameData currentFrame = Frame?.D;
            if (prevFrame == null || currentFrame == null)
                return;

            int prevState = prevFrame.state;
            int currentState = currentFrame.state;
            bool spawned = false;
            bool hasEffectResources = LF2ObjectPointFactory.Instance != null &&
                                      ResolveRuntimeCharacterConfig(999) != null;
            int availableSlots = hasEffectResources ? CountAvailableTransitionEffectSlots() : 0;

            if (hasEffectResources &&
                (prevState == 13 || (Frame?.Prev ?? 0) == 200) &&
                currentState != 13 && (Frame?.N ?? 0) != 200)
            {
                Match?.QueueSound("SFX_066", Runtime.XInt);
                spawned |= SpawnTransitionEffectBranch1(ref availableSlots);
            }

            if (prevState != 18 && prevState != 19)
                return;

            int count = 0;
            if (currentState != 18 && currentState != 19)
                count = 7;
            else if (BattleRandInt(0, 4) == 0)
                count = 1;

            if (count > 0)
                spawned |= SpawnTransitionEffectBranch2(count, ref availableSlots);

            if (spawned)
                RefreshRuntimeSnapshot();
        }

        private bool SpawnTransitionEffectBranch1(ref int availableSlots)
        {
            int initialSlots = availableSlots;
            for (int n = 0; n < 15; n++)
            {
                if (availableSlots <= 0)
                    break;

                float y = (float)(Runtime.Y - BattleRandInt(0, 29));
                float x = (float)(Runtime.X + BattleRandInt(0, 39) - 19.0);
                float vy = -((float)BattleRandInt(0, 20) / 2f) - 8f;
                float vx = (float)(Runtime.Vx * 0.5f + BattleRandInt(0, 11) - 5f);
                int frameId = n < 2 ? 120 : n < 5 ? 130 : n < 9 ? 125 : 135;
                SpawnTransitionEffect(
                    frameId,
                    x,
                    y,
                    vx,
                    vy);
                availableSlots--;
            }

            return availableSlots < initialSlots;
        }

        private bool SpawnTransitionEffectBranch2(int count, ref int availableSlots)
        {
            int initialSlots = availableSlots;
            for (int n = 0; n < count; n++)
            {
                if (availableSlots <= 0)
                    break;

                float y = (float)(Runtime.Y - BattleRandInt(0, 29));
                float x = (float)(Runtime.X + BattleRandInt(0, 59) - 29.0);
                float vx = (float)(Runtime.Vx + BattleRandInt(0, 11) - 5f);
                int frameId = 140 + BattleRandInt(0, 1);
                SpawnTransitionEffect(
                    frameId,
                    x,
                    y,
                    vx,
                    -1f);
                availableSlots--;
            }

            return availableSlots < initialSlots;
        }

        private int CountAvailableTransitionEffectSlots()
        {
            if (Match == null)
                return 350;

            int available = 0;
            for (int slot = 50; slot < 400; slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null)
                    available++;
            }

            return available;
        }

        private void SpawnTransitionEffect(int frameId, float x, float y, float vx, float vy)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = 999,
                kind = 0,
                action = frameId,
                facing = Runtime.Dir == "right" ? 0 : 1,
                x = 0,
                y = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
            };
            task.parent = null;
            task.team = Team;
            task.relationTeam = RelationTeam != 0 ? RelationTeam : Team;
            task.useExplicitRelationIdentity = true;
            task.holderCopySlot = -1;
            task.pos = new Vector3(x, y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.useDirectVelocity = true;
            task.directVx = vx;
            task.directVy = vy;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.TransitionEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            factory.EnqueueCreateObject(task);
        }

        public virtual void FreeEntityLikeExe()
        {
            OnTransitDestroy();
        }

        public virtual void DirectWriteFramePreserveWaitCounter(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        internal void DirectWriteHeldFramePreserveWaitCounter(int frameId)
        {
            if (Frame == null)
                return;

            Frame.N = frameId;
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        public virtual void DirectWriteFrameImmediateWaitReset(int frameId)
        {
            SetFrameTickDirect(frameId, 0);
        }

        private void ApplyStateDataTransform(int targetObjectId, bool applyHitStop140)
        {
            if (targetObjectId < 0)
                return;

            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            Runtime.WeaponFlightCounter = wrapper.characterData?.weapon_hp ?? 0;
            ImmediateFrame(0);

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            if (applyHitStop140)
                HitStun = 140;

            RefreshRuntimeSnapshot();
        }

        internal static LF2CharacterDataWrapper ResolveRuntimeCharacterConfig(int targetObjectId)
        {
            LF2CharacterDataWrapper overrideWrapper = RuntimeCharacterConfigResolverOverride?.Invoke(targetObjectId);
            if (overrideWrapper != null)
                return overrideWrapper;

            return CharacterAnimtorManager.Instance?.GetCharacterConfig(targetObjectId);
        }

        internal bool TryApplyRuntimeIdentity(
            int targetObjectId,
            int targetFrameId,
            bool resetWaitCounter,
            out LF2CharacterDataWrapper wrapper)
        {
            wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return false;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            WeaponCount = wrapper.characterData?.weapon_hp ?? 0;

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            Frame.N = targetFrameId;
            Frame.D = FrameCache.GetFrameDataById(targetFrameId);
            if (Frame.D != null)
            {
                int waitCounter = resetWaitCounter ? 0 : (Trans?.WaitCounter ?? 0);
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
            }

            RefreshRuntimeSnapshot();
            return true;
        }

        internal bool TryReloadCurrentFrameDataForRuntimeIdentity(int targetObjectId)
        {
            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return false;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            WeaponCount = wrapper.characterData?.weapon_hp ?? 0;

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            Frame.D = FrameCache.GetFrameDataById(Frame.N);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);

            RefreshRuntimeSnapshot();
            return true;
        }

        public virtual int GetCurrentDataObjectTypeForSimulation() => ResolveCurrentDataObjectType(this);

        public virtual int GetCurrentDataObjectType() => GetCurrentDataObjectTypeForSimulation();

        /// <summary>
        /// 参考 C# release 的 `ObjTypeRules.ToRuntimeObjType(...)`：
        /// 运行时粗分类只区分“角色”与“非角色”。
        /// Unity 内部仍然保留完整 DAT type 供大多数逻辑使用，
        /// 这里只在 runtime 身份快照/校验层复用 release 语义。
        /// </summary>
        public static int ResolveReferenceRuntimeObjTypeFromDataType(int currentDataType)
        {
            return currentDataType == (int)LF2ObjectType.Character ? 0 : 1;
        }

        /// <summary>
        /// 按当前 DAT 包装器解析对象 type。
        /// C# 基准工程 EntityCategoryResolver 使用 CharData.ObjType，而不是实体子类类型；
        /// Unity 的对象池类型只决定实例来自哪个池，战斗判定必须读取当前 DAT type。
        /// </summary>
        public static int ResolveCurrentDataObjectType(LF2Entity entity)
        {
            if (entity == null)
                return -1;

            int wrapperOid = entity.FrameCache?.Wrapper?.characterId ?? entity.ObjectId;
            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            return definition?.type ?? entity.ReleaseEntityType;
        }

        public virtual bool ShouldDeferInitialRuntimeSnapshot() => false;

        public virtual LF2FrameData GetCollisionFrameData()
        {
            return Frame?.Prev2D ?? Frame?.D;
        }

        public virtual void CaptureCollisionFrameSnapshot()
        {
            SyncCollisionSnapshotToCurrentFrame();
        }

        internal void SyncCollisionSnapshotToCurrentFrame()
        {
            if (Frame == null)
                return;

            Frame.Prev2 = Frame.N;
            Frame.Prev2D = Frame.D;
            Runtime.PrevFrame2 = Frame.Prev2;
        }

        internal bool ReloadCurrentFrameDataFromWrapper()
        {
            if (Frame == null || FrameCache == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(Frame.N);
            if (targetFrame == null)
                return false;

            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
            return true;
        }

        public virtual int GetRenderPicIndex()
        {
            int pic = Frame?.D?.pic ?? -1;
            return pic >= 0 ? pic + Runtime.RenderPicOffset : pic;
        }

        public virtual float GetDisplayZ()
        {
            if (GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                Runtime != null &&
                System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (float)(Runtime.Z - Runtime.Type3VisualZOffset);
            }

            return GetRenderZInt();
        }

        public virtual int GetRenderSortingOrder()
        {
            int order = GetRenderZInt() + Mathf.RoundToInt(Runtime?.Zz ?? 0f);
            if (ShouldRenderAboveCharacters())
                order += OverlaySortingOrderOffset;
            return order;
        }

        public virtual float GetSpriteWidthPxForRender()
        {
            float width = Sprite?.GetWidthPx() ?? 0f;
            if (width <= 0f)
                width = GetSpriteWidthPxForCollision();
            return width;
        }

        public virtual float GetSpriteHeightPxForRender()
        {
            return Sprite?.GetHeightPx() ?? 0f;
        }

        public virtual int GetRuntimeXInt()
        {
            return Runtime.XInt != 0 ? Runtime.XInt : ReleaseInt(Runtime.X);
        }

        public virtual int GetRuntimeYInt()
        {
            return Runtime.YInt != 0 ? Runtime.YInt : ReleaseInt(Runtime.Y);
        }

        public virtual int GetRenderZInt()
        {
            return Runtime.ZInt != 0 ? Runtime.ZInt : ReleaseInt(Runtime.Z);
        }

        public virtual int GetCollisionZInt() => GetCollisionZInt(GetCollisionFrameData());

        public virtual int GetCollisionZInt(LF2FrameData frame)
        {
            if (GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack && Runtime != null)
            {
                if (System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
                    return ReleaseInt(Runtime.Z - Runtime.Type3VisualZOffset);

                if (frame != null && frame.hit_j > 0)
                    return ReleaseInt(Runtime.Z - (frame.hit_j - 50));
            }

            return GetRenderZInt();
        }

        public virtual float GetRenderOffsetX() => Runtime.RenderOffsetX;

        public void QueueBattleSound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            Match?.QueueSound(soundId, GetRuntimeXInt());
        }

        public virtual int ResolveReleaseNeutralHolderSlotOrImplicitZero()
        {
            int slot = HolderCopySlot;
            return slot >= 0 ? slot : 0;
        }

        public virtual int ResolveReleaseNegativeLinkHolderSlotOrImplicitZero()
        {
            int slot = Runtime.HolderStableId;
            if (slot < 0)
                slot = HolderCopySlot;
            return slot >= 0 ? slot : 0;
        }

        protected virtual float ResolveCurrentSpriteFileWidthPx()
        {
            float width = Sprite?.GetCurrentSpriteWidthPx() ?? 0f;
            return width > 0f ? width : Sprite?.GetWidthPx() ?? 0f;
        }

        protected virtual bool ShouldRenderAboveCharacters()
        {
            int semantic = Runtime?.SpawnSemantic ?? 0;
            return semantic == (int)ReleaseSpawnSemantic.ImmediateEffect ||
                   semantic == (int)ReleaseSpawnSemantic.TransitionEffect;
        }

        protected virtual bool IsBlockedByReleaseLinkOrCaughtCpoint()
        {
            return Runtime.LinkState < 0;
        }

        protected virtual void ApplyReleaseSceneQueryConsumeEffects(SceneQueryHit hitInfo)
        {
            if (hitInfo.ZeroAttackerHpOnConsume && Health != null)
                Health.HP = 0;

            if (hitInfo.ReleaseHeavyHeldTargetOnConsume && hitInfo.Target != null)
                ApplyHeavyHeldTargetReleaseConsumeEffect(hitInfo.Target);
        }

        internal void ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(SceneQueryHit hitInfo)
            => ApplyReleaseSceneQueryConsumeEffects(hitInfo);

        /// <summary>
        /// C++ release `HitResolve.PreprocessCandidate` 中，重武器附着目标在特定 kind=0 命中前会先断开 2/-2 双向附着，
        /// 并把附着子物体切到随机落地帧、写入一个轻微下落速度。
        /// 这里补的是那条“命中前消费语义”，不是普通 held release。
        /// </summary>
        private void ApplyHeavyHeldTargetReleaseConsumeEffect(LF2Entity holderTarget)
        {
            if (holderTarget?.Runtime == null)
                return;

            int holderSlot = holderTarget.Runtime.SlotIndex;
            int heldTargetSlot = holderTarget.Runtime.ResolveActiveHeldSlotIndex();
            if (heldTargetSlot < 0)
            {
                holderTarget.Runtime.LinkState = 0;
                return;
            }

            LF2Entity heldTarget = holderTarget.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot);
            if (heldTarget?.Runtime == null ||
                !heldTarget.Runtime.IsActivelyHeldBySlot(holderSlot) ||
                heldTarget.Runtime.LinkState != -2)
            {
                holderTarget.Runtime.LinkState = 0;
                return;
            }

            int attackerSlot = Runtime?.SlotIndex ?? -1;
            if (attackerSlot >= 0)
                holderTarget.ItrRest?.SetVrest(attackerSlot, 45);

            holderTarget.ItrRest?.SetVrest(heldTargetSlot, 30);
            holderTarget.Runtime.LinkState = 0;
            heldTarget.Runtime.LinkState = 0;
            heldTarget.ImmediateFrame(heldTarget.BattleRandInt(0, 6));
            heldTarget.Runtime.Vy = -1f;
            heldTarget.RefreshRuntimeSnapshot();
            holderTarget.RefreshRuntimeSnapshot();
        }

        public virtual void ApplySignedCpointFrame(int frameId)
        {
            if (frameId == 0)
                return;

            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            SetFrameTickDirect(frameId);
        }

        public virtual void ApplySignedImmediateFrameWaitReset(int frameId)
        {
            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            DirectWriteFrameImmediateWaitReset(frameId);
        }

        public virtual void ResetPooledEntityState()
        {
            _hasForcedRuntimeIntPosition = false;
            Runtime.PendingFlushDestroy = false;
            Runtime.TransformOriginalObjectId = -1;
            Runtime.TransformTargetObjectId = -1;
            Runtime.RenderOffsetX = 0f;
        }

        public virtual void ApplyForcedRuntimeIntPosition(int x, int y, int z)
        {
            Runtime.XInt = x;
            Runtime.YInt = y;
            Runtime.ZInt = z;
            _hasForcedRuntimeIntPosition = true;
        }

        public virtual void ClearForcedRuntimeIntPosition()
        {
            _hasForcedRuntimeIntPosition = false;
        }

        public virtual void ConsumeForcedRuntimeIntPosition()
        {
            _hasForcedRuntimeIntPosition = false;
            RefreshRuntimeIntPosition();
        }

        public virtual void ReleaseForcedRuntimeIntPositionAfterFirstPresentation(int tickIndex)
        {
            if (tickIndex >= Runtime.FirstPresentationTick)
                ConsumeForcedRuntimeIntPosition();
        }

        public virtual void RunCpointCheckStep10()
        {
            // step10 cpoint 维护是 battle loop 的交互阶段逻辑。
            // 它读取的是 collision snapshot / runtime link / cpoint 数据，
            // 不属于角色本地 `DispatchCurrentStateEvent(...)` 的 state 事件。
            LF2FrameData catcherFrame = GetCollisionFrameData();
            CatchPoint cpoint = catcherFrame?.cpoint;
            if (cpoint == null || cpoint.kind != 1 || FrameDelay < 0)
                return;

            LF2Entity victim = Match?.FindEntityByRuntimeSlotForQuery(CaughtSlotIndex);
            if (victim == null || victim.Frame == null)
            {
                SetCpointRawFramePreserveWait(0);
                return;
            }

            bool skipActions = false;
            bool skipDecrease = false;
            LF2FrameData victimFrame = victim.GetCollisionFrameData();
            if (victim.CatcherSlotIndex != (Runtime?.SlotIndex ?? -1) ||
                victimFrame?.cpoint == null ||
                victimFrame.cpoint.kind != 2)
            {
                SetCpointRawFramePreserveWait(0);
                skipActions = true;
                skipDecrease = true;
            }

            if (!skipDecrease && cpoint.decrease > 0)
            {
                Runtime.CaughtDuration -= cpoint.decrease;
            }
            else if (!skipDecrease && cpoint.decrease < 0)
            {
                Runtime.CaughtDuration += cpoint.decrease;
                if (Runtime.CaughtDuration < 0)
                {
                    SetCpointRawFramePreserveWait(0);
                    victim.SetCpointRawFramePreserveWait(181);
                    HitCount = 1;
                    victim.HitCount = 1;
                    victim.KnockbackVx = GetReleaseXInt() > victim.GetReleaseXInt() ? -4f : 4f;
                    victim.KnockbackVy = -3f;
                    skipActions = true;
                }
            }

            if (!skipActions)
                RunCpointActionSelectionStep10(cpoint, victim);

            if (cpoint.throwvx != 0)
                ApplyCpointThrowStep10(cpoint, victim, catcherFrame);

            ApplyCpointDirControlStep10(cpoint);
        }

        public virtual void RunCpointMismatchTailStep10()
        {
            // 这里是 step10 的 mismatch 收尾，
            // 仍然属于 pass 级交互维护，不是 frame/TU/state_entry 一类本地事件。
            CatchPoint cpoint = Frame?.D?.cpoint;
            if (cpoint == null || cpoint.kind != 2)
                return;

            bool valid = false;
            LF2Entity catcher = Match?.FindEntityByRuntimeSlotForQuery(CatcherSlotIndex);
            if (catcher != null && catcher.CaughtSlotIndex == (Runtime?.SlotIndex ?? -1))
            {
                CatchPoint catcherCpoint = catcher.Frame?.D?.cpoint;
                valid = catcherCpoint != null && catcherCpoint.kind == 1;
            }

            if (valid)
                return;

            SetCpointRawFramePreserveWait(212);
            Runtime.Vy = -3f;
            if (Runtime.Y > -2f)
                Runtime.Y = -2f;
            RefreshRuntimeSnapshot();
        }

        public virtual void RunWeaponSyncHeldStep10()
        {
            LF2FrameData currentFrame = Frame?.D;
            CatchPoint cpoint = currentFrame?.cpoint;
            if (currentFrame == null || cpoint == null || cpoint.kind != 1 || currentFrame.state != LF2States.Catching)
                return;

            LF2Entity victim = Match?.FindEntityByRuntimeSlotForQuery(CaughtSlotIndex);
            if (victim == null || victim.CatcherSlotIndex != (Runtime?.SlotIndex ?? -1))
                return;

            LF2FrameData victimFrame = victim.Frame?.D;
            if (victimFrame?.cpoint == null || victimFrame.cpoint.kind != 2)
                return;

            SyncCaughtByCpointStep10(victim, currentFrame, cpoint);
        }

        public virtual void ClearHitCandidateCarriers()
        {
            HitConfirm2 = 0;
        }

        protected virtual void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            if (Runtime == null || cpoint == null || victimEntity == null)
                return;

            bool attackReady = IsSharedCharacterDatAttackInputReadyInternal();
            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();

            if (attackReady && cpoint.aaction != 0)
            {
                bool dirOk = (Runtime.KeyLeft == 0 && Runtime.KeyRight == 0) || cpoint.taction == 0;
                if (dirOk)
                    ApplySharedCpointActionStep10(cpoint.aaction, victimEntity);
            }

            if (attackReady && cpoint.taction != 0)
            {
                bool anyDir = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
                if (anyDir)
                    ApplySharedCpointActionStep10(cpoint.taction, victimEntity);
            }

            if (jumpReady && cpoint.jaction != 0)
                ApplySharedCpointActionStep10(cpoint.jaction, victimEntity);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            ApplyCpointThrowStep10(cpoint, victimEntity, null);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity, LF2FrameData throwFrameSnapshot)
        {
            if (cpoint == null || victimEntity == null)
                return;

            if (cpoint.throwinjury == -1 && HasStep10ThrowTransformVictimData(victimEntity))
            {
                ApplyCpointThrowTransformToSelfAndOwnedObjects(victimEntity);
            }

            if (cpoint.throwinjury > 0)
                victimEntity.WeaponCount = cpoint.throwinjury;

            // cpoint_check keeps using the source cpoint, but geometry and next are read
            // from the attacker's current DAT/current frame after action/transform.
            LF2FrameData throwFrame = FrameCache?.GetFrameDataById(Frame?.N ?? 0) ?? Frame?.D;

            int centerX = throwFrame?.centerx ?? 0;
            int centerY = throwFrame?.centery ?? 0;
            int y = GetReleaseYInt() - centerY + cpoint.y;
            int x = Runtime.Dir == "right"
                ? GetReleaseXInt() - centerX + cpoint.x
                : centerX - cpoint.x + GetReleaseXInt();

            victimEntity.Runtime.X = x;
            victimEntity.Runtime.Y = y;
            victimEntity.Runtime.Vx = Runtime.Dir == "right" ? cpoint.throwvx : -cpoint.throwvx;
            victimEntity.Runtime.Vy = cpoint.throwvy;
            SetVictimThrowVzStep10(cpoint, victimEntity);

            int nextFrame = throwFrame?.next ?? 0;
            SetCpointRawFramePreserveWait(nextFrame);
            SetCpointRawPrevFrame2(nextFrame);
            AttackingCounter = 0;
            victimEntity.SetCpointRawFramePreserveWait(cpoint.vaction);
            victimEntity.SetCpointRawPrevFrame2(cpoint.vaction);
        }

        protected void ApplyCpointThrowTransformToSelfAndOwnedObjects(LF2Entity victimEntity)
        {
            if (victimEntity == null)
                return;

            LF2CharacterDataWrapper victimConfig = ResolveRuntimeCharacterConfig(victimEntity.ObjectId);
            if (victimConfig == null)
                return;

            TransformOriginalObjectId = ObjectId;
            TransformTargetObjectId = victimEntity.ObjectId;
            FrameCache.Load(victimConfig);
            ObjectId = victimEntity.ObjectId;
            WeaponCount = victimConfig.characterData?.weapon_hp ?? 0;
            SetCpointRawFramePreserveWait(0);
            Frame.PN = Frame.N;
            EnsureSharedCharacterDatControllerForSimulation();
            PropagateCpointThrowTransformToOwnedObjects(victimConfig, victimEntity.ObjectId);
        }

        protected virtual void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim)
        {
            if (cpoint == null || victim == null)
                return;

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
                victim.Runtime.Vz = -cpoint.throwvz;
            else if (Runtime.KeyUp == 0 && Runtime.KeyDown != 0)
                victim.Runtime.Vz = cpoint.throwvz;
        }

        protected virtual void ApplyCpointDirControlStep10(CatchPoint cpoint)
        {
            if (Runtime == null || cpoint == null || AttackingCounter != 2)
                return;

            if (cpoint.dircontrol == 1)
            {
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("right");
                else if (Runtime.KeyRight == 0 && Runtime.KeyLeft != 0)
                    SwitchDir("left");
            }
            else if (cpoint.dircontrol == -1)
            {
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("left");
                else if (Runtime.KeyRight == 0 && Runtime.KeyLeft != 0)
                    SwitchDir("right");
            }
        }

        protected virtual void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury)
        {
            if (victimEntity == null || victimEntity.Health == null)
                return;

            if (injury > 0)
            {
                int actualInjury = injury;
                if (victimEntity.FallDamageDiv > 0)
                    actualInjury = injury * 100 / victimEntity.FallDamageDiv;

                if (victimEntity.Health.HP > 0 &&
                    actualInjury >= victimEntity.Health.HP &&
                    victimEntity.KillCount == -1)
                {
                    LF2Entity holder = Match?.FindEntityByRuntimeSlotForQuery(HolderCopySlot);
                    if (holder != null)
                        holder.KillStat++;

                    int killStatIndex = victimEntity.Unk344;
                    if (Match?.KillStats != null && killStatIndex > 0 && killStatIndex < 3 && killStatIndex < Match.KillStats.Length)
                        Match.KillStats[killStatIndex]++;
                }

                victimEntity.Health.HP -= actualInjury;
                victimEntity.Health.HPBound -= actualInjury / 3;
                victimEntity.ComboCountVic += actualInjury;
                AttackingCounter = 1;
                FrameDelay = 2;
                victimEntity.FrameDelay = -3;
                LF2Entity comboHolder = Match?.FindEntityByRuntimeSlotForQuery(HolderCopySlot);
                if (comboHolder != null)
                    comboHolder.ComboCountAtk += actualInjury;

                int damageStatIndex = victimEntity.Unk344;
                if (Match?.DamageStats != null && damageStatIndex > 0 && damageStatIndex < 3 && damageStatIndex < Match.DamageStats.Length)
                    Match.DamageStats[damageStatIndex] += actualInjury;
                return;
            }

            victimEntity.Health.HP += injury;
            victimEntity.Health.HPBound += injury / 3;
            AttackingCounter = 1;
        }

        internal bool HasStep10ThrowTransformVictimData(LF2Entity victimEntity)
        {
            return victimEntity?.FrameCache?.Wrapper?.characterData != null;
        }

        /// <summary>
        /// shared character-DAT 的攻击输入入口。
        /// 这里使用的是参考 C# 当前已落地的交叉 cooldown 语义：
        /// `KeyJump + CdAttack` 才表示这一拍要走 attack 输入分支。
        /// 把读取位置收束到单点，是为了后续如果还要细调输入链，
        /// 只需要改这一层，不必回头散改 step10 / shared character-DAT 调用点。
        /// </summary>
        protected virtual bool IsSharedCharacterDatAttackInputReadyInternal()
        {
            return Runtime.KeyJump != 0 && Runtime.CdAttack > 0;
        }

        /// <summary>
        /// shared character-DAT 的跳跃输入入口。
        /// 对齐参考 C# 的交叉 cooldown 语义：
        /// `KeyDefend + CdJump` 表示 jump 输入分支。
        /// </summary>
        protected virtual bool IsSharedCharacterDatJumpInputReadyInternal()
        {
            return Runtime.KeyDefend != 0 && Runtime.CdJump > 0;
        }

        /// <summary>
        /// shared character-DAT 的防御输入入口。
        /// 对齐参考 C# 的交叉 cooldown 语义：
        /// `KeyAttack + CdDefend` 表示 defend 输入分支。
        /// </summary>
        protected virtual bool IsSharedCharacterDatDefendInputReadyInternal(bool requireDefendLockOpen = false)
        {
            if (Runtime.KeyAttack == 0 || Runtime.CdDefend <= 0)
                return false;

            return !requireDefendLockOpen || Runtime.CdDefendLock <= 0;
        }

        private void ApplySharedCpointActionStep10(int actionFrame, LF2Entity victim)
        {
            if (victim == null)
                return;

            ApplySignedCpointActionFramePreserveWait(actionFrame);
            int victimAction = Frame?.D?.cpoint?.vaction ?? 0;
            victim.SetCpointRawFramePreserveWait(victimAction);
            victim.AttackingCounter = 0;
            AttackingCounter = 0;
        }

        internal void ApplySignedCpointActionFramePreserveWait(int frameId)
        {
            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            SetCpointRawFramePreserveWait(frameId);
        }

        private void PropagateCpointThrowTransformToOwnedObjects(LF2CharacterDataWrapper wrapper, int targetObjectId)
        {
            var objects = new List<LF2Entity>();
            Match?.GetAllEntities(objects);
            int selfSlotIndex = Runtime?.SlotIndex ?? -1;
            if (selfSlotIndex < 0)
                return;

            for (int i = 0; i < objects.Count; i++)
            {
                LF2Entity entity = objects[i];
                if (entity == null || entity == this)
                    continue;
                if (!(Match?.IsActiveForCurrentPassInternal(entity) ?? false))
                    continue;
                if (entity.KillCount != selfSlotIndex)
                    continue;

                entity.FrameCache.Load(wrapper);
                entity.ObjectId = targetObjectId;
                entity.WeaponCount = wrapper.characterData?.weapon_hp ?? 0;
                entity.EnsureSharedCharacterDatControllerForSimulation();

                if (!entity.ReloadCurrentFrameDataFromWrapper())
                    entity.RefreshRuntimeSnapshot();
            }
        }

        protected virtual void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint)
        {
            if (victimEntity == null || catcherFrame == null || catcherCpoint == null)
                return;

            int catcherX = GetReleaseXInt();
            int catcherY = GetReleaseYInt();
            int catcherZ = GetReleaseZInt();
            int dx = Runtime.Dir == "right"
                ? catcherX - catcherFrame.centerx + catcherCpoint.x
                : catcherFrame.centerx - catcherCpoint.x + catcherX;
            int dy = catcherY - catcherFrame.centery + catcherCpoint.y;

            LF2FrameData victimActionFrame = victimEntity.FrameCache?.GetFrameDataById(catcherCpoint.vaction);
            LF2FrameData victimCurrentFrame = victimEntity.FrameCache?.GetFrameDataById(victimEntity.Frame?.N ?? 0);
            int victimCpointX = victimActionFrame?.cpoint?.x ?? 0;
            int victimCpointY = victimActionFrame?.cpoint?.y ?? 0;
            int victimCenterX = victimCurrentFrame?.centerx ?? 0;
            int victimCenterY = victimCurrentFrame?.centery ?? 0;

            victimEntity.Runtime.X = victimEntity.Runtime.Dir == "right"
                ? victimCenterX - victimCpointX + dx
                : victimCpointX - victimCenterX + dx;
            victimEntity.Runtime.Y = victimCenterY - victimCpointY + dy;
            victimEntity.Runtime.Z = catcherZ;

            int coverDiv = catcherCpoint.cover / 10;
            int coverRem = catcherCpoint.cover % 10;
            if (coverRem != 0)
            {
                victimEntity.Runtime.Z += 1f;
                victimEntity.Runtime.Y -= 1f;
            }
            else
            {
                victimEntity.Runtime.Z -= 1f;
                victimEntity.Runtime.Y += 1f;
            }

            if (coverDiv == 1)
                victimEntity.SwitchDir(Runtime.Dir);
            else if (coverDiv == 2)
                victimEntity.SwitchDir(Runtime.Dir == "right" ? "left" : "right");

            victimEntity.RefreshRuntimeSnapshot();
        }

        private void SyncCaughtByCpointStep10(LF2Entity victim, LF2FrameData catcherFrame, CatchPoint cpoint)
        {
            if (victim == null || cpoint == null)
                return;

            if (cpoint.hurtable == 0 || (victim.FrameDelay == 0 && cpoint.hurtable == 1))
            {
                victim.SetCpointRawFramePreserveWait(cpoint.vaction);
            }

            if (victim.Frame?.N < 0)
            {
                victim.SwitchDir(victim.Runtime.Dir == "left" ? "right" : "left");
                victim.SetCpointRawFramePreserveWait(-victim.Frame.N);
            }

            int injury = cpoint.injury;
            if (injury != 0 && AttackingCounter == 0)
                ApplyCpointHeldInjuryStep10(victim, injury);

            SyncCpointHeldPositionStep10(victim, catcherFrame, cpoint);
        }

        internal void SetCpointRawFramePreserveWait(int frameId)
        {
            if (Frame == null || FrameCache == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            Frame.N = frameId;
            Frame.D = targetFrame;
            if (targetFrame != null)
                Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
        }

        internal void SetCpointRawPrevFrame2(int frameId)
        {
            if (Frame == null)
                return;

            Frame.Prev2 = frameId;
            Frame.Prev2D = FrameCache?.GetFrameDataById(frameId);
            Runtime.PrevFrame2 = frameId;
        }

        private int GetReleaseXInt()
        {
            return Runtime.XInt;
        }

        private int GetReleaseYInt()
        {
            return Runtime.YInt;
        }

        private int GetReleaseZInt()
        {
            return Runtime.ZInt;
        }

        // 当 FrameTransistor 发现“当前 frame 已经不是 waitCounter 记录的那一帧”时，会先通知这里。
        public virtual void OnFrameTickFrameChangedFromWaitCounter()
        {
            int frameId = Frame?.N ?? -1;
            string soundId = Frame?.D?.sound;
            if (frameId < 0 || frameId >= 400 || string.IsNullOrWhiteSpace(soundId))
                return;

            Match?.QueueSound(soundId, Runtime.XInt);
        }

        // FrameTransistor 在真正比较 wait 之前，会先进这里。
        // 公共计数器衰减和某些早退条件，都在这一层统一处理。
        public virtual bool OnFrameTickBeforeWaitAdvance(int previousFrame)
        {
            if (Frame?.D == null)
                return false;

            RunReleaseFrameTickCounters();

            if (Frame.D.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            return ApplyObjectSpecificFrameTickBeforeWaitAdvance();
        }

        // FrameTransistor 决定要换帧时，通过这个钩子把目标帧请求交给实体自身处理。
        public virtual void OnFrameTickTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans);
        }

        // 真正换帧成功后，才会走到这个后置钩子。
        public virtual void OnFrameTickAfterWaitAdvance(int previousFrame, bool allowJumpInit)
        {
            ApplyCommonCaughtExitHitStop(previousFrame);
            ApplyCommonFrameTickPpDisplayPostAdvance();
        }

        // next=999 的最终落点由实体自己决定，不同对象可以有不同语义。
        public virtual int ResolveFrameTickNext999Target(out bool allowJumpInit)
        {
            allowJumpInit = false;
            return 0;
        }

        protected virtual bool ApplyObjectSpecificFrameTickBeforeWaitAdvance() => true;

        /// <summary>
        /// C# 基准工程 FrameTick.Tick 的公共计数器衰减段。
        /// 该段位于 cpoint kind=2 早退之前，所有实体都要按同一顺序执行。
        /// </summary>
        private void RunReleaseFrameTickCounters()
        {
            // AttackExempt is now decremented in RunCommonFrameTick before LinkState guard (BMD-062)

            if (HitStun > 0)
                HitStun--;
            else if (HitStun < 0)
                HitStun++;

            if (FallCounter > 0)
                FallCounter--;

            if (HitStateCount > 0)
                HitStateCount--;

            if (HitConfirmCounter > 0)
                HitConfirmCounter--;
        }

        protected virtual void ApplyCommonCaughtExitHitStop(int previousFrameId)
        {
            LF2FrameData previousFrame = FrameCache?.GetFrameDataById(previousFrameId);
            if (previousFrame == null || previousFrame.state != LF2States.Lying)
                return;

            if ((Frame?.D?.state ?? 0) == LF2States.Frozen)
                return;

            if (RelationTeam == 5 || Unk344 != 0)
            {
                if ((Match?.Difficulty ?? 2) == 2)
                    return;

                int gameMode = Match?.BattleGameModeId ?? 0;
                bool oidSkip = (gameMode == 1 || gameMode == 4) &&
                               ObjectId / 5 == 3 &&
                               ObjectId != 38;
                if (oidSkip)
                    return;
            }

            HitStun = 15;
        }

        protected virtual bool IsFrameTickLeftPressed() => Runtime?.KeyLeft != 0;

        protected virtual bool IsFrameTickRightPressed() => Runtime?.KeyRight != 0;

        protected virtual bool IsFrameTickUpPressed() => Runtime?.KeyUp != 0;

        protected virtual bool IsFrameTickDownPressed() => Runtime?.KeyDown != 0;

        protected virtual int GetFrameTickCdUp() => Runtime?.CdUp ?? 0;

        protected virtual int GetFrameTickCdDown() => Runtime?.CdDown ?? 0;

        protected virtual void ApplyFrame212JumpInit()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return;

            Runtime.Vy = characterData.jump_height;
            if (IsFrameTickRightPressed() && !IsFrameTickLeftPressed())
                Runtime.Vx = characterData.jump_distance;
            else if (IsFrameTickLeftPressed() && !IsFrameTickRightPressed())
                Runtime.Vx = -characterData.jump_distance;

            if (IsFrameTickUpPressed() && !IsFrameTickDownPressed())
                Runtime.Vz = -characterData.jump_distancez;
            else if (IsFrameTickDownPressed() && !IsFrameTickUpPressed())
                Runtime.Vz = characterData.jump_distancez;
        }

        /// <summary>
        /// 对齐参考 `FrameTick` 的负 mp 帧推进后处理。
        /// 当前只收敛已确认的 PP 真值与 PpDisplay 累计面，不扩展到 HUD 刷新。
        /// </summary>
        protected void ApplyCommonFrameTickPpDisplayPostAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Health == null || !IsPpModeEnabled())
                return;
            if ((Frame?.N ?? -1) >= 400)
                return;
            int mpDelta = frame.mp;
            if (mpDelta >= 0)
                return;

            if (Health.PP < mpDelta)
            {
                SetFrameTickDirect(frame.hit_d);
                frame = Frame?.D;
                if (frame == null)
                    return;
            }
            else
            {
                Health.PP += mpDelta;
                SpendPpDisplay(-mpDelta);
            }

            int turnNext = frame.hit_d;
            if (turnNext <= 0 || GetRuntimeYInt() != 0)
                return;

            bool left = Runtime?.KeyLeft != 0;
            bool right = Runtime?.KeyRight != 0;
            if (left && !right && Runtime?.Dir == "right")
                SetFrameTickDirect(turnNext);
            else if (right && !left && Runtime?.Dir == "left")
                SetFrameTickDirect(turnNext);
        }

        protected bool TryEnterReleaseFrameAdvanceAfterDelay()
        {
            if (ThrowFrameGuard >= 0 && ThrowFrameGuard == (Frame?.N ?? -1))
                return false;

            if (FrameDelay > 0)
            {
                FrameDelay--;
                return false;
            }

            if (FrameDelay < 0)
            {
                FrameDelay++;
                return false;
            }

            return true;
        }

        protected void RunSharedCharacterDatFrameAdvanceAsCharacter(int tickIndex, bool consumeForcedRuntimeIntPosition = true)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return;

            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return;

            float mass = NTSDGlobal.Default.Machanics.Mass;
            var mechanics = new CharacterMechanics();
            var context = new CharacterMechanicsContext(
                Runtime,
                Frame?.D,
                GetSpriteWidthPxForCollision(),
                mass,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                point =>
                {
                    SimulationWorld world = Match;
                    return world == null || world.IsGroundPointWalkable(point);
                });

            MechanicsStepResult stepResult = mechanics.Step(context);
            if (stepResult.landed)
                ApplySharedCharacterDatLandingIfNeeded(stepResult.verticalVelocityBeforeLanding);

            Runtime.SyncIntegerPosition();
            PromoteSharedCharacterDatState12AirborneFrameIfNeeded(tickIndex);
            PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded();
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            if (consumeForcedRuntimeIntPosition)
                ConsumeForcedRuntimeIntPosition();
        }

        protected bool RunSharedNonCharacterDatFrameAdvance(bool consumeForcedRuntimeIntPosition = true)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return false;
            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return false;
            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            ApplyNonCharacterFrameVelocityForFrameAdvance();

            int dataType = GetCurrentDataObjectTypeForSimulation();
            LF2FrameData frame = Frame?.D;
            if (Runtime == null || frame == null)
                return false;

            if (dataType == (int)LF2ObjectType.ThrowWeapon || ObjectId == 120)
                Runtime.X += Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;
            if (ObjectId == 101)
                Runtime.X -= Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;

            if (dataType == (int)LF2ObjectType.SpecialAttack && frame.hit_j > 0)
            {
                double visualZ = frame.hit_j - 50;
                Runtime.Z += visualZ;
                Runtime.Type3VisualZOffset += visualZ;
            }

            if ((dataType == (int)LF2ObjectType.ThrowWeapon || dataType == (int)LF2ObjectType.Drink) &&
                frame.state == 1000 &&
                System.Math.Abs(Runtime.Vx) > 9.0)
            {
                SetFrameTickDirect(40);
                frame = Frame?.D ?? frame;
            }

            double gravity = ResolveCurrentDatWeaponGravity(dataType, frame.state);
            bool landed = CharacterMechanics.WeaponDynamics(Runtime, gravity, out double landingVy);
            ApplyCurrentDatNonCharacterLanding(dataType, frame, landingVy, landed);
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            Runtime.SyncIntegerPosition();
            if (consumeForcedRuntimeIntPosition)
                ConsumeForcedRuntimeIntPosition();
            RefreshRuntimeSnapshot();
            return true;
        }

        protected bool ApplyCurrentDatNonCharacterLanding(
            int dataType,
            LF2FrameData landingFrame,
            double landingVy,
            bool crossedGround)
        {
            if (Runtime == null || landingFrame == null)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            int dropHurt = characterData?.weapon_drop_hurt ?? 0;
            string dropSound = characterData?.weapon_drop_sound;
            int state = landingFrame.state;

            if (dataType == (int)LF2ObjectType.LightWeapon)
            {
                if (!crossedGround)
                    return true;

                Runtime.WeaponFlightCounter -= dropHurt;
                Runtime.Y = 0.0;
                if (landingVy <= 9.9)
                {
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(state == LF2States.WeaponThrowing ? 70 : 60);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }
                else if (state == LF2States.WeaponThrowing)
                {
                    Runtime.Vy = -8.0;
                    SetFrameTickRawDirect(7);
                    SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                    Runtime.Vx *= 0.5;
                    QueueBattleSound(dropSound);
                }
                else
                {
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(60);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }

                return true;
            }

            if (dataType == (int)LF2ObjectType.HeavyWeapon)
            {
                if (!crossedGround)
                    return true;

                Runtime.WeaponFlightCounter -= 1;
                Runtime.Y = 0.0;
                if (landingVy > 9.0)
                {
                    QueueBattleSound(dropSound);
                    Runtime.Vy = -5.0;
                    SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                    Runtime.Vx *= 0.5;
                }
                else
                {
                    Runtime.WeaponFlightCounter -= dropHurt;
                    if (Runtime.WeaponFlightCounter < 0)
                        Runtime.WeaponFlightCounter = 0;
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(20);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }

                return true;
            }

            if (dataType == (int)LF2ObjectType.ThrowWeapon ||
                dataType == (int)LF2ObjectType.Drink)
            {
                if (!crossedGround)
                    return true;

                Runtime.WeaponFlightCounter -= dropHurt;
                if (dataType == (int)LF2ObjectType.Drink && Health != null && Health.HP <= 0)
                    Runtime.WeaponFlightCounter = -1;

                Runtime.Y = 0.0;
                bool highSpeed = landingVy > 8.5 || Runtime.Vx < -10.0 || Runtime.Vx > 10.0;
                bool bounceState = state == LF2States.WeaponThrowing || state == LF2States.WeaponInSky;
                if (highSpeed && bounceState)
                {
                    Runtime.Vy = landingVy * -0.7;
                    if (Runtime.Vy < -10.0)
                        Runtime.Vy = -10.0;
                    Runtime.Vx *= 0.7;
                    SetFrameTickRawDirect(0);
                    QueueBattleSound(dropSound);
                }
                else
                {
                    Runtime.Vy = 0.0;
                    Runtime.Vx *= 0.7;
                    SetFrameTickRawDirect(state == LF2States.WeaponThrowing ? 70 : 60);
                    AttackingCounter = 0;
                }

                return true;
            }

            if (ObjectId == 999 && crossedGround)
            {
                Runtime.Y = 0.0;
                Runtime.Vy = 0.0;
                Runtime.Vx = 0.0;
                SetFrameTickRawDirect(101);
                AttackingCounter = 0;
                return true;
            }

            return false;
        }

        private double ResolveCurrentDatWeaponGravity(int dataType, int state)
        {
            if (dataType == (int)LF2ObjectType.SpecialAttack)
                return 0.0;
            if (dataType == (int)LF2ObjectType.Drink)
                return NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
            if (dataType == (int)LF2ObjectType.ThrowWeapon)
                return 0.85;
            if (state != LF2States.WeaponThrowing)
                return NTSDGlobal.Gameplay.WeaponGravityDefault;

            switch (ObjectId)
            {
                case 124:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub7C;
                case 120:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub78;
                case 101:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
                default:
                    return NTSDGlobal.Gameplay.WeaponGravityDefault1002;
            }
        }

        private void ApplySharedCharacterDatLandingIfNeeded(double landedVy) // P0-f-2b B2-1: float→double
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return;

            if (frame.state == LF2States.Falling || frame.state == LF2States.Burning)
            {
                QueueBattleSound("SFX_006");
                ApplySharedCharacterDatLandingWeaponCountDamage();

                if (landedVy <= 11.0 &&
                    Runtime.Vx <= 9.0 &&
                    Runtime.Vx >= -9.0 &&
                    frame.state != LF2States.Burning)
                {
                    Runtime.Y = 0.0;
                    Runtime.Vy = 0.0;
                    Runtime.Vx *= 0.3333333333333333;
                    AttackingCounter = 0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack
                        ? LF2StandardFrames.LyingBack
                        : LF2StandardFrames.Lying);
                }
                else
                {
                    Runtime.Y = 0.0;
                    Runtime.Vy = -3.5;
                    if (Runtime.Vx > 7.0)
                        Runtime.Vx = 7.0;
                    if (Runtime.Vx < -7.0)
                        Runtime.Vx = -7.0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack && frame.state != LF2States.Burning
                        ? LF2StandardFrames.FallingBack5
                        : LF2StandardFrames.FallingFront5);
                }

                return;
            }

            if (frame.state == LF2States.Frozen && landedVy > 0.0001)
            {
                Runtime.Y = 0.0;

                if (landedVy <= 17.0 && Runtime.Vx <= 9.0 && Runtime.Vx >= -9.0)
                {
                    Runtime.Vx *= 0.3333333333333333;
                    Runtime.Vy = 0.0;
                    return;
                }

                int injury = FallDamageDiv == 0 ? 10 : 1000 / FallDamageDiv;
                if (Health != null)
                    Health.HP -= injury;

                Runtime.Vy = -3.5;
                if (Runtime.Vx > 7.0)
                    Runtime.Vx = 7.0;
                if (Runtime.Vx < -7.0)
                    Runtime.Vx = -7.0;
                ImmediateFrame(LF2StandardFrames.FallingFront5);
                return;
            }

            Runtime.Y = 0.0;
            Runtime.Vy = 0.0;
            Runtime.Vx *= 0.3333333333333333;
            AttackingCounter = 0;

            int landingFrame;
            if (frame.state == LF2States.CustomSkill1)
                landingFrame = 94;
            else if (Frame.N == LF2StandardFrames.JumpingAir || frame.state == LF2States.Rowing)
                landingFrame = LF2StandardFrames.Crouch;
            else
                landingFrame = LF2StandardFrames.Crouch2;

            ImmediateFrame(landingFrame);
        }

        private void ApplySharedCharacterDatLandingWeaponCountDamage()
        {
            if (WeaponCount == 0 || Health == null)
                return;

            int damage = WeaponCount < 0 ? -WeaponCount : WeaponCount;
            if (FallDamageDiv > 0)
                damage = damage * 100 / FallDamageDiv;

            Health.HP -= damage;
            Health.HPBound -= damage;
            WeaponCount = 0;
        }

        private void PromoteSharedCharacterDatState12AirborneFrameIfNeeded(int tickIndex)
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                return;

            if (Runtime == null || Runtime.Y >= 0f)
                return;

            int frameId = Frame.N;
            double vy = Runtime.Vy;

            if (frameId < LF2StandardFrames.FallingFront5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingFront3);

                PromoteSharedCharacterDatState12NegativeWeaponCountCadenceOverride(tickIndex);
            }
            else if (frameId > LF2StandardFrames.FallingFront5 && frameId < LF2StandardFrames.FallingBack5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingBack3);
            }
        }

        private void PromoteSharedCharacterDatState12NegativeWeaponCountCadenceOverride(int tickIndex)
        {
            if (WeaponCount >= 0)
                return;

            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                return;

            if (Runtime == null || Runtime.Y >= 0f || Runtime.Vy >= 12f)
                return;

            int cadencePhase = (tickIndex - 1) % 12;
            if (cadencePhase < 0)
                cadencePhase += 12;

            SetFrameTickDirect(cadencePhase >= 6
                ? LF2StandardFrames.FallingFront2
                : LF2StandardFrames.FallingFront1);
        }

        private void PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Burning)
                return;

            if (Frame.N >= LF2StandardFrames.Fire2)
                return;

            if (Runtime == null || Runtime.Y >= 0f || Runtime.Vy <= 1.0f)
                return;

            SetFrameTickDirect(LF2StandardFrames.Fire2);
        }

        protected void ResetWeaponCountOutsideState12FrameAdvanceTail()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                WeaponCount = 0;
        }

        protected void SetFrameTickDirect(int frameId)
        {
            SetFrameTickDirect(frameId, Trans?.WaitCounter ?? 0);
        }

        protected void SetFrameTickDirect(int frameId, int waitCounter)
        {
            if (Frame == null || FrameCache == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return;

            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, waitCounter);
        }

        /// <summary>
        /// 处理参考命中特效里的编码 effect 段。
        /// 5000..5999 表示直接扣 PP，6000..6999 表示直接写目标帧。
        /// 这两段只改逻辑真值，不属于 PpDisplay 的输入/表现累计来源。
        /// </summary>
        internal bool ApplyCommonEncodedHitEffectRange(int effectNum)
        {
            if (effectNum >= 5000 && effectNum < 6000)
            {
                if (Health != null)
                {
                    int nextPp = Health.PP - (effectNum - 5000);
                    Health.PP = nextPp < 0 ? 0 : nextPp;
                }

                return true;
            }

            if (effectNum >= 6000 && effectNum < 7000)
            {
                DirectWriteFramePreserveWaitCounter(effectNum - 6000);
                return true;
            }

            return false;
        }

        protected virtual bool RunCommonFrameTick()
        {
            if (ThrowFrameGuard >= 0 && ThrowFrameGuard == (Frame?.N ?? -1))
                return false;

            int dataType = GetCurrentDataObjectTypeForSimulation();
            if (FrameDelay != 0 && dataType != (int)LF2ObjectType.SpecialAttack)
                return false;

            if (AttackExempt > 0)
                AttackExempt--;

            if ((Runtime?.LinkState ?? 0) < 0)
                return false;

            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return false;
            if (frame.cpoint != null && frame.cpoint.kind == 2)
                return false;

            if (dataType == (int)LF2ObjectType.SpecialAttack && frame.hit_a > 0 && Health != null)
            {
                Health.HP -= frame.hit_a;
                if (Health.HP <= 0)
                {
                    Health.HP = 0;
                    SetFrameTickRawDirect(frame.hit_d);
                    frame = Frame?.D;
                    if (frame == null)
                        return false;
                }
            }

            RunReleaseFrameTickCounters();

            int waitCounter = Trans?.WaitCounter ?? 0;
            if ((Frame?.N ?? 0) != waitCounter)
            {
                OnFrameTickFrameChangedFromWaitCounter();
                AttackingCounter = 0;
            }

            AttackingCounter++;

            int state = frame.state;
            bool suppressJumpInit = false;
            if (state == 0 && GetRuntimeYInt() < 0)
            {
                SetFrameTickRawDirect(212);
                suppressJumpInit = true;
                frame = Frame?.D;
                if (frame == null)
                    return false;
                state = frame.state;
            }

            if (dataType == (int)LF2ObjectType.HeavyWeapon &&
                state == LF2States.HeavyWeaponInSky &&
                GetRuntimeYInt() == 0 &&
                System.Math.Abs(Runtime.Vx) < 0.1)
            {
                return false;
            }

            if (state == LF2States.Lying && Health != null && Health.HP <= 0)
            {
                if ((KillCount >= 0 || RelationTeam == 5 || (Runtime?.SlotIndex ?? -1) >= 20) && HitStun <= 0)
                    HitStun = 30;
                AttackingCounter = 0;
            }

            if (state == LF2States.HeavyWeaponInSky)
                SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            int wait = Trans?.Wait ?? frame.wait;
            if (AttackingCounter > wait)
            {
                int next = Trans?.Next ?? frame.next;
                AttackingCounter = 0;
                if (next != 0)
                {
                    bool allowJumpInit = true;
                    int targetFrame = next;
                    if (targetFrame == 999)
                    {
                        bool to212 = GetRuntimeYInt() != 0 && dataType == (int)LF2ObjectType.Character;
                        targetFrame = to212 ? 212 : 0;
                        suppressJumpInit = to212;
                        allowJumpInit = false;
                    }
                    else if (targetFrame < 0)
                    {
                        targetFrame = -targetFrame;
                        SwitchDir(Runtime?.Dir == "left" ? "right" : "left");
                    }

                    int previousFrame = waitCounter;
                    OnFrameTickTransit(targetFrame, false);
                    int frameAfterTransit = Frame?.N ?? targetFrame;
                    if (frameAfterTransit < 0 || frameAfterTransit >= 400 || Frame?.D == null)
                        return false;

                    ApplyCommonCaughtExitHitStop(previousFrame);
                    if (frameAfterTransit == 212 && allowJumpInit && !suppressJumpInit)
                        ApplyFrame212JumpInit();
                    ApplyCommonFrameTickPpDisplayPostAdvance();
                }
            }

            int currentFrame = Frame?.N ?? -1;
            if (currentFrame == 110 || currentFrame == 114)
                Runtime.CdDefendLock = 3;
            if (currentFrame == 202)
                HitStun = 20;

            LF2FrameData currentData = Frame?.D;
            if (currentData != null)
                Trans?.SyncWaitCounterFrame(currentFrame);

            return true;
        }

        internal bool RunCommonFrameTickFromTransistor()
        {
            return RunCommonFrameTick();
        }

        private void SetFrameTickRawDirect(int frameId)
        {
            if (Frame == null)
                return;

            Frame.N = frameId;
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        protected void SpendPpDisplay(int ppCost)
        {
            if (ppCost > 0 && Runtime != null)
                Runtime.PpDisplay += ppCost;
        }

        protected void RefundPpDisplay(int ppDelta)
        {
            if (ppDelta > 0 && Runtime != null)
                Runtime.PpDisplay -= ppDelta;
        }

        protected void ApplyNonCharacterFrameVelocityForFrameAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Runtime == null)
                return;

            double vx = Runtime.Vx;
            ApplyFrameAxisVelocity(frame.dvx, ref vx, Dirh());
            Runtime.Vx = vx;

            if (frame.dvy > 500)
                Runtime.Vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                Runtime.Vy += frame.dvy;

            if (frame.dvz > 500)
            {
                Runtime.Vz = frame.dvz - 550;
                return;
            }

            if (frame.dvz == 0)
                return;

            if (IsFrameTickUpPressed() && GetFrameTickCdUp() >= GetFrameTickCdDown())
                Runtime.Vz = -frame.dvz;
            if (IsFrameTickDownPressed() && GetFrameTickCdDown() >= GetFrameTickCdUp())
                Runtime.Vz = frame.dvz;
        }

        private static void ApplyFrameAxisVelocity(int value, ref double velocity, int direction) // P0-f: double sim velocity
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 550)
            {
                velocity = 0f;
                return;
            }

            if (value > 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity < target)
                        velocity = target;
                }
                else if (velocity > target)
                {
                    velocity = target;
                }

                return;
            }

            if (value >= 0)
                return;

            float negativeTarget = value * direction;
            if (direction >= 0)
            {
                if (velocity > negativeTarget)
                    velocity = negativeTarget;
            }
            else if (velocity < negativeTarget)
            {
                velocity = negativeTarget;
            }
        }



        /// <summary>分配稳定 ID。</summary>
        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
            Runtime.StableId = StableId;
        }

        /// <summary>重置稳定 ID。</summary>
        protected void ResetStableId()
        {
            StableId = 0;
            Runtime.StableId = 0;
        }

        /// <summary>写入运行时槽位索引。</summary>
        public void SetRuntimeSlotIndex(int slotIndex)
        {
            Runtime.SlotIndex = slotIndex;
        }

        /// <summary>刷新 Runtime 中的派生字段和非位置状态。</summary>
        public void RefreshRuntimeSnapshot()
        {
            RefreshRuntimeFromEntity();
        }

        protected virtual void RefreshRuntimeFromEntity()
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();

            Runtime.StableId = StableId;
            Runtime.ObjectId = ObjectId;
            Runtime.ObjType = ResolveReferenceRuntimeObjTypeFromDataType(currentDataType);
            Runtime.EntityType = currentDataType;
            Runtime.Team = Team;
            Runtime.OwnerSlotIndex = OwnerEntityIndex;
            Runtime.OwnerStableId = OwnerId;
            Runtime.GrabbedBy = GrabbedBy;
            Runtime.TrackerFlag = TrackerFlag;
            Runtime.Frame = Frame?.N ?? 0;
            Runtime.WaitCounter = Trans?.WaitCounter ?? 0;
            Runtime.NextFrame = Trans?.Next ?? 0;
            Runtime.AttackingCounter = AttackingCounter;
            Runtime.FrameDelay = FrameDelay;
            Runtime.HitStop = HitStun;
            Runtime.AttackExempt = AttackExempt;
            Runtime.HealTimer = HealTimer;
            Runtime.KillCount = KillCount;
            Runtime.ShotCount = ShotCount;
            Runtime.HPOrig = HPOrig;
            Runtime.HP2Orig = HP2Orig;
            Runtime.RespawnCount = RespawnCount;

            if (!_hasForcedRuntimeIntPosition)
                RefreshRuntimeIntPosition();

            if (Health != null)
            {
                Runtime.HP = Health.HP;
                Runtime.MP = Health.MP;
                Runtime.PP = Health.PP;
                Runtime.PPMax = Health.MaxPP;
                Runtime.PPBound = Health.PPBound;
                Runtime.HPLost = Health.HPLost;
                Runtime.HPBound = Health.HPBound;
                Runtime.MPMax = Health.MaxMP;
            }
        }

        private void RefreshRuntimeIntPosition()
        {
            Runtime.SyncIntegerPosition();
        }

        /// <summary>
        /// C# 基准工程的 Physics.SyncIntegers 使用 (int) 强制转换。
        /// 这里必须截断而不是四舍五入，否则阴影、碰撞和 opoint 的整数坐标会持续偏移。
        /// </summary>
        private int ReleaseInt(double value) // P0-f: truncate double directly (baseline (int)X); float callers widen
        {
            return (int)value;
        }

    }
}


--- File: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs ---
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Game;
using NTSD.Input;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Test
{
    /// <summary>
    /// 战斗运行时自检工具。
    /// 只在测试场景或编辑器菜单中手动启用，用最小帧数据验证 C++ release 对齐过的关键战斗分支。
    /// </summary>
    public sealed class BattleRuntimeSelfCheck : MonoBehaviour
    {
        [Header("启动设置")]
        [Tooltip("进入 Play 后自动执行自检")]
        [SerializeField] private bool runOnStart = false;

        [Tooltip("全部通过后销毁该 GameObject")]
        [SerializeField] private bool destroyWhenPassed = false;

        private void Start()
        {
            if (runOnStart)
                RunAllChecks();
        }

        [ContextMenu("运行战斗运行时自检")]
        public void RunAllChecks()
        {
            RunAllChecksStatic();

            if (destroyWhenPassed)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        public static void RunAllChecksStatic()
        {
            try
            {
                using var singletonSceneObjects = new TemporarySingletonSceneObjectScope();
                BattleRuntimeSelfCheckCore.RunAllChecks();
                CheckReferencePoolObjectIdPreserved();
                CheckReferencePoolRejectsUnownedObjects();
                CheckSpriteDimensionsUseFullRect();
                CheckEntityAndShadowRenderPositionFormula();
                CheckUnityBattleCameraRemainsDisabled();
                CheckCharacterGroundMovementUsesIntegerSnapshot();
                CheckCatchingAttackAction();
                CheckCatchingJumpAction();
                CheckCatchingThrow();
                CheckCpointDirControlUsesRuntimeInput();
                CheckBeingCaughtPositionSync();
                CheckCpointStateExitPreservesRunningDefendJumpChain();
                CheckCpointNegativeActionMatrix();
                CheckCpointHeldSyncVactionMatrix();
                CheckCpointThrowRawAndTransformMatrix();
                CheckCpointDecreaseEscape();
                CheckCpointEscapeAndMismatchStillRunTail();
                CheckSharedDatCpointStep10StatsAndInputOrder();
                CheckBattleFlowToggleAndTeleportMatrix();
                CheckValidatePositiveLinksMatrix();
                CheckHeldReferenceSlotReuseContracts();
                CheckHeldWeaponActCoverOffsets();
                CheckHeldWeaponActSkipsOrdinaryStrengthAttack();
                CheckHeldKind5ConsumesFrozenCandidates();
                CheckGenericHeldStep12ContinuationContracts();
                CheckReleaseTickRunsHeldStep12Once();
                CheckLateHolderFrameChangeResyncsHeldPose();
                CheckReleaseTickCpointSyncPrecedesCandidates();
                CheckReleaseTickZClampPrecedesCandidates();
                CheckPreFrameXBoundsMatrix();
                CheckQueuedObjectPointPassBoundaries();
                CheckInteractionRuntimeSlotContracts();
                CheckSimulationWorldLateMutation();
                CheckCollisionCandidateCapAndNewbornIsolation();
                CheckCollisionAudit3Contracts();
                CheckSpecialAttackStep4AndLateFrameTick();
                CheckCurrentDatFrameLogicSharedRouting();
                CheckFrameTickPpDisplayAndCurrentDatMatrix();
                CheckGameTickInputClearBoundaries();
                CheckSharedCharacterLandingNumericAndDamageBoundaries();
                CheckStateTransformLandingMatrix();
                CheckStateTransformInteractionPhaseRouting();
                CheckSerialTickInterleaveAndFrameEdgeMatrix();
                CheckState0BelowGroundFrame212PreservesAttackingCounter();
                CheckSimulationPassesImmediateFrameDoesNotZeroAttacking();
                CheckArestCooldownRule();
                CheckFrameTickDefendLockTail();
                CheckKind0HitRecords();
                CheckAudit4AttackExemptAndStandardHitContracts();
                CheckAudit4FrozenCandidateAndKind3Contracts();
                CheckAudit4ArchitectDefectContracts();
                CheckAudit4SpecialHeldAndOpointContracts();
                CheckAlternateHurtTriggerMatrix();
                CheckAlternateDamageCoreSideEffects();
                CheckAlternateDamageMotionTailMatrix();
                CheckAlternateDamageCharacterEntry();
                CheckAlternateDamageSharedDatEntry();
                CheckAlternateDamageHeavyWeaponEntries();
                CheckAlternateDamageInteractionVrest();
                CheckSpecialAttackDamagePreprocess();
                CheckOid5152MergeSuccessAndDormantIsolation();
                CheckOid5152MergeCooldownOneTriggersSameTick();
                CheckOid5152AuthorityGateMatrix();
                CheckOid5152MirrorIdentityAndPresentation();
                CheckOid5152SplitSuccessAndOddTruncate();
                CheckOid5152SplitFailurePartialRecovery();
                CheckOid5152DjaReleaseTriggersSameTickSplit();
                CheckRespawnPassWithoutStoredCount();
                CheckRespawnPassFreeEntityGate();
                CheckRespawnPassWithStoredCountAndEffectSpawn();
                CheckKind15CharacterWhirlwind();
                CheckKind16CharacterSideEffects();
                CheckLateDeathBounceFrame();
                CheckComboWrappersCharacterFrameJumps();
                CheckComboLocalShadowCommitContracts();
                CheckNarutoDdjSixCloneProductionChain();
                CheckOid6DjaGuardComboHold();
                CheckStageWaveBootstrapAndSpawnContract();
                CheckStageWaveImmediateSpawnAndAdvance();
                CheckStageWavePositiveSpawnRefill();
                CheckAiTargetCacheCoordinateAndDeterminism();
                CheckAiHumanInputIsolation();
                CheckAiHeldInactiveSlotContract();
                CheckAiSharedCharacterDatShell();
                CheckBufferedHumanInputSemantics();
                CheckRecordedInputAlignmentContracts();
                CheckParityTraceInfrastructure();
                Debug.Log("[BattleRuntimeSelfCheck] 战斗运行时自检通过。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BattleRuntimeSelfCheck] 自检失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static void CheckParityTraceInfrastructure()
        {
            var rng = new DeterministicRng(0x12345678u);
            Expect(rng.State == 0x12345678u && rng.CallCount == 0,
                "parity RNG must expose its seeded state with a zero call count");
            uint expectedState = unchecked(0x12345678u * 0x343FDu + 0x269EC3u);
            int expectedRaw = (int)((expectedState >> 16) & 0x7FFFu);
            Expect(rng.NextRaw() == expectedRaw && rng.State == expectedState && rng.CallCount == 1,
                "parity RNG state/call count must advance with NextRaw");
            rng.Seed(0x12345678u);
            Expect(rng.State == 0x12345678u && rng.CallCount == 0,
                "parity RNG Seed must reset CallCount for replay bootstrap");

            var canonicalA = new Dictionary<string, object>
            {
                ["z"] = 2,
                ["a"] = 1,
            };
            var canonicalB = new Dictionary<string, object>
            {
                ["a"] = 1,
                ["z"] = 2,
            };
            string canonicalHash = BattleCanonicalJson.Sha256(canonicalA);
            Expect(canonicalHash == BattleCanonicalJson.Sha256(canonicalB),
                "canonical parity hash must not depend on dictionary insertion order");
            canonicalB["z"] = 3;
            Expect(canonicalHash != BattleCanonicalJson.Sha256(canonicalB),
                "canonical parity hash must be sensitive to field changes");

            LF2CharacterData data = new LF2CharacterData
            {
                name = "SelfCheck_ParityInput",
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        frameName = "self_check_parity_input",
                        state = 0,
                        wait = 10,
                        next = 0,
                    },
                },
            };
            LF2Character character = CreateCharacter("SelfCheck_ParityInput", 2, data);
            character.Team = 1;
            var world = new SimulationWorld();
            BattleSlotRuntimeState rosterSlot = world.Runtime.Roster.Slots[0];
            rosterSlot.Active = true;
            rosterSlot.IsHuman = true;
            rosterSlot.CharacterId = 2;
            rosterSlot.Team = 1;
            world.Runtime.Roster.ActiveSlotCount = 1;
            world.Register(character);

            world.ApplyFrameInputSet(new FrameInputSet(1, new[]
            {
                new SimulationPlayerInput(
                    0,
                    SimulationInputButtons.Right | SimulationInputButtons.Attack),
            }));
            character.RunPostCooldownInputPhase(1);
            Expect(character.Runtime.KeyRight == 1 && character.Runtime.KeyAttack == 1 &&
                   character.Runtime.PrevRight == 0 && character.Runtime.PrevAttack == 0 &&
                   character.Runtime.CdRight == 5 && character.Runtime.CdDefend == 5,
                "full frame input must create right/attack held state and fresh edges in the same tick");
            Expect(rosterSlot.RuntimeSlotIndex == character.Runtime.SlotIndex,
                "frame input must bind the roster player to the resolved fixed runtime slot");

            world.ApplyFrameInputSet(new FrameInputSet(2, new[]
            {
                new SimulationPlayerInput(
                    0,
                    SimulationInputButtons.Right | SimulationInputButtons.Attack),
            }));
            character.RunPostCooldownInputPhase(2);
            Expect(character.Runtime.KeyRight == 1 && character.Runtime.PrevRight == 1 &&
                   character.Runtime.CdRight == 4,
                "held frame input must not manufacture a second press edge");

            world.ApplyFrameInputSet(new FrameInputSet(3, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
            }));
            character.RunPostCooldownInputPhase(3);
            Expect(character.Runtime.KeyRight == 0 && character.Runtime.KeyAttack == 0 &&
                   character.Runtime.PrevRight == 1 && character.Runtime.PrevAttack == 1,
                "full frame input must apply releases on the requested tick");

            world.AdvanceBattleFlowTick(3);
            BattleParityFrameSnapshot first = world.CaptureParityFrameSnapshot(
                3,
                FrameInputSet.Empty(3));
            BattleParityFrameSnapshot second = world.CaptureParityFrameSnapshot(
                3,
                FrameInputSet.Empty(3));
            Expect(first.Hashes.Overall == second.Hashes.Overall &&
                   first.Hashes.ARest == "2e37abc158da57c53691211785eceb5b1a93f0d0f6f06bfeca854cbabdd11cfa" &&
                   first.Hashes.Events == "ea70093faeca028415dc3a0ab08d57702700f4941b5cdd279fe6f709106888c4",
                "parity snapshot hashes must be deterministic and use canonical empty rest/event domains");
            Expect(first.CompactSlotsDomain.Length == 1 &&
                   !first.ToJson().Contains("\"runtimeSlot\":399"),
                "compact parity JSON must omit reset-equivalent fixed runtime slots");

            string slotsBefore = first.Hashes.Slots;
            character.Runtime.X += 1.0;
            character.Runtime.SyncIntegerPosition();
            BattleParityFrameSnapshot moved = world.CaptureParityFrameSnapshot(
                3,
                FrameInputSet.Empty(3));
            Expect(moved.Hashes.Slots != slotsBefore,
                "slot domain hash must change when a fixed-slot runtime changes");

            character.ItrRest.Arest = 4;
            character.ItrRest.SetVrest(character.Runtime.SlotIndex, 6);
            BattleParityFrameSnapshot rested = world.CaptureParityFrameSnapshot(
                3,
                FrameInputSet.Empty(3));
            Expect(rested.Hashes.ARest != first.Hashes.ARest &&
                   rested.Hashes.VRest != first.Hashes.VRest,
                "arest/vrest domain hashes must be sensitive to sparse cooldown state");

            world.Unregister(character);
        }

        private static void CheckReferencePoolObjectIdPreserved()
        {
            LF2ReferencePool pool = LF2ReferencePool.Instance;
            Expect(pool != null, "reference pool identity fixture requires an LF2ReferencePool singleton");

            const int requestedObjectId = 407;
            ILF2Object obj = pool.Get(LF2ObjectType.Character, requestedObjectId);
            try
            {
                Expect(obj != null && obj.ObjectId == requestedObjectId,
                    $"reference pool Get must preserve requested ObjectId after Reset; actual={obj?.ObjectId ?? -1}");
            }
            finally
            {
                obj?.Reset();
                pool.Release(obj);
            }
        }

        private static void CheckReferencePoolRejectsUnownedObjects()
        {
            LF2ReferencePool pool = LF2ReferencePool.Instance;
            Expect(pool != null, "reference-pool ownership regression requires the production singleton");

            int availableBefore = pool.GetAvailableCount(LF2ObjectType.SpecialAttack);
            var unowned = new FlowSelfCheckEntity(LF2ObjectType.SpecialAttack);
            pool.Release(unowned);
            Expect(pool.GetAvailableCount(LF2ObjectType.SpecialAttack) == availableBefore,
                "releasing an object that was never borrowed must not contaminate the SpecialAttack pool");

            ILF2Object borrowed = pool.Get(LF2ObjectType.SpecialAttack, 205);
            Expect(borrowed is LF2SpecialAttack,
                $"SpecialAttack pool must return LF2SpecialAttack, actual={borrowed?.GetType().Name ?? "null"}");
            pool.Release(borrowed);
        }

        private static void CheckCharacterGroundMovementUsesIntegerSnapshot()
        {
            var data = new LF2CharacterData
            {
                name = "SelfCheck_IntegerGroundMovement",
                walking_speed = 4f,
                walking_speedz = 2f,
                walking_frame_rate = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(5, LF2States.Walking, 1, 5, 39, 79),
                },
            };

            LF2Character character = CreateCharacter("SelfCheck_IntegerGroundMovement", 1, data);
            character.Runtime.KeyRight = 1;
            character.Runtime.Y = -0.5;
            character.Runtime.YInt = 0;
            character.Runtime.Vx = 0.0;

            character.ApplyWalkRunFrameInternal(heavy: false);
            Expect(System.Math.Abs(character.Runtime.Vx - 4.0) <= 1e-12,
                $"walking ground gate must use YInt==0 when Y is fractional; actual Vx={character.Runtime.Vx:R}");

            character.Runtime.KeyRight = 0;
            character.Runtime.KeyUp = 1;
            const double initialVx = 123456789.125;
            character.Runtime.Vx = initialVx;
            character.ApplyRunLaneInternal(3f);
            double expectedVx = initialVx * (5.0 / 6.0);
            Expect(System.Math.Abs(character.Runtime.Vx - expectedVx) <= 1e-9,
                $"running lane factor must preserve double 5/6 precision; actual={character.Runtime.Vx:R}, expected={expectedVx:R}");

            CheckSharedCharacterDatGroundMovementUsesIntegerSnapshot();
        }

        private static void CheckBufferedHumanInputSemantics()
        {
            CheckMappedAction(
                "AttackAction/J",
                (input, pressed) => input.SetAttackActionPressed(pressed),
                expectedKeyJump: 1,
                expectedKeyDefend: 0,
                expectedKeyAttack: 0,
                expectedCooldown: runtime => runtime.CdAttack,
                expectedFrame: character => character.Frame.N == LF2StandardFrames.Punch ||
                                            character.Frame.N == LF2StandardFrames.Punch4);

            CheckMappedAction(
                "JumpAction/K",
                (input, pressed) => input.SetJumpActionPressed(pressed),
                expectedKeyJump: 0,
                expectedKeyDefend: 1,
                expectedKeyAttack: 0,
                expectedCooldown: runtime => runtime.CdJump,
                expectedFrame: character => character.Frame.N == LF2StandardFrames.Jumping);

            CheckMappedAction(
                "DefendAction/L",
                (input, pressed) => input.SetDefendActionPressed(pressed),
                expectedKeyJump: 0,
                expectedKeyDefend: 0,
                expectedKeyAttack: 1,
                expectedCooldown: runtime => runtime.CdDefend,
                expectedFrame: character => character.Frame.N == LF2StandardFrames.Defend);

            CheckBufferedActionCombination();
            CheckBufferedDirectionReversal();
            CheckFrameSnapshotHumanInputPolling();
        }

        private static void CheckRecordedInputAlignmentContracts()
        {
            LF2FrameData velocityFrame = Frame(20, LF2States.Attack, 1, 20, 39, 79);
            velocityFrame.dvy = 2;
            var data = new LF2CharacterData
            {
                name = "SelfCheck_InputAlignmentContracts",
                running_speedz = 3f,
                dash_distance = 10f,
                dash_height = -4f,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(10, LF2States.DeepSpecific, 1, 10, 39, 79),
                    Frame(11, LF2States.FirenSpecific, 1, 11, 39, 79),
                    velocityFrame,
                    Frame(LF2StandardFrames.SuperPunch, LF2States.Attack, 1, LF2StandardFrames.SuperPunch, 39, 79),
                    Frame(LF2StandardFrames.JumpAttack, LF2States.Attack, 1, LF2StandardFrames.JumpAttack, 39, 79),
                    Frame(LF2StandardFrames.DashAttack, LF2States.Attack, 1, LF2StandardFrames.DashAttack, 39, 79),
                    Frame(LF2StandardFrames.Defend, LF2States.Defending, 1, LF2StandardFrames.Defend, 39, 79),
                    Frame(LF2StandardFrames.JumpingAir, LF2States.Jump, 1, LF2StandardFrames.JumpingAir, 39, 79),
                    Frame(LF2StandardFrames.DashForward, LF2States.Dash, 1, LF2StandardFrames.DashForward, 39, 79),
                },
            };

            LF2Character character = CreateCharacter("SelfCheck_InputAlignment", 1, data);

            character.ImmediateFrame(LF2StandardFrames.Defend);
            character.Runtime.Vx = 7.0;
            character.Runtime.Vz = 5.0;
            character.ProcessReleaseInput();
            Expect(character.Frame.N == LF2StandardFrames.Defend &&
                   Nearly(character.Runtime.Vx, 7.0) && Nearly(character.Runtime.Vz, 5.0),
                "INPUT-1: state 7 must not be dispatched through release input");

            character.ImmediateFrame(LF2StandardFrames.JumpingAir);
            character.Runtime.Y = -0.5;
            character.Runtime.YInt = 0;
            character.Runtime.KeyJump = 1;
            character.AttackingCounter = 7;
            character.ProcessReleaseInput();
            Expect(character.Frame.N == LF2StandardFrames.JumpingAir && character.AttackingCounter == 7,
                "INPUT-2: jump input gate must use YInt and reject fractional Y when YInt is grounded");

            character.Runtime.YInt = -1;
            character.ProcessReleaseInput();
            Expect(character.Frame.N == LF2StandardFrames.JumpAttack && character.AttackingCounter == 0,
                "INPUT-2/7: airborne jump attack must run and retain its explicit attacking reset");

            character.ImmediateFrame(10);
            character.Runtime.Y = -0.5;
            character.Runtime.YInt = 0;
            character.Runtime.KeyJump = 0;
            character.Runtime.KeyUp = 1;
            character.Runtime.Vz = 0.0;
            character.ProcessReleaseInput();
            Expect(Nearly(character.Runtime.Vz, -3.0),
                "INPUT-3: state 301/19 lane input must use the grounded YInt snapshot");

            character.ImmediateFrame(11);
            character.Runtime.KeyUp = 0;
            character.Runtime.KeyDown = 1;
            character.Runtime.Vz = 0.0;
            character.ProcessReleaseInput();
            Expect(Nearly(character.Runtime.Vz, 3.0),
                "INPUT-3: state 19 lane input must share the grounded YInt gate");

            character.ImmediateFrame(0);
            character.Runtime.KeyUp = 0;
            character.Runtime.KeyDown = 0;
            character.Runtime.KeyJump = 1;
            character.Runtime.CdAttack = 5;
            character.HitConfirmEa = 3;
            character.ProcessReleaseInput();
            Expect(character.Frame.N == LF2StandardFrames.SuperPunch && character.HitConfirmEa == 3,
                "INPUT-6: Super Punch selection must not consume HitConfirm early");

            character.ImmediateFrame(LF2StandardFrames.DashForward);
            character.Runtime.KeyJump = 1;
            character.Runtime.CdAttack = 0;
            character.Runtime.Dir = "right";
            character.Runtime.Vx = 8.0;
            character.AttackingCounter = 9;
            character.ProcessReleaseInput();
            Expect(character.Frame.N == LF2StandardFrames.DashAttack && character.AttackingCounter == 9,
                "INPUT-7: raw input frame writes must preserve attacking when the source branch does not clear it");

            character.Frame.PN = 777;
            character.Trans.SyncDirectFrameData(character.Frame.D.wait, character.Frame.D.next, 13);
            character.SetInputFrameDirectInternal(LF2StandardFrames.DashAttack);
            Expect(character.Frame.PN == 777 && character.Trans.WaitCounter == 13,
                "INPUT-7: raw input frame writes must preserve PrevFrame/PN and the frame wait counter");

            LF2Character human = CreateCharacter("SelfCheck_InputVelocityHuman", 2, data);
            human.ImmediateFrame(20);
            human.Runtime.Vy = 0.0;
            human.RunPostCooldownInputPhase(1);
            Expect(Nearly(human.Runtime.Vy, 2.0),
                "INPUT-4: human post-cooldown input must apply the frame velocity tail exactly once");

            LF2Character ai = CreateCharacter("SelfCheck_InputVelocityAi", 3, data);
            ai.ImmediateFrame(20);
            ai.AiControlled = true;
            ai.Runtime.Vy = 0.0;
            ai.RunPostCooldownInputPhase(2);
            Expect(Nearly(ai.Runtime.Vy, 2.0),
                "INPUT-4: AI post-cooldown input must apply the frame velocity tail exactly once");

            LF2Character lockOwner = CreateCharacter("SelfCheck_RuntimeDefendLock", 4, data);
            lockOwner.Runtime.CdDefendLock = 7;
            lockOwner.InputState.SyncFromRuntime(lockOwner.Runtime);
            lockOwner.Runtime.CdDefendLock = 3;
            var world = new SimulationWorld();
            world.Register(lockOwner);
            world.VrestTickAll(1);
            lockOwner.RunPostCooldownInputPhase(1);
            Expect(lockOwner.Runtime.CdDefendLock == 2,
                "INPUT-5: Runtime must be the sole defend-lock truth and decrement it once per tick");

            CheckSharedCharacterDatInputAlignmentContracts();
            CheckLocomotionSingleAdvanceAndMoveRawWriteContracts();
        }

        private static void CheckLocomotionSingleAdvanceAndMoveRawWriteContracts()
        {
            var data = new LF2CharacterData
            {
                name = "SelfCheck_LocomotionSingleAdvance",
                walking_speed = 4f,
                walking_speedz = 2f,
                walking_frame_rate = 1,
                running_speed = 8f,
                running_speedz = 3f,
                running_frame_rate = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(LF2StandardFrames.WalkingStart, LF2States.Walking, 0, LF2StandardFrames.WalkingStart, 39, 79),
                    Frame(LF2StandardFrames.Walking1, LF2States.Walking, 0, LF2StandardFrames.Walking1, 39, 79),
                    Frame(LF2StandardFrames.Walking2, LF2States.Walking, 0, LF2StandardFrames.Walking2, 39, 79),
                    Frame(LF2StandardFrames.WalkingEnd, LF2States.Walking, 0, LF2StandardFrames.WalkingEnd, 39, 79),
                    Frame(LF2StandardFrames.RunningStart, LF2States.Running, 0, LF2StandardFrames.RunningStart, 39, 79),
                    Frame(LF2StandardFrames.Running1, LF2States.Running, 0, LF2StandardFrames.Running1, 39, 79),
                    Frame(LF2StandardFrames.RunningEnd, LF2States.Running, 0, LF2StandardFrames.RunningEnd, 39, 79),
                },
            };

            LF2Character moveRaw = CreateCharacter("SelfCheck_MoveRawWrite", 10, data);
            moveRaw.Frame.PN = 777;
            moveRaw.AttackingCounter = 9;
            moveRaw.Trans.SyncDirectFrameData(moveRaw.Frame.D.wait, moveRaw.Frame.D.next, 13);
            moveRaw.SetMoveFrameDirectInternal(LF2StandardFrames.WalkingStart);
            Expect(moveRaw.Frame.N == LF2StandardFrames.WalkingStart &&
                   moveRaw.Frame.PN == 777 &&
                   moveRaw.Trans.WaitCounter == 13 &&
                   moveRaw.AttackingCounter == 9,
                "RISK-2: move raw writes must preserve PrevFrame/PN, WaitCounter, and AttackingCounter");

            LF2Character walking = CreateCharacter("SelfCheck_WalkingSingleAdvance", 11, data);
            walking.Controller = new CharacterInputModule();
            walking.Runtime.Vx = 7.0;
            walking.Controller.InputBuffer.EnqueueForTick(1, FuncKeyMask.up, true);
            walking.RunPostCooldownInputPhase(1);
            Expect(walking.Runtime.AnimCounter == 1 &&
                   walking.Frame.N == LF2StandardFrames.Walking1 &&
                   Nearly(walking.Runtime.Vx, 5.0) &&
                   Nearly(walking.Runtime.Vz, -2.0),
                "RISK-1: walking input pass must advance locomotion exactly once before late frame rollover");
            walking.SimFrameTick(1);
            Expect(walking.Runtime.AnimCounter == 1 &&
                   walking.Frame.N == LF2StandardFrames.Walking1 &&
                   Nearly(walking.Runtime.Vx, 5.0) &&
                   Nearly(walking.Runtime.Vz, -2.0),
                "RISK-1: walking wait rollover must not run the locomotion resolver a second time in FrameEvent");

            LF2Character running = CreateCharacter("SelfCheck_RunningSingleAdvance", 12, data);
            running.Controller = new CharacterInputModule();
            running.ImmediateFrame(LF2StandardFrames.RunningStart);
            running.Controller.InputBuffer.EnqueueForTick(2, FuncKeyMask.up, true);
            running.RunPostCooldownInputPhase(2);
            double expectedRunVx = 8.0 * (5.0 / 6.0);
            Expect(running.Runtime.AnimCounter == 1 &&
                   running.Frame.N == LF2StandardFrames.Running1 &&
                   Nearly(running.Runtime.Vx, expectedRunVx) &&
                   Nearly(running.Runtime.Vz, -3.0),
                "RISK-1: running input pass must advance locomotion exactly once before late frame rollover");
            running.SimFrameTick(2);
            Expect(running.Runtime.AnimCounter == 1 &&
                   running.Frame.N == LF2StandardFrames.Running1 &&
                   Nearly(running.Runtime.Vx, expectedRunVx) &&
                   Nearly(running.Runtime.Vz, -3.0),
                "RISK-1: running wait rollover must not run the locomotion resolver a second time in FrameEvent");

            LF2Character stateEntry = CreateCharacter("SelfCheck_WalkingStateEntry", 13, data);
            stateEntry.Runtime.AnimCounter = 7;
            stateEntry.Runtime.AnimSub = 4;
            stateEntry.OnFrameTransit(LF2StandardFrames.WalkingStart, false);
            Expect(stateEntry.Runtime.AnimCounter == 0 && stateEntry.Runtime.AnimSub == 0,
                "RISK-1: removing duplicate frame locomotion must retain walking state-entry side effects");
        }

        private static void CheckSharedCharacterDatInputAlignmentContracts()
        {
            LF2FrameData dashFrame = Frame(
                LF2StandardFrames.DashForward,
                LF2States.Dash,
                1,
                LF2StandardFrames.DashForward,
                39,
                79);
            dashFrame.dvy = 2;
            var data = new LF2CharacterData
            {
                name = "SelfCheck_SharedInputAlignment",
                running_speed = 8f,
                running_speedz = 3f,
                running_frame_rate = 1,
                dash_distance = 10f,
                dash_distancez = 4f,
                dash_height = -4f,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(LF2StandardFrames.RunningStart, LF2States.Running, 1, LF2StandardFrames.RunningStart, 39, 79),
                    Frame(LF2StandardFrames.Running1, LF2States.Running, 1, LF2StandardFrames.Running1, 39, 79),
                    Frame(LF2StandardFrames.RunningEnd, LF2States.Running, 1, LF2StandardFrames.RunningEnd, 39, 79),
                    Frame(LF2StandardFrames.SuperPunch, LF2States.Attack, 1, LF2StandardFrames.SuperPunch, 39, 79),
                    Frame(LF2StandardFrames.RunAttack, LF2States.Attack, 1, LF2StandardFrames.RunAttack, 39, 79),
                    Frame(LF2StandardFrames.Rowing2, LF2States.Rowing, 1, LF2StandardFrames.Rowing2, 39, 79),
                    Frame(LF2StandardFrames.JumpAttack, LF2States.Attack, 1, LF2StandardFrames.JumpAttack, 39, 79),
                    Frame(LF2StandardFrames.JumpingAir, LF2States.Jump, 1, LF2StandardFrames.JumpingAir, 39, 79),
                    dashFrame,
                    Frame(LF2StandardFrames.Crouch, LF2States.StopRunning, 1, LF2StandardFrames.Crouch, 39, 79),
                    Frame(LF2StandardFrames.StopRunning, LF2States.StopRunning, 1, LF2StandardFrames.StopRunning, 39, 79),
                },
            };

            SelfCheckCharacterDatShell running = CreateSharedInputShell(data, LF2StandardFrames.RunningStart);
            running.Frame.PN = 444;
            running.Runtime.KeyJump = 1;
            running.Runtime.KeyAttack = 1;
            running.Runtime.KeyDefend = 1;
            running.Runtime.CdAttack = 5;
            running.Runtime.CdDefend = 5;
            running.Runtime.CdJump = 5;
            running.RunPostCooldownInputPhase(1);
            Expect(running.Frame.N == LF2StandardFrames.DashForward,
                "INPUT-8: shared running must apply attack, then defend, then jump so jump wins simultaneous input");
            Expect(running.Frame.PN == 444,
                "INPUT-7: shared raw input frame writes must preserve PrevFrame/PN");
            Expect(Nearly(running.Runtime.Vy, -2.0),
                "INPUT-4: shared post-cooldown input must apply the final frame velocity tail exactly once");

            SelfCheckCharacterDatShell runningDefend = CreateSharedInputShell(data, LF2StandardFrames.RunningStart);
            runningDefend.Runtime.KeyAttack = 1;
            runningDefend.Runtime.CdDefend = 5;
            runningDefend.RunPostCooldownInputPhase(2);
            Expect(runningDefend.Frame.N == LF2StandardFrames.Rowing2,
                "INPUT-8: shared running must retain the authority running-defend branch");

            SelfCheckCharacterDatShell stopRunning = CreateSharedInputShell(data, LF2StandardFrames.RunningStart);
            stopRunning.Runtime.KeyLeft = 1;
            stopRunning.RunPostCooldownInputPhase(3);
            Expect(stopRunning.Frame.N == LF2StandardFrames.StopRunning &&
                   Nearly(stopRunning.Runtime.Vx, 8.0),
                "INPUT-8: reverse running input must enter stop-running while preserving old-direction velocity");

            SelfCheckCharacterDatShell groundedJump = CreateSharedInputShell(data, LF2StandardFrames.JumpingAir);
            groundedJump.Runtime.Y = -0.5;
            groundedJump.Runtime.YInt = 0;
            groundedJump.Runtime.KeyJump = 1;
            groundedJump.RunPostCooldownInputPhase(4);
            Expect(groundedJump.Frame.N == LF2StandardFrames.JumpingAir,
                "INPUT-2: shared jump attack must gate on YInt and reject a grounded integer snapshot");

            groundedJump.Runtime.YInt = -1;
            groundedJump.RunPostCooldownInputPhase(5);
            Expect(groundedJump.Frame.N == LF2StandardFrames.JumpAttack,
                "INPUT-2: shared jump attack must accept an airborne integer snapshot");

            SelfCheckCharacterDatShell superPunch = CreateSharedInputShell(data, 0);
            superPunch.Runtime.KeyJump = 1;
            superPunch.Runtime.CdAttack = 5;
            superPunch.HitConfirmCounter = 3;
            superPunch.RunPostCooldownInputPhase(6);
            Expect(superPunch.Frame.N == LF2StandardFrames.SuperPunch && superPunch.HitConfirmCounter == 3,
                "INPUT-6: shared Super Punch selection must not consume HitConfirm early");

            SelfCheckCharacterDatShell crouchAttack = CreateSharedInputShell(data, LF2StandardFrames.Crouch);
            crouchAttack.Runtime.KeyJump = 1;
            crouchAttack.Runtime.CdAttack = 5;
            crouchAttack.RunPostCooldownInputPhase(7);
            Expect(crouchAttack.Frame.N == LF2StandardFrames.Crouch,
                "INPUT-9: frame 215 must not accept the extra KeyJump/CdAttack action branch");

            SelfCheckCharacterDatShell crouchDefend = CreateSharedInputShell(data, LF2StandardFrames.Crouch);
            crouchDefend.Runtime.KeyAttack = 1;
            crouchDefend.Runtime.CdDefend = 5;
            crouchDefend.RunPostCooldownInputPhase(8);
            Expect(crouchDefend.Frame.N == LF2StandardFrames.Rowing2,
                "INPUT-9: frame 215 must retain the authority KeyAttack/CdDefend branch");
        }

        private static SelfCheckCharacterDatShell CreateSharedInputShell(
            LF2CharacterData data,
            int frameId)
        {
            var shell = new SelfCheckCharacterDatShell();
            shell.InitializeForCpoint();
            shell.Name = $"SelfCheck_SharedInput_{frameId}";
            shell.ObjectId = 2;
            shell.FrameCache.Load(new LF2CharacterDataWrapper(2, data));
            shell.Frame.N = frameId;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(frameId);
            shell.Frame.PN = frameId;
            shell.Runtime.Dir = "right";
            shell.Runtime.Y = 0.0;
            shell.Runtime.YInt = 0;
            shell.Runtime.PP = 500;
            shell.Runtime.HP = 500;
            shell.Runtime.HPBound = 500;
            shell.Trans.SyncDirectFrameData(shell.Frame.D.wait, shell.Frame.D.next, 0);
            shell.RefreshRuntimeSnapshot();
            return shell;
        }

        private static void CheckMappedAction(
            string name,
            Action<CharacterInputModule, bool> setPressed,
            byte expectedKeyJump,
            byte expectedKeyDefend,
            byte expectedKeyAttack,
            Func<NTSDEntityRuntime, byte> expectedCooldown,
            Func<LF2Character, bool> expectedFrame)
        {
            LF2Character character = CreateBufferedInputCharacter($"SelfCheck_{name}");
            var input = (CharacterInputModule)character.Controller;

            setPressed(input, true);
            character.RunPostCooldownInputPhase(1);

            Expect(character.Runtime.KeyJump == expectedKeyJump &&
                   character.Runtime.KeyDefend == expectedKeyDefend &&
                   character.Runtime.KeyAttack == expectedKeyAttack,
                $"{name} must map to the NTSD internal field contract");
            Expect(expectedCooldown(character.Runtime) == 5,
                $"{name} must set only its matching NTSD semantic cooldown edge");
            Expect(expectedFrame(character),
                $"{name} must trigger its logical action in the target simulation tick");

            setPressed(input, false);
            character.RunPostCooldownInputPhase(2);
            Expect(character.Runtime.KeyJump == 0 &&
                   character.Runtime.KeyDefend == 0 &&
                   character.Runtime.KeyAttack == 0,
                $"{name} release must clear its internal field without a stuck input");
        }

        private static void CheckBufferedActionCombination()
        {
            LF2Character character = CreateBufferedInputCharacter("SelfCheck_ActionCombination");
            var input = (CharacterInputModule)character.Controller;

            input.SetAttackActionPressed(true);
            input.SetJumpActionPressed(true);
            character.RunPostCooldownInputPhase(1);
            Expect(character.Runtime.KeyJump == 1 && character.Runtime.KeyDefend == 1 && character.Runtime.KeyAttack == 0,
                "J+K must preserve both internal held fields without writing the defend field");
            Expect(character.Runtime.CdAttack == 5 && character.Runtime.CdJump == 5 && character.Runtime.CdDefend == 0,
                "J+K must update attack/jump cooldowns without cross-writing defend cooldown");

            input.SetAttackActionPressed(false);
            character.RunPostCooldownInputPhase(2);
            Expect(character.Runtime.KeyJump == 0 && character.Runtime.KeyDefend == 1 && character.Runtime.KeyAttack == 0,
                "releasing J while K stays held must not clear or duplicate the K field");

            input.SetJumpActionPressed(false);
            character.RunPostCooldownInputPhase(3);
            Expect(character.Runtime.KeyJump == 0 && character.Runtime.KeyDefend == 0 && character.Runtime.KeyAttack == 0,
                "releasing the final combination key must leave no held action field");
        }

        private static void CheckBufferedDirectionReversal()
        {
            GameObject presentationFixture = new GameObject("SelfCheck_DirectionPresentation");
            try
            {
                SpriteRenderer renderer = presentationFixture.AddComponent<SpriteRenderer>();
                LF2Character walking = CreateBufferedInputCharacter("SelfCheck_WalkingReversal");
                walking.Sprite.Initialize(renderer, new List<Sprite>());
                SimInputBuffer walkingBuffer = walking.Controller.InputBuffer;
                walkingBuffer.EnqueueForTick(1, FuncKeyMask.right, true);
                walking.RunPostCooldownInputPhase(1);
                walkingBuffer.EnqueueForTick(2, FuncKeyMask.right, false);
                walkingBuffer.EnqueueForTick(2, FuncKeyMask.left, true);
                walking.RunPostCooldownInputPhase(2);

                Expect(walking.Runtime.KeyRight == 0 && walking.Runtime.KeyLeft == 1,
                    "D release + A press must clear right and set left in the same target tick");
                Expect(walking.Runtime.Dir == "left" && walking.PS.dir == "left" && walking.Runtime.Vx < 0.0,
                    "standing/walking reversal must face left and write negative velocity immediately");
                Expect(walking.Sprite.Dir == "left" && renderer.flipX,
                    "standing/walking reversal must flip the visible sprite in the same tick");

                walking.PS.dir = "right";
                walking.Sprite.SwitchLR("right");
                renderer.flipX = false;
                walking.SwitchDir("left");
                Expect(walking.Runtime.Dir == "left" && walking.PS.dir == "left" &&
                       walking.Sprite.Dir == "left" && renderer.flipX,
                    "SwitchDir must repair PS and renderer drift even when Runtime already has the requested direction");

                LF2Character running = CreateBufferedInputCharacter("SelfCheck_RunningReversal");
                running.ImmediateFrame(LF2StandardFrames.RunningStart);
                running.SwitchDir("right");
                SimInputBuffer runningBuffer = running.Controller.InputBuffer;
                runningBuffer.EnqueueForTick(1, FuncKeyMask.right, true);
                running.RunPostCooldownInputPhase(1);
                runningBuffer.EnqueueForTick(2, FuncKeyMask.right, false);
                runningBuffer.EnqueueForTick(2, FuncKeyMask.left, true);
                running.RunPostCooldownInputPhase(2);

                Expect(running.Runtime.KeyRight == 0 && running.Runtime.KeyLeft == 1,
                    "running reversal must consume the same-tick D release + A press snapshot");
                Expect(running.Frame.N == LF2StandardFrames.StopRunning && running.Runtime.Vx > 0.0 &&
                       running.Runtime.Dir == "right" && running.PS.dir == "right",
                    "running reversal must enter frame 218 while preserving the old-direction velocity for that tick");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presentationFixture);
            }
        }

        private static void CheckFrameSnapshotHumanInputPolling()
        {
            CheckHeldSnapshotEdgesAndHistory();

            LF2Character right = CreateFrameSnapshotComboCharacter("SelfCheck_HeldRightDrj", 5);
            HoldDirectionForTicks(right, FuncKeyMask.right, 1, 6);
            EnqueueInputTick(right, 7, (FuncKeyMask.att, true));
            right.RunPostCooldownInputPhase(7);
            Expect(right.Frame.N == 102 && right.Runtime.ComboDrj == 1,
                $"partial held-right+defend must preserve advanced DRJ progress through direct hit_d=102; " +
                $"frame={right.Frame.N},combo={right.Runtime.ComboDrj},cdD={right.Runtime.CdDefend}," +
                $"cdR={right.Runtime.CdRight},cdJ={right.Runtime.CdJump}");
            EnqueueInputTick(right, 8, (FuncKeyMask.def, true));
            right.RunPostCooldownInputPhase(8);
            Expect(right.Frame.N != 240,
                $"jump after the partial held-right direct tail must not incorrectly enter frame 240; " +
                $"frame={right.Frame.N},combo={right.Runtime.ComboDrj},cdD={right.Runtime.CdDefend}," +
                $"cdR={right.Runtime.CdRight},cdJ={right.Runtime.CdJump}");

            LF2Character left = CreateFrameSnapshotComboCharacter("SelfCheck_HeldLeftDrj", 8);
            HoldDirectionForTicks(left, FuncKeyMask.left, 1, 6);
            EnqueueInputTick(left, 7, (FuncKeyMask.att, true));
            left.RunPostCooldownInputPhase(7);
            Expect(left.Frame.N == 102 && left.Runtime.ComboDlj == 1,
                $"partial held-left+defend must preserve advanced DLJ progress through direct hit_d=102; " +
                $"frame={left.Frame.N},combo={left.Runtime.ComboDlj},cdD={left.Runtime.CdDefend}," +
                $"cdL={left.Runtime.CdLeft},cdJ={left.Runtime.CdJump}");
            EnqueueInputTick(left, 8, (FuncKeyMask.def, true));
            left.RunPostCooldownInputPhase(8);
            Expect(left.Frame.N != 240,
                $"jump after the partial held-left direct tail must not incorrectly enter frame 240; " +
                $"frame={left.Frame.N},combo={left.Runtime.ComboDlj},cdD={left.Runtime.CdDefend}," +
                $"cdL={left.Runtime.CdLeft},cdJ={left.Runtime.CdJump}");

            LF2Character sameTick = CreateFrameSnapshotComboCharacter("SelfCheck_RightSameTickDrj", 5);
            EnqueueInputTick(sameTick, 1,
                (FuncKeyMask.right, true), (FuncKeyMask.att, true), (FuncKeyMask.def, true));
            sameTick.RunPostCooldownInputPhase(1);
            Expect(sameTick.Frame.N == 240 && sameTick.Runtime.Dir == "right",
                "fresh right plus defend+jump in one tick must complete DRJ from frame 5");

            LF2Character sameTickLeft = CreateFrameSnapshotComboCharacter("SelfCheck_LeftSameTickDrj", 8);
            EnqueueInputTick(sameTickLeft, 1,
                (FuncKeyMask.left, true), (FuncKeyMask.att, true), (FuncKeyMask.def, true));
            sameTickLeft.RunPostCooldownInputPhase(1);
            Expect(sameTickLeft.Frame.N == 240 && sameTickLeft.Runtime.Dir == "left",
                "fresh left plus defend+jump in one tick must complete DLJ from frame 8");

            LF2Character releasedDefend = CreateFrameSnapshotComboCharacter("SelfCheck_ReleasedDefendDrj", 5);
            HoldDirectionForTicks(releasedDefend, FuncKeyMask.right, 1, 6);
            EnqueueInputTick(releasedDefend, 7, (FuncKeyMask.att, true));
            releasedDefend.RunPostCooldownInputPhase(7);
            EnqueueInputTick(releasedDefend, 8, (FuncKeyMask.att, false), (FuncKeyMask.def, true));
            releasedDefend.RunPostCooldownInputPhase(8);
            Expect(releasedDefend.Frame.N != 240,
                "releasing defend before the jump tick must not fabricate a staggered DRJ trigger");

            LF2Character both = CreateFrameSnapshotComboCharacter("SelfCheck_BothDirectionsDrj", 5);
            EnqueueInputTick(both, 1,
                (FuncKeyMask.right, true), (FuncKeyMask.left, true),
                (FuncKeyMask.att, true), (FuncKeyMask.def, true));
            both.RunPostCooldownInputPhase(1);
            Expect(both.Frame.N == 240 && both.Runtime.Dir == "right",
                "when both horizontal directions are held, DRJ must retain the authority right-wrapper priority");

            LF2Character dja = CreateFrameSnapshotComboCharacter("SelfCheck_NoDirectionDja", 5);
            int hpBefore = dja.Health.HP;
            EnqueueInputTick(
                dja,
                1,
                (FuncKeyMask.att, true),
                (FuncKeyMask.def, true),
                (FuncKeyMask.jump, true));
            dja.RunPostCooldownInputPhase(1);
            Expect(dja.Frame.N == 290 && dja.Health.HP == hpBefore &&
                   dja.Runtime.KeyRight == 0 && dja.Runtime.KeyLeft == 0 &&
                   dja.Runtime.KeyUp == 0 && dja.Runtime.KeyDown == 0,
                $"same-tick directionless DJA must enter hit_ja=290 without fabricating movement or damage; " +
                $"frame={dja.Frame.N},combo={dja.Runtime.ComboDja}");
        }

        private static void CheckHeldSnapshotEdgesAndHistory()
        {
            LF2Character character = CreateFrameSnapshotComboCharacter("SelfCheck_HeldSnapshot", 240);
            EnqueueInputTick(
                character,
                1,
                (FuncKeyMask.right, true),
                (FuncKeyMask.left, true),
                (FuncKeyMask.up, true),
                (FuncKeyMask.down, true),
                (FuncKeyMask.att, true),
                (FuncKeyMask.def, true),
                (FuncKeyMask.jump, true));

            character.RunPostCooldownInputPhase(1);
            NTSDEntityRuntime runtime = character.Runtime;
            Expect(runtime.KeyRight == 1 && runtime.KeyLeft == 1 &&
                   runtime.KeyUp == 1 && runtime.KeyDown == 1 &&
                   runtime.KeyAttack == 1 && runtime.KeyDefend == 1 && runtime.KeyJump == 1,
                "input polling must retain all final held keys");
            Expect(runtime.PrevRight == 0 && runtime.PrevLeft == 0 &&
                   runtime.PrevUp == 0 && runtime.PrevDown == 0 &&
                   runtime.PrevAttack == 0 && runtime.PrevDefend == 0 && runtime.PrevJump == 0,
                "first held snapshot must compare against the prior unheld state");
            Expect(runtime.CdRight == 5 && runtime.CdLeft == 5 &&
                   runtime.CdUp == 5 && runtime.CdDown == 5 &&
                   runtime.CdDefend == 5 && runtime.CdJump == 5 && runtime.CdAttack == 5,
                "new press edges must write cooldown 5");
            Expect(runtime.InputHistory[1] == 8 && runtime.InputHistory[2] == 2 &&
                   runtime.InputHistory[3] == 9 && runtime.InputHistory[4] == 0 &&
                   runtime.InputHistory[5] == 5,
                "new press history must use right,left,up,down,attack,defend,jump order");

            int[] historyAfterFirstPoll = (int[])runtime.InputHistory.Clone();
            character.RunPostCooldownInputPhase(2);
            Expect(runtime.PrevRight == 1 && runtime.PrevLeft == 1 &&
                   runtime.PrevUp == 1 && runtime.PrevDown == 1 &&
                   runtime.PrevAttack == 1 && runtime.PrevDefend == 1 && runtime.PrevJump == 1,
                "continuous held polling must carry the previous tick held snapshot");
            Expect(runtime.CdRight == 4 && runtime.CdLeft == 4 &&
                   runtime.CdUp == 4 && runtime.CdDown == 4 &&
                   runtime.CdDefend == 4 && runtime.CdJump == 4 && runtime.CdAttack == 4,
                "continuous holds must decrement cooldowns without fabricating repeat edges");
            Expect(System.Linq.Enumerable.SequenceEqual(runtime.InputHistory, historyAfterFirstPoll),
                "continuous holds must not push duplicate input-history entries");
        }

        private static LF2Character CreateFrameSnapshotComboCharacter(string name, int startFrame)
        {
            LF2FrameData frame5 = Frame(5, 999, 1, 5, 39, 79);
            frame5.hit_Fj = 240;
            frame5.hit_ja = 290;
            frame5.hit_d = 102;
            LF2FrameData frame8 = Frame(8, 999, 1, 8, 39, 79);
            frame8.hit_Fj = 240;
            frame8.hit_ja = 290;
            frame8.hit_d = 102;
            var data = new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 999, 1, 0, 39, 79),
                    frame5,
                    frame8,
                    Frame(102, 999, 1, 102, 39, 79),
                    Frame(240, 999, 1, 240, 39, 79),
                    Frame(290, 999, 1, 290, 39, 79),
                },
            };

            LF2Character character = CreateCharacter(name, 2, data);
            character.ImmediateFrame(startFrame);
            return character;
        }

        private static void HoldDirectionForTicks(
            LF2Character character,
            FuncKeyMask direction,
            int startTick,
            int count)
        {
            EnqueueInputTick(character, startTick, (direction, true));
            for (int tick = startTick; tick < startTick + count; tick++)
                character.RunPostCooldownInputPhase(tick);
        }

        private static void EnqueueInputTick(
            LF2Character character,
            int tickIndex,
            params (FuncKeyMask key, bool down)[] events)
        {
            SimInputBuffer inputBuffer = character.Controller.InputBuffer;
            for (int i = 0; i < events.Length; i++)
                inputBuffer.EnqueueForTick(tickIndex, events[i].key, events[i].down);
        }

        private static LF2Character CreateBufferedInputCharacter(string name)
        {
            var data = new LF2CharacterData
            {
                name = name,
                walking_speed = 4f,
                walking_speedz = 2f,
                walking_frame_rate = 1,
                running_speed = 8f,
                running_speedz = 3f,
                running_frame_rate = 1,
                dash_distance = 10f,
                dash_height = -4f,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(5, LF2States.Walking, 1, 5, 39, 79),
                    Frame(9, LF2States.Running, 1, 9, 39, 79),
                    Frame(10, LF2States.Running, 1, 10, 39, 79),
                    Frame(11, LF2States.Running, 1, 11, 39, 79),
                    Frame(LF2StandardFrames.Punch, LF2States.Attack, 1, LF2StandardFrames.Punch, 39, 79),
                    Frame(LF2StandardFrames.Punch4, LF2States.Attack, 1, LF2StandardFrames.Punch4, 39, 79),
                    Frame(LF2StandardFrames.Defend, LF2States.Defending, 1, LF2StandardFrames.Defend, 39, 79),
                    Frame(LF2StandardFrames.Jumping, LF2States.Jump, 1, LF2StandardFrames.Jumping, 39, 79),
                    Frame(LF2StandardFrames.StopRunning, LF2States.StopRunning, 1, LF2StandardFrames.StopRunning, 39, 79),
                },
            };

            LF2Character character = CreateCharacter(name, 1, data);
            character.Controller = new CharacterInputModule();
            character.Runtime.Y = 0.0;
            character.Runtime.YInt = 0;
            return character;
        }

        private static void CheckSharedCharacterDatGroundMovementUsesIntegerSnapshot()
        {
            var data = new LF2CharacterData
            {
                name = "SelfCheck_SharedIntegerGroundMovement",
                walking_speed = 4f,
                walking_speedz = 2f,
                walking_frame_rate = 1,
                heavy_walking_speed = 3f,
                heavy_walking_speedz = 1f,
                running_speed = 6f,
                running_speedz = 3f,
                heavy_running_speed = 7f,
                heavy_running_speedz = 3f,
                running_frame_rate = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(5, LF2States.Walking, 1, 5, 39, 79),
                    Frame(9, LF2States.Running, 1, 9, 39, 79),
                    Frame(10, LF2States.Running, 1, 10, 39, 79),
                    Frame(16, LF2States.Running, 1, 16, 39, 79),
                    Frame(17, LF2States.Running, 1, 17, 39, 79),
                },
            };

            var shell = new SelfCheckCharacterDatShell();
            shell.InitializeForCpoint();
            shell.Name = "SelfCheck_SharedIntegerGroundMovement";
            shell.ObjectId = 2;
            shell.FrameCache.Load(new LF2CharacterDataWrapper(2, data));
            shell.Frame.N = 0;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Runtime.Y = -0.5;
            shell.Runtime.YInt = 0;
            shell.Runtime.Dir = "right";
            shell.Runtime.KeyRight = 1;
            shell.Runtime.Vx = 0.0;

            shell.RunPostCooldownInputPhase(1);
            Expect(System.Math.Abs(shell.Runtime.Vx - 4.0) <= 1e-12,
                $"shared character-DAT walking ground gate must use YInt==0; actual Vx={shell.Runtime.Vx:R}");

            shell.Runtime.LinkState = 2;
            shell.Frame.N = 0;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Runtime.KeyUp = 0;
            shell.Runtime.Vx = 0.0;
            shell.RunPostCooldownInputPhase(2);
            Expect(System.Math.Abs(shell.Runtime.Vx - 3.0) <= 1e-12,
                $"shared character-DAT heavy walking ground gate must use YInt==0; actual Vx={shell.Runtime.Vx:R}");

            shell.Runtime.LinkState = 0;
            shell.Frame.N = 9;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(9);
            shell.Runtime.KeyRight = 0;
            shell.Runtime.KeyUp = 1;
            shell.Runtime.Vx = 0.0;
            shell.RunPostCooldownInputPhase(3);
            double expectedRunVx = 6.0 * (5.0 / 6.0);
            Expect(System.Math.Abs(shell.Runtime.Vx - expectedRunVx) <= 1e-12,
                $"shared character-DAT running lane must apply exact double 5/6; actual={shell.Runtime.Vx:R}, expected={expectedRunVx:R}");

            shell.Runtime.LinkState = 2;
            shell.Frame.N = 16;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(16);
            shell.Runtime.Vx = 0.0;
            shell.RunPostCooldownInputPhase(4);
            double expectedHeavyRunVx = 7.0 * (5.0 / 6.0);
            Expect(System.Math.Abs(shell.Runtime.Vx - expectedHeavyRunVx) <= 1e-12,
                $"shared character-DAT heavy running lane must preserve exact double 5/6; actual={shell.Runtime.Vx:R}, expected={expectedHeavyRunVx:R}");
        }

        private static void CheckSpriteDimensionsUseFullRect()
        {
            GameObject fixtureObject = null;
            Texture2D texture = null;
            Sprite tightSprite = null;

            try
            {
                fixtureObject = new GameObject("SelfCheck_TightSpriteDimensions");
                SpriteRenderer renderer = fixtureObject.AddComponent<SpriteRenderer>();

                texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                var pixels = new Color[64];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.clear;
                for (int y = 2; y < 6; y++)
                {
                    for (int x = 2; x < 6; x++)
                        pixels[y * 8 + x] = Color.white;
                }
                texture.SetPixels(pixels);
                texture.Apply();

                tightSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0f),
                    100f,
                    0,
                    SpriteMeshType.Tight);
                renderer.sprite = tightSprite;

                var sprite = new LF2Sprite();
                sprite.Initialize(renderer, new List<Sprite> { tightSprite });

                Expect(Mathf.Approximately(sprite.GetWidthPx(), tightSprite.rect.width),
                    $"sprite width must use full rect; actual={sprite.GetWidthPx()}, rect={tightSprite.rect.width}");
                Expect(Mathf.Approximately(sprite.GetHeightPx(), tightSprite.rect.height),
                    $"sprite height must use full rect; actual={sprite.GetHeightPx()}, rect={tightSprite.rect.height}");
            }
            finally
            {
                if (fixtureObject != null)
                    DestroySelfCheckObject(fixtureObject);
                if (tightSprite != null)
                    DestroySelfCheckAsset(tightSprite);
                if (texture != null)
                    DestroySelfCheckAsset(texture);
            }
        }

        private static void CheckEntityAndShadowRenderPositionFormula()
        {
            Vector2 right = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500,
                -12,
                240.9f,
                7.9f,
                0,
                0,
                8,
                false,
                48f,
                64f,
                13f,
                51f,
                1.5f);
            Expect(Nearly(right.x, 523.5f) && Nearly(right.y, 247.5f),
                $"Unity fixed-world draw must truncate positive render/display offsets; actual={right}");
            ExpectRenderAnchor(right, false, 507f, 228f, 48f, 64f, 13f, 51f, 1.5f,
                "right-facing scaled DAT anchor");

            Vector2 leftNegativeOffset = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500,
                -12,
                240.9f,
                -7.9f,
                0,
                0,
                8,
                true,
                48f,
                64f,
                13f,
                51f,
                1.5f);
            Expect(Nearly(leftNegativeOffset.x, 476.5f) && Nearly(leftNegativeOffset.y, 247.5f),
                $"draw_entity must truncate negative render offsets toward zero and preserve left-facing centerx; actual={leftNegativeOffset}");
            ExpectRenderAnchor(leftNegativeOffset, true, 493f, 228f, 48f, 64f, 13f, 51f, 1.5f,
                "left-facing scaled DAT anchor");

            Vector2 quarterPixelPivot = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500,
                -12,
                240.9f,
                7.9f,
                0,
                0,
                8,
                false,
                47f,
                49f,
                22f,
                31f,
                1.5f);
            Expect(Nearly(quarterPixelPivot.x, 509.25f),
                $"scaled odd-width DAT anchor must preserve quarter-pixel pivot placement; actual={quarterPixelPivot.x}");
            ExpectRenderAnchor(quarterPixelPivot, false, 507f, 228f, 47f, 49f, 22f, 31f, 1.5f,
                "quarter-pixel scaled DAT anchor");

            Vector2 evenHitStop = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500, -12, 240.9f, 7.9f, 0, -1, 8, false, 48f, 64f, 13f, 51f, 1.5f);
            Vector2 oddHitStop = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500, -12, 240.9f, 7.9f, 0, -1, 9, false, 48f, 64f, 13f, 51f, 1.5f);
            Expect(Nearly(evenHitStop.x, 520.5f) && Nearly(oddHitStop.x, 526.5f),
                $"negative FrameDelay must alternate draw_entity X by -3/+3; even={evenHitStop.x}, odd={oddHitStop.x}");

            Vector2 type3DisplayZ = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500,
                -12,
                205.75f,
                7.9f,
                0,
                0,
                8,
                false,
                48f,
                64f,
                13f,
                51f,
                1.5f);
            Expect(Nearly(type3DisplayZ.x, 523.5f) && Nearly(type3DisplayZ.y, 212.5f),
                $"type3 logical display Z must be truncated before draw_entity positioning; actual={type3DisplayZ}");

            int shadowCenterX = 500 + (int)7.9f;
            Expect(shadowCenterX == 507,
                $"Unity fixed-world shadow must share draw_entity render-offset truncation; actual={shadowCenterX}");

            CheckScaledWeaponAnchorSample("oid120", 48f, 48f, 24f, 30f, 9f);
            CheckScaledWeaponAnchorSample("oid124", 48f, 48f, 24f, 35f, 6.5f);
        }

        private static void CheckUnityBattleCameraRemainsDisabled()
        {
            var world = new SimulationWorld();
            LF2Character movingCharacter = CreateCharacter(
                "SelfCheck_DisabledCameraMover", 1, BuildCatchingFrames());
            LF2Character stationaryCharacter = CreateCharacter(
                "SelfCheck_DisabledCameraTarget", 2, BuildVictimFrames());
            movingCharacter.SetRuntimeSlotIndex(0);
            stationaryCharacter.SetRuntimeSlotIndex(1);
            movingCharacter.Runtime.SetPosition(100f, 0f, 180f);
            stationaryCharacter.Runtime.SetPosition(500f, 0f, 240f);
            movingCharacter.Runtime.SyncIntegerPosition();
            stationaryCharacter.Runtime.SyncIntegerPosition();
            movingCharacter.SwitchDir("right");
            stationaryCharacter.SwitchDir("left");
            world.Register(movingCharacter);
            world.Register(stationaryCharacter);

            world.Runtime.Stage.SetSceneSnapshot(2400, 100, 500, 35, -20);
            SetPrivateField(world, "_cameraX", 173);
            SetPrivateField(world, "_cameraVel", 9);
            movingCharacter.Runtime.RenderOffsetX = 11.5f;
            stationaryCharacter.Runtime.RenderOffsetX = -7.5f;
            Expect(!world.IsUnityFixedWorldCameraStateClear,
                "disabled Unity battle camera check must inject a non-zero stale camera state");
            world.ResetUnityFixedWorldRenderOffsets();

            float firstEntityX = ComputeCameraDisabledEntityX(stationaryCharacter, world);
            int firstShadowX = stationaryCharacter.GetRuntimeXInt() +
                               (int)stationaryCharacter.Runtime.RenderOffsetX -
                               world.ReleaseCameraX;
            Expect(world.ReleaseCameraX == 0 && world.IsUnityFixedWorldCameraStateClear,
                "Unity battle camera state must stay zero regardless of stage width");
            Expect(Nearly(movingCharacter.Runtime.RenderOffsetX, 0f) &&
                   Nearly(stationaryCharacter.Runtime.RenderOffsetX, 0f),
                "Unity battle entities must not retain baseline C# camera perspective offsets");

            movingCharacter.Runtime.SetPosition(1800f, 0f, 420f);
            movingCharacter.Runtime.SyncIntegerPosition();
            movingCharacter.SwitchDir("left");
            world.Runtime.Stage.SetSceneSnapshot(3200, 20, 720, -60, 40);
            SetPrivateField(world, "_cameraX", 211);
            SetPrivateField(world, "_cameraVel", -4);
            stationaryCharacter.Runtime.RenderOffsetX = 19f;
            Expect(!world.IsUnityFixedWorldCameraStateClear,
                "character movement fixture must re-inject a non-zero stale camera state");
            world.ResetUnityFixedWorldRenderOffsets();

            float secondEntityX = ComputeCameraDisabledEntityX(stationaryCharacter, world);
            int secondShadowX = stationaryCharacter.GetRuntimeXInt() +
                                (int)stationaryCharacter.Runtime.RenderOffsetX -
                                world.ReleaseCameraX;
            Expect(world.ReleaseCameraX == 0 &&
                   world.IsUnityFixedWorldCameraStateClear &&
                   Nearly(stationaryCharacter.Runtime.RenderOffsetX, 0f),
                "character movement and facing must not re-arm the Unity battle camera");
            Expect(Nearly(firstEntityX, secondEntityX) && firstShadowX == secondShadowX,
                "another character moving must not shift a stationary entity or its shadow");
        }

        private static float ComputeCameraDisabledEntityX(LF2Entity entity, SimulationWorld world)
        {
            return entity.GetRuntimeXInt() + (int)entity.Runtime.RenderOffsetX - world.ReleaseCameraX;
        }

        private static void CheckScaledWeaponAnchorSample(
            string sample,
            float spriteWidth,
            float spriteHeight,
            float centerx,
            float centery,
            float expectedUncompensatedVerticalError)
        {
            const float visualScale = 1.5f;
            Vector2 pivot = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                420,
                0,
                260f,
                0f,
                0,
                0,
                2,
                false,
                spriteWidth,
                spriteHeight,
                centerx,
                centery,
                visualScale);

            ExpectRenderAnchor(
                pivot,
                false,
                420f,
                260f,
                spriteWidth,
                spriteHeight,
                centerx,
                centery,
                visualScale,
                sample);

            float uncompensatedError = (visualScale - 1f) * (spriteHeight - centery);
            Expect(Nearly(uncompensatedError, expectedUncompensatedVerticalError),
                $"{sample} fixture must retain the observed uncompensated scale error; actual={uncompensatedError}");
        }

        private static void ExpectRenderAnchor(
            Vector2 pivot,
            bool facingLeft,
            float expectedOriginX,
            float expectedOriginY,
            float spriteWidth,
            float spriteHeight,
            float centerx,
            float centery,
            float visualScale,
            string context)
        {
            float anchorX = facingLeft
                ? pivot.x - visualScale * (centerx - spriteWidth * 0.5f)
                : pivot.x - visualScale * (spriteWidth * 0.5f - centerx);
            float anchorY = pivot.y - visualScale * (spriteHeight - centery);
            Expect(Nearly(anchorX, expectedOriginX) && Nearly(anchorY, expectedOriginY),
                $"{context} must stay on the entity/shadow logical origin; anchor=({anchorX}, {anchorY}), expected=({expectedOriginX}, {expectedOriginY})");
        }

        private static void CheckCatchingAttackAction()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_Attacker", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_Victim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Jump = true, Right = true };
            attacker.Controller = controller;
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.Runtime.CaughtDuration = 300;
            attacker.Runtime.KeyJump = 0;
            attacker.Runtime.CdAttack = 5;
            attacker.Runtime.KeyLeft = 0;
            attacker.Runtime.KeyRight = 0;
            attacker.Runtime.KeyUp = 0;
            attacker.Runtime.KeyDown = 0;
            attacker.Trans.SetWait(attacker.Frame.D.wait, 7);
            victim.Trans.SetWait(victim.Frame.D.wait, 8);
            world.CaptureCollisionFrameSnapshotsAll();

            Expect(attacker.Match == world && victim.Match == world,
                "catch self-check entities must resolve their registered SimulationWorld");
            Expect(attacker.Runtime.SlotIndex >= 0 && victim.Runtime.SlotIndex >= 0 &&
                   attacker.Runtime.SlotIndex != victim.Runtime.SlotIndex,
                "catch self-check entities must receive distinct runtime slots");
            Expect(attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "catch self-check must establish both runtime cpoint links");
            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 100 && victim.CurrentFrameId == 130,
                "live Controller jump must not trigger aaction when Runtime.KeyJump is clear");

            controller.Jump = false;
            attacker.Runtime.KeyJump = 1;

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 120,
                "runtime aaction with no runtime direction must ignore conflicting live Controller direction");
            Expect(victim.CurrentFrameId == 131, "aaction 目标帧 cpoint.vaction 应直接写入被抓者帧 131");
            Expect(attacker.Trans.WaitCounter == 7 && victim.Trans.WaitCounter == 8,
                "aaction direct frame writes must preserve both wait counters");
            Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 0, "aaction 后双方 attacking 应清零");

            attacker.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            controller.Right = false;
            attacker.Runtime.KeyRight = 1;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 121,
                "runtime direction must select taction even when the live Controller has no direction");
            Expect(victim.CurrentFrameId == 131,
                "taction must read vaction from the newly selected catcher frame");
        }

        private static void CheckCatchingJumpAction()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_JumpAction", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_JumpVictim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Defend = true };
            attacker.Controller = controller;
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(160);
            victim.ImmediateFrame(130);
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.Runtime.KeyDefend = 0;
            attacker.Runtime.CdJump = 5;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 160 && victim.CurrentFrameId == 130,
                "live Controller defend must not trigger jaction when Runtime.KeyDefend is clear");

            controller.Defend = false;
            attacker.Runtime.KeyDefend = 1;
            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 120 && victim.CurrentFrameId == 131,
                "jaction must use Runtime.KeyDefend + Runtime.CdJump");
        }

        private static void CheckCatchingThrow()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_Thrower", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_ThrowVictim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Up = true };
            attacker.Controller = controller;
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(110);
            victim.ImmediateFrame(130);
            attacker.SwitchDir("left");
            attacker.Runtime.SetPosition(100f, 20f, 7f);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(0f, 0f, 1f);
            victim.Runtime.SyncIntegerPosition();
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.Runtime.KeyUp = 0;
            attacker.Runtime.KeyDown = 1;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 112, "throwvx 分支应让抓取者进入当前帧 next=112");
            Expect(victim.CurrentFrameId == 132, "throwvx 分支应无条件写入 victim vaction=132");
            Expect(Nearly(victim.Runtime.X, 124f) && Nearly(victim.Runtime.Y, -36f),
                "throwvx branch must place the victim from the catcher frame/cpoint geometry");
            Expect(Nearly(victim.Runtime.Vx, -8f), "左向投掷应反转 victim.vx");
            Expect(Nearly(victim.Runtime.Vy, -4f), "投掷应写入 victim.vy");
            Expect(Nearly(victim.Runtime.Vz, 3f), "按下方向投掷应写入正 throwvz");
            Expect(victim.WeaponCount == 25, "throwinjury>0 应写入 victim.WeaponCount");
            Expect(attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "throw kind1 sub-pass must not invent runtime link cleanup");
        }

        private static void CheckCpointDirControlUsesRuntimeInput()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_DirControl", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_DirControlVictim", 2, BuildVictimFrames());
            attacker.Controller = new SelfCheckController { Left = true };
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(150);
            victim.ImmediateFrame(130);
            attacker.SwitchDir("left");
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.AttackingCounter = 2;
            attacker.Runtime.KeyLeft = 0;
            attacker.Runtime.KeyRight = 1;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.Runtime.Dir == "right",
                "dircontrol must follow Runtime.KeyRight instead of conflicting live Controller left");
        }

        private static void CheckBeingCaughtPositionSync()
        {
            var world = new SimulationWorld();
            var catcher = CreateCharacter("SelfCheck_Catcher", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_BeingCaught", 2, BuildVictimFrames());
            world.Register(catcher);
            world.Register(victim);

            catcher.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            catcher.SwitchDir("left");
            catcher.PS.dir = "right";
            victim.SwitchDir("right");
            catcher.Runtime.SetPosition(50f, 12f, 4f);
            catcher.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(0f, 0f, 0f);
            victim.Runtime.SyncIntegerPosition();
            catcher.Catching = victim;
            victim.Catching = catcher;
            catcher.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = catcher.Runtime.SlotIndex;
            victim.FrameDelay = 0;
            victim.Trans.SetWait(victim.Frame.D.wait, 9);
            world.CaptureCollisionFrameSnapshotsAll();

            catcher.RunWeaponSyncHeldStep10();

            Expect(victim.CurrentFrameId == 131, "被抓位置同步应按 catcher cpoint.vaction 写入被抓者帧");
            Expect(victim.Trans.WaitCounter == 9,
                "being-caught vaction direct frame write must preserve the victim wait counter");
            Expect(Nearly(victim.Runtime.X, 94f),
                "held sync must use left-facing Runtime.Dir even when PS.dir is stale right");
            Expect(Nearly(victim.Runtime.Y, 20f), "被抓者 y 应按垂直坐标计算并应用 cover 修正");
            Expect(Nearly(victim.Runtime.Z, 3f), "被抓者 z 应复制 catcher 深度并应用 cover 修正");
            Expect(victim.Runtime.Dir == "left", "cover=10 应复制抓取者 Runtime.Dir");
            Expect(catcher.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == catcher.Runtime.SlotIndex,
                "position sync must preserve the established runtime cpoint links");
        }

        private static void CheckCpointStateExitPreservesRunningDefendJumpChain()
        {
            var world = new SimulationWorld();
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheck_NarutoRunningDefendJump",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(86, 15, 1, 87, 39, 79),
                    Frame(87, 15, 1, 88, 39, 79),
                    Frame(88, 15, 1, 999, 39, 79),
                    Frame(276, LF2States.Catching, 15, 277, 39, 79, new CatchPoint
                    {
                        kind = 1,
                        x = 80,
                        y = 39,
                        vaction = 131,
                        hurtable = 0,
                        decrease = 7,
                    }),
                    Frame(277, 15, 5, 278, 39, 79),
                    Frame(278, 15, 3, 279, 39, 79),
                    Frame(279, 15, 6, 86, 39, 79),
                },
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheck_NarutoRunningDefendJumpVictim",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(130, LF2States.BeingCaught, 99, 130, 35, 70, new CatchPoint
                    {
                        kind = 2,
                        x = 8,
                        y = 12,
                        hurtable = 1,
                    }),
                    Frame(131, LF2States.BeingCaught, 99, 131, 34, 69, new CatchPoint
                    {
                        kind = 2,
                        x = 9,
                        y = 13,
                        hurtable = 1,
                    }),
                    Frame(212, LF2States.Jump, 1, 212, 39, 79),
                },
            };

            LF2Character attacker = CreateCharacter(
                "SelfCheck_NarutoRunningDefendJumpAttacker", 2, attackerData);
            LF2Character victim = CreateCharacter(
                "SelfCheck_NarutoRunningDefendJumpVictim", 3, victimData);
            attacker.SetRuntimeSlotIndex(0);
            victim.SetRuntimeSlotIndex(1);
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(276);
            victim.ImmediateFrame(130);
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.Runtime.CaughtDuration = 300;
            attacker.PS.zz = 4f;
            attacker.CaptureCollisionFrameSnapshot();
            victim.CaptureCollisionFrameSnapshot();

            attacker.OnFrameTransit(277, false);

            Expect(attacker.Frame.N == 277 &&
                   ReferenceEquals(attacker.Catching, victim) &&
                   ReferenceEquals(victim.Catching, attacker) &&
                   attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex &&
                   attacker.Runtime.CaughtDuration == 300 &&
                   Nearly(attacker.PS.zz, 4f),
                "NARUTO-RUN-DJ: 276->277 state exit must preserve both cpoint references, slots, duration, and zz");

            world.PreInteractionTickAll(2);
            Expect(attacker.Frame.N == 277 &&
                   attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex &&
                   attacker.Runtime.CaughtDuration == 293,
                "NARUTO-RUN-DJ: next-tick PrevFrame2=276 kind1 pass must keep frame277 and the reciprocal links");

            for (int i = 0; i < 6; i++)
                attacker.SimFrameTick(3 + i);
            Expect(attacker.Frame.N == 278,
                "NARUTO-RUN-DJ: frame277 wait/next must continue to frame278 after the cpoint state exit");

            for (int i = 0; i < 4; i++)
                attacker.SimFrameTick(9 + i);
            Expect(attacker.Frame.N == 279,
                "NARUTO-RUN-DJ: frame278 wait/next must continue to frame279");

            for (int i = 0; i < 7; i++)
                attacker.SimFrameTick(13 + i);
            Expect(attacker.Frame.N == 86 &&
                   attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "NARUTO-RUN-DJ: frame279 must reach frame86 without state-transition catch cleanup");

            attacker.Reset();
            Expect(attacker.Catching == null && attacker.CaughtSlotIndex == -1 &&
                   attacker.CatcherSlotIndex == -1 && attacker.Runtime.CaughtDuration == 0,
                "NARUTO-RUN-DJ: full entity Reset must still clear preserved cpoint relationships");
        }

        private static void CheckCpointNegativeActionMatrix()
        {
            string[] actionKinds = { "aaction", "taction", "jaction" };
            for (int shellIndex = 0; shellIndex < 2; shellIndex++)
            {
                bool realCharacter = shellIndex == 0;
                for (int actionIndex = 0; actionIndex < actionKinds.Length; actionIndex++)
                {
                    string actionKind = actionKinds[actionIndex];
                    CatchPoint sourceCpoint = new CatchPoint
                    {
                        kind = 1,
                        x = 20,
                        y = 30,
                        cover = 0,
                        hurtable = 1,
                    };
                    if (actionKind == "aaction") sourceCpoint.aaction = -120;
                    else if (actionKind == "taction") sourceCpoint.taction = -120;
                    else sourceCpoint.jaction = -120;

                    LF2CharacterData attackerData = new LF2CharacterData
                    {
                        name = $"SelfCheck_NegativeAction_{actionKind}",
                        frames = new List<LF2FrameData>
                        {
                            Frame(0, 0, 0, 0, 39, 79),
                            Frame(100, 9, 1, 100, 39, 79, sourceCpoint),
                            Frame(120, 9, 2, 120, 41, 81, new CatchPoint
                            {
                                kind = 1, vaction = -131, hurtable = 1
                            }),
                        },
                    };
                    LF2CharacterData victimData = BuildCpointMatrixVictimFrames();
                    SimulationWorld world = new SimulationWorld();
                    LF2Entity attacker = CreateCpointMatrixEntity(realCharacter, $"NegativeAction_{actionKind}_Attacker", 1, attackerData);
                    LF2Entity victim = CreateCpointMatrixEntity(realCharacter, $"NegativeAction_{actionKind}_Victim", 2, victimData);
                    world.Register(attacker);
                    world.Register(victim);
                    LinkCpointEntities(attacker, victim);
                    attacker.SetCpointRawFramePreserveWait(100);
                    victim.SetCpointRawFramePreserveWait(130);
                    attacker.SwitchDir("right");
                    victim.SwitchDir("right");
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 7);
                    victim.Trans.SetWait(victim.Frame.D.wait, 8);
                    attacker.AttackingCounter = 5;
                    victim.AttackingCounter = 6;
                    attacker.Runtime.CdAttack = 5;
                    attacker.Runtime.CdJump = 5;
                    if (actionKind == "aaction") attacker.Runtime.KeyJump = 1;
                    else if (actionKind == "taction")
                    {
                        attacker.Runtime.KeyJump = 1;
                        attacker.Runtime.KeyRight = 1;
                    }
                    else attacker.Runtime.KeyDefend = 1;
                    world.CaptureCollisionFrameSnapshotsAll();

                    attacker.RunCpointCheckStep10();

                    string label = $"{(realCharacter ? "character" : "shared-DAT")} {actionKind}";
                    Expect(attacker.Frame.N == 120 && attacker.Runtime.Dir == "left",
                        $"{label}: negative action must flip attacker once and use the absolute frame");
                    Expect(victim.Frame.N == -131 && victim.Frame.D == null && victim.Runtime.Dir == "right",
                        $"{label}: action-produced victim vaction must remain a raw negative frame without flipping");
                    Expect(attacker.Trans.WaitCounter == 7 && victim.Trans.WaitCounter == 8,
                        $"{label}: action frame writes must preserve both wait counters");
                    Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 0,
                        $"{label}: action selection must explicitly clear both attacking counters");
                    Expect(attacker.Frame.Prev2 == 100 && victim.Frame.Prev2 == 130,
                        $"{label}: action selection must not overwrite collision prev_frame2 snapshots");
                }
            }
        }

        private static void CheckCpointHeldSyncVactionMatrix()
        {
            int[] vactions = { -131, 0, 131 };
            for (int shellIndex = 0; shellIndex < 2; shellIndex++)
            {
                bool realCharacter = shellIndex == 0;
                for (int i = 0; i < vactions.Length; i++)
                {
                    int vaction = vactions[i];
                    LF2CharacterData catcherData = new LF2CharacterData
                    {
                        name = $"SelfCheck_Held_{vaction}",
                        frames = new List<LF2FrameData>
                        {
                            Frame(0, 0, 0, 0, 39, 79),
                            Frame(100, 9, 1, 100, 39, 79, new CatchPoint
                            {
                                kind = 1, x = 20, y = 30, vaction = vaction, cover = 0, hurtable = 1
                            }),
                        },
                    };
                    SimulationWorld world = new SimulationWorld();
                    LF2Entity catcher = CreateCpointMatrixEntity(realCharacter, $"Held_{vaction}_Catcher", 1, catcherData);
                    LF2Entity victim = CreateCpointMatrixEntity(realCharacter, $"Held_{vaction}_Victim", 2, BuildCpointMatrixVictimFrames());
                    world.Register(catcher);
                    world.Register(victim);
                    LinkCpointEntities(catcher, victim);
                    catcher.SetCpointRawFramePreserveWait(100);
                    victim.SetCpointRawFramePreserveWait(130);
                    catcher.SwitchDir("right");
                    victim.SwitchDir("right");
                    catcher.Runtime.SetPosition(50, 12, 4);
                    catcher.Runtime.SyncIntegerPosition();
                    victim.Trans.SetWait(victim.Frame.D.wait, 9);
                    catcher.AttackingCounter = 5;
                    victim.AttackingCounter = 6;
                    victim.FrameDelay = 0;

                    catcher.RunWeaponSyncHeldStep10();

                    int expectedFrame = vaction < 0 ? -vaction : vaction;
                    string expectedDirection = vaction < 0 ? "left" : "right";
                    float expectedX = vaction < 0 ? -3f : (vaction == 0 ? 58f : 56f);
                    float expectedY = vaction < 0 ? 33f : 20f;
                    string label = $"{(realCharacter ? "character" : "shared-DAT")} held vaction={vaction}";
                    Expect(victim.Frame.N == expectedFrame && victim.Runtime.Dir == expectedDirection,
                        $"{label}: held sync must raw-write, then flip/abs a negative vaction exactly once");
                    Expect(victim.Trans.WaitCounter == 9,
                        $"{label}: held sync must preserve the victim wait counter");
                    Expect(Nearly(victim.Runtime.X, expectedX) && Nearly(victim.Runtime.Y, expectedY) && Nearly(victim.Runtime.Z, 3f),
                        $"{label}: held position must use raw-vaction cpoint coordinates and resolved-frame centers");
                    Expect(catcher.AttackingCounter == 5 && victim.AttackingCounter == 6,
                        $"{label}: zero-injury held sync must preserve attacking counters");
                }
            }
        }

        private static void CheckCpointThrowRawAndTransformMatrix()
        {
            for (int shellIndex = 0; shellIndex < 2; shellIndex++)
            {
                bool realCharacter = shellIndex == 0;
                for (int directionMode = 0; directionMode < 2; directionMode++)
                {
                    LF2CharacterData attackerData = BuildCpointThrowFrames(-112, -132, 25);
                    SimulationWorld world = new SimulationWorld();
                    LF2Entity attacker = CreateCpointMatrixEntity(realCharacter, "RawThrow_Attacker", 1, attackerData);
                    LF2Entity victim = CreateCpointMatrixEntity(realCharacter, "RawThrow_Victim", 2, BuildCpointMatrixVictimFrames());
                    world.Register(attacker);
                    world.Register(victim);
                    LinkCpointEntities(attacker, victim);
                    attacker.SetCpointRawFramePreserveWait(110);
                    victim.SetCpointRawFramePreserveWait(130);
                    attacker.SwitchDir("left");
                    victim.SwitchDir("right");
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 11);
                    victim.Trans.SetWait(victim.Frame.D.wait, 12);
                    attacker.AttackingCounter = 5;
                    victim.AttackingCounter = 6;
                    victim.Runtime.Vz = 6f;
                    if (directionMode == 1)
                    {
                        attacker.Runtime.KeyUp = 1;
                        attacker.Runtime.KeyDown = 1;
                    }
                    world.CaptureCollisionFrameSnapshotsAll();

                    attacker.RunCpointCheckStep10();

                    string label = $"{(realCharacter ? "character" : "shared-DAT")} raw throw mode={directionMode}";
                    Expect(attacker.Frame.N == -112 && attacker.Frame.D == null && attacker.Frame.Prev2 == -112,
                        $"{label}: attacker next must raw-write frame and prev_frame2");
                    Expect(victim.Frame.N == -132 && victim.Frame.D == null && victim.Frame.Prev2 == -132,
                        $"{label}: victim vaction must raw-write frame and prev_frame2");
                    Expect(attacker.Runtime.Dir == "left" && victim.Runtime.Dir == "right",
                        $"{label}: raw throw writes must not flip either entity");
                    Expect(attacker.Trans.WaitCounter == 11 && victim.Trans.WaitCounter == 12,
                        $"{label}: raw throw writes must preserve wait counters");
                    Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 6,
                        $"{label}: throw clears only attacker attacking");
                    Expect(Nearly(victim.Runtime.Vz, 6f),
                        $"{label}: neither/both depth inputs must preserve the previous victim Vz");
                }

                CheckCpointThrowTransformUsesCurrentDat(realCharacter);
            }
        }

        private static void CheckCpointThrowTransformUsesCurrentDat(bool realCharacter)
        {
            LF2CharacterData sourceData = BuildCpointThrowFrames(112, -132, -1);
            sourceData.frames.Add(Frame(130, 10, 1, 130, 35, 70, new CatchPoint { kind = 2 }));
            LF2CharacterData targetData = new LF2CharacterData
            {
                name = "SelfCheck_ThrowTransformTarget",
                weapon_hp = 321,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 3, -7, 100, 200),
                    Frame(130, 10, 4, 130, 777, 778, new CatchPoint { kind = 2 }),
                    Frame(132, 10, 5, 132, 33, 68, new CatchPoint { kind = 2 }),
                },
            };
            LF2CharacterDataWrapper targetWrapper = new LF2CharacterDataWrapper(2, targetData);
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                int resolverCalls = 0;
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                {
                    resolverCalls++;
                    return oid == 2 ? targetWrapper : null;
                };
                SimulationWorld world = new SimulationWorld();
                LF2Entity attacker = CreateCpointMatrixEntity(realCharacter, "TransformThrow_Attacker", 1, sourceData);
                LF2Entity victim = CreateCpointMatrixEntity(realCharacter, "TransformThrow_Victim", 2, targetData);
                LF2Entity ownedChild = CreateCpointMatrixEntity(realCharacter, "TransformThrow_Child", 1, sourceData);
                world.Register(attacker);
                world.Register(victim);
                world.Register(ownedChild);
                LinkCpointEntities(attacker, victim);
                ownedChild.KillCount = attacker.Runtime.SlotIndex;
                attacker.SetCpointRawFramePreserveWait(110);
                victim.SetCpointRawFramePreserveWait(130);
                ownedChild.SetCpointRawFramePreserveWait(130);
                attacker.SwitchDir("right");
                victim.SwitchDir("right");
                attacker.Runtime.SetPosition(100, 20, 7);
                attacker.Runtime.SyncIntegerPosition();
                attacker.Trans.SetWait(attacker.Frame.D.wait, 11);
                victim.Trans.SetWait(victim.Frame.D.wait, 12);
                victim.Runtime.Vz = 6f;
                world.CaptureCollisionFrameSnapshotsAll();

                Expect(victim.ObjectId == 2 && attacker.HasStep10ThrowTransformVictimData(victim),
                    $"transform throw fixture must expose victim DAT; victimOid={victim.ObjectId}");
                Expect(attacker.GetCollisionFrameData()?.cpoint?.throwinjury == -1,
                    $"transform throw fixture must preserve throwinjury=-1; actual={attacker.GetCollisionFrameData()?.cpoint?.throwinjury}");

                attacker.RunCpointCheckStep10();

                string label = realCharacter ? "character transform throw" : "shared-DAT transform throw";
                Expect(attacker.ObjectId == 2,
                    $"{label}: throwinjury=-1 must replace attacker ObjectId; actual={attacker.ObjectId}, frame={attacker.Frame.N}, resolverCalls={resolverCalls}");
                Expect(attacker.FrameCache.Wrapper == targetWrapper,
                    $"{label}: throwinjury=-1 must load target DAT wrapper; actual={attacker.FrameCache.Wrapper?.characterId}");
                Expect(attacker.Frame.N == -7 && attacker.Frame.Prev2 == -7 && attacker.Trans.WaitCounter == 11,
                    $"{label}: throw next must come from transformed DAT frame 0 and raw-write without changing wait");
                Expect(Nearly(victim.Runtime.X, 16f) && Nearly(victim.Runtime.Y, -156f),
                    $"{label}: throw geometry must use transformed DAT frame 0 centers");
                Expect(ownedChild.ObjectId == 2 && ownedChild.FrameCache.Wrapper == targetWrapper &&
                       ownedChild.Frame.D != null && ownedChild.Frame.D.centerx == 777,
                    $"{label}: owned child must reload current Frame.D after DAT propagation");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckCpointDecreaseEscape()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_Decrease", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_EscapeVictim", 2, BuildVictimFrames());
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(140);
            victim.ImmediateFrame(130);
            attacker.Runtime.SetPosition(30f, 0f, 0f);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(10f, 0f, 0f);
            victim.Runtime.SyncIntegerPosition();
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            victim.FrameDelay = 0;
            attacker.Runtime.CaughtDuration = 3;
            attacker.Trans.SetWait(attacker.Frame.D.wait, 10);
            victim.Trans.SetWait(victim.Frame.D.wait, 11);
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 0, "decrease<0 逃脱后抓取者应回 frame 0");
            Expect(victim.CurrentFrameId == 181, "decrease<0 逃脱后被抓者应进入 frame 181");
            Expect(attacker.Trans.WaitCounter == 10 && victim.Trans.WaitCounter == 11,
                "decrease escape raw frame writes must preserve both wait counters");
            Expect(attacker.Frame.D != null && attacker.Frame.D.frameId == 0 &&
                   victim.Frame.D != null && victim.Frame.D.frameId == 181,
                "decrease escape raw frame writes must keep Frame.D synchronized");
            Expect(attacker.HitCount == 1 && victim.HitCount == 1, "decrease<0 逃脱后双方 HitCount 应为 1");
            Expect(Nearly(victim.KnockbackVx, -4f), "抓取者在右侧时被抓者 knockback_vx 应为 -4");
            Expect(Nearly(victim.KnockbackVy, -3f), "逃脱后被抓者 knockback_vy 应为 -3");
            Expect(Nearly(victim.Runtime.Vx, 0f) && Nearly(victim.Runtime.Vy, 0f),
                "decrease escape must leave runtime velocity untouched before FramePostProcess");
            Expect(attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "decrease kind1 sub-pass must not invent runtime link cleanup");

            world.FramePostProcessAll();

            Expect(Nearly(victim.Runtime.Vx, -4f) && Nearly(victim.Runtime.Vy, -3f) && victim.HitCount == 0,
                $"FramePostProcess must consume escape knockback exactly once; vx={victim.Runtime.Vx}, vy={victim.Runtime.Vy}, hitCount={victim.HitCount}, frameDelay={victim.FrameDelay}");
        }

        private static void CheckCpointEscapeAndMismatchStillRunTail()
        {
            LF2CharacterData throwData = BuildCpointTailFrames(
                decrease: -5,
                throwVx: 8,
                dirControl: 0);
            var throwWorld = new SimulationWorld();
            LF2Entity thrower = CreateCpointMatrixEntity(false, "SelfCheck_EscapeThrow_Shared", 1, throwData);
            LF2Entity throwVictim = CreateCpointMatrixEntity(false, "SelfCheck_EscapeThrowVictim_Shared", 2, BuildCpointMatrixVictimFrames());
            throwWorld.Register(thrower);
            throwWorld.Register(throwVictim);
            LinkCpointEntities(thrower, throwVictim);
            thrower.SetCpointRawFramePreserveWait(110);
            throwVictim.SetCpointRawFramePreserveWait(130);
            thrower.Runtime.CaughtDuration = 3;
            throwWorld.CaptureCollisionFrameSnapshotsAll();

            thrower.RunCpointCheckStep10();

            Expect(thrower.Frame.N == 0 && throwVictim.Frame.N == 132,
                "CaughtDuration<0 must skip actions but still execute the throw tail using source cpoint data");
            Expect(Nearly(throwVictim.Runtime.Vx, 8f) && Nearly(throwVictim.Runtime.Vy, -4f),
                "CaughtDuration<0 throw tail must still write victim velocity");

            LF2CharacterData dirData = BuildCpointTailFrames(
                decrease: -5,
                throwVx: 0,
                dirControl: 1);
            var dirWorld = new SimulationWorld();
            LF2Entity dirCatcher = CreateCpointMatrixEntity(false, "SelfCheck_EscapeDir_Shared", 1, dirData);
            LF2Entity dirVictim = CreateCpointMatrixEntity(false, "SelfCheck_EscapeDirVictim_Shared", 2, BuildCpointMatrixVictimFrames());
            dirWorld.Register(dirCatcher);
            dirWorld.Register(dirVictim);
            LinkCpointEntities(dirCatcher, dirVictim);
            dirCatcher.SetCpointRawFramePreserveWait(110);
            dirVictim.SetCpointRawFramePreserveWait(130);
            dirCatcher.Runtime.CaughtDuration = 3;
            dirCatcher.AttackingCounter = 2;
            dirCatcher.SwitchDir("left");
            dirCatcher.Runtime.KeyRight = 1;
            dirWorld.CaptureCollisionFrameSnapshotsAll();

            dirCatcher.RunCpointCheckStep10();

            Expect(dirCatcher.Runtime.Dir == "right",
                "CaughtDuration<0 must still execute dircontrol after skipping action selection");

            var mismatchWorld = new SimulationWorld();
            LF2Entity mismatchCatcher = CreateCpointMatrixEntity(false, "SelfCheck_MismatchThrow_Shared", 1, BuildCpointTailFrames(0, 8, 0));
            LF2Entity mismatchVictim = CreateCpointMatrixEntity(false, "SelfCheck_MismatchThrowVictim_Shared", 2, BuildCpointMatrixVictimFrames());
            mismatchWorld.Register(mismatchCatcher);
            mismatchWorld.Register(mismatchVictim);
            mismatchCatcher.CaughtSlotIndex = mismatchVictim.Runtime.SlotIndex;
            mismatchVictim.CatcherSlotIndex = -1;
            mismatchCatcher.SetCpointRawFramePreserveWait(110);
            mismatchVictim.SetCpointRawFramePreserveWait(130);
            mismatchCatcher.Trans.SetWait(mismatchCatcher.Frame.D.wait, 9);
            mismatchWorld.CaptureCollisionFrameSnapshotsAll();

            mismatchCatcher.RunCpointCheckStep10();

            Expect(mismatchCatcher.Frame.N == 0 && mismatchVictim.Frame.N == 132,
                "cpoint mismatch must suppress actions but still run the source throw tail");
            Expect(mismatchCatcher.Frame.D != null && mismatchCatcher.Frame.D.frameId == 0 &&
                   mismatchCatcher.Trans.WaitCounter == 9,
                "cpoint mismatch frame0 fallback must preserve wait and synchronize Frame.D");
            Expect(Nearly(mismatchVictim.Runtime.Vx, 8f),
                "cpoint mismatch throw tail must still apply horizontal throw velocity");
        }

        private static void CheckSharedDatCpointStep10StatsAndInputOrder()
        {
            LF2CharacterData catcherData = BuildSharedCpointStatsFrames();
            LF2CharacterData victimData = BuildCpointMatrixVictimFrames();
            var world = new SimulationWorld();
            LF2Entity holder = CreateCpointMatrixEntity(false, "SelfCheck_SharedCpointHolder", 3, victimData);
            LF2Entity catcher = CreateCpointMatrixEntity(false, "SelfCheck_SharedCpointCatcher", 1, catcherData);
            LF2Entity victim = CreateCpointMatrixEntity(false, "SelfCheck_SharedCpointVictim", 2, victimData);
            world.Register(holder);
            world.Register(catcher);
            world.Register(victim);
            LinkCpointEntities(catcher, victim);
            catcher.SetCpointRawFramePreserveWait(100);
            victim.SetCpointRawFramePreserveWait(130);
            catcher.Runtime.KeyJump = 1;
            catcher.Runtime.CdAttack = 5;
            catcher.Runtime.KeyDefend = 1;
            catcher.Runtime.CdJump = 5;
            catcher.Runtime.KeyRight = 1;
            catcher.SwitchDir("right");
            world.CaptureCollisionFrameSnapshotsAll();

            catcher.RunCpointCheckStep10();

            Expect(catcher.Frame.N == 122 && victim.Frame.N == 133,
                "simultaneous attack/direction/jump input must resolve taction before the final jaction");
            Expect(catcher.Runtime.Dir == "right",
                "positive simultaneous cpoint actions must preserve shared-DAT shell facing");

            catcher.SetCpointRawFramePreserveWait(100);
            victim.SetCpointRawFramePreserveWait(130);
            catcher.AttackingCounter = 2;
            catcher.Runtime.KeyJump = 0;
            catcher.Runtime.KeyDefend = 0;
            catcher.Runtime.KeyRight = 0;
            catcher.Runtime.KeyLeft = 1;
            world.CaptureCollisionFrameSnapshotsAll();
            catcher.RunCpointCheckStep10();
            Expect(catcher.Runtime.Dir == "left",
                "shared-DAT non-character cpoint dircontrol must use runtime input without a CLR character gate");

            catcher.SetCpointRawFramePreserveWait(100);
            victim.SetCpointRawFramePreserveWait(130);
            catcher.AttackingCounter = 0;
            catcher.HolderCopySlot = holder.Runtime.SlotIndex;
            victim.Health.HP = 20;
            victim.Health.HPBound = 20;
            victim.Health.HPLost = 7;
            victim.KillCount = -1;
            victim.Unk344 = 1;
            world.KillStats[1] = 0;
            world.DamageStats[1] = 0;

            catcher.RunWeaponSyncHeldStep10();

            Expect(victim.Health.HP == -10 && victim.Health.HPBound == 10,
                "shared-DAT held cpoint injury must apply HP and HPBound damage");
            Expect(victim.Health.HPLost == 7,
                "held cpoint injury must not write the unrelated HPLost accumulator");
            Expect(holder.KillStat == 1 && world.KillStats[1] == 1,
                "lethal held cpoint injury must credit holder and indexed kill statistics");
            Expect(holder.ComboCountAtk == 30 && victim.ComboCountVic == 30 && world.DamageStats[1] == 30,
                "held cpoint injury must credit combo and indexed damage statistics exactly once");
        }

        private static LF2CharacterData BuildCpointTailFrames(int decrease, int throwVx, int dirControl)
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_CpointTail",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(110, LF2States.Catching, 1, 0, 40, 80, new CatchPoint
                    {
                        kind = 1,
                        x = 16,
                        y = 24,
                        vaction = 132,
                        aaction = 120,
                        decrease = decrease,
                        throwvx = throwVx,
                        throwvy = -4,
                        throwvz = 3,
                        dircontrol = dirControl,
                        hurtable = 1,
                    }),
                    Frame(120, LF2States.Catching, 1, 120, 39, 79, new CatchPoint { kind = 1, vaction = 131, hurtable = 1 }),
                },
            };
        }

        private static LF2CharacterData BuildSharedCpointStatsFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_SharedCpointStats",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(100, LF2States.Catching, 1, 100, 39, 79, new CatchPoint
                    {
                        kind = 1,
                        vaction = 131,
                        aaction = 120,
                        taction = 121,
                        jaction = 122,
                        dircontrol = 1,
                        injury = 30,
                        hurtable = 1,
                    }),
                    Frame(120, LF2States.Catching, 1, 120, 39, 79, new CatchPoint { kind = 1, vaction = 131, hurtable = 1 }),
                    Frame(121, LF2States.Catching, 1, 121, 39, 79, new CatchPoint { kind = 1, vaction = 132, hurtable = 1 }),
                    Frame(122, LF2States.Catching, 1, 122, 39, 79, new CatchPoint { kind = 1, vaction = 133, hurtable = 1 }),
                },
            };
        }

        private static void CheckBattleFlowToggleAndTeleportMatrix()
        {
            var flowWorld = new SimulationWorld();
            for (int tick = 1; tick <= 13; tick++)
            {
                flowWorld.AdvanceBattleFlowTick(tick);
                if (tick <= 4 || tick >= 11)
                {
                    Expect(flowWorld.CurrentTickIndex == tick,
                        $"flow tick {tick}: CurrentTickIndex must advance at tick head");
                    Expect(flowWorld.InputPhase == (tick & 1),
                        $"flow tick {tick}: InputPhase parity mismatch");
                    Expect(flowWorld.FrameMod12 == tick % 12,
                        $"flow tick {tick}: FrameMod12 mismatch");
                    Expect(flowWorld.FrameToggle == (tick & 1),
                        $"flow tick {tick}: FrameToggle parity mismatch");
                }
            }

            flowWorld.ResetRuntimeState();
            Expect(flowWorld.CurrentTickIndex == 0 && flowWorld.InputPhase == 0 &&
                   flowWorld.FrameMod12 == 0 && flowWorld.FrameToggle == 0,
                "battle flow reset must clear tick, input phase, FrameMod12 and FrameToggle");

            var gatedWorld = new SimulationWorld();
            FlowSelfCheckEntity gatedSource = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportGate_Source", LF2ObjectType.Character,
                LF2States.TeleportToEnemy, 1, 10, 20, 0);
            FlowSelfCheckEntity gatedTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportGate_Target", LF2ObjectType.Character,
                LF2States.Standing, 2, 300, 40, 1);
            gatedWorld.Register(gatedSource);
            gatedWorld.Register(gatedTarget);

            gatedWorld.AdvanceBattleFlowTick(1);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(1);
            Expect(gatedSource.GetRuntimeXInt() == 10 && gatedSource.GetRenderZInt() == 20,
                "FrameToggle=1 on tick 1 must gate state 400 teleport");

            gatedWorld.AdvanceBattleFlowTick(2);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(gatedSource.GetRuntimeXInt() == 180 && gatedSource.GetRenderZInt() == 41,
                "FrameToggle=0 on tick 2 must run state 400 teleport");

            gatedSource.Runtime.SetPosition(25f, -8f, 30f);
            gatedSource.Runtime.SyncIntegerPosition();
            gatedWorld.AdvanceBattleFlowTick(3);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(3);
            Expect(gatedSource.GetRuntimeXInt() == 25 && gatedSource.GetRenderZInt() == 30,
                "FrameToggle=1 on tick 3 must gate state 400 teleport");

            gatedWorld.AdvanceBattleFlowTick(4);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(4);
            Expect(gatedSource.GetRuntimeXInt() == 180 && gatedSource.GetRenderZInt() == 41,
                "FrameToggle=0 on tick 4 must run state 400 teleport");

            var selfWorld = new SimulationWorld();
            FlowSelfCheckEntity selfSource = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport401_Self", LF2ObjectType.Character,
                LF2States.TeleportToTeammate, 3, 100, 20, 5);
            selfWorld.Register(selfSource);
            AdvanceFlowToEvenToggle(selfWorld);
            selfWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(selfSource.GetRuntimeXInt() == 40 && selfSource.GetRenderZInt() == 21,
                "state 401 must be allowed to select self when no farther teammate exists");

            var sourceTypeWorld = new SimulationWorld();
            FlowSelfCheckEntity nonCharacterSource = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_NonCharacterSource", LF2ObjectType.Other,
                LF2States.TeleportToEnemy, 1, 0, 0, 10);
            FlowSelfCheckEntity characterTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_CharacterTarget", LF2ObjectType.Character,
                LF2States.Standing, 2, 250, 50, 11);
            sourceTypeWorld.Register(nonCharacterSource);
            sourceTypeWorld.Register(characterTarget);
            AdvanceFlowToEvenToggle(sourceTypeWorld);
            sourceTypeWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(nonCharacterSource.GetRuntimeXInt() == 130 && nonCharacterSource.GetRenderZInt() == 51,
                "state 400 source must not require Character DAT when its target is Character DAT");

            var selectionWorld = new SimulationWorld();
            FlowSelfCheckEntity selectionSource = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Source", LF2ObjectType.Character,
                LF2States.TeleportToEnemy, 1, 0, 0, 20);
            FlowSelfCheckEntity farCharacter = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Far", LF2ObjectType.Character,
                LF2States.Standing, 2, 400, 0, 21);
            FlowSelfCheckEntity nearCharacter = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Near", LF2ObjectType.Character,
                LF2States.Standing, 2, 200, 0, 22);
            FlowSelfCheckEntity ignoredNonCharacter = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Ignored", LF2ObjectType.Other,
                LF2States.Standing, 2, 20, 0, 23);
            selectionWorld.Register(selectionSource);
            selectionWorld.Register(farCharacter);
            selectionWorld.Register(nearCharacter);
            selectionWorld.Register(ignoredNonCharacter);
            AdvanceFlowToEvenToggle(selectionWorld);
            selectionWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(selectionSource.GetRuntimeXInt() == 80 && selectionSource.GetRenderZInt() == 1,
                "state 400 must select the nearest live Character DAT target and ignore non-character targets");

            var noTargetWorld = new SimulationWorld();
            FlowSelfCheckEntity noTargetSource = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_NoTarget", LF2ObjectType.Character,
                LF2States.TeleportToEnemy, 4, 70, 30, 30);
            FlowSelfCheckEntity sameTeamOnly = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_NoTarget_SameTeam", LF2ObjectType.Character,
                LF2States.Standing, 4, 200, 30, 31);
            noTargetSource.Runtime.Y = -12f;
            noTargetSource.Runtime.YInt = -12;
            noTargetSource.Runtime.Vx = 5f;
            noTargetSource.Runtime.Vy = -6f;
            noTargetSource.Runtime.Vz = 7f;
            noTargetWorld.Register(noTargetSource);
            noTargetWorld.Register(sameTeamOnly);
            AdvanceFlowToEvenToggle(noTargetWorld);
            noTargetWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(noTargetSource.GetRuntimeXInt() == 70 && noTargetSource.GetRenderZInt() == 30 &&
                   noTargetSource.GetRuntimeYInt() == 0 && Nearly(noTargetSource.Runtime.Vx, 0f) &&
                   Nearly(noTargetSource.Runtime.Vy, 0f) && Nearly(noTargetSource.Runtime.Vz, 0f),
                "state 400 no-target branch must preserve X/Z and clear Y/velocity");
        }

        private static void CheckHeldReferenceSlotReuseContracts()
        {
            var world = new SimulationWorld();
            LF2Character holder = CreateCharacter("SelfCheck_HeldReuseHolder", 1, BuildCatchingFrames());
            holder.SetRuntimeSlotIndex(0);
            FlowSelfCheckEntity held = CreateFlowSelfCheckEntity(
                "SelfCheck_HeldReuseOriginal", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 50);
            world.Register(holder);
            world.Register(held);
            holder.HoldWeapon(held);
            held.TrackerParent = holder;
            Expect(ReferenceEquals(holder.GetHeldWeapon(), held) &&
                   holder.Runtime.TargetSlotIndex == 50 &&
                   held.Runtime.HolderStableId == 0,
                "RISK-3: initial HoldWeapon binding must survive runtime slot validation");

            world.Unregister(held);
            Expect(held.TrackerParent == null,
                "RISK-3: unregister must clear the removed entity's TrackerParent cache");
            FlowSelfCheckEntity sameSlotNewborn = CreateFlowSelfCheckEntity(
                "SelfCheck_HeldReuseSameSlot", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 50);
            world.Register(sameSlotNewborn);
            Expect(holder.GetHeldWeapon() == null &&
                   holder.Runtime.LinkState == 0 &&
                   holder.Runtime.TargetSlotIndex == -1 &&
                   holder.Runtime.HeldWeaponStableId == -1,
                "RISK-3: same-slot newborn without the reverse holder relation must not inherit a stale held cache");

            LF2Character differentSlotHolder = CreateCharacter(
                "SelfCheck_HeldReuseDifferentHolder", 2, BuildCatchingFrames());
            differentSlotHolder.SetRuntimeSlotIndex(1);
            FlowSelfCheckEntity differentHeld = CreateFlowSelfCheckEntity(
                "SelfCheck_HeldReuseDifferentOriginal", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 51);
            world.Register(differentSlotHolder);
            world.Register(differentHeld);
            differentSlotHolder.HoldWeapon(differentHeld);
            world.Unregister(differentHeld);
            FlowSelfCheckEntity differentSlotNewborn = CreateFlowSelfCheckEntity(
                "SelfCheck_HeldReuseDifferentNewborn", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 52);
            world.Register(differentSlotNewborn);
            Expect(differentSlotHolder.GetHeldWeapon() == null,
                "RISK-3: different-slot reuse must also invalidate the stale held reference");

            FlowSelfCheckEntity trackerParent = CreateFlowSelfCheckEntity(
                "SelfCheck_TrackerReuseParent", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 60);
            FlowSelfCheckEntity trackerChild = CreateFlowSelfCheckEntity(
                "SelfCheck_TrackerReuseChild", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 61);
            world.Register(trackerParent);
            world.Register(trackerChild);
            trackerParent.Runtime.LinkState = 1;
            trackerParent.Runtime.TargetSlotIndex = 61;
            trackerChild.Runtime.LinkState = -1;
            trackerChild.Runtime.HolderStableId = 60;
            trackerChild.TrackerParent = trackerParent;
            Expect(ReferenceEquals(trackerChild.ResolveTrackerParentFromRuntime(), trackerParent),
                "RISK-3: valid TrackerParent must resolve through slot and reverse relation");
            world.Unregister(trackerParent);
            FlowSelfCheckEntity trackerSameSlotNewborn = CreateFlowSelfCheckEntity(
                "SelfCheck_TrackerReuseSameSlot", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 60);
            world.Register(trackerSameSlotNewborn);
            Expect(trackerChild.ResolveTrackerParentFromRuntime() == null && trackerChild.TrackerParent == null,
                "RISK-3: TrackerParent must not bind to a same-slot newborn without the reverse relation");
        }

        private static void CheckHeldWeaponActCoverOffsets()
        {
            LF2Character holder = CreateCharacter(
                "SelfCheck_HeldActCoverHolder", 1, BuildCatchingFrames());
            holder.SwitchDir("right");
            holder.Runtime.SetPosition(0.0, 0.0, 42.0);
            holder.Runtime.SyncIntegerPosition();

            HeldActSelfCheckWeapon weapon = CreateHeldActSelfCheckWeapon(
                "SelfCheck_HeldActCoverWeapon", weaponType: 0);
            var holderWPoint = new WeaponPoint
            {
                weaponact = 0,
                cover = 0,
            };
            var holdpoint = new Vector3(100f, 200f, 42f);

            weapon.PS.zz = 9f;
            WeaponActResult frontResult = weapon.Act(holder, holderWPoint, holdpoint);
            Expect(!frontResult.Thrown &&
                   Nearly(weapon.Runtime.Z, 43.0) &&
                   Nearly(weapon.Runtime.Y, 199.0) &&
                   Nearly(weapon.PS.zz, 0f),
                "BATTLE-AUDIT3-02: real held Act with cover=0 must apply Z+1/Y-1 and clear zz");

            holderWPoint.cover = 10;
            weapon.PS.zz = 9f;
            WeaponActResult backResult = weapon.Act(holder, holderWPoint, holdpoint);
            Expect(!backResult.Thrown &&
                   Nearly(weapon.Runtime.Z, 41.0) &&
                   Nearly(weapon.Runtime.Y, 201.0) &&
                   Nearly(weapon.PS.zz, 0f),
                "BATTLE-AUDIT3-02: real held Act with nonzero cover must apply Z-1/Y+1 and clear zz");
        }

        private static void CheckHeldWeaponActSkipsOrdinaryStrengthAttack()
        {
            LF2Character holder = CreateCharacter(
                "SelfCheck_HeldActStrengthHolder", 1, BuildCatchingFrames());
            holder.SwitchDir("right");

            HeldActSelfCheckWeapon weapon = CreateHeldActSelfCheckWeapon(
                "SelfCheck_HeldActStrengthWeapon", weaponType: 0);
            weapon.SetWeaponStrengthList(new List<WeaponStrengthEntry>
            {
                new WeaponStrengthEntry
                {
                    index = 1,
                    injury = 25,
                    arest = 4,
                    vrest = 8,
                },
            });

            var holderWPoint = new WeaponPoint
            {
                weaponact = 0,
                attacking = 1,
                dvx = 0,
            };

            weapon.Act(holder, holderWPoint, Vector3.zero);

            Expect(weapon.ProcessAttackCallCount == 0,
                "BATTLE-AUDIT3-05: ordinary weapon_strength held Act must not call ProcessAttack");
        }

        private static void CheckHeldKind5ConsumesFrozenCandidates()
        {
            RunHeldKind5ConsumeCase(attackingItrIndex: 1, expectDamage: true);
            RunHeldKind5ConsumeCase(attackingItrIndex: 0, expectDamage: false);
        }

        private static void RunHeldKind5ConsumeCase(int attackingItrIndex, bool expectDamage)
        {
            string suffix = expectDamage ? "Active" : "Dormant";
            InteractionArea holderPlaceholder = MakeInteractionItr(0, 1, 0, 0);
            InteractionArea holderAttack = MakeInteractionItr(0, 6, 25, 4);
            LF2FrameData holderFrame = InteractionFrame(holderPlaceholder);
            holderFrame.itrs.Add(holderAttack);
            holderFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { attacking = attackingItrIndex },
            };

            LF2Character holder = CreateInteractionCharacter(
                $"SelfCheck_Audit3HeldKind5Holder_{suffix}",
                1,
                new LF2CharacterData
                {
                    name = $"SelfCheck_Audit3HeldKind5Holder_{suffix}",
                    frames = new List<LF2FrameData> { holderFrame },
                });

            InteractionArea heldKind5 = MakeInteractionItr(5, 1, 0, 0);
            LF2FrameData heldFrame = InteractionFrame(heldKind5);
            var heldData = new LF2CharacterData
            {
                name = $"SelfCheck_Audit3HeldKind5Weapon_{suffix}",
                frames = new List<LF2FrameData> { heldFrame },
            };
            AlternateDamageSelfCheckWeapon heldWeapon = CreateSelfCheckWeapon(
                $"SelfCheck_Audit3HeldKind5Weapon_{suffix}",
                434,
                1,
                heldData,
                0);
            LF2Character victim = CreateInteractionCharacter(
                $"SelfCheck_Audit3HeldKind5Victim_{suffix}",
                37,
                BuildInteractionVictimData($"SelfCheck_Audit3HeldKind5Victim_{suffix}", 37));

            var world = new SimulationWorld();
            world.Register(holder);
            world.Register(heldWeapon);
            world.Register(victim);
            ConfigureCollisionAuditEntity(holder, 1, 0.0);
            ConfigureCollisionAuditEntity(heldWeapon, 1, 0.0);
            ConfigureCollisionAuditEntity(victim, 2, 0.0);

            int holderSlot = holder.Runtime.SlotIndex;
            int heldSlot = heldWeapon.Runtime.SlotIndex;
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = heldSlot;
            heldWeapon.Runtime.LinkState = -1;
            heldWeapon.Runtime.HolderStableId = holderSlot;
            heldWeapon.HolderCopySlot = holderSlot;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            var query = world.SceneQuery as BruteForceSceneQuery;
            Expect(query != null &&
                   query.TryGetCollisionCandidateSequence(heldWeapon, out List<SceneQueryHit> candidates) &&
                   candidates.Count == 1 &&
                   candidates[0].Target == victim,
                $"BATTLE-AUDIT3-05: held kind5 {suffix} case must freeze a real weapon candidate");

            heldWeapon.FrameDelay = 3;
            heldWeapon.AttackExempt = 2;
            world.PostInteractionTickAll(1);
            world.ObjectInteractionTickAll(1);
            world.EndCollisionCandidateConsumption();

            if (expectDamage)
            {
                int appliedVrest = victim.ItrRest.GetVrest(heldSlot);
                Expect(victim.Health.HP < 100 && appliedVrest > 0,
                    $"BATTLE-AUDIT3-05: held kind5 must consume the frozen pair and apply holder Prev2 itr damage; " +
                    $"hp={victim.Health.HP}, vrest={appliedVrest}");
            }
            else
            {
                Expect(victim.Health.HP == 100 && victim.ItrRest.GetVrest(heldSlot) == 0,
                    "BATTLE-AUDIT3-05: attacking=0 held kind5 must remain dormant and deal no damage");
            }
        }

        private static HeldActSelfCheckWeapon CreateHeldActSelfCheckWeapon(string name, int weaponType)
        {
            LF2FrameData heldFrame = Frame(0, LF2States.Standing, 1, 0, 0, 0);
            heldFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 0, y = 0 },
            };
            var data = new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData> { heldFrame },
            };

            var weapon = new HeldActSelfCheckWeapon();
            weapon.BindData(name, 990, weaponType, data);
            return weapon;
        }

        private static void CheckGenericHeldStep12ContinuationContracts()
        {
            RunGenericHeldStep12Case(
                "DamagedDvx",
                LF2ObjectType.LightWeapon,
                new WeaponPoint { weaponact = 20, dvx = 8, dvy = -3 },
                (held, result) =>
                {
                    Expect(result.ForceDrop && result.Thrown && held.ImmediateFrameCallCount == 0,
                        "BATTLE-AUDIT3-12/AUDIT4-06: damaged generic held object must continue into dvx using raw frame writes");
                    Expect(Nearly(held.Runtime.Vx, 8.0) && Nearly(held.Runtime.Vy, -3.0) &&
                           held.Runtime.LinkState == 0,
                        "BATTLE-AUDIT3-12: damaged dvx continuation must apply throw velocity and release the link");
                });

            RunGenericHeldStep12Case(
                "DamagedKind3",
                LF2ObjectType.LightWeapon,
                new WeaponPoint { weaponact = 20, kind = 3 },
                (held, result) =>
                {
                    Expect(result.ForceDrop && !result.Thrown && held.ImmediateFrameCallCount == 0,
                        "BATTLE-AUDIT3-12/AUDIT4-06: damaged generic held object must continue into kind3 using raw frame writes");
                    Expect(held.Runtime.LinkState == 0 && Nearly(held.Runtime.Zz, 0f),
                        "BATTLE-AUDIT3-12: damaged kind3 continuation must leave the held object released and unlayered");
                });

            RunGenericHeldStep12Case(
                "HeavyThrow",
                LF2ObjectType.HeavyWeapon,
                new WeaponPoint { weaponact = 0, dvx = 9, dvy = -4 },
                (held, result) =>
                {
                    Expect(result.Thrown && held.FrameDelay == 1,
                        "BATTLE-AUDIT3-12: generic IronBall/heavy throw must write FrameDelay=1");
                    Expect(Nearly(held.Runtime.Vx, 9.0) && Nearly(held.Runtime.Vy, -4.0),
                        "BATTLE-AUDIT3-12: generic IronBall/heavy throw must apply authored velocity");
                });

            CheckWorldLevelRealWeaponStep12Contracts();
        }

        private static void CheckWorldLevelRealWeaponStep12Contracts()
        {
            RunWorldLevelRealWeaponStep12Case(
                "DamagedDvx",
                weaponType: 1,
                new WeaponPoint { weaponact = 20, dvx = 8, dvy = -3, dvz = 4 },
                (holder, weapon) =>
                {
                    Expect(weapon.Frame.N == 40 &&
                           weapon.Runtime.WeaponState == LF2States.WeaponThrowing &&
                           Nearly(weapon.Runtime.Vx, 8.0) && Nearly(weapon.Runtime.Vy, -3.0),
                        "BATTLE-AUDIT3-12: world-level real LF2Weapon damaged release must continue into dvx throw");
                    Expect(holder.Runtime.LinkState == 0 && weapon.Runtime.LinkState == 0,
                        "BATTLE-AUDIT3-12: world-level damaged dvx continuation must clear both real weapon links");
                });

            RunWorldLevelRealWeaponStep12Case(
                "DamagedKind3",
                weaponType: 0,
                new WeaponPoint { weaponact = 20, kind = 3 },
                (holder, weapon) =>
                {
                    Expect(weapon.Frame.N >= 0 && weapon.Frame.N < 6 &&
                           weapon.Runtime.Vx >= -3.0 && weapon.Runtime.Vx <= 3.0 &&
                           weapon.Runtime.Vy >= -3.0 && weapon.Runtime.Vy <= 0.0 &&
                           weapon.Runtime.Vz >= -0.4 && weapon.Runtime.Vz <= 0.4,
                        "BATTLE-AUDIT3-12: world-level real LF2Weapon damaged release must continue into kind3 random drop");
                    Expect(holder.Runtime.LinkState == 0 && weapon.Runtime.LinkState == 0 &&
                           Nearly(weapon.Runtime.Zz, 0f),
                        "BATTLE-AUDIT3-12: world-level damaged kind3 continuation must leave the real weapon released");
                });

            RunWorldLevelRealWeaponStep12Case(
                "IronBall",
                weaponType: 2,
                new WeaponPoint { weaponact = 0, dvx = 9, dvy = -4, dvz = 5 },
                (holder, weapon) =>
                {
                    Expect(weapon.Frame.N >= 0 && weapon.Frame.N < 6 &&
                           weapon.FrameDelay == 1 &&
                           weapon.Runtime.WeaponState == LF2States.WeaponThrowing,
                        "BATTLE-AUDIT3-12: world-level real IronBall throw must select a random frame and force FrameDelay=1");
                    Expect(Nearly(weapon.Runtime.Vx, 9.0) && Nearly(weapon.Runtime.Vy, -4.0) &&
                           holder.Runtime.LinkState == 0 && weapon.Runtime.LinkState == 0,
                        "BATTLE-AUDIT3-12: world-level real IronBall throw must apply authored velocity and clear links");
                });
        }

        private static void RunWorldLevelRealWeaponStep12Case(
            string label,
            int weaponType,
            WeaponPoint holderWPoint,
            Action<FlowSelfCheckEntity, HeldActSelfCheckWeapon> verify)
        {
            LF2FrameData holderFrame = Frame(0, LF2States.Standing, 100, 0, 39, 79);
            holderFrame.wpoints = new List<WeaponPoint> { holderWPoint };
            var holderData = new LF2CharacterData
            {
                name = $"SelfCheck_WorldHeld_{label}_Holder",
                frames = new List<LF2FrameData> { holderFrame },
            };

            var weaponFrames = new List<LF2FrameData>(41);
            for (int frameId = 0; frameId <= 40; frameId++)
            {
                int state = frameId == 20 ? LF2States.Falling : LF2States.Standing;
                LF2FrameData frame = Frame(frameId, state, 100, frameId, 0, 0);
                frame.wpoints = new List<WeaponPoint> { new WeaponPoint() };
                weaponFrames.Add(frame);
            }

            var weaponData = new LF2CharacterData
            {
                name = $"SelfCheck_WorldHeld_{label}_Weapon",
                frames = weaponFrames,
            };

            var world = new SimulationWorld();
            var holder = new FlowSelfCheckEntity(LF2ObjectType.Other);
            holder.BindData(holderData.name, 993, holderData);
            holder.SetRuntimeSlotIndex(10);
            holder.FrameDelay = 7;
            holder.Runtime.SetVelocity(90.0, -12.0, 6.0);
            holder.SwitchDir("right");

            var weapon = new HeldActSelfCheckWeapon();
            weapon.BindData(weaponData.name, 994, weaponType, weaponData);
            world.Register(holder);
            world.Register(weapon);

            int holderSlot = holder.Runtime.SlotIndex;
            int weaponSlot = weapon.Runtime.SlotIndex;
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = weaponSlot;
            holder.Runtime.HeldWeaponStableId = weaponSlot;
            weapon.Runtime.LinkState = -1;
            weapon.Runtime.HolderStableId = holderSlot;
            weapon.GrabbedBy = -1;

            world.HeldObjectProcessAll(1);

            verify(holder, weapon);
        }

        private static void CheckReleaseTickRunsHeldStep12Once()
        {
            const int drinkOid = 992;
            LF2FrameData holderFrame = Frame(0, LF2States.Charging, 0, 1, 0, 0);
            holderFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { weaponact = 0 },
            };
            LF2FrameData holderNextFrame = Frame(1, LF2States.Charging, 100, 1, 0, 0);
            holderNextFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { weaponact = 0 },
            };
            var holderData = new LF2CharacterData
            {
                name = "SelfCheck_HeldOnceHolder",
                frames = new List<LF2FrameData> { holderFrame, holderNextFrame },
            };
            LF2FrameData drinkFrame = Frame(0, LF2States.Standing, 100, 0, 0, 0);
            drinkFrame.wpoints = new List<WeaponPoint> { new WeaponPoint() };
            var drinkData = new LF2CharacterData
            {
                name = "SelfCheck_HeldOnceDrink",
                type_sub = 0x7A,
                frames = new List<LF2FrameData> { drinkFrame },
            };
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [drinkOid] = new LF2CharacterDataWrapper(drinkOid, drinkData),
            };
            var objectTypes = new Dictionary<int, int>
            {
                [drinkOid] = (int)LF2ObjectType.Drink,
            };
            using var runtimeConfigs = new TemporaryRuntimeObjectConfigs(objectTypes, wrappers);

            var world = new SimulationWorld();
            LF2Character holder = CreateCharacter("SelfCheck_HeldOnceHolder", 1, holderData);
            holder.SetRuntimeSlotIndex(0);
            holder.Trans.SyncDirectFrameData(holderFrame.wait, holderFrame.next, holder.Frame.N);
            var drink = new HeldActSelfCheckWeapon();
            drink.BindData("SelfCheck_HeldOnceDrink", drinkOid, 6, drinkData);
            drink.Health.HP = 20;
            drink.Health.HPBound = 20;
            world.Register(holder);
            world.Register(drink);
            holder.HoldWeapon(drink);

            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.RunReleaseTick(1);

            Expect(drink.Health.HP == 19,
                $"BATTLE-AUDIT3-06: one release tick must run held step12 exactly once; HP={drink.Health.HP}");
            Expect(holder.Frame.N == 1,
                "BATTLE-AUDIT3-06: the once-per-tick fixture must really cross a late holder frame boundary");
            Expect(holder.Runtime.LinkState == 6 && drink.Runtime.LinkState == -1,
                "BATTLE-AUDIT3-06: a nonempty drink must remain linked after its single held step12 pass");
        }

        private static void CheckLateHolderFrameChangeResyncsHeldPose()
        {
            RunLateHolderFrameChangeResyncCase(useRealWeapon: true);
            RunLateHolderFrameChangeResyncCase(useRealWeapon: false);
        }

        private static void RunLateHolderFrameChangeResyncCase(bool useRealWeapon)
        {
            string kind = useRealWeapon ? "Real" : "Generic";
            LF2FrameData holderFrame = Frame(0, LF2States.Standing, 0, 1, 40, 80);
            holderFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 10, y = 20, weaponact = 0, cover = 0 },
            };
            LF2FrameData holderNextFrame = Frame(1, LF2States.Standing, 100, 1, 60, 90);
            holderNextFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 35, y = 45, weaponact = 5, cover = 1 },
            };
            var holderData = new LF2CharacterData
            {
                name = $"SelfCheck_LateHeldPose_{kind}_Holder",
                frames = new List<LF2FrameData> { holderFrame, holderNextFrame },
            };

            LF2FrameData heldFrame = Frame(0, LF2States.Standing, 100, 0, 5, 10);
            heldFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 1, y = 2 },
            };
            LF2FrameData heldActionFrame = Frame(5, LF2States.Standing, 100, 5, 20, 30);
            heldActionFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 4, y = 6 },
            };
            var heldData = new LF2CharacterData
            {
                name = $"SelfCheck_LateHeldPose_{kind}_Held",
                frames = new List<LF2FrameData> { heldFrame, heldActionFrame },
            };

            var world = new SimulationWorld();
            LF2Character holder = CreateCharacter(holderData.name, 995, holderData);
            holder.Trans.SyncDirectFrameData(holderFrame.wait, holderFrame.next, holder.Frame.N);
            holder.Runtime.SetPosition(100.0, 0.0, 30.0);
            holder.Runtime.SyncIntegerPosition();
            holder.SwitchDir("right");

            LF2Entity held;
            if (useRealWeapon)
            {
                var weapon = new HeldActSelfCheckWeapon();
                weapon.BindData(heldData.name, 996, 0, heldData);
                held = weapon;
            }
            else
            {
                var generic = new HeldStep12SelfCheckEntity(LF2ObjectType.LightWeapon);
                generic.BindData(heldData.name, 997, heldData);
                held = generic;
            }

            world.Register(holder);
            world.Register(held);
            int holderSlot = holder.Runtime.SlotIndex;
            int heldSlot = held.Runtime.SlotIndex;
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = heldSlot;
            holder.Runtime.HeldWeaponStableId = heldSlot;
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = holderSlot;
            held.HolderCopySlot = holderSlot;
            held.AttackingCounter = 9;
            held.ItrRest.Arest = 11;

            world.HeldObjectProcessAll(1);
            Expect(held.Frame.N == 0 &&
                   held.Runtime.XInt == 74 && held.Runtime.YInt == -53 && held.Runtime.ZInt == 31,
                $"HELD-LATE-POSE: {kind} step12 must establish the frame-0 pose before late frame_tick");
            int attackingAfterStep12 = held.AttackingCounter;
            int arestAfterStep12 = held.ItrRest.Arest;

            GameObject holderView = new GameObject($"SelfCheck_LateHeldPose_{kind}_HolderView");
            holderView.SetActive(false);
            holderView.AddComponent<SpriteRenderer>();
            LF2ObjectRenderer holderRenderer = holderView.AddComponent<LF2ObjectRenderer>();
            SetPrivateField(holderRenderer, "_logicObject", holder);
            holder.Init(null, holderRenderer);

            GameObject heldView = new GameObject($"SelfCheck_LateHeldPose_{kind}_HeldView");
            heldView.SetActive(false);
            heldView.AddComponent<SpriteRenderer>();
            LF2ObjectRenderer heldRenderer = heldView.AddComponent<LF2ObjectRenderer>();
            SetPrivateField(heldRenderer, "_logicObject", held);
            if (held is HeldActSelfCheckWeapon realWeapon)
                realWeapon.AttachRenderer(heldRenderer);
            else
                ((HeldStep12SelfCheckEntity)held).AttachRenderer(heldRenderer);

            holderView.transform.position = new Vector3(999f, 998f, 0f);
            heldView.transform.position = new Vector3(997f, 996f, 0f);
            try
            {
                world.LateEntityUpdateAll(1);

                int immediateFrameCalls = useRealWeapon
                    ? ((HeldActSelfCheckWeapon)held).ImmediateFrameCallCount
                    : ((HeldStep12SelfCheckEntity)held).ImmediateFrameCallCount;
                Expect(holder.Frame.N == 1 && held.Frame.N == 5,
                    $"HELD-LATE-POSE: {kind} held weaponact must follow the holder's late wait/next transition in the same tick");
                Expect(held.Runtime.Dir == "right" && held.FrameDelay == holder.FrameDelay &&
                       held.Runtime.XInt == 91 && held.Runtime.YInt == -20 && held.Runtime.ZInt == 29 &&
                       Nearly(held.Runtime.Zz, 0f),
                    $"HELD-LATE-POSE: {kind} late pose must use the new centers/wpoints, facing, frameDelay, and cover");
                Expect(held.AttackingCounter == attackingAfterStep12 &&
                       held.ItrRest.Arest == arestAfterStep12 &&
                       immediateFrameCalls == 0 &&
                       holder.Runtime.LinkState > 0 && held.Runtime.LinkState < 0,
                    $"HELD-LATE-POSE: {kind} late pose sync must be raw and must not rerun step12 side effects");

                Vector3 expectedHolderViewPosition = CalculateRendererWorldPosition(holder, holderView.transform.position.z);
                Vector3 expectedHeldViewPosition = CalculateRendererWorldPosition(held, heldView.transform.position.z);
                Expect(Vector3.Distance(holderView.transform.position, expectedHolderViewPosition) < 0.001f &&
                       Vector3.Distance(heldView.transform.position, expectedHeldViewPosition) < 0.001f,
                    $"HELD-LATE-POSE: {kind} holder and held renderers must both refresh after the late frame transition");

                holder.Runtime.SetPosition(107.0, -3.0, 32.0);
                holder.Runtime.SyncIntegerPosition();
                world.HeldObjectProcessAll(2);
                Expect(holder.Frame.N == 1 && held.Frame.N == 5 &&
                       held.Runtime.XInt == 98 && held.Runtime.YInt == -23 && held.Runtime.ZInt == 31,
                    $"HELD-LATE-POSE: {kind} ordinary same-frame movement must keep using the normal step12 pose sync");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holderView);
                UnityEngine.Object.DestroyImmediate(heldView);
            }
        }

        private static Vector3 CalculateRendererWorldPosition(LF2Entity entity, float worldZ)
        {
            LF2FrameData frame = entity.Frame?.D;
            Vector2 pivot = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                entity.GetRuntimeXInt(),
                entity.GetRuntimeYInt(),
                entity.GetDisplayZ(),
                entity.GetRenderOffsetX(),
                entity.Match?.ReleaseCameraX ?? 0,
                entity.FrameDelay,
                entity.Match?.CurrentTickIndex ?? 0,
                entity.Runtime.Dir == "left",
                entity.GetSpriteWidthPxForRender(),
                entity.GetSpriteHeightPxForRender(),
                frame?.centerx ?? 0,
                frame?.centery ?? 0,
                NTSDRenderSpace.BattleVisualScale);
            return NTSDRenderSpace.ScreenPixelToWorld(pivot.x, pivot.y, worldZ);
        }

        private static void CheckReleaseTickCpointSyncPrecedesCandidates()
        {
            var world = new SimulationWorld();
            world.RefreshStageRuntimeSnapshotFromScene();
            int testZ = world.Runtime.Stage.ZMin + 20;
            LF2Character catcher = CreateCharacter(
                "SelfCheck_PreCandidateCpointCatcher", 1, BuildCatchingFrames());
            LF2CharacterData victimData = BuildVictimFrames();
            LF2FrameData syncedVictimFrame = victimData.frames.Find(frame => frame.frameId == 131);
            InteractionArea syncedVictimItr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            syncedVictimItr.x = 14;
            syncedVictimItr.y = 49;
            syncedVictimFrame.itrs.Add(syncedVictimItr);
            var victim = new CandidatePassProbeCharacter();
            victim.BindData("SelfCheck_PreCandidateCpointVictim", 2, victimData);
            LF2Character target = CreateInteractionCharacter(
                "SelfCheck_PreCandidateCpointTarget",
                37,
                BuildInteractionVictimData("SelfCheck_PreCandidateCpointTarget", 37));

            catcher.SetRuntimeSlotIndex(0);
            victim.SetRuntimeSlotIndex(1);
            target.SetRuntimeSlotIndex(2);
            world.Register(catcher);
            world.Register(victim);
            world.Register(target);

            catcher.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            catcher.SwitchDir("right");
            victim.SwitchDir("right");
            catcher.Runtime.SetPosition(50.0, 12.0, testZ + 1);
            catcher.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(-500.0, 20.0, testZ);
            victim.Runtime.SyncIntegerPosition();
            target.Runtime.SetPosition(56.0, 20.0, testZ);
            target.Runtime.SyncIntegerPosition();
            catcher.Catching = victim;
            victim.Catching = catcher;
            catcher.CaughtSlotIndex = 1;
            victim.CatcherSlotIndex = 0;
            catcher.FrameDelay = 1;
            victim.FrameDelay = 1;

            catcher.Team = 1;
            catcher.RelationTeam = 1;
            victim.Team = 1;
            victim.RelationTeam = 1;
            target.Team = 2;
            target.RelationTeam = 2;
            victim.ExpectedCandidateTarget = target;

            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.RunReleaseTick(1);

            Expect(victim.PostInteractionObserved && victim.ObservedFrame == 131 &&
                   Nearly(victim.ObservedX, 56.0) && Nearly(victim.ObservedY, 20.0) &&
                   Nearly(victim.ObservedZ, testZ),
                "BATTLE-AUDIT3-07: cpoint held position/frame sync must be visible at candidate consumption; " +
                $"actual=observed:{victim.PostInteractionObserved},frame:{victim.ObservedFrame}," +
                $"x:{victim.ObservedX},y:{victim.ObservedY},z:{victim.ObservedZ}");
            Expect(victim.ObservedCaughtSlot == 0 && catcher.CaughtSlotIndex == 1 &&
                   victim.CandidateContainsExpectedTarget,
                $"BATTLE-AUDIT3-07: cpoint links and held sync must be visible to snapshot/collect in the same tick; " +
                $"observedCaughtSlot={victim.ObservedCaughtSlot}, catcherCaughtSlot={catcher.CaughtSlotIndex}, " +
                $"candidateContainsExpected={victim.CandidateContainsExpectedTarget}");
        }

        private static void CheckReleaseTickZClampPrecedesCandidates()
        {
            var world = new SimulationWorld();
            world.RefreshStageRuntimeSnapshotFromScene();
            int stageZMin = world.Runtime.Stage.ZMin;

            InteractionArea itr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            LF2FrameData attackerFrame = InteractionFrame(itr);
            attackerFrame.wait = 100;
            var attacker = new CandidatePassProbeCharacter();
            attacker.BindData("SelfCheck_PreCandidateClampAttacker", 1, new LF2CharacterData
            {
                name = "SelfCheck_PreCandidateClampAttacker",
                frames = new List<LF2FrameData> { attackerFrame },
            });
            LF2Character target = CreateInteractionCharacter(
                "SelfCheck_PreCandidateClampTarget",
                37,
                BuildInteractionVictimData("SelfCheck_PreCandidateClampTarget", 37));
            attacker.SetRuntimeSlotIndex(0);
            target.SetRuntimeSlotIndex(1);
            world.Register(attacker);
            world.Register(target);

            attacker.Team = 1;
            attacker.RelationTeam = 1;
            target.Team = 2;
            target.RelationTeam = 2;
            attacker.Runtime.SetPosition(0.0, 0.0, stageZMin - 50.0);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SetPosition(0.0, 0.0, stageZMin);
            target.Runtime.SyncIntegerPosition();
            attacker.FrameDelay = 1;
            target.FrameDelay = 1;
            attacker.ExpectedCandidateTarget = target;

            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.RunReleaseTick(1);

            Expect(attacker.PostInteractionObserved && Nearly(attacker.ObservedZ, stageZMin),
                $"BATTLE-AUDIT3-08: character Z must be clamped before candidate consumption; " +
                $"observed={attacker.ObservedZ}, zMin={stageZMin}");
            Expect(attacker.CandidateContainsExpectedTarget,
                "BATTLE-AUDIT3-08: candidate collection must use the pre-collect clamped character Z");
        }

        private static void RunGenericHeldStep12Case(
            string label,
            LF2ObjectType heldType,
            WeaponPoint holderWPoint,
            Action<HeldStep12SelfCheckEntity, WeaponActResult> verify)
        {
            var world = new SimulationWorld();
            FlowSelfCheckEntity holder = CreateFlowSelfCheckEntity(
                $"SelfCheck_HeldStep12_{label}_Holder",
                LF2ObjectType.Other,
                LF2States.Standing,
                0,
                0,
                0,
                10);
            holder.Frame.D.wpoints = new List<WeaponPoint> { holderWPoint };
            var held = new HeldStep12SelfCheckEntity(heldType);
            held.BindData($"SelfCheck_HeldStep12_{label}_Held", 991, damagedFrame: 20);
            held.SetRuntimeSlotIndex(50);
            world.Register(holder);
            world.Register(held);

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 50;
            holder.Runtime.HeldWeaponStableId = 50;
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = 10;
            held.GrabbedBy = -1;

            bool ran = LF2HeldObjectRuntime.RunStep12(holder, held, holderWPoint, out WeaponActResult result);

            Expect(ran, $"BATTLE-AUDIT3-12: generic held step12 fixture {label} must enter production runtime");
            verify(held, result);
        }

        private static void CheckValidatePositiveLinksMatrix()
        {
            var world = new SimulationWorld();

            LF2Character characterHolder = CreateCharacter(
                "SelfCheck_PositiveLink_CharacterHolder", 1, BuildCatchingFrames());
            characterHolder.SetRuntimeSlotIndex(0);
            FlowSelfCheckEntity edgeTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_EdgeTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 399);
            characterHolder.Runtime.LinkState = 1;
            characterHolder.Runtime.TargetSlotIndex = 399;
            characterHolder.Runtime.HeldWeaponStableId = 77;
            edgeTarget.Runtime.HolderStableId = 0;
            edgeTarget.Runtime.LinkState = -2;

            FlowSelfCheckEntity nonCharacterHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_NonCharacterHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 23);
            FlowSelfCheckEntity neutralTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_NeutralTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 3);
            nonCharacterHolder.Runtime.LinkState = 2;
            nonCharacterHolder.Runtime.TargetSlotIndex = 3;
            nonCharacterHolder.Runtime.HeldWeaponStableId = 88;
            neutralTarget.Runtime.HolderStableId = 23;
            neutralTarget.Runtime.LinkState = 0;

            FlowSelfCheckEntity positiveTargetHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_PositiveTargetHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 31);
            FlowSelfCheckEntity positiveTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_PositiveTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 32);
            FlowSelfCheckEntity positiveTargetChild = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_PositiveTargetChild", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 33);
            positiveTargetHolder.Runtime.LinkState = 3;
            positiveTargetHolder.Runtime.TargetSlotIndex = 32;
            positiveTarget.Runtime.HolderStableId = 31;
            positiveTarget.Runtime.LinkState = 5;
            positiveTarget.Runtime.TargetSlotIndex = 33;
            positiveTargetChild.Runtime.HolderStableId = 32;

            FlowSelfCheckEntity negativeTargetHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_NegativeTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 7);
            negativeTargetHolder.Runtime.LinkState = 1;
            negativeTargetHolder.Runtime.TargetSlotIndex = -1;
            negativeTargetHolder.Runtime.HeldWeaponStableId = 101;

            FlowSelfCheckEntity highTargetHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_HighTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 8);
            highTargetHolder.Runtime.LinkState = 1;
            highTargetHolder.Runtime.TargetSlotIndex = 400;
            highTargetHolder.Runtime.HeldWeaponStableId = 102;

            FlowSelfCheckEntity mismatchHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_MismatchHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 200);
            FlowSelfCheckEntity mismatchTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_MismatchTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 201);
            mismatchHolder.Runtime.LinkState = 1;
            mismatchHolder.Runtime.TargetSlotIndex = 201;
            mismatchHolder.Runtime.HeldWeaponStableId = 103;
            mismatchTarget.Runtime.HolderStableId = 199;

            FlowSelfCheckEntity zeroLink = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_Zero", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 300);
            zeroLink.Runtime.LinkState = 0;
            zeroLink.Runtime.TargetSlotIndex = 400;
            zeroLink.Runtime.HeldWeaponStableId = 104;

            FlowSelfCheckEntity negativeLink = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_Negative", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 301);
            negativeLink.Runtime.LinkState = -2;
            negativeLink.Runtime.TargetSlotIndex = 400;
            negativeLink.Runtime.HeldWeaponStableId = 105;

            world.Register(nonCharacterHolder);
            world.Register(neutralTarget);
            world.Register(highTargetHolder);
            world.Register(negativeTargetHolder);
            world.Register(characterHolder);
            world.Register(edgeTarget);
            world.Register(mismatchTarget);
            world.Register(mismatchHolder);
            world.Register(positiveTarget);
            world.Register(positiveTargetChild);
            world.Register(positiveTargetHolder);
            world.Register(negativeLink);
            world.Register(zeroLink);

            world.ValidateHeldLinksAll(1);

            Expect(characterHolder.Runtime.LinkState == 1 &&
                   characterHolder.Runtime.TargetSlotIndex == 399 &&
                   characterHolder.Runtime.HeldWeaponStableId == 77,
                "positive link slot 0->399 must remain valid for a character holder");
            Expect(nonCharacterHolder.Runtime.LinkState == 2 &&
                   nonCharacterHolder.Runtime.TargetSlotIndex == 3 &&
                   nonCharacterHolder.Runtime.HeldWeaponStableId == 88,
                "positive link validation must include non-character holders");
            Expect(positiveTargetHolder.Runtime.LinkState == 3,
                "target positive LinkState must not invalidate an otherwise valid relation");
            Expect(edgeTarget.Runtime.LinkState == -2 && neutralTarget.Runtime.LinkState == 0 &&
                   positiveTarget.Runtime.LinkState == 5,
                "target LinkState sign must be irrelevant to positive holder validation");

            Expect(negativeTargetHolder.Runtime.LinkState == 0 &&
                   negativeTargetHolder.Runtime.TargetSlotIndex == -1 &&
                   negativeTargetHolder.Runtime.HeldWeaponStableId == -1,
                "target slot -1 must clear all forward holder relation fields");
            Expect(highTargetHolder.Runtime.LinkState == 0 &&
                   highTargetHolder.Runtime.TargetSlotIndex == -1 &&
                   highTargetHolder.Runtime.HeldWeaponStableId == -1,
                "target slot 400 must clear all forward holder relation fields");
            Expect(mismatchHolder.Runtime.LinkState == 0 &&
                   mismatchHolder.Runtime.TargetSlotIndex == -1 &&
                   mismatchHolder.Runtime.HeldWeaponStableId == -1 &&
                   mismatchTarget.Runtime.HolderStableId == 199,
                "holder mismatch must clear the forward relation without erasing another holder's reverse field");
            Expect(zeroLink.Runtime.LinkState == 0 && zeroLink.Runtime.TargetSlotIndex == 400 &&
                   zeroLink.Runtime.HeldWeaponStableId == 104,
                "link==0 entities must not be processed");
            Expect(negativeLink.Runtime.LinkState == -2 && negativeLink.Runtime.TargetSlotIndex == 400 &&
                   negativeLink.Runtime.HeldWeaponStableId == 105,
                "link<0 entities must not be processed");

            var inactiveWorld = new SimulationWorld();
            FlowSelfCheckEntity inactiveHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_InactiveHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 12);
            FlowSelfCheckEntity inactiveTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_InactiveTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 13);
            inactiveHolder.Runtime.LinkState = 1;
            inactiveHolder.Runtime.TargetSlotIndex = 13;
            inactiveHolder.Runtime.HeldWeaponStableId = 106;
            inactiveTarget.Runtime.HolderStableId = 12;
            inactiveTarget.Runtime.LinkState = -2;
            inactiveTarget.Runtime.TargetSlotIndex = 12;
            inactiveTarget.Runtime.HeldWeaponStableId = 107;
            inactiveTarget.GrabbedBy = 12;
            inactiveWorld.Register(inactiveTarget);
            inactiveWorld.Register(inactiveHolder);
            inactiveWorld.Unregister(inactiveTarget);
            inactiveWorld.ValidateHeldLinksAll(1);
            Expect(inactiveHolder.Runtime.LinkState == 0 &&
                   inactiveHolder.Runtime.TargetSlotIndex == -1 &&
                   inactiveHolder.Runtime.HeldWeaponStableId == -1,
                "inactive target must clear all forward holder relation fields");
            Expect(inactiveTarget.Runtime.LinkState == -2 &&
                   inactiveTarget.Runtime.HolderStableId == 12 &&
                   inactiveTarget.Runtime.TargetSlotIndex == 12 &&
                   inactiveTarget.Runtime.HeldWeaponStableId == 107 &&
                   inactiveTarget.GrabbedBy == 12,
                "inactive target reverse relation fields must remain unchanged when the holder is invalidated");
        }

        private static void AdvanceFlowToEvenToggle(SimulationWorld world)
        {
            world.AdvanceBattleFlowTick(1);
            world.AdvanceBattleFlowTick(2);
        }

        private static void CheckPreFrameXBoundsMatrix()
        {
            const float baseStageWidth = 800f;
            const int xMaxOverride = 500;

            FlowSelfCheckEntity ordinary = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsOrdinary", LF2ObjectType.Character, 0, 1, -1, 200, 0);
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) &&
                   ordinary.Runtime.X == 0f && ordinary.Runtime.XInt == 0,
                "slot<20 ordinary character must clamp its lower X bound to zero");
            ordinary.Runtime.X = 700f;
            ordinary.Runtime.HitStop = 0;
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && ordinary.Runtime.X == 500f,
                "slot<20 ordinary character must apply the phase X override when hit stop is zero");
            ordinary.Runtime.X = 700f;
            ordinary.Runtime.HitStop = 1;
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && ordinary.Runtime.X == 700f,
                "slot<20 ordinary character must ignore the phase X override during hit stop");
            ordinary.Runtime.X = 900f;
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && ordinary.Runtime.X == 800f,
                "base stage width clamp must still run while the phase override is hit-stop gated");

            FlowSelfCheckEntity relationFive = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsRelationFive", LF2ObjectType.Character, 0, 5, -301, 200, 1);
            Expect(!relationFive.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && relationFive.Runtime.X == -300f,
                "RelationTeam 5 character must use the -300 lower X bound");
            relationFive.Runtime.X = 700f;
            Expect(!relationFive.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && relationFive.Runtime.X == 700f,
                "RelationTeam 5 character must ignore the phase X override");

            FlowSelfCheckEntity reservedSlot = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsReserved", LF2ObjectType.Character, 0, 1, -101, 200, 20);
            Expect(!reservedSlot.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && reservedSlot.Runtime.X == -100f,
                "slot>=20 character must use the -100 lower X bound");
            reservedSlot.Runtime.X = 901f;
            Expect(!reservedSlot.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && reservedSlot.Runtime.X == 900f,
                "slot>=20 character must use base stage width plus 100 and ignore phase override");

            FlowSelfCheckEntity type3LowerEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsType3LowerEdge", LF2ObjectType.SpecialAttack, 0, 1, -300, 200, 2);
            FlowSelfCheckEntity type3UpperEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsType3UpperEdge", LF2ObjectType.SpecialAttack, 0, 1, 1100, 200, 3);
            Expect(!type3LowerEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) &&
                   !type3UpperEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "type3 exact -300/base+300 edges must remain active");
            FlowSelfCheckEntity type3Outside = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsType3Outside", LF2ObjectType.SpecialAttack, 0, 1, 1101, 200, 4);
            Expect(type3Outside.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "type3 outside base stage width plus 300 must be freed");

            FlowSelfCheckEntity oid122 = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsOid122", LF2ObjectType.LightWeapon, 0, 1, 0, 200, 5);
            oid122.ObjectId = 122;
            oid122.Unk344 = 1;
            Expect(!oid122.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && oid122.Runtime.X == 10f,
                "oid122 with Unk344>0 must clamp to the 10 lower X bound");
            FlowSelfCheckEntity oid123 = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsOid123", LF2ObjectType.Other, 0, 1, 800, 200, 6);
            oid123.ObjectId = 123;
            oid123.Unk344 = 2;
            Expect(!oid123.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && oid123.Runtime.X == 790f,
                "oid123 with Unk344>0 must clamp to base stage width minus 10");
            FlowSelfCheckEntity wrongWeaponField = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsWrongWeaponField", LF2ObjectType.LightWeapon, 0, 1, 5, 200, 7);
            wrongWeaponField.ObjectId = 122;
            wrongWeaponField.Unk344 = 0;
            wrongWeaponField.Runtime.WeaponFlightCounter = 100;
            Expect(!wrongWeaponField.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && wrongWeaponField.Runtime.X == 5f,
                "oid122 bounds must use Unk344 rather than WeaponFlightCounter");

            FlowSelfCheckEntity groundedLowerEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsGroundedLowerEdge", LF2ObjectType.Other, 0, 1, 0, 200, 8);
            groundedLowerEdge.Runtime.YInt = 0;
            FlowSelfCheckEntity groundedUpperEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsGroundedUpperEdge", LF2ObjectType.Other, 0, 1, 800, 200, 9);
            groundedUpperEdge.Runtime.YInt = 0;
            Expect(!groundedLowerEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) &&
                   !groundedUpperEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "ordinary grounded non-character exact stage edges must remain active");
            FlowSelfCheckEntity groundedOutside = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsGroundedOutside", LF2ObjectType.Other, 0, 1, -1, 200, 10);
            groundedOutside.Runtime.YInt = 0;
            Expect(groundedOutside.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "ordinary grounded non-character outside the base stage must be freed");
            FlowSelfCheckEntity airborneOutside = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsAirborneOutside", LF2ObjectType.Other, 0, 1, 900, 200, 11);
            airborneOutside.Runtime.YInt = 1;
            Expect(!airborneOutside.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && airborneOutside.Runtime.X == 900f,
                "ordinary airborne non-character outside the base stage must remain active");

            FlowSelfCheckEntity truncation = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsXInt", LF2ObjectType.Character, 0, 1, 123, 200, 12);
            truncation.Runtime.X = 123.75f;
            Expect(!truncation.ApplyPreFrameXBounds(baseStageWidth, 0) && truncation.Runtime.XInt == 123,
                "surviving PreFrame X bounds must mirror truncated XInt");

            var transformedCharacter = new BoundsSelfCheckCharacter(LF2ObjectType.SpecialAttack);
            transformedCharacter.BindData("SelfCheck_BoundsClrCharacterDatType3", 912, BuildCatchingFrames());
            transformedCharacter.SetRuntimeSlotIndex(13);
            transformedCharacter.Runtime.X = 1101f;
            Expect(transformedCharacter.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "current DAT type must select type3 bounds even for a character CLR shell");

            var world = new SimulationWorld();
            FlowSelfCheckEntity hitStopped = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsWorldBaseWidth", LF2ObjectType.Character, 0, 1, 700, 200, 0);
            hitStopped.Runtime.HitStop = 1;
            world.Register(hitStopped);
            world.Runtime.Stage.ApplyPhaseBound(xMaxOverride);
            Expect(world.Runtime.Stage.StageWidthPx == xMaxOverride,
                "phase setup must retain the existing active StageWidthPx contract");
            world.ApplyPreFrameBoundsAll();
            Expect(hitStopped.Runtime.X == 700f && world.Runtime.Stage.BaseStageWidthPx >= 794,
                "PreFrame entity bounds must use base stage width separately from active phase width");

            FlowSelfCheckEntity worldFreed = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsWorldFree", LF2ObjectType.SpecialAttack, 0, 1, 5000, 200, 14);
            world.Register(worldFreed);
            world.ApplyPreFrameBoundsAll();
            var entities = new List<LF2Entity>();
            world.GetAllEntities(entities);
            Expect(!entities.Contains(worldFreed),
                "PreFrame out-of-bounds free must remove the entity through the world lifecycle");
        }

        private static FlowSelfCheckEntity CreateFlowSelfCheckEntity(
            string name,
            LF2ObjectType objectType,
            int state,
            int relationTeam,
            int x,
            int z,
            int slot)
        {
            var entity = new FlowSelfCheckEntity(objectType);
            entity.BindData(name, 900 + slot, new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state, 0, 0, 39, 79),
                },
            });
            entity.RelationTeam = relationTeam;
            entity.SetRuntimeSlotIndex(slot);
            entity.Runtime.SetPosition(x, 0f, z);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.Dir = "right";
            return entity;
        }

        private static void CheckState0BelowGroundFrame212PreservesAttackingCounter()
        {
            // BMD-023-extended: standing-state-but-below-ground branch
            // (LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance, frame state 0 + Y < 0)
            // must mirror baseline FrameTick.cs:67-76 (SetFrameImmediate(entity, 212)).
            // Baseline's SetFrameImmediate writes Frame + FrameWaitCounter only, never Attacking.
            // Unity's old ImmediateFrame path zeroed AttackingCounter as a side effect
            // (LF2Entity.cs:824). Verify the replacement path preserves AttackingCounter.
            var character = CreateCharacter("SelfCheck_State0BelowGround", 1, BuildCatchingFrames());

            // BuildCatchingFrames frame 0 is state=0, wait=0, next=0 — standing on ground.
            // Drive it to frame 0 explicitly so Frame.D.state resolves correctly.
            character.ImmediateFrame(0);

            // Below-ground runtime state: standing frame, but Y < 0 (drops through ground).
            character.Runtime.YInt = -10;

            // Stash an arbitrary AttackingCounter; the fix must preserve this through the tick.
            const int attackingBefore = 7;
            character.AttackingCounter = attackingBefore;

            // OnFrameTickBeforeWaitAdvance is the public entry that routes through
            // ApplyObjectSpecificFrameTickBeforeWaitAdvance on LF2Character.
            character.OnFrameTickBeforeWaitAdvance(0);

            Expect(character.Frame != null && character.Frame.N == 212,
                "state=0 + Y<0 分支应强制切到 212 空中跳跃帧");
            Expect(character.AttackingCounter == attackingBefore,
                "BMD-023-extended: 切帧必须保留 AttackingCounter，" +
                "ImmediateFrame 路径会在 LF2Entity.cs:824 将其清零，违反 baseline parity");
        }

        private static void CheckSimulationPassesImmediateFrameDoesNotZeroAttacking()
        {
            // BMD-023: SimulationWorld.Passes.partial.cs state=500/501 transform branches
            // used to call entity.ImmediateFrame(N), which zeros AttackingCounter as a side
            // effect (LF2Entity.cs:824). Baseline FrameTick.cs:67-76 SetFrameImmediate only
            // writes Frame + FrameWaitCounter. The fix routes through
            // DirectWriteFramePreserveWaitCounter, which delegates to SetFrameTickDirect and
            // leaves AttackingCounter alone.
            //
            // We test the replacement path end-to-end: build an entity, set Frame.N to a
            // state=500 frame, stash AttackingCounter, call the replacement, and assert
            // AttackingCounter survives while Frame advances. This covers all three call
            // sites (SimulationWorld.Passes.partial.cs:140/:168/:186) since they share the
            // same SetFrameTickDirect backing.
            var character = CreateCharacter("SelfCheck_PassesAttacking", 1, BuildCatchingFrames());
            character.ImmediateFrame(0);
            const int attackingBefore = 11;
            character.AttackingCounter = attackingBefore;

            character.DirectWriteFramePreserveWaitCounter(212);

            Expect(character.Frame != null && character.Frame.N == 212,
                "BMD-023: DirectWriteFramePreserveWaitCounter 必须把 Frame.N 写到目标帧 212");
            Expect(character.AttackingCounter == attackingBefore,
                "BMD-023: state=500/501 修复点必须保留 AttackingCounter，" +
                "ImmediateFrame 路径会在 LF2Entity.cs:824 将其清零，违反 baseline parity");
        }

        private static void CheckArestCooldownRule()
        {
            Expect(LF2Entity.ResolveArestCooldown(0, 0) == 4, "arest (0,0) must resolve to 4");
            Expect(LF2Entity.ResolveArestCooldown(3, 0) == 4, "arest (3,0) must resolve to 4");
            Expect(LF2Entity.ResolveArestCooldown(4, 0) == 4, "arest (4,0) must remain 4");
            Expect(LF2Entity.ResolveArestCooldown(15, 0) == 15, "arest (15,0) must remain 15");
            Expect(LF2Entity.ResolveArestCooldown(0, 1) == 0, "arest (0,1) must remain 0");
            Expect(LF2Entity.ResolveArestCooldown(2, 20) == 2, "arest (2,20) must remain 2");
            Expect(LF2Entity.ResolveArestCooldown(15, 20) == 15, "arest (15,20) must remain 15");
        }

        private static void CheckFrameTickDefendLockTail()
        {
            var character = CreateCharacter("SelfCheck_FrameTickDefendLock", 1, new LF2CharacterData
            {
                name = "SelfCheckFrameTickDefendLock",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 5, 0, 39, 79),
                    Frame(1, 0, 5, 1, 39, 79),
                    Frame(110, 0, 5, 110, 39, 79),
                    Frame(114, 0, 5, 114, 39, 79),
                }
            });

            character.ImmediateFrame(110);
            character.Runtime.CdDefendLock = 0;
            character.SimFrameTick(1);
            Expect(character.Runtime.CdDefendLock == 3,
                "frame_tick tail must set CdDefendLock=3 on frame 110");

            character.ImmediateFrame(114);
            character.Runtime.CdDefendLock = 0;
            character.SimFrameTick(2);
            Expect(character.Runtime.CdDefendLock == 3,
                "frame_tick tail must set CdDefendLock=3 on frame 114");

            character.ImmediateFrame(110);
            character.Frame.D.cpoint = new CatchPoint { kind = 2 };
            character.Runtime.CdDefendLock = 0;
            character.SimFrameTick(3);
            Expect(character.Runtime.CdDefendLock == 0,
                "frame_tick cpoint kind=2 early return must not set CdDefendLock on frame 110");
            character.Frame.D.cpoint = null;

            character.ImmediateFrame(1);
            character.Runtime.CdDefendLock = 7;
            character.SimFrameTick(4);
            Expect(character.Runtime.CdDefendLock == 7,
                "frame_tick tail must not change CdDefendLock on an ordinary frame");

            var world = new SimulationWorld();
            world.Register(character);
            character.Runtime.CdDefendLock = 3;
            world.VrestTickAll(5);
            Expect(character.Runtime.CdDefendLock == 2,
                "cooldowns pass must decrement CdDefendLock from 3 to 2");
            world.VrestTickAll(6);
            Expect(character.Runtime.CdDefendLock == 1,
                "cooldowns pass must decrement CdDefendLock from 2 to 1");
            world.VrestTickAll(7);
            Expect(character.Runtime.CdDefendLock == 0,
                "cooldowns pass must decrement CdDefendLock from 1 to 0");
            world.VrestTickAll(8);
            Expect(character.Runtime.CdDefendLock == 0,
                "cooldowns pass must keep CdDefendLock at zero");
        }

        private static void CheckKind0HitRecords()
        {
            LF2CharacterData frameData = new LF2CharacterData
            {
                name = "SelfCheckKind0HitRecord",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 40, 50),
                }
            };
            var attacker = CreateCharacter("SelfCheck_HitRecordAttacker", 1, frameData);
            var victim = CreateCharacter("SelfCheck_HitRecordVictim", 2, frameData);
            attacker.SwitchDir("right");
            attacker.Runtime.XInt = 100;
            attacker.Runtime.YInt = -20;
            victim.Runtime.XInt = 120;
            victim.Runtime.YInt = -10;

            attacker.Runtime.ZInt = 30;
            victim.Runtime.ZInt = 20;
            attacker.SetRuntimeSlotIndex(8);
            victim.SetRuntimeSlotIndex(3);
            victim.RecordKind0Hit(attacker, new InteractionArea
            {
                kind = 0, x = 5, y = 7, w = 30, h = 20, fall = 61, effect = 0
            });
            Expect(attacker.HitRecordCount == 1 && victim.HitRecordCount == 0,
                "kind0 hit record must use the entity with the larger ZInt as owner");
            Expect(attacker.GetHitRecordAge(0) == 0,
                "effect=0 and fall>60 must create timer 0");
            Expect(attacker.GetHitRecordX(0) >= 91 && attacker.GetHitRecordX(0) <= 99,
                "kind0 hit record X must use the integer frame/itr formula plus [-4,4] RNG");
            Expect(attacker.GetHitRecordZ(0) >= -27 && attacker.GetHitRecordZ(0) <= -19,
                "kind0 hit record Z must use the integer frame/itr formula plus [-4,4] RNG");

            attacker.Runtime.ZInt = 10;
            victim.Runtime.ZInt = 20;
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 60, effect = 0 });
            Expect(victim.HitRecordCount == 1 && victim.GetHitRecordAge(0) == 10,
                "effect=0 and fall<=60 must create timer 10 on the larger-Z victim");

            attacker.Runtime.ZInt = 15;
            victim.Runtime.ZInt = 15;
            attacker.SetRuntimeSlotIndex(9);
            victim.SetRuntimeSlotIndex(2);
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 61, effect = 1 });
            Expect(attacker.HitRecordCount == 2 && attacker.GetHitRecordAge(1) == 20,
                "equal ZInt must use the larger runtime slot owner; effect=1/fall>60 timer must be 20");

            attacker.SetRuntimeSlotIndex(2);
            victim.SetRuntimeSlotIndex(9);
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 60, effect = 1 });
            Expect(victim.HitRecordCount == 2 && victim.GetHitRecordAge(1) == 30,
                "equal ZInt must use the larger runtime slot owner; effect=1/fall<=60 timer must be 30");

            attacker.Runtime.ZInt = 10;
            victim.Runtime.ZInt = 20;
            for (int i = victim.HitRecordCount; i < LF2Entity.MaxHitRecordSlots; i++)
                victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 60, effect = 0 });

            int tailAge = victim.GetHitRecordAge(LF2Entity.MaxHitRecordSlots - 1);
            int tailX = victim.GetHitRecordX(LF2Entity.MaxHitRecordSlots - 1);
            int tailZ = victim.GetHitRecordZ(LF2Entity.MaxHitRecordSlots - 1);
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 61, effect = 1 });
            Expect(victim.HitRecordCount == LF2Entity.MaxHitRecordSlots,
                "kind0 hit records must not grow beyond 10 slots");
            Expect(victim.GetHitRecordAge(LF2Entity.MaxHitRecordSlots - 1) == tailAge &&
                   victim.GetHitRecordX(LF2Entity.MaxHitRecordSlots - 1) == tailX &&
                   victim.GetHitRecordZ(LF2Entity.MaxHitRecordSlots - 1) == tailZ,
                "a full kind0 hit-record owner must leave its tail record unchanged");
        }

        private static void CheckAudit4AttackExemptAndStandardHitContracts()
        {
            InteractionArea attackItr = MakeInteractionItr(kind: 0, vrest: 0, injury: 1, dvx: 1);

            var cleanupWorld = new SimulationWorld();
            var withItr = new FlowSelfCheckEntity(LF2ObjectType.Other);
            withItr.BindData("SelfCheck_Audit4AttackExemptItr", 940, new LF2CharacterData
            {
                name = "SelfCheck_Audit4AttackExemptItr",
                frames = new List<LF2FrameData> { InteractionFrame(attackItr) },
            });
            var withoutItr = new FlowSelfCheckEntity(LF2ObjectType.Other);
            withoutItr.BindData("SelfCheck_Audit4AttackExemptNoItr", 941, new LF2CharacterData
            {
                name = "SelfCheck_Audit4AttackExemptNoItr",
                frames = new List<LF2FrameData> { InteractionFrame(null) },
            });
            cleanupWorld.Register(withItr);
            cleanupWorld.Register(withoutItr);
            withItr.AttackExempt = 7;
            withoutItr.AttackExempt = 7;
            cleanupWorld.VrestTickAll(1);
            Expect(withItr.AttackExempt == 7 && withoutItr.AttackExempt == 0,
                "BATTLE-AUDIT4-01: AttackExempt cleanup must inspect current-frame itr presence");

            LF2FrameData holderFrame = InteractionFrame(null);
            holderFrame.wpoints.Add(new WeaponPoint { attacking = 1 });
            var holder = new FlowSelfCheckEntity(LF2ObjectType.Character);
            holder.BindData("SelfCheck_Audit4AttackExemptHolder", 942, new LF2CharacterData
            {
                name = "SelfCheck_Audit4AttackExemptHolder",
                frames = new List<LF2FrameData> { holderFrame },
            });
            LF2FrameData heldFrame = InteractionFrame(attackItr);
            heldFrame.state = LF2States.WeaponOnHand;
            var held = new FlowSelfCheckEntity(LF2ObjectType.LightWeapon);
            held.BindData("SelfCheck_Audit4AttackExemptHeld", 943, new LF2CharacterData
            {
                name = "SelfCheck_Audit4AttackExemptHeld",
                frames = new List<LF2FrameData> { heldFrame },
            });
            cleanupWorld.Register(holder);
            cleanupWorld.Register(held);
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = holder.Runtime.SlotIndex;
            held.AttackExempt = 6;
            cleanupWorld.VrestTickAll(2);
            Expect(held.AttackExempt == 6,
                "BATTLE-AUDIT4-01: held state1001 itr must remain armed while holder wpoint attacks");
            holder.Frame.D.wpoints[0].attacking = 0;
            cleanupWorld.VrestTickAll(3);
            Expect(held.AttackExempt == 0,
                "BATTLE-AUDIT4-01: held state1001 itr must clear when holder wpoint stops attacking");

            LF2CharacterData hitData = BuildAudit4StandardHitData("SelfCheck_Audit4StandardHit");
            var attacker = new FlowSelfCheckEntity(LF2ObjectType.Other);
            attacker.BindData("SelfCheck_Audit4NonLivingAttacker", 944, hitData);
            LF2Character victim = CreateCharacter("SelfCheck_Audit4RealVictim", 2, hitData);
            attacker.SwitchDir("right");
            victim.SwitchDir("right");
            victim.FallCounter = 20;
            victim.Runtime.SetPosition(-500.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
            bool realHit = victim.Hit(attackItr, attacker, new Vector3(500f, 0f, 0f), default);
            Expect(realHit && attacker.AttackExempt == 4 && victim.FrameDelay == -3 &&
                   victim.Frame.N == LF2StandardFrames.Injured4,
                "BATTLE-AUDIT4-02/07: standard hit must arm non-living attacker, use -3 delay, and derive hurt frame from facing");

            var sharedVictim = new FlowSelfCheckEntity(LF2ObjectType.Character);
            sharedVictim.BindData("SelfCheck_Audit4SharedVictim", 3, hitData);
            sharedVictim.Health.HP = 100;
            sharedVictim.Health.HPBound = 100;
            sharedVictim.FallCounter = 20;
            sharedVictim.SwitchDir("left");
            attacker.AttackExempt = 0;
            attacker.FrameDelay = 0;
            attacker.ItrRest.Reset();
            bool sharedHit = LF2CharacterDatHitResolver.TryResolveHit(
                sharedVictim, attackItr, attacker, Vector3.zero, default);
            Expect(sharedHit && sharedVictim.FrameDelay == -3 &&
                   sharedVictim.Frame.N == LF2StandardFrames.Injured2,
                $"BATTLE-AUDIT4-02/07: shared-DAT standard hit must match real-character delay and facing matrix; " +
                $"resolved={sharedHit}, delay={sharedVictim.FrameDelay}, frame={sharedVictim.Frame.N}, " +
                $"attackerDir={attacker.Runtime.Dir}, victimDir={sharedVictim.Runtime.Dir}");

            AlternateDamageSelfCheckWeapon flyingWeapon = CreateSelfCheckWeapon(
                "SelfCheck_Audit4FlyingWeapon", 100, 1, BuildAlternateDamageWeaponFrames(), 20);
            flyingWeapon.Trans.SetWait(flyingWeapon.Frame.D.wait, 61);
            LF2Character flyingVictim = CreateCharacter(
                "SelfCheck_Audit4FlyingWeaponVictim", 8, hitData);
            flyingVictim.SwitchDir("right");
            bool flyingHit = flyingVictim.Hit(
                new InteractionArea { kind = 0, injury = 1, fall = 1, dvx = 4, arest = 4, vrest = 0 },
                flyingWeapon,
                Vector3.zero,
                default);
            Expect(flyingHit && flyingWeapon.Frame.N >= 0 && flyingWeapon.Frame.N < 16 &&
                   Nearly(flyingWeapon.Runtime.Vx, flyingVictim.KnockbackVx * -0.5) &&
                   Nearly(flyingWeapon.Runtime.Vy, -4.0) && flyingWeapon.Trans.WaitCounter == 61,
                "BATTLE-AUDIT4-03: state1002 standard hit must randomize frame0..15, bounce from victim knockback, and preserve wait");

            var sparkAttacker = CreateCharacter("SelfCheck_Audit4SparkAttacker", 4, hitData);
            var sparkVictim = CreateCharacter("SelfCheck_Audit4SparkVictim", 5, hitData);
            sparkAttacker.Runtime.ZInt = 20;
            sparkVictim.Runtime.ZInt = 10;
            var sparkItr = MakeInteractionItr(kind: 0, vrest: 0, injury: 1, dvx: 1);
            sparkItr.effect = 6;
            bool sparkHit = sparkVictim.Hit(sparkItr, sparkAttacker, Vector3.zero, default);
            Expect(sparkHit && sparkAttacker.HitRecordCount + sparkVictim.HitRecordCount == 1,
                "BATTLE-AUDIT4-11: effect6 standard hit must still publish a spark hit record");
        }

        private static LF2CharacterData BuildAudit4StandardHitData(string name)
        {
            var frames = new List<LF2FrameData>
            {
                InteractionFrame(null),
            };
            for (int frameId = LF2StandardFrames.Injured; frameId <= LF2StandardFrames.Injured9; frameId++)
                frames.Add(Frame(frameId, LF2States.Injured, 1, frameId, 0, 0));
            return new LF2CharacterData { name = name, type_sub = 1, frames = frames };
        }

        private static void CheckAudit4FrozenCandidateAndKind3Contracts()
        {
            InteractionArea itr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheck_Audit4FrozenCandidateAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(itr) },
            };
            var world = new SimulationWorld();
            LF2Character attacker = CreateInteractionCharacter(
                "SelfCheck_Audit4FrozenCandidateAttacker", 1, attackerData);
            LF2Character first = CreateInteractionCharacter(
                "SelfCheck_Audit4FrozenCandidateFirst", 2,
                BuildInteractionVictimData("SelfCheck_Audit4FrozenCandidateFirst", 2));
            LF2Character second = CreateInteractionCharacter(
                "SelfCheck_Audit4FrozenCandidateSecond", 3,
                BuildInteractionVictimData("SelfCheck_Audit4FrozenCandidateSecond", 3));
            world.Register(attacker);
            world.Register(first);
            world.Register(second);
            ConfigureCollisionAuditEntity(attacker, 1, 0.0);
            ConfigureCollisionAuditEntity(first, 2, 0.0);
            ConfigureCollisionAuditEntity(second, 2, 0.0);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            first.Runtime.SetPosition(10000.0, 0.0, 0.0);
            first.Runtime.SyncIntegerPosition();
            first.RelationTeam = attacker.RelationTeam;
            world.PostInteractionTickAll(1);
            world.EndCollisionCandidateConsumption();
            Expect(first.Health.HP < 100 && second.Health.HP < 100,
                $"BATTLE-AUDIT4-02: frozen candidates must not be re-filtered by later geometry/team mutation and must continue across targets; " +
                $"firstHp={first.Health.HP}, secondHp={second.Health.HP}");

            LF2FrameData attacker0 = Frame(0, LF2States.Standing, 5, 0, 10, 20);
            LF2FrameData attacker297 = Frame(297, LF2States.Catching, 5, 297, 10, 20,
                new CatchPoint { x = 7, y = 11, kind = 1 });
            LF2FrameData victim0 = Frame(0, LF2States.Standing, 6, 0, 8, 16);
            LF2FrameData victim130 = Frame(130, LF2States.BeingCaught, 6, 130, 8, 16,
                new CatchPoint { x = 5, y = 9, kind = 2 });
            LF2Character grabber = CreateCharacter("SelfCheck_Audit4NarutoGrabber", 6, new LF2CharacterData
            {
                name = "SelfCheck_Audit4NarutoGrabber",
                frames = new List<LF2FrameData> { attacker0, attacker297 },
            });
            LF2Character grabbed = CreateCharacter("SelfCheck_Audit4NarutoGrabbed", 7, new LF2CharacterData
            {
                name = "SelfCheck_Audit4NarutoGrabbed",
                frames = new List<LF2FrameData> { victim0, victim130 },
            });
            var grabWorld = new SimulationWorld();
            grabWorld.Register(grabber);
            grabWorld.Register(grabbed);
            grabber.Runtime.SetPosition(100.0, 30.0, 0.0);
            grabbed.Runtime.SetPosition(140.0, 0.0, 0.0);
            grabber.Runtime.SyncIntegerPosition();
            grabbed.Runtime.SyncIntegerPosition();
            grabber.Trans.SetWait(grabber.Frame.D.wait, 41);
            grabbed.Trans.SetWait(grabbed.Frame.D.wait, 52);
            var heldMarker = new FlowSelfCheckEntity(LF2ObjectType.LightWeapon);
            heldMarker.BindData("SelfCheck_Audit4GrabbedHeldMarker", 945, BuildAudit4StandardHitData("SelfCheck_Audit4GrabbedHeldMarker"));
            grabWorld.Register(heldMarker);
            grabbed.HoldWeapon(heldMarker);
            int heldSlot = grabbed.Runtime.HeldWeaponStableId;

            bool grabbedResult = LF2CharacterInteractionResolver.TryApplyKind3Grab(grabber, grabbed,
                new InteractionArea { kind = 3, catchingact = new[] { 297 }, caughtact = new[] { 130 } });
            Expect(grabbedResult && grabber.Frame.N == 297 && grabbed.Frame.N == 130 &&
                   grabber.Trans.WaitCounter == 41 && grabbed.Trans.WaitCounter == 52,
                "BATTLE-AUDIT4-NARUTO: kind3 must raw-write 297/130 while preserving wait counters");
            Expect(grabber.CaughtSlotIndex == grabbed.Runtime.SlotIndex &&
                   grabbed.CatcherSlotIndex == grabber.Runtime.SlotIndex &&
                   grabbed.Runtime.HeldWeaponStableId == heldSlot,
                "BATTLE-AUDIT4-NARUTO: kind3 must establish runtime links without dropping victim weapon");

            var sharedGrabbed = new FlowSelfCheckEntity(LF2ObjectType.Character);
            sharedGrabbed.BindData("SelfCheck_Audit4SharedGrabbed", 946, new LF2CharacterData
            {
                name = "SelfCheck_Audit4SharedGrabbed",
                frames = new List<LF2FrameData> { victim0, victim130 },
            });
            grabWorld.Register(sharedGrabbed);
            sharedGrabbed.Runtime.SetPosition(150.0, 0.0, 0.0);
            sharedGrabbed.Runtime.SyncIntegerPosition();
            grabber.CaughtSlotIndex = -1;
            bool sharedGrab = LF2CharacterInteractionResolver.TryApplyKind3Grab(grabber, sharedGrabbed,
                new InteractionArea { kind = 3, catchingact = new[] { 297 }, caughtact = new[] { 130 } });
            Expect(sharedGrab && sharedGrabbed.Frame.N == 130 &&
                   sharedGrabbed.CatcherSlotIndex == grabber.Runtime.SlotIndex,
                "BATTLE-AUDIT4-NARUTO: kind3 must accept a current character-DAT shell");
        }

        private static void CheckAudit4ArchitectDefectContracts()
        {
            CheckAudit4SpecialAttackFrozenCandidateContracts();
            CheckAudit4CurrentDatDispatchContracts();
            CheckAudit4PendingFrameSoundContracts();
        }

        private static void CheckAudit4SpecialAttackFrozenCandidateContracts()
        {
            InteractionArea itr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheck_Audit4SpecialFrozenAttacker",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { InteractionFrame(itr) },
            };
            var world = new SimulationWorld();
            var attacker = new Audit4SelfCheckSpecialAttack();
            attacker.BindData("SelfCheck_Audit4SpecialFrozenAttacker", 210, attackerData);
            LF2Character first = CreateInteractionCharacter(
                "SelfCheck_Audit4SpecialFrozenFirst", 2,
                BuildAudit4StandardHitData("SelfCheck_Audit4SpecialFrozenFirst"));
            LF2Character second = CreateInteractionCharacter(
                "SelfCheck_Audit4SpecialFrozenSecond", 3,
                BuildAudit4StandardHitData("SelfCheck_Audit4SpecialFrozenSecond"));
            world.Register(attacker);
            world.Register(first);
            world.Register(second);
            ConfigureCollisionAuditEntity(attacker, 1, 0.0);
            ConfigureCollisionAuditEntity(first, 2, 0.0);
            ConfigureCollisionAuditEntity(second, 2, 0.0);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            first.Runtime.SetPosition(10000.0, 0.0, 0.0);
            first.Runtime.SyncIntegerPosition();
            first.RelationTeam = attacker.RelationTeam;
            attacker.Team = 0;
            attacker.Interaction();
            world.EndCollisionCandidateConsumption();
            Expect(first.Health.HP < 100 && second.Health.HP < 100,
                $"BATTLE-AUDIT4-SPECIAL-CANDIDATE: SpecialAttack must consume frozen geometry/team and continue across targets; " +
                $"firstHp={first.Health.HP}, secondHp={second.Health.HP}");

            var abortWorld = new SimulationWorld();
            var abortAttacker = new Audit4SelfCheckSpecialAttack();
            abortAttacker.BindData("SelfCheck_Audit4SpecialAbortAttacker", 211, attackerData);
            var abortTarget = new Audit4SelfCheckSpecialAttack();
            abortTarget.BindData("SelfCheck_Audit4SpecialAbortTarget", 300, new LF2CharacterData
            {
                name = "SelfCheck_Audit4SpecialAbortTarget",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { InteractionFrame(null) },
            });
            var afterAbort = new SelfCheckCharacterDatShell();
            afterAbort.BindData(
                "SelfCheck_Audit4SpecialAfterAbort", 4,
                BuildAudit4StandardHitData("SelfCheck_Audit4SpecialAfterAbort"));
            abortWorld.Register(abortAttacker);
            abortWorld.Register(abortTarget);
            abortWorld.Register(afterAbort);
            ConfigureCollisionAuditEntity(abortAttacker, 1, 0.0);
            ConfigureCollisionAuditEntity(abortTarget, 2, 0.0);
            ConfigureCollisionAuditEntity(afterAbort, 2, 0.0);
            abortTarget.ObjectId = 999;
            abortWorld.CaptureCollisionFrameSnapshotsAll();
            abortWorld.CollectCollisionCandidatesAll();
            abortAttacker.Interaction();
            abortWorld.EndCollisionCandidateConsumption();
            Expect(afterAbort.Health.HP == 100,
                "BATTLE-AUDIT4-SPECIAL-CANDIDATE: explicit oid300 abort must stop later frozen hit pairs");
        }

        private static void CheckAudit4CurrentDatDispatchContracts()
        {
            InteractionArea itr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            LF2CharacterData shellData = BuildAudit4StandardHitData("SelfCheck_Audit4DispatchShellData");

            var weaponWorld = new SimulationWorld();
            var weapon = new AlternateDamageSelfCheckWeapon();
            weapon.BindData("SelfCheck_Audit4DispatchWeapon", 100, (int)LF2ObjectType.LightWeapon,
                new LF2CharacterData
                {
                    name = "SelfCheck_Audit4DispatchWeapon",
                    type_sub = (int)LF2ObjectType.LightWeapon,
                    frames = new List<LF2FrameData> { InteractionFrame(itr) },
                }, 0);
            var weaponShell = new FlowSelfCheckEntity(LF2ObjectType.Character);
            weaponShell.BindData("SelfCheck_Audit4WeaponDispatchShell", 401, shellData);
            weaponWorld.Register(weapon);
            weaponWorld.Register(weaponShell);
            ConfigureCollisionAuditEntity(weapon, 1, 0.0);
            ConfigureCollisionAuditEntity(weaponShell, 2, 0.0);
            bool weaponHit = weapon.TryApplyHit(itr, weaponShell);
            Expect(weaponHit && weaponShell.Health.HP < 100,
                $"BATTLE-AUDIT4-CURRENT-DAT: weapon dispatch must hit a current character-DAT shell; hp={weaponShell.Health.HP}");

            var specialWorld = new SimulationWorld();
            var special = new Audit4SelfCheckSpecialAttack();
            special.BindData("SelfCheck_Audit4DispatchSpecial", 214, new LF2CharacterData
            {
                name = "SelfCheck_Audit4DispatchSpecial",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { InteractionFrame(itr) },
            });
            var specialShell = new FlowSelfCheckEntity(LF2ObjectType.Character);
            specialShell.BindData("SelfCheck_Audit4SpecialDispatchShell", 402, shellData);
            specialWorld.Register(special);
            specialWorld.Register(specialShell);
            ConfigureCollisionAuditEntity(special, 1, 0.0);
            ConfigureCollisionAuditEntity(specialShell, 2, 0.0);
            specialWorld.CaptureCollisionFrameSnapshotsAll();
            specialWorld.CollectCollisionCandidatesAll();
            special.Interaction();
            specialWorld.EndCollisionCandidateConsumption();
            Expect(specialShell.Health.HP < 100 && special.Health.HP == 0,
                $"BATTLE-AUDIT4-CURRENT-DAT: SpecialAttack dispatch must hit a current character-DAT shell before oid214 self-destruct; " +
                $"targetHp={specialShell.Health.HP}, attackerHp={special.Health.HP}");
        }

        private static void CheckAudit4PendingFrameSoundContracts()
        {
            LF2FrameData living0 = Frame(0, LF2States.Standing, 100, 0, 0, 0);
            LF2FrameData living1 = Frame(1, LF2States.Standing, 100, 1, 0, 0);
            living1.sound = "SFX_AUDIT4_LIVING";
            LF2Character living = CreateCharacter("SelfCheck_Audit4LivingSound", 1, new LF2CharacterData
            {
                name = "SelfCheck_Audit4LivingSound",
                frames = new List<LF2FrameData> { living0, living1 },
            });
            var world = new SimulationWorld();
            world.Register(living);
            world.AdvanceBattleFlowTick(17);
            living.Runtime.SetPosition(123.0, 0.0, 0.0);
            living.Runtime.SyncIntegerPosition();
            living.OnFrameTransit(1, false);
            Expect(world.PendingSounds.Count == 1 &&
                   world.PendingSounds[0].Cue == "SFX_AUDIT4_LIVING" &&
                   world.PendingSounds[0].WorldX == 123 &&
                   world.PendingSounds[0].Tick == 17,
                "BATTLE-AUDIT4-SOUND: living frame sound must enqueue Cue/WorldX/Tick exactly once");

            LF2FrameData weapon0 = Frame(0, LF2States.WeaponOnHand, 100, 0, 0, 0);
            LF2FrameData weapon1 = Frame(1, LF2States.WeaponOnHand, 100, 1, 0, 0);
            weapon1.sound = "SFX_AUDIT4_WEAPON";
            var weapon = new AlternateDamageSelfCheckWeapon();
            weapon.BindData("SelfCheck_Audit4WeaponSound", 100, (int)LF2ObjectType.LightWeapon,
                new LF2CharacterData
                {
                    name = "SelfCheck_Audit4WeaponSound",
                    type_sub = (int)LF2ObjectType.LightWeapon,
                    frames = new List<LF2FrameData> { weapon0, weapon1 },
                }, 0);
            world.Register(weapon);
            weapon.Runtime.SetPosition(321.0, 0.0, 0.0);
            weapon.Runtime.SyncIntegerPosition();
            weapon.OnFrameTransit(1, false);
            Expect(world.PendingSounds.Count == 2 &&
                   world.PendingSounds[1].Cue == "SFX_AUDIT4_WEAPON" &&
                   world.PendingSounds[1].WorldX == 321 &&
                   world.PendingSounds[1].Tick == 17,
                "BATTLE-AUDIT4-SOUND: weapon frame sound must enqueue Cue/WorldX/Tick exactly once");

            LF2FrameData special0 = Frame(0, LF2States.ProjectileFlying, 100, 0, 0, 0);
            LF2FrameData special1 = Frame(1, LF2States.ProjectileFlying, 100, 1, 0, 0);
            special1.sound = "SFX_AUDIT4_SPECIAL";
            var special = new Audit4SelfCheckSpecialAttack();
            special.BindData("SelfCheck_Audit4SpecialSound", 210, new LF2CharacterData
            {
                name = "SelfCheck_Audit4SpecialSound",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { special0, special1 },
            });
            var specialWorld = new SimulationWorld();
            specialWorld.Register(special);
            specialWorld.AdvanceBattleFlowTick(18);
            special.Runtime.SetPosition(456.0, 0.0, 0.0);
            special.Runtime.SyncIntegerPosition();
            special.OnFrameTransit(1, false, 0);
            Expect(specialWorld.PendingSounds.Count == 1 &&
                   specialWorld.PendingSounds[0].Cue == "SFX_AUDIT4_SPECIAL" &&
                   specialWorld.PendingSounds[0].WorldX == 456 &&
                   specialWorld.PendingSounds[0].Tick == 18,
                "BATTLE-AUDIT4-SOUND: SpecialAttack frame sound must enqueue Cue/WorldX/Tick exactly once");

            var emptyWorld = new SimulationWorld();
            emptyWorld.AdvanceBattleFlowTick(20);
            emptyWorld.QueueSound("SFX_AUDIT4_CLEAR", 7);
            new NTSDBattleTickSystem(emptyWorld).RunReleaseTick(21);
            Expect(emptyWorld.PendingSounds.Count == 0,
                "BATTLE-AUDIT4-SOUND: the next logical tick head must clear prior PendingSounds");
            emptyWorld.QueueSound("SFX_AUDIT4_RESET", 8);
            emptyWorld.ResetRuntimeState();
            Expect(emptyWorld.PendingSounds.Count == 0,
                "BATTLE-AUDIT4-SOUND: ResetRuntimeState must clear PendingSounds");
        }

        private static void CheckAudit4SpecialHeldAndOpointContracts()
        {
            LF2FrameData heldFrame = Frame(0, LF2States.Standing, 7, 0, 0, 0);
            heldFrame.wpoints.Add(new WeaponPoint { x = 0, y = 0 });
            LF2FrameData heldAction = Frame(5, LF2States.Standing, 9, 5, 0, 0);
            heldAction.wpoints.Add(new WeaponPoint { x = 0, y = 0 });
            var heldData = new LF2CharacterData
            {
                name = "SelfCheck_Audit4Held",
                frames = new List<LF2FrameData> { heldFrame, heldAction },
            };
            LF2Character holder = CreateCharacter("SelfCheck_Audit4HeldHolder", 1, BuildCatchingFrames());
            holder.SwitchDir("left");
            HeldActSelfCheckWeapon weapon = CreateHeldActSelfCheckWeapon("SelfCheck_Audit4HeldWeapon", 0);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(990, heldData));
            weapon.Frame.N = 0;
            weapon.Frame.D = weapon.FrameCache.GetFrameDataById(0);
            weapon.Trans.SetWait(heldFrame.wait, 33);
            weapon.ItrRest.Arest = 8;
            holder.ItrRest.Arest = 9;
            weapon.Act(holder, new WeaponPoint { weaponact = 5, cover = 0 }, Vector3.zero);
            Expect(weapon.Frame.N == 5 && weapon.Trans.WaitCounter == 33 && weapon.Runtime.Dir == "left" &&
                   weapon.ItrRest.Arest == 8 && holder.ItrRest.Arest == 9,
                "BATTLE-AUDIT4-06: held sync must raw-write frame, preserve wait/facing, and not clear arest");

            var oid5Spawned = new FlowSelfCheckEntity(LF2ObjectType.Other);
            oid5Spawned.BindData("SelfCheck_Audit4Oid5Spawn", 5, BuildAudit4StandardHitData("SelfCheck_Audit4Oid5Spawn"));
            oid5Spawned.Health.HP = 100;
            oid5Spawned.Health.HP3 = 100;
            oid5Spawned.Health.HPBound = 100;
            oid5Spawned.Health.PP = 100;
            var postInitLiving = typeof(LF2ObjectPointFactory).GetMethod(
                "PostInitLiving",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Expect(postInitLiving != null,
                "BATTLE-AUDIT4-08: oid5 opoint fixture requires the production PostInitLiving entry");
            postInitLiving.Invoke(LF2ObjectPointFactory.Instance, new object[]
            {
                oid5Spawned,
                null,
                new ObjectPoint { kind = 1, oid = 5 },
                (int)LF2ObjectType.Other,
                0f,
                false,
                true,
            });
            Expect(oid5Spawned.Health.HP == 10 && oid5Spawned.Health.HP3 == 10 &&
                   oid5Spawned.Health.HPBound == 10 && oid5Spawned.Health.PP == 5,
                "BATTLE-AUDIT4-08: oid5 opoint initialization must write HP/HP3/HPBound/PP together");
            LF2Character oid5Victim = CreateCharacter(
                "SelfCheck_Audit4Oid5Victim", 5, BuildAudit4StandardHitData("SelfCheck_Audit4Oid5Victim"));
            oid5Victim.Health.HP = 10;

            LF2FrameData zeroOidFrame = InteractionFrame(null);
            zeroOidFrame.opoints.Add(new ObjectPoint { kind = 1, oid = 0, action = 0 });
            var zeroOidSpawner = new FlowSelfCheckEntity(LF2ObjectType.Other);
            zeroOidSpawner.BindData("SelfCheck_Audit4ZeroOidOpoint", 947, new LF2CharacterData
            {
                name = "SelfCheck_Audit4ZeroOidOpoint",
                frames = new List<LF2FrameData> { zeroOidFrame },
            });
            int queuedBefore = GetQueuedObjectPointTaskCount(LF2ObjectPointFactory.Instance);
            LF2ObjectPointFactory.Instance.ProcessOpointSpawn(zeroOidSpawner);
            Expect(GetQueuedObjectPointTaskCount(LF2ObjectPointFactory.Instance) == queuedBefore,
                "BATTLE-AUDIT4-08: first opoint with oid<=0 must not enqueue a spawn");

            LF2CharacterData specialData = BuildAudit4StandardHitData("SelfCheck_Audit4SpecialVictim");
            specialData.frames.Add(Frame(20, LF2States.Standing, 5, 20, 0, 0));
            specialData.frames.Add(Frame(30, LF2States.Standing, 5, 30, 0, 0));
            specialData.frames.Add(Frame(200, LF2States.Standing, 5, 200, 0, 0));
            specialData.frames.Add(Frame(203, LF2States.Burning, 5, 203, 0, 0));
            var specialVictim = new SelfCheckCharacterDatShell();
            specialVictim.BindData("SelfCheck_Audit4SpecialVictim", 210, specialData);
            var specialAttacker = new FlowSelfCheckEntity(LF2ObjectType.Other);
            specialAttacker.BindData("SelfCheck_Audit4SpecialAttacker", 948, BuildAudit4StandardHitData("SelfCheck_Audit4SpecialAttacker"));
            specialVictim.KnockbackVx = -3.0;
            bool specialHit = specialVictim.Hit(new InteractionArea { kind = 0, effect = 2 }, specialAttacker);
            Expect(specialHit && specialVictim.Frame.N == 203 && specialVictim.Runtime.Dir == "left" &&
                   specialVictim.AttackingCounter == 0,
                "BATTLE-AUDIT4-04: type3 effect tail must enter burning frame and face after the ordered motion reset");

            var selfDestructMethod = typeof(LF2SpecialAttack).GetMethod(
                "ApplyPostHitSelfDestruct",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Expect(selfDestructMethod != null,
                "BATTLE-AUDIT4-04: self-destruct direction fixture requires the production post-hit tail");
            var oid201 = new AlternateDamageSelfCheckSpecialAttack();
            oid201.BindData("SelfCheck_Audit4Oid201", 201, specialData);
            var selfDestructWorld = new SimulationWorld();
            selfDestructWorld.Register(oid201);
            selfDestructWorld.Register(oid5Victim);
            int oid201Slot = oid201.Runtime.SlotIndex;
            int oid5VictimSlot = oid5Victim.Runtime.SlotIndex;
            selfDestructMethod.Invoke(oid201, new object[] { oid5Victim });
            Expect(selfDestructWorld.FindEntityByRuntimeSlotForQuery(oid201Slot) == null &&
                   selfDestructWorld.FindEntityByRuntimeSlotForQuery(oid5VictimSlot) == oid5Victim,
                "BATTLE-AUDIT4-04: oid201 must destroy the attacking special, not its character victim");
            var oid214 = new AlternateDamageSelfCheckSpecialAttack();
            oid214.BindData("SelfCheck_Audit4Oid214", 214, specialData);
            oid214.Health.HP = 100;
            selfDestructMethod.Invoke(oid214, new object[] { oid5Victim });
            Expect(oid214.Health.HP == 0 && oid5Victim.Health.HP == 10,
                "BATTLE-AUDIT4-04: oid214 must zero the attacking special HP, not the character victim HP");
        }

        private static void CheckAlternateHurtTriggerMatrix()
        {
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheckAlternateHurtAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateHurtVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(10, LF2States.Standing, 0, 10, 39, 79),
                    Frame(20, LF2States.Standing, 0, 20, 39, 79),
                    Frame(23, LF2States.Defending, 0, 23, 39, 79),
                    Frame(110, LF2States.Defending, 0, 110, 39, 79),
                }
            };
            var attacker = CreateCharacter("SelfCheck_AlternateHurtAttacker", 1, attackerData);
            var victim = CreateCharacter("SelfCheck_AlternateHurtVictim", 2, victimData);
            var itr = new InteractionArea
            {
                kind = 0,
                effect = 0,
                bdefend = 0,
                dvx = 5,
            };

            attacker.SwitchDir("right");
            victim.SwitchDir("right");
            victim.Health.HP = 500;
            victim.Runtime.PrevFrame2 = 0;
            victim.HitStateCount = 15;
            victim.ImmediateFrame(20);
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid37 must use alternate hurt while HitStateCount is within 15");
            itr.effect = 6;
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid37 heavy effects must reject alternate hurt");

            victimData.type_sub = 6;
            victim.HitStateCount = 1;
            itr.effect = 0;
            victim.ImmediateFrame(10);
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid6 must use alternate hurt below frame 20");
            victim.ImmediateFrame(20);
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid6 frame 20 in a non-special state must reject alternate hurt");
            victim.ImmediateFrame(23);
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid6 state 7 must use alternate hurt at frame 20 or later");

            victimData.type_sub = 52;
            victim.HitStateCount = 15;
            victim.ImmediateFrame(20);
            attackerData.type_sub = 1;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid52 must use alternate hurt for an ordinary attacker within its hit window");
            attackerData.type_sub = 208;
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "attacker oid208 must reject oid52 alternate hurt");

            victimData.type_sub = 1;
            victim.HitStateCount = 100;
            victim.Runtime.PrevFrame2 = 110;
            victim.Health.HP = 500;
            attackerData.type_sub = 1;
            itr.bdefend = 60;
            itr.dvx = 5;
            attacker.SwitchDir("right");
            victim.SwitchDir("left");
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "PrevFrame2 state 7 must allow alternate hurt when facings differ");
            victim.SwitchDir("right");
            itr.dvx = -1;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "PrevFrame2 state 7 must allow alternate hurt for negative dvx");
            itr.dvx = 5;
            attackerData.type_sub = 124;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "special defend attacker oid124 must allow alternate hurt with matching facings");
            attackerData.type_sub = 1;
            victim.SwitchDir("left");
            itr.bdefend = 61;
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "PrevFrame2 defend alternate hurt must reject bdefend above 60");

            victimData.type_sub = 37;
            victim.HitStateCount = 0;
            victim.Runtime.PrevFrame2 = 0;
            itr.kind = 9;
            itr.effect = 0;
            itr.bdefend = 0;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "raw kind9 fixture must otherwise satisfy alternate-hurt selection");
            Expect(!(itr.kind != 9 && LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr)),
                "the caller gate must keep raw kind9 out of alternate hurt");
        }

        private static void CheckAlternateDamageCoreSideEffects()
        {
            var ordinaryData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageOrdinary",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(110, LF2States.Defending, 5, 110, 39, 79),
                    Frame(112, LF2States.BrokenDefend, 5, 112, 39, 79),
                }
            };
            var world = new SimulationWorld();
            var holder = CreateCharacter("SelfCheck_AlternateDamageHolder", 3, ordinaryData);
            var attacker = CreateCharacter("SelfCheck_AlternateDamageAttacker", 1, ordinaryData);
            var victim = CreateCharacter("SelfCheck_AlternateDamageVictim", 37, victimData);
            world.Register(holder);
            world.Register(attacker);
            world.Register(victim);

            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = holder.Runtime.SlotIndex;
            attacker.SwitchDir("right");
            attacker.FrameDelay = -9;
            attacker.AttackExempt = 9;
            attacker.Runtime.ZInt = 10;

            holder.KillStat = 0;
            holder.ComboCountAtk = 0;
            holder.FrameDelay = 0;

            victim.ImmediateFrame(110);
            victim.Runtime.PrevFrame2 = 110;
            victim.Runtime.Y = 0f;
            victim.Runtime.YInt = 0;
            victim.Runtime.Vx = 0f;
            victim.Runtime.ZInt = 20;
            victim.KnockbackVx = 0f;
            victim.Health.HP = 5;
            victim.Health.HPBound = 101;
            victim.Health.HPLost = 7;
            victim.FallDamageDiv = 200;
            victim.KillCount = -1;
            victim.Unk344 = 1;
            victim.ComboCountVic = 0;
            victim.FallCounter = 0;
            victim.AttackingCounter = 7;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.AttackExempt = 13;
            victim.Trans.SetWait(victim.Frame.D.wait, 73);

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                bdefend = 31,
                dvx = 5,
                arest = 2,
                vrest = 15,
                effect = 0,
            };

            LF2AlternateDamageResolver.ApplyAlternateDamage(attacker, victim, victim.HitCounters, itr);
            victim.RecordKind0Hit(attacker, itr);

            Expect(victim.Health.HP == 0 && victim.Health.HPBound == 100,
                "alternate damage must apply adjusted injury 50, reduced to 5, with integer HPBound division");
            Expect(victim.Health.HPLost == 7,
                "alternate damage must leave HPLost unchanged");
            Expect(holder.KillStat == 1 && holder.ComboCountAtk == 5 && victim.ComboCountVic == 5,
                "lethal alternate damage must update holder kill/combo and victim combo stats once");
            Expect(world.KillStats[1] == 1 && world.DamageStats[1] == 5,
                "alternate damage must update world kill and damage stat slot Unk344=1");
            Expect(victim.FallCounter == 80 && victim.AttackingCounter == 0 &&
                   victim.HitStateCount == 31 && victim.HitCount == 1,
                "alternate damage must write lethal fall, attacking, hit-state, and hit-count fields");
            Expect(attacker.FrameDelay == 3 && victim.FrameDelay == -5,
                "alternate damage must overwrite both attacker and victim frame delays");
            Expect(victim.CurrentFrameId == 112 && victim.Trans.WaitCounter == 73,
                "grounded defended alternate damage must enter frame 112 without resetting wait_counter");
            Expect(Nearly(victim.KnockbackVx, 2f),
                "ground alternate knockback must use integer dvx/2 for dvx=5");
            Expect(attacker.AttackExempt == 2 && victim.AttackExempt == 13,
                "alternate damage must apply arest to the attacker only");
            Expect(holder.FrameDelay == 3,
                "a negative-link attacker must propagate its overwritten delay to the active holder");

            int attackerSlot = attacker.Runtime.SlotIndex;
            Expect(victim.ItrRest.HasVrest(attackerSlot),
                "alternate damage must create victim-side vrest for the attacker slot");
            for (int i = 0; i < 11; i++)
                victim.ItrRest.TickVrestForAttacker(attackerSlot);
            Expect(victim.ItrRest.HasVrest(attackerSlot),
                "vrest=15 must clamp to 12 rather than expire after 11 ticks");
            victim.ItrRest.TickVrestForAttacker(attackerSlot);
            Expect(!victim.ItrRest.HasVrest(attackerSlot),
                "vrest=15 must expire after the clamped twelfth tick");

            Expect(attacker.HitRecordCount + victim.HitRecordCount == 1,
                "the alternate-damage caller must record exactly one kind0 hit");

            int[] killStats = world.KillStats;
            int[] damageStats = world.DamageStats;
            world.ResetRuntimeState();
            Expect(ReferenceEquals(killStats, world.KillStats) && ReferenceEquals(damageStats, world.DamageStats),
                "world reset must preserve alternate-damage stat array identity");
            for (int i = 0; i < killStats.Length; i++)
            {
                Expect(killStats[i] == 0 && damageStats[i] == 0,
                    "world reset must clear every alternate-damage stat slot");
            }
        }

        private static void CheckAlternateDamageMotionTailMatrix()
        {
            LF2CharacterData frameData = BuildAlternateDamageMotionFrames();

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 3.0),
                    "ground Fall80/dvx0 with a right-facing ordinary attacker must add +3 knockback"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.SwitchDir("left");
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -3.0),
                    "ground Fall80/dvx0 with a left-facing ordinary attacker must add -3 knockback"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 6.0),
                    "ground state2000 Fall80/dvx0 must add +6 when the attacker is left of the victim"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 20.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -6.0),
                    "ground state2000 Fall80/dvx0 must add -6 when the attacker is right of the victim"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 5.0),
                    "ground state2000 nonzero dvx must use attacker/victim X ordering"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(attacker, 20.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    itr.effect = 22;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 5.0),
                    "ground effect22 must add +dvx when victim X is not greater than attacker X"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    itr.effect = 23;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -5.0),
                    "ground effect23 must add -dvx when victim X is greater than attacker X"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(victim, 10.0, -10.0);
                    victim.FallCounter = 80;
                    victim.Runtime.Vx = 5.0;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 6.0),
                    "air Fall80 with abs(Vx)<6 and dvx<6 must use right-facing +6 knockback"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, -10.0);
                    itr.effect = 23;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -5.0),
                    "air effect23 must use victim/attacker X ordering"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.SwitchDir("left");
                    SetAlternateDamagePosition(victim, 10.0, -10.0);
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -5.0),
                    "air generic alternate knockback must use the full signed dvx"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    victim.ImmediateFrame(110);
                    victim.Runtime.PrevFrame2 = 0;
                    victim.HitStateCount = 0;
                    victim.Trans.SetWait(victim.Frame.D.wait, 47);
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    victim.CurrentFrameId == 111 && victim.Trans.WaitCounter == 47,
                    "ground frame110 with HitStateCount<=30 must enter frame111 and preserve wait_counter"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(20);
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 63);
                    attacker.Runtime.Vz = 6.0;
                    victim.KnockbackVx = 8.0;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    attacker.CurrentFrameId >= 0 && attacker.CurrentFrameId < 16 &&
                    Nearly(attacker.Runtime.Vx, -4.0) &&
                    Nearly(attacker.Runtime.Vy, -4.0) &&
                    Nearly(attacker.Runtime.Vz, -4.0) &&
                    attacker.Trans.WaitCounter == 63,
                    "state1002 tail must select frame0..15, apply reflected velocity, and preserve wait_counter"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    attacker.Runtime.Vx = 5.0;
                    attacker.Runtime.Vz = 10.0;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    Nearly(attacker.Runtime.Vx, 2.0) && Nearly(attacker.Runtime.Vz, 4.0),
                    "state2000 attacker moving toward the victim must damp Vx and Vz by 0.4"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    attacker.Runtime.Vx = -5.0;
                    attacker.Runtime.Vz = 10.0;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    Nearly(attacker.Runtime.Vx, -5.0) && Nearly(attacker.Runtime.Vz, 10.0),
                    "state2000 attacker moving away from the victim must not damp velocity"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(22);
                    attacker.AttackingCounter = 7;
                    attacker.Runtime.Vx = 5.0;
                    attacker.Runtime.Vz = 9.0;
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 71);
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    attacker.CurrentFrameId == 10 &&
                    attacker.AttackingCounter == 0 &&
                    Nearly(attacker.Runtime.Vx, 0.0) &&
                    Nearly(attacker.Runtime.Vz, 9.0) &&
                    attacker.Trans.WaitCounter == 71,
                    "state3000 tail must enter frame10, clear attacking/Vx, preserve Vz and wait_counter"));
        }

        private static void CheckAlternateDamageCharacterEntry()
        {
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageCharacterAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageCharacterVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_AlternateDamageCharacterAttacker", 1, attackerData);
            var victim = CreateCharacter("SelfCheck_AlternateDamageCharacterVictim", 37, victimData);
            world.Register(attacker);
            world.Register(victim);

            attacker.SwitchDir("right");
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.ZInt = 10;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 7;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FrameDelay = 0;
            victim.Runtime.Y = 0f;
            victim.Runtime.YInt = 0;
            victim.Runtime.Vx = 0f;
            victim.Runtime.ZInt = 20;
            victim.KnockbackVx = 0f;

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                dvx = 5,
                bdefend = 0,
                arest = 4,
                vrest = 0,
                effect = 0,
            };
            var volume = new PhysicsState.BattleVolume(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

            bool resolved = victim.Hit(itr, attacker, Vector3.zero, volume);

            Expect(resolved,
                "LF2Character.Hit must resolve the shared alternate-damage branch");
            Expect(victim.Health.HP == 90 && victim.Health.HPBound == 97 && victim.Health.HPLost == 7,
                "LF2Character.Hit alternate damage must apply reduced injury without changing HPLost");
            Expect(victim.FrameDelay == -5 && victim.HitCount == 1 && Nearly(victim.KnockbackVx, 2f),
                "LF2Character.Hit alternate damage must apply victim delay, hit count, and integer half-dvx");
            Expect(attacker.FrameDelay == 3 && attacker.AttackExempt == 4,
                "LF2Character.Hit alternate damage must apply attacker delay and arest");
            Expect(attacker.HitRecordCount + victim.HitRecordCount == 1,
                "LF2Character.Hit alternate damage must record exactly one kind0 hit");
        }

        private static void CheckAlternateDamageSharedDatEntry()
        {
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageSharedAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageSharedVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "SelfCheck_AlternateDamageSharedAttacker",
                1,
                attackerData);
            var victim = new AlternateDamageSelfCheckEntity();
            victim.BindData(37, victimData);
            world.Register(attacker);
            world.Register(victim);

            attacker.SwitchDir("right");
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.ZInt = 10;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 7;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FrameDelay = 0;
            victim.Runtime.Y = 0f;
            victim.Runtime.YInt = 0;
            victim.Runtime.Vx = 0f;
            victim.Runtime.ZInt = 20;
            victim.KnockbackVx = 0f;

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                dvx = 5,
                bdefend = 0,
                arest = 4,
                vrest = 0,
                effect = 0,
            };

            bool resolved = LF2CharacterDatHitResolver.TryResolveHit(
                victim,
                itr,
                attacker,
                Vector3.zero,
                default);

            Expect(resolved,
                "shared-DAT character entry must resolve the shared alternate-damage branch");
            Expect(victim.Health.HP == 90 && victim.Health.HPBound == 97 && victim.Health.HPLost == 7,
                "shared-DAT alternate damage must apply reduced injury without changing HPLost");
            Expect(victim.FrameDelay == -5 && victim.HitCount == 1 && Nearly(victim.KnockbackVx, 2f),
                "shared-DAT alternate damage must apply victim delay, hit count, and integer half-dvx");
            Expect(attacker.FrameDelay == 3 && attacker.AttackExempt == 4,
                "shared-DAT alternate damage must apply attacker delay and arest");
            Expect(attacker.HitRecordCount + victim.HitRecordCount == 1,
                "shared-DAT alternate damage must record exactly one kind0 hit");
        }

        private static void CheckAlternateDamageHeavyWeaponEntries()
        {
            LF2CharacterData weaponData = BuildAlternateDamageWeaponFrames();
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateHeavyVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                dvx = 5,
                bdefend = 0,
                arest = 4,
                vrest = 0,
                effect = 0,
            };

            var characterWorld = new SimulationWorld();
            AlternateDamageSelfCheckWeapon characterAttacker = CreateSelfCheckWeapon(
                "SelfCheck_AlternateHeavyCharacterAttacker",
                1,
                2,
                weaponData,
                20);
            LF2Character characterVictim = CreateCharacter(
                "SelfCheck_AlternateHeavyCharacterVictim",
                37,
                victimData);
            characterWorld.Register(characterAttacker);
            characterWorld.Register(characterVictim);
            PrepareAlternateEntry(characterAttacker, characterVictim);
            characterAttacker.Runtime.WeaponState = LF2States.WeaponThrowing;
            characterAttacker.Runtime.Vz = 6.0;

            bool characterResolved = characterVictim.Hit(itr, characterAttacker, Vector3.zero, default);

            Expect(characterResolved && characterVictim.Health.HP == 90,
                "real-character alternate damage must use the heavy weapon's original injury");
            Expect(characterAttacker.Frame.N >= 0 && characterAttacker.Frame.N < 16 &&
                   Nearly(characterAttacker.Runtime.Vx, -1.0) &&
                   Nearly(characterAttacker.Runtime.Vy, -4.0) &&
                   Nearly(characterAttacker.Runtime.Vz, -4.0),
                "state1002 alternate tail must update frame and reflected velocity on a real weapon");
            Expect(characterAttacker.Runtime.WeaponState == LF2States.WeaponThrowing,
                "state1002 alternate tail must not rewrite the independent runtime weapon state");

            var sharedWorld = new SimulationWorld();
            AlternateDamageSelfCheckWeapon sharedAttacker = CreateSelfCheckWeapon(
                "SelfCheck_AlternateHeavySharedAttacker",
                1,
                2,
                weaponData,
                0);
            var sharedVictim = new AlternateDamageSelfCheckEntity();
            sharedVictim.BindData(37, victimData);
            sharedWorld.Register(sharedAttacker);
            sharedWorld.Register(sharedVictim);
            PrepareAlternateEntry(sharedAttacker, sharedVictim);

            bool sharedResolved = LF2CharacterDatHitResolver.TryResolveHit(
                sharedVictim,
                itr,
                sharedAttacker,
                Vector3.zero,
                default);

            Expect(sharedResolved && sharedVictim.Health.HP == 90,
                "shared-DAT alternate damage must use the heavy weapon's original injury");

            var guardWorld = new SimulationWorld();
            LF2Character guardAttacker = CreateCharacter(
                "SelfCheck_AlternateGuardAttacker",
                1,
                BuildAlternateDamageMotionFrames());
            AlternateDamageSelfCheckWeapon guardVictim = CreateSelfCheckWeapon(
                "SelfCheck_AlternateGuardWeaponVictim",
                2,
                1,
                weaponData,
                0);
            guardWorld.Register(guardAttacker);
            guardWorld.Register(guardVictim);
            guardVictim.Health.HP = 100;
            guardVictim.Health.HPBound = 100;

            LF2AlternateDamageResolver.ApplyAlternateDamage(guardAttacker, guardVictim, null, itr);

            Expect(guardVictim.Health.HP == 100 && guardVictim.Health.HPBound == 100,
                "alternate damage must reject a non-character DAT victim");
        }

        private static void CheckAlternateDamageInteractionVrest()
        {
            var weaponItr = MakeInteractionItr(0, 1, 100, 4);
            var weaponData = new LF2CharacterData
            {
                name = "SelfCheckAlternateVrestWeapon",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(weaponItr) },
            };
            var victimData = BuildInteractionVictimData("SelfCheckAlternateVrestWeaponVictim", 37);
            var weaponWorld = new SimulationWorld();
            AlternateDamageSelfCheckWeapon weapon = CreateSelfCheckWeapon(
                "SelfCheck_AlternateVrestWeapon",
                1,
                1,
                weaponData,
                0);
            LF2Character weaponVictim = CreateInteractionCharacter(
                "SelfCheck_AlternateVrestWeaponVictim",
                37,
                victimData);
            RegisterInteractionPair(weaponWorld, weapon, weaponVictim);

            weaponWorld.CaptureCollisionFrameSnapshotsAll();
            weaponWorld.CollectCollisionCandidatesAll();
            weaponWorld.ObjectInteractionTickAll(1);
            weaponWorld.EndCollisionCandidateConsumption();

            int weaponSlot = weapon.Runtime.SlotIndex;
            Expect(weaponVictim.Health.HP == 90 && weaponVictim.ItrRest.GetVrest(weaponSlot) == 4,
                "weapon interaction must preserve alternate vrest clamp 1->4 after Hit returns");
            Expect(weaponItr.vrest == 1,
                "weapon interaction must not mutate authored raw vrest data");

            var sharedItr = MakeInteractionItr(0, 20, 100, 4);
            var sharedData = new LF2CharacterData
            {
                name = "SelfCheckAlternateVrestSharedAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(sharedItr) },
            };
            var sharedWorld = new SimulationWorld();
            var sharedAttacker = new AlternateDamageSelfCheckEntity();
            sharedAttacker.BindData(1, sharedData);
            LF2Character sharedVictim = CreateInteractionCharacter(
                "SelfCheck_AlternateVrestSharedVictim",
                37,
                BuildInteractionVictimData("SelfCheckAlternateVrestSharedVictim", 37));
            RegisterInteractionPair(sharedWorld, sharedAttacker, sharedVictim);

            sharedWorld.CaptureCollisionFrameSnapshotsAll();
            sharedWorld.CollectCollisionCandidatesAll();
            sharedWorld.PostInteractionTickAll(1);
            sharedWorld.EndCollisionCandidateConsumption();

            int sharedSlot = sharedAttacker.Runtime.SlotIndex;
            Expect(sharedVictim.Health.HP == 90 && sharedVictim.ItrRest.GetVrest(sharedSlot) == 12,
                "shared-DAT interaction must preserve alternate vrest clamp 20->12 after Hit returns");
            Expect(sharedItr.vrest == 20,
                "shared-DAT interaction must not mutate authored raw vrest data");
        }

        private static void CheckSpecialAttackDamagePreprocess()
        {
            RunSpecialAttackPreprocessCase(
                kind: 4,
                rawVrest: 1,
                arrange: special =>
                {
                    special.WeaponCount = 1;
                    special.SwitchDir("left");
                    special.Runtime.Vx = 1.0;
                },
                verify: (special, victim, sourceItr) =>
                {
                    Expect(special.Health.HP == 100,
                        "kind4 preprocessing must not zero special-attack HP");
                    Expect(Nearly(victim.KnockbackVx, 3.0),
                        "kind4 preprocessing must convert to kind0 and flip dvx for reverse travel");
                    Expect(victim.ItrRest.GetVrest(special.Runtime.SlotIndex) == 4,
                        "special-attack kind4 must preserve alternate vrest clamp 1->4");
                    Expect(sourceItr.kind == 4 && sourceItr.dvx == 6,
                        "kind4 preprocessing must not mutate authored itr data");
                });

            RunSpecialAttackPreprocessCase(
                kind: 9,
                rawVrest: 20,
                arrange: special => special.SwitchDir("right"),
                verify: (special, victim, sourceItr) =>
                {
                    Expect(special.Health.HP == 0,
                        "kind9 character preprocessing must zero special-attack HP before consume");
                    Expect(victim.Health.HP == 90,
                        "kind9 character preprocessing must convert to kind0 and enter alternate damage");
                    Expect(victim.ItrRest.GetVrest(special.Runtime.SlotIndex) == 12,
                        "special-attack kind9 must preserve alternate vrest clamp 20->12");
                    Expect(sourceItr.kind == 9,
                        "kind9 preprocessing must not mutate authored itr data");
                });
        }

        private static void CheckSimulationWorldLateMutation()
        {
            var world = new SimulationWorld();
            var spawner = new MutationSelfCheckEntity(1, registerDuringLate: true);
            var remover = new MutationSelfCheckEntity(2, unregisterDuringLate: true);

            world.Register(spawner);
            world.Register(remover);
            int removerSlot = remover.Runtime.SlotIndex;

            world.LateEntityUpdateAll(1);

            Expect(spawner.LateTickCount == 1,
                "LateEntityUpdateAll must execute the original spawner entity");
            Expect(remover.LateTickCount == 1,
                "LateEntityUpdateAll must allow an entity to request unregister during SimFrameTick");
            Expect(spawner.Spawned != null && spawner.Spawned.LateTickCount == 1,
                "an entity spawned into a later runtime slot must execute in the same late pass");
            Expect(spawner.Spawned.Runtime.SlotIndex > removerSlot,
                "the mutation fixture must place the spawned entity in a later runtime slot");
            Expect(world.FindEntityByRuntimeSlotForQuery(removerSlot) == null,
                "the unregistering entity must be removed when the late pass flushes mutations");
            Expect(world.ObjectCount == 2,
                "the late-pass mutation flush must leave only the spawner and spawned entity");

            world.LateEntityUpdateAll(2);

            Expect(spawner.LateTickCount == 2 && spawner.Spawned.LateTickCount == 2,
                "the remaining entities must each continue on the second late pass");
            Expect(remover.LateTickCount == 1 &&
                   world.FindEntityByRuntimeSlotForQuery(removerSlot) == null,
                "the removed entity must not execute or reappear on the second late pass");
            Expect(world.ObjectCount == 2,
                "the second late pass must preserve the two remaining entities");

            var lowSlotWorld = new SimulationWorld();
            var releasedLowSlot = new MutationSelfCheckEntity(10);
            var beforeSpawner = new MutationSelfCheckEntity(11);
            var highSlotSpawner = new MutationSelfCheckEntity(12, registerDuringLate: true);
            lowSlotWorld.Register(releasedLowSlot);
            lowSlotWorld.Register(beforeSpawner);
            lowSlotWorld.Register(highSlotSpawner);
            int lowSlot = releasedLowSlot.Runtime.SlotIndex;
            int highSlot = highSlotSpawner.Runtime.SlotIndex;
            lowSlotWorld.Unregister(releasedLowSlot);

            lowSlotWorld.LateEntityUpdateAll(3);

            Expect(highSlotSpawner.Spawned != null && highSlotSpawner.Spawned.Runtime.SlotIndex == lowSlot,
                "the low-slot mutation fixture must reuse the released runtime slot");
            Expect(highSlotSpawner.Spawned.Runtime.SlotIndex < highSlot,
                "the spawned low-slot entity must be behind the current dynamic late scan cursor");
            Expect(highSlotSpawner.Spawned.LateTickCount == 0,
                "an entity spawned into an already-scanned lower runtime slot must wait until the next late pass");

            lowSlotWorld.LateEntityUpdateAll(4);

            Expect(highSlotSpawner.Spawned.LateTickCount == 1,
                "the deferred lower-slot entity must execute exactly once on the next late pass");
        }

        private static void CheckQueuedObjectPointPassBoundaries()
        {
            var oid999Wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [999] = new LF2CharacterDataWrapper(999, BuildSelfCheckOpoint999Data()),
            };
            var oid999Types = new Dictionary<int, int>
            {
                [999] = (int)LF2ObjectType.Other,
            };
            using var oid999Config = new TemporaryRuntimeObjectConfigs(oid999Types, oid999Wrappers);
            using var objectPoolState = new TemporaryObjectPoolInitialization();
            using var sinkWorld = new TemporarySimulationDriverWorld(new SimulationWorld());

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            Expect(factory != null && referencePool != null,
                "queued opoint self-check requires the production factory and reference pool singletons");

            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "queued opoint self-check must start from an empty production queue");

            var frameLogicWorld = new SimulationWorld();
            var producer = new QueuedBoundarySelfCheckEntity(
                QueuedBoundarySelfCheckEntity.Phase.FrameLogic,
                ReleaseSpawnSemantic.ImmediateEffect);
            var observer = new QueuedBoundarySelfCheckEntity(
                QueuedBoundarySelfCheckEntity.Phase.ObserveFrameLogic,
                ReleaseSpawnSemantic.None);
            frameLogicWorld.Register(producer);
            frameLogicWorld.Register(observer);

            frameLogicWorld.FrameLogicBeforeAdvanceAll(1);

            Expect(producer.EnqueueCount == 1 && producer.LastTask != null,
                "the queued frame_logic fixture must publish exactly one task");
            Expect(observer.QueueCountObservedAtFrameLogic == 0,
                "frame_logic tasks must flush before the next runtime-slot entity enters the same pass");
            Expect(IsRecycledAndCleared(producer.LastTask),
                "the frame_logic task must be consumed and recycled at its production boundary");
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the frame_logic boundary must leave no queued publication behind");

            frameLogicWorld.FlushQueuedObjectPointTasks();
            Expect(producer.EnqueueCount == 1 && !producer.LastTask.IsFromPool &&
                   GetQueuedObjectPointTaskCount(factory) == 0,
                "an outer safety flush must not publish or recycle an already-consumed frame_logic task twice");

            QueuedBoundarySelfCheckWeapon directBrokenWeapon = CreateQueuedBoundaryWeapon();
            bool completedBrokenCleanup = directBrokenWeapon.TryRunLatePostOpointCleanupPhase();

            Expect(completedBrokenCleanup && directBrokenWeapon.Runtime.PendingFlushDestroy,
                "the real weapon late cleanup phase must mark a depleted destroyable weapon for deferred destroy");
            Expect(GetQueuedObjectPointTaskCount(factory) == 5,
                "oid 100 real weapon cleanup must queue its five C++ broken-weapon fragments");

            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the broken-fragment production boundary must consume the real factory queue");

            directBrokenWeapon.TryRunLatePostOpointCleanupPhase();
            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "repeating cleanup and its safety flush must not publish broken fragments twice");

            var brokenWorld = new SimulationWorld();
            QueuedBoundarySelfCheckWeapon brokenWeapon = CreateQueuedBoundaryWeapon();
            brokenWorld.Register(brokenWeapon);
            int brokenSlot = brokenWeapon.Runtime.SlotIndex;

            brokenWorld.LateEntityUpdateAll(2);

            Expect(brokenWeapon.PendingDestroyObserved && brokenWeapon.TransitDestroyCount == 1,
                $"the full late pass must enter pending destroy and finalize the depleted weapon exactly once; " +
                $"pendingObserved={brokenWeapon.PendingDestroyObserved}, transitDestroyCount={brokenWeapon.TransitDestroyCount}, " +
                $"slotEntity={brokenWorld.FindEntityByRuntimeSlotIncludingPending(brokenSlot)?.Name ?? "null"}");
            Expect(brokenWorld.FindEntityByRuntimeSlotForQuery(brokenSlot) == null,
                "the depleted weapon must leave the world after its real fragment queue boundary");
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the full broken-weapon late pass must not leak fragment tasks into a later pass");

            QueuedBoundaryTransitionSelfCheckEntity directTransition = CreateQueuedBoundaryTransitionEntity();
            directTransition.RunLateTailBeforePrevFrame();
            Expect(GetQueuedObjectPointTaskCount(factory) == 15,
                "leaving state 13 through the real late tail must queue the fifteen C++ transition effects");

            factory.FlushTasks();
            directTransition.MirrorLatePrevFrame();
            directTransition.RunLateTailBeforePrevFrame();
            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "mirroring prev_frame and repeating the safety flush must not republish transition effects");

            var transitionWorld = new SimulationWorld();
            QueuedBoundaryTransitionSelfCheckEntity transition = CreateQueuedBoundaryTransitionEntity();
            transitionWorld.Register(transition);

            transitionWorld.LateEntityUpdateAll(3);

            Expect(transition.Frame.Prev == transition.Frame.N,
                "the full late pass must mirror prev_frame after the real transition-effect production phase");
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the transition-effect late boundary must consume the real factory queue before the pass continues");

            transitionWorld.FlushQueuedObjectPointTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "a repeated late safety flush must not duplicate real transition-effect publication");

            CheckRealOpointSpawnSlotVisibility();
        }

        private static LF2CharacterData BuildSelfCheckOpoint999Data()
        {
            var frames = new List<LF2FrameData>(200);
            for (int frameId = 0; frameId < 200; frameId++)
                frames.Add(Frame(frameId, 9999, 100, frameId, 13, 27));

            return new LF2CharacterData
            {
                name = "SelfCheck_RealOpoint999",
                type_sub = 999,
                frames = frames,
            };
        }

        private static void CheckRealOpointSpawnSlotVisibility()
        {
            var highWorld = new SimulationWorld();
            using (new TemporarySimulationDriverWorld(highWorld))
            {
                var producer = new RealOpointProducerSelfCheckEntity("HighSlot");
                producer.Runtime.SetPosition(140.0, 23.0, 37.0);
                producer.Runtime.SyncIntegerPosition();
                highWorld.Register(producer);
                int producerSlot = producer.Runtime.SlotIndex;

                highWorld.LateEntityUpdateAll(30);

                LF2Entity spawned = FindOidEntity(highWorld, 999);
                Expect(spawned is LF2OtherObject && spawned.Renderer != null,
                    "production oid999 opoint must create a real pooled LF2OtherObject with a renderer");
                Expect(spawned.Runtime.SlotIndex > producerSlot,
                    "production oid999 opoint must register into the next free high runtime slot");
                Expect(spawned.AttackingCounter == 1,
                    "an actual oid999 spawned into a later slot must execute frame_tick in the same late pass");
                Expect(Nearly(spawned.Runtime.Y, producer.Runtime.Y - producer.Frame.D.centery) &&
                       Nearly(spawned.Runtime.Z, producer.Runtime.Z + 1.0),
                    $"late opoint must keep logical Y separate from depth Z; " +
                    $"parentY={producer.Runtime.Y}, parentZ={producer.Runtime.Z}, " +
                    $"spawnedY={spawned.Runtime.Y}, spawnedZ={spawned.Runtime.Z}");
            }

            var lowWorld = new SimulationWorld();
            using (new TemporarySimulationDriverWorld(lowWorld))
            {
                var releasedLow = new DynamicSlotSelfCheckEntity(9001);
                var beforeProducer = new DynamicSlotSelfCheckEntity(9002);
                var producer = new RealOpointProducerSelfCheckEntity("LowSlot");
                lowWorld.Register(releasedLow);
                lowWorld.Register(beforeProducer);
                lowWorld.Register(producer);
                int lowSlot = releasedLow.Runtime.SlotIndex;
                int producerSlot = producer.Runtime.SlotIndex;
                lowWorld.Unregister(releasedLow);

                lowWorld.LateEntityUpdateAll(31);

                LF2Entity spawned = FindOidEntity(lowWorld, 999);
                Expect(spawned is LF2OtherObject && spawned.Renderer != null,
                    "low-slot production opoint must still create the actual oid999 pooled entity");
                Expect(spawned.Runtime.SlotIndex == lowSlot && spawned.Runtime.SlotIndex < producerSlot,
                    $"production oid999 opoint must reuse the released lower dynamic slot; " +
                    $"released={lowSlot}, spawned={spawned.Runtime.SlotIndex}, producer={producerSlot}");
                Expect(spawned.AttackingCounter == 0,
                    "an actual oid999 spawned behind the late scan cursor must not execute in the creation tick");

                lowWorld.LateEntityUpdateAll(32);

                Expect(spawned.AttackingCounter == 1,
                    "the deferred low-slot oid999 must execute exactly once on the next late pass");
                Expect(FindOidEntity(lowWorld, 999) == spawned,
                    "the producer must publish one actual oid999 task rather than duplicating it next tick");
            }
        }

        private static LF2Entity FindOidEntity(SimulationWorld world, int oid)
        {
            for (int slot = 0; slot < 400; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                if (entity != null && entity.ObjectId == oid)
                    return entity;
            }

            return null;
        }

        private static void CheckInteractionRuntimeSlotContracts()
        {
            var oid999Wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [999] = new LF2CharacterDataWrapper(999, BuildSelfCheckOpoint999Data()),
                [733] = new LF2CharacterDataWrapper(733, new LF2CharacterData
                {
                    name = "SelfCheck_OpointCharacterRegistration",
                    type_sub = 733,
                    frames = new List<LF2FrameData>
                    {
                        Frame(307, LF2States.Standing, 2, 308, 39, 79),
                        Frame(308, LF2States.Standing, 2, 999, 39, 79),
                    },
                }),
            };
            var oid999Types = new Dictionary<int, int>
            {
                [999] = (int)LF2ObjectType.Other,
                [733] = (int)LF2ObjectType.Character,
            };
            using var oid999Config = new TemporaryRuntimeObjectConfigs(oid999Types, oid999Wrappers);
            using var objectPoolState = new TemporaryObjectPoolInitialization();

            CheckDynamicRuntimeSlotBoundaryAndFailure();
            CheckPendingSameInstanceReregister();
            CheckState3003RuntimeSlotVrest();
            CheckNonCharacterKind2RuntimeSlotLink();
            CheckOpointCharacterRegistrationAfterModuleBind();
        }

        private static void CheckOpointCharacterRegistrationAfterModuleBind()
        {
            var world = new SimulationWorld();
            using var driverWorld = new TemporarySimulationDriverWorld(world);
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;

            OPointCreateTask singleTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            singleTask.opoint = new ObjectPoint { kind = 1, oid = 733, action = 307, facing = 0 };
            singleTask.team = 2;
            singleTask.pos = Vector3.zero;
            singleTask.dir = "right";
            singleTask.preserveActionZero = true;

            LF2Entity single = factory.CreateObjectImmediate(singleTask);
            LF2ReferencePool.Instance.Recycle(singleTask);
            Expect(single is LF2Character && single.StableId > 0 && single.Runtime.SlotIndex >= 50 &&
                   world.FindEntityByRuntimeSlotIncludingPending(single.Runtime.SlotIndex) == single,
                "single character opoint must allocate identity, bind its DAT, and register before slot validation");
            single.FreeEntityLikeExe();

            OPointCreateMultipleTask multipleTask = LF2ReferencePool.Instance.Fetch<OPointCreateMultipleTask>();
            multipleTask.opoint = new ObjectPoint { kind = 1, oid = 733, action = 307, facing = 0 };
            multipleTask.team = 2;
            multipleTask.pos = Vector3.zero;
            multipleTask.dir = "right";
            multipleTask.number = 2;
            multipleTask.preserveActionZero = true;
            factory.EnqueueCreateMultipleObjects(multipleTask);
            factory.FlushTasks();

            var spawned = new List<LF2Entity>();
            for (int slot = 50; slot < 400; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                if (entity?.ObjectId == 733)
                    spawned.Add(entity);
            }

            Expect(spawned.Count == 2 && spawned.TrueForAll(entity => entity is LF2Character),
                $"multi character opoint must register every bound character; actual={spawned.Count}");
            var stableIds = new HashSet<int>();
            for (int i = 0; i < spawned.Count; i++)
            {
                Expect(spawned[i].StableId > 0,
                    "every multi character opoint must allocate a positive StableId before registration");
                stableIds.Add(spawned[i].StableId);
            }
            Expect(stableIds.Count == 2,
                $"multi character opoints must receive unique StableIds; actual={string.Join(",", stableIds)}");
            for (int i = 0; i < spawned.Count; i++)
                spawned[i].FreeEntityLikeExe();
        }

        private static void CheckDynamicRuntimeSlotBoundaryAndFailure()
        {
            var world = new SimulationWorld();
            using var driverWorld = new TemporarySimulationDriverWorld(world);

            var reserved49 = new FlowSelfCheckEntity(LF2ObjectType.Character);
            reserved49.SetRuntimeSlotIndex(49);
            world.Register(reserved49);

            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.InitializeForRuntimeSlotContract(9001);
            world.Register(special);
            Expect(reserved49.Runtime.SlotIndex == 49 && special.Runtime.SlotIndex == 50,
                "dynamic SpecialAttack registration must start at slot 50 and preserve reserved slot 49");
            Expect(special.StableId == 9001 && special.Runtime.SlotIndex != special.StableId,
                "dynamic SpecialAttack slot identity must remain independent from StableId");

            var producer = new RuntimeSlotOpointProducerSelfCheckEntity(
                "FullDynamicRange", LF2States.Standing, 0, 9100);
            world.Register(producer);
            for (int i = 0; i < 348; i++)
                world.Register(new DynamicSlotSelfCheckEntity(10000 + i));

            Expect(world.FindEntityByRuntimeSlotIncludingPending(399) != null,
                "dynamic runtime allocation must include the final authority slot 399");
            Expect(world.ObjectCount == 351,
                $"slot fixture must occupy reserved 49 plus all 350 dynamic slots; actual={world.ObjectCount}");

            int rendererCountBefore = GetObjectPoolActiveCount();
            int logicCountBefore = LF2ReferencePool.Instance.ActiveCount;
            int bucketCountBefore = GetSimulationBucketCount(world);
            LF2ObjectPointFactory.Instance.ProcessOpointSpawn(producer);

            Expect(FindOidEntity(world, 999) == null && world.ObjectCount == 351,
                "opoint spawn must fail without falling back into slots 0..49 when 50..399 are full");
            Expect(GetObjectPoolActiveCount() == rendererCountBefore &&
                   LF2ReferencePool.Instance.ActiveCount == logicCountBefore &&
                   GetSimulationBucketCount(world) == bucketCountBefore,
                "rejected full-slot opoint must not leave a renderer, logic object, or empty registry bucket behind");
        }

        private static void CheckPendingSameInstanceReregister()
        {
            var world = new SimulationWorld();
            var entity = new DynamicSlotSelfCheckEntity(11001);
            world.Register(entity);
            int originalSlot = entity.Runtime.SlotIndex;

            SetPrivateField(world, "_ticking", true);
            try
            {
                world.Unregister(entity);
                world.Register(entity);

                var pending = GetPrivateField(world, "_pendingUnregister") as List<ISimObject>;
                Expect(pending != null && pending.Count == 0,
                    "same-tick pooled-instance registration must remove its old pending-unregister entry");
                Expect(entity.Runtime.SlotIndex == originalSlot &&
                       world.FindEntityByRuntimeSlotIncludingPending(originalSlot) == entity,
                    "same-tick pooled-instance registration must finalize the old lifecycle before reusing its slot");
            }
            finally
            {
                SetPrivateField(world, "_ticking", false);
            }
        }

        private static void CheckState3003RuntimeSlotVrest()
        {
            var world = new SimulationWorld();
            using var driverWorld = new TemporarySimulationDriverWorld(world);

            var linked = new DynamicSlotSelfCheckEntity(12001);
            var spawner = new RuntimeSlotOpointProducerSelfCheckEntity(
                "State3003", LF2States.ProjectileTeleport, 20, 12002);
            world.Register(linked);
            world.Register(spawner);
            spawner.Runtime.AnimCounter = linked.Runtime.SlotIndex;

            LF2ObjectPointFactory.Instance.ProcessOpointSpawn(spawner);

            var newborns = new List<LF2Entity>();
            for (int slot = 50; slot < 400; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                if (entity != null && entity.ObjectId == 999)
                    newborns.Add(entity);
            }

            Expect(newborns.Count == 2,
                $"state3003 facing=20 must create two production opoint entities; actual={newborns.Count}");
            for (int i = 0; i < newborns.Count; i++)
            {
                LF2Entity newborn = newborns[i];
                int newbornSlot = newborn.Runtime.SlotIndex;
                Expect(linked.ItrRest.GetVrest(newbornSlot) == 10 &&
                       newborn.ItrRest.GetVrest(linked.Runtime.SlotIndex) == 10,
                    "state3003 must write bilateral vrest between AnimCounter linked slot and newborn slot");
                Expect(!spawner.ItrRest.HasVrest(newbornSlot) &&
                       !newborn.ItrRest.HasVrest(spawner.StableId),
                    "state3003 must not substitute the spawner or StableId for the linked runtime slot");
            }

            Expect(newborns[0].ItrRest.GetVrest(newborns[1].Runtime.SlotIndex) == 40 &&
                   newborns[1].ItrRest.GetVrest(newborns[0].Runtime.SlotIndex) == 40,
                "multi-opoint mutual vrest must use newborn runtime slots");
        }

        private static void CheckNonCharacterKind2RuntimeSlotLink()
        {
            var world = new SimulationWorld();
            using var driverWorld = new TemporarySimulationDriverWorld(world);

            var parent = new DynamicSlotSelfCheckEntity(13001);
            world.Register(parent);
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint { kind = 2, oid = 999, action = 0, facing = 0 };
            task.parent = parent;
            task.team = 3;
            task.pos = Vector3.zero;
            task.dir = "right";
            task.preserveActionZero = true;

            LF2Entity child = LF2ObjectPointFactory.Instance.CreateObjectImmediate(task);
            LF2ReferencePool.Instance.Recycle(task);
            Expect(child != null && parent.StableId != parent.Runtime.SlotIndex &&
                   child.StableId != child.Runtime.SlotIndex,
                "non-character kind2 fixture requires real entities whose StableIds differ from runtime slots");
            Expect(parent.Runtime.LinkState == 1 &&
                   parent.Runtime.TargetSlotIndex == child.Runtime.SlotIndex &&
                   parent.Runtime.HeldWeaponStableId == child.Runtime.SlotIndex &&
                   child.Runtime.LinkState == -1 &&
                   child.Runtime.HolderStableId == parent.Runtime.SlotIndex,
                "non-character kind2 must store runtime slots in all holder/target link fields");

            world.ValidateHeldLinksAll(1);
            Expect(parent.Runtime.LinkState == 1,
                "ValidateHeldLinksAll must preserve a valid non-character kind2 runtime-slot link");
        }

        private static int GetObjectPoolActiveCount()
        {
            object active = GetPrivateField(LF2ObjectPool.Instance, "_activeObjects");
            return active is HashSet<GameObject> objects ? objects.Count : -1;
        }

        private static int GetSimulationBucketCount(SimulationWorld world)
        {
            object buckets = GetPrivateField(world, "_buckets");
            var countProperty = buckets?.GetType().GetProperty("Count");
            return countProperty?.GetValue(buckets) is int count ? count : -1;
        }

        private static bool IsSimulationObjectRegistered(SimulationWorld world, ISimObject target)
        {
            if (world == null || target == null)
                return false;

            object buckets = GetPrivateField(world, "_buckets");
            if (buckets is not System.Collections.IEnumerable entries)
                return false;

            foreach (object entry in entries)
            {
                object bucket = entry?.GetType().GetProperty("Value")?.GetValue(entry);
                object items = bucket?.GetType().GetField("items")?.GetValue(bucket);
                if (items is System.Collections.IList list && list.Contains(target))
                    return true;
            }

            return false;
        }

        private static QueuedBoundarySelfCheckWeapon CreateQueuedBoundaryWeapon()
        {
            var weapon = new QueuedBoundarySelfCheckWeapon();
            weapon.BindData(new LF2CharacterData
            {
                name = "SelfCheck_QueuedBrokenWeapon",
                type_sub = 100,
                weapon_hp = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 100, 0, 39, 79),
                },
            });
            weapon.Runtime.WeaponFlightCounter = -1;
            weapon.Runtime.SetPosition(100, -20, 100);
            weapon.Runtime.SyncIntegerPosition();
            return weapon;
        }

        private static QueuedBoundaryTransitionSelfCheckEntity CreateQueuedBoundaryTransitionEntity()
        {
            return new QueuedBoundaryTransitionSelfCheckEntity(new LF2CharacterData
            {
                name = "SelfCheck_QueuedTransition",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 100, 0, 39, 79),
                    Frame(10, 13, 100, 10, 39, 79),
                },
            });
        }

        private static void CheckCollisionAudit3Contracts()
        {
            CheckCollisionAudit3Kind5AndGeometry();
            CheckCollisionAudit3NearestAndBodyXAcrossModes();
            CheckCollisionAudit3Oid9PairGate();
            CheckCollisionAudit3FrameSourceGates();
        }

        private static void CheckCollisionAudit3Kind5AndGeometry()
        {
            InteractionArea kind5 = MakeCollisionAuditItr(5, -20, -20, 40, 40, 20, vrest: 1);
            LF2FrameData kind5Frame = BuildCollisionAuditFrame(0, LF2States.Standing, kind5, null);
            var kind5World = new SimulationWorld();
            LF2Character kind5Attacker = CreateInteractionCharacter(
                "SelfCheck_Audit3Kind5OnlyAttacker",
                434,
                BuildCollisionAuditData("SelfCheck_Audit3Kind5OnlyAttacker", kind5Frame));
            LF2Character kind5Target = CreateInteractionCharacter(
                "SelfCheck_Audit3Kind5OnlyTarget",
                37,
                BuildInteractionVictimData("SelfCheck_Audit3Kind5OnlyTarget", 37));
            RegisterCollisionAuditPair(kind5World, kind5Attacker, kind5Target, 1, 2);
            List<SceneQueryHit> kind5Candidates = CollectCollisionAuditCandidates(kind5World, kind5Attacker, true);
            Expect(kind5Candidates.Count == 1 && kind5Candidates[0].Target == kind5Target,
                "BATTLE-AUDIT3-03: a kind5-only frame must pass pair-aware coarse collection");

            InteractionArea farOrdinary = MakeCollisionAuditItr(0, 10000, 10000, 40, 40, 20, vrest: 1);
            InteractionArea hugeKind5 = MakeCollisionAuditItr(5, -100000, -100000, 200000, 200000, 20, vrest: 1);
            LF2FrameData mixedFrame = BuildCollisionAuditFrame(0, LF2States.Standing, farOrdinary, null);
            mixedFrame.itrs.Add(hugeKind5);
            var pollutionWorld = new SimulationWorld();
            LF2Character holder = CreateInteractionCharacter(
                "SelfCheck_Audit3Kind5Holder",
                1,
                BuildInteractionVictimData("SelfCheck_Audit3Kind5Holder", 1));
            LF2Character mixedAttacker = CreateInteractionCharacter(
                "SelfCheck_Audit3Kind5MixedAttacker",
                434,
                BuildCollisionAuditData("SelfCheck_Audit3Kind5MixedAttacker", mixedFrame));
            LF2Character mixedTarget = CreateInteractionCharacter(
                "SelfCheck_Audit3Kind5MixedTarget",
                37,
                BuildInteractionVictimData("SelfCheck_Audit3Kind5MixedTarget", 37));
            pollutionWorld.Register(holder);
            ConfigureCollisionAuditEntity(holder, 2, 0);
            RegisterCollisionAuditPair(pollutionWorld, mixedAttacker, mixedTarget, 1, 2);
            mixedAttacker.HolderCopySlot = holder.Runtime.SlotIndex;
            pollutionWorld.CaptureCollisionFrameSnapshotsAll();

            var pollutionQuery = pollutionWorld.SceneQuery as BruteForceSceneQuery;
            Expect(pollutionQuery != null,
                "BATTLE-AUDIT3-03: kind5 coarse regression requires BruteForceSceneQuery");
            var coarseMethod = typeof(BruteForceSceneQuery).GetMethod(
                "PassesReleaseCoarsePrefilter",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Expect(coarseMethod != null,
                "BATTLE-AUDIT3-03: kind5 coarse regression reflection contract changed");
            bool pollutedCoarse = (bool)coarseMethod.Invoke(
                pollutionQuery,
                new object[]
                {
                    mixedAttacker,
                    mixedAttacker.Frame.D,
                    mixedAttacker.GetCollisionFrameData(),
                    mixedTarget,
                    mixedTarget.Frame.D,
                    mixedTarget.GetCollisionFrameData(),
                });
            Expect(!pollutedCoarse,
                "BATTLE-AUDIT3-03: a rejected huge kind5 probe must not inflate the ordinary itr union");
            List<SceneQueryHit> mixedCandidates = CollectCollisionAuditCandidates(pollutionWorld, mixedAttacker, false);
            Expect(mixedCandidates.Count == 0,
                "BATTLE-AUDIT3-03: rejected kind5 and non-overlapping ordinary itr must produce no candidate");

            AssertCollisionAuditGeometryCandidate(
                "BATTLE-AUDIT3-04:y80000",
                MakeCollisionAuditItr(3, 15, 80000, 45, 65, 20, vrest: 1),
                new BodyBox { kind = 0, x = 15, y = 80000, w = 45, h = 65 });
            AssertCollisionAuditGeometryCandidate(
                "BATTLE-AUDIT3-13:full-height",
                MakeCollisionAuditItr(0, -20, -20, 40, 40, 20, vrest: 1),
                new BodyBox { kind = 0, x = -200, y = int.MinValue, w = 900, h = 999 });
            AssertCollisionAuditGeometryCandidate(
                "BATTLE-AUDIT3-13:large-nonzero-kind",
                MakeCollisionAuditItr(0, -6000, 700, 6000, 5000, 6000, vrest: 1),
                new BodyBox { kind = 7, x = -6000, y = 700, w = 6000, h = 5000 });
        }

        private static void CheckCollisionAudit3NearestAndBodyXAcrossModes()
        {
            int[] modes = { 0, 1, 2 };
            for (int i = 0; i < modes.Length; i++)
            {
                int mode = modes[i];
                InteractionArea nearestItr = MakeCollisionAuditItr(0, -100, -40, 200, 80, 20, vrest: 0);
                var nearestWorld = new SimulationWorld();
                nearestWorld.Runtime.Match.BattleGameModeId = mode;
                LF2Character attacker = CreateInteractionCharacter(
                    $"SelfCheck_Audit3NearestAttacker_{mode}",
                    1,
                    BuildCollisionAuditData(
                        $"SelfCheck_Audit3NearestAttacker_{mode}",
                        BuildCollisionAuditFrame(0, LF2States.Standing, nearestItr, null)));
                LF2Character farTarget = CreateInteractionCharacter(
                    $"SelfCheck_Audit3NearestFar_{mode}",
                    37,
                    BuildInteractionVictimData($"SelfCheck_Audit3NearestFar_{mode}", 37));
                LF2Character nearTarget = CreateInteractionCharacter(
                    $"SelfCheck_Audit3NearestNear_{mode}",
                    38,
                    BuildInteractionVictimData($"SelfCheck_Audit3NearestNear_{mode}", 38));
                nearestWorld.Register(attacker);
                ConfigureCollisionAuditEntity(attacker, 1, 0);
                nearestWorld.Register(farTarget);
                ConfigureCollisionAuditEntity(farTarget, 2, 30);
                nearestWorld.Register(nearTarget);
                ConfigureCollisionAuditEntity(nearTarget, 2, 10);

                List<SceneQueryHit> nearestCandidates = CollectCollisionAuditCandidates(nearestWorld, attacker, true);
                Expect(nearestCandidates.Count == 1 && nearestCandidates[0].Target == nearTarget,
                    $"BATTLE-AUDIT3-14: nearest path must select the nearest target in mode {mode}");

                InteractionArea bodyXItr = MakeCollisionAuditItr(0, 1000, -20, 40, 40, 20, vrest: 0);
                var bodyXWorld = new SimulationWorld();
                bodyXWorld.Runtime.Match.BattleGameModeId = mode;
                LF2Character bodyXAttacker = CreateInteractionCharacter(
                    $"SelfCheck_Audit3BodyXAttacker_{mode}",
                    1,
                    BuildCollisionAuditData(
                        $"SelfCheck_Audit3BodyXAttacker_{mode}",
                        BuildCollisionAuditFrame(0, LF2States.Standing, bodyXItr, null)));
                LF2FrameData bodyXTargetFrame = BuildCollisionAuditFrame(
                    0,
                    LF2States.Standing,
                    null,
                    new BodyBox { kind = 0, x = 1000, y = -20, w = 40, h = 40 });
                LF2Character bodyXTarget = CreateInteractionCharacter(
                    $"SelfCheck_Audit3BodyXTarget_{mode}",
                    37,
                    BuildCollisionAuditData($"SelfCheck_Audit3BodyXTarget_{mode}", bodyXTargetFrame));
                RegisterCollisionAuditPair(bodyXWorld, bodyXAttacker, bodyXTarget, 5, 2);

                List<SceneQueryHit> bodyXCandidates = CollectCollisionAuditCandidates(bodyXWorld, bodyXAttacker, true);
                Expect(bodyXCandidates.Count == 0,
                    $"BATTLE-AUDIT3-14: bodyX>=1000 nearest rejection must apply in mode {mode}");
            }
        }

        private static void CheckCollisionAudit3Oid9PairGate()
        {
            var gateMethod = typeof(BruteForceSceneQuery).GetMethod(
                "IsBlockedReleasePair",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Expect(gateMethod != null,
                "BATTLE-AUDIT3-15: oid9 pair-gate reflection contract changed");

            int[] blockedOids = { 200, 203, 205, 206, 207, 215, 216 };
            for (int i = 0; i < blockedOids.Length; i++)
            {
                bool blocked = InvokeCollisionAuditOid9Gate(
                    gateMethod,
                    blockedOids[i],
                    targetOid: 9,
                    targetFrameId: 301,
                    hitA: 999,
                    hitD: 999,
                    hitJ: 999,
                    attackerRelation: 7,
                    targetRelation: 7);
                Expect(blocked,
                    $"BATTLE-AUDIT3-15: oid {blockedOids[i]} must be blocked by the complete oid9/frame301 pair gate");
            }

            Expect(!InvokeCollisionAuditOid9Gate(gateMethod, 199, 9, 301, 999, 999, 999, 7, 7),
                "BATTLE-AUDIT3-15: an oid outside the release set must not be blocked");
            Expect(!InvokeCollisionAuditOid9Gate(gateMethod, 205, 8, 301, 999, 999, 999, 7, 7),
                "BATTLE-AUDIT3-15: a victim other than oid9 must not be blocked");
            Expect(!InvokeCollisionAuditOid9Gate(gateMethod, 205, 9, 300, 999, 999, 999, 7, 7),
                "BATTLE-AUDIT3-15: an oid9 victim outside frame301 must not be blocked");
            Expect(!InvokeCollisionAuditOid9Gate(gateMethod, 205, 9, 301, 999, 998, 999, 7, 7),
                "BATTLE-AUDIT3-15: all three hit fields must be 999");
            Expect(!InvokeCollisionAuditOid9Gate(gateMethod, 205, 9, 301, 999, 999, 999, 7, 8),
                "BATTLE-AUDIT3-15: different relation teams must not be blocked");
            Expect(!InvokeCollisionAuditOid9Gate(gateMethod, 205, 9, 301, 999, 999, 999, 0, 0),
                "BATTLE-AUDIT3-15: relation team zero must not be blocked");
        }

        private static void CheckCollisionAudit3FrameSourceGates()
        {
            InteractionArea ordinaryItr = MakeCollisionAuditItr(0, -20, -20, 40, 40, 20, vrest: 1);
            LF2FrameData standing = BuildCollisionAuditFrame(0, LF2States.Standing, ordinaryItr, null);
            LF2FrameData burning = BuildCollisionAuditFrame(1, LF2States.Burning, ordinaryItr.ShallowCopy(), null);

            List<SceneQueryHit> prev2Burning = CollectCollisionAuditFrameSourceCase(
                "SelfCheck_Audit3Prev2Burning",
                standing,
                burning,
                sameTeam: true);
            Expect(prev2Burning.Count == 1,
                "BATTLE-AUDIT3-16: same-team burning exception must read PrevFrame2/collision state");

            List<SceneQueryHit> currentBurning = CollectCollisionAuditFrameSourceCase(
                "SelfCheck_Audit3CurrentBurning",
                burning,
                standing,
                sameTeam: true);
            Expect(currentBurning.Count == 0,
                "BATTLE-AUDIT3-16: current burning state must not replace a non-burning PrevFrame2 state");

            InteractionArea kind8NormalItr = MakeCollisionAuditItr(8, -20, -20, 40, 40, 20, vrest: 1);
            LF2FrameData normalKind8 = BuildCollisionAuditFrame(0, LF2States.Standing, kind8NormalItr, null);
            LF2FrameData leadKind8 = BuildCollisionAuditFrame(
                1,
                LF2States.ObjectFlying,
                kind8NormalItr.ShallowCopy(),
                null);
            leadKind8.hit_Fa = 1;

            List<SceneQueryHit> currentLead = CollectCollisionAuditFrameSourceCase(
                "SelfCheck_Audit3Kind8CurrentLead",
                leadKind8,
                normalKind8,
                sameTeam: false);
            Expect(currentLead.Count == 0,
                "BATTLE-AUDIT3-17: kind8 state3005 lead-in must read the active/current frame");

            List<SceneQueryHit> prev2Lead = CollectCollisionAuditFrameSourceCase(
                "SelfCheck_Audit3Kind8Prev2Lead",
                normalKind8,
                leadKind8,
                sameTeam: false);
            Expect(prev2Lead.Count == 1,
                "BATTLE-AUDIT3-17: a state3005 PrevFrame2 must not defer kind8 when current frame is ordinary");
        }

        private static List<SceneQueryHit> CollectCollisionAuditFrameSourceCase(
            string name,
            LF2FrameData currentFrame,
            LF2FrameData collisionFrame,
            bool sameTeam)
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateInteractionCharacter(
                name + "_Attacker",
                1,
                BuildCollisionAuditData(name + "_Attacker", currentFrame, collisionFrame));
            LF2Character target = CreateInteractionCharacter(
                name + "_Target",
                37,
                BuildInteractionVictimData(name + "_Target", 37));
            RegisterCollisionAuditPair(world, attacker, target, 1, sameTeam ? 1 : 2);
            SetCollisionAuditFramePair(attacker, currentFrame, collisionFrame);
            SetCollisionAuditFramePair(target, target.Frame.D, target.Frame.D);
            return CollectCollisionAuditCandidates(world, attacker, false);
        }

        private static bool InvokeCollisionAuditOid9Gate(
            System.Reflection.MethodInfo gateMethod,
            int attackerOid,
            int targetOid,
            int targetFrameId,
            int hitA,
            int hitD,
            int hitJ,
            int attackerRelation,
            int targetRelation)
        {
            LF2Character attacker = CreateInteractionCharacter(
                $"SelfCheck_Audit3PairAttacker_{attackerOid}",
                attackerOid,
                BuildInteractionVictimData($"SelfCheck_Audit3PairAttacker_{attackerOid}", attackerOid));
            LF2FrameData targetFrame = BuildCollisionAuditFrame(
                targetFrameId,
                LF2States.Standing,
                null,
                new BodyBox { kind = 0, x = -20, y = -20, w = 40, h = 40 });
            targetFrame.hit_a = hitA;
            targetFrame.hit_d = hitD;
            targetFrame.hit_j = hitJ;
            LF2FrameData targetFrame0 = targetFrameId == 0
                ? targetFrame
                : BuildCollisionAuditFrame(
                    0,
                    LF2States.Standing,
                    null,
                    new BodyBox { kind = 0, x = -20, y = -20, w = 40, h = 40 });
            LF2Character target = CreateInteractionCharacter(
                $"SelfCheck_Audit3PairTarget_{targetOid}_{targetFrameId}",
                targetOid,
                targetFrameId == 0
                    ? BuildCollisionAuditData("SelfCheck_Audit3PairTarget", targetFrame)
                    : BuildCollisionAuditData("SelfCheck_Audit3PairTarget", targetFrame0, targetFrame));
            attacker.RelationTeam = attackerRelation;
            target.RelationTeam = targetRelation;
            SetCollisionAuditFramePair(target, targetFrame, targetFrame);
            return (bool)gateMethod.Invoke(null, new object[] { attacker, target });
        }

        private static void AssertCollisionAuditGeometryCandidate(
            string label,
            InteractionArea itr,
            BodyBox body)
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateInteractionCharacter(
                label + "_Attacker",
                1,
                BuildCollisionAuditData(
                    label + "_Attacker",
                    BuildCollisionAuditFrame(0, LF2States.Standing, itr, null)));
            LF2Character target = CreateInteractionCharacter(
                label + "_Target",
                37,
                BuildCollisionAuditData(
                    label + "_Target",
                    BuildCollisionAuditFrame(0, LF2States.Standing, null, body)));
            RegisterCollisionAuditPair(world, attacker, target, 1, 2);
            List<SceneQueryHit> candidates = CollectCollisionAuditCandidates(world, attacker, true);
            Expect(candidates.Count == 1 && candidates[0].Target == target,
                $"{label}: authored release geometry must remain collision-eligible");
        }

        private static List<SceneQueryHit> CollectCollisionAuditCandidates(
            SimulationWorld world,
            LF2Entity attacker,
            bool captureSnapshots)
        {
            if (captureSnapshots)
                world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            var query = world.SceneQuery as BruteForceSceneQuery;
            Expect(query != null,
                "BATTLE-AUDIT3 collision regression requires BruteForceSceneQuery");
            Expect(query.TryGetCollisionCandidateSequence(attacker, out List<SceneQueryHit> candidates),
                "BATTLE-AUDIT3 collision regression must expose the candidate carrier");
            var snapshot = new List<SceneQueryHit>(candidates);
            world.EndCollisionCandidateConsumption();
            return snapshot;
        }

        private static void RegisterCollisionAuditPair(
            SimulationWorld world,
            LF2Character attacker,
            LF2Character target,
            int attackerRelation,
            int targetRelation)
        {
            world.Register(attacker);
            ConfigureCollisionAuditEntity(attacker, attackerRelation, 0);
            world.Register(target);
            ConfigureCollisionAuditEntity(target, targetRelation, 0);
        }

        private static void ConfigureCollisionAuditEntity(LF2Entity entity, int relationTeam, double x)
        {
            entity.Team = relationTeam;
            entity.RelationTeam = relationTeam;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.FrameDelay = 0;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(x, 0.0, 0.0);
            entity.Runtime.SetVelocity(0.0, 0.0, 0.0);
            entity.Runtime.SyncIntegerPosition();
        }

        private static void SetCollisionAuditFramePair(
            LF2Entity entity,
            LF2FrameData currentFrame,
            LF2FrameData collisionFrame)
        {
            entity.Frame.N = currentFrame.frameId;
            entity.Frame.D = currentFrame;
            entity.Frame.Prev2 = collisionFrame.frameId;
            entity.Frame.Prev2D = collisionFrame;
        }

        private static LF2CharacterData BuildCollisionAuditData(string name, params LF2FrameData[] frames)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData>(frames),
            };
        }

        private static LF2FrameData BuildCollisionAuditFrame(
            int frameId,
            int state,
            InteractionArea itr,
            BodyBox body)
        {
            LF2FrameData frame = Frame(frameId, state, 1, frameId, 0, 0);
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static InteractionArea MakeCollisionAuditItr(
            int kind,
            int x,
            int y,
            int w,
            int h,
            int zwidth,
            int vrest)
        {
            return new InteractionArea
            {
                kind = kind,
                x = x,
                y = y,
                w = w,
                h = h,
                zwidth = zwidth,
                injury = 10,
                dvx = 1,
                arest = 4,
                vrest = vrest,
                effect = 0,
            };
        }

        private static void CheckCollisionCandidateCapAndNewbornIsolation()
        {
            InteractionArea itr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheck_CandidateCapAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(itr) },
            };
            var world = new SimulationWorld();
            LF2Character attacker = CreateInteractionCharacter("SelfCheck_CandidateCapAttacker", 1, attackerData);
            world.Register(attacker);
            attacker.Team = 1;
            attacker.RelationTeam = 1;
            attacker.Runtime.SetPosition(0, 0, 0);
            attacker.Runtime.SyncIntegerPosition();

            var targets = new List<LF2Character>();
            for (int i = 0; i < 21; i++)
            {
                LF2Character target = CreateInteractionCharacter(
                    $"SelfCheck_CandidateCapTarget_{i}",
                    100 + i,
                    BuildInteractionVictimData($"SelfCheckCandidateCapTarget{i}", 100 + i));
                world.Register(target);
                target.Team = 2;
                target.RelationTeam = 2;
                target.Runtime.SetPosition(0, 0, 0);
                target.Runtime.SyncIntegerPosition();
                targets.Add(target);
            }

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            var query = world.SceneQuery as BruteForceSceneQuery;
            Expect(query != null,
                "candidate cap self-check must use the production BruteForceSceneQuery");
            bool hasCandidateCarrier = query.TryGetCollisionCandidateSequence(attacker, out List<SceneQueryHit> candidates);
            Expect(hasCandidateCarrier,
                "candidate cap self-check must consume the production collision carrier");
            Expect(attacker.Runtime.HitCandidateCount == 20 && candidates.Count == 20,
                "collision collection must cap an ordinary attacker carrier at 20 candidates");
            for (int i = 0; i < 20; i++)
            {
                Expect(candidates[i].Target == targets[i],
                    $"collision candidate {i} must preserve runtime-slot scan order");
            }
            Expect(!candidates.Exists(hit => hit.Target == targets[20]),
                "the 21st runtime-slot target must be excluded by the 20-candidate cap");

            LF2Character newborn = CreateInteractionCharacter(
                "SelfCheck_Step8Newborn",
                999,
                BuildInteractionVictimData("SelfCheckStep8Newborn", 999));
            world.Register(newborn);
            newborn.Team = 2;
            newborn.RelationTeam = 2;
            newborn.Runtime.SetPosition(0, 0, 0);
            newborn.Runtime.SyncIntegerPosition();

            Expect(query.TryGetCollisionCandidateSequence(attacker, out List<SceneQueryHit> afterSpawn) &&
                   afterSpawn.Count == 20 && !afterSpawn.Exists(hit => hit.Target == newborn),
                "a step8 newborn must not enter an existing step6 collision candidate carrier");
            Expect(query.TryGetCollisionCandidateSequence(newborn, out List<SceneQueryHit> newbornCarrier) &&
                   newbornCarrier.Count == 0 && newborn.Runtime.HitCandidateCount == 0,
                "a step8 newborn must not receive a retroactive candidate carrier in the same tick");

            world.EndCollisionCandidateConsumption();
        }

        private static void CheckSpecialAttackStep4AndLateFrameTick()
        {
            LF2FrameData frame0 = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            frame0.hit_a = 5;
            var data = new LF2CharacterData
            {
                name = "SelfCheck_SpecialStep4Late",
                frames = new List<LF2FrameData> { frame0 },
            };
            var world = new SimulationWorld();
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData("SelfCheck_SpecialStep4Late", 200, data);
            world.Register(special);
            special.Health.HP = 20;
            special.AttackingCounter = 0;
            special.FrameDelay = 0;
            special.Trans.SyncDirectFrameData(frame0.wait, frame0.next, 0);

            world.SerialTickAll(1);

            Expect(special.Frame.N == 0 && special.AttackingCounter == 0,
                "SpecialAttack step4 must run TU only and must not advance frame_tick wait/next state");
            Expect(special.Health.HP == 20,
                "SpecialAttack step4 TU must not apply the type3 frame_tick hit_a drain");

            world.LateEntityUpdateAll(1);

            Expect(special.Health.HP == 15 && special.AttackingCounter == 1,
                "SpecialAttack late update must advance frame_tick once and apply hit_a exactly once");

            world.LateEntityUpdateAll(2);

            Expect(special.Health.HP == 10 && special.AttackingCounter == 2,
                "each subsequent late pass must apply one, and only one, type3 hit_a drain");
        }

        private static void CheckCurrentDatFrameLogicSharedRouting()
        {
            LF2CharacterData otherData = BuildHitFa10FrameLogicData(
                "SelfCheck_HitFa10Other",
                LF2ObjectType.Other);
            LF2CharacterData specialData = BuildHitFa10FrameLogicData(
                "SelfCheck_HitFa10Special",
                LF2ObjectType.SpecialAttack);
            var world = new SimulationWorld();

            var other = new TransformedLandingSelfCheckEntity();
            other.BindSource(otherData);
            other.Runtime.SetPosition(0, 9, 0);
            other.Runtime.SetVelocity(1, 0, 0);
            other.Runtime.SyncIntegerPosition();
            other.SwitchDir("left");
            world.Register(other);

            var characterShell = new BoundsSelfCheckCharacter(LF2ObjectType.SpecialAttack);
            characterShell.BindData("SelfCheck_HitFa10CharacterShell", 741, specialData);
            characterShell.Runtime.SetPosition(0, 9, 0);
            characterShell.Runtime.SetVelocity(-1, 0, 0);
            characterShell.Runtime.SyncIntegerPosition();
            characterShell.SwitchDir("right");
            world.Register(characterShell);

            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData("SelfCheck_HitFa10Special", 742, specialData);
            special.Runtime.SetPosition(0, 9, 0);
            special.Runtime.SetVelocity(1, 0, 0);
            special.Runtime.SyncIntegerPosition();
            special.SwitchDir("left");
            world.Register(special);

            world.FrameLogicBeforeAdvanceAll(1);

            Expect(Nearly(other.Runtime.Vx, 2.1) && Nearly(other.Runtime.Y, 3.0) && other.Runtime.Dir == "right",
                "LF2OtherObject current non-character DAT must execute shared hit_Fa=10 in the pre-advance pass");
            Expect(Nearly(characterShell.Runtime.Vx, -2.1) &&
                   Nearly(characterShell.Runtime.Y, 3.0) &&
                   characterShell.Runtime.Dir == "left",
                "a character CLR shell with current SpecialAttack DAT must execute the shared non-character frame logic");
            Expect(Nearly(special.Runtime.Vx, 2.1) && Nearly(special.Runtime.Y, 3.0) && special.Runtime.Dir == "right",
                "LF2SpecialAttack must execute hit_Fa=10 through the shared base frame-logic path");

            special.SimTU(1);
            Expect(Nearly(special.Runtime.Vx, 1.1) && Nearly(special.Runtime.X, 2.1),
                "LF2SpecialAttack SimTU must apply one ground-friction tick after moving by pre-friction Vx, " +
                "without a second hit_Fa execution (which would leave Vx=2.2)");

            CheckCurrentDatFrameLogicRepresentativeCategories();
        }

        private static void CheckCurrentDatFrameLogicRepresentativeCategories()
        {
            CheckCurrentDatHitFa3Routing();
            CheckCurrentDatHitFa4CatchRouting();
            CheckCurrentDatHitFa14Routing();
        }

        private static void CheckCurrentDatHitFa3Routing()
        {
            SimulationWorld world = CreateCurrentDatFrameLogicWorld(
                "HitFa3",
                hitFa: 3,
                includeFrame50: false,
                includeFrame60: false,
                out LF2Entity[] sources,
                out LF2Character target);
            target.Runtime.SetPosition(100.0, 0.0, 20.0);
            target.Runtime.SyncIntegerPosition();
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].Runtime.SetPosition(0.0, 0.0, 0.0);
                sources[i].Runtime.SetVelocity(0.0, 0.0, 0.0);
                sources[i].Runtime.SyncIntegerPosition();
            }

            world.FrameLogicBeforeAdvanceAll(1);
            for (int i = 0; i < sources.Length; i++)
            {
                Expect(Nearly(sources[i].Runtime.Vx, 0.7) && Nearly(sources[i].Runtime.Vz, 0.17) &&
                       sources[i].OwnerEntityIndex == target.Runtime.SlotIndex,
                    $"BATTLE-AUDIT3-10: hit_Fa=3 tick1 must run exactly once for {sources[i].GetType().Name}");
            }

            world.FrameLogicBeforeAdvanceAll(2);
            for (int i = 0; i < sources.Length; i++)
            {
                Expect(Nearly(sources[i].Runtime.Vx, 1.4) && Nearly(sources[i].Runtime.Vz, 0.34),
                    $"BATTLE-AUDIT3-10: hit_Fa=3 tick2 must add exactly one tracking step for {sources[i].GetType().Name}");
            }
        }

        private static void CheckCurrentDatHitFa4CatchRouting()
        {
            SimulationWorld world = CreateCurrentDatFrameLogicWorld(
                "HitFa4",
                hitFa: 4,
                includeFrame50: false,
                includeFrame60: true,
                out LF2Entity[] sources,
                out LF2Character target);
            target.Runtime.SetPosition(100.0, 30.0, 20.0);
            target.Runtime.SyncIntegerPosition();
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].Runtime.SetPosition(90.0, 0.0, 20.0);
                sources[i].Runtime.SetVelocity(3.0, 4.0, 5.0);
                sources[i].Runtime.SyncIntegerPosition();
                sources[i].OwnerEntityIndex = target.Runtime.SlotIndex;
            }

            world.FrameLogicBeforeAdvanceAll(1);
            for (int i = 0; i < sources.Length; i++)
            {
                Expect(sources[i].Frame.N == 60 &&
                       Nearly(sources[i].Runtime.Vx, 0.0) &&
                       Nearly(sources[i].Runtime.Vy, 0.0) &&
                       Nearly(sources[i].Runtime.Vz, 0.0),
                    $"BATTLE-AUDIT3-10: hit_Fa=4 must enter the catch frame through {sources[i].GetType().Name}");
            }
            Expect(target.CatchTimer == 100,
                "BATTLE-AUDIT3-10: hit_Fa=4 shared routing must arm the target catch timer");
        }

        private static void CheckCurrentDatHitFa14Routing()
        {
            SimulationWorld world = CreateCurrentDatFrameLogicWorld(
                "HitFa14",
                hitFa: 14,
                includeFrame50: true,
                includeFrame60: false,
                out LF2Entity[] sources,
                out LF2Character target);
            target.Runtime.SetPosition(100.0, 50.0, 20.0);
            target.Runtime.SyncIntegerPosition();
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].Runtime.SetPosition(0.0, 0.0, 0.0);
                sources[i].Runtime.SetVelocity(0.0, 0.0, 0.0);
                sources[i].Runtime.SyncIntegerPosition();
            }

            world.FrameLogicBeforeAdvanceAll(1);
            for (int i = 0; i < sources.Length; i++)
            {
                Expect(sources[i].Frame.N == 50 &&
                       Nearly(sources[i].Runtime.Vx, 0.7) &&
                       Nearly(sources[i].Runtime.Vz, 0.4) &&
                       Nearly(sources[i].Runtime.Y, 1.0),
                    $"BATTLE-AUDIT3-10: hit_Fa=14 tick1 must run one frame-band tracking step for {sources[i].GetType().Name}");
            }

            world.FrameLogicBeforeAdvanceAll(2);
            for (int i = 0; i < sources.Length; i++)
            {
                Expect(sources[i].Frame.N == 50 &&
                       Nearly(sources[i].Runtime.Vx, 1.4) &&
                       Nearly(sources[i].Runtime.Vz, 0.8) &&
                       Nearly(sources[i].Runtime.Y, 1.4),
                    $"BATTLE-AUDIT3-10: hit_Fa=14 tick2 must add exactly one tracking step for {sources[i].GetType().Name}");
            }
        }

        private static SimulationWorld CreateCurrentDatFrameLogicWorld(
            string label,
            int hitFa,
            bool includeFrame50,
            bool includeFrame60,
            out LF2Entity[] sources,
            out LF2Character target)
        {
            LF2CharacterData otherData = BuildRepresentativeHitFaData(
                $"SelfCheck_{label}_Other", LF2ObjectType.Other, hitFa, includeFrame50, includeFrame60);
            LF2CharacterData specialData = BuildRepresentativeHitFaData(
                $"SelfCheck_{label}_Special", LF2ObjectType.SpecialAttack, hitFa, includeFrame50, includeFrame60);
            var world = new SimulationWorld();

            target = CreateCharacter(
                $"SelfCheck_{label}_Target",
                37,
                new LF2CharacterData
                {
                    name = $"SelfCheck_{label}_Target",
                    type_sub = (int)LF2ObjectType.Character,
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, LF2States.Standing, 100, 0, 39, 79),
                    },
                });
            target.Team = 2;
            target.RelationTeam = 2;
            world.Register(target);

            var other = new TransformedLandingSelfCheckEntity();
            other.BindSource(otherData);
            var characterShell = new BoundsSelfCheckCharacter(LF2ObjectType.SpecialAttack);
            characterShell.BindData($"SelfCheck_{label}_CharacterShell", 741, specialData);
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData($"SelfCheck_{label}_Special", 742, specialData);
            sources = new LF2Entity[] { other, characterShell, special };

            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].Team = 1;
                sources[i].RelationTeam = 1;
                world.Register(sources[i]);
            }

            return world;
        }

        private static LF2CharacterData BuildRepresentativeHitFaData(
            string name,
            LF2ObjectType currentDataType,
            int hitFa,
            bool includeFrame50,
            bool includeFrame60)
        {
            LF2FrameData frame0 = Frame(0, LF2States.ProjectileFlying, 100, 0, 39, 79);
            frame0.hit_Fa = hitFa;
            var frames = new List<LF2FrameData> { frame0 };
            if (includeFrame50)
            {
                LF2FrameData frame50 = Frame(50, LF2States.ProjectileFlying, 100, 50, 39, 79);
                frame50.hit_Fa = hitFa;
                frames.Add(frame50);
            }
            if (includeFrame60)
                frames.Add(Frame(60, LF2States.ProjectileFlying, 100, 60, 39, 79));

            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)currentDataType,
                frames = frames,
            };
        }

        private static LF2CharacterData BuildHitFa10FrameLogicData(string name, LF2ObjectType currentDataType)
        {
            LF2FrameData frame = Frame(0, LF2States.ProjectileFlying, 100, 0, 39, 79);
            frame.hit_Fa = 10;
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)currentDataType,
                frames = new List<LF2FrameData> { frame },
            };
        }

        private static void CheckFrameTickPpDisplayAndCurrentDatMatrix()
        {
            LF2FrameData ppSource = Frame(0, LF2States.Standing, 0, 1, 39, 79);
            LF2FrameData ppCost = Frame(1, LF2States.Standing, 10, 1, 39, 79);
            ppCost.mp = -30;
            ppCost.hit_d = 2;
            var ppData = new LF2CharacterData
            {
                name = "SelfCheck_PpDisplayFrameTick",
                frames = new List<LF2FrameData>
                {
                    ppSource,
                    ppCost,
                    Frame(2, LF2States.Standing, 1, 2, 39, 79),
                },
            };
            var ppWorld = new SimulationWorld();
            LF2Character ppCharacter = CreateCharacter("SelfCheck_PpDisplayFrameTick", 1, ppData);
            ppWorld.Register(ppCharacter);
            ppCharacter.Health.PP = 100;
            ppCharacter.PpDisplay = 0;
            ppCharacter.AttackingCounter = 0;
            ppCharacter.Trans.SyncDirectFrameData(ppSource.wait, ppSource.next, 0);

            ppCharacter.SimFrameTick(1);

            Expect(ppCharacter.Frame.N == 1 && ppCharacter.Health.PP == 70,
                "negative frame mp must consume PP after a real wait/next transition");
            Expect(ppCharacter.PpDisplay == 30,
                "PP consumption must increase PpDisplay with a positive cost sign");

            LF2FrameData type3Frame = Frame(0, LF2States.ProjectileFlying, 0, 999, 39, 79);
            type3Frame.hit_a = 4;
            LF2FrameData caughtFrame = Frame(10, LF2States.ProjectileFlying, 0, 999, 39, 79, new CatchPoint { kind = 2 });
            caughtFrame.hit_a = 4;
            var type3Data = new LF2CharacterData
            {
                name = "SelfCheck_CurrentDatType3Shell",
                frames = new List<LF2FrameData>
                {
                    type3Frame,
                    caughtFrame,
                    Frame(212, LF2States.Jump, 1, 212, 39, 79),
                },
            };
            var type3World = new SimulationWorld();
            var characterShell = new BoundsSelfCheckCharacter(LF2ObjectType.SpecialAttack);
            characterShell.BindData("SelfCheck_CurrentDatType3Shell", 200, type3Data);
            type3World.Register(characterShell);
            characterShell.Health.HP = 20;
            characterShell.FrameDelay = 2;
            characterShell.Runtime.SetPosition(0, -10, 0);
            characterShell.Runtime.SyncIntegerPosition();
            characterShell.Trans.SyncDirectFrameData(type3Frame.wait, type3Frame.next, 0);

            characterShell.SimFrameTick(1);

            Expect(characterShell.Health.HP == 16 && characterShell.FrameDelay == 2,
                "a character CLR shell with current type3 DAT must run frame_tick despite nonzero FrameDelay");
            Expect(characterShell.Frame.N == 0,
                "current type3 DAT next=999 must resolve to frame0 even when the CLR shell is a character and airborne");

            characterShell.Health.HP = 20;
            characterShell.Runtime.LinkState = -1;
            characterShell.SimFrameTick(2);
            Expect(characterShell.Health.HP == 20,
                "current type3 DAT frame_tick must honor the shared negative-link gate before hit_a");

            characterShell.Runtime.LinkState = 0;
            characterShell.SetCpointRawFramePreserveWait(10);
            characterShell.SimFrameTick(3);
            Expect(characterShell.Health.HP == 20 && characterShell.Frame.N == 10,
                "current type3 DAT frame_tick must honor the caught cpoint kind2 gate before hit_a/next");

            var characterData = new LF2CharacterData
            {
                name = "SelfCheck_CurrentDatCharacterShell",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 999, 39, 79),
                    Frame(10, LF2States.Standing, 0, 999, 39, 79, new CatchPoint { kind = 2 }),
                    Frame(212, LF2States.Jump, 1, 212, 39, 79),
                },
            };
            var characterWorld = new SimulationWorld();
            LF2Entity specialShell = CreateCpointMatrixEntity(false, "SelfCheck_CurrentDatCharacterShell", 1, characterData);
            characterWorld.Register(specialShell);
            specialShell.Runtime.SetPosition(0, -10, 0);
            specialShell.Runtime.SyncIntegerPosition();
            specialShell.FrameDelay = 1;
            specialShell.Trans.SyncDirectFrameData(0, 999, 0);

            specialShell.SimFrameTick(1);
            Expect(specialShell.Frame.N == 0,
                "a SpecialAttack CLR shell with current character DAT must honor the character FrameDelay gate");

            specialShell.SimTU(1);
            Expect(specialShell.FrameDelay == 0 && specialShell.Frame.N == 0,
                "current character DAT step4 must decay FrameDelay before returning without dynamics");
            specialShell.Runtime.LinkState = -1;
            double heldY = specialShell.Runtime.Y;
            specialShell.SimTU(2);
            Expect(Nearly(specialShell.Runtime.Y, heldY),
                "current character DAT step4 must honor the negative-link dynamics gate");

            specialShell.Runtime.LinkState = 0;
            specialShell.SimFrameTick(2);
            Expect(specialShell.Frame.N == 212,
                "current character DAT next=999 must resolve airborne shells to frame212");

            LF2FrameData heavyFrame = Frame(0, LF2States.HeavyWeaponInSky, 10, 0, 39, 79);
            var heavyData = new LF2CharacterData
            {
                name = "SelfCheck_CurrentDatHeavyShell",
                frames = new List<LF2FrameData>
                {
                    heavyFrame,
                    Frame(20, LF2States.HeavyWeaponOnGround, 1, 20, 39, 79),
                },
            };
            var heavyWorld = new SimulationWorld();
            var heavyShell = new BoundsSelfCheckCharacter(LF2ObjectType.HeavyWeapon);
            heavyShell.BindData("SelfCheck_CurrentDatHeavyShell", 150, heavyData);
            heavyWorld.Register(heavyShell);
            heavyShell.Runtime.SetPosition(0, 0, 0);
            heavyShell.Runtime.SetVelocity(0, 0, 0);
            heavyShell.Runtime.SyncIntegerPosition();
            heavyShell.Trans.SyncDirectFrameData(heavyFrame.wait, heavyFrame.next, 0);

            heavyShell.SimFrameTick(1);

            Expect(heavyShell.Frame.N == 0,
                "a character CLR shell with current type2 DAT must keep state2000 at exact ground height without a downward crossing");

            var lyingData = new LF2CharacterData
            {
                name = "SelfCheck_State14FrameTick",
                frames = new List<LF2FrameData> { Frame(0, LF2States.Lying, 10, 0, 39, 79) },
            };
            var lyingWorld = new SimulationWorld();
            LF2Character lying = CreateCharacter("SelfCheck_State14FrameTick", 1, lyingData);
            lyingWorld.Register(lying);
            lying.Health.HP = 0;
            lying.KillCount = -1;
            lying.RelationTeam = 1;
            lying.HitStun = 0;

            lying.SimFrameTick(1);
            Expect(lying.HitStun == 0 && lying.AttackingCounter == 0,
                "state14 HP<=0 must not arm hit stop for an ordinary unowned low runtime slot");

            lying.KillCount = 0;
            lying.SimFrameTick(2);
            Expect(lying.HitStun == 30 && lying.AttackingCounter == 0,
                "state14 HP<=0 must arm mapped hit stop for an owned entity and keep attacking cleared");
        }

        private static void CheckGameTickInputClearBoundaries()
        {
            var characterData = new LF2CharacterData
            {
                name = "SelfCheck_GameTickInputClear",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 100, 0, 39, 79),
                },
            };

            LF2Character realCharacter = CreateCharacter("SelfCheck_InputClearReal", 760, characterData);
            var sharedCharacter = new SelfCheckCharacterDatShell();
            sharedCharacter.BindData("SelfCheck_InputClearShared", 761, characterData);
            var nonCharacterProbe = new FrameAdvanceInputProbeEntity();
            var clearWorld = new SimulationWorld();
            clearWorld.Register(realCharacter);
            clearWorld.Register(sharedCharacter);
            clearWorld.Register(nonCharacterProbe);
            FillInputState(realCharacter.Runtime);
            FillInputState(sharedCharacter.Runtime);
            FillInputState(nonCharacterProbe.Runtime);
            clearWorld.SetNeedClearInput(true);

            new NTSDBattleTickSystem(clearWorld).RunReleaseTick(1);

            Expect(!clearWorld.NeedClearInput && clearWorld.CurrentTickIndex == 1,
                "GT-01: battle-entry input clear must consume NeedClearInput on the first tick");
            ExpectInputStateReset(realCharacter.Runtime, "GT-01 real character");
            ExpectInputStateReset(sharedCharacter.Runtime, "GT-01 shared character-DAT shell");
            Expect(nonCharacterProbe.Runtime.KeyAttack == 1 &&
                   nonCharacterProbe.Runtime.PrevAttack == 1 &&
                   nonCharacterProbe.Runtime.CdAttack == 5,
                "GT-01: battle-entry input clear must not reset non-character DAT input storage");
            Expect(nonCharacterProbe.TransitCount == 0 && nonCharacterProbe.TuCount == 0,
                "GT-01: NeedClearInput must return the whole tick before frame advance and later passes");

            LF2Character serialReal = CreateCharacter("SelfCheck_FrameAdvanceClearReal", 762, characterData);
            var serialShared = new SelfCheckCharacterDatShell();
            serialShared.BindData("SelfCheck_FrameAdvanceClearShared", 763, characterData);
            var serialProbe = new FrameAdvanceInputProbeEntity();
            var serialWorld = new SimulationWorld();
            serialWorld.Register(serialReal);
            serialWorld.Register(serialShared);
            serialWorld.Register(serialProbe);
            FillFrameAdvanceInputKeys(serialReal.Runtime);
            FillFrameAdvanceInputKeys(serialShared.Runtime);
            FillFrameAdvanceInputKeys(serialProbe.Runtime);

            serialWorld.SerialTickAll(2);

            ExpectCurrentInputKeysCleared(serialReal.Runtime, "GT-02 real character");
            ExpectCurrentInputKeysCleared(serialShared.Runtime, "GT-02 shared character-DAT shell");
            ExpectCurrentInputKeysCleared(serialProbe.Runtime, "GT-02 current-DAT shell");
            Expect(serialProbe.TransitCount == 1 && serialProbe.TuCount == 1 &&
                   serialProbe.KeysClearedBeforeTransit && serialProbe.PreviousKeysPreservedBeforeTransit,
                "GT-02: every active slot must clear only current runtime keys before its frame advance begins");
        }

        private static void FillInputState(NTSDEntityRuntime runtime)
        {
            FillFrameAdvanceInputKeys(runtime);
            runtime.CdAttack = runtime.CdJump = runtime.CdDefend = runtime.CdDefendLock = 5;
            runtime.CdRight = runtime.CdLeft = runtime.CdUp = runtime.CdDown = 5;
            runtime.ComboDra = runtime.ComboDla = runtime.ComboDua = runtime.ComboDda = 1;
            runtime.ComboDrj = runtime.ComboDlj = runtime.ComboDuj = runtime.ComboDdj = runtime.ComboDja = 1;
            for (int i = 0; i < runtime.InputHistory.Length; i++)
                runtime.InputHistory[i] = i + 1;
        }

        private static void FillFrameAdvanceInputKeys(NTSDEntityRuntime runtime)
        {
            runtime.KeyUp = runtime.KeyDown = runtime.KeyLeft = runtime.KeyRight = 1;
            runtime.KeyAttack = runtime.KeyJump = runtime.KeyDefend = 1;
            runtime.PrevUp = runtime.PrevDown = runtime.PrevLeft = runtime.PrevRight = 1;
            runtime.PrevAttack = runtime.PrevJump = runtime.PrevDefend = 1;
        }

        private static void ExpectInputStateReset(NTSDEntityRuntime runtime, string label)
        {
            ExpectCurrentInputKeysCleared(runtime, label);
            Expect(runtime.PrevUp == 0 && runtime.PrevDown == 0 &&
                   runtime.PrevLeft == 0 && runtime.PrevRight == 0 &&
                   runtime.PrevAttack == 0 && runtime.PrevJump == 0 && runtime.PrevDefend == 0 &&
                   runtime.CdAttack == 0 && runtime.CdJump == 0 && runtime.CdDefend == 0 &&
                   runtime.CdDefendLock == 0 && runtime.CdRight == 0 && runtime.CdLeft == 0 &&
                   runtime.CdUp == 0 && runtime.CdDown == 0 &&
                   runtime.ComboDra == 0 && runtime.ComboDla == 0 &&
                   runtime.ComboDua == 0 && runtime.ComboDda == 0 &&
                   runtime.ComboDrj == 0 && runtime.ComboDlj == 0 &&
                   runtime.ComboDuj == 0 && runtime.ComboDdj == 0 && runtime.ComboDja == 0,
                $"{label}: full runtime input state must reset");
            for (int i = 0; i < runtime.InputHistory.Length; i++)
                Expect(runtime.InputHistory[i] == 0, $"{label}: input history index {i} must reset");
        }

        private static void ExpectCurrentInputKeysCleared(NTSDEntityRuntime runtime, string label)
        {
            Expect(runtime.KeyUp == 0 && runtime.KeyDown == 0 &&
                   runtime.KeyLeft == 0 && runtime.KeyRight == 0 &&
                   runtime.KeyAttack == 0 && runtime.KeyJump == 0 && runtime.KeyDefend == 0,
                $"{label}: current runtime input keys must be clear");
        }

        private static void CheckSharedCharacterLandingNumericAndDamageBoundaries()
        {
            const double initialVx = 7.123456789012345;
            var genericLanding = new SelfCheckCharacterDatShell();
            genericLanding.BindData("SelfCheck_PhysicsDoubleLanding", 764, new LF2CharacterData
            {
                name = "SelfCheck_PhysicsDoubleLanding",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 100, 0, 39, 79),
                    Frame(LF2StandardFrames.Crouch2, LF2States.Standing, 100, LF2StandardFrames.Crouch2, 39, 79),
                },
            });
            var genericWorld = new SimulationWorld();
            genericWorld.Register(genericLanding);
            genericLanding.Runtime.SetPosition(0.0, -1.0, 0.0);
            genericLanding.Runtime.SetVelocity(initialVx, 2.0, 0.0);
            genericLanding.Runtime.SyncIntegerPosition();
            genericWorld.SerialTickAll(1);
            double expectedVx = initialVx * 0.3333333333333333;
            Expect(System.Math.Abs(genericLanding.Runtime.Vx - expectedVx) <= 1e-15,
                $"PH-03: character landing must retain the authority double factor; " +
                $"actual={genericLanding.Runtime.Vx:R}, expected={expectedVx:R}");

            var falling = new SelfCheckCharacterDatShell();
            falling.BindData("SelfCheck_PhysicsNegativeLanding", 765, new LF2CharacterData
            {
                name = "SelfCheck_PhysicsNegativeLanding",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Falling, 100, 0, 39, 79),
                    Frame(LF2StandardFrames.Lying, LF2States.Lying, 100, LF2StandardFrames.Lying, 39, 79),
                },
            });
            var fallingWorld = new SimulationWorld();
            fallingWorld.Register(falling);
            falling.Health.HP = 3;
            falling.Health.HPBound = 2;
            falling.WeaponCount = 10;
            falling.Runtime.SetPosition(0.0, -1.0, 0.0);
            falling.Runtime.SetVelocity(0.0, 2.0, 0.0);
            falling.Runtime.SyncIntegerPosition();
            fallingWorld.SerialTickAll(2);
            Expect(falling.Health.HP == -7 && falling.Health.HPBound == -8 && falling.WeaponCount == 0,
                $"PH-04: state12 landing damage must preserve negative HP/HPBound; " +
                $"hp={falling.Health.HP}, hpBound={falling.Health.HPBound}, weaponCount={falling.WeaponCount}");

            var frozen = new SelfCheckCharacterDatShell();
            frozen.BindData("SelfCheck_PhysicsNegativeFrozen", 766, new LF2CharacterData
            {
                name = "SelfCheck_PhysicsNegativeFrozen",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Frozen, 100, 0, 39, 79),
                    Frame(LF2StandardFrames.FallingFront5, LF2States.Falling, 100, LF2StandardFrames.FallingFront5, 39, 79),
                },
            });
            var frozenWorld = new SimulationWorld();
            frozenWorld.Register(frozen);
            frozen.Health.HP = 3;
            frozen.Health.HPBound = 2;
            frozen.FallDamageDiv = 0;
            frozen.Runtime.SetPosition(0.0, -1.0, 0.0);
            frozen.Runtime.SetVelocity(0.0, 18.0, 0.0);
            frozen.Runtime.SyncIntegerPosition();
            frozenWorld.SerialTickAll(3);
            Expect(frozen.Health.HP == -7 && frozen.Health.HPBound == 2,
                $"PH-04: state13 high-speed landing must preserve negative HP without touching HPBound; " +
                $"hp={frozen.Health.HP}, hpBound={frozen.Health.HPBound}");
        }

        private static void CheckStateTransformLandingMatrix()
        {
            const int lightOid = 741;
            const int heavyOid = 742;
            const int throwOid = 743;
            const int lightSkyOid = 745;
            const int drinkOid = 101;
            const int otherOid = 999;

            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [lightOid] = new LF2CharacterDataWrapper(lightOid,
                    BuildTransformedLandingData("SelfCheck_TransformLight", 31, 3, LF2States.WeaponThrowing, 70)),
                [heavyOid] = new LF2CharacterDataWrapper(heavyOid,
                    BuildTransformedLandingData("SelfCheck_TransformHeavy", 32, 4, LF2States.HeavyWeaponInSky, 20)),
                [throwOid] = new LF2CharacterDataWrapper(throwOid,
                    BuildTransformedLandingData("SelfCheck_TransformThrow", 33, 5, LF2States.WeaponInSky, 0)),
                [lightSkyOid] = new LF2CharacterDataWrapper(lightSkyOid,
                    BuildTransformedLandingData("SelfCheck_TransformLightSky", 35, 2, LF2States.WeaponInSky, 60)),
                [drinkOid] = new LF2CharacterDataWrapper(drinkOid,
                    BuildTransformedLandingData("SelfCheck_TransformDrink", 34, 6, LF2States.WeaponThrowing, 70)),
                [otherOid] = new LF2CharacterDataWrapper(otherOid,
                    BuildTransformedLandingData("SelfCheck_TransformOther999", 0, 0, 9999, 101)),
            };
            var types = new Dictionary<int, int>
            {
                [lightOid] = (int)LF2ObjectType.LightWeapon,
                [heavyOid] = (int)LF2ObjectType.HeavyWeapon,
                [throwOid] = (int)LF2ObjectType.ThrowWeapon,
                [lightSkyOid] = (int)LF2ObjectType.LightWeapon,
                [drinkOid] = (int)LF2ObjectType.Drink,
                [otherOid] = (int)LF2ObjectType.Other,
            };
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;

            using (new TemporaryRuntimeObjectConfigs(types, wrappers))
            {
                try
                {
                    LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                        wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                    TransformedLandingSelfCheckEntity light = CreateTransformedLandingShell(lightOid, false);
                    light.Runtime.WeaponFlightCounter = 20;
                    light.Runtime.WeaponState = 4321;
                    RunTransformedLandingPasses(light, 5.0, 8.0, 1);
                    Expect(light.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.LightWeapon &&
                           light.Frame.N == 70 && light.Runtime.WeaponFlightCounter == 17 &&
                           light.Runtime.WeaponState == 4321,
                        "state4000 transform must dispatch type1 landing to frame70 and subtract weapon_drop_hurt durability");
                    Expect(Nearly(light.Runtime.Vx, 4.0) && Nearly(light.Runtime.Vy, 0.0) && light.WeaponCount == 0,
                        "type1 transformed landing must halve vx, stop vy, and clear WeaponCount outside state12");

                    TransformedLandingSelfCheckEntity lightStop = CreateTransformedLandingShell(lightSkyOid, false);
                    lightStop.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(lightStop, 5.0, 8.0, 11);
                    Expect(lightStop.Frame.N == 60 && lightStop.Runtime.WeaponFlightCounter == 18,
                        "transformed type1 non-throwing low-speed landing must enter frame60 and consume durability");

                    TransformedLandingSelfCheckEntity lightBounce = CreateTransformedLandingShell(lightOid, false);
                    lightBounce.Runtime.WeaponFlightCounter = 20;
                    lightBounce.Runtime.WeaponState = 4322;
                    lightBounce.SwitchDir("right");
                    RunTransformedLandingPasses(lightBounce, 10.0, 8.0, 12);
                    Expect(lightBounce.Frame.N == 7 && Nearly(lightBounce.Runtime.Vy, -8.0) &&
                           Nearly(lightBounce.Runtime.Vx, 4.0) && lightBounce.Runtime.Dir == "left" &&
                           lightBounce.Runtime.WeaponFlightCounter == 17 && lightBounce.Runtime.WeaponState == 4322,
                        "transformed type1 throwing high-speed landing must enter frame7, bounce -8, flip, and consume durability");

                    TransformedLandingSelfCheckEntity heavy = CreateTransformedLandingShell(heavyOid, false);
                    heavy.Runtime.WeaponFlightCounter = 20;
                    heavy.Runtime.WeaponState = 4323;
                    RunTransformedLandingPasses(heavy, 5.0, 8.0, 2);
                    Expect(heavy.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.HeavyWeapon &&
                           heavy.Frame.N == 20 && heavy.Runtime.WeaponFlightCounter == 15 &&
                           heavy.Runtime.WeaponState == 4323,
                        "state4000 transform must dispatch low-speed type2 landing to frame20 and consume 1+drop durability");
                    Expect(Nearly(heavy.Runtime.Vx, 4.0) && Nearly(heavy.Runtime.Vy, 0.0) && heavy.WeaponCount == 0,
                        "type2 transformed landing must stop on frame20 and clear WeaponCount outside state12");

                    TransformedLandingSelfCheckEntity heavyBounce = CreateTransformedLandingShell(heavyOid, false);
                    heavyBounce.Runtime.WeaponFlightCounter = 20;
                    heavyBounce.Runtime.WeaponState = 4324;
                    heavyBounce.SwitchDir("right");
                    RunTransformedLandingPasses(heavyBounce, 10.0, 8.0, 13);
                    Expect(heavyBounce.Frame.N == 0 && Nearly(heavyBounce.Runtime.Vy, -5.0) &&
                           Nearly(heavyBounce.Runtime.Vx, 4.0) && heavyBounce.Runtime.Dir == "right" &&
                           heavyBounce.Runtime.WeaponFlightCounter == 19 && heavyBounce.Runtime.WeaponState == 4324,
                        $"transformed type2 high-speed landing must preserve frame0, bounce -5, consume one durability, " +
                        $"then let late state2000 face final vx; frame={heavyBounce.Frame.N}, vx={heavyBounce.Runtime.Vx}, " +
                        $"vy={heavyBounce.Runtime.Vy}, y={heavyBounce.Runtime.Y}, dir={heavyBounce.Runtime.Dir}, " +
                        $"durability={heavyBounce.Runtime.WeaponFlightCounter}, weaponCount={heavyBounce.WeaponCount}, " +
                        "inputLandingVy=10, inputVx=8");

                    TransformedLandingSelfCheckEntity thrown = CreateTransformedLandingShell(throwOid, false);
                    thrown.Runtime.WeaponFlightCounter = 20;
                    thrown.Runtime.WeaponState = 4325;
                    RunTransformedLandingPasses(thrown, 10.0, 12.0, 3);
                    Expect(thrown.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon &&
                           thrown.Frame.N == 0 && thrown.Runtime.WeaponFlightCounter == 15 &&
                           thrown.Runtime.WeaponState == 4325,
                        "state4000 transform must dispatch high-speed type4 landing to frame0 and subtract drop durability");
                    Expect(Nearly(thrown.Runtime.Vx, 8.4) && Nearly(thrown.Runtime.Vy, -7.0) && thrown.WeaponCount == 0,
                        "type4 transformed landing must apply the release 0.7 bounce and clear WeaponCount");

                    TransformedLandingSelfCheckEntity thrownStop = CreateTransformedLandingShell(throwOid, false);
                    thrownStop.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(thrownStop, 5.0, 8.0, 14);
                    Expect(thrownStop.Frame.N == 60 && Nearly(thrownStop.Runtime.Vx, 5.6) &&
                           Nearly(thrownStop.Runtime.Vy, 0.0) && thrownStop.Runtime.WeaponFlightCounter == 15,
                        "transformed type4 low-speed landing must stop on frame60 with 0.7 vx and consume durability");

                    TransformedLandingSelfCheckEntity drinkBounce = CreateTransformedLandingShell(drinkOid, false);
                    drinkBounce.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(drinkBounce, 10.0, 12.0, 15);
                    Expect(drinkBounce.Frame.N == 0 && Nearly(drinkBounce.Runtime.Vx, 8.4) &&
                           Nearly(drinkBounce.Runtime.Vy, -7.0) && drinkBounce.Runtime.WeaponFlightCounter == 14,
                        "oid101 transformed type6 high-speed landing must take the common 0.7 bounce branch");

                    TransformedLandingSelfCheckEntity drink = CreateTransformedLandingShell(drinkOid, true);
                    drink.Runtime.WeaponFlightCounter = 20;
                    drink.Runtime.WeaponState = 4326;
                    drink.Health.HP = 0;
                    RunTransformedLandingPasses(drink, 5.0, 8.0, 4);
                    Expect(drink.HitStun == 139,
                        $"state8000 transform hit-stop must count down once in the following production late frame_tick; " +
                        $"actual={drink.HitStun}, immediate=140");
                    Expect(drink.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Drink &&
                           drink.Frame.N == 70 && drink.Runtime.WeaponFlightCounter == -1 &&
                           drink.Runtime.WeaponState == 4326,
                        "state8000 transform must dispatch type6 landing and mark depleted drink durability -1");
                    Expect(Nearly(drink.Runtime.Vx, 5.6) && Nearly(drink.Runtime.Vy, 0.0) && drink.WeaponCount == 0,
                        "type6 transformed landing must use the 0.7 stop branch and clear WeaponCount");

                    TransformedLandingSelfCheckEntity other999 = CreateTransformedLandingShell(otherOid, false);
                    RunTransformedLandingPasses(other999, 5.0, 8.0, 16);
                    Expect(other999.Frame.N == 101 && Nearly(other999.Runtime.Vx, 0.0) &&
                           Nearly(other999.Runtime.Vy, 0.0),
                        "state4999 transform must dispatch oid999 default landing to frame101 and stop all planar motion");

                    TransformedLandingSelfCheckEntity grounded999 = CreateTransformedLandingShell(otherOid, false);
                    grounded999.Runtime.SetPosition(0.0, 0.0, 0.0);
                    grounded999.Runtime.SetVelocity(4.0, 0.0, 0.0);
                    grounded999.Runtime.SyncIntegerPosition();
                    var grounded999World = new SimulationWorld();
                    grounded999World.Register(grounded999);
                    grounded999World.SerialTickAll(17);
                    Expect(grounded999.Frame.N == 0,
                        $"PH-05: exact-ground oid999 must not enter frame101; frame={grounded999.Frame.N}");

                    const double preciseVx = 0.12345678901234567;
                    TransformedLandingSelfCheckEntity preciseThrow = CreateTransformedLandingShell(throwOid, false);
                    preciseThrow.Runtime.SetPosition(0.0, -100.0, 0.0);
                    preciseThrow.Runtime.SetVelocity(preciseVx, 0.0, 0.0);
                    preciseThrow.Runtime.SyncIntegerPosition();
                    var preciseThrowWorld = new SimulationWorld();
                    preciseThrowWorld.Register(preciseThrow);
                    preciseThrowWorld.SerialTickAll(18);
                    double preciseExpectedX = preciseVx + preciseVx * 0.2;
                    Expect(System.Math.Abs(preciseThrow.Runtime.X - preciseExpectedX) <= 1e-16,
                        $"PH-03: current-DAT weapon extra-X must retain double 0.2 precision; " +
                        $"actual={preciseThrow.Runtime.X:R}, expected={preciseExpectedX:R}");

                    CheckTransformedPendingDestroyCrossSimOrder(lightOid);
                }
                finally
                {
                    LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
                }
            }
        }

        private static void CheckStateTransformInteractionPhaseRouting()
        {
            const int nonCharacterOid = 771;
            const int characterOid = 772;
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [nonCharacterOid] = new LF2CharacterDataWrapper(nonCharacterOid, new LF2CharacterData
                {
                    name = "SelfCheck_TransformPhaseType3",
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, LF2States.ProjectileFlying, 10, 0, 39, 79),
                    },
                }),
                [characterOid] = new LF2CharacterDataWrapper(characterOid, new LF2CharacterData
                {
                    name = "SelfCheck_TransformPhaseCharacter",
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, LF2States.Standing, 10, 0, 39, 79),
                    },
                }),
            };
            var types = new Dictionary<int, int>
            {
                [nonCharacterOid] = (int)LF2ObjectType.SpecialAttack,
                [characterOid] = (int)LF2ObjectType.Character,
            };
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;

            using (new TemporaryRuntimeObjectConfigs(types, wrappers))
            {
                try
                {
                    LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                        wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                    var characterWorld = new SimulationWorld();
                    var characterShell = new PhaseRoutingSelfCheckCharacter();
                    characterShell.BindSource("SelfCheck_CharacterShellToType3", 770, nonCharacterOid);
                    characterWorld.Register(characterShell);
                    characterShell.RunStateSpecialPreCollision();

                    Expect(characterShell.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.SpecialAttack,
                        "phase-routing fixture must transform the character CLR shell to non-character DAT");
                    Expect(!characterShell.SupportsPostInteractionPhase() &&
                           characterShell.SupportsObjectInteractionPhase(),
                        "character shell transformed to non-character DAT must select only the object interaction pass");

                    characterWorld.PostInteractionTickAll(1);
                    characterWorld.ObjectInteractionTickAll(1);
                    Expect(characterShell.PostInteractionCount == 0 && characterShell.ObjectInteractionCount == 1,
                        "character shell transformed to non-character DAT must skip step7 and enter step9 exactly once");

                    var specialWorld = new SimulationWorld();
                    var specialShell = new PhaseRoutingSelfCheckSpecialAttack();
                    specialShell.BindSource("SelfCheck_SpecialShellToCharacter", 773, characterOid);
                    specialWorld.Register(specialShell);
                    specialShell.RunStateSpecialPreCollision();

                    Expect(specialShell.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character,
                        "phase-routing fixture must transform the special CLR shell to character DAT");
                    Expect(specialShell.SupportsPostInteractionPhase() &&
                           !specialShell.SupportsObjectInteractionPhase(),
                        "special shell transformed to character DAT must select only the character interaction pass");

                    specialWorld.PostInteractionTickAll(1);
                    specialWorld.ObjectInteractionTickAll(1);
                    Expect(specialShell.PostInteractionCount == 1 && specialShell.ObjectInteractionCount == 0,
                        "special shell transformed to character DAT must enter step7 once and never run its old step9 entry");
                }
                finally
                {
                    LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
                }
            }
        }

        private static LF2CharacterData BuildTransformedLandingData(
            string name,
            int weaponHp,
            int dropHurt,
            int sourceState,
            int landingFrame)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, sourceState, 100, 0, 39, 79),
            };
            int[] landingFrames = { 7, 20, 60, 70, 101 };
            for (int i = 0; i < landingFrames.Length; i++)
            {
                int frameId = landingFrames[i];
                int landingState = frameId == 20
                    ? LF2States.HeavyWeaponOnGround
                    : frameId == 7 ? LF2States.WeaponInSky
                    : frameId == 60 ? LF2States.WeaponOnGround
                    : frameId == 101 ? 9999
                    : LF2States.WeaponJustOnGround;
                frames.Add(Frame(frameId, landingState, 100, frameId, 39, 79));
            }

            return new LF2CharacterData
            {
                name = name,
                weapon_hp = weaponHp,
                weapon_drop_hurt = dropHurt,
                frames = frames,
            };
        }

        private static TransformedLandingSelfCheckEntity CreateTransformedLandingShell(int targetOid, bool hitStopTransform)
        {
            int transformState = (hitStopTransform ? 8000 : 4000) + targetOid;
            var shell = new TransformedLandingSelfCheckEntity();
            shell.BindSource(new LF2CharacterData
            {
                name = $"SelfCheck_TransformSource_{targetOid}",
                frames = new List<LF2FrameData>
                {
                    Frame(0, transformState, 100, 0, 39, 79),
                },
            });
            shell.RunStateSpecialPreCollision();

            Expect(shell.ObjectId == targetOid && shell.FrameCache.Wrapper?.characterId == targetOid &&
                   shell.Frame.N == 0 && shell.Frame.D != null,
                $"state transform must load oid {targetOid} wrapper and enter its frame0");
            Expect(shell.WeaponCount == 0,
                $"state transform must preserve the source WeaponCount for oid {targetOid}");
            Expect(shell.Runtime.WeaponFlightCounter == shell.FrameCache.Wrapper.characterData.weapon_hp,
                $"state transform must initialize oid {targetOid} durability from target weapon_hp");
            Expect(shell.HitStun == (hitStopTransform ? 140 : 0),
                $"state transform immediate hit-stop mismatch for oid {targetOid}; " +
                $"actual={shell.HitStun}, expected={(hitStopTransform ? 140 : 0)}");
            return shell;
        }

        private static void RunTransformedLandingPasses(
            TransformedLandingSelfCheckEntity shell,
            double landingVy,
            double vx,
            int tickIndex)
        {
            var world = new SimulationWorld();
            world.Register(shell);
            shell.Runtime.SetPosition(0, -1, 0);
            shell.Runtime.SetVelocity(vx, landingVy, 0);
            shell.Runtime.SyncIntegerPosition();

            world.SerialTickAll(tickIndex);
            int landingFrame = shell.Frame.N;
            world.LateEntityUpdateAll(tickIndex);

            Expect(shell.Frame.N == landingFrame && shell.AttackingCounter == 1,
                "transformed landing must remain visible through the production late frame_tick pass");
        }

        private static void CheckTransformedPendingDestroyCrossSimOrder(int targetOid)
        {
            var world = new SimulationWorld();
            var shell = new TransformingSimOrderSelfCheckEntity(targetOid);
            shell.BindSource(new LF2CharacterData
            {
                name = "SelfCheck_TransformPendingDestroy",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 4000 + targetOid, 100, 0, 39, 79),
                },
            });
            int originalOrder = shell.SimOrder;
            world.Register(shell);
            int slot = shell.Runtime.SlotIndex;

            shell.RunStateSpecialPreCollision();
            Expect(shell.SimOrder != originalOrder,
                "cross-SimOrder destroy fixture must change its exposed order after the current-DAT transform");
            shell.Runtime.PendingFlushDestroy = true;

            world.LateEntityUpdateAll(20);

            Expect(shell.TransitDestroyCount == 1,
                "transformed PendingFlushDestroy must finalize through OnTransitDestroy exactly once");
            Expect(world.FindEntityByRuntimeSlotIncludingPending(slot) == null && world.ObjectCount == 0,
                "transformed PendingFlushDestroy must release its runtime slot and remove every registry reference");
            Expect(!WorldContainsSimulationBucket(world, originalOrder),
                "transformed PendingFlushDestroy must remove the entity from its original registration bucket");

            world.LateEntityUpdateAll(21);
            Expect(shell.TransitDestroyCount == 1,
                "a later late pass must not finalize the transformed entity twice");
        }

        private static bool WorldContainsSimulationBucket(SimulationWorld world, int simOrder)
        {
            var field = typeof(SimulationWorld).GetField(
                "_buckets",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            object buckets = field?.GetValue(world);
            var containsKey = buckets?.GetType().GetMethod("ContainsKey");
            return containsKey != null && (bool)containsKey.Invoke(buckets, new object[] { simOrder });
        }

        private static void CheckSerialTickInterleaveAndFrameEdgeMatrix()
        {
            var serialEvents = new List<string>();
            var serialWorld = new SimulationWorld();
            var low = new SerialOrderSelfCheckEntity("low", serialEvents);
            var high = new SerialOrderSelfCheckEntity("high", serialEvents);
            serialWorld.Register(low);
            serialWorld.Register(high);

            serialWorld.SerialTickAll(1);

            Expect(serialEvents.Count == 4 &&
                   serialEvents[0] == "low:transit" &&
                   serialEvents[1] == "low:tu" &&
                   serialEvents[2] == "high:transit" &&
                   serialEvents[3] == "high:tu",
                "SerialTickAll must interleave Transit/TU per runtime slot instead of running two global loops");

            LF2FrameData singlePhysicsFrame = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            singlePhysicsFrame.dvx = 3;
            var specialData = new LF2CharacterData
            {
                name = "SelfCheck_SpecialSinglePhysics",
                frames = new List<LF2FrameData> { singlePhysicsFrame },
            };
            var specialWorld = new SimulationWorld();
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData("SelfCheck_SpecialSinglePhysics", 200, specialData);
            specialWorld.Register(special);
            special.Runtime.SetPosition(0, -10, 0);
            special.Runtime.SetVelocity(0, 0, 0);
            special.Runtime.SyncIntegerPosition();

            specialWorld.SerialTickAll(1);

            Expect(Nearly(special.Runtime.X, 3.0) && Nearly(special.Runtime.Vx, 3.0),
                "SpecialAttack step4 must apply authored non-character velocity and horizontal physics exactly once");

            LF2FrameData weaponSpecialFrame = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            weaponSpecialFrame.dvx = 2;
            var weaponSpecialData = new LF2CharacterData
            {
                name = "SelfCheck_WeaponShellSpecialDat",
                frames = new List<LF2FrameData> { weaponSpecialFrame },
            };
            var weaponSpecialWorld = new SimulationWorld();
            var weaponSpecialShell = new CurrentDatSelfCheckWeapon(LF2ObjectType.SpecialAttack);
            weaponSpecialShell.BindData("SelfCheck_WeaponShellSpecialDat", 200, 1, weaponSpecialData, 0);
            weaponSpecialWorld.Register(weaponSpecialShell);
            weaponSpecialShell.Runtime.SetPosition(0, -10, 0);
            weaponSpecialShell.Runtime.SetVelocity(0, 0, 0);
            weaponSpecialShell.Runtime.SyncIntegerPosition();

            weaponSpecialShell.SimTU(1);

            Expect(Nearly(weaponSpecialShell.Runtime.X, 2.0) && Nearly(weaponSpecialShell.Runtime.Vy, 0.0),
                "weapon CLR shell with current type3 DAT must use shared type3 physics with zero gravity");

            LF2FrameData weaponOtherFrame = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            weaponOtherFrame.dvx = 2;
            var weaponOtherData = new LF2CharacterData
            {
                name = "SelfCheck_WeaponShellOtherDat",
                frames = new List<LF2FrameData> { weaponOtherFrame },
            };
            var weaponOtherWorld = new SimulationWorld();
            var weaponOtherShell = new CurrentDatSelfCheckWeapon(LF2ObjectType.Other);
            weaponOtherShell.BindData("SelfCheck_WeaponShellOtherDat", 300, 1, weaponOtherData, 0);
            weaponOtherWorld.Register(weaponOtherShell);
            weaponOtherShell.Runtime.SetPosition(0, -10, 0);
            weaponOtherShell.Runtime.SetVelocity(0, 0, 0);
            weaponOtherShell.Runtime.SyncIntegerPosition();

            weaponOtherShell.SimTU(1);

            Expect(Nearly(weaponOtherShell.Runtime.X, 2.0) &&
                   Nearly(weaponOtherShell.Runtime.Vy, NTSDGlobal.Gameplay.WeaponGravityDefault),
                "weapon CLR shell with current other DAT must use shared ordinary non-character gravity");

            LF2CharacterData negativeNextData = new LF2CharacterData
            {
                name = "SelfCheck_NegativeNext",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, -1, 39, 79),
                    Frame(1, LF2States.Standing, 10, 1, 39, 79),
                },
            };
            var negativeWeapon = new CurrentDatSelfCheckWeapon(LF2ObjectType.LightWeapon);
            negativeWeapon.BindData("SelfCheck_NegativeNextWeapon", 100, 1, negativeNextData, 0);
            negativeWeapon.SwitchDir("right");
            negativeWeapon.Trans.SyncDirectFrameData(0, -1, 0);
            negativeWeapon.SimFrameTick(1);
            Expect(negativeWeapon.Frame.N == 1 && negativeWeapon.Runtime.Dir == "left",
                "weapon negative next must enter the absolute frame and flip facing exactly once");

            var negativeSpecial = new AlternateDamageSelfCheckSpecialAttack();
            negativeSpecial.BindData("SelfCheck_NegativeNextSpecial", 200, negativeNextData);
            negativeSpecial.SwitchDir("right");
            negativeSpecial.Trans.SyncDirectFrameData(0, -1, 0);
            negativeSpecial.SimFrameTick(1);
            Expect(negativeSpecial.Frame.N == 1 && negativeSpecial.Runtime.Dir == "left",
                "SpecialAttack negative next must enter the absolute frame and flip facing exactly once");

            var caughtExitData = new LF2CharacterData
            {
                name = "SelfCheck_CaughtExitFrozen",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Lying, 0, 1, 39, 79),
                    Frame(1, LF2States.Frozen, 10, 1, 39, 79),
                    Frame(2, LF2States.Standing, 10, 2, 39, 79),
                },
            };
            LF2Character frozenExit = CreateCharacter("SelfCheck_CaughtExitFrozen", 1, caughtExitData);
            frozenExit.HitStun = 0;
            frozenExit.Trans.SyncDirectFrameData(0, 1, 0);
            frozenExit.SimFrameTick(1);
            Expect(frozenExit.Frame.N == 1 && frozenExit.HitStun == 0,
                "leaving state14 into Frozen must not arm the generic caught-exit hit stop");

            caughtExitData.frames[0].next = 2;
            LF2Character ordinaryExit = CreateCharacter("SelfCheck_CaughtExitOrdinary", 1, caughtExitData);
            ordinaryExit.HitStun = 0;
            ordinaryExit.Trans.SyncDirectFrameData(0, 2, 0);
            ordinaryExit.SimFrameTick(1);
            Expect(ordinaryExit.Frame.N == 2 && ordinaryExit.HitStun == 15,
                "leaving state14 into an ordinary state must arm the mapped caught-exit hit stop");
        }

        private static bool IsRecycledAndCleared(OPointCreateTask task)
        {
            return task != null &&
                   !task.IsFromPool &&
                   task.opoint.oid == 0 &&
                   task.parent == null &&
                   task.releaseSpawnSemantic == ReleaseSpawnSemantic.None;
        }

        private static int GetQueuedObjectPointTaskCount(LF2ObjectPointFactory factory)
        {
            var queueField = typeof(LF2ObjectPointFactory).GetField(
                "_taskQueue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            object queue = queueField?.GetValue(factory);
            var countProperty = queue?.GetType().GetProperty("Count");
            return countProperty == null ? -1 : (int)countProperty.GetValue(queue);
        }

        private static LF2CharacterData BuildAlternateDamageWeaponFrames()
        {
            var frames = new List<LF2FrameData>();
            for (int frameId = 0; frameId < 16; frameId++)
                frames.Add(Frame(frameId, LF2States.Standing, 1, frameId, 39, 79));
            frames.Add(Frame(20, LF2States.WeaponThrowing, 1, 20, 39, 79));

            return new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageWeapon",
                type_sub = 1,
                frames = frames,
            };
        }

        private static void PrepareAlternateEntry(LF2Entity attacker, LF2Entity victim)
        {
            attacker.SwitchDir("right");
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.LinkState = 0;
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            SetAlternateDamagePosition(attacker, 0.0, 0.0);

            victim.SwitchDir("right");
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FallCounter = 0;
            victim.KillCount = 0;
            victim.FrameDelay = 0;
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            SetAlternateDamagePosition(victim, 10.0, 0.0);
        }

        private static InteractionArea MakeInteractionItr(int kind, int vrest, int injury, int dvx)
        {
            return new InteractionArea
            {
                kind = kind,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 20,
                injury = injury,
                dvx = dvx,
                bdefend = 0,
                arest = 4,
                vrest = vrest,
                effect = 0,
            };
        }

        private static LF2FrameData InteractionFrame(InteractionArea itr)
        {
            LF2FrameData frame = Frame(0, LF2States.Standing, 1, 0, 0, 0);
            frame.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
            });
            if (itr != null)
                frame.itrs.Add(itr);
            return frame;
        }

        private static LF2CharacterData BuildInteractionVictimData(string name, int objectId)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = new List<LF2FrameData> { InteractionFrame(null) },
            };
        }

        private static AlternateDamageSelfCheckWeapon CreateSelfCheckWeapon(
            string name,
            int objectId,
            int weaponType,
            LF2CharacterData data,
            int frameId)
        {
            var weapon = new AlternateDamageSelfCheckWeapon();
            weapon.BindData(name, objectId, weaponType, data, frameId);
            return weapon;
        }

        private static LF2Character CreateInteractionCharacter(string name, int objectId, LF2CharacterData data)
        {
            var character = new InteractionSelfCheckCharacter();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new SelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRuntimeSlotIndex(character.StableId);
            return character;
        }

        private static void RegisterInteractionPair(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Character victim)
        {
            world.Register(attacker);
            world.Register(victim);

            attacker.Team = 1;
            attacker.RelationTeam = 1;
            attacker.Health.HP = 100;
            attacker.Health.HPBound = 100;
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.LinkState = 0;
            attacker.ItrRest.Reset();
            attacker.Runtime.SetPosition(0.0, 0.0, 0.0);
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            attacker.Runtime.SyncIntegerPosition();

            victim.Team = 2;
            victim.RelationTeam = 2;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FallCounter = 0;
            victim.KillCount = 0;
            victim.FrameDelay = 0;
            victim.ItrRest.Reset();
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            victim.Runtime.SetPosition(0.0, 0.0, 0.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
        }

        private static void RunSpecialAttackPreprocessCase(
            int kind,
            int rawVrest,
            Action<AlternateDamageSelfCheckSpecialAttack> arrange,
            Action<AlternateDamageSelfCheckSpecialAttack, LF2Character, InteractionArea> verify)
        {
            InteractionArea sourceItr = MakeInteractionItr(kind, rawVrest, 100, 6);
            var specialData = new LF2CharacterData
            {
                name = $"SelfCheckSpecialPreprocess{kind}",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(sourceItr) },
            };
            var world = new SimulationWorld();
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData($"SelfCheck_SpecialPreprocess{kind}", 1, specialData);
            LF2Character victim = CreateInteractionCharacter(
                $"SelfCheck_SpecialPreprocessVictim{kind}",
                37,
                BuildInteractionVictimData($"SelfCheckSpecialPreprocessVictim{kind}", 37));
            RegisterInteractionPair(world, special, victim);
            arrange(special);

            special.SimTU(1);
            Expect(victim.Health.HP == 100,
                "special-attack TU must not consume interaction candidates");

            special.FrameDelay = 0;
            special.AttackExempt = 0;
            special.Runtime.SetPosition(0.0, 0.0, 0.0);
            special.Runtime.SetVelocity(0.0, 0.0, 0.0);
            special.Runtime.SyncIntegerPosition();
            arrange(special);
            victim.Runtime.SetPosition(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            world.ObjectInteractionTickAll(2);
            world.EndCollisionCandidateConsumption();

            Expect(victim.Health.HP == 90,
                "special-attack object-interaction pass must resolve alternate damage");
            verify(special, victim, sourceItr);
        }

        private static void RunAlternateDamageMotionCase(
            LF2CharacterData frameData,
            Action<LF2Character, LF2Character, InteractionArea> arrange,
            Action<LF2Character, LF2Character, InteractionArea> verify)
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter("SelfCheck_AlternateMotionAttacker", 1, frameData);
            LF2Character victim = CreateCharacter("SelfCheck_AlternateMotionVictim", 2, frameData);
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(0);
            victim.ImmediateFrame(0);
            attacker.SwitchDir("right");
            victim.SwitchDir("right");
            SetAlternateDamagePosition(attacker, 0.0, 0.0);
            SetAlternateDamagePosition(victim, 10.0, 0.0);
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            attacker.Runtime.LinkState = 0;
            attacker.Runtime.HolderStableId = -1;
            attacker.HolderCopySlot = -1;
            attacker.AttackExempt = 0;
            attacker.FrameDelay = 0;
            victim.Health.HP = 500;
            victim.Health.HPBound = 500;
            victim.Health.HPLost = 0;
            victim.FallDamageDiv = 0;
            victim.KillCount = 0;
            victim.Unk344 = 0;
            victim.ComboCountVic = 0;
            victim.FallCounter = 0;
            victim.AttackingCounter = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            victim.FrameDelay = 0;
            victim.Runtime.PrevFrame2 = 0;

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 0,
                bdefend = 0,
                dvx = 0,
                arest = 0,
                vrest = 0,
                effect = 0,
            };

            arrange(attacker, victim, itr);
            LF2AlternateDamageResolver.ApplyAlternateDamage(attacker, victim, victim.HitCounters, itr);
            verify(attacker, victim, itr);
        }

        private static void SetAlternateDamagePosition(LF2Entity entity, double x, double y)
        {
            entity.Runtime.SetPosition(x, y, 0.0);
            entity.Runtime.SyncIntegerPosition();
        }

        private static LF2CharacterData BuildAlternateDamageMotionFrames()
        {
            var frames = new List<LF2FrameData>();
            for (int frameId = 0; frameId < 16; frameId++)
                frames.Add(Frame(frameId, LF2States.Standing, 1, frameId, 39, 79));

            frames.Add(Frame(20, LF2States.WeaponThrowing, 2, 20, 39, 79));
            frames.Add(Frame(21, LF2States.HeavyWeaponInSky, 2, 21, 39, 79));
            frames.Add(Frame(22, LF2States.ProjectileFlying, 2, 22, 39, 79));
            frames.Add(Frame(110, LF2States.Defending, 5, 111, 39, 79));
            frames.Add(Frame(111, LF2States.Defending, 6, 111, 39, 79));

            return new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageMotion",
                type_sub = 1,
                frames = frames,
            };
        }

        private static void CheckOid5152MergeSuccessAndDormantIsolation()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                var world = new SimulationWorld();
                LF2Character self = CreateCharacter("SelfCheck_Oid7", 7, wrappers[7].characterData);
                LF2Character partner = CreateCharacter("SelfCheck_Oid8", 8, wrappers[8].characterData);
                self.SetRuntimeSlotIndex(0);
                partner.SetRuntimeSlotIndex(11);
                world.Register(self);
                world.Register(partner);

                self.ImmediateFrame(10);
                partner.ImmediateFrame(10);
                self.Team = 1;
                partner.Team = 2;
                self.RelationTeam = 0;
                partner.RelationTeam = 0;
                self.Health.HP = 130;
                self.Health.HPBound = 150;
                self.Health.HP3 = 200;
                partner.Health.HP = 120;
                partner.Health.HPBound = 80;
                self.Health.PP = 10;
                partner.Health.PP = 20;
                self.Runtime.SetPosition(100f, 0f, 5f);
                partner.Runtime.SetPosition(121f, 0f, 12f);
                self.Runtime.SyncIntegerPosition();
                partner.Runtime.SyncIntegerPosition();
                self.Runtime.Vy = 7f;
                partner.Runtime.Vy = 3f;
                self.Trans.SyncDirectFrameData(self.Frame.D.wait, self.Frame.D.next, 37);
                partner.ItrRest.Arest = 6;
                partner.ItrRest.SetVrest(0, 8);
                partner.ItrRest.SetVrest(19, 11);

                world.Oid5152RuntimeMaintenanceAll(1);

                Expect(self.ObjectId == 51 && self.CurrentFrameId == 290,
                    "oid 7/8 merge must convert self into oid 51 frame 290");
                Expect(self.Health.HPBound == 200 && self.Health.HP == 200,
                    "oid 7/8 merge must clamp aggregate HP/HPBound by self HP3");
                Expect(self.Health.PP == 500,
                    "oid 7/8 merge must set self PP to 500");
                Expect(self.GetRuntimeXInt() == 110 && self.GetRenderZInt() == 8,
                    "oid 7/8 merge must write integer midpoint X/Z");
                Expect(Nearly(self.Runtime.Vy, 7f) && Nearly(partner.Runtime.Vy, 0f),
                    "oid 7/8 merge must preserve self Vy and zero partner Vy");
                Expect(self.Trans.WaitCounter == 37,
                    "oid 7/8 merge identity switch must preserve the self wait counter");
                Expect(partner.ItrRest.Arest == 6 && partner.ItrRest.GetVrest(0) == 8 &&
                       partner.ItrRest.GetVrest(19) == 11,
                    "oid 7/8 merge must not clear the dormant partner's external arest/vrest state");
                Expect(self.Runtime.Unk328 == 1 &&
                       self.Runtime.Unk32C == 11 &&
                       self.Runtime.Unk330 == 7 &&
                       self.Runtime.Unk334 == 8 &&
                       self.Runtime.Unk338 == 4500,
                    "oid 7/8 merge must write merge bookkeeping fields");
                Expect(partner.Runtime.OidMergeDormant,
                    "merged partner must become dormant instead of being unregistered");
                Expect(partner.Runtime.SlotIndex == 11 && partner.ObjectId == 8,
                    "merged partner must retain original slot and DAT identity");
                Expect(world.ObjectCount == 1,
                    "dormant merged partner must be excluded from ObjectCount");
                Expect(world.FindEntityByRuntimeSlotForQuery(11) == null,
                    "ordinary runtime-slot query must hide dormant merged partner");
                Expect(world.FindEntityByRuntimeSlotIncludingPending(11) == partner,
                    "including-pending runtime-slot query must still find dormant merged partner");

                var entities = new List<LF2Entity>();
                world.GetAllEntities(entities);
                Expect(entities.Count == 1 && entities[0] == self,
                    "ordinary entity enumeration must exclude dormant merged partner");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152MergeCooldownOneTriggersSameTick()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                var world = new SimulationWorld();
                LF2Character self = CreateCharacter("SelfCheck_Oid7_Cooldown", 7, wrappers[7].characterData);
                LF2Character partner = CreateCharacter("SelfCheck_Oid8_Cooldown", 8, wrappers[8].characterData);
                self.SetRuntimeSlotIndex(1);
                partner.SetRuntimeSlotIndex(12);
                world.Register(self);
                world.Register(partner);

                self.ImmediateFrame(10);
                partner.ImmediateFrame(10);
                self.RelationTeam = 1;
                partner.RelationTeam = 1;
                self.Health.HP = 100;
                self.Health.HPBound = 100;
                self.Health.HP3 = 200;
                partner.Health.HP = 90;
                partner.Health.HPBound = 90;
                self.Runtime.Unk338 = 1;
                self.Runtime.SetPosition(50f, 0f, 5f);
                partner.Runtime.SetPosition(80f, 0f, 8f);
                self.Runtime.SyncIntegerPosition();
                partner.Runtime.SyncIntegerPosition();

                world.Oid5152RuntimeMaintenanceAll(1);

                Expect(self.ObjectId == 51 && self.Runtime.Unk338 == 4500,
                    "merge cooldown 1 must decrement to 0 and still allow same-tick merge");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152AuthorityGateMatrix()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                AssertOid5152MergeRejected(wrappers, "both-high-slots", 7, 10, 11, null);
                AssertOid5152MergeRejected(wrappers, "self-dead", 7, 0, 11,
                    (world, self, partner) => self.Health.HP = 0);
                AssertOid5152MergeRejected(wrappers, "self-state", 7, 0, 11,
                    (world, self, partner) => self.ImmediateFrame(0));
                AssertOid5152MergeRejected(wrappers, "self-cooldown", 7, 0, 11,
                    (world, self, partner) => self.Runtime.Unk338 = 2);
                AssertOid5152MergeRejected(wrappers, "self-hp-gate", 7, 0, 11,
                    (world, self, partner) => self.Health.HP = 177);
                AssertOid5152MergeRejected(wrappers, "Unk364-mismatch", 7, 0, 11,
                    (world, self, partner) => partner.RelationTeam = 4);
                AssertOid5152MergeRejected(wrappers, "partner-cooldown", 7, 0, 11,
                    (world, self, partner) => partner.Runtime.Unk338 = 2);
                AssertOid5152MergeRejected(wrappers, "partner-state14", 7, 0, 11,
                    (world, self, partner) =>
                    {
                        partner.Frame.D = Frame(10, 14, 1, 10, 39, 79);
                    });
                AssertOid5152MergeRejected(wrappers, "dx-boundary", 7, 0, 11,
                    (world, self, partner) =>
                    {
                        self.Runtime.SetPosition(100f, 0f, 5f);
                        partner.Runtime.SetPosition(150f, 0f, 5f);
                        self.Runtime.SyncIntegerPosition();
                        partner.Runtime.SyncIntegerPosition();
                    });
                AssertOid5152MergeRejected(wrappers, "dz-boundary", 7, 0, 11,
                    (world, self, partner) =>
                    {
                        self.Runtime.SetPosition(100f, 0f, 5f);
                        partner.Runtime.SetPosition(120f, 0f, 13f);
                        self.Runtime.SyncIntegerPosition();
                        partner.Runtime.SyncIntegerPosition();
                    });
                AssertOid5152MergeRejected(wrappers, "both-low-equal-x", 7, 0, 1,
                    (world, self, partner) =>
                    {
                        self.Runtime.SetPosition(100f, 0f, 5f);
                        partner.Runtime.SetPosition(100f, 0f, 5f);
                        self.Runtime.SyncIntegerPosition();
                        partner.Runtime.SyncIntegerPosition();
                    });

                SimulationWorld orderedWorld = CreateOid5152MergeCandidate(
                    wrappers, 7, 0, 1, out LF2Character orderedSelf, out LF2Character orderedPartner);
                orderedSelf.Runtime.SetPosition(121f, 0f, 5f);
                orderedPartner.Runtime.SetPosition(100f, 0f, 5f);
                orderedSelf.Runtime.SyncIntegerPosition();
                orderedPartner.Runtime.SyncIntegerPosition();
                orderedWorld.Oid5152RuntimeMaintenanceAll(1);
                Expect(orderedSelf.ObjectId == 51 && orderedPartner.Runtime.OidMergeDormant,
                    "two low slots must merge only when the earlier self slot is strictly right of its partner");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152MirrorIdentityAndPresentation()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            GameObject selfView = null;
            GameObject partnerView = null;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
                Expect(animatorManager != null,
                    "oid 7/8 presentation identity check requires CharacterAnimtorManager");
                using var sprites7 = new TemporaryCharacterSpriteConfig(animatorManager, 7, 1);
                using var sprites8 = new TemporaryCharacterSpriteConfig(animatorManager, 8, 1);
                using var sprites51 = new TemporaryCharacterSpriteConfig(animatorManager, 51, 1);

                SimulationWorld world = CreateOid5152MergeCandidate(
                    wrappers, 8, 0, 11, out LF2Character self, out LF2Character partner);
                self.Team = 8;
                partner.Team = 7;
                self.RelationTeam = 0;
                partner.RelationTeam = 0;
                self.Trans.SyncDirectFrameData(self.Frame.D.wait, self.Frame.D.next, 29);

                selfView = new GameObject("SelfCheck_Oid8_MirrorRenderer");
                selfView.SetActive(false);
                SpriteRenderer selfSpriteRenderer = selfView.AddComponent<SpriteRenderer>();
                LF2ObjectRenderer selfRenderer = selfView.AddComponent<LF2ObjectRenderer>();
                self.Sprite.Initialize(selfSpriteRenderer, new List<Sprite> { sprites8.Sprite });
                SetPrivateField(selfRenderer, "_logicObject", self);

                partnerView = new GameObject("SelfCheck_Oid7_MirrorRenderer");
                partnerView.SetActive(false);
                SpriteRenderer partnerSpriteRenderer = partnerView.AddComponent<SpriteRenderer>();
                LF2ObjectRenderer partnerRenderer = partnerView.AddComponent<LF2ObjectRenderer>();
                partner.Sprite.Initialize(partnerSpriteRenderer, new List<Sprite> { sprites7.Sprite });
                SetPrivateField(partnerRenderer, "_logicObject", partner);

                selfRenderer.ForceRefreshPresentation();
                partnerRenderer.ForceRefreshPresentation();
                Expect(selfSpriteRenderer.sprite == sprites8.Sprite && partnerSpriteRenderer.sprite == sprites7.Sprite,
                    "initial oid 8/7 renderers must bind their own sprite catalogs");

                world.Oid5152RuntimeMaintenanceAll(1);
                selfRenderer.ForceRefreshPresentation();
                partnerRenderer.ForceRefreshPresentation();
                Expect(self.ObjectId == 51 && self.Runtime.Unk330 == 8 && self.Runtime.Unk334 == 7,
                    "oid 8 must mirror oid 7 as an equally valid active merge owner");
                Expect(self.Trans.WaitCounter == 29,
                    "oid 8 merge identity switch must preserve self wait counter");
                Expect(selfSpriteRenderer.enabled && selfSpriteRenderer.sprite == sprites51.Sprite,
                    "merged oid 51 renderer must rebind the oid 51 sprite catalog");
                Expect(!partnerSpriteRenderer.enabled,
                    "inactive merged partner presentation must be hidden like C++ active=false");

                partner.FrameDelay = -7;
                partner.KnockbackVx = 8.5;
                partner.KnockbackVy = -4.5;
                partner.KnockbackVz = 2.5;
                partner.HolderCopySlot = 3;
                partner.Frame.PN = 71;
                partner.Frame.Prev = 72;
                partner.Frame.Prev2 = 73;
                partner.Frame.Prev2D = partner.Frame.D;
                SeedStaleOid5152PartnerEffectState(partner, 12);
                self.Runtime.X = 77.75;
                self.Runtime.XInt = 77;
                self.Runtime.Y = -5.5;
                self.Runtime.YInt = -5;
                self.Runtime.Z = 9.25;
                self.Runtime.ZInt = 9;
                self.Frame.N = 500;
                self.Frame.D = null;
                self.Runtime.Unk338 = 0;
                self.Trans.SyncDirectFrameData(4, 500, 31);
                world.Oid5152RuntimeMaintenanceAll(2);
                selfRenderer.ForceRefreshPresentation();
                partnerRenderer.ForceRefreshPresentation();

                Expect(self.ObjectId == 8 && partner.ObjectId == 7 &&
                       self.Frame.N == 112 && partner.Frame.N == 112,
                    "oid 51 split must work from an out-of-range frame number even when Frame.D is null");
                Expect(self.Trans.WaitCounter == 31 && partner.Trans.WaitCounter == 0,
                    "mirrored split must preserve self wait and Reset partner wait");
                Expect(Nearly(self.Runtime.X, 77.75) && self.Runtime.XInt == 77 &&
                       Nearly(self.Runtime.Z, 9.25) && self.Runtime.ZInt == 9 &&
                       Nearly(partner.Runtime.X, 77.75) && partner.Runtime.XInt == 77 &&
                       Nearly(partner.Runtime.Z, 9.25) && partner.Runtime.ZInt == 9,
                    "mirrored split must preserve fractional X/Z while copying their integer snapshots");
                Expect(partner.RelationTeam == 0 && partner.Team == 0,
                    "split partner must inherit exact Unk364=0 without falling back to self Team");
                Expect(partner.FrameDelay == 0 &&
                       Nearly(partner.KnockbackVx, 0.1) && Nearly(partner.KnockbackVy, 0.1) &&
                       Nearly(partner.KnockbackVz, 0.1) && partner.HolderCopySlot == 99 &&
                       partner.Frame.PN == 0 && partner.Frame.Prev == 0 && partner.Frame.Prev2 == 0 &&
                       partner.Frame.Prev2D == null && HasFormalOid5152PartnerEffectDefaults(partner),
                    "mirrored split partner must use formal Entity::reset defaults before M-1 contract writes");
                Expect(selfSpriteRenderer.enabled && selfSpriteRenderer.sprite == sprites8.Sprite &&
                       partnerSpriteRenderer.enabled && partnerSpriteRenderer.sprite == sprites7.Sprite,
                    "split renderers must restore visibility and rebind the original oid 8/7 sprite catalogs");
            }
            finally
            {
                if (selfView != null) DestroySelfCheckObject(selfView);
                if (partnerView != null) DestroySelfCheckObject(partnerView);
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void AssertOid5152MergeRejected(
            Dictionary<int, LF2CharacterDataWrapper> wrappers,
            string label,
            int selfOid,
            int selfSlot,
            int partnerSlot,
            Action<SimulationWorld, LF2Character, LF2Character> mutate)
        {
            SimulationWorld world = CreateOid5152MergeCandidate(
                wrappers, selfOid, selfSlot, partnerSlot, out LF2Character self, out LF2Character partner);
            mutate?.Invoke(world, self, partner);
            world.Oid5152RuntimeMaintenanceAll(1);
            Expect(self.ObjectId != 51 && partner.ObjectId != 51,
                $"oid 7/8 merge gate '{label}' must reject both candidates");
        }

        private static SimulationWorld CreateOid5152MergeCandidate(
            Dictionary<int, LF2CharacterDataWrapper> wrappers,
            int selfOid,
            int selfSlot,
            int partnerSlot,
            out LF2Character self,
            out LF2Character partner)
        {
            int partnerOid = 15 - selfOid;
            var world = new SimulationWorld();
            self = CreateCharacter($"SelfCheck_Oid{selfOid}_Candidate", selfOid, wrappers[selfOid].characterData);
            partner = CreateCharacter($"SelfCheck_Oid{partnerOid}_Candidate", partnerOid, wrappers[partnerOid].characterData);
            self.SetRuntimeSlotIndex(selfSlot);
            partner.SetRuntimeSlotIndex(partnerSlot);
            world.Register(self);
            world.Register(partner);
            self.ImmediateFrame(10);
            partner.ImmediateFrame(10);
            self.RelationTeam = 3;
            partner.RelationTeam = 3;
            self.Health.HP = 100;
            self.Health.HPBound = 100;
            self.Health.HP3 = 500;
            partner.Health.HP = 100;
            partner.Health.HPBound = 100;
            self.Runtime.SetPosition(120f, 0f, 5f);
            partner.Runtime.SetPosition(100f, 0f, 5f);
            self.Runtime.SyncIntegerPosition();
            partner.Runtime.SyncIntegerPosition();
            return world;
        }

        private static void CheckOid5152SplitSuccessAndOddTruncate()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                SimulationWorld world = CreateOid5152MergedWorld(wrappers, out LF2Character self, out LF2Character partner);
                partner.FrameDelay = -6;
                partner.KnockbackVx = 6.5;
                partner.KnockbackVy = -3.5;
                partner.KnockbackVz = 1.5;
                partner.HolderCopySlot = 4;
                partner.Frame.PN = 81;
                partner.Frame.Prev = 82;
                partner.Frame.Prev2 = 83;
                partner.Frame.Prev2D = partner.Frame.D;
                SeedStaleOid5152PartnerEffectState(partner, 13);
                self.Health.HP = 201;
                self.Health.HPBound = 199;
                self.Runtime.Unk338 = 1;
                self.Runtime.Vy = 9f;
                self.Runtime.Vz = 4f;
                self.Runtime.X = 90.75;
                self.Runtime.XInt = 90;
                self.Runtime.Y = -3.25;
                self.Runtime.YInt = -3;
                self.Runtime.Z = 6.5;
                self.Runtime.ZInt = 6;
                self.Trans.SyncDirectFrameData(self.Frame.D.wait, self.Frame.D.next, 41);

                world.Oid5152RuntimeMaintenanceAll(2);

                Expect(self.ObjectId == 7 && self.CurrentFrameId == 112,
                    "oid 51 split must restore self identity and enter frame 112");
                Expect(partner.ObjectId == 8 && partner.CurrentFrameId == 112,
                    "oid 51 split must revive dormant partner into frame 112");
                Expect(self.Health.HP == 100 && self.Health.HPBound == 99 &&
                       partner.Health.HP == 100 && partner.Health.HPBound == 99,
                    "oid 51 split must floor-divide odd HP and HPBound for both sides");
                Expect(self.Health.HP3 == 200 && partner.Health.HP3 == 500,
                    "oid 51 split must preserve self HP3 and keep partner Reset default HP3");
                Expect(self.Health.PP == 0 && partner.Health.PP == 0,
                    "oid 51 split must zero PP for both sides");
                Expect(self.Runtime.Unk328 == -1 && self.Runtime.Unk338 == 900,
                    "oid 51 split must clear merge flag and write 900 cooldown on self");
                Expect(!partner.Runtime.OidMergeDormant && world.ObjectCount == 2,
                    "split success must reactivate dormant partner and restore ObjectCount");
                Expect(partner.Team == 0 && partner.OwnerId == -1 && partner.Runtime.Unk328 == -1,
                    "split success partner must come from Reset defaults before contract overwrites");
                Expect(Nearly(self.Runtime.Vy, 9f) && Nearly(partner.Runtime.Vy, 0f) && Nearly(partner.Runtime.Vz, 0f),
                    "split success must preserve self Vy/Vz and keep partner Reset default vertical velocity");
                Expect(Nearly(self.Runtime.X, 90.75) && self.Runtime.XInt == 90 &&
                       Nearly(self.Runtime.Z, 6.5) && self.Runtime.ZInt == 6 &&
                       Nearly(partner.Runtime.X, 90.75) && partner.Runtime.XInt == 90 &&
                       Nearly(partner.Runtime.Z, 6.5) && partner.Runtime.ZInt == 6 &&
                       Nearly(self.Runtime.Y, 0.0) && self.Runtime.YInt == 0 &&
                       Nearly(partner.Runtime.Y, 0.0) && partner.Runtime.YInt == 0,
                    "oid 51 split must copy float and integer X/Z independently and zero only Y");
                Expect(self.Trans.WaitCounter == 41 && partner.Trans.WaitCounter == 0,
                    "oid 51 split must preserve self wait counter while revived partner starts from Reset wait 0");
                Expect(partner.FrameDelay == 0 &&
                       Nearly(partner.KnockbackVx, 0.1) && Nearly(partner.KnockbackVy, 0.1) &&
                       Nearly(partner.KnockbackVz, 0.1) && partner.HolderCopySlot == 99 &&
                       partner.Frame.PN == 0 && partner.Frame.Prev == 0 && partner.Frame.Prev2 == 0 &&
                       partner.Frame.Prev2D == null && HasFormalOid5152PartnerEffectDefaults(partner),
                    "split partner must use formal Entity::reset defaults before M-1 contract writes");
                Expect(partner.ItrRest.Arest == 6 && partner.ItrRest.GetVrest(0) == 8 &&
                       partner.ItrRest.GetVrest(19) == 11,
                    "oid 51 split Reset must preserve external partner arest and all vrest keys");
                Expect(self.Runtime.Dir != partner.Runtime.Dir,
                    "split success must face revived partner opposite to self");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void SeedStaleOid5152PartnerEffectState(LF2Character partner, int deadBlinkCount)
        {
            partner.DeadBlinkCountInternal = deadBlinkCount;
            partner.Effect.Num = 7;
            partner.Effect.Dvx = 2f;
            partner.Effect.Dvy = -3f;
            partner.Effect.Stuck = true;
            partner.Effect.Oscillate = 4;
            partner.Effect.Blink = true;
            partner.Effect.Super = true;
            partner.Effect.TimeIn = -5;
            partner.Effect.TimeOut = 6;
            partner.Effect.OscillateDirection = -1;
            partner.Effect.BlinkCounter = 8;
        }

        private static bool HasFormalOid5152PartnerEffectDefaults(LF2Character partner)
        {
            LF2EffectState effect = partner.Effect;
            return partner.DeadBlinkCountInternal == -1 &&
                   effect != null && effect.Num == -99 && Nearly(effect.Dvx, 0.0) &&
                   Nearly(effect.Dvy, 0.0) && !effect.Stuck && effect.Oscillate == 0 &&
                   !effect.Blink && !effect.Super && effect.TimeIn == 0 && effect.TimeOut == 0 &&
                   effect.OscillateDirection == 1 && effect.BlinkCounter == 0;
        }

        private static void CheckOid5152SplitFailurePartialRecovery()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                SimulationWorld world = CreateOid5152MergedWorld(wrappers, out LF2Character self, out LF2Character partner);
                self.Health.PP = 123;
                self.Health.HP = 180;
                self.Health.HPBound = 180;
                self.Runtime.Unk32C = 399;
                self.Runtime.Unk338 = 0;

                world.Oid5152RuntimeMaintenanceAll(3);

                Expect(self.ObjectId == 7,
                    "split partial recovery must still restore self identity first");
                Expect(self.Runtime.Unk328 == -1 && self.Runtime.Unk338 == 900,
                    "split partial recovery must persist self cooldown writes");
                Expect(self.CurrentFrameId == 290 && self.Health.PP == 123 &&
                       self.Health.HP == 180 && self.Health.HPBound == 180,
                    "split partial recovery must not apply frame112, PP0 or HP halving");
                Expect(self.Frame.D == null,
                    "split partial recovery must leave self frame data reloaded against original DAT even when frame 290 is absent");
                Expect(partner.Runtime.OidMergeDormant && world.ObjectCount == 1,
                    "split partial recovery must not revive dormant partner or increment ObjectCount");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152DjaReleaseTriggersSameTickSplit()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                var world = new SimulationWorld();
                LF2Character self = CreateCharacter("SelfCheck_Oid51_Dja", 7, wrappers[7].characterData);
                LF2Character partner = CreateCharacter("SelfCheck_Oid8_Dormant", 8, wrappers[8].characterData);
                self.SetRuntimeSlotIndex(0);
                partner.SetRuntimeSlotIndex(11);
                world.Register(self);
                world.Register(partner);

                self.RelationTeam = 4;
                partner.RelationTeam = 4;
                self.TryApplyRuntimeIdentity(51, 290, true, out _);
                self.Runtime.Unk328 = 1;
                self.Runtime.Unk32C = 11;
                self.Runtime.Unk330 = 7;
                self.Runtime.Unk334 = 8;
                self.Runtime.Unk338 = 30;
                self.TransformOriginalObjectId = 77;
                self.Health.HP = 180;
                self.Health.HPBound = 180;
                self.Health.HP3 = 200;
                self.Runtime.SetPosition(60f, 0f, 7f);
                self.Runtime.SyncIntegerPosition();

                partner.Runtime.OidMergeDormant = true;
                partner.Runtime.SetPosition(60f, 0f, 7f);
                partner.Runtime.SyncIntegerPosition();

                SetPrivateField(self.InputState, "_comboDJA", (byte)2);
                ((SelfCheckController)self.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);

                var tickSystem = new NTSDBattleTickSystem(world);
                tickSystem.RunReleaseTick(1);

                Expect(self.ObjectId == 7 && partner.ObjectId == 8 && world.ObjectCount == 2,
                    "DJA release in PostCooldownInput must reach M-1 on the same tick and trigger immediate split");
                Expect(self.Runtime.Unk338 == 900,
                    "same-tick DJA release split must end with split cooldown 900");

                LF2Character djaOnly = CreateCharacter("SelfCheck_Oid51_DjaOnly", 51, wrappers[51].characterData);
                djaOnly.ImmediateFrame(290);
                djaOnly.Runtime.Unk328 = 1;
                djaOnly.Runtime.Unk338 = 77;
                SetPrivateField(djaOnly.InputState, "_comboDJA", (byte)3);
                djaOnly.ApplyFrameInputFromLocalState();

                Expect(djaOnly.Runtime.Unk338 == 77,
                    "missing DJA target must not fall through to the merged Unk338 release branch");
                Expect((byte)GetPrivateField(djaOnly.InputState, "_comboDJA") == 0,
                    "nonzero hit_ja DJA must clear comboDJA even when the target frame is absent");

                wrappers[51].characterData.frames.Add(Frame(300, 0, 1, 300, 39, 79));
                LF2Character validDja = CreateCharacter("SelfCheck_Oid51_ValidDja", 51, wrappers[51].characterData);
                validDja.ImmediateFrame(290);
                validDja.Runtime.Unk328 = 1;
                validDja.Runtime.Unk338 = 66;
                SetPrivateField(validDja.InputState, "_comboDJA", (byte)3);
                validDja.ApplyFrameInputFromLocalState();
                Expect(validDja.Frame.N == 300 && validDja.Runtime.Unk338 == 66 &&
                       (byte)GetPrivateField(validDja.InputState, "_comboDJA") == 0,
                    "valid merged DJA target must jump and clear comboDJA without releasing the split timer");

                SimulationWorld aiWorld = CreateOid5152MergedWorld(
                    wrappers, out LF2Character aiSelf, out LF2Character aiPartner);
                aiSelf.AiControlled = true;
                aiSelf.TransformOriginalObjectId = 77;
                aiSelf.Runtime.ComboDja = 3;
                aiSelf.Runtime.Unk338 = 30;
                var aiTickSystem = new NTSDBattleTickSystem(aiWorld);
                aiTickSystem.RunReleaseTick(2);
                Expect(aiSelf.ObjectId == 7 && aiPartner.ObjectId == 8 && aiWorld.ObjectCount == 2 &&
                       aiSelf.Runtime.Unk338 == 900 && aiSelf.Runtime.ComboDja == 3,
                    "AI post-cooldown DJA release must run before M-1 and split on the same tick while preserving comboDJA");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckRespawnPassWithoutStoredCount()
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter("SelfCheck_Respawn_NoCount", 1, BuildRespawnCharacterData("SelfCheck_Respawn_NoCount"));
            LF2Character allyA = CreateCharacter("SelfCheck_Respawn_AllyA", 2, BuildRespawnCharacterData("SelfCheck_Respawn_AllyA"));
            LF2Character allyB = CreateCharacter("SelfCheck_Respawn_AllyB", 3, BuildRespawnCharacterData("SelfCheck_Respawn_AllyB"));
            dead.SetRuntimeSlotIndex(0);
            allyA.SetRuntimeSlotIndex(1);
            allyB.SetRuntimeSlotIndex(2);
            world.Register(dead);
            world.Register(allyA);
            world.Register(allyB);

            dead.RelationTeam = 5;
            allyA.RelationTeam = 5;
            allyB.RelationTeam = 5;
            dead.ImmediateFrame(14);
            dead.Health.HP = 0;
            dead.Health.HP3 = 180;
            dead.Health.HPBound = 60;
            dead.HP2Orig = 3;
            dead.Health.PP = 12;
            dead.HitStun = 3;
            dead.Runtime.SetPosition(40.0, 0.0, 5.0);
            dead.Runtime.SetVelocity(0.0, -7.0, 0.0);
            dead.Runtime.SyncIntegerPosition();

            allyA.Runtime.SetPosition(100.0, 0.0, 40.0);
            allyA.Runtime.SetVelocity(0.0, 0.0, 0.0);
            allyA.Runtime.SyncIntegerPosition();
            allyB.Runtime.SetPosition(160.0, 0.0, 20.0);
            allyB.Runtime.SetVelocity(0.0, 0.0, 0.0);
            allyB.Runtime.SyncIntegerPosition();

            DeterministicRng expectedRng = new DeterministicRng(0x4E545344u);
            int expectedX = 130 + expectedRng.NextInt(0, 51) - 26;
            int expectedZ = 30 + expectedRng.NextInt(0, 31) - 16;

            world.PostFrameAdvanceDeathCleanupAll(1);

            Expect(dead.HP2Orig == 2,
                "respawn no-count branch must decrement HP2 overlay by 1");
            Expect(dead.Health.HP == 180 && dead.Health.HPBound == 180,
                "respawn no-count branch must restore HP and HPBound from HP3");
            Expect(dead.Health.PP == 500,
                "respawn no-count branch must refill PP to 500");
            Expect(dead.CurrentFrameId == 212 && dead.HitStun == 20,
                "respawn no-count branch must enter frame 212 and arm 20 hit stop");
            Expect(dead.GetRuntimeYInt() == -300 && Nearly(dead.Runtime.Vy, 0.0),
                "respawn no-count branch must set y to -300 and zero Vy");
            Expect(dead.GetRuntimeXInt() == expectedX && dead.GetRenderZInt() == expectedZ,
                $"respawn no-count branch must respawn around same-relation teammates using release RNG offsets; " +
                $"expected=({expectedX},{expectedZ}) actual=({dead.GetRuntimeXInt()},{dead.GetRenderZInt()}) " +
                $"runtimeXZ=({dead.Runtime.X},{dead.Runtime.Z}) alliesXZ=({allyA.GetRuntimeXInt()},{allyA.GetRenderZInt()})/({allyB.GetRuntimeXInt()},{allyB.GetRenderZInt()})");
        }

        private static void CheckRespawnPassFreeEntityGate()
        {
            var world = new SimulationWorld();
            LF2Character freed = CreateCharacter("SelfCheck_Respawn_Free", 1, BuildRespawnCharacterData("SelfCheck_Respawn_Free"));
            LF2Character gated = CreateCharacter("SelfCheck_Respawn_Gated", 2, BuildRespawnCharacterData("SelfCheck_Respawn_Gated"));
            freed.SetRuntimeSlotIndex(0);
            gated.SetRuntimeSlotIndex(1);
            world.Register(freed);
            world.Register(gated);

            freed.RelationTeam = 5;
            freed.ImmediateFrame(14);
            freed.Health.HP = 0;
            freed.HP2Orig = 1;
            freed.HitStun = 2;

            gated.RelationTeam = 4;
            gated.ImmediateFrame(14);
            gated.Health.HP = 0;
            gated.HP2Orig = 5;
            gated.HitStun = 2;
            gated.Runtime.SetPosition(33.0, 0.0, 12.0);
            gated.Runtime.SetVelocity(0.0, 0.0, 0.0);
            gated.Runtime.SyncIntegerPosition();

            world.PostFrameAdvanceDeathCleanupAll(2);

            Expect(world.FindEntityByRuntimeSlotForQuery(0) == null,
                "respawn no-count branch must free entity immediately when HP2Orig < 2");
            Expect(gated.CurrentFrameId == 14 && gated.HP2Orig == 5 &&
                   gated.GetRuntimeXInt() == 33 && gated.GetRenderZInt() == 12,
                "respawn pass must respect slot<20 + relation/kill gate and leave gated lying entity unchanged");
        }

        private static void CheckRespawnPassWithStoredCountAndEffectSpawn()
        {
            System.Func<SimulationWorld, LF2Entity, LF2Entity> previousOverride = SimulationWorld.RespawnEffectSpawnOverride;
            RespawnSelfCheckEffectEntity spawned = null;
            try
            {
                SimulationWorld.RespawnEffectSpawnOverride = (world, source) =>
                {
                    spawned = new RespawnSelfCheckEffectEntity();
                    spawned.BindData(998, BuildRespawnEffectData());
                    spawned.RelationTeam = source.RelationTeam;
                    spawned.SpawnerEntityIndex = source.Runtime?.SlotIndex ?? -1;
                    spawned.Runtime.SetPosition(source.GetRuntimeXInt(), source.GetRuntimeYInt(), source.GetRenderZInt() + 1.0);
                    spawned.Runtime.SetVelocity(0.0, 0.0, 0.0);
                    spawned.Runtime.SyncIntegerPosition();
                    spawned.SetRuntimeSlotIndex(25);
                    world.Register(spawned);
                    return spawned;
                };

                var world = new SimulationWorld();
                LF2Character dead = CreateCharacter("SelfCheck_Respawn_WithCount", 0x1E, BuildRespawnCharacterData("SelfCheck_Respawn_WithCount"));
                dead.SetRuntimeSlotIndex(0);
                world.Register(dead);

                dead.RelationTeam = 3;
                dead.KillCount = 0;
                dead.ImmediateFrame(14);
                dead.Health.HP = 0;
                dead.Health.PP = 77;
                dead.Health.HPBound = 10;
                dead.Health.HP3 = 10;
                dead.HPOrig = 6;
                dead.HP2Orig = 4;
                dead.RespawnCount = 80;
                dead.AttackingCounter = 9;
                dead.HitStun = 4;
                dead.Runtime.SetPosition(77.0, -12.0, 19.0);
                dead.Runtime.SetVelocity(0.0, 0.0, 0.0);
                dead.Runtime.SyncIntegerPosition();

                world.PostFrameAdvanceDeathCleanupAll(3);

                Expect(dead.HP2Orig == 6 && dead.HPOrig == 0,
                    "respawn stored-count branch must copy HP overlay before clearing HPOrig");
                Expect(dead.Health.PP == 0,
                    "respawn stored-count branch must zero PP");
                Expect(dead.Health.HP == 80 && dead.Health.HPBound == 80 && dead.Health.HP3 == 80,
                    "respawn stored-count branch must restore HP/HPBound/HP3 from RespawnCount");
                Expect(dead.RespawnCount == 0 && dead.RelationTeam == 1,
                    "respawn stored-count branch must clear RespawnCount and reset relation identity to 1");
                Expect(dead.Runtime.RenderPicOffset == 0x8C,
                    "respawn stored-count branch must write render pic offset 0x8C for oid 0x1E..0x24");
                Expect(dead.CurrentFrameId == 0xDB && dead.FrameDelay == 0xA && dead.AttackingCounter == 0,
                    "respawn stored-count branch must enter frame 0xDB with frame delay 10 and clear attacking");
                Expect(spawned != null && world.ObjectCount == 2,
                    "respawn stored-count branch must spawn oid998 effect into the world");
                Expect(spawned.ObjectId == 998 && (spawned.Frame?.N ?? -1) == 6,
                    "respawn effect spawn must use oid998 frame 6");
                Expect(spawned.GetRuntimeXInt() == 77 &&
                       spawned.GetRuntimeYInt() == -12 &&
                       spawned.GetRenderZInt() == 20,
                    "respawn effect spawn must copy x/y and use z_int + 1");
                Expect(spawned.RelationTeam == 1 && spawned.SpawnerEntityIndex == dead.Runtime.SlotIndex,
                    "respawn effect spawn must inherit post-respawn relation identity and spawner slot");
            }
            finally
            {
                SimulationWorld.RespawnEffectSpawnOverride = previousOverride;
            }
        }

        private static void CheckKind15CharacterWhirlwind()
        {
            var world = new SimulationWorld();
            LF2CharacterData data = BuildKind1516CharacterData("SelfCheck_Kind1516");
            LF2Character attacker = CreateCharacter("SelfCheck_Kind15_Attacker", 1, data);
            LF2Character groundedVictim = CreateCharacter("SelfCheck_Kind15_Grounded", 2, data);
            LF2Character airVictim = CreateCharacter("SelfCheck_Kind15_Air", 3, data);

            world.Register(attacker);
            world.Register(groundedVictim);
            world.Register(airVictim);

            attacker.ImmediateFrame(0);
            groundedVictim.ImmediateFrame(0);
            airVictim.ImmediateFrame(0);

            attacker.Runtime.SetPosition(0.0, 0.0, 0.0);
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            attacker.Runtime.SyncIntegerPosition();

            groundedVictim.Runtime.SetPosition(10.0, 0.0, 5.0);
            groundedVictim.Runtime.SetVelocity(2.0, -5.0, 1.0);
            groundedVictim.Runtime.SyncIntegerPosition();
            groundedVictim.KnockbackVx = 0.0;
            groundedVictim.KnockbackVy = 0.0;
            groundedVictim.KnockbackVz = 0.0;

            bool groundedResolved = groundedVictim.Hit(
                new InteractionArea { kind = 15 },
                attacker,
                Vector3.zero,
                default);

            Expect(groundedResolved, "kind15 should resolve on grounded character victim");
            Expect(Mathf.Approximately((float)groundedVictim.Runtime.Vx, 1f) &&
                   Mathf.Approximately((float)groundedVictim.KnockbackVx, 1f),
                "kind15 should rewrite victim vx from runtime vx ± 1");
            Expect(Mathf.Approximately((float)groundedVictim.Runtime.Vz, 0.5f) &&
                   Mathf.Approximately((float)groundedVictim.KnockbackVz, 0.5f),
                "kind15 should rewrite victim vz from runtime vz ± 0.5");
            Expect(groundedVictim.GetRuntimeYInt() == -2 &&
                   Mathf.Approximately((float)groundedVictim.Runtime.Y, -2f) &&
                   Mathf.Approximately((float)groundedVictim.Runtime.Vy, -6f),
                "kind15 grounded branch should clamp Y/YInt to -2 and set Vy=-6");

            airVictim.Runtime.SetPosition(10.0, -5.0, 5.0);
            airVictim.Runtime.SetVelocity(0.0, -5.0, 0.0);
            airVictim.Runtime.SyncIntegerPosition();
            airVictim.KnockbackVx = 0.0;
            airVictim.KnockbackVy = 0.0;
            airVictim.KnockbackVz = 0.0;

            bool airResolved = airVictim.Hit(
                new InteractionArea { kind = 15 },
                attacker,
                Vector3.zero,
                default);

            Expect(airResolved, "kind15 should resolve on airborne character victim");
            Expect(airVictim.GetRuntimeYInt() == -5, "kind15 airborne branch should preserve YInt below -2");
            Expect(Mathf.Approximately((float)airVictim.Runtime.Vy, -8f) &&
                   Mathf.Approximately((float)airVictim.KnockbackVy, -8f),
                "kind15 airborne branch should subtract vyStep=3.0 and mirror KnockbackVy");
        }

        private static void CheckKind16CharacterSideEffects()
        {
            var world = new SimulationWorld();
            LF2CharacterData data = BuildKind1516CharacterData("SelfCheck_Kind1516");
            LF2Character holder = CreateCharacter("SelfCheck_Kind16_Holder", 1, data);
            LF2Character attacker = CreateCharacter("SelfCheck_Kind16_Attacker", 2, data);
            LF2Character victim = CreateCharacter("SelfCheck_Kind16_Victim", 3, data);
            LF2Character heldTarget = CreateCharacter("SelfCheck_Kind16_HeldTarget", 4, data);

            world.Register(holder);
            world.Register(attacker);
            world.Register(victim);
            world.Register(heldTarget);

            holder.ImmediateFrame(0);
            attacker.ImmediateFrame(0);
            victim.ImmediateFrame(0);
            heldTarget.ImmediateFrame(10);

            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            victim.Health.HP = 70;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 0;
            victim.FallDamageDiv = 50;
            victim.KillCount = -1;
            victim.ComboCountVic = 0;
            victim.AttackingCounter = 5;
            victim.Runtime.LinkState = 2;
            victim.Runtime.TargetSlotIndex = heldTarget.Runtime.SlotIndex;

            heldTarget.Runtime.LinkState = -2;
            heldTarget.Runtime.HolderStableId = victim.Runtime.SlotIndex;
            heldTarget.Runtime.Vy = 0.0;

            bool resolved = victim.Hit(
                new InteractionArea
                {
                    kind = 16,
                    injury = 40,
                    vrest = 12,
                },
                attacker,
                Vector3.zero,
                default);

            Expect(resolved, "kind16 should resolve on character victim");
            Expect(victim.Health.HP == -10, "kind16 should scale injury by FallDamageDiv rather than MaxMP");
            Expect(victim.Health.HPBound == 74, "kind16 should reduce HPBound by adjustedInjury/3 with integer division");
            Expect(victim.Health.HPLost == 0, "kind16 should not accumulate HPLost via generic injury path");
            Expect(victim.ComboCountVic == 80, "kind16 should add adjusted injury to victim combo counter");
            Expect(holder.KillStat == 1, "kind16 lethal hit should increment holder KillStat");
            Expect(holder.ComboCountAtk == 80, "kind16 should add adjusted injury to holder ComboCountAtk");
            Expect(victim.Frame.N == LF2StandardFrames.MpDrain && victim.AttackingCounter == 0,
                "kind16 should jump victim to frame 200 and clear attacking counter");
            Expect(victim.ItrRest.GetVrest(attacker.Runtime.SlotIndex) == 45,
                "kind16 release path should overwrite attacker-side vrest to 45 when victim is holding a target");
            Expect(victim.ItrRest.GetVrest(heldTarget.Runtime.SlotIndex) == 30,
                "kind16 release path should write held-target vrest=30");
            Expect(victim.Runtime.LinkState == 0 && heldTarget.Runtime.LinkState == 0,
                "kind16 should break 2/-2 hold links");
            Expect(Mathf.Approximately((float)heldTarget.Runtime.Vy, -1f),
                "kind16 should launch released held target with Vy=-1");
        }

        private static void CheckLateDeathBounceFrame()
        {
            var world = new SimulationWorld();
            LF2CharacterData data = BuildDeathBounceCharacterData("SelfCheck_DeathBounce");
            LF2Character victim = CreateCharacter("SelfCheck_DeathBounceVictim", 1, data);
            world.Register(victim);

            victim.ImmediateFrame(5);
            victim.Health.HP = 0;
            victim.Runtime.SetPosition(12.0, 0.0, 3.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
            victim.KnockbackVy = 0f;

            victim.RunLateDeathOpointPreCleanupPhase();

            Expect(victim.Frame.N == 186,
                "late death bounce should force frame 186 for dead lying character in frame<12");
            Expect(victim.GetRuntimeYInt() == -1 &&
                   Mathf.Approximately((float)victim.Runtime.Y, -1f) &&
                   Mathf.Approximately((float)victim.Runtime.Vy, -3f) &&
                   Mathf.Approximately((float)victim.KnockbackVy, -3f),
                "late death bounce should set y/yInt to -1 and vy/knockbackVy to -3");

            victim.ImmediateFrame(212);
            victim.Health.HP = 0;
            victim.Runtime.SetPosition(12.0, 0.0, 3.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
            victim.KnockbackVy = 0f;

            victim.RunLateDeathOpointPreCleanupPhase();

            Expect(victim.Frame.N == 186,
                "late death bounce should re-launch grounded death frame 212");
        }

        private static void CheckComboWrappersCharacterFrameJumps()
        {
            AssertComboFrameJump(
                "SelfCheck_DRA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DRA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.right, FuncKeyMask.jump },
                100,
                "right",
                verifyCooldownClear: true);

            AssertComboFrameJump(
                "SelfCheck_DLA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DLA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.left, FuncKeyMask.jump },
                100,
                "left");

            AssertComboFrameJump(
                "SelfCheck_DUA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DUA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.up, FuncKeyMask.jump },
                101,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DDA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DDA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.down, FuncKeyMask.jump },
                102,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DRJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DRJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.right, FuncKeyMask.def },
                103,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DLJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DLJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.left, FuncKeyMask.def },
                103,
                "left");

            AssertComboFrameJump(
                "SelfCheck_DUJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DUJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.up, FuncKeyMask.def },
                104,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DDJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DDJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.down, FuncKeyMask.def },
                105,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DJA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DJA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.def, FuncKeyMask.jump },
                180,
                "right");
        }

        private static void CheckComboLocalShadowCommitContracts()
        {
            LF2Character partial = CreateCharacter(
                "SelfCheck_ComboLocalPartial",
                1,
                BuildComboWrapperCharacterData("SelfCheck_ComboLocalPartial", 180));
            SetComboLocalSeeds(partial.InputState, ordinaryValue: 1, djaValue: 0);
            ((SelfCheckController)partial.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.att, true);
            partial.RunPostCooldownInputPhase(1);
            ExpectComboLocalSeeds(partial.InputState, ordinaryValue: 0, djaValue: 1,
                "formal input direct mutation must commit partial combo progress before the early return");

            LF2Character failedJump = CreateCharacter(
                "SelfCheck_ComboLocalFailedDjaJump",
                1,
                BuildComboWrapperCharacterData("SelfCheck_ComboLocalFailedDjaJump", 399));
            SetComboLocalSeeds(failedJump.InputState, ordinaryValue: 1, djaValue: 2);
            ((SelfCheckController)failedJump.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);
            failedJump.RunPostCooldownInputPhase(1);
            Expect(failedJump.Frame.N == 0,
                "BATTLE-AUDIT3-18 missing DJA target must leave the current frame unchanged");
            ExpectComboLocalSeeds(failedJump.InputState, ordinaryValue: 0, djaValue: 0,
                "formal input direct mutation must commit all wrapper interrupts and failed-target DJA reset");

            LF2Character unk328 = CreateCharacter(
                "SelfCheck_ComboLocalUnk328",
                1,
                BuildComboWrapperCharacterData("SelfCheck_ComboLocalUnk328", 300));
            unk328.TransformOriginalObjectId = 77;
            unk328.Runtime.Unk328 = 1;
            unk328.Runtime.Unk338 = 91;
            SetComboLocalSeeds(unk328.InputState, ordinaryValue: 1, djaValue: 2);
            ((SelfCheckController)unk328.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);
            unk328.RunPostCooldownInputPhase(1);
            Expect(unk328.Frame.N == 0 && unk328.Runtime.Unk338 == 0,
                "BATTLE-AUDIT3-18 Unk328 branch must clear only Unk338 and keep the current frame");
            ExpectComboLocalSeeds(unk328.InputState, ordinaryValue: 0, djaValue: 3,
                "Unk328 release must commit ordinary wrapper interrupts while preserving completed DJA state");

            LF2Character committed = CreateCharacter(
                "SelfCheck_ComboLocalCommit",
                1,
                BuildComboWrapperCharacterData("SelfCheck_ComboLocalCommit", 0));
            SetComboLocalSeeds(committed.InputState, ordinaryValue: 1, djaValue: 2);
            ((SelfCheckController)committed.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);
            committed.RunPostCooldownInputPhase(1);
            ExpectComboLocalSeeds(committed.InputState, ordinaryValue: 0, djaValue: 3,
                "BATTLE-AUDIT3-18 normal DJA tail must commit all ordinary locals and the DJA local");
        }

        private static void SetComboLocalSeeds(NTSDInputStateModule input, byte ordinaryValue, byte djaValue)
        {
            SetPrivateField(input, "_comboDRA", ordinaryValue);
            SetPrivateField(input, "_comboDLA", ordinaryValue);
            SetPrivateField(input, "_comboDUA", ordinaryValue);
            SetPrivateField(input, "_comboDDA", ordinaryValue);
            SetPrivateField(input, "_comboDRJ", ordinaryValue);
            SetPrivateField(input, "_comboDLJ", ordinaryValue);
            SetPrivateField(input, "_comboDUJ", ordinaryValue);
            SetPrivateField(input, "_comboDDJ", ordinaryValue);
            SetPrivateField(input, "_comboDJA", djaValue);
        }

        private static void ExpectComboLocalSeeds(
            NTSDInputStateModule input,
            byte ordinaryValue,
            byte djaValue,
            string message)
        {
            bool ordinaryMatch =
                (byte)GetPrivateField(input, "_comboDRA") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDLA") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDUA") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDDA") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDRJ") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDLJ") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDUJ") == ordinaryValue &&
                (byte)GetPrivateField(input, "_comboDDJ") == ordinaryValue;
            Expect(ordinaryMatch && (byte)GetPrivateField(input, "_comboDJA") == djaValue, message);
        }

        private static void CheckNarutoDdjSixCloneProductionChain()
        {
            // Use the production-loaded DAT graph so this covers recursive late-pass spawning and presentation.
            CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
            Expect(animatorManager != null,
                "Naruto DDJ regression requires the production CharacterAnimtorManager singleton");

            LF2CharacterDataWrapper narutoWrapper = LoadProductionDatWrapper(
                animatorManager, 2, "Assets/NTSD/Config/AnimationConfig/Mingren/naruto.dat");
            LF2CharacterDataWrapper cloneWrapper = LoadProductionDatWrapper(
                animatorManager, 33, "Assets/NTSD/Config/FrameConfig/naruto_clone.dat");
            LF2CharacterDataWrapper windWrapper = LoadProductionDatWrapper(
                animatorManager, 204, "Assets/NTSD/Config/specialattack/wind.dat");
            LF2CharacterDataWrapper poisonWrapper = LoadProductionDatWrapper(
                animatorManager, 205, "Assets/NTSD/Config/specialattack/poison.dat");
            Expect(narutoWrapper?.characterData != null &&
                   cloneWrapper?.characterData != null &&
                   windWrapper?.characterData != null &&
                   poisonWrapper?.characterData != null,
                "Naruto DDJ regression requires real oid 2/33/204/205 DAT configs");

            LF2FrameData standing = narutoWrapper.characterData.frames?.Find(frame => frame.frameId == 0);
            Expect(standing != null && standing.hit_Dj == 271,
                "Naruto real standing DAT must map internal att-down-def to frame 271");
            LF2FrameData frame272Data = narutoWrapper.characterData.frames?.Find(frame => frame.frameId == 272);
            bool frame272SpawnsPoison98 =
                (frame272Data?.opoints?.Exists(op => op.kind > 0 && op.oid == 205 && op.action == 98) ?? false) ||
                (frame272Data?.opoint.HasValue == true &&
                 frame272Data.opoint.Value.kind > 0 && frame272Data.opoint.Value.oid == 205 &&
                 frame272Data.opoint.Value.action == 98);
            Expect(frame272SpawnsPoison98,
                "Naruto real frame 272 DAT must author oid205/action98 before same-tick late scanning advances it");

            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [2] = narutoWrapper,
                [33] = cloneWrapper,
                [204] = windWrapper,
                [205] = poisonWrapper,
            };
            var objectTypes = new Dictionary<int, int>
            {
                [2] = (int)LF2ObjectType.Character,
                [33] = (int)LF2ObjectType.Character,
                [204] = (int)LF2ObjectType.SpecialAttack,
                [205] = (int)LF2ObjectType.SpecialAttack,
            };
            using var runtimeConfigs = new TemporaryRuntimeObjectConfigs(objectTypes, wrappers);
            using var cloneSprites = new TemporaryCharacterSpriteConfig(animatorManager, 33, 1000);
            using var objectPoolState = new TemporaryObjectPoolInitialization();
            var world = new SimulationWorld();
            using var driverWorld = new TemporarySimulationDriverWorld(world);
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            Expect(factory != null,
                "Naruto DDJ regression requires the production LF2ObjectPointFactory singleton");
            Expect(GameDataManager.Instance?.GetObjectById(205)?.type == (int)LF2ObjectType.SpecialAttack &&
                   animatorManager.GetCharacterConfig(205)?.characterData != null,
                "Naruto DDJ fixture must expose oid205 type/config through the production managers");
            factory.FlushTasks();

            LF2Character naruto = CreateCharacter(
                "SelfCheck_NarutoDdjSixClone",
                2,
                narutoWrapper.characterData);
            naruto.SwitchDir("right");
            naruto.Team = 1;
            naruto.RelationTeam = 7;
            const int expectedHolderCopySlot = 23;
            naruto.HolderCopySlot = expectedHolderCopySlot;
            naruto.Health.PP = 500;
            naruto.SetRuntimeSlotIndex(0);
            world.Register(naruto);
            var tickSystem = new NTSDBattleTickSystem(world);

            SelfCheckController narutoController = (SelfCheckController)naruto.Controller;
            narutoController.InputBuffer.EnqueueForTick(1, FuncKeyMask.att, true);
            narutoController.InputBuffer.EnqueueForTick(1, FuncKeyMask.down, true);
            narutoController.InputBuffer.EnqueueForTick(1, FuncKeyMask.def, true);
            var comboTrace = new List<string>();
            tickSystem.RunReleaseTick(1);
            comboTrace.Add(
                $"t1:frame={naruto.Frame.N},cdD={naruto.Runtime.CdDefend}," +
                $"cdDown={naruto.Runtime.CdDown},cdJ={naruto.Runtime.CdJump}," +
                $"combo={naruto.Runtime.ComboDdj},pp={naruto.Health.PP}");
            LF2FrameData target271 = naruto.FrameCache.GetFrameDataById(271);
            Expect(naruto.Frame.N == 271,
                $"Naruto physical defend-down-jump (internal att-down-def) must enter frame 271; " +
                $"has271={naruto.FrameCache.HasFrame(271)},targetMp={target271?.mp.ToString() ?? "null"}," +
                $"trace={string.Join(" | ", comboTrace)}");
            narutoController.InputBuffer.EnqueueForTick(2, FuncKeyMask.att, false);
            narutoController.InputBuffer.EnqueueForTick(2, FuncKeyMask.down, false);
            narutoController.InputBuffer.EnqueueForTick(2, FuncKeyMask.def, false);

            var poisonFrames = new HashSet<int>();
            var windFrames = new HashSet<int>();
            var windFrame147StableIds = new HashSet<int>();
            var cloneRuntimeSlots = new HashSet<int>();
            var cloneStableIds = new HashSet<int>();
            var clonesSeenAtAction307 = new HashSet<int>();
            var visibleCloneStableIds = new HashSet<int>();
            bool sawFrame272 = false;
            bool allOpointRelationIdentitiesMatch = true;
            bool allOpointSlotsAreDynamic = true;
            bool allCloneStableIdsArePositive = true;
            var relationMismatchTrace = new HashSet<string>();
            var relationCheckedStableIds = new HashSet<int>();
            int ppAfterCost = naruto.Health.PP;
            bool capturedPpAfterCost = false;
            var runtimeTrace = new List<string>();

            try
            {
                for (int tick = 2; tick < 240 && visibleCloneStableIds.Count < 6; tick++)
                {
                    tickSystem.RunReleaseTick(tick);

                    sawFrame272 |= naruto.Frame.N == 272;
                    if (!capturedPpAfterCost && naruto.Health.PP != 500)
                    {
                        ppAfterCost = naruto.Health.PP;
                        capturedPpAfterCost = true;
                    }

                    var tickEntities = new List<string>();
                    for (int slot = 0; slot < 400; slot++)
                    {
                        LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                        if (entity == null)
                            continue;

                        if (tick < 24)
                        {
                            LF2FrameData entityFrame = entity.Frame?.D;
                            ObjectPoint? entityOpoint = entityFrame?.opoint;
                            tickEntities.Add(
                                $"{slot}:{entity.ObjectId}/{entity.Frame?.N ?? -1}/{entity.GetType().Name}/" +
                                $"atk{entity.AttackingCounter}/wait{entity.Trans?.Wait ?? -1}/" +
                                $"wc{entity.Trans?.WaitCounter ?? -1}/next{entity.Trans?.Next ?? -1}/" +
                                $"op{entityOpoint?.kind ?? -1}:{entityOpoint?.oid ?? -1}:{entityOpoint?.action ?? -1}");
                        }

                        if (entity.ObjectId == 205)
                        {
                            allOpointSlotsAreDynamic &= entity.Runtime.SlotIndex >= 50;
                            bool relationMatches =
                                entity.Team == naruto.Team && entity.RelationTeam == naruto.RelationTeam;
                            if (relationCheckedStableIds.Add(entity.StableId))
                                relationMatches &= entity.HolderCopySlot == expectedHolderCopySlot;
                            allOpointRelationIdentitiesMatch &= relationMatches;
                            if (!relationMatches && relationMismatchTrace.Count < 12)
                                relationMismatchTrace.Add(
                                    $"t{tick}/s{slot}/oid205:team{entity.Team}/rel{entity.RelationTeam}/holder{entity.HolderCopySlot}");
                            poisonFrames.Add(entity.Frame?.N ?? -1);
                        }
                        else if (entity.ObjectId == 204)
                        {
                            allOpointSlotsAreDynamic &= entity.Runtime.SlotIndex >= 50;
                            bool relationMatches =
                                entity.Team == naruto.Team && entity.RelationTeam == naruto.RelationTeam;
                            if (relationCheckedStableIds.Add(entity.StableId))
                                relationMatches &= entity.HolderCopySlot == expectedHolderCopySlot;
                            allOpointRelationIdentitiesMatch &= relationMatches;
                            if (!relationMatches && relationMismatchTrace.Count < 12)
                                relationMismatchTrace.Add(
                                    $"t{tick}/s{slot}/oid204:team{entity.Team}/rel{entity.RelationTeam}/holder{entity.HolderCopySlot}");
                            windFrames.Add(entity.Frame?.N ?? -1);
                            if (entity.Frame?.N == 147)
                                windFrame147StableIds.Add(entity.StableId);
                        }
                        else if (entity.ObjectId == 33)
                        {
                            allOpointSlotsAreDynamic &= entity.Runtime.SlotIndex >= 50;
                            bool relationMatches =
                                entity.Team == naruto.Team && entity.RelationTeam == naruto.RelationTeam;
                            if (relationCheckedStableIds.Add(entity.StableId))
                                relationMatches &= entity.HolderCopySlot == expectedHolderCopySlot;
                            allOpointRelationIdentitiesMatch &= relationMatches;
                            if (!relationMatches && relationMismatchTrace.Count < 12)
                                relationMismatchTrace.Add(
                                    $"t{tick}/s{slot}/oid33:team{entity.Team}/rel{entity.RelationTeam}/holder{entity.HolderCopySlot}");
                            cloneRuntimeSlots.Add(entity.Runtime.SlotIndex);
                            cloneStableIds.Add(entity.StableId);
                            allCloneStableIdsArePositive &= entity.StableId > 0;
                            if (entity.Frame?.N == 307)
                                clonesSeenAtAction307.Add(entity.StableId);

                            SpriteRenderer spriteRenderer = entity.Renderer != null
                                ? entity.Renderer.GetComponent<SpriteRenderer>()
                                : null;
                            if (entity.Renderer != null && !IsSimulationObjectRegistered(world, entity.Renderer))
                                world.Register(entity.Renderer);
                            if (spriteRenderer != null && spriteRenderer.enabled && spriteRenderer.sprite != null)
                                visibleCloneStableIds.Add(entity.StableId);
                        }
                    }

                    if (tick < 24)
                    {
                        LF2FrameData currentNarutoData = naruto.Frame?.D;
                        int currentOpointCount = currentNarutoData?.opoints?.Count ?? 0;
                        ObjectPoint? currentOpoint = currentNarutoData?.opoint;
                        runtimeTrace.Add(
                            $"t{tick}:naruto={naruto.Frame.N}/D{currentNarutoData?.frameId.ToString() ?? "null"}/" +
                            $"atk{naruto.AttackingCounter}/delay{naruto.FrameDelay}/ops{currentOpointCount}/" +
                            $"op{currentOpoint?.kind ?? -1}:{currentOpoint?.oid ?? -1}:{currentOpoint?.action ?? -1};" +
                            $"objects=[{string.Join(",", tickEntities)}]");
                    }
                }

                Expect(sawFrame272,
                    "Naruto DDJ frame_tick must advance 271 to the real cost/opoint frame 272");
                Expect(ppAfterCost == 295,
                    $"Naruto frame 272 mp=-205 must reduce PP from 500 to 295; actual={ppAfterCost}");
                Expect(poisonFrames.Contains(99) && poisonFrames.Contains(325) && poisonFrames.Contains(341),
                    $"oid205 helper must traverse actions 98/99/325/341; observed={string.Join(",", poisonFrames)}; " +
                    $"trace={string.Join(" | ", runtimeTrace)}");
                Expect(windFrames.Contains(131),
                    $"Naruto frame 273 oid204/action130 root must advance to observable action131; " +
                    $"observed={string.Join(",", windFrames)}");
                Expect(allOpointSlotsAreDynamic,
                    "Naruto oid205/204/33 opoints must use the authority dynamic runtime range 50..399");
                Expect(allOpointRelationIdentitiesMatch,
                    $"Naruto oid205/204/33 opoints must inherit Team, RelationTeam/Unk364, and HolderCopy " +
                    $"from their spawner; expected={naruto.Team}/{naruto.RelationTeam}/{expectedHolderCopySlot}; " +
                    $"mismatches={string.Join(",", relationMismatchTrace)}");
                Expect(cloneRuntimeSlots.Count == 6,
                    $"Naruto DDJ oid204 chain must create exactly six logical oid33 clones; " +
                    $"actual={cloneRuntimeSlots.Count}; wind={string.Join(",", windFrames)}; " +
                    $"wind147Parents={windFrame147StableIds.Count}[{string.Join(",", windFrame147StableIds)}]; " +
                    $"cloneStableIds={cloneStableIds.Count}[{string.Join(",", cloneStableIds)}]; " +
                    $"poison={string.Join(",", poisonFrames)}; trace={string.Join(" | ", runtimeTrace)}");
                Expect(allCloneStableIdsArePositive && cloneStableIds.Count == 6,
                    $"all six Naruto clone opoints must receive unique positive StableIds before registration; " +
                    $"actual={cloneStableIds.Count}, ids={string.Join(",", cloneStableIds)}");
                Expect(clonesSeenAtAction307.Count == 6,
                    $"all six Naruto clones must be observed at spawn action 307; actual={clonesSeenAtAction307.Count}");
                Expect(visibleCloneStableIds.Count == 6,
                    $"all six Naruto clones must leave hidden 307/308 and acquire visible production sprites; " +
                    $"actual={visibleCloneStableIds.Count}");
            }
            finally
            {
                var spawned = new List<LF2Entity>();
                for (int slot = 50; slot < 400; slot++)
                {
                    LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                    if (entity?.Renderer != null)
                        spawned.Add(entity);
                }

                for (int i = 0; i < spawned.Count; i++)
                    spawned[i].FreeEntityLikeExe();

                factory.FlushTasks();
            }
        }

        private static LF2CharacterDataWrapper LoadProductionDatWrapper(
            CharacterAnimtorManager animatorManager,
            int objectId,
            string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            string datPath = Path.GetFullPath(Path.Combine(projectRoot ?? string.Empty, projectRelativePath));
            Expect(File.Exists(datPath), $"production DAT fixture is missing: {projectRelativePath}");

            string datText = Lf2DatDecryptor.DecryptFile(
                datPath,
                "odBearBecauseHeIsVeryGoodSiuHungIsAGo");
            var parser = new Lf2DatParserV2();
            Lf2DatFile datFile = parser.Parse(datText, datPath);
            Expect(datFile != null && datFile.Frames.Count > 0,
                $"production DAT fixture failed to parse: {projectRelativePath}");

            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            System.Reflection.MethodInfo buildMethod = typeof(CharacterAnimtorManager).GetMethod(
                "BuildCharacterDataFromDat",
                flags);
            Expect(buildMethod != null,
                "production DAT fixture CharacterAnimtorManager conversion contract changed");

            var characterData = buildMethod.Invoke(
                animatorManager,
                new object[] { datFile, Path.GetDirectoryName(datPath) }) as LF2CharacterData;
            Expect(characterData != null,
                $"production DAT fixture failed to convert: {projectRelativePath}");
            if (characterData.type_sub == 0)
                characterData.type_sub = objectId;
            return new LF2CharacterDataWrapper(objectId, characterData);
        }

        private static void CheckOid6DjaGuardComboHold()
        {
            var world = new SimulationWorld();
            LF2Character guarded = CreateCharacter("SelfCheck_Oid6_DjaGuard", 6, BuildComboWrapperCharacterData("SelfCheck_Oid6_DjaGuard", 300));
            guarded.SwitchDir("right");
            guarded.Health.HP = 200;
            world.Register(guarded);
            world.Runtime.Flow.DjaGuardGlobal44F224 = 0;

            SetComboLocalSeeds(guarded.InputState, ordinaryValue: 1, djaValue: 2);
            ((SelfCheckController)guarded.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);
            guarded.RunPostCooldownInputPhase(1);

            Expect(guarded.Frame.N == 0,
                "oid6 DjaGuard must block DJA frame jump when hit_ja=300 and guard flag is active");
            ExpectComboLocalSeeds(guarded.InputState, ordinaryValue: 0, djaValue: 3,
                "oid6 DjaGuard must commit prior wrapper mutations while preserving completed DJA state");

            LF2Character released = CreateCharacter("SelfCheck_Oid6_DjaRelease", 6, BuildComboWrapperCharacterData("SelfCheck_Oid6_DjaRelease", 300));
            released.SwitchDir("right");
            released.Health.HP = 200;
            world.Register(released);
            world.Runtime.Flow.DjaGuardGlobal44F224 = 1;

            SetComboLocalSeeds(released.InputState, ordinaryValue: 1, djaValue: 2);
            ((SelfCheckController)released.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);
            released.RunPostCooldownInputPhase(1);

            Expect(released.Frame.N == 300,
                "oid6 DJA must frame jump once DjaGuardGlobal44F224 no longer blocks it");
            ExpectComboLocalSeeds(released.InputState, ordinaryValue: 0, djaValue: 0,
                "successful oid6 DJA must commit all direct wrapper mutations and clear DJA");
        }

        private static void CheckStageWaveImmediateSpawnAndAdvance()
        {
            const int stageOid = 201;
            LF2CharacterDataWrapper stageWrapper = BuildStageSpawnWrapper(stageOid, "SelfCheck_StageImmediate");
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid => oid == stageOid ? stageWrapper : null;

                var world = new SimulationWorld();
                world.Runtime.Match.BattleGameModeId = 1;
                world.Runtime.Stage.SetSceneSnapshot(1000, 180, 350, 0, 0);
                world.StageCampaigns.Add(new BattleStageCampaignData
                {
                    Id = 9,
                    Phases = new List<BattleStagePhaseData>
                    {
                        new BattleStagePhaseData
                        {
                            Spawns = new List<BattleStageSpawnData>
                            {
                                new BattleStageSpawnData
                                {
                                    Id = stageOid,
                                    Act = 0,
                                    Hp = 321,
                                    Times = 1,
                                    X = 100,
                                    Y = -20,
                                    Ratio = 0.0,
                                },
                            },
                        },
                        new BattleStagePhaseData
                        {
                            Bound = 1400,
                        },
                    },
                });
                world.StageProgression.StageSeriesIdx = 9;
                world.StageProgression.WaveIdx = 0;
                world.SetStageProgressionValid(true);

                world.CurrentWaveStageTickAll();

                var entities = new List<LF2Entity>();
                world.GetAllEntities(entities);
                Expect(entities.Count == 1,
                    "stage immediate spawn must create exactly one entity for one immediate entry");
                LF2Entity spawned = entities[0];
                int spawnedSlot = spawned.Runtime?.SlotIndex ?? -1;
                Expect(spawned.ObjectId == stageOid && spawnedSlot >= 50,
                    "stage immediate spawn must use a dynamic runtime slot");
                Expect(spawned.Frame.N == 0 && spawned.FrameDelay == 0,
                    "stage immediate spawn must preserve configured action zero with zero frame delay");
                Expect(spawned.Health.HP == 321 && spawned.Health.HPBound == 321 && spawned.Health.HP3 == 321,
                    "stage immediate spawn must apply configured HP to HP, HPBound and HP3");
                Expect(spawned.Team == 2 && spawned.RelationTeam == 2 &&
                       spawned.Unk344 == 2 && spawned.HitStun == 20 &&
                       spawned.HolderCopySlot == spawnedSlot,
                    "stage immediate character spawn must apply team, Unk344, init and self-holder contracts");
                Expect(spawned.AiControlled,
                    "stage/opoint character spawns must be AI-controlled by default");
                Expect(world.StageSpawnWaveApplied == 0 && world.StageProgression.WaveIdx == 0,
                    "stage immediate producer must initialize once without advancing while its entity is alive");

                world.CurrentWaveStageTickAll();
                Expect(world.StageProgression.WaveIdx == 0,
                    "stage wave must not advance while a configured stage entity remains active");

                world.Unregister(spawned);
                LF2Character reservedSlotEntity = CreateCharacter(
                    "SelfCheck_StageReservedSlot",
                    stageOid,
                    stageWrapper.characterData);
                reservedSlotEntity.SetRuntimeSlotIndex(20);
                world.Register(reservedSlotEntity);
                world.CurrentWaveStageTickAll();

                Expect(world.StageProgression.WaveIdx == 1,
                    "stage wave must ignore matching non-stage entities below the Unity dynamic slot range");
                Expect(world.Runtime.Stage.XMaxOverride == 1400 &&
                       world.Runtime.Stage.CameraMaxOverride == 606 &&
                       world.Runtime.Stage.StageWidthPx == 1400,
                    "stage phase advance must apply bound and camera bound overrides");
                Expect(world.StageSpawnWaveApplied == 1 && world.StageSpawnWaveDeferredEntryApplied == 1,
                    "empty next phase must initialize both stage spawn producer markers");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckStageWaveBootstrapAndSpawnContract()
        {
            const string stageText =
                "<stage> id: 12 #self-check\n" +
                "<phase> bound: 900\n" +
                "id: 205 act: 0 hp: 275 times: 3 x: -100 y: -20 ratio: 1.5 join: 8\n" +
                "<phase_end>\n" +
                "<stage_end>\n";

            List<BattleStageCampaignData> campaigns = BattleStageCampaignLoader.ParseText(stageText);
            Expect(campaigns.Count == 1 && campaigns[0].Id == 12 && campaigns[0].Comment == "self-check",
                "stage campaign parser must load stage identity and comment");
            BattleStagePhaseData phase = campaigns[0].Phases[0];
            BattleStageSpawnData spawn = phase.Spawns[0];
            Expect(phase.Bound == 900 && spawn.Id == 205 && spawn.Act == 0 && spawn.Hp == 275 &&
                   spawn.Times == 3 && spawn.X == -100 && spawn.Y == -20 &&
                   Nearly(spawn.Ratio, 1.5) && spawn.Join == 8,
                "stage campaign parser must map all phase and spawn fields");

            string tempStagePath = Path.Combine(Application.temporaryCachePath, "ntsd_stage_campaign_self_check.dat");
            try
            {
                File.WriteAllText(tempStagePath, stageText);
                List<BattleStageCampaignData> loadedCampaigns =
                    BattleStageCampaignLoader.LoadFromFile(tempStagePath);
                Expect(loadedCampaigns.Count == 1 && loadedCampaigns[0].Id == 12,
                    "stage campaign production loader must read an explicit plaintext DAT path");
            }
            finally
            {
                if (File.Exists(tempStagePath))
                    File.Delete(tempStagePath);
            }

            var world = new SimulationWorld();
            world.ConfigureStageCampaigns(campaigns, 12, -1);
            Expect(world.StageProgressionValid && world.StageProgression.StageSeriesIdx == 12 &&
                   world.StageProgression.WaveIdx == -1,
                "stage production bootstrap must retain authority pre-wave state after data load");
            Expect(world.StartInitialStageWave() && world.StageProgression.WaveIdx == 0 &&
                   world.Runtime.Stage.XMaxOverride == 900,
                "stage production bootstrap must advance pre-wave to wave zero and apply its bound");

            OPointCreateTask task = SimulationWorld.BuildStageSpawnTask(spawn, 10, -20, 200, "right");
            Expect(task.preserveActionZero && task.opoint.action == 0,
                "stage factory task must preserve authored action zero");

            var character = new StageSpawnContractSelfCheckEntity(LF2ObjectType.Character);
            character.SetRuntimeSlotIndex(50);
            SimulationWorld.ApplyStageSpawnRuntimeContract(character, 300);
            Expect(character.Team == 2 && character.RelationTeam == 2 &&
                   character.Unk344 == 2 && character.HitStun == 20 && character.HolderCopySlot == 50,
                "stage character contract must map Unk364 to RelationTeam=2 and use character init semantics");

            var type5 = new StageSpawnContractSelfCheckEntity(LF2ObjectType.Other);
            type5.SetRuntimeSlotIndex(51);
            SimulationWorld.ApplyStageSpawnRuntimeContract(type5, 301);
            Expect(type5.RelationTeam == 2 && type5.HitStun == 20 && type5.Unk344 == 2,
                "stage DAT type 5 contract must use authority character-init semantics");

            var projectile = new StageSpawnContractSelfCheckEntity(LF2ObjectType.SpecialAttack);
            projectile.SetRuntimeSlotIndex(52);
            SimulationWorld.ApplyStageSpawnRuntimeContract(projectile, 302);
            Expect(projectile.Team == 2 && projectile.RelationTeam == 0 &&
                   projectile.HitStun == 0 && projectile.Unk344 == 2,
                "stage non-character contract must preserve Team=2 but clear RelationTeam/Unk364");
        }

        private static void CheckStageWavePositiveSpawnRefill()
        {
            const int stageOid = 202;
            LF2CharacterDataWrapper stageWrapper = BuildStageSpawnWrapper(stageOid, "SelfCheck_StagePositive");
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid => oid == stageOid ? stageWrapper : null;

                var world = new SimulationWorld();
                world.Runtime.Match.BattleGameModeId = 2;
                LF2Character factorCharacter = CreateCharacter(
                    "SelfCheck_StageFactor",
                    1,
                    BuildStageSpawnCharacterData("SelfCheck_StageFactor"));
                factorCharacter.SetRuntimeSlotIndex(0);
                world.Register(factorCharacter);
                world.StageCampaigns.Add(new BattleStageCampaignData
                {
                    Id = 10,
                    Phases = new List<BattleStagePhaseData>
                    {
                        new BattleStagePhaseData
                        {
                            Spawns = new List<BattleStageSpawnData>
                            {
                                new BattleStageSpawnData
                                {
                                    Id = stageOid,
                                    Act = 7,
                                    Hp = 250,
                                    Times = 2,
                                    X = 200,
                                    Ratio = 1.0,
                                },
                            },
                        },
                    },
                });
                world.StageProgression.StageSeriesIdx = 10;
                world.StageProgression.WaveIdx = 0;
                world.SetStageProgressionValid(true);

                world.CurrentWaveStageTickAll();

                Expect(world.StageSpawnWaveDeferredEntryApplied == 0 && world.StageSpawnRuntimeWave == 0,
                    "positive stage producer must initialize its deferred marker and runtime wave");
                Expect(world.StageSpawnRuntimeEntryCount.Count == 1 &&
                       world.StageSpawnRuntimeEntryCount[0] == 1 &&
                       world.StageSpawnRuntimeTargetTotal[0] == 2 &&
                       world.StageSpawnRuntimeSpawnedTotal[0] == 1,
                    "positive stage runtime must derive one concurrent entry and two total spawns from factor 1");
                int firstSlot = world.StageSpawnRuntimeSlots[0][0];
                LF2Entity firstSpawn = world.FindEntityByRuntimeSlotForQuery(firstSlot);
                Expect(firstSlot >= 50 && firstSpawn != null && firstSpawn.ObjectId == stageOid,
                    "positive stage producer must track its active spawned entity by dynamic runtime slot");

                world.CurrentWaveStageTickAll();
                Expect(world.StageSpawnRuntimeSpawnedTotal[0] == 1 &&
                       world.StageSpawnRuntimeSlots[0][0] == firstSlot,
                    "positive stage producer must not exceed its concurrent entry count while the slot is alive");

                world.Unregister(firstSpawn);
                world.CurrentWaveStageTickAll();

                int replacementSlot = world.StageSpawnRuntimeSlots[0][0];
                LF2Entity replacement = world.FindEntityByRuntimeSlotForQuery(replacementSlot);
                Expect(replacement != null && replacement.ObjectId == stageOid,
                    "positive stage producer must refill a cleared concurrent slot");
                Expect(world.StageSpawnRuntimeSpawnedTotal[0] == 2,
                    "positive stage producer must increment total spawned count on refill");

                world.Unregister(replacement);
                world.CurrentWaveStageTickAll();
                Expect(world.StageSpawnRuntimeSlots[0][0] == -1 &&
                       world.StageSpawnRuntimeSpawnedTotal[0] == 2,
                    "positive stage producer must stop refilling after reaching target total");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckAiTargetCacheCoordinateAndDeterminism()
        {
            LF2CharacterData data = BuildComboWrapperCharacterData("SelfCheck_AI", 180);
            SimulationWorld firstWorld = BuildAiSelfCheckWorld(data, 12345, out LF2Character firstAi, out LF2Character firstTarget);
            firstWorld.AiInputAndComboAll(2);

            Expect(firstAi.Runtime.Unk360 == firstTarget.Runtime.SlotIndex,
                "AI must cache the selected target by runtime slot");
            Expect(firstAi.Runtime.KeyRight != 0 || firstAi.Runtime.KeyLeft != 0 ||
                   firstAi.Runtime.KeyUp != 0 || firstAi.Runtime.KeyDown != 0 ||
                   firstAi.Runtime.KeyAttack != 0 || firstAi.Runtime.KeyJump != 0 || firstAi.Runtime.KeyDefend != 0 ||
                   firstAi.Runtime.ComboDra != 0 || firstAi.Runtime.ComboDla != 0 || firstAi.Runtime.ComboDua != 0 ||
                   firstAi.Runtime.ComboDda != 0 || firstAi.Runtime.ComboDrj != 0 || firstAi.Runtime.ComboDlj != 0 ||
                   firstAi.Runtime.ComboDuj != 0 || firstAi.Runtime.ComboDdj != 0 || firstAi.Runtime.ComboDja != 0,
                "AI target pass must produce movement, action, or combo intent");

            SimulationWorld secondWorld = BuildAiSelfCheckWorld(data, 12345, out LF2Character secondAi, out _);
            secondWorld.AiInputAndComboAll(2);
            Expect(AiInputSignature(firstAi.Runtime) == AiInputSignature(secondAi.Runtime),
                "AI decisions must be deterministic for the same seed and runtime-slot world state");

            SimulationWorld coordinateWorld = new SimulationWorld();
            LF2Character coordinateAi = CreateCharacter("SelfCheck_AI_Coordinate", 33, data);
            coordinateAi.SetRuntimeSlotIndex(3);
            coordinateAi.AiControlled = true;
            coordinateAi.RelationTeam = 1;
            coordinateAi.Runtime.SetPosition(100, 0, 100);
            coordinateAi.Runtime.SyncIntegerPosition();
            coordinateAi.Runtime.Unk3FC = 500;
            coordinateAi.Runtime.Unk400 = 300;
            coordinateAi.Runtime.KeyRight = 1;
            coordinateWorld.Runtime.Flow.AiRand3 = 5;
            coordinateWorld.Runtime.Match.Difficulty = 2;
            coordinateWorld.Rng.Seed(3);
            coordinateWorld.Register(coordinateAi);
            coordinateWorld.AiInputAndComboAll(2);
            Expect(coordinateAi.Runtime.KeyRight == 1 && coordinateAi.Runtime.KeyDown == 1,
                "AI coordinate mode must move toward Unk3FC/Unk400 without requiring a target");
            Expect(coordinateAi.Runtime.Unk360 == -1,
                "AI coordinate mode must not mutate the cached combat target");
            Expect(coordinateWorld.Runtime.Flow.AiRand3 == 5 && coordinateAi.Runtime.PrevRight == 0,
                "AI coordinate mode must reuse the previous world AiRand3 before normal-path globals are recomputed");
            Expect(coordinateAi.Runtime.CdRight == 5 &&
                   coordinateAi.Runtime.InputHistory[4] == 6 &&
                   coordinateAi.Runtime.InputHistory[5] == 2,
                "AI coordinate movement must apply right then down edges in authority history order");
        }

        private static void CheckAiHeldInactiveSlotContract()
        {
            LF2CharacterData data = new LF2CharacterData
            {
                name = "SelfCheck_AI_HeldInactive",
                frames = new List<LF2FrameData> { Frame(0, 2, 1, 0, 39, 79) },
            };
            SimulationWorld world = new SimulationWorld();
            world.Runtime.Match.Difficulty = 0;
            world.Rng.Seed(3);
            LF2Character ai = CreateCharacter("SelfCheck_AI_HeldInactive_Source", 40, data);
            LF2Character target = CreateCharacter("SelfCheck_AI_HeldInactive_Target", 41, data);
            ai.SetRuntimeSlotIndex(0);
            target.SetRuntimeSlotIndex(1);
            ai.AiControlled = true;
            ai.RelationTeam = 1;
            target.RelationTeam = 2;
            ai.Runtime.LinkState = 1;
            ai.Runtime.TargetSlotIndex = 5;
            ai.Runtime.SetPosition(100, 0, 200);
            target.Runtime.SetPosition(140, 0, 200);
            ai.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            world.Register(ai);
            world.Register(target);
            world.AiInputAndComboAll(2);
            int nextRng = world.Rng.NextRaw();
            Expect(AiInputSignature(ai.Runtime) == "1:0100000:0000000:000000000",
                "a valid but inactive held slot must continue through the authority self-state branch before returning");
            Expect(nextRng == 12168,
                "a valid but inactive held slot must preserve the authority RNG consumption count");
        }

        private static void CheckAiSharedCharacterDatShell()
        {
            LF2CharacterData data = new LF2CharacterData
            {
                name = "SelfCheck_AI_SharedShell",
                frames = new List<LF2FrameData> { Frame(0, 3, 1, 0, 39, 79) },
            };
            SimulationWorld world = new SimulationWorld();
            var shell = new SelfCheckCharacterDatShell();
            shell.ObjectId = 40;
            shell.FrameCache.Load(new LF2CharacterDataWrapper(40, data));
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Frame.N = 0;
            shell.Runtime.HP = 500;
            shell.SetRuntimeSlotIndex(4);
            shell.AiControlled = true;
            shell.RelationTeam = 1;
            shell.Runtime.Unk3FC = 400;
            shell.Runtime.Unk400 = 200;
            shell.Runtime.SetPosition(100, 0, 100);
            shell.Runtime.SyncIntegerPosition();
            world.Register(shell);
            world.AiInputAndComboAll(2);
            Expect(shell.Runtime.KeyRight == 1 && shell.Runtime.KeyDown == 1,
                "current character-DAT entities must run AI even when their CLR shell is not LF2Character");
        }

        private static void CheckAiHumanInputIsolation()
        {
            LF2CharacterData data = BuildComboWrapperCharacterData("SelfCheck_AI_Human", 180);
            SimulationWorld world = new SimulationWorld();
            LF2Character human = CreateCharacter("SelfCheck_HumanIsolation", 1, data);
            human.SetRuntimeSlotIndex(0);
            human.AiControlled = false;
            ((SelfCheckController)human.Controller).InputBuffer.EnqueueForTick(2, FuncKeyMask.right, true);
            world.Register(human);

            world.PostCooldownHumanInputAll(2);
            byte humanRight = human.Runtime.KeyRight;
            world.AiInputAndComboAll(2);

            Expect(humanRight == 1 && human.Runtime.KeyRight == 1,
                "human input must be consumed before M1 and remain untouched by the AI pass");
            Expect(human.Runtime.Unk360 == -1,
                "human-controlled characters must not run AI target selection");
        }

        private static SimulationWorld BuildAiSelfCheckWorld(
            LF2CharacterData data,
            int seed,
            out LF2Character ai,
            out LF2Character target)
        {
            SimulationWorld world = new SimulationWorld();
            world.Rng.Seed(seed);
            world.Runtime.Match.Difficulty = 2;
            ai = CreateCharacter("SelfCheck_AI_Source", 33, data);
            target = CreateCharacter("SelfCheck_AI_Target", 4, data);
            ai.SetRuntimeSlotIndex(0);
            target.SetRuntimeSlotIndex(1);
            ai.AiControlled = true;
            target.AiControlled = false;
            ai.RelationTeam = 1;
            target.RelationTeam = 2;
            ai.Runtime.SetPosition(100, 0, 200);
            target.Runtime.SetPosition(260, 0, 210);
            ai.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            world.Register(ai);
            world.Register(target);
            return world;
        }

        private static string AiInputSignature(NTSDEntityRuntime r)
        {
            return $"{r.Unk360}:{r.KeyRight}{r.KeyLeft}{r.KeyUp}{r.KeyDown}{r.KeyAttack}{r.KeyJump}{r.KeyDefend}:" +
                   $"{r.PrevRight}{r.PrevLeft}{r.PrevUp}{r.PrevDown}{r.PrevAttack}{r.PrevJump}{r.PrevDefend}:" +
                   $"{r.ComboDra}{r.ComboDla}{r.ComboDua}{r.ComboDda}{r.ComboDrj}{r.ComboDlj}{r.ComboDuj}{r.ComboDdj}{r.ComboDja}";
        }

        private static LF2CharacterDataWrapper BuildStageSpawnWrapper(int objectId, string name)
        {
            return new LF2CharacterDataWrapper(objectId, BuildStageSpawnCharacterData(name));
        }

        private static LF2CharacterData BuildStageSpawnCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(7, LF2States.Standing, 1, 7, 39, 79),
                },
            };
        }

        private static Dictionary<int, LF2CharacterDataWrapper> BuildOid5152Wrappers()
        {
            return new Dictionary<int, LF2CharacterDataWrapper>
            {
                [7] = new LF2CharacterDataWrapper(7, BuildOid5152BaseData("SelfCheck_Oid7")),
                [8] = new LF2CharacterDataWrapper(8, BuildOid5152BaseData("SelfCheck_Oid8")),
                [51] = new LF2CharacterDataWrapper(51, BuildOid5152MergedData()),
            };
        }

        private static LF2CharacterData BuildOid5152BaseData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(10, 2, 1, 10, 39, 79),
                    Frame(112, 0, 1, 112, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildOid5152MergedData()
        {
            LF2FrameData frame290 = Frame(290, 2, 1, 290, 39, 79);
            frame290.hit_ja = 300;

            return new LF2CharacterData
            {
                name = "SelfCheck_Oid51",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(112, 0, 1, 112, 39, 79),
                    frame290,
                },
            };
        }

        private static LF2CharacterData BuildRespawnCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(14, 14, 1, 14, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                    Frame(0xDB, 0, 1, 0xDB, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildKind1516CharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(10, 0, 1, 10, 39, 79),
                    Frame(LF2StandardFrames.MpDrain, 18, 1, LF2StandardFrames.MpDrain, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildDeathBounceCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(5, LF2States.Lying, 1, 5, 39, 79),
                    Frame(14, LF2States.Lying, 1, 14, 39, 79),
                    Frame(186, LF2States.Lying, 1, 186, 39, 79),
                    Frame(212, LF2States.Lying, 1, 212, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildComboWrapperCharacterData(string name, int djaTargetFrame)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        frameName = "self_check_combo_root",
                        state = 0,
                        wait = 1,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                        hit_Fa = 100,
                        hit_Ua = 101,
                        hit_Da = 102,
                        hit_Fj = 103,
                        hit_Uj = 104,
                        hit_Dj = 105,
                        hit_ja = djaTargetFrame,
                    },
                    Frame(100, 0, 1, 100, 39, 79),
                    Frame(101, 0, 1, 101, 39, 79),
                    Frame(102, 0, 1, 102, 39, 79),
                    Frame(103, 0, 1, 103, 39, 79),
                    Frame(104, 0, 1, 104, 39, 79),
                    Frame(105, 0, 1, 105, 39, 79),
                    Frame(180, 0, 1, 180, 39, 79),
                    Frame(300, 0, 1, 300, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildComboWrapperData(string name, int djaTargetFrame)
        {
            return BuildComboWrapperCharacterData(name, djaTargetFrame);
        }

        private static void AssertComboFrameJump(
            string name,
            int objectId,
            LF2CharacterData data,
            FuncKeyMask[] sequence,
            int expectedFrame,
            string expectedDir,
            bool verifyCooldownClear = false)
        {
            LF2Character character = CreateCharacter(name, objectId, data);
            character.SwitchDir("right");

            SelfCheckController controller = (SelfCheckController)character.Controller;
            for (int i = 0; i < sequence.Length; i++)
                controller.InputBuffer.EnqueueForTick(1, sequence[i], true);
            character.RunPostCooldownInputPhase(1);

            Expect(character.Frame.N == expectedFrame,
                $"{name} should jump to frame {expectedFrame} after combo wrapper input");
            Expect(character.Runtime.Dir == expectedDir,
                $"{name} should face {expectedDir} after combo wrapper input");

            if (verifyCooldownClear)
            {
                Expect(character.Runtime.CdRight == 0 &&
                       character.Runtime.CdLeft == 0 &&
                       character.Runtime.CdUp == 0 &&
                       character.Runtime.CdDown == 0 &&
                       character.Runtime.CdAttack == 0 &&
                       character.Runtime.CdJump == 0 &&
                       character.Runtime.CdDefend == 0,
                    $"{name} should clear action and direction cooldowns after successful combo frame jump");
            }
        }

        private static LF2CharacterData BuildRespawnEffectData()
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_RespawnEffect998",
                frames = new List<LF2FrameData>
                {
                    Frame(6, 9998, 1, 1000, 39, 79),
                },
            };
        }

        private static SimulationWorld CreateOid5152MergedWorld(
            Dictionary<int, LF2CharacterDataWrapper> wrappers,
            out LF2Character self,
            out LF2Character partner)
        {
            var world = new SimulationWorld();
            self = CreateCharacter("SelfCheck_Oid7_Merged", 7, wrappers[7].characterData);
            partner = CreateCharacter("SelfCheck_Oid8_Merged", 8, wrappers[8].characterData);
            self.SetRuntimeSlotIndex(0);
            partner.SetRuntimeSlotIndex(11);
            world.Register(self);
            world.Register(partner);

            self.ImmediateFrame(10);
            partner.ImmediateFrame(10);
            self.RelationTeam = 3;
            partner.RelationTeam = 3;
            self.Health.HP = 100;
            self.Health.HPBound = 100;
            self.Health.HP3 = 200;
            partner.Health.HP = 100;
            partner.Health.HPBound = 100;
            partner.ItrRest.Arest = 6;
            partner.ItrRest.SetVrest(0, 8);
            partner.ItrRest.SetVrest(19, 11);
            self.Runtime.SetPosition(90f, 0f, 6f);
            partner.Runtime.SetPosition(120f, 0f, 9f);
            self.Runtime.SyncIntegerPosition();
            partner.Runtime.SyncIntegerPosition();
            self.Trans.SyncDirectFrameData(self.Frame.D.wait, self.Frame.D.next, 37);

            world.Oid5152RuntimeMaintenanceAll(1);
            return world;
        }

        private static object GetPrivateField(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(instance);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(instance, value);
        }

        private static LF2Entity CreateCpointMatrixEntity(
            bool realCharacter,
            string name,
            int objectId,
            LF2CharacterData data)
        {
            if (realCharacter)
                return CreateCharacter(name, objectId, data);

            var shell = new SelfCheckCharacterDatShell();
            shell.InitializeForCpoint();
            shell.Name = name;
            shell.ObjectId = objectId;
            shell.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            shell.Frame.N = 0;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Frame.PN = 0;
            shell.Frame.Prev2 = 0;
            shell.Frame.Prev2D = shell.Frame.D;
            shell.Runtime.HP = 500;
            shell.Runtime.HPBound = 500;
            shell.Runtime.PP = 500;
            shell.SetRuntimeSlotIndex(shell.StableId);
            shell.RefreshRuntimeSnapshot();
            return shell;
        }

        private static void LinkCpointEntities(LF2Entity catcher, LF2Entity victim)
        {
            catcher.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = catcher.Runtime.SlotIndex;
            catcher.FrameDelay = 0;
            victim.FrameDelay = 0;
            catcher.Runtime.CaughtDuration = 300;
        }

        private static LF2CharacterData BuildCpointMatrixVictimFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_CpointMatrixVictim",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 10, 2, 0, 30, 60, new CatchPoint
                    {
                        kind = 2, x = 3, y = 4, hurtable = 1
                    }),
                    Frame(130, 10, 3, 130, 35, 70, new CatchPoint
                    {
                        kind = 2, x = 8, y = 12, hurtable = 1
                    }),
                    Frame(131, 10, 4, 131, 34, 69, new CatchPoint
                    {
                        kind = 2, x = 9, y = 13, hurtable = 1
                    }),
                    Frame(132, 10, 5, 132, 33, 68, new CatchPoint
                    {
                        kind = 2, x = 6, y = 10, hurtable = 1
                    }),
                    Frame(181, 11, 1, 181, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildCpointThrowFrames(int nextFrame, int victimAction, int throwInjury)
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_CpointThrow",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(110, 9, 1, nextFrame, 40, 80, new CatchPoint
                    {
                        kind = 1,
                        x = 16,
                        y = 24,
                        vaction = victimAction,
                        throwvx = 8,
                        throwvy = -4,
                        throwvz = 3,
                        throwinjury = throwInjury,
                        cover = 0,
                        hurtable = 1,
                    }),
                    Frame(112, 0, 1, 112, 39, 79),
                },
            };
        }

        private static LF2Character CreateCharacter(string name, int objectId, LF2CharacterData data)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new SelfCheckController();
            // 自检只验证纯战斗逻辑，不注册到 SimulationWorld，避免批处理验证污染场景运行时。
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRuntimeSlotIndex(character.StableId);
            return character;
        }

        private static LF2CharacterData BuildCatchingFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheckCatcher",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                    Frame(100, 9, 1, 100, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, aaction = 120, taction = 121, cover = 10, hurtable = 1
                    }),
                    Frame(110, 9, 1, 112, 40, 80, new CatchPoint
                    {
                        kind = 1, x = 16, y = 24, vaction = 132, throwvx = 8, throwvy = -4, throwvz = 3,
                        throwinjury = 25, cover = 10, hurtable = 1
                    }),
                    Frame(112, 0, 0, 0, 39, 79),
                    Frame(120, 9, 1, 120, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, cover = 10, hurtable = 1
                    }),
                    Frame(121, 9, 1, 121, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, cover = 10, hurtable = 1
                    }),
                    Frame(140, 9, 1, 140, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, decrease = -5, cover = 10, hurtable = 1
                    }),
                    Frame(150, 9, 1, 150, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, dircontrol = 1, cover = 10, hurtable = 1
                    }),
                    Frame(160, 9, 1, 160, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, jaction = 120, cover = 10, hurtable = 1
                    }),
                }
            };
        }

        private static LF2CharacterData BuildVictimFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheckVictim",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(130, 10, 99, 130, 35, 70, new CatchPoint
                    {
                        kind = 2, x = 8, y = 12, hurtable = 1
                    }),
                    Frame(131, 10, 99, 131, 34, 69, new CatchPoint
                    {
                        kind = 2, x = 9, y = 13, hurtable = 1
                    }),
                    Frame(132, 10, 99, 132, 33, 68, new CatchPoint
                    {
                        kind = 2, x = 6, y = 10, hurtable = 1
                    }),
                    Frame(181, 11, 1, 181, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                }
            };
        }

        private static LF2FrameData Frame(
            int id,
            int state,
            int wait,
            int next,
            int centerx,
            int centery,
            CatchPoint cpoint = null)
        {
            return new LF2FrameData
            {
                frameId = id,
                frameName = $"self_check_{id}",
                state = state,
                wait = wait,
                next = next,
                centerx = centerx,
                centery = centery,
                cpoint = cpoint
            };
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static bool Nearly(float actual, float expected)
        {
            return Mathf.Abs(actual - expected) <= 0.001f;
        }

        private static bool Nearly(double actual, double expected)
        {
            return System.Math.Abs(actual - expected) <= 0.001;
        }

        private sealed class SelfCheckController : ILF2Controller
        {
            public bool Up { get; set; }
            public bool Down { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool Attack { get; set; }
            public bool Jump { get; set; }
            public bool Defend { get; set; }

            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();

            bool ILF2Controller.IsUp => Up;
            bool ILF2Controller.IsDown => Down;
            bool ILF2Controller.IsLeft => Left;
            bool ILF2Controller.IsRight => Right;
            bool ILF2Controller.IsAttack => Attack;
            bool ILF2Controller.IsJump => Jump;
            bool ILF2Controller.IsDefend => Defend;

            public int Dirv()
            {
                if (Up && !Down) return -1;
                if (Down && !Up) return 1;
                return 0;
            }

            public (int dx, int dz) GetMoveInput()
            {
                int dx = Right == Left ? 0 : Right ? 1 : -1;
                int dz = Down == Up ? 0 : Down ? 1 : -1;
                return (dx, dz);
            }

            public void SetInputID(int inputId)
            {
            }
        }

        private sealed class InteractionSelfCheckCharacter : LF2Character
        {
            public override float GetSpriteWidthPxForCollision() => 100f;
        }

        private sealed class PhaseRoutingSelfCheckCharacter : LF2Character
        {
            public int PostInteractionCount { get; private set; }
            public int ObjectInteractionCount { get; private set; }

            public void BindSource(string name, int objectId, int targetOid)
            {
                ModuleInitialize();
                Controller = new SelfCheckController();
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, new LF2CharacterData
                {
                    name = name,
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, 4000 + targetOid, 10, 0, 39, 79),
                    },
                }));
                Frame.N = 0;
                Frame.PN = 0;
                Frame.D = FrameCache.GetFrameDataById(0);
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, 0);
                Initialize(500, 500);
                FrameDelay = 0;
            }

            public override void SimPostInteraction(int tickIndex)
            {
                PostInteractionCount++;
                base.SimPostInteraction(tickIndex);
            }

            public override void SimObjectInteraction(int tickIndex)
            {
                ObjectInteractionCount++;
                base.SimObjectInteraction(tickIndex);
            }
        }

        private sealed class PhaseRoutingSelfCheckSpecialAttack : LF2SpecialAttack
        {
            public int PostInteractionCount { get; private set; }
            public int ObjectInteractionCount { get; private set; }

            public void BindSource(string name, int objectId, int targetOid)
            {
                Name = name;
                ObjectId = objectId;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, new LF2CharacterData
                {
                    name = name,
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, 4000 + targetOid, 10, 0, 39, 79),
                    },
                }));
                Frame.N = 0;
                Frame.PN = 0;
                Frame.D = FrameCache.GetFrameDataById(0);
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, 0);
                Health.HP = 100;
                Health.HPBound = 100;
            }

            public override void SimPostInteraction(int tickIndex)
            {
                PostInteractionCount++;
                base.SimPostInteraction(tickIndex);
            }

            public override void SimObjectInteraction(int tickIndex)
            {
                ObjectInteractionCount++;
                base.SimObjectInteraction(tickIndex);
            }
        }

        private sealed class FlowSelfCheckEntity : LF2Entity
        {
            private readonly LF2ObjectType objectType;

            public override LF2ObjectType ObjectTypeEnum => objectType;

            public FlowSelfCheckEntity(LF2ObjectType objectType)
            {
                this.objectType = objectType;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 500;
                Health.HPBound = 500;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)objectType;

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class FrameAdvanceInputProbeEntity : LF2Entity
        {
            public int TransitCount { get; private set; }
            public int TuCount { get; private set; }
            public bool KeysClearedBeforeTransit { get; private set; }
            public bool PreviousKeysPreservedBeforeTransit { get; private set; }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public FrameAdvanceInputProbeEntity()
            {
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public override void SimTransit(int tickIndex)
            {
                TransitCount++;
                KeysClearedBeforeTransit = Runtime.KeyUp == 0 && Runtime.KeyDown == 0 &&
                    Runtime.KeyLeft == 0 && Runtime.KeyRight == 0 &&
                    Runtime.KeyAttack == 0 && Runtime.KeyJump == 0 && Runtime.KeyDefend == 0;
                PreviousKeysPreservedBeforeTransit = Runtime.PrevUp == 1 && Runtime.PrevDown == 1 &&
                    Runtime.PrevLeft == 1 && Runtime.PrevRight == 1 &&
                    Runtime.PrevAttack == 1 && Runtime.PrevJump == 1 && Runtime.PrevDefend == 1;
            }

            public override void SimTU(int tickIndex)
            {
                TuCount++;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class BoundsSelfCheckCharacter : LF2Character
        {
            private readonly LF2ObjectType currentDataType;

            public BoundsSelfCheckCharacter(LF2ObjectType currentDataType)
            {
                this.currentDataType = currentDataType;
                ModuleInitialize();
                Controller = new SelfCheckController();
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Initialize(500, 500);
                FrameDelay = 0;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)currentDataType;
        }

        private sealed class CandidatePassProbeCharacter : LF2Character
        {
            public LF2Entity ExpectedCandidateTarget { get; set; }
            public bool PostInteractionObserved { get; private set; }
            public bool CandidateContainsExpectedTarget { get; private set; }
            public int ObservedFrame { get; private set; }
            public int ObservedCaughtSlot { get; private set; }
            public double ObservedX { get; private set; }
            public double ObservedY { get; private set; }
            public double ObservedZ { get; private set; }

            public CandidatePassProbeCharacter()
            {
                ModuleInitialize();
                Controller = new SelfCheckController();
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.N = 0;
                Frame.PN = 0;
                Frame.D = FrameCache.GetFrameDataById(0);
                Initialize(500, 500);
                FrameDelay = 0;
            }

            public override void SimPostInteraction(int tickIndex)
            {
                PostInteractionObserved = true;
                ObservedFrame = Frame?.N ?? -1;
                ObservedCaughtSlot = CatcherSlotIndex;
                ObservedX = Runtime.X;
                ObservedY = Runtime.Y;
                ObservedZ = Runtime.Z;

                if (Match?.SceneQuery is BruteForceSceneQuery query &&
                    query.TryGetCollisionCandidateSequence(this, out List<SceneQueryHit> candidates))
                {
                    CandidateContainsExpectedTarget = candidates.Exists(
                        hit => hit.Target == ExpectedCandidateTarget);
                }

                base.SimPostInteraction(tickIndex);
            }
        }

        private sealed class HeldStep12SelfCheckEntity : LF2Entity
        {
            private readonly LF2ObjectType currentDataType;

            public HeldStep12SelfCheckEntity(LF2ObjectType currentDataType)
            {
                this.currentDataType = currentDataType;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public int ImmediateFrameCallCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum => currentDataType;
            public override int GetCurrentDataObjectTypeForSimulation() => (int)currentDataType;

            public void BindData(string name, int objectId, int damagedFrame)
            {
                Name = name;
                ObjectId = objectId;
                var frames = new List<LF2FrameData>(41);
                for (int frameId = 0; frameId <= 40; frameId++)
                {
                    int state = frameId == damagedFrame ? LF2States.Falling : LF2States.Standing;
                    frames.Add(Frame(frameId, state, 100, frameId, 0, 0));
                }

                FrameCache.Load(new LF2CharacterDataWrapper(objectId, new LF2CharacterData
                {
                    name = name,
                    frames = frames,
                }));
                Frame.N = 0;
                Frame.PN = 0;
                Frame.D = FrameCache.GetFrameDataById(0);
                Health.HP = 500;
                Health.HPBound = 500;
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.N = 0;
                Frame.PN = 0;
                Frame.D = FrameCache.GetFrameDataById(0);
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 500;
                Health.HPBound = 500;
            }

            public override void ImmediateFrame(int frameId)
            {
                ImmediateFrameCallCount++;
                base.ImmediateFrame(frameId);
            }

            public void AttachRenderer(LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }

            public override void Reset() { }
            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class HeldActSelfCheckWeapon : LF2Weapon
        {
            public int ProcessAttackCallCount { get; private set; }
            public int ImmediateFrameCallCount { get; private set; }

            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(
                string name,
                int objectId,
                int weaponType,
                LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                SetWeaponType(weaponType);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 500;
                Health.HPBound = 500;
            }

            protected override WeaponAttackResult ProcessAttack(
                LF2Entity holder,
                WeaponPoint wpoint,
                LF2FrameData frame)
            {
                ProcessAttackCallCount++;
                return new WeaponAttackResult();
            }

            public override void ImmediateFrame(int frameId)
            {
                ImmediateFrameCallCount++;
                base.ImmediateFrame(frameId);
            }

            public void AttachRenderer(LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }
        }

        private sealed class AlternateDamageSelfCheckWeapon : LF2Weapon
        {
            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(
                string name,
                int objectId,
                int weaponType,
                LF2CharacterData data,
                int frameId)
            {
                Name = name;
                ObjectId = objectId;
                SetWeaponType(weaponType);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(frameId);
                Frame.PN = frameId;
                Frame.N = frameId;
                Runtime.Frame = frameId;
                Runtime.PrevFrame2 = frameId;
                Health.HP = 500;
                Health.HPBound = 500;
            }
        }

        private sealed class StageSpawnContractSelfCheckEntity : LF2Entity
        {
            private readonly LF2ObjectType objectType;

            public override LF2ObjectType ObjectTypeEnum => objectType;

            public StageSpawnContractSelfCheckEntity(LF2ObjectType objectType)
            {
                this.objectType = objectType;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class AlternateDamageSelfCheckSpecialAttack : LF2SpecialAttack
        {
            public override float GetSpriteWidthPxForCollision() => 100f;

            public void InitializeForRuntimeSlotContract(int stableId)
            {
                StableId = stableId;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 100;
                Health.HPBound = 100;
            }
        }

        private sealed class Audit4SelfCheckSpecialAttack : LF2SpecialAttack
        {
            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 100;
                Health.HPBound = 100;
            }
        }

        private sealed class AlternateDamageSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;
            public override float GetSpriteWidthPxForCollision() => 100f;

            public AlternateDamageSelfCheckEntity()
            {
                Name = "SelfCheck_AlternateDamageSharedVictim";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
            }

            public void BindData(int objectId, LF2CharacterData data)
            {
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class RespawnSelfCheckEffectEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            public override float GetSpriteWidthPxForCollision() => 100f;

            public RespawnSelfCheckEffectEntity()
            {
                Name = "SelfCheck_RespawnEffect998";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public void BindData(int objectId, LF2CharacterData data)
            {
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(6);
                Frame.PN = 6;
                Frame.N = 6;
                Runtime.Frame = 6;
                Runtime.PrevFrame2 = 6;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class SerialOrderSelfCheckEntity : LF2Entity
        {
            private readonly string label;
            private readonly List<string> events;

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public SerialOrderSelfCheckEntity(string label, List<string> events)
            {
                this.label = label;
                this.events = events;
                Name = $"SelfCheck_SerialOrder_{label}";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public override void SimTransit(int tickIndex)
            {
                events.Add($"{label}:transit");
            }

            public override void SimTU(int tickIndex)
            {
                events.Add($"{label}:tu");
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private class TransformedLandingSelfCheckEntity : LF2OtherObject
        {
            public void BindSource(LF2CharacterData data)
            {
                Name = data.name;
                ObjectId = 740;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 500;
                Health.HPBound = 500;
            }
        }

        private sealed class TransformingSimOrderSelfCheckEntity : TransformedLandingSelfCheckEntity
        {
            private readonly int targetOid;

            public int TransitDestroyCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum =>
                ObjectId == targetOid ? LF2ObjectType.LightWeapon : LF2ObjectType.Other;

            public TransformingSimOrderSelfCheckEntity(int targetOid)
            {
                this.targetOid = targetOid;
            }

            public override void OnTransitDestroy()
            {
                TransitDestroyCount++;
                UnregisterFromWorld();
            }
        }

        private sealed class TemporaryRuntimeObjectConfigs : IDisposable
        {
            private readonly GameDataManager dataManager;
            private readonly CharacterAnimtorManager animatorManager;
            private readonly System.Reflection.FieldInfo objectLookupField;
            private readonly System.Reflection.FieldInfo cachedConfigField;
            private readonly System.Reflection.FieldInfo frameConfigField;
            private readonly Dictionary<int, ObjectDefinition> originalObjectLookup;
            private readonly GameDataConfig originalCachedConfig;
            private readonly Dictionary<int, LF2CharacterDataWrapper> originalFrameConfigs;
            private readonly Dictionary<int, ObjectDefinition> replacedDefinitions = new Dictionary<int, ObjectDefinition>();
            private readonly Dictionary<int, LF2CharacterDataWrapper> replacedWrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            private readonly HashSet<int> addedDefinitions = new HashSet<int>();
            private readonly HashSet<int> addedWrappers = new HashSet<int>();

            public TemporaryRuntimeObjectConfigs(
                Dictionary<int, int> objectTypes,
                Dictionary<int, LF2CharacterDataWrapper> wrappers)
            {
                dataManager = GameDataManager.Instance;
                animatorManager = CharacterAnimtorManager.Instance;
                Expect(dataManager != null && animatorManager != null,
                    "runtime config fixture requires GameDataManager and CharacterAnimtorManager singletons");

                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                objectLookupField = typeof(GameDataManager).GetField("objectLookup", flags);
                cachedConfigField = typeof(GameDataManager).GetField("cachedConfig", flags);
                frameConfigField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", flags);
                Expect(objectLookupField != null && cachedConfigField != null && frameConfigField != null,
                    "runtime config fixture reflection contract changed");

                originalObjectLookup = objectLookupField.GetValue(dataManager) as Dictionary<int, ObjectDefinition>;
                originalCachedConfig = cachedConfigField.GetValue(dataManager) as GameDataConfig;
                originalFrameConfigs = frameConfigField.GetValue(animatorManager) as Dictionary<int, LF2CharacterDataWrapper>;

                Dictionary<int, ObjectDefinition> objectLookup = originalObjectLookup ?? new Dictionary<int, ObjectDefinition>();
                Dictionary<int, LF2CharacterDataWrapper> frameConfigs =
                    originalFrameConfigs ?? new Dictionary<int, LF2CharacterDataWrapper>();
                if (originalObjectLookup == null)
                    objectLookupField.SetValue(dataManager, objectLookup);
                if (originalCachedConfig == null)
                    cachedConfigField.SetValue(dataManager, new GameDataConfig());
                if (originalFrameConfigs == null)
                    frameConfigField.SetValue(animatorManager, frameConfigs);

                foreach (KeyValuePair<int, int> pair in objectTypes)
                {
                    if (objectLookup.TryGetValue(pair.Key, out ObjectDefinition existing))
                        replacedDefinitions[pair.Key] = existing;
                    else
                        addedDefinitions.Add(pair.Key);
                    objectLookup[pair.Key] = new ObjectDefinition(pair.Key, pair.Value, "self-check.dat");
                }

                foreach (KeyValuePair<int, LF2CharacterDataWrapper> pair in wrappers)
                {
                    if (frameConfigs.TryGetValue(pair.Key, out LF2CharacterDataWrapper existing))
                        replacedWrappers[pair.Key] = existing;
                    else
                        addedWrappers.Add(pair.Key);
                    frameConfigs[pair.Key] = pair.Value;
                }
            }

            public void Dispose()
            {
                if (originalObjectLookup == null)
                {
                    objectLookupField.SetValue(dataManager, null);
                }
                else
                {
                    foreach (int oid in addedDefinitions)
                        originalObjectLookup.Remove(oid);
                    foreach (KeyValuePair<int, ObjectDefinition> pair in replacedDefinitions)
                        originalObjectLookup[pair.Key] = pair.Value;
                }
                cachedConfigField.SetValue(dataManager, originalCachedConfig);

                if (originalFrameConfigs == null)
                {
                    frameConfigField.SetValue(animatorManager, null);
                }
                else
                {
                    foreach (int oid in addedWrappers)
                        originalFrameConfigs.Remove(oid);
                    foreach (KeyValuePair<int, LF2CharacterDataWrapper> pair in replacedWrappers)
                        originalFrameConfigs[pair.Key] = pair.Value;
                }
            }
        }

        private sealed class TemporaryCharacterSpriteConfig : IDisposable
        {
            private readonly Dictionary<int, List<Sprite>> spritesByObjectId;
            private readonly int objectId;
            private readonly bool hadOriginal;
            private readonly List<Sprite> originalSprites;
            private readonly Sprite temporarySprite;

            public Sprite Sprite => temporarySprite;

            public TemporaryCharacterSpriteConfig(
                CharacterAnimtorManager animatorManager,
                int objectId,
                int spriteCount)
            {
                this.objectId = objectId;
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                System.Reflection.FieldInfo spritesField = typeof(CharacterAnimtorManager).GetField(
                    "MergedSprites",
                    flags);
                Expect(spritesField != null,
                    "production sprite fixture CharacterAnimtorManager.MergedSprites contract changed");

                spritesByObjectId = spritesField.GetValue(animatorManager) as Dictionary<int, List<Sprite>>;
                Expect(spritesByObjectId != null,
                    "production sprite fixture requires an initialized sprite dictionary");
                hadOriginal = spritesByObjectId.TryGetValue(objectId, out originalSprites);

                temporarySprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0f),
                    100f);
                var sprites = new List<Sprite>(spriteCount);
                for (int i = 0; i < spriteCount; i++)
                    sprites.Add(temporarySprite);
                spritesByObjectId[objectId] = sprites;
            }

            public void Dispose()
            {
                if (hadOriginal)
                    spritesByObjectId[objectId] = originalSprites;
                else
                    spritesByObjectId.Remove(objectId);

                if (temporarySprite != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(temporarySprite);
                    else
                        UnityEngine.Object.DestroyImmediate(temporarySprite);
                }
            }
        }

        private sealed class TemporarySimulationDriverWorld : IDisposable
        {
            private readonly SimulationTickDriver driver;
            private readonly GameObject temporaryDriverObject;
            private readonly System.Reflection.FieldInfo instanceField;
            private readonly SimulationTickDriver originalDriverInstance;
            private readonly System.Reflection.FieldInfo worldField;
            private readonly SimulationWorld originalWorld;
            private readonly SimulationWorld temporaryWorld;
            private bool worldWasReplaced;

            public TemporarySimulationDriverWorld(SimulationWorld world)
            {
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Static |
                            System.Reflection.BindingFlags.NonPublic;
                Type singletonBaseType = typeof(SimulationTickDriver).BaseType;
                instanceField = singletonBaseType?.GetField("<Instance>k__BackingField", flags);
                originalDriverInstance = instanceField?.GetValue(null) as SimulationTickDriver;
                SimulationTickDriver resolvedDriver = SimulationTickDriver.Instance;
                if (resolvedDriver == null)
                {
                    resolvedDriver = FindSceneComponent<SimulationTickDriver>();
                    if (resolvedDriver != null)
                    {
                        instanceField?.SetValue(null, resolvedDriver);
                    }
                    else
                    {
                        temporaryDriverObject = new GameObject("SelfCheck_TemporarySimulationTickDriver");
                        resolvedDriver = temporaryDriverObject.AddComponent<SimulationTickDriver>();
                        if (SimulationTickDriver.Instance == null)
                            instanceField?.SetValue(null, resolvedDriver);
                    }
                }
                driver = resolvedDriver;
                Expect(driver != null && SimulationTickDriver.Instance == driver,
                    "real opoint fixture failed to create its temporary SimulationTickDriver singleton");
                worldField = typeof(SimulationTickDriver).GetField(
                    "_world",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Expect(worldField != null, "real opoint fixture SimulationTickDriver._world contract changed");
                originalWorld = worldField.GetValue(driver) as SimulationWorld;
                temporaryWorld = world;
                worldField.SetValue(driver, temporaryWorld);
                worldWasReplaced = true;
            }

            public void Dispose()
            {
                try
                {
                    var spawned = new List<LF2Entity>();
                    for (int slot = 0; slot < 400; slot++)
                    {
                        LF2Entity entity = temporaryWorld.FindEntityByRuntimeSlotIncludingPending(slot);
                        if (entity != null && entity.ObjectId == 999 && entity.Renderer != null)
                            spawned.Add(entity);
                    }

                    for (int i = 0; i < spawned.Count; i++)
                        spawned[i].FreeEntityLikeExe();
                }
                finally
                {
                    if (worldWasReplaced)
                    {
                        try
                        {
                            worldField.SetValue(driver, originalWorld);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[BattleRuntimeSelfCheck] Failed to restore SimulationTickDriver._world: {ex.Message}");
                        }
                    }

                    try
                    {
                        instanceField?.SetValue(null, originalDriverInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[BattleRuntimeSelfCheck] Failed to restore SimulationTickDriver.Instance: {ex.Message}");
                    }

                    if (temporaryDriverObject != null)
                    {
                        try
                        {
                            DestroySelfCheckObject(temporaryDriverObject);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[BattleRuntimeSelfCheck] Failed to destroy its temporary SimulationTickDriver: {ex.Message}");
                        }
                    }
                }
            }
        }

        private sealed class TemporarySingletonSceneObjectScope : IDisposable
        {
            private readonly HashSet<GameObject> originalObjects = new HashSet<GameObject>();

            public TemporarySingletonSceneObjectScope()
            {
                CaptureSceneObjects<GameDataManager>(originalObjects);
                CaptureSceneObjects<CharacterAnimtorManager>(originalObjects);
                CaptureSceneObjects<LF2ObjectPointFactory>(originalObjects);
                CaptureSceneObjects<LF2ReferencePool>(originalObjects);
                CaptureSceneObjects<LF2ObjectPool>(originalObjects);
                CaptureSceneObjects<SimulationTickDriver>(originalObjects);
            }

            public void Dispose()
            {
                var currentObjects = new HashSet<GameObject>();
                CaptureSceneObjects<GameDataManager>(currentObjects);
                CaptureSceneObjects<CharacterAnimtorManager>(currentObjects);
                CaptureSceneObjects<LF2ObjectPointFactory>(currentObjects);
                CaptureSceneObjects<LF2ReferencePool>(currentObjects);
                CaptureSceneObjects<LF2ObjectPool>(currentObjects);
                CaptureSceneObjects<SimulationTickDriver>(currentObjects);

                foreach (GameObject currentObject in currentObjects)
                {
                    if (currentObject == null || originalObjects.Contains(currentObject))
                        continue;

                    try
                    {
                        DestroySelfCheckObject(currentObject);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(
                            $"[BattleRuntimeSelfCheck] Failed to destroy self-check singleton '{currentObject.name}': {ex.Message}");
                    }
                }
            }
        }

        private static T FindSceneComponent<T>() where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && component.gameObject.scene.IsValid())
                    return component;
            }

            return null;
        }

        private static void CaptureSceneObjects<T>(HashSet<GameObject> objects) where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && component.gameObject.scene.IsValid())
                    objects.Add(component.gameObject);
            }
        }

        private static void DestroySelfCheckObject(GameObject target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        private static void DestroySelfCheckAsset(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        private sealed class TemporaryObjectPoolInitialization : IDisposable
        {
            private readonly LF2ObjectPool pool;
            private readonly GameObject temporaryPoolObject;
            private readonly System.Reflection.FieldInfo availableField;
            private readonly System.Reflection.FieldInfo activeField;
            private readonly System.Reflection.FieldInfo releaseMapField;
            private readonly System.Reflection.FieldInfo spritePoolField;
            private readonly System.Reflection.FieldInfo cachedPrefabField;
            private readonly object originalAvailable;
            private readonly object originalActive;
            private readonly object originalReleaseMap;
            private readonly object originalSpritePool;
            private readonly object originalCachedPrefab;
            private readonly bool ownsState;

            public TemporaryObjectPoolInitialization()
            {
                LF2ObjectPool resolvedPool = LF2ObjectPool.Instance;
                if (resolvedPool == null)
                {
                    temporaryPoolObject = new GameObject("SelfCheck_TemporaryLF2ObjectPool");
                    resolvedPool = temporaryPoolObject.AddComponent<LF2ObjectPool>();
                }
                pool = resolvedPool;
                Expect(pool != null, "real opoint fixture requires an LF2ObjectPool singleton");

                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                Type type = typeof(LF2ObjectPool);
                availableField = type.GetField("_availableObjects", flags);
                activeField = type.GetField("_activeObjects", flags);
                releaseMapField = type.GetField("_releaseTimeMap", flags);
                spritePoolField = type.GetField("_spritePool", flags);
                cachedPrefabField = type.GetField("_cachedLF2ObjectPrefab", flags);
                Expect(availableField != null && activeField != null && releaseMapField != null &&
                       spritePoolField != null && cachedPrefabField != null,
                    "real opoint fixture LF2ObjectPool field contract changed");

                originalAvailable = availableField.GetValue(pool);
                originalActive = activeField.GetValue(pool);
                originalReleaseMap = releaseMapField.GetValue(pool);
                originalSpritePool = spritePoolField.GetValue(pool);
                originalCachedPrefab = cachedPrefabField.GetValue(pool);
                ownsState = originalAvailable == null || originalActive == null || originalReleaseMap == null;
                if (!ownsState)
                    return;

                availableField.SetValue(pool, new LinkedList<GameObject>());
                activeField.SetValue(pool, new HashSet<GameObject>());
                releaseMapField.SetValue(pool, new Dictionary<GameObject, float>());
                spritePoolField.SetValue(pool, new Stack<SpriteRenderer>());
                cachedPrefabField.SetValue(pool, null);
            }

            public void Dispose()
            {
                try
                {
                    if (ownsState)
                    {
                        var objects = new HashSet<GameObject>();
                        if (availableField.GetValue(pool) is LinkedList<GameObject> available)
                        {
                            foreach (GameObject item in available)
                                if (item != null) objects.Add(item);
                        }
                        if (activeField.GetValue(pool) is HashSet<GameObject> active)
                        {
                            foreach (GameObject item in active)
                                if (item != null) objects.Add(item);
                        }

                        foreach (GameObject item in objects)
                        {
                            if (Application.isPlaying)
                                UnityEngine.Object.Destroy(item);
                            else
                                UnityEngine.Object.DestroyImmediate(item);
                        }

                        availableField.SetValue(pool, originalAvailable);
                        activeField.SetValue(pool, originalActive);
                        releaseMapField.SetValue(pool, originalReleaseMap);
                        spritePoolField.SetValue(pool, originalSpritePool);
                        cachedPrefabField.SetValue(pool, originalCachedPrefab);
                    }
                }
                finally
                {
                    if (temporaryPoolObject != null)
                    {
                        if (Application.isPlaying)
                            UnityEngine.Object.Destroy(temporaryPoolObject);
                        else
                            UnityEngine.Object.DestroyImmediate(temporaryPoolObject);
                    }
                }
            }
        }

        private sealed class CurrentDatSelfCheckWeapon : LF2Weapon
        {
            private readonly LF2ObjectType currentDataType;

            public CurrentDatSelfCheckWeapon(LF2ObjectType currentDataType)
            {
                this.currentDataType = currentDataType;
            }

            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(
                string name,
                int objectId,
                int weaponType,
                LF2CharacterData data,
                int frameId)
            {
                Name = name;
                ObjectId = objectId;
                SetWeaponType(weaponType);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(frameId);
                Frame.PN = frameId;
                Frame.N = frameId;
                Runtime.Frame = frameId;
                Runtime.PrevFrame2 = frameId;
                Health.HP = 500;
                Health.HPBound = 500;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)currentDataType;
        }

        private sealed class RealOpointProducerSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            internal override bool UsesDynamicRuntimeSlot() => true;

            public RealOpointProducerSelfCheckEntity(string label)
            {
                Name = $"SelfCheck_RealOpointProducer_{label}";
                ObjectId = 739;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);

                LF2FrameData frame = Frame(0, LF2States.Standing, 100, 0, 39, 79);
                frame.opoint = new ObjectPoint { kind = 1, oid = 999, action = 0, facing = 0 };
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, new LF2CharacterData
                {
                    name = Name,
                    frames = new List<LF2FrameData> { frame },
                }));
                Frame.D = frame;
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override void RunLateTailBeforePrevFrame()
            {
                AttackingCounter = 1;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class RuntimeSlotOpointProducerSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            internal override bool UsesDynamicRuntimeSlot() => true;

            public RuntimeSlotOpointProducerSelfCheckEntity(
                string label,
                int state,
                int facing,
                int stableId)
            {
                Name = $"SelfCheck_RuntimeSlotOpointProducer_{label}";
                StableId = stableId;
                ObjectId = 740;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);

                LF2FrameData frame = Frame(0, state, 100, 0, 39, 79);
                frame.opoint = new ObjectPoint { kind = 1, oid = 999, action = 0, facing = facing };
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, new LF2CharacterData
                {
                    name = Name,
                    type_sub = (int)LF2ObjectType.SpecialAttack,
                    frames = new List<LF2FrameData> { frame },
                }));
                Frame.D = frame;
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
                => (int)LF2ObjectType.SpecialAttack;

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class DynamicSlotSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            internal override bool UsesDynamicRuntimeSlot() => true;

            public DynamicSlotSelfCheckEntity(int stableId)
            {
                StableId = stableId;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class QueuedBoundarySelfCheckEntity : LF2Entity
        {
            public enum Phase
            {
                FrameLogic,
                ObserveFrameLogic,
            }

            private readonly Phase phase;
            private readonly ReleaseSpawnSemantic[] semantics;

            public int EnqueueCount { get; private set; }
            public int QueueCountObservedAtFrameLogic { get; private set; } = -1;
            public OPointCreateTask LastTask { get; private set; }
            public List<OPointCreateTask> PublishedTasks { get; } = new List<OPointCreateTask>();
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public QueuedBoundarySelfCheckEntity(Phase phase, params ReleaseSpawnSemantic[] semantics)
            {
                this.phase = phase;
                this.semantics = semantics ?? Array.Empty<ReleaseSpawnSemantic>();
                Name = $"SelfCheck_QueuedBoundary_{phase}";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);

                var data = new LF2CharacterData
                {
                    name = Name,
                    frames = new List<LF2FrameData>
                    {
                        new LF2FrameData
                        {
                            frameId = 0,
                            state = LF2States.Standing,
                            wait = 1,
                            next = 0,
                            hit_Fa = 5,
                        },
                    },
                };
                FrameCache.Load(new LF2CharacterDataWrapper(0, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
            {
                return phase == Phase.FrameLogic || phase == Phase.ObserveFrameLogic;
            }

            public override void RunFrameLogicBeforeAdvance()
            {
                if (phase == Phase.ObserveFrameLogic)
                {
                    QueueCountObservedAtFrameLogic = GetQueuedObjectPointTaskCount(LF2ObjectPointFactory.Instance);
                    return;
                }

                if (phase == Phase.FrameLogic)
                    Publish(semantics.Length > 0 ? semantics[0] : ReleaseSpawnSemantic.ImmediateEffect);
            }

            private void Publish(ReleaseSpawnSemantic semantic)
            {
                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint { oid = -700 - EnqueueCount, kind = 0, action = 0 };
                task.parent = this;
                task.team = Team;
                task.releaseSpawnSemantic = semantic;
                LF2ObjectPointFactory.Instance.EnqueueCreateObject(task);
                LastTask = task;
                PublishedTasks.Add(task);
                EnqueueCount++;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class QueuedBoundarySelfCheckWeapon : LF2Weapon
        {
            public int TransitDestroyCount { get; private set; }
            public bool PendingDestroyObserved { get; private set; }

            public void BindData(LF2CharacterData data)
            {
                Name = data.name;
                ObjectId = 100;
                SetWeaponType((int)LF2ObjectType.LightWeapon);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 1;
                Health.HPBound = 1;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                bool completed = base.TryRunLatePostOpointCleanupPhase();
                PendingDestroyObserved |= Runtime.PendingFlushDestroy;
                return completed;
            }

            public override void OnTransitDestroy()
            {
                TransitDestroyCount++;
                UnregisterFromWorld();
            }
        }

        private sealed class QueuedBoundaryTransitionSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public QueuedBoundaryTransitionSelfCheckEntity(LF2CharacterData data)
            {
                Name = data.name;
                ObjectId = 700;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 10;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.SetPosition(100, -20, 100);
                Runtime.SyncIntegerPosition();
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class MutationSelfCheckEntity : LF2Entity
        {
            private readonly bool _registerDuringLate;
            private readonly bool _unregisterDuringLate;

            public MutationSelfCheckEntity Spawned { get; private set; }
            public int LateTickCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public MutationSelfCheckEntity(int stableId, bool registerDuringLate = false, bool unregisterDuringLate = false)
            {
                StableId = stableId;
                _registerDuringLate = registerDuringLate;
                _unregisterDuringLate = unregisterDuringLate;
            }

            public override void SimFrameTick(int tickIndex)
            {
                LateTickCount++;

                if (_registerDuringLate && Spawned == null)
                {
                    Spawned = new MutationSelfCheckEntity(1000 + StableId);
                    Match.Register(Spawned);
                }

                if (_unregisterDuringLate && LateTickCount == 1)
                    Match.Unregister(this);
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class SelfCheckCharacterDatShell : LF2SpecialAttack
        {
            public void InitializeForCpoint()
            {
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                InitializeForCpoint();
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.N = 0;
                Frame.PN = 0;
                Frame.D = FrameCache.GetFrameDataById(0);
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 100;
                Health.HPBound = 100;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)LF2ObjectType.Character;
            public override void Reset() { }
        }
    }
}


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# Independent review: GameTick / Physics parity batch 1

Review the production changes for GT-01, GT-02, PH-03, PH-04, PH-05 and PH-06.
The only authority is `J:\QQFile\NTSD2.4\ntsd_release_C#`. Do not read, cite,
or infer behavior from C++, disassembly, pseudocode or legacy implementations.

Authority entry points:

- `src/BattleCore/Simulation/GameTick.cs`
- `src/BattleCore/Frame/FrameAdvance.cs`
- `src/BattleCore/Frame/Physics.cs`

Audit report and Unity files are supplied as context. Verify exact ordering and
field semantics, especially:

- NeedClearInput ordering and early whole-tick return;
- whether current, previous, cooldown, combo and history input fields are reset
  at the same boundary as authority;
- per-active-slot key clearing immediately before frame advance while retaining
  previous-edge state;
- double constants and no implicit float arithmetic;
- landing damage retaining negative HP/HpMax-equivalent values;
- oid999 exact-ground versus crossed-ground frame 101;
- no unauthorized WeaponState writes during weapon landing;
- real Character, shared Character-DAT and transformed current-DAT routing;
- whether focused tests could pass while production behavior is still wrong.

Review only; do not edit files. Lead with severity-ordered findings grounded in
exact file/line references. If there are no blockers, explicitly state PASS and
list residual test gaps. Write the review to the output file.
