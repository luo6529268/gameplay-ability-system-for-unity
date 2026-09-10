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


最终状态VERIFIED / SCOPED_RETIREMENT / REVIEW_PENDING。已验证的范围为旧OID→Oscillate producer退休：LF2LivingObject.EffectCreate只删除7行efid/lookup/条件赋值，原BOM保持；num0/2映射旧OID300/302的写4/3行为归零，调用前的默认及非零Oscillate值保持。新focused10项RED9FAIL/1PASS→GREEN10PASS，同时验证优先级、Stuck、Dvx/Dvy、Num、TimeIn/TimeOut等字段。
LF2LivingObject中的NTSDSpec引用归零；全仓仍保留LF2Character mass及WeaponLinkResolver compat的2文件/5表达式，NTSDSpec.cs本体未改。ProcessEffects晚帧consumer及base-shell capture/restore读取保留；旧snapshot恢复的Oscillate仍可被晚帧consumer使用，reader退休等联合schema窗口。不声明B9 native render_phase±3 body-shake已对齐。本包没有自然producer caller，未执行Play；不以synthetic EffectCreate测试冒充自然Play。
共享B6610、refill9、oldcompat7、fullSelfCheck、双build0error和固定Scene/范围检查已通过，完整证据见同IDRecord最终节。报告后停止。
