# R8-WP01G-R08-R03 — negative-height body geometry self-check classification

> 建立日期：2026-08-24  
> 状态：`VERIFIED / R-HC-01 CLOSED / LATER SELF-CHECK BLOCKER RECORDED`  
> Change ID：`R8-GEOMETRYCHECK-001`  
> Blocker：`R-HC-01`

## Goal

关闭恢复正式type0 DAT后`BattleRuntimeSelfCheck.CheckDeployableResolvedGeometryRisks`对5个
`w=21/h=-999` body的错误“unclassified non-positive shape”判定，同时保持C++ raw rectangle与Unity production
collision collector现有行为不变，使full self-check可以继续执行后续OID5152及其他检查。

## Scope

### 允许

1. 只读确认C++ release `hit.cpp`、`collision_collect.cpp`对负高度body的world rect与strict overlap语义；
2. 只读确认正式Unity DAT中负高度body的OID/frame/shape集合；
3. 只修改`BattleRuntimeSelfCheck.cs`的几何风险分类与focused self-check夹具；
4. 将当前已知`x39/y-555/w21/h-999/kind0`单独统计为raw inverted body，不与零宽line geometry混为一类；
5. 使用production `BruteForceSceneQuery`同时证明普通小itr不命中、跨越倒置两端点的大itr按C++ strict条件命中，
   并覆盖目标左右朝向；
6. fresh compile、聚焦测试、完整`BattleRuntimeSelfCheck`、必要的collision回归与ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不修改DAT、data.txt、parser、BodyBox/InteractionArea数据模型；
- 不在production碰撞链过滤、归一化、绝对值化或删除负尺寸geometry；
- 不把任意非正尺寸都无条件标为合法；负宽、零高、其他负高模式仍必须fail closed；
- 不修改角色技能、OID专项行为、candidate顺序、broadphase、hit、render、AI、T8或服务器代码；
- 不借此宣称C++ executable full trace已获得或整个战斗系统完全对齐。

## Authority / Evidence

- C++ `src/entity/hit.cpp:13-16,26-35,68-78`：body使用`bottom=top+local_h`，strict overlap为
  `ay1 < by2 && ay2 > by1`；没有正数化或过滤；
- C++ `src/entity/collision_collect.cpp:42-64,349-353`：union与exact body均保留raw尺寸语义；
- Unity `BruteForceSceneQuery.cs::IsReleaseBody/VerticalWorldRect/Overlap`：非null body全部参与，并采用同一raw
  `top+height`和strict overlap；
- 正式DAT：OID58 frames75/76与OID10 frames75/76/77均为`x=39/y=-555/w=21/h=-999`；
- 当前self-check结果`2026-08-24T01:33:25Z`只因`invalidBodyCount=5`触发R-HC-01；同报告列出的`itr w=0/h=79`
  已属于现有允许的zero-width positive-height line合同。

## Files likely involved

- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；
- `docs/ai/CHANGE-RECORDS/R8-GEOMETRYCHECK-001.md`；
- `docs/ai/CHANGE-LEDGER.md`、`docs/ai/STATE.md`、主计划与handoff。

## Unknowns

1. 完整self-check越过R-HC-01后是否暴露新的独立失败；若出现，按first-difference停止并另建Change；
2. 是否需要单独覆盖负高度itr；当前部署数据与失败证据没有该形态，不在本包预先扩大。

## Deliverables

1. 先失败后通过的倒置body production-collector断言；
2. 精确的`negativeHeightPositiveWidthBodyCount`与`otherNonPositiveBodyCount`分类；
3. fresh compile与完整self-check结果；
4. Ledger/STATE/main plan/residual audit/handoff同步。

## Verification

1. 部署数据仍必须包含5个确切的negative-height positive-width body；
2. `invalidBodyCount`必须只由这5个组成，其他非正body数量为0；
3. 既有zero-width positive-height itr数量保持大于0，其他非正itr仍为0；
4. 倒置body必须保持raw strict-overlap语义：普通小itr不产生candidate，包围两个倒置端点的大itr产生candidate，
   左右朝向一致；
5. full self-check越过R-HC-01并继续运行；若后续检查失败，报告新的first difference而不把本包误记为全量PASS；
6. Change Ledger validator PASS。

## Stop conditions

- 需要修改production collision、parser或DAT；
- C++ source显示负高度存在特殊归一化/分支，与当前推断冲突；
- 部署DAT出现除上述5个以外的新负高度/负宽/零高body；
- full self-check越过R-HC-01后出现scope外独立失败；
- 用户改变范围或拒绝该test-only修复。

## Out of scope

R1-WP02 full trace、AI策略、状态树/行为树、T8、Android、IL2CPP、服务器、渲染与性能架构。

## Authorization

用户已于2026-08-24明确批准执行`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001`并恢复目标。授权只覆盖
本Task中的test-only分类/夹具修改与既定验证；production collision、DAT、parser及其他模块继续禁止修改。

## Implementation update（2026-08-24）

- `CheckDeployableResolvedGeometryRisks`现分别统计zero-width itr、已知negative-height positive-width body、
  unexpected negative-height body与其他non-positive body；
- 只接受当前部署数据的5个`kind0/x39/y-555/w21/h-999`条目；数量、形状或其他non-positive body变化继续fail；
- 新夹具走真实`BruteForceSceneQuery`，验证普通小itr对倒置body左右朝向均不命中，而跨过倒置两个Y端点的大itr
  对左右朝向均按C++ strict条件命中；
- production collision、DAT、parser与其他脚本0改动。compile/self-check/回归尚未运行。

## Verification result（2026-08-24）

- fresh force-all compile：`Assembly-CSharp.dll 02:13:18Z`晚于source，Unity Console error=0；
- full self-check实际运行并越过R-HC-01；日志：137 definitions、82200 resolved frames、4389 itrs、13847 bodies，
  90个zero-width positive-height itr、5个known negative-height body、0 unexpected/other non-positive；
- ordinary/enclosing × right/left四个production collector断言均已执行通过，否则self-check不会进入后续检查；
- full self-check随后在独立`CheckMovementDatLoadingContracts`失败：仍硬编码已迁移删除的
  `Assets/NTSD/Config/AnimationConfig/Mingren/naruto.dat`；该失败发生在R-HC-01之后，不属于本包；
- 因此本包按source+compile+实际self-check证据VERIFIED，full self-check整体仍由新的资源fixture path blocker阻塞。
