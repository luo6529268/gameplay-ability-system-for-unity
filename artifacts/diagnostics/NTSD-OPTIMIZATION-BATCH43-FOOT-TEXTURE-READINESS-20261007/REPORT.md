# 第43批 Foot纹理准备验证报告

## 当前裁定（2026-10-07；覆盖下方事前快照）

`PARTIAL / REENTRY_WINDOW_ZERO_GC_PASS / RUNTIME_PENDING`。原Editor有效8 RED→39/39 GREEN；两次原saved Battle完整窗口结果已回收。第二次重进的完整1800相机范围可靠0event通过；第一次迟发12event仍有效FAIL、调用点UNKNOWN，不能用第二次PASS消去或宣称H11阶段完成。H07千人性能未达状态不变，本批未测FPS。

本轮新鲜留痕验证：pwsh Tools/Validate-ChangeLedger.ps1 -RepositoryRoot准确原项目，exit0/error0/4263匹配WARNING行（历史声明提示保留）；git diff --check exit0。原39/39具名summary已重新读取，未重复测试；主agent源差异review完成，Probe相对before只12菜单行，42既有改动保留。

| 项目 | camera-01 | camera-02（必要重进） |
|---|---|---|
| 实际存储 / 目标帧 | 1800 / 1800 | 1800 / 1800 |
| tick / 实时pipeline | 8→1292 / URP | 8→1275 / URP |
| camera GC事件 | 12，全部ordinal937 / unity4048 / tick678 / slot1 | 0，首帧也0 |
| 累计camera scope / observer scope | 1801 / 3602（不同于存储帧数） | 1800 / 3600 |
| observer事件 / invalid / unattributed | 0 / 0 / 0 | 0 / 0 / 0 |
| camera与observer前后四校准 | 全PASS | 全PASS |
| 每帧Foot / Health下限；submission slots | 2 / 2；2 | 2 / 2；2 |
| CPU DrawMesh录制 / 执行 | 11536 / 11536，逐帧无差 | 11570 / 11570，逐帧无差 |
| 完成build / publication / alpha | 1800 / 1278 / 522 | 1800 / 1262 / 538 |
| entity顶点上传bytes / 次数 | 1408000 / 1800 | 1414336 / 1800 |
| build失败 / 拒绝；增长 / 非零CPU lease帧 | 0 / 0；0 / 0 | 0 / 0；0 / 0 |
| 十一阶段关闭；objects / slots / borrowers | PASS；0 / 0 / 0 | PASS；0 / 0 / 0 |
| Scene clean / 磁盘SHA不变 | PASS / PASS | PASS / PASS |
| 原始终态 | FAIL / DONE | PASS / DONE |

第二次Play期间仅进程外文件/PID观察，无MCP、Console、动态代码、刷新或测试命令；code/request/full scope/首帧/校准不变。第一次中途MCP状态查询与单簇迟发事件仅有时间相关性，没有调用栈或受控因果证据，**不得**将12event归因于工具或豁免。42首次Foot40B事件未在本批早期重现，只能支持冷引用候选，不能证明旧35两事件或本批12事件的精确原因。不得追加第三同构长窗刷PASS。

CPU热方法未改：28行仅冷设置准备与保持Texture2D wrapper引用；同长度复用、配置替换/移除及owner注销清引用。既有选帧、回退、动画时间、排序、segment、逻辑保持。不是新Texture/Sprite/GPU像素分配；引用数组/header/wrapper实际驻留bytes未测，计入steady说明，不声称ATLAS或全进程预算已认证。共享URP feature可跨World驻留，不能推断Scene卸载必然Dispose；原有序关闭链未改变。

报告中`diagnoseFootCoverage=false`：`runtimeFootEnabled`及`footAuthoringBefore/After`是未启用诊断的默认值，不据此断言Foot关闭/缓存实际6帧已反射验证。生产消费者活动数每帧2已证，当前资源与代码前后SHA稳定；新增强引用 storage的正确性由39聚焦门覆盖。无额外SelfCheck/千人/Profiler/GPU/设备验收。

02结果DONE后才通过MCP核实原PID19040 idle/nonPlay/noncompiling/testsInactive，Battle11roots clean；正常打开原saved Menu后单Scene8roots clean，未保存Scene。21guards、9原backup、5个pre-camera指纹及HEAD全部匹配。原始窗口及哈希、计数详[terminal-audit-01.json](terminal-audit-01.json)；CPU lease为0与Execute返回不作为GPU完成证明。GC raw unit是TimeNanoseconds，01 raw300不是300B；旧不可靠0B counter不覆盖严格event FAIL。

下一：不重跑已通过评估或盲采同版相机；H11保留01未知的必要归因门，H07转回既有普通Brute残余collector热点的有据路径判断。已有40约50–53ms collector仍占逻辑约60%，不能以此Foot修复替代千人性能工作。阶段4/6、限定产物5/6、34父关闭0；已执行22–37及39–43共21批、38仅PLANNED，累计22不归零，次数只复盘，Goal保持active。

H07本次必要只读代码复核：BruteForceSceneQuery.cs:2335/2336已通过ref readonly读取RoleAwareFormalParticipantBuffer，不存在List indexer的大struct复制缺口；不得另造影子数组声称新收益。该文件:5755–5768几何拒绝仍有target.ItrRest.IsBound，:6932起几何通过有HasVrest；LF2ItrRestTracker.cs:25、203–208、317–329确认都执行EnsureActiveBinding且失效时ClearBinding，不能直接删除或无条件提前校验。:6045起粗判普通union不重叠后仍遍历ITR检查kind5，对应38历史候选，但其独立残余成本/当前生产收益未测，不能只因编号PLANNED就实施。未修改这些文件、未新建rest缓存/空间索引或切collector。后继需先取得两分支残余成本证据，冻结必要诊断的准确范围，不再复做39旧慢路径采集或全局对齐。

39测试中的4096次GetAllocatedBytesForCurrentThread零读数仅是原局部断言结果；已知旧counter在正对照中不可靠，不能把它当校准0GC或精确40B根因证据。完整窗口以已校准GC.Alloc event为准。根README缺失/错误只读定位命令没有造成资源/代码修改，正确技能引用与当前源码已补读。

PLANNED。42已观察40B动画取纹理调用链；wrapper首次物化仍是待验证推断。
本批不改逻辑/排序/Scene/资源，不称FPS优化完成。测试、完整相机和后置保护结果随后追加。

有效RED：原Editor job4b879277b5a34bd5b9ebccae2c6698b5实际8个case执行，八个新增storage断言失败；原件red-01.json。生产修复尚未写入，GREEN与1800camera未运行。本结果是test-first预期失败，不是新增运行时回归。H11/H07未达，Goal active，阶段完成条件而非次数停点继续生效。

更新：生产仅冷设置新增28行引用持有、原选帧/回退不变；原Editor新DLL/CS0，GREEN39/39（Foot24＋aux capacity/lease15，含8新增）实际PASS。21guards/9backup/HEAD同、diff检查0。完整camera/重进仍待，未有FPS/千人收益。原失败和工具codedom文件名过长限制保留，不启动另一Editor或改插件。
