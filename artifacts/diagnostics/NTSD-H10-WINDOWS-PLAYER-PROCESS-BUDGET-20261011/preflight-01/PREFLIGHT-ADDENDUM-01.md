# PRE-FLIGHT-01 具名供证与方案补齐（继续PLANNED）

本件补充原PLAN/PRE-FLIGHT/READ-SCOPE/H10-COVERAGE；原时点记录保留。Task/Change不变，没有代码/build/Player/Settings/sidecar执行许可。来源是主会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193独立审查后传入的具名供证；下表不是本执行会话重新打开方法体的记录。

## R1-R4供证与已确定最小方案

- R1 MatchConfig.cs7–30：gameMode引用、players、nativeBattlePlayerNames、nativeBracketPlayerNames、nativeKnockoutFeedRuntimeDisplayEnabled、backgroundId、difficulty、stageCampaignFilePath、stageSeriesId、seed、oneTuInput。诊断副本只把players设新非null空列表；全部标量照原值，gameMode同一引用，两个姓名列表按实际值复制且保持null/非null语义，不改源config。没有content/profile字段；这些身份独立记录，不发明字段。PlayerSlotConfig6–21为use/isHuman/characterId/team/inputId/aiId。
- R2 BattleTestBootstrap.Start62–76在已有AppManager时直接return；suppression原为true则先ready=false再在early return置true。OnDestroy529–535没有静态ready复位；App存在直接返回，否则处理TimeWheel。最小方案选择**启动资格要求suppression原值false，并始终保持false**，不写静态ready、不调用旧stress CLI host；异常true直接拒绝，不覆盖其它诊断状态。App空players和这个已证early return共同支持空World候选，仍实际检查对象/slot/两池active0，不删除任何角色来满足门。
- R2真实地图：BattleBootstrap38–41/43–125/134–139显示TryPrepare可能在无catalog/id时返回true但prepared仍false。每轮运行资格同时要求实际bootstrap、IsMapConfigurationPrepared=true、PreparedMapId非空、实际boundary定义与当前MatchConfig对应。关闭仍由App.DisablePresentation→IsRuntimeMapCleared→stage11，不能拿harness内部map=true冒充。
- R3 CharacterSelectionController.OnEnable121–129不等于ResetAll；StartBattle455–476同步slots、构造cfg、同步MatchConfirmed回调后App.Set/Load。ResetAll487–517重置step、background=-1/difficulty2/seed0、players/slots；每轮真实UI进入后先公开ResetAll、再SetSeed、再选角色/设置，不复用上轮Confirmed状态。SetSeed617只是赋值，不证明两RNG同态。MatchConfirmed只观察，不修改原config；combat副本的拦截/后续App调用时点仍需在新probe方案验证不会触发原非空Battle（不可在事件里偷偷篡改源）。
- R3 LoadingPrewarmController114–178有generation/running/Preserve；formal root非空走mgr.PrewarmConfiguredLoganContentAsync，root空才走legacy config/sprite任务。CharacterUIResourceManager65–69只是base和Dictionary，不是raw入口。当前不读GameDataManager legacy方法体来为copy找依据。
- R4 主会话额外已读Manager71–97 public PrewarmConfiguredLoganContentAsync：inactive/source检查、ProjectBattleModeConfig Snapshot、ResolveConfiguredLoganSource、调用Core。仅UI目录File/LoadSprite/bmp等rg无其它消费者的结果不能证明全域无raw依赖。

剩余准确范围见NEW-READ-DEPENDENCIES-01.md（R4A/B、R5A/B）和OBSERVATION-SEAM-REQUEST.md。新发现现有Windows postprocessor必须先核；当前2752清单是known domains，不是完整部署证书。没有读Q06/producer/factory/StageWave，没有递归解析全项目。

## 对原H10覆盖表的必要更正

H10卡的门是全catalog静态生产者加实际运行请求覆盖，以及有限代表单/多cue、持续技能、声道边界和多角色技能最坏集合；**不是每个producer/角色/技能动态穷举**。原表“全部producer动态覆盖”如被理解为全面穷举，应以此纠正。全量保守回退保持，未声明外部来源不据此默认准入；不为当前PC预算引入新的音频架构重审。

具体候选已经落到CASE-MATRIX-CANDIDATE.md：三轮各以2/16/507为P1，其余两人同队人类中性；鸣人持续053、Gaara j4→043、Kim j1 stereo。只需已知P1物理键，不增加进程/次数。普通seed建议682973786，与有据代表一致；千人原seed1314149188保留。完整回退的实际prepared资源集合承担资源包络，三角色只是有据代表，不能叫全游戏最大技能证明。

