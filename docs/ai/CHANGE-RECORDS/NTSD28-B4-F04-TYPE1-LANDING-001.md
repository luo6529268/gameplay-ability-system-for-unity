# NTSD28-B4-F04-TYPE1-LANDING-001 — type 1 state-1002 landing threshold

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-TYPE1-LANDING-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4Type1LandingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/PooledEntityReuseAllocationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan physics_integrator.h type1_state1002_bounce_threshold and physics_integrator.cpp type-1 landing branch; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED-1-OF-3-ACTION7-VS70 / COMPILE0 / FOCUSED4 / RELATED33 / NTSD28-BROAD459 / SELFCHECK-PASS-2026-09-05T04:35:25Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / TYPE1_THRESHOLD_BRANCH / FULL_PHYSICS_PENDING`

## 原状

Unity 用 `landingVy <= 9.9` 将 type 1/state1002 分成settle/bounce；Authority 当前正式
机器码读取的 threshold 是 `5.2571022450498032e120`，且比较为严格 `motion.y > threshold`。

## 计划

新增无状态 predicate owner并由production landing调用；测试锁定正常12.0不反弹、等于阈值
不反弹、仅严格超阈值反弹，以及非state1002永不进入该bounce。

## 已写入

- 红灯 job `ac302a8043dc4f3c97198ad3a4262cae`：3项执行，正常12.0冲击预期70、实际7；
  其余两项通过，证明失败来自旧阈值而非fixture。
- 新`BattleNativeType1LandingKernel`保存正式巨大threshold并执行state1002+strict `>` predicate。
- production type1 landing改为predicate决定唯一bounce分支；普通state1002落到70，其他state落到60。
- focused test新增等于阈值、严格超阈值、非state1002与三组production写入。
- 相关零分配夹具原先把普通12.0冲击固定为旧action7+sound；按当前Authority更正为
  action70+no sound，零分配循环本身不变。
- SelfCheck transformed type1 fixture同样仍断言旧12.0 bounce；更正为action70、Vy0、方向不翻转，
  durability与Vx减半断言保留。

## 验证

- 编译：Unity Console error 0。
- focused job `418f18b4b48d44ebae3516b2b7598354`：4/4。
- related job `a25e604e73b140a3936e723cfc6082c7`：33/33；其中零分配旧fixture已按Authority更正。
- final broad job `87ef8201f04449528b1bcc83cb99aefa`：459/459；先前同实现broad
  `57adfc01f47a4fa88fe72033814ec9f0`亦为459/459。
- BattleRuntimeSelfCheck：首次准确捕获旧transformed fixture；更正后
  `2026-09-05T04:35:25Z` PASS。
- Scene SHA/length/mtime仍为`0D74E174...D77 / 203477 / 2026-09-04T13:12:45Z`；
  clear后Console error0；`git diff --check`仅换行提示；Ledger 213 Records / 192 governed
  code files PASS。

## 未关闭

collision-Y reference/effective floor、type0/2/3/4/6、identity extras、统一integrator、
Audio与正式资源不在本包；不得把本状态扩大为完整type1、F04或B4对齐。

## 回滚

删除新 predicate/test，恢复 type 1 原分支；不涉及数据迁移、Scene、Prefab、DAT 或 Authority。
