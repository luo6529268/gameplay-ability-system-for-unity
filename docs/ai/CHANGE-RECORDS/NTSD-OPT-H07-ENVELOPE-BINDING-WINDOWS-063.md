<!-- CHANGE-RECORD
id: NTSD-OPT-H07-ENVELOPE-BINDING-WINDOWS-063
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: approved six-item optimization continuation and effective goal Task sections0-8; Batch63 new envelope ON rejected-binding OFF ON real Windows comparison; formal336 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH63-ENVELOPE-BINDING-WINDOWS-20261008/REPORT.md
-->
# 第63批：包络＋拒绝绑定复用实景对照接线

最终治理 UTC2026-10-08T00:59:15.5998120Z：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 exit0/PASSED/1359records/4252历史WARNING/0ERROR；git diff --check exit0/26CRLF提示/无其它。最初只读摘要regex误匹配所有含Record的warning导致工具输出截断/JSON解析失败，不是validator失败；改窄为两summary行后重新审计成功，无测试/测量重跑。第64批既有参与资格复用与63组合的必要资格/四窗已独立Task READY；未改runtime或默认，不计新执行批次。

SCOPED_WINDOWS_AB_COMPLETED / GAIN_SIGNAL / PERFORMANCE_FAIL / NOT_ADMITTED。四actual1000AI/120warm+180sample/300ticks均有效、两侧包络ON，仅绑定复用OFF/ON；logic mean73.653375→70.766603ms（3.9194%）/74.766059→71.202391ms（4.7664%），ON P9588.318355/92.455340ms、drop619/598仍FAIL。两组20终态hash与完整snapshot同；四前后校准有效完整StepOneTick稳态180 scopes/0 GC.Alloc events，0B由zero-event证明，不TimeNanoseconds换算、不替代H11完整camera。双flag未漂移/实际应用符合OFF0、ON有probe/reuse，均恢复原false；capacity critical0、正常11阶段对象/slots/borrowers及其余记录残留0、双Scene不变/原Scene恢复。单顺序AB只为增量收益信号，不承诺稳定比例/生产FPS。候选default OFF/61字节预算UNKNOWN；正式1800与central独立unresolved/stale/H11门未过，不追加1800刷PASS、不开专项、不停Goal。42已执行/阶段4of6/父关闭0。实际原件见同批windows-final-summary-01.json，终态比较/DrawMesh与全帧SetPass分离见windows-final-comparison-01.json；源190378B SHA03EECC43A5DF30130465A4EF8FB0CCE0D3150149AEB52E30BF806D0350E93F71，精确dirty diff520增5删。109guards/9backup/HEAD于00:54:02Z全部保持，最终文档后validator/audit另记。以下WINDOWS_RUNNING/READY等均为当时历史观察，已被本终态取代。

WINDOWS_RUNNING：get_editor_state observed_at_unix_ms1791420195822确认原Editor Battle Play=true/changing=true、无compile/test，域已重载；不据此前Connect refused判终止，也未重新launch。四实际采样结果仍待，当前只证明同Suite/原Editor仍live。以下LAUNCH_SUBMITTED为先前观察，不再表示未取得Play证据。

RUNTIME_PENDING / WINDOWS_LAUNCH_SUBMITTED：唯一原Menu执行已返回，四CreateNew request实存，各65字段除output同60/1000AI/120+180/seed1314149188/brute/noWorker。源SHA 03EECC43A5DF30130465A4EF8FB0CCE0D3150149AEB52E30BF806D0350E93F71与109guards/HEAD冻结；原PID19040仍存，进入Play域重载期间连接拒绝尚未取得新鲜Play/tick证据，不称实际窗口有效/收益。freeze中的envelope/binding是矩阵期望，不是actual应用，须后续RunState字段证明。实际request身份见windows-request-source-freeze-01.json，公共SharedResult原件已事前副本。运行期间不改C#/导入/重启或重发菜单。另一次重排总表hunk导致EndPatch不在最后、验证拒绝零写入，已正序重建成功，无文件丢弃。

FOCUSED_TEST_PASS / WINDOWS_READY：原Editor job ea63b3cd510045be831a3d249013a18b succeeded，83/83（新39＋旧60同域29＋label12＋production纯3）、0skip、6.069347s。原GREEN XML 63657B SHA 15031FC32202ED7236EB5EAA960F668F5C68F106FFF72B23C5B9E6BC4666097D已freshcopy，filter/实际83case在同批test-green原件；树9833不是执行数。源SHA 03EECC43A5DF30130465A4EF8FB0CCE0D3150149AEB52E30BF806D0350E93F71；旧60八方法逐文本与准确dirty副本相同，109guards/9backup/HEAD保持。原Menu clean8roots idle/noPlay/noCompile/noTest；pwsh validator0/4252历史WARNING/0ERROR，diffcheck0/26CRLF/noother。42已执行、阶段4/6、H07/H11 OPEN、Goal active；尚无新实景/FPS/0GC。下一新组合四窗，源冻结/不导入重启。一次整份文档patch重复指定INDEX导致验证拒绝，零文件写入；已按单文件合并hunk，不删除原件。

受影响异常恢复补两具名case：BruteEnvelopeBinding_ExitRestoresAndRecordsOnlyCurrentRun(False/True)，验证当前Run的双restore日志，以及completed runIndex不越界、仍解除owner/恢复flag。此两case是37有效RED后的实现复核覆盖，不声称它们有独立RED；本批新39case，窄GREEN另保留旧同域门。未执行外部退出实景。

