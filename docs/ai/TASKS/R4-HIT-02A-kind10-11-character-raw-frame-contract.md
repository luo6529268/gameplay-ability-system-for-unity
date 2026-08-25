# R4-HIT-02A — kind10/11 character raw-frame contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 最小脚本、Unity compile与full self-check已通过；C++ trace / Play Mode待补。  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-HIT-002` 的第一子包。  
> 关联 Change ID：`R4-HIT-002A`。

## Goal

使 character victim接收 kind10/11时的 frame写入遵循 C++ `collision.cpp` 的 raw `frame=182` 合同：
保留既有 kind10/11 force/stat逻辑，同时不由 Unity helper附带改写前帧、攻击标记或当前 wait counter。

## Scope

仅允许修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs`
   - exact `LF2Character` route的 `ApplyFluteCharacterForce`；
2. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
   - shared character-DAT route的同名方法；
3. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - existing exact/shared、kind10/11 matrix增加 raw-write negative-side-effect assertions；
4. 本包的 Change Record、ledger、STATE、diff register、主计划与handoff。

禁止：

- 全局修改 `ImmediateFrame`、`SetFrameDirect`、`DirectWriteRawFramePreserveWaitCounter`、`FrameTransistor`；
- 处理 kind16、weapon victim、weapon attacker、CPoint、held/link、opoint、candidate、input、scheduler、AI、render、DAT/资源或 C++ authority；
- 重新排序 kind10/11 stat、velocity或 air-step writes；
- 启动 C++ executable、C++ trace、Play Mode、完整构建或性能测试。

## Authority / Evidence

### VERIFIED — C++ release source

- `src/entity/collision.cpp:1193-1237`：character kind10/11在 velocity阻尼后直接写
  `vic_core_local.frame = 182`；该 case不写 prev frame、attacking或 wait counter；
- 同 case的 weapon count、conditional holder combo、damage stat和Y/Vy air step保持既有顺序；
- release build参与性已由项目既有 Makefile source contract记录，且本包只读复核相关 live path。

### VERIFIED — Unity current source

- exact/shared两条 `ApplyFluteCharacterForce` 都调用 `ImmediateFrame(182)`；
- `LF2Entity.ImmediateFrame` 会写 `Frame.PN = Frame.N`、`AttackingCounter=0`并以目标帧的 wait/next同步；
- `DirectWriteRawFramePreserveWaitCounter` 已被现有 landing raw-frame contract使用，保留 PN、attacking和
  `Trans.WaitCounter`，同时更新 Unity-required Frame.Data mirror。

### UNKNOWN / excluded

- C++ runtime trace、真实 flute Play Mode和跨tick presentation；
- type1/2/4 weapon的同类writer和kind16，均各自归后续子包。

## Required behavior

对 exact/shared route、kind10和kind11的四个fixture组合：

1. current `Frame.N` 与 `Runtime.Frame`成为182，且 `Frame.D` 指向182 DAT frame；
2. 预置的 `Frame.PN`、`AttackingCounter`、`Trans.WaitCounter`不变；
3. 既有 weapon count、combo/damage stat、速度阻尼、Y/Vy处理仍按当前 C++ 已确认合同运行；
4. 不创建集合/对象，不增加 RNG，不改 ITR authored字段。

## Deliverables

1. 两处 precise helper调用替换；
2. exact/shared × kind10/11 raw-side-effect focused self-check；
3. Unity compile、full self-check、ledger validator和`git diff --check`的实际结果；
4. 更新 Change Record、STATE、差异登记、主计划和handoff，最高状态仅可为 `RUNTIME_PENDING`。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 | C++ case10/11 raw frame source和两条Unity writer路径复核。 |
| S1 | 四个fixture均保持 PN / attacking / wait，并写 target frame182。 |
| S2 | 原既有 kind10/11 stat结果保持；不引入 RNG/ITR/candidate side effect。 |
| S3 | Unity scripts compile=0 error、full self-check PASS、`pwsh` ledger validator与`git diff --check`通过。 |
| S4 | 仅 `RUNTIME_PENDING`；C++ trace/Play Mode保持未关闭。 |

## Stop conditions

- 发现 C++ case10/11 在未读 live caller中有同tick prev/attacking/wait writer；
- direct raw writer无法保留Unity required data mirror而需修改全局 helper；
- fixture显示需改kind16、weapon、scheduler、candidate、DAT或其他 scope外模块才能通过；
- 要求改变 C++ authority、C++ executable、既有保护的 CentralOnly/capacity/30Hz/FrameInputSet/对象池边界。

## Out of scope

`R4-HIT-02B`、`R4-HIT-02C`、`R4-HIT-02D`、`D-HIT-003`、R5～R8、T8、C++ executable、Unity Play Mode、服务器、Android、性能、render。

## 实施进度（2026-08-22）

- exact/shared两个 `ApplyFluteCharacterForce` 均已仅将 `ImmediateFrame(182)`替换为已有
  `DirectWriteRawFramePreserveWaitCounter(182)`；没有改任何全局frame helper；
- existing kind10/11 matrix现为 exact/shared × kind10/11四种组合，预置`PN=71`、`AttackingCounter=29`、
  `WaitCounter=17`，并断言它们保持不变、`Frame.N/Runtime.Frame=182`、Frame.Data mirror正确以及既有stats不变；
- UnityMCP scripts refresh/domain reload后 filtered `error CS`=0；
  `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入2026-08-22 05:43:54 +08:00；
- full self-check后console的两条MCP disposed-connection提示和两条runtime-rest negative-control log不构成
  编译错误或fixture失败，必须与`PASS`结果一起解释；
- C++ runtime trace与真实 flute Play Mode未执行，故最高状态保持`RUNTIME_PENDING`。
