# 第52批：缓存组合真实实景结果

状态：SCOPED_WINDOWS_AB_COMPLETED / MIXED_RESULT / COMBINATION_NOT_ADMITTED / PERFORMANCE_FAIL / DEFAULTS_OFF。
Change NTSD-OPT-H07-COMBINED-CACHE-WINDOWS-052 的诊断接入限定 VERIFIED，不表示H07阶段或父项通过。

## 实际结果
原Editor四窗MEASUREMENTS_COMPLETED / DONE，每窗真实1000AI、120warm＋180sample、workloadValid true；eligibility全四窗300应用，kind5 OFF0/ON300，原四production/roster/cache/geometry300/noFallback。两flags配置/复原通过。
本机原Editor Unity2022.3.62f3，Windows10 19045 / i5-13600KF / RTX4070（driver32.0.15.9649）；硬件只作本次环境，GameView配置未改变且未另取其宽高，不升级Player/Android/120FPS证书。

| 工作负载 | logic mean OFF→ON ms | logic P95 OFF→ON ms | collector mean OFF→ON ms | display frame mean OFF→ON ms | inverse-mean FPS估算 OFF→ON | dropped OFF→ON |
|---|---|---|---|---|---|---|
| dispersed1000 | 92.624 → 117.744 | 139.017 → 162.293 | 53.406 → 62.770 | 368.252 → 341.129 | 2.716 → 2.931 | 1293 → 1181 |
| combat1000 | 79.295 → 76.518 | 106.821 → 113.060 | 48.919 → 45.763 | 251.922 → 228.716 | 3.969 → 4.372 | 774 → 664 |

分散logic mean增加27.12%、collector增加17.53%；混战logic mean减少3.50%、collector减少6.45%，但两ON P95 162.293/113.060ms都>33、drop1181/664非零。这一轮结果不支持两布局稳定收益，禁止组合推广；仅一次顺序OFF→ON/layout，不证明kind5必然导致负收益。分散未改CharacterInput14.086→19.416ms、LateEntityUpdate7.266→9.802ms也变慢，说明有跨pass时序/成本变化，外部干扰与原因UNKNOWN，不凭此臆造归因。显示帧倒数与logic方向不同，不用FPS估算掩盖logic反例。
scope不缩，不重跑同窗找PASS，不用51扫描省次数称实景时间收益。

## 适用正确性、生命周期、0GC
各布局20个extended/lockstep终态hash及tick300完整snapshot SHA同；Dispersed F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA，Combat E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。不是每tick/native全World位级验证；51资格指纹Query D253...和测试5894...保持，复用其88paired适用门。
各capacityCriticalDelta0、teardown.activeGameObjectsAfter/worldObjectsAfter/worldEntitiesAfter/claimedSlotsAfter/objectPoolActiveAfter/referencePoolActiveAfter0、cleanupExceptionCount0；Suite orderedShutdown true、remainingObjects/Slots/Borrowers0。实际不存在的central unresolved/stale独立字段不造0，父门仍待完整覆盖。
logicGC仍UNCALIBRATED_COUNTER / UNKNOWN，raw0不晋升。H11 43迟发12event FAIL不被本窗抵消；正式120+1800未过，不用冒烟替代。
SetPass平均约1994.315/1995，central submission draw约1991.281/1991.494，OFF/ON同；前者是全帧ProfilerRecorder指标，后者CPU/central提交口径，不等于真实GPU batch，不执行capture或instancing。

## 实际改动与验证
唯一C# Suite相对current-dirty副本189增/4删，SHA3772320A4DDAABA110D85D46A0A1D958AAD89D3B5D184E165A496C91B20C81C2/121537B；原Query、生产默认、旧family guard/关闭主顺序未改。11case RED jobd0af25459bf64bd1a4cd7912ad20be03 failed11；28case GREEN job5dece4bb9f3642ee83aa36ba0bcfa10c PASS28/28、0skip、2.9810608s。test-red-final-01是误命名running快照，真实终态是test-red-terminal-02，不删除旧原件。
四65字段请求仅outputPath改变、原seed/input/roster/layout保持；windows-request-freeze-01。原Editor只一次菜单执行，无第二Editor/重启/刷新在Play中/重复测试或测量。
58guards/8准确dirty与旧temp副本/HEAD/sourcefreeze全部同，terminal-source-audit-01；Temp最终与03terminal artifact完全相同、SharedRequest不存在。原Editorobserved1791392840974 idle/nonPlay/noTest，原Menu clean8roots；Battle/原Menu文件SHA保持，用户既有Git dirty不清理。
两只读辅助PowerShell第一次parser/manifest句柄误处理均未写入；修正只读命令成功，未重启Unity测量。源改动完成后未再改C#。

## 下一有效动作与阶段状态
PairExactLoop仍占collector约98%（本次52.492/61.506/48.363/45.217ms），collector绝对仍45–63ms，超过33ms整tick预算。组合不采用，下一在同普通Brute既有热循环根据现有cost核下一处可消除重复工作/更高杠杆，而不是再堆扫描次数证明、48微收益推广或重开H06/改变collector。必须先有成本/语义证据和独立Task/Change再改脚本，不先预设收益；H11事件定位亦保持范围内待办。
H07/H11 OPEN，阶段条件4/6、限定产物5/6、34父关闭0，22—52共31已执行子批，Goal active。EXT1/ATLAS/Mono/Role-aware生产推广等专项hold保持；次数只复盘，不是停止整个Goal的边界。

最终治理UTC2026-10-07T17:14:31.0434634Z：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity exit0，validatorErrors空；git -c core.safecrlf=false diff --check exit0/无输出。validation-final-02.json为摘要原件；首次完整warnings超过输出上限导致JSON解析失败，截断原件保留，不冒称完整日志。这里只重核治理摘要，无测试或测量重跑、未再改源。原件包括测试、4请求/报告/terminal/observations、完整checksum、sourcefreeze、审计与Editor前后状态。Goal新鲜active，未操作状态。
