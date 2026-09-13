# Q03 碰撞几何双端见证结果

日期2026-09-13。Change `NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001`。状态 **VERIFIED_CAPTURE_ONLY / PARITY_DIFFERENCES_CONFIRMED / PRODUCTION_UNCHANGED**。

共享14组DAT/位置夹具实际调用当前native源码与Unity生产候选收集器，**42项比较中15相同、27不同**。三种Unity模式各有9/14用例不同。此处确认了需处理的规则/数据差异；没有修改生产逻辑，也没有宣称B5或Q03已关闭。

## 实际输出

所有行均为候选数量。Unity三列分别为ForceBruteForce、ForceLegacyUnionAabb、ForceRoleAware；RoleAware明确设置ForceRoleAwareDirectForDiagnostics=true，不构成所有密集场景/并行kernel/cache路径的证书。

| 用例 | native | Unity三模式 | 结论 |
|---|---:|---|---|
| inside：Z=14，默认宽度 | 1 | 1/1/1 | 相同 |
| positive_edge：Z=15 | 1 | 0/0/0 | native包含深度端点，Unity排除 |
| negative_edge：Z=-15 | 1 | 0/0/0 | 同上 |
| outside：Z=16 | 0 | 0/0/0 | 相同 |
| explicit_zero：ITR zwidth0，Z=15 | 1 | 0/0/0 | 零值在native投影时回退15；端点差异仍在 |
| negative_width：ITR zwidth-1，Z=0 | 0 | 1/1/1 | native仅零回退，Unity将负值也改成15 |
| body_depth_edge：BDY zwidth5，Z=20 | 1 | 0/0/0 | body深度参与native半径和，Unity内容未保留且query不消费 |
| body_depth_inside：同上Z=19 | 1 | 0/0/0 | 不仅是端点差异 |
| body_z_ignored：BDY z999，Z=0 | 1 | 1/1/1 | native忽略BDY z；不能盲给Body加z偏移效果 |
| itr_z_edge：ITR z10，Z=25 | 1 | 0/0/0 | native攻击中心加ITR z |
| itr_z_inside：同上Z=24 | 1 | 0/0/0 | ITR偏移缺失独立于端点 |
| xy_edge：X相接但不重叠 | 0 | 0/0/0 | 2D仍使用严格重叠，不得将所有比较统一改为包含端点 |
| missing_body_x：BDY省略x | 0，diagnostic1 | 1/1/1 | native无完整几何不生成box；Unity缺失填0并继续 |
| zero_body_width：显式w0且位于攻击内部 | 1 | 1/1/1 | 显式零宽度不等于缺失；不能用w>0过滤修复上项 |

原始输入、位置与SHA见fixtures/cases.tsv及fixture-manifest.json；实际输出native.tsv/unity.tsv；逐项比较comparison.json。全部29个fixture输入hash验后无漂移。native第二次输出native-rerun.tsv与第一次相同，SHA `210DE5C691B0E53AC895727D0E0CB8B5AD90DCF200196E4D9E0E5DBBB646F63A`。

## 权威与生产调用层级

正式EXE身份经build脚本强校验，hash见native-build-identity.json。native源码读取自权威目录，未修改；诊断程序调用真实DatParser及HitCandidateBuilder28::append_pair_geometry。该函数在正式battle_world.cpp:4356被调用，随后按其overlapping_candidates进行选择。collision_geometry.cpp进行frame center/朝向/X/Y/Z投影和重叠，hit_candidates.cpp使用target BDY逐块生成候选。

Unity实际路径：ParseLoganContent -> ConvertToFrameData -> FrameCache -> SimulationWorld.CaptureCollisionFrameSnapshotsAll -> CollectCollisionCandidatesAll -> BruteForceSceneQuery正式collector -> TryGetCollisionCandidateSequence。没有手写几何公式冒充生产结果。

已定位实际差异入口：Converter.ConvertToBodyBox丢失BDY zwidth与geometry presence；InteractionArea缺省15在内容与投影层混合；RecordOverlappingBodyCandidates/RecordOverlappingBodyCandidatesCached/HitsTargetCached采用正值判断、严格深度边界且不加ITR z。Role/union AABB构建也必须纳入后续审查，不能只修最终一处比较。

这不是正式EXE的实时战斗录制；Unity为真实Editor中非Mono逻辑world的定向收集测试，未进入Play。伤害消费、held weapon strength override、dense/cache/parallel fast path及整场规则尚未由这些普通type0用例覆盖。

## 正式内容相关性

从Q01已冻结source-linked capture检查全部405DAT：86383个BDY块、19461个ITR块。1127个BDY声明非零zwidth、78个ITR声明非零z；这些相关块均可映射到正式object catalog。例：OID204 `a/cha/cha.dat` frame179/306/310/312/313的BDY zwidth999。完整字段与catalog/source-capture hash见corpus-geometry-fields.json。

语料中未找到负zwidth，故负值案例目前是边界合同证据；82个ITR块缺少至少一个几何字段、27个ITR有显式零宽/高，需要按kind/control-only及真正reader继续分类，不能把它们一律认定为坏DAT。目录可达不等于本轮已操作复现每个技能。

## 编译、执行与保护

- 实际 `Tools/NTSD28Q03Geometry/Build-And-Capture.ps1`：g++编译成功，native14输出；source/header前后hash一致，正式EXE身份一致。
- Unity refresh编译并成功加载新Editor测试；`error CS` Console查询0。实际test job `8c6b30b020fa4569b928072785a8d5e8`，1个capture test通过并写入42条生产结果，见unity-test-result.json。**该PASS只表示capture成功，parity确实有27项失败。**
- 第一次run_tests连接超时；随后Editor状态确认idle、无current job和无结果文件，才提交成功的job。域重载观察失败没有被当作测试完成，也没有启动第二个Editor。
- NTSD_Battle仍isDirty=false、rootCount14。场景读取曾有一次connection closed，重新读取成功；未修改或保存Scene。
- 台账实跑PASS：471 records /43 governed code files。新增代码仅两个离线工具脚本及一个Editor测试和其meta。无production、DAT/PNG、框架、版本或有序关闭变更。
- 本诊断范围未重跑完整SelfCheck、Play或全项目测试；Q02既有证据不扩大为本包运行时对齐证据。

## 对后继合同的影响

Q03必须纳入BDY深度宽度、几何字段presence/有效性以及ITR缺省原始值与effective值的分层。不能只凭四项Body DTO已足够而冻结Q05窗口。内容identity/canonical writer与相关测试在Q05统一迁移，精确几何consumer及三种collector、union/cache边界在Q06独立行为包修改并回归本见证。

Q03父任务仍IN_PROGRESS：接下来继续held strength的有效深度选择链、两条OPoint materializer、CPoint canonical float及联合版本全reader表；不能以本包已测量差异代替Q03完整出口。
