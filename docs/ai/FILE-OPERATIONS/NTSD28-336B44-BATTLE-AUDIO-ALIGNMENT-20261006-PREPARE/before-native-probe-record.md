<!-- CHANGE-RECORD
id: NTSD28-336B44-BATTLE-AUDIO-EVENT-RULES-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleAudioAlignmentEditorTests.cs
authority: user 2026-10-06 battle audio request; formal 336B44 and verified playable battle audio live paths
evidence: docs/ai/TASKS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006.md
-->

# NTSD28-336B44-BATTLE-AUDIO-EVENT-RULES-001
修改前建立。状态 PLANNED。
需求及权威：用户要求先修复角色与战斗全部音效，不改DAT。正式根336B44、runtime sound.dat/角色DAT及当前稳定音频live分支；源树候选非战斗开发不能自动晋升，正式EXE逐事件音频导出限制如实记录。
Unity 原状与预期：按已确认普通命中/防御/护甲/特攻命中与非角色反弹落地公共分支修声音选择、事件顺序和 source X；同步 ECS shadow projection。动作、伤害、运动、RNG、碰撞结果均不改；kind9/dash候选只在闭合来源证明后处理，不凭猜测改。
受影响路径：metadata 精确清单；新增测试仅验证共享规则/资源差异，不建立全角色操作矩阵。
不变项：原框架、非战斗场景、33ms/3ms、固定相机/统一比例、原资源 GUID、DAT字节、项目模式Asset、Scene/Input、旧目标关闭状态。
验证：先聚焦首差；原Editor自动Refresh/编译；具名共享分支测试、实际音频加载/voice见证、必要代表Play；文件SHA/ChangeLedger/diff审查。
风险：帧多声列表预热后的加载时长/内存；旧测试可能固定错误音效；双路径声音预测必须一致。主代理负责集成，禁止改变伤害/物理/状态逻辑。
回滚：PREPARE操作before实际原字节或对应hunk；任何回滚必须保护并发改动并另记操作，不恢复整仓库HEAD。
不可回退边界：不得回退用户/其他任务修改、DAT及Scene；无外部发布/删除。

2026-10-06 CODE_WRITTEN：实际脚本已按声明路径编辑；资源966新增WAV+966meta+53foldermeta已逐SHA验证，原12保持。cue新两项RED均预期首差；原Editor Refresh已提交，正在导入资源，初次请求30秒超时不等于失败/编译验收。事件规则worker三个指定文件已静态检查，ECS投影和运行仍待主代理集成。无DAT/Scene/非战斗修改。
