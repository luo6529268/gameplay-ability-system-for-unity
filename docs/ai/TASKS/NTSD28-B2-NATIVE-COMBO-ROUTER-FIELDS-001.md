# Task Contract — NTSD28-B2-NATIVE-COMBO-ROUTER-FIELDS-001

> 状态：`FOCUSED_TEST_PASS / FRAME_FIELDS_AND_SELECTOR_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

补齐`hit_aj/hit_ad/hit_jd` frame/parser字段，并建立只读选择native combo10字段的pure selector；显式
consume只在调用者确认authority“attempted”后清理exact combo attempt。

## 允许修改

- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeComboRouteSelector.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboRouteSelectorEditorTests.cs`（新增）及meta
- 本Task/Change/Ledger/STATE/handoff/总表

## 不变量

- selection priority精确为`Fa/Fj/Ua/Uj/Da/Dj/aj/ad/ja/jd`；不得用legacy 9-combo顺序替代。
- 横向facing只随combo0/1给出，且action字段为0时调用者不得改朝向。
- selector不得修改runtime/frame/HP/PP/facing；consume必须显式调用。
- consume复用native clear合同：清BE..C4对应六edge、defend reentry、combo10、五键history；保留下行
  edge、current/previous和proxy tail。
- 不实现`hit_ja`特殊family副作用、不迁移human/AI producer时点、不接production调用。
- 不修改`Assets/NTSD/Config`、DAT、Scene、ProjectSettings、Packages或权威目录。

## 验收

1. test-first因三个frame字段与selector类型缺失而红；
2. parser精确保留三个字段；
3. 十个combo单项映射、全量priority、zero-field/facing边界通过；
4. explicit consume和未consume不变性通过；
5. warmed selector零分配；compile、focused/related、SelfCheck、Console0、Ledger通过。

## 回滚

删除新增selector/test及meta，移除三个新增frame/parser字段；Unity Config不受影响。

## 结果

test-first 13项缺失类型/字段错误；final focused17/17、相关42 pass+1 explicit skip、08:21:06
SelfCheck PASS、Console0、4096 warmed zero-allocation。production与Config均未连接/修改。
