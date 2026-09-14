<!-- CHANGE-RECORD
id: NTSD28-Q06-COLLISION-FRAME-UNITY-001
status: IN_PROGRESS
change-kind: NATIVE_COLLISION_FRAME_READERS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CollisionFrameLookupEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidatePairSnapshotFactory.cs
authority: Current BattleWorld28 snapshot_actions/rebuild_geometric_hit_candidates/advance_catch_relations, original 252 source witness; current definition and tick_action_snapshot determine collision descriptors.
evidence: Caller audit 35 calls across 7 files, no override; legacy HasFrame and Prev2D/current fallback remain in getter, current/Prev2 query helpers and pair factory.
-->

# Native碰撞帧查询及消费接入

IN_PROGRESS / TEST_FIRST。准确四脚本，先新增测试，原252初值/输出为oracle；两profile和普通/role-aware读取、kind1推进、actual候选结果分开记录。原source端点缺失current仍可保留snapshot候选，但正式driver快照时点不同，诊断初值与正式可达输入明确区分，不用前者盲目删除资格门。

生产预定责任：LF2Entity.GetCollisionFrameData按当前FrameCache的Native snapshot号返回，不fallback current或依赖陈旧Prev2D。BruteForceSceneQuery current/Prev2私有点查和必要的实际候选资格按同源caller修改，保留pass顺序、缓存观察和nonbattle接口。PairSnapshotFactory.Capture的current/previous/snapshot查询成组核对。不得修改共享旧GetFrameDataById/HasFrame、CPoint raw/throw writer、UI/Scene/资源或GAS框架。发现额外符号必须先更新准确范围和依据。

风险是缺帧null不再借用当前帧，会暴露调用方错误假设、缓存fast path资格或旧测试构造；必须保留RED和逐字段首差，不以两条Unity路径一致代替原版一致。定义数据模板只读；无新增持久字段/schema/服务/队列，既有World有序关闭不变。

验收：原向量before/after raw47+抓取字段，显式记录3MISSING；Native getter描述符身份、空帧/999/越界与不同current无回退；normal/role-aware、actual/Shadow及CPoint/no-op；compile、focused、必要既有检查、SelfCheck、真实Scene双factory及关闭/Scene checksum。资源仍为合成fixture，不能报告正式DAT图片已切换。回滚仅本差量、须按规则授权；保留用户HUDBg30和其他用户工作。禁止computer-use。

已写测试脚本：252原向量×2profile×普通/role-aware，before/collection/catch raw与descriptor和候选分开记差异；生产尚未改。等待Unity编译及RED。

RED job576839c85fc744fd8cfd0029a0ef1e0c 四组全部失败，原before raw无首差；陈旧定义descriptor、旧高帧门/current fallback导致抓取及候选差异，原JSON/XML保存在red。先落实已声明三个Native读帧入口，暂保留独立current null资格，复跑后按原live caller决定下一必要修复。
生产三入口已改：collision getter直接Native snapshot，不使用Prev2D/current回退；BruteForce两个私有getter按当前cache Native点查；pair factory current/previous Native点查。其它null资格/过滤暂未改，准备复跑。

9efe00d15694402e9a63636d58cb2ea4终态12项：8PASS（kind2四组/4800与kind3四组/3200原向量）、4FAIL（新collision每组raw/descriptor/catch零差异，但各24candidate差异，全部current1000诊断初值）。MCP观测曾timeout，已查询同job取得终态，未重跑；after-readers完整XML/JSON保留。

64/64既有pair/group/catch定向回归通过（job11c9f34dcd794fee94aa5614915d39e9），XML已保存。测试同文件将接原expanded336并新增真实Scene probe：只选新增两端current=snapshot的42输入×双factory×普通/role-aware=168，Scene checksum/borrowers及回收断言；仍不豁免旧24诊断候选失败，不称collision包已关闭。

测试已扩展原336，并新增同文件Play probe42双端同值×双factory×双query=168；Unity编译后CS错误0。没有为旧24candidate差异增加豁免，也未修改额外current准入。等新4组终态后执行SelfCheck/Play。

新SelfCheck请求05:43:02Z，结果05:43:27Z FAIL：CheckLooseQuadtreeShadowBroadphaseContracts 第1805行，immediate query invalid AABB fallback目标未保留。原结果归SelfCheck-first-fail.result；需查真实query或夹具定义身份，不能报告SelfCheck通过。暂未Play。

## 当前检查点（非完整关闭）

IN_PROGRESS / READER_RUNTIME_PASS_QUALIFICATION_PENDING。expanded四组336仍四FAIL：descriptor/raw/catch0，每组24candidate首差。旧kind2/3八组PASS、pair/group/catch64PASS。独立SelfCheck两处修订后05:48:49Z完整PASS；真实Play168读帧/几何/抓取0差异、Renderer2→2/Scene checksum保持；Shutdown05:50:09Z恢复4→4、World/slots/两pool0、两帧Stopped。最终CS0/Editor idle非Play，Scene dirtyfalse/root14/hash bcd1047b…保持。完整REPORT/原失败/运行证据见同ID artifact。下一唯一COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001，父不标VERIFIED、不跳Q07。
最终账本验证PASS：555 Records、29 governed code files in当前diff，ledger-final.txt；git diff --check退出0，diff-check-final.txt。历史不在当前diff的声明路径WARNING不代表本轮新增错误。父保持IN_PROGRESS，无提交或推送。

## 当前检查点06:40Z（仍IN_PROGRESS）

原336四组+新480四组共3264端点候选/pair/raw/classification全部0差异。完整driver四组仍FAIL：8例的bdefend0/45和RNG legacy3/CRT0对源CRT2，HP/caught/current/snapshot与其它raw一致、Shadow2effects/mask0。源45是battle_world无护甲分支显式覆写，非DAT默认；DatHitResolver Spark两次旧随机已定位。下一UNARMORED-BDEFEND-WRITER-AUDIT，再HIT-SPARK-TRANSACTION-AUDIT，最后回当前完整driver。

新SelfCheck06:27:14Z PASS；原70回归69PASS/1旧期望，独立修订后4矩阵PASS；Play480/Renderer2→2/Scene checksum保持；Shutdown06:38:10Z恢复4→4、World/slots/两pool0、两帧Stopped。最终CS0/Editor idle/Scene dirtyfalse/root14/hash bcd1047b…；接口6402由状态文件发现。完整证据见COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001/REPORT.md。资源/非战斗/框架未改，父不标VERIFIED。
