<!-- CHANGE-RECORD
id: NTSD28-B9-NTSDSPEC-OSCILLATE-PRODUCER-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: SCOPED_BEHAVIOR_RETIREMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B9NtsdSpecOscillateProducerRetirementEditorTests.cs
authority: 用户2026-09-10恢复授权与复核方playable正向证明
evidence: RED_9_FAIL_1_PASS / FOCUSED_10_PASS / B6_610_PASS / REFILL_9_PASS / OLD_COMPAT_7_PASS / SELFCHECK_PASS / BUILDS_0_ERROR / SCOPE_UNCHANGED / REVIEW_PENDING
-->

当前结论：VERIFIED / SCOPED_RETIREMENT / REVIEW_PENDING。下方早期BLOCKED/PLANNED/未运行段落为已解除的过程历史，以最终验收节为准。

# NTSD28-B9-NTSDSPEC-OSCILLATE-PRODUCER-RETIREMENT-PRODUCTION-001

本合同在本轮任何脚本修改前建立。状态：PLANNED / PRECHANGE_SCOPE_BLOCKED / PRODUCTION_UNCHANGED_FROM_P3。
需求来源：用户2026-09-10批准三个独立退休包并限定文件范围，硬性要求既有测试保持PASS；Goal14 triage仅为定位线索，当前Authority需重新核验。
范围：仅删EffectCreate中旧OID→Oscillate producer；其他effect字段、ProcessEffects和snapshot恢复保持；不声明native B9 body-shake已对齐。
原状与实际路径：
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs

新增focused测试及meta已获授权，尚未创建；实施前须将实际测试路径登记code-path。无新runtime模块、无关闭阶段变化。
前置：包1沿当前playable caught/damage/settlement链正向证明无hurt consumer；包2/3完整repo caller/serialized/反射名称审查，发现生产caller即硬停。当前整体暂停源于旧测试范围冲突，见Temp/Goal17_PrechangeScopeReview.md；不得擅自修改未授权旧测试。
不变量：schema/Snapshot/Checksum/shell、NTSDSpec本体、DAT/converter、Scene/资源不动；不引入FrontHurtAct/BackHurtAct替代，不修改其他effect/impact/caught逻辑，不清理用户修改。
验收：新focused先RED后GREEN；三包一次共享B6(583+新增)、前置92/80/17/72/24/23/32/140、refill9、fullSelfCheck、双build0error、validator、固定Scene SHA/dirtyfalse。新测试失败与旧测试失败分开报告；本轮尚未运行上述检查。
副作用/兼容：仅改变各自退休行为；旧snapshot字段布局不变，但不能承诺旧行为续跑等值。外部程序集/API兼容与repo内部caller区别记录，不凭repo扫描断言不存在所有外部调用。
回滚：用户明确批准后仅反向本包增量，保留当前P1/P2/P3未提交基线。当前脚本增量为0，无需回滚。禁止Git清理/提交/push与跨包顺手修复。
当前验证：只读工作树、源码和Unity状态检查；未运行本轮RED/编译/回归/Play。没有报告新测试失败；这是确定的事前授权范围冲突。


## 2026-09-10 用户追加授权恢复
IN_PROGRESS / TEST_FIRST。用户确认恢复本Goal17三包；此前范围阻塞解除。新增测试路径：Assets/NTSD/Scripts/Test/Editor/NTSD28B9NtsdSpecOscillateProducerRetirementEditorTests.cs及meta。全批次一次共享回归；P3 Record保持已确认VERIFIED/USER_REVIEW_ACCEPTED，不再修改。
先完成全repo C#/序列化/反射字符串名审查，再创建RED测试；发现生产caller立即报告，不删动态caller。Luna只负责本包文件和新测试，主线程独占Unity测试/编译、共享文件、旧测试和最终审阅；RED实测完成前禁止改生产。

新测试13/4/10项已写入声明文件、生产未改。首次run_tests job44f849169f1a4db3aeead0bb43a5f6ed返回0tests（新脚本未导入），不计RED证据；已force all refresh，Editor csproj确认三个新Compile路径存在，等待真实RED。

删除前认证：Temp/Goal17_OscillateProducerAudit.md，精确EffectCreate仅生产声明；生产caller/序列化/反射精确名称引用0，BrokenEffectCreate与VisualEffectCreate独立分类。EffectNumToId常量300，focused使用num0/2对应旧OID300/302；保留ProcessEffects与snapshot恢复，不声明native±3摇动已对齐。

