<!-- CHANGE-RECORD
id: NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001
status: VERIFIED
change-kind: COLLISION_SNAPSHOT_SOURCE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/collision_frame_lookup_witness.cpp
authority: Current playable BattleWorld28.snapshot_actions, rebuild_geometric_hit_candidates and advance_catch_relations; current definition.frame(tick_action_snapshot), no current-action fallback.
evidence: Read-only caller audit finds legacy HasFrame gates, cached Prev2D identity and current fallback in Unity collision readers; original source runner is required before production edits.
-->

# 碰撞快照帧原函数见证

IN_PROGRESS / SOURCE_ONLY。前置审计为 COLLISION-FRAME-NATIVE-LOOKUP-AUDIT-001。
唯一脚本为 Tools/NTSD28AuthorityTrace/collision_frame_lookup_witness.cpp；复用既有 source capture build，不修改权威源码、正式 EXE、Unity C#、Scene 或资源。

输入：声明/隐式/越界快照帧、不同当前动作、当前定义替换；分别观察几何候选与无投掷/无输入 kind1 抓取推进。输出当前及快照描述符可用性/数值、原函数结果和两实体 before/after raw、抓取关系及计时；固定 seed，记录 RNG 调用。定义替换为诊断初值扰动，不冒充正式变身路径验收。

验收：正式 EXE 与构建源闭包身份一致；诊断 runner 编译运行成功，两次字节一致；缺失快照不回退当前，隐式零帧存在但不凭空含 itr/bdy/cpoint，当前定义替换后读取新定义；对照输入至少覆盖 0/99/857/998/999/1000/-1。保留失败输出。只关闭 SOURCE_MODEL_WITNESS，不关闭 Unity 对齐或正式 EXE 场景验收。

无生产字段、schema、模块、生命周期或关闭顺序副作用。回滚仅撤回本新增诊断脚本及本包文档差量，按既有规则取得授权；不覆盖用户工作。下一步须单独准确声明 Unity 修改路径及测试，不能把本 Record 当作全局 getter 替换授权。禁止 computer-use、非战斗/Unity-GAS/Server/Gen/Plugins 修改。

实际已写单 CPP：252 输入（7 snapshot×声明2×current3×替换侧3×目标snapshot模式2），输出 before/afterCollection/afterCatch、候选及抓取 counters/RNG。build session 68454 进行中，尚未报告编译或运行通过。

## 最终限定出口

VERIFIED / SOURCE_MODEL_WITNESS_ONLY。build session68454退出0；原EXE/75源身份均匹配。252向量、3780断言、重复SHA fa3586923fc1b51be6437b9da0a6e8c3fe990efc2438eda03ff6f973b1e41897；72有候选/108kind1推进，RNG0、raw不变，timeout78/75。实际命令、完整证据和诊断初值/正式可达性限制见 artifacts/diagnostics/NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001/REPORT.md。单CPP新增，无Unity生产/Scene/资源变更。未跑新Unity compile/SelfCheck/Play；不得借source通过升级Unity状态。下一Task COLLISION-FRAME-UNITY-001 / READY_UNITY_RED_AND_LIVE_GATES，须精确新Record。原当前1000而snapshot有效可保留候选的端点结果，不自动证明正式collection入口可达。最终Ledger/diff-check另写结果。

最终交付检查：Tools/Validate-ChangeLedger.ps1退出0（历史已声明但不在当前diff的路径为WARNING）；git diff --check退出0，证据在同包ledger.txt与diff-check.txt。Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6再次一致。无提交/推送。

## 追加正式快照时点输入（修改前）

原252/3780限定VERIFIED事实保持。Unity读帧修后raw/descriptor/catch零差异，剩每组24候选都是current1000/snapshot分离诊断初值。现复用同一CPP追加84个current=snapshot输入（7×声明2×替换侧3×双方快照2），确认高动作和零帧在刚snapshot的候选入口；前252输出字节必须保持。输出另存expanded，不覆盖旧first/repeat。无Unity生产变更，Record暂IN_PROGRESS直到新增向量验毕。

追加84已验证：总336，两遍字节相同SHA43f6713de73a76ba5f2c07c1627bf058ecf9e1706baa93701112200ef1ff7499；前252字节前缀完全保留。新增84中42为两端current=snapshot，余42为attacker同值、target保留snapshot0的混合端点；不得称全部84两端同值。96有候选/144kind1，RNG0，原EXE/75源身份保持。expanded目录独立证据，原252未覆盖。
