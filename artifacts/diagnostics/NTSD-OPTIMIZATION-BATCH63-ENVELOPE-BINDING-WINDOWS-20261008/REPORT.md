# 第63批：包络＋拒绝绑定复用组合实际收益对照

## 最终结论

SCOPED_WINDOWS_AB_COMPLETED / GAIN_SIGNAL / PERFORMANCE_FAIL / NOT_ADMITTED。四actual1000AI/120warm+180sample/300ticks均有效、两侧包络ON，仅绑定复用OFF/ON；logic mean73.653375→70.766603ms（3.9194%）/74.766059→71.202391ms（4.7664%），ON P9588.318355/92.455340ms、drop619/598仍FAIL。两组20终态hash与完整snapshot同；四前后校准有效完整StepOneTick稳态180 scopes/0 GC.Alloc events，0B由zero-event证明，不TimeNanoseconds换算、不替代H11完整camera。双flag未漂移/实际应用符合OFF0、ON有probe/reuse，均恢复原false；capacity critical0、正常11阶段对象/slots/borrowers及其余记录残留0、双Scene不变/原Scene恢复。单顺序AB只为增量收益信号，不承诺稳定比例/生产FPS。候选default OFF/61字节预算UNKNOWN；正式1800与central独立unresolved/stale/H11门未过，不追加1800刷PASS、不开专项、不停Goal。42已执行/阶段4of6/父关闭0。实际原件见同批windows-final-summary-01.json，终态比较/DrawMesh与全帧SetPass分离见windows-final-comparison-01.json；源190378B SHA03EECC43A5DF30130465A4EF8FB0CCE0D3150149AEB52E30BF806D0350E93F71，精确dirty diff520增5删。109guards/9backup/HEAD于00:54:02Z全部保持，最终文档后validator/audit另记。以下WINDOWS_RUNNING/READY等均为当时历史观察，已被本终态取代。

| 工作负载 | binding OFF logic mean/P95 ms | binding ON logic mean/P95 ms | collector mean OFF→ON ms | PairExactLoop mean OFF→ON ms | dropped OFF→ON |
|---|---|---|---|---|---|
| Dispersed1000 | 73.653375 / 115.039480 | 70.766603 / 88.318355 | 42.447571→40.373297 | 41.879162→39.799789 | 690→619 |
| Combat1000 | 74.766059 / 93.628075 | 71.202391 / 92.455340 | 44.168224→41.974861 | 43.566372→41.444106 | 632→598 |

显示平均234.544925→218.996377ms、221.452524→212.984938ms；倒数估算4.264→4.566、4.516→4.695FPS，不是Present计数或Player/Android/120FPS证书。四request65字段除output同旧60相应基线、seed1314149188/ForceBruteForce/noWorker/四admitted flags true/其它candidate和timing false。63两侧只新binding值不同，由RunState实际应用/恢复证明，freeze矩阵期望本身不作为运行证据。

每窗中央计数有89有效frame：Dispersed两侧CPU submission DrawMesh平均1991.280899、物理segment1990.280899，全帧Profiler SetPass1994.314607；Combat两侧分别1991.494382/1990.494382/1995.000000。这些是各自既有计数，不能把CPU命令或全帧SetPass直接称真实GPU batch；本批无GPU capture、没有中央合批收益。summary的reportedDrawFields为空只是按旧prefix筛选未命中，不能据此声称没记录draw，已由comparison重新取准确字段。

ON抽样Last maxima：Dispersed probe1000/reuse110222，Combat probe1000/reuse105987；OFF均0。不是300tick累计。每窗完整Driver覆盖300有效scope，稳态180/invalid0/GC.Alloc0events，前后empty和known-positive校准均过；单位TimeNanoseconds只作marker元数据，0B由零事件推导。仍不覆盖H11完整物化—上传—录制—提交。

两组终态20hash分别完全相同；Dispersed完整snapshot SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA，Combat E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。不是逐tick/native全World证明；62适用Driver组合资格依109guard内Admission/原证据指纹复用。正常四退出均无cleanup异常/World entities和objects/slots/GameObjects/两pool残留0；不声称异常用户退出已实景覆盖。

下一动作依据：PairExactLoop仍39.799789/41.444106ms，占collector约98.6%—98.7%；继续同已批准Brute路径的有据复盘，而非重复本窗、重跑已过资格、再测61无效API或自动切Role-aware/EXT1。正式1800、可靠byte budget、central独立门与H11完整scope保持未完成。

## 历史运行记录（保留当时事实，非当前状态）

运行追加：WINDOWS_LAUNCH_SUBMITTED / RUNTIME_PENDING。原Menu已返回，四fresh request实存，65字段除output同原60；源/109guards/HEAD冻结。原PID19040仍在，域重载连接暂拒，尚未取得新鲜Play/实际tick证据，不把菜单成功或冻结文件当有效运行。后续只观察同一Suite，严禁重复launch/改C#/导入/重启；性能/GC/flag实际应用/关闭仍待。以下WINDOWS_READY为事前事实。

FOCUSED_TEST_PASS / WINDOWS_READY / GAIN_UNKNOWN / NOT_ADMITTED。只对照接线通过，不是新FPS或0GC证书；H07性能FAIL/H11完整scope FAIL及61byte budget UNKNOWN保持。42已执行、阶段4/6、34父关闭0、Goal active。

唯一修改C#为既有Suite；独立新组合mode，两侧envelope ON、binding OFF/ON，原BeginSuite17参数和旧60八方法全文保持。双flag owner/previous/apply/unchanged/restore与抽样Last max；正常Complete核验后恢复，异常/外部退出先restore记Run再Finish GC。Query/Harness/observer/Admission/生产默认不改。

有效RED：37case全部缺新接口/字段，原XML58817B SHA6E8821E412F2A6B70AAD8F722FC50881875F6D930A3BBDC58B80B08152ADBB53已freshcopy。实现复核另补两退出case，无独立RED。GREEN ea63b3cd510045be831a3d249013a18b succeeded，83/83（39新＋29旧60＋12label＋3production纯）、0skip/6.069347s。原GREEN XML 63657B SHA 15031FC32202ED7236EB5EAA960F668F5C68F106FFF72B23C5B9E6BC4666097D；filter/实际83case见原件，不以树9833作执行数。

事前9dirty副本/109guards/HEAD保持，原Menu clean8roots idle；源SHA 03EECC43A5DF30130465A4EF8FB0CCE0D3150149AEB52E30BF806D0350E93F71冻结。旧60八方法全文相同，pwsh validator0/4252WARNING/0ERROR、diffcheck0/26CRLF/noother。Scene/资源/Gen/Plugins/Server/authority不改，Q06只hash，无第二Editor/安装升级/破坏性Git/删除/move/push。一次重复INDEX patch验证拒绝零写入已纠正，原失败保留。

下一一次四新窗口：Dispersed1000/Combat1000各binding OFF/ON，120warm+180sample、两侧envelope ON/四production机制true/ForceBruteForce/noWorker，其它candidate/timingfalse，原65request字段仅output不同。完整StepOneTick GC沿原前后empty/positive校准，不TimeNs转bytes/不复跑61 API；Last字段仅抽样max，不称300tick总量。收益/GC/容量/关闭/终态parity仍待，末snapshot不替代62逐tick资格。

运行中不改C#/导入/重启；短窗明显FAIL不加1800刷PASS，无收益不采用，不停止Goal。正式性能/central独立门/bytes/完整H11/Android/120FPS不自动认证。
