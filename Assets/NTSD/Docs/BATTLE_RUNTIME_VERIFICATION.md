# Unity NTSD 战斗运行时验证记录

日期：2026-06-04

## 范围

当前 Unity 复刻范围只覆盖战斗场景运行时：固定 tick/pass、输入/AI、帧推进/状态、实体位移与逻辑 X 边界、碰撞/命中、武器/cpoint/opoint、死亡复活、波次与实体生命周期。菜单、选人、加载、HUD/结算、camera/background/render、audio playback、network、replay/rollback 和编辑器预览不作为本轮验证目标。

战斗逻辑基准：

- `J:\QQFile\NTSD2.4\ntsd_release`
- 不使用 C++ Debug 宏路径作为正式战斗逻辑依据。
- 不再把反汇编文档作为 Unity 当前实现的直接基准；C++ release 是 Unity 复刻的中间还原工程。

## 当前处理状态

- `NTSDBattleTickSystem` 和 `SimulationWorld` 已按战斗运行时 pass 顺序与稳定实体遍历重新整理。
- `LF2Character` 及 partials 已瘦身，保留输入、抓取、命中、自然恢复、死亡复活、移动和特殊状态等正式战斗逻辑。
- `LF2Entity` / `LF2LivingObject` 已收敛到运行时状态、直接写帧和通用实体生命周期。
- `LF2WeaponBase` / `LF2Weapon` / `LF2SpecialAttack` 已按 C++ release 语义审计武器、技能对象、kind=8、命中冷却和随机掉落路径。
- `itr.kind` 判定已移除旧别名匹配路径。C++ release 命中分支按精确 kind 处理：攻击类保留 `0/4/8/9/10/11/15/16`，预交互类保留 `1/2/3/7`。
- `NTSDEntityRuntime.AttackingCounter` 用于镜像 C++ release `Entity::attacking`，避免继续混用旧 `HitStun` 语义。
- `ImmediateFrame()` / `SetFrameDirect()` 作为 C++ release 直接写帧入口，绕过普通转帧请求仲裁。
- `SimulationWorld` 的普通战斗 pass 使用稳定 runtime-slot 快照；`LateEntityUpdateAll` 使用动态 runtime-slot 扫描以匹配 authority 的固定槽循环：当前实体生成到更大空闲 slot 的对象可在同一 late pass 后续被处理，生成到已扫描 slot 的对象则从下一 tick 参与。
- 旧 FLF/反汇编残留的状态壳、旧 catch 字段、旧输入 pending command 模式、旧统计适配层和无行为状态分发已逐步移除。

## 抓取自检

新增自检文件：

- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheckCore.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleRuntimeSelfCheckEditor.cs`

覆盖内容：

- cpoint `aaction` 直接写入抓取者帧，并用目标帧 `cpoint.vaction` 写入被抓者帧。
- `throwvx/throwvy/throwvz` 投掷速度、方向反转、`throwinjury>0` 和抓取关系清理。
- 被抓者位置同步，包含 `PS.x` 横向、`PS.y` 垂直、`PS.z` 深度和 `cover` 修正。
- `decrease<0` 逃脱，包含双方帧、`HitCount`、`KnockbackVx/KnockbackVy` 和关系清理。
- `LateEntityUpdateAll` 遍历期间注册/注销对象不应触发集合枚举修改异常；新对象是否同 tick 参与取决于 runtime slot 是否位于当前扫描位置之后。

入口：

- 组件右键菜单：`运行战斗运行时自检`
- 编辑器菜单：`NTSD/验证/运行战斗运行时自检`
- batchmode：

```powershell
& 'D:\Unity\HubEditor\2022.3.34f1\Editor\Unity.exe' -batchmode -quit -projectPath 'I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity' -executeMethod NTSD.EditorTools.BattleRuntimeSelfCheckEditor.RunForBatchmode -logFile 'I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\ntsd_selfcheck_unity.log'
```

## 已执行验证

编译：

```powershell
dotnet build Assembly-CSharp.csproj /v:minimal /m:1
dotnet build Assembly-CSharp-Editor.csproj /v:minimal /m:1
```

结果：

- `Assembly-CSharp.csproj`：通过，`0 errors / 51 warnings`。
- `Assembly-CSharp-Editor.csproj`：通过，`0 errors / 25 warnings`。

警告为项目既有 Unity/package 引用冲突、`TimeWheel` nullable 注解上下文、少量 UI/App 未使用字段和编辑器未使用变量；新增自检脚本没有引入新的编译 error。

2026-06-04 追加验证：

- 修复 `SimulationWorld.LateEntityUpdateAll` 在遍历 `_buckets` 时由 opoint/销毁触发注册或注销对象导致的 `InvalidOperationException: Collection was modified`。
- `dotnet build Assembly-CSharp.csproj /v:minimal /m:1`：通过，`0 errors / 51 warnings`。
- `dotnet build Assembly-CSharp-Editor.csproj /v:minimal /m:1`：通过，`0 errors / 25 warnings`。
- 静态检查 `SimulationWorld.cs` 中已无直接 `foreach (_buckets)` / `foreach (_buckets.Values)` / `foreach (kvp.Value.items)` 形式的可变桶枚举。
- 已新增 `BattleRuntimeSelfCheck.CheckSimulationWorldLateMutation` 覆盖 late pass 中注册和延迟注销对象的场景。当前用户重新打开 Unity 项目，`Temp/UnityLockfile` 存在，batchmode 自检需在项目关闭后再跑。

静态扫描：

```powershell
rg -n "反汇编|disassembl|disassembly|old source|原版 LF2|DEBUG_SKIP_CHARSEL|NTSD_TRACE_|GetStatesSwitchDir|FrameAniOscillate|LF2AnimationState|FrameForceEvent|TUForceEvent|TransTrans|ReloadCharacterFrameData|SetNextFrame|PlayFrameByID|NTSDCharacterStats|LF2BattleStat|ComboCount|PickupCount|KillStat" Assets/NTSD/Scripts/Animation Assets/NTSD/Scripts/Simulation Assets/NTSD/Scripts/Input Assets/NTSD/Scripts/Test -g "*.cs" -g "!Gen/**" -g "!**/Editor/CharacterFramePreviewWindow.cs"
rg -n "caught_b|caught_throw|caught_throwinjury|caught_decrease_counter|_catchingStateTU|_catchingCounter|_catchingAttacks|_caughtDecayAccum|NTSDDamageCalculator|ProcessCatchingPostCombo|ApplyCollisionCheck1CaughtLogic|ApplyCatchingTransformToVictim\(" Assets/NTSD/Scripts/Animation Assets/NTSD/Scripts/Simulation Assets/NTSD/Scripts/Input Assets/NTSD/Scripts/Test -g "*.cs" -g "!Gen/**"
rg -n "MatchesKindAlias|MatchesKindAliasValue|QueryItrs\(|MatchItrKind\(" Assets/NTSD/Scripts/Animation Assets/NTSD/Scripts/NTSD_Extensions -g "*.cs"
```

上述旧逻辑扫描无匹配。另行扫描常见 mojibake 字形（例如替换符、UTF-8/GBK 错解高频片段）时，战斗运行时脚本也无匹配。`CharacterFramePreviewWindow.cs` 属于编辑器预览，不在战斗运行时范围内。

Unity batchmode 自检：

- 已通过运行时闭环。
- 命令完成后生成 `Temp/NTSD_BattleRuntimeSelfCheck.result`，内容为 `PASS`。
- 日志 `ntsd_selfcheck_unity.log` 包含：
  - `[BattleRuntimeSelfCheck] 战斗运行时自检通过。`
  - `[BattleRuntimeSelfCheckEditor] 自检完成。`
- 第一次 batchmode 进入时 Unity 先触发脚本重编译，Bee 返回 `ExitCode: 4` 后追加构建，后续 `Tundra build success`；再次执行后自检正常写出 `PASS`。
- 自检完成并写出 `PASS` 后，Unity Editor 退出阶段出现 `CoreBusinessMetrics` / `BackgroundJobQueue` 相关崩溃。该崩溃发生在自检完成之后，不影响当前战斗运行时断言结果，但属于 Unity batchmode 退出稳定性风险。

2026-07-14 最新验证：

- fresh `dotnet build Assembly-CSharp.csproj /v:minimal /m:1`：`0 errors / 18 warnings`。
- 最终代码的 fresh Unity batch 运行时证据是 sibling 隔离项目中的日志：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity-selfcheck\Unity-BattleRuntimeSelfCheck-P0-rerun7.log`。该日志包含“战斗运行时自检通过/自检完成”。
- 主工作区没有对应的 `Temp/NTSD_BattleRuntimeSelfCheck.result`；本次验收依据是上述 fresh 日志，不是 result 文件。
- T0-T9 针对性断言通过；这仍不是完整对局逐帧对拍。
- T8 默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。

## 已知风险

