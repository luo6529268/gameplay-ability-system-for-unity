# R8-WP01G-R11 — authored state2000 / state8xxx production runtime acceptance

> 建立日期：2026-08-24  
> 状态：`COMPLETE / VERIFIED / TEST-ONLY`  
> Change IDs：`R8-AUTHOREDSTATE-PLAY-001`

## Goal

只对 C++ Release 正式 DAT 中真实存在且 production 可达的残余状态样板补 Unity Play 验收：

1. 正式 weapon/object `state2000` 在完整 tick 中按 `Vx > 0 ? right : left` 写朝向；
2. 正式 type0 角色 authored `state8xxx` 经完整 tick 切换 current DAT、frame0、`RenderPicOffset=140`，并产生正确 CentralOnly snapshot/command/catalog/UV。

## Scope

### 允许

- 新增一个 Editor-only production Play probe；
- 使用 `data.txt`、当前 `Config/Character` 与生产 manager/pool/factory；
- 使用真实 OID32 Hunter frame0/state8032，以及正式 state2000 weapon样板；
- 通过完整 `SimulationTickDriver.StepOneTick(...)` 验收，不直接调用 gameplay helper 代替 tick；
- 验证 slot/generation、对象池与场景状态清理恢复。

### 禁止

- 不修改 production gameplay、DAT/BMP、C++、CentralOnly、Texture2DArray、动态 Mesh、1.5× visual scale或固定相机；
- 不伪造 type0 state2000：C++ 正式 DAT 中不存在该 authored sample；
- 不强制 OID999 frame399：正式 release producer/next/opoint 均不可达；
- 不伪造 CLR shell/current-DAT mismatch：C++ 统一 Entity 不存在这种 production sample；
- 不把 Unity Play PASS 写成 C++ executable full trace。

## Authority / Evidence

- `frame_advance.cpp:884-887`：state2000 facing；
- `game_tick.cpp:375-382`：`8000 <= state < 9000` 切换 OID、frame0、`unk_318=140`；
- C++ 正式 DAT 只读全量盘点：38个state2000均为type1/2/4；8个state8xxx由OID9/30/32/39/51/55 authored；
- 当前 Unity adapted DAT 已恢复同8个state8xxx语义帧。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleAuthoredStateResidualPlayModeProbeEditor.cs`；
- 对应 `.meta`；
- 本 Task、Change Record、Ledger、STATE、主计划与 handoff。

## Unknowns

- OID32与目标OID32同身份切换时 production runtime 是否保留当前 handle；由本 probe 裁决；
- 完整 tick 后 source frame 是否推进；验收以 C++ pass 时点的 current DAT/frame/offset 与最终 Central command 为准。

## Deliverables / Verification

1. fresh compile 0 error；
2. probe用正式 production pool 创建样板并执行完整 tick；
3. state2000正/负Vx朝向通过；
4. state8032 current DAT/frame0/offset140/effective-pic/Central command/UV通过；
5. cleanup恢复，Console无意外error；
6. full `BattleRuntimeSelfCheck`仍PASS；
7. Change Ledger validator PASS。

## Stop conditions

- 需要修改 production gameplay 或DAT才能让probe通过；
- production full tick暴露新的first difference；
- Unity Editor/资源/编译阻塞；
- 用户改变范围。

## Out of scope

OID999不可达frame399、CLR/current-DAT synthetic mismatch、F1/F2、AI parity、T8、R1-WP02 full trace、Android、服务器。

## Final evidence — 2026-08-24

- Unity fresh compile：`Assembly-CSharp-Editor.dll`于12:19:48完成编译，当前Console无C#编译错误；
- production Play result：`Temp/NTSD_R8_WP01G_R11_AuthoredStateResidual.result.json = PASS`；
- state2000：OID150正Vx→right、负Vx→left；
- state8032：DAT32/frame0/offset140/effective pic140；worker逻辑snapshot延迟到主线程materialize后生成18条命令，目标body command、catalog与UV一致；
- cleanup：world、slot、object pool、logic pool均恢复样板执行前基线；
- focused regression：功能键4/4、snapshot/checksum/restore 18/18 PASS；
- full `BattleRuntimeSelfCheck`：2026-08-24 12:28:41 `PASS`；
- production gameplay、DAT/BMP和C++均未修改。
