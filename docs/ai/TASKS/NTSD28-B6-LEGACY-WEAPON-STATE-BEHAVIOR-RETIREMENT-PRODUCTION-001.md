# NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001

Goal20 R3; `IN_PROGRESS / TEST_FIRST`. 本包只退休 Unity 旧的 WeaponState 生产行为，保留 reserved carrier 与现有 snapshot/copy/ECS fingerprint/checksum/parity 结构。用户授权的生产代码范围仅为 `LF2WeaponFrameLogicResolver.cs`、`LF2WeaponHeldStateResolver.cs`、`LF2WeaponBase.cs` 中与 `WeaponState` 直接相关的符号；测试、Task/Record 与 `Temp/Goal20_R3_*` 证据可新增。不得修改 schema、`NTSDSpec.cs`、`Gen/`、`Plugins/`、content、Prefab、Scene、`+0x2F8` 或 relation 本体。

## Authority and current facts

- Authority 依据：`NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 / VERIFIED`，以及正式 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。对应 source closure 由 `source/README_SOURCE.md` 声明的 `ntsd28_playable/scripts/build.ps1 -Target playable` 定义。
- `ntsd28_core` fresh grep 对 `weapon_state`/`weaponstate` 为 0 命中。`BattleWorld28`、`NativeAi28`、`PhysicsIntegrator28` 的 state 读取来自实体当前 `definition->frame(frame.action)`，不是第二套可变 WeaponState；Unity `Frame.D.state`/`CurrentFrameState()` 读取必须保留。
- Unity 当前唯一 production carrier reader 是 `LF2WeaponBase.ResolveRuntimeWeaponState()` → `GetRuntimeWeaponState()`，调用者为 `LF2WeaponFrameLogicResolver`。当前旧逻辑还包含 state1000 fast-to-40、1002→2000、2000 每 tick Vx×0.5 并在阈值进入 3000；其中 state1000 fast-to-40 若由当前 frame 的实际 state 触发，必须保留并改为 `CurrentFrameState()` 读取。
- `LF2WeaponHeldStateResolver` 当前有五处动态写入：Drop 两处 reserved zero、held follow 的 1001、throw 的 1002、damaged-drop 的 zero。`LF2WeaponBase.Reset()` 的 zero、`NTSDEntityRuntime` copy/reset、ECS/fingerprint/checksum/parity/snapshot 位置属于 carrier/schema，必须保留。
- 当前/release OID124 action40..55 为 16 帧 `state=1002 / hit_Fa=12` 循环；Tenten/Criminal2 direct OPoint、kind2 + Naruto clone DVX 均为可达 witness。旧逻辑已冻结为 tick1 checksum 首差、tick2 motion 首差。

## Exact implementation scope

1. `LF2WeaponFrameLogicResolver.cs`：删除仅依赖并行 `WeaponState` 的 1002→2000、2000 Vx halving→3000 旧 prelude；state1000 fast-to-40 若由当前 frame 的实际 state 触发则保留，并改为 `CurrentFrameState()` 读取。hit_Fa dispatch 继续按 actual current frame state/数据执行，保持 frame advance、physics、hit_Fa、RNG、relation 与 action 语义。
2. `LF2WeaponBase.cs`：删除仅为上述旧 reader 服务的 `ResolveRuntimeWeaponState()` 与 `GetRuntimeWeaponState()`；保留 `CurrentFrameState()`、`GetResolvedWeaponStateForExternalUse()`、Reset zero 及所有 carrier/schema 读写。
3. `LF2WeaponHeldStateResolver.cs`：删除五处动态 `Runtime.WeaponState` 写入；Drop/held/throw/damaged-drop 的 frame、motion、relation、RNG 与 cleanup 顺序不变。
4. 新增 focused test：`Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyWeaponStateRetirementProductionEditorTests.cs` 与其 `.meta`。测试只验证 source guard、OID124 actual-state 循环、两 tick 无 Vx halving/载体恒 zero、held follow/throw/drop carrier 恒 zero，以及现有 canonical copy/checksum field 仍可流动；不得使用 `Assert.Multiple`。
5. 新建当前包 C++ witness harness 与构建/结果文件于 `Temp/Goal20_R3_*`。harness 必须重新调用当前 playable core 的 frame/physics 相关路径，以同 seed/input/tick 记录退休前 tick1 checksum 与 tick2 motion 首差；退休后由 Unity focused/runtime witness 对同一窗口给出 `firstDifference=null`。旧 Goal18 impact harness/output 只能作工具参考，不能作为本包 authority witness。

## Test-first and acceptance

- 生产改动前先完成可达 OID124 fixture 与旧 behavior RED，保存 `Temp/Goal20_R3_RED_*`；RED 必须能指出 tick1 checksum 与 tick2 motion 的首差，不能只靠静态断言。
- 生产改动后 focused 测试必须证明：OID124 action40..55 actual frame state 始终 1002；carrier `WeaponState` 在 reset、held follow、throw、drop、damaged-drop 与连续 frame logic 中保持 0；Vx 不发生旧 halving；canonical copy/checksum/parity/snapshot 结构仍可读取同一 reserved 0。
- 保留所有实际 current frame state consumers（包括 type1/2/4/6 的 hit/landing/physics 分支）；不得因移除 carrier reader 而改写 `LF2States`、`NTSDEntityRuntime` 或其他模块的 actual frame state 判断。
- focused/编译/Unity Play 与 C++ witness 均取得证据后，才可将 Record 推进；没有真实 Editor/Play 时最多报告 `RUNTIME_PENDING`。共享 B6/NTSD28 regression、full SelfCheck、双 build、validator、Console、Scene SHA 由批末统一执行一次。
- 现有 `BattleRuntimeSelfCheck.cs` 中 `FL-WEAPON-STATE` 旧迁移期望约在 23013–23041，当前不在本包已声明三份生产脚本范围；若共享回归需要修订，必须由主代理另行确认授权并在对应 Record 中登记，不能在本包悄然改动。

## Invariants, risks and rollback

- reserved `WeaponState` 默认值固定为 0；不得删除 carrier、改变字段位置、改变 snapshot/checksum schema 或引入新状态字段。
- 不改变 `hit_Fa=4/12` 目标选择、frame advance、physics、landing、RNG、OPoint、held reciprocal relation、生命周期或 shutdown 顺序。
- 若 fresh callgraph 暴露三份授权脚本之外的动态 semantic reader/writer，或需要修改禁改文件，立即硬停并报告。
- 回滚只允许在取得明确批准后，按本包变更块恢复；不执行 `git restore/reset/clean/rm`，不覆盖他人工作，不 `git add/commit/push`。

当前状态：Task Contract 已建立；生产与测试代码尚未修改。RED、focused、C++ witness、Unity compile/Play、共享回归均为 `NOT_RUN`。

Final status VERIFIED within declared behavior scope. RED 3FAIL/2PASS; focused5/5; native normalized firstDifference/firstChecksumDifference/firstMotionDifference null. Closure/limitations and exact evidence: corresponding Change Record and Temp/Goal20_FinalSummary.json. Earlier NOT_RUN progress is superseded; original broad failure retained and reconciled with24-case focused recheck.
