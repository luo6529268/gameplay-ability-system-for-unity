<!-- CHANGE-RECORD
id: NTSD-H06-ROLE-SCOPED-ADMISSION-001
status: BLOCKED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs
authority: 2026-10-09 explicit user H06 scoped Role admission; D1=A; formal336B44 unchanged
evidence: docs/ai/TASKS/NTSD-H06-ROLE-SCOPED-ADMISSION-20261009.md
-->
# H06 现有Role限定准入

收尾释放测试2ed7ca384e344d21adb7a3a276e2f36b失败：Profiler非idle。本次UnityTest在旧历史导出错误后未完成finally释放，不能声称已原样恢复设置。同文件新增显式H06RoleAdmission_ReleaseOwnedFailedProfilerCapture，仅清理本次已知开启的Profiler/driver录制并将本次写入的profileEditor/allocationCallstacks复位false；此前RequireAvailable证明录制/deep/GPU起始false，但profileEditor/allocationCallstacks原值未持久化，明确不能声称两项精确恢复。保存前后快照，断言deep/GPU未被他人接管，不清空历史。调用栈诊断增补事前持久化settings和导出后显式错误门，防止再次在坏历史后开启录制；保持Explicit，不重试调用栈。先恢复原Editor安全状态再交付。

调用栈job30d28fef30a6461bb778a62da7934c8b失败于导出旧Profiler历史：Unity报告frame6大于2GB；prior-history.raw是部分输出，不是完整备份，不作有效调用栈证据。未清空旧Profiler历史，未重试长窗。收尾仅新增H06RoleAdmission_EditorReleasedAndDefaultStillBrute，检查既有Profiler空闲前置、测试flag已释放、resolver空值仍Brute并保存读出设置；不修改Profiler设置。调用栈及phase诊断标Explicit防止普通全量测试意外运行；原0GC失败测试保留，不以Explicit隐藏准入失败。

qualification04已定位：Role snapshot0/collect234/end0，Brute collect记录器256饱和（非精确计数），其余0；非Role独有证据。下一同Admission文件新增H06RoleAdmission_OPointCollectorAllocationCallsites（UnityTest），只对已失败合成形状启用闲置Editor CPU profiler/callstacks，并在诊断collect放具名marker；复用既有Profiler设置读取/恢复/历史备份方法，finally恢复设置和测试静态flag，保存新UUID artifact。无生产hook/缓存/行为修改，不读Q06方法体；该实验只回答共同收集分配调用点，不能作收益或0GC通过。若原Profiler正在使用则拒绝，不抢占。新静态flag仅Editor测试owner、bool容量固定、finally失效，不接入Runtime十一阶段。

qualification03实际4PASS/1FAIL：OPoint tick1受控生成32tick配对通过，三个形状collector校准GC 8/8零；OPointBurst-shaped 8样本各234 GC.Alloc events，前后正空校准有效、未饱和，unit TimeNanoseconds不可当bytes。默认保持Brute，Suite未改，不进入收益窗口。下一同Admission测试增加phaseDiagnostic显式case，只对失败形状做Role/Brute的snapshot/collect/end三段分配定位；4warm/8sample不变，保存raw，诊断完成不等于0GC准入，未通过原门不得切默认。原结果保存qualification-03-gc-fail。

qualification02实际3PASS/1FAIL：D/C/Concentrated完整32tick配对通过。OPoint新增身份断言发现tick8目标slot984为自然生成SpecialAttack；旧raw亦显示tick2..7实体984→990→999，故事前“tick7仍984”的夹具假设错误，并非后端首差。修改仅OPoint测试排队时点为首tick前，先断言16槽为空，tick1断言生成1000及正确身份；仍真实Driver flush与后续32tick，不取消身份断言、不改AI/生产。原XML/源码保存在qualification-02-opoint-type-fail，后续不重跑三个未受影响PASS。

脚本前登记 IN_PROGRESS。原状：空配置Brute，Role仅显式选择；25/81/82证据各有准确局部范围。准确路径/符号、不变量、容量/owner/十一阶段关闭、风险、最小验证与回滚见Task。

先测试缺口，任何失败不切生产默认。无新战斗行为合同；输入/RNG/checksum/33ms/pass/消费顺序/publication只读/透明顺序/fail-closed不变。条件默认改动尚未实施，不将计划路径写成已改事实。

