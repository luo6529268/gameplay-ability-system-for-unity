<!-- CHANGE-RECORD
id: NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001
status: VERIFIED
change-kind: BATTLE_FRICTION_GATE_RETIREMENT
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04MassFrictionGateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04MassFrictionPlayProbeEditor.cs
code-path: Tools/NTSD28Q04Mass/AuthorityFrictionWitness.cpp
authority: Active user goal and Q03 exit; current formal NTSD2.8-Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 playable PhysicsIntegrator28::step/damp_toward_zero.
evidence: VERIFIED_MASS_GATE_ONLY / RED_6_FAIL_3_PASS / FOCUSED_14_PASS / FULL_SELFCHECK_PASS / REAL_DRIVER_GROUND_LANDING_MASS_INDEPENDENT / PLAY_CLEANUP_PASS / SCENE_CLEAN / RELATED_19_PASS_1_UNRELATED_PHASE_FAIL / LANDING_ULP_REVIEW_PENDING / Q05_CARRIERS_RESERVED
-->

# NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001

## 事前合同

同ID Task为准确范围与验收来源，Q03已DELIVERED_CONTRACT_ONLY。唯一production修改为CharacterMechanics.StepBattleLogic中的startedGrounded &&ctx.mass>0改成startedGrounded，保持积分先于摩擦、整数Y/collision reference、摩擦数值/epsilon、重力/落地/边界与33ms不变。

原状：mass0/负值能跳过摩擦；real character _mass与ECS context均传入。新状态：这些值不再定义摩擦，context/runtime/shell字段仍保留为Q05过渡，不提前改变12/20/23或character1。

测试新增Editor focused，覆盖core不同速度与grounded/刚落地/边界、real LF2Character ApplyDynamics与ECS exact-character执行；Play probe只在现有Play world暂停后加入无renderer测试角色，调用真实driver和登记结果，finally注销角色/恢复pause，核对对象slot基线，不写Scene/资源。新增native离线witness链接原PhysicsIntegrator，用相同场景取得期望输出；不写权威目录。

无新production module/manager/queue/worker，关闭十一阶段不变。测试实体临时绑定已有World，离线工具退出即释放资源。无外部提交/push或资源删除。风险为latent state移除；普通type0 mass1路线必须回归。允许的文件只有metadata所列及它们meta、诊断artifact、Task/Record/状态文档。回滚经用户批准仅反向本包小范围改动，不覆盖其他工作。

## 验证与实际变化

尚未写测试/production；下一先跑RED，取得实际失败后再改单个gate。完整SelfCheck与目标Play未完成前，不报告VERIFIED。

RED实测：Unity job fa10ab39cd7d4d8c83605b663f722d2f，9项中6失败/3通过，mass0/-2在core、real character、ECS三路径均保留5/-5而native要求4/-4；mass1通过。原版源码witness8场景已运行，native.tsv保留全physics结果。Unity core仅处理落地前结果，landing case对VX/VY按既有调用方分层断言未执行后处理，不伪称core输出等同整个native step；其余位置/摩擦/重力直接对照native。接下来仅改单个mass条件。

已写：CharacterMechanics.StepBattleLogic仅删除`&& ctx.mass > 0f`并加Alignment contract注释；没有删除mass数据载体。新增Play probe已声明，运行真实driver ground/landing tick与三种mass比较，finally注销逻辑实体恢复pause。编译请求已提交，GREEN/SelfCheck/Play仍待，不报告完成。

过程验证：新Play probe首次编译因不存在的SimulationTickDriver.TryGetInstance报CS0117，已改为只查现有组件的FindObjectOfType，未改driver。重编译后相关job4dc48dc005eb403a89e7b75b2ddd6623完成20项，19通过/1失败；失败为既有NTSD28C06NestedPhysicsProductionEditorTests空World的硬编码phase[28]期望FrameAdvance而实际Stage。该用例没有实体，不调用本包mass分支；生产pass表也未改，故记录为独立旧phase断言差异，不改主循环让旧断言通过。随后单独跑本包9项+B4核心5项取得精确GREEN结果；原20项报告完整保留green-related.json。

GREEN job05ce4f3ab0b34684b90a4a8c0a9cb339实际14/14；完整SelfCheck按01:22:33 UTC新request运行，结果timestamp更新后PASS，已复制selfcheck-result.txt。首次Play ground tick已得到Vx4/Vz-4，但固定Z200被真实stage边界钳到237，导致位置断言失败；记录play-first-stage-boundary.json，cleanup通过。仅修probe：先实际driver warmup刷新stage，再在Z范围中点测试；production没有新增改动。后续Play待重新验证。

最终限定出口：重跑真实Play PASS，ground tick2076→2077三mass均位移5/-5和速度4/-4；后续landing三者一致、cleanup恢复原实体/槽位数。退出Play Scene dirtyfalse/root14、CS0、Ledger473/49PASS。完整证据与实际命令见同ID artifacts/diagnostics/REPORT.md。新观察到独立旧落地乘1/3与native除3的一ULP差异，登记后续Q06精度审查，不把本包mass独立性夸大为完整落地对齐；旧empty-world phase断言失败也保留。Q04-A已关闭，Q05 carrier/schema仍待，下一Q04-B。