接线静态复核补充：新RestoreBruteEnvelopeBindingForExit在异常ShutdownAndExit/外部ExitingPlayMode入口先恢复双flag并写当前RunState结果，再执行可能失败的GC Finish；避免Finish异常跳过新owner恢复。Complete仍先检查漂移再恢复；旧60 owner方法/顺序不改。这是本已声明异常恢复接线，不新增生产模块或更改关闭十一阶段；定向异常运行尚未验，不能声称整体Play关闭通过。

CODE_WRITTEN：唯一Suite新增独立mode/4request root、双flag previous/apply/unchanged/observed/restored与最大计数、正常完成/异常/外部退出restore接线。BeginSuite17参数及旧CoarseEnvelopeQueryDefaultsValid/Apply/Observe/Complete/Restore全文保持；新calibratedscope仅新mode允许已owned绑定配置，旧逻辑路径仍要求binding false。没有新Runtime/Query/Harness/Observer/Admission修改或实景启动；下一37新＋旧60/label/production同域GREEN，尚无编译/收益结论。

有效RED：原Editor job f5706fd8e6a1445b9c2aea98bfa934de terminal failed；37total/0pass/37fail/0skip，duration 4.477172s，新方法或字段缺失为预期首差。原XML 58817B/SHA 6E8821E412F2A6B70AAD8F722FC50881875F6D930A3BBDC58B80B08152ADBB53已freshcopy核同，完整37case见test-red-summary-01.json；树9831不当执行数。导入重载期间一次Connect refused没有提交作业，随后同Editoridle才启动唯一RED，无重启/清除。

TESTS_WRITTEN：唯一Suite既有Test类新增37case（请求4/越界1/menu1/mode1/前置5/apply2/prior restore4/drift4/其它flag6/owner2/准入6/max1）；全部使用反射定位新接口，当前生产接线尚未写。下一最窄BruteEnvelopeBinding* RED，不跑旧Driver或实景。

PLANNED；当前仅事前留痕，没有本批脚本、测试或实景收益。需求来自第62批组合Driver资格与第60批单包络收益/性能失败；不是重采旧窗口。六阶段4/6，41已执行，H07/H11 OPEN，Goal active；候选默认false/NOT_ADMITTED、61字节预算UNKNOWN保持。

唯一C#：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs。准确影响：新增BruteEnvelopeBindingOutputRoot、startingEnvelopeBindingWindow、envelopeBindingQuery/两个previous值、SuiteState.bruteEnvelopeBindingCandidate、RunState绑定应用/不变/恢复/抽样Last计数；新BeginBruteEnvelopeBinding/BuildBruteEnvelopeBindingRequest/Apply/Observe/Complete/Restore/ObservationValid；在BeginSuite/BuildCurrentRequest/Update/StartCurrentRun/ObserveRunningSample/ShutdownAndExit/OnPlayMode/IsOwnedTerminalOrAbsent最小接线。原BeginSuite17参数、旧60 owner严格binding OFF、旧60请求和tests不放宽。

冷态新模式独占原Suite。两侧envelope ON，只binding OFF/ON；Apply在任何tick前保存原值，完整期间两个flag及四production defaults/其它flag必须保持；Complete先检查后恢复；异常与外部Play退出也幂等恢复，解除owner。观察只取Last字段的抽样最大值与实际applied，不声称300tick累计。0GC观察不改原完整StepOneTick scope，不添加生产hook/owner。

先在同文件既有Test类新增具名BruteEnvelopeBinding*请求矩阵/越界、菜单17签名/coldownership、旧JSON默认为false/路由、互斥前置、两开关apply-complete/恢复、漂移/其它flag/双owner/观察准入和max语义。先缺接口RED，不制造编译错误；GREEN只新测试＋旧60同域29＋label12/production request/default3，不跑完整campaign。Query/Buffer/Harness/Observer/Admission只读，62Driver资格依指纹复用。

固定后继一次四新实景：Dispersed1000/Combat1000各OFF/ON，120warm+180sample，seed/input/roster/profile/33ms/ForceBruteForce/noWorker/原request字段除output同60；四既有admitted flags true，kind5/eligibility/coarseDispatch/timingfalse。新CreateNew目录windows-01，不覆盖旧原件。完整Driver GC前后empty/known-positive校准保持，不TimeNs转字节/不运行61无效API。若本轮仅接线与聚焦通过，必须明确实景尚未启动，不称收益。

验收：正确请求和owner/异常恢复、窄测试、可靠GC覆盖、真实1000AI/完整300tick、flag实际应用/恢复、capacity拒绝0/declared终态20hash与完整snapshot对照、11stage残留0、SceneSHA/dirty不变；logic mean/P95/collector/frame/dropped如实报告。短窗明显FAIL不加正式1800刷PASS，不自动推广；无收益据原热点改向而非停止Goal。正式性能/byte budget/central独立门/完整camera H11仍待，非Android/120FPS认证。

保护：正式336/33ms/3ms/max2/checksum/RNG/input/pass/publication只读/透明segment/failclosed/seal/十一阶段与Join。Scene/Prefab/资源/settings/Input/Gen/Plugins/Server不改，Q06只hash。EXT1/Mono/ATLAS不解冻；不删除/move/Git破坏/push/新Editor/重启。

事前UTC 2026-10-08T00:28:24.7992070Z / HEAD 45bbed41c64e0599601fa0df4028072d9f83303a。9个准确current-dirty已有文件＋109保护见Operation before-manifest-01.json；先fresh Copy-Item每份核SHA，之后才写现有治理和C#。公共XML原62GREEN与SharedResult原60终态均先保护。回滚来源为该9份原字节副本；需另批准反向本批hunk，不Git恢复/覆盖用户工作。
Operation ../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH63-ENVELOPE-BINDING-WINDOWS-20261008/RECORD.md。
实际实现/测试/未验证/失败随后追加，不把PLANNED当完成。