后续准确符号事前登记：Admission.H06RoleAdmission_FourShapeCollectorZeroGc；Suite.BeginH06ScopedRoleAdmission、BuildH06ScopedRoleRequest、H06ScopedRoleQueryValid及既有BeginSuite/BuildCurrentRequest/CurrentPostBindingsRoleQueryValid/StartRun观察接线。目的/范围/owner/GC校准/最小验证与回滚见Task受影响0GC门细化。原项目当前四场景测试结束前不写入或导入脚本。

备份及命令审计：docs/ai/FILE-OPERATIONS/NTSD-H06-ROLE-SCOPED-ADMISSION-20261009/RECORD.md。当前尚无新增编译/聚焦/运行/收益证据。H06/H07开放，Android未认证。

CODE_WRITTEN：唯一Admission测试脚本新增H06RoleAdmission_ConfiguredFourSceneDriverParity/RunH06Admission/H06AdmissionRow；已有WithRoleDriverScenario仅末尾可选backend、PrepareThousandAi仅末尾可选participantCount，原调用默认保持。四形状32tick、候选store只读hash/完整输入与双RNG/checksum、容量引用、原有序关闭；生产默认及Suite未改。尚未编译/执行。

原jobedb86e0ab3c64c1db570e09f9e9dad33实际4/0/4：四场景256完整step状态/hash/输入/双RNG同、关闭和3数组/容量通过，全部失败在新探针candidateCount=0；Manual默认LegacyOnly而探针只读store，非生产首差。四UUID原件和失败XML/源码已保存qualification-01-observer-fail。修订仍同包/同测试文件：只读实际legacy或store来源，每tick一次捕获断言、OPoint slot身份及queue逐tick比较、95默认OFF守卫；不改生产collector/default/资源，不删除断言或旧结果。随后只复核受影响四case。

qualification03编译：唯一Admission文件18:46:50，原Editor程序集18:46:58，reload完成/idle/非Play、Console error0后启动job6bd9788e9b0f424cbb8855f0ac642b27，只选OPoint复核及四形状collector GC共5case。未运行已通过三个Driver场景。

收尾：BLOCKED / GC_GATE_FAIL / NO_DEFAULT_PROMOTION。唯一实际脚本Admission测试；条件Profile/Suite未修改。完整当前结果、失败原件、命令、复用/未执行门与H07-INPUT建议见artifacts/diagnostics/NTSD-H06-ROLE-SCOPED-ADMISSION-20261009/REPORT.md。33ms非本子阶段阻塞。

元数据收尾校正：条件Profile/Suite从实际code-path清单移除，两者未改；事前条件范围仍在Task/本Record历史中保留。

## 最终检查（2026-10-09）

原Editor最后程序集19:08:37晚于源码19:08:23，reload完成；job0e10a3458abe4d9eba4b70014af23cab通过1/1（0.596s），H06RoleAdmission_EditorReleasedAndDefaultStillBrute确认Profiler空闲、测试flag释放、空配置resolver仍Brute。最终XML/源码见qualification-08-final-release-pass，Profiler读出见editor-release-and-default UUID JSON。原Menu唯一8roots/clean、非Play/非Paused/无测试/无编译；没有第二个同Library实例，没有保存Scene。

最终closeout-audit-final.json：1150保护项仅Admission测试脚本变化，SHA09B9204AA87BA4956210C10C2B5CBCDF1241A332DDBE1745F952DFB3ABC85C15；15份before备份全同。Profile仍475BD0BF6FF0D83337DB8BB3BEDEF06D30329E4B5118B5662EF4056F79A3F1CB，Suite仍DF63DFB1A4C01BAF32672022B22AA089EE9D59006822EE8C3867CFCD26188013。

实际命令`./Tools/Validate-ChangeLedger.ps1`最终exit0 / PASSED，1391 Records、9 governed diff files、0 errors、4265非阻断warnings（主要历史记录路径不在当前diff，未做无关清理）；原日志change-ledger-validation-final.txt。`git diff --check -- <本任务脚本及治理路径>`exit0，仅LF/CRLF提示，没有空白错误。未执行git add/commit/push、删除/恢复/重置。

GC失败后新增仅阶段/调用栈诊断与释放检查，未改变通过的Driver比较逻辑、collector准入阈值及生产字节，所以未重复三场景/OPoint一致性或已PASS三个GC门。后续收益/default未启动的原因是必要GC门失败，不是33ms。
