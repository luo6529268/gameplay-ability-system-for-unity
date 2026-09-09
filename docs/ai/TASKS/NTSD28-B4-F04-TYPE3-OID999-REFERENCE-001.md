# Task Contract — NTSD28-B4-F04-TYPE3-OID999-REFERENCE-001

> 状态：`VERIFIED / HIT_G_AND_REMAINING_NONCHAR_REFERENCE / TYPE0_PRODUCERS_AUDIO_PENDING`

## 目标

建立shared/derived共用的type3 special-state与real-OID999 physics tail，并将剩余non-character
production owner迁到reference-aware core。

## 不变量

- type1/2/4/6已验证landing body不变。
- type3 helper先于OID999 override；hit_g action不重置counter，OID999 action101必须重置。
- OID999只认real `ObjectId`，不认alias/type_sub；Vz按正式顺序保留。
- type5普通contact只运行common core，不发明action/clamp。
- legacy direct wrapper暂不删除；producer/type0/Audio/content/Scene/Authority不改。
- 只新增当前正式physics已读取的frame `hit_g`强类型carrier与converter映射；不修改任何DAT值。
- 更正SelfCheck中仍固定旧y0/全停OID999结果的历史断言；只改夹具与期望，不为测试改变production。

## 验收

先以parser/converter测试闭合`hit_g`；随后test-first覆盖type3 hit_g/non-hit_g、type3+OID999 override、type5 OID999、alias999 noop与
negative-reference ordinary type5；编译、focused/related/broad/SelfCheck/Scene/Ledger闭合。

## 回滚

删除focused test和single body，恢复remaining nonchar legacy bool分支；无数据迁移。

## 验证结论

- prerequisite compile red：`LF2FrameData.hit_g`缺失；carrier/converter补齐后编译0。
- parser 1 green + behavior red6 job `802057bff97443d087bc831dfd9ff519`；实现后 focused `6dc3ffd7dbd443889ecf85ecc5798a6a` 7/7。
- related `1d1a90459b4746c98ee1e22f353781ef` 93/93。
- 首轮 broad `9164f9c0535c491ab8aa725ad0617c2e` 498/498；两条旧SelfCheck OID999夹具依次被捕获并按Authority更正，最终 SelfCheck `2026-09-05T06:44:53.8155651Z` PASS。
- SelfCheck更正后的最终 broad `9ad2230c66654912b9abe1c56626b95a` 498/498；Scene不变、Console 0、Ledger PASS。
