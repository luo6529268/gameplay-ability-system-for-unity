# NTSD28-B4-F04-TYPE3-OID999-REFERENCE-001 — type3/OID999 reference physics

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-TYPE3-OID999-REFERENCE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4Type3Oid999ReferenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan PhysicsIntegrator28 type3, real OID999 and remaining-object tails; EXE B1E13AE1, closure 39DDDA15.
evidence: PREREQUISITE-COMPILE-RED-CS0117 / HIT_G-CARRIER-COMPILE0 / PARSER-GREEN1 / BEHAVIOR-RED6 / IMPLEMENTATION-CS0051-CORRECTED / FOCUSED7 / RELATED93 / BROAD498 / SELFCHECK-OLD-FIXTURES2-CORRECTED / SELFCHECK-PASS-2026-09-05T06:44:53Z / FINAL-BROAD498 / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / HIT_G_AND_REMAINING_NONCHAR_REFERENCE / TYPE0_PRODUCERS_AUDIO_PENDING`

边界来自 `NTSD28-B4-F04-TYPE3-OID999-AUDIT-001`。只迁remaining nonchar result core与
type3/OID999 single body；producer/type0/Audio/content不改。回滚见Task Contract。

首次导入 focused tests 后 Unity 编译报 `CS0117: LF2FrameData` 无 `hit_g`。该结果揭示正式
`BattleWorld28::step_physics` 从current frame读取的必需carrier在Unity完全缺失；它是实现前置
编译红灯，不冒充运行时红灯。本包已扩展声明范围到 `LF2FrameData` 与converter，仅接字段和
解析映射，不写DAT内容。

补齐carrier/converter后编译0 error；focused job `802057bff97443d087bc831dfd9ff519`
中parser 1项通过，6项behavior全部按预期失败，分别证明type3 helper、real OID999 +9 override、
ordinary type5 reference core与derived type3仍缺失。该6项是有效行为红灯。

首次实现编译暴露 `CS0051`（protected方法公开internal result参数）；该single body只需同assembly
shared/derived调用，访问级别收窄为`internal`，不扩大API或改变行为，待重新编译。

实现后focused7/7、related93/93、NTSD28 broad498/498；随后SelfCheck于
`2026-09-05T06:38:53Z`准确捕获PH-02仍断言旧OID999 y0/Vx0/Vy0的历史夹具。按正式
negative-reference `<-9` 场景更正为Y/Vx/Vy=+9并断言Vz保留；不修改production来迁就旧断言。

第二次SelfCheck在state4999 transformed fixture又捕获旧“crossed y0即frame101且全停”期望；
helper仅为该case增加可选reference/startY/Vz参数，改为正式`reference=-20/contact=-14`场景，
断言frame101、Y/Vx/Vy=+9、post-friction Vz=1。最终SelfCheck
`2026-09-05T06:44:53.8155651Z` PASS。

## 实际改动

- `LF2FrameData.hit_g` 与 `Lf2DatConverter` 映射闭合，raw property仍保留；未改DAT内容。
- shared与derived所有non-character都消费reference-aware mechanics；ordinary type5只走common core。
- 单一type3/OID999 body严格执行special state clamp、可选hit_g action/XYZ clear，再执行real OID999
  contactY `< -9` override；alias不触发，OID999只写Y/Vx/Vy=+9并保留当时Vz。
- 旧四参/legacy bool direct wrapper暂保留，不再定义production remaining-nonchar结果。

## 最终验证

- focused `6dc3ffd7dbd443889ecf85ecc5798a6a`：7/7。
- related `1d1a90459b4746c98ee1e22f353781ef`：93/93。
- final broad `9ad2230c66654912b9abe1c56626b95a`：498/498。
- SelfCheck `2026-09-05T06:44:53.8155651Z` PASS；Console 0 error。
- Scene `0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z` 未变化。

## 未关闭

type0 landing actions/environment damage、collision-Y producers、legacy owner retirement与B10 Audio仍待；
本记录不声明F-04/B4完成。
