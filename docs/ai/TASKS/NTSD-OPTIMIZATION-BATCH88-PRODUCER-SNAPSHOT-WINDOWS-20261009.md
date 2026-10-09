# 第88批：生产者快照复用的实际 Driver 资格与同版 Windows 窗口

状态：SCOPED_WINDOWS_COMPLETED / GAIN_SIGNAL / H07_PERFORMANCE_FAIL / DEFAULT_OFF。创建于任何脚本修改前。
需求来源：用户批准持续完成六项首阶段；有效 bounded autogoal 第0—8节。前一问答仅只读核对空间分区，没有新增优化结果；本包推进87候选尚缺的真实Driver/千人收益证据，不重开H06、不切生产默认。
当前事实：87局部15新/49旧聚焦通过；85已测ProducerSnapshotRefresh约0.95ms/tick、whole logic约42—43ms。87候选默认false。本包不修改World或87测试。

## 精确代码范围
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：新增独立ProducerSnapshotReuse冷入口、四请求、SuiteState/RunState、owned World两开关（87reuse及85boundary timing）、Apply/Restore/Observe/资格gate、生产runner前tick0接线、自然/外部Play exit恢复、owned terminal根及嵌入聚焦测试。旧请求矩阵和17参数BeginSuite接口保持。
- Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs：新增ProducerSnapshotDriver default/root两方向/32tick两千人布局资格及新Owner；既有Run添加nullable诊断参数（放在原最后compose参数前）、记录实际per-input-pass skip/fullRefresh；只对新nullable存在的world做冷设置与finally恢复。保留旧比较断言、root内容/trace、slot seal与shutdown owner；不改RawCapture。
- 五治理写入：STATE、CHANGE-LEDGER、FILE-OPERATIONS/INDEX、当前handoff、唯一优化总表。
- 两既有自然输出允许更新：Temp/Goal18_LastTestResults.xml；Temp/NTSD_ProductionEntityStress.result。SharedRequest必须仍不存在，不创建文件触发器。
其余现存文件均受before递归保护，新增证据仅本ID目录、Task/Record/Operation，目标必须不存在再创建。

## 冻结的必要验证
1. test-first reflection API missing RED；GREEN新冷request/owner/容量域+适用旧owner/旧最后参数默认回归。
2. 同一formal336指纹和冻结82诊断fixture：root左右各12tick；1000 Sakura OID1 DataOrientedCanonical Dispersed/Combat各32tick，reuse OFF/ON均显式Role。逐tick全部既有20-domain/Lockstep hash、完整native scalar RNG、legacy state/calls、route与AI postcommit、预备数组和容量保持，实际AI1000；千人ON必须确有skip>0，否则保留反例、禁止窗口/推广。
3. 资格通过才唯一启动同版4窗：两布局各OFF/ON，120warmup+180sample，request除output一致、显式Role且其它未准入候选关闭，共享binding保持；85boundary timing均ON，用现有四边界计时，无新增生产hot计数。
4. Suite记录sample Editor观测的last completed input-pass skip/fullRefresh最大值/次数，只用于实际生效见证，不称全tick累计。校准完整同步StepOneTick logic GC、tick300完整checksum、容量拒绝/unresolved/stale/自然十一阶段关闭、原Menu clean与双Scene磁盘SHA；现有Windows请求保留真实显示FPS口径。
5. 收益根据同版cost比较；无可辨whole收益不默认采用。不把~0.95ms子段收益外推33ms成功。若短窗P95仍>=33或drop非0，H07仍FAIL，不刷1800正式证书；若满足门，才按原合同进入必要正式窗口。
6. Tools/Validate-ChangeLedger.ps1、git diff --check、逐文件before备份/保护SHA/HEAD/staged复核，保留全部失败原件。

## 风险、生命周期、边界、回滚
新增的owner仅Editor冷层持有既有World引用，两个bool只在tick0设置/结束或外部exit恢复，不新增runtime cache/queue/worker/lease，停止前先恢复，不改十一阶段主序。禁止热路径扩容及publication反写。风险为诊断owner交叉污染/未恢复、实际未触发却采样、末态相同冒充逐tick/正式EXE全域一致；上述gate分别覆盖。
World、87测试、Brute默认、Scene/Prefab/resources/Importer/Settings/Input/Gen/Plugins/Server/Q06活跃方法体、EXT1/Mono/ATLAS均不改。正式336/33ms/3ms/max2/RNG/sort/segment/fail-closed保持。
仅本包hunks可按独立批准恢复；准确当前dirty before副本是恢复源，禁止HEAD整体恢复/删除/覆盖失败。Goal active、5/6阶段、66已执行和34父项关闭0在实际新执行前不增加。

执行修订：窗口01首OFF完成但新冷owner错误要求已正常关闭的计时器仍live，Suite按原fail-closed自然退出PARTIAL。结束态和live态分开检查，新增三冷RED/GREEN及原80回归；Admission/World不变，原4Driver资格保持复用。88菜单下一使用新windows-02根，保持windows-01终态/失败原件，两已知根均只接受terminal。只补确实失效的四窗，不重复未受影响资格或扩大资源/collector默认范围。

