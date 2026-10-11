# H10 具名声音覆盖窄读

Task: NTSD-H10-NARROW-AUDIO-COVERAGE-20261011
Created status: IN_PROGRESS
授权：人类在讨论会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193针对R1–R6窄只读提案回复“继续处理”，通过本次具名指令转交。此前 minimal-read-permissions.json 的未批准状态是提案时点历史，本任务不改写它。

唯一写域为本新 artifacts/diagnostics 目录，CREATE_NEW_ONLY，只新增Task、读取清单/摘录、SHA/Git证据、JSON和报告。所有既有脚本、资源、Scene、Settings、旧报告和治理文件零写域。无Unity、测试、编译、测量、子代理、工具修复、预算、catalog裁剪或新生产实施。

## 准确许可

- R1：Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs::ApplyWeaponDamage，只取local sound赋值、声音选择条件、definition identity及QueueBattleSound；不读其他伤害/RNG/HP/生命周期写入。
- R2：同文件ApplyNativeType5HurtTail，只取cue来源、选择guard和声音调用。
- R3：同文件ApplySpecialObjectHurtTail，同样只读声音片段。
- R4：Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::QueueNativeC25FrameSounds，允许整个小型纯声音方法；不展开周边事务。
- R5：Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs::StateExitEvent与State_Frozen，只读冰冻state_exit派发/转发和绕行判断，不读其他状态体。
- R6：Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::RunNativeC25FrameTransaction，只读决定冰冻StateExitEvent调用/绕过的调用行及对应条件，不输出完整事务。

只在三份准确文件和批准方法内定点匹配，先声明/声音值赋值/调用，再补最近必要条件。每次实际输出行号、原文件SHA和读取目的入清单；不输出整个文件/大方法，不读取旧越界片段。如果另一个受限helper的来源仍不足，准确记录并停止，不自动追踪。

## 验收与保护

每项形成guard→值来源→有限常量/字段→既有1000cue identity/alias的证据链，注明同代定义假设。armor/int字段判断仅限三个producer。IceShatter区分兼容调用、静态可达/绕行及未运行。若发现有据生产缺陷，只提出最小写域、行为变化、验证/回滚供独立审阅，不实施。Menu UNKNOWN、预算未设与其他暂缓状态保持。

原218.314MiB、PCM和每局所有权证据复用，不重跑。开始及结束核三份目标源码、相关旧证据、HEAD/staged/tracked差异与Task外Git状态。无代码变更，无新增runtime owner/缓存/关闭责任，ChangeLedger不适用。终态记录于RESULT/REPORT，本创建合同保持原样；本有限Task完成即交回，不自动后继或恢复Goal/队列。
