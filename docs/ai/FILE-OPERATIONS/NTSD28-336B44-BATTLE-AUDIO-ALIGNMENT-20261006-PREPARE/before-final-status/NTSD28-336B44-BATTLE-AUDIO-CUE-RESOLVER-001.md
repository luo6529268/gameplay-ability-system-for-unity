<!-- CHANGE-RECORD
id: NTSD28-336B44-BATTLE-AUDIO-CUE-RESOLVER-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleAudioCatalogEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleAudioSceneProbeEditor.cs
authority: user 2026-10-06 battle audio request; formal 336B44 and verified playable battle audio live paths
evidence: docs/ai/TASKS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006.md
-->

# NTSD28-336B44-BATTLE-AUDIO-CUE-RESOLVER-001
修改前建立。状态 PLANNED。
需求及权威：用户要求先修复角色与战斗全部音效，不改DAT。正式根336B44、runtime sound.dat/角色DAT及当前稳定音频live分支；源树候选非战斗开发不能自动晋升，正式EXE逐事件音频导出限制如实记录。
Unity 原状与预期：战斗专用统一解析 SFX_ddd -> data/ddd.wav，正式 WAV 资源闭包，FrameSounds 前20条预热；generic/菜单路径、增益/声像/64voice 不改。缺失generic预热不发起无效加载，保留存在文件的兼容预热。
受影响路径：metadata 精确清单；新增测试仅验证共享规则/资源差异，不建立全角色操作矩阵。
不变项：原框架、非战斗场景、33ms/3ms、固定相机/统一比例、原资源 GUID、DAT字节、项目模式Asset、Scene/Input、旧目标关闭状态。
验证：先聚焦首差；原Editor自动Refresh/编译；具名共享分支测试、实际音频加载/voice见证、必要代表Play；文件SHA/ChangeLedger/diff审查。
风险：帧多声列表预热后的加载时长/内存；旧测试可能固定错误音效；双路径声音预测必须一致。主代理负责集成，禁止改变伤害/物理/状态逻辑。
回滚：PREPARE操作before实际原字节或对应hunk；任何回滚必须保护并发改动并另记操作，不恢复整仓库HEAD。
不可回退边界：不得回退用户/其他任务修改、DAT及Scene；无外部发布/删除。

2026-10-06 CODE_WRITTEN：实际脚本已按声明路径编辑；资源966新增WAV+966meta+53foldermeta已逐SHA验证，原12保持。cue新两项RED均预期首差；原Editor Refresh已提交，正在导入资源，初次请求30秒超时不等于失败/编译验收。事件规则worker三个指定文件已静态检查，ECS投影和运行仍待主代理集成。无DAT/Scene/非战斗修改。

实际Play出口实施前声明准确测试脚本 NTSD28BattleAudioSceneProbeEditor.cs：原已保存Battle Scene，生产角色/声音预热/Driver/voice与退出；自然Naruto DJJ/DJA沿既有07855tick序列，受控180/213帧声音与一次60攻击命中代表。不称受控帧注入为自然受击/自然冲刺。结果采用本Task新CreateNew文件，不覆盖旧探针原件，Scene/config不写盘。

2026-10-06 FOCUSED_TEST_PASS：原Editor实际compile0 error；audio-focused-green-final-result 25/25（含978原WAV UnityWebRequest samples/channels/frequency逐个一致，代表8声音voice，128稳定播放GC0）；post-audio-focused-final-result16/16；post-audio-ecs-final-result6/6。去重为39具名case，不称47个不同case。native当前Core22/22应用成功/0anomalies，普通effect0/1/type3/prefix，effect2/3/23后效，reduced0/7/70/75 DAT回退/type3目标X，type2反弹。正式EXE音频事件未捕获，当前source身份与正式根精确对应仍受历史来源限制；当前Core trace不晋升formal EXE。681保护哈希、978三端WAV一致、原12 WAV/meta24项与开始Git基线完全同。
原Scene run01只在正式内容预热startup180秒超时，没有战斗tick/声音测量，报告FAIL；正常有序关闭/退出后voice0/hashclean保留，不包装PASS。run02用既有078十分钟启动先例、12分钟总限，30秒CreateNew状态快照，正在运行；collision门已要求同tick实际assigned+playing+playdelta，有序关闭失败停止不卸载。无DAT、Scene/Input或非战斗改动。

2026-10-06 最新更正：Scene02提前退出由用户明确确认是他或其他任务的操作；尚未进入声音预热/战斗tick，所以不是运行音频失败也不是PASS。Scene03在已保存Battle/Editor idle前置后启动，输出新03保留01/02。Dash三RED额外1个cue，删三行后同三项GREEN，动作/速度/DAT帧a7断言通过；累积42去重case，原39和978加载未重复。独立只读复审无新增阻断；原Scene声音/普通SelfCheck整体未知仍明确未晋升。
