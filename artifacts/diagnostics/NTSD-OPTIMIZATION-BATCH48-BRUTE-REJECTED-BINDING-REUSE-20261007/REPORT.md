# 第48批普通Brute拒绝方向绑定检查复用


## 当前结论

FOCUSED_TEST_PASS / LOCAL_SIGNAL_ONLY / NOT_ADMITTED。原13有效RED后实际22/22 PASS（新13＋旧9），在1000participant逻辑夹具平衡顺序4warm＋8sample比较中，本次mean21.3312625→20.8129125ms，差0.51835ms/2.4300%。只是一次具名局部信号，没有方差分布/真实1000AI/FPS因果证据；不把它说成方案错误或稳定收益，不因此独立扩成实景长窗口或默认推广。新flag仍false；原四生产默认、kind5 flag false与collector/backend不变。H07/H11仍OPEN，阶段4/6、限定产物5/6、34父关闭0，22—48共27已执行子批；Goal active，次数不停止目标。

## 实际修改与副作用

两声明文件从本批准确current-dirty副本追加：
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：51新增/6替换，只newopt-in及三per-collection诊断属性、ordinary targetOrdinal、coarse拒绝原资格门内ProbeBruteRejectedBinding、现有participant一个每build bool。初次合资格实际IsBound仍在原位置，之后同targetOrdinal/entity本collection复用。接受方向HasVrest、冷却值、kind5几何、pair顺序/body payload/handle/nearest与RNG不变；Role-aware及exact容量fallback不应用。
- Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：同文件FormalCollector fixture新增13case与2helper，原测试/断言保留。首次误放Shadow fixture导致缺helper CS0103/CS0246，纠正本包测试归属，不能称有效RED；后Console errorCS0才运行正式RED。

本次重扫：Query2333 ordinary loop、2461 exact-cache构建、5701 suppression、7257 pending谓词；Tracker321附近EnsureActiveBinding只验证store/token及失效清理，外部kernel RuntimeRestStore579 IsBindingValid只读身份/地址/token。Query record/nearest writes为候选/count/RNG，不变更rest owner。不能将这些静态事实升级为所有未来调用者/全项目线程安全证明；复用只限既有同collection缓存合同，不跨tick/collection/World，不缓存vrest判定。一个bool沿原fixed participant buffer/lifecycle，未新增owner/array/热扩容；逻辑元数据下界capacity×1byte，managed对齐/steady-transition/native/GPU全预算未知，不宣称已认证。

## 新鲜测试证据

原Editor PID19040/Unity2022.3.62f3/MCP6402，原Menu空闲窗口：
- RED job fdf1b3c288154b388e655ccf83741459，terminal failed，13 completed/13缺候选属性失败，test-red-final-01.json；result null，不虚构RED skip/duration。test-red-01.json是早期running快照，不能替代末结果。
- GREEN job ee32e28183294629b631d69ff6f2517b，22/22 PASS、0skip、3.4237206s，test-green-01.json。原9为geometry多body/nearest3、far vrest1、stale base gate2、kind5 outside1、kind5 capacity1、production defaults1；未重跑47/235/全历史。
- 新13：default OFF/四默认；8对象3攻击者21合资格拒绝访问降为8实际probe＋13reuse（vrest9保留）；同tick两种重收集均重新8/13；attack-exempt0probe；kind0/4/5多body/near vrest3；snapshot后人为失效绑定两资格场景首访问清理/不清理同原；容量不足回原不增长；Role-aware不应用；1000participant候选序列/handle/RNG一致和平衡局部cost。
- 局部cost输出原样：BRUTE_REJECTED_BINDING_REUSE_COST participants=1000 warmup=4 sample=8 baselineMeanMs=21.3312625 candidateMeanMs=20.8129125 scope=collector_fixture_not_AI_or_FPS gc=UNKNOWN。

这不是actual1000 AI/完整Driver/native对照，也不是热路径可靠0GC证书。没有本批Play/Profiler/GPU采集、M0或正式1800逻辑窗；原43有效12event FAIL不消去。最新38实景P9589.403/98.277ms/drop694/660/logicGC UNKNOWN保持，不声称新FPS。

## 保护及必要下一动作

terminal-source-audit-01.json实际2026-10-07T14:41:43.3823793Z：45guards、8准确cold副本、HEAD全同；两final C#身份364252B/SHA35436030...4D20D与290452B/SHA8EFCE96A...1E08D。原Menu单Scene8roots clean、Editor1791384102382 idle/nonPlay/noTest/noncompiling/errorCS0，editor-final-01.json。本包备份为治理PLANNED登记后的真实current bytes（before-current-01/backup-audit），before.json是前缀追加前manifest，不混称同版本。无Scene/资源/Settings/Server/Q06 body/Gen/Plugins修改，无删除/移动/恢复/破坏性Git/commit/push，未启动第二Editor。暂拒连接发生domain reload，只重新观察原PID/6402，未重启或重复测试。

下一在已批准ordinary Brute中评估“复用既有exact-cache参与资格”：本次重扫TryBuildBruteExactCache2474—2478已经每participant核null/PS/pending/suppression并以default.Entity null表达排除；当前pair loop又对每i/j重复相同检查。IsCollisionCandidateSuppressed只读SuppressCollisionCandidateUntilTick，IsPendingFlushDestroy只读PendingFlushDestroy；既有record链不写它们。由此可形成有据候选，先准确新Task/Change，再在useExactCache成立且cache同collection下保留原i/j方向顺序，fallback完全保留原checks；不换索引/collector、不重开H06/EXT1/ATLAS/Mono。不预设收益、不继续微调48或重复已有资格；若形成有意义信号再准确冻结完整Driver/真实窗口。

最终必要治理（2026-10-07T14:44:56.8834971Z）：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity 实际exit0，1344 Record/14 governed code/4257历史warning/0error；git -c core.safecrlf=false diff --check exit0/无消息，原件validation-final-01.json。没有新Script/Play/测量或Goal状态变更；45保护/8副本/HEAD核同。阶段未达仍active，不将22聚焦或局部2.43%当完成。

## 历史事前快照

PLANNED / NO_NEW_CODE_OR_GAIN。假设与13case冻结见Task；先RED再实现，局部无收益则不采用，不升默认/不扩大到Suite/真实窗口。H07/H11仍未达，Goal active。
