# NTSD28-B2-NATIVE-COMBO-ROUTER-FIELDS-001 — exact combo route field contract

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-COMBO-ROUTER-FIELDS-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeComboRouteSelector.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboRouteSelectorEditorTests.cs
authority: NTSD 2.8-Logan input_routing.cpp route_combo_fields/clear_combo_attempt and skill entry/runtime corpus route field mapping.
evidence: TEST-FIRST-13-CS0246-CS0103 / COMPILE-0 / FOCUSED-17-OF-17-12CCB6B6 / RELATED-42-PASS-1-EXPLICIT-SKIP-56A2C9FF / SELFCHECK-PASS-2026-09-03-082106 / CONSOLE-0 / AUTHORITY-ROUTE-PRIORITY-10 / WARM-4096-ZERO-ALLOC / UNITY-CONFIG-UNCHANGED / PRODUCTION-UNCONNECTED
-->

> 状态：`FOCUSED_TEST_PASS / FRAME_FIELDS_AND_SELECTOR_READY / PRODUCTION_UNCONNECTED`

本包只建立缺失frame字段、parser与pure exact route selection/attempt-consume合同；不接production调用。

## 实际改动

- `LF2FrameData`新增`hit_aj/hit_ad/hit_jd`及`Hit["aj"/"ad"/"jd"]`镜像。
- `Lf2DatConverter`按不区分大小写的现有规则解析三个字段。
- 新增allocation-free `NTSD28NativeComboRouteSelector`，按
  `Fa/Fj/Ua/Uj/Da/Dj/aj/ad/ja/jd`固定优先级返回field/action/facing decision。
- selector纯读；`ConsumeAttempt`是独立显式调用并复用已验证的native clear边界。`hit_ja`特殊family
  副作用未在本包实现。

## 验证

- test-first刷新得到13项预期`CS0246/CS0103`；实现后最终job
  `12ccb6b6c8cf4ba385897c6b649e2a0e`为17/17。
- 相关native combo/parser/content job `56a2c9ffa2c3437aa16fab3b94c9efcc`为42 pass、0 fail、
  1项artifact capture explicit skip。
- parser三字段、十路单项映射、全量priority、zero-field不消费、显式consume、下行edge/current/
  previous/proxy-tail保留及4096 warmed zero-allocation均通过。
- 完整SelfCheck于`2026-09-03 08:21:06 +08:00` PASS；清空预期负例后Console error为0。
- `Assets/NTSD/Config`、Scene、ProjectSettings、Packages和权威目录无本包diff。

## 未关闭

- production尚未调用selector；native combo仍未成为human/AI真值。
- `hit_ja`特殊family route、generic action attempt/apply、producer时点迁移和联合trace均属后续包。
- Unity正式Config仍无三个字段值；B11内容策略未决定，本包没有导入权威内容。