已从旧实际C#inventory抽取2/16/507/434和4cue，当前四WAV哈希全部与该inventory相等（case-evidence-inputs.json）。新LocalFreeRun技能动作/事件尚未运行；既有Manual精确tick/诊断坐标只作对应旧范围证据，不套成新窗预期数。

## 采样质量具体提案（供主审锁定，不作已测事实）

- 100ms绝对QPC deadline，不用上一采样结束再Sleep100导致漂移。每次记录read-start/read-end QPC、UTC、身份、private/WS、计划slot、lateness；两内存列分列不相加。
- 硬质量门候选：所有验收域读数有效且private>0/PID+CreationTime一致；相邻有效采样的read-midpoint gap最大250ms；每个warm/sample/transition连续域有效slot覆盖至少99%；域起止到首末有效样本距离均不超过250ms。slot按固定100ms网格编号，同slot多个样本只算1slot。短于100ms的过渡仍保留原采样，其间峰值不可观测就不宣称精确峰。
- phase带同QPC/Frequency。把read区间或相邻采样覆盖区间跨越的样本归入所有相关bucket，采用更严2GiB门；不把无法归属的高样本扔掉。未知开始/结束的runner active范围全部按steady2GiB判，完整冷启动/卸载按3GiB。采样间瞬时峰未知，结论只称100ms目标节拍的实际采样峰。
- 任一零/非法身份/缺采质量FAIL、私有内存超额、闭包拒绝、0GC/容量失败都停止后续轮/进程；不重复同版长窗刷PASS。保留全部有效与无效行、缺slot及间隔分布，不能只输出最大值。
- proposed256 phase rows、owned2048/round累计6144保持。代表输入表只1次组合/round，预分配状态；新事件tee若采用必须有独立准确容量/0GC/owner恢复资格。其成本是Player真实成本，不扣除。不能从旧63scope推导新tee0GC。

## 安全恢复及有限失败处置提案

唯一build仍只temporarily backend Mono，原API及真实字节guard/Operation/finally恢复不变；不SaveAssets/Burst改变/切target/第二实例。现有postprocessor输出先校验，不自动关它。Build45min watchdog只记录，不kill Editor、不自重启。

Player每组总3600s、UI480/120s、Battleprepare180s、warm/sample600s/round、close/tail120s上限保持。新增明确失败动作：新probe仅自己捕获的App公开close安全完成才申请Quit非零；外部sampler停止新采样并保存已知数据。若Player无响应或close未完成，方案默认不kill、不动其它进程，保存准确PID+CreationTime及UNKNOWN_REQUIRES_DISPOSITION；只有后续明确批准才可终止该精确诊断进程。无需为了收尾发明自动kill权限，不把未退出计为PASS。

启动sidecar必须作为本Task所有者先就绪，再启动唯一Player并绑定Process返回身份；进程运行与采样协议尚未实现。sidecar自己的PS分配不计PlayerPrivate，probe/声音观察分配仍在Player预算内。所有运行输出预先拒绝已有同名，已有harness自然mutable结果每轮前备份/终态snapshot。不能覆盖原项目Temp/旧证据。

## 已完成的实际检查

`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`已运行并退出0：1423 records、4534 warnings，其中本PLANNED记录3条是拟新增脚本尚不存在/无diff；本轮已直接核原日志对应行。没有把它写成0warnings。

protection-audit.json：4094受保护条目、406旧H10 artifact、18源/程序集/Settings项无漂移，备份错误0/非治理dirty漂移0，HEAD相同/暂存空/原10tracked dirty同集合，旧untracked未丢失，新三脚本/两meta/build-01仍不存在。此结果是13:20保存的检查，后续文件补充另有最终核对，不能冒称已替代最终检查。

本轮没有调用read_thread、process CommandLine/environment，没有新Unity命令/编译/测试或进程重启。开头“重启后安全状态”沿用旧消息的措辞已澄清为未观察/未执行重启，不作为事件事实。额外一个带猜测名称的旧windows-player-plan.md路径检索不存在，只是只读定位失败；没有构造或恢复该文件。

下一是主审具名供证/复核新read依赖与具体矩阵、锁定完整manifest/质量/恢复，然后才裁决3新文件实现。H10父项持续OPEN，不默认暂停，不推进其它优化点。
