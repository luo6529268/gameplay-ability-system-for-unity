# R3-LAND-01 — character landing raw-frame writer

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source/static、existing Unity Editor compile和full self-check已通过；Play Mode和C++ runtime trace未关闭。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 对应差异：`D-MOV-002`。

## Goal

只关闭 character-DAT 落地分支的一个中间态差异：C++ `physics_update` 直接写 `core.frame`，而
Unity 的这些同类路径调用 `ImmediateFrame(...)`，额外修改 `Frame.PN`、`AttackingCounter`、Sprite 和
frame-transistor。将 Unity 的落地帧写入改为保留 C++ 物理阶段可观察中间态的 raw writer；仅在 C++
明确写 `special.attacking = 0` 的分支同步清零。

本包不改变落地阈值、伤害、速度、音效、对象类型分流或 pass 顺序。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDamageStateResolver.cs`
   - `ApplyFrozenLanding` 的 state13 high-speed frame185；
   - `HandleFallingGroundEvent` 的 state12/state18 low/high 与 ordinary landing target frame。
2. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
   - `ApplySharedCharacterDatLandingIfNeeded` 的相同 character-DAT compatibility 分支。
3. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 增加/调整 F04（physics landing）之后、F07（late frame_tick）之前的最小 frame/PN/attacking/wait
     fixture；覆盖 real `LF2Character` 与 shared-character-DAT adapter。

禁止：

- 修改 `ImmediateFrame`、通用 `SetFrameTickDirect`、`FrameTransistor`、pass 顺序、candidate/consume、
  hit/CPoint/held/link/opoint、render handoff、physical input、DAT/scene/pool/capacity/worker；
- 修改 type1/2/4/6/oid999 non-character landing，或扩张至 legacy `LF2Weapon.OnLanded` fallback；
- 修改 C++ authority 源码、构建、资源、配置或执行 C++ executable；
- 以本包 self-check 将 C++ runtime trace、Play Mode 或全战斗对齐标为通过。

## Authority / Evidence

### VERIFIED — C++ release source

- `src/entity/game_tick.cpp:1247-1276`：F02 frame advance/physics 在 candidate collect 之前；
  `game_tick.cpp:1645-1655` 才快照 `prev_frame2` 并收集候选；
  `game_tick.cpp:577-587` 的 late `frame_tick` 在全部 interaction/postprocess 后执行。
- `src/entity/physics.cpp:153-223`：character landing 只写 `core.frame`；只有以下分支同时写
  `special.attacking = 0`：state12 low-speed（frame230/231）和 ordinary state（frame94/215/219）。
  state12 high-speed、state18 bounce、state13 high-speed（frame185）均不在 physics 内清 attacking。
- `src/entity/frame_advance.cpp:847-855,995`：F07 late `frame_tick` 检测 `frame != wait_counter` 后才清
  attacking、递增，再在尾部写新的 `wait_counter`。

### VERIFIED — Unity source crosswalk

- exact character F04：`BattleEcsCharacterFrameAdvancePass.ExecuteExactCharacter` →
  `LF2Character.HandleLandingEventForFrameAdvance` → `LF2CharacterDamageStateResolver`；
- shared-character-DAT adapter：`LF2OtherObject` / `LF2SpecialAttack` / `LF2WeaponBase` 的 current DAT
  character path → `RunSharedCharacterDatFrameAdvanceAsCharacter` →
  `ApplySharedCharacterDatLandingIfNeeded`；
- `LF2Entity.ImmediateFrame` 写 `Frame.PN`、`AttackingCounter=0`、Sprite 与重新同步 Trans；
  `DirectWriteRawFramePreserveWaitCounter` 写 target `Frame.N/D` 并保留 `PN`、attacking、wait counter，
  同时用 target DAT 更新 wait/next，供之后 F07 frame_tick 使用；
- `BruteForceSceneQuery` / candidate execution 不直接读取 `Frame.PN` 或 `AttackingCounter` 作为 C03/C07
  filter；它们仍是之后 frame progression 的直接输入，故本包只验证 F04 中间态，不能把不直接读取
  误写成“副作用无关”。

### BLOCKED / UNKNOWN

- R1-WP02 的 C++ executable trace 仍 `BLOCKED`；本包只能以 release source contract、Unity compile 和
  focused fixture 给出代码级证据。
- 真实 battle DAT、physical Play Mode、技能链与 legacy weapon fallback 不由本包认证。

## Planned branch contract

| C++ state / condition | C++ F04 writer | Unity target writer | attacking after F04 |
|---|---|---|---|
| state13, `vy <= 17 && |vx| <= 9` | no frame write | keep existing path | preserve |
| state13 high-speed | `frame=185` | raw write 185 | preserve |
| state12, low-speed, non-state18 | `frame=230/231`; explicit clear | raw write 230/231 | zero |
| state12 high-speed | `frame=185/191` | raw write 185/191 | preserve |
| state18 / burning | `frame=185` | raw write 185 | preserve |
| ordinary state | `frame=94/215/219`; explicit clear | raw write selected frame | zero |

`Frame.PN` must remain the pre-existing value throughout F04. The later F07 common frame tick remains the only
place in this package that sees the raw frame mismatch against `wait_counter`, clears attacking, advances frame
timing and writes the new wait counter.

## Acceptance

1. **S0 source/static**：上述 branch matrix、C++ F04→candidate→F07 order 和 Unity exact/shared writers
   都能定位；不再在这两个 character landing paths 调用 `ImmediateFrame`。
2. **S1 focused fixture**：在 F04 后、F07 前检查：
   - state12 low-speed：frame230/231、`PN` preserved、attacking=0；
   - state12 high-speed、state18、state13 high-speed：target frame、`PN` preserved、attacking preserved；
   - ordinary landing：target frame、`PN` preserved、attacking=0；
   - target `Frame.D` / transistor wait-next 跟目标 DAT 同步，wait counter 不被落地 writer 重置。
3. **S2 build/regression**：existing Unity Editor scripts compile、filtered C# errors=0、full
   `BattleRuntimeSelfCheck` PASS，且 `Tools/Validate-ChangeLedger.ps1` PASS。
4. **S3 evidence boundary**：状态最多提升至 `RUNTIME_PENDING`；不声称 C++ executable trace、physical
   Play Mode 或整个 R3 已对齐。

## Stop conditions

- 任一 branch 显示需要修改 `ImmediateFrame` / `FrameTransistor` 通用语义，或需要变更 candidate、late tick、
  CPoint、hit、render 才能通过；
- exact character 与 shared-character-DAT 无法由同一 source contract覆盖；
- focused fixture揭示 target frame / wait-counter / attacking 映射不明确，需要新 source audit；
- 需要 C++ executable、修改 authority、真实 scene/DAT/physical input，或用户提出新 Change Request。

## Out of scope

`D-MOV-001`、`D-MOV-003～005`、R3-PHY-01、R4～R8、R1-WP02、T8 default `stage.dat`、服务器、Android。

## 实际验证结果（2026-08-22）

- **最小脚本写入**：exact character 的 `ApplyFrozenLanding` / `HandleFallingGroundEvent` 与
  shared-character-DAT 的 `ApplySharedCharacterDatLandingIfNeeded` 已将对应 landing target frame 写入改为
  `DirectWriteRawFramePreserveWaitCounter`。state12 low-speed和ordinary branch仍显式清 attacking；state12
  high-speed、state18和state13 high-speed不在 F04 清 attacking。
- **静态 guard**：两个目标 landing method block 的 `ImmediateFrame(` 均为 `false`；exact falling-ground
  block有3处 raw writer，加上 frozen high-speed为4处；shared block有4处 raw writer。
- **首次编译 first-difference**：首次 existing Unity Editor scripts refresh 后，Unity 报
  `LF2Entity.cs(5569,25) CS0136`，原因是本包内层 `landingFrame` 与同方法外层局部变量重名。只将内层
  变量改为 `fallingLandingFrame`，未扩大范围。
- **最终 compile / self-check**：修正后，UnityMCP `refresh_unity(force/scripts/compile)` 于约 02:41:50
  在预期 domain reload/reconnect 后恢复 ready；执行 `NTSD/验证/运行战斗运行时自检` 后，
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 **2026-08-22 02:42:40 +08:00** 写入 `PASS`。新 fixture
  实际覆盖 exact/shared 的 state12 low front/back、state12 high、state18、state13 high、ordinary state100、
  ordinary frame212和ordinary default，检查 target frame、PN、attacking、wait counter、target DAT wait/next。
- **治理**：`Tools/Validate-ChangeLedger.ps1` PASS（12 records / 11 governed code files）；
  `git diff --check` exit 0，仅有既有 LF/CRLF warning。

这些证据只证明 Unity 代码级 adaptation；physical Play Mode、实际技能/落地表现、legacy non-character fallback
和 C++ executable trace仍未验证。
