# R4-HIT-02B — kind16 character raw-frame contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 最小脚本、Unity compile与full self-check已通过；C++ trace / Play Mode待补。  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-HIT-002` 的第二子包。  
> 关联 Change ID：`R4-HIT-002B`。

## Goal

使 character target接收kind16时的Unity frame write符合C++ `frame=200` raw-write合同，同时保留C++紧随其后的
显式`attacking=0`。目标不是移除attacking reset，而是阻止它被错误地作为`ImmediateFrame`的隐式副作用发生。

## Scope

仅允许修改：

1. `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs`
   - `ApplyKind16` 中仅一个frame writer callsite；
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - existing exact/shared kind16 fixture的PN/wait/raw-frame assertions和snapshot字段；
3. 本Change ID的ledger、STATE、diff register、主计划与handoff。

禁止：

- 删除或移动`victim.AttackingCounter=0`；
- 修改kind16伤害/stat、sound、vrest、held-release或RNG逻辑；
- 修改`BattleEcsHitExecutionPlan`、global frame helper、exact/shared resolver入口；
- 修改kind10/11、weapon raw-frame、CPoint、held/link、opoint、candidate、scheduler、input、AI、render、DAT、资源或C++ authority；
- 启动C++ executable、C++ trace、Unity Play Mode、完整构建或性能测试。

## Authority / Evidence

### VERIFIED — C++ release source

- `src/entity/hit.cpp:664-793`：kind16 character branch在SFX后直接`frame=200`，下一句独立
  `attacking=0`，随后才vrest/held-release；
- 同case未写prev frame或wait counter；
- release build参与性已由既有Makefile source contract记录，本包只读复核live path。

### VERIFIED — Unity current source

- exact/shared kind16均委托`BattleDamageWriter.ApplyKind16`；
- current `ImmediateFrame(MpDrain)`隐式写PN、attacking/wait，而method后已有C++对应的显式
  `victim.AttackingCounter=0`；
- existing `CheckKind16CharacterSideEffects`已覆盖actual/shared damage/stat/vrest/link，但尚未预置/断言PN与wait。

### INFERRED / excluded

- `BattleEcsHitExecutionPlan`是diagnostic projection而非canonical writer；它不在本包改动范围；
- C++ runtime trace与真实Play Mode未取得。

## Required behavior

在 exact/shared kind16 fixture：

1. 预置PN和wait后，`Frame.N`、`Runtime.Frame`与Frame.Data mirror成为200；PN/wait保持预置值；
2. `AttackingCounter`仍变为0，证明C++显式clear保留；
3. 既有lethal HP/HPBound/combo/kill/damage stat、vrest、held link、held random frame/Vy结果保持；
4. 不增加RNG、分配、candidate或authored ITR副作用。

## Deliverables

1. `ApplyKind16`唯一raw-writer替换，保留显式attacking assignment；
2. exact/shared kind16 raw-frame focused assertions；
3. Unity compile、full self-check、ledger validator与`git diff --check`的实际结果；
4. 完整Change Record/STATE/diff register/main plan/handoff；最高状态仅可为`RUNTIME_PENDING`。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 | C++ kind16 field/order与Unity canonical writer交叉复核。 |
| S1 | exact/shared PN/wait preserved，frame/Data mirror=200。 |
| S2 | explicit attacking=0仍成立；已有vital/stat/vrest/link/held contract保持。 |
| S3 | Unity compile=0 error、full self-check PASS、`pwsh` ledger validator与`git diff --check`通过。 |
| S4 | 仅`RUNTIME_PENDING`；C++ trace与Play Mode保持未关闭。 |

## Stop conditions

- source发现kind16有未读live same-tick prev/wait writer；
- 必须改global frame helper、projection、weapon、scheduler或scope外模块才可通过fixture；
- existing kind16 fixture在切换raw writer后暴露非frame contract的有效C++差异；
- 要求改C++ authority、C++ executable、既有CentralOnly/capacity/30Hz/FrameInputSet/对象池边界。

## Out of scope

`R4-HIT-02A`的再验证、`R4-HIT-02C`、`R4-HIT-02D`、`D-HIT-003`、R5～R8、T8、C++ executable、Unity Play Mode、服务器、Android、性能、render。

## 实施进度（2026-08-22）

- `ApplyKind16` 的唯一frame callsite已由`ImmediateFrame(MpDrain)`替换为现有raw writer，下一句
  `victim.AttackingCounter=0`保留在原顺序；
- existing actual/shared kind16 snapshot预置frame10、`PN=71`、`WaitCounter=17`，并断言frame/Data mirror=200、
  PN/wait保留、explicit attacking=0及既有lethal vital/stat/vrest/link/held结果；
- UnityMCP scripts refresh后filtered `error CS`=0；完整self-check结果为
  `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入2026-08-22 05:58:02 +08:00；
- post-check console仅有两条existing runtime-rest negative control；不是compiler或fixture failure；
- C++ trace、真实kind16 Play Mode和frame/presentation joint仍未执行，最高状态保持`RUNTIME_PENDING`。
