# Unity NTSD 战斗运行时验证记录

日期：2026-06-04

## 范围

当前 Unity 复刻范围只覆盖战斗场景运行时。菜单、选人、加载、HUD/结算、编辑器预览和完整应用流程不作为本轮验证目标，除非它们直接影响战斗模拟。

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
- `SimulationWorld` 的战斗 pass 已改为 SimOrder key 快照 + 对象快照遍历；`LateEntityUpdateAll` 中 opoint/销毁导致注册或注销对象时，不再破坏 `SortedDictionary` 枚举器。
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
- `LateEntityUpdateAll` 遍历期间注册新对象和注销旧对象，不应触发集合枚举修改异常；新对象从下一次 late pass 开始参与遍历。

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

## 已知风险

- 当前最小 battle runtime 自检已闭环，但仍不是完整对局回放验证。
- Unity batchmode 在自检完成后的退出阶段出现过 `CoreBusinessMetrics` / `BackgroundJobQueue` 崩溃；如后续继续自动化验证，应优先隔离或关闭相关编辑器指标上报路径。
- `FrameTransistor` 仍是 Unity 适配层，不是 C++ release 原始结构；直接写帧路径已绕过仲裁，但普通请求路径后续仍可继续审计。
- `HitStun` 剩余使用应只保留在 hit_stop 门控语义中，后续若发现 `attacking=0` 或帧推进语义混用，需要继续迁移到 `AttackingCounter`。
- 抓取逻辑已按 C++ release 重写并有自检覆盖，但实际角色场景仍需验证连续抓取、方向投掷和 `throwinjury=-1` 变身/子对象数据传播。