- 当前最小 battle runtime 自检已闭环，但仍不是完整对局回放验证。
- Unity batchmode 在自检完成后的退出阶段出现过 `CoreBusinessMetrics` / `BackgroundJobQueue` 崩溃；如后续继续自动化验证，应优先隔离或关闭相关编辑器指标上报路径。
- `FrameTransistor` 仍是 Unity 适配层，不是 C++ release 原始结构；直接写帧路径已绕过仲裁，但普通请求路径后续仍可继续审计。
- `HitStun` 剩余使用应只保留在 hit_stop 门控语义中，后续若发现 `attacking=0` 或帧推进语义混用，需要继续迁移到 `AttackingCounter`。
- negative `vaction` 三类语义已完成并通过 fresh Unity batch：action selection 对 attacker 负 action 做翻面+绝对帧、victim vaction raw 写；throw 将 `next`/`vaction` raw 写入 frame/prev2；held-sync 先 raw 写、负值再翻面取绝对帧，位置使用原始 signed `vaction` 对应 cpoint 坐标，且 `vaction==0` 写 frame 0。三项矩阵均覆盖 real character 与 shared-DAT shell。
- `FrameToggle`/`FrameMod12` 已纳入统一 tick-head Flow 推进；state 400/401 的奇偶 gate、state401 self、non-character source、Character target 选择和 no-target 清速度矩阵已通过 Unity 运行时自检。
- `ValidatePositiveLinks` 已由 `ValidateHeldLinksAll` 全局覆盖 slot `0..399`，包括 character/non-character holder、边界 slot、越界/inactive/mismatch、target link 正负无关及只清 `LinkState` 契约，已通过 Unity 运行时自检。
- PreFrame X 逻辑边界已完成并通过 fresh Unity 运行时自检：entity pass 分离 `BaseStageWidthPx` 与 `XMaxOverride`，按 current DAT type/OID 中央分派，并覆盖 `RelationTeam`/`HitStop`/`Unk344`/`YInt`/strict edges/`XInt`/free lifecycle。其余纯战斗 backlog 从 DAT transform shell、Step10 专项开始，另含 opoint/pass visibility、per-class `frame_advance`、`frame_tick` 内部与 collision snapshot。完整状态/authority/验收标准见 `csharp-vs-unity-battle-alignment.md` §10。
- `InputPhase`、state 500/501、M5 死亡弹地和 T5 respawn 已完成，不再属于已知风险。Mode2/`InitStats` 是 F7-F9/debug 路径，排除于正式战斗对齐。

2026-07-14 P0 cpoint 追加验证：fresh `dotnet build` 为 `0 errors / 18 warnings`；独立 Unity batch 日志 `Unity-BattleRuntimeSelfCheck-P0-rerun7.log` 明确包含“战斗运行时自检通过/自检完成”。

2026-07-15 P1 Flow/link 追加验证：fresh `dotnet build Assembly-CSharp.csproj /v:minimal /m:1` 为 `0 errors / 42 warnings`；主 Editor request 于 00:57:49 fresh 返回 `PASS`。隔离 clone batch 的 clean/增量脚本编译均成功且无 C# 错误，但两次都在 post-compile domain reload 后停滞，未生成 PASS，因此不能作为运行时证据。

2026-07-15 P1 BOUNDS-X 追加验证：fresh `dotnet build Assembly-CSharp.csproj /v:minimal /m:1` 为 `0 errors / 18 warnings`。symlink clone 仍会停在 post-compile reload，因此最终使用 detached physical worktree `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity-boundsverify`、精确 Unity `2022.3.4f1c1` 和 direct `BattleRuntimeSelfCheckEditor.RunForBatchmode` 做 fresh batch；日志 `Unity-BattleRuntimeSelfCheck-BOUNDS-X-fresh.log` 明确包含 `Application.AssetDatabase Initial Refresh End`、`[BattleRuntimeSelfCheck] 战斗运行时自检通过。`、`[BattleRuntimeSelfCheckEditor] 自检完成。`，且无 `error CS` / `Compilation failed`。batch 退出会清理该临时项目的 result 文件，因此本次验收依据是 fresh 双日志证据，不是 request 路径 PASS。request harness 同步改为 `EditorApplication.update` 轮询：发现待处理 request 后先删除旧 result；删除失败时 warning、保留 request 并重试；`EditorApplication.isCompiling || EditorApplication.isUpdating` 时不消费 request；in-progress gate 仅降低重复执行风险，不作为绝对幂等保证。该 harness 加固已通过 dotnet 编译，但不冒充本次 direct batch 的运行时证据。
