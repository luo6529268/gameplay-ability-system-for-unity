# R7-AI-02F — full 1–39 dispatcher integration

> 日期：2026-08-23
> 状态：`RUNTIME_PENDING / CODE + AUTOMATED EVIDENCE CLOSED`

## Goal

以C++ release `prepare_ai_input`为唯一行为权威，在一次原子变更中把02C～02E的unwired module与
positions38/39接入Legacy和DataOriented两条production路径，形成outer random gate内完整1–39固定顺序，
并把现有positions28/38/39从gate外移回gate内。

## Scope

- positions1–6由现有helper保持原体，只把compound OR改为顺序可观测dispatcher；
- positions7–37调用02C～02E同一实例module；
- positions38/39移入module并只在outer gate内执行；
- `AiDecisionSnapshot`持久拥有module，不在tick/AI evaluation中`new`；
- DataOriented kernel和Legacy共享同一module逻辑、相同scan adapter与RNG公式；
- Legacy使用pass级shared rows；不可用时只在随机边界内使用持久fallback snapshot，不得回退旧缩减链；
- Legacy module RNG通过value stream执行后恢复`DeterministicRng`，并把预分配trace逐项追加到现有shadow；
- module side-effect即使不early-return也必须提交回canonical/runtime input；
- witness新增matched position并进入Full/Indexed比较；scan row visits纳入既有诊断；
- 删除/停用kernel与Legacy gate外positions28/38/39调用，确保只消费一次RNG。

## Authority / Evidence

- C++ dispatcher：`J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp:2055-2204`；
- helpers：同文件`120-1179`；
- `MAX_OBJECTS=400`：`include\game_world.h:13`；
- 02A source-derived 39-position/outer-gate red witnesses；
- 02B HitJ data contract；02C/02D/02E focused 19/19、26/26、31/31 unwired modules。

## Ownership and adapter contract

1. `AiDecisionSnapshot.CharacterDecisionModule`是snapshot生命周期内的持久owner；复制owned state不复制/替换owner。
2. Legacy优先使用当前pass的`aiDecisionSharedSnapshot.Rows`；shared rows对Legacy也成为必备pass产品。
3. shared rows不可用时使用独立持久`aiCharacterDecisionLegacyFallbackSnapshot`；捕获失败不得在已消费RNG后静默继续。
4. position21 global scan：
   - self slot 0..399：`min(400, rows.Capacity)`，额外Unity实体不改变C++ authority domain；
   - self slot >=400：Unity adapter扫描完整`rows.Capacity`，保证扩展实体可参与Unity扩展域；
   - 该adapter是Unity扩展规则，不反向定义C++。
5. position30/34仍固定first20/first100，不随capacity扩展，因为源代码本身是显式常量域。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ai/AiCharacterDecisionModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiDecisionShadow.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiDecisionAuthorityChainContractEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiCharacterDecisionFullDispatcherEditorTests.cs`（new）
- existing AI tests only if source-correct expectations require explicit supersede。

## Deliverables

1. module full dispatcher positions7–39与positions38/39 source-derived helpers；
2. kernel outer-gated1–39顺序及matched-position witness；
3. Legacy shared-row/input/RNG bridge与atomic removal of gate-out helpers；
4. 02A red witnesses转为ordinary green tests；
5. fixed-seed kernel/Legacy/profile-pair/adapter/0B fixtures；
6. compile、AI regression、fresh self-check、ledger证据与handoff。

## Verification

- source table仍为39 positions且dispatcher matched position准确；
- 02A position7 expected DRJ=3和position28 outer-gate-miss expected DUA=0均PASS；
- positions38/39 outer-gate hit/miss、RNG calls/order与early-return PASS；
- Legacy与DataOriented对代表OID/seed的input、RNG state/calls/order、matched position一致；
- FullScan/Indexed witness含position并一致；
- self<400与self>=400 scan adapter fixtures PASS，first20/100保持固定；
- module、Legacy bridge和kernel warmed path 0 B；
- existing AI regression、full self-check与validator PASS；
- Play Mode/C++ runtime trace仍如实标pending。

## Stop conditions

- 需要改变outer gate以外的common tail、input edges/cooldown时点或pass顺序；
- shared/fallback snapshot无法在随机边界内提供source-required rows；
- Legacy与DataOriented不能在同seed下闭合input/RNG；
- 需要新增C++未定义的gameplay字段或改变capacity总体策略；
- 需要修改C++、render、collision或physical input binding。

## Out of scope

- 真实角色Play Mode/R8；
- C++ executable/trace；
- AI策略重写、行为树、难度重平衡；
- first20/100/global scan索引性能优化；
- R7其他difference repair packages。

## Execution result

- Legacy与DataOriented已原子接入outer-gated positions1–39；positions28/38/39不再在gate外重复执行；
- snapshot持久拥有module；Legacy使用pass级shared rows，并在共享产品不可用时使用构造期预分配的
  fallback snapshot；随机边界后捕获失败会hard-fail，不会回退旧缩减链；
- matched position已加入Full/Indexed/Legacy shadow比较，module RNG逐项进入原有预分配trace；
- self slot 0..399保持C++ 400域，self slot >=400使用Unity完整capacity；first20/100保持常量域；
- shared row对`InputHistory`改为只读，不再通过`HasInputHistoryGate()`隐式创建数组；
- source-derived authority witness 3/3、full dispatcher 5/5、fixed-seed production profile-pair 1/1、
  AI相关矩阵286/286、warmed dispatcher 0 B、Unity compile 0 error、2026-08-23 02:07:58
  fresh-domain full self-check PASS；
- 真实角色Play Mode、C++ runtime trace与R8联合场景仍未完成，因此状态只能是`RUNTIME_PENDING`。
