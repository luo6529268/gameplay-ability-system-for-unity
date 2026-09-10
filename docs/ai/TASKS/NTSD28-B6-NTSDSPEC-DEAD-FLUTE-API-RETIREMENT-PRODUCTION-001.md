# NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-RETIREMENT-PRODUCTION-001

本合同在本轮任何脚本修改前建立。状态：PLANNED / PRECHANGE_SCOPE_BLOCKED / PRODUCTION_UNCHANGED_FROM_P3。
需求来源：用户2026-09-10批准三个独立退休包并限定文件范围，硬性要求既有测试保持PASS；Goal14 triage仅为定位线索，当前Authority需重新核验。
范围：删除无repo调用的旧FluteForce实现和空override；impact正式链保持；PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER。
原状与实际路径：
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs

新增focused测试及meta已获授权，尚未创建；实施前须将实际测试路径登记code-path。无新runtime模块、无关闭阶段变化。
前置：包1沿当前playable caught/damage/settlement链正向证明无hurt consumer；包2/3完整repo caller/serialized/反射名称审查，发现生产caller即硬停。当前整体暂停源于旧测试范围冲突，见Temp/Goal17_PrechangeScopeReview.md；不得擅自修改未授权旧测试。
不变量：schema/Snapshot/Checksum/shell、NTSDSpec本体、DAT/converter、Scene/资源不动；不引入FrontHurtAct/BackHurtAct替代，不修改其他effect/impact/caught逻辑，不清理用户修改。
验收：新focused先RED后GREEN；三包一次共享B6(583+新增)、前置92/80/17/72/24/23/32/140、refill9、fullSelfCheck、双build0error、validator、固定Scene SHA/dirtyfalse。新测试失败与旧测试失败分开报告；本轮尚未运行上述检查。
副作用/兼容：仅改变各自退休行为；旧snapshot字段布局不变，但不能承诺旧行为续跑等值。外部程序集/API兼容与repo内部caller区别记录，不凭repo扫描断言不存在所有外部调用。
回滚：用户明确批准后仅反向本包增量，保留当前P1/P2/P3未提交基线。当前脚本增量为0，无需回滚。禁止Git清理/提交/push与跨包顺手修复。
当前验证：只读工作树、源码和Unity状态检查；未运行本轮RED/编译/回归/Play。没有报告新测试失败；这是确定的事前授权范围冲突。


## 2026-09-10 用户追加授权恢复
IN_PROGRESS / TEST_FIRST。用户确认恢复本Goal17三包；此前范围阻塞解除。新增测试路径：Assets/NTSD/Scripts/Test/Editor/NTSD28B6NtsdSpecDeadFluteApiRetirementEditorTests.cs及meta。全批次一次共享回归；P3 Record保持已确认VERIFIED/USER_REVIEW_ACCEPTED，不再修改。
先完成全repo C#/序列化/反射字符串名审查，再创建RED测试；发现生产caller立即报告，不删动态caller。Luna只负责本包文件和新测试，主线程独占Unity测试/编译、共享文件、旧测试和最终审阅；RED实测完成前禁止改生产。


最终状态VERIFIED / SCOPED_RETIREMENT / REVIEW_PENDING。已验证的范围为无repo生产caller的FluteForce API退休：删除LF2Entity完整方法及LF2WeaponBase空override，未把impact规则迁回旧virtual。新architecture4项RED3FAIL/1PASS→GREEN4PASS，reflection base/inherited方法均不存在，production source引用归零，既有impact owner与独立character mass owner保持。
PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER。仓库外预编译程序集不由repo扫描证明；本次runtime/editor编译和目标回归通过。NTSDSpec本体和mass/compat生产行为未改。早期审计中的969-994是旧定位简写，最终diff删除完整方法及相邻自身注释，下一SetPos文档原1002起保持；没有截断函数。
共享B6610、refill9、oldcompat7、fullSelfCheck、双build0error和固定Scene/范围检查已通过，完整证据见同IDRecord最终节。报告后停止。
