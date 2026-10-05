<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-HITFA7-SOURCE-DEPTH-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
authority: current formal 336B44 playable NativeAi28 step_non_character_hit_fa source depth dead-zone; user D-024 common spatial ratio requirement
evidence: artifacts/diagnostics/NTSD28-336B44-Q07-D024-HITFA7-SOURCE-DEPTH-20261005/REPORT.md
-->

# hit_Fa7 active-target 源深度死区

2026-10-05证据口径更正：下方“旧fixture只借加载形状”表述不完整。完整Driver测试复用旧schema、seed、诊断Stage23边界及部分初态，运行当前LoganRuntime与项目模式配置，并显式构造本案源位置和目标关系。旧fixture的5EDA版本元数据不作为当前规则权威；通过结果只证明原Editor受控EditMode的一个完整生产tick，不证明原Battle Scene自然Play或正式根EXE同初态。独立只读复核确认仅执行一次StepOneTick、scope检查对象/槽/logic pool三项归零；本条更正不新增测试或扩大验收范围。

2026-10-05必要完整Driver出口已取得：原Editor精确job `3ff1eaf080a1479b8c620385b11ccf3f` 正式summary 1/1通过/0失败、22.677129秒；完整一tick后sourceZ600/Vz0/viewDeltaZ0、target604/槽0/两实体，既有scope的正常有序关闭对象/槽/logic pool均0。原Scene仍非Play/clean/root11/Console0error，四保护SHA保持；正式336B44和两端OID875 decoded DAT本轮再次SHA核对。MCP刷新响应短EOF未启动job，随后新程序集/干净前置后才启动唯一job；旧fixture5EDA只借加载形状、不裁决规则。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-HITFA7-SOURCE-DEPTH-20261005/REPORT.md)保存原件和边界。状态RUNTIME_PENDING仅保留原Scene自然输入/根EXE等未覆盖边界，必要ONE已完成，转REUSE/相关改动或实际首差回访，不重跑6项或完整tick。

2026-10-05完整Driver消费者已写且生成0错：仅新增Task事前声明的同文件方法，旧fixture保持字节/旧元数据不重标；当前LoganRuntime＋项目mode Asset、固定投影后显式源600/604与Y-40、target0/Vz0，单次生产StepOneTick，已有scope负责有序关闭和发布恢复。不写临时trace或资源文件，不改变原Scene。生成工程退出0/301warnings/0errors/9.22秒；MCP刷新与精确单方法启动中，尚无终态，不重跑6项。

2026-10-05原Editor GREEN已取得：具名job `bab0e57e9c924ecfaef23b870472fac5` 正式summary为6/6通过/0失败、48.7978624秒。四边界及旧active/empty两邻例通过，源精确位置和view增量符合规则；独立后验idle/非Play/原Battle clean/root11/Console0error/四保护SHA稳。完整Driver仍未知，状态RUNTIME_PENDING。按Task事前新增同文件 `RealOid875SourceDepthDeadZoneSurvivesFullDriverTick` 一个受控生产tick消费者；旧三tickfixture只借形状，不引用其旧版源证书，当前源/内容权威保持336B44。该方法尚未写，ONE1，不重跑六例。

2026-10-05生产增量已写：仅在active target且双方SourceRulePositionInitialized时，self/targetZ共同读取各自SourceRuleZInt；否则整对保留旧view/raw读取。与before相比无其它行为改变，旧平台阴影补丁完整保留。修改后的生成工程退出0/334warnings/0errors/12.34秒；原EditorMCP刷新/同四参数和旧active/empty两邻例GREEN正在准备，尚无终态。独立只读代理对比两个before确认边界，无阻断发现；未运行其自己的测试，不能替代主线程GREEN或完整Driver证据。下方“生产尚未改”为RED时点事实。

2026-10-05原Editor RED已确认：生成工程退出0/301warnings/0errors/9.36秒；MCP刷新响应因domain reload发生WinError10054，未启动测试，随后只读核查新程序集已晚于源文件、idle/非Play/原Battle clean/Console0error，未重复Refresh。具名四项job `fab30ec5c9144062893824c9d89f39f2` 终态failed/completed4，固定gap±4的Vz期望0分别实得±0.4；identity/gap4与fixed/gap6通过。MCP result为null，计数由completed4及两项failures_so_far证明；8845为发现数。两个RED均落在预定速度首差，无夹具前置失败。生产尚未改，按Task只准备共同source历史门和Z操作数增量。

2026-10-05测试先行：仅新增已声明四参数方法，双方source整数单独同步，期望Vz/精确sourceZ/最终view增量由源死区及独立1152/730给定；finally注销两对象，无新输出文件或服务。两个脚本原字节/SHA/Git状态与四保护SHA已存同ID artifacts before-manifest；生产尚未改，生成编译和原EditorRED待运行。

2026-10-05脚本前登记。静态候选为源差4在当前view整数差6时额外Vz0.4；现有正式875/action55夹具可最窄判别，完整来源/调用闭包、前置、两路径和符号、副作用、保留raw历史边界、验收/回滚见[Task](../TASKS/NTSD28-336B44-Q07-D024-HITFA7-SOURCE-DEPTH-001.md)。尚未修改代码或运行测试；不会从静态公式宣称原Scene自然首差。

不增生命周期模块，不改有序关闭主序列；测试沿原World/entity注册并finally注销，生产只读双方已有source历史，源/视图积分仍由现有mechanics所有。

2026-10-05交付留痕核查：实际运行 `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，退出0/PASSED，1272份Record、diff中23份受管脚本均覆盖；`git diff --check`退出0。只读重计336B44前缀Record为219份/219份已解析status，其中58份未关闭（COMPILE_PASS1、FOCUSED_TEST_PASS20、RUNTIME_PENDING37），与当前总表58份及REUSE44/TRIGGER14调度一致。没有新增Unity运行或改变本包验收范围。