真实RED jobed786d2d624b4fc3a4096b8c78a2f5b3执行27项：包1=10FAIL/3PASS，包2=3FAIL/1PASS，包3=9FAIL/1PASS；全部控制通过，XML Temp/Goal17_RED_All.xml。type0实测230/232而期待220；type3实测310/320而期待0；Oscillate实测4/3而期望默认/原值。测试代码编译0error/104warnings，Unity已成功装载新测试。现在按合同开始生产最小删除。


只移除EffectCreate的7行efid/lookup/非零Oscillate赋值，原BOM保持；其他Effect字段/ProcessEffects/snapshot完全保留。


## 最终退休结论
已验证的范围为旧OID→Oscillate producer退休：LF2LivingObject.EffectCreate只删除7行efid/lookup/条件赋值，原BOM保持；num0/2映射旧OID300/302的写4/3行为归零，调用前的默认及非零Oscillate值保持。新focused10项RED9FAIL/1PASS→GREEN10PASS，同时验证优先级、Stuck、Dvx/Dvy、Num、TimeIn/TimeOut等字段。
LF2LivingObject中的NTSDSpec引用归零；全仓仍保留LF2Character mass及WeaponLinkResolver compat的2文件/5表达式，NTSDSpec.cs本体未改。ProcessEffects晚帧consumer及base-shell capture/restore读取保留；旧snapshot恢复的Oscillate仍可被晚帧consumer使用，reader退休等联合schema窗口。不声明B9 native render_phase±3 body-shake已对齐。本包没有自然producer caller，未执行Play；不以synthetic EffectCreate测试冒充自然Play。

## 本轮最终共享验收（2026-09-10）
三包独立Record共同引用同一次B6共享执行，不重复计算证据：
- 真实RED jobed786d2d624b4fc3a4096b8c78a2f5b3：27执行，22FAIL/5控制PASS；此前0tests job不计证据。
- focused GREEN job9d06f0c4cda5491e9a1c925967703fdb：27/27。
- 旧converter/HitPlan job2fd4f9c6ab3e41ee8687bee53e079a8b：7/7。
- 唯一B6共享 jobbc65a4f03fc743eeb146438659ed0d2d：610/610=583基线+27新增，含Goal11/12/13b/15/16的92/80/17+72+24/23+32/140及三新包13/4/10。
- refill jobac2482bc16ba42ba9be4ef4421e6428e：9/9。
- fullSelfCheck通过既有Temp请求文件触发，2026-09-10 06:02:12Z新鲜结果PASS，Temp/Goal17_Final_SelfCheck.result；未运行新Play场景。
- dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：0error/47warnings；Editor工程同命令0error/104warnings。Temp/Goal17_Final_RuntimeBuild.txt、Goal17_Final_EditorBuild.txt保存实际输出。
- Unity 2022.3.62f3 / b1b02287 / NTSD_Battle，isDirtyfalse/root13；Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
- Console7条均为既有负向registration/rest/release夹具，compiler0，本次MinMaxAABB0；不声称Console0或历史MinMaxAABB已修复。
- Temp/Goal17_Final_Integrity.json核验9209基线文件、1537保护项，无missing/越界/保护漂移；P3 Record字节未变，保持VERIFIED / USER_REVIEW_ACCEPTED。

名称审计见Temp/Goal17_FluteApiAudit.txt、Goal17_OscillateProducerAudit.md。根线程还补充扫描Assets/Packages/Tools全部相关C#/serialized/XML/YAML/JSON/meta/DAT/Lua/JS/TS文本，含Gen与Plugins（只读），Temp/Goal17_Final_AllSourceSerializedNames.txt：FluteForce仅新测试字符串；EffectCreate仅保留生产声明和新测试调用。无新增生产caller。对应Gen/Plugins并未修改，补充扫描不以删除caller制造dead。精确名字/变体/反射字面量的repo静态认证不等于外部动态构造调用的全局证明。

实际code改动：7个production、3个获批旧测试、3个新focused及meta；未修改schema/Snapshot/Checksum/shell数据格式、DAT/converter/Scene/资源/NTSDSpec/P3 Record。最终10个既有脚本的完整diff见Temp/Goal17_Final_ProductionAndExpectations.diff；XML见Temp/Goal17_RED_All.xml、GREEN_All.xml、Shared_B6.xml、OldCompat.xml、Refill.xml，汇总见Temp/Goal17_Final_RegressionSummary.json。
回滚仍需用户明确批准，仅反向对应包增量；无git add/commit/push，无清理或恢复用户文件。报告后停止等待复核，不启动drain/recovery、mass/compat、native B9或联合schema后继。

最终交付门实测：Tools/Validate-ChangeLedger.ps1 PASS（449 records、13 governed code files），完整Temp/Goal17_Final_Validator.txt；git diff --check PASS（仅既有CRLF提示）。最终Scene SHA再次相同，P3 Record字节再次核验不变。三包到此停止等待复核。
