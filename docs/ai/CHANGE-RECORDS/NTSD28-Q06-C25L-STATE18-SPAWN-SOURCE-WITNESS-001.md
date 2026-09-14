<!-- CHANGE-RECORD
id: NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001
status: VERIFIED
change-kind: C25L_STATE18_ORIGINAL_SPAWN_WITNESS
code-path: Tools/NTSD28AuthorityTrace/state18_spawn_witness.cpp
authority: Formal BattleWorld28.materialize_state18_broken_weapon_particles/spawn_at and SimulationTickDriver28::step playable closure.
evidence: F08 and prior C25L record remain resource-spawn pending; Unity uses legacy Match.Rng and ordinary OPoint initialization/team.
-->

# State18粒子出生及C25组合顺序原函数见证

前置parent FRAME-TRANSACTION final caller join。已闭合fragment与core不重做；本Record仅新增一个workspace native runner，不改正式EXE/source、Unity生产/资源。

静态确证：native synchronized selection0x416A40及四tuple0x4212DE/4212FE/421339/421367；旧Unity BattleRandInt→Match.Rng。原generic出生HP/MP500、owner/group默认、facing默认false，初始整数位置是source整数，精确位置再偏移；现旧TransitionEffect继承team/dir并走ordinary OPoint出生。最少source反例必须先实测，不直接改断言。

矩阵：previous/current18/19/非适用；多个seed与delay0/positive；catalog999缺失、空槽耗尽/单槽、source20/70/高位；declared/missing999及pending状态；粒子定义type0..6与非默认stats；直接C25L出生端点和独立原完整driver；另外普通OPoint→state18→builtin/DAT碎片→lifecycle组合目标，输出事件slots/raw50/独立generation/RNG calls/frame参与/诊断。不要把global generation用作per-slot epoch；本例每slot首次出生raw epoch1。

构建复用Build-AuthoritySourceCapture.ps1，输出仅Temp/artifacts，正式EXE/75source身份由构建器核验。runner两次stdout字节一致、检查rows/事件/RNG/生命周期；source诊断不作为正式EXE物理键或图像证据。后继Unity先精确Record/RED，保持state13/200旧tail范围，动态global-delay producer仍Q08/B8，不重复旧C25L ownerplacement。回滚仅本新runner且删除须批准，无新runtime模块/关闭阶段。

VERIFIED_SOURCE_MODEL_ONLY：1550行/12759检查、重复字节相同、18条预置异常诊断留证；首782无持续选中分支，已补seed sweep。命令Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06State18Spawn -RunnerSource Tools/NTSD28AuthorityTrace/state18_spawn_witness.cpp -ExecutableName state18_spawn_witness.exe；运行binary两次并JSON检查。manifest/validation/REPORT含边界，Unity未修，新脚本仅此runner。

生产验收补充正式999内容见证：同一runner增加可选runtime根参数，load330正式catalog并覆盖仅父fixture888/151和组合777，999使用正式定义；原1550 synthetic输出/manifest不覆盖，新增formal.jsonl/formal-build-manifest。含直接出生与完整driver，不能用synthetic700 max_mp冒充正式资源。准确code-path不增加，权威源只读。

正式999补充已VERIFIED：96向量=48直接出生+48完整driver，重复输出一致，frame/lifecycle错误0/诊断0，实际type5；formal.jsonl与formal-build-manifest单独保存，原synthetic1550及manifest保持。
