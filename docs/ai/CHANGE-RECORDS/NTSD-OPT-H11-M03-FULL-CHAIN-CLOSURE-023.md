<!-- CHANGE-RECORD
id: NTSD-OPT-H11-M03-FULL-CHAIN-CLOSURE-023
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRealTextureSubmissionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user six item bounded first phase and start execution 2026-10-07; formal336 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH23-FULL-CHAIN-CLOSURE-20261007/REPORT.md
-->
# 第23批完整选定CPU链与重进限定验收
2026-10-07 correction（Batch25）：已知存活1MiB正对照counter0；本Record更正RUNTIME_PENDING / ZERO_GC_EVIDENCE_PENDING，原24/24和0B观测/显示/容量/生命周期事实保持，但旧未校准0B不再通过H11完整0GC门。不是已证production分配/行为回归，不重测旧矩阵；详Batch25 REPORT和counter原件。
脚本前PLANNED；准确范围/原状/副作用/固定矩阵/验收/回滚见同名Task。复用现有Editor fixture与生产窗，不新建runtime owner、不改生产规则。旧完整桥aux disabled，本批在同一Execute加活动aux并提交其draw；旧source/slot/CPUlease语义保持。新菜单仅独立输出/一次重进，无catalog重复重放。
所有代码验证尚未运行；首阶段新子批2/8，本问题不新增诊断/优化候选。

代码追加：原real-texture fixture新增两具名active-aux矩阵，复用原Run/Execute/Slot；aux在seal前预热且每次实际DrawMesh，采样涵盖capture/motion/upload/record/execute/CPU lease/retire；旧六case签名/输出语义保持。独立Batch23重进入口复用严格camera门，禁重复catalog replay。尚待原Editor编译/聚焦及重进，未报告通过。

首次fixture作业698c6d3ebcbd4bd696f7cbabb28eaea6 completed24/status failed，两新case在constructor发生 Cannot resize sealed foot marker storage；result null，不猜补22/24全结果。原因是新增fixture过早seal，随后现有submission.PrepareCapacity再次准备同owner。只将fixture seal移到既有submission准备之后，不改production；原failure及两个FAIL result保留。本批修复复验轮1/3，下一只固定两新增＋原slot负例和同组回归，不扩大诊断。

同轮静态闭合：现有submission.PrepareCapacity/SealCapacity已负责辅助owners，故撤销新增fixture的冗余辅助prepare/seal，直接复用原Slot构造（无需另定owner）。Batch23严格aux门与是否执行catalog replay解耦，新显式requireActiveAuxiliaryCoverage=true；旧批逻辑不变。尚未运行第二作业，不新增修复轮。

原Editor实际重载/编译无errorCS；第二固定作业77b89de1c7d1446e81b443ef722b3fd2实际24/24 Passed、0fail/skipped，9296仅discovered。两新增完整CPU桥各64warm/1800sample：1000Strict+1000aux与4097Ordered+17aux均当前线程0B/growth0/CPUlease0，draw总1803600和9000；CPU mean6.265/12.274ms不是AI/Frame/GPU。新case仅受控publication，不称生产catalog/排序或1000AI证书。原Scene严格重进尚待执行；一轮fixture复验保持1/3。

最终本Task VERIFIED：原Battle严格重进1800camera/tick8→1313，Foot/Health每帧2、两slot、recorded/executed DrawMesh各11504、badFrames0、1800Build（publication1297/alpha503）、camera/observer0B/growth0。Editor global三代collection各3且两个全域硬门false，不称全局0GC。11阶段objects/slots/borrowers0、Scene clean/SHA同；原Menu clean8roots/nonPlay/idle恢复、errorCS0。13当前五文件hash同，15消费域源同及16–18证据复用，首阶段H11/M03限定交付通过；父OPEN、六项Goal仍active/2 of6、新批2/8。最终文件保护/validator见REPORT与final-validation；无production/资源/设置/专项门修改。
