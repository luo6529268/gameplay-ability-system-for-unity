# 第87批 H07 producer 快照复用候选

当前 FOCUSED_TEST_PASS / UNITY_COMPILE_PASS / DEFAULT_OFF / PERFORMANCE_PENDING。H07仍未达到千人33ms阶段门；Goal保持active、选定阶段5/6、66已执行、34父项关闭0。此报告仅记录准确2C#候选，不是默认采用、千人收益或完整runtime验收。

## 原状与改动

85的1000AI ProducerSnapshotRefresh边界约0.95ms，只用来定位，不冒充本批OFF/ON收益。原RefreshCharacterInputSnapshotsAfterProducer每次先整份Runtime自复制，然后更新必要AI可见行。本批在World新增默认false开关及两per-pass int计数；仅exact LF2Character、AiControlled、既有ObjType/EntityType/Frame/WaitCounter/NextFrame current guard全部成立时省复制。测试producer mutation hook存在、未知派生或陈旧字段走原完整刷新。AI行刷新块852字符与dirty before相同，规则/cadence/RNG/pass不改。没有缓存、资源、queue、worker、lease或热路径扩容，关闭顺序无新增owner。

准确本批diff：SimulationWorld23增1删；CharacterInputLiveSlotLoopEditorTests252增0删（相对于本批before，而非HEAD）。去掉新测试块后原49测试全文内容不变。原dirty内容均保留；不触Q06方法体/Scene/Prefab/资源/settings/Gen/Plugins/Server，也不切Role默认。

## 实际执行

- 原Editor PID74724 / Unity2022.3.62f3 / TCP6402；无第二Editor启动。
- RED job a2e1660e1b60474ba12c0fb42312bd15：15/15因新API缺失失败，原XML SHA156148AA9077DF2320EF449DD0B82FF8DAD16923274CEB38C50E38D8EF79E736。人工16计数已以实际15纠正。
- GREEN01 job676dd50d14e44b89b9ff67e5d8de0278：64实际case/63PASS/1FAIL；新alias fixture的默认世界不允许ExtendedChecksum，未出现候选逐位差异。XML SHA1312A6484638F393B1106518B6B29C71E9F22E9232AA88B78C6FCA06828DD989，原失败保留。
- 仅新alias fixture的两个world修为DesktopExtended512，完整checksum及所有断言保留；GREEN02 job29b9be582dac4550be1f22b6ae48b3e2实际64/64 PASS/0skip（新15/旧49），duration88.7520286s，UTC22:40:39—22:42:15，最终XML SHA D4694E6304E8D61ED4716424EA7E6D60BCF8D6F09376EC440B158883903320D8与公共结果同。先实际终态，再记录PASS。
- World/test准确导入后原Editor生成新runtime+Editor DLL，idle/非Play/0CS；Menu sole clean/root8。一次错误Hashtable import返回Invalid JSON后准确JSON重试成功；World reload期间两次6402连接拒绝，原PID恢复；GREEN01同job一次Read超时，原job未重启而自然结束。
- git diff --no-index退出1表示上述预期内容差异，非测试失败；一次只读Runtime路径误定位已由rg --files纠正。所有文件编辑apply_patch；证据副本只Copy-Item至已确认不存在目标，不Force。

## 范围与未完成项

before递归800保护、10预期不存在路径、8当前before副本均核同无漂移，HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50/staged空。Menu SHA F2EE25D075548B78380BE817209152047B919C4C15DA8743D75D2E0C165CBEA1；Battle SHA253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010保持。Tools/Validate-ChangeLedger.ps1实际exit0/1382Records/15governed diff/4180既有warning/0error；最终治理复核结果另存validation.json及after-01，不把static证明当完整runtime证书。一次多文件治理patch因STATE首行link不同验证失败，未应用；重新逐个读取真实首行后仅本5治理header及原tracker三个H07行更新，历史原文不删除。

## 聚焦门与下一步

15新case覆盖默认关闭、精确current AI确有skip、OFF完整refresh、五非别名陈旧修复、未知派生仍走virtual、producer hook/human回退、Runtime读写别名及完整Extended一致、native/legacy各16输入tick的Extended/Lockstep/native scalar/legacy RNG/proxy一致、预热64+256局部委托0B。49旧case全部保持并通过。后续完整AI可见行更新不删、不调顺序，完整Driver/千人收益仍待；这里的native输入profile夹具不是当前权威native全域证书。

本批保持已有生产默认：new flag默认false，Role-aware默认推广未执行。H07继续候选实际准入/成本对比，不因本聚焦包完成停止Goal，也不重开H11选定CPU阶段。

最终UTC22:46:34（本地2026-10-09 06:46:34）复核：[validation.json](validation.json)，800保护/10absent/8before漂移0，HEAD同/staged空，最终Ledger exit0/1382Records/15governed diff/4180旧warning/0error；git diff --check exit0，仅原CRLF提示无空白错误。原Editor idle/非Play/非编译/Menu soleclean8/0CS。没有额外源码修改、测试、Play或性能测量；最终文件/输出SHA在Operation after-01.json。下一包继续H07候选实际资格/成本，Goal保持active，不以计数或此包PASS关闭H07。

本批没有Profiler/性能测量、1000AI OFF/ON窗、正式完整Driver逐tick或native全域证明；纯委托0B仅候选局部夹具。采用前必须扩展准确Suite冷接线范围、存新before、冻结同版四窗/恢复/可靠同步logicGC/checksum/关闭；没有可辨收益或actual skip0不推广。Role默认变更仍待人类准确授权，不因这项授权待定停止其它H07工作。H11选定CPU阶段已通过不重开，专项EXT1/ATLAS/Mono/Grid/Burst不解冻。
