# Current status addendum for recovery candidates

This note supersedes historical status snapshots in the Git HEAD baselines where they conflict. It does not claim to reproduce lost uncommitted text byte-for-byte.

- The latest authorized content scope is formal NTSD 2.8-Logan runtime DAT and role-related images, except native background DAT, background-mode DAT, and both mode DAT families. Project background/map/mode content and the user's visual exceptions remain. Never alter DAT field values without the user's explicit request.
- BATCH-01/Q01 delivered. BATCH-02/Q02–Q05 delivered at their stated load/contract/retirement/schema exits. BATCH-03/Q06 delivered scoped, not full battle equivalence. BATCH-04/Q07 and Q08 remain in progress. BATCH-05/Q09 and Q10 have independent partial evidence, not aggregate exits. BATCH-06/Q11 and Q12 wait on prerequisites. The total battle-alignment goal remains active.
- Q07 current byte audits: 353 nonexcluded formal DAT are in scope, 337 staged and identical by same-path SHA, 16 remaining with separate owner/scope decisions. The latest addition is formal `data/sound.dat` (397 bytes, exact SHA, 18 ordered entries), staged without WAV/audio-code changes under `NTSD28-Q07-SOUND-TABLE-DAT-STAGING-001`; its 18 WAV paths all exist in formal VFS and 0/18 exist in staged VFS, so Q10 audio deployment/playback remains pending. The 1,010/1,010 indexed images are staged and SHA-identical, of which 104 `smallb` HUD images are excluded from runtime replacement. The old 138 indexed DAT and 383 indexed images remain present with zero deletion authorizations. The 174 other old image paths require owner classification; 164 exist and no bulk deletion is authorized.
- Q07 current serialized-owner correction: the committed Battle Scene's inactive editor preview already points to formal Sasuke `sasu.png` and source rect `(0,881,79,79)`; its Inspector sample and focused test also use the formal path. A fresh 933-file GUID scan found 37 of the prior 52 old-image rows still serialized (39 file-reference pairs); 15 prior rows have no current serialized GUID reference. All 14 current Battle Scene and 40 Menu Scene nonzero `m_Sprite` GUIDs resolve to present `.meta` files, so the missing old GUIDs have not left a serialized missing-Sprite slot. This is static binding evidence, not Play appearance. The older proposed preview rebind is historical, while dynamic-reader and per-file deletion gates remain open. Exact evidence: `artifacts/diagnostics/NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001/CURRENT-SERIALIZED-OWNER-REFRESH-20260925.md`.
- Q07 excluded-content caller check: none of the 52 user-excluded native background/mode DATs is present in the staged `decoded_dat` root. Configured production prewarm captures `ProjectBattleModeConfig.asset` and passes its snapshot through candidate/catalog creation and freshness re-read, so that selected call chain does not read native mode DATs; no background DAT reader was found in production scripts. The public no-snapshot catalog API still permits explicit historical/diagnostic mode-DAT reads and remains a future-caller risk, not a reason to restage excluded inputs. Exact bounded evidence: `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/PRODUCTION-CALLER-CLOSURE-20260925.md`.
- Q07 direct Battle test bootstrap rejects an empty formal content root in current source and retains formal prewarm for a nonempty root. Independent Roslyn compilation using Unity-generated response files passed for runtime and Editor assemblies (0 errors; 24 and 6 warnings respectively). Later the original Editor PID 11944 imported the current test source: its first two-test request returned 1 pass/1 error because the historical formal-root helper's separate Temp PNG fixture was absent; the new empty-root test passed. After reproducing that fixture in Temp, a second unique request returned **2 passed/0 failed** for the two exact focused tests, with Menu/Battle Scene SHA stable. `NTSD28-Q07-EMPTY-ROOT-BATTLE-ENTRY-001` is `FOCUSED_TEST_PASS / GOVERNANCE_BLOCKED / DIRECT_BATTLE_PLAY_PENDING`; full Q07 remains open. Evidence: `artifacts/diagnostics/NTSD28-Q07-EMPTY-ROOT-BATTLE-ENTRY-001/original-editor-focused-20260925/ACCEPTANCE.md`. Menu/Inspector/preview/SelfCheck historical empty-root paths are separately owned.
- `Tools/Validate-ChangeLedger.ps1` last failed because six unrelated active Change IDs were absent from the NUL-truncated live `docs/ai/STATE.md`; all six already appear in the readable Git HEAD baseline. The recovery candidate additionally records post-commit `NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001` as `IN_PROGRESS` per its current Change Record. Candidate coverage is a text-level check only; the now-corrupted live progress files prevent a valid new governance run until recovered. Do not state that the Q07 package is deliverable.
- Menu and Battle Scene disk SHA-256 remain `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. No Scene, DAT, image or unrelated UI source was changed by this recovery package.
- The three live progress documents were observed at 01:17:40 local time as short UTF-8 prefixes followed by NUL bytes. Exact damaged bytes and the last committed baselines are preserved in this directory. A recovery candidate needs explicit review because uncommitted post-commit details cannot be reproduced exactly from Git HEAD alone.

---

> 2026-09-24 D-024最新：d024-ai-child-play-10 原Editor正式内容180完整tick，indexed最近目标检索在tick76/90/120/180各1次，AI目标仍-1、无子体；提交phase call计数缺记录，不作未提交证据。下个有界出口为同状态Legacy/DataOrientedCanonical候选与committed/fallback首差，停止重复整段Play。Change NTSD28-USER-D024-AI-CHILD-SCENE-PLAY-001仍RUNTIME_PENDING；Q07后置。

> 2026-09-24 D-024 恢复点：原项目 Play 请求 d024-ai-child-play-9 完成正式内容预热、OID2/7 创建、180 个生产 Driver tick 及有序关闭，AI 计数 1/tick 但目标 -1、无子体；tick76 后玩家 HitStop0/HP500/RelationTeam1/type0。原因未定，下一步同状态 Legacy/DataOrientedCanonical 首差；Change NTSD28-USER-D024-AI-CHILD-SCENE-PLAY-001 仍 RUNTIME_PENDING。两 Scene SHA 保持，Battle Scene 两相机序列化启用；未改生产脚本、Scene 或 DAT。Q07 后置，D-024/总目标开放。

> 2026-09-24 Q09/P-04相机更正：用户确认原Battle Scene的HUDCamera与ScenesCamera默认都应启用，并指出本任务导致前次两字段关闭。已精确恢复两处m_Enabled=1、Scene磁盘SHA回到9409F2...3B3A39、Git Scene diff为空；原Editor重新加载且Scene clean。受控Play可见性探针于20:25:31 PASS：ScenesCamera enabled/active true、central plan与submission有效、source commands4/draws6、firstDifference=NO_DIAGNOSTIC_DIFFERENCE；退出后Scene哈希仍保持。此前“并行写入者未证/待用户确认/不回退”记录以本条覆盖。BattleBootstrap.OnDisable是可能调用链，但独立Play未复现写盘触发，未改生产脚本。证据：artifacts/diagnostics/NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001/ACCEPTANCE.md。P-04遮挡像素及正式EXE A/B仍待。

> 2026-09-24 P-04并行Scene变动更正：同Z Play探针结果写于20:12:16，退出后首次磁盘核对Battle Scene仍为SHA 9409F2...3B3A39；20:14:05 Scene后续被写为7D7286...0E567，当前Git diff仅两台Camera的m_Enabled 1→0。写入者/意图未证，不回退、不归因探针。新排序见证仅证明原Scene当时的受控物化顺序；后续像素验收必须先复核当前相机可用性。

> 2026-09-24 NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001 / VERIFIED_SCENE_ORDER_ONLY：原Battle Scene真实Play中CentralOnly生产World的受控同Z排序在tick5为slot51 rank0/order1、slot50 rank1/order5，两个handle generation1，对象/占用slot 4/2→4/2、pause恢复；Editor已退出Play且Battle Scene磁盘SHA不变。原Editor新脚本编译0错；首轮45秒启动等待超时与正式内容加载有关，保留首轮FAIL，第二轮180秒有界等待PASS。探针显式物化、夹具无sprite/绘制命令，故自然LateUpdate/GPU遮挡像素、Legacy视觉出口及正式EXE同场景仍待；P-04/Q09/总目标不关闭。详artifacts/diagnostics/NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001/ACCEPTANCE.md。

> 2026-09-24 NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001 / PLANNED：P-04原Battle Scene旧R07B探针依赖旧DAT `hit_Fa` 前置而未给同Z见证；已建独立test-only Task/Change，以原Scene生产World的临时同Z实体和central materialized order补运行时顺序证据。当前仅合同，无脚本/Play结果；像素遮挡、Legacy视觉出口与正式EXE对照仍是后续出口。

> 2026-09-24 NTSD28-Q09-SAME-Z-PAINTER-ORDER-001 / FOCUSED_TEST_PASS / RUNTIME_PENDING：按正式playable entity_commands 的同Z大物理slot先绘，已更正Unity Stage/Coordinator与central radix的战斗表现排序，冻结快照保持原物理slot存储；原Editor编译0错、定向14/14、fresh SelfCheck PASS。原Battle Scene旧R07B探针未建立正式内容下夹具，退出结果FAIL不构成P-04反证；遮挡像素/正式EXE同场景仍待。Scene哈希不变，DAT/相机/逻辑/非战斗未改。全量EditMode误触发提前失败不作本包证据。详artifacts/diagnostics/NTSD28-Q09-SAME-Z-PAINTER-ORDER-001/ACCEPTANCE.md。Q09/R16/总目标仍开放。

> NTSD28-Q09-SAME-Z-PAINTER-ORDER-001 / PLANNED：正式playable的entity_commands同深度先绘较大物理slot，Unity Stage/Coordinator比较器和central radix快路均较小slot在前，P-04首差确认。已建Task/Change/Ledger，下一步test-first只改这两战斗表现脚本及两定向测试/探针，深度顺序、独立sprite/spark源数组和逻辑真相不动；正式例外与Q09总目标开放。

> 2026-09-24 NTSD28-Q09-C01-PLAY-PROBE-RNG-001 / VERIFIED（仅测试探针）：原Battle Scene CentralOnly真实Play四tick 1928-1931 PASS；每tick正式原生CRT 2次、共享RNG武器门槛1次，HitRecord物化命令1/2/3/0，C01年龄与Late幂等通过，退出时两流和对象/slot/pool等基线恢复。原Editor编译0错、Battle/Menu Scene哈希不变。旧探针3次共享RNG断言已按Q06正式双流合同更正；正式EXE同种子像素、legacy出口和30/60/120采样仍待，Q09/R14/R17与总目标开放。证据：artifacts/diagnostics/NTSD28-Q09-C01-PLAY-PROBE-RNG-001/ACCEPTANCE.md。

> NTSD28-Q09-C01-PLAY-PROBE-RNG-001 / PLANNED：原Battle Scene C01 Play探针仍把命中火花的原生CRT两次抽样算进共享world.Rng，旧断言3次与现Q06生产双流合同冲突。已建独立Task/Change/Ledger；拟仅改该Editor测试探针，逐tick记录原生CRT两次Y-X与共享武器门槛一次，并恢复两流基线。不改生产逻辑、DAT/图片/Scene；Q09与总目标开放。

> 2026-09-24 NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001 / RUNTIME_PENDING：正式SPARK.png黑键及20个有效格已通过原Unity Editor编译、几何/资源测试、330对象正式发布回收和BattleRuntimeSelfCheck；raw 0..99 ID在legacy与central出口共用可绘制映射，DAT/Scene/战斗逻辑未改。Battle Scene C01自然命中Play探针在tick3573先遇RNG delta 1 vs旧夹具期望3，尚未到SPARK画面断言；需按当前正式命中链审计该首差，另做两出口Play、30/60/120采样和正式EXE像素对照。Q09/R14/R17及总目标未关闭。证据：artifacts/diagnostics/NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001/ACCEPTANCE.md。

> NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001 / IN_PROGRESS：正式全局SPARK发布和两个战斗渲染出口接线，脚本前Task/Change/Ledger已建。原始0..99 ID仅在可绘制资源查找时压缩为现有20键；不改C01逻辑生命周期、不改DAT/Scene/非战斗与已排除HUD，旧BMP分支保留。原Editor编译、聚焦与Play/正式像素尚待；Q09/总目标开放。

> 2026-09-24 Q09正式SPARK输入前置已验收：NTSD28-Q09-NATIVE-SPARK-INPUT-001 / VERIFIED。候选现在捕获resource.dat索引43的正式SPARK.png、system.dat的99x79及图像SHA，三类变化使旧候选失效；正式与暂存候选身份1/1、原Editor候选8/8及追加变更反例1/1 PASS，编译0错、Scene哈希不变、Ledger785/21 PASS。角色图候选仍906，旧SPARK.bmp发布和两个绘制出口尚未替换；下一Q09须接正式PNG黑键、原始ID稀疏裁切和legacy/central消费，再验Play/正式像素，R14/R17与总目标仍开放。详artifacts/diagnostics/NTSD28-Q09-NATIVE-SPARK-INPUT-001/ACCEPTANCE.md。

> NTSD28-Q09-NATIVE-SPARK-INPUT-001 / IN_PROGRESS：Q09正式全局SPARK输入捕获子包。脚本前Task/Change/Ledger已建；仅将resource.dat索引43、system.dat宽高与SPARK.png SHA纳入正式候选身份/新鲜度，保留906角色图候选与旧20图发布直到后续正式接线。原HUD/KO feed例外不受此包影响；Q09及总目标开放。

> 2026-09-24 Q09 smallb权限纠正已验收：NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001 / VERIFIED。依据P-17，原生角色HUD为USER_EXCLUDED；已精确撤销误加的smallb候选、解码、发布和肖像API，生产候选恢复906张，磁盘正式/暂存图片1010/1010逐SHA证据仍有效。原Editor编译0错、聚焦5/5、候选7/7、Ledger与diff检查通过，Scene SHA不变；旧smallb发布记录仅为已SUPERSEDED历史。项目Battle HUD、HeadImg和用户HUDBg x30不变。后续仅推进非排除Q09战斗表现与Q07资源引用闭环，Q09及总目标均未关闭。详artifacts/diagnostics/NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001/ACCEPTANCE.md。

> 2026-09-24 smallb权限更正：本表§0.2/§1.2及P-17、DECISIONS已将完整原生角色HUD列为USER_EXCLUDED。104张smallb正式图片的磁盘部署/SHA 1010/1010事实保留，但其HUD消费不是Q09/R17待修项。前两条把静态HUD首差误判为必修，并曾新增smallb运行时发布；该建议已被本条覆盖，代码正按NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001精确纠正。项目原Battle HUD、HeadImg、用户HUDBg x30保持，不重做非战斗UI。

> NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001 / IN_PROGRESS：P-17明定完整原生HUD用户排除；前包smallb发布属于误扩范围。已先建Task/Change/Ledger，拟仅撤销本代理新增的smallb候选/解码/发布，保留正式1010图的磁盘SHA证据和项目mode Asset测试修正，不改Scene、DAT、图片或非战斗行为。

> 2026-09-24 NTSD28-Q09-SMALLB-PUBLICATION-001 / FOCUSED_TEST_PASS：正式smallb图已进入候选SHA、预热与原子发布，独立BattlePortraitSprite供后续HUD读取；正式/暂存候选各1010图，原Editor聚焦5/5、330对象发布回收1/1、候选类8/8 PASS。Battle Scene HeadImg仍为静态small图，HUD槽消费者/Play像素/正式EXE同场景未验；Q09/R17/总目标保持开放。详 artifacts/diagnostics/NTSD28-Q09-SMALLB-PUBLICATION-001/ACCEPTANCE.md。

> NTSD28-Q09-SMALLB-PUBLICATION-001 / IN_PROGRESS：脚本修改前Task/Change/Ledger已建；准确范围为候选哈希、smallb图预热/原子发布和5项聚焦夹具。正式HUD选择优先smallb，缺失回退small；Scene消费者与Play像素独立后续。保留HUDBg x30、菜单/Unity-GAS/DAT/旧资源。

> 2026-09-24 Q09 smallb HUD静态首差已确认：正式HUD优先smallb；Unity当前HeadImg静态Naruto图实际是正式small字节，104张已部署smallb未入候选/发布，动态HUD consumer未见。只读报告 artifacts/diagnostics/NTSD28-Q09-SMALLB-HUD-CONSUMER-AUDIT-001/REPORT.md；Q09/R17及Play像素仍开放，保留用户HUDBg x30、非战斗UI和旧图。

> 2026-09-24 Q07正式索引图片覆盖：正式/暂存VFS对Q01不同索引图1,010/1,010存在且SHA相同；现候选906与原始1,010差104全部为`smallb`独立图片。正式战斗HUD优先smallb、缺省small；Unity当前候选只收file/head/small，smallb生产读者未见，归Q09 HUD表现/R17而非Q07资源缺失。详 `artifacts/diagnostics/NTSD28-Q07-INDEXED-IMAGE-COVERAGE-001/REPORT.md`及逐图SHA表。旧资源退场、Q07/Q09实际表现仍开放；SPARK原始ID接线归Q09，未改资源/生产。

> 2026-09-24 Q07旧资源动态owner门槛：旧data.txt当前SHA与Q01冻结值相同，137索引对象/type0 42；旧138 DAT+383索引图逐字节521/521与Q01相同，383图均有旧DAT声明owner。正式根默认预热不读旧内容，但菜单/测试bootstrap/Inspector/帧预览的空根legacy入口仍可达；静态0引用不是删除证书。详 `artifacts/diagnostics/NTSD28-Q07-LEGACY-DYNAMIC-OWNER-GATE-001/REPORT.md`。下一独立包处理空根兼容入口和历史夹具；旧资源未动、Q07开放。

> 2026-09-24 Q07旧383角色图静态重扫：当前Assets所选9,825个文本/序列化文件中，同路径存在383、GUID owner 0、精确路径引用0，逐行 `deleteAuthorized=False`；只证静态文本/GUID，旧DAT动态声明和空根legacy运行时可达性待审，不能删图。新CSV见 `artifacts/diagnostics/NTSD28-Q07-OLD-CHARACTER-IMAGE-CURRENT-OWNER-001/post-formal-test-indexed-image-reference-scan.csv`。

> 2026-09-24 Q07 NTSD28-Q07-FORMAL-CHARACTER-DEPLOYMENT-001 / VERIFIED（仅角色部署测试）：42旧DAT/BMP断言已改为正式/暂存158条type0的DAT、解析帧及声明图片逐项SHA检查；原项目Editor编译，目标+相邻内容身份+正式鸣人PNG/GPU 3/3 PASS（job 3bc6f024f2784e928d749118b17d4982），Scene dirty=false、双Scene SHA稳定、Ledger PASS。生产、DAT、图片、Scene未因本包修改；旧资源退场和Q07整体继续开放。

> 2026-09-24 Q07 NTSD28-Q07-LEGACY-GRID-TEST-FIXTURE-001 / VERIFIED（仅测试夹具）：已将两条旧Naruto/Sasuke BMP精确路径换成800×560内存网格；原项目Editor重编译，旧网格/帧内绿色保护/正式鸣人PNG生产GPU聚焦3/3 PASS（job 91c3e05b90844d64ae160d2c24bd8003）。未改生产脚本、DAT、图片或Scene；旧383图删留及Q07整体继续开放。详Task/Record和本包ACCEPTANCE。

> 2026-09-24 STATE/handoff 恢复说明：本轮写入故障曾产生 NUL 字节；已从 Git HEAD 完整正文及当前 Q07 对齐总表重建，原未提交顶部流水账不声明逐字节恢复。详 artifacts/diagnostics/NTSD28-Q07-STATE-HANDOFF-RECOVERY-001/REPORT.md；恢复后 Ledger PASS。

> 2026-09-24 Q07 角色帧预览旧索引入口限定闭合：NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001 / VERIFIED。正确单例夹具下原窗口正式根仍加载旧data.txt，RED1/2；现正式根打开窗口不读旧索引、空根保持旧行为，原Editor聚焦2/2、相邻Inspector/legacy2/2、Battle Scene dirty=false且双Scene SHA稳定。两次早期夹具失败不计行为证据。详Task/Change及 artifacts/diagnostics/NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001/ACCEPTANCE.md；旧资源删留与Q07整组仍开放。

> 2026-09-24 Q07 Inspector正式资源刷新限定完成：NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001 / VERIFIED。原Inspector按钮在配置正式根时仍读旧data.txt/BMP，actual-button测试先RED 0/1（发布键null）；现复用现有正式预热，按钮+相邻调用2/2、旧显式数据1/1，原Editor编译、Scene双SHA稳定、Ledger PASS。空根历史入口与383旧索引图删留仍开放。详Task/Change与 artifacts/diagnostics/NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001/ACCEPTANCE.md。

> 2026-09-24 Q07 旧索引角色图片新鲜静态扫描：从旧Q01清单取383张，当前同路径存在383；扫描Assets内2901个文本Scene/Prefab/Asset/脚本/配置，序列化GUID owner为0，精确路径文本引用仅naruto_0.bmp与sasuke_0.bmp两项Editor网格测试。详 artifacts/diagnostics/NTSD28-Q07-OLD-CHARACTER-IMAGE-CURRENT-OWNER-001/current-indexed-image-reference-scan.csv 及REPORT.md。旧DAT动态声明与手动/空根加载仍开放，不能据381项无静态引用批准删除；Q07继续。

> 2026-09-24 Q07 旧角色图片当前 owner 回访：当前 Battle Scene 禁用佐助预览已指向正式 `sasu.png` GUID，2026-09-22 图中旧 BMP Scene owner 结论过期。配置化默认预热使用正式根、项目模式 Asset，旧任务在该根非空时取消；但手动 Editor 刷新、空根旧加载路径、42 角色旧资源部署测试及两个 BMP 网格测试仍有旧内容依赖，不能据默认预热成功批量退场 383 个旧索引图。当前态与下一逐文件门槛见 `artifacts/diagnostics/NTSD28-Q07-OLD-CHARACTER-IMAGE-CURRENT-OWNER-001/REPORT.md`。本轮仅只读审计/文档回链，删除授权仍为零，Q07 开放。

> 2026-09-24 Q07 排除后新鲜清单：正式405 DAT明确排除背景/两类模式52，当前范围上限353；项目目录现336 DAT均有正式同路径同SHA，额外0、不同0、缺17。17项分别进入原生菜单/HUD例外、Q09活动证据、Q10音频及Q08/stage部署暂缓，见 `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/CURRENT-SCOPED-DAT-INVENTORY.md`，不得再机械复制。原版背景/模式资源统一待用户清理文件夹与项目模式Asset的限定验收见同目录REPORT及Q07 ProjectMode记录。随后原Editor再编译、与旧mode身份相关的三项Q07聚焦测试3/3 PASS（`c82592c1ad694d3dbf5cb4e358ab36af`），ProjectMode生产接线限定验收完成。下一Q07优先回到角色相关旧图的序列化引用/活动可达性与正式图重绑清单；先按既有owner分类作只读核对，不得按目录批量删除或把Q07整组写成完成。

> 2026-09-24 Q07 范围修正已落地：按用户确认，原版背景24 DAT、背景模式26 DAT、`data/mode.dat` 与 `data/mode/ntsd.dat` 共52 DAT及配套meta，连同误加的未接入解析器/测试，已从 Assets 可读路径精确移入 `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/for-user-deletion/` 单一文件夹（136文件，含已逐项SHA验证的135项zip），留给用户自行删除；未删原件。新的独立 `Assets/NTSD/Resources/ProjectBattleModeConfig.asset` 已接管配置化生产内容入口。原Editor Asset聚焦1/1、生产快照/首tick聚焦1/1、预热缓存2/2、SelfCheck PASS；无原版mode DAT的临时内容 Play 和正式内容 Play 均完成World4/发布键一致/有序关闭/零残留，Scene磁盘哈希不变。此前本文件下方“原件仍在Assets/下一步接线”文字均是被此项覆盖的历史阶段。Q07未整体关闭，后续按排除后的内容范围继续。

> 2026-09-24 用户补充确认：不使用原版 NTSD 背景、背景模式和 mode DAT；排除 `b/*/b.dat`、`data/bg_mode.dat`、`data/bg/*.dat`、`data/mode.dat`、`data/mode/ntsd.dat`。当前后两项已进入生产内容指纹及 combo/KO 消费链，先脱钩并验证，不直接移走造成战斗断链。本轮误加的背景类50 DAT及未接入解析器已核验打包到 `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/excluded-native-background-and-mode-staging.zip`；原文件仍在 Assets，压缩包不是退场证明。下方旧接线计划均以本条及对齐总表 §0.2 为准。

> 2026-09-24 用户范围更正（覆盖下文 D-023 DAT 概括及 Q07 背景接线计划）：本项目不采用原版 NTSD 背景和背景模式 DAT，包括 `b/*/b.dat`、`data/bg_mode.dat`、`data/bg/*.dat`。项目自己的背景、地图、模式内容和既有例外保持。此前 50 个正式 DAT 暂存与未接入的背景模式解析器属于范围错误，停止接线，待精确退场；下文相关数量与计划只保留历史事实，不能作为当前任务指令。

> 2026-09-24 D-024 AI child Play probe `NTSD28-USER-D024-AI-CHILD-SCENE-PLAY-001 / RUNTIME_PENDING`: first request dirty-Menu preflight; second entered Play but result missing; test-only phase-progress update gave third final `FAIL` at Menu prewarm (`Configured content request is no longer current`), tick0, no app birth/AI child. Editor ended non-Play with Battle active and both Scene disk hashes unchanged. Inspect prewarm ownership/loaded scenes before any retry; Q07 paused. See Task/Change.

> 2026-09-24 latest D-024 controlled AI Driver: original Editor formal-content human12/AI-controlled3 parameterized full-Driver job `2e60f6a680704c2aacfd550852e7b1ab` 2/2 PASS: source carriers complete, AI target slot0 observed and physical/source X exact `2048/1333`; independent production App birth initializer2/2 PASS. Isolated AI tick4 attempted OPoint without the test fixture's Renderer pool; this does not prove a production bug and leaves child birth/natural Battle Scene Play/formal EXE open. Noncharacter X-edge decision pending; Q07 paused. See Change Record.

> 2026-09-24 latest D-024 AI full-Driver: controlled formal-content two-participant held-right 12-tick original Editor job `c1b53cb922a142ab880a9cf2d7100f81` PASS: after seeding the legacy replay fixture's initial source positions, every active carrier stayed initialized and approved physical X separated from source X. The fixture's initial missing carriers and neutral direct-Vx no-motion failures were fixture/input limitations, not observed production defects. Prior two-profile AI4/4, adjacent13/13, zero-allocation1/1 and standard SelfCheck PASS remain. Natural AI participant/child birth/Scene Play/formal EXE and noncharacter X-edge decision open; Q07 paused. See Change Record.

> 2026-09-24 latest D-024 character AI: `NTSD28-USER-SOURCE-CHARACTER-AI-DOMAIN-001 / FOCUSED_TEST_PASS` now uses one source-rule or physical X/Z domain in both production profiles, including Legacy nearest-facts and unified MoveMode shadow. A direct Legacy facts RED observed expected source X120/actual physical X200; strengthened original Editor two-profile 4/4, adjacent candidate 13/13, warmed 128-input 0 B allocation 1/1, and post-fix standard BattleRuntimeSelfCheck at local 13:02:44 PASS. Full Driver/natural Play/formal EXE and all source writers remain open; noncharacter X-edge decision pending. Q07 remains paused. See Change Record.

> 2026-09-24 current D-024 focused state: original-project Editor PID10576/bridge6403, C: free space restored. Kind14 integer-source correction `NTSD28-USER-KIND14-INTEGER-OPERAND-AUTHORITY-CORRECTION-001` exact 11/11 PASS; continuation OID998 source-birth matrix4/4 and final C07 class7/7 PASS; C07 stale phase fixture corrected test-only. Prior broad job lost with old Editor PID288224 and never counted as acceptance. Formal EXE fractional, continuous source-rule motion/flags, full Driver/Play and OID219/fusion remain open; Q07 paused.

> 2026-09-24 C07 phase-fixture correction FOCUSED_TEST_PASS: old job9155fd12 class6/7 failed slot28 FrameAdvance versus Stage, then focused RED total34 versus actual33; test-only `NTSD28-USER-C07-REVIVAL-PHASE-ASSERTION-ORDER-001` now asserts FrameAdvance27/Stage28/count33. Original Editor single1/1 and final class7/7 PASS. No production change under this ID; D-024/Q07 open.

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-FRAME-MOTION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: source-rule linked-platform then direct-frame X/Z advances from independent integer mirrors with raw displacement and immediate rounding. Original Editor PID10576 exact frame/platform 14/14 and adjacent full tick/replay 3/3 PASS. Physical D-024 scale unchanged; source physics/teleport/bounds/flags, Play/EXE and gameplay readers remain open. D-024/Q07 open.

> 2026-09-24 D-024 kind14 precision focused correction (historical, superseded): original Editor first difference X5.5/int5, formal-expected RED job7e9ebbb7, precise X/Z active/projection/fallback operand correction, GREEN job7415d299 7/7 and direct C32 b2274720 1/1. The formal-expected assertion was wrong because playable kind14 uses integer X/Z; those passes do not establish parity. D-024 source-rule coordinate/flag history, OID219/fusion, full Driver/Play/EXE remain open. Q07 paused. See successor correction Record.

> 2026-09-24 D-024 non-perceptual audit: kind14 formal precise X/Z collision-block threshold versus Unity battle integer X/Z is a static first-difference candidate; official-content focused/runtime witness pending. Source-rule coordinate carrier cannot reuse battle-domain block flags after scaled travel. OID219/fusion defects remain; Q07 paused. See KIND14-COORDINATE-DOMAIN-AUDIT.md.

> 2026-09-24 D-024 CPoint投掷位置双视野FOCUSED_TEST_PASS：原项目Editor精确job a75fa674dcb1434d9324c5a5fa351453 2/2；正式row0在实际投掷写入下，投掷者/目标整数X148/159及173/184，局部差11，目标Y-24/Z200和Vx1.5/Vy-2.25保持。双坐标carrier与OID219/fusion仍开放，Q07后置。详CPOINT-THROW-POSE-TWO-VIEW-WITNESS.md。

> 2026-09-24 D-024 CPoint持有姿态双视野FOCUSED_TEST_PASS：原项目Editor精确job b1e0ecd2071448b580d1b62e66b45f0a 2/2；正式row1在实际持有同步写入下，持有者/被持有者整数X348/373及373/398，局部差25、Y4/Z249均保持。投掷与非零CPoint Z未验，OID219/fusion双坐标历史开放，Q07后置。详CPOINT-HELD-POSE-TWO-VIEW-WITNESS.md。

> 2026-09-24 D-024 WPoint武器组件路径双视野FOCUSED_TEST_PASS：原项目Editor精确job 13805c76522f442cac6bc7917647d39b 2/2；正式row21/type1在Unity活跃`LF2WeaponBase` writer下，持有者/武器整数X为348/359及373/384，原始局部差11与Z251均保留。此为聚焦挂点算术见证，非完整Driver/Play/EXE；CPoint与OID219/fusion双坐标历史开放，Q07仍后置。详WPOINT-WEAPON-COMPONENT-TWO-VIEW-WITNESS.md。

> 2026-09-24 非角色早期StageZ定向修复FOCUSED_TEST_PASS：正式两次stage-depth全active/非type0±1边界，Unity先前两次仅type0、非角色后续PreFrame才裁；原Editor RED type1 Z500≠351，修默认及Legacy/Shadow生产写入后4/4GREEN、最终相关StageZ/StageBounds/WPoint 16/16PASS。生产只改早期Z/ZInt与旧断言，DAT/相机/Scene/非战斗未改。完整Driver/命中/Play/EXE和双坐标carrier未验，OID219/fusion开放、Q07后置。详NONCHAR-EARLY-STAGE-Z-ACCEPTANCE.md及Record。

> 2026-09-24 D-024 WPoint普通持有路径见证FOCUSED_TEST_PASS：原项目Editor job bdb426058e5f448da3a79d711c11d9f4两例2/2，formal row1持有者按raw Vx48移动后factor1 holder/held整数X348/359、全景373/384，局部间距均11/Z251。武器组件活跃另一写入分支未验。只读源码比对还发现正式两次stage-depth裁全部实体，Unity对应两次仅type0，非角色后续PreFrame才裁；后继NONCHAR-EARLY-STAGE-Z-001已聚焦修正。无生产/DAT/Scene/相机/非战斗改动的历史阶段，OID219/fusion开放、Q07后置。详WPOINT-POSE-TWO-DOMAIN-WITNESS.md和POSITION-WRITER-OWNERSHIP-GATE.md。

> 2026-09-24 D-024 StageZ独立边界见证FOCUSED_TEST_PASS：原项目Editor最终强化job ddce615b36c54e6b902682f627431db7仅两例2/2，独立断言比例/裁剪前Z。原始视口raw Z340未触350，配置全景Battle Z363.123...被默认StageZ裁350；reference必须独立保留正式Z边界轨迹。仅测试脚本，DAT/Scene/相机/非战斗未改。非角色Z、正式EXE/Play及双坐标carrier仍待；OID219/fusion未修，Q07后置。详STAGE-DEPTH-TWO-DOMAIN-WITNESS.md及Change Record。

> 2026-09-24 D-024 kind1 OPoint随机出生X/Z出口FOCUSED_TEST_PASS：原项目Editor先RED（raw X147、配置期望225.848462/实际147），共享出生写入处一次按X 2048/1333、投影Z 1152/730换算后，强化X/Z定向1/1GREEN、默认正式源World与组件物化2/2PASS。Y/动作/RNG调用顺序与DAT未改；Ledger732/22通过，Scene哈希保持。真实Yagura418 Play/正式EXE和双坐标历史仍待，OID219/fusion未修，Q07后置。详KIND1-OPOINT-RANDOM-BIRTH-RATIO-ACCEPTANCE.md。

> 2026-09-24 D-024 kind1 OPoint随机出生包PLANNED：NTSD28-USER-OPOINT-KIND1-RANDOM-BIRTH-RATIO-001已在脚本修改前限定共享BattleNativeOpointBirthWriter与现有Q06物化器测试，正式Yagura418的kind1随机X/Z有非零幅度；先测配置视口RED，再仅在最终出生位移按比例换算，保留DAT/RNG次序/Y/动作。OID219/fusion和双坐标历史仍开放，Q07后置。

> 2026-09-24 D-024抓取姿态见证已通过：原项目Editor job 6fe9428e5d1f45ea9bd5e4049b3eb22e仅新增两例2/2；正式局部挂点公式下factor1抓取后X148/140，配置全景X160.5/152.5，局部间距均-8。仅测试脚本，生产/DAT/Scene/相机/非战斗未改；双坐标载体及OID219/fusion仍待，Q07暂停。详GRAB-POSE-TWO-DOMAIN-WITNESS.md。

> 2026-09-24 D-024抓取姿态定向见证PLANNED：NTSD28-USER-GRAB-POSE-VIEW-WITNESS-001已在脚本修改前声明一项Kind3抓取测试，复用正式局部centerx/cpoint公式与现有生产Writer，分别测factor1及2048/1333视口。仅测试脚本，未动生产/DAT/Scene/相机/非战斗；OID219/fusion未修，Q07暂停。

> 2026-09-24 D-024 stage-edge dependency original Editor verified: focused job 57c29991fd964a428e6e856c41e1c209 2/2 after production character mechanics + default ECS preframe bounds; width2048/startX2000/rawVx40 gives factor1 X2040 inside edge, configured Sx2048/1333 battle X~2061.46 then clamps2048. Formal raw-reference position remains2040, so future source-rule carrier must not copy battle clamp/blocked state. Test-only; no production/DAT/Scene/camera edit. OID219/fusion defects open, Q07 paused.

> 2026-09-24 D-024 stage-edge history witness PLANNED: NTSD28-USER-STAGE-EDGE-HISTORY-WITNESS-001 pre-script Task/Change/Ledger limits one existing Editor test to width2048, start X2000, raw Vx40, factor1/configured production physics and preframe bounds. This will distinguish formal raw X2040 from approved battle X2048 clamp before carrier implementation. No production/DAT/Scene/camera change authorized; OID219/fusion defects open, Q07 paused.

> 2026-09-24 D-024 coordinate-history gate refined (read-only): POSITION-WRITER-OWNERSHIP-GATE.md maps current precise/int and PS.x/z aliases, actual spawn/physics/teleport/grab/fusion/respawn/attachment/bounds owners, canonical snapshot/reset/checksum and dead helper exclusions. It identifies a boundary issue: scaled battle position can reach an unchanged stage edge before formal raw position, so reference physics cannot blindly copy battle blocked flags. No carrier script edit yet; OID219/fusion defects OPEN, Q07 paused.

> 2026-09-24 D-024 state9996 relative child birth FOCUSED_TEST_PASS/RUNTIME_PENDING: original Editor stricter configured-view RED 1/2 (child precise X327 vs target325.39084771192796), shared writer scales only final relative X/Z offsets, final all-five-child factor1/configured focused job f1dc8c0b632a43d4a36c33380a865e53 2/2 GREEN; exact formal row0 immediate/following 2/2. Raw RNG/Y/velocity/DAT, Scene/camera/nonbattle unchanged. OID219/fusion history defects, Play/EXE and Q07 gate remain open. See NTSD28-USER-STATE9996-CHILD-RATIO-001 Record.

> 2026-09-24 D-024 state9996 child ratio fix PLANNED: original Editor job f2fbd17c1a1548b480ce8166db78f62f measured relative child X -3/1333 default versus -3/2048 configured; this is a birth ratio defect. NTSD28-USER-STATE9996-CHILD-RATIO-001 Task/Change/Ledger predeclare only shared state9996 writer plus focused test for precise X/Z relative offset RED->GREEN, raw RNG/Y/velocity unchanged. OID219/fusion history defects remain open; Q07 paused.

> 2026-09-24 D-024 state9996 birth witness PLANNED: NTSD28-USER-STATE9996-CHILD-VIEW-RATIO-WITNESS-001 has pre-script Task/Change/Ledger for two original-Editor tests of formal row0 OID217 child relative X=-3 after raw20px parent motion at factor1/configured view. Production/DAT/Scene/camera unchanged; OID219 and fusion confirmed defects remain open, Q07 paused.

> 2026-09-24 D-024 fusion witness VERIFIED_SCOPED_DEFECT: original Editor focused job 42504562540d48f29a0f42433c24eced 2/2 PASS; exact formal fusion row0 after raw20px movement merges at factor1 gap40 but configured Sx=2048/1333 current gap50 is rejected by production Oid5152 scan. Passing test characterizes a gameplay parity defect, not correction. See FUSION-VIEW-DISTANCE-FIRST-DIFFERENCE.md and NTSD28-USER-FUSION-VIEW-DISTANCE-WITNESS-001 Record. OID219 target-history defect also open; Q07 paused. No production/DAT/Scene/camera edit this package.

> 2026-09-24 D-024 fusion witness PLANNED: NTSD28-USER-FUSION-VIEW-DISTANCE-WITNESS-001 has pre-script Task/Change/Ledger for one original-Editor test of the formal X<50/Z<8 gate after scaled movement. Production, DAT, Scene and camera untouched. OID219 confirmed history defect remains open; Q07 paused.

> 2026-09-24 D-024 writer follow-up: measured OID219 same-current-gap Vx3/4 difference remains OPEN. X-WRITER-SEMANTIC-INVENTORY.md adds PS.x/SetPos aliases and source-matched fusion X<50/Z<8 risk; fusion, state9996 and grab are not yet runtime-confirmed defects. Dual-coordinate writer/snapshot contract precedes production fix. Q07 paused; no script/DAT/Scene/camera changes this pass.

> 2026-09-24 D-024 same-current-gap proof: original Editor focused job d248af5583144fa4822db084fa43cee3 passed 11/11. Static initial gap224 and a target moved once from initial gap151 both yield Unity integer gap224, but playable source-rule hit_Fa5 velocities are respectively4 and3. Current Unity returns4 for both. Thus no stateless multiplier/divider of current gap can satisfy both histories. Test-only change this turn; general two-domain coordinate/history contract is required, including snapshot/reset/checksum and all position writers. D-024 OPEN, Q07 paused. See TARGET-HISTORY-VELOCITY-FIRST-DIFFERENCE.md.

> 2026-09-24 D-024 target-history measured first difference: original Editor focused job 92b7728e66144755aafa43f9b4e8deb0 passed 10/10, including two defect-characterization cases using production character movement, formal-DAT-loaded OID219 child birth and next non-character step. One prior 48px target move yields formal/current child Vx 3/4; two yield 4/5. These passing tests prove a parity defect, not alignment. Equal-start controls remain GREEN. Mixed raw absolute spawn and scaled prior movement require a general coordinate-history contract; do not apply blanket inverse scaling. Full Driver/Play and formal EXE pending. D-024 OPEN; Q07 paused. See TARGET-HISTORY-VELOCITY-FIRST-DIFFERENCE.md.

> 2026-09-24 target-history first difference: indexed OID219 hit_Fa5 equal-start birth/one-step passed, but source-derived velocity sees a scaled World target gap after prior D-024 movement. Initial gap151 plus one formal48px target move gives formal Vx3 versus Unity Vx4 and next screen fraction4/1333 rather than3/1333; two moves give formal4 versus Unity5. This is static source-formula evidence, not dynamic Editor/EXE trace. See artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/TARGET-HISTORY-VELOCITY-FIRST-DIFFERENCE.md. Blanket inverse scaling breaks the equal-start case. D-024 OPEN, Q07 paused.

> 2026-09-24 D-024 HitFa5 UPDATED: FOCUSED_TEST_PASS; original Editor RED 4/4 (old 3.02 vs formal 3 at 151px), formal-DAT-loaded World birth plus one production mechanics step GREEN 8/8; configured screen fraction 3/1333. No inverse velocity scaling. Dynamic targets, full Driver/Play, formal EXE view and unindexed HitFa6 remain open. D-024 OPEN; Q07 paused. Supersedes earlier PLANNED snapshot.

> 2026-09-24 D-024 `NTSD28-USER-HITFA5-TARGET-VELOCITY-001 / PLANNED`：正式索引OID219帧51 hit_Fa5 的子体X速度先对整数目标差/50截断，Unity当前/50.0；同起点151像素应3而非3.02。Task/Change/Ledger已先建，先实际World出生及下一物理步RED，验证配置视图无需逆向除比例，再修精确整数算法。Q07后置。

> 2026-09-24 D-024 `NINE_BYPASS_FOCUSED_PASS / FULL_AUDIT_OPEN`：本轮九条非体感位移出口均有原Editor配置视图RED→聚焦GREEN，type3武器专用hit_j精确Z最后一包聚焦类12/12；DAT/Scene/相机/非战斗代码不改。HitFa11相对出生、抓持挂点、stage绝对/边界、Y/floor/碰撞与真实Play/正式EXE占比尚未闭合；不能说“无其他问题”，Q07仍后置。详parent Task、九Record、NON-INTEGRATOR-WRITER-AUDIT。

> 2026-09-24 D-024 `NTSD28-USER-WEAPON-TYPE3-Z-RATIO-001 / PLANNED`：正式type3 frame hit_j使精确Z直移；Unity generic路径已调最终Z比例但武器专用路径仍原值。先注册World配置/默认RED，仅改武器最终精确Z，不动原始Type3VisualZOffset。前一武器identity额外X已旧2配置RED→聚焦类10/10。Q07后置。

> 2026-09-24 D-024 `NTSD28-USER-WEAPON-IDENTITY-X-RATIO-001 / PLANNED`：正式物理积分含独立20% identity X位移，Unity在武器通用积分前直接加X，尚未按World比例。先实际注册World/配置默认RED后仅调该额外X出口，保留原始速度及identity分支。复活随机相对X/Z已旧配置RED→聚焦类9/9。Q07后置。

> 2026-09-24 D-024 `NTSD28-USER-REVIVAL-OFFSET-RATIO-001 / PLANNED`：state14普通复活以队友整数均值为绝对底座，再加同步随机X/Z；Unity仍为原始像素。先原Editor实际Pass配置/默认RED，仅处理最终随机相对偏移；保留整数镜像、RNG、Y/floor、HP/帧。state18粒子精确X已配置旧1例RED→2/2 GREEN、正式源64例聚焦1/1；Play/EXE未验。Q07后置。

> 2026-09-23 D-024 `NTSD28-USER-STATE18-PARTICLE-RATIO-001 / PLANNED`：正式state18/19破碎粒子以父体整数XYZ出生，只把子体精确X加随机偏移；Unity该精确X仍为原像素。先据正式见证原Editor实际结构出生RED，仅在最终精确X加World比例，整数镜像/RNG/Y/初速保持；Q07后置。

> 2026-09-23 D-024当前 `NON_PERCEPTUAL_XZ_BYPASS_FOCUSED_PASS / FULL_AUDIT_OPEN`：晚生OPoint、native帧dx/dz、平台乘客X/Z、传送目标相对X、武器碎片X/Z五条同类出口均先原Editor配置视图RED后定向GREEN；碎片出生4/4且默认源随机全出生见证2配置×157行通过。DAT/Scene/相机/非战斗不改。继续分类state18粒子、复活随机位置、抓取/持有、Y/floor和stage边界；未完成全域Play/正式EXE可视比率，不得称“无其他问题”或恢复Q07新包。详五Change Record及NON-INTEGRATOR-WRITER-AUDIT。

> 2026-09-23 D-024 `NTSD28-USER-WEAPON-PIECE-BIRTH-RATIO-001 / PLANNED`：正式和Unity武器碎片由父实体位置加随机dx/dz出生，D-024目前仍是原始偏移。先用现成源见证建实际structural birth RED，仅改最终子体X/Z相对偏移；随机调用序列、父体绝对坐标、Y、初速、DAT保持。传送包已原Editor配置旧4例RED→native+legacy 8/8、类13/13；Play/EXE未验。Q07继续后置。

> 2026-09-23 D-024 `NTSD28-USER-TELEPORT-OFFSET-RATIO-001 / PLANNED`：正式playable state400/401把实体放到目标绝对X±120/60；Unity native与legacy传送路径仍加原始像素。只检验/修复目标相对X出口，保持目标绝对坐标、Y、Z+1、候选排序、DAT和相机；先原Editor实际World RED，后定向改动。Q07继续后置。

> 2026-09-23 D-024 `NTSD28-USER-PLATFORM-CARRY-RATIO-001 / FOCUSED_TEST_PASS`：原Editor实际World乘客跟随RED配置视图左右X 204/196 vs 目标206.1455/193.8545；只改乘客X/Z最终出口，GREEN 4/4、既有正式源平台运动21/21。Y/floor/完整Play与正式EXE可视证据仍待。Q07继续后置。

> 2026-09-23 D-024 `NTSD28-USER-PLATFORM-CARRY-RATIO-001 / PLANNED`：实际平台实体共用X/Z积分已按World比例，乘客ApplyLinkedPlatformMotion仍加原始帧DV，可能在全景相机下相对漂移。Task/Change/Ledger先建，只准LF2Entity linked X/Z出口及声明帧尾测试；先实际World配置/默认RED，再决定生产修正。Y/floor/碰撞与其他位移另包，Q07后置。

> 2026-09-23 用户澄清“其他问题”指非体感差异，尤其其他实体位移原始像素相同而画面比例不一致；因此先完成D-024同类路径审计，不能按上一条试玩反馈直接回Q07。已测出并定向修复两个独立首差：late OPoint相对X/Z出生偏移（配置视口旧4例RED→kind1/kind2双物化共16/16PASS，旧深度6/6、源物化2/2）和native帧dx/dz直接位移（正式OID736及左右dx配置旧3例RED→6/6PASS，旧正式帧尾12/12）。只改通用位置出口，DAT/Scene/相机/非战斗不改；Scene SHA保持。平台携带、传送相对偏移、碎片随机出生、Y高度与floor/碰撞、stage边界和附件仍有已确认生产候选，尚不能说没有类似问题。D-024继续优先，Q07暂停新包；详NON-INTEGRATOR-WRITER-AUDIT与两Change Record。

> 2026-09-23 D-024下一直接位移包 `NTSD28-USER-FRAME-DIRECT-MOTION-RATIO-001 / PLANNED`：正式可达OID736/action120有dz:-2、dvz:550；Unity NativeFrameMotionTail以原始dz直接写位置并清Vz，绕开已修共用积分，配置视口时预计Z250→248而目标为250-2×1152/730。Task/Change/Ledger已先建，只准两个声明脚本定向RED→X/Z出口修正，Y/平台另包，DAT不改；Q07仍后置。

> 2026-09-23 D-024 非体感审计发现 `NTSD28-USER-OPOINT-BIRTH-RATIO-001 / IN_PROGRESS`：两个实际late OPoint物化生产者仍把原始DAT X/Z偏移加到已缩放父坐标，造成固定全景画面中子实体相对出生距离比例偏小。原Editor新鲜idle，Task/Change/Ledger先更新；先两路径左右/默认倍率定向RED，再仅修相对出口与聚焦验收。DAT/非战斗/Scene不动。用户要求先排此类问题，Q07继续后置。

> 2026-09-23 用户试玩回访允许恢复BATCH-04/Q07：落地持续D+K局部Play比例通过，螺旋丸两处253物理J均下tick转301；用户体感基本正常。保留正式EXE画面时点、错过变招后的OID434 motionX首差和D-024全实体后续，不能声明完全修复。Q07本轮只读重入核对正式/暂存DAT405/338缺67、PNG1255/1031缺224，缺PNG仅b/110与sprite/114；详NTSD28-Q07-RESUME-CONTENT-GATE-001/REPORT.md。旧资源521项删除授权0，DAT数据不改，非战斗/双Scene保持，下一按可达caller路由Q07/Q08/Q09/Q10，不盲复制。
> 2026-09-23 `NTSD28-USER-CORE-MOTION-OUTPUT-RATIO-001 / FOCUSED_TEST_PASS / RUNTIME_PENDING`：角色与非角色共用物理X/Z积分按World比例一次性写位置，跑/冲刺Writer恢复DAT原始速度；正式一单位摩擦后的逻辑速度不再被早乘。原Editor刷新编译后既有聚焦4/4、11/11、13/13、4/4、5/5通过；新增合成World完整core tick 1/1、生产物化type-1武器物理通道1/1通过。core tick夹具曾因Z=100低于stage最小180而在后续边界pass被夹至180，现以合法Z=250验证；这也说明边界坐标合同仍须单独处理。完整Driver tick/真实Play、Y高度、生成/持有/传送、边界/碰撞几何及其他视口未验。一次桥接测试误用`filter`字段导致非目标全量运行，不能作为本包通过证据；详Task/Record。DAT不改。

> 2026-09-23 D-024语义审计新发现：当前角色跑/冲刺虽已按横向2048/1333、纵深投影1152/730通过聚焦Writer测试，但倍率在速度写入时应用；正式及Unity物理随后对该速度做每tick一单位摩擦，因此逻辑速度/后续速度读者可能改变。不能把该局部写法直接推广到武器、道具等所有实体，也不能在积分器再叠乘一次。须在全实体任务中先定唯一位移出口与生成/持有/边界坐标合同，再以完整tick同时验屏幕位移比例与保留的逻辑速度。详`artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md`；D-024及自然Play仍未闭。

> 2026-09-23 D-024 新全局位移比例目标：用户把原先原始像素位移相同的验收出口，改为角色、武器、道具、飞行物等所有战斗实体相对于各自画面的位移比例一致。保留完整背景固定相机，DAT数据未经另行要求不得修改。`NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001 / IN_PROGRESS`已有词法候选清单与生产路径初图（`artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md`），还须语义穷尽和分批实施；候选数量不可视为全部真实位移写入。当前角色跑/冲刺包仅局部覆盖，且纵深Z投影应按纵向1152/730而非横向2048/1333；其他地图/画面比须另测。D-024总体未完成，Naruto自然操作/螺旋丸时机及Q07仍开放。

> 2026-09-23 `NTSD28-USER-FIXED-VIEW-RUN-RATIO-001 / IN_PROGRESS`：用户确认保留相机和完整背景，接受实际奔跑战斗位移与正式版不同，目标16:9 Training取景水平距离倍率2048/1333≈1.536；并新增DAT数据不可修改的硬约束。首轮定向测试暴露Native/ECS Writer仍写原速度15，已改通用Writer出口，原Editor比例2/2、冲刺11/11、地面普通/重载跑13/13通过。首次真实Play在最终Writer修改前长按方向+跳跃过落地三tick仅7像素且转walking；最终Writer后重测时合成按键未进入FrameInputSet，不能算用户场景通过。正式源码模型的两组自然持续按键也在落地后由215转普通行走，但正式EXE及用户按键起点仍待对照；螺旋丸续按攻击另查，Q07和总目标开放。

> 2026-09-23 R15双端B0 v2诊断采集器限定通过：源码模型kind案例三流双跑相同且九次严格校验通过，Unity端原Editor编译与聚焦7/7 PASS、v2 neutral B0严格3tick有效；旧v1默认/拒绝保持。Unity正式type3/action同场景尚未构建，跨端kind主/B2/B0及正式EXE可见对照未验；当前最早未闭Q07，Q10音频局部成果保持，Q07/R15/总目标开放。

> 2026-09-23 R15新证据：NTSD28-R15-B0-SOURCE-V2-PRODUCER-001 / FOCUSED_TEST_PASS。源码模型kind案例显式B0 v2三tick两次，main/B0/B2逐字节相等，九次严格校验通过；tick1槽1 OID206→213且epoch1→1。默认v1 neutral有效，v1 kind准确拒绝。Unity v2 producer已建包实施中；同场景Unity type3/action与正式EXE可见对照待，Q07/R15/总目标开放。

> 2026-09-23 R15最新：NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001 / FOCUSED_TEST_PASS。B0 v2诊断契约聚焦33/33与比较器11/11 PASS，旧双端v1各3tick仍有效，混版拒绝；源/Unity producer仍是v1，kind同场景跨端和正式EXE可见对照未验。下一先接两个v2 producer，再做Unity type3/action场景；Q07/R15/总目标开放。

> 2026-09-23 R15 kind依赖源码模型预检：`NTSD28-R15-KIND-DEPENDENT-SOURCE-PREFLIGHT-001 / SOURCE_MODEL_3TICK_MAIN_B2_VALID / UNITY_PENDING`。正式OID213/action176对OID206/action0同seed无输入三tick双跑逐字节一致，首tick slot1原位206→213/type3、action/latch/previous40、owner0/group1/HP445，主raw和B2严格校验通过、certificate false。B0因为同slot/epoch原位改ID触发当前诊断合同异常并标INVALID；Unity capture尚无初始action且默认type0，不能宣称跨端对齐。下一独立诊断Task/Change先补严格identity-change事件与type3/action场景入口，不动战斗生产，不伪造正式pup.dat不存在的frame40；Q07/R15/总目标开放。详同ID REPORT。

> 2026-09-23 Q07 Player限定出口：`NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001 / VERIFIED_SCOPED_PLAYER_CALLBACK`与`NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001 / VERIFIED_PLAYER_CONTENT_COPY_GATE`。原Editor编译与Menu/Battle顺序Windows Mono Development构建0错；1371正式侧载文件/46883057字节逐项清单SHA一致。隐藏图形Player两次真实Menu冷启动经组件回调预热正式资源、Naruto OID2、Additive BattleRunning/World2、三发布键一致，有序回Menu且Stopped/池借用0；第二次exit0。首次`-nographics`预热因阴影材质语义FAIL，不计PASS。双Scene及BuildSettings SHA不变。此仅Player回调/内容闭包，物理输入、像素、Android及正式EXE行为未验，Q07/总目标开放；详同ID REPORT。

> 2026-09-23 Q07 Player `NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001 / CODE_WRITTEN`：用户确认Menu第一、Battle第二；原Editor回调PASS、内容清单V2已编译。独立包将仅扩展诊断构建入口并加Development Player探针，验证原项目Menu冷启动到Additive战斗及有序返回；生产脚本/Scene不动，Q07仍开放。

> 2026-09-23 Q10受控整链证据：原Battle Scene直接Play实际为battle mode0，首个完整致死探针在创建夹具前准确FAIL，原报告已另存。重跑时选定正式KO feed已发布，只在Play-only World切mode1，临时战斗体经真实Driver tick5→6令受击者HP=-10、KO事件0→1、m_join.wav入队，场景音频播放0→1、未预热拒绝0；Editor退出后Scene SHA 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0不变。此为受控mode1临时战斗体整tick，不证明真实菜单选mode、物理玩家输入、扬声器可听、Player或正式EXE等价；Q10/总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record和scene-natural-ko-audio-probe.json。

> 2026-09-23 Q10真实场景补证：原项目Editor PID173216在保存的NTSD_Battle Scene两次Play（第二次退出重进）中，选定正式两WAV已在封存音频目录，每轮用合成PendingSoundEvent经生产NTSDSoundPlayer呈现，调用播放计数0→2、未预热拒绝0→0；两轮退出后Editor非Play，Scene SHA-256 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0不变。原Editor聚焦8/8 PASS。此为合成事件的真实场景音频呈现证据，不能替代自然击倒整链、可听设备输出、正式EXE听感或Player/其余cue验收；Q10及总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record和scene-play-audio-probe-first.json / scene-play-audio-probe.json。

> 2026-09-23 Q10最新：原项目Editor PID173216 编译后定向EditMode 8/8 PASS；真实选定LoganRuntime两mode cue已在NTSDSoundPlayer.PrepareBattleCuesAsync中加载，战斗目录封存后各有非空AudioClip，未发生未预热cue拒绝。两正式WAV SHA匹配且GUID各唯一。仅此EditMode夹具的封存预热已验；真实Battle Scene可听、退出重进、Player打包及正式EXE音画行为对照未验，Q10/总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record与editor-sealed-prewarm-result.json。

> 2026-09-23 Q10新证据：原项目Editor PID173216当前编译后，NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 定向EditMode 7/7 PASS；真实选定LoganRuntime中两mode cue进入战斗预热集合，NTSDSoundPlayer将路径解析到Assets/NTSD/Sound/data，现有资源加载器将两正式WAV解码为非空AudioClip。两WAV SHA匹配，meta GUID在Assets各唯一。首次路径分隔符断言失败已修正测试，生产逻辑未改。完整封存预热、Scene可听、退出重进、Player打包及正式EXE听感未验；Q10/总目标继续开放。详Q10包Record与editor-audio-decode-rerun-result.json。

> 2026-09-23 Q10当前进度：原项目 Unity Editor PID173216 路径为 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity；独立 I:\UnityPreject\test 自09-21已打开，不用于本任务验证。NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 已在原项目编译并完成定向 EditMode 5/5 PASS，正式 mode KO 新事件选择双阵营 cue、同tick位置、无重播及门槛有覆盖；两正式WAV按SHA暂存。首轮断言误计普通命中声的RED保留在Record。预热实际加载、场景可听、退出重进、Player及正式EXE对照仍待；Q10与总目标开放。下方早期 PLANNED/Editor无进程记录为历史快照。

> 2026-09-23 Q10下个独立包 `NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 / PLANNED`：脚本修改前Task/Change已登记，准确范围为战斗tick、Host mode发布、现有战斗音频预热、聚焦测试及两条正式WAV/meta；此时尚无脚本/资源修改。正式同tick新KO追加音频且在tail裁剪前，选定sound1/sound2，Unity当前缺生产与m_join预热、两条Sound/data资源。保留Q06已闭frame-sound、Scene/非战斗/旧Click音频；Player打包、可听EXE与剩余cue后置。详本包Task/Record和Q10静态审计；总目标开放。

> 2026-09-23 08:56+08 原项目当前进度：Unity Editor PID173216（确认项目路径为本仓库）已对Q09图标当前测试重新编译并完成聚焦 job a825513e8f674351b5dc006768d49502，EditMode 1/1 PASS。前一轮 job 44ab32a4831547aa893ba0c8606465ef 因未声明预期BMPLoader错误日志失败，测试修正和两份终态JSON均已留证；生产代码未随之修改。`NTSD28-Q09-KILL-ICON-PUBLICATION-001 / FOCUSED_TEST_PASS` 仅覆盖7槽/3图发布、损坏Temp图保旧代及目录租约回收，图文屏幕consumer、同tick、Play/EXE像素、退出重进仍待。下方“原Editor无进程/旧PID33236活跃”均为不同时点快照；独立I:\UnityPreject\test不作本项目证据。Q09/R17及总目标开放。

> 2026-09-23 08:50+08 当前会话再核对：原项目 Unity Editor PID173216 于08:46经Hub启动，进程路径明确为 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity，Responding=True；08:46之前“原项目无Editor”的记录仅是当时快照。原项目程序集仍停留2026-09-22 20:19/18:44本地时间，当前修订编译与Q09聚焦测试须以新会话新鲜状态再验。I:\UnityPreject\test 独立会话不作本项目证据。总目标开放。

> 2026-09-23 当前会话：原项目无运行中的 Unity Editor；下方 PID33236/worker 活跃为昨日快照。I:\UnityPreject\test 是独立项目，不作本项目验收。Q09 图标原 Editor 编译/NUnit 待；Q10 击倒音频静态首差详 NTSD28-Q10-KNOCKOUT-AUDIO-HANDOFF-AUDIT-001/REPORT.md。总目标开放。

> Q09图标包`NTSD28-Q09-KILL-ICON-PUBLICATION-001 / CODE_WRITTEN`：正式3PNG/7槽发布及无效PNG保旧代测试已写；当前源码离线runtime+Editor编译0错。原Editor10:44Z DLL早于测试10:54Z，当前测试编译/NUnit待；导入worker活跃、bridge超时。screen consumer/像素/退出未验，Q09/R17与总目标开放。

> 2026-09-22 Q09进度：原项目WORDS一次性测试1/1 PASS、行快照当前完整聚焦5/5 PASS，原Editor编译/重载已证；首跑夹具RED及修正已留Change。图标尚未发布/绘制，正式同tick、Play层级/像素及退出未验；Q09/R17和总目标开放。

> Q08 `NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001 / CODE_WRITTEN`：原项目标准致死处新增独立逐次事件但保持旧计数只加一次；native tail过期、预热+超额保留、pending快照/恢复、锁步校验、扩展/lockstep parity v2及reset已接，frozen Authority400 v3投影保持。原项目Temp-only绝对targets离线runtime+Editor编译0错，确认新增聚焦测试进入DLL；**原Editor编译/NUnit/SelfCheck/同seed/Play/退出重进未验**。正式选定mode的70tick仍是捕获常量，Q09须接数据驱动；R15版本化复核待。Q09 WORDS旧请求无结果，不并发跑Q08测试。详Record。

> Q08 `NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001 / IN_PROGRESS`：脚本接线开始前Task/Change已建立并补NTSDBattleTickSystem准确路径；致死事件、单次统计、tail过期、快照/校验/恢复/清理为同一合同，尚无通过结论。赛前按runtime slots预热、超额仍保留事件并计量分配，不任意封顶；Q09图文/Q10声音后置。原Editor WORDS请求无结果，不并发重跑。

> 当前Q08→Q09击倒提示链 `NTSD28-Q08-Q09-KNOCKOUT-FEED-CHAIN-AUDIT-001 / STATIC_PRODUCER_AND_CONSUMER_GAP_CONFIRMED`：正式除致死统计还有持久事件、Session尾部过期、30tick显示窗与mode图文；Unity所检脚本已有致死统计但未见事件载体、mode读者、击倒行命令。Q08先补事件/时序，Q09补图文，Q10补声音；均需Task/Change与原项目验证，Q08/Q09/R17开放。详本包REPORT。WORDS请求已消费无结果，禁并发重发。

> 当前Q09击倒提示图标内容前置 `NTSD28-Q09-NATIVE-KNOCKOUT-FEED-ICONS-STAGING-001 / VERIFIED_EXACT_CONTENT_STAGING_ONLY`：正式mode `#killtext`选定三PNG已在原项目逐SHA暂存，四新GUID各唯一；现337 DAT/1031 PNG，缺224正式PNG。Unity reader/画面、EXE同条件像素未验；Q09/R17开放。WORDS原Editor请求已消费仍无结果，禁止并发重发。详本包Acceptance与WORDS Run-Pending。下方1028/缺227为本包前状态。

> 当前Q09活动INKHUD内容前置 `NTSD28-Q09-ACTIVE-FRAME-HUD-CONTENT-STAGING-001 / VERIFIED_EXACT_CONTENT_STAGING_ONLY`：正式active frame双DAT与七PNG精确暂存，逐SHA、全Assets新meta GUID唯一；现337 DAT/1028 PNG，缺227正式PNG。仅证明正式内容可达，不证明Unity HUD reader/画面；Q09/R17开放。WORDS聚焦请求已被原Editor消费但结果未产生，禁止并发重发。详Q09两包Acceptance/Run-Pending。

> 当前Q07 `NTSD28-Q07-MODE-COMBO-PUBLISHED-ACTIVATION-001 / COMPILE_PASS / RUNTIME_PENDING`：原Editor PID33236于07:58Z编译并重载晚于本包全部源码/测试的程序集；正式mode输入已进入catalog五组件V2身份、候选新鲜度及共用seal。公开seal/reset/tick0 restore聚焦NUnit尚未运行，旧V1 trace/探针待R15版本化，Q07未闭。Q09 WORDS最终定向测试现已编译，但结果仍未产生。只用原项目，禁computer-use和新Unity副本。

> 当前Q07 `NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001 / CODE_WRITTEN`：正式mode双DAT已逐SHA暂存，新纯输入投影的Add-Type编译/正式与暂存指纹/反例2/2通过；生产combo tuple仍未激活。后续独立联合identity+World激活包才改行为。Task/Change已更新，原项目Editor程序集仍早于新脚本、Unity编译/Play待，禁computer-use和第二Unity项目。

> Q08 `NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001 / CODE_WRITTEN / COMPILE_PENDING`：准确单个EditMode用例已写，完整`RunReleaseTick`中kind0 itr伤害20HP目标，计划验致死tick native timer0、次tick timer1及锁定胜组；原Editor测试程序集仍早于新源码，不能报编译/测试通过。生产、Scene、正式资源与非战斗未改；Task/Record/Ledger齐备，禁computer-use。

> Q08新聚焦见证`NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001 / PLANNED`：正式playable已有实际Attack致死tick12计时0、次tick13计时1双跑见证；Unity现有Q08用例仅跨tick直接设HP，缺完整tick内部碰撞致死。准确单测试脚本Task/Change已建，生产不改；等待用例写入及原Editor编译/聚焦验收。禁computer-use、非战斗/Scene/资源改动。

> Q08 mode-4 reserve 专项：正式playable完整GameSession mode-4一存活组timer1/350转202双跑一致；所检BattleConfig无Unity committed-result-reserve同条件入口，不能据源码负搜索删现有RESULT-RESERVE-09补reserve逻辑。详MODE4-RESERVE-CALLER-AUDIT；同条件正式EXE可见结果待证。Q08未闭。

> Q08 `NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001 / NATIVE_GROUP_TIMING_COMBAT_LETHAL_PASS / UNITY_GROUP_RED_4_TIMING_DIRECT_RED_2_FULLTICK_RED_1`：正式playable隔离源码`GameSession28::step()`组六例+计时锁定/80/101/144→350+真实Attack致死tick12计时0/tick13计时1，双跑同输出；隔离Unity组别4例、直接producer计时锁定/过早结果页2例及`RunReleaseTick`恢复组别timer不增1例目标RED（类3PASS/7FAIL）。TeamIds[2]/HadBoth绑定结果UI、schema1快照与checksum；独立原生组合同见CARRIER-CONTRACT，下一Unity同条件自然命中/准确Change。未动生产，Q08未闭、Q06出口保持，禁computer-use。

> Q08 `NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001 / FOCUSED_TEST_PASS_ISOLATED`：正式HP0/剩余生命分支已用现有HP2Orig接入；同SHA隔离Unity三例RED 1/3→PASS 3/3，相邻结果seam 2/2+3/3。原Editor/完整SelfCheck/真实战斗及Q08其它结果语义待验；详ACCEPTANCE。Q06出口保持，禁computer-use。

> Q08 `NTSD28-Q08-RESULT-FLOW-SOURCE-MATCHED-001 / REVIVE2_DIRECT_AND_UNITY_FULLTICK_RED_REPRODUCED`：隔离Unity EditMode直接writer与RunReleaseTick第2 tick各1例RED，组1/2、HP0/HP2Orig2实测BattleEndPhase1，正式source对应timer0；均编译并在目标断言失败，证据见CALLER-AUDIT/REPORT与XML。正式端同初态完整driver、其它分支及精确生产Change仍待；Q06出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / ISOLATED_VISIBLE_PREVIEW_PASS`：同SHA隔离Unity副本正式PNG 1/1、预览类11/11及现有图形验证JSON/PNG PASS；画面已查看、Scene hash保持。原Editor重载及其现场可见预览待，Q07整批未闭；详PROGRESS。Q06限定出口保持，禁computer-use。

> Q07 Sasuke旧图磁盘owner回访：退场CSV为修改前快照；当前磁盘Scene旧GUID引用0、精确脚本文字仅保留旧BMP网格测试1处，旧空根DAT动态路径仍可达；Editor未Reload，删授权仍0。详SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001/OLD-ASSET-OWNER-REFRESH.md。

> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / CODE_WRITTEN / UNITY_MODAL_RELOAD_PENDING`：正式sasu.png importer因RED1024先由独立子Task仅调nPOTScale并1/1PASS；预览PNG alpha、Editor示例/验证、Battle Scene禁用预览GUID及y881已精确写入，Scene仅两字段diff、HUDBg x30/旧BMP/Menu保持。现有Editor外部Scene修改弹窗阻断后续编译/预览图验收，已请用户手动Reload，禁computer-use/第二Editor；详PROGRESS。Q07未闭。

> Q08结果设置stage计数当前权威面修正：正式EXE SHA匹配，但其host只有战斗Scene loop/GUI/smoke及转换边界冻结的LFR回放，已检live path没有mode-4结果设置stage动作。不能继续把“直接取同条件EXE见证”当可执行下一步，更不能把赛前24背景ID写入结果计数；Unity零计数仍仅静态候选。详Q08 RESULT-STAGE-COUNT-AUTHORITY-AUDIT-001/SHIPPED-HOST-SURFACE-ADDENDUM.md；Q08其他项继续，禁computer-use。

> 当前Q07未索引旧`effect/weapon4.dat`静态调用链已审：它不是data.txt OID120的`chars/weapon4.dat`；默认正式/旧索引/手动刷新/正式catalog/Editor补丁未见已知reader，GUID无序列化owner。仅`STATIC_NO_KNOWN_READER`，不证明全局不可达，仍`deleteAuthorized=false`、未删。详UNINDEXED-EFFECT-WEAPON4-REACHABILITY-001/REPORT.md；Q07未闭，禁computer-use。

> 当前Q07 `NTSD28-Q07-WINDOWS-PLAYER-NATURAL-SKILL-001 / VERIFIED_PLAYER_NARUTO_REPRESENTATIVE_ONLY`：Windows Mono Development build-3 0错，独立Player物理L/D/J于tick4/6→frame285 tick8→OID33隐藏tick13/正式ncl.png pic1 tick14，进程exit0/PASS；关闭Stopped/borrowers0、双Scene SHA保持。前build-1编译/前run-2探针方向码错误保留，生产未改。详本包ACCEPTANCE；Q07整批仍IN_PROGRESS，Q06本地出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-LEGACY-DAT-IMAGE-RETIREMENT-GATE-AUDIT-001 / VERIFIED_STATIC_RETIREMENT_GATE_ONLY`：旧DAT138中137由旧data.txt直接索引、1个effect/weapon4.dat仍待动态归属；旧索引图片383有保留的legacy/Editor读取，1张sasuke_0.bmp仍是Battle Scene预览序列化引用；174其他图片归HUD/Menu/地图/阴影等独立owner。521行退场门槛表逐项`deleteAuthorized=false`，正式默认战斗仍用LoganRuntime，未删/重绑任何文件。详本包REPORT/CSV；Q07 IN_PROGRESS、Q06本地出口保持，禁computer-use。

> 当前Q08 `NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001 / VERIFIED_PLAYER_F4_CLOSE_SCOPE`：正式F4的Player关闭效果已接入战斗宿主；两次真实Windows Player物理F4均tick3→3、Stopped/对象池0，第二次进程退出码0；Editor物理键PASS、F4拒绝路由8/8、Scene保持。录像save-pending保护因Unity无对应owner仍待，Q08结果计数和总目标未闭；详本包ACCEPTANCE。下条只读缺口是实施前历史，已由本包局部取代；禁computer-use。

> Q08 `NTSD28-Q08-F4-CLOSE-OWNER-AUDIT-001 / CONFIRMED_PRODUCTION_EFFECT_GAP`：正式playable的F4经录像待保存保护关闭整应用；Unity已有物理键handoff但仅测试诊断消费，生产无关闭owner。先精确Task/Change再接战斗host/Player关闭与有序shutdown验收；不得改为返回菜单。Q07 Scene closure首Scene选择已异步询问，其他可独立工作继续，禁computer-use。

> `NTSD28-Q07-SASUKE-NEEDLE-PHYSICAL-001 / VERIFIED_SOURCE_FORMULA_AND_SCOPED_PLAY_ONLY`：正式Sasuke OID11自然L/D/J两次真实Play通过，tick15帧264四个OID440，四子位置/速度逐项符合正式源码公式、chi.png pic0绑定观察通过；Editor退出、Scene hash/dirty保持、Console error0、Ledger653/15PASS。原EXE同条件tick、命中/完整生命周期/屏幕表现及Q07出口仍待；详ACCEPTANCE，禁computer-use。

> 当前Q07旧图owner清单已复核：52个现存旧路径序列化引用=禁用Editor预览角色图1、Battle HUD/UI 26、Menu UI 14、GameConfig UI 8、地图2、通用阴影1。唯一旧角色图sasuke_0.bmp仍被Battle Scene预览及Editor测试引用，正式sasu.png仅为待验证重绑候选；清单见Q07 OLD-ASSET-REFERENCE-REFRESH-001/SERIALIZED-OWNER-CLASSIFICATION.md。没有重绑或删除，Scene/非战斗保持；Q07仍IN_PROGRESS，Q08 mode-4结果计数仍待权威同条件见证，禁computer-use。

> 当前Q08 `NTSD28-Q08-RESULT-STAGE-COUNT-AUTHORITY-AUDIT-001` 已闭只读边界：正式playable的24有效背景ID进入赛前post-roster菜单；mode-4战果设置stage action在所检playable live path未给出同条件规则。Unity正式根`RuntimeStageCount`静态为0但尚非已证首差，禁止直接写24。下一需正式EXE mode-4同stage/按键与Unity同条件见证；Q08其他有权威证据的子项可先行。详同ID REPORT；Q07仍IN_PROGRESS、Q06本地出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001` 限定VERIFIED：GameDataManager初始化不再隐式读取旧data.txt；空根显式加载保留，聚焦EditMode两次1/1、正式完整发布1/1、序列化根menu Play q07-lazy-menu-1 PASS。最终无行为的空覆盖删除后重新编译/聚焦通过，完整发布/Play为此前等效路径。Q08背景计数、Menu Scene空Build Settings、旧资源删除权限仍未闭合；详本包ACCEPTANCE，禁computer-use。

> 当前Q07 `NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001`限定VERIFIED：真实物理L/D/J的OID33 tick11 frame241/pic999无entry→tick12 frame242/pic1/key(33,1)/正式ncl.png 79×79/中央binding有效，fresh Play PASS；首轮探针FAIL保留。只闭逻辑到图片目录绑定，像素/排序/阴影/整技能待；Menu Scene闭包首差独立保留，Q07 IN_PROGRESS、Q06 DELIVERED_SCOPED。详本包ACCEPTANCE。

> 活跃依赖`NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001`保持BLOCKED：真实Menu回调到Fight后因Build Settings空Scene表无法加法加载Battle，生产Scene闭包确定后重跑。该包的两次Play及负例preflight证据保留；不以Naruto图片目录PASS覆盖此缺口。

> 当前Q07 Windows Player编译/打包子包限定VERIFIED：初28个Player编译错误已修，聚焦AI 11/11PASS；q07-package-build-4 Succeeded/errors0，正式1343文件/46594829字节逐hash及SPARK一致。实际Player内源选择/发布/战斗/关闭待办，GameConfig空根、Scene保持，四个Odin生成文件保留。



> 当前Q07 `NTSD28-Q07-STAGED-FORMAL-CALLER-PLAY-001` 限定VERIFIED：真实App/Menu Play正式暂存内容指纹与三owner一致，World4、旧/新owner零残留、borrowers0、两帧Stopped；GameConfig空根、Scene/旧资源不动。下一Player打包/根映射和实际Player验证，再决定生产切换。

> 当前Q07 `STAGED-FULL-PUBLICATION-001` FOCUSED_TEST_PASS：正式暂存330对象完整Unity发布、906有效图片、三owner同key/所有声明UI图及显式生产清理零残留，job1711ab45 1/1PASS；EditMode夹具OnDestroy事件未触发的三次RED保留，真实Play退出仍待。Player原始资源打包/根映射缺口另见BUILD-PORTABILITY-AUDIT；GameConfig空根、Scene未变、Q07未交付。

> Q07下一子包`NTSD28-Q07-STAGED-FULL-PUBLICATION-001` PLANNED：正式项目本地330对象/906有效图片的完整Unity发布及资源回收定向Editor测试；Task/Change已建，未改生产/Scene/GameConfig。构建携带路径仍未证，生产切换不得提前。

> 当前Q07 `STAGED-CANDIDATE-IDENTITY-001` FOCUSED_TEST_PASS：Unity真实候选正式/暂存两根330对象/906有效图像及全部内容/视觉指纹一致，EditMode job f0b2e9a7 1/1PASS；初次raw1010误作effective906的RED已记录。1343资源字节导入后仍逐hash一致，Scene SHA保持，Ledger640PASS；生产root未切，Q07发布/加载/Play未验。详READINESS.md/Task/Record。

> Q07下一精确子包`NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001` PLANNED：仅新增一个Editor测试，从项目本地root和正式root构建生产候选，比较330对象/1010图像及内容身份；Task/Change已建，GameConfig/Scene/旧资源保持。

> 当前BATCH-04/Q07 `NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001`仅资源暂存VERIFIED：330对象DAT、1010引用PNG及catalog/index/fusion三文件共1343/46,594,829字节逐hash一致；GameConfig未切换，Unity候选/真实加载/Play待验，Q07未交付。见Q07 READINESS.md/Task/Record。Q06本地出口保持，旧资源/Scene/非战斗未动，禁computer-use。

> 当前BATCH-03/Q06 `DELIVERED_SCOPED / Q06_LOCAL_EXIT`：NTSD28-Q06-OPOINT-ZERO-FRAME-SLOT-VISIBILITY-001 VERIFIED，source4/Unity完整tick4+组件1、稳定SelfCheck15:44:25Z、真实Renderer Play3与Q05关闭15:47:21Z、Ledger638/46PASS，Scene SHA保持；详本包ACCEPTANCE与EXIT-RECONCILIATION-001/CLOSED-EXIT.md。下一BATCH-04/Q07 READY，正式DAT/角色图片未迁移，先做只读catalog/parser/引用/GUID清单再精确Task。Q08～Q12/R及例外保持，禁computer-use/非战斗/Scene/未列清单资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 限定VERIFIED：source17/source4双跑、实测RED后kind9/15、type3原始Z、state1002同步RNG及缺帧原始绑定已修；四组立即/下一tick PASS，稳定SelfCheck15:12:22Z、真实Scene Renderer18/关闭15:13:27Z PASS，Scene哈希未变。详ACCEPTANCE-FINAL4.md。Q06旧CPoint/DamageWriter代码出口阻塞均解除，下一仅做Q06/R最终回访和范围保持出口审计；未审前BATCH-03仍IN_PROGRESS，Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 CANDIDATE_PASS：actual fa0e9603六代表+独立case4均PASS。根因LF2Entity.GetCollisionZInt把type3 hit_j当纵深偏移；准确删该fallback，保留显式offset待ownership审计。fixture补角色PostInteraction阶段，早期失败留证。详本包CANDIDATE-RESULT.md。direct17+6/replay6x2复用；下一explicit Type3VisualZOffset写入guard/throwing tail核查，再稳定SelfCheck/Renderer关闭。Q06 HOLD/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 IN_PROGRESS：同初态RED后准确DamageWriter/WeaponBase修kind9动作归属、kind15仅XZ/保留counter，direct17+6/following全0diff；snapshot replay6x2 PASS。actual candidate测试f680ff21在case4(hit_j857/900)候选0vs源1，原因未定，不跳过不改期望。详本包UNITY-PROGRESS.md。下一核collector/fixture/gate，之后稳定SelfCheck/Renderer关闭；throwing tail待，Q06 HOLD/Q07未开始。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 SOURCE17_PASS：actual relation+following双跑SHA9E1BE31E一致；kind9 hit_j优先级/owner/counter分支和kind15仅XZ/无201202排除已测源码确认。详本包SOURCE-RESULT.md。下一声明Unity对应fixture，先完整初态和实际candidate路径RED再最小生产修正；throwing tail仍待分类。Q06 HOLD/Q07未迁移，未重跑已闭任务，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06下一 NTSD28-Q06-DAMAGE-REMAINING-SPECIAL-KINDS-001 IN_PROGRESS/SOURCE_FIRST：DamageWriter余调用实际分类见本包CALLER-AUDIT.md。kind9与kind15出现动作/owner/速度语义候选差异，不能只换binder；type3 hurt中非type3受击帧分支不可由正式typed caller进入，旧weapon helpers无caller保持。throwing tail尚待。准确源码见证Task/Change已建，无生产修改；Q06 HOLD/Q07未开始。

> 当前Q06 CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001限定VERIFIED：24x2/旧8复用，following3+snapshot replay全PASS，稳定SelfCheck14:03:31Z，真实Renderer3/关闭14:04:35Z PASS（borrowers2→2、restore4→4、World/slots双pool0、两帧Stopped）。Scene哈希保持，详本包ACCEPTANCE.md。下一只核查DamageWriter剩余旧binder实际gate并处理确认差异；Q06仍HOLD/Q07未开始，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001 FOCUSED_TEST_PASS：原24x2初态0、实测旧wait1/高帧结算位置RED后仅BattleCpointWriter5调用改原生binder，job615b76c0新24x2+旧8共10/10PASS，raw50/descriptor/relation0diff。详本包FOCUSED-RESULT.md。下一代表following/replay和真实Renderer/关闭，稳定SelfCheck一次；尚非VERIFIED。随后DamageWriter余caller分类。Q06 HOLD/Q07未开始，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001 IN_PROGRESS/SOURCE24_PASS：实际settle/advance双跑一致，声明857..999 kind2可继续/负值翻向，隐式wait0无kind2早退；失效关系wholepass还执行victim孤立212，超时0/181 counter1。详本包SOURCE-RESULT.md。下一准确Unity fixture/真实pass RED，不只RunKind1误判孤立分支；Unity生产未改。Q06出口HOLD/Q07未开始，禁computer-use/非战斗/Scene/资源改动。

> Q06最终出口审计结论HOLD：原始reader表仍有活跃未闭调用，明确为CPoint settlement vaction及失效/超时raw0/181旧binder；DamageWriter generic/type3剩余调用待按实际gate分类，不批量改。详artifacts/diagnostics/NTSD28-Q06-EXIT-RECONCILIATION-001/EXIT-GATE.md（原要求/R后置映射）。下一NTSD28-Q06-CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001 READY_SOURCE_AUDIT，Task已建/无脚本改动。不重做已闭selector/throw。raw50已闭，Q06未交付/Q07未开始；禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 RAW-REMAINING-THREE-BINDINGS-001限定VERIFIED：raw50/0已接，Unity字段10+完整采集2、工具22+87PASS；fresh Logan同内容源/Unity neutral3tick/6pairs/300字段0diff、50项相等，certificatefalse。详本包ACCEPTANCE.md。不是全场景证书；原raw47/3留历史。下一唯一Q06总出口/live-reader/R02/R04-R13/R16归属最终审计；已使用World epoch及后续mode/视听边界保留，Q07正式迁移尚未开始。禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 RAW-REMAINING-THREE-BINDINGS-001 IN_PROGRESS：三carrier raw绑定50/0已接，Unity字段10/10PASS、tool22/22PASS（首次旧missing预期失败留存，新增三null拒绝）；尚待whole-capture/trace工具及fresh同身份源对照。六准确路径，无战斗算法变更。之后Q06总出口/R回访最终审计；未宣布Q06完成/Q07未迁移，禁止computer-use/非战斗/Scene/资源改动。

> 当前Q06总出口审计确认raw3是实际采集缺口：Runtime平台/环境三carrier已存在，但capture仍null、tool仍Missing。下一NTSD28-Q06-RAW-REMAINING-THREE-BINDINGS-001 PLANNED，准确5路径Task/Change已建，先字段/版本合同与RED再接线，尚未改脚本。详本包AUDIT.md。继续保留reader/R回访总审计、已使用World epoch及Q09shadow后置；不重做已闭生产。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 NATIVE-LIFECYCLE-STATE-CARRIER-001限定VERIFIED：已核对原386 XML/载体实际Play及后继full-driver Renderer224/672ticks、碎片和零残留关闭；稳定SelfCheck复用，未重跑。详本包EXIT-RECONCILIATION.md/证据JSON。下一Q06总出口逐项审计及R02/R04-R13/R16后置归属核对，不能只据子任务标签宣告DELIVERED；已使用World epoch/raw3/内容模式视听边界保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 BDEFEND-FIELD-FAMILY-UNITY-001限定VERIFIED：旧四256通过复用，新增真实Renderer八分支13:27:15Z全PASS（raw/Shadow/legacy241/held-free），13:27:16Z恢复4→4/关闭全0/两帧Stopped；SelfCheck13:19:44Z复用，Scene保持，无生产改动。详本包ACCEPTANCE.md。下一核对NATIVE-LIFECYCLE-STATE-CARRIER父出口与已闭frame/fragment后继证据，再Q06剩余出口；不重做Bdefend。Q06未完/Q07未迁移，平台shadow Q09，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06父出口审计：BDEFEND原四256在两个后继XML均PASS，旧失败不再当前阻塞；FORMAL-CANDIDATE-ENTRY-ORACLE限定VERIFIED，FIELD-FAMILY父改FOCUSED_TEST_PASS但保留真实Renderer字段独立性出口。详本包PARENT-EXIT-AUDIT.md/固定XML证据JSON。下一只补四语义分支代表实际Renderer与legacy241/恢复，不重跑256或已验source/SelfCheck。State13包已闭；Q06未完/Q07未迁移，平台shadow Q09，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 STATE13-EXIT-TAIL-RETIREMENT-001限定VERIFIED：source6/focused6复用；两处旧15粒子SelfCheck断言已按源证据修正且失败留存，13:19:44Z SelfCheck PASS；13:20:38Z真实Renderer三代表PASS，13:20:39Z恢复4→4/关闭全0/两帧Stopped。Scene哈希保持。详本包ACCEPTANCE.md。下一核对Q06父记录出口与既有证据，优先BDEFEND字段/入口回链，不重复已验矩阵；平台shadow Q09保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 STATE13-EXIT-TAIL-RETIREMENT-001 FOCUSED_TEST_PASS：实测旧exit各15额外对象/audio1/legacy额外60；准确断开LF2Entity旧producer调用，source6对应job b67b2ac0全PASS。保留C17批准legacy1/virtualN30/state18/反射slot容量helper，未删除资源或非战斗。详本包FOCUSED-RESULT.md。下一稳定SelfCheck/真实Renderer/关闭，尚非VERIFIED；CPoint已闭，Q06未完/Q07未迁移，禁computer-use。

> 当前准确Task NTSD28-Q06-STATE13-EXIT-TAIL-RETIREMENT-001 / IN_PROGRESS：先六例UnityRED，无生产修改。

> 当前Q06 STATE13-EXIT-TAIL-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE6：完整driver双跑SHAE7C23807，state13/action200退出/保持与neutral无新增/RNG/audio，state18正向7粒子。详本包SOURCE-RESULT.md。Unity旧分支候选差异尚待同例RED，不能仅据source直接删除；selfcheck反射slot-counter和virtual N30职责须保护。下一准确Unity Task/Change；CPoint限定VERIFIED，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前精确source Task NTSD28-Q06-STATE13-EXIT-TAIL-SOURCE-WITNESS-001 IN_PROGRESS；六代表完整tick含state18正向控制，Unity生产未改；CPoint限定VERIFIED保持，Q06未完/Q07未迁移。

> 当前Q06 CPOINT-INPUT-ACTION-SELECTION-001已限定VERIFIED：即时/后继370×2、replay6、SelfCheck12:57:55Z、真实pooledRenderer2与Q05恢复/关闭全0/Stopped2framesPASS；见本包ACCEPTANCE.md。下一state13/action200旧late tail只读current-source审计，再精确新Task/Change；不重做已闭CPoint/OPoint。平台shadow明确Q09未完成，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPoint包 FOCUSED_TEST_PASS / FOLLOWING370x2_REPLAY6_PASS：正式Controller+human roster夹具接线后job0fccc80a各370全初态/即时/后继raw47+B2/RNG/关系零差异；job49f1368c六代表snapshot重放及两阶段checksum全PASS。无生产输入改动，旧unbound失败保留。详本包FOLLOWING-REPLAY-RESULT.md。下一稳定SelfCheck+真实Renderer代表/关闭验收；平台shadow Q09、state13/action200审计保留，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPoint包 FOCUSED_TEST_PASS / FOLLOWING_INPUT_BOUNDARY_DIFFERENCE：扩展B2/RNG/关系初态370×2均0差异；jobc4d57852后继各1194差异（input886/其余308），首差previousMask/edge/history。详本包FOLLOWING-FIRST-DIFFERENCE.md。下一核对source非AI sample_pending与Unity active-human/controller门的等价接线/可达性，禁止夹具强写预期或直接重写输入系统。即时370×2/旧throw已验保持；replay/Play未验，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-INPUT-ACTION-SELECTION-001 FOCUSED_TEST_PASS / IMMEDIATE370_TWO_PROFILES：夹具输入投影纠正后RED各2242差异/初态0；准确writer两方法接有序选择、零取消和native绑定，jobac32c386新370×2零差异+旧throw两profile共4测试PASS。详本包IMMEDIATE-RESULT.md。下一完整input/RNG/关系初态与following tick、replay及稳定包验收；不标VERIFIED。平台shadow Q09、state13/action200审计保留，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06下一任务恢复 NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 / SOURCE_CAPTURE_ONLY：source370双跑字节/manifest已复核，Unity仍A/T/J逐次Apply，完整选择链待RED/实施；详本包RETURN-AUDIT.md。总表0.11/0.13已纠正旧OPoint和schema游标。平台fulltick/replay3通过，原阴影出口显式归Q09且仍未完成，parent不升格VERIFIED。state13/action200审计待；Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前平台 parent FOCUSED_TEST_PASS / FULLTICK_REPLAY3_PASS：actual Unity job af41921a 三代表连续两tick及snapshot重放全部通过，12逐tick投影/XML已归档；详 artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/FULLTICK-UNITY-RESULT.md。无生产新增，复用稳定SelfCheck/Renderer。下一核对parent原阴影出口与Q09明确交接及Q06剩余清单，不能静默缩小VERIFIED范围。Q06未完/Q07未迁移，禁止computer-use/非战斗/Scene/资源改动。

> 当前平台 parent：完整 source tick 三代表已双跑一致（SHA3079EE34），原21分段输出字节不变；首tick建link/次tick跟随已观察。详 artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/FULLTICK-SOURCE-RESULT.md。下一仅补Unity相同三代表连续tick及snapshot replay；尚未验证，不升格VERIFIED。复用稳定SelfCheck/Renderer，Q09阴影回访保留；Q06未完/Q07未迁移，禁止computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 已限定VERIFIED：source12/61+Unity17+following/replay3，稳定SelfCheck复用；真实Renderer四代表（平台0/physics20、tail7/9）12:22:51Z全PASS，scenechecksum/borrowers2→2，Q05 restore/全0/两帧Stopped12:22:52Z PASS。详tail ACCEPTANCE.md。平台parent仍FOCUSED_TEST_PASS，下一补其actual source完整tick及snapshot replay（不能用分段API组合或tail测试代替），mixed3已验；Q09阴影consumer回访保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-DESTROY-POOL-OWNER-001 已限定VERIFIED。真实factory Renderer三类first12:18:20Z/reenter12:19:39Z各3PASS，原pool归还/独立World/重复Destroy通过，scene checksum及borrowers2→2；各自Q05 restore4→4/Worldslots双pool0/两帧Stopped PASS（末次12:19:40Z）。详本包ACCEPTANCE.md；稳定SelfCheck复用，无生产追加。下一平台/frame-motion代表Renderer语义验收仍独立待做，Q09阴影回访保持；mixed3/旧late两项已闭证据保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001 FOCUSED_TEST_PASS / UNITY3_AND_STABLE_SELFCHECK_PASS。actual World候选job bde7da1f3/3PASS，runtime/range候选0/1/0及integer/preciseY/reference/link一致，无需生产修改；本组一次fullSelfCheck 2026-09-21T12:14:34.368890+00:00新鲜PASS，详本包UNITY-RESULT.md/SELF-CHECK.json。下一准确代表Renderer平台/frame-tail与Destroy原pool归还/退出重进零残留验收，不能把SelfCheck升格整包关闭；Q09阴影consumer回访保留。旧late两失败已闭，frame-tail17+following3复用。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001 IN_PROGRESS / SOURCE3_PASS。actual source平台20/rider21与attack19或22：ordinary候选0/1/0，证明吸附后用更新integerY而非preciseY；双跑SHA62D0258D。初版攻击者自身被平台吸附导致全0已留证，移至范围外并保留世界攻击框后预期通过。详本包SOURCE-RESULT.md。下一准确Unity3例fixture声明/实际候选入口验证，生产未改；旧late两失败已闭，frame-tail17+following3复用。稳定SelfCheck/Renderer/Destroy关闭与Q09阴影回访待，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001 IN_PROGRESS / SOURCE_FIRST，准确单CPP测试平台吸附前后slot顺序与ordinary窄几何交错，Unity未改。两旧late失败已限定关闭；Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-STATE2000-FACING-ORACLE-001 已限定VERIFIED / TEST_ORACLE_ONLY。actual source step_frame_slot四例双跑SHAF4846CA2，证明state2000 held正零负速度保留朝向/negative-next才翻转；仅旧测试改名和源期望+翻转/action/counter控制，job492179a3两测试PASS，CS0/Ledger631PASS。详本包ACCEPTANCE.md，无生产改动。此前late两失败已分别由RECOVERY-NOOP-FIXTURE与本包闭合，不改写旧23中21PASS/2FAIL历史。下一回平台mixed普通交互及稳定SelfCheck/Renderer/Destroy关闭；frame-tail17+following3保持，Q09阴影回访保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-STATE2000-FACING-ORACLE-001 IN_PROGRESS / SOURCE_FIRST，先actual source step_frame_slot正零负速度/negative-next控制见证，不改生产或先反转旧断言。回血no-op测试前提已关闭；Q06未完/Q07未迁移。

> 当前 NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001 已限定VERIFIED / TEST_ONLY：只给旧no-op测试显式World phase12/3=1，原HP/PP/NoOp断言保留，额外确认partial调用不推进相位；job b24b523e非周期+周期对照2/2PASS，CS0/Ledger630PASS，详本包ACCEPTANCE.md。此前两个late失败现解决回血前提一项；state2000朝向仍待actual source step_frame_slot见证，不能恢复旧兼容分支来迎合断言。frame-tail17+following3证据保持；平台mixed/稳定SelfCheck/Renderer/Destroy关闭及Q09阴影回访仍待。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001 IN_PROGRESS。已确认旧no-op测试未设非恢复World相位；仅准确测试方法先声明后修前提，保留HP/PP/no-op断言。state2000朝向失败仍待源码见证，平台和frame-tail已验职责保持；Q06未完/Q07未迁移。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 FOCUSED_TEST_PASS / FOLLOWING_REPLAY3_PASS。实际source fulltick rows1/7/9双跑SHA4F9E3443，原immediate字节保持；Unity夹具AI配置顺序修正后jobe49f2970三个following+snapshot replay全PASS（位置/速度/reference/delay源对照+完整runtime回放checksum；未对照全部native raw/RNG）。详本包FOLLOWING-REPLAY.md，复用17既有证据。平台阴影缺口已审：snapshot/copy/legacy/central未消费offset，shared地面锚点还供脚下标记，必须仅改shadow，见parent SHADOW-CONSUMER-AUDIT.md；Q09回访依赖保留。下一Q06 mixed平台/普通交互与两个late失败回访，再稳定包SelfCheck/Renderer/Destroy关闭；Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 FOCUSED_TEST_PASS / SOURCE_TAIL17_PASS。source12/61复用，Unity RED12中5PASS/7尾部FAIL→准确LF2Entity尾部接入后job0bf80a40共17/17PASS（source12+旧kernel5）；新鲜compile CS0，详本包UNITY-FOCUSED.md。现有整数kernel/linked保持，下一真实following tick/replay及平台mixed/shadow，再稳定包SelfCheck/Renderer关闭。两个late失败与Destroy-owner Renderer门槛仍待。Q06未完/Q07未迁移，schema17/25/28 raw47/3；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 IN_PROGRESS / SOURCE12_TABLE61_PASS。actual source API双跑一致SHA2AD148A4，12场景/61表断言通过，身份匹配；详本包SOURCE-WITNESS.md。自身整数速度kernel保留，正delay四分之一与float dx/dy/dz、linked-before-own及pending/失效link已获源证据；Unity RED/实现尚待准确声明。平台/Destroy-owner未验出口和2个late失败回访保留，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。权威frame_motion.cpp证实自身dvx/dvy/dvz按integer读取，纠正此前“自身float速度缺口”的推断；不改已匹配integer kernel。缺口为正DelayTimer134速度×0.25和float dx/dy/dz尾部，先准确两source诊断脚本。平台与Destroy-owner未验出口及2个late失败回访保留。Q06未完/Q07未迁移，schema17/25/28、raw47/3；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / PREVIOUS_Y_CORE：3个成功mechanics尾部记录旧integerY，source20RED1→virtual/ECS+skip8PASS，type1core4PASS。相邻FrameAdvanceRuntimeSnapshot实际23中21PASS/2FAIL（late回血400vs401、state2000朝向rightvsleft），代码入口不经过本次physics但未证历史先前通过；保留失败，需current-source回访，禁止直接改断言。详本包PHYSICS-HISTORY-STAGE.md。下一优先own-frame native float/delay134/dxdy尾部完整源见证，再mixed/fulltick/replay/shadow；Destroy owner子包Renderer/关闭待。schema17/25/28、raw47/3，Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-DESTROY-POOL-OWNER-001 FOCUSED_TEST_PASS / LOGIC_ONLY_OWNER_RETURN：三个Destroy override先保存原pool再detach，RED9b993b58三类均失败→e7476287三类全PASS，含独立World隔离/重复Destroy/关闭。详本包FOCUSED-RESULT.md；Renderer路径及稳定包关闭仍待，不能标VERIFIED。下一回平台parent补physics previousY与自身float/delay/dxdy尾部，再mixed/fulltick/replay/shadow，最终联合验证保留本包。HEAD外部推进72ecf16e已保留。schema17/25/28、raw47/3，Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-DESTROY-POOL-OWNER-001 IN_PROGRESS / RED_FIRST：独立池归还回访，准确3个Destroy override+新fixture已声明；先逻辑对象3类实际factory/隔离World复现。平台父包保持未关闭，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / LINKED_MOTION_REPRESENTATIVES：linked X/Z/Y已接，初RED21→eaef8c44中20PASS+移除案例90ba7a6a单独1PASS；不是一次21PASS。详本包MOTION-STAGE.md。移除测试现用既有Free契合source直接despawn；另发现Destroy在Unregister后找pool导致独立World清理失败风险，下一单独精确Task/Change回访，不能因fixture绕开而丢弃。随后actual physics previousY、自身float/delay/dxdy尾部、mixed candidate/fulltick/replay/阴影待。schema17/25/28、raw47/3；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / CANDIDATE_SOURCE21_PASS：有平台时slot有序ordinary/platform交错，current ITR+snapshot frame、float32 dvy、点接触/吸附已接；source21×default/brute42PASS，相邻11PASS，初RED保留。详本包CANDIDATE-STAGE.md。下一先准确声明linked motion与physics previousY生产路径，扩展afterMotion/真实following/replay，再阴影。history当前仅fixture种入，未实际生产；非完整平台验收。schema17/25/28、raw47/3；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / CARRIER_TRANSPORT_ONLY：新增platformSlot/shadowOffset/previousY持久carrier，初RED3→恢复/hash/identity4PASS，trace合同87PASS，source诊断编译exit0。新联合schema entity17/aggregate25/checksum28（core12/shell2/2），raw47/3未改。详本包CARRIER-STAGE.md；候选吸附RED2未修，history producer/linked motion/阴影consumer尚未接入。下一声明准确有序候选与位移路径，不称平台完成。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 COMPILE_PASS / CANDIDATE_RED_CONFIRMED：job a7ca8aa2执行4，边界2PASS/普通吸附2FAIL（Y期望-20实际-10），初态位置/reference通过。证据本包candidate-red-a7ca8aa2/REPORT.md；仅投影测试，production/history/schema/阴影未实施。下一先精确完整carrier/候选有序路径合同后实施，不跑无关全套。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 IN_PROGRESS / FOCUSED_RED_FIRST：准确新平台测试路径已声明，先 nominal/strict-edge 的真实候选投影；生产未改，完整history/schema/阴影仍待。Q06未完/Q07未迁移。

> Q06 平台接入审计更新（2026-09-21）：只读确认普通候选 bdy 过滤不可用于平台点接触，需保持每对 ordinary/platform 双向交错及吸附后缓存语义；已定位角色/共享非角色/武器物理同步和 legacy/central 阴影消费者。详 `artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001/UNITY-INTEGRATION-AUDIT.md`。下一完成 shared-character/weapon 提前整数同步及 snapshot/hash/capture 精确路径后建 Unity Task/Change。生产尚未实施；本轮无 Unity 测试，复用源 21/277，按受影响分支验证。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE21_FORMULA277_PASS。source-final双跑SHA9671B8D3，21/277通过；多平台顺序、snapshot/current分工、slot0/失效link、fractional取整及physics previousXYZ已测，详EXTENDED-ACCEPTANCE.md。诊断2脚本，Unity未实施；API组合不是fulltick证书。下一先精确审Unity physics整数同步/候选顺序/frame-motion及carrier reset/copy/snapshot/hash/shadow合同，再建准确实现Task。旧environment/碰撞reference和OPoint出生已闭职责保持。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE10_PASS_EXTENDED_CONTRACT_PENDING。准确单CPP actual候选→frame-motion已build/双跑SHA74FD90C9，10场景120检查PASS；整数吸附与精确Y不同步、整数origin位移及type3状态门槛已测。详同ID PROGRESS.md。下一补多平台/顺序、snapshot/current、ITRdvy、sentinel及完整tick previous-position源合同，再声明Unitycarrier；尚无Unity生产修改/不称平台已对齐。旧environment/碰撞reference已闭职责保持，OPoint事务限定VERIFIED。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确单diagnostic CPP已预声明，先actual候选op30→linked frame motion源见证，Unity生产未声明未改。CollisionYReference reset与environment伤害/投掷保持关闭；平台需previous-position/slot/shadow完整合同，不以raw null推断环境carrier缺失。父OPoint事务已限定VERIFIED；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 已限定VERIFIED / NATIVE_ORDINARY_LATE_MATERIALIZATION。source23+rest3+boundary6；immediate46、followingReplay2、boundary14、resolver3/V3各329PASS；稳定SelfCheck、Renderer3/关闭及最终guard受影响Renderer2/10:38:57Z关闭PASS。scenechecksum/borrowers2→2、restore4→4/finalWorldslots两pool0/Stopped2frames；详同ID ACCEPTANCE.md。父emission调度仍属上游，不把局部证据当全部场景证书。下一回Q06剩余依赖审计，优先platform/environment及父生成门槛所有权，不重做已关闭出生事务。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / ACCEPTANCE_GATES_PASS_CLOSURE_AUDIT_PENDING。state3003源3/113双跑SHAca4698c6确认无新rest并保留7；精确旧SelfCheck断言纠正后PASS。真实Renderer3于10:31:32Z/关闭10:31:33Z均PASS，scenechecksum/borrowers2→2、restore4→4/finalWorldslots两pool0/Stopped2frames。详同ID ACCEPTANCE-PENDING.md；复用immediate46/followingReplay2/resolver3/V3各329证据。下一收尾审查first-record Kind/Oid整帧早退与random越界action未覆盖分支，必要精确source/test后再修，不能直接宣称全事务完成。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / FOLLOWING_REPLAY_PASS_SELF_CHECK_RED。immediate46复用；source0/15真实fulltick+snapshotReplay2PASS；组件准入复用既有resolver修正后capacity/normal/missing3PASS；旧V3指纹fixture精确纠正后两profile各329出生值PASS。SelfCheck旧lives断言按source修正后，重跑停state3003额外bilateral10/mutual40 vrest旧断言；详同ID FOLLOWING-PROGRESS.md，失败均归档。下一先补type3/state3003关联对象源rest见证，不能直接反转断言。真实Renderer probe0/5/15已声明编译未运行，Play/关闭待SelfCheck；Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / IMMEDIATE_46_PASS。准确四生产路径共用native ordinary birth已写；先6代表PASS，再稳定包job90b6b8f4联合46/46PASS，全部before/after/RNG差异0。详同ID IMMEDIATE-PASS.md及immediate-pass-90b6b8f4。仅logic-only两late caller，真实Renderer/后续tick/replay/稳定包SelfCheck与Play关闭仍待；独立review受agent limit未运行，不冒充完整验收。Scene dirtyfalse/root14/hashBCD1047B保持，Ledger625/122PASS。下一既有fixture补source0/15真实following/replay并审真正Renderer入口，不重跑无关全套。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / ADMISSION_WRITTEN_PARTIAL_TEST_PASS。准确两late caller已修kind/缺OID/缺action/容量终止控制；compile CS0，focused job78c84f30仅8例4PASS/4FAIL，19/20通过、18/22拓扑匹配但初始化5项差异仍失败，断言未弱化。详同ID admission-78c84f30/REPORT.md。下一先声明准确初始化生产路径，再补共同birth/RNG/native action/link/double motion；本轮未跑fullSelfCheck/Play。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / UNITY_RED_CONFIRMED。归档job8a2ae10b77b3436188b906a582d62fb9：46例初态全部零差异，生成后2PASS/44FAIL，仅满容量双入口控制通过；详同ID RED-EVIDENCE.md。两actual late caller均为logic-only materializer，不代表Renderer factory/Play已验。生产路径尚未声明、尚未修改；下一审准入/初始化共用边界并先登记准确路径。按影响分支验证，稳定包才集中回归，复用未变证据。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / UNITY_RED_FIRST，准确新EditorTests已声明，生产未改。source23/360/双跑身份已验；先两actual late caller逻辑载体代表，Renderer factory/Play另验，不冒充两factory完整通过。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE23_FORMULA360_PASS；最终source-final双跑SHA12ACA913…250FD7，正式EXE/closure匹配。详同ID REPORT；准确source2脚本，无Unity生产修改。下一为两actual materializer建立精确Unity RED fixture Record，比较完整初态/出生字段/RNG/link/admission及代表following，再声明生产路径；保护clone/piece/vitals已验职责。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；准确单诊断CPP已预声明，Unity生产未改。父OPoint剩余consumer回访，复用vitals/weaponHp/owner成果；补完整生成顺序、RNG、continuation/depth/link/admission代表。前post/display VERIFIED；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06游标：NTSD28-Q06-OPOINT-REMAINING-CONSUMER-AUDIT-001 READ_ONLY_RETURN / 父frame依赖已解除，优先R06/R12完整materializer。post/display保持VERIFIED。最新只读审计见artifacts/diagnostics/NTSD28-Q06-REMAINING-DEPENDENCY-RETURN-20260921/AUDIT.md；总表0.11/0.13已纠正旧游标/schema/display状态。平台op30缺完整producer及linked motion，需要previous-position/shadow联合合同；environment两字段已有carrier，raw null不代表未实现，KO事件归Q08。下一先准确source witness Change与两factory调用链，禁止重复旧vitals/owner/clone职责。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06：NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 已限定VERIFIED。准确三脚本；source2379零差异，focused10/10及真实frame/replay+相邻joint6/6通过；一次SelfCheck09:37:56Z、真实pooled Renderer代表1于09:39:41Z、Q05关闭09:39:42Z PASS。scene checksum/borrowers2→2，restore4→4/finalWorldslots两pool0/Stopped两帧；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6。独立review PASS/GO-WITH-NOTES；synthetic catalog和source-derived405边界保留，不冒充正式DAT/画面对齐。下一回Q06剩余live-path/platform/environment及Host依赖清单，先只读定位，不重做已关闭display/post职责。Q06未完/Q07未迁移/目标ACTIVE；禁止computer-use/非战斗/Scene/资源改动。

> 当前Q06：NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 FOCUSED_TEST_PASS / 10_CASES_AND_2379_VECTORS。job8866ce8e实际10/10 PASS，sourceVectorsExecuted=2379/differences=0；local/native pending两例及raw-action后computer两方向通过。准确三脚本生产已写；保留初RED、pending夹具纠正及computer真实RED。仍待真实frame body接续、组合snapshot/replay代表、稳定包联合/一次SelfCheck/Play关闭；不是VERIFIED。Q06未完/Q07未迁移/目标ACTIVE，禁止computer-use/非战斗/Scene/资源改动；复用未变证据，不重跑全角色。

> 当前执行NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 IN_PROGRESS / SOURCE_REUSE_AND_FOCUSED_RED_FIRST，准确三路径Task/Record已建。post2379 SHA4C8CC601…8B644/sourceclosure07CD47复用，先向量及生产位置RED；新无状态writer整事务，不重做preHP/MP/display。前clone/display均VERIFIED；Q06未完/Q07未迁移/目标ACTIVE，禁止computer-use/非战斗/Scene/资源改动。

> 当前Q06：STATE9996-DIRECT-SPAWN-TRANSACTION-001与父NATIVE-DISPLAY-PROGRESSION-001均限定VERIFIED。子包准确13脚本，source12/813、immediate/shared20、following/C25六、SelfCheck09:06:05.218Z、真实Renderer2/关闭09:07:01Z PASS；父display新joint13/980 PASS。保留初RED/旧identity及nativeHP10自检失败和纠正证据；限定诊断source/单following tick/相对容量，不冒充EXE画面。Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持，raw47/3/schema不变。下一唯一任务NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 READY_FOR_EXACT_PRECHANGE_RECORD，WAIT_DISPLAY_OWNER解除：复用2379source，先精确Change/路径与RED，补405非零motion；尚未生产实施。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；禁止computer-use/非战斗/Scene/资源修改，不重做已验职责。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 FOCUSED_TEST_PASS / IMMEDIATE_AND_SHARED_BIRTH_REPRESENTATIVES；非VERIFIED。准确10脚本（source2/test2/生产6），native direct500/implicit/current descriptor/pending/RNG后容量及两factory已写；初RED12中9FAIL/初态全同，修复后clone12+poolreuse1+admission5 PASS。piece旧V3前fingerprint先失败，准确声明后只修测试身份前置；shared piece+state18两个代表再PASS，共20项有效证据。编译CS0、scene dirtyfalse/root14/hashBCD1047B…0E9FB6、Ledger622/113 PASS。下一补source following0/2（helper后driver确实再生5个，禁止counter补偿）、renderer代表、旧C25 native oracle后稳定包验收；本轮未跑fullSelfCheck/Play。详本包PROGRESS.md。父display/Q06未完，Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 IN_PROGRESS / UNITY_RED_CONFIRMED_IMPLEMENTING。2026-09-21 08:44 UTC job3056fb54实际12例3PASS/9FAIL，所有beforeDiff0，证据unity-red-3056fb54；差异为native出生500、display/frame历史/pending motion、implicit准入、容量后RNG、pending guard。已先声明准确6生产路径，再实施专用transient clone标记/Clear、两factory/shared direct birth及producer顺序；新增pool归还后普通OPoint隔离测试待编译。不是完整验收；following/双materializer/稳定包回归仍待。按受影响分支测试，未跑全角色；父display/Q06未完、Q07未迁移、总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 IN_PROGRESS / SOURCE12_PASS_UNITY_RED_PENDING，准确source2+新fixture1路径声明，Unity生产未改。source-final/first.jsonl才是最终证据：12例/163239bytes双跑SHA090B9773…1D09F6，独立813/root复跑PASS；native500/500、implicit帧、partial/full仍34RNG已实测。following是helper后driver，未改counter所以再生成5个，不能误作两自然tick。UNITY-SEAM-AUDIT记录专用transientflag/Clear、两即时factory/共享direct birth候选；不得冒充weaponpiece或改普通OPoint。native1000 vsUnity400/1050容量案例只比相对空槽边界，不伪报全occupancy。新Unity fixture正在编写，未运行。前普通Stage/AIalias保持VERIFIED；父display/Q06未完，Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确2工具脚本预声明，Unity未改。当前source07CD47采用direct requestHP/MP500、implicit0..3可用、先随机tuple后spawn_transient容量拒绝且继续剩余attempt；Unity仍HP10/HasAuthoredFrame/提前容量break。旧B3克隆record的closure39DDDA15已追加current-authority回访说明，不能重用旧HP10/容量假设；成功5克隆拓扑和callsite顺序职责保留。下一实际source API代表见证+独立校验→准确Unity RED/整事务接线，不只改HP。普通/Stage显示已VERIFIED/证据复用；父display随后联验再post-display2379。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001 已限定VERIFIED / ORDINARY_STAGE_DISPLAY_BIRTH，准确4脚本；复用identity匹配的display980源证据，初7个真实显示RED+1snapshot夹具假设错误保留，joint21/21PASS；snapshot先清local释放slot再restore八markers通过。一次SelfCheck08:15:15.455Z、Play普通/type3 Stage Renderer2于08:15:55.922Z、Q05close08:15:56.504Z PASS；scene checksum/borrowers2→2、restore4→4/finalworldslots两pool0/Stopped2帧，CS0/dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一明确依赖state9996 direct clone整出生资源：source battle_world2998 requestHP/MP500(EngineProfile)，Unity LateModule native分支792后硬写10且先用普通OPoint stats，先准确Task/Change+实际source见证，不能仅把display同步10或修改通用OPoint已验算法。随后父DISPLAY-PROGRESSION联验，再接POST-DISPLAY原2379事务。AIalias/普通出生/OPoint/融合已验职责复用。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/Play；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001 IN_PROGRESS / FOCUSED_RED_FIRST，准确4脚本已声明，生产尚未改。DISPLAY-BIRTH-RETURN-AUDIT确认普通Initialize/Stage最终HP未同步出生显示；旧OPoint与weapon-piece出生已VERIFIED不可重做。另发现state9996 native direct clone应HP/MP500，当前Unity普通OPoint后硬写10，需独立整事务源见证，禁止只把display同步到10。优先普通/Stage显示子包→clone完整资源→父display联验→既有post-display2379事务；modeQ08/正式内容Q07仍待。前AIalias已VERIFIED/证据复用；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001 已限定VERIFIED / NATIVE_AI_PERSISTED_ALIAS_CONSUMPTION，准确9脚本，详本包ACCEPTANCE。source18/408双跑SHA E3CCEF…C07D，初carrier RED4/decision RED8of15保留，joint27PASS；真实source16/17两tick+snapshot恢复3/3、正gatecaller1及staleguard1通过。一次fullSelfCheck07:50:14.786Z、Play pooledRenderer2于07:51:03.615Z、Q05关闭07:51:04.211Z PASS；scene checksum/borrowers2→2，restore4→4/finalWorldslots两pool0/Stopped2帧，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。仅fixture初始化canonical/acceptedtrace/Match.Difficulty0纠正，源DAT/pending未变，无每tick补偿。默认/配置DataOrientedCanonical静态已确认；异常fallback/显式legacy未制造，不宣称全部AI路径。下一回Q06剩余live-path/late-display-postdisplay/platform清单，先审最接近消费者及依赖；已有alias/融合职责不要重做。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/Play；禁computer-use/非战斗/Scene/资源修改，按受影响分支验证。

> 当前Q06：NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001 IN_PROGRESS / ROWS_AND_KERNEL_FOCUSED_PASS，source18/408双跑一致，carrier初RED4，decision初RED8/15→joint27/27PASS。四row+kernel已写且独立review限定PASS；actualID1追击保留，unsupported只停AI decision不停止tick。准确9脚本已声明，新增真实two-tick/restore/stale fixture编写中，尚未运行；main正gate新测试待运行。详本包PROGRESS.md；整包未验收、Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改，不重跑已验融合。

> 当前Q06：NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确初始两个diagnostic路径已声明，Unity脚本尚未修改。先实际source API证明classifier/match/actualID分离及special提前返回/RNG/采样，再Unity RED和准确生产接线。审计见NATIVE-AI-ALIAS-CONSUMER-AUDIT-001；融合证据复用，不跑全角色。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NATIVE-AI-ALIAS-CONSUMER-AUDIT-001 静态审计及独立复核完成，详 artifacts/diagnostics/NTSD28-Q06-NATIVE-AI-ALIAS-CONSUMER-AUDIT-001/AUDIT.md。已确认派生AI行缺alias，classifier/match语义不同，special 0x3c后非零alias停止ordinary但仍采样/推进tick；正式方法名step_profiled_combat。下一先准确source witness Task/Change与Unity RED，再修行采集/增长/比较及特定kernel分支，禁止全局替换ObjectId或重复已验融合职责。本轮仅文档，无脚本/Unity测试/Play；按受影响分支代表验证，复用无变化证据。Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：融合四包FUSION-RECORD-TRANSACTION / PERSISTENT-CARRIERS / SELFCHECK-ORACLE / CATALOG-TRANSACTION-WITNESS均按各自限定scope VERIFIED，详各ACCEPTANCE。原source4/1737保持；当前即时4/following2/replay4/AI2、requiredslot拒绝通过；oracle初2/7→joint17/18→仅mirror1/1纠正，非虚报18/18独立job。一次fullSelfCheck06:59:18.984Z PASS；真实Play6于07:00:39.067Z PASS(renderer4+Mobilelogic2，scene fast64/borrowers2→2)；Q05close07:00:39.650Z PASS，restore4→4/finalworldslots两pool0/Stopped2frames。Editor idle非Play，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。schema16/24/27/core12，raw47/3不提升。不可把本批解释为完整Host/AI：featurekey准入/跨场景保留、poststory第二投影/stage时序、NativeAI alias消费者、previousXYZ/opoint诊断仍待；source suspended槽本来禁止reuse，不重构allocator。下一回remaining live-path审计，优先精确追踪NativeAI persisted alias→Unity row/resolver再建Task，不凭字段存在称AI已齐；late/display/postdisplay/platform等backlog保持。Q06未完/Q07资源未迁移/总目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-SELFCHECK-ORACLE-001 IN_PROGRESS / FOCUSED_RED_FIRST，准确SelfCheck七方法及新反射runner两路径已声明；修正旧partialRecovery/partnerReset/latch断言，不改已验融合生产。先七方法定向实测，再更新源依据；一次fullSelfCheck留稳定后。融合事务前即时4/following2/replay4/AI2保持，代表Play/关闭仍待，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-RECORD-TRANSACTION-001 IN_PROGRESS / IMMEDIATE_FOLLOWING_REPLAY_PASS_SELFCHECK_PLAY_PENDING，准确3脚本已改。source4 before均0，原RED差异已归零；最终job33a3f30bde7640f481836e941a08db2f即时4+following2全PASS，jobe0a516a9f1564cf4999c0b5504b45d62融合前/暂存snapshot replay4+AI成员2全PASS，CS0/独立review。完整record merge/split已写，partner不Reset、失败无部分提交、implicit310/属性/状态保持符合支持tuple。native suspended槽原本就禁止reuse（source1233/1297/1504），不要重构allocator；opointLatch/previousXYZ仍source-only诊断，raw3不提升。下一唯一收口依赖：准确新SelfCheck oracle Task，先7个CheckOid5152方法定向RED再按源纠正旧WaitCounter37/partnerReset/partialRecovery断言；之后本组一次fullSelfCheck与代表Play/有序关闭，未运行这些验收不能关闭整包。persistent carrier父仍IN_PROGRESS；featurekey/poststory/NativeAI回访保留。Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-RECORD-TRANSACTION-001 IN_PROGRESS / UNITY_SOURCE4_RED_FIRST，准确单新fixture路径已声明，先真实World scan与source4对照，未改融合生产。persistent carrier26路径前阶段证据保持，整包仍待整体验证；global poststory/键准入/NativeAI消费者回访保留。下一根据真实RED完整替换record merge/split并精确声明生产路径，禁止只改getter/partner.Reset/混用formal身份与synthetic数据。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS / FEATURE_CONFIG_CORE_PROJECTION_PASS_FULL_FUSION_PENDING，准确26路径。World显式双global配置、registry first→occupied entity/raw(排除dormant/空raw)、共用RunTick preBattleFlow(main/worker)及rawscenario同名bool已接。初RED7，最终job553b98f94db849928070495c94152d5e联合11/11，新10+正式capture1；midtick出生默认false到下一实际tick更新已测。未启动新的worker线程/未跑无关SelfCheckPlay。source poststory第二投影依赖stageHost旧时序待办，完整功能键准入/跨场景保留未接，不得把此stage标完整Host对齐。下一创建准确record-driven融合事务Task和Unity source4 RED；prepared/table/identity/5carrier/birth/input/显式global已足够，替换完整merge/split而非getter，保持C12/C25h/timer已验职责。carrier整包仍IN_PROGRESS等待整体验证，Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS / BIRTH_INPUT_FOCUSED_PASS_GLOBAL_PROJECTION_PENDING，21准确声明路径。alias/drop四shell出生helper及独立ModuleBind开关、snapshot caller false、输入RouteNativeHitJa持久alias已接；常规DAT换绑不覆写。初RED4保留，joint18/18(job13e9bdc8a1264cc385db1123cfc43b6d)，再直接相关fresh-shell/removal replay2/2(job1a0df8ce8660436a8454dde9e7b2441a)，独立review通过。前storage14及schema16/24/27/core12证据保持，不重测无关项。下一同Task先准确声明global配置/first→entity投影：审共同Host/worker入口和source preclassification/poststory时点，rawscenario加同源bool，不改菜单/Host键序列准入、不用getter投影或GameMode猜值；随后完整fusion source4事务。Stage/roster本轮是ModuleBind共享调用链证明非新stagePlay。整包未关闭、Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS / STORAGE_SCHEMA_FOCUSED_PASS_PRODUCERS_PENDING。12声明路径已写，3entity+2World字段copy/reset/corecapture/restore/fast64checksum完成；当前entity16/aggregate24/checksum27/core12、shell2/2。初RED5、初joint21中12PASS9FAIL（fixture误用独立diagnosticJSONchecksum，原结果保留），纠正入口后job130432bfbcb54709bbec6bbf83fb5684新14/14PASS；genericentity/raw1+Q05trace6先前已过且生产未变。Parity84/84+19/19、新nativebuild与Unityfreshheader一致、CS0/review通过。不得关闭整包：下一先准确追加birth/helper/aliasreader/fixture路径，显式覆盖四shell+roster/stage并保留snapshot/普通变身；不可复用OPoint-only writer或偷偷扩展armorflag语义。global配置及投影时序也待落实，完整fusion未实现。诊断JSON与raw47/3未自动扩展。无运行job/build/Play，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS，准确12初始脚本声明；联合entity16/aggregate24/checksum27/core12存储合同，先字段/copy/reset/快照/hash，出生与消费路径必须追加准确声明后继续，尚未闭合。旧prepared9/9与identity33/33证据保持各自版本范围；raw47/3不提升。总目标ACTIVE/Q06未完/Q07未迁移，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-CARRIER-PRODUCER-AUDIT-001 只读审计完成，新增证据已归档CARRIER-PRODUCER-MATRIX.md。正式global第一feature值明确投影到entitygate（init/切换/分类前与story出生后pre-tick），二者不同所有权但有关联，不能简单称完全独立；toggle可反转/LFR恢复，Host键准入未对齐。AIalias/drop仅birth/fusion写，FrameCache.Load非通用初始化点；四shell显式birth及stage/roster需覆盖，OPoint vitals helper不完整。当前尚未改carrier代码；下一建准确联合carrier/schemaTask（建议entity16/aggregate24/checksum27/core12一次升级，当前仍15/23/26/core11），先copy/reset/snapshot/hash负例再显式birth/input reader；不自动改AI/菜单或覆盖entitygate时序。prepared接线9/9保持VERIFIED；完整fusion/source4仍待，Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-PREPARED-WORLD-CATALOG-001 已限定VERIFIED / PREPARED_DATA_WIRING_ONLY，准确6脚本。新World table/identity、Host已发布双manager引用守卫beforeUnseal、两rawcapture/replay入口接线完成；legacy显式null且重备清旧引用；Logan staged resolver避免拒绝时部分修改。初RED4保留，最终job20024fe8ede64b78a70e919d4c6bf2d3联合9/9（新7+旧catalog1+正式capture1）、CS0、独立review通过，新actualheader保持V3。未激活融合consumer；null config容许partial，不能称全部融合资源ready/整个wrapper深冻结。未跑无关SelfCheck/Play。下一准确carrier/schema及出生/变身持久化合同：190独立、AIalias/drop不可当前wrapper派生、全局featurepair与entitygate分离、318复用RenderPicOffset，随后source4完整融合事务RED/修复。Q06未完/Q07未迁移/目标ACTIVE；Scene hashBCD1047B…0E9FB6保持，无运行job/build/Play；禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-PREPARED-WORLD-CATALOG-001 IN_PROGRESS / PREPARED_DATA_ONLY，准确6路径已声明。把已冻结Logan table/identity一起传入既有World catalog和pre-seal入口；legacy显式null，不从环境猜fallback；不改融合事务/新carrier/schema。先新focused RED，再实施及相关capture验证，不跑无关全量。前COMPOSITE身份已VERIFIED。总目标ACTIVE/Q06未完/Q07未迁移，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001 已限定VERIFIED / COMPOSITE_CONTENT_IDENTITY_ONLY，准确12脚本。Unity33/33(job906f2e19a88c4e1d82bbe615868b17e6)、CS0、Parity81/81+raw19/19、新鲜native与Unity实际raw完整content header及独立Python向量一致。对象O保留，融合F/S入组合C和V3；旧V2/tag-only不能current，legacy保持；candidate同bytes高优先级失效并沿既有cache IOException路径重载。source诊断非正式EXE录像；native双root不同值仅静态核对，本次实跑canonical同root。neutral3tick比较仍47equal/3MISSING（platformSourceSlot/environmentState/environmentSourceSlot），不伪报全对齐。未重跑无关SelfCheck/Play；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一回父FUSION-CATALOG-TRANSACTION-WITNESS：先精确声明prepared World fusion catalog接线，再carrier/schema及完整融合事务；禁止只接getter假闭环。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/Play，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：新NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001 IN_PROGRESS，准确11脚本预声明，尚未改代码。三端审计发现Unityrawcapture两处仍传object-onlyraw，Parity7合成header点也需同步；新合同保留O，组合C=SHA256(ASCII专用tag+NUL+O/F/S三32byte)，semanticV3及LE64，header显式3分量/新scope；旧V2历史保留但current拒绝。独立Python正式向量C3A7FF5…C37CC4/MFD18D6…008147/projection0FEFD4B968D618FD已存，不是当前runtimeheader。Unity单root明确canonical双root同值限制，nativecapture须用其真实独立两实参；不从ImageRoot猜根。下一按Task实施模型/candidate/Unitytrace/native/Parity和新focused tests，再三端新鲜交叉验证；不得仅改tag。前parser24/freeze14/source4-1737保持；Worldcatalog/carriers/完整融合仍未实现，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-INPUT-FREEZE-001 已限定VERIFIED，准确2新脚本、Unity14/14(job5d8aa21ce3dd47b89fac788cfd9155f6)/CS0/reviewPASS。5原生路径优先级、firstexistinginvalid失败、missing-onlylocked2、byte冻结/高优先级出现失效及fusion专用双hash已证；未接Host/World/全局LoganContentIdentity，不声称端到端融合完成。Parser24/24及source4/1737保持。下一必须精确全局contentidentity/preparedcatalog接线合同：现LoganObjectCatalog只hash角色DAT，需将fusion身份显式纳入且同步source/tool校验后才能activate；明确extractedroot不同于统一runtime根，不能猜。随后独立carrier/schema(190/AIalias/drop/globalfeaturepair，318复用RenderPicOffset)及source4UnityRED完整事务。未改资源/Scene/非战斗，Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent。

> 活跃 NTSD28-Q06-FUSION-INPUT-FREEZE-001 IN_PROGRESS，准确2新脚本，冻结fusion输入及专用fingerprint，全局contentidentity/host接线仍待；禁止错把统一runtime根当extracted根。

> 当前Q06：NTSD28-Q06-FUSION-CATALOG-PARSER-001 已限定VERIFIED / IMMUTABLE_TEXT_PARSER_ONLY，准确3新脚本、Unity24/24(job b1873ce0593347a99062b15d6537fc99)/CS0/独立review通过，未接生产，不重复SelfCheck/Play。父NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001仍IN_PROGRESS，source4/1737证据保持，Unity完整事务未实现。carrier审计确认318复用RenderPicOffset；190独立持久值不可复用Unk338；AIalias/drop在lockedkind换definition时保留，不能始终derive当前wrapper；两fusionfeaturegate是全局且与entityFeatureGate分离。下一建立准确preparedfusioncatalog/contentidentity和carrier/schema任务（详UNITY-CARRIER-ENTRY-AUDIT），声明路径后才改，随后source4UnityRED及完整事务。C12/C25h调度保持；Q06未完/Q07未迁移/总目标ACTIVE；无运行测试/build/agent，禁computer-use/非战斗/Scene/资源修改。

> 活跃 NTSD28-Q06-FUSION-CATALOG-PARSER-001 IN_PROGRESS，准确3新脚本，纯融合目录解析，不接战斗生产；source4父见证及完整融合事务仍待。

> 当前Q06 NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001 IN_PROGRESS / SOURCE4_PASS_UNITY_DATA_CONTRACT_PENDING。正式fusion.dat两record实际load，source4双跑SHAff7083f5…a4d0b4bf/58471bytes，独立可见状态1737checksPASS：record1/2 merge-defuse（record2真实implicit310），HP177拒绝、缺原partner定义分离失败公开state保持。timer0为明确测试干预，不称4500/200自然到期；private suspended无公开读取不称全字节证明。完整following已capture尚未Unity对照。下一先精确Unity carrier/catalog合同：核对display190/revive318/AI-drop/第二featuregate及snapshot/hash，不凭grep加字段；利用现有preparedRuntimeDataCatalog边界，另建精确Task后才实施。Unity生产/资源尚未改，不能只改getter或保留旧partner.Reset假装融合对齐；C12/C25h调度保持已验。前firstBDY两包限定VERIFIED，Q06未完/Q07未迁移/总目标ACTIVE；无运行build/test/agent，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06 NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。只读审计发现正式fusion.dat两record（7/8→51与10/11→52），Unity硬编码第一条且gate/历史/定义发布/分离Reset/失败前置不同，不能getter-only修复。准确Task/Change只声明新诊断CPP，source4基线计划两record merge/defuse+HP边界拒绝+缺partner定义拆分失败；生产/资源未改。保持已验C12/C25h调度；完整差异见AUTHORITY-GAP-AUDIT。前firstBDY两个包已限定VERIFIED。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前BATCH-03/Q06：NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 与 NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001 已限定VERIFIED。source8/222、主8smoke4 before/after/following零差异、joint19/19、SelfCheck04:53:09Z、sameWorld replay4场景8tick（含负action目标释放）、Play8与Q05close04:57:08Z均PASS。Scenechecksum/borrowers2→2、restore4→4/final World slots pools0/Stopped2frames，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。旧B5仅encoded counter错载体精确纠正，原决策/RNG等保持。next唯一恢复入口remaining-live-readers审计：优先只读核对Oid5152FusionScanAll→BattleOid5152RuntimeModule→TryApplyRuntimeIdentity fixed290/112及retained split action/857 gate的完整source事务与已闭职责，再建新Task；尚未改identity生产或建新sourceTask。lateEffects/display/postdisplay/CPointselector及raw3/platform/previousXYZ待办保留。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；无运行job/build/agent/Play，Editor33236复用，禁computer-use/非战斗/Scene/资源修改。

> 新依赖 NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001 IN_PROGRESS，encoded真实计数器RED已确认，准确3文件先登记；父firstBDY仍IN_PROGRESS。

> 当前Q06 NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE8_PASS_UNITY_PENDING。source8已构建/双跑一致SHA98ca9c2…10a97f8，focused222 PASS；初次私有入口编译失败保留后改公开ordinary入口。formal330全部DAT及catalog.csv哈希匹配：55simple+157encoded/目标全显式max705，encoded攻击者112/800/999skip；不声称正式玩家Bug。下一先准确声明Unity单fixture，以真实CharacterInteraction/shared consumer复建before并比即时/following，不能直接DamageWriter绕过firstBDY。生产两旧binder尚未改；已有B5规则/RNG/hold/manualdamage不重做。前lockedkind/snapshot已限定VERIFIED，Q06未完/Q07未迁移/目标ACTIVE；无运行build/test/agent，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06 NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确Task/Change已建，第一写域仅新source CPP；8分支代表、真实candidate→public hit→following，Unity shared consumer要求已写。尚未改生产/Unity fixture。前lockedkind与mutable snapshot两包已限定VERIFIED，不重做。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 与 NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 已限定VERIFIED。最终entity-only类型校验保留独立raw原值；joint42/42（b834ddc108714f14ae50ab78d3b76f48）、SelfCheck04:31:47Z、renderer replay2及Q05close04:32:46Z全部PASS，Scene checksum/hash/borrowers2→2、restore4→4/final World slots pools0/Stopped2frames保持。错误raw等同假设及中断失败完整保留，两ACCEPTANCE有准确范围。跨type仍拒绝/weapon派生覆盖与跨Worldepoch未提升。下一回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001：FIRST-BDY-NEXT-ACCESS-AUDIT.md已只读定位BattleFirstBodyResponseWriter两个旧binder，先审formal catalog可达域并建精确Task/source约8代表，必须shared candidate入口而非直接DamageWriter；已有B5决策/RNG/hold/manualdamage/早退职责不重做。尚未创建新sourceTask/修改该生产。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE；现有Editor33236已恢复，禁computer-use/非战斗/Scene/资源修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 IN_PROGRESS / CORRECTION_WRITTEN_VALIDATION_PENDING。首次绑定修复13/13 PASS仍为旧版本证据；后新增raw==entity type守卫错误，联合job84364ef670d3421baa4ca9cc3df6c5af最后观测36完成/4FAIL，无最终XML。已按RuntimeSlotTable及Q05独立raw合同纠正为仅entity类型检查，raw保真控制替代错误拒绝断言；当前纠正尚未编译/运行，禁止引用旧13/13称最终PASS。证据joint-interrupted/OBSERVATION.md，独立review已明确撤回raw等同假设。旧Editor62860已退出/6403拒绝连接/Temp结果消失；当前三个Open Project窗口、unity status无实例，项目lock实际共享冲突被占用，禁止第二实例或删lock。下一恢复现有连接后跑focused8+父replay及相关raw/native测试，稳定后一次SelfCheck/代表rendererPlay关闭。父lockedkind仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 IN_PROGRESS。精确生产恢复顺序已写；新type0/type3成功例初RED2及晚slot拒绝PASS已存focused-red。修后相关snapshot类+原lockedkind变身前回放13/13 PASS（cfffb63e14c74ea0b4f5eb2d92715aa5），CS0，证据after-binding-fix/results.xml。原EntityIdentityMismatch已不再当前失败，但父NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001仍IN_PROGRESS，依赖待反例硬化与retained-renderer验收。下一先审snapshot private raw/entity runtime类型一致性，声明精确额外路径再补无修改preflight校验与坏catalog/缺帧反例；禁止Load掩盖坏payload。独立review认可Load顺序，反对ModuleBind/初始化。尚未重跑SelfCheck/Play，待生产稳定一次联合；跨type仍拒绝/weapon子类待证据。Q06未完/Q07未迁移/总目标ACTIVE；保护Scene/HUDBg30和非战斗，禁computer-use。以下旧检查点仅历史。

> 当前 BATCH-03/Q06：NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 仍 IN_PROGRESS。source2/67、focused9/9、captured2/2、SelfCheck03:32:42Z、Play4及Q05关闭03:38:12Z已有PASS，证据归档 play-pass-replay-pending/ACCEPTANCE-PENDING.md；原sameWorld变身前snapshot回放FAIL EntityIdentityMismatch，不能称完成。新依赖 NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 PLANNED：expected200/current213被preflight拒绝，且当前帧恢复仍读现有DAT，禁止仅删除检查或清local shell绕过。下一先审FrameCache.Load/控制器副作用并做精确测试，再实现保留同shell/Renderer的原DAT恢复；生产尚未改。稳定身份/类型/失败无修改必须保持。按分支代表验证，复用未受影响证据。Q06未完/Q07未迁移/总目标ACTIVE；禁止computer-use、非战斗、Scene和资源修改。以下较早检查点只保留历史，以本条为准。

> 当前Q06 NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确Task/Change已建，仅新CPP由cpoint_acceptance编写，尚未build/Unity/production修改。正式kind.dat加载优先decoded路径已确认；同表实际SHA39e30d…00011。indexed绑定8当前type0有40、213type3缺40且7条kind0攻击、209未在indexed行；仅静态不称动态Bug。计划真实catalog213→200 explicit40/implicit40两例，targetcurrent10state3000，完整identity/history/pendingCount/after/following，不能用只变ID或改targetstate0遮蔽snapshot差异。前generic binder已VERIFIED不重做，generic高位projection保持独立后继。Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，仅新CPP预声明；正式kind.dat优先路径确认，213缺40且有kind0 ITR，动态transform待见证。生产未改。

> 当前BATCH-03/Q06：TYPE3-TARGET-GENERIC-NATIVE-FRAME-BINDING-001已限定VERIFIED。source11双跑SHA c4d7afdc…d5ebee/独立419；Unity11+smoke5零差异、联合13/13、captured两mode2/2、SelfCheck03:11:12Z、representative sameWorld5场景10重放tick、Play10及Q05关闭03:14:12Z全PASS。Scenechecksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。只修单generic binder，真实pair中间900最终70/71及贡献count1→2正确；highpair投影旧gate未覆盖不称PASS。首次新方法pre-reload空test0已保留不计PASS。下一优先只读审计ApplyNativeLockedKindTransform HasFrame40/oldrawbinder及lockedprojection门，源无显式40要求；报告LOCKED-KIND-NEXT-ACCESS-AUDIT.md含真实catalog加载链、formal decoded kind.dat已观察与fallback同表/SHA39e30d…00011。尚未建下一Task/source或改production；需精确Task和显式/隐式40 wholehit见证，不重做已闭genericowner/attacker职责。generic高位projection后继独立，其他identity/late/display/platform/raw3保留。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> NTSD28-Q06-TYPE3-TARGET-GENERIC-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，仅新CPP声明11代表含真实后置pairreset；actual binder与projection门分开，生产未改。

> 当前BATCH-03/Q06：TYPE3-ATTACKER-POSTHIT-NATIVE-FRAME-ACCESS-001已限定VERIFIED。source13双跑SHAfb3f06bf…bf0e31/独立214、Unity主13+smoke6零差异、旧11+新2联合13/13、Object captured两mode source字段通过、SelfCheck02:49:00Z、代表sameWorld6场景12重放tick、真实Play12与Q05关闭02:51:18Z全PASS。Scenechecksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。captured初用错Character pass已纠正；type3target3005非匹配pair被既有CanProjectStandardType3DamageWriterEffect明确排除，只证明actual，不称writerprojectionPASS。原失败与准确scope见ACCEPTANCE。下一回remaining-live-readers，先只读审计BattleDamageWriter.ApplyNativeType3TargetGenericContinuation的DirectWriteHeldFramePreserveWaitCounter单caller及target response frame/owned关系完整source事务；与locked kind transform/identity及旧投影HasFrame门区分，避免重做已验owner/清pending职责。尚未建下一sourceTask/改该生产caller。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> NTSD28-Q06-TYPE3-ATTACKER-POSTHIT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确新CPP写域，13代表/三live消费者，生产未改。前Kind0/reaction已VERIFIED不重做。

> 当前BATCH-03/Q06：KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001及STANDARD-REACTION-HISTORY-NATIVE-READERS-001均限定VERIFIED。source6+reaction2双跑/默认6bytes不变；联合25/25、SelfCheck02:24:27Z、代表sameWorld4场景8重放tick、真实Play10与Q05关闭02:27:23Z PASS，Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。reaction最终仅实际+投影两个previous reader；snapshot原fallback控制已过不改。父case1为early拒绝，不能称post-effect state18抑制覆盖。准确限制见两ACCEPTANCE。下一回remaining-live-readers审计，优先Type3 attacker post-hit共享selected reader/binder；POST-EFFECT-REMAINING-ACCESS-AUDIT已有定位，先确认standard type0/armored type0/特殊对象当前live消费者及原B5证据，已native unarmored/reduced helper不要重做。尚未建新sourceTask/修改type3生产。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE；无运行job/build/agent/Play，复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS，两访问点已修但依赖未关。source6双跑SHAc01031a3…c2bcae4/独立124PASS；其中5真实hit+1early拒绝（effect20 prev18前置拒绝，不能声称后置18抑制覆盖）。Unity原RED主6即时10/后继12；两访问修后job440cbbd2bef64c5e9f7f49ce15b76b7e旧15PASS/新两矩阵FAIL，仅case0普通reaction剩fall20vs80/action220vs186/pendingX0vs5/Y17vs10，before均0；其余5例零差异。下一唯一Task NTSD28-Q06-STANDARD-REACTION-HISTORY-NATIVE-READERS-001 PLANNED：source CPP增加--reaction的snapshot900state12/state0两例（尚未写），默认6字节保持，之后准确声明普通ApplyStandardFall与投影4reader。父证据production-red/after-post-effect-access保存；完整SelfCheck/Play待依赖修好联合一次。当前无运行job/build/agent/Play；Q06未完/Q07未迁移/总目标ACTIVE，前3包VERIFIED保持不重做。禁computer-use/非战斗/Scene/资源修改，保留HUDBg30。

> NTSD28-Q06-KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。只声明新CPP，生产未改；6代表，保护既有B5规则，前3包已VERIFIED不重做。

> 当前BATCH-03/Q06：EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS、STANDARD-HIT-FALL80-PRESERVATION、STANDARD-HIT-PENDING-Y-PROJECTION三个001包已限定VERIFIED。联合30/30、完整SelfCheck01:55:20Z、代表sameWorld6场景12重放tick、真实Play22与Q05关闭02:00:04Z均PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。初次Play测试日志context失败已保留并只修测试入口，未再跑全套。准确scope/限制见各ACCEPTANCE。下一唯一入口回remaining-live-readers审计，先Kind0 post-effect旧previous reader+200/203 binder（报告POST-EFFECT-REMAINING-ACCESS-AUDIT.md已有6代表建议），需新Task/Change及source witness后才改生产；随后type3共享selected reader/binder，但已native的unarmored/reduced helper不重做。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE。无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：effect native frame access、standard hit Fall80保持、pendingY projection三关联包生产修复已联合30/30 PASS（job d41e706b23d048448436ee57d9f2f0c9，1.777秒），完整SelfCheck 2026-09-21T01:55:20Z PASS；证据各包joint-pass。source3/default dvy0与vertical2非零/截断及effect16+smoke6已验证，旧失败保留。新增仅测试的6代表sameWorld replay与一次Play22合并入口已写，Editor正在刷新编译，尚未运行新增replay/Play；下一先读Editor readiness/CS0，再只跑新增replay与改动的captured tests，之后EffectPlay请求和Q05关闭，不重复完整SelfCheck/旧矩阵。三个Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；Scene/HUDBg30和非战斗保持，禁computer-use。

> NTSD28-Q06-STANDARD-HIT-PENDING-Y-PROJECTION-001 IN_PROGRESS；source3原输出保持，--vertical补2边界，尚未改投影条件。Fall80两clear/2oracle已写待新编译验证，effect父等待联合出口。

> NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：仅新CPP3代表预声明；effect四访问修后before0/即时1/后继1只余致死Fall80独立clear，生产clear未改，父effect等待本包。

> EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 source16已build/double-run一致SHA52093e69d3e286ddd5e496ab862efb7e35ec385b63b41472fa5b0dda51696f0d，focused独立365检查PASS；仅gate/override/descriptor/历史字段保持及固定injury5 HP，不模拟全hit sideeffects/RNG/following。初版target显式wait误填37（源41）两失败已保留并纠正。真实普通unarmored source正常HP395/致死-4，高位latch900state602/BDY50抑制及previous900state12匹配已触发。下一准确扩Record单Unity fixture，slot0/70完整before及whole标准hit后/下tick对照，先RED再决定normal两reader/两binder。实际标准hurt支持帧180/186/220已在同源DAT显式声明以隔离别的binder。HitPlan身份投影两reader另需真实转换代表，普通16不关闭它；Kind0PostEffect/Type3PostHit仍独立后继。生产未改；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用，禁computer-use/Scene/资源/非战斗修改，按分支代表验证。

> EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：正式effect8字段801条，catchingact/pickedact全0。审计纠正旧getter会拒绝显式857..999；normal latch/previous两reader及两binder需见证，identity projectedDAT两reader另需实际转换证据。source16新CPP由cpoint_acceptance编写中，未build/run/Unity/生产修改。原B5职责保持，Kind0PostEffect/Type3PostHit不混改，测试按代表等价类。前IMPACT已VERIFIED不重跑；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：只新CPP预声明16代表，identity投影另需证据，生产未改。旧getter对显式高帧亦拒绝，审计错误已纠正。

> Q06 IMPACT-NATIVE-FRAME-BINDING-001已限定VERIFIED：source20/18285、20+6零差异、联合53/53、SelfCheck01:06:28Z、代表replay12场景24tick、Play12与Q05关闭01:09:41Z全PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14。单Action caller修复，原B6职责保持。下一回remaining-live-reader审计，优先只读检查BattleDamageWriter.ApplyNativeEffectActionOverride两caller及其正式kind0/weapon/type3消费上下文；旁边Kind0PostEffectAction是独立计数器/朝向事务，须分清顺序再定最小source代表，不批改getter。其它identity/CPoint/lateEffects/display/post-display/platform/raw3保持。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按用户分支等价类验证不重复全套。

> IMPACT单caller修后fullSelfCheck 2026-09-21T01:06:28.907250+00:00 PASS已归档after-binding-fix；此前运行中状态由本条覆盖，当前无运行job/build/agent/Play。联合53/53与20+6零差异保持，独立生产review单行通过。下一仅6代表sameWorld replay/Play及Q05关闭，不重复已过矩阵或SelfCheck。Task IN_PROGRESS/Q06未完/Q07未迁移/总目标ACTIVE。

> IMPACT-NATIVE-FRAME-BINDING-001生产单caller已修：source20/独立18285，Unity有效RED主20 before0/即时15/后继5、smoke6 before0/即时3/后继1；Action操作改native binder后20+6均before/即时/following0。联合job5b92ec860e18446cb36b010bef8608cb 53/53PASS(新2+literal3+相关HitPlan48)，约1.42秒，未跑旧B6全量；证据after-binding-fix，原JSON数字tag夹具失败及有效RED均保留。fullSelfCheck请求已提交，现Editor62860执行，结果待新Temp/NTSD_BattleRuntimeSelfCheck.result；不得重复启动/并行Unitytests或C#编辑。下一仅6代表sameWorld replay与Play(Authority/DataOriented/renderer6+Mobile/Legacy/logic6)及Q05关闭，脚本前准确登记Record；无需再跑矩阵/旧测试除非新失败。Task未关闭，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> IMPACT-NATIVE-FRAME-BINDING-001 source20已build并双跑一致SHA4041ba46914f43292e97058fb9c8687f67482cf384dbcb3590e1d0a0946d6edd，独立18285检查PASS（captured-before→四实体即时after、准入/descriptor/pending/RNG，不独立重建spawn或following）。Build manifest闭包07CD47…778F，源CPP FFBAFC…60D8A。下一准确扩Record新增单Unity fixture，复用正式catalog type与四实体factory，全部spawn后恢复0→1→2 ownerchain及target70 before；额外capture environment/catchSource/impactSource/pendingXYZ/descriptor，sourcepreviousXYZ仍明确source-only；主20代表+必要smoke，不扩乘积。先before0/即时/following分开RED再决定唯一BattleDamageWriter Action caller，生产未改。静态formal respond全0且唯一implicit182 OID899无bdy/itr，不认定实战Bug。前state1218已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用；禁computer-use/非战斗/Scene/资源修改。

> Q06 IMPACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。独立审计确认唯一旧binder位于BattleDamageWriter.TryApplyNativeImpact的Action操作；HitPlan.ProjectNativeImpactWriterEffect不读descriptor，旁边kind15不是本包。原B6 I1/I2/I3已验行为保持。formal330 kind10=121/kind11=61/respond全0；type0 target182显式157/隐式1(OID899 spe)，但该对象22frame无bdy/itr，不能认定实战可达差异。20代表source runner由cpoint_acceptance编写，只有新Tools CPP写域，尚未build/run/Unitytest/生产修改。报告remaining审计FORMAL330-IMPACT两JSON及新包UNITY-MAPPING-AUDIT.md。测试按用户等价类原则，不做profile/角色全乘积。前state1218两包VERIFIED保持，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-IMPACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：仅新CPP预声明20代表，生产未改；正式respond全0且type0潜在隐式182仅OID899，非动态可达证明。

> Q06 STATE1218-CONTACT/AIRBORNE-NATIVE-FRAME-BINDING两包已限定VERIFIED。source480/55双跑与独立模型、联合29/29、SelfCheck00:45:27Z、代表sameWorld40场景80tick、真实Play40与Q05关闭00:48:58Z全PASS。Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped；dirtyfalse/root14/hashBCD1047B…0E9FB6保持。按用户要求不重复四配置矩阵及SelfCheck；代表限制见ACCEPTANCE。下一唯一入口回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001，先只读核对BattleDamageWriter.TryApplyNativeImpact的Action操作与正式resolve_special_relation_hit对应完整事务及已有B6证据，再定最小source witness；禁止重做已闭行为或共享getter批改。其它impact/effect/identity/CPoint/lateEffects/display/post-display及platform/raw3/previousXYZ保留。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，Editor62860可复用，禁computer-use/非战斗/Scene/资源修改。

> state12/18 contact+airborne联合29/29及最新完整SelfCheck 2026-09-21T00:45:27.002427+00:00 PASS，证据两包joint-pass。当前无运行job/build/agent/Play；下一仅代表sameWorld replay/Play与关闭，不重跑全矩阵或无新生产修改的SelfCheck。两个Task仍IN_PROGRESS；Q06未完/Q07未迁移/总目标ACTIVE，测试等价类收敛规则保持。

> 按用户分支等价类验证：airborne源55双跑一致SHAa4d7c8fd…52e691/独立24696检查PASS；Unity RED55即时30与smoke8即时7，before/following0，后仅改LF2Entity.ApplyCurrentDatType0AirborneAction单binder。联合job e6d47ce73ce2473180ce73ea5b4eca50已29/29PASS（airborne17旧+contact9旧+新55/8两测试+contact主路径480一个测试），实际8.03秒。新55/8与contact480 before/即时/following均0，证据两包joint-pass。不要重复另外三contact矩阵；已过分支复用证据。完整SelfCheck请求已消费，PID62860当前运行，等新结果勿并行C#改动/Unitytests。下一用代表案例补sameWorld replay与一次Play：contact选覆盖soft/pending/invalid/explicit999/hardmotion的约12例，两条配置路径各验证必要代表；airborne8代表；不要跑旧3840全乘积Probe。代表filter/计数变更前扩Record。两包仍IN_PROGRESS，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 用户要求收敛重复验证（2026-09-21）：按相关分支/数据等价类选代表案例，不能只按角色名扩矩阵；局部修改先编译+原差异最小案例+受影响边界。未变化已通过证据复用，只有新修改/失败/未决风险才扩测。完整SelfCheck与相关联合回归在一个闭合执行包出口集中一次；Play按实际受影响路径/生命周期选代表，不每改一行都重跑所有配置/角色。Q07内容迁移与Q12完整集成出口不缩减，原失败不得删除/排除来变绿。

> NTSD28-Q06-STATE1218-AIRBORNE-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，只新CPP预声明；contact两binder修后job45daa31087ba4319ac0fa19b0f9e36fb待终态，不混改生产。

> STATE1218-CONTACT-NATIVE-FRAME-BINDING-001已测RED：job87f3c88664724d3b80bf2501db16de3b终态FAILED4/4，两profile×两路径各480 before0/immediate410/following120；即时392 contact+18 airborne-control，后继120全部contact。production-red四JSON/XML已存；生产未改。下一先精确扩Record仅LF2Entity.ApplyCurrentDatType0State1218ContactAction两个raw binder，修后保持完整480控制不删；airborne18另建独立Task/source见证，不在contact包顺手改第三caller。源480/228481模型PASS，但stepflags/following未独立模型（review泛称flags由模型承担不适用当前脚本，按实际scope）。静态正式330默认目标全声明及19461 ITR无自定义pending动作保持，不把synthetic RED称正式玩家Bug；动态身份域仍待。前ordinary/candidate已VERIFIED不重做，Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play，复用Editor62860；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 STATE1218-CONTACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS。源480 build/double-run一致SHAd898b84d…ffc6c1，独立初态/即时228481检查PASS；原模型implicit999失败8处已保留纠正。正式330默认contact目标hard2512/soft1853全部显式，19461 ITR无非0pickedact/pickingact，仅静态域不证明动态身份切换；不称已确认玩家可见Bug。新Unity单fixture pending7字段已恢复，CS0，job87f3c88664724d3b80bf2501db16de3b运行四直接矩阵，必须读终态；生产尚未修改，airborne另属后继。源与fixture准确Record已建。前ordinary/candidate两个包VERIFIED不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-STATE1218-CONTACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；只声明新CPP，生产未改。airborne独立后继，保护已闭物理行为。

> 普通落地NATIVE-FRAME-BINDING及CANDIDATE-COLLISION-REFERENCE-RESET均已限定VERIFIED。source186四矩阵0差异、联合15+旧6、sameWorld744场景1488tick、完整SelfCheck、真实Play1488、退出重进Q05关闭00:17:02Z/00:18:05Z全部PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001：优先只读确认state12/18 contact/airborne剩余三个raw caller及完整authority事务，必要时新建源见证Task，禁止重做已闭物理规则或批量改getter。platform op30/previousXYZ/raw3明确保留，identity/impact/lateEffects/display/post-display继续待。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，Editor62860可复用；禁computer-use/非战斗/Scene/资源修改。

> 最新验收：candidate reference reset与普通落地native binder联合15/15PASS，source186×四组before/即时/following均0；旧B4普通落地已以正确Editor namespace补跑6/6PASS（job4191c162de2543eb83166574b9983c84）。完整SelfCheck 2026-09-21T00:11:47.663892Z PASS文件与时间证据已存candidate包。CS0、Ledger597/11PASS、diffcheck无错误，Scene hashBCD1047B…0E9FB6保持。下一唯一工作：同一普通落地186夹具补同World回放，再真实Play两profile/两路径/两factory及Q05关闭重入；脚本扩展前登记Record准确符号。两个包仍IN_PROGRESS（尚未完成replay/Play），不是平台op30域对齐。当前无运行test/SelfCheck/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，Q06未完/Q07未迁移/总目标ACTIVE。

> NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001 IN_PROGRESS：Q06候选高度参考reset独立包已生产写入；联合job0bbead38ad244ed1adc8ac109ebdabd3 SUCCEEDED15/15（新5/载体6/源矩阵4），普通落地186×两profile×两路径before/即时/following全部0。实际未运行旧B4 fixture（初始filter漏Editor namespace），后继须NTSD.Test.Editor.NTSD28B4Type0OrdinaryLandingEditorTests补跑。完整SelfCheck请求已消费，现有Editor PID62860执行中，实际日志Logs/kind8-real-play-editor.log已有RunAllChecksStatic/CheckActivatedRuntimeProfileContracts调用；结果待新Temp/NTSD_BattleRuntimeSelfCheck.result，禁止重复启动或测试并行。之后同World回放/真实Play及关闭仍待。平台op30/previousXYZ/其他两个reference字段仍未闭合，不称整域完成。Q06 active/Q07未迁移/禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001 IN_PROGRESS / TEST_FIRST；准确query入口及新focused测试预声明，生产未改。父落地完整tick依赖本项，Q06仍active。

> 当前BATCH-03/Q06：普通落地单caller native绑定已写。job01658e8a1a8241caab89c3c3fb25234e终态FAILED4/4，四组各186 before0/immediate0/following78，仅combat.collisionYReference=-10 expected0；证据after-binding-fix。源battle_world.cpp4026在pair pass前清零collision_y_reference/platform_source_slot_f4/render_shadow_offset_10c；Unity仅carrier reset已有，需审完整生产pass及平台依赖，不能在fixture清零掩盖。下一先只读确认并独立Task/Change声明必要生产修复，再完整回归/回放/SelfCheck/Play。CS查询0、Ledger596/9PASS、diffcheck无错误、Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。落地Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use与非战斗/Scene/资源修改。

> NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；源单CPP写域，Unity生产未改。

> 当前BATCH-03/Q06：KIND8-NATIVE-RAW-BINDING-001已限定VERIFIED。source134/62045、两profile0差异、53回归、268场景536replay、SelfCheck成功日志、真实Play536与关闭23:33:57Z全PASS；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001 PLANNED / SOURCE_WITNESS_FIRST，只有新CPP预声明，尚未写脚本。ordinary landing规则及canonical tail已闭不重做，只核对raw descriptor binding；formal330静态7隐式hit_g不是动态可达证明。目标Editor62860当前idle/notPlaying，MCP6403/status-b1b02287可复用，禁止第二个同项目实例。无运行test/build/agent。CPoint370保留source-only、selectors正式全零排后，identity/impact/lateEffects/display/post-display仍待；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS / FOCUSED_REPLAY_SELF_CHECK_PASS_PLAY_PENDING。kind8单caller原生binder修复后两profile134 before/立即/following0；联合53/53PASS含268场景536重放tick。旧projection反射2FAIL已独立KIND8-PROJECTION-TEST-ENTRY-001限定VERIFIED，原失败保留。完整SelfCheck本轮batch PID122096已终止，日志成功marker与SHA已存full-selfcheck-completion-proof.json；退出后Temp结果缺失，不声称文件归档。下一在同新Editor fixture添加Play536入口，真实场景两profile两factory保护及Q05关闭；目前入口未写，勿重复旧source/回归。场景hashBCD1047B…0E9FB6保持，无目标Unity/job/build运行。CPoint370仅source capture/局部oracle且正式selectors全零，排后未取消目标；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/资源/Scene改动。

> NTSD28-Q06-KIND8-PROJECTION-TEST-ENTRY-001 IN_PROGRESS / TEST_ONLY，修两旧projection用例反射缺两个可选参数；source134两profile已0差异，父验收未完成。

> 2026-09-21当前Q06：KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS。源134双跑一致SHAc0543119…b26009，62045独立即时transition检查0失败，132applied/2拒绝；新单Editor fixture已写、生产未改。目标项目原未打开，root启动Unity2022.3.62f3独立EditMode PID51800（Temp/NTSD28_Kind8Batch.pid），测试filter NTSD.Test.NTSD28Q06Kind8NativeRawBindingEditorTests，结果Logs/kind8-134-editmode.xml/日志kind8-134-editor.log；当前等待该进程终态，不重复启动同项目实例。CPoint选择370源双跑/2590部分oracle已存，formal330八selector均0，因此source-only保留，排在kind8正式内容30隐式目标之后，非已完成。held已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁止computer-use与非战斗/Scene/资源修改。

> NTSD28-Q06-KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；有正式DAT隐式目标正例，生产优先于CPoint全零selector。后者只源诊断构建中session28190，不改Unity；准确范围见各Record。

> NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，只有新CPP写域，Unity生产未改。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001已VERIFIED（初次140/释放1150/补给242及formal330静态内容声明范围），最终SelfCheck/Play968/关闭21:09:16Z证据保持。SyncHeldPose仅定义；同2033边selectedchild10/12/18均0，动态identity保留关系必须后继回访。当前source无child12/18unsupported，旧游标表述已纠正。下一唯一执行Task NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 READY_SOURCE_WITNESS，Record PLANNED只声明新CPP，尚未改脚本：完整8路输入选择/最终0取消/原生selected frame与vaction，保护既有throw和kind2。raw-binding矩阵已交付，canonical kind8 dvx999哨兵等排后；identity/rawsetter/lateEffects/display/post-display继续保留。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_RELEASE_REFILL_SCOPES_VERIFIED_REMAINING_CALLERS_PENDING。refill242两profile正常0差异、generic202两profile0、484场景968重放tick、旧100回归与联合11/11、完整SelfCheck21:07:43Z、真实Play968全部PASS；关闭21:09:16Z restore4→4/worldslots两pool0/两帧Stopped。Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。两个refill oracle已限定VERIFIED，旧失败保留。下一回remaining-live-reader审计：SyncHeldPose全仓仅定义无caller，不修改；held damaged12/10与源unsupported12/18及formal330域需最小只读核对后决定是否本Task仍有真实残余。不要重做已验140/1150/242，不批量改共享getter。identity/rawsetter/lateEffects/display/post-display后继保持；Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play；禁computer-use、非战斗/Scene/资源修改。

> refill两独立oracle已VERIFIED / TEST_ONLY：REFILL-NATIVE-RNG-EDITOR-ORACLE-001与REFILL-REAL-OID-SELF-CHECK-ORACLE-001；100/100及新完整SelfCheck21:07:43Z PASS。父refill Play run-refill968正在执行，尚待结果及关闭。

> Q06 refill生产修复已两profile242 before/立即/following全0；旧100回归经独立RNG oracle修订后100PASS。SelfCheck原21:00:51Z伪OID992失败保留，独立REFILL-REAL-OID-SELF-CHECK-ORACLE-001只改该const为122，尚待新fullSelfCheck。当前job a2a98665947e4e2babea21e39e0aa359：generic202两profile、refill242 replay两profile、补给Editor最终标记断言回归，必须查询终态。refill Play run-refill968入口已写未运行；原release/initial已验不重做。父IN_PROGRESS，两个oracle未关闭，Q06未完/Q07未迁移/总目标ACTIVE；禁止computer-use、非战斗/Scene/资源修改。

> NTSD28-Q06-REFILL-REAL-OID-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY，准确测试范围见Record；不改production。

> NTSD28-Q06-REFILL-NATIVE-RNG-EDITOR-ORACLE-001 IN_PROGRESS / TEST_ONLY，准确测试范围见Record；不改production。

> 当前Q06 refill已获得确定RED：source242双跑一致SHA83af13d…42d5dc0、229293独立检查PASS，原140/1150字节保持；Unity job fd1f7330323c4a5e9e1f3893c13c372d两profile242均before0/immediate660/following992，终态FAILED，证据refill-production-red。差异含耗尽旧legacy RNG、隐式0 wait、type0通用实体未补给；fixture已模拟正式type_sub默认，初态完全一致。下一先冻结最小共享补给事务生产符号/Record，再修已测分支，不动框架/Scene/资源。cpoint_acceptance只读审最小改动设计；无Unity job/build/Play运行。父HELD-NATIVE-FRAME-BINDING IN_PROGRESS，初次140及release1150限定验收保持不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_AND_RELEASE_SCOPES_VERIFIED_REFILL_PENDING。release源1150/独立296552 PASS，正常及generic664两profile零差异；旧回归91/query5、同World replay2300场景4600tick、完整SelfCheck20:39:17Z、真实Play4600全部PASS；关闭20:45:41Z restore4→4、world/slots/logic/render0、两帧Stopped，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。独立HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001已VERIFIED（测试字段范围）。下一同held Task扩refill源向量，精确核对耗尽action0/nativeRNG/Zz及源system_rules对象id门；不要重做初次140或release1150。remaining reader/identity/rawsetter/lateEffects/display/post-display仍待。Q06未完/Q07未迁移/总目标ACTIVE，无运行job/build/agent/Play；禁止computer-use、非战斗/Scene/资源修改。

> 当前Q06：HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001 VERIFIED / TEST_ONLY；独立字段断言已修，完整SelfCheck2026-09-14T20:39:17Z PASS，原失败保留。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS；已扩单Editor fixture的release1150两profile同World回放及run-release Play4600入口，等待编译和实际运行。普通1150/generic664零差异保持，下一先release replay再Play/关闭。Q06未完/Q07未迁移/总目标ACTIVE；无agent运行，禁止computer-use及非战斗/Scene/资源修改。

> NTSD28-Q06-HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY：准确限定两个SelfCheck方法，修订2F8与Spawner独立字段断言；生产不改，验收待新完整SelfCheck。

> Q06最新验证：release1150正常路径及generic武器DAT664子集两profile均0差异，query5/5与旧回归91/91 PASS；但新完整SelfCheck 2026-09-14 20:35:34 UTC FAIL R5-HOLD-002（旧断言要求释放写Spawner，源写独立+2F8）。失败已归档，下一先独立审计该oracle并建立准确Task/Change后修改测试，再SelfCheck及release replay/Play。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS，Q07未迁移，总目标ACTIVE。无运行Unity test/build/Play；只读cpoint_acceptance正在审计oracle。禁止computer-use、Scene/资源/非战斗修改。

> 当前 BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS。初次绑定140限定验收保持；release源1150双跑及296552独立检查PASS，正常factory两profile1150 before/立即/following均0差异（job b573b82a834a415c9925f12441a11b0f，release-after-fix）。通用LF2OtherObject承载type1/2/4/6武器DAT的664行两profile补验已SUCCEEDED 2/2、before/立即/following全0（job f60db972d4724aacb78ecd0a5c51aa1d，release-generic-pass）；限定受控CLR适配子集。释放阶段最新生产修复后的replay/SelfCheck/Play仍待，不能复用初次140旧验收代替。refill及remaining live readers、display/post-display继续待处理。Q06未关闭，Q07正式资源未迁移，总目标ACTIVE；保留用户HUDBg30，禁止computer-use及非战斗/Scene/资源修改。以下旧检查点保留历史，以本条覆盖。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_BINDING_VERIFIED_RELEASE_READERS_PENDING。源140两profile立即/fulltick0、query5/5、旧回归91/91、280replay/560tick、SelfCheck20:01:02Z、真实Play560及关闭20:02:44Z全PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，hashBCD1047B…0E9FB6保持。非holder-query、旧fixture、slotreuse-oracle三个独立子包已VERIFIED声明范围。下一同held Task先保留source140，再扩kind1/3+DVX等后继原生源见证；type2非kind3-DVX旧legacy RNG与后续frame40/随机/补给0旧setter待对照，不重做初次绑定或批量改共享接口。formal330 cover2/state12/18静态域仍0，限定dormant报告已存。remaining reader总审计/identity/rawsetter/lateEffects等继续保留；Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play；禁止computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-HELD-SLOT-REUSE-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY：SelfCheck same-slot关系清理期望-1纠正为source/既有writer0，缓存及反向关系断言保持。

> NTSD28-Q06-HELD-LEGACY-FIXTURE-NATIVE-CONTRACT-001 IN_PROGRESS / TEST_ONLY，保留新140全部零差异；两旧fixture初始化和失效oracle独立修订，未改productionbinder。

> 新独立 NTSD28-Q06-HELD-QUERY-NONHOLDER-RELATION-PRESERVATION-001 IN_PROGRESS / TEST_FIRST。持有140已before0/立即0，fulltick90差异定位CPoint同步后角色GetHeldEntity把负/零关系清为0/-1；只拆非持有者cache查询分支，正关系失效处理保持，先精确测试。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS，Q06未完。

> 当前Q06 NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE140_PASS_UNITY_PENDING。源build/double-run140行1467643bytes SHA8ddb447…5b9610，105valid/21unsupported/14terminal，1344+420独立检查PASS；following126alive/全部lifecycleSuccess，完整following尚未Unity对照。初版reciprocal建立过早导致零分支，失败归档后已修。下一在同Record先新增准确单Editor fixture路径，以两profile/两factory复建完整before并比立即与following，再根据RED限定held两caller native读帧；不要直接改共享setter(type3命中也调用)。remaining reader审计187occurrences/初步matrix仍IN_PROGRESS，identity/rawsetter/lateEffects authority等后继保留，前已验三包不重做。无运行build/test/agent/Play；Q06未完/Q07未部署/总目标ACTIVE，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06剩余reader审计IN_PROGRESS：187词法occurrences已存inventory，初步live/compat/unknown矩阵已写。下一独立 NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；仅新诊断CPP预声明，正式held两条分支旧HasFrame857/descriptor阻断隐式与高帧后续定位，先source完整状态，不批量替换getter。identity/fusion、通用rawsetter、lateEffects authority及其它hit/AI/spawn caller仍待审计；旧throw/shared native-input绕过路径保留。前cost/physics/oracle三包已限定VERIFIED不重做。Q06整体未完成、Q07未部署、目标ACTIVE。

> 当前BATCH-03/Q06：NATIVE-INPUT-ACTION-COST-FRAME-READERS-001、CANONICAL-CHARACTER-PHYSICS-TAIL-001及NATIVE-DJA-UNAVAILABLE-SELF-CHECK-ORACLE-001均VERIFIED（各自声明范围）。源151四完整tick0差异、302replay/604tick、56实际physics、SelfCheck19:09:57Z、真实Play1208+56、最终关闭19:11:02Z全部PASS；Scene checksum/borrowers2→2、restore4→4、world/slots/pools0、两帧Stopped，dirtyfalse/root14/HUDBg30/hashBCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001 READY_READ_ONLY：按实际caller分类余下legacy getter/raw setter，禁止批量替换/重做已关闭包；之后回DISPLAY-PROGRESSION/POST-DISPLAY。Q06整体未完成、Q07正式内容未迁移、raw3缺口/跨Worldepoch/stage USER_HOLD/例外保持；总目标ACTIVE。无运行job/build/Play，禁止computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-NATIVE-DJA-UNAVAILABLE-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY：只修CheckComboLocalShadowCommitContracts失败夹具399→1000。

> 当前新增独立 NTSD28-Q06-CANONICAL-CHARACTER-PHYSICS-TAIL-001 IN_PROGRESS / RED_CONFIRMED。源边界统一后151×两profile Legacy0差异、DataOriented326差异；准确Task/Change已建，只修快速物理尾部接线并新增定向测试。父输入费用Task等待该出口；Q06/总目标仍未完成。

> 最新Q06费用fulltick隔离：job53acd6f9d30f483daebeda0f0cdc12c2四组151 before0，Legacy各44差异(仅负X钳制)，DataOriented各370(还含state12落地动作/counter/Vy)。CS0，失败归档following-path-isolation-red。下一统一横向世界边界后再验证physics差异并建立独立准确Task/Change；勿扩大BCAW费用生产范围或用Legacy代替快路径验收。当前测试已终止、无运行job，Q06未关闭/Q07未迁移/总目标ACTIVE。

> 当前 BATCH-03/Q06：NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 IN_PROGRESS。151输入费用两profile端点0差异、Combo18/18通过；新增完整相邻tick两profile失败并归档following-initial-red。下一先统一world stage边界(source Z0/Unity默认ZMin180)，再对照Legacy/DataOriented physics：快速路径缺少普通路径已有state12/18 contact调用，尚未运行分离验证，不判定完成。Q07未迁移，总目标ACTIVE；禁止computer-use/Scene/资源/非战斗修改。此前顶部READY/生产未改描述已过时，仅保留历史。

> 当前BATCH-03/Q06：CPOINT-THROW-NATIVE-RAW-BINDING-001与NATIVE-INPUT-MISSING-STATE-ROUTING-001均已VERIFIED（各自声明范围）。source392/432、两profile输入及投掷立即/fulltick、112场景224重放tick、SelfCheck18:13:58Z、真实Play18:15:16Z1568+864及关闭18:16:07Z全部PASS；Scene checksum保持/Renderer2→2、恢复4→4、World/slots/两pool0、两帧Stopped，Editor idle/notPlaying、dirtyfalse/root14、HUDBg30/hash BCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 READY_LIVE_SOURCE_MAPPING：BCAW ApplyNativeInputActionCore、rowing与builtin cost仍旧Has/Get；先追真实source apply_action/资源重定向与fallback完整事务并建准确Record，未改该项脚本。不要重做已关闭Cpoint/source/replay/Play。Q06/full alignment仍未完成，Q07正式资源未迁移，raw3缺口/跨Worldepoch/stage USER_HOLD/例外保持，禁止computer-use/非战斗/Scene/资源修改，总目标ACTIVE，无运行job/build/agent/Play。

> 当前BATCH-03/Q06：qualification真实driver补验8例16:54:03Z PASS、关闭16:55:03Z PASS；qualification/collision-frame/prelude-feedback三父包已按声明范围VERIFIED，旧失败不再当前阻塞。下一唯一Task NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001 READY_SOURCE_WITNESS_AND_EXACT_RECORD（已创建精确Task，尚未改脚本）：生产ApplyThrow旧next预取+两个Cpoint raw setter旧857门/descriptor，会让原生高帧action与snapshot分离；先当前source见证，保护RunKind2Validation212，禁止全局getter替换。987/reduced/gain/各端点及kind2/kind3/C25已验不重做。BDEFEND等其它父字段记录需按各自出口回链，不据三包VERIFIED自动标全Q06。现无build/test/agent/Play运行；Q07资源迁移未启动，原raw3缺口/跨Worldepoch/stage USER_HOLD/用户例外保持，禁止computer-use/非战斗/Scene/资源修改，总目标ACTIVE。

> 当前阶段BATCH-03/Q06。VERIFIED / DECLARED_NONCHARACTER_REDUCED_AND_FALLBACK_SCOPE。987四组before0/diff0，资源联合19/19、type3联合27/27、完整SelfCheck16:36:40Z PASS；本批local replay54场景/108重放tick（job7fa21a8a1a9546ef96d214ed84048cdb 2/2 PASS）；真实Play16:45:17Z 3948/3948 PASS，两factory/direct-Shadow/Scene checksum保持/Renderer2→2；关闭16:45:31Z PASS，restore4→4、World/slots/两pool0、两帧Stopped。独立最终review四生产路径未发现确定新错误。当前四个qualification完整driver回访job65d082698b534717b51999e3be656a5b 4/4 PASS；原失败保留。不据此关闭全部Q06、跨World恢复或正式内容/视听；资源0/1hop/gain边界及两级resolver focused为实际范围，未穷尽所有owner组合。 源子项同样限定VERIFIED。下一最小任务：qualification父出口审查/其明确目标Play证据（若缺则只补四个完整driver在真实Play的场景保护验证），随后收口qualification/collision-frame依赖并返回NATIVE-FRAME-RUNTIME-READER-MIGRATION剩余live reader调用图。不要重做原3264端点、source构建、spark/feedback/weapon/type5/reduced；旧父文档失败检查点已追加纠正。当前无build/test/agent/Play运行，Editor已退Play；Q07正式DAT/图片仍未部署，raw3缺口/epoch/stage USER_HOLD及用户例外保持，总目标ACTIVE。禁止computer-use/非战斗/Scene/资源修改，未提交推送。

> 当前阶段仍BATCH-03/Q06。NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / ALL987_AND_SELF_CHECK_PASS。source owner987双跑SHA42367528…44bc9/545439PASS；Unity先before0 direct140/Shadow284，确认旧helper读Runtime.MP错误，root新gain块改Runtime.PP/MPMax，旧MP/helper保持；HitPlan独立resource-owner tuple(PP/消费/类型/上限/localflag/冻结slot+handle)及最多2级独立resolver已写，gain非0观察限制已移除。job6c1197e1854b4f2c8d420f73393a5d3a 19/19 PASS含987四组0差异/owner解析/旧纯函数/scope。后C30 SelfCheck暴露type3 target hit声，parent生产和标准/D1预测修正，音频oracle重新开/关留痕；job a568ca84a4a948e5942c57e0e2244a85 27/27 PASS含987及type3专项，完整SelfCheck16:36:40Z PASS。XML/失败/最新PASS全部parent artifact。下一直接本批真实Play987×两factory×directShadow=3948、关闭及本批local replay；尚未新增对应probe，不要重跑已过矩阵或重做source。若需两级resource-owner实际Shadow/非零localMode边缘可补必要focused，当前987只有0/1hop/invalid、旧resolver两级7项已过，不声称整个owner所有输入穷尽。当前无job/build/agent运行，工作树HEAD观察为f9f7b133（本任务未提交），既有外部提交不回退。Q07资源未部署、schema/raw缺口/跨World epoch/例外保持，总目标ACTIVE，禁止computer-use/非战斗/Scene/资源修改。

> 2026-09-15 当前阶段：BATCH-03 / Q06，IN_PROGRESS。Q01～Q05已达到各自限定出口；Q07正式DAT/角色图片迁移尚未启动，Q08～Q12等待前置。当前Q06子项为NONCHARACTER-REDUCED-HIT-TRANSACTION：843四组0差异及原984回归已通过；正在补非零gain/resource-owner独立观察。新增source owner987（含144 owner/gain边界）已双跑一致SHA42367528b7429d0dfb525a5c3208151f1bdd0937db7657b7b4a0b40066044bc9、545439独立检查PASS；Unity fixture已准备切owner987并恢复OwnerSlotIndex，但尚未刷新/跑987 RED，Shadow仍gain!=0不覆盖。下一步明确：刷新→987 before0及覆盖RED→扩独立resource-owner tuple→复测，再本批SelfCheck/Play/关闭/replay。无运行build/test/agent；不得将source987通过当Unity987通过。以下旧检查点仅保留历史，当前入口以此为准。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / ALL843_FOCUSED_PASS。root已新增LF2Entity.NativeHitCandidateScope可空原Route/嵌套恢复，runner先Resolve再Begin；DamageWriter weapon/type5/type3水平后按明确BrokenFallback消费、bypass -1保持、type3 generic音效去除；HitPlan已同步原route与独立fallback预测。job46a5d9c9928c4c13b69d75976a1feb87 5/5 PASS：843四组before0/diff0+scope嵌套异常恢复。原回归jobf25fb55daf6941ca8c49a4cbe64d36b1 18项17PASS/1旧type3音频断言FAIL，原984/684early/Bdefend/prelude replay全PASS；独立oracle NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001 VERIFIED，单断言修正后job4b866e2569774961ad93f303a054d398 4/4 PASS。所有XML同parent artifact保留。下一必须项非零gain/resource-owner独立捕获/比较与源/Unity向量，现CanProjectNativeNoncharacterReduced明确gain非0拒绝，不能当完成；还需新selfcheck、Play两factory/关闭及本批local replay和必要type3专项音频回归。当前无job/build/agent运行；本批四生产文件+单新fixture及单oracle，未改Scene/资源/非战斗。Scene SHA BCD1047B…0E9FB6保持。总目标ACTIVE/Q07未部署，禁止computer-use。

> 新子项 NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001 PLANNED，等待当前回归终态后修正旧音频断言；parent仍IN_PROGRESS。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / REDUCED_CORE_PASS_FALLBACK_RED。三生产文件已改：runner原dispatch传prelude.Route，新DamageWriter.TryApplyNativeNoncharacterReducedHit完整reduced；HitPlan保存原NativeRoute并独立预测（worker单文件完成root已读）。刷新编译无CS错误，job843b7b0c7dea469085d07194dda2a170终态四FAIL：843 before0，direct67差异(原14271)，Shadow94(原14346)；681实际reduced行四组全部0差异，剩余仅162fallback子集中的hp_activation/mp_activation/bypass_minus_one。原red和本轮reduced-core-pass-fallback-red均归档。下一步继续修复原命中route到unarmored fallback的明确broken pointer消费，不能HP==-1猜；type3回退音频及Shadow fallback guard未覆盖也需处理，精确必要新增路径先入Record。潜在方式扩展已有LF2Entity.NativeHitCandidateScope瞬时route并确保嵌套恢复/重置，但尚未声明或改该文件本批。新reduced production有非零gain owner处理，Shadow明确gain!=0不预测，需resource owner tuple与非零专项源/运行验证，不能永久跳过；843gain0仅本范围通过。无SelfCheck/Play/原984新回归，未整批关闭。当前无build/test/agent运行，禁止computer-use/非战斗/Scene/资源修改，Q07未部署、总目标ACTIVE。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / PRODUCTION_RED_CONFIRMED。新增单Editor fixture接source843。首次job268c98…因integer/precise归一化before274失败（fixture-position-red保留），仅恢复Runtime.XInt/YInt/ZInt修正。第二job256ee489459a428c9e21be219e390c31终态四FAIL：每组843 before0，direct14271差异、Shadow14346差异，production-red目录完整JSON/XML。source独立review可推进该合成矩阵RED，仍明确未激活resource transfer/status/weak/scale、非零reference/其它attacker类型是后续验证边界。下一步在本Record先加入准确SequenceRunner/DamageWriter/HitPlan路径再实施native reduced route传递与完整事务及独立预测，不再重复RED。原parent108小伤害不足以收口；843所有groups及原984保护都需回归。当前无测试job/build/Play运行，本批Unity生产未改。禁止computer-use/非战斗/Scene/资源修改，Q07未部署、总目标ACTIVE。

> 当前 NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / TEST_FIRST_ONLY，root新增单Editor fixture接843，source子项仍保留实际独立验证边界。尚未改本批Unity生产。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / ACTION843_PASS_SOURCE_REVIEW。新增96 current4/7/70/75/action30/110/Y±5/Bdefend39/40/41行，总843；成功构建两run exit0/11275383bytes一致SHA c2e036569bb39f3692e34c8fce9061083acc18deec228c5cf3d04f413b1abdd0，459183检查PASS，action843归档。发现并纠正预期：state70/75不论朝向优先defense(armor null/threshold30)，state7同向才type1 armor(threshold40)；实际取current而非snapshot0state4。所有行新增完整CRT递推/sync标量事件/火花geometry与selected-route ID检查；162fallback追加broken-pointer HP0对比bypass初始-1保持、原HP/HPBound及不扣armorMP断言。full reduced raw/extra/rest及全部finalizer已验；fallback其它普通尾部仅source输出，不冒充独立全尾部证明。下一步收敛该源任务实际验证边界并独立审查，进入843 Unity fixture RED（source完整DAT/before/after可直接复建）；不要再无证据扩张源矩阵。保留资源/动作/route所有组，不缩到108小伤害测试。首次build参数输出误入helper错误及case771错误route预期已记录，成功重建数据为准。无build/test/agent运行，本批Unity生产未动，总目标ACTIVE/Q07未部署，禁止computer-use/非战斗修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / RELATION747_PASS_REMAINING_SOURCE_CHECKS。675的validator已强化全部reduced raw/extra/rest独立预测和全部行after→finalizer全状态计算；再加72 negative-parent/packed-delay行共747，构建/双跑exit0、9920711bytes一致SHA1cd3335b9f912a52a2a05c174a3bf192fa3a3f67c096e5e6f59b1701741c1931，379755检查PASS，relation747目录完整归档。有效parent2无reciprocal也复制rest之后hold，不取负；无效9不fallback；delay105/-205有符号计算已验。162 fallback仍只有选择/activation及finalizer检查，完整普通尾部不冒充验证。下一步补current7/70/75、action110阈值与snapshot分离、spark/随机完整独立递推、broken-fallback关键指针差异；然后Unity fixture RED及三生产文件接线。两Tools root独占，worker撤权停止；本批未改Unity生产，无build/test运行。源任务未关闭，总目标ACTIVE/Q07未部署，matched限定VERIFIED不重做，禁止computer-use/非战斗/Scene/资源修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / RESOURCE675_PASS_FULL_TUPLE_PENDING。root两Tools追加324 MP临界、72 armorHP临界、6 bypass_minus_one，共675；构建/两run exit0，8952470bytes一致SHAeb64b286317163c9435bcf1bd42d08d133bc976b52cf52a9c54af34533697bec，独立22239检查PASS。resource675目录含first/repeat/manifest/validation，273和24历史保持。162例确实fallback，validator只验它们decision/activation，不把全部普通尾部算通过；其余513 reduced行已验资源/HP/durability/status/horizontal/post等。新增params armorMp/armorHp/decrease/currentMp/runtimeArmorHp/bdefend，输出armorDecision/activationAvailable/activationMpCost/armorBroken，不仅selectedArmorType。下一步补negative parent hold/current-action阈值、full tuple/finalizer及fallback关键指针差异独立断言，后再Unity RED/三文件实现；本批Unity生产仍未改。无运行build/test/agent，原source worker撤权停止。matched已限定VERIFIED不重做；总目标ACTIVE/Q07未部署/禁止computer-use与非战斗修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / MOTION273_PASS_RESOURCE_PENDING。root两Tools已扩展273例（原24+motion216+effect_position24+post2000九例），构建与双跑exit0、3573700bytes一致SHA5e87945ff3ab9368b5225972a0b436333893cadb45aa3284f375ce07107c3c9b，独立9843检查PASS，motion273子目录含manifest。验证了地面±1/小数半速、Y/reference三侧、1002 0xF3及Z/-1.5、2000远离阻尼/相等不减速、22/23无反向例外。完整源任务仍未关闭：资源/armor临界/不足/破裂/bypass、negative parent hold、current/snapshot/action阈值、full tuple/finalizer断言仍待。下一步直接扩展ReducedCase/dat/emit与validator；armor_resolution.cpp105确认MPcost0强制1、currentMP<cost不足，HP<=effectiveInjury破裂（仅mp0）；记录路由不可只用selectedArmorType推断fallback。现无build/test运行，原worker已撤权停止，本批Unity生产未动。matched限定VERIFIED不重做，总目标ACTIVE/Q07未部署，禁止computer-use和非战斗修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / BASE24_PASS_FULL_MATRIX_PENDING。原worker两次无产出已中断且撤销写权限，root接管并实际新增两Tools。基础24例构建exit0、双跑exit0/309580bytes一致SHA e5f597a7435e5fb03d705b3bc3519f3fd4adc7bd4f034ccfb0160607816774fe，独立792检查PASS，base24子目录完整归档含manifest。六type×active/defense×injury±7，dvx5；明确尚缺全部资源/armor临界/运动/关系扩展，不能标完整源任务完成。下一步直接扩展当前ReducedCase/reduced_dat/emit_reduced和validator，不重做matched，不改本批Unity生产。Build/NTSD28NoncharacterReduced无运行进程；两子代理已停止或完成。精确路由和consumer/horizontal审计在父NONCHARACTER-REDUCED-HIT-TRANSACTION Task。总目标ACTIVE，Q07未部署，禁止computer-use/非战斗修改。

> 当前启动 NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE_ONLY；两Tools脚本worker独占，root核对Unity候选级route消费，尚不改本批Unity生产。matched批次已限定VERIFIED不重做。

> 2026-09-14 15:15Z当前唯一游标：NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001 VERIFIED / DECLARED_NO_ARMOR_TYPE5_MATCHED_SCOPE；源NTSD28-Q06-TYPE5-MATCHED-PAIR-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_ONLY。138四组无差异、24+4相关回归、SelfCheck/Play552/关闭已通过；最后matched回放2/2 PASS，80场景160重放tick。两生产文件本轮未再次修改，只补原测试文件回放。下一唯一任务NONCHARACTER-REDUCED-HIT-TRANSACTION-001：先为完整reduced资源/armor/运动/随机补新source见证及准确Record，不能只把原108小伤害样本修绿。只读定位已写入该Task，三生产候选SequenceRunner/DamageWriter/HitPlan，命中时Route需保留，不用armorHP=-1猜。当前无运行job/build/Play。总目标ACTIVE/full incomplete，Q07未部署，schema/raw缺口/跨World epoch/stage.dat USER_HOLD及用户例外保持。禁止computer-use、非战斗与资源变化。

> 2026-09-14 15:09Z当前游标：TYPE5-MATCHED-PAIR-EARLY生产两文件已修复；四138 before0/diff0，24/24相关测试及另旧type3四项4/4 PASS。SelfCheck15:07:54Z PASS；真实Play15:08:59Z 552 PASS（两factory/direct+Shadow、Renderer2→2、场景checksum保持），关闭15:09:11Z PASS（恢复4→4、World/slots/两pool0、两帧Stopped），Scene dirtyfalse/root14。所有证据同ID artifact。唯一剩余本批验收：专门matched local snapshot replay未补（普通type5/weapon replay已通过，不能替代）；补该项及source/主Task治理收口后再进入NONCHARACTER-REDUCED108。当前无运行测试或build/Play probe，下一步不用重做已过矩阵与Play。总目标ACTIVE，Q07未部署，禁止computer-use/非战斗修改，HUDBg30保留。

> matched生产修复当前：138四组before0/diff0，job2f14606469fe40a6a8dab6c794c2ee2d 24/24通过；旧type3准确namespace补跑job33d31e5cef654979b380316477b99dc6 4/4通过。原Editor文件新增本批552真实Play probe，已请求刷新，尚未执行；本批SelfCheck及matched local replay仍待完成。生产两文件独立review未发现新增错误，Scene hash保持。无测试job运行；不要重复原RED或已通过矩阵。

> 2026-09-14 matched批次更新：source138/78933 PASS，Unity有效RED四组before0/3432差异后，已按准确Record修改DamageWriter和HitPlan：无armor type5 matched入口、native current/latch/raw reset、独立Shadow投影。编译Console暂未见CS错误；job 2f14606469fe40a6a8dab6c794c2ee2d正在执行本批/普通type5/weapon。旧type3实际namespace为NTSD.Test.Editor，需补跑准确名称，不能把未选中的类算回归通过。SelfCheck/Play本批未跑，保持IN_PROGRESS。

> 活跃 `NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001` IN_PROGRESS / TEST_FIRST_ONLY；root单Editor测试读取source DAT/before，源worker仍在验证，未改生产。

> 当前执行 `NTSD28-Q06-TYPE5-MATCHED-PAIR-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY；两个Tools文件由worker独占，根代理检查Unity映射。普通type5/weapon已验收不重做，尚未改本批Unity生产。

当前唯一恢复游标（2026-09-14 14:28Z）：

- `NTSD28-Q06-TYPE5-UNARMORED-UNITY-001` VERIFIED / DECLARED_NO_ARMOR_ORDINARY_TYPE5_SCOPE。两生产文件已改：type5 native40/20/0分档/保留80/动作/机械/rest/post/音频，HitPlan独立type5预测；worker只读原型由root重写集成，reviewer只读复核。不要恢复旧50/30/10、清零80、legacy random/post或旧通用音频，不重做已验weapon。
- 源`TYPE5-UNARMORED-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY：585/14048，两遍SHA c164b073b4ee789df121f18dcffe8253d0771c21c37703eca35b2e73d4acf30e，source build在Build/NTSD28Type5Witness（避免Editor退出清理Temp），独立validator已保存Tools目录并登记Record。
- 原四组585 before0/直接各2277、Shadow各2862差异已清零；首次22/22 PASS含weapon14与Bdefend四256。后扩展50项46PASS/4FAIL：type5新增14个local replay/28ticks、原34和684早期等全部PASS；4FAIL仅原完整984 reduced108，每组846差异、before0、Shadow额外0。
- `TYPE5-HIT-PLAN-COVERAGE-AUDIT-001` VERIFIED / DECLARED_NO_ARMOR_TYPE5_WRITER_COVERAGE。原16例实测plan valid=true/failure0/dispatch1/writer0（不是实际伤害未执行）；先修真实type5再独立投影，现在Bdefend四256全PASS、每candidate观察1。armor/special states/全部DAT-CLR组合未据此关闭。
- 完整SelfCheck14:21:38Z PASS；真实Play14:24:37Z两factory×direct/Shadow×585=2340 PASS，Scene checksum保持/Renderer2→2；14:24:58Z关闭PASS：restore4→4，World/slots/两pool全0，两帧Stopped。Editor已退出Play、Scene dirtyfalse/root14/hash bcd1047b…保持，生产hash保持。
- **下一唯一Task：`NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001`，READY_NOW。** reviewer已定位native6616非角色初始matched3005/3006早返，Unity仅type3；旧pair reset/latch getter上限857在latch/action900错误。先新源见证和完整RED，正确位置复用rest→target reset→attacker reset→holder hold release，再独立Shadow；不能只加类型gate。正常非match下prev13/snapshot12、非零reference/正向child-rest也补最小向量。精确合同及source位置在Task和artifact independent-review.md。
- 然后`NONCHARACTER-REDUCED-HIT-TRANSACTION-001`处理108例（90active type1+18defense），破甲需进入命中时selected route，不能仅RuntimeArmorHp=-1猜测。父NONCHARACTER-ARMOR-FEEDBACK/BDEFEND/collision及Q06 umbrella未整体关闭；之后回完整984/driver/reader/表现，再Q07。
- 2022 GUI Editor曾退出，原MCP job无终态未伪报；已确认别项目FPSTest2023，两个独立2022 batch分别exit2保留RED。用户重新打开本项目2022 GUI后，预检阻止第二实例，已转原Editor MCP。现在无运行中build/test/Play/request；两个Astra子代理均只读完成。
- 总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。Q07正式DAT/角色图片未部署，Scene仍旧内容/合成fixture；未验物理按键/Logan图片一致性。schema15/23/26/2/2、raw47/3、跨World allocation epoch缺口、stage.dat USER_HOLD/用户例外保持。用户HUDBg30已确认其或其他任务修改并保留，禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server变化；未提交推送。

以下历史检查点由以上当前游标优先：

> 当前实现入口 `NTSD28-Q06-TYPE5-UNARMORED-UNITY-001` IN_PROGRESS / TEST_FIRST_ONLY。source585/14048通过，源two-run SHA c164b073…cf30e；单新增测试复用完整tuple，生产未改。type5旧16例明确plan valid=true、failure0、writer观察0，实际dispatch1，不能将mask0当覆盖通过。

> 当前必要依赖 `NTSD28-Q06-TYPE5-UNARMORED-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。静态确认type5实际旧反应阈值/80清零与源不同，先585向量，不仅补Shadow。原2022 Editor已退出，旧job无最终证据；已核对只有别项目FPSTest的2023 Editor，当前独立2022 batch定向测试在运行，不操作别项目。

> 当前执行 `NTSD28-Q06-TYPE5-HIT-PLAN-COVERAGE-AUDIT-001` IN_PROGRESS / DIAGNOSTIC_ONLY：单Bdefend测试补完整plan/CLR/当前DAT诊断，生产本批尚未改。上一武器包已验收，不重做。

当前唯一恢复游标（2026-09-14 10:39Z）：

- `NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` VERIFIED / DECLARED_WEAPON_TRANSACTION_SCOPE。三生产路径（DamageWriter、BruteForce kind0 heavy预处理、HitPlan）与测试已闭合声明事务；不要重做旧随机team/frame/self-rest尾部或恢复heavy减半。原source2100/43202，两遍SHA28cad088…55a。
- 最终48/48 PASS：source2100×2profile×direct/Shadow=8400全部0差异；原34、整数X两例、8case本地回放/16ticks、六oracle入口通过。每候选prelude/writer各观察1次。state2000读int X；state1002用native同步0xEE/16，state3000 post只执行一次；保持counter/latch/Fall80/team、精确0.55及type2 low-fall跳过Y/action。
- 独立`NTSD28-Q06-WEAPON-REACTION-SELF-CHECK-ORACLE-001` VERIFIED / TEST_ORACLE_ONLY：七个旧SelfCheck方法按当前源纠正，旧三次FAIL保留。完整SelfCheck10:33:50Z PASS。`NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY。
- 真实Play10:36:58Z两factory×direct/Shadow×2100=8400 PASS，Scene checksum不变/Renderer2→2；10:37:31Z关闭PASS，原位restore4→4、World/slots/两pool全0、两帧Stopped。Editor idle非Play，Console error0，Scene dirtyfalse/root14/hash bcd1047b…保持。生产/正式EXE hash保持，未提交。
- **下一唯一Task：`NTSD28-Q06-TYPE5-HIT-PLAN-COVERAGE-AUDIT-001`**。最新BDEFEND两profile direct256均PASS，Shadow每profile只剩16无armor type5观察guard；需输出valid/count/failure再落实对应owner，不删断言。当前DAT/CLR不一致的weapon旧Shadow guard亦为相关待核线索，未证明全部shell已覆盖。
- 接着`NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001`：最新完整984四组各846差异/108例（90type1 active+18defense），before0、Shadow额外0；124无护甲weapon已清。原34、684早期四组与本地回放全PASS。父NONCHARACTER-ARMOR-FEEDBACK/BDEFEND/collision仍未关闭；reduced后回完整984/父矩阵，再reader/表现/Q07。
- 总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。Q07正式DAT/角色图片未部署；真实Scene仍Unity旧内容与合成fixture，未做物理按键/Logan图片一致性验收。schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD和既有例外保持。
- 用户确认HUDBg x30由其或其他任务修改，保留且不再询问；禁止computer-use、非战斗/Unity-GAS框架/Scene/资源/Server改动。本批准确记录/原始证据在同ID artifacts REPORT。所有build/test/Play/request均终态，无待轮询job。

以下历史检查点由以上当前游标优先：

> 活跃依赖 `NTSD28-Q06-WEAPON-REACTION-SELF-CHECK-ORACLE-001` IN_PROGRESS：完整SelfCheck10:16:55Z旧HitConfirm2断言FAIL；定向收集后按正式源纠正，禁止回退已过8400生产。

> 当前武器批次：`NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` IN_PROGRESS / DIRECT_2100_PASS_SHADOW_PENDING。三生产路径已改、两profile各2100直接对照0差异；当前正编译并补Shadow独立预测。源见证`NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY（2100/43202）。非战斗/HUDBg30保留，Q07未部署、完整对齐尚未完成。旧下文TEST_FIRST_ONLY/生产未改描述仅为历史检查点。

> 活跃 `NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` IN_PROGRESS / TEST_FIRST_ONLY，source2100与单Editor测试，生产尚未改。

> 当前执行 `NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。补武器完整反应边界，不重做已过前置；Unity生产本轮尚未改。

当前唯一恢复游标（2026-09-14 09:31Z）：

- `NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001` IN_PROGRESS / PRELUDE_FEEDBACK_RUNTIME_PASS_DAMAGE_DEPENDENCIES_OPEN。四生产文件已写：新BattleNativeOrdinaryHitPrelude、Resolver专用native入口、Runner真实前置顺序、HitPlan独立前置token/反馈预测。已不再是TEST_ONLY，禁止重复实现。详见同ID artifact REPORT.md。
- source984四组最新before0差异，after每组1518条/232case（原4641/712），Shadow额外错误0/每候选前置观察1次。source684前置/反馈/拒绝契约四组全PASS（210 feedback+294 rejected+180 unsupported），feedback另断言writer观察1。原完整984失败保留，不把684当全包完成。
- 旧34回归全PASS；完整SelfCheck09:17:43Z PASS；真实Play两factory×direct/Shadow×684=2736 PASS，before/after0差异、主Scene checksum保持、Renderer2→2；有序关闭PASS（原位恢复4→4、World/slots/两pool全0、两帧Stopped）。本地回放16场景/32replayed ticks通过，实际随机状态/links/rest/spark/完整checksum一致、Shadow有效。内部SynchronizedGeneration在restore故意失效旧cursor，测试已另验证旧cursor不能commit，非canonical allocation epoch问题。
- **下一唯一执行Task：`NTSD28-Q06-UNARMORED-WEAPON-REACTION-001`。** 处理完整frame/team/hitReaction/rest/legacy随机尾部；BDEFEND256剩64武器例×3 raw=192，新984对应124个无护甲/绕过护甲武器例。原已有源只是部分state/height，按live调用补必要边界，准确新Record后实施；不能只改frame3→186，不能重做已过前置/反馈。
- 然后`TYPE5-HIT-PLAN-COVERAGE-AUDIT-001`剩无armor16 guard（首type0的16已清）；再新Task`NONCHARACTER-REDUCED-HIT-TRANSACTION-001`的108例（90type1 active+18defense）。之后回完整984/BDEFEND256/父collision。完整984剩232=124weapon+108reduced。
- BDEFEND测试direct已纠正到正式candidate入口，独立`NTSD28-Q06-BDEFEND-FORMAL-CANDIDATE-ENTRY-ORACLE-001` IN_PROGRESS / ENTRY_FIXED_PARENT_DAMAGE_PENDING；原raw/HitStateCount241/C25断言保持。当前每profile direct192/Shadow208，四组FAIL待上述依赖，不把测试入口修订当生产伤害全通过。
- 前置关键规则保持：unarmored先reciprocal2/-2 release与special-rest，再type0反馈，再普通rest/firstbody；同步0xEC/6写child raw action，target Vy精确-1.0000000000000258、child Vy不改，getter矩阵[0][2]=45/[1][2]=30，槽历史保留。type1 bypass/active、defense rest早返后的fallback不可合并。新版Shadow在新token中捕获全Native随机与保留字段，旧ZeroAttackerHp consume位置保持，legacy heavy flag不二次执行。Native代次仅用于游标失效，不修改persistent schema。
- 所有build/test/Play均已终态，Editor已退出Play；MCP当前端口6403，每次仍读取状态文件发现，不硬编码。HEAD 61b3b6cf，本轮未提交。生产hash在SelfCheck/Play后保持，仅测试/文档再补回放。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE，各父记录未关闭。
- Scene hash bcd1047b…、用户HUDBg x30、Unity/GAS/非战斗/Server边界保持，禁止computer-use。Q07正式DAT/角色图片未部署；schema15/23/26/2/2、raw47/3、跨World allocation epoch缺口、stage.dat USER_HOLD及其余例外不变。

以下历史检查点由以上当前游标优先：

> 活跃测试子项 `NTSD28-Q06-BDEFEND-FORMAL-CANDIDATE-ENTRY-ORACLE-001` IN_PROGRESS，保持原断言，只纠正direct入口边界。

当前唯一恢复游标（2026-09-14 08:43Z）：

- `NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY。984向量、5108检查、两遍逐字节一致SHA12323954821eceea6cfc7aa3155ae66102c4389f4fdb47813dd8febc374c0a4a；正式EXE/75源身份保持。完整source调用/字段/限定范围在同ID artifact REPORT。
- **下一唯一Task仍为`NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001`，IN_PROGRESS / RED_CONFIRMED_BEFORE_INPUT_MATCHED。** 新984×两profile实际runner对照已跑，before受测raw47/3+links/rest/sparks0差异，after各4641条/712case。原FAIL见同ID artifact red/；当前Change Record只声明单新增测试脚本，尚未修改本批Unity生产。下一步按该REPORT落实单一ordinary hit前置owner、列准确生产code-path/Shadow阶段，再实施；不要继续重复source build或新建泛化审计。
- 必须一起解决的已证顺序：原unarmored在armor/普通rest/first-body前执行reciprocal2/-2 release和special-link-rest；Unity目前CanConsumeRecordedCandidate先查live vrest，first-body又早于ConsumeEffects，不能只在DamageWriter最前面加return。源synchronized0xEC/6→child action；target/holder Vy=-1.0000000000000258而child Vy7.5保持；rest按getter实际矩阵[0][2]=45、[1][2]=30。父子slot历史保留，special gate可作用于dormant，不能用active-only getter替代。
- 分支陷阱已补源样本：type1 ratio15<Bdefend17是bypass，会回unarmored prelude；Bdefend0的active/reduced另序。defended wrapper的reduced在rest提前返回时未保留defense_decision，普通unarmored入口可能继续fallback（state7+type0+rest5仍feedback）；type1顶层直接返回reduced，不可合成一个简单armor/defense bool。action_latch的Unity映射仍Runtime.WaitCounter，勿新造字段。
- 984含720基本组合、48 current第一bdy1033/1100500000、180 type1 active、36 defense/rest向量。source210 feedback、198前置后rest拒绝、180前置后unsupported、96 reduced rest全状态不变；24反馈跳过current第一bdy。此为诊断冻结候选初值，未证明完整driver可达或默认spawn links；测试显式赋source默认links0。
- 当前BDEFEND256也已重新测量：两profile direct各968/Shadow各1000条差异（4组FAIL），保留在source artifact unity-parent-red。新984测试入口已覆盖旧heavy release前置；不要把失败只当已排队weapon reaction而略过feedback/时序。尚未新测Shadow984/真实Play；当前生产上一批Spark的34/34、SelfCheck08:06:43Z、Play1440及关闭证据保持，Spark限定职责不重做。
- 本轮只新增1个源诊断CPP与1个Unity Editor测试及文档；上一批生产未再修改。所有build/test均终态，无运行中job。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；父BDEFEND/collision/qualification仍未关闭。反馈之后继续weapon reaction/type5覆盖，再回父包，reader/display后Q07正式资源迁移。
- 保留用户HUDBg x30、Scene hash bcd1047b…、Unity/GAS/非战斗/Server边界；禁止computer-use。Q07未部署、schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD及其它用户例外不变。

以下历史检查点由以上当前游标优先：

> `NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001` IN_PROGRESS / TEST_FIRST_ONLY，当前仅准确单测试脚本；生产未改，源码证据与RED并行准备。

> 当前执行 `NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。反馈前的heavy release旧RNG/字段写入存在差异，先原完整前置取证，不直接早返；Unity生产本轮未改。

当前唯一恢复游标（2026-09-14 08:14Z）：

- `NTSD28-Q06-HIT-SPARK-UNITY-001` VERIFIED / DECLARED_HIT_SPARK_TRANSACTION_SCOPE。公共Append源guard/owner/capacity/编码/snapshot/cover/整数平均/target Z/Y-X CRT，candidate瞬时scope，角色route owner唯一emission、两个重复外层移除，Shadow独立CRT capture/compare已闭合。
- 最终34/34 PASS：source438×2端点876、角色actual282×2=564（复制itr/原index2）、异常嵌套/非0 guard、原完整driver四组8向量及Shadow0差异、local replay8case/16replayed ticks；完整SelfCheck08:06:43Z PASS。真实Play两factory端点+actual共1440于08:11:26Z PASS，Scene checksum不变/Renderer2→2；08:11:53Z关闭PASS，原位恢复4→4、World/slots/两pool全0/连续两帧Stopped。
- 旧C01 hook与SelfCheck观测分别由`NTSD28-Q06-SPARK-C01-TEST-HOOK-001`、`NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` VERIFIED纠正；全部旧FAIL保留。唯一详细证据：`artifacts/diagnostics/NTSD28-Q06-HIT-SPARK-UNITY-001/REPORT.md`。主包8脚本+两个单测试子项，共10脚本；HEAD61b3b6cf，未提交。
- **下一唯一Task：`NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001 / READY_SOURCE_ORDER_AND_EXACT_RECORD`。** 公共spark已就绪，先闭合原selected armor/special-link-rest前置及各Unity活入口，准确新Record后再接反馈Append(...,armor,false,false)，不能无条件早返或只保留Bdefend。非角色首type0原96例曾误伤害；type1不能从type0推断。之后UNARMORED-WEAPON-REACTION和TYPE5-HIT-PLAN-COVERAGE，再回BDEFEND256/父collision/qualification。96/64/32是之前生产的测量，未按本轮重测，不当新鲜结果。
- 父BDEFEND/collision/qualification、reader umbrella与总目标仍IN_PROGRESS / ACTIVE，FULL_ALIGNMENT_INCOMPLETE；旧四组完整driver的Spark RNG首差已经清除，不再当当前阻塞。reader/display其它职责后才Q07，正式DAT/角色图片**尚未部署**。Scene仍旧内容/合成fixture，未做物理按键或Logan图片一致性验收。
- 正式EXE SHA B1E13AE1…D2819033、源438 trace b5df6113…ce6f8复核保持；Scene dirtyfalse/root14/SHA bcd1047b…保持，用户HUDBg x30确认归其或其他任务并保留。schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD及既有例外保持。禁止computer-use、非战斗/Unity-GAS框架/Scene/资源/Server改动。

以下历史检查点由以上当前游标优先：

> `NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` 已VERIFIED；完整SelfCheck08:06:43Z PASS，以下活跃旧行由本状态替代。

> `NTSD28-Q06-SPARK-C01-TEST-HOOK-001` 已VERIFIED，旧回调fixture修正后22/22；以下活跃旧行由本状态替代。

> 活跃测试子项 `NTSD28-Q06-SPARK-C01-TEST-HOOK-001` IN_PROGRESS，见同ID Record。

> 活跃测试子项 `NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` IN_PROGRESS，见同ID Record。

> 当前执行 `NTSD28-Q06-HIT-SPARK-UNITY-001` IN_PROGRESS / TEST_FIRST，准确七脚本；公共writer与candidate瞬时scope、route owner一起闭合，禁止computer-use。

> **当前唯一恢复游标（2026-09-14 07:37Z）：** HIT-SPARK-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_ONLY。原438/2994断言、两遍一致SHAb5df6113…；366追加+CRT2/72不追加+CRT0，Native0，原EXE/75源身份保持。完整owner/capacity/编码/index/armor/负数几何/CRT数组已证；本轮只新CPP/文档，未改Unity生产、未跑新Unity测试。Scene文件hash bcd1047b…保持。
> **下一唯一Task：`NTSD28-Q06-HIT-SPARK-UNITY-001 / READY_EXACT_CONTEXT_AND_TEST_FIRST_RECORD`。** 先读source artifact REPORT，准确公共emitter/真实caller/瞬时原itr index与hit前selected armor上下文Record，再438 Unity RED/实现。重要纠正：LF2Character.Hit→Dat普通分支ApplyStandardCharacterDamage后RecordKind0Hit即return，普通命中不走底部旧SpawnSpark；不能只改它两行。旧CurrentItrIndex仅Character BeforeDispatch写，generic为空，不能当可靠通用index。route owner保留armor和unarmored/reduced/feedback flags，每次hit只发一次，移除重复外层调用，C01/C25记录生命周期保持。
> 后继顺序仍Spark Unity→NONCHARACTER-ARMOR-FEEDBACK（96）→UNARMORED-WEAPON-REACTION（64）/TYPE5-HIT-PLAN-COVERAGE（32guard）→回BDEFEND256/完整driver。BDEFEND字段已写但父仍IN_PROGRESS；旧完整driver只有RNG差异，所有原失败保留。此前SelfCheck07:18:42Z是旧生产证据，本轮无新Play/自检；无运行中build/test/exec。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
> 用户HUDBg30保留，禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server更改；raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD及用户例外保持。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-HIT-SPARK-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。普通命中实际走RecordKind0Hit而非底部旧SpawnSpark；原三入口/index/armor/geometry/capacity/CRT一起取证，生产未改。

> **当前唯一恢复游标（2026-09-14 07:21Z）：** BDEFEND-FIELD-FAMILY-UNITY-001仍IN_PROGRESS / FIELD_FIX_WRITTEN_DEPENDENT_HIT_PATHS_OPEN。三生产文件已写Runtime.Bdefend的45/signed累加/阈值、armor delay读Bdefend、Shadow独立transient TargetBdefend（保留legacy观测），C25/compat/schema不改。新256×4仍FAIL，direct各968/Shadow各1000；角色全部和type3/5无armor raw/字段已匹配。当前完整driver四组bdefend0/45已清，仅Spark RNG legacy3/native0/CRT0对源CRT2失败。全部原FAIL保留，未标已对齐。
> **下一唯一Task：`NTSD28-Q06-HIT-SPARK-TRANSACTION-AUDIT-001 / READY_NOW`。** 原非角色所选armor反馈只调用完整spark事务即return，故必须先Spark，再NONCHARACTER-ARMOR-FEEDBACK-001（96例误走damage），再UNARMORED-WEAPON-REACTION-001（64无armor武器team/frame/fall首差）、TYPE5-HIT-PLAN-COVERAGE-AUDIT-001（32Shadow附加guard失败mask0，待打印valid/count）。随后回BDEFEND256/完整driver和两factory运行验收。不能用空return或只保留Bdefend掩盖反馈事务，C17例外仅legacy1。
> 独立BDEFEND-TEST-ORACLE-001 VERIFIED：原SelfCheck07:14:34Z旧HitStateCount观测FAIL保留，准确StandardCharacter/C30/Alternate fixture改Bdefend，07:18:42Z完整PASS；其他断言/compat API保持。本轮尚无新Play验收，不引用旧Play作为新生产通过。Editor idle非Play，无运行中test/build/exec；Scene文件SHA bcd1047b…保持。下次只需继续Spark源调用链和当前失败，不重建已验256/3264端点。
> 父BDEFEND/qualification/collision均进行中，总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。用户HUDBg30保留；禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server更改。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署和stage.dat USER_HOLD/用户例外保持。

以下历史检查点由以上当前游标优先：

> NTSD28-Q06-BDEFEND-TEST-ORACLE-001 IN_PROGRESS / TEST_ONLY；07:14:34Z SelfCheck旧HitStateCount观测失败，按原Bdefend字段更新准确fixture并重验。

> 当前执行 `NTSD28-Q06-BDEFEND-FIELD-FAMILY-UNITY-001` IN_PROGRESS / TEST_FIRST，准确四脚本，原256和独立legacy HitStateCount哨兵，C25/兼容API保持。

> **当前唯一恢复游标（2026-09-14 07:00Z）：** BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_ONLY。原256/1280断言、两遍一致SHA6ba2e26f…；分支148覆写45、96非角色首type0反馈保留、6 signed累加、6armorHP保护，接续C25 held/free恢复验证。确认Runtime.Bdefend raw绑定及既有C25h owner正确；旧HitStateCount无当前C25h owner，不可全局alias。当前轮只新诊断CPP/文档，未改Unity生产或资源，未跑新的Unity测试。
> **下一唯一Task：`NTSD28-Q06-BDEFEND-FIELD-FAMILY-UNITY-001 / READY_EXACT_CALLERS_AND_TEST_FIRST_RECORD`。** 先精确actual三类writer/armor route reader/Shadow分支，原256设Bdefend与legacy HitStateCount不同值做RED。无护甲写45，非角色首type0反馈不得写45，reduced signed add不能clamp，armor matcher读Bdefend，Shadow须独立观测该字段；原C25恢复/copy/schema/compat字段保持。OID300/kind7/D1投影及reduced current/Prev2/Y条件不能按grep泛改，按新Task补必要源证据。之后HIT-SPARK-TRANSACTION-AUDIT-001，再回4完整driver FAIL。
> 父COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001和COLLISION-FRAME-UNITY-001仍IN_PROGRESS；3264端点已0差异，但8个完整driver向量仍bdefend0/45与RNG legacy3/native0/CRT0（源CRT2）首差，未豁免。此前SelfCheck06:27:14Z、Play480/关闭等通过是前轮证据，不报本轮新运行。无运行中build/test/exec；Scene文件SHA bcd1047b…再次保持。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
> 用户HUDBg x30保持；禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server更改。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD及例外不变。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。已确认Bdefend是+0x0B8且C25h正确恢复，需迁移命中写入/护甲读取/Shadow同字段家族；不把旧HitStateCount全局alias。

> **当前唯一恢复游标（2026-09-14 06:40Z）：** COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001及父COLLISION-FRAME-UNITY-001仍IN_PROGRESS / QUALIFICATION_RUNTIME_PASS_FULL_DRIVER_DEPENDENCIES。普通/cached/immediate/consumer的current额外门已移除，snapshot几何/current state0分离、pair current-null state0、previous Native；原336×4+新480×4共3264端点全部0差异，旧96candidate首差清零。最终12项8PASS/4FAIL，4FAIL是完整driver两例×四组的独立bdefend0/45与RNG legacy3/native0/CRT0（源CRT2）差异；其余raw/HP499/caught动作/snapshot0一致、Shadow有效/2effects/mask0。原所有FAIL保留，不能标全包VERIFIED。
> **下一唯一Task：`NTSD28-Q06-UNARMORED-BDEFEND-WRITER-AUDIT-001 / READY_SOURCE_WRITER_AND_FIELD_MAP`。** 原battle_world6743无护甲命中直接写bdefend_accumulator45；Unity raw是Runtime.Bdefend，但actual/plan多处仍写独立HitStateCount。先字段/消费者/恢复与分支闭环，准确Record再改；不是DAT默认值，不改converter补45。之后HIT-SPARK-TRANSACTION-AUDIT-001：DatHitResolver.SpawnSpark828/829旧BattleRandInt两次应核整个原CRT/snapshot/容量/owner事务，两factory一起；C17例外仅legacy1，不豁免额外2。两子项后回4完整driver FAIL，不重做已通过kind2/3和3264端点。
> Source COLLISION-QUALIFICATION-SOURCE-WITNESS-001已VERIFIED限定源480+2/3020断言/重复一致；独立COLLISION-ROLE-MATRIX-ORACLE-001 VERIFIED，原70回归69PASS/1旧期望FAIL，按source纠正后4组合PASS。完整SelfCheck06:27:14Z PASS，真实Play480/Renderer2→2/Scene checksum保持、有序关闭06:38:10Z恢复4→4、World/slots/两pool0/两帧Stopped。Editor idle非Play、接口6402（状态文件发现）、CS0、Scene dirtyfalse/root14/SHA bcd1047b…保持。无运行中test/build/exec，不再等旧job。
> 总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。用户HUDBg x30确认归其或其他任务并保留；禁止computer-use/非战斗/Unity-GAS/Server改动。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD和用户例外不变。

以下历史恢复检查点由以上当前游标优先：

> NTSD28-Q06-COLLISION-ROLE-MATRIX-ORACLE-001 IN_PROGRESS / TEST_ONLY；70回归69PASS/1旧期望FAIL，按原snapshot几何改为1，保留测试断言。

> NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001 IN_PROGRESS / TEST_FIRST；准确三脚本，原480非零state/frozen eligibility加完整driver2例，先Unity RED，平台current规则不改。

> 当前执行 `NTSD28-Q06-COLLISION-QUALIFICATION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY；单CPP补pair非零state/原eligibility与完整driver换帧见证，Unity父包仍未关闭。

> **当前唯一恢复游标（2026-09-14 05:51Z）：** COLLISION-FRAME-UNITY-001仍IN_PROGRESS / READER_RUNTIME_PASS_QUALIFICATION_PENDING。三生产reader已改（LF2Entity collision、BruteForce current/Prev2、pair factory current/previous），新336×4 descriptor/raw/catch0，但各24candidate首差（合96）保留。kind2/3旧八组PASS、pair/group/catch64PASS；完整SelfCheck05:48:49Z PASS、真实Play168/Renderer2→2/Scene checksum与有序关闭全0/两帧Stopped通过。独立COLLISION-FIXTURE-DEFINITION-IDENTITY-001两处测试修订VERIFIED，原两SelfCheck FAIL保留。源COLLISION-FRAME-SOURCE-WITNESS-001追加84后336限定VERIFIED、旧252前缀不变。
> **下一唯一Task：`NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001 / READY_SOURCE_CONSUMER_AND_QUALIFICATION_MAP`。** 原普通geometry按snapshot itr，platform另按current itr；Unity carrier/current itr/body/null门额外拒绝。先原consumer/pair非零state/当前0语义和正式可达性，准确子Record后整体处理普通/cached/consumer，不豁免24失败。无需重做kind2/3和已验证source336。parent collision未关闭，pair state/filter与actual+Shadow新边界尚欠证据。
> 当前无运行中test/build/exec；Editor idle非Play、CS0、Scene dirtyfalse/root14/SHA bcd1047b…保持。用户HUDBg x30确认归其或其他任务并保留。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署及stage.dat USER_HOLD/用户例外不变。禁止computer-use、非战斗/Unity-GAS/Server改动。

以下为历史恢复检查点，以上当前游标优先：

> NTSD28-Q06-COLLISION-FIXTURE-DEFINITION-IDENTITY-001 IN_PROGRESS / TEST_ONLY；新SelfCheck失败是invalidFirst仍带body定义、只改Frame指针的旧夹具，先用真实空定义修夹具，保留断言。

> NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001 追加84个current=snapshot原输入，暂IN_PROGRESS；旧252验证不撤销、不覆盖。Unity同一job9efe00d15694402e9a63636d58cb2ea4仍待终态，不因MCP观测timeout重跑。

> 当前实施 `NTSD28-Q06-COLLISION-FRAME-UNITY-001` IN_PROGRESS / TEST_FIRST；准确四脚本，先原252输入Unity RED，禁止computer-use及非战斗变更。

> **当前唯一恢复游标（2026-09-14 05:29Z）：** COLLISION-FRAME-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_WITNESS_ONLY。252原输入/3780断言、两遍字节一致，72有候选/108kind1推进、RNG0，正式EXE/75源身份保持。35调用/7文件、无override；本轮只新增诊断CPP，未改Unity生产/资源/Scene。用户已确认HUDBg x30为其或其他任务修改，保留。
> **下一唯一Task：`NTSD28-Q06-COLLISION-FRAME-UNITY-001 / READY_UNITY_RED_AND_LIVE_GATES`。** 原当前定义+snapshot点查不fallback current。LF2Entity、BruteForce current/Prev2及pair factory有耦合旧门，先原向量Unity RED并核对正式时点可达性，准确新Record再实施。source current1000/snapshot0仍保留候选属于诊断分离初值，不能单独授权删全部current门；正式collection前刚snapshot。CPoint kind1用snapshot，孤立kind2用current。旧throw局部缓存/ThrowInjury==-1变身须另找当前authority，不能沿用历史结论。
> 此轮未跑新Unity compile/SelfCheck/Play；上次05:10:10Z SelfCheck是旧生产证据。无运行中build/test/exec。Scene SHA bcd1047b…保持；总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD及所有用户例外不变。禁止computer-use/非战斗/Unity-GAS/Server改动。

以下为历史恢复检查点，以上当前游标优先：

> 当前执行 `NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。碰撞 getter 审计已定位 35 处调用/7 文件（另 1 声明）、无 override；原当前定义+snapshot 查询，无 current fallback。仅单 CPP 原函数见证；Unity/资源未改，用户确认 HUDBg x30 为其或其他任务修改并保留。

> **当前唯一恢复游标（2026-09-14 05:12Z）：** KIND2-PICKUP-NATIVE-FRAME-LOOKUP-001和NATIVE-PHYSICS-MISSING-FRAME-GUARD-001均VERIFIED限定职责。原1200/Unity四组4800拾取+紧接holder物理0差异，新16/16、旧与完整frame215/215、SelfCheck05:10:10Z PASS、Play800/Scene checksum/Renderer2→2、有序关闭全0/两帧Stopped。actual/HitPlan Native目标读取及窄raw holder绑定一起闭合；缺帧physics gate保留pending/hold/link先后，valid998/decl999继续运行。无运行中test/build/exec，不再等待旧job。
> **下一唯一Task：`NTSD28-Q06-COLLISION-FRAME-NATIVE-LOOKUP-AUDIT-001 / READY_SOURCE_AND_CALLER_MAP`。** LF2Entity.GetCollisionFrameData仍旧HasFrame门并存在Prev2→current fallback；先列全部真实caller/override及非战斗边界，核对source snapshot/current/definition身份，不能机械换getter。它影响后继碰撞/命中和CPoint，优先于继续throw/direct setter。此前kind3/kind2/C25已关职责不重做；原reader umbrella仍进行中。
> 其它CPoint action/throw原定义快照、raw/held/input/hit/生成reader仍未全部迁移；之后display其余出生/post→Q07正式DAT/角色图。资源未部署、schema15/23/26/2/2、raw47/3、已使用World epoch恢复缺口和所有用户例外/stage.dat USER_HOLD保持。Scene dirtyfalse/root14/用户HUDBg x30/SHA bcd1047b…保持，禁止computer-use/非战斗/Unity-GAS/Server改动。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复检查点，状态由以上当前游标替代：

> 当前必要子项 `NTSD28-Q06-NATIVE-PHYSICS-MISSING-FRAME-GUARD-001` IN_PROGRESS / TEST_FIRST；pickup getter/binding已写且拾取端0差异，后继physics1250/route缺帧移动仍待修。准确三脚本，保留hold/link/pending顺序和dead normalization。

> 当前实施 `NTSD28-Q06-KIND2-PICKUP-NATIVE-FRAME-LOOKUP-001` IN_PROGRESS / SOURCE_FIRST，准确五脚本；kind2 getter与raw holder native绑定共同闭合，pure plan/legacy共享入口不改。

> **当前唯一恢复游标（2026-09-14 04:36Z）：** KIND3-CATCH-NATIVE-FRAME-LOOKUP-001已VERIFIED / KIND3_PAIR_ENTRY_ONLY。原800（512允许/288双实体不变拒绝）与Unity四组3200 before/after raw47+catch字段零差异，最终25/25、完整SelfCheck04:34:40Z PASS、真实Play320/Scene checksum/Renderer2→2、有序关闭全0/两帧Stopped。仅actual/HitPlan的kind3 Native descriptor先检后写，kind1/shared setter未改。旧99/98不可用fixture与SelfCheck已按源改1000/1001，原FAIL保留；总计六脚本。无运行中test/native build/exec，不再等待旧job。
> **下一唯一Task：`NTSD28-Q06-KIND2-PICKUP-NATIVE-FRAME-LOOKUP-AUDIT-001 / READY_SOURCE_AND_CALLER_MAP`。** 审计actual TryApplyPickup和HitPlan.ProjectPickupWriterEffect的target legacy getter，以及同一pickup事务SetHolderAction→DirectWriteRawFramePreserveWaitCounter；先原kind2完整分支/已锁定表与caller，不预判或全局改getter。需要改时准确新Record，getter+必要holder绑定一起闭合。继续原Native reader umbrella，CPoint/throw/direct/raw/held/input/hit其它旧reader仍未全部迁移。
> C25/frame已关职责和本kind3不重做。已使用World snapshot canonical allocationEpoch恢复缺口仍OBSERVED/owner-review，不能称任意恢复一致。reader后display其它出生/post→Q07正式DAT/角色图迁移；资源未部署、schema15/23/26/2/2、raw47/3与所有用户例外/stage.dat USER_HOLD保持。Scene dirtyfalse/root14/用户HUDBg x30/SHA bcd1047b…保持，禁止computer-use/非战斗/Unity-GAS/Server改动。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复检查点，状态由以上当前游标替代：

> 当前实施 `NTSD28-Q06-KIND3-CATCH-NATIVE-FRAME-LOOKUP-001` IN_PROGRESS / SOURCE_FIRST，kind3 actual与HitPlan都仍旧HasFrame；成对Native descriptor先检后写，准确五脚本。其它direct/CPoint/throw/pickup旧reader未自动迁移，C25已关职责不重做。

> **当前唯一恢复游标（2026-09-14 04:00Z）：** NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS与UNITY均已限定VERIFIED，父NATIVE-FRAME-TRANSACTION-INTEGRATION也已VERIFIED / OFFLINE_NATIVE_FRAME_TRANSACTION_SCOPE。原450/1350，Unity20组1800/5400零差异，local/fresh-transfer replay8组32/64及真实Play224/672、Scene checksum/Renderer2→2、有序关闭全0/两帧Stopped通过。生产本轮未改，最新同生产SelfCheck仍03:22:37Z PASS（未重跑）。最终CS0/Editor idle、Scene dirtyfalse/root14/SHA bcd1047b…保持。没有运行中的test或exec，不再等待旧job。
> **下一唯一Task：`NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_REMAINING_LIVE_READER_MAP`。** 从当前真实caller继续剩余direct setter/input/命中/CPoint/held/生成Native查询迁移，优先核对LF2Entity.SetFrameTickDirect/SetFrameTickRawDirect/SetFrameTickImmediateRawDirect以及Cpoint raw reader；它们仍可见legacy getter，先追活入口和原函数，不能只凭grep全部改。C25帧body/声音/成本/prev078/fragment/lifecycle与已关闭源向量无需重做。该umbrella Task尚无通用Change Record，选定准确code-path后建立子Record，不把本轮测试Record扩成任意生产授权。
> **保留未修缺口：** SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT-001 OBSERVED / RECOVERY_OWNER_REVIEW_REQUIRED。预分配目标World跨恢复的canonical epoch2/1有32原失败；完整checksum不覆盖该差异，不能称任意恢复已对齐。当前只证local及空World同历史转移，未改shared Kernel/Server或恢复政策。该formal恢复边界原S0已未实现，继续离线reader/display/资源工作，后续涉及恢复必须回访。
> 普通OPoint effect/continuation(type0/5+parent credit门)/defend及其他字段仍见OPoint-REMAINING-CONSUMER-AUDIT；reader后回display其余出生/post→Q07正式DAT/角色图迁移，资源未部署。schema15/23/26/2/2、raw47/3、所有用户例外及stage.dat USER_HOLD保持。用户HUDBg x30保留；禁止computer-use、非战斗/Unity-GAS框架改造。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复记录，状态由以上当前游标替代：

> 新发现保留：`NTSD28-Q06-SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT-001` OBSERVED / RECOVERY_OWNER_REVIEW_REQUIRED；已使用目标World转移epoch2/1，原32差异不忽略。formal snapshot preservation原S0边界未实现；当前frame单测试只补同出生历史空World转移，不修改Server/epoch政策或声称所有恢复完成。

> 当前执行 `NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-UNITY-001` IN_PROGRESS / TEST_ONLY；source450/1350/934声音事件0frame/lifecycle错误、两次一致已VERIFIED_SOURCE_MODEL。先初始raw再三tick，保留raw47/3和C17边界，生产未改。

> 当前执行 `NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY，准确单CPP，450 case×3tick；Normal/Practice正式默认HP28=1/MP2c=1/drop4c=2、中性输入，上一轮六修复限定VERIFIED保持。

> **当前唯一恢复入口（2026-09-14 03:23Z）：** C25L-STATE18-SPAWN、LATE-OPOINT-DEPTH-AND-LIVES（generic基线限定）、OPOINT-TARGET-WORLD-INITIALIZATION、NATIVE-PENDING-PRE-C25-MOTION-GUARD、STATE18-RNG-EXCEPTION-FIXTURE、OPOINT-WEAPON-HP-BIRTH 六Record均已VERIFIED各自限定职责。原24回归终态22PASS/2FAIL仅weaponHp；修复后30/30通过含两个失败chunk5。最新full1514+未受影响direct1507合计3021向量0差异，分次证据明确记录；真实Play12+84+96、最终SelfCheck03:22:37Z PASS、有序关闭全0及两帧Stopped、CS0/Scene dirtyfalse/root14/hash bcd1047b…保持。没有运行中的Unity job或未收取exec，不再等待旧5a104/ce0f等job。
> **下一唯一Task：`NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS-001 / READY_SOURCE_DRIVER_JOIN`**，服务于仍IN_PROGRESS的父NATIVE-FRAME-TRANSACTION-INTEGRATION：复用2676端点和既有state18/fragment完整组合成果，补高动作/成本回退连续完整driver、sound/previous078/lifecycle及真实Play证据；不得重做已关闭载体/碎片/本六修复。先核对正式GameSession options（尤其resource mode28=1）和现有FrameCase，准确新source CPP Record后才写。上一轮宣布高动作见证时因真实weaponHp差异暂停，尚未写这个runner。
> 已纠正旧归因：原276 non-RNG=252 pending位置+24普通OPoint weaponHp；独立C17例外546=273稀疏×两mode，未修改随机掉落生产。新增源210 witness明确kind2 positive hp覆盖weapon_hp；普通OPoint effect/continuation(type0/5+parent credit gate)/defend等仍在OPoint-REMAINING-CONSUMER-AUDIT（父frame后）中，不把generic1/0/0写成最终全部生成字段已对齐。
> 父frame完成后自动返回原frame reader/input/碰撞/held/生成余项→display其余出生/post→Q07正式DAT/角色图迁移；资源尚未部署、schema15/23/26/2/2、raw47/3、所有用户例外及stage.dat USER_HOLD保持。用户HUDBg x30保留；禁止computer-use、非战斗或Unity-GAS框架改造。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复检查点，状态被上面的当前游标替代：

> **纠正旧276条归因：** 旧24 full chunk中276 non-RNG条目包含252 pending位置条目及24普通OPoint weaponHp=0/17条目；此前声称276全为pending不准确。新回归chunk5已复现weaponHp，不能关闭父包。当前必要子项 `NTSD28-Q06-OPOINT-WEAPON-HP-BIRTH-001` IN_PROGRESS，先原birth/kind2完整字段见证（Tools CPP），待同一Unity job终态后再写Unity；高动作完整driver见证延后到这项闭合后，未创建其脚本。

> **当前唯一执行游标（2026-09-14 03:00Z后）：** 正在回跑原24个失败完整tick组，同一Unity job `5a104f86aa1147a3921e27253a5fcc8f`，共1514向量。不得重复启动/编辑正在执行的脚本；首个64向量chunk零差异不是最终结果。旧82项终态FAILED及24旧chunk均已归档，不再等待旧job。桥接exec session49308已收取并以30秒观测timeout结束，不再poll该session；超时不等于测试失败，继续查询同一Unity job。
> `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001`两生产入口与World exact gate已写：6/6 focused（84 pending资格+12原full-driver）、真实Play12逻辑夹具、Scene checksum保持/Renderer2→2、有序关闭World/slots/两pool全0且两帧Stopped、最终SelfCheck **02:59:21Z PASS**（请求02:58:40Z）。尚有源实体C25后位置/编码state/pending/code显式断言要在当前24组终态后补入同一pending测试脚本；现full-driver CompareChildren覆盖子粒子raw，并不等于源47字段全部比较，不能漏掉或误报。
> `NTSD28-Q06-STATE18-RNG-EXCEPTION-FIXTURE-001` IN_PROGRESS / TEST_ONLY：独立C17两profile/freeSlots -1/0/1/3共8/8 PASS，稀疏1次、密集0次，实体raw不变/NativeRandom零。据此full-tick只限定legacy 1/0并输出逐向量计数，直接C25仍0，native调用/raw47/生命周期/池断言均保留；生产随机掉落例外未改。546旧guard=273稀疏×两mode；276真实pending首差已由生产修复而非豁免。
> 三关联包C25L particles、late OPoint depth/lives、renderer targetWorld及上述两项仍IN_PROGRESS，待24终态/必要源断言最终证据再限定关闭，随后回父NATIVE-FRAME-TRANSACTION-INTEGRATION的完整C25出口。不要重做已验证源1550/96、direct1507或已关闭碎片。Q06/总目标ACTIVE；15/23/26/2/2、raw47/3、Q07正式资源尚未部署；用户HUDBg x30/Scene SHA BCD1047B…保持。禁止computer-use/非战斗/Unity-GAS框架改造。

> 当前必要测试子项 `NTSD28-Q06-STATE18-RNG-EXCEPTION-FIXTURE-001` IN_PROGRESS；pending motion三脚本6/6已通过，先独立C17测量后限定完整tick legacy断言。

> 当前实施 `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001` IN_PROGRESS / TEST_FIRST，准确三脚本；motion/physics拒绝pending但保留独立dead normalization/C25，禁止computer-use及非战斗修改。

> **最终测试已结束（不再等待旧job）：** `aa6b0c9f033e4b8b83174826936e782c`终态FAILED，实际82项：58PASS/24FAIL，48/48 chunks、3021向量均执行，XML归final-regression-82.xml。直接出生两profile1507向量全0差异；完整tick失败含546条whole-tick legacy-RNG guard及276条真实raw首差。静态C17独立world.Rng用户例外要限定，不能抹除pending=1/code1101的C25前运动首差（原源不动，Unity多走velocity）。**下一唯一Task `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001 / READY_FOR_CALLER_MAP_AND_EXACT_RECORD`**。先读完整chunk5/原pending向量，闭合motion/physics所有前置资格后针对失败输入重验，再限定RNG例外与回访C25L/最终SelfCheck/Play，不重复已验1550/96或重新开旧测试。
> C25L粒子、late OPoint depth/lives、renderer target World三个Record仍IN_PROGRESS，17 focused/正式192/最终Play96和关闭全0的限定证据保持，但不能升整体VERIFIED。Editor71188/6401，Temp旧bridge已清，内联代码BRIDGE.md/函数store可用；所有此前pending exec handles已结束，不再poll52994。源码/资源/Scene/Unity-GAS框架边界保持，总目标ACTIVE。

> **当前最优先恢复（验证仍运行）：** 同一Unity job `aa6b0c9f033e4b8b83174826936e782c`，已落盘46/48 chunks、2904/3021 vectors，508条whole-tick legacy-RNG guard和276条其他差异。**先等待/归档终态，不重启或修改运行中的脚本。** 已确认其他首差属于pending=1/code1101样例的C25前源运动未抑制（Source过滤346/348/352等；原不动，Unity多走3.25/-1.25/.75），下一必要Task `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001`。不能把这些raw位置差异作为C17例外忽略。C17的world.Rng独立随机武器流是批准例外，另作限定与计数验证，不要求整tick legacy流零。
> 三实施Record仍IN_PROGRESS：C25L粒子、late OPoint depth/lives、renderer OPoint target World。它们已有17 focused、正式192及Play96/关闭全0，但大矩阵不通过，最终SelfCheck（最后World绑定后）待。当前Editor71188/bridge6401；内联代码见同artifact BRIDGE.md，functions store unityBridgeInline；Temp/Goal13_bridge.py不存在。最后桥接查询exec session52994可能timeout，先收取后查同一job。总目标ACTIVE；当前schema15/23/26/2/2、raw47/3、正式资源未部署、用户Scene bcd1047b保持。

> **最新验证游标：** 同一job `aa6b0c9f033e4b8b83174826936e782c`仍待终态；当前40/48 chunks、2520/3021 vectors、497失败条目，最新批次还含其他差异（见current-test-progress和chunk原文），不能归并为legacy例外；此前返回组主要为full-tick `legacy RNG consumed`。**暂不改断言或重启测试。** 静态已定位C17 BattleRandomWeaponDropModule.RunNormalDrop:65在weaponCount<4时调用world.Rng.NextInt(0,200)；总表R-04/W-07/Unity独有F明确这是用户批准例外。直接C25L phase0两profile已完成1507向量0差异，证明C25L本身未消费legacy流。待job结束核对全部失败是否仅此，并用调用链/定向测量限定例外；full-tick不能笼统要求整个legacy流为0，也不能删除未知下游差异。需要调整时只约束isolated C25并保留full-tick NativeRandom/raw和例外计数证据，再重跑必要失败组。
> 桥接恢复代码已写artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001/BRIDGE.md；functions store `unityBridgeInline`可直接用。其余code/17/Play96/关闭成果及三Record仍IN_PROGRESS，最终SelfCheck/Scene/Ledger还待。

> **当前执行检查点（2026-09-14）：** C25L-STATE18-SPAWN-TRANSACTION、LATE-OPOINT-DEPTH-AND-LIVES、OPOINT-TARGET-WORLD-INITIALIZATION三个Record仍IN_PROGRESS；代码已写，17 focused/正式192端点、SelfCheck（World绑定修复前）及最终正式999两factory完整tick Play96、借用2→2/Scene checksum/关闭全0与两帧Stopped有据。Play先发现普通OPoint未绑定目标World，已修；随后8个空闲Renderer不足人工峰值24，probe用现有诊断API预热到26后96PASS，未改slot400/配置/资产。
> **下一唯一动作：继续等待同一Unity测试job `aa6b0c9f033e4b8b83174826936e782c`，不得重复启动。** 当前已落盘26/48组、1635/3021向量、44差异（最新chunk已出现，待定位），尚无最终XML。选择器含State18新全类（48分组+其余7）、DepthLives6、旧C25L owner、旧SpawnVitals及NativeWeaponPiece。终态后保存Temp/Goal18_LastTestResults.xml、核对实际执行数（不是7540 discovery）、汇总48 chunk；再跑最终完整SelfCheck、Scene hash/CS0/Ledger/diff-check，才决定三项限定关闭及回父Frame。
> **环境恢复：** 外部Editor退出/重开，原PID58092与Temp/Goal13_bridge.py消失；当前同项目PID71188、MCP6401。内联桥接代码保存在functions store `unityBridgeInline`，取出后用PowerShell here-string管道给Python `- <command> '<json>'`；不要依赖已不存在Temp脚本。旧RED三矩阵有文件，第四被中断，未计通过。当前一个get_test_job桥接exec session52994可能返回timeout，先poll该session再查原job，timeout不是终态。Native原formal/synthetic证据在artifacts，部分Temp binary已由外部重启清掉，不需因此重做源见证。
> schema15/23/26/2/2、raw47/3、正式资源未迁移、Scene用户bcd1047b…保持。外部HEAD现2afa54d7，之前部分文件已入基线，代理未commit/push/reset；只保留当前差量。总目标ACTIVE，禁止computer-use/非战斗/Unity-GAS框架改造。

> 当前必要子修复 `NTSD28-Q06-OPOINT-TARGET-WORLD-INITIALIZATION-001` IN_PROGRESS，Play renderer组合错误定义/生命周期，保留无World配置的兼容预览fallback；父C25L/深度lives仍待联验。

> 当前必要子修复 `NTSD28-Q06-LATE-OPOINT-DEPTH-AND-LIVES-001` IN_PROGRESS / TEST_FIRST；源整数Z+point.Z+1及1/0/0出生计数，父C25L仍CODE_WRITTEN/FOCUSED_PARTIAL，禁止computer-use。

> `NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001` IN_PROGRESS补正式999内容向量，原1550证据保持，Unity RED作业继续。

> 当前实施 `NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001` IN_PROGRESS / TEST_FIRST，准确五脚本，Native RNG/generic birth/即时slot及flush；B8动态delay保持独立，禁止computer-use。

> **当前恢复游标（2026-09-14）：** C25L-STATE18-SPAWN-SOURCE-WITNESS已VERIFIED_SOURCE_MODEL_ONLY，1550行775出生+775完整driver、12759检查，重复字节相同；18条预设pending/无定义动作motion诊断留证，frame/lifecycle错误0。原证mixed OPoint slot50→state18 slots51..57→weapon pieces，源20/70顺序；generic HP/MP500/owner-1/team0/right、精确/整数位置分离；seed2/7/12/17/22/24覆盖持续1粒。**F08 Unity仍旧Match.Rng/普通OPoint出生，尚未修。下一唯一Task `NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001 / READY_UNITY_RED_AND_EXACT_RECORD`**，保留已正确C25L owner与state13/200，只闭合剩余生成事务；之后回父Frame联合/reader/display-post/Q07。本轮仅一native诊断CPP，未新增Unity/Play结论，前碎片20/12/SelfCheck/173Play成果保持；schema15/23/26/2/2、raw47/3、资源未迁移、用户Scene bcd1047b…保持，禁止computer-use/非战斗/框架改动，总目标ACTIVE。

> 当前执行 `NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001` IN_PROGRESS，F08原生成/RNG/身份及C25组合见证；不是重做owner位置，Unity未改，禁止computer-use。

> **当前恢复游标（2026-09-14）：** WEAPON-PIECE-SPAWN-ADMISSION-EDGE及父NATIVE-WEAPON-PIECE-TRANSACTION均已VERIFIED / WEAPON_PIECE_SCOPE_ONLY。三生产+一测试脚本：native OID0专用准入、共用动作预检、Renderer factory在Init前用现有pool API绑定目标World；普通OPoint保持。原1800/Unity5152差异0、20/20及binding后12/12、完整SelfCheck PASS、Play172+Renderer耗尽1、旧Scene两route四例0/5/0/5和4→4、关闭World/slot/两pool全0及两帧Stopped，已退出Play。两个Play失败及修正有留证。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FINAL_C25_CALLER_AND_TRACE_JOIN`**，返回父组合顺序及剩余出口联验，不重做已验2676/core/carrier/death/fragment；其后reader余项/display-post/Q07。15/23/26/2/2、raw47/3、正式资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保留，禁止computer-use/非战斗/框架修改；不是全B1-B12或正式图像完整对齐结论。

> `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` IN_PROGRESS诊断epoch投影纠正，原出生/slot/RNG事实保持。

> 当前实施 `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001` IN_PROGRESS / TEST_FIRST，准确四脚本；OID0专用准入/共同动作校验/失败回收，普通OPoint保持，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` 已 VERIFIED_SOURCE_MODEL_ONLY：900出生+900完整driver/17286断言、重复字节相同、frame/lifecycle/diagnostic错误0、最终构建无warning。原证OID0可生成、999需声明且高slot同tick删除/低slot保留、variant RNG先于缺catalog/无slot；两Unity factory统一oid<=0仍为待修差异。**下一唯一Task `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001 / READY_UNITY_RED_AND_EXACT_RECORD`**，源见证不重跑；核对Init/ModuleBind后准确Record、两factory/失败回收/高低slot动态验收，再回父frame/reader/display-post/Q07。前type2 fixture4/2592及完整SelfCheck PASS保持，本轮仅一个诊断CPP，无新Unity Play结论；用户HUDBg x30/Scene bcd1047b…保留，禁止computer-use及非战斗/框架修改。15/23/26/2/2、raw47/3、Q07正式资源未迁移，Q06/总目标ACTIVE。

> 当前执行 `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_MODEL_ONLY；原生成准入及高低slot完整driver见证，Unity未改，禁止computer-use。

> **当前恢复游标（2026-09-14）：** TYPE2-LANDING-FACING-SOURCE-WITNESS和FIXTURE均已限定VERIFIED：原648 physics+648 full tick，648对朝向无差异；原报告场景right→left物理flip在C25保留。Unity两外壳×两端点4组/2592比较全PASS，旧heavyBounce自检已按双初始朝向修正，**完整BattleRuntimeSelfCheck新鲜PASS**，生产未修改。前death prelude退休/STATE9998/64联合成果保持，不恢复旧state2000按vx覆盖。**下一唯一Task `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001 / READY_SOURCE_WITNESS_AND_CALLER_MAP`**：先原函数证OID0/声明或缺失999/非法初始动作/失败RNG及高低slot完整driver参与，再准确Record成组实现，不能把既有157+3或单个条件放宽当完整fragment。之后回父frame/其余reader/display-post与Q07资源。15/23/26/2/2、raw47/3；资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗/Unity-GAS框架改动。证据见type2两个REPORT；完整自检PASS不等于全部对齐。

> NTSD28-Q06-TYPE2-LANDING-FACING-FIXTURE-001 IN_PROGRESS，原1296确认物理flip在完整tick保留；生产正确，修旧fixture并新增对照。

> 当前执行 NTSD28-Q06-TYPE2-LANDING-FACING-SOURCE-WITNESS-001 IN_PROGRESS；原type2物理明确翻面，完整tick保持性待实测，Unity未改。禁止computer-use。

> **当前恢复游标（2026-09-14）：** C25-EXTRA-DEATH-PRELUDE-RETIREMENT已 **VERIFIED / EXTRA_PRELUDE_REMOVAL_ONLY**：准确八脚本退休额外death/bounce/drop及hook，真实hit/physics/WPoint保持；6480原函数区分3240frame端点与3240完整tick，四配置各3240向量及RNG0差异，最终64/64 PASS，独立目标SelfCheck与真实Scene HP0+kind2持有C25保持frame0/Y/Vy0/links1,-1、4→4 checksum及关闭全0/两帧Stopped通过。两个clock/lifecycle fixture Record限定VERIFIED；STATE9998 retirement此前48次上游差异已消除，恢复限定VERIFIED。**完整SelfCheck仍FAIL，下一唯一Task `NTSD28-Q06-TYPE2-LANDING-FACING-AUDIT-001 / READY_SOURCE_PHYSICS_AND_C25_WITNESS`**：type2高速落地实际left与旧state2000强制right期望，先原函数/fixture调用链，不恢复旧行为迁就；随后fragment OID0/999准入及slot完整driver、父frame/其它reader/display-post和Q07资源。15/23/26/2/2、raw47/3，资源未迁移；Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗及Unity/GAS框架改动；旧死亡前置不可恢复。证据见death-prelude REPORT。

> NTSD28-Q06-LATE-SNAPSHOT-LIFECYCLE-FIXTURE-001 IN_PROGRESS，准确旧夹具范围，保留原FAIL。

> NTSD28-Q06-LATE-NOOP-NATIVE-CLOCK-FIXTURE-001 IN_PROGRESS，准确旧夹具范围，保留原FAIL。

> 当前执行 NTSD28-Q06-C25-EXTRA-DEATH-PRELUDE-RETIREMENT-001 IN_PROGRESS / TEST_FIRST，准确八脚本；6480原函数区分frame端点与完整driver，退休额外C25死亡前置与hook，真实hit/physics/WPoint保持。禁止computer-use/非战斗改动。

> 当前 NTSD28-Q06-DEAD-CHARACTER-FRAME-AND-HELD-SOURCE-WITNESS-001 IN_PROGRESS；先原函数frame/完整driver/held矩阵，尚未修改死亡逻辑。

> **当前恢复游标（2026-09-14）：** GT08 fixture已VERIFIED_TEST_ONLY（17/17及完整SelfCheck越过），STATE9998-SOURCE-DRIVER-WITNESS已VERIFIED_SOURCE_MODEL（224场景672完整tick全存活）。**STATE9998-LEGACY-CLEANUP-RETIREMENT为 COMPILE_PASS / FOCUSED_PARTIAL / SCOPED_PLAY_PASS**：准确五脚本，移除Serial末尾额外9998删除、其余职责/顺序不动；224场景删除差异0，联合28/29、最终6/7，保留type0 HP0共48次动作差异，beforeSerial已186。真实Scene当前descriptor9998经Serial存活/checksum恢复4→4/关闭全0及两帧Stopped通过。**下一唯一Task `NTSD28-Q06-C25-DEAD-CHARACTER-EXTRA-BOUNCE-AUDIT-001 / READY_SOURCE_CALLER_MAPPING`**，先查C25额外death bounce与held关系，不直接删真实hit/physics反应；随后核验完整SelfCheck最新type2落地方向FAIL（已过GT08/GT09），再回fragment OID0/999准入及完整driver/父frame。15/23/26/2/2、raw47/3保持；Q07资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use及非战斗/Unity-GAS框架改动。证据见两个STATE9998 REPORT；不得把6/7或限定Play改写为完整对齐。

> 当前执行 NTSD28-Q06-STATE9998-LEGACY-CLEANUP-RETIREMENT-001 IN_PROGRESS / TEST_FIRST；原完整driver224/672全存活，移除Unity额外9998删除须先RED并保留其余Serial职责。GT08 17/17及完整SelfCheck已越过，后续landing matrix有独立首差异。禁止computer-use/非战斗修改。

> NTSD28-Q06-STATE9998-SOURCE-DRIVER-WITNESS-001 IN_PROGRESS；先原完整driver测量state9998再裁决旧Serial残余，不依据grep直接删除。

> 当前执行 NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001 IN_PROGRESS，准确两测试脚本；GT08改实际producer及Native state独立断言，GT09 state9998原链缺证另建见证，不沿用旧权威。禁止computer-use。

> **当前恢复游标（2026-09-14）：** 原函数WEAPON-PIECE-SOURCE-WITNESS已VERIFIED；NATIVE-WEAPON-PIECE-TRANSACTION为 **FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / FULL_TRANSACTION_INCOMPLETE**。六脚本两阶段及两个factory专用出生已写，两profile各157/762片与实际3/50片0差异、49联合通过；真实旧内容Scene DataOriented完整Late pass四向量（两factory×healthy0片/broken5片）通过，每次checksum恢复4→4，关闭全0/两帧Stopped。RAW-OBJECT-TYPE-PROJECTION与三个fixture Record已限定VERIFIED。**完整SelfCheck仍FAIL，已越过武器/LC02/GT07，下一唯一Task `NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001 / READY_SOURCE_AND_FIXTURE_MAPPING`**：旧1299→HitStun=-199与mock/Native pending须成组核验；随后fragment OID0/999/非法动作/pool失败/高低slot完整driver边缘，再回父FRAME-TRANSACTION。15/23/26/2/2、raw47/3，Q07正式资源未迁移；Q06/总目标ACTIVE。用户已确认HUDBg x30归本人/其他任务，Scene bcd1047b…保持。禁止computer-use、非战斗或Unity/GAS框架改动。证据详见fragment REPORT；先前SELF_CHECK_PENDING_WEAPON_PIECES被本游标更新，不能沿用“producer未写”。

> NTSD28-Q06-GT07-BREAK-TYPE-MATRIX-FIXTURE-001 IN_PROGRESS，准确GT07矩阵段，原FAIL保留。

> NTSD28-Q06-LC02-INVALID-FRAME-FIXTURE-001 IN_PROGRESS，单SelfCheck构造器两个赋值857→1000；原FAIL存档。

> NTSD28-Q06-WEAPON-BREAK-SELF-CHECK-FIXTURE-001 IN_PROGRESS，单SelfCheck脚本准确fixture100/999与Native pending，原FAIL保留。

> NTSD28-Q06-RAW-OBJECT-TYPE-PROJECTION-001 IN_PROGRESS：仅两脚本raw投影，当前粗分类objectType不能称完整绑定，先七类型RED。父武器生成继续。

> 当前执行 NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST；原函数见证VERIFIED（157+3例/762+50片），准确六脚本两生成阶段及两factory出生适配。完整SelfCheck仍FAIL，禁止只改断言。用户确认HUDBg x30为本人/其他任务修改，保留。禁止computer-use。

> 当前执行 NTSD28-Q06-WEAPON-PIECE-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY；一个native runner验证两种碎片的RNG/slot/完整出生规则，Unity未改，父碎片实现继续。禁止computer-use。

> **当前恢复游标（2026-09-14）：** LIFECYCLE-STATE-CARRIER-001为 **FOCUSED_TEST_PASS / SCOPED_PLAY_PASS**；父FRAME-TRANSACTION-INTEGRATION仍IN_PROGRESS。三字段/copy/checksum/raw及C25尾部接线已写，当前 **15/23/26/2/2、raw47/3**；2676×22两端点/实际Late两路径、41 focused、386/386联合、88工具、真实Scene encoded恢复4→4/关闭全0两帧Stopped通过。Module已改，旧857/HitStun消费者已移除，private shadow已移除。**完整SelfCheck FAIL（旧PendingFlushDestroy断言且OID100无碎片旧假设），碎片producer确实未实现；不能只改断言变绿。下一唯一Task `NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / READY_SOURCE_WITNESS_AND_EXACT_RECORD`**，完整内置+DAT生成/slot/RNG/出生及SelfCheck native fixture后返回父验收。原始FAIL/invalid raw原因与修复都留证；Scene用户bcd1047b…保持，CS0/dirtyfalse/root14。总目标ACTIVE，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / IN_PROGRESS / TEST_FIRST，三字段及15/23/26/2/2、raw47/3联合迁移未发布；父frame紧接着消费，不保留private影子。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001` **IN_PROGRESS / CORE_FOCUSED_PASS / FULL_TRANSACTION_INCOMPLETE**。正式C25 core已写，2676例×14字段由9103差异降到0；core9+相关19最终28/28通过，独立C25-RENDER-PHASE-20-FIXTURE已VERIFIED_TEST_ONLY。**Module尚未修改，仍旧857/HitStun消费，不能做完整高位Play或发布完整frame对齐。下一必要Task `NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：runtime_state_code与render_phase分离，建立pending/code持久真值并替换当前未接线private结果，随后返回本事务按OPoint/state18/078/weapon pieces/lifecycle顺序闭合。两生产+新测试已变，未跑本轮完整SelfCheck/Play；14/22/25/2/2保持，Scene用户bcd1047b…保持。总目标/Q06 ACTIVE，禁止computer-use/非战斗改动。

> 附属测试修正 NTSD28-Q06-C25-RENDER-PHASE-20-FIXTURE-001 / IN_PROGRESS，纠正旧action202自动写20假设，父FRAME-TRANSACTION仍IN_PROGRESS。

> 当前执行 NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST；准确四脚本先Unity原2676 core RED与C25成组接线，完整driver/particles/weapon pieces等出口未完不关闭。14/22/25/2/2保持，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-SOUND-LATCH-CARRIER-001` 已 VERIFIED / SOUND_LATCH_CARRIER_AND_SCHEMA_ONLY：独立NativeSoundActionLatch默认/reset -1、canonical copy/ECS fingerprint/完整checksum/full parity，联合版本 **14/22/25/2/2** 已344/344、完整SelfCheck、真实Scene sound857/frame1分离保存/两checksum恢复4→4及关闭全0/两帧Stopped验证。工具88/88，新鲜native/Unity content与五版本头一致，原raw50仍44相等/6MISSING（新sound latch不在该raw表）。准确22脚本，Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：数据前置已备，直接依据2676矩阵和CALLER-MAP做Unity RED/成组frame、成本、终止及声音事件接线，定义/fusion的latch重置待；实际WAV播放由Q10回访，不形成循环。Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移；禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-SOUND-LATCH-CARRIER-001 / IN_PROGRESS / TEST_FIRST，Runtime独立声音latch数据前置及entity14/aggregate22/checksum25未发布联合迁移；shell2/2不变，producer仍由父帧事务后继。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-STEP-LIFECYCLE-SOURCE-WITNESS-001` 已 VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY：仅一native诊断脚本，真实World step_frame_slot/resolve_pending_lifecycle完成2676向量，复跑逐字节一致；正式EXE/75源码身份保持。Unity生产本轮未改。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：读取CALLER-MAP与矩阵，先完整数据/行为Record；正负999/YReference、原destination负HP/MP成本及fallback、212 stayed、encoded reset的latch/collision镜像、sound独立latch及完整driver tail必须闭合。禁止仅替换857或把两个端点见证当完整driver。前快照绑定244/SelfCheck/Play成果保持，用户Scene bcd1047b…保持；Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-FRAME-STEP-LIFECYCLE-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY；仅一native诊断脚本，先完整帧成本/生命周期原函数向量，Unity生产不变。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001` 已 VERIFIED / SNAPSHOT_NATIVE_DESCRIPTOR_BINDING_ONLY：准确三脚本，Native current/collision descriptor原地与跨World恢复；20项RED16失败→20/20，联合244/244、完整SelfCheck、真实Scene857/998两份checksum恢复4→4及关闭全0/两帧Stopped通过。13/21/24/2/2不变，未证明高位动作完整tick。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_FRAME_STEP_LIFECYCLE_MAPPING`**：frame绑定/direct/next/cost/terminal必须成组，source数据声明999可读但C25存活<999，负next先翻面再999解析；先完整调用表与原函数见证，不能批量857→1000。父零帧reader、其余display出生、post、Q07正式资源均未完。Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE；禁止computer-use与非战斗/Unity-GAS框架改动。

> 当前实施 NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001 / IN_PROGRESS / TEST_FIRST，准确三脚本快照帧绑定；数据声明999可读与C25存活<999是不同合同，frame推进/终止成组后继，禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001` 已 VERIFIED / NATIVE_ACCESSOR_AND_RESOURCE_OWNER_ADMISSION_ONLY：独立Native查询支持0..998零帧/声明999，旧Get/Has/Max857保持；仅资源资格/chp/cmp迁移。原函数81行（63合法/18错误AST）、223/223＋独立旧接口1/1、完整SelfCheck、实际资源owner两入口857/998/999六例/恢复4→4/关闭全0及两帧Stopped通过。INVALID-FRAME-FIXTURE已VERIFIED_TEST_ONLY；HP/MP两个Record恢复限定资源事务VERIFIED，但不能扩大为高位动作完整tick。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_FOR_LIVE_CALLER_MAPPING`**：先实际frame绑定/direct/next/快照读取，逐组迁移其它live reader；父ZERO-FRAME-CACHE-CONTRACT、完整display与post均未完。原388检索清单为改前，406条最新候选见reader-inventory-after-accessor.json，不全是live调用方。Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，资源未迁移；13/21/24/2/2与所有例外不变，总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use/非战斗改动。

> NTSD28-Q06-NATIVE-INVALID-FRAME-FIXTURE-CORRECTION-001 / PLANNED / TEST_ONLY；HP/MP旧invalid7夹具改真正无效9999，随当前Native访问器包继续。

> 当前实施 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 / IN_PROGRESS / TEST_FIRST，四脚本新增Native访问器并修资源读取；旧API保持，全部reader迁移/父零帧任务未完。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001` 已 VERIFIED / OPOINT_VITALS_AND_DISPLAY_BIRTH_ONLY：3716 native、329实际Logan正OID（0按原规则跳过）、212/212、完整SelfCheck、两完整生成路径各两次真实Play/原模式与checksum恢复4→4/关闭全0及两帧Stopped通过。五脚本、仅三生产文件；正式资源/Scene不变，用户HUDBg x30与bcd1047b…保持，CS0/dirtyfalse/root14。**下一唯一Task `NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001 / READY_READONLY`**：native0..998隐式零帧与Unity缓存857/HasFrame声明判定不同；HP、MP两个资源Record已降为 FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION，原有效声明帧公式/phase/Play证据保留。先闭合该reader合同和必要修复，再回DISPLAY-PROGRESSION补非OPoint出生初值/联验，随后POST-DISPLAY。父display/Q06/总目标仍ACTIVE / FULL_ALIGNMENT_INCOMPLETE；13/21/24/2/2、Unity/GAS/非战斗/例外保持，禁止computer-use。

> 资格覆盖纠正：NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 与 NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 降为 FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION。已测有效声明帧成果保留；未声明0..998/cache857/HasFrame需ZERO-FRAME-CACHE-CONTRACT独立闭合，当前出生资源继续。

> 当前实施 NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本。新增零帧reader差异须独立追踪，不能将旧HP/MP有效帧通过推广；父display出生仍待，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001` 仍 IN_PROGRESS，当前递推/slot阶段 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS：新13测试含980 native全部通过，相关181不同测试经177PASS/4旧FAIL＋独立6PASS逐项闭合，完整SelfCheck/真实两tick显示与source真值保持/checksum恢复4→4/关闭全0及两帧Stopped通过。独立C25-POISON-PHASE-FIXTURE、C25-DISPLAY-SCHEMA-FIXTURE均VERIFIED_TEST_ONLY。**出生初始化未完成，不能关闭完整display。下一唯一Task `NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：原OPoint hp/mp及ohp/omp选择/百分比与两生成入口先原子处理，再返回display补全部出生初值与联验，随后POST-DISPLAY（WAIT_DISPLAY_OWNER）。准确四脚本+两个独立测试修正，无非战斗/资源/Scene变更；用户HUDBg x30/Scene bcd1047b…保持，CS0/root14/dirtyfalse。13/21/24/2/2保持，R05/R07显示子条件PARTIAL_RETURN；Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use。

> NTSD28-Q06-C25-DISPLAY-SCHEMA-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，独立旧fixture纠正；父display任务及出生依赖仍待。

> NTSD28-Q06-C25-POISON-PHASE-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，独立旧fixture纠正；父display任务及出生依赖仍待。

> 当前实施 NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001 / IN_PROGRESS / TEST_FIRST，准确四脚本递推+slot适配；出生初值发现OPoint hp/mp/ohp/omp依赖，未处理前不能关闭完整display任务。禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001` 已 VERIFIED_AUDIT_ONLY；`NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001` 已 VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY（980 display＋2379 post向量，直接stdout复跑逐字节一致，正式EXE及75源码/header身份复核）。确认生产C25缺display/post owner，且display需出生初值和独立slot资格；frame_0mp保留原frame状态、display先于limit等边界已测。**下一唯一Task `NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：先闭合现有出生/复用/clone与slot适配，再准确Record实施完整C25d；随后POST-DISPLAY-RESOURCE-TRANSACTION（WAIT_DISPLAY_OWNER）。本轮仅一新native诊断脚本，HP/MP六脚本hash和用户HUDBg x30/Scene bcd1047b保持，无新Unity/Play结论；前HP134/SelfCheck/Play成果保留。R05/R07待生产接通再回访，Q07正式资源未迁移，13/21/24/2/2保持，总目标/Q06 ACTIVE / FULL_ALIGNMENT_INCOMPLETE。禁止computer-use与非战斗改动。

> 当前必要诊断 NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_ONLY，一新runner；父资源显示/post-display审计继续，禁止computer-use，不改Unity生产。

> **当前恢复游标（2026-09-14）：** Q06 HP事务 `NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001` 已 VERIFIED / SCOPED_HP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS；134/134（含3768 HP及2028 MP原函数向量）、两profile真实Logan12tick、完整SelfCheck、旧内容真实Scene HP/恢复4→4/有序关闭全0与两帧Stopped通过。独立 `NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001` 已 VERIFIED_TEST_ONLY。正式默认mode28=1已接；下方旧计划mode0/未写/待验为历史。Scene用户HUDBg x30与bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。**下一唯一Task `NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001 / READY_READONLY`**；先资源display/post-display消费链审计，再精确实施，Q08正式模式投影及Q07资源迁移仍待。13/21/24/2/2保持，Q06及总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；禁止computer-use及非战斗改动。

> NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001 / IN_PROGRESS / TEST_ONLY；GT-06无World夹具补私有World phase，保持HP/MP及反向类型断言，不改生产。

> HP默认来源纠正：正式GameSession/scenario默认28=1，核心ResourceSystemRules孤立默认0不能替代playable入口；生产将传1。HP原版3768向量/13项RED11已取得，追加Logan12tick见证。

> 当前执行 NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本；HP完整分支与共同资格、World phase12、不可变mode28默认0；保留MP成果及用户确认HUDBg x30/Scene bcd1047b…，禁止computer-use/非战斗改动。

> **2026-09-14最新确认与游标：** 用户已确认HUDBg x50→30为自己或其他任务修改，Scene bcd1047b…现状必须保留，旧SCENE_ORIGIN_PENDING标记已解除。NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 已VERIFIED / SCOPED_MP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS（2028 native/119 tests/SelfCheck/实际MP及关闭全0，原数值首差闭合，只剩6MISSING）。下一唯一Task NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD；Q08正式mode投影和其他Q06/资源迁移仍待。禁止computer-use及非战斗改动，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q06 MP限定出口（2026-09-13）：** NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / SCENE_ORIGIN_PENDING。完整MP事务及两caller已接，native2028向量、119/119、完整SelfCheck、真实Scene tick5→6默认MP200/显式mode0=201/weak抑制/恢复4→4/关闭全0通过。当前同源raw44已绑定字段一致，只剩原6MISSING，MP200/201差异已消除；NTSD28-Q06-DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001已VERIFIED_TEST_ONLY，原34FAIL与SelfCheck旧断言留证。**Scene HUDBg x50→30，SHA bcd1047b…，15:38:24Z保存，早于Play；已异步询问来源，保留不回退，不能写Scene unchanged。** 下一唯一Task NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD，可继续独立HP工作。Q08正式mode注入仍待，13/21/24/2/2及所有例外保持，正式资源未迁移，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗改动。

> NTSD28-Q06-DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001 / IN_PROGRESS / TEST_ONLY；34个旧默认MP/phase断言按当前native纠正，生产规则保持。

> 当前执行 NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本；完整MP事务+两caller，显式不可变mode值/source默认1和F6输入，不加未校验World字段或版本。先native分支见证和RED，禁止computer-use及非战斗改动。

> **当前Q06入口（2026-09-13）：** NTSD28-Q06-RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001 已 VERIFIED_AUDIT_ONLY / CAUSE_CONFIRMED。当前正式EXE/runner及75源码-header hash已复核；四次native模式原值对照确认gate恰1抑制普通MP恢复，0/2/-1则tick3增加1。当前Unity新鲜capture 1/1 PASS且内容头完整同值，MP差异仍200/201；未修改脚本/资源，不能写成已修。下一唯一Task **NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD**：完整frame.cmp/regen族/阈值/bound/weak/F6/mode/限幅及两生产caller，明确不可变模式输入与Q08投影边界，不偷加未校验mutable字段/版本。Q05限定交付保持；BATCH-03/Q06 ACTIVE，总目标FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗改动。

> **当前恢复游标（2026-09-13）：BATCH-02 / Q05 已限定交付，下一 BATCH-03 / Q06。** NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001、NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001、NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001 均 VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。实际Logan两profile24tick/22tick重放、slot/pool/错误identity、最终82/82、完整SelfCheck、两次旧内容真实Scene恢复4→4/关闭全0/两帧Stopped/重入通过；CS0，Scene旧SHA/root14/dirtyfalse。三Record共6脚本，仅2个生产snapshot文件，保留Unity/GAS/非战斗。13/21/24/2/2联合schema基线已验证，trace3/raw-source2/50字段保持；正式资源未迁移、六MISSING及MP200/201仍由Q06/后继解决。下一唯一Task **NTSD28-Q06-RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001 / READY_READONLY**。禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。以下较早启动语句仅历史，不应重开已验Q05。

> 当前必要修复 `NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001 / IN_PROGRESS / TEST_FIRST`：真实Scene两个Renderer计入ObjectCount但不占战斗槽，原地restore拒绝；准确三脚本保留原注册/活动计数分域。父Q05未关闭，pool修复保持，禁止computer-use。

> NTSD28-Q05-WORLD-CLOCK-PHASE-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，clock相邻phase顺序夹具纠正；生产pass保持。

> Q05真实恢复验证发现退休shell未归还pool；当前必要修复 `NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001 / IN_PROGRESS / TEST_FIRST`，准确2脚本。移动回放两profile已过，攻击测试入口需改用既有logic-only executor；禁止computer-use，父验收继续。

> 当前执行 `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001 / IN_PROGRESS / VALIDATION_IMPLEMENTATION`，准确2个Editor脚本，真实Logan两profile/24tick恢复回放与pool/slot验收；生产规则保持，禁止computer-use，版本13/21/24/2/2未发布。

> **Q05 trace/raw身份与字段限定出口（2026-09-13）：** `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / FOCUSED_TEST_PASS / SAME_CONTENT_CAPTURE_PASS / RAW_PARITY_DIFFERENT`。19脚本，trace v3/raw-source v2/50字段44绑定6MISSING；实际Logan输入native/Unity raw4EFE/semanticDB57/projection3900完整同值，native复跑字节相同。50不同Unity tests、88工具tests、native2F8非默认unit、完整SelfCheck、旧内容真实Play/零新增缺失通过；Scene旧SHA保持，禁止computer-use。已修诊断入口当前MP误作最大MP及首差排序，生产战斗规则未改。真实3tick比较仍7类差异：原6MISSING与tick3 currentMp native200/Unity201（Q06待追实际consumer）。**下一唯一Task `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，完成同版本有意义的restore/replay/slot-pool/重入验收再关Q05；13/21/24/2/2仍未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。


> 当前执行 `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / IN_PROGRESS / TEST_FIRST`，准确19脚本（含CLI退出码与独立native字段见证），真实内容根/语义头/trace v3/raw v2/50字段；13/21/24/2/2未发布，禁止computer-use，非战斗/正式资源保持。

> **Q05五版本及恢复头部已限定验证（2026-09-13）：** `NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / TRACE_IDENTITY_PENDING`。当前正式代码常量已为 **13/21/24/2/2**，仍INTERMEDIATE_UNPUBLISHED_Q05_WINDOW；修复外层有效而内层旧版仍可恢复的漏洞，复用原发布全子域header predicate。RED11+1→最终287不同测试有通过证据（主286PASS/1旧phaseFAIL经独立`NTSD28-Q05-WORLD-CLOCK-PHASE-FIXTURE-001`定向1PASS闭合），新增23全PASS、完整SelfCheck/CS0/真实暂停World双队列Play tick5对象4→4通过。`NTSD28-Q05-REMAINING-CONTENT-HASH-CONSUMER-AUDIT-001`已VERIFIED_AUDIT_ONLY：未发现额外frame/meta生产hash漏项，明确trace仍strategy-pending/缺完整语义头/49字段。**下一唯一Task `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，同窗口trace v3/raw/source v2/50字段2F8及真实source/raw/decode/semantic/schema绑定，再完整replay/Play；Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。Scene旧SHA/Foot18既有缺失保持，禁止computer-use及非战斗改动。


> NTSD28-Q05-WORKER-LATE-OPOINT-FLOOR-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，旧worker Y45预期按native落地改5并断言parent0；不改生产规则。

> 当前执行 `NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED`，准确15脚本，五版本目标13/21/24/2/2；hash消费者审计已闭合，trace身份/50字段后继，禁止computer-use。

> **Q05快照边界限定出口（2026-09-13）：** `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。11脚本统一完整Host/core/worker/kernel tick、structural及两OPoint owner前置；原body/pass保持，拒绝无队列/World/worker副作用。RED14与HostRED1→最终187/187、完整SelfCheck、真实暂停World tick5/对象4→4双队列拒绝与空闲capture通过；该Play无dedicated worker，不宣称物理技能或worker实战。独立`NTSD28-Q05-WORKER-LATE-OPOINT-FLOOR-FIXTURE-001 / VERIFIED_TEST_ONLY`按native落地修旧child45→5并断言parent0；首次175中174PASS/1旧FAIL留证。Scene旧SHA/Foot18缺失保持，无新增缺失，CS0。**下一唯一Task `NTSD28-Q05-REMAINING-CONTENT-HASH-CONSUMER-AUDIT-001 / READY_READONLY_CONTRACT`**，再联合13/21/24/2/2、trace/replay；当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移，禁止computer-use及非战斗改动。


# NTSD 长期项目状态

> 当前执行 `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / IN_PROGRESS / TEST_FIRST`，准确9脚本，统一tick/structural/双队列/host快照前置，拒绝无副作用；不改pass和关闭顺序，禁止computer-use，版本/trace后继。


> **Q05语义身份已接线并限定验证（2026-09-13）：** `NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001 / FOCUSED_TEST_PASS / SCOPED_PUBLICATION_PLAY_PASS / JOINT_SCHEMA_PENDING`。raw DAT/visual算法保持，V2 tag+raw32 SHA256/LE ulong进入catalog/candidate/cache、两publisher和本地验证session；75主回归PASS、Visual旧类6PASS/1过期六DAT断言由`NTSD28-Q05-FORMAL-VISUAL-CANDIDATE-ADMISSION-FIXTURE-001 / VERIFIED_TEST_ONLY`定向1PASS闭合（82不同focused有通过证据），完整SelfCheck/独立Python hash通过。正式330/906输入capture成功；隔离native格式源实际menu重进cache1/三key同/World4/46资源全释放/borrower0/两帧Stopped通过，非正式330全渲染或整技能结论。**下一唯一Task `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父步骤3双OPoint guard，再相关内容hash/联合版本/trace/replay。当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移；禁止computer-use，Unity/GAS/非战斗、Scene旧SHA/Foot18缺失/例外保持。


> `NTSD28-Q05-FORMAL-VISUAL-CANDIDATE-ADMISSION-FIXTURE-001 / IN_PROGRESS / TEST_ONLY`更正已解决六DAT后遗留的formal candidate拒绝断言；父semantic identity75PASS，SelfCheck/Play后继。


> 当前执行 `NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001 / IN_PROGRESS / TEST_FIRST`，准确7脚本接冻结semantic身份、现有cache/publication与本地验证；raw指纹/协议保持，双OPoint guard/版本/trace后继，禁止computer-use。


> **Q05 HolderCopy载体已清理，步骤2限定出口（2026-09-13）：** `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。36脚本/13生产仅runtime/Entity/task/ECS/校验/HitPlan旧载体删除，bit33退休空洞、真实关系保持；首次863=859PASS/4旧统计预期FAIL，独立`NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`经27中26PASS+最窄1PASS逐项闭合（保留Cpoint原nativeKO1）。完整SelfCheck与真实pickup/replacement/current OPoint tick/注销Play通过，去旧holderCopy后的有效见证前后完全一致/对象4→4。**下一唯一Task `NTSD28-Q05-CONTENT-IDENTITY-AND-CAPTURE-BOUNDARY-001 / READY_FOR_EXACT_PRECHANGE_RECORD`（父步骤3）**，再联合13/21/24/2/2及trace/回放；五类载体不重做。当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。禁止computer-use；Unity/GAS/非战斗、Scene旧SHA/Foot18缺失/例外保持，正式资源未迁移。


> `NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001 / IN_PROGRESS / TEST_ONLY`处理父HolderCopy focused863中的4旧统计预期冲突；production不改，父SelfCheck/Play待。


> 当前执行 `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，准确36脚本，仅退休HolderCopy载体/task/ECS/HitPlan诊断；保留真实关系及其他mask位，禁止computer-use，identity/联合版本后继。


> **Q05 WeaponState载体已清理（2026-09-13）：** `NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。五生产文件仅旧field/copy/reset/init/ECS hash/checksum/parity删除，真实frame.state/GetResolvedWeaponStateForExternalUse保持；282/282、完整SelfCheck、OID124/action40两次pre-frame Play前后有效观察一致，附带四释放用例PASS/对象4→4。**下一唯一Task `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，五类退休载体只剩HolderCopy；之后identity/双OPoint guard/联合13/21/24/2/2/回放继续。当前12/20/23/1/1未发布中间态，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。禁止computer-use；非战斗/Unity/GAS、Scene旧SHA/Foot18既有缺失/例外保持。


> 当前执行 `NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，准确10脚本，仅退休WeaponState载体；真实frame.state与有效状态接口保留，禁止computer-use，联合版本后继。


> **Q05 ReleaseTick载体已清理（2026-09-13）：** `NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。runtime/copy/reset/ECS hash/checksum/parity及三个无效参数/全部caller已删，四生产文件仅参数变化。267/267、完整SelfCheck复跑、四例当前数据Play前后有效输出一致且对象4→4；首轮SelfCheck旧JSON断言遗漏已纠正并留证。**下一唯一Task `NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，再HolderCopy、identity/双OPoint guard、联合版本与回放；Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。当前12/20/23/1/1未发布中间态，正式资源未迁移。禁止computer-use；Unity/GAS/非战斗、Scene旧SHA、Foot18既有缺失及例外保持。


> 当前执行 `NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，准确14脚本，仅退休ReleaseTick载体与无效参数；真实释放关系保持，禁止computer-use，联合版本后继。


> **Q05 GrabbedBy/TrackerFlag载体已清理（2026-09-13）：** `NTSD28-Q05-GRABBEDBY-TRACKERFLAG-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。runtime/Entity/ECS存储、copy/reset/init/自赋值/hash已删；407/SelfCheck/实际pickup-replacement/OPoint tick与unregister Play通过，真实TrackerParent及Owner/Spawner/2F8快照保持。**下一唯一Task `NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，后续WeaponState/HolderCopy及identity/OPoint guard/联合版本/回放继续；五类未全部完。Scene旧SHA/Foot18既有缺失保持，禁止computer-use；当前12/20/23/1/1未发布中间态，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-GRABBEDBY-TRACKERFLAG-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，五类退休字段内先两flag，保留TrackerParent/Owner/Spawner/2F8；剩余三类和联合版本后继，禁止computer-use。

> **Q05 Mass/Oscillate载体已清理（2026-09-13）：** `NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。20处Context参量/旧runtime、spec及两shell字段已移除；890/SelfCheck、实际driver运动与CentralOnly效果snapshot probe通过，三规则核心主体未变。首轮跨ID整tick错误假设已留证并修正夹具，未改规则。**下一唯一Task `NTSD28-Q05-FIVE-RESERVED-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**；2F8/raw恢复和内容来源成果保留。当前shell形状已变而版本仍1/1，只属同Q05未发布中间态，后续13/21/24/2/2、identity/OPoint guard/旧版本拒绝/回放/Play必须继续。Scene旧SHA和Foot18任务外缺失保留；禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED`，准确21脚本，清mass/Oscillate存储与shell，保留Q04已验证行为；禁止computer-use，版本同窗口后继。

> **Q05独立+2F8载体及raw恢复已限定交付（2026-09-13）：** `NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001` 与 `NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001` 均FOCUSED_TEST_PASS，46/46/完整SelfCheck/CS0/无新缺失；ObjectAiExcludedGroupSourceSlot2F8独立int/-1、claimed/raw copy/reset/ECS/hash，已补未占用raw恢复漏项并验证旧字段/checksum。**下一唯一Task `NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，再五reserved；父Q05步骤3身份/OPoint guard、步骤4统一13/21/24/2/2、步骤5回放/Play仍待。当前仍12/20/23/1/1且字段集合处于INTERMEDIATE_UNPUBLISHED，禁止发布/跨版本交换/Q07。+2F8 held writer/AI消费留Q06。Foot18既有缺失和Scene旧SHA保留；禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> +2F8联验发现未占用raw恢复漏项，必要子包`NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001 / IN_PROGRESS / TEST_FIRST`先闭合已有snapshot职责；2F8仍待，禁止computer-use。

> 当前执行 `NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED_Q05_WINDOW`，五脚本独立+2F8，保留Spawner/Owner，Q06消费及联合版本后继；禁止computer-use。

> **Q05护甲/碎片实际来源已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001 / FOCUSED_TEST_PASS / VERIFIED_ARMOR_PIECE_SOURCE_ONLY`。39夹具/405语料/18armor/三piece与330实际metadata；1829不同测试/完整SelfCheck/CS0/旧138仅source身份差异，原双参数构造保留，Temp probe重建。Q05步骤1数据来源已限定完成，**下一唯一Task `NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`（父步骤2）**，再身份/联合schema/consumer/Play，不能直接跳Q07。任务外Foot18文件删除+blue/red/yellow新目录保留，Scene旧SHA保持；禁止computer-use。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，所有例外保持。

> **用户检测方式约束（2026-09-13）：禁止使用computer-use。** 后续Unity检测使用桥接接口、日志、测试结果、进程状态等方式，不做桌面自动化。

> 当前执行 `NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST`，准确12脚本，先native armor/piece结构与typed witness；BMP/stats前包保持，consumer/identity后继。

> **Q05 BMP/stats实际来源已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001 / FOCUSED_TEST_PASS / VERIFIED_BMP_STATS_SOURCE_ONLY`。逐行原版BMP/global stats、动作计数/结束/同一行sheet与实际NativeMetadata接线；405语料/330catalog/28夹具、1390不同测试/完整SelfCheck/CS0、旧138零新投影差异。sentinel/cache与旧sprite准入测试更正均留证，附属`NTSD28-Q05-SPRITE-CORPUS-ADMISSION-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`（402成功/3拒绝）。Scene旧精度差异SHA保持；正式资源、identity/schema/消费者和Play未完成。**下一唯一Task `NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父metadata，不重做BMP/stats。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，例外保持。

> 附属test-only `NTSD28-Q05-SPRITE-CORPUS-ADMISSION-FIXTURE-CORRECTION-001 / IN_PROGRESS`：旧Q02全语料夹具新增原版3失败应拒绝断言，402有效逐sheet保持；BMP/stats主包继续。

> 当前执行 `NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST`，八脚本事前合同；先BMP/stats AST与实际NativeMetadata关联，保留frame/strength与legacy，armor/piece后继仍待。

> **Q05 metadata字段合同已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001 / FOCUSED_TEST_PASS / VERIFIED_FIELDSET_MODEL_ONLY`。不可变ordered Bmp/Stats、Ordinal last-win、预解码int/double有效性及caller fallback已写，新增TryFinite64区分invalid/overflow与有效underflow/零。RED4→486PASS（新4+numeric43+typed439）、4045原版optional、完整SelfCheck PASS、CS0/dotnet0error、Ledger485/95PASS。**未接manager/metadata AST，520精度/3096默认等实际差异仍待。下一唯一Task `NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父metadata integration，不重做模型/数值；identity/schema/consumer/Play及例外保持，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001 / IN_PROGRESS / TEST_FIRST / DATA_MODEL_ONLY`，先optional有效性和不可变字段合同，再父metadata AST/manager/piece接线；原值/资源/例外保持。

> **Q05 definition头部审计已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 / VERIFIED_AUDIT_ONLY / METADATA_GAPS_CONFIRMED`。330真实catalog构建后capture与fresh native同源对照：156角色520个float32精度差、172非角色3096个缺省值差（不冒称全是已复现战斗故障）；stats.max_mp158缺载体，weapon_piece三定义各3组4variant缺结构/载体，BMP shadow30/bound98待非例外Q09。18armor/1320sequence/990weapon sound一致，不重写。stats.y3属平台取景例外，smallb154属HUD、hidden/random各158属选择流程排除。capture1/1、CS0、Ledger484/92PASS，production/资源未改。**下一唯一Task `NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，完成metadata native模型/解析接线后再Q05 identity/carrier/联合版本；前包帧内容与330可构建证据保持。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 / IN_PROGRESS / READONLY_CAPTURE`，已声明两个诊断脚本，核对真实metadata/精度/default/presence；帧内容已验证，不重做或更改production。

> **Q05-A2 native typed帧接线已验证（2026-09-13）：** `NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 / FOCUSED_TEST_PASS / VERIFIED_TYPED_FRAME_CONTENT_AND_CONSTRUCTION_ONLY`。405 DAT/55348声明frame完整40int+六double+27/24/40/9/geometry/ordered sound投影通过，330实际candidate全构建成功，原六文件九frame异常全部消除（不是角色runtime全对齐）。557不同focused有通过证据（555+2）、binary64 2567/原numeric14742回归、完整SelfCheck PASS、CS0/dotnet0error、旧138零新增投影差异、Ledger483/90PASS。**下一唯一Task `NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 / READY_READONLY_CONTRACT`**，核对尚未完整覆盖的BMP/stats/armor/weapon等definition域，再推进Q05 identity/carrier/联合版本。新增FrameSounds/profile/centerz/chp/cmp及六double须进入identity，Q06 motion/resource、Q09 centerz、Q10音频回访保持。Scene旧精度差异保护；Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。

> 当前执行 `NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 / IN_PROGRESS / TEST_FIRST`；AST前置保持，现接37帧标量及27/24/40/9/geometry实际转换，先完整native typed witness与RED。Scene差异保护，identity/schema/Play后继。

> **Q05-A2 native frame AST已验证（2026-09-13）：** `NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001 / FOCUSED_TEST_PASS / VERIFIED_FRAME_AST_ONLY`。405 DAT/55348声明frame原版有序结构双跑与Unity对照、37语法夹具通过；ssnk数字键7已修且actual manager成功，当前剩余**五文件八frame**。原始RED370含336换行编码假差异，已保留并更正为34实质RED；后续旧六失败与路径分隔符断言修正留证，当前518不同测试有通过证据（517+单独1）、完整SelfCheck PASS、dotnet/CS0、旧138无新增投影差异、Ledger482/88PASS。**下一唯一入口 `NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001 / TYPED_CONVERTER_NEXT`**，先准确Record，再完整27/24/40/19/geometry/WPoint9及frame标量接线/投影；不重做AST。identity/schema/carrier/Play后继保持，Scene精度差异仍保护，Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。

> 当前执行 `NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001 / IN_PROGRESS / TEST_FIRST`；父native frame source Task继续，先修原版行边界/AST前置，再接typed converter。strength加载限定证据保持，Scene差异保护。

> **Q05-A2 native strength整表加载已验证（2026-09-13）：** `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / FOCUSED_TEST_PASS / VERIFIED_TABLE_LOAD_ONLY`；34语法夹具、正式405中10个strength定义40条原版双跑，真实Unity manager接线；RED34→189PASS+补正确namespace74PASS（45重复，共218不同测试），完整SelfCheck PASS、CS0/dotnet0error、旧138零新增投影差异、Ledger481/85PASS。Scene先前disabled已恢复，现仅UI精度差异，来源仍pending，未回退/不认证Scene unchanged。**下一唯一Task `NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：先核对native frame/FieldBag，再完整27/24/40/19/geometry接线与投影；identity/schema/carrier/Play及全部后继保持。Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。

> 当前执行 `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / IN_PROGRESS / TEST_FIRST`；现有Scene变化保护，来源确认未回，不重复询问。

> **Q05-A2 ITR40/strength19 record已写（2026-09-13）：** `NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001 / FOCUSED_TEST_PASS / TABLE_SOURCE_INTEGRATION_PENDING`保持活跃；三字段/clone/projection/hash、40/19单记录decoder已落盘。RED120→初次488/489，改用native真实FieldBag后最终489/489，完整SelfCheck PASS、CS0、旧138无新增旧投影差异。表头合同已纠正：native仅1..9/拒绝重复，caption非字段，catalog拒绝错误definition。**Scene文件12:15:20多出HUDCamera/ScenesCamera/Canvas disabled及UI坐标差异，ORIGIN_PENDING；已异步询问用户，保留未回退，isDirty=false不可当Scene unchanged。** 下一唯一Task `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；source/identity/schema/Play及其他后继保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001 / IN_PROGRESS / TEST_FIRST`；表头准入已按native更正为1..9/拒绝重复；整表和来源接线后继。

> **Q05-A2 Geometry已写/待接线（2026-09-13）：** `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_AND_ALGORITHM_INTEGRATION_PENDING`保持活跃；BDY ZWidth/HasGeometry、ITR z/有效性及copy/projection/fingerprint已落盘。native58文件88记录双跑一致，RED92→首次371/375，独立`NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`纠正4旧HolderCopy/kind7夹具后最终375/375；完整SelfCheck PASS、CS0/Scene clean/root14、旧138无新增旧投影差异。没有修候选算法或切来源，Q03旧27首差留Q06。下一唯一Task `NTSD28-Q05-ITR-STRENGTH-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint/OPoint/Geometry均在A2/Q05来源/identity/Play回访，其他后继与例外保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 关联测试修正 `NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001 / IN_PROGRESS`；Geometry新92通过，完整375中4旧HolderCopy/kind7夹具需按已实施合同修正。

> 当前执行 `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`；CPoint27/OPoint24仍SOURCE_INTEGRATION_PENDING。

> **Q05-A2 OPoint24已写/待接线（2026-09-13）：** `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING`保持活跃。Value/DTO/adapter24与native block decoder已落盘；RED49FAIL→98PASS（新49+旧6+CPoint43）、native43文件45x24、实际多转单和pool复用、完整SelfCheck PASS；CS0/Scene clean/root14。旧138加载成功，相对CPoint轮无新增投影差异；旧工具19/8投影不证明新27/24。没有切来源、修materializer或发布新版本。下一唯一Task `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint与OPoint均在A2/Q05来源/identity/Play回访，全部后继约束保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`。CPoint27仍FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING。

> **Q05-A2 CPoint27已写/待接线（2026-09-13）：** `NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING`保持活跃。DTO/value/canonical27与3float32/独立hurt/new8int已落盘，新block decoder未切manager；RED42FAIL/1PASS→实际97+13 PASS、完整SelfCheck PASS、CS0/Scene clean/root14。旧138加载成功，710处throwvz均native float32舍入，不能称数值无变；raw资源未改。CPoint新ABI尚未和外层identity/schema一起完成，禁止发布半迁移baseline。下一唯一Task `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint接线/投影/identity/Play在A2/Q05回访，旧phase/landing ULP/stage暂缓保持。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前Q05-A2执行 `NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`；先9脚本CPoint27契约，不切来源、不升版本、不部署资源。

> **Q05-A1限定交付（2026-09-13）：** `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / VERIFIED_NUMERIC_HELPER_ONLY`。RED41失败→Unity43/43，含native3622数值与raw969字段共14742扩展比较0差异；完整SelfCheck PASS、CS0/Scene clean/root14、Ledger475/56PASS，保护3059与Q04出口同差异/零缺失。旧Converter/caller/schema/正式资源未改，不宣称运行时接线。下一唯一Task `NTSD28-Q05-LOGAN-CONTENT-MODEL-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`，先准确路径Record再同Q05窗口迁移模型/入口；115候选inventory复用。Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，旧phase/landing ULP与stage暂缓保持。

> **Q05当前执行（2026-09-13）：** `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / IN_PROGRESS`，新增加载期pure decoder，旧Converter/caller/schema/正式资源尚未改。RED41失败→Unity41/41；native3622数值10866比较及raw969字段3876比较0差异。扩展两组Unity回归待重载后运行；不是Q05出口。115候选路径inventory已存父Q05工件，下一继续同窗口模型/身份/runtime迁移，不重做Q03/Q04。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> Q05窗口已启动；当前子Change `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / IN_PROGRESS / TEST_FIRST`。先纯加载期数值解码，旧caller/schema未改，不是第二个版本窗口。

> **Q04已交付，下一Q05（2026-09-13）：** Q04-B `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / VERIFIED_LEGACY_OSCILLATE_READER_ONLY`：RED8FAIL/6PASS→28/28、完整SelfCheck、实际CentralOnly Play slot0偏移/Blink/timeout/延迟速度及共享production快照/恢复PASS，CS0/Scene clean/root14/Ledger474-52PASS。SpriteRenderer前置失败保留，验证改用实际managed表现路径，不宣称GPU全域。Q04-A/B仅行为退休完成，carrier/schema仍旧。下一Q05 Task `NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001 / READY_PRECHANGE_INVENTORY`，先准确code-path/Record再同窗口迁移；旧phase/landing ULP后继保留，R13行为PARTIAL_RETURN；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前Q04-B `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`；准确Record已建立，先RED，不修改carrier/schema。

> **Q04-A已验证，下一Q04-B（2026-09-13）：** `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / VERIFIED_MASS_GATE_ONLY`。production单gate；RED6FAIL/3PASS→focused14/14、完整SelfCheck PASS、真实driver ground/landing三mass0/-2/1一致、cleanup通过、Scene dirtyfalse/root14、CS0、Ledger473/49PASS。首次Play坐标被stage钳制的失败保留，probe已取实际范围中点后通过。相关20项中1个空World旧phase断言失败保留到Q12；另实测既有landing乘1/3与native除3有一ULP首差，加入Q06/B4精确数值待办，不宣称完整landing parity。下一唯一Task `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / READY_FOR_PRECHANGE_RECORD`，先建Record/RED，只退旧reader。mass/reserved/schema保留到Q05；R13 mass行为PARTIAL_RETURN，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q04-A当前执行（2026-09-13）：** `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / PLAY_PENDING`。production仅移除CharacterMechanics mass>0条件；RED6失败/3通过，focused14/14、完整SelfCheck PASS。相关20项有1个空World旧phase[28]断言失败已保留，未改pass。首次Play摩擦正确但fixture Z200被stage min237钳制，cleanup通过；已改probe先warmup取真实stage中点，下一复验Play，不重跑已完成审计或覆盖现有Record。mass/snapshot/schema仍留Q05。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前实施Change：`NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`；只改grounded摩擦mass gate，carrier/schema保持，先RED。

> **Q03已交付，进入Q04（2026-09-13）：** `NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001 / DELIVERED_CONTRACT_ONLY`，出口证据Q03-EXIT-REPORT.md。数值见证37 native双跑稳定、Unity源链接37/37，333值113异；几何42项15同/27异。出口复核更正：旧Oscillate晚帧reader/base-shell仍在，Q04仅退reader，Q05统一删载体并base1→2；其producer退休保持。Q05版本集合12→13/20→21/23→24/character1→2/base1→2，当前版本均未改。下一唯一入口Q04-A Task `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001`，先建立准确Change Record与RED测试；随后Q04-B OSCILLATE-CONSUMER。Q03 numeric Change限定VERIFIED，Ledger472/45PASS。R13/R15仅合同PARTIAL_RETURN，Q02成果保持，正式资源未迁移；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE、BATCH-02未完成。

> Q03 closed diagnostic Change `NTSD28-Q03-NUMERIC-DECODE-WITNESS-001 / VERIFIED_SOURCE_LINKED_NUMERIC_CAPTURE_ONLY`，只添加离线native位模式见证；生产代码/版本/资源不变。

> **Q03版本/身份合同推进（2026-09-13）：** 同Q03工件 `VERSION-IDENTITY-AND-CAPTURE-CONTRACT.md` 已记录联合12→13/20→21/23→24/character1→2，其他payload保持；OPoint双owner空队列capture/restore前置（不Flush）、语义摘要确定编码、CPoint27顺序及float32 bit规范、339处reserved/alias分类和+2F8两个object-AI reader。只读合同与规范向量，尚非生产实现。下一唯一动作：native数值语法/bit witness，再逐项核对Q03-A/B完整出口；不重复Q02/几何14/42/两条factory清单。父Q03仍IN_PROGRESS，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，schema/资源/production保持。

> **Q03消费合同推进（2026-09-13）：** OPoint24字段及logic-only/renderer两条factory/initializer/PostInit/copy/reset已记录于同Q03工件 `OPOINT-AND-HELD-DEPTH-CONTRACT.md`。确认显式team与hp/mp有后处理覆盖、缺definition/无slot的整loop终止、未声明0..998零frame、held candidate取holder当前WPoint选择武器strength。WeaponStrength index+8对native index+19的新内容缺口纳入Q05同窗口；不把旧无caller ProcessAttack接成正式路径。此轮只读代码/更新合同，无新脚本/测试/资源变更。父Q03仍IN_PROGRESS；下一为canonical float、reserved/AI全reader、decode-version与Lockstep identity绑定、snapshot的OPoint队列边界。几何14/42见证已交付，不重复；Q02关闭职责保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q03几何见证已交付（2026-09-13）：** `NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001 / VERIFIED_CAPTURE_ONLY`；同DAT native14用例与真实Unity三模式42项比较，15相同/27不同，明确深度端点、BDY zwidth、ITR z、负宽度及缺失几何差异；不是parity PASS。正式indexed内容有1127个非零BDY zwidth/78个非零ITR z。报告位于同ID artifacts/diagnostics/REPORT.md；CS0、Scene dirtyfalse/root14、Ledger471/43PASS。只新增诊断工具/Editor测试，production/schema/正式资源不变。父Q03仍IN_PROGRESS，下一继续held strength有效深度、两条OPoint materializer、canonical float及联合字段全reader；新增BDY/presence合同纳入Q05同一窗口。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，Q02限定交付保持，不重做已关闭职责。

> Q03诊断Change：`NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001 / VERIFIED_CAPTURE_ONLY`，结果15同/27异已记录；production/schema/资源不变，父Q03合同未冻结。

> **Q03当前游标（2026-09-13）：** 总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02 / Q03 / NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001 / IN_PROGRESS。六正式DAT九frame已逐块分类（hir frame414为CPoint drain）；复用source-linked双端capture测得CPoint浮点截断/alias、整数准入与WPoint未知字段差异，27/9/24字段形状已捕获。+2F8独立于Spawner、mass/reserved复制/重置/快照链及BDY/ITR候选深度差异已记录；完整consumer/联合版本矩阵仍未冻结。下一继续同一Q03，先完成BDY/ITR live caller与边界证据、两条OPoint materializer、canonical float及全部schema reader矩阵；不跳到Q04/Q05。报告：artifacts/diagnostics/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001/Q03-PROGRESS-REPORT.md，联合表JOINT-FIELD-MATRIX.md。Q02限定交付保持；本轮无脚本/正式资源/schema修改，无新增Unity/Play证据。schema12/20/23与character shell1保持，Q07正式迁移未执行；以下旧READY/下一E2均为历史。

> **当前执行游标（2026-09-13）：** 总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02继续。Q02加载基础已DELIVERED / VERIFIED_LOAD_INFRASTRUCTURE_ONLY；E3 `NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001 / VERIFIED_SOURCE_CACHE_CALLER_LOAD_GATES_ONLY`：最终focused40/40、完整SelfCheck最终PASS、native direct/app/menu/menu重进4次Play（每次World4/三key一致/实际pool0/46资源零残留/两帧仍Stopped），默认旧内容App回归也PASS。CS0、NTSD_Battle dirtyfalse/root14、Ledger470/40PASS、保护3059中3047不变/12声明脚本变化/零缺失。首轮late-injection Play失败保留，BeforeSceneLoad测试clone解决注入顺序，原asset未改。下一唯一入口：docs/ai/TASKS/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001.md / READY_CONTRACT（Q03六DAT九frame及CPoint27/OPoint24/+2F8/mass/reserved联合合同）；先只读权威消费链/字段矩阵，不提前升schema或部署资源。Q07正式DAT/图片迁移及B11/B12完整验收未完成；schema12/20/23、33ms、十一阶段、Unity/GAS与非战斗行为保持。

> **Q02 E2进行中（2026-09-13最新）：** `NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / FOCUSED_TEST_PASS`只覆盖候选输入绑定与verified decoder：Unity20/20（7+13）、CS0、Scene dirtyfalse、Ledger468/25PASS。新增LoganVisualContentCandidate绑定catalog/config与sheet/head/small hash；BMPLoader在实际decode bytes上验证SHA。E2整体仍IN_PROGRESS，下一继续同一Task/Change的实际staging/停止世代/prepared object-UI/无await提交/资源重绑退休；不是新子目标或完整交付。已确认旧commit提前退休及IsPrewarmCompleted早于UI，uGUI Image保留旧Sprite引用，细节在CANDIDATE-INPUT-REPORT.md；后续脚本前扩充准确Record。global/非战斗/资源未切换，schema12/20/23保持；Q03六DAT阻塞仍在，E3/Q07后继。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE；以下旧READY/下一步以本条覆盖。

> Q02 E2 `NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / IN_PROGRESS / CANDIDATE_INPUT_BINDING`；同一Change先做定义+图片绑定和verified decode，随后继续原子提交/生命周期/UI资源绑定；首段通过不关闭E2。

> **Q02目录E1已验证（2026-09-13最新）：** `NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001 / VERIFIED_CATALOG_AND_CONFIG_CANDIDATE_GATES_ONLY`；真实source-linked native330/330与Unity目录逐项一致，Unity39/39（本包15+路径24）、CS0、Scene dirtyfalse、Ledger467/23PASS。正式config仍由6个DAT Converter失败阻止，未返回partial，不能称新内容整体可用。DefinitionFingerprint仅catalog/DAT，PNG/head/small等完整身份留E2/E3。下一 `docs/ai/TASKS/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001.md / READY_CONTRACT`；父CATALOG-PUBLICATION为E1已交付/E2原子发布/E3缓存caller未做。Q03字段合同可独立准备；Q02/BATCH-02/总目标ACTIVE，FULL_ALIGNMENT_INCOMPLETE。global/非战斗/正式资源未切换，schema12/20/23保持；以下旧下一步以本条覆盖。

> Q02目录事务E1 `NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001 / IN_PROGRESS / TEST_FIRST`；先native catalog与全量候选，任何转换失败禁止partial返回；E2原子发布/E3缓存caller后继，global/菜单未切源。

> **Q02 PNG alpha已验证（2026-09-13最新）：** `NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001 / VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY`；Unity21/21，本包6+PNG13+Overlap2。隔离/正式nar实际sheet staging→atlas pixels→SpriteCatalog→现有shader共10个GPU样点通过（Direct3D11/URP/Gamma），CS0，Scene dirtyfalse，Ledger466/19PASS。旧UI/BMP/processor/shader保持，资源未迁移，schema12/20/23不变。下一 `docs/ai/TASKS/NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001.md / READY_CONTRACT`，从Q02-A已确认共享caller冻结目录/source/cache/publication事务；Q03字段合同也可准备。R17 raw/range/alpha子条件PARTIAL_RETURN；Q02/BATCH-02及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。未验全局prewarm/正式迁移/全场Play和所有绘制路径，以下旧下一步由本条覆盖。

> Q02 PNG alpha子包 `NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001 / IN_PROGRESS / TEST_FIRST` 已启动；准确范围为BMPLoader格式metadata、manager明确source sheet入口及测试；旧UI/BMP/全局source不变，最终验证待执行。

> **Q02-C已验证（2026-09-13最新）：** `NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001 / VERIFIED_SOURCE_RANGE_ADMISSION_ONLY`；真实Unity14/14，405DAT/773sheet原始声明/有效范围/尺寸/路径与native capture一致，CS0，Scene dirtyfalse，Ledger465/18PASS。新增Logan配对parser/builder；旧入口和字段形状/schema12/20/23保持。3059保护中仅本包parser/manager及前包BMPLoader声明变化，零缺失。下一 `docs/ai/TASKS/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001.md / READY_CONTRACT`，再做catalog/cache/publication；Q03也可准备。Q02/BATCH-02与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移、未验整场Play/GPU；R17 raw+range仅PARTIAL_RETURN。以下旧下一步以本条为准。

> Q02-C已启动：NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001 / IN_PROGRESS / TEST_FIRST；新增明确Logan源parser/builder，AST保留declared、现有内容字段消费native effective。旧入口/非战斗/12-20-23版本保持；正式源/cache/publication未切换。

> **Q02-B已验证：** NTSD28-B11-PNG-WORKER-DECODE-001 / VERIFIED_RAW_WORKER_DECODE_ONLY；正式1255/1255尺寸/RGBA hash匹配、Unity13/13及最大17.5MP worker补测1/1，BMP/mainthread保持，CS0/Scene dirtyfalse。原3059文件仅BMPLoader声明变更，正式PNG未改。下一Q02-C `docs/ai/TASKS/NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001.md`；另新增P-21/PNG-SHEET-ALPHA-CONTRACT-001，现有sheet去黑处理会丢PNG半透明，必须独立解决。R17 raw输入/解码PARTIAL_RETURN；Q02/range/alpha/catalog/cache/publication/迁移/GPU未闭合，总目标ACTIVE。

> Q02限定子包已验证：`NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001 / VERIFIED_PURE_PATH_ONLY`；源链接24/24与真实Unity24/24、CS错误0、3059既有文件hash不变、Scene dirtyfalse。Q02-A审计已交付：catalog.csv/registry_index才是对象目录权威；data.txt不能替代；25文件仍有条件性sprite range集合差异。BATCH-02/Q02仍IN_PROGRESS；下一`docs/ai/TASKS/NTSD28-B11-PNG-WORKER-DECODE-001.md / READY`（已测量1255PNG的palette1/2/4/8与RGBA8格式）。既有全局cache、菜单和加载caller未切源；PNG、range与catalog/cache/publication未闭合，不关闭Q02或总目标。

> **总目标活动中，第一批已交付（2026-09-13）：** `BATCH-01 / Q01 / NTSD28-B11-CONTENT-ENTRY-INVENTORY-001 / DELIVERED / VERIFIED_OFFLINE_AUDIT_ONLY`。正式330对象/24背景，405新版DAT与138旧DAT实际双端捕获，输入/源码/header身份通过；6文件9帧Converter拒绝及root/PNG/字段缺口已登记。报告：`artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/Q01-REPORT.md`。R15仅Q01身份子条件PARTIAL_RETURN；Q05/Q07条件保留。下一`BATCH-02 / Q02-A / docs/ai/TASKS/NTSD28-B11-SOURCE-ROOT-AND-CACHE-CONTRACT-AUDIT-001.md / READY`，从source/cache合同审计开始；Q03也具备准备前置。3059文件保护hash不变，production/非战斗/资源/Scene未修改，未运行Unity/Play。总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE；下方旧HOLD/准备态和旧下一包均为历史，不覆盖当前游标。


> **2026-09-13 Foot Marker 六帧动画代码完成、Unity验收待连接：** `BATTLE-CENTRAL-FOOT-MARKER-ANIMATION-001 / COMPILE_PASS / STATIC_ANIMATION_CONTRACT_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`。用户重导出的frame_01～06均为128×48、Point/no-mip且bbox一致；GameConfig已按序绑定6×80ms，draw按unscaled presentation time换texture，全部Self同步并保持单Foot draw。runtime/editor build均0 error、静态合同PASS、validator 461/8 PASS；Unity Pipeline无实例，focused/Play未运行。用户PNG/meta未修改。


> **2026-09-13 Foot Marker 已改走 GameConfig：** `BATTLE-CENTRAL-FOOT-MARKER-GAMECONFIG-001 / COMPILE_PASS / STATIC_CONTRACT_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`。生产脚本固定路径和 Scene 独立 Sprite 字段已移除；静态 fallback 最初绑定旧 FootSelf，后由 `BATTLE-CENTRAL-FOOT-MARKER-ANIMATION-001` 改绑 frame_01。runtime/editor 外部编译均0 error、静态合同PASS；focused/Play待连接。


> **当前任务视图（2026-09-12 已逐项修订）：** `NTSD28-ALIGNMENT-REPLAN-20260912 / CURRENT_MATRIX_RECONCILED / DOCUMENTATION_ONLY / FULL_ALIGNMENT_INCOMPLETE`。
> 用户已通过 D-023 明确 DAT/角色相关图片采用 NTSD 2.8-Logan，迁移未执行；不再等待该内容方向，也未撤销其他例外。
> **总目标＋六批次已准备，禁止执行（2026-09-13最新用户要求）：** `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / PREPARED_NOT_STARTED / EXECUTION_USER_HOLD`。BATCH-01～06均未启动；当前 `PREPARED_BATCH=BATCH-01 / NEXT=Q01 / ACTIVE=NONE / READY_RETURNS=NONE`，Q01为PREPARED_HOLD。
> 恢复先读 `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` 第0.14节总目标/批次/启动状态，再读0.11队列、0.12回访、0.13游标；仅准备，不启动Q01审计、goal、线程、自动化或Unity验证。用户明确启动某批才做该批；明确启动总目标持续执行才按批推进。
> 硬边界：保持Unity/GAS框架与非战斗行为；只改经声明的战斗逻辑及必要适配和D-023资源。共用脚本若无法避免影响非战斗功能，该部分先停并说明，未经用户明确扩大范围不得修改。保护已有Foot Marker等用户工作。
> B1当前生产职责完成，B2基础与路由职责关闭；B3/B5仅出口放行、整域后置，B4/B6部分子集完成，B7～B12未完全完成。B1/B2已关闭职责移出实施队列；下游依赖/最终验收不撤销既有成果。
> 确认脚本缺口包括 CPoint27/alias、OPoint24对Unity8、资源根/后台PNG、+2F8语义、mass/联合schema、结果分类/时点、同Z排序/插值和表现音频consumer。
> Q子包完成后立即检查R回访：B4复活等待B7 producer/新版内容，B5资源KO等待CPoint/B8，B3残余等待B7/B8接管，B1 worker等待B9启用条件。已满足触发的回访优先；Q11只核对是否漏做，不能全部拖到B12。
> 数据定义/旧行为退休→一次联合schema窗口→生产接线→内容可用→视听→最终集成；schema不等待整场Play，资源载入不等待最终视听，避免循环依赖。本次仅整理文档，无资源删除或运行时修改，未新增编译/测试/Play证据。
> D-022路线已决定但当前runtime/snapshot/checksum仍12/20/23；正式新内容、整场trace、物理技能键和视听验收尚未完成。

> **Goal17三个限定退休包已验证（2026-09-10）：** `NTSD28-B6-CPOINT-KIND2-HURT-CONSUMER-RETIREMENT-PRODUCTION-001 / VERIFIED`, `NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-RETIREMENT-PRODUCTION-001 / VERIFIED`, `NTSD28-B9-NTSDSPEC-OSCILLATE-PRODUCER-RETIREMENT-PRODUCTION-001 / VERIFIED`。focused13/4/10全PASS，一次共享B6610/610含全部指定前置，refill9/9、旧converter/HitPlan7/7、freshSelfCheckPASS、双build0error；Scene固定SHA/dirtyfalse，schema/内容/NTSDSpec本体/P3 Record均保持。包1不实现drain/recovery，包2无自然Play caller，包3保留Oscillate晚帧consumer/恢复，不声明native B9对齐。报告后等待复核，以下启动记录为过程历史。

> **Goal17已获追加授权恢复：** 三包 `NTSD28-B6-CPOINT-KIND2-HURT-CONSUMER-RETIREMENT-PRODUCTION-001`, `NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-RETIREMENT-PRODUCTION-001`, `NTSD28-B9-NTSDSPEC-OSCILLATE-PRODUCER-RETIREMENT-PRODUCTION-001` 均IN_PROGRESS/TEST_FIRST；包1引用复核方完整正向证明，限定旧测试范围已批准，drain/recovery延后；包2/3先全量名称认证。P3 Record不再修改。以下事前暂停为已解除历史。

> **2026-09-10三退休包事前暂停：** 包1`NTSD28-B6-CPOINT-KIND2-HURT-CONSUMER-RETIREMENT-PRODUCTION-001 / BLOCKED`；包2`NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-RETIREMENT-PRODUCTION-001 / PLANNED`；包3`NTSD28-B9-NTSDSPEC-OSCILLATE-PRODUCER-RETIREMENT-PRODUCTION-001 / PLANNED`。现有SelfCheck/HitPlan仍要求hurt覆盖且旧converter测试直接调用拟删helper，相关旧测试未在本轮授权文件清单内。详见Temp/Goal17_PrechangeScopeReview.md，脚本改动0，新RED/共享验收未执行。P3本轮已获用户复核确认，当前VERIFIED；此前REVIEW_HOLD已解除。

> **Goal16 P3限定子集已验证（2026-09-10）：** `NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001 / VERIFIED / REVIEW_HOLD`。RED126失败/14控制→focused140PASS；B6 583/583（含所有指定前置）、refill9/9、旧pickup2/2、fresh SelfCheckPASS、双build0error。两scoped Play tick6/8与当前playable C++源码同seed/input/tick比较6记录117字段无首差；该证据是Direction-B夹具源码执行，不是正式EXE完整应用parity。8个旧期望站点逐项留痕。Scene固定SHA/dirtyfalse、37保护哈希不变；P2/schema/Attack映射/old-child cleanup无改。报告后等复核，不启动后继；以下启动/阻塞/恢复条目为过程历史。

> **Goal16 P3已获追加授权并恢复：** NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST_RESUMED。用户批准两处R-HC-04期望及授权文件内四类P2/Authority直接矛盾的旧期望修订（counter/HolderCopy/count/kind7），逐站点登记；先新RED再生产。下方BLOCKED为已解除的过程历史，类外/P2缺陷仍硬停。

> **Goal16 P3事前范围阻塞：** NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001 / BLOCKED / PRODUCTION_UNCHANGED_FROM_GOAL15。SelfCheck14770仍要求unsupported type3保留counter7、14859仍要求HolderCopy镜像；与P2/用户目标冲突且位于明确授权修订行之外。两处最小提案见Temp/Goal16_ProposedAdditionalSelfCheckExpectations.diff，尚未应用。P3 RED/生产/Play/共享回归未执行；需用户追加这两处旧期望范围再恢复，以下启动条目为过程历史。

> **Goal16 P3启动：** NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST。用户已批准本包覆盖Goal15等待复核状态；只消费既有P2计划，kind7副作用退休，state2004替换旧child保留，schema/Attack映射不改；独立Task/Record已先建。

> **Goal15限定子集已验证（2026-09-10）：** NTSD28-B6-KIND2-PICKUP-RELATION-COUNT-SYSTEM-RULES-PRODUCTION-001 / VERIFIED（rules/relation-count）；NTSD28-B6-KIND2-PICKUP-PURE-TRANSACTION-PRODUCTION-001 / VERIFIED（pure plan）。P1绑定+35C已证明，RED16失败/7控制→23PASS；P2骨架RED32执行、失败明细20条截断→32PASS。共享B6 443/443、refill9/9、fresh SelfCheck PASS、双build0error；34个schema文件哈希不变，Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11/dirtyfalse。PLAY_NOT_PERFORMED_RULES_PURE_LAYER；P3_NOT_CONNECTED，完整pickup仍未关闭。D-022只登记路线图，本Goal无schema变更。报告后等待用户复核；以下启动条目为过程历史。

> **P2启动：** NTSD28-B6-KIND2-PICKUP-PURE-TRANSACTION-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST；P1 NTSD28-B6-KIND2-PICKUP-RELATION-COUNT-SYSTEM-RULES-PRODUCTION-001已23/23 PASS。只新增pure有序计划，P3未接线、schema不改，最后共享回归。

> **P1启动：** NTSD28-B6-KIND2-PICKUP-RELATION-COUNT-SYSTEM-RULES-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST / PICKUPCOUNT_BINDING_PROVEN。P2纯计划后继，P3未授权；遵循D-022但本Goal不改schema。

> **2026-09-10用户裁定 D-022：** 行为退休→一次提升→接线，entity12→13/aggregate20→21/checksum23→24及必要shell升版，旧midbattle严格拒绝按seed/input重放，无adapter；首批system规则locked immutable不支持alternate。reserved只过渡，历史checksum/parity按版本分组，CPoint19→27内容合同独立。本Goal不触碰schema文件，先P1/P2，P3待复核。

> **Goal13b已验证三个限定子集：** NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001、NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001（依赖guard）、NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001均VERIFIED。guard RED6/11→17PASS，missing72复跑PASS且Sakura/Sakon双Play两轮均Free0，DVX RED8/16→24PASS且200→199→拾取199→投掷199。共享一次B6 388/388（含17/72/24/92/80），refill9/9，fresh SelfCheckPASS，双build0error，SceneSHA不变。最终SelfCheck Console保留7条负向夹具日志+8条MinMaxAABB引擎断言，根因未排查，不声明Console0。详见三Record；报告后GOAL14_USER_HOLD，不自动推进+2F8/schema。

> **Goal13b包2启动：** NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST。守卫NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001已有17/17及双Play；包1NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001已依赖守卫升级VERIFIED（72复跑+第二轮双Play）；批次最终共享回归待包2完成。

> **Goal13b：** NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST。用户已授权C25限定guard，先守卫/双Play与72收口，再包2，共享最终回归；原Goal13硬停止由此限定解除。

> **Goal13硬停止：** NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001 / BLOCKED / FOCUSED72_PASS / PLAY_SAKURA_PASS_SAKON_LATE_FREE_FAIL。Sakon-888在C09/C20保持但后续C25 Free1；修复点BattleLateEntityLifecycleModule不在清单。包2未启动、共享回归未运行，不标VERIFIED、不自动扩包；下一步等用户复核。详见同ID Record。

> **Goal13包1：** NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST；仅missing-action精确子集；包2尚未启动，最后共享回归。用户当前授权覆盖此前Goal13 HOLD。

> **Goal12 Part A（2026-09-10）：** NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001 / VERIFIED_TERMINAL_SUBSET / RED_COMPLETED80_FAILED_CAPPED25 / FOCUSED80_PASS / B6_275_PASS / KIND3_REFILL101_PASS / LIFECYCLE_STRUCTURAL35_PASS / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / ROCKLEE254_SCOPED_PLAY_PASS / CONSOLE0 / SCENE_UNCHANGED / LEDGER440_379。tick6/C09 Free/childUnregister/generationRelease各1、terminal RNG/sound0，World4→4及两个pool2→2。详见Record；Part B结果仅最终回复，GOAL13_USER_HOLD。

> **Goal11 kind3 release包（2026-09-10）：** NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001 / VERIFIED_KIND3_SUBSET / RED_COMPLETED92_FAILED_CAPPED25 / FOCUSED92_PASS / B6_195_PASS / REFILL9_PASS / RNG_RELATIONS41_PASS / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCOPED_ROCKLEE255_PLAY_PASS / CONSOLE0 / SCENE_UNCHANGED / LEDGER439_378。tick12同步原值0,5,1,0，left-facing final motion100,-1,-2；owned cleanup4→4、roster恢复。Play为current DAT pickup consumer与standing同tick native chord，详见Record。后置族不变，GOAL12_USER_HOLD。

> **Goal9三timer消费包（2026-09-09）：** NTSD28-B5-RECOVERY-STATUS-CONSUMERS-NO-STATS-001 / VERIFIED / NO_STATS_THREE_CONSUMERS_ONLY / FOCUSED_69_OF_69 / B5_846_OF_846 / NTSD28_316_OF_316 / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_UNCHANGED / PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER / GOAL10_USER_HOLD。恢复先于C25h递减，render_phase已建模；shared writer仅消费weak/HP-double/MP-bonus。69项RED至少25实测失败（MCP capped）→69/69 GREEN，B5原777+69与NTSD28原247+69全绿，full SelfCheck14:36:14Z PASS，Scene不变。bonus两timer无自然producer，按许可Editor强制载体验收；完整证据见同ID Record。其余C25c-e/B11/B8继续后置，停止等待Goal10。

> **Goal8限定边界包（2026-09-09）：** NTSD28-PP-RECOVERY-LOW-THRESHOLD-INCLUSIVE-BOUNDARY-001 / VERIFIED / PP150_BOUNDARY_ONLY / RED_3_FAIL_15_PASS / BOUNDARY_18_OF_18 / B5_777_OF_777 / NTSD28_247_OF_247 / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_UNCHANGED / PLAY_NOT_PERFORMED_BOUNDARY_ONLY / GOAL9_USER_HOLD。仅两处>=150改>150与18组新增测试；RED3 fail/15 pass→GREEN18/18，B5原759+18及NTSD28原229+18全部通过，fresh full SelfCheck13:48:37Z PASS。指定Scene不变，完整证据见同ID Record；只关闭等号边界，C25c-e其余算法继续后置，完成停止等待Goal9。

> **Goal7 PartA限定包（2026-09-09）：** NTSD28-RESPAWN-STALE-INTEGER-FIXTURE-READ-CORRECTION-001 / VERIFIED / TEST_FIXTURE_ONLY / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_UNCHANGED / GOAL8_USER_HOLD。仅respawn stale-int的raw整数断言及消息；fresh full SelfCheck于13:24:41Z PASS，目标全部断言（含后续全局同步guard）通过，无下一停点；两套build0 error，指定Scene SHA不变。完整验收见同ID Record。PartB零文件改动，结论仅报告；其他任务继续USER_HOLD，停止等待Goal8。

> **Goal6 PartA限定包（2026-09-09）：** NTSD28-UNIFIED-AI-SNAPSHOT-REFRESH-EXPECTATION-FIXTURE-CORRECTION-001 / VERIFIED / TEST_FIXTURE_ONLY / SINGLE_1_OF_1 / STRESS_280_OF_280 / CORE_STRESS_256_OF_256 / BUILDS_0_ERROR / SCENE_UNCHANGED / GOAL7_USER_HOLD。仅具名RefreshCount期望1→2及两阶段注释；单测一次1/1、ProductionEntityStress完整组一次280/280（main256/timing17/capacity7），两套build0 error。完整证据见同ID Task/Record。其余工作保持USER_HOLD；PartB零文件改动，结论仅在报告；完成等待Goal7。

> **Goal5限定双包完成（2026-09-09）：** NTSD28-R4-HIT02A-FLUTE-STAT-SENTINEL-FIXTURE-CORRECTION-001 / VERIFIED / TEST_FIXTURE_ONLY / FOUR_CASES_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_LATER_RESPAWN_STALE_INT / GOAL6_USER_HOLD；NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001 / VERIFIED / EVIDENCE_CAPTURE_ONLY / SINGLE_RUN_FAILED_REFRESH_1_VS_2 / ASSERTIONS_UNCHANGED / GOAL6_USER_HOLD。A四组通过后停在既有respawn stale-int，不修；B单跑取证1/1/1/2/1/1050，首失败RefreshCount原期望1/实际2，断言未改。两套build0 error，指定Scene SHA不变。各自Task/Record保留完整证据。停止等待Goal6，production及其余B6继续USER_HOLD。

> **2026-09-09 Goal3 GT06限定夹具修正已验证：** `NTSD28-GT06-RECOVERY-PP-EXPECTATION-FIXTURE-CORRECTION-001 / VERIFIED / TEST_FIXTURE_ONLY / GT06_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_LATER_R4_HIT_02A / SCENE_UNCHANGED / GOAL4_USER_HOLD`。只改PP20→4一处期望，字节SHA证明其他内容未变。fresh SelfCheck12:04:37Z越过GT06，停在既有后续`CheckKind10And11CharacterStatsWithoutDamage / R4-HIT-02A`，只记录不修；build47/129 warnings、均0 error，指定Scene SHA不变。证据见同ID Task/Record。本包不代表全SelfCheck通过；停止等待用户复核，B6剩余族及任何production修改继续暂停。

> **2026-09-09 Goal 2限定验收完成：** `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001 / VERIFIED / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / NTSD28_CATEGORY_229_OF_229 / B5_NAME_GROUP_759_OF_759 / TARGETED_PLAY_6_OF_6 / REFILL_SAMPLES_26 / EXHAUSTION_EVENTS_6 / BUILDS_0_ERROR / CONSOLE_0_ERROR / SCENE_UNCHANGED / SELFCHECK_BLOCKED_UNRELATED_GT06 / HP_BASEMAX_DEFERRED`。本轮只新增test-only Play探针，未改production；正式DAT Sakura/Naruto在NTSD_Battle生产driver tick5→20获得精确消费、exhaustion与清理见证。环境random-drop OID150/slot52的来源已由allocate调用栈确认。退出Scene SHA为用户指定`D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`，fresh SelfCheck11:36:59Z仍停独立GT06。完整证据见该包Task/Record；不得扩大为HP/baseMax、完整OPoint materializer或整个B6完成。**到此停止等待用户复核；Goal3、stress甄别、B6剩余族及任何production修改继续USER_HOLD，总目标仍FULL_ALIGNMENT_INCOMPLETE。** 下方旧暂停/PLAY_PENDING记录保留为历史，以上述具名包最新证据为准。

> **2026-09-09 用户暂停总体对齐目标：** `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / USER_HOLD /
> FULL_ALIGNMENT_INCOMPLETE`。用户将把当前情况交给GLM核验现有进度和已处理脚本，再整理遗漏与未处理项；
> 禁止从头重做已有成果。暂停期间不启动新包、不继续Play、
> 不修SelfCheck/stress独立失败、不修改production/content/Scene。面向GLM的新会话提示词为
> `docs/ai/GLM-INCREMENTAL-CONTINUATION-PROMPT-2026-09-09.md`，详细交接为
> `docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`。

> **2026-09-09 B6 positive-link validation已退休：** `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 / STRESS_255_OF_256_1_UNRELATED_AI_REPORT / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / CONSOLE_0_ERROR / SCENE_UNCHANGED / PHASE_33 / NO_POSITIVE_EVENT`。phase 34→33、World/pass/stress/U6旧owner与positive structural event均已退出；compat entry为obsolete 0B no-op。真实Play确认三步post-catch与positive mismatch preserve，lifecycle仍原子cleanup。full SelfCheck已越过旧阻塞并停在独立R2 exact-catch旧夹具；下一先做B6剩余入口/exit只读复核。

> **2026-09-09 B6 held injury caughtact event已验证：** `NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001 / VERIFIED / RED_5_FAIL_10_PASS_OF_15 / FOCUSED_15_OF_15 / B6_CATEGORY_97_OF_97 / NTSD28_223_OF_223 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_428_364 / POST_SETTLEMENT_EVENT_EXACT`。真实applied正injury事件现按slot收集，并在完整settlement后消费；Play证实count0→1且未重复。下一strict首差为positive-link validation retirement。

> **2026-09-09 B6 held injury caughtact event启动记录（已由上条VERIFIED关闭）：** 本包曾以`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`启动；精确RED与最终证据见上条及Change Record。

> **2026-09-09 B6 held injury accounting/cover已验证：** `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001 / VERIFIED / RED_14_FAIL_3_PASS_OF_17 / FOCUSED_17_OF_17 / B6_CATEGORY_82_OF_82 / NTSD28_208_OF_208 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / RELATED_FIXTURES_9_OF_9 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_427_363 / CANONICAL_ACCOUNTING_COVER_EXACT`。held injury现写IncomingDamageScale340、direct owner/type0 self、canonical consumed/score/KO与cover排除timer，legacy统计保持；真实Battle grab通过。下一caughtact event，MP/world KO feed后置。

> **2026-09-09 B6 settlement vaction preflight已验证：** `NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_4_PASS_OF_8 / FOCUSED_8_OF_8 / B6_CATEGORY_65_OF_65 / NTSD28_191_OF_191 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / TARGETED_PLAY_8_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_426_362 / ACTUAL_PREFLIGHT_EXACT`。hurtable分支现先提交signed/zero vaction，再重验新frame首kind2 CPoint；三类invalid均terminal且不再执行settlement tail。下一严格包处理held injury exact accounting。

> **2026-09-09 B6 mixed catch advance/exact consumer已验证：** `NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001 / VERIFIED / RED_1_PASS_7_FAIL_OF_8 / FOCUSED_16_OF_16 / PREINTERACTION_15_OF_15 / B6_CATEGORY_57_OF_57 / HITPLAN_185_OF_185 / NTSD28_183_OF_183 / TARGETED_PLAY_16_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_425_361 / SINGLE_MIXED_ADVANCE_EXACT_CONSUMERS`。pipeline现为slot升序单一mixed advance加独立settlement，三个consumer读plain exact +0x90，mismatch/negative release均terminal；下一严格包为settlement vaction preflight，再处理held accounting。

> **2026-09-09 B6 catch relation exact-field production已验证：** `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001 / VERIFIED / RED_0_OF_3 / FOCUSED_19_OF_19 / B6_CATEGORY_41_OF_41 / HITPLAN_185_OF_185 / NTSD28_167_OF_167 / TARGETED_PLAY_19_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / EXACT_RELATION_ATOMIC`。kind3 actual/HitPlan现共享first/signed、双frame preflight、exact+compat与respond规则；current criminal witness已绿，kind1保持。下一mixed catch fences。

> **2026-09-09 B6 invalid negative-held reciprocal preserve已验证：** `NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001 / VERIFIED / RED_2_OF_2 / FOCUSED_7_OF_7 / B6_CATEGORY_22_OF_22 / RELATED_47_OF_47 / BROAD_56_OF_62_6_UNRELATED_NATIVE_INPUT_PROXY / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / DIAGNOSTIC_PRESERVE`。C09/C20 invalid child现保留关系并输出counter/optional trace；前置cleanup防正常ABA。下一进入catch exact-field producer，positive-link/settlement/accounting仍后置。

> **2026-09-09 B6 entity-link lifecycle cleanup已验证：** `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED / FOCUSED_7_OF_7 / B6_CATEGORY_15_OF_15 / RELATED_91_OF_91 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / ATOMIC_RELEASE_CLEANUP / RED_NOT_EXECUTED`。release-success后、generation release/reuse前已由single writer清held/catch exact+compat关系并阻断ABA；P7旧夹具已纠正。下一先做invalid reciprocal preserve，再按既定顺序进入catch/mixed-fence/vaction/accounting。

> **2026-09-09 B6 CPoint throw精确子集已验证：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / VERIFIED / FOCUSED_8_OF_8 / RELATED_17_OF_17 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_CURRENT_BASELINE_UNCHANGED / FULL_RESOURCE_DEFERRED / ILLEGAL_CATEGORY_RETIRED`。真实Unity focused/related与抓取Play探针均通过；旧raw-throw clear-Vz夹具和非法category已纠正。full SelfCheck的新首差为held-CPoint injury legacy accounting；完整throw MP resource仍后置B7/B8/B11/H。

> **2026-09-09 B5 negative environment shared recovery已验证：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001 / VERIFIED / RED_11_OF_12 / FOCUSED_12_OF_12 / RELATED_154_OF_154 / RELATED_B5_981_OF_981 / TARGETED_PLAY_12_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SINGLE_EXACT_TRANSACTION / FLUTE_FALSE_POSITIVE_RETIRED`。Legacy/DataOriented/derived入口已共享EnvironmentState/native phase/rule/scale/two-hop exact writer，并按纠正合同post-accounting clamp0；当时的CPoint throw Vz阻塞已由后继B6包关闭，fresh full SelfCheck当前推进到独立held injury accounting。B6 producer/B8 event/schema不混入。

> **2026-09-09 negative environment clamp合同已纠正：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SOURCE_HASH_MATCH / CLAMP_TO_ZERO_REQUIRED / PRIOR_NO_CLAMP_CLAUSE_SUPERSEDED`。无漂移playable source与tests明确在exact accounting后clamp HP/HPBound到0；前置audit仅no-clamp一句作废。下一single consumer必须覆盖overkill。

> **2026-09-09 B5 negative environment rule carrier已验证：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001 / VERIFIED / RED_13 / FOCUSED_6_OF_6 / RELATED_106_OF_106 / NTSD28_1394_OF_1394 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_BASELINE_UNCERTIFIED / CARRIER_READY / RECOVERY_CONSUMER_NEXT`。raw carrier与schema `11/20/23`闭合；RED13、focused6、related106、exact NTSD28 broad1394、builds0、Ledger绿。SelfCheck仍为既有CPoint阻塞；Scene基线未认证且用户改动保持。下一single exact recovery consumer。

> **2026-09-09 B5 negative environment recovery owner审计已闭合：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WEAPONCOUNT_WRONG_CARRIER / EXACT_TRANSACTION_REQUIRED / TWO_PACKAGE_ROUTE`。两套WeaponCount分支与Authority不符且flute可达；下一data-first补world rule90 carrier，再接single exact consumer。

> **2026-09-09 B5 input HP-cost shared transaction已验证：** `NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001 / VERIFIED / RED_0_OF_6 / FOCUSED_6_OF_6 / RELATED_INPUT_187_OF_187 / RELATED_B5_926_OF_926 / TARGETED_PLAY_6_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / TWO_INPUT_WRITERS_RETIRED / NEGATIVE_RECOVERY_AUDIT_NEXT`。三个generic action入口已共享exact core且两处ComboVic writer归零；下一只读审计negative recovery，CPoint/schema仍不动。

> **2026-09-09 B5 input HP-cost compatibility stats审计已闭合：** `NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / COMPAT_PATH_LIVE / PARTIAL_RETIREMENT_UNSAFE / SHARED_TRANSACTION_PRODUCTION_DEFINED`。LegacyCanonical仍是可配置production；旧helper缺完整native transaction，不能只把ComboVic改写到exact。下一独立production复用`ApplyNativeInputAction`单一core。

> **2026-09-09 B5 type3/weapon/flute legacy stats安全退休已验证：** `NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / FOCUSED_4_OF_4 / RELATED_B5_777_OF_777 / TARGETED_PLAY_4_CASES / LIVE_COLLISION_MATRIX_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SCOPED_LEGACY_STATS_RETIRED / INPUT_COMPAT_AUDIT_NEXT`。三族actual/HitPlan extra stats已归零，exact HP/KO/+0x2F4保持；下一只读审计input HP cost compat，其他stats/schema仍不动。

> **2026-09-09 B5 standard/reduced exact KO producer已验证：** `NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED / RED_1_OF_4 / FOCUSED_4_OF_4 / RELATED_270_OF_270 / TARGETED_PLAY_4_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_KO_PRODUCER_READY / TYPE3_WEAPON_FLUTE_STATS_NEXT`。direct/two-owner/redirected与拒绝矩阵闭合，actual/HitPlan只写一次KO；full SelfCheck独立CPoint阻塞。下一安全legacy stats retirement。

> **2026-09-09 B5 legacy stats retirement readiness已闭合：** `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / GLOBAL_RETIREMENT_BLOCKED / FOUR_PREREQUISITE_ROUTES`。13/8/3/7/3处legacy mutation中，standard/reduced缺KO、held缺全套exact、input/recovery各有前置；type3/weapon/flute可独立退休。下一`STANDARD-REDUCED-KNOCKOUT-PRODUCER-001`。

> **2026-09-09 B5 OrdinaryCreditGate2F4 producer/consumer纠正已验证：** `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_287_OF_287 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_2F4_READERS_PRODUCERS / LEGACY_STATS_WRITER_NEXT`。15处scoped误绑定归零，type0 fallback读parent physical slot且non-type0保持-1；full SelfCheck仍为既有CPoint阻塞。下一route6退休legacy stats writer。

> **2026-09-09 B5 type3 HolderCopy extra-writer退休已验证：** `NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_3 / FOCUSED_4_OF_4 / RELATED_284_OF_284 / TARGETED_PLAY_3_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / FOUR_TYPE3_WRITERS_RETIRED / TYPE3_SPECIFIC_FAMILY_EXIT_READY / ORDINARY_CREDIT_GATE_2F4_NEXT`。actual1+HitPlan3处extra write已退休，target99/source77保持且type3核心事务不变；full SelfCheck仍为既有CPoint阻塞。下一严格route为B5 `OrdinaryCreditGate2F4` producer/consumer correction，stats/schema后置。

> **2026-09-09 B5 kind5 linked-parent binding已验证：** `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED / TYPE3_WRITER_NEXT`。frozen pair/kind5/negative-link现只读HolderStableId/implicit-zero；HolderCopy sentinel不变。下一严格route为type3额外HolderCopy writer退休，stats/schema后置。

> **2026-09-09 B4 revival exit已验证：** `NTSD28-B4-REVIVAL-EXIT-AUDIT-001 / VERIFIED / REVIVAL_TRACE_EQUAL_13_RECORDS_416_FIELDS / DOUBLE_RUN_BYTE_STABLE / RELATED_54_OF_54 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_REVIVAL_EXIT_READY / PRODUCTION_UNCHANGED / B7_H_PRODUCER_PENDING`。Authority/Unity trace双跑稳定且13/416 first difference空；真实Play/Console/Scene绿。full SelfCheck仍被既有CPoint阻塞；B7/H queued producer/schema继续后置，下一严格route返回B5 HolderCopy binding/extra-write corrections。

> **2026-09-09 B4 normal floor/RNG已验证：** `NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001 / VERIFIED / RED_0_OF_7 / FOCUSED_7_OF_7 / RELATED_59_OF_59 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXIT_AUDIT_NEXT`。effective floor、type0/group peer、sumX gate、sync RNG0x90/0x91、precise-only X/Z与exact vitals tail已闭合；下一B4 joint exit audit。

> **2026-09-09 B4 queued continuation已验证：** `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001 / VERIFIED / RED_4_OF_11 / FOCUSED_13_OF_13 / RELATED_47_OF_47 / TARGETED_PLAY_13_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / NORMAL_ROUTE_NEXT / PRODUCER_B7_H_PENDING / SCHEMA_DEFERRED`。+0x360 active/missing/-1 fallback、queued fields、controller group、explicit/default visual、action219/counter0/hold10与OID998调用前状态已闭合；下一normal floor/RNG。

> **2026-09-09 B4 revival participant gate与branch ownership已验证：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001 / VERIFIED / RED_10_OF_16 / FOCUSED_16_OF_16 / RELATED_34_OF_34 / TARGETED_PLAY_20_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / QUEUED_ROUTE_NEXT`。两个legacy HitStun arm与C07 KillCount/team gate已退休，lives-first queued/primary-retain/transient-free/normal分支已恢复；下一queued continuation，normal floor/RNG与exit后置。

> **2026-09-09 B0 direct revival默认producer已验证：** `NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001 / VERIFIED / RED_1_OF_3 / DIRECT_DEFAULTS_1_0_0 / FOCUSED_3_OF_3 / DIRECT_OWNER_REGRESSION_15_OF_15 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_RUNTIME_RESUMABLE`。valid slot0/19在首次注册前与active snapshot均为1/0/0，raw backing仍0/0/0，invalid不写；focused3/3、owner15/15、Play绿。B4 gate恢复。

> **2026-09-09 B4 revival owner审计已验证：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KILLCOUNT_NOT_REVIVAL_AUTHORITY / THREE_ENTRY_POINTS_SPLIT / DIRECT_DEFAULT_PRODUCER_MISSING / FOUR_RUNTIME_ROUTES_DEFINED / PRODUCTION_HELD`。Authority C25/C07无KillCount；Unity三旧gate、wrong branch、primary free与queued/floor/RNG后继已拆分。Direction-B state14=235；先闭合B0 direct默认producer。

> **2026-09-09 B3 1100..1299 child propagation退休已验证：** `NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / PRODUCTION_CHILD_SCAN_REMOVED / FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELF_RESET_PRESERVED / CURRENT_ITACHI_1250_COVERED / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。RED显示matched child额外变成0/-99/-149/-198而normal child为40；删除唯一world traversal后focused4/4与真实Play绿，self reset保留并覆盖Itachi1250。full SelfCheck独立CPoint阻塞；下一B4 revival gate。

> **2026-09-09 B3 legacy state501生产退休已验证：** `NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001 / VERIFIED / RED_0_OF_1 / PRODUCTION_BRANCH_REMOVED / FOCUSED_1_OF_1 / EARLY_M2_11_OF_11 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。RED精确命中child ObjectId 31→9000；移除fast/fallback/legacy 501分支后，两组10实体canonical runtime/definition/identity/frame全不变，M2 11/11与真实Play通过。full SelfCheck仍由独立CPoint阻塞；下一严格route为B3 1100..1299 child propagation retirement。

> **2026-09-09 B3 legacy state501审计已闭合：** `NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_STATE501_TRANSFORM / DIRECTION_B_GAMEPLAY_ZERO / RELEASE_GAMEPLAY_ZERO / HUD_RADAR_ONLY_TWO_TOKENS / UNITY_SYNTHETIC_BRANCH_CONFIRMED / PRODUCTION_RETIREMENT_DEFINED`。Authority current production source 0个501分支且C25只处理8000..8999；Unity early-frame独有synthetic self/child mutation。审计无生产改动。

> **2026-09-09 B2 AI owner guard family已验证：** `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / VERIFIED / OWNER_GUARD_TRACE_EQUAL_8_RECORDS_48_FIELDS / UNITY_FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / DOUBLE_RUN_BYTE_STABLE / BUILDS_0_ERROR / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / SELFCHECK_BLOCKED_UNRELATED`。Authority source-model/Unity production snapshot trace为8 records/48 fields且first difference空，双端各自双跑稳定；B0 producer owner 0/7/99实际消费。真实NTSD_Battle Play通过、Console0、Scene不变。full SelfCheck仍由独立CPoint Vz阻塞；下一严格route为B3 state501 legacy child transform retirement audit。

> **2026-09-09 B0 owner-slot producer出口已验证，B2 runtime恢复：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / VERIFIED / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS / DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / B0_OWNER_PRODUCER_EXIT_READY / B2_RUNTIME_RESUMABLE / PRODUCTION_UNCHANGED`。Authority/Unity owner专项trace覆盖direct self、owner7/target0分离、two-hop OPoint、F8 99、state9996 -1、type3 mutation与slot reuse，first difference空；Authority/Unity输出各自双跑SHA稳定。01:06:52真实NTSD_Battle Play通过、Console0、Scene SHA/dirty/root不变。full SelfCheck仍更早停在既有CPoint throw-Vz，不扩大为full parity；下一恢复B2 AI owner runtime验收。

> **2026-09-09 B0 owner-slot route 5出口审计已启动：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / IN_PROGRESS / B0_OWNER_PRODUCER_ROUTE_5 / OWNER_ONLY_JOINT_TRACE_DESIGN / PRODUCTION_UNCHANGED`。既有通用raw capture固定两角色/三tick，无法安全覆盖dynamic owner生命周期；已冻结只比较+0x354/OwnerSlot的双端规范化trace，覆盖self、two-hop OPoint、F8 99、state9996 -1、type3 mutation和slot reuse。F8内容/数量/位置、全战斗parity与B8 physical consumer不借本包扩大；诊断代码与验证待执行。

> **2026-09-09 B0 ordinary OPoint owner route 4 focused通过、runtime待验：** `NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_7_OF_7 / SELFCHECK_BLOCKED_BEFORE_PRESENTATION_ASSERT_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_4 / LOGIC_AND_PRESENTATION_PRODUCERS_WRITTEN / FRAGMENT_AND_LEGACY_BRANCHES_EXCLUDED`。logic/presentation两个world-owned producer各只写parent literal owner；精确RED=`2/7`，builds 0 error，Unity 00:09:55 GREEN=`7/7`覆盖owner矩阵、kind/type/single/multi/two-hop/holder/raw backing与built-in -1。00:11:22 SelfCheck在更早CPoint停止，未到presentation新增断言；Play/joint待验。下一route5 exit audit。

> **2026-09-08 B0 F8 owner99 route 3 focused通过、runtime待验：** `NTSD28-B0-F8-OWNER99-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_3_OF_3 / SELFCHECK_REACHED_UNRELATED_CPOINT_AFTER_NEW_CHECK / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_3 / MODE2_MATERIALIZER_OWNER_ONLY / PHYSICAL_F8_EFFECT_WIRING_EXCLUDED`。精确RED=`1/3`后只在mode2 factory前写task owner=`99`；builds均0 error，Unity 23:25:56 GREEN=`3/3`覆盖slot50/399、claimed entity/raw backing分离、frame/位置/四次RNG及normal owner`-1`/六次RNG。23:27:20 SelfCheck通过本包检查后停在较后的既有CPoint throw-Vz；物理F8/Play/joint待验。下一route 4 ordinary OPoint owner propagation。

> **2026-09-08 B0 route 4 OPoint/fragment边界已更正：** Authority ordinary frame OPoint无条件传播parent literal owner；hit_Fa5/6已分离source owner与target +3F8。weapon fragments则必须拆分：built-in OID999保持owner -1/group0，只有DAT `<weapon_piece>`继承source owner。Unity正式late OPoint入口`BattleLogicObjectPointRuntime.ProcessOneLateOpoint()`缺task owner；built-in/state9996当前-1正确；DAT weapon_piece尚无parser/materializer，仅有pass骨架，完整行为归B7；hit_Fa8/9/13仍独立未知。route 4只补ordinary OPoint producer并覆盖two-hop/single/multi/first claimed active-slot runtime/raw-trace projection，独立raw backing保持`-1`，不做factory全局parent推断。本轮无code/content/Scene；route 2 focused已通过，route 3先行。

> **2026-09-08 B0 F8 owner99 route 3前实施边界已闭合：** 只读复核确认Authority在`GameSession28::step()`完成战斗tick后消费F8 pending，并由`NativeFunctionKeyDropSpawn28.owner_slot=99`原样进入`spawn_at`。Unity正式F8当前只写`FunctionKeys.PendingObjectCommand`且没有生产consumer；现有`Mode2Request==1 -> SpawnMode2RandomWeapons()`仍来自legacy diagnostic latch并缺owner。route 2 Unity focused gate现已满足；后继route 3只能先给该既有生成task写99并验证slot50/high first claimed active-slot runtime/raw-trace projection，独立raw backing保持`-1`；保留`requiredRuntimeSlot=-1`的lowest-free factory分配，不加post-register fix-up。物理F8 effect接线仍归B8；`RunNormalDrop`继续默认owner -1，用户保留的candidate/RNG/position不得改变。本轮无code/content/Scene。

> **2026-09-08 B0 direct/stage self-owner focused通过、runtime待验：** `NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_15_OF_15 / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_2 / DIRECT_AND_STAGE_SELF_OWNER_ONLY`。direct App/bootstrap在ModuleBind前声明required/self slot，adapter按Authority只接受physical slot `0..19`，actual slot不匹配时统一reset/recycle并跳过roster；stage task携带owner=required，各entity OPoint initializer在首次注册前消费explicit owner，stage/results-reserve最终保持self owner。runtime `0 error / 47 warnings`、Editor进程外compile `0 error / 104 warnings`；重启后的Unity于22:51:33完成程序集刷新。首轮7/15仅暴露测试误把独立raw backing当active entity runtime，纠正后22:51:46实际`15/15`通过，并保护raw backing owner仍为`-1`。22:52:37 full SelfCheck在更早的既有CPoint throw-Vz断言停止，Play/joint trace仍待验。F8、ordinary OPoint、state9996和其他writer保持独立。

> **2026-09-08 B0 object-AI target +0x3F8 focused 7/7通过：** `NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / 7_OF_7_PASS / SELF_CHECK_BLOCKED_BY_UNRELATED_CPOINT / PLAY_PENDING / JOINT_TRACE_PENDING / RUNTIME_PENDING / B0_OWNER_PRODUCER_PREREQUISITE / EXISTING_STORAGE_REUSED / NO_SCHEMA_CHANGE`。`PickerStableId`底层int已提供canonical target入口，Authority已闭合common/4/7/11与5/6 child已和owner解耦并修正stale/gate顺序；8/9/13、+0x2F8、schema及raw inactive adapter不在本包。Unity compile无CS错误，v2 Unity EditMode focused于20:48实际通过7/7；完整SelfCheck被更早既有CPoint throw-Vz阻塞，Play/joint trace仍待。已满足后继owner producer的compile+focused前置，下一包为direct/stage self-owner。

> **2026-09-08 B0 owner-slot production审计已闭合，B2 runtime前置重开：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIELD_BINDING_RETAINED / FORMAL_SELF_OWNER_MISSING / OPOINT_OWNER_PROPAGATION_MISSING / F8_OWNER99_MISSING / TARGET_MULTIPLEXING_CONFLICT / FIVE_ROUTES_DEFINED / B2_RUNTIME_BLOCKED / PRODUCTION_HELD`。既有B0只验字段映射；Unity primary/stage未写self owner、ordinary OPoint未传播parent owner、F8未写99，且generic hit_Fa把OwnerSlot当+3F8 target。127行/生产59行18文件已审计；先target deconflict，再self/F8/OPoint producer与exit trace。本轮无code。

> **2026-09-08 B2 AI owner-slot纠正代码/隔离验证完成，runtime待验：** `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / RUNTIME_PENDING / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / ISOLATED_RUNTIME_AND_EDITOR_COMPILE_PASS / FOCUSED_PURE_PASS / SELFCHECK_BLOCKED_UNRELATED`。SoA/unified/legacy及publisher已脱离KillCount并读OwnerSlotIndex；runtime/editor builds均0 error，owner -1/slot0/high OID122/123 harness PASS。完整SelfCheck被既有CPoint throw Vz断言阻塞；Unity NUnit、Play/joint trace待验，不删legacy carrier其他用途。

> **2026-09-08 B5 legacy damage/stats owner审计已闭合，B2/B3/B4字段绑定重开：** `NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_LEGACY_STATS_AUTHORITY / KILLCOUNT_MULTIPLEXED / EARLIER_PHASE_BINDINGS_REOPENED / EXACT_NATIVE_CARRIERS_EXIST / SEVEN_ROUTES_DEFINED / JOINT_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。577行扫描中生产207行/29文件。`KillCount`混合+0x2F4、AI owner、revival和legacy child传播；其余旧stats与world数组和Authority `+0x34C/+0x348/+0x358/+0x1E0/+0x1E4`重复。先按B2 AI→B3 state501/11xx→B4 revival→B5 +2F4/stats处理；carrier删除须加入roster1→2及既有13/20/23联合schema。本轮无code。

> **2026-09-08 B6 legacy HolderCopy multiplexed-slot owner已闭合，B5字段绑定重开：** `NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_AUTHORITY_FIELD / MULTIPLEXED_OWNER_CONFIRMED / B5_EXIT_CORRECTIONS_REQUIRED / CURRENT_TWO_HOP_GRAPH_WITNESS / ROUTES_SPLIT / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority没有HolderCopy；direct holder/owner/group/control/self slot分别对应HolderStableId/OwnerSlotIndex/RelationTeam/AnimCounter/SlotIndex。Unity151行引用中63行生产跨26文件；frozen pair/BruteForce/HitPlan误以HolderCopy取linked holder，type3 actual/HitPlan仍额外复制该字段。current117 ITR-kind2、62 OPoint-kind2，另71条/14 OID/30 pairs两跳生成图会使root copy与immediate holder分离。legacy stats审计已闭合并回溯B2-B4；先修早期binding及B5 linked-holder/type3，才继续B6生产。本轮无code。

> **2026-09-08 B6 legacy GrabbedBy relation mirror已闭合：** `NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_SECOND_RELATION_FIELD / CURRENT_OPOINT_WRITER_REACHABLE / LEGACY_READER_ROUTED / TWO_NEW_PACKAGES_PLUS_EXISTING_CONSUMER / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority只有LinkState与parent/child slots。Unity current62条OPoint-kind2/39 Character sources会写child `GrabbedBy=-1`，但shared117条ITR-kind2不写；旧raw-kind5是唯一gameplay reader且已由tracker consumer包退休。该字段进runtime snapshot/ECS fingerprint却不进checksum/parity。先复用consumer退休，再退nonzero producers；carrier删除并入ReleaseTick/WeaponState/Tracker联合schema方向。本轮无code。

> **2026-09-08 B6 legacy Tracker relation owner已闭合：** `NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_TRACKER_LAYER / CURRENT_OPOINT_WRITERS_REACHABLE / LEGACY_READERS_DEFAULT_BYPASSED / THREE_PACKAGE_SPLIT_DEFINED / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority OPoint kind2、kind5与type3 owner只用reciprocal link。Unity两factory为current62条OPoint kind2/56 edges额外写TrackerFlag1/-1和managed TrackerParent；shared kind5已在dispatch前按link转换，旧两raw readers默认被绕过，13个OPoint target与current353 kind5 ITR交集0。先退producer，再退旧consumer/cache fallback；field/base-shell snapshot删除并入ReleaseTick/WeaponState联合schema方向。本轮无code。

> **2026-09-08 B6 object-AI target +0x3F8 owner已闭合：** `NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / EXISTING_CARRIER_RECLASSIFIED / OWNER_354_CORRUPTION_CURRENT_REACHABLE / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。Authority的owner+0x354、held excluded-group source+0x2F8与non-character target cache+0x3F8独立。Unity specialized OID124已把`PickerStableId` storage当+0x3F8使用，但generic common/4/7/11与5/6 child路径把target塞进`OwnerSlotIndex`，污染attribution。current 206 generic common frames、OID219 5→4和9条3→7链可达。后继复用现有int语义→修common/child producer→再接既有+0x2F8 consumer；本轮无code。

> **2026-09-08 B6 legacy WeaponState owner已闭合：** `NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_PARALLEL_STATE / BEHAVIOR_RETIREMENT_DEFINED / CURRENT_OID124_WITNESS / CARRIER_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority只以actual current frame state驱动C02/physics/hit，没有平行WeaponState或1002→2000→3000/Vx-halving prelude。Unity唯一gameplay reader会先写额外state并随后每tick减半Vx；OID124 action40..55的16帧state1002/hit_Fa12循环、Tenten/Criminal2 direct spawn及kind2+Naruto clone DVX均证明current可达，tick1 checksum、tick2 motion首差。先退休behavior/producer并暂留reserved0；carrier删除须与ReleaseTick联合取得schema13/20/23方向。本轮无code。

> **2026-09-08 B6 legacy ReleaseTick owner已闭合：** `NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_FIELD_OR_READER / TWO_PACKAGE_SPLIT / CURRENT_RELEASE_WITNESS / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority无字段/writer/reader；Unity DVX/kind3/consume写tick并污染copy/fingerprint/checksum/parity。current valid-action witness为DVX2125 rows、kind3 11116（含Sasori→OID213 generic2）、OID122/123 edges3/35。先退休producer保留reserved-1；carrier/schema13/20/23需用户方向。本轮无code。

> **2026-09-08 B6 held missing-action continue owner已闭合：** `NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ACTION_WRITE_THEN_CONTINUE / REAL_AND_GENERIC_OWNER / CURRENT_16796_WITNESS / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`。Authority写child action后若target frame缺失只diagnostic/continue，保留relation且零RNG；Unity real/generic会继续pose/DVX/kind3，real entry null gate还阻断下次refill/recovery。full union有16796 rows/2402 triples；cover2/state12/18重验0。后继等待cleanup→terminal runtime，本轮无code。

> **2026-09-08 B6 held relation producer domain已纠正并完成后继复核：** `NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ITR_AND_OPOINT_UNION / CURRENT_REACHABILITY_REBASELINED / OWNERS_RETAINED / PRODUCTION_HELD`。先前39-holder只覆盖ITR kind2 pickup，漏了62条OPoint kind2 direct-link。完整current union为41 source definitions/668 edges；primary WPoint7624、kind3 811、nonkind3 DVX186、terminal33/23 defs全1000；missing-action 16796/2402。missing owner已闭合，cover2/state12/18在union上重验0；本轮无code。

> **2026-09-08 B6 terminal WPoint structural owner已闭合（relation domain corrected）：** `NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FREE_NOT_DESTROY_OWNER / POST_REFILL_PRE_POSE_GATE / CURRENT_33_WITNESS / CORRECTED_RELATION_DOMAIN / PRODUCTION_HELD`。Authority terminal在refill/exhaustion之后、child frame/pose/DVX/kind3之前无RNG调用`despawn()`；Unity应以transient outcome交给world调用`StructuralWriter.Free`，不能用会触发weapon break audio的`Destroy`。完整ITR+OPoint held union terminal33/23 definitions且值均1000；原28/22是pickup子集。先取得entity-link cleanup runtime绿灯，再实施terminal；本轮无code。

> **2026-09-08 B6 Direction-B multiline corpus总纠正已闭合（relation domain后继纠正）：** `NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ROOT_CAUSE_MULTILINE_OMISSION / CURRENT_REACHABILITY_REBASELINED / OWNER_RULES_RETAINED / PRODUCTION_HELD`。旧临时正则只匹配单行subblock，漏掉current多行DAT；正式projection为WPoint7995/CPoint1426/ITR4437。后继OPoint domain纠正后，完整held union为terminal33、kind3 811且authored-DV overlap1、nonkind3-DVX186；CPoint kind1/state9 776/760、front/back170、正held injury223；Tayuya kind10/11=15/5；OID417 tree有40个invalid post-vaction pairs。规则/owner保留；missing-action owner及dormant union复核已闭合，本轮无code。

> **2026-09-08 B6 kind2 pickup corpus已纠正：** `NTSD28-B6-KIND2-PICKUP-CORPUS-CORRECTION-001 / VERIFIED / GOVERNANCE_ONLY / CURRENT_CORPUS_CORRECTED / OWNER_AND_THREE_PACKAGES_RETAINED / PRODUCTION_HELD`。Direction-B frozen projection实际为117条kind2、39个holder definitions，不是1/唯一Naruto clone；supported state1004/2004 target为31 frames，不是17，旧漏项来自OID447/449/501/502/506的15个type1 frames。release仍375/53，kind7两端0，target WPoint 0/0。OID120 witness有效但非唯一；规则/owner/三包不变，测试矩阵扩大。本轮无code。

> **2026-09-08 B6 positive-link validation retirement owner已闭合：** `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_MUTATING_PASS_CONFIRMED / POST_LIFECYCLE_RETIREMENT_PACKAGE_DEFINED / PRODUCTION_HELD`。Authority post-catch位置无独立校验pass，关系一致性由writer与despawn/spawn lifecycle拥有；Unity每tick invalid时只清holder LinkState而保留其余正反向字段并发额外structural event。当前invalid可达源于registry cleanup缺口，故必须先完成lifecycle production并runtime绿，再原子退休phase/pass/mode/stress/parity witness与专用positive bitmap；AI relation projection保留，C09/C20 invalid-preserve仍另包。本轮无code。

> **2026-09-08 B6 kind2 pickup relation owner已闭合（corpus corrected）：** `NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / CURRENT_OID120_WITNESS / KIND7_DORMANT_RETIREMENT / PRODUCTION_HELD`。Authority kind2 type1/2/4/6 transaction、conditional +35C、exact OwnerSlot、weapon_throw表与WPoint tail已冻结。current/release kind2=117/375、kind7=0/0；39个current holder均可构造OID120 relation101，而Unity写1并漏OwnerSlot。31/53个supported candidate ground frames均无WPoint；kind7与unsupported tail为dormant。后续carrier/system rules→pure→atomic actual/HitPlan/legacy三包，runtime未清无code。

> **2026-09-08 B6 C22 horizontal impulse finalizer standalone已退出：** `NTSD28-B6-HORIZONTAL-IMPULSE-FINALIZER-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / C22_STANDALONE_EXIT_READY / PRODUCER_AND_STAGE_DEPENDENCIES_ROUTED / JOINT_TRACE_PENDING / NO_NEW_PRODUCTION_PACKAGE`。FrameDelay/HitCount/Knockback三轴映射、hold gate、正count三轴`2/(count+1)`、count≤0只清pending、负count保留与升序active scan逐项一致；legacy/data-oriented/shadow同合同。catch escape误写count、native impact与B8 stage removal已回路由，B12 joint trace待验；不新建重复code包。

> **2026-09-08 B6 kind2 CPoint旧hurt-action consumer owner已闭合（corpus rebased）：** `NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_HURT_OVERRIDE_CONFIRMED / ATOMIC_ACTUAL_SHADOW_LEGACY_RETIREMENT_DEFINED / CURRENT_CORPUS_WITNESS / PRODUCTION_HELD`。2.8 playable无front/back hit-tail consumer；Unity旧alias进入actual/HitPlan/legacy。current/release front/back rows=170/398且均kind2、无raw explicit injury/cover；OID300/OID33只是一个witness。后续原子退休与schema顺序不变，matrix扩大。

> **2026-09-08 B6 CPoint 27-scalar schema owner已闭合（corpus rebased）：** `NTSD28-B6-CPOINT-27-SCALAR-SCHEMA-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / AUTHORITY_27_SCALARS_REQUIRED / RELEASE_DRAIN_WITNESS / PRODUCTION_HELD`。Authority27字段；Unity19缺8字段。current/release CPoint=1426/4019，current front/back=170、raw injury298/223、cover142/106、A/T各9；8 missing fields current explicit0。release drain600三witness不变；old consumer retirement后versioned schema，resource仍等B7/B8/B11/H。

> **2026-09-08 B6 catch控制流 fences owner已闭合（corpus rebased）：** `NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ADVANCE_PACKAGE_AMENDED / SETTLEMENT_PACKAGE_DEFINED / CURRENT_AND_RELEASE_TREE_WITNESS / PRODUCTION_HELD`。Authority mismatch/negative release立即continue；Unity误继续tail且写错counter。current kind1/state9/negative=776/760/204，OID417 tree有40个post-vaction invalid pairs；release OID555有134。mixed/exact advance与settlement preflight owners不变，runtime未清。

> **2026-09-08 B6旧FluteForce dead API owner已闭合：** `NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / DEAD_API_RETIREMENT_PACKAGE_DEFINED / IMPACT_SEPARATE / PRODUCTION_HELD`。全repo仅base旧mass/threshold实现和weapon空override，无调用/序列化引用；actual kind10/11与Authority均不经过它。后续单包删除两个符号并禁止virtual dispatch复活；真实impact仍走独立carrier→pure→atomic。runtime未清，本轮无code。

> **2026-09-08 B6旧NTSDSpec character mass owner已闭合：** `NTSD28-B6-NTSDSPEC-MASS-CARRIER-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CHARACTER_MASS_RETIREMENT_PACKAGE_DEFINED / FORMAL_OUTPUT_UNCHANGED / PRODUCTION_HELD`。Authority grounded friction无mass gate；Unity唯一行为reader是`ctx.mass>0`。旧非空mass IDs均非current/release type0，正式character恒为1，故formal输出当前不变；但synthetic值可改physics且只进character shell snapshot。后续原子删除context/character/snapshot mass并将shell schema1→2；dead Flute lookup另包。runtime未清，本轮无code。

> **2026-09-08 B6旧NTSDSpec compat weapon action owner已闭合：** `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SHARED_SELECTOR_PACKAGE_DEFINED / DEFAULT_PROFILE_UNAFFECTED / PRODUCTION_HELD`。正式默认DataOriented已用link-state+linked stats且不读旧表；Legacy仍可由配置/CLI进入生产。四个NTSDSpec bool表达式都在合法relation已分流后的else，无正式决策owner；Legacy另有type6 neutral 52-vs55、heavy fields、relation4 all-directions和nonzero stats差异。后续一包抽shared selector、修matrix并删bool wrappers；OID122/123提供current Legacy witness。runtime未清，本轮无code。

> **2026-09-08 B6 native impact 10/11/17/18 owner已闭合（current witness corrected）：** `NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / CURRENT_AND_RELEASE_WITNESS / PRODUCTION_HELD`。规则与三包owner不变；current/release kind10/11/17/18现为15/5/0/0与121/61/0/0。current20条10/11均在OID36 Tayuya actions243..247，因此10/11已current reachable；17/18仍synthetic。

> **2026-09-08 B6旧NTSDSpec生产owner inventory已闭合：** `NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。生产引用穷尽为4文件6行7表达式；Authority closure无mass/oscillate字段，ground friction无条件，held action读取link-state+linked DAT stats，kind10/11/17/18与render-phase各有独立owner。当前/release linked action stats entry均0；当前impact kind10/11/17/18为0，release为121/61/0/0；FluteForce/EffectCreate在repo closure无producer。旧ID已别名到不同对象，禁止继续使用。后续mass、compat weapon、native impact、B9 oscillate、empty-shell五包；现有B6 runtime栈未清，本轮无code。

> **2026-09-08 B6 catch advance slot顺序首差已路由：** `NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SINGLE_ASCENDING_MIXED_PASS_REQUIRED / EXACT_CONSUMER_PACKAGE_DEFINED / PRODUCTION_HELD`。Authority单一升序loop逐slot二选一处理kind1/current-kind2；Unity先全体kind1再全体kind2，低槽caught/高槽catcher会被错误当作catcher之后。当前throw vaction180/181无kind2交叉匹配；release有2条action343→vaction132/next344且134个type0 definition的132为kind2。后续合并mixed-order与exact CatchSourceSlot90 consumer迁移，保持settlement独立；真实Hinata/Neji Play待验，本轮无code。

> **2026-09-08 B6 held WPoint规则分类已二次纠正并复核：** `NTSD28-B6-HELD-WPOINT-DORMANT-RULES-AUDIT-001 / VERIFIED / CORRECTED_BY_MULTILINE_AND_RELATION_DOMAIN_AUDITS / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`。current全量WPoint7995、完整held-union primary7624，terminal33已current reachable且structural owner已闭合；kind3 811、nonkind3 DVX186。cover2与referenced state12/18在668-edge union仍为0，恢复current/release dormant；generic Vz/damaged-drop owner保留。

> **2026-09-08 B6 catch relation exact-field owner已闭合（corpus rebased）：** `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_ACTUAL_AND_SHADOW_PACKAGE_DEFINED / PRODUCTION_HELD`。Unity actual/HitPlan漏exact `CatchSourceSlot90`；current/release kind3=249/548，current249均有pair且respond0。compat只作mirror，atomic actual+shadow owner不变，current matrix扩大。

> **2026-09-08 B6 entity-link lifecycle cleanup owner已闭合：** `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。exact映射为LinkState/TargetSlotIndex/HolderStableId/CaughtSlotIndex/CatchSourceSlot90/CaughtDuration；CatcherSlotIndex/HeldWeaponStableId只作compat，HolderCopy与Kind4 count保留。registry在slot release成功后、返回/复用前原子扫描清引用；先cleanup production，runtime绿后再改invalid handler。本轮无code。

> **2026-09-08 B6 invalid held relation生命周期可达已确认：** `NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / LIFECYCLE_REACHABILITY_CONFIRMED / ATOMIC_DESPAWN_LINK_CLEANUP_REQUIRED / PRODUCTION_HELD`。Authority despawn先全局清held/catch反向引用再释放slot；Unity registry先release且可同tick复用，无关联scan，C09/C20 invalid清零只是下游补偿并存在slot ABA窗口。下一先冻结完整held/catch exact字段与structural owner，不能局部改invalid handler；本轮无code。

> **2026-09-08 B6 invalid held reciprocal旧reachability状态已关闭：** `NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001 / SUPERSEDED / REACHABILITY_RESOLVED_BY_NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`。同状态差异与terminal/cover2/state12/18统计保留，但动态可达已由despawn/reuse审计证明；以后继lifecycle与owner记录为准。

> **2026-09-08 B6 held DVX +0x2F8 carrier已拆分：** `NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。Authority +0x2F8默认-1，仅由type1/4/6 held DVX写holder slot并由non-character AI排除其battle group；Unity通用Spawner有respawn/lifecycle等额外writers，不能复用。后续拆carrier→writer→AI consumer；consumer须先确认C02/legacy唯一owner。runtime阻塞未变，本轮无code。

> **2026-09-08 B6 held DVX weapon HP首差已路由（relation domain rebased）：** `NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SUBSEQUENT_WEAPON_HP_PRESERVATION_ROUTED / PRODUCTION_HELD`。Authority不写weaponHP，Unity OnThrown重置。完整current held-union/release non-kind3 DVX=186/599；原184是pickup-only。owner与顺序不变，matrix扩大。

> **2026-09-08 B6 WPoint kind3 owner已纠正（relation domain rebased）：** `NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001 / VERIFIED / GOVERNANCE_ONLY / TWO_ACTUAL_BRANCHES_DEFINED / CURRENT_AND_RELEASE_WITNESS / PRODUCTION_HELD`。Authority DVX后继续kind3；Unity提前return。完整current held-union kind3=811且Rock Lee255已有authored DV overlap；原770是pickup-only，release2744/5。RunStep12+DropRandomly owner不变。

> **2026-09-09 B6 held-refill MP/exhaustion暂停于Play前：** `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001 / RUNTIME_PENDING / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / BUILDS_0_ERROR / PLAY_PENDING / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / HP_BASEMAX_DEFERRED / USER_HOLD`。原代码已取得真实Unity focused与C09 placement证据；专门refill/exhaustion Play尚未执行，用户要求暂停，禁止提升为VERIFIED。

> **2026-09-08 B6 throw许可诊断已穷尽无密钥路径：** 第四次以本机Hub已使用的无密钥`-useHub/-hubIPC/-licensingIpc`参数启动，versioned LicensingClient能handshake，但沙箱进程无Hub access token，Editor以无有效license/exit1退出。Hub CLI本身又在尝试写`AppData/Roaming/UnityHub/user-settings` 时EPERM。没有读取、传入或回显敏感token；不再继续许可尝试。四次均未进入Test Runner、均无XML，throw仍RUNTIME_PENDING。

> **2026-09-08 B6 C09再纠正至更早的refill exhaustion分支：** `NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001 / VERIFIED / HELD_REFILL_MP_EXHAUSTION_ROUTED`。kind3 WPoint之前，OID122 HP=1或OID123 HP=2即可当次耗尽。Authority只写一次random Vx、Vy=0并保留Vz；Unity写Vy=-8/Vz=0。OID123对HP<=0的入场仍应执行HP-=2和exhaustion，且应以`OrdinaryCreditGate2F4`将child MP截到150；Unity早退、误用KillCount并误写holder MP。因此这是当前全局下一包，kind3 owner保留为后续包。

> **2026-09-08 B6 held-refill MP/exhaustion owner已闭合：** `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001 / VERIFIED / ACTUAL_ONLY_PACKAGE_DEFINED / HP_BASEMAX_DEFERRED`。actual仅在`LF2WeaponHeldStateResolver.ProcessDrinkConsumption()`；修OID123 nonpositive entry、+0x2F4 child cap与共享exhaustion Vx/Vy/Vz。OID122 HPBound/baseMax clamp仍依赖B11/H，不混入；throw runtime未验前未启动production。

> **2026-09-08 B6全局入口顺序已纠正（relation domain rebased）：** `NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001 / VERIFIED / GLOBAL_B6_ORDER_CORRECTED / WPOINT_KIND3_ROUTED`。C09早于geometry；release kind3=2744，完整current held-union kind3=811且authored overlap1。refill仍是更早分支；kind3与terminal均为current后继，CPoint throw仍是post-hit有效修正。

> **2026-09-08 B6 WPoint kind3旧owner已被纠正：** `NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001 / SUPERSEDED / CORRECTED_BY_NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001`。其四draw/final-write事实保留，但“仅改DropRandomly”不再可实施；必须同时处理RunStep12的DVX→kind3 continuation。

> **2026-09-08 B6 throw第三次Unity重试仍受许可阻塞：** 项目lock记录PID 7192已不存在，因此只启动一个batch focused run。Unity成功拉起LicensingClient PID 53168，但`LicenseClient-Logan` IPC channel等待60.01秒后仍不存在，以199退出；`Temp/NTSD28-B6-CpointThrow-focused-retry.xml`未生成。这不是focused test失败，而是Test Runner未启动；状态继续RUNTIME_PENDING。

> **2026-09-08 B6 held-injury owner已闭合：** `NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / FULL_RESOURCE_DEFERRED`。第一包闭合display lead、`IncomingDamageScale340`、直接owner/type0-self credit、canonical HP/score/KO accounting和cover timers；第二包在整个settlement后生产caughtact combo。Authority world KO event/feed仍需独立owner；full MP resource仍受B7/B8/B11/H阻塞。throw package尚无Unity runtime绿灯，本轮未叠加新行为修改。

> **2026-09-08 B6 post-throw settlement首差已路由（corpus rebased）：** `NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / HELD_INJURY_ACCOUNTING_ROUTED`。release正injury484；current kind1正injury223、cover0:205/1:15/11:3。Unity accounting/timer差异与两包owner不变；测试矩阵扩大。

> **2026-09-08 B6 post-throw settlement只读审计已启动：** `NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY`。在throw runtime仍受Licensing阻塞时，仅继续读取dircontrol、held injury/cover/position、caughtact combo与后置settlement，选择下一首差；不改code/content/Scene，不绕过B7/B8/B11/H full-resource依赖。

> **2026-09-08 B6 CPoint throw可精确子集已写、运行待验：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / RUNTIME_PENDING / CODE_WRITTEN / ISOLATED_COMPILE_PASS / UNITY_LICENSING_BLOCKED / FULL_RESOURCE_DEFERRED`。invalid MPMax transaction已移除；display/environment/self-source/WeaponCount exclusion/Vz XOR已写，focused 8 cases与SelfCheck/Play probe断言已同步。Runtime47/0、Editor104/0、source7/7、Ledger355/307与Scene SHA通过；Unity batch两次在Test Runner前被LicenseClient IPC以199终止，无focused/SelfCheck/Play绿灯。

> **2026-09-08 B6 CPoint throw范围已纠正并继续：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST / FULL_RESOURCE_DEFERRED`。编译后复读B5 readiness，确认完整MP transaction仍受B7 child suppression、B8 mode与B11/H baseMax阻塞；旧atomic包已SUPERSEDED，后继包先移除MPMax代替，再只闭合display/environment/WeaponCount/Vz。Unity batch两次在测试框架前被Licensing IPC以199阻断；runtime尚未通过。

> **2026-09-07 B6 CPoint throw atomic production已启动：** `NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。owner已冻结为BattleCpointWriter actual-only，复用B5 resource helpers与现有Environment carriers；先写RED，再修正resource/environment/WeaponCount/Vz和旧测试断言。不改HitPlan/content/Scene。

> **2026-09-07 B6 CPoint throw owner审计已闭合：** `NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_PRODUCTION_SPLIT_DEFINED`。当前throw所需scalar与parity carrier齐备；75条throw的drain/gain均0，无需先扩schema；HitPlan排除。下一atomic production。

> **2026-09-07 B6 CPoint throw owner审计已启动：** `NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_OWNER_SPLIT`。入口审计已把首个正式内容可达差异定位到kind-1 throw：Unity误写WeaponCount、缺资源事务，并在无独占depth输入时误清Vz。当前只读冻结formal/actual/resource/environment/HitPlan责任，不改code/content/Scene。

> **2026-09-07 B6入口审计已闭合：** `NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CPOINT_THROW_SETTLEMENT_ROUTED`。Authority post-hit顺序和4019条CPoint corpus已闭合；A/T/D/UZ/DZ/F/B/J规则虽比Unity完整，但正式corpus无这些显式action字段，首个可达差异是75条throw中的environment/resource与无depth Vz语义。下一owner audit。

> **2026-09-07 B6入口审计已启动：** `NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。从post-hit catch relation/settlement、held/cpoint与impulse tail开始对照Unity；caughtact combo与cpoint resource等待真实B6事件。当前不改code/content/Scene。

> **2026-09-07 B5出口门已闭合：** `NTSD28-B5-EXIT-GATE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / B5_PLACEMENT_EXIT_READY / B5_FULL_CLOSE_DEFERRED`。连续audit001～011及combo三包后，Authority standalone hit tail已扫到driver return，无新的独立B5首差；允许进入B6。B6/B7/B8/B10/B11/H与最终joint trace仍明确后置，不能称整个命中系统已完全一致；C-07旧“全局终止”已更正为per-attacker abort。

> **2026-09-07 B5 native combo expiry已验证：** `NTSD28-B5-NATIVE-COMBO-EXPIRY-001 / VERIFIED / CORE_COMBO_EXPIRE_ROUTED / FORMAL_TUPLE_INACTIVE`。C25 live-slot尾后按record/negative-respond gate与inclusive elapsed扫描active slots，只清positive count且保留lastTick；focused5、B5-831、NTSD28-1178、fresh SelfCheck、Console0通过，Scene unchanged。B6/B10/H未接。

> **2026-09-07 B5 native combo ordinary producer已验证：** `NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001 / VERIFIED / PRODUCTION_ROUTED / FORMAL_TUPLE_INACTIVE`。shared runner普通Damage成功后按slot回查并执行bound/type/facing/one-hop/current-process-tick生产；focused7、B5-826（唯一MCP污染项isolated1）、NTSD28-1173、fresh SelfCheck、Console0通过，Scene unchanged。expiry、B6/B10/H未接。

> **2026-09-07 B5 native combo carriers已验证：** `NTSD28-B5-NATIVE-COMBO-CARRIERS-001 / VERIFIED / CARRIERS_READY / BEHAVIOR_UNCONNECTED`。独立entity count/lastTick与world mode tuple已贯通reset/copy/ECS/snapshot/checksum/parity/restore，默认不激活；focused6、related29、B5-819、NTSD28-1166及fresh SelfCheck通过，Scene SHA/mtime不变。ordinary producer、expiry、B6/B10/H仍未连接。

> **2026-09-07 B5 native combo owner audit已闭合：** `NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED`。实施顺序为carrier→ordinary producer→C25后expiry；caughtact依赖B6，表现归B10，正式tuple激活归H。审计无行为写入。

> **2026-09-07 B5 native combo runtime owner audit已启动：** `NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_OWNER_SPLIT`。只读冻结entity/world carrier、普通命中producer、B6 caughtact依赖、C25后expiry及B10表现边界；不改code/content/Scene。

> **2026-09-07 B5 remaining exit audit 011已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-011 / VERIFIED / GOVERNANCE_ONLY / NATIVE_COMBO_RUNTIME_ROUTED / B5_EXIT_NOT_READY`。正式combo record与playable mapping可达；Unity缺专用count/last tick、producer及实际CoreComboExpire。下一owner audit。

> **2026-09-07 B5 remaining exit audit 011已启动：** `NTSD28-B5-REMAINING-EXIT-AUDIT-011 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。state2000 away damping已闭合；现继续只读复核reduced余尾、unarmored/non-character continuation、hit-record与命中后生命周期，选择下一唯一首差或B5退出证据；不改code/content/Scene。

> **2026-09-07 B5 reduced state2000 away damping production已验证：** `NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001 / VERIFIED / REDUCED_STATE2000_AWAY_DAMPING_ALIGNED`。actual与HitPlan共享double-X严格away predicate并`/2.5`；equal/toward不衰减。RED10、focused14、HitPlan185、B5-813；NTSD28 broad 1159/1160仅MCP日志污染且isolated1通过；SelfCheck PASS、Console0、Scene dirtyfalse/root13/SHA不变。下一remaining audit011。

> **2026-09-07 B5 reduced state2000 away damping production已启动：** `NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。Authority按double X只衰减away；Unity actual/HitPlan使用XInt与toward极性。正式OID150 type2 state2000 kind0可达。先RED再共享predicate；不改Scene/content。

> **2026-09-07 B5 remaining exit audit 010已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-010 / VERIFIED / GOVERNANCE_ONLY / REDUCED_STATE2000_AWAY_DAMPING_ROUTED / B5_EXIT_NOT_READY`。下一首差已定位并路由；审计无行为写入。

> **2026-09-07 B5 remaining exit audit 010已启动：** `NTSD28-B5-REMAINING-EXIT-AUDIT-010 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。system-table terminal已闭合；从Authority后续target-type/effect/audio/spark/deferred lifecycle与reduced tail继续选下一唯一B5首差。当前不改code/content/Scene/Authority。

> **2026-09-07 B5 system-table attacker terminal production已验证：** `NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001 / VERIFIED / SYSTEM_TABLE_ATTACKER_TERMINAL_ALIGNED`。OID214在unarmored writer内按Authority早期清零；OID201以预解析route保留到Unity hit-record tail后释放；armor/defense reduced排除，broken fallback保留。RED2+顺序RED、focused11、HitPlan185、B5-149、NTSD28-330、18:45:27Z SelfCheck PASS；Console0、Scene dirtyfalse/root13/SHA不变。下一remaining audit010。

> **2026-09-07 B5 system-table attacker terminal production已启动：** `NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。audit009确认Unity OID201/214 shell tail会泄漏到selected-armor/ordinary-defense reduced hit，且214清零时点晚于Authority；先以reduced-path RED固定，再把两表移入DamageWriter的unarmored精确顺序。SceneView/Hierarchy修复仍为VERIFIED，当前后台测试不会向SceneView提交无Hierarchy owner的中央像素。

> **2026-09-07 B5 remaining exit audit 009已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-009 / VERIFIED / GOVERNANCE_ONLY / SYSTEM_TABLE_ATTACKER_TERMINAL_ROUTED / B5_EXIT_NOT_READY`。Authority仅在unarmored/type0 continuation消费john_biscuit214与henry_arrow201；Unity旧shell tail对所有accepted kind0 character hit生效。下一production001；审计无code/content/Scene修改。

> **2026-09-07 B5 remaining exit audit 009已启动：** `NTSD28-B5-REMAINING-EXIT-AUDIT-009 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。在first-BDY production与formal criminal Play闭合后，从Authority后续hit tail继续重扫Unity owner、正式可达性与既有family coverage；当前不改代码/content/Scene/Authority。

> **2026-09-07 B5 first-BDY response atomic production已验证：** `NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / FIRST_BDY_RESPONSE_PRODUCTION_ALIGNED / FORMAL_CRIMINAL_PLAY_PASS`。shared runner已在consume前统一提交1xxx/2xxx/encoded响应、同步RNG、actual writes与per-attacker abort；HitPlan/DataOriented同步。实施中以正式criminal OID300发现并修正Unity提前`Oid300Redirect`绕过权威响应的首差。writer/HitPlan及OID300 RED→focused17、B5-138、HitPlan185、NTSD28-318、01:37:51 SelfCheck与formal criminal Play11 PASS；Console0、Scene不变、Ledger341/297。下一`NTSD28-B5-REMAINING-EXIT-AUDIT-009`。

> **2026-09-07 B5 first-BDY response pure core已验证：** `NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / BEHAVIOR_UNCONNECTED`。1xxx/2xxx、respond、encoded chance/action/effect与raw injury均已纯投影，外置roll且warm 100000次0 allocation。RED CS0246、focused39/39、B5 653/653、NTSD28 1118/1118、SelfCheck PASS、Console0、Scene不变、Ledger340/295。下一atomic production。

> **2026-09-06 B5 first-BDY response carrier已验证：** `NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。first-kind一般化入口与first-BDY respond parser/carrier已补齐；formal body仍X/Y/W/H。RED7、focused4/4、B5 614/614、NTSD28 1079/1079、SelfCheck PASS、Console0、Scene不变、Ledger339/293。runner/damage/RNG/HitPlan仍未接；下一包pure core。

> **2026-09-06 B5 first-BDY response owner audit已闭合：** `NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED`。保留formal body X/Y/W/H，first-kind新增一般化入口并独立补respond；pure resolver外置roll；shared runner在consume effects前执行，HitPlan新增attempt shadow覆盖同步cursor和success/failure，成功只终止当前attacker。下一carrier→pure→atomic；本审计无行为/content/Scene修改。

> **2026-09-06 B5 remaining exit audit 008已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-008 / VERIFIED / GOVERNANCE_ONLY / FIRST_BDY_RESPONSE_ROUTED / B5_EXIT_NOT_READY`。下一首差为目标当前帧第一个BDY的1xxx/2xxx动作/阵营/hold响应与十进制encoded响应；Unity缺`respond` carrier、RNG/actual/HitPlan和成功后的per-attacker abort。Authority正式runtime有55个1xxx首BDY帧与157个encoded首BDY帧；Unity冻结Config自身也有22个1xxx首BDY帧。下一owner audit；本审计无行为/content/Scene修改。

> **2026-09-06 B5 hit-group eligibility规则族允许退出：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / HIT_GROUP_ELIGIBILITY_FAMILY_EXIT_READY`。唯一resolver、三collector pre-nearest筛选、frozen shared/cached consumer、kind5 holder及store/HitPlan identity复审无残余首差；下一remaining audit 008。本审计无行为/content/Scene修改。

> **2026-09-06 B5 hit-group eligibility atomic production已验证：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / HIT_GROUP_ELIGIBILITY_PRODUCTION_ALIGNED`。formal三collector在nearest/capacity/RNG前冻结并统一筛选，shared/cached consumer复用snapshot，kind5 holder亦冻结；red8、focused25、related255、B5 610、NTSD28 1168、build0、Play10 candidates、22:38 SelfCheck、Console/Scene/Ledger均PASS。下一独立exit audit。

> **2026-09-06 B5 hit-group eligibility pure core已验证：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。七步truth table、state190反转、mode/state effect排除、target type/facing与zero-allocation闭合；red7、focused52、B5 585、NTSD28 1143、build0、21:34 SelfCheck、Console0、Scene/Ledger PASS。下一`NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001`。

> **2026-09-06 B5 hit-candidate pair snapshot carrier已验证：** `NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001 / VERIFIED / CARRIER_READY / FORMAL_PRODUCER_UNCONNECTED`。完整18-payload+valid readonly carrier已贯穿SceneQueryHit/store/shadow/cached rebuild/shared runner/HitPlan identity；formal producer仍为invalid，未接eligibility且无persistent schema变化。red2、dedicated4、HitPlan185、RoleAware92、B5 533、NTSD28 1091、build0、21:03 SelfCheck、Console0、Scene/Ledger PASS。下一`NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001`。

> **2026-09-06 B5 world hit-group mode gate carrier已验证：** `NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_AND_CONTENT_UNCONNECTED`。`ActiveModeHitGroupGate18`已进入default/reset/restore/core snapshot/checksum/full parity，schema9/18/21且entity schema11不变；red9、focused19+12+21、B5 529、NTSD28 1087、build0、20:27 SelfCheck、Console0、Scene/Ledger PASS。下一`NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001`；candidate/consumer、background content与full-restore behavior仍未接。

> **2026-09-06 B5 hit-group owner audit已闭合：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FOUR_PACKAGE_SPLIT_DEFINED`。完整18-payload+valid pair snapshot、三collector唯一汇合点、shared/cached consumer、store shadow、HitPlan identity与world mode18 scalar/schema责任已冻结；下一`NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001`，再pair carrier→pure core→atomic production。本审计无行为/content/Scene/authority写入。

> **2026-09-06 B5 remaining exit audit 007已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-007 / VERIFIED / GOVERNANCE_ONLY / HIT_GROUP_ELIGIBILITY_FROZEN_PAIR_ROUTED / B5_EXIT_NOT_READY`。下一首差为native hit-group eligibility及candidate frozen pair：Unity错读collision-state、缺state190反转/state180/mode gate、opposing-facing极性相反、kind-set不全且consumer重读live fields。下一owner/snapshot audit；Authority state190为39 frames/19 DAT、Unity冻结Config为0，内容不在本审计改写。

> **2026-09-06 B5 special-hit latch atomic已验证：** `NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / SPECIAL_HIT_LATCH_PRODUCTION_ALIGNED / LEGACY_WEAPON_CONFIRM_PRESERVED`。四type3 actual、shared pre-writer type0 gate与五HitPlan projection/diff已原子迁独立latch；普通weapon旧`HitConfirm2` writers/clears保持。red4、atomic4/type3-18/HitPlan184/B5-524/NTSD28-1082、18:58 SelfCheck、collision-hit Play（10 candidates、三类carrier见证）均PASS；Console0、Scene SHA/dirty/root不变。下一remaining exit audit 007。

> **2026-09-06 B5 special-hit latch owner audit已闭合：** `NTSD28-B5-SPECIAL-HIT-LATCH-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`。Authority四true producer/两pre-writer consumer、Unity四actual写面/单shared gate/五HitPlan projection已冻结；下一包必须原子迁新carrier并保留普通weapon旧`HitConfirm2` writers/clears。本审计无code/content/Scene/authority写入。

> **2026-09-06 B5 special-hit latch carrier已验证：** `NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。独立bool的full reset/input-preserve/copy/snapshot/checksum/ECS hash/parity及Unity/C++ raw字段已闭合，schema11/17/20、raw49/43/6；focused5/raw3、related39、B5 520、NTSD28 1078、Parity21/5、C++ raw3ticks/6entities、18:25:08 SelfCheck、Console/Scene/Ledger均PASS。下一owner audit；本包未改任何hit producer/consumer/HitPlan或旧`HitConfirm2`行为。

> **2026-09-06 B5 remaining exit audit 006已路由special-hit latch：** `NTSD28-B5-REMAINING-EXIT-AUDIT-006 / VERIFIED / GOVERNANCE_ONLY / SPECIAL_HIT_LATCH_LIFECYCLE_ROUTED / B5_EXIT_NOT_READY`。Authority `special_hit_latch_0eb`为object-lifetime bool、四producer/两consumer且无tick clear；Unity复用`HitConfirm2`却在C25/C11清零，且普通weapon也写该字段，不能直接永久化。下一独立`SpecialHitLatch0EB` carrier，再owner/atomic迁移；本审计无code/content/Scene/authority写入。

> **2026-09-06 SceneView/Hierarchy可见性修复已验证：** `BATTLE-SCENEVIEW-HIERARCHY-VISIBILITY-001 / VERIFIED / SCENEVIEW_ISOLATED / TRANSIENT_GO_HIERARCHY_VISIBLE`。正常战斗SceneView真实Play为world19/7、gate/lease false、pixels0；benchmark runner/presenter/children/camera统一DontSave且Hierarchy可见，第二红灯精确、green1/1、benchmark38/38，最终09:06:42Z SelfCheck PASS。Test Runner保存了测试前既有dirty Scene状态，未回退用户内容。主线回B5 audit006。

> **2026-09-06 B5 weapon durability专项已退出：** `NTSD28-B5-WEAPON-DURABILITY-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WEAPON_DURABILITY_ATTACKING_INJURY_EXIT_READY`。type1/2/4/6 actual/HitPlan与全部priority/fallback/bdefend100/HP隔离复审无首差。下一remaining audit 006。

> **2026-09-06 B5 weapon durability production已验证：** `NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-PRODUCTION-001 / VERIFIED / WEAPON_DURABILITY_ATTACKING_INJURY_ALIGNED`。red7/10；focused10、HitPlan184、B5 515、Unity侧 `NTSD28` 自动回归1073、08:28:37Z SelfCheck、Console/Scene PASS。下一专项exit audit。

> **2026-09-06 B5 weapon durability owner audit已闭合：** `NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`。复用既有pure resolver；actual `ApplyWeaponDamage` 与HitPlan counter projection是唯一写面，无需新carrier/schema。下一test-first production。

> **2026-09-06 B5 remaining exit audit 005已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-005 / VERIFIED / GOVERNANCE_ONLY / WEAPON_DURABILITY_ATTACKING_INJURY_ROUTED / B5_EXIT_NOT_READY`。下一首差为type1/2/4/6武器耐久：Authority扣native attacking injury，Unity actual/HitPlan扣raw injury。下一owner audit。

> **2026-09-06 B5 kind4专项已退出：** `NTSD28-B5-KIND4-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KIND4_SPECIFIC_FAMILY_EXIT_READY`。三candidate模式、carrier生命周期、runtime/direct/HitPlan转换、heavy-held、catch-source/two-owner score与success-only decrement均复审闭合；production无kind4/WeaponCount gate。下一 `NTSD28-B5-REMAINING-EXIT-AUDIT-005`；B6 producer排除。

> **2026-09-06 B5 kind4 dead WeaponCount selection已退役：** `NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001 / VERIFIED / DEAD_WEAPONCOUNT_KIND4_SELECTION_RETIRED`。red1/31；focused31、B5 505、Unity侧 `NTSD28` 自动回归1063、08:08:03Z SelfCheck、Console/Scene PASS。下一kind4-specific exit audit。

> **2026-09-06 B5 kind4 atomic production已验证：** `NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / KIND4_ENVIRONMENT_CONSUMPTION_ALIGNED`。valid red8/10；focused30、HitPlan184、RoleAware67、B5 504、Unity侧 `NTSD28` 自动回归1062、07:47:21Z SelfCheck、Console/Scene PASS。下一kind4-specific exit audit；B6 producer仍排除。

> **2026-09-06 B5 kind4 +92 carrier已验证：** `NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。red13；focused5、related47、B5 474、Unity侧NTSD28回归939、06:26:19Z SelfCheck、Console/Scene/Ledger PASS。下一atomic behavior。

> **2026-09-06 B5 kind4 environment owner audit已闭合：** `NTSD28-B5-KIND4-ENVIRONMENT-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_ATOMIC_SPLIT_DEFINED`。EnvironmentState320/CatchSourceSlot90保留，缺Kind4SourceCount92先补carrier，再原子接candidate/actual/HitPlan/attribution/decrement；B6 producer独立。

> **2026-09-06 B5 remaining audit 004已路由kind4 environment chain：** `NTSD28-B5-REMAINING-EXIT-AUDIT-004 / VERIFIED / GOVERNANCE_ONLY / KIND4_ENVIRONMENT_CONSUMPTION_ROUTED / B5_EXIT_NOT_READY`。Unity已有EnvironmentState320，但actual/HitPlan错误读WeaponCount，且缺+92 producer/attribution/decrement。下一owner audit。

> **2026-09-06 B5 multi-body candidate规则族允许退出：** `NTSD28-B5-MULTI-BODY-CANDIDATE-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / MULTI_BODY_CANDIDATE_FAMILY_EXIT_READY`。三种collector、源序、20容量、nearest RNG与direct保护无该规则族首差；不代表全B5退出。

> **2026-09-06 B5 multi-body candidate production已验证：** `NTSD28-B5-MULTI-BODY-CANDIDATE-PRODUCTION-001 / VERIFIED / MULTI_BODY_CANDIDATE_MULTIPLICITY_ALIGNED`。red7/8；focused8、RoleAware67、HitPlan184、B5 469、broad934、05:51:00Z SelfCheck、Console/Scene/Ledger PASS。下一exit audit。

> **2026-09-06 B5 multi-body candidate owner audit已闭合：** `NTSD28-B5-MULTI-BODY-CANDIDATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PRODUCTION_SEAMS_FROZEN`。brute/loose共享pair路径，role-aware exact有源序body cache，fallback回pair；每BDY继续调用唯一TryRecord owner，direct query不变。下一production。

> **2026-09-06 B5 remaining audit 003已路由multi-body candidate multiplicity：** `NTSD28-B5-REMAINING-EXIT-AUDIT-003 / VERIFIED / GOVERNANCE_ONLY / MULTI_BODY_CANDIDATE_MULTIPLICITY_ROUTED / B5_EXIT_NOT_READY`。Authority每个重叠BDY独立产生candidate；Unity brute/cached均首个重叠即返回。下一owner audit。

> **2026-09-06 B5 candidate effect/type规则族允许退出：** `NTSD28-B5-CANDIDATE-EFFECT-TYPE-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CANDIDATE_EFFECT_TYPE_SPECIFIC_FAMILY_EXIT_READY`。candidate各模式及shared consumer二次门无该规则族首差；不代表全B5退出。

> **2026-09-06 B5 candidate effect/type production filter已验证：** `NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001 / VERIFIED / CANDIDATE_AND_CONSUMER_FILTER_ALIGNED`。有效red6/15；focused15、HitPlan184、B5 461、broad926、05:13:45Z SelfCheck、Console/Scene/Ledger PASS。下一exit audit。

> **2026-09-06 B5 candidate effect/type pure core已验证：** `NTSD28-B5-CANDIDATE-EFFECT-TYPE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。13→0、14→3、15→0/3、16→1/2/3/4/6与其他unrestricted闭合；red10/26、focused26、B5 446、broad911、04:53:26Z SelfCheck、Console/Scene/Ledger PASS。下一production filter。

> **2026-09-06 B5 remaining audit 002已路由candidate effect/type：** `NTSD28-B5-REMAINING-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / CANDIDATE_EFFECT_TYPE_FILTER_ROUTED / B5_EXIT_NOT_READY`。Authority在几何前以effect13..16过滤target type；Unity candidate无对应门，且晚阶段action-override matcher语义不同。下一独立pure core。

> **2026-09-06 B5 kind8专属规则族允许退出：** `NTSD28-B5-KIND8-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KIND8_SPECIFIC_FAMILY_EXIT_READY`。所有candidate模式、runtime defensive gate、shared四壳actual owner与HitPlan已闭合，无kind8-specific首差；不代表全B5退出。

> **2026-09-06 B5 kind8 control relation已验证：** `NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / KIND8_CONTROL_RELATION_ALIGNED`。candidate与consumer复用selector；shared runner唯一actual writer及HitPlan已闭合conditional heal/PP/action、dvy precise模式与int不写。red14、focused14、HitPlan184、collision258、B5 420、clean broad885、04:33:16Z SelfCheck、Console/Scene/Ledger PASS。下一kind8 exit audit。

> **2026-09-06 B5 kind8 production owner审计已闭合：** `NTSD28-B5-KIND8-PRODUCTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`。现有字段完整；下一包必须原子接`BruteForceSceneQuery` candidate gate、shared runner唯一actual writer与HitPlan defensive projection。group/owner/mode映射固定为RelationTeam/OwnerSlotIndex/BattleGameModeId；整数坐标本事务不写。

> **2026-09-06 B5 kind8 eligibility pure core已验证：** `NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。0..6 exact、7 weapon group、8 unrestricted及respond0..4 group/owner/mode真值表闭合；red至少25/35、focused35、B5 406、broad871、03:59:36Z SelfCheck、clean Console/Scene/Ledger PASS。下一production owner/write-surface audit；candidate/actual/HitPlan尚未接。

> **2026-09-06 B5 remaining exit audit已闭合：** `NTSD28-B5-REMAINING-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KIND8_FAMILY_ROUTED / B5_EXIT_NOT_READY`。下一独立首差为kind8 control relation：Unity candidate错误硬限制type0，actual/HitPlan缺selector复核、caughtact MP、dvx999、dvy precise坐标模式与条件写。下一先建eligibility pure core，再原子接candidate+consumer；跨阶段resource/CPoint/audio/spark/content不混入。

> **2026-09-06 B5 unarmored HP消费累计已验证：** `NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001 / VERIFIED / UNARMORED_HP_CONSUMPTION_ALIGNED / FULL_RESOURCE_BLOCKED`。type0/1/2/3/4/5按effective HP damage累加`+0x34C`，type6跳过，actual+HitPlan闭合；red13/15、focused15、HitPlan184、B5 371、broad836、03:33:54Z SelfCheck、Console/Scene/diff/Ledger PASS。下一B5 remaining exit audit；完整MP resource仍受三项跨阶段依赖阻塞。

> **2026-09-06 B5 hit-resource production readiness复审已闭合：** `NTSD28-B5-HIT-RESOURCE-PRODUCTION-READINESS-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / THREE_BLOCKERS_REMAIN / HP_CONSUMPTION_READY`。stats/type1 blockers已解除；完整MP transaction仍等baseMax H/B11、mode B8/H、child suppression B7/C25b，cpoint归B6。下一独立unarmored +0x34C production。

> **2026-09-06 B5 type1 armor专属规则族允许退出：** `NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / TYPE1_ARMOR_SPECIFIC_FAMILY_EXIT_READY`。selection/match/activation/reduced/fallback/break/runtime recovery/actual+HitPlan已闭合，无第二production owner。cross-family resource/audio/spark/content仍阻止4.7完成；下一hit-resource production readiness audit 002。

> **2026-09-06 B5 type1 armor break/vertical order已闭合：** `NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001 / VERIFIED / BREAK_VERTICAL_POSTHIT_ORDER_ALIGNED`。broken fallback现按horizontal→break→vertical→attacker post-hit提交actual+HitPlan；red2/3、focused3、HitPlan184、B5 356、broad821、SelfCheck/Console/Scene/diff/Ledger PASS。下一exit audit 002。

> **2026-09-06 B5 type1 armor首次退出审计已闭合但family未退出：** `NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / BREAK_VERTICAL_ORDER_ROUTED`。actual在horizontal/break前可能先写vertical，HitPlan在break后先写attacker post-hit再写vertical；下一`NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001`。跨族resource/audio/spark/content仍独立。

> **2026-09-06 B5 type1 armor atomic production integration已闭合：** `NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED / TYPE1_ARMOR_CORE_TRANSACTION_ALIGNED`。defense优先、type1 match/activation、selected reduced、unarmored fallback与broken `-1→action→0`已原子接actual+HitPlan；最终focused10、B5 353、HitPlan184、broad818、SelfCheck/Console/Scene/diff/Ledger PASS。下一type1 armor exit audit；resource/audio/spark/content保持独立。

> **2026-09-06 B5 type1 armor HitPlan runtime carriers已闭合：** `NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001 / VERIFIED / HITPLAN_CARRIERS_READY / ARMOR_TRANSACTION_UNCONNECTED`。runtime armor HP与input HP/MP消费累计已进入writer-effect capture/compare；red4、focused4、B5 343、broad808、SelfCheck/Console/Scene/Ledger PASS。下一type1 armor atomic production integration；本包未接selection/damage/content/Scene。

> **2026-09-06 B5 definition attacking carrier已闭合：** `NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001 / VERIFIED / DATA_CARRIER_READY / PRODUCTION_CONSUMPTION_UNCONNECTED`。已补`stats.attacking` typed field与正式converter；red4、focused4、B5 328、broad804、SelfCheck/Console/Scene/Ledger PASS。下一HitPlan runtime armor/consumption carriers；本包未接命中行为、content或Scene。

> **2026-09-06 B5 type1 armor原子生产审计已闭合：** `NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PREREQUISITES_ROUTED`。actual+HitPlan布尔分流不足；先补definition attacking，再补HitPlan runtime armor/consumption carriers，最后原子接selection/activation/reduced/fallback/break。content仍gated。

> **2026-09-06 B5 type1 armor runtime profile接线已验证：** `NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001 / VERIFIED / PRODUCTION_PROFILE_CONNECTED / HIT_SELECTION_UNCONNECTED`。首armor profile、ModuleBind出生/reuse、C25i生产读取与snapshot-skip已闭合；red7、focused8、B5 324、broad800、01:06:53Z SelfCheck、Console/Scene/Ledger PASS。下一atomic production audit；未接selection/damage/content。

> **2026-09-06 B5 type1 armor runtime初始化审计已闭合：** `NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`。Authority出生/C25i均读首armor；Unity carrier/kernel存在但profile固定false、出生仍0/-1。下一runtime-profile integration；break tail留atomic hit，content继续gated。

> **2026-09-06 B5 type1 armor activation pure core已验证：** `NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。MP两级64位成本、最低1、exact availability与runtime armor HP严格破甲`-1`已闭合；red11、focused12、B5 316、broad792、00:49:44Z SelfCheck、Console/Scene/Ledger PASS。下一runtime-initialization audit；未接production/content。

> **2026-09-06 B5 type1 armor match pure core已验证：** `NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。kind/facing/strict threshold/effect-id bypass/frame-state OR/2.8.3.3 invalid-state truth table已闭合；red24、focused25、B5 304、broad780、00:36:52Z SelfCheck、Console/Scene/Ledger PASS。下一activation pure core；未接production/content。

> **2026-09-06 B5 type1 armor data contract已验证：** `NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001 / VERIFIED / DATA_CONTRACT_READY / PRODUCTION_SELECTION_UNCONNECTED`。ArmorRecord28完整typed model、frame双整数parser、last-win/fallback converter、deep-copy/fingerprint与formal loader seam已闭合；red6、focused7、B5 279、broad755、00:19:49Z SelfCheck、Console/Scene/Ledger PASS。下一type1 armor match pure core；activation/content仍未接。

> **2026-09-06 B5 ordinary-defense/null-armor family允许退出：** `NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NULL_ARMOR_FAMILY_EXIT_READY`。actual两入口、HitPlan consumers及damage/rest pure owner已闭合；旧selection启发式无production残留。下一type1 armor data contract；本审计无code/content/Scene修改。

> **2026-09-06 B5 ordinary-defense/null-armor reduced-hit生产接线已验证：** `NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED / NULL_ARMOR_DEFENSE_ALIGNED`。actual selector/DamageWriter/HitPlan已原子接入current-state defense、raw `/10` + target scale及exact reduced rest；red3、focused3、B5 272、HitPlan184、NTSD28 broad748、23:53:08Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一exit audit；type1 content仍受H/B11门约束。

> **2026-09-06 B5 ITR defense fields carrier已验证：** `NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。ITR `spark/dbdefend` typed parser、CopyFrom、HitPlan projection/fingerprint已闭合；red4、focused4、B5 269、HitPlan184、NTSD28 broad745、23:18:31Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一ordinary-defense/reduced-hit原子生产接线。

> **2026-09-06 B5 reduced-hit damage pure core已验证：** `NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。selected-armor damage、HP/MP分流、runtime armor-HP delta与target +0x340缩放已闭合；red20、focused21、B5 265、HitPlan184、NTSD28 broad741、22:59:44Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一ordinary-defense/reduced-hit原子生产接线。

> **2026-09-06 B5 ordinary-defense pure resolver已验证：** `NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。kind/effect、state7/70/75、HP、facing、spark/dbdefend/dvx与OID822真值表已闭合；red13、focused14、B5+HitPlan439、NTSD28 broad720、22:41:19Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一reduced-hit damage pure。

> **2026-09-06 B5 reduced-hit rest pure resolver已验证：** `NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。default/effect/reduction、signed packed-delay hold、frame-counter flag与direct 4/12/native-byte rest已闭合；red13、focused14、B5+HitPlan425、NTSD28 broad706、22:27:22Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一ordinary-defense pure。

> **2026-09-06 B5 armor/reduced-hit审计已闭合：** `NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED / TYPE1_CONTENT_GATED`。formal armor18（type1-12/type0-6）vs Unity0；selection/damage/rest/HP/tail首差已拆包。下一reduced-hit-rest pure；type1正式content仍受H门约束。

> **2026-09-06 B5 standard-hit rest family允许退出：** `NTSD28-B5-STANDARD-HIT-REST-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / STANDARD_REST_FAMILY_EXIT_READY`。Authority 2 caller、Unity actual 4→one adapter、HitPlan 6→one adapter已闭合；残留固定公式均属于OID300/kind9、alternate/armor或unreachable legacy tail。下一`NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001`。

> **2026-09-06 B5 standard-hit rest production integration已验证：** `NTSD28-B5-STANDARD-HIT-REST-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED / STANDARD_REST_ALIGNED`。character/weapon/special/other/initial pair actual与HitPlan六个standard投影已统一使用pure resolver；red7、focused7、B5+HitPlan411、NTSD28 broad692、22:03:59Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一standard-rest exit audit。

> **2026-09-06 B5 standard-hit rest pure resolver已验证：** `NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。recover/definition-effect/reduction/arest/uint8-vrest allocation-free truth table已闭合；red20、focused21、B5+HitPlan404、exact105/779、21:32:09Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一接actual/HitPlan生产链。

> **2026-09-06 B5 standard-hit rest数据载体已验证：** `NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。typed recover、definition bmp effect和world timing-reduction0..5已进入parser/copy/HitPlan fingerprint/reset/snapshot/restore/checksum/parity，schema8/15/18。red5；focused5、related74、B5+HitPlan383、exact104/758、21:17:06Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一pure resolver；未接selection UI/命中行为。

> **2026-09-06 B5 standard-hit rest/recover审计已闭合：** `NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_RESOLVER_SPLIT_DEFINED`。Unity缺ITR recover、definition effect与world timing-reduction三个typed carrier；正式405 DAT与Unity138 DAT前两者当前均0，default reduction0下现有rest值无首差，但正式nondefault1..5会改变hold/arest/vrest。下一`NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001`；resolver/integration后置，不实现排除的selection UI。

> **2026-09-06 B5 type3-specific family允许退出：** `NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-003 / VERIFIED / GOVERNANCE_ONLY / TYPE3_SPECIFIC_FAMILY_EXIT_READY`。candidate、initial early branch、attacker、generic/transform、late pair/hold和effect handoff已无specific首差。共同standard-rest仍归B5/F11，audio/spark归B10，relation归B6，内容归H；因此不能扩大为具体Type3场景整体完成。下一`NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001`。

> **2026-09-06 B5 type3 matching-pair early branch已验证：** `NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001 / VERIFIED / CLOSED`。initial matching3005/3006现于普通damage/audio/status/hit-record前只写rests+pair reset+hold并return；HitPlan同步。red3/1；focused4、HitPlan183、B5+HitPlan378、exact103/753、20:40:26Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一第三次type3 exit audit；全局standard-rest数值仍归B5/F11。

> **2026-09-06 B5 type3第二次退出审计已闭合但family仍未退出：** `NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / EARLY_BRANCH_DIFFERENCE_ROUTED`。正式`battle_world.cpp:6615-6651`对non-character matching 3005/3006在damage/audio/status/hit-record前只写rests+pair reset+hold并return；Unity当前先完成伤害再尾部reset，造成可观察首差。下一`NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001`；本审计无C#/content/Scene写入。

> **2026-09-06 B5 type3 pair/hold/effect-tail已验证：** `NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001 / VERIFIED / CLOSED`。matching pair现逐实体读action-latch帧`hit_Uj`（0→20）且仅清pending impulse；hold release每次type3 continuation后执行；legacy type3 effect5000/6000/23已退役，HitPlan同步。red6/1；focused7、HitPlan183、B5+HitPlan374、exact102/749、20:14:00Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002`。

> **2026-09-06 B5 type3 post-hit退出审计已闭合但family未退出：** `NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / REMAINING_TAIL_DIFFERENCES_ROUTED`。新确认matching pair应读双方latch-frame hit_Uj且只清pending total、motion-hold release应无条件执行、Unity legacy type3 5000/6000/23 tail无权威分支。下一`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001`；本审计无code/content/Scene写入。

> **2026-09-06 B5 type3 kind-catalog transform已验证：** `NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001 / VERIFIED / TYPE3_LOCKED_KIND_TRANSFORM_ALIGNED`。3 bound×7 respond、direct attacker definition/id/type/group/owner、action/latch/Prev40、pending-total-only与transformed-definition effect projection已闭合；active209扫描退役。red4/7+effect red1→focused190、B5+HitPlan367、exact101/742、19:31:26Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001`。

> **2026-09-06 B5 type3 kind-catalog transform审计已闭合：** `NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SEAM_DEFINED`。正式kind.dat SHA39E30DF8、唯一effect209/frame40、bound3/respond7及playable加载链已闭合；Unity candidate gate等价，transform确认错误依赖active209并错写identity/owner/history/motion/weapon count。carrier已齐，无需部署kind.dat。下一`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001`；本审计无code/content/Scene写入。

> **2026-09-06 B5 type3 target generic continuation已验证：** `NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001 / VERIFIED / TYPE3_TARGET_GENERIC_CONTINUATION_ALIGNED`。非kind candidate的state3005 skip、direct/active-parent ownership、仅清pending total并保留count/runtime velocity、hit_Fj/Uj及30/20 fallback、actual/hit-plan TargetOwnerSlot均已闭合；red7/11→focused11、HitPlan182、B5+HitPlan359、exact100/735、18:29:12Z SelfCheck、filtered CS0、Ledger PASS、Scene unchanged。下一`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001`；locked kind transform仍未关闭。

> **2026-09-06 B5 type3 target前置载体审计已闭合：** `NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / RUNTIME_CARRIERS_READY`。B0已验证OwnerSlotIndex/AnimCounter，RelationTeam/HitConfirm2与KnockbackXYZ+HitCount也可直接承载；当前缺口是生产事务及hit-plan TargetOwnerSlot投影。下一直接实施generic continuation，kind catalog后置。

> **2026-09-06 B5 type3 attacker post-hit action已验证：** `NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001 / VERIFIED / TYPE3_ATTACKER_POST_HIT_ACTION_ALIGNED`。frame cover、state3000/state3007-cover、hit_Fj仅0回退10、selected dvx→Z及四条actual/hit-plan已闭合；red9→focused11、B5+hit-plan348、exact99/724、17:34:52Z SelfCheck、filtered CS0、Ledger PASS。下一target continuation前置载体审计。

> **2026-09-06 B5 type3 post-hit action审计已闭合：** `NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`。攻击者固定10、错误dvz/漏Z、缺state3007 cover；target固定20/30、owner/control/impulse事务及kind catalog均确认不等价。Authority corpus state3000=902、同帧hit_Fj=187。下一先实施攻击者hit_Fj/dvx独立包。

> **2026-09-06 B5 kind0 direct post-effect action已验证：** `NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001 / VERIFIED / KIND0_DIRECT_POST_EFFECT_ACTION_ALIGNED`。3/30→200、2/21/22/合格20→203、previous-state gate、final-X facing及actual/hit-plan已闭合；red10/6→focused16、related336、clean exact98/713、16:25:18Z SelfCheck、Console0、Ledger PASS。下一type3 post-hit action审计。

> **2026-09-05 B5 remaining FallDamageDiv审计已闭合：** `NTSD28-B5-REMAINING-FALLDAMAGEDIV-CONSUMER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / REMAINING_OWNERS_ROUTED`。alternate→armor/H、cpoint→B6、landing/recovery→B4/B7、results→B8；另确认Unity itr.kind16伤害/旋风无2.8 authority分支，effect16是另一字段。下一退役kind16生产路由。

> **2026-09-05 B5 nonchar unarmored damage-scale consumer已验证：** `NTSD28-B5-NONCHAR-UNARMORED-DAMAGE-SCALE-CONSUMER-001 / VERIFIED / TYPE1_5_UNARMORED_SCALE_WEAK_ALIGNED / TYPE6_SKIP_VERIFIED`。weapon type1/2/4及type3/5 normal、hit-plan projection已迁`+340 -> weak/2`；raw durability/display与type6 skip保持。red5→focused6、related290、精确NTSD28 broad670、14:37:47Z SelfCheck，Console/Scene/Ledger通过。下一剩余FallDamageDiv consumer审计。

> **2026-09-05 B5 nonchar damage-scale owner审计已闭合：** `NTSD28-B5-NONCHAR-DAMAGE-SCALE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NONCHAR_MIGRATION_SEAM_DEFINED`。Authority只以`IncomingDamageScale340`作为伤害除数；Unity `FallDamageDiv`是Results等旧owner的独立遗留字段，不能双轨/双重缩放。下一迁weapon/type3 standard；其余consumer分流B4/B6/B8/B5。

> **2026-09-05 B5 type0 unarmored damage-scale consumer已验证：** `NTSD28-B5-TYPE0-UNARMORED-DAMAGE-SCALE-CONSUMER-001 / VERIFIED / TYPE0_UNARMORED_SCALE_WEAK_ALIGNED`。32位`target +340 -> attacker weak/2`、effective HP/stat/KO与raw display分离闭合；red4→focused9、related284、精确NTSD28 broad664、14:14:56Z SelfCheck，Console/Scene/Ledger通过。下一nonchar scale owner审计；producer/armor/effect后置。

> **2026-09-05 B5 damage-scale/effect审计已闭合：** `NTSD28-B5-DAMAGE-SCALE-EFFECT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / DAMAGE_AND_EFFECT_SPLIT_DEFINED`。Authority unarmored严格`target +340 -> attacker weak/2`，selected armor只+340；Unity三条standard damage owner均未按此消费。下一unarmored consumer；producer/armor/effect eligibility/post-action/override/audio/spark独立处理。

> **2026-09-05 B5 resource production readiness审计已闭合：** `NTSD28-B5-RESOURCE-PRODUCTION-READINESS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PRODUCTION_DEPENDENCIES_ROUTED`。pure core/resolver/world rules/F6已ready；definition attacking/baseMax、mode override、child suppression、armor及cpoint六类依赖分别路由H/B11、B8/H、B7和B6，禁止局部接入伪造0值。下一damage-scale/effect审计。

> **2026-09-05 B5 F6 resource gate projection已验证：** `NTSD28-B5-F6-RESOURCE-GATE-PROJECTION-001 / VERIFIED / ACTIVE_AND_REGISTRATION_PROJECTION_ALIGNED`。accepted gate-change同tick occupied-slot投影、locked no-op、成功registration继承、entity/raw同步和zero allocation闭合；red3→focused5、clean related69+isolated1、精确NTSD28 broad655、13:49:24Z SelfCheck，Console/Scene/Ledger通过。下一resource production readiness审计；mode override仍后置。

> **2026-09-05 B5 world hit-resource rules carrier已验证：** `NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001 / VERIFIED / CARRIER_READY / F6_PROJECTION_DEFERRED`。world numeric `1C/34/38`默认/reset、snapshot/restore、checksum/parity及schema7/14/17闭合；red24→focused16、related59、精确NTSD28 broad650、13:29:18Z SelfCheck，Console/Scene/Ledger通过。下一F6 active projection与registration inheritance；mode override/production后置。

> **2026-09-05 B5 world resource rules/F6审计已闭合：** `NTSD28-B5-WORLD-RESOURCE-RULES-F6-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WORLD_RULES_SPLIT_DEFINED`。FunctionKeys保持唯一local gate；1C/34/38建numeric world carrier；F6 active projection与registration inheritance独立接；selected-mode override归B8/H。下一rules carrier。

> **2026-09-05 B5 resource-attacker resolver已验证：** `NTSD28-B5-RESOURCE-ATTACKER-RESOLVER-001 / VERIFIED / RESOLVER_READY / PRODUCTION_UNCONNECTED`。active physical slot、negative/self terminal、两跳上限、missing-next fail-closed与stable/holder字段隔离闭合；red8→focused7、相关302、精确NTSD28 broad645、13:03:42Z SelfCheck；Scene unchanged。下一world rules/F6审计。

> **2026-09-05 B5 hit-resource suppression carrier已验证：** `NTSD28-B5-HIT-RESOURCE-SUPPRESSION-CARRIER-001 / VERIFIED / CARRIER_READY / PRODUCER_DEFERRED`。default/input/full reset、copy、snapshot、checksum、full parity及schema9/13/16闭合；red12→focused6、相关69、精确NTSD28 broad638、12:49:59Z SelfCheck；Scene unchanged。下一resource-attacker resolver；child producer仍归B7/C25b。

> **2026-09-05 B5 resource carrier/attribution审计已闭合：** `NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_ATTRIBUTION_SPLIT_DEFINED`。resource attacker固定`OwnerSlotIndex`两跳、missing next失败；suppression缺carrier；F6缺world transaction投影；stats.attacking/baseMax保持B11/H gate。下一suppression carrier。

> **2026-09-05 B5 resource transaction pure core已验证：** `NTSD28-B5-RESOURCE-TRANSACTION-PURE-CORE-001 / VERIFIED / PURE_TRANSACTION_READY / PRODUCTION_UNCONNECTED`。local gate、type0 reward、suppression、drain/gain all-or-nothing与reward→drain→gain顺序闭合；red6→focused7、related256、精确NTSD28 broad632、12:26:10Z SelfCheck；Scene unchanged。下一carrier/attribution审计，production仍未接。

> **2026-09-05 B5 resource injury pure core已验证：** `NTSD28-B5-RESOURCE-INJURY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。double、definition/mode优先、49/50 rounding、negative remainder与32位overflow闭合；red3→focused9、related249、精确NTSD28 broad625、12:12:35Z SelfCheck；Scene unchanged。production仍等待carrier；下一resource transaction pure core。

> **2026-09-05 B5 hit display-step producer已验证：** `NTSD28-B5-HIT-DISPLAY-STEP-PRODUCER-001 / VERIFIED / TYPE0_5_DISPLAY_STEPS_ALIGNED / TYPE6_SKIP_VERIFIED`。positive `/10,/10,/10,/20`、zero/negative preserve、type0..5 production与type6 skip闭合；red2→focused10、related255、精确NTSD28 broad616、12:00:34Z SelfCheck；Scene unchanged。下一resource injury pure core。

> **2026-09-05 B5 hit-resource前置审计已闭合：** `NTSD28-B5-HIT-RESOURCE-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / RESOURCE_SPLIT_DEFINED`。四display-step写入位于local/F6 gate前且carrier ready，可立即实施；resource rules/F6/attribution另包，armor/weapon-strength保持H/B11 gate，effect/damage scale另审。下一display-step producer。

> **2026-09-05 B5 non-type0 hit-motion arm已验证：** `NTSD28-B5-NONTYPE0-HIT-MOTION-ARM-001 / VERIFIED / TYPE1_6_HIT_MOTION_ARM_ALIGNED`。weapon/special/other均在reaction后、horizontal前接arm；type1..6 armed、type6 status-skip+arm及strict bypass闭合；red7/pass1→focused8、完整hit related241、精确NTSD28 broad606、11:45:00Z SelfCheck；Scene unchanged。producer family闭合，下一审计B5 resource/armor/effect剩余链。

> **2026-09-05 B5 non-type0 encoded+join已验证：** `NTSD28-B5-NONTYPE0-ENCODED-JOIN-001 / VERIFIED / TYPE1_5_ENCODED_JOIN_ALIGNED / TYPE6_SKIP_VERIFIED`。type1/2/4 weapon与type3/5 special/other已接producer+join，non-type0 mimic不enable，type6保持zero-RNG/status/join；red11→focused11、完整hit related233、精确NTSD28 broad598、11:30:59Z SelfCheck；Scene unchanged。下一non-type0 arm。

> **2026-09-05 B5其他target producer审计已闭合：** `NTSD28-B5-OTHER-TARGET-PRODUCER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / OTHER_TARGET_SPLIT_DEFINED`。type1/2/3/4/5执行encoded+join，type6跳过；type1..6全部执行arm。Unity weapon/special/other owners与两包拆分已闭合；下一non-type0 encoded+join，resource/armor/effect另包。

> **2026-09-05 B5 type0 join/mimic即时副作用已验证：** `NTSD28-B5-TYPE0-JOIN-MIMIC-SIDE-EFFECTS-001 / VERIFIED / TYPE0_JOIN_MIMIC_IMMEDIATE_EFFECTS_ALIGNED / OTHER_TYPES_DEFERRED`。join gate/re-entry/original group与mimic type/counter/enabled/source slot、encoded→same-hit activation闭合；red5→focused10、related44、精确NTSD28 broad587、11:13:36Z SelfCheck；Scene unchanged。下一审计其他target type confirmed-hit生产链。

> **2026-09-05 B5 type0 hit-motion arm已验证：** `NTSD28-B5-TYPE0-HIT-MOTION-ARM-001 / VERIFIED / TYPE0_HIT_MOTION_ARM_ALIGNED / OTHER_TYPES_DEFERRED`。strict bypass、-5/+5边界、pending-Z、facing/motion/gain/default/explicit action及production order闭合；red5→focused10、related34、精确NTSD28 broad577、11:00:36Z SelfCheck；Scene unchanged。下一type0 join/mimic immediate side effects；其他target type/B8后置。

> **2026-09-05 B5 type0 encoded status已验证：** `NTSD28-B5-TYPE0-ENCODED-STATUS-001 / VERIFIED / TYPE0_ENCODED_PRODUCER_ALIGNED / ARM_AND_OTHER_TYPES_DEFERRED`。red3→focused4、related216、精确NTSD28 broad567、10:42:11Z SelfCheck；Scene/Console/Ledger通过。下一type0 arm；join-mimic/other types后置。

> **2026-09-05 B5 ITR status fields已验证：** `NTSD28-B5-ITR-STATUS-FIELDS-001 / VERIFIED / ITR_STATUS_CONTRACT_READY / PRODUCER_DEFERRED`。red53→focused5、related195、精确NTSD28 broad563、09:02:27Z SelfCheck；Scene/Console/Ledger通过。13-field model/converter/copy/ECS contract闭合；producer/content后置。

> **2026-09-05 B5 status producer审计已闭合：** `NTSD28-B5-STATUS-PRODUCER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PRODUCER_SPLIT_DEFINED`。确认encoded+arm两producer、13个缺失ITR字段、strict RNG顺序与type0 owner；Unity冻结内容0条、authority内容92条，内容仍hold。下一ITR carrier。

> **2026-09-05 state12/18 environment credit已验证：** `NTSD28-B4-F04-STATE12-18-ENVIRONMENT-CREDIT-001 / VERIFIED / DAMAGE_CREDIT_COUNT_ALIGNED / B8_EVENT_DEFERRED`。behavior red6/7→focused7、related71、精确NTSD28 broad558、08:45:39Z SelfCheck；Scene/Console/Ledger通过。contact前abs/scale damage、source decode、two-owner credit、score/lethal KO count闭合；B8 event/B5 producer后置。

> **2026-09-05 state12/18 contact action已验证：** `NTSD28-B4-F04-STATE12-18-CONTACT-ACTION-001 / VERIFIED / CONTACT_ACTION_SINGLE_OWNER / ENVIRONMENT_DAMAGE_DEFERRED`。red→final focused9、related64、精确NTSD28 broad551；SelfCheck更正shared/exact旧WeaponCount damage夹具x4后08:32:06Z PASS，Scene/Console/Ledger通过。下一environment damage/credit；B5/B8后置。

> **2026-09-05 hard-motion consumer已验证：** `NTSD28-B4-F04-HARD-MOTION-CONSUMER-001 / VERIFIED / PURE_KERNEL_READY / PRODUCTION_TRANSACTION_DEFERRED`。red7→focused15、related55、精确NTSD28 broad542、08:12:40Z SelfCheck；Scene/Console/Ledger通过。下一接state12/18 contact transaction；B5 producer/B8 event后置。

> **2026-09-05 status motion carrier已验证：** `NTSD28-B4-F04-STATUS-MOTION-CARRIER-001 / VERIFIED / CARRIER_READY / PRODUCER_CONSUMER_DEFERRED`。red21→final focused6、related60、精确NTSD28 broad527、08:00:57Z SelfCheck；Scene/Console/Ledger通过。7-field reset/copy/snapshot/checksum/parity已闭合，raw保留48-leaf exact schema；下一hard-motion consumer，B5 producer/B8 event后置。

> **2026-09-05 state12/18 transaction前置审计已闭合：** `NTSD28-B4-F04-STATE12-18-TRANSACTION-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_OWNER_SPLIT_DEFINED`。缺7个status motion/action carrier；env/credit计数多数已有，KO event sink缺失。下一先建carrier；producer归B5，event归B8。

> **2026-09-05 state12/18 airborne selector已验证：** `NTSD28-B4-F04-STATE12-18-AIRBORNE-001 / VERIFIED / SINGLE_AIRBORNE_SELECTOR / LANDING_TRANSACTION_PENDING`。compile-red→focused17、related99、broad521、07:26:33Z SelfCheck；Scene/Console/Ledger PASS。exact/shared现读Airborne/Env320/upcoming phase且不reset counter；下一landing transaction prerequisite audit。

> **2026-09-05 state12/18 airborne审计已闭合：** `NTSD28-B4-F04-STATE12-18-AIRBORNE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SELECTOR_SEAM_DEFINED`。Unity错读WeaponCount、tickIndex和absolute y；Authority读Env320、C23前upcoming phase与effective-floor Airborne。下一实现。

> **2026-09-05 type0 ordinary landing已验证：** `NTSD28-B4-F04-TYPE0-ORDINARY-LANDING-001 / VERIFIED / TYPE0_ORDINARY_SINGLE_BODY / STATE12_18_PENDING`。red2/6→focused6、related77、final broad504；SelfCheck捕获并更正旧state13 Frozen特判夹具后07:08:54Z PASS，Scene/Console/Ledger PASS。下一state12/18 airborne selector审计。

> **2026-09-05 type0 action审计已闭合：** `NTSD28-B4-F04-TYPE0-ACTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TYPE0_SPLIT_DEFINED`。ordinary landing首差为遗漏hit_g和negative floor被handler重写0；state12/18另有upcoming phase、contact-side transaction、environment damage/credit及缺失hit-motion carriers。下一先ordinary single body，不混大事务。

> **2026-09-05 type3/OID999与remaining nonchar reference已验证：** `NTSD28-B4-F04-TYPE3-OID999-REFERENCE-001 / VERIFIED / HIT_G_AND_REMAINING_NONCHAR_REFERENCE / TYPE0_PRODUCERS_AUDIO_PENDING`。先发现并补齐hit_g carrier；behavior red6→focused7、related93、final broad498。SelfCheck两次捕获旧y0 OID999夹具并更正后06:44:53Z PASS；Scene/Console/Ledger PASS。下一type0 action审计。

> **2026-09-05 type3/OID999审计已闭合：** `NTSD28-B4-F04-TYPE3-OID999-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / REMAINING_NONCHAR_SEAM_DEFINED`。正式type3 special helper先执行，real OID999 `<-9`再以+9 Y/Vx/Vy覆盖并保留Vz；alias999不触发。普通type5也必须消费reference core。Unity旧OID999 y0/全停相反。

> **2026-09-05 derived weapon reference迁移已验证：** `NTSD28-B4-F04-DERIVED-WEAPON-REFERENCE-001 / VERIFIED / DERIVED_TYPE1_2_4_6_REFERENCE / TYPE3_OID999_PENDING`。red5→focused5、related59；首轮broad仅受MCP disposed-stream日志污染，对应组5/5后最终无中途连接broad491/491；06:21:49Z SelfCheck、Scene/Console/Ledger PASS。下一type3/OID999。

> **2026-09-05 derived weapon owner审计已闭合：** `NTSD28-B4-F04-DERIVED-WEAPON-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / MIGRATION_SEAM_DEFINED`。正常pooled LF2Weapon仍走legacy bool/y0 owner；absolute-Y in-flight gate与四参landing wrapper均漏掉reference。下一以同调用栈virtual overload迁移，不新增snapshot字段；type3/OID999/producer另包。

> **2026-09-05 shared type2/4/6 reference landing已验证：** `NTSD28-B4-F04-SHARED-TYPE2-4-6-REFERENCE-001 / VERIFIED / SHARED_TYPE2_4_6_REFERENCE / DERIVED_AND_OID999_PENDING`。red6→focused6、related31、broad486、06:03:19Z SelfCheck；Scene/Console/Ledger PASS。shared type2/4/6 已消费 negative collision-Y 与各自 strict predicate；下一审计 derived owner，type3/OID999/producer仍待。

> **2026-09-05 shared type1 reference landing已验证：** `NTSD28-B4-F04-SHARED-TYPE1-REFERENCE-001 / VERIFIED / SHARED_TYPE1_REFERENCE / DERIVED_AND_OTHER_TYPES_PENDING`。初次red因夹具fallback type5作废；更正后green2、focused4、related48、broad480、05:47:11Z SelfCheck，Scene/Console/Ledger PASS。无分配result core与shared type1已消费negative collision-Y；下一type2/4/6，derived与OID999另包。

> **2026-09-05 non-character result seam审计已闭合：** `NTSD28-B4-F04-NONCHARACTER-RESULT-SEAM-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SHARED_FIRST_SPLIT`。现有bool丢失previous/contact/effective-floor；3类caller已闭合。下一先新增result core并迁shared path，derived/on-landed、direct wrapper、OID999后置。

> **2026-09-05 identity X extras已验证：** `NTSD28-B4-F04-IDENTITY-X-EXTRAS-001 / VERIFIED / INDEPENDENT_IDENTITY_EXTRAS / FLOOR_LANDING_PENDING`。red3/6→focused6、related44、broad476、05:27:46Z SelfCheck；Scene/Console/Ledger PASS。type4/real-or-alias120 add与real-or-alias101 subtract现独立执行；下一non-character result seam审计。

> **2026-09-05 non-character physics审计已闭合：** `NTSD28-B4-F04-NONCHARACTER-PHYSICS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。完整landing依赖contact/effective-floor result seam；首个独立差异为derived weapon的identity X extras用了else-if且遗漏real ID。下一identity小包。

> **2026-09-05 type0 reference-aware physics core已验证：** `NTSD28-B4-F04-TYPE0-PHYSICS-CORE-001 / VERIFIED / TYPE0_CORE / ACTIONS_AND_PRODUCERS_PENDING`。red3/5→focused5、related33、broad470；SelfCheck两轮捕获并更正旧epsilon/contact-side夹具后05:16:05Z PASS。Scene/Console/Ledger PASS。landing action、non-character与producer仍待；下一non-character core审计。

> **2026-09-05 type0 physics core审计已闭合：** `NTSD28-B4-F04-TYPE0-PHYSICS-CORE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。首差为Unity零地面假设及缺previous<floor strict crossing；新carrier可由现有type0/shared-character owner直接消费。下一test-first core；无code写入。

> **2026-09-05 B4 F04 collision-Y carrier已验证：** `NTSD28-B4-F04-COLLISION-Y-CARRIER-001 / VERIFIED / CARRIER_READY / PRODUCERS_UNCONNECTED`。red14→focused6、联合29、raw fixture3、tool trace21/raw5、final broad465、04:58:24Z SelfCheck；Scene/Console/Ledger PASS。schema7/11/14，raw42/6；operation30、physics、teleport、next999、input/hit均未接。下一审计physics core消费。

> **2026-09-05 collision-Y owner审计已闭合：** `NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_BOUNDARY_DEFINED`。Authority C11每tick清0并由operation30保留最低reference，linked dvy与defusion也可写；physics/teleport/next999/input/fusion/hit读取。Unity无等价字段，下一先建carrier deterministic lifecycle，producer/consumer按B4/B5/B6另包。

> **2026-09-05 B4 F04 type1 landing branch已验证：** `NTSD28-B4-F04-TYPE1-LANDING-001 / VERIFIED / TYPE1_THRESHOLD_BRANCH / FULL_PHYSICS_PENDING`。red1/3准确复现12.0 expected70/actual7；正式巨大threshold+strict `>`已接入production。focused4、related33、final broad459、04:35:25Z SelfCheck、Scene/Console/Ledger PASS。collision-Y、其余type、统一integrator与Audio均未关闭；下一collision-Y carrier审计。

> **2026-09-05 B4 F04 physics owner审计已闭合：** `NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PHYSICS_SPLIT_DEFINED`。已完成gate、XYZ、friction/floor、gravity、type0/1/2/3/4/6 landing和integer sync矩阵；首个不依赖缺失collision-Y carrier的独立差异为type1/state1002阈值`9.9` vs正式`5.2571022450498032e120`。下一type1小包；完整F04仍未对齐。

> **2026-09-05 B4 F02 frame-motion kernel已验证：** `NTSD28-B4-F02-FRAME-MOTION-KERNEL-001 / VERIFIED / PRODUCTION_NATIVE_KERNEL / STRICT_DEPTH_INTENT`。production C04统一XYZ公式，Up/Down strict XOR；双按不再按cooldown写Vz，legacy direct保持。red1→focused5、related45、broad455、04:12:57Z SelfCheck、Scene/Console/Ledger PASS。下一F04 physics audit。

> **2026-09-05 B4 frame-motion入口首差已闭合：** `NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIRST_DIFFERENCE_DEPTH_INTENT`。XYZ阈值/偏置/X facing clamp主体匹配；Authority Up/Down严格互斥、双按none，Unity cooldown tie-break双按仍写Z是首差。下一`NTSD28-B4-F02-FRAME-MOTION-KERNEL-001`；无code/content/Scene/Authority写入。

> **2026-09-05 B3 placement退出门已通过、完全关闭后置：** `NTSD28-B3-EXIT-GATE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / B3_PLACEMENT_EXIT_READY / FULL_CLOSE_DEFERRED`。允许进入B4，但C25后临时SerialTickAll仍有special state/death、snapshot/state9998 cleanup；global post-tail仍有F7/carrier cleanup。分别路由B4/B5/B7/B8/B10/B11/H，接管前不删除、不宣称B3 complete。下一B4 entry audit；无code/content/Scene/Authority写入。

> **2026-09-05 C25m survivor commit已验证：** `NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001 / VERIFIED / SURVIVOR_COMMIT / TERMINAL_PATH_PENDING_B7`。C25l后立即commit，virtual tail通过try/finally old-Prev context保留state13/200且不绕过LF2Character/探针；C25p在snapshot前。red4→focused3；旧placement断言按l→m→cleanup更正，related69、broad450、03:58:35Z SelfCheck、Scene/Console/Ledger PASS。terminal路径留B7/C25o；下一B3 exit gate audit。

> **2026-09-05 C25m virtual-tail覆写审计已闭合：** `NTSD28-B3-C25M-PREVIOUS-ACTION-OVERRIDE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TRANSIENT_CONTEXT_DEFINED`。生产LF2Character及诊断覆写均需保留virtual dispatch；下一实现以调用栈内old-Prev override让state13/200兼容branch继续读提交前值，同时把C25m commit移到C25l后/cleanup前。terminal early exit留B7/C25o。无code/content/Scene/Authority写入。

> **2026-09-05 C25l程序顺序已验证、资源运行时待验：** `NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001 / RUNTIME_PENDING / PROGRAMMATIC_ORDER_VERIFIED / RESOURCE_SPAWN_PENDING`。state18/19 branch已在OPoint后、cleanup/C25m前；state13/200 virtual tail/N30保持。red4→focused4/related63；SelfCheck捕获并修复空任务extra flush，最终18、broad447、03:44:32Z SelfCheck、Scene/Console/Ledger PASS。正式OID999 7/1粒子、slot/RNG tuple Play待H/B11/B7；下一C25m override audit。

> **2026-09-05 C25k/m前置审计已纠正依赖顺序：** `NTSD28-B3-C25K-M-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / DEPENDENCY_ORDER_CORRECTED`。terminal pending由Authority C25g在保留原始code后写入；Unity缺carrier且部分999已丢失，C25k不能单独前移。C25m必须等C25l读取旧Prev。下一改为C25l/m；C25k并入B7 terminal producer/carrier/C25o联合包。无code/content/Scene/Authority写入。

> **2026-09-05 C25p healing owner已验证：** `NTSD28-B3-C25P-HEALING-OWNER-001 / VERIFIED / PER_SLOT_HEALING_OWNER / GLOBAL_DUPLICATE_REMOVED`。type0 living survivor现于dynamic slot tail推进encoded→ordinary→state1700；global post-tail只保留F7/carrier/transient/snapshot。red3→focused6；related32+44、NTSD28 broad443、03:27:04Z SelfCheck；Scene unchanged/Console0/Ledger PASS。C25k-o未改；下一C25k/m。

> **2026-09-05 C25k-p owner审计已闭合：** `NTSD28-B3-C25K-P-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`。Authority同槽k→l→m→n→o→p与o消费后跳p已闭合；Unity确认early lifecycle、mixed transition、late Prev commit、missing weapon pieces/pending carrier及global healing差异。先独立迁C25p；k/m、l、n/o分包，n/o依赖B7/B10/B11/H。本审计无code/content/Scene/Authority写入。

> **2026-09-05 C25i armor recovery core已验证：** `NTSD28-B3-C25I-ARMOR-RECOVERY-001 / VERIFIED / PROGRAMMATIC_CORE_AND_PLACEMENT / FORMAL_CONTENT_PENDING`。C25h→i→j production顺序、timer/reload/gate与默认false profile seam闭合。red8→focused5；一次真实C25h误早退回归14项被原测试捕获并修复，最终related96、NTSD28 broad437、11:05:02 SelfCheck；Scene unchanged/Console0。正式armor schema/content/hit仍归B5+H/B11；下一C25k-p。

> **2026-09-05 C25g common frame body已验证：** `NTSD28-B3-C25G-FRAME-BODY-001 / VERIFIED / DOWNSTREAM_BEHAVIOR_ROUTED`。exact/fallback已共用单一core；production terminal state14 hold、type3 state3007与Unity-only heavy早退闭合。red4→focused4、related85、NTSD28 broad432、10:50:29 SelfCheck；Scene unchanged/Console0。collision-Y/cost/pending/audio/content仍归B4/B7/B10/B11/H；下一C25i。

> **2026-09-05 C25g frame-body owner审计已闭合：** `NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。C25g已位于同一live-slot的C25f→g→h结构，不再新增或移动writer；但terminal retain、type3 state3007、next999 collision-Y、blink、negative transition cost、pending/free与multi-sound仍有行为差异，已唯一分流到B4/B7/B10/B11/H。下一C25i armor recovery owner审计；本包无code/asset/Scene/Authority写入。

> **2026-09-05 AI render-phase consumers已验证：** `NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001 / VERIFIED / PHYSICAL_Y_PRESERVED`。target role/index、abnormal/C8、held blocker与synchronized state17已从Y迁到verified HitStop；3组旧Y fixture同步闭合。red4→compile0；focused121、all-AI385、NTSD28 broad428、10:29:46 SelfCheck PASS；Scene unchanged、Console0。下一C25g frame body / C25i armor recovery。

> **2026-09-05 AI render-phase consumer审计已闭合：** `NTSD28-B3-AI-RENDER-PHASE-CONSUMER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CONSUMER_CROSSWALK_COMPLETE`。target role/index、cached/primary、abnormal dispatch、held blocker和synchronized state17的Y误读已逐项定位；AoS/SoA HitStop carrier已存在。角色专项高度Y明确排除。下一`NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001`按互异矩阵test-first实施；本审计无code/asset/Scene/Authority写入。

> **2026-09-05 C25f/h/j timer owners已验证：** `NTSD28-B3-C25F-H-J-TIMER-OWNERS-001 / VERIFIED / C25F-H-J-PRODUCTION-OWNERS / JOINT-RAW-RENDER-PHASE-EQUAL`。production live-slot现按f→g→h→j执行computer refresh、reaction/render、bank/status/poison/cleanup与rest；direct compatibility保留。red13→compile0；focused15、adjacent29、NTSD28 broad425、related38、tool21+5、09:48:48 SelfCheck；renderPhase joint raw仍equal，Scene不变、未Play、Console0。caller审计确认legacy -0.5 recovery不在当前post-C25 production serial中，未留下虚假suppression状态。下一AI render-phase consumer/C25g/C25i。

> **2026-09-05 C25f-j state carriers已验证：** `NTSD28-B3-C25F-J-STATE-CARRIERS-001 / VERIFIED / STATE_CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHMS-EXCLUDED`。12个computer/timer/status/join/poison/armor字段已进入reset/copy/snapshot6/full10/checksum13及独立parity组；raw armor变为41 verified/7 missing并在双端trace equal。red60→compile0；joint66、NTSD28 broad410、tool21+5、09:11:30 SelfCheck；Scene不变、未Play、Console0。算法与内容仍待。

> **2026-09-05 render phase绑定已验证：** `NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001 / VERIFIED / RENDER-PHASE-HITSTOP-BINDING / JOINT-RAW-EQUAL / NO-RUNTIME-BEHAVIOR-CHANGE`。48-field raw新增`combat.renderPhase`；Y/phase互异3tick两端phase均为4/-4→3/-3→2/-2，comparator 6 pairs/288 occurrences将其列为equal。red1/3；tool21+5、focused15、NTSD28 broad406、08:53:55 SelfCheck PASS；Scene不变、未Play、Console0。旧AI Y映射已失效，production consumer与C25h owner仍待后续行为包。

> **2026-09-05 C25f-j字段/owner审计已闭合：** `NTSD28-B3-C25F-J-FIELD-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIELD_OWNER_MATRIX_COMPLETE / NEXT_RENDER_PHASE_BINDING`。C25f-j五段gate/order及Unity writer矩阵已完成。发现HitStop高度吻合render_phase但旧AI用Y的绑定冲突、Bdefend -0.5 vs native -1、十字段bank/positive-HP status无owner、computer/armor载体缺失；Authority armor18（6 hp1）、poison28、delay16而Unity0。无code/content/Scene/Authority写入；下一先做render-phase trace绑定。

> **2026-09-05 C25c-e entity carriers已验证：** `NTSD28-B3-C25C-E-ENTITY-CARRIERS-001 / VERIFIED / ENTITY-CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHM-EXCLUDED`。21个C25c/e status/attribution及C25d value/step载体已进入reset、canonical copy、snapshot5/full9/checksum12和独立`nativeResourceDisplay` parity组。red100→compile0；focused4、snapshot/checksum29、NTSD28 broad405、08:16:42 SelfCheck PASS；Scene不变、未进Play、Console0。这里只闭合确定性载体边界，C25c/d/e算法、B11内容schema、B5/B8 producer仍待。

> **2026-09-05 C25c-e current MP绑定已纠正：** `NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001 / VERIFIED / CURRENT-MP-PP-BINDING / RAW-PROJECTION-CORRECTED / NO-RUNTIME-BEHAVIOR-CHANGE`。MP200/PP173红灯证明旧raw错误输出200；contract/exporter现唯一读PP，未双写或删除旧MP。tool build0/0、21/21、raw5/5、format；Unity compile0、joint49/49、NTSD28 401/401、07:42:52 SelfCheck PASS；contract SHA`1AE87A06...EDF4`。无Play需求，Scene不变、Editor非Play、post-clear Console0。下一C25c-e runtime carriers。

> **2026-09-05 B3 C25c-e字段审计已完成：** `NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001 / VERIFIED / GOVERNANCE_ONLY / FIELD_MATRIX_COMPLETE / CURRENT-MP-CONFLICT-FOUND`。C25d 8字段全缺，PpDisplay不可复用；Authority locked object DAT有max_mp158、cmp7、chp4而Unity schema无承载。更关键：B0 parity/raw把current_mp绑定Runtime.MP，正式action/damage/C06/C25b却用Health.PP，初值500会掩盖冲突。下一先做非零资源joint trace并唯一纠正current MP，再加runtime carriers；B11 gate不变。本审计无code/asset/Scene改动，validator PASS。

> **2026-09-05 B3 C25a-b已验证：** `NTSD28-B3-C25A-B-DEFINITION-CLONE-001 / VERIFIED / C25A-B-PRODUCTION / TARGETED-PLAY-PASS / B11-DEFINITION-STATS-PENDING`。production仅对state8000..8999按旧source next原子切换definition/action，旧9995/4000/render-offset链留direct compatibility；state9996五分身使用2.8 synchronized RNG精确34 callsite，legacy RNG 0 call，native birth字段闭合。red5/6→compile0；focused11、related30、NTSD28 broad399、07:13:17 SelfCheck与Play PASS；Play为OID7/action3/type3、5 clones，artifact SHA`15285774...C903`，Scene不变、Play退出、Console0。target definition max_mp/defend等stats载体留B11；下一C25c-e resource/display。

> **2026-09-05 B3 C25 skeleton已验证：** `NTSD28-B3-C25-NESTED-TAIL-SKELETON-001 / VERIFIED / C25-SINGLE-PRODUCTION-ENTRY / COMPLETED-TICK-RENDER / TARGETED-PLAY-PASS / C25A-P-BEHAVIOR-PENDING`。正常tick C24后立即进入现有dynamic late-slot loop，legacy serial显式后置，Render位于Stage/session tails/Results之后；step-wait旧路径保持。red1/2→compile0；focused2、placement/W05/worker51、presentation14、NTSD28 broad123、06:38:37 SelfCheck及真实Play PASS。tick6 phase25～33 exact、publishedTick6、artifact SHA`8AE8AB88...D2F5`，Scene不变、Play退出、Console0。下一C25a-b definition/special clone；a～p算法仍未整体完成。

> **2026-09-05 B3 C25 writer inventory已闭合：** `NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / C25A-P-WRITERS-MAPPED / NEXT-C25-SKELETON`。修复后Authority C25a～p已逐项映射到Unity `SerialTickAll`、`BattleLateEntityLifecycleModule`与`EntityPostFrameTailAll`。确认Unity动态slot cursor及高slot同tick/低slot下tick基础已有证据，但三个全局scan、early lifecycle、loop-end flush和Render-before-tail均不等价。详细表见`docs/ai/MANIFESTS/NTSD28-B3-C25-WRITER-INVENTORY.md`；下一建立C24后C25 single production entry/skeleton，再分A-B/C-E/F-J/K-P闭合行为。

> **2026-09-05 B3 C23/C24 native world clock已验证：** `NTSD28-B3-C23-C24-WORLD-CLOCK-001 / VERIFIED / C23-C24-SINGLE-OWNER / SNAPSHOT-CHECKSUM-CLOSED / TARGETED-PLAY-PASS / C25-PENDING`。Unity runtime自有phase12/phase3/sequence clock已在C22后、serial前single-owner提交并进入reset、core snapshot6、full snapshot8、checksum11、restore/parity；共享Server package与C25 consumers不改。red20→compile0；focused14、snapshot/checksum78、C04-C24/actual51、06:01:19 SelfCheck及Play PASS。真实tick6为5→6、2→0、5→6，phase24～27=C22/C23/C24/serial，SHA`186F7F02...D05F3`，Scene不变、Play退出、Console0。full/partial34/4；下一结构首差C25 nested tail。

> **2026-09-05 B3 C21/C22 placement已验证：** `NTSD28-B3-C21-C22-PLACEMENT-001 / VERIFIED / C21-C22-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHMS-PENDING-B4-B5-B8`。`PreFrameBounds`与`FramePostProcess`已移到current C20后/serial前；`CurrentWaveStage`及算法不改。red2→compile0；focused2、C04-C22/actual46、related21、05:28:56 SelfCheck与Play PASS。真实tick6 slot50 X=-200→-100、Vx10、HitCount/Knockback0，phase23～27=C21/C22/serial/Stage/Render，SHA`2E3A6570...9AF8A`，cleanup、Scene不变、Play退出、Console0。full/partial32/4；下一结构首差C23/C24 single owners与C25逐slot tail。

> **2026-09-05 B3 residual serial tail rehome已验证：** `NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001 / VERIFIED / C14-C15-ADJACENCY / SERIAL-AFTER-C20 / TARGETED-PLAY-PASS / BODY-PRESERVED`。现有serial caller已从C14与C15之间原样后移到current C20后；C15例外及serial/type3/state9998本体均未改。red2→compile0；focused2、C04-C14/actual44、related289、05:07:12 SelfCheck及Play PASS。真实tick6 hit3，RNG5→6且serial看到6，phase15/16/17/22/23=C14/C15/C16/current-C20/serial，SHA`E3A0B326...29D58`，cleanup、Scene不变、Play退出、Console0。full/partial32/4；下一结构首差推进到C21/C22相对临时serial proxy，C17～C20算法仍归B6、C25逐slot仍未实施。

> **2026-09-05 B3 C14 type-zero hit placement已验证：** `NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001 / VERIFIED / C14-PLACEMENT / TARGETED-PLAY-PASS / HIT-ALGORITHM-PRESERVED`。现有type0 caller已移到C13后/serial前；InteractionPipeline/consumer不改。red2→compile0；focused2、C04-C14/actual42、hit related196、04:33:34 SelfCheck与Play PASS。真实tick6 serial/final均看到3个hit executions，phase14～18=C13/C14/serial/drop/C16；SHA`3695117E...C6A95`，cleanup、Scene不变、Play退出、Console0。旧W06私有阶段断言已按不重复C14修正。full/partial32/4；下一首差是Authority C15 drop对残留serial，先审计serial拆分，不改用户例外本体。

> **2026-09-05 B3 C13 active weapon count placement已验证：** `NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001 / VERIFIED / C13-PLACEMENT / EXACT-TYPE-SET / RANDOM-DROP-EXCEPTION-PRESERVED / TARGETED-PLAY-PASS`。C13在C12后/serial前扫描active current-DAT并只计1/2/4/6；drop正文及all-non-character门槛不改。red6→compile0；focused4、C04-C13/actual40、related13、4096=0B、04:11:29 SelfCheck与Play PASS。真实tick6 baseline0，expected/serial/snapshot/current exact均4且captured tick6；SHA`2D382636...23D9B`，cleanup、Scene不变、Play退出、Console0。full/partial32/4；下一结构首差C14 type-zero hit consume对serial remainder。

> **2026-09-05 B3 C12 fusion barrier placement已验证：** `NTSD28-B3-C12-FUSION-BARRIER-PLACEMENT-001 / VERIFIED / C12-PLACEMENT / TARGETED-PLAY-PASS / EXISTING-ALGORITHM-PRESERVED`。C12已从serial后移到C11后、serial前；OID算法及C25h timer不改。red2→compile0；focused2、C04-C12/actual36、OID+C12 6、03:47:14 SelfCheck及Play PASS。真实tick6中native route frame10→9/state2，随后融合OID7/8→51/frame290，serial看到51，timer4499、partner dormant；result SHA`FFAD6915...504DA`，cleanup、Scene不变、Play退出、目标Play Console0。fixture缺frame9～12导致的初次Play伪失败已留档。full/partial31/4；下一结构首差C13 active weapon count对serial remainder。

> **2026-09-05 B3 C11 candidate transaction placement已验证：** `NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001 / VERIFIED / C11-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHM-PENDING-B5`。rest→PairVRest→CandidateCollect已整体移到C10后、serial前。red2→compile0；focused2、C04-C11/actual34、related28、03:08:13 SelfCheck及Play PASS；serial rest0、pair visit5，SHA3C43ADC9、cleanup、Scene不变、Play退出、Console0。full/partial31/4；下一结构首差C12 fusion对serial remainder。

> **2026-09-05 B3 C10 collision-action snapshot placement已验证：** `NTSD28-B3-C10-COLLISION-ACTION-SNAPSHOT-PLACEMENT-001 / VERIFIED / C10-SNAPSHOT-PLACEMENT / FOLLOWUP-C11-SUPERSEDED-TEMP-REST-BOUNDARY`。snapshot-only仍位于C09后；C10关闭时“rest暂留serial后”的阶段状态已由上方C11取代，当前正式顺序是C10→C11→serial。C10原验证SHA D96A619A保持有效。

> **2026-09-05 B3 C04基础production owner已验证：** `NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001 / VERIFIED / BASIC-C04-OWNER / REAL-PLAY-PASS / FULL-C04-B4-PENDING / NEXT-C05`。现有dvx/dvy/dvz已从production C03与non-character SimTU抽到显式C04全slot唯一owner；direct CharacterInputAll/SimTU默认兼容。red6、compile0、focused11、related211、00:43:10 SelfCheck及kind0 Play tick3～6均PASS，result SHA`5E379F2D...15E21`，Scene不变、Play退出、Console0。full/partial=30/4；完整linked-platform/delay/dxyz留B4，下一首差C05 native teleport/combined EarlyFrameAdvance。

> **2026-09-05 B3 C04/C05/C06 owner审计已闭合：** `NTSD28-B3-FRAME-MOTION-OWNER-AUDIT-001 / VERIFIED / C04-C06-OWNERS-CLOSED / C04-BASIC-IMPLEMENTED`。审计发现的基础C04 owner差异已由上方C04包闭合；完整C04字段、C05 half-cadence/self/Y/state500/501 extra与C06 immediate normalize仍按分包处理。

> **2026-09-05 B3 OID51/52 production拆分已验证：** `NTSD28-B3-OID5152-PRODUCTION-SPLIT-001 / VERIFIED / C12-FUSION / C25H-TIMER / REAL-PLAY-PASS / NEXT-FRAME-MOTION`。production fusion已移到candidate后/hit前并读取pre-decrement timer；正值`Unk338`只在per-slot frame后递减；combined direct兼容入口不变。red4、compile0、focused10、related182、00:14:06 SelfCheck与真实4503-tick OID Play均PASS；merge4499、timer0不提前split、下一C12 split后899/双方PP5。result SHA`D003E910...A2218`，Scene不变、Play退出、Console0。actual full/partial=29/4，下一首差`CoreFrameMotion / EarlyFrameAdvance`。

> **2026-09-04 B3 OID51/52 maintenance placement审计闭合：** `NTSD28-B3-OID5152-MAINTENANCE-PLACEMENT-AUDIT-001 / VERIFIED / C12-C25H-OWNERS-CLOSED / IMPLEMENTED-BY-NTSD28-B3-OID5152-PRODUCTION-SPLIT-001`。Authority C12 fusion读取未递减`+0x338`，C25h才在frame后减timer；当时Unity combined maintenance先减后split且NeedClear也运行的差异，已由后续production split闭合。

> **2026-09-04 B3 early Cooldown writer extraction已验证：** `NTSD28-B3-COOLDOWN-WRITER-EXTRACTION-001 / VERIFIED / C11-C25J-OWNERS / REAL-PLAY-PASS / NEXT-FRAME-MOTION-VS-RUNTIME-MAINTENANCE`。production早期Cooldown occurrence已移除；no-ITR/state1001 rest clear归C11 candidate prelude，frame tick后只在canonical/mirror原本一致时同步`AttackExempt→ItrRest.Arest`，兼容held step12独立mirror。full/partial sequence=29/5。red7、compile0、focused10、related136、23:48:17 SelfCheck PASS；真实kind0 Play tick3～6与age/RNG/cleanup PASS，result SHA0945501F，Scene不变、Play退出、Console0。下一首差为FrameMotion/RuntimeMaintenance。

> **2026-09-04 B3 C02/C03 production placement已验证：** `NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001 / VERIFIED / REAL-PLAY-PASS / NEXT-FRAME-MOTION-VS-COOLDOWN`。Human poll、按slot交错的non-character hit_Fa/character producer及第二遍proxy/route现均在Cooldown前；后续FrameLogic occurrence移除，direct CharacterInputAll仍character-only。red CS1061→compile0，focused8、input/AI/worker related110、23:24:44 SelfCheck PASS、7 known negative复核、Console0；真实kind0 Play tick3～6及C01 age/RNG/presentation/cleanup均PASS，result SHAC52DE7F5，Scene 0D74E174/time未变并已退出Play。下一首差是Authority FrameMotion / Unity Cooldown；Cooldown writer需拆到C11/C25j。

> **2026-09-04 Bug修复版AUTHORITY晋升与B0-B3影响分类已验证：** `GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002 / VERIFIED / USER_CONFIRMED / AUTHORITY-PROMOTED`。当前唯一正式EXE为`B1E13AE1...9033`，82-file playable closure为`39DDDA15...6109`，75-file capture子闭包为`07CD47A0...778F`；旧身份只保留历史。capture C++ build通过、3场景9条stream数据行除header外逐字节一致、三类validator PASS、tool 49/49、formal human/AI 900-tick报告SHA不变、system.dat 33/3重证。B3已重基为57项并继续到C02/C03 production placement；authority零写入。

> **2026-09-04 B3 producer scan/Cooldown边界审计已恢复并扩展为新版pass rebaseline：** `NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001 / IN_PROGRESS / AUTHORITY-PROMOTED / READ_ONLY-REBASELINE`。新版C00～C17主体仍可辨认，但horizontal impulse与第二次clamp/refill/stage settlement次序已变，display/reaction/armor/rest/opoint/healing进入逐slot嵌套尾部。先修订全pass manifest/contract并重新确定actual首差，再实施Cooldown extraction。

> **2026-09-04 Bug修复版B3 immutable pass contract已重新基线：** `NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001 / FOCUSED_TEST_PASS / 57-CHECKPOINT / PRODUCTION-UNCONNECTED`。旧52项contract已修为57项：physics/dead-resource normalize是2步per-slot transaction；hit为type0→drop→non-type0；第二次clamp/refill/stage settlement后才impulse；slot tail为16步。test-first 18个CS0117后compile0，focused10 job3c2d2532、B3 related38 job5b5f1b65、22:58:20 SelfCheck PASS、7 known negative复核并清Console0。C00～C03未变，下一首差仍为producer/Cooldown。

> **2026-09-04 B3 native spark C01 production integration已验证：** `NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001 / VERIFIED / PRODUCTION-C01-SINGLE-WRITER / PRESENTATION-READ-ONLY / REAL-PLAY-PASS / NEXT-FIRST-DIFF-CORE-PRODUCER-SCAN-VS-COOLDOWN`。C01现位于BattleFlow/input phase后、Cooldown前，resource-independent推进；RenderDispatch/同步Host/worker/stress只冻结、物化或ack，不再改logical age。final focused38、related98、22:14:51 SelfCheck、7 known negative复核后Console0；真实kind0 Play tick3～6的live/frozen ages为`[0]→[1,0]→[2,1,0]→[3,2,1,0]`，前三tick命令1/2/3、Late只读、warm alloc 0/0、cleanup PASS、result SHA397AD7D9、Scene pre/post SHA0D74E174。B9 resource mapping仍另阶段；B3下一首差producer scan/Cooldown。

> **2026-09-04 B3 native spark lifecycle core通过focused：** `NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001 / FOCUSED_TEST_PASS / NATIVE-SPARK-CORE-READY / TERMINAL-TAIL-EXACT / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-C01-INTEGRATION`。red14后，LF2Entity精确实现末位9 terminal、non-tail retain、tail-only pop、0～98 increment、invalid keep且不写旧presentation tick guard。compile0、focused16、related67、满10槽4096次0B、21:35:21 SelfCheck PASS、7 known negative复核后Console0；下一C01 production integration。

> **2026-09-04 B3 spark advance边界审计闭合：** `NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001 / VERIFIED / PRESENTATION-DRIVEN-LIFECYCLE-CONFIRMED / EXISTING-CALL-NOT-MOVABLE / NEXT-NATIVE-SPARK-LIFECYCLE-CORE`。Authority C01使用0～99 native cell、末位9 terminal与tail-only pop，新hit在C14后生所以本tick不进位；Unity current在U25 capture后由presentation/no-publication写回，旧valid age范围不同且资源缺失会冻结逻辑。full snapshot含hit-record而checksum暂缺。下一先建native core，再单独接C01。

> **2026-09-04 B3 Unity实际phase顺序基线通过focused：** `NTSD28-B3-ACTUAL-PHASE-SEQUENCE-BASELINE-001 / FOCUSED_TEST_PASS / ACTUAL-SEQUENCE-READY / FULL-30-PARTIAL-5 / FIRST-DIFF-SPARK-VS-COOLDOWN / ZERO-ALLOC / PRODUCTION-BEHAVIOR-UNCHANGED`。red18个CS1061后，opt-in fixed64 occurrence recorder已落地；真实空world完整tick30项、input-clear partial5项，首差为共同C00后`expected CoreSparkAdvance / actual Cooldown`。compile0、focused6、related90、4096 0B、21:24:11 SelfCheck PASS、7 known negative复核后Console0；下一spark boundary audit。

> **2026-09-04 B3不可变pass order contract通过focused：** `NTSD28-B3-PASS-ORDER-CONTRACT-001 / FOCUSED_TEST_PASS / IMMUTABLE-PASS-CONTRACT-READY / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-ACTUAL-SEQUENCE-BASELINE`。red CS0246后建立52项ID、5 domains、8 traversals、7 flags与private descriptor；C24a～h nested、double refill、dual-action boundary、F-key pre/post、random-drop exception及completed snapshot末位均有断言。compile0、focused10、related42、4096 0B、21:17:21 SelfCheck PASS、7 known negative复核后Console0；production顺序未切。

> **2026-09-04 B3主pass骨架入口审计闭合：** `NTSD28-B3-PASS-SKELETON-ENTRY-AUDIT-001 / VERIFIED / ENTRY-AUDIT-CLOSED / ORDER-AND-BOUNDARY-DIFFERENCES-CONFIRMED / NEXT-PASS-ORDER-CONTRACT`。已冻结Authority G00～G17、C00～C30/C24a～h与Unity UH/U/UF实际顺序；确认global barrier、collision action snapshot、C24 nested tail及completed-tick presentation均有结构差异。S01～16已路由，S11随机掉落保留用户例外；下一不可变pass contract，不改B4～B8行为。

> **2026-09-04 B2阶段退出门通过：** `NTSD28-B2-EXIT-GATE-AUDIT-001 / VERIFIED / B2-EXIT-READY / FUNCTION-KEY-PHYSICAL-PASS / DOWNSTREAM-OWNERS-ROUTED / NEXT-B3-ENTRY-AUDIT`。B2 input/proxy/combo/AI/dual-RNG基础、NONAI 50+2 owner crosswalk与F1～F12 route/carrier/production/real Play均闭合；AI tick3首差归B11，下游RNG/effects归B3+。下一步B3 entry audit。

> **2026-09-04 B2功能键physical Play probe通过：** `NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001 / VERIFIED / REAL-PLAY-PHYSICAL-F3-F12-PASS / SCENE-UNCHANGED / PRODUCTION-UNCHANGED`。真实NTSD_Battle、driver Running、Keyboard device1、tick0→5；F6/F7/F8+F9/F3+F6/plain F10/F11+F12 held+release/F4/Ctrl+F10九组PASS。result SHA881C44CC；Play已退出；Scene SHA20984749前后不变；20:50:48 SelfCheck、Console0、Ledger157/115。

> **2026-09-04 B2功能键production integration通过运行时验收：** `NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION-CONNECTED / SESSION-EXACTLY-ONCE / LEGACY-PHYSICAL-ISOLATED / REAL-PLAY-PHYSICAL-PASS / EFFECTS-DEFERRED-DOWNSTREAM`。red23；compile0、focused11、相关96、worker与zero-alloc通过，后续真实Play九组PASS。F4/F6～F12 effects仍按B3/B5/B8/B10/B11，不误报完成。

> **2026-09-04 B2功能键Session carrier通过focused：** `NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001 / FOCUSED_TEST_PASS / SESSION-CARRIER-READY / SNAPSHOT-CHECKSUM-RESTORE-READY / PRODUCTION-UNCONNECTED / NEXT-PRODUCTION-INTEGRATION`。red50后mask0xF4、fixed dispatch、lock/count/pending/consume、root reset与core5/full7/checksum10/restore闭合；compile0、新9、相关65、4096 zero-alloc、20:19:23 SelfCheck PASS、Console0、Ledger155/110。下一统一production integration。

> **2026-09-04 B2纯功能键route契约通过focused：** `NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001 / FOCUSED_TEST_PASS / PURE-ROUTE-READY / PRODUCTION-UNCONNECTED / NEXT-SESSION-STATE-CARRIER`。test-first fresh123 missing-type后，F1～F12/virtual key/九级priority/result已实现；compile0、router8/8、router+旧FunctionKey+B1 Host19/19、4096 zero-alloc、20:01:21 SelfCheck PASS、预期7日志清空后Console0、Ledger154/106。不接physical/runtime/effects；下一Session carrier。

> **2026-09-04 B2功能键路由交叉闭合：** `NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001 / VERIFIED / FULL-F1-F12-CROSSWALK / IMPLEMENTATION-SPLIT-DEFINED / NEXT-ROUTE-CONTRACT`。F1～F12、九级route priority、Host/Session/continuous/maintenance、mask0xF4、固定F3→F6→F7→F8→F9 dispatch与same-window F9 wins已manifest。Unity旧F7全属性500明确不等于Authority current-MP-only；B1 F1/F2/F5复用，下游归B3/B5/B8/B10/B11。validator153/104；下一pure route contract。

> **2026-09-04 B2非AI RNG call-site交叉闭合：** `NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001 / VERIFIED / FRESH-50-SYNC-PLUS-2-CRT-CLOSED / DOWNSTREAM-OWNERS-ROUTED / NEXT-FUNCTION-KEY-CROSSWALK`。BW32+IN2+TD1+GS15及CRT2已逐项manifest，纠正旧43；Unity fresh87语法命中/86调用候选已分legacy/optimized/exception/domain owner。input与BGM已迁移，其余按B3～B8/B11/B12随single-owner行为迁移；不在B2机械切流。下一function-key crosswalk。

> **2026-09-04 B2退出门更新：** real Play physical门已由`NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001`关闭；B2现为`B2-EXIT-READY`，下一B3 entry audit。

> **2026-09-04 B2正式EXE headless输入观察通过：** `NTSD28-B2-FORMAL-EXE-HEADLESS-INPUT-OBSERVATION-001 / VERIFIED / FORMAL-EXE-HUMAN-AND-AI-HEADLESS-PASS / INPUT-RESET-OBSERVED / PER-CALL-RNG-NOT-EXPOSED`。SHA锁定根formal EXE的human/AI两次均exit0、900tick PASS；human P1/P2七键mask127、动作/位置/无输入对照与双方reset clean通过，AI正式配置与reset通过。报告SHA 65D4FF97/2F845CBD；authority清单71275342、2809 files、110386049 bytes前后不变。exact/per-call RNG不在smoke schema，继续保持边界。

> **2026-09-04 B2 AI exact input store roundtrip通过focused：** `NTSD28-B2-AI-EXACT-INPUT-STORE-ROUNDTRIP-001 / FOCUSED_TEST_PASS / AI-TICKS1-2-EXACT-EQUAL / NEXT-FIRST-DIFFERENCE-B11-CONTENT-DOWNSTREAM-TICK3 / FORMAL_EXE_PENDING`。redc83e0f44后post-route AI runtime已handle-safe回写canonical store；compile0、exporter11、broad200，common/standing equal，AI ticks1-2 exact+RNG equal；tick3 current首差由tick2 action650/9内容分叉传导，归B11。19:21:55 SelfCheck、Console0；B2继续formal证据。

> **2026-09-04 B2 AI host pending投影通过focused：** `NTSD28-B2-AI-HOST-PENDING-PROJECTION-001 / FOCUSED_TEST_PASS / AI_HOST_PREVIOUS_READY / RUN_TRIGGER_READY / NEXT_FIRST_DIFFERENCE_AI_HISTORY_ROUNDTRIP / ACTION_CONTENT_B11 / FORMAL_EXE_PENDING`。red0b7d3ad5后compile0；exporter11、broad169，common/standing均3tick/6pairs equal，AI previous/run闭合且首差下移tick2 history[3] 4/-1；19:09:33 SelfCheck PASS、Console0。action9/650归B11；B2继续。

> **2026-09-04 B2 AI native history绑定顺序通过focused：** `NTSD28-B2-AI-NATIVE-HISTORY-BIND-ORDER-001 / FOCUSED_TEST_PASS / NATIVE_HISTORY_PRE_BIND_READY / AI_TICK1_EXACT_EQUAL / NEXT_FIRST_DIFFERENCE_TICK2_PREVIOUS_MASK / FORMAL_EXE_PENDING`。redc9956771后成功registration改为init后Bind；compile0、exporter11、broad183，18:50:38 SelfCheck与Console0。common/standing equal；AI tick1 exact+RNG全equal，新首差tick2 previousMask authority0/Unity2，另包继续；B2未退出。

> **2026-09-04 B2 首有效 tick input/AI readiness通过focused：** `NTSD28-B2-FIRST-TICK-CHARACTER-INPUT-AI-READINESS-001 / FOCUSED_TEST_PASS / FIRST_TICK_READY / AI_NATIVE_RNG_JOINT_EQUAL / FIRST-DIFFERENCE-SUPERSEDED-BY-AI-NATIVE-HISTORY-BIND-ORDER / FORMAL_EXE_PENDING`。red5153cb5d后guard仅拒绝非正tick；compile0、exporter11、broad149，18:38:00 SelfCheck与Console0。AI RNG逐次equal；该包当时捕获的keyHistory首差已由后续Bind-order包关闭。

> **2026-09-04 B2 accepted AI RNG per-call joint trace历史首差：** `NTSD28-B2-AI-ACCEPTED-RNG-PER-CALL-JOINT-TRACE-001 / FOCUSED_TEST_PASS / ACCEPTED-AI-TRACE-READY / FIRST-DIFFERENCE-SUPERSEDED-BY-NTSD28-B2-FIRST-TICK-CHARACTER-INPUT-AI-READINESS-001 / FORMAL-EXE-PENDING`。该包当时准确捕获首次eligible晚1tick；后续first-tick包已关闭并证明AI RNG逐次equal。observer/v3证据保留，当前首差已下移至AI keyHistory。

> **2026-09-04 B2 direct RNG per-call joint trace通过focused：** `NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001 / FOCUSED_TEST_PASS / DIRECT-PER-CALL-V2-READY / INPUT-COMMON-STANDING-EQUAL / AI-FORMAL-PENDING`。red Unity5/.NET3后，authority linker wrap、Unity null observer及v2 validator完成；C++/.NET/Unity compile0，selftests5+5/21/12/6、Unity31+25、AI/lockstep86全PASS，null observer4096零allocation。input-common及standing-attack均valid 3tick/6pairs equal，后者tick2为site0x82/bound2/result1。18:08:40 SelfCheck、Console0；AI cursor/formal EXE另待。

> **2026-09-04 B2 defend re-entry exact refresh通过focused：** `NTSD28-B2-DEFEND-REENTRY-EXACT-FRAME-REFRESH-001 / FOCUSED_TEST_PASS / EXACT-REFRESH-READY / INPUT-COMMON-JOINT-EQUAL / FORMAL-PER-CALL-PENDING`。red3后compile0、focused75/75；writer同步legacy+exact，frame gate/pass顺序/input decrement不动。input-common B2 exact input/native RNG为3tick/6pairs全equal、firstDifference null；17:47:38 SelfCheck、Console0。formal/per-call仍待，B2不退出。

> **2026-09-04 B2 direct-battle synchronized pre-draw通过focused：** `NTSD28-B2-NATIVE-RNG-DIRECT-BATTLE-BOOTSTRAP-001 / FOCUSED_TEST_PASS / DIRECT-BATTLE-RNG-CURSOR-READY / JOINT-FIRST-DIFFERENCE-CLOSED / AUDIO-SELECTION-B10`。red4后compile0；共享seed/table→0x004021E0消费已由local、lockstep、diagnostic共用。focused28/28+17/17；B2 joint initial双RNG全equal，首差下移completed tick2 slot1 defend cooldown authority3/Unity0。17:38:34 SelfCheck PASS、Console0。generic reset/restore、selection flow和BGM音频未改。

> **2026-09-04 B2 exact input/native dual RNG观测包通过focused：** `NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001 / FOCUSED_TEST_PASS / INPUT-RNG-OBSERVABILITY-READY / REAL-FIRST-DIFFERENCE-CAPTURED / DIAGNOSTIC_ONLY`。red.NET12/Unity2/C++1后build0；selftest4+旧5/12/6、Unity9/9+相关40/40。双端3tick/6entity valid，首差initial synchronized counter authority1/Unity0（lastSite0x004021E0 Random BGM）；次级exact audit仅余defend cooldown tick2/3 3/0。17:23:28 SelfCheck、Console0；production/authority/Config/DAT/Scene未改，per-call/formal EXE仍待。

> **2026-09-04 B2 Unity trace物理按键翻译通过focused与joint rerun：** `NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001 / FOCUSED_TEST_PASS / TRACE_TRANSLATION_CORRECTED / PRODUCTION_UNCHANGED / JOINT_TRACE_RERUN_PASS`。red3 CS0117后，Editor-only parser完成J/K/L→FrameInput Jump/Defend/Attack与domain canonical反投影；compile0、final8/8、相关输入20/20。joint action/latch/previous/snapshot/state/counter全equal，37 equal/10 diff，首差转B11 baseMaxMp500/200，其余9 missing；16:56:26 SelfCheck PASS、Console0、Ledger140/94。production input、Config/DAT/Scene与authority未改。

> **2026-09-04 B2 INPUT JOINT TRACE关闭：** `NTSD28-B2-INPUT-JOINT-TRACE-001 / VERIFIED / B2-SCOPE-JOINT-CLOSED / HUMAN-SCENARIOS-EQUAL / AI-TICKS1-2-EXACT-AND-RNG-EQUAL / FORMAL-BEHAVIOR-PASS / DOWNSTREAM-FIRST-DIFFERENCE-B11`。common/standing全等；AI ticks1-2 exact+RNG闭合，tick3首差归B11。formal human/AI 900tick behavior与F-key real Play补齐；不冒充formal per-call certificate。

> **2026-09-04 B2 ground+air生产接线完成到运行时边界：** `NTSD28-B2-NATIVE-TYPE0-BUILTINS-PRODUCTION-INTEGRATION-001 / FOCUSED_TEST_PASS / PRODUCTION_CONNECTED / RUNTIME_PENDING / JOINT_TRACE_PENDING`。native顺序已为combo→three→direction→ground/air→projection；native LF2Character与共享Character-DAT壳旧动作解析已抑制，Legacy profile保留；dead type0在清空/投影后早退。compile0；integration8/8、NTSD28 group167/167、CharacterInput37/37、worker/AI101/101、snapshot/checksum/ring31/31、SelfCheck PASS；Console0；Ledger138/94。真实Play/joint trace待验；environment producer/raw、Config/DAT/Scene不动。

> **2026-09-04 B2 native ground+air core就绪：** `NTSD28-B2-NATIVE-TYPE0-AIR-DASH-REDIRECT-BUILTINS-001 / FOCUSED_TEST_PASS / GROUND_AIR_CORE_READY / PRODUCTION_UNCONNECTED`。action215、state4/5、state85/86与action182/188 rowing direct-test core已完成。compile0；air10/10、ground+air22/22、NTSD28 group159/159、CharacterInput37/37、snapshot/checksum/ring31/31、SelfCheck PASS；Console0；Ledger137/93。统一production integration另包；Config/DAT/Scene与environment producer/raw不动。

> **2026-09-04 B2 environment_state_320 carrier完成：** `NTSD28-B2-ENVIRONMENT-STATE-CARRIER-001 / FOCUSED_TEST_PASS / CARRIER_READY / PRODUCERS_UNCONNECTED / AIR_CONSUMER_PENDING`。独立signed scalar已覆盖default/input-reset/full-reset/copy、entity4/aggregate6/checksum9。compile0；新5/5、carrier10/10、snapshot/checksum/ring31/31、NTSD28 group149/149、SelfCheck均PASS；Console0；Ledger136/92通过。raw projection仍missing，B4/B5/B6 producer与air consumer不在本包。

> **2026-09-04 B2 native type-0 ground built-ins core通过focused：** `NTSD28-B2-NATIVE-TYPE0-GROUND-BUILTINS-001 / FOCUSED_TEST_PASS / GROUND_CORE_READY / HIGH_FRAME_READY / PRODUCTION_UNCONNECTED / AIR_PENDING`。red37后首轮10/11定位cache600 cap；formal decoded max856，cache改857 exclusive后12/12、B2 broad259/259。SelfCheck依次修正DATA-01B/C、FT-02、LC-02三处同源旧600夹具，最终15:46:50 PASS；预期7日志清除后Console0、Ledger135/91。two-pass/resolver仍未改，ground core不可达；air/integration/joint trace另包。

> **2026-09-04 B2 native type-0 built-in数据seam通过focused：** `NTSD28-B2-NATIVE-TYPE0-BUILTIN-DATA-SEAMS-001 / FOCUSED_TEST_PASS / DATA_SEAMS_READY / CONSUMERS_UNCONNECTED`。red37后，4组有序movement sequence与9个absent→0 linked stats的model/parser/carrier/converter已闭合，重复convert clear/replace。compile0；new6/6、parser/carrier20/20、B2 broad247/247、15:18:00 SelfCheck PASS、预期7日志清除后Console0、Ledger134/89。Config/DAT与runtime consumer未改；ground、air/dash/redirect及joint trace仍待。

> **2026-09-04 当前B2 native direct/hold/direction已通过focused：** `NTSD28-B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING-001 / FOCUSED_TEST_PASS / DIRECT_HOLD_DIRECTION_READY / PRODUCTION_CONNECTED / BUILTINS_JOINT_TRACE_PENDING`。red31后，production按combo→three-button→direction→projection接线；每次field重读当前frame，严格保留缓存比较、失败仍消费、同tick多field、朝向重判、counter不清及四条depth不对称cleanup，DataOriented legacy owner为release-only。Unity compile0；new12/12、input-owner37/37、B2-related241/241、snapshot/checksum32/32、worker/AI-shadow101/101；14:55:49 SelfCheck PASS，预期7条负向日志清除后Console0；Ledger133/86与diff check通过。真实Play、type0 built-ins、sync82/83/84与joint trace仍待，B2及全项目未完成。

> **2026-09-04 B2 native combo action transaction通过focused：** `NTSD28-B2-NATIVE-COMBO-ACTION-TRANSACTION-001 / FOCUSED_TEST_PASS / COMBO_ACTION_TRANSACTION_READY / PRODUCTION_CONNECTED / JOINT_TRACE_PENDING`。red65；current remap/bound、exact combo production caller、generic lock/redirect/source-cost/resource/fallback/facing与`hit_ja`事务已闭合，DataOriented跳过legacy combo owner但保留direct/release。authority 46/46 main closure与fresh binary PASS；gameplay Unity PID51752 compile0，focused18/18、B2 broad261/261、含B0 raw-capture超集267/267、snapshot/checksum31/31、14:31:34 SelfCheck PASS、7条预期负向日志清除后Console0、Ledger132/84。direct/hold/direction、type0 built-ins、sync82/83/84、跨阶段producer与joint trace仍为后续包，B2及全项目未完成。

> **2026-09-03 B2 native action data carriers通过focused：** `NTSD28-B2-NATIVE-ACTION-DATA-CARRIERS-001 / FOCUSED_TEST_PASS / DATA_CARRIERS_READY / PRODUCTION_UNCONNECTED`。red128→compile0；runtime/frame/definition carrier、reset/deep-copy/fail-closed、schema3/5/8与逐字段checksum闭合。final10/10、B2 255/255、snapshot31/31、0B；13:55:53 SelfCheck PASS，Console0，Ledger131/80。Config/DAT/Scene未由本包改；action/timer/B3/B5/B7/B8 producer未接。

> **2026-09-03 B2 native action routing交叉审计闭合：** `NTSD28-B2-NATIVE-ACTION-ROUTING-CROSSWALK-001 / VERIFIED / SOURCE_CHAIN_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。authority正式build与46个定义/46个`main`调用tests、combo→three-button→direction→built-ins顺序、generic/direct resource事务已冻结；初始44计数经直接重计纠正。carrier addendum已冻结20项type/default/producer owner及snapshot `2→3`、aggregate `4→5`、checksum `7→8`路径，B2不得提前实现B3/B5/B7/B8 producer。Unity selector无production caller，hold/direction/action carrier、remap/jump-suppress及sync `82/83/84`缺失；按5包实施，无source改动。

> **2026-09-03 B2 native input producer生产与真实Play验收闭合：** `NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001 / FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_DDJ_DRA_PASS / JOINT_TRACE_PENDING`。DataOriented producer raw-only→proxy→native edge/history/combo once→projection，含dead type0/history-1；final focused `e0c1...` 8/8、broad239/239、AI367/367、zero-alloc。最终DDJ exact1→2→3/Frame271/ticks2-4-15，DRA exact1→2→4/Frame263/ticks40-42-53，均1/1/1 attempts；batch自动退Play。13:19:28 SelfCheck PASS，Console0，Ledger130/77。exact action fields/joint trace仍待，未扩大为B2整体完成。

> **2026-09-03 B2 native input producer迁移已立项：** `NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。范围为DataOriented human/AI producer raw-only、proxy后统一native edge/history/combo once、exact→legacy兼容投影与初始history；Legacy保持。exact action fields与joint trace另包。

> **2026-09-03 B2 AI production synchronized commit通过focused：** `NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001 / FOCUSED_TEST_PASS / PRODUCTION_SYNC_COMMIT_READY / LEGACY_RNG_ISOLATED / JOINT_TRACE_PENDING`。每AI capture、accepted-only commit、same-generation stale reject、oracle/shadow/fallback discard与call-site compare已接production；red5/81，final81/81、AI362/362、broad234/234、补强shadow69/69+native12/12、11:36:54 SelfCheck、Console0、Ledger128/76。下一包native input producer迁移；B2 joint trace未完成。

> **2026-09-03 B2 AI RNG owner旧SelfCheck seam已修复：** `NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001 / VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`。11:29:43 target均7而旧通用RNG equality失败；fixture改为双seed，legacy推进旧RNG、indexed只推进NativeRandom；11:36:54 PASS，production零改。

> **2026-09-03 B2验证发现AI RNG owner旧SelfCheck seam：** `NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001 / IN_PROGRESS / RED_SELF_CHECK_CAPTURED / TEST_ONLY`。11:29:43 full SelfCheck中target两边均7，仅旧断言仍比较legacy/indexed的通用RNG state/calls；IndexedCanonical现在依法只提交NativeRandom。将只修测试fixture与owner断言，production不改。

> **2026-09-03 B2 AI production synchronized commit已立项：** `NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。范围为IndexedCanonical每AI cursor capture、accepted-only唯一commit、oracle/shadow/fallback零提交、legacy RNG隔离、same-generation stale reject与call-site oracle compare；input producer/joint trace不在本包。

> **2026-09-03 B2 AI held RNG通过focused：** `NTSD28-B2-AI-HELD-RNG-001 / FOCUSED_TEST_PASS / HELD_SITES_28_35_READY / ALL_40_LIVE_IDS_READY / PRODUCTION_UNCONNECTED`。valid link-before-28、subject-group blocker、115/300阈值、weapon-run早停、state17 Y与combo index4/5闭合；red1、19/19、AI361/361、full `14→3C→1C→28` HeldDecision、4096 zero-alloc、10:56:19 SelfCheck、Console0、Ledger126/75。下一包production authoritative-only cursor commit。

> **2026-09-03 B2 AI held RNG已立项：** `NTSD28-B2-AI-HELD-RNG-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。范围为0x28..0x35、native返回/早停、subject-group line blocker、115/300阈值、state17 Y门与combo index4/5；legacy与production cursor不动。

> **2026-09-03 B2 AI ordinary non-held RNG通过focused：** `NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001 / FOCUSED_TEST_PASS / NONHELD_SITES_READY / SURPLUS_69_ISOLATED / PRODUCTION_UNCONNECTED`。静态BattleMode与cadence已分离；动态1A/1C、1B/1D及1E..21、26/27、37..3B闭合，剩余4个prewrite surplus连同既有65个profile surplus均在sync模式隔离。red11、15/15、AI342/342、full Complete order、4096 zero-alloc、10:34:40 SelfCheck、Console0、Ledger125/74。下一包held0x28..35。

> **2026-09-03 B2 AI ordinary non-held RNG已立项：** `NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。authority已确认静态`battle_mode`不能复用cadence `InputPhase`，正式playable的`global_direction_lock_0049f608`保持默认0；范围为动态1A/1C、1B/1D及1E..21、26/27、37..3B，剩余4个prewrite surplus隔离。held与production cursor另包。

> **2026-09-03 B2 AI special-profile RNG通过focused：** `NTSD28-B2-AI-SPECIAL-PROFILE-RNG-001 / FOCUSED_TEST_PASS / SITES_3C_6C_READY / SURPLUS_65_ISOLATED / PRODUCTION_UNCONNECTED`。candidate严格`0x3C→optional0x6C`，oid33命中置combo index2=hit_Ua；旧profile树65个无ID表达式已隔离，legacy保持。red5、6/6、AI303/303、4096 zero-alloc、10:01:37 SelfCheck、Console0、Ledger124/72。

> **2026-09-03 B2 two-pass routing visibility旧测试已更正：** `NTSD28-B2-TWO-PASS-ROUTING-VISIBILITY-TEST-001 / VERIFIED / TWO_PASS_EXPECTATION_CORRECTED / TEST_ONLY`。旧测试错误要求slot0 routing state14对slot1同tick producer可见；已改为route不回流并保留cached slot0。red296/297+isolated，最终297/297、SelfCheck、Console0；production零改。

> **2026-09-03 B2 AI pickup RNG通过focused：** `NTSD28-B2-AI-PICKUP-RNG-001 / FOCUSED_TEST_PASS / PICKUP_SITES_16_17_READY / PRODUCTION_UNCONNECTED`。native candidate state1000/2004、legacy1004/2004与`0x16/0x17`闭合；red1、6/6、相关297/297、4096 zero-alloc、09:50:31 SelfCheck、Console0、Ledger123/71。完整selection和production另包。

> **2026-09-03 B2 AI target-prefix RNG通过focused：** `NTSD28-B2-AI-TARGET-PREFIX-RNG-001 / FOCUSED_TEST_PASS / SITES_13_14_15_18_19_READY / PRODUCTION_UNCONNECTED`。`0x13/14/15/18/19`及cached-active→RNG→type、state7不消费15、boundary不冒充force字段已闭合；red7、final7/7、AI185/185、4096 zero-alloc、09:31:53 SelfCheck、Console0、Ledger121/68。production/pickup未接。

> **2026-09-03 B2 scripted AI RNG transaction通过focused：** `NTSD28-B2-AI-SCRIPTED-RNG-TRANSACTION-001 / FOCUSED_TEST_PASS / SCRIPTED_TRANSACTION_READY / PRODUCTION_UNCONNECTED`。snapshot/call-site trace→kernel candidate→witness→explicit commit闭合，scripted左右只消费`0x11/0x12`；red19、6/6、AI178/178、4096 zero-alloc、09:18:37 SelfCheck、Console0、Ledger120/67。production capture/commit未接。

> **2026-09-03 B2 AI synchronized RNG stream seam通过focused：** `NTSD28-B2-AI-SYNC-RNG-STREAM-SEAM-001 / FOCUSED_TEST_PASS / STREAM_SEAM_READY / PRODUCTION_UNCONNECTED`。显式site synchronized cursor、site/bound/raw/value trace、mode fail-closed与显式commit已实现；red34、7/7、native18/18、world+kernel18/18、AI172/172、5000 bit-exact、4096 zero-alloc、09:03:38 SelfCheck、Console0、Ledger119/64。旧CRT与production consumer未改。

> **2026-09-03 B2 AI synchronized RNG live-call crosswalk闭合：** `NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001 / VERIFIED / LIVE_CLOSURE_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。authority `native_ai.cpp`42文本表达式排除4个nonlive后为38 live expressions/40 possible IDs；Unity canonical为107 expressions，69个无live ID。legacy按钮字段按`KeyJump=attack/KeyDefend=jump/KeyAttack=defend`交叉映射；真实差异为消费时点、阈值、combo index、force-attack/use_ai缺字段及`0x26/27`重复，完整顺序已冻结到manifest；production未改。

> **2026-09-03 B2 AI synchronized RNG cursor通过focused：** `NTSD28-B2-AI-SYNC-RNG-CURSOR-001 / FOCUSED_TEST_PASS / SYNCHRONIZED_CURSOR_READY / CONSUMERS_UNMIGRATED`。共享table、独立scalar、generation-gated commit已实现；test-first20、final11/11、相关16/16、5000步bit-exact、4096 zero-alloc、08:35:12 SelfCheck、Console0。generation不进snapshot/checksum；AI call-sites尚未迁移。

> **2026-09-03 B2 native combo router字段包通过focused：** `NTSD28-B2-NATIVE-COMBO-ROUTER-FIELDS-001 / FOCUSED_TEST_PASS / FRAME_FIELDS_AND_SELECTOR_READY / PRODUCTION_UNCONNECTED`。已补`hit_aj/ad/jd` frame/parser、pure combo10 priority selector与explicit attempt consume；test-first13、final17/17、related42+1 explicit skip、08:21:06 SelfCheck、Console0、4096 zero-alloc。Unity Config未改；`hit_ja`特殊副作用、action执行与human/AI迁移另包。

> **2026-09-03 B2 native combo production crosswalk闭合：** `NTSD28-B2-NATIVE-COMBO-PRODUCTION-CROSSWALK-001 / VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。2.8第一遍只producer/sample/record，第二遍先proxy再edge/history/combo10/action；Unity human/AI当前都提前推进旧edge/combo。Unity frame/parser缺`hit_aj/ad/jd`，权威为1110/1089/99处而Unity Config均0。后续拆router-fields、producer migration、RNG call-sites、joint trace四包；本包只读、权威未写。

> **2026-09-03 B2 production input two-pass通过focused与真实输入路由Play：** `NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001 / FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_INPUT_ROUTE_PASS / JOINT_TRACE_PENDING`。`CharacterInputAll`已拆为all producer/sample freeze→ascending exact proxy copy+route；后续AI只看producer发布，route结果仅进入保留态。focused7/7、AI80/80、B2联合198/198、07:51:26 SelfCheck、DDJ tick1588→1590→1601与DRA tick2614→2616→2627真实Play、Console0。legacy AI→exact仍为迁移桥；native combo producer/action readers和joint trace未关闭。

> **2026-09-03 B2 proxy control lifecycle通过focused：** `NTSD28-B2-PROXY-CONTROL-LIFECYCLE-001 / FOCUSED_TEST_PASS / CONTROL_CORE_READY / PRODUCTION_UNCONNECTED`。counter status、type0 confirmed-hit activation、positive-HP/body-gated decrement与unconditional expiry core已实现；13/13+组合44/44、4096 zero-alloc、full SelfCheck、Console0。hit writer归B5，global tail placement归B3，当前无production caller。

> **2026-09-03 B2 native combo bridge通过focused：** `NTSD28-B2-NATIVE-COMBO-BRIDGE-001 / FOCUSED_TEST_PASS / COMBO10_CORE_READY / PRODUCTION_UNCONNECTED`。exact combo10、native edge/history顺序、same-sample/early-terminal、history priority、tail/clear边界及严格受限legacy projection已实现；19/19→最终combo/proxy/carrier 31/31、full SelfCheck、Console0。`hit_aj/ad/jd`不做错误别名；production调用仍未接。

> **2026-09-03 B2 native input carrier通过focused：** `NTSD28-B2-NATIVE-INPUT-STATE-CARRIER-001 / FOCUSED_TEST_PASS / CARRIER_READY / WRITERS_UNCONNECTED`。每runtime exact33-byte block+counter/source/enabled已覆盖reset、canonical deep-copy、entity/aggregate snapshot与checksum；5/5、相关38/38、pooled23/23、full SelfCheck及Console0。旧Key/Combo与production writer/two-pass仍未改。

> **2026-09-03 B2 proxy production crosswalk闭合：** `NTSD28-B2-PROXY-INTEGRATION-AUDIT-001 / VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。current/previous与7 edge物理映射、CdDefendLock同源已确认；native combo10与Unity旧ComboD*9非1:1，proxy control3+tail缺失。authority为AI/sample全slot第一遍→proxy/routing第二遍，Unity当前AI+combo逐实体交错。后续按carrier→combo→control lifecycle→two-pass→joint trace五包实施。

> **2026-09-03 B2 exact proxy block focused通过：** `NTSD28-B2-INPUT-PROXY-BLOCK-001 / FOCUSED_TEST_PASS / PROXY_BLOCK_READY / PRODUCTION_UNCONNECTED`。test-first CS0246；job `9bda...d161` 5/5，33-byte offset、deep copy、excluded surface、invalid targets、4096次zero-allocation通过；compile/Console0。entity/AI/two-pass待。

> **2026-09-03 B2 input phase cadence focused通过：** `NTSD28-B2-INPUT-PHASE-CADENCE-001 / FOCUSED_TEST_PASS / PHASE_CADENCE_READY / AI_PROXY_PENDING`。test-first14；pending七键、default2tu 1/0与oneTu恒0已接snapshot/checksum。final related `73c9...737f` 86/86、full SelfCheck PASS、compile/Console0；2tu tick2 edge5/tick3=4/tick6=1且单一history已证。AI/proxy/RNG consumer待。

> **2026-09-03 B2 dual RNG world-state focused通过：** `NTSD28-B2-RNG-WORLD-STATE-001 / FOCUSED_TEST_PASS / WORLD_STATE_READY / CONSUMERS_UNMIGRATED`。新5/5、final related `9c40...1509` 50/50；初次7个restore failure暴露并修正tableSeed provenance。core1024/restore128/checksum256均零分配，fresh transfer通过，full SelfCheck PASS，compile/Console0。consumer与B0 exporter/JSON trace未迁移。

> **2026-09-03 B2三项旧SelfCheck seam已修复：** `NTSD28-B2-SELFCHECK-PRIVATE-MEMBER-SEAM-001`、`NTSD28-B2-SELFCHECK-REST-OWNER-PATH-001`、`NTSD28-B2-SELFCHECK-RUNTIME-SLOT-SEAM-001` 均为 `VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`。依次修复camera private property注入、ItrRest重组后owner paths、runtime-slot internal owner accessor；production零改动。

> **2026-09-03 B2验证暴露旧ItrRest owner路径：** `NTSD28-B2-SELFCHECK-REST-OWNER-PATH-001 / IN_PROGRESS / RED_SELF_CHECK_CAPTURED / TEST_ONLY`。相机helper修复后full SelfCheck继续到ItrRest路径guard；当前4个production调用均在已治理重组后的Runtime/Passes/Lockstep-Snapshot三个owner，旧测试仍匹配重组前路径。仅更新精确path/store断言。

> **2026-09-03 B2验证暴露旧SelfCheck反射seam：** `NTSD28-B2-SELFCHECK-PRIVATE-MEMBER-SEAM-001 / IN_PROGRESS / RED_SELF_CHECK_CAPTURED / TEST_ONLY`。full SelfCheck在固定世界相机stale-state注入前置断言失败；源码确认 `_cameraX/_cameraVel` 已是private property而helper仅查field并静默no-op。仅修test helper支持field/property+missing fail-closed，production不改。

> **2026-09-03 B2 dual RNG world-state已立项：** `NTSD28-B2-RNG-WORLD-STATE-001 / IN_PROGRESS / TEST_FIRST / PRE_CODE`。范围为world owner、default/match/bootstrap seed、reset、allocation-free scalar snapshot/fresh restore/runtime checksum；legacy RNG和consumer保持不动，B0 exporter/JSON trace另包。先取缺失seam red。

> **2026-09-03 B2 standalone dual RNG 基元 focused通过：** `NTSD28-B2-NATIVE-DUAL-RNG-PRIMITIVE-001 / FOCUSED_TEST_PASS / PRIMITIVE_READY / WORLD_UNCONNECTED`。test-first 12个CS0246；final job `736e...c58f` 为本包6/6、shared RNG1/1、boundary RNG3/3。B0 authority seed精确得到CRT state1758127634/table hash `A1BA1B90EA55796D`；compile/Console0。未接world、AI或现有consumer。

> **2026-09-03 B2 input/RNG 源链审计通过：** `NTSD28-B2-INPUT-RNG-SOURCE-AUDIT-001 / VERIFIED / SOURCE_INVENTORY_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。权威 input phase 为 1tu=0、2tu=1/0，AI 每 tick sampling、human 仅 phase0 更新 current，recording 在 proxy 前冻结，proxy 精确 0x21 bytes。RNG 为 CRT+3001-byte synchronized 双流；生产 83 expressions/80 IDs、direct CRT 2。Unity 当前通用 RNG 37、AI `.Rand` 文本 168 且无 synchronized state。下一包只做未接 world 的双流基元和 authority vectors。

> **2026-09-03 B1退出审计通过、进入B2：** `NTSD28-B1-TIME-HOST-EXIT-AUDIT-001 / VERIFIED / B1_CURRENT_PRODUCTION_READY / B2_READY / GOVERNANCE_ONLY`。当前production Host精确33/3ms、two-interval drain、F1/F2/F5/pause合同已闭合；focused7/7、related31/31、Play32.71883/3.88752ms、ratio0.118816、report `DAC9D1...C0ED`。worker因Unity bindings当前ineligible，B9解除前强制回B1复验；OS实体键留B12。下一阶段B2输入与双RNG。

> **2026-09-03 B1 worker pacing边界闭合：** `NTSD28-B1-WORKER-PACING-AUDIT-001 / FOCUSED_TEST_PASS / REAL_REASON_CAPTURED / CURRENT_PATH_INACTIVE / B9_REVALIDATION_TRIGGER`。Scene设useWorker=true，但真实reason为`unity-presentation-bindings-are-still-attached`，failure/submission为空；当前正式inline路径已通过。source确认single-in-flight不能同Update第二tick，因此B9若解除binding，必须先补B1 worker cadence实现/trace，不得直接启用。report `DAC9D1...C0ED`。

> **2026-09-03 B1 Unity Host-loop/Present适配 focused+Play通过：** `NTSD28-B1-UNITY-HOST-LOOP-BRIDGE-001 / FOCUSED_TEST_PASS / REAL_PLAY_PASS / WORKER_PATH_PENDING`。red `7ac8...c0e3` 6/7捕获旧second interval拒绝，final `f5c9...dbd7` 7/7、related `bb77...8021` 31/31。真实Play Normal32.76602ms、Fast4.4956583ms、ratio0.1372049、max jump2，report SHA `07364B...B95F`。当前场景worker inactive，worker同Update第二submit仍未证。

> **2026-09-03 旧R8 Play poller无request隔离通过：** `NTSD28-B1-EDITOR-PROBE-REQUEST-ISOLATION-001 / FOCUSED_TEST_PASS / REAL_PLAY_NO_REQUEST_PASS / REQUEST_SCENARIO_NOT_RERUN`。OID5152真实栈与CentralLiveness同构已修为request存在性先于一切unpause；final F1 pause跨150ms稳定，`SetPaused`只保留bootstrap一次。旧R8有request完整场景未重跑。

> **2026-09-03 B1 Host edge latch focused+设备路径通过：** `NTSD28-B1-HOST-PHYSICAL-EDGE-LATCH-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / EDGE_LATCH_7_7 / REAL_PLAY_DEVICE_PATH_PASS`。test-first CS0246→job `3be4...c286` 7/7；false→true、held no-repeat、release/clear rearm已实现，related31/31。真实Play临时Input System Keyboard走production入口通过；OS实体键未自动化。

> **2026-09-03 B1真实Play Host trace通过（带边界）：** `NTSD28-B1-HOST-RUNTIME-TRACE-001 / FOCUSED_TEST_PASS / REAL_PLAY_SYNTHETIC_DEVICE_PASS / OS_PHYSICAL_PENDING / WORKER_INACTIVE`。report SHA `07364B...B95F`：Normal32.76602ms、Fast4.4956583ms、ratio0.1372049；pause稳定、F2 +1、running F2 latent=false、max jump2。自动化用临时虚拟Keyboard但走production input/Driver；OS实体键和worker runtime未覆盖。

> **2026-09-03 B1 F1/F2/F5 Host control focused 通过：** `NTSD28-B1-HOST-CONTROL-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / HOST_CONTROL_6_6 / RUNTIME_PENDING`。LocalFreeRun生产入口已接F1 pause、paused-only F2 one-step、F5 33/3ms与debt reset；single-step复用production `StepOneTickInternal`。final job `3625...b6a0` 6/6、related `a92c...ee4` 33/33，Console 0 error；tool 6/12/21/5与Ledger95/22通过。物理按键、真实Play cadence/pause debt/worker runtime仍留下一B1包。

> **2026-09-03 B1 cadence合同 focused 通过：** `NTSD28-B1-CADENCE-CONTRACT-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / HOSTPOLICY_4_4 / RUNTIME_PENDING`。red job `bbd2...a50b`实测旧33.333ms/8tick debt并2项预期失败；最终精确33/3ms、cap2、one/update与cadence change clear通过job `d112...b3a6` 4/4，related11/11、stress3/3、B0 joint9/9。Manual domain SHA不变，物理换算30和Manual/Lockstep未改；F1/F2/F5与真实Play留后续包。Ledger94/22。

> **2026-09-03 B1时间与Host源链审计通过：** `NTSD28-B1-TIME-HOST-SOURCE-AUDIT-001 / VERIFIED / SOURCE_CHAIN_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。权威normal/fast精确33/3ms，每Host loop最多1tick、debt cap2 intervals；F5切速并清logic/render debt，pause每轮清logic debt，F2仅paused立即单步且running不排队。Unity目前1/30、默认8tick cap、pause保留accumulator且无F1/F2/F5 Host合同。下一包先做纯cadence/HostPolicy合同与测试，再接Host control。

> **2026-09-03 B0基线退出审计通过、进入B1：** `NTSD28-B0-BASELINE-EXIT-AUDIT-001 / VERIFIED / B0_BASELINE_READY / B1_READY / GOVERNANCE_ONLY`。B0已具备双端schema、真实非空input、source-native RNG topology、slot/epoch、47实体字段、validator与first-difference comparator；这只表示基线可用，不表示行为已对齐。RNG首差路由B2，9 missing字段路由B4/B5/B7，baseMaxMp路由B11，正式EXE certificate/全场景路由B12。下一活动阶段B1时间与Host。

> **2026-09-03 B0 domain first-difference focused 通过：** `NTSD28-B0-DOMAIN-FIRST-DIFFERENCE-001 / FOCUSED_TEST_PASS / BUILD_0_0 / COMPARATOR_6_6 / REAL_FIRST_DIFFERENCE / PRODUCTION_UNCHANGED`。真实3tick的input、slot occupant/epoch、lifecycle均equal；capacity1000/400仅应用用户例外。首差为`rng.streamAvailability / STREAM_TOPOLOGY_DIFFERENCE`：authority CRT+sync各0/0/0，Unity deterministic1/2/1；报告`B0AED0...CF185`。既有12/12+21/21+5/5，Ledger91/18；RNG修复留B2。

> **2026-09-03 B0 Unity domain raw exporter focused 通过：** `NTSD28-B0-UNITY-DOMAIN-RAW-EXPORTER-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / JOINT_9_9 / REAL_DOMAIN_VALID / PRODUCTION_UNCHANGED`。job `1f2d...e9dc` 9/9；entity/domain raw `ADD7AE...ED97D`/`D88820...BBFC8`两次确定性且domain validator通过。mask与authority一致为`(17,2)/(1,96)/(0,12)`；Unity RNG initial0/delta1,2,1，对照authority0,0,0形成B2差异；capacity400为用户例外，slot0/1 epoch1稳定。跨端entity raw20/27/75首差tick3 slot0 controlSlot2/1；Ledger90/16。

> **2026-09-03 B0 authority domain raw exporter focused 通过：** `NTSD28-B0-AUTHORITY-DOMAIN-RAW-EXPORTER-001 / FOCUSED_TEST_PASS / CPP_BUILD_0_0 / REAL_DOMAIN_VALID / AUTHORITY_READ_ONLY`。真实三tickmask `(17,2)/(1,96)/(0,12)`；initial CRT/sync calls `3000/1`且tick delta全0；slot0/1 epoch1稳定、无伪lifecycle事件。entity raw `609D39...56C32`与domain raw `A3C337...C39A6`均两次确定性，validator通过；旧可选模式仍valid。Ledger89/16；certificate false，Unity exporter仍待。

> **2026-09-03 B0 input/RNG/slot raw合同 focused 通过：** `NTSD28-B0-INPUT-RNG-SLOT-RAW-CONTRACT-001 / FOCUSED_TEST_PASS / BUILD_0_0 / DOMAIN_12_12 / EXISTING_21_21 / RAW_5_5 / PRODUCTION_UNCHANGED`。合同SHA `7185B5D2...DCC68AD`；七动作held mask、authority CRT/synchronized、Unity deterministic三个source-native stream、slot occupant/epoch和snapshot-derived birth/death/reuse已冻结，availability/null与RNG delta fail-closed。Ledger88/16与format通过；exporter、非空输入和真实双端raw留后续包，B0仍未完成。

> **2026-09-03 B0 lifecycle pending binding纠错 focused 通过：** `NTSD28-B0-LIFECYCLE-PENDING-BINDING-CORRECTION-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_38_0_9 / REAL_CLASSIFICATION_CORRECTED`。authority terminal-code pending+lifecycle_code支持encoded reset，Unity PendingFlushDestroy是更广direct-release标志；错误candidate已改missing/null。首次self-test 20/21暴露并修正candidate非空旧guard，最终21/21；job `d4bc39d450b94265b18d0e4214140bd4` 证明true不泄漏；raw `A9F6A286...EBE09E`为10/37/60，生产实现留B7。

> **2026-09-03 B0 environment state binding纠错 focused 通过：** `NTSD28-B0-ENVIRONMENT-STATE-BINDING-CORRECTION-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_38_1_8 / REAL_CLASSIFICATION_CORRECTED`。native Entity+0x320 environment_state在Unity无等价字段；错误Unk328 candidate已改为missing/null。job `a86feda5f97e4e2a9f4423d0cd2ba22a` 证明Unk328=-3不泄漏；raw `1BEF3981...65768E6`仍9/38/54但分类为UNITY_BINDING_MISSING；生产实现留B4/B5。

> **2026-09-03 B0 owner slot binding focused 通过：** `NTSD28-B0-OWNER-SLOT-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_38_2_7 / REAL_BASELINE_UNCHANGED`。native Entity+0x354 owner_slot→Unity OwnerSlotIndex晋级VERIFIED；job `b4b2472b4c85491291a05d28bc6978cd` 以OwnerSlot17并用19/21/23相邻引用防误绑；raw `7EB8DF9C...B6728F9`，差异9/equal38/occurrence54保持；production owner未改。

> **2026-09-03 B0 participant class binding focused 通过：** `NTSD28-B0-PARTICIPANT-CLASS-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_37_3_7 / REAL_BASELINE_UNCHANGED`。native Entity+0x344 participant_class→Unity Unk344晋级VERIFIED；job `e879e34cb57b4f4291d1b33d01d71f9e` 以非零4通过；raw `8DA9F2B6...1C79FB`，差异9/equal38/occurrence54保持；production participant未改。

> **2026-09-03 B0 battle group binding focused 通过：** `NTSD28-B0-BATTLE-GROUP-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_36_4_7 / REAL_BASELINE_UNCHANGED`。native Entity+0x364 battle_group从错误Team candidate更正为RelationTeam并晋级VERIFIED；job `6bc4214b1894458a85cbb8bab998577c` 以Team11/RelationTeam13验证输出13；raw `FDD8F748...AB2E05D`，差异9/equal38/occurrence54保持；production relation未改。

> **2026-09-03 B0 HP bound bindings focused 通过：** `NTSD28-B0-HP-BOUND-BINDINGS-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_35_5_7 / REAL_BASELINE_UNCHANGED`。effective/base max HP→HPBound/HP3源链闭合；job `b050dd2519714976b62eb10f7c1b05bc` 以480/500互异值通过；raw `D1628540...78E1D5`，差异9/equal38/occurrence54保持；一次误跑全量1601项有10项任务外失败，精确目标6/6通过；production vitals与DAT未改。

> **2026-09-03 B0 attacker rest binding focused 通过：** `NTSD28-B0-ATTACKER-REST-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_33_7_7 / REAL_BASELINE_UNCHANGED`。job `d43cf6e368094a6c81d2151055a9e787` 以AttackExempt5通过；raw `B9CFC67D...11A7E87`，差异9/equal38/occurrence54保持；production rest未改。

> **2026-09-03 B0 hit reaction binding correction focused 通过：** `NTSD28-B0-HIT-REACTION-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_32_8_7 / REAL_BASELINE_UNCHANGED / GLOBAL_LEDGER_PASS`。旧HitStop candidate已更正为Fall；job `42b910d17b5a47fd8aa2b4614aaa12fb` 以Fall60/HitStop4通过；raw `E0C56A7B...381C055`，差异9/equal38/occurrence54保持不变；Ledger80/14 PASS，production hit未改。

> **2026-09-03 B0 revival triplet correction focused 通过：** `NTSD28-B0-REVIVAL-FIELD-BINDINGS-001 / FOCUSED_TEST_PASS / BUILD_0_0 / AUTHORITY_RAW_VALID / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_31_9_7 / GLOBAL_LEDGER_PASS`。current lives=HP2Orig、next lives=HPOrig、next HP=RespawnCount；job `34e6199d932240f797443549ee5d468c` 以4/7/320通过；Unity raw SHA `97617669...ED37B7D`，差异11→9、equal36→38、occurrence66→54；Ledger79/14 PASS，production revival未改。

> **2026-09-03 B0 motion hold / FrameDelay binding focused 通过：** `NTSD28-B0-MOTION-HOLD-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / SELFTEST_21_21 / RAW_SELFTEST_5_5 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_28_10_9 / GLOBAL_LEDGER_PASS`。job `0f6ee3e497964378b1d2b8d9b7c141b8` 明确覆盖+3/-5；raw SHA `362F1811...7C09D95`，真实差异12→11、equal35→36、occurrence72→66；Ledger78/14 PASS，生产逻辑未改。

> **2026-09-03 B0 weapon HP binding focused 通过：** `NTSD28-B0-WEAPON-HP-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / SELFTEST_21_21 / RAW_SELFTEST_5_5 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_27_10_10 / GLOBAL_LEDGER_PASS`。job `16bf469ff22d48f9ad1ba1d977496949` 明确覆盖37与-1；raw SHA `AE935277...24F67E`，真实差异13→12、equal34→35、occurrence78→72；Ledger 77 records/14 files PASS，生产writer/DAT/资源未改。

> **2026-09-03 B0 frame history bindings focused 通过：** `NTSD28-B0-FRAME-HISTORY-BINDINGS-001 / FOCUSED_TEST_PASS / REAL_BASELINE_PASS / REQUEST_2_2_DETERMINISTIC / JOINT_6_6 / MATURITY_26_10_11`。工具0/0+21/21+5/5，Unity compile0；同一Editor request两次raw同SHA `9F5ABB4E...6E0D5`，真实差异14→13。桥恢复后job `16bf469ff22d48f9ad1ba1d977496949` 实际通过非零Frame.Prev9/PrevFrame2=6防交叉断言。

> **2026-09-03 B0 actionLatch/WaitCounter binding focused 通过：** `NTSD28-B0-ACTION-LATCH-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / SELFTEST_21_21 / RAW_SELFTEST_5_5 / UNITY_COMPILE_0 / FINAL_JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_24_11_12 / GLOBAL_LEDGER_PASS`。第一次5/6仅为旧null断言，最终非零WaitCounter8已测；真实差异15→14、equal32→33、occurrence90→84；不改生产frame逻辑。

> **2026-09-02 B0 controlSlot/AnimCounter binding focused 通过：** `NTSD28-B0-CONTROL-SLOT-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / SELFTEST_21_21 / RAW_SELFTEST_5_5 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_23_11_13 / GLOBAL_LEDGER_PASS`。AnimCounter5非零投影已测；真实差异16→15、equal31→32、occurrence96→90；不改production writer。

> **2026-09-02 B0 allocationEpoch normalization focused 通过：** `NTSD28-B0-ALLOCATION-EPOCH-NORMALIZATION-001 / FOCUSED_TEST_PASS / CPP_BUILD_0_0 / CAPTURE_VALID_3_TICKS_6_ENTITIES / REAL_DIFF_CLOSED / AUTHORITY_GUARD_PASS / SELFTEST_21_21 / RAW_SELFTEST_5_5 / GLOBAL_LEDGER_PASS`。slot0/1首次epoch均1；真实差异17→16、equal30→31、occurrence99→96。只改workspace writer；authority/Unity Slot未改。

> **2026-09-02 B0 Unity completed-tick boundary focused 通过：** `NTSD28-B0-UNITY-COMPLETED-TICK-BOUNDARY-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / JOINT_EDITMODE_6_6 / EXACT_CHARACTER_DELTA_2_PER_TICK / GLOBAL_LEDGER_PASS`。diagnostic bootstrap不再注入entry-clear；每个输出tick必须真实跑两个exact character frame-tick。新Unity raw SHA `6CCC4FAE...2AB86`；不改production NeedClearInput/driver。

> **2026-09-02 B0 frameCounter binding focused 通过：** `NTSD28-B0-FRAME-COUNTER-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / SELFTEST_21_21 / RAW_SELFTEST_5_5 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_22_11_14 / GLOBAL_LEDGER_PASS`。真实双实体frameCounter为tick1/2/3，差异18→17、equal29→30、occurrence105→99；只修diagnostic binding，不改战斗writer。contract SHA `B186E4C6...834C3`。

> **2026-09-02 B0 raw entity comparator focused 通过：** `NTSD28-B0-RAW-ENTITY-DIFFERENCE-001 / FOCUSED_TEST_PASS / BUILD_0_WARN_0_ERROR / RAW_SELFTEST_5_5 / EXISTING_SELFTEST_21_21 / REAL_3_TICKS_6_PAIRS_282_FIELDS / DIFFERENCE_105_OCCURRENCES_18_UNIQUE / EQUAL_29 / GLOBAL_LEDGER_PASS`。严格区分14个UNITY_BINDING_MISSING与4个非空差异，不归一化例外；首差异为tick1 slot1 allocationEpoch authority2/Unity1。

> **2026-09-02 B0 Unity raw exporter focused 通过：** `NTSD28-B0-UNITY-RAW-SCENARIO-EXPORTER-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0_ERROR / EDITMODE_3_3_PASS / REAL_3_TICKS_6_ENTITIES / OID99_FAIL_CLOSED / DETERMINISTIC_RERUN / GLOBAL_LEDGER_PASS`。真实 current-DAT→LF2Character→Manual StepOneTick 输出 OID2/7 tick1..3；临时driver/数据singleton恢复。Unity raw SHA `5293992E...D5D9C7`；同场景 authority raw SHA `954D3F77...80566` 且validator valid。首次47-field盘点为29字段相等、18字段差异（14 missing + allocationEpoch/frameCounter/baseMaxMp/environmentState）。

> **2026-09-02 B0 Unity entity projection 通过 focused：** `NTSD28-B0-UNITY-ENTITY-PROJECTION-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0_ERROR / EDITMODE_3_3_PASS / FIELDS_47_BINDINGS_21_12_14 / MISSING_NULL / GLOBAL_LEDGER_PASS`。通过现有 MCP bridge 刷新同一 Editor；前两次 focused 暴露 fixture reset 与 RawRuntime/live runtime 所有权错误，最终确认 slot/epoch 来自 ReadOnlySlotView、战斗真值来自 `view.Entity.Runtime`，第三次 3/3 pass。不含 tick/scenario/file writer。

> **2026-09-02 Ledger C++ coverage 完成：** `CHANGE-LEDGER-CPP-COVERAGE-001 / VERIFIED / CPP_ACTUAL_DIFF_COVERED / SYNTHETIC_UNRECORDED_CPP_FAIL_CLOSED / GLOBAL_LEDGER_PASS`。Tools 下 `.c/.cc/.cpp/.cxx/.h/.hpp` 已纳入审计；不改变 roots/exclusions/runtime/resource/authority。

> **HISTORICAL 2026-09-02 B0 source capture（旧身份，当前不得恢复）：** `NTSD28-B0-AUTHORITY-SOURCE-CAPTURE-001`当时使用`C59BD8D3...F2D75`。当前capture工具与身份已由`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002`更新为`B1E13AE1...9033`/`07CD47A0...778F`；旧值只用于历史比较。

> **HISTORICAL 2026-09-02 B0 exporter discovery：** ScenarioLoader的legacy `5EDA5144...19D86B`与当时正式`1277B70B...DAF75`不同；两者当前均非正式身份。当前唯一正式SHA是顶部`B1E13AE1...9033`，workspace source-model与formal-EXE证据边界仍保持分离。

> **2026-09-02 B0 entity field schema 就绪：** `NTSD28-B0-ENTITY-FIELD-SCHEMA-001 / FOCUSED_TEST_PASS / TRACE_SCHEMA_V2 / ENTITY_FIELDS_47 / BINDINGS_21_VERIFIED_12_CANDIDATE_14_MISSING / BUILD_0_WARN_0_ERROR / SELF_TEST_17_17 / CONTRACT_SHA_F5E0154A / GLOBAL_LEDGER_PASS`。47 字段全部 strict；候选/缺失不会被静默忽略。exporter/runtime 尚未开始，因此不是战斗行为已对齐。

> **2026-09-02 Ledger 治理元数据修复完成：** `CHANGE-LEDGER-GOVERNANCE-ONLY-METADATA-001 / VERIFIED / PARSE_PASS / GLOBAL_LEDGER_PASS / NEGATIVE_FIXTURES_PASS`。validator 现严格支持唯一 `change-kind: GOVERNANCE_ONLY` + `code-path: NONE`；普通缺路径、混用路径或用 NONE 覆盖模拟脚本 diff 均 fail closed。旧 Frame Structure parent 已按 Server 同 ID 证据准确标记，无 Unity/runtime/resource/authority 变化。

> **2026-09-02 B0 首包工具合同就绪：** `NTSD28-B0-TRACE-CONTRACT-001 / FOCUSED_TEST_PASS / TOOL_CONTRACT_READY / RELEASE_BUILD_0_WARN_0_ERROR / SELF_TEST_13_13 / CONTRACT_SHA_5B5E4ABF / FORMAT_PASS / GLOBAL_LEDGER_PASS`。独立 `Tools/NTSD28Parity` 已实现 trace contract、validator、streaming first-difference comparator；旧 `Tools/NTSDParity`、Unity/C++ runtime、33 ms/3 ms、Slot 容量、输入/RNG 生产逻辑、Scene 和资源均未修改。首包已闭合，下一包进入 B0 实体字段 schema；exporter/真实双端 trace 仍未开始。

> **2026-09-02 NTSD 2.8 对齐已进入 B0：** `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / STATIC_INVENTORY_COMPLETE / B0_TOOLING_STARTED / UNITY_RUNTIME_IMPLEMENTATION_NOT_STARTED / TRACE_EXPORTERS_PENDING`。唯一差异总表为 `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`。最终目标是非例外战斗规则、状态、时序和战斗表现与正式 NTSD 2.8-Logan 完全一致；Slot 容量模型以及头顶血条、FootSelf、移动端取景、多边形边界、当前随机掉武器和固定世界相机按用户决定保留，原生 HUD、结果表现、背景多层/cycle、完整选择流程按用户决定排除。旧 `NTSDSpec` 与内容/数值/资源差异必须处理；内容策略仍待用户决定。

> **HISTORICAL 2026-09-02 AUTHORITY MIGRATION（SHA已由PROMOTION-002取代）：** 原`1277B70B...DAF75`身份不得恢复；当前唯一正式SHA是顶部`B1E13AE1...9033`。该历史决定中NTSD 2.4/C#/旧game_tick废止、Direction B保护和唯一恢复入口规则仍有效。

> **2026-09-02 旧对齐文档已删除：** 旧 C# authority、NTSD 2.4 C++ release、R0～R8 和 U0～U9 campaign 的 459 份 Markdown 及配套 39 份 Unity `.meta` 已完成审计，并由用户从工作树删除。下方旧状态只保留为压缩历史，不能据此恢复旧 Task/Change；Git 历史中的旧文件也不是当前输入。

> **2026-09-02 Play Mode FootSelf代码已写、Unity运行验收待Editor恢复：** `BATTLE-CENTRAL-RUNTIME-FOOTSELF-001 / CODE_WRITTEN / EXTERNAL_COMPILE_PASS / UNITY_COMPILE_PENDING / FOCUSED_PENDING / PLAY_RUNTIME_PENDING / PRESENTATION_ONLY`。只给human roster input绑定角色显示FootSelf；稳定ground anchor、Preview 64×24/offset/tint复用、单Mesh/submesh、central submission lease与FootSelf→Shadow/actor→HP顺序已写，外部dotnet compile0。当前运行中的Editor 6401 bridge持续timeout且ScriptAssemblies未fresh，不能宣称Unity compile/测试/Play通过；退出当前Play后继续。

> **2026-09-02 原Shadow与FootSelf中央Editor预览可用：** `BATTLE-CENTRAL-EDITOR-FOOT-MARKER-PREVIEW-001 / FOCUSED_TEST_PASS / EDITOR_PREVIEW_READY / USER_VISUAL_REVIEW_PENDING / RUNTIME_NOT_STARTED / PRESENTATION_ONLY / GLOBAL_LEDGER_BLOCKED_EXTERNAL`。Preview把正式`GameConfig.ShadowPrefab`通用阴影与新增FootSelf作为两个独立dynamic-mesh batch，固定shadow→marker→actor→health；FootSelf默认128×48并提供Inspector size/offset、黄色bounds和全局Scene手柄，Shadow使用正式native size/Pivot与灰色bounds。Unity compile0、focused10/10；1000 Shadow与1000 FootSelf各自单segment/单draw；offscreen PASS为shadow1/segment1、foot1/segment1、yellow75、green0、Scene clean。正式Play Mode own-player选择和runtime FootSelf snapshot/batching未开始，等待用户视觉确认；global validator仅被任务外记录阻塞。

> **2026-09-02 Direction B external compile blocker cleared, own validation pending：** `GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001 / BLOCKED / CODE_WRITTEN / CONFIG_HEAD_EQUIVALENCE_PROVEN / RAW_MANIFEST_D32B49D3 / PARSER_ROLLBACK_WRITTEN / FIXTURE_CLEANUP_COMPLETE / NORMALIZED_PROJECTION_PENDING / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。并行`SIMULATION-DIRECTORY-REORGANIZATION-001`旧路径CS2001已清除且Unity compile0；本目录Change未代跑Direction-B focused请求、normalized projection或其SelfCheck，因此该Change自身仍待恢复验证，不据此晋级。

> **2026-09-02 Simulation目录重组实现与运行时验收完成、全局治理受外部记录阻塞：** `SIMULATION-DIRECTORY-REORGANIZATION-001 / BLOCKED / IMPLEMENTATION_COMPLETE / UNITY_COMPILE_0 / MANIFEST_142_142 / GUID_142_142 / CONTENT_HASH_142_142 / ROOT_CS_0 / OLD_PATH_0 / PATH_MATRIX_18_PASS_2_KNOWN / RELATED_200_PASS_1_KNOWN / FULL_1585_EXECUTED_5_EXTERNAL / TWO_CLEAN_PLAY_STOP / SCENE_DIRTY_FALSE / GLOBAL_LEDGER_BLOCKED_EXTERNAL`。142个cs+meta已按Core/Host/Runtime/Passes/Ai/DataContracts/Ecs/Input/Lockstep/Stage/Presentation/Spatial/Diagnostics分层；namespace/API/logic未改。fresh SelfCheck仍停在既有central P4。global validator只因任务外`CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001`缺code-path而exit1，故不报告governance可交付。

> **2026-09-02 SimulationWorld M10 代码与定向运行时已完成、最终验收受外部基线阻塞：** `SIMULATION-WORLD-MODULE-EXTRACTION-001 / IN_PROGRESS / M1_15_15 / M2_11_11 / M3_67_67 / M4_242_242 / M5_24_24 / M6_29_29 / M7_112_112 / M8_110_110 / M9_212_PASS_PLUS_1_KNOWN_BASELINE / M10_COMPILE_0 / M10_AI_158_158 / M10_CONTRACT_35_35 / M10_STALE_PATH_3_3 / M10_TWO_CLEAN_PLAY_STOP / SCENE_DIRTY_FALSE / FINAL_ACCEPTANCE_BLOCKED_EXTERNAL`。full EditMode job `4d26dc2aaed44165807b5da87b4714cf` 完整执行1763项，但仍被position38、package 0.6.0/0.8.0、Blood/Catch static guard和并行S0 WPoint基线阻塞；fresh SelfCheck停在任务外central-render P4断言。`SimulationWorld.cs=6040`行，超过2500报警线的剩余根职责已在Change Record解释，partial与历史partial文件均为0。

> **2026-09-01 Battle Runtime 有序关闭已写入、全量 SelfCheck 外部阻塞：** `BATTLE-RUNTIME-ORDERED-SHUTDOWN-001 / BLOCKED / COMPILE_0 / FOCUSED_4_4 / WORKER_20_20 / OPOINT_8_8 / CENTRAL_13_13 / SINGLETON_2_2 / LIVE_TWO_CYCLE_CLEAN`。固定 11 阶段 `Running→Stopping→Stopped` 事务、worker/spawn/publication/task/renderer/World/pool/boundary cleanup 与 Editor ExitingPlayMode bridge 已实现；最终代码真实两轮 Play/Stop 均 0 cleanup warning、Scene 不脏、rootCount 稳定为13、无 factory/pool/boundary runtime carrier。完整 SelfCheck 连续停在既有 Naruto DDA 240-247 throw-chain 断言，且全局 ledger validator 被两个无关旧 Record 阻断；按范围均未顺手修改，因此 Change 暂不记 VERIFIED。

> **2026-08-31 Scene teardown singleton 重建：** `BATTLE-SCENE-TEARDOWN-SINGLETON-001 / VERIFIED / COMPILE_0 / FOCUSED_1_1_PASS / LIVE_TEARDOWN_PASS / CLEANUP_WARNING_0`。`SimulationTickDriver.OnSingletonDestroyed -> EndBattleAllocationSeal -> BattleRuntimeAllocationGate.Unseal` 原先在 factory/pool 已销毁时用创建型 `.Instance`；现仅 teardown lookup 改为 `TryGetInstance()`，正常 prepare/seal 不变。真实 Play 中两者各1，退出后均0，目标 cleanup warning 0；Scene 未保存。

> **2026-08-31 Runtime BMP 绿色 gutter：** `BATTLE-SPRITE-GRID-SEPARATOR-001 / VERIFIED / COMPILE_0 / FOCUSED_29_29_PASS / LIVE_GREEN_SCAN_0 / PRESENTATION_ONLY`。根因是整 sheet 上传仍保留不透明绿色网格分隔带；现按 BMP 自身像素拓扑清高覆盖率 separator alpha，不依赖 DAT 引用方，不做全局 green-key。真实 Play Mode 全图无长度 >=8 的匹配绿线，两名角色邻域匹配绿色像素均为0；Scene/战斗逻辑未改。

> **2026-08-31 Runtime 头顶 HP：** `BATTLE-CENTRAL-RUNTIME-HEALTH-001 / VERIFIED / COMPILE_0 / RUNTIME_PREVIEW_14_14_PASS / CENTRAL_20_20_PASS / LIVE_STYLE_AND_STABLE_ANCHOR_PASS / PRESENTATION_ONLY`。真实 `LF2Character HP/HPBound/HP3` 进入 immutable frame；每个 central submission slot 使用一张 health mesh，RenderFeature 在 actor segments 后至多追加一次 draw。Play Mode 已复用 Editor authoring 的120x10/-16样式，不同动画姿势的条位置稳定；删除 preview 后回退默认样式但仍渲染。不改战斗写入和30Hz tick。

> **2026-09-02 CAP-S0-1 ACTIVE / manifest drift stop：** `S0-FORMAL-CONTENT-CLOSURE-001 / CHECKPOINT3_REPLAY_COMPLETE / INTERNAL_CHECKPOINT_4_OPOINT_FOCUSED_GREEN / WPOINT_ENVIRONMENT_DIAGNOSIS_ACCEPTED / WPOINT_GOAL_COMPARISON_ACCEPTED / WPOINT_MANIFEST_DRIFT_BLOCKED / CONFIG_DAT_HEAD_MATCH_OBSERVED_AGAIN / WPOINT_CURRENT_MANIFEST_3D07A4E1 / WPOINT_COMPILE_NOT_RUN_AFTER_RESUMPTION / WPOINT_BASELINE_NOT_CAPTURED / WPOINT_PRODUCTION_EDIT_NOT_STARTED / WPOINT_A37_RESOURCE_EDIT_NOT_STARTED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。用户已接受 commit `46460c36...` 诊断与 Goal 对照；但 compile 前 24-DAT manifest 为 `3d07a4e...e6afb`，不是冻结 `afe65d17...cf89c26`，恰有 11 个 Git-clean DAT 漂移。按外部状态停报条件，未运行 compile、未改 ParserV2/WPoint/A37、未 replay/restore；等待恢复方向。

> **Superseded micro-record anchor：** `CLIENT-CONTENT-FRAME-SCALAR-ALIGNMENT-001` 保留为历史0dk-b Task/Change与安全事故记录；CAP-S0-1继承其38条矩阵但不重新打开该Change ID。

> **2026-08-31 Edit Mode 中央预览跟进：** `BATTLE-CENTRAL-EDITOR-PREVIEW-001 / FOCUSED_TEST_PASS / BMP-GRID-SEPARATOR-RECT-FIXED / PERSISTENT-SCENEVIEW-AUTHORING / GLOBAL-LEDGER-BLOCKED-BY-UNRELATED-RECORD / EDITOR-ONLY / PRESENTATION_ONLY`。Editor 示例/验证已改用正式左上切图 Rect；最终 compile0、focused6/6、pixel637/70/green-separator0、Scene dirty unchanged。正式 HP runtime 快照接线、Scene/URP asset、战斗Tick/HP真值、Server/lockstep均未改。

> **2026-08-31 Queue选择（已被上条能力包状态取代）：** Queue0cu/0cx/0d1/0d5/0d6/0db/0df/0dg/0dk-a及parent0dk已VERIFIED/CLOSED；历史0dk-b已并入CAP-S0-1。

> **2026-08-31 Parent0dk结果：** `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001 / VERIFIED / GOVERNANCE_CLOSED`。52/52 unique OID路径映射与scalar/Itr/Bdy/OPoint/WPoint/topology六个串行批次已冻结；无源码/资源/Unity动作。

> **2026-08-31 Queue0dk-a结果：** `CLIENT-CPP-FRAME-MULTIVALUE-PARSER-ALIGNMENT-001 / VERIFIED / CLOSED`。compile0、focused4/4、related287/287、SelfCheck15:35:48、Server dual和validators PASS；无DAT/resource改动。

> **2026-08-31 Queue0dg结果：** `CLIENT-FORMAL-KERNEL-CPOINT-VALUE-SEAM-001 / VERIFIED / CLOSED`。compile0、focused13/13、related295/295、SelfCheck15:00:19、corpus SHA、warmed0B、Server dual和validators PASS；formal marker false、S0 NOT_VERIFIED。

> **2026-08-31 Queue0df结果：** `CLIENT-CPP-CPOINT-RESOLVED-HURT-ACTION-ALIGNMENT-001 / VERIFIED / CLOSED`。compile0、focused5/5、related238/238、SelfCheck14:17:26、corpus SHA、Server dual和validators PASS；formal marker false、S0 NOT_VERIFIED。

> **2026-08-31 Queue0db结果：** `CLIENT-FORMAL-KERNEL-BPOINT-CATALOG-SEAM-001 / VERIFIED / CLOSED`。compile0、focused7/7、related78/78、SelfCheck13:41:15、corpus SHA、Server dual和validators PASS。未增加HUD runtime或battle-state字段。

> **2026-08-31 Queue0d6结果：** `CLIENT-FORMAL-KERNEL-WPOINT-VALUE-SEAM-001 / VERIFIED / CLOSED`。compile0、focused7/7、related239/239、SelfCheck13:11:31、corpus SHA、warmed0B、Server dual和validators PASS。Extra full1522 run的六个非WPoint失败已如实记录；未宣称全量PASS。

> **2026-08-31 Queue0d5结果：** `CLIENT-CPP-WPOINT-DEFAULT-ALIGNMENT-001 / VERIFIED / CLOSED`。Test-first red；fresh compile0；focused10/10；held/cooldown/hit-plan/S0/lockstep related232/232；SelfCheck 12:36:26 PASS；WPoint corpus SHA PASS；Server Debug/Release builds和四套可执行测试双配置PASS；governance validators PASS。仅`WeaponPoint.kind`默认值、focused test和SelfCheck发生变更。

> **2026-08-31 Formal-content/Client授权结果：** formal content实施按Unity依赖顺序；shared Core与adapter/Server preworld consumer边界已冻结，没有合法Server-only替身包。`GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002`持续覆盖Queue具名Client包；每包仍须独立事前Task/Change，但不再逐包重新批准。

> **2026-08-31 Queue0cu `CLIENT-FORMAL-KERNEL-OPOINT-VALUE-SEAM-001` verified / bridge correction：** Unity fresh compile0；official EditMode job `c1f48ca2ef7b4c4c9d1d395b19131ff2` 52/52 PASS；fresh SelfCheck 11:12:51 PASS。用户面板一直Active；fresh `6401 LISTENING`与`WELCOME UNITY-MCP 1 FRAMING=1`证明旧no-listener snapshot只是瞬态，问题是Codex tool visibility，不是Session未启动。Queue0cu CLOSED；marker/S0 unchanged。

> **2026-08-31 Queue0cx `CLIENT-FORMAL-KERNEL-BDY-VALUE-SEAM-001` verified：** fresh Unity compile0；official EditMode job `8a4bb5df745a44659ccae65e1824ff49` 212/212 PASS；fresh SelfCheck 11:52:09 PASS；frozen SHA/warmed0B/Server dual PASS；Client Ledger136/88。Queue0cx CLOSED；marker/S0 unchanged。

> **2026-08-31 Queue0d1 `CLIENT-CPP-ITR-PARSER-DEFAULTS-ALIGNMENT-001` verified：** test-first 4 expected fail/2 pass；fresh Unity compile0；focused6/6；EditMode job `53db60de214d49c982be616e17518057` 212/212；SelfCheck12:09:45 PASS；corpus SHA/Server dual PASS；Client Ledger137/89。Queue0d1 CLOSED；marker/S0 unchanged。

> **2026-08-31 Background/bundle合同与语料：** Queue0dp-d冻结four-int background value/ascending catalog；0dp-e corpus=38 LF/2941 bytes/SHA `B3AFCC...4074`；0dp-f冻结bundle/selection/admission与OPoint closure；0dp-g corpus=32 LF/3510 bytes/SHA `408AD4...A9AB`。均governance-closed/read-only。

> **2026-08-31 Background boundary结果：** Queue0dp-c确认release17个numeric backgrounds、Width/Z为simulation identity，perspective/shadow/layers为presentation；Client data.txt背景数0，单一`Sunagakure` string map未绑定release ID，Scene float derivation不能成为formal content。

> **2026-08-31 bundle boundary结果：** Queue0dp-b已分离artifact/room selection并冻结preworld transitive admission；完整Stage identity因缺少immutable background width/Z/perspective合同而继续gated，未实现producer/hash/world action。

> **2026-08-31 Frame full-catalog correction结果：** `GOVERNANCE-S0-FRAME-FULL-CATALOG-EVIDENCE-CORRECTION-001 / ANALYSIS_COMPLETE / FULL_CATALOG_PRODUCTION_PARSER_PROJECTION_CLOSED / RESOURCE_PARSER_PRESENTATION_SCOPES_SEPARATED / CLIENT_GATES_FROZEN / GOVERNANCE_CLOSED / READ_ONLY`。Client/release parser entries15,395/15,377、last-wins IDs15,371/15,377；Queue0dk冻结52-OID field/block scope；Queue0dk-a记录241个common Frame的双值Itr token捕获缺口；309个sound差异是locator mapping。旧977/三项/312-sound结论已supersede。

> **2026-08-31 Character合同/语料结果：** Queue0dp已冻结immutable object/source-order catalog API、18个binary64 default bits、writer/admission/exclusions；Queue0dp-a冻结24 LF/4039-byte corpus，SHA `5ACC300E4D07149869884FFCA9DF03DE45411041809E2E2205D7D3076B2E1FE4`。两者均governance-closed/read-only；Queue0dp-c现为unique active。

> **2026-08-31 Character/Object boundary结果：** `GOVERNANCE-S0-FORMAL-CHARACTER-CONTENT-AUTHORITY-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_CHARACTER_OBJECT_GRAPH_MAPPED / RELEASE_CHARDATA_SCHEMA_CONFIRMED / EIGHTEEN_BINARY64_MOVEMENT_FIELDS_CONFIRMED / FOUR_HUNDRED_SEVENTEEN_WIDTH_FIRST_DIFFERENCES_CONFIRMED / OID_TYPE_CATALOG_BINDING_CONFIRMED / CATALOG_SOURCE_ORDER_BATTLE_SEMANTIC / FRAMESET_OWNER_CONFIRMED / WEAPON_SOUND_RESOURCE_DIFFERENCES_CONFIRMED / PRESENTATION_METADATA_EXCLUDED / SPRITE_COLLISION_ADAPTER_MISOWNERSHIP_CONFIRMED / CHARACTER_CONTRACT_SELECTED / CLIENT_GATES_RECORDED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。两端137 IDs/types/order与numeric text一致；Client有417 binary-width和156 weapon-sound identity差异；Queue0dq/0dr/0ds未授权、未开始。

> **2026-08-31 Frame corpus结果：** `GOVERNANCE-S0-FRAME-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / STRUCTURE_CHECK_PASS / DUAL_DIGEST_PASS / FRAME_CLIENT_GATES_RECORDED / CHARACTER_CONTENT_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。30 LF/4780 bytes/SHA `747C2754BE8E7E65E993A25C8BA1F1D5715D83FC27FE5470BCC7BEC42D922BEC`。

> **2026-08-31 Frame authority合同结果：** `GOVERNANCE-S0-FORMAL-FRAME-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_FRAME_API_FROZEN / RELEASE_TWENTY_TWO_INT_SCHEMA_FROZEN / SOUND_AND_SIX_LIST_IDENTITY_FROZEN / PRESENCE_AND_EMPTY_FALLBACK_FROZEN / FRAME_ID_SORT_AND_DUPLICATE_REJECTION_FROZEN / SIGNED_SENTINEL_PRESERVATION_FROZEN / METADATA_AND_RUNTIME_STATE_EXCLUDED / CLIENT_RESOURCE_AND_POINT_DEPENDENCIES_FROZEN / FRAME_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。`BattleFrameValue`/`BattleFrameSetValue`与canonical writer已冻结；Queue0dn Client seam仍gated。

> **2026-08-31 Frame boundary schema结果 / incidence superseded：** `GOVERNANCE-S0-FORMAL-FRAME-AUTHORITY-BOUNDARY-001 / ANALYSIS_COMPLETE / RELEASE_TWENTY_TWO_INT_PLUS_SOUND_SCHEMA_CONFIRMED / DEFAULT_AND_EMPTY_FALLBACK_MATCHED / SOURCE_ORDER_LAST_WINS_MATCHED / FRAME_VACTION_SCHEMA_GAP_CONFIRMED / CURRENT_CONTENT_INCIDENCE_SUPERSEDED_BY_QUEUE0DO_C / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。保留generic schema/lookup结论；不得再使用旧977/三项scope。

> **2026-08-31 weapon-strength boundary结果：** `GOVERNANCE-S0-FORMAL-WEAPON-STRENGTH-AUTHORITY-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_LEGACY_GRAPH_MAPPED / PRODUCTION_CALL_GRAPH_UNREACHABLE / CURRENT_312_WPOINT_ATTACKING_ZERO / RELEASE_SCHEMA_ABSENT / RELEASE_KIND5_ITR_OWNER_CONFIRMED / FORMAL_CONTENT_EXCLUDED / CLIENT_RETIREMENT_GATE_RECORDED / FRAME_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。7 blocks/28 entries是legacy-only；release以holder-frame Itr拥有kind-5；Queue0di Client retirement未授权、未开始。

> **2026-08-31 CPoint corpus结果：** `GOVERNANCE-S0-CPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / CPOINT_CLIENT_GATES_RECORDED / WEAPON_STRENGTH_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。16 LF/3700 bytes/SHA `7FDEA9EB056452FD204BA1302E46F6D042F7818CF3EECB4C6D112AD514C75E88`。

> **2026-08-31 CPoint authority合同结果：** `GOVERNANCE-S0-FORMAL-CPOINT-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_CPOINT_API_FROZEN / RELEASE_NINETEEN_SCALAR_SCHEMA_FROZEN / ZERO_DEFAULT_AND_SIGNED_PRESERVATION_FROZEN / ALIAS_RESOLUTION_AND_FINGERPRINT_FROZEN / ORDERED_LIST_AND_PRIMARY_FROZEN / RUNTIME_ENTITY_WRITER_BOUNDARY_FROZEN / CLIENT_CORRECTION_AND_SEAM_GATES_FROZEN / CPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。19-scalar/list/alias/runtime-owner合同已冻结；Queue0df已关闭，Queue0dg READY。

> **2026-08-31 CPoint boundary结果：** `GOVERNANCE-S0-FORMAL-CPOINT-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_CPOINT_GRAPH_MAPPED / RELEASE_NINETEEN_SCALAR_SET_CONFIRMED / ALIAS_SOURCE_ORDER_MATCHED / UNITY_RESOLVED_HURT_CONSUMER_FIRST_DIFFERENCE_CONFIRMED / UNITY_SINGLETON_LAST_WINS_DIFFERENCE_CONFIRMED / RUNTIME_ENTITY_WRITER_OWNER_CONFIRMED / CURRENT_33_BLOCK_INCIDENCE_FROZEN / CPOINT_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Current content 33 blocks；parser alias aligned；two caught-hurt consumer差异已由Queue0df验证闭合。

> **2026-08-31 BPoint corpus结果：** `GOVERNANCE-S0-BPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / BPOINT_CLIENT_SEAM_GATED / CPOINT_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。13 LF/597 bytes/SHA `AD8B3E1DD4D020196183C2F2B8B76C1E27F5CFD8FBD938A48AA8DBA95FC81647`。

> **2026-08-31 BPoint catalog合同结果：** `GOVERNANCE-S0-FORMAL-BPOINT-CATALOG-VALUE-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_BPOINT_API_FROZEN / TWO_SCALAR_SCHEMA_FROZEN / ORDERED_LIST_AND_PRIMARY_FROZEN / EMPTY_LIST_DISTINCT_FROM_ZERO_VALUE / CATALOG_WRITER_FROZEN / BATTLE_STATE_EXCLUSIONS_FROZEN / CLIENT_SEAM_FROZEN / BPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Two-scalar/list identity/primary/catalog-only domain已冻结；Queue0db Client seam未开始。

> **2026-08-31 BPoint domain结果：** `GOVERNANCE-S0-FORMAL-BPOINT-DOMAIN-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_BPOINT_GRAPH_MAPPED / RELEASE_TWO_SCALAR_SET_CONFIRMED / RENDERER_ONLY_LIVE_USE_CONFIRMED / BATTLE_STATE_AND_CHECKSUM_EXCLUDED / CATALOG_IDENTITY_INCLUDED / UNITY_SINGLETON_LAST_WINS_DIFFERENCE_CONFIRMED / CURRENT_DEPLOYED_BPOINT_ZERO / BPOINT_CATALOG_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。BPoint进入CatalogFingerprint但排除于battle world/checksum/snapshot/history；current content为0条。

> **2026-08-31 WPoint kind5 fallback结果：** `GOVERNANCE-S0-WPOINT-KIND5-FALLBACK-REACHABILITY-001 / ANALYSIS_COMPLETE / STATIC_PRODUCTION_CALL_GRAPH_UNREACHABLE / RUNNER_PREPROCESS_ALWAYS_APPLIED / DISABLED_SHADOW_DATA_ORIENTED_MODES_CLOSED / INVALID_PLAN_FALLBACK_REUSES_RUNNER / DIRECT_TEST_DIAGNOSTIC_ENTRY_RETAINED / WPOINT_EXTRAS_NOT_FORMAL / FUTURE_FAIL_CLOSED_REMOVAL_GATE_FROZEN / BPOINT_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Current production candidate modes/fallbacks均复用runner runtime-Itr preprocess；direct internal test entry保留为未来fail-closed cleanup gate。

> **2026-08-31 WPoint corpus结果：** `GOVERNANCE-S0-WPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / WPOINT_CLIENT_GATES_RECORDED / KIND5_FALLBACK_AUDIT_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。16 LF/1608 bytes/SHA `5A3B6B197BBEBA859ECCD4C4EE853CA8A655B3ABF378FE34E1FF7641DB95A926`。

> **2026-08-31 WPoint authority合同结果：** `GOVERNANCE-S0-FORMAL-WPOINT-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_WPOINT_API_FROZEN / RELEASE_NINE_SCALAR_SCHEMA_FROZEN / ZERO_DEFAULT_FROZEN / SOURCE_ORDER_AND_PRIMARY_ENTRY_FROZEN / UNITY_EXTRAS_FAIL_CLOSED / EMPTY_PRIMARY_FALLBACK_FROZEN / CLIENT_GATES_FROZEN / WPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Nine-scalar/all-zero/full-list identity/primary-entry runtime边界已冻结；Queue0d5/0d6 Client包未开始。

> **2026-08-31 WPoint boundary结果：** `GOVERNANCE-S0-FORMAL-WPOINT-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_WPOINT_GRAPH_MAPPED / RELEASE_NINE_SCALAR_SET_CONFIRMED / KIND_DEFAULT_FIRST_DIFFERENCE_CONFIRMED / UNITY_EXTRAS_CLASSIFIED / FIRST_ENTRY_RUNTIME_OWNER_CONFIRMED / LEGACY_KIND5_FALLBACK_RISK_MAPPED / WPOINT_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Current content 312 blocks、无multi-WPoint frame、无显式Unity extras。

> **2026-08-31 Itr corpus结果：** `GOVERNANCE-S0-ITR-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / ITR_CLIENT_CORRECTION_GATED / WPOINT_VALUE_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。13 LF/3442 bytes/SHA `0F43B27514C3E26B4DBAC75C4CA7EF8AB2B994730BC5EEAE705DDDE2086516D1`。

> **2026-08-31 Itr authority合同结果：** `GOVERNANCE-S0-FORMAL-ITR-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / UNITY_CONSUMER_ORDER_FROZEN / IMMUTABLE_ITR_API_FROZEN / RELEASE_26_SCALAR_SCHEMA_FROZEN / ZWIDTH_DEFAULT_FROZEN / PAIR_AND_SECONDARY_FINGERPRINT_FROZEN / UNITY_EXTRAS_FAIL_CLOSED / MUTABLE_RUNTIME_PROJECTION_FROZEN / ITR_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。实现/迁移顺序以Unity消费链为准；Queue0d1由standing authorization覆盖，等待Queue顺序和事前Task/Change。

> **2026-08-31 Itr boundary结果：** `GOVERNANCE-S0-FORMAL-ITR-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_ITR_GRAPH_MAPPED / ZWIDTH_DEFAULT_FIRST_DIFFERENCE_CONFIRMED / PAIR_ENCODING_FIRST_DIFFERENCE_CONFIRMED / UNITY_EXTRA_FIELDS_CLASSIFIED / KIND5_RUNTIME_COPY_MATCHED / ITR_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。358个结构闭合Itr加weapon4 Frame48一个unclosed raw start；既有value first differences不变。

> **2026-08-31 Bdy corpus结果：** `GOVERNANCE-S0-BDY-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / BDY_CLIENT_SEAM_GATED / ITR_VALUE_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。10 LF/508 bytes/SHA `309F4F41AAF152DCCA352A2ABEE4DBD49E0B13221C6734E3849404B6B32EE650`。

> **2026-08-31 Bdy value-contract结果：** `GOVERNANCE-S0-FORMAL-BDY-VALUE-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_BDY_API_FROZEN / KIND_RAW_EXCLUDED / SOURCE_ORDER_AND_RAW_GEOMETRY_FROZEN / FULL_HEIGHT_SENTINEL_FROZEN / CLIENT_SEAM_SCOPE_FROZEN / BDY_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Exact X/Y/W/H与external geometry ownership已冻结。

> **2026-08-31 OPoint corpus结果：** `GOVERNANCE-S0-OPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / OPOINT_CLIENT_SEAM_GATED / BDY_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。10 LF/852 bytes/SHA `2363910A2686D28D5FDE161C00C1777717408FD0736AEF3D5AB7A7CC57C7360E`。

> **2026-08-31 OPoint value-contract结果：** `GOVERNANCE-S0-FORMAL-OPOINT-VALUE-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_OPOINT_API_FROZEN / ORDER_AND_ALIAS_FROZEN / LEGACY_TASK_ADAPTER_FROZEN / INVALID_ENTRY_PRESERVATION_FROZEN / CLIENT_SEAM_SCOPE_FROZEN / OPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Exact API、task adapter与later Client seam scope已冻结；未授权/实施源码。

> **2026-08-31 Point-value boundary结果：** `GOVERNANCE-S0-FORMAL-POINT-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_POINT_GRAPH_MAPPED / OPOINT_EIGHT_SCALAR_SEMANTIC_SET_CONFIRMED / UNITY_EXTRA_FIELDS_CLASSIFIED / OTHER_POINT_BLOCKERS_MAPPED / OPOINT_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。按Unity真实数据流先行完成；ObjectPoint最早可闭合，其他point families各自保留阻塞。

> **2026-08-31 Stage-container shared-owner结果：** `CLIENT-FORMAL-KERNEL-STAGE-CONTAINER-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_STAGE_CONTAINER_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / UNITY_COMPILE_0 / PACKAGE_8_8 / STAGE_RELATED_11_11 / S0_LOCKSTEP_24_24 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Single source/GUID已归Server Core；Unity与.NET 0.8.0 consumers通过；runtime/hash/marker未改。

> **2026-08-31 Stage-container cross-consumer合同：** `GOVERNANCE-S0-STAGE-CONTAINER-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / STAGE_CONTAINER_SHARED_OWNER_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_PRODUCTION_SOURCE_CHANGE`。10 lines/656 bytes，SHA `39816AB63F6BD54E04CE70A589B5CCB40A4D321DCCB9D50328D31B40CD774848`；next0cq。

> **2026-08-31 Stage-container seam结果：** `CLIENT-FORMAL-KERNEL-STAGE-CONTAINER-SEAM-001 / FOCUSED_TEST_PASS / STAGE_CONTAINER_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / TEST_FIRST_MISSING_SEAM_RED / UNITY_COMPILE_0 / FOCUSED_5_5 / RELATED_39_39 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。world content已是defensive-copied immutable set，projection atomic/fail-closed，StageWave/preallocation/snapshot capacity消费value，runtime/snapshot语义未变。

> **2026-08-31 Stage-container seam合同：** `GOVERNANCE-S0-FORMAL-STAGE-CONTAINER-SEAM-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_CONTAINER_API_FROZEN / DEFENSIVE_COPY_AND_FAIL_CLOSED_FROZEN / SOURCE_ORDER_AND_COMMENT_CLASSIFICATION_FROZEN / STAGE_CONTAINER_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY`。三类BCL owner、atomic projection、duplicate/order/comment分类和runtime/snapshot separation已冻结；next0co。

> **2026-08-31 Stage parser defaults alignment结果：** `CLIENT-CPP-STAGE-CAMPAIGN-PARSER-DEFAULTS-ALIGNMENT-001 / FOCUSED_TEST_PASS / STAGE_CAMPAIGN_PARSER_DEFAULTS_ALIGNED / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / TEST_FIRST_2_FAIL_2_PASS / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_30_30 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。failed optional parse不再覆盖初始化默认；stage id/times保留`-1/1`，required spawn id仍丢弃row，valid/order/duplicate保持。

> **2026-08-31 Stage-container boundary审计：** `GOVERNANCE-S0-FORMAL-STAGE-CONTAINER-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_CONTENT_RUNTIME_SPLIT_MAPPED / PARSER_DEFAULT_FIRST_DIFFERENCE_CONFIRMED / LOADER_DEFAULT_ALIGNMENT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY`。static content与progression/wave buffers/snapshot已分离；Unity failed `out`会把C++默认`-1/1`改成0，因此next0cm先修正再冻结containers。

> **2026-08-31 Shared stage-spawn value owner结果：** `CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_STAGE_SPAWN_VALUE_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_7_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / UNITY_COMPILE_0 / UNITY_PACKAGE_7_7 / RELATED_27_27 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。唯一source/GUID现由Server Core持有；Unity仍按loader→DTO→value→task/factory→world消费，adapters/gameplay未变。

> **2026-08-31 Stage-spawn cross-consumer审计：** `GOVERNANCE-S0-STAGE-SPAWN-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / UNITY_ORDER_MAPPED / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / STAGE_SPAWN_SHARED_OWNER_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_PRODUCTION_SOURCE_CHANGE`。14 lines/1269 bytes，SHA `EF0DE76F5DE89D3CE429E80D9F26CB2252DBE90EA77D80EB24A0A2F3F4C03591`；未运行Unity、未移动source。

> **2026-08-31 Stage-spawn value seam结果：** `CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SEAM-001 / FOCUSED_TEST_PASS / STAGE_SPAWN_VALUE_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_23_23 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。八字段immutable value、DTO投影与normal/reserve adapter已focused闭合；warmed mapping 0 B且mutable reserve scratch已移除。

> **2026-08-31 Content model closure审计：** `GOVERNANCE-S0-FORMAL-CONTENT-MODEL-CLOSURE-001 / ANALYSIS_COMPLETE / CONTENT_GRAPH_LAYERED / FULL_CATALOG_CLOSURE_SELECTED / ORDERED_MIGRATION_CUTS_FROZEN / STAGE_SPAWN_VALUE_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。BCL semantic layers、full-catalog+transitive validation与spawn→stage containers→points→frame→character→catalog→producer→factory cuts已冻结；next0ci。

> **2026-08-31 Content producer/binding审计：** `GOVERNANCE-S0-FORMAL-CONTENT-PRODUCER-BINDING-BOUNDARY-001 / ANALYSIS_COMPLETE / PRODUCER_OWNERSHIP_MAPPED / NO_REAL_SERVER_ONLY_PRODUCER / PREWORLD_COMPARISON_POINT_DEFINED / CONTENT_MODEL_CLOSURE_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。Server无semantic content artifact、formal rule/factory或immutable compatible release tuple；actual comparison必须位于future formal factory且早于world construction；next0ch。

> **2026-08-31 Server formal content identity value结果：** `S0-SERVER-FORMAL-CONTENT-IDENTITY-VALUE-001 / FOCUSED_TEST_PASS / SERVER_FORMAL_CONTENT_IDENTITY_VALUE_READY / GOVERNANCE_CLOSED / SERVER_ONLY / CLIENT_INTEGRATION_REQUIRED / DEBUG_RELEASE_0_WARN_0_ERROR / SERVER_CHAIN_PASS / NO_NETWORK_HOST_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Server Protocol现有rule/catalog/stage/build/factory domain/schema/sha256 digest value，StartBarrier不再接受旧三字符串；Client没有修改、编译或验证，actual producer/binding仍待。

> **2026-08-31 Content canonicalization审计：** `GOVERNANCE-S0-FORMAL-CONTENT-CANONICALIZATION-CONTRACT-001 / ANALYSIS_COMPLETE / CANONICAL_IDENTITY_LAYERS_FROZEN / DUPLICATES_FAIL_CLOSED / SHA256_DOMAIN_VALUE_SELECTED / SERVER_IDENTITY_VALUE_PACKAGE_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。semantic manifest、map/list ordering、duplicate/unknown/missing/finite scalar与verify-before-mutation合同已冻结；next0cf。

> **2026-08-31 Content/factory identity boundary审计：** `GOVERNANCE-S0-FORMAL-CONTENT-FACTORY-IDENTITY-BOUNDARY-001 / ANALYSIS_COMPLETE / IDENTITY_TOKENS_UNBOUND / UNITY_BOOTSTRAP_ORDER_MAPPED / CANONICALIZATION_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。当前rule/catalog/stage仅为未绑定token，build/factory identity缺失；已按Unity生产流程映射verify-before-mutation顺序；next0ce。

> **2026-08-31 World-bootstrap factory seam结果：** `CLIENT-FORMAL-KERNEL-WORLD-BOOTSTRAP-FACTORY-SEAM-001 / FOCUSED_TEST_PASS / WORLD_BOOTSTRAP_FACTORY_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_114_114 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。现有construct/validate/logic-only/seed/roster已显式拆出；仍是Client-owned，不含content/stage/AI/shared factory identity。

> **2026-08-31 Atomic-result boundary审计：** `GOVERNANCE-S0-FORMAL-WORLD-ATOMIC-RESULT-BOUNDARY-001 / ANALYSIS_COMPLETE / TERMINAL_WORLD_DISCARD_BOUNDARY_DEFINED / IMMUTABLE_RESULT_DEFERRED / WORLD_BOOTSTRAP_FACTORY_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。S0 failed world终止且不重试；rollback留给S3/S5；final result DTO等待formal domain/Event与真实.NET producer；next0cc。

> **2026-08-31 Full-return commit seam结果：** `CLIENT-FORMAL-KERNEL-FULL-RETURN-COMMIT-SEAM-001 / FOCUSED_TEST_PASS / FULL_RETURN_COMMIT_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_3_3 / RELATED_110_110 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Entry-clear/step-wait现在于host checksum/history/current-tick发布前终止；failed world rollback、immutable result schema、shared world与marker仍待。

> **2026-08-31 Formal snapshot/marker readiness审计：** `GOVERNANCE-S0-FORMAL-SNAPSHOT-MARKER-READINESS-001 / ANALYSIS_COMPLETE / FORMAL_S0_PROOF_MATRIX_CLOSED / FULL_RETURN_COMMIT_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。现有aggregate snapshot是Client-only S3 foundation，不是formal cross-runtime completeness；当前无immutable completed-tick result，formal AI/event/marker gate仍待。

> **2026-08-31 Results reserve terminal integration结果：** `CLIENT-CPP-RESULTS-RESERVE-TERMINAL-INTEGRATION-001 / FOCUSED_TEST_PASS / RESULTS_RESERVE_TERMINAL_INTEGRATION_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_103_103 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Persistent Authority400 teams、mode4 reserve-before-guard和success/failure writes已focused；marker仍false。

> **2026-08-30 Results reserve terminal integration审计：** `GOVERNANCE-S0-RESULTS-RESERVE-TERMINAL-INTEGRATION-001 / ANALYSIS_COMPLETE / RESULTS_RESERVE_TERMINAL_INTEGRATION_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`。Team0、persistent two buckets、third-team ignore、both-alive pause、reserve success/failure writes已映射；next0by。

> **2026-08-30 Results reserve transaction seam结果：** `CLIENT-CPP-RESULTS-RESERVE-TRANSACTION-SEAM-001 / FOCUSED_TEST_PASS / RESULTS_RESERVE_TRANSACTION_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_101_101 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Direct seam覆盖slot20..399、no-RNG preflight、one-Z-RNG、per-entry partial commit和rest conflict fail-closed；尚未接入terminal observer。

> **2026-08-30 Results reserve boundary审计：** `GOVERNANCE-S0-RESULTS-RESERVE-TRANSACTION-BOUNDARY-001 / ANALYSIS_COMPLETE / RESULTS_RESERVE_TRANSACTION_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`。C++ per-entry partial commit、lowest slot20+、missing-data/capacity no-RNG、one Z RNG、entity/rest/committed order和Client owner/gap已闭合；next0bw。

> **2026-08-30 Results activation-reset结果：** `CLIENT-CPP-RESULTS-ACTIVATION-RESET-ALIGNMENT-001 / FOCUSED_TEST_PASS / RESULTS_ACTIVATION_RESET_ALIGNMENT_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_94_94 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。只增加既有table reset再live-guard reset；test-first0/2、final2/2+94/94、fresh SelfCheck和Server双配置通过；scan/reserve/schema/host action未改。

> **2026-08-30 Results terminal alignment审计：** `GOVERNANCE-S0-RESULTS-TERMINAL-ALIGNMENT-SELECTION-001 / ANALYSIS_COMPLETE / RESULTS_ACTIVATION_RESET_ALIGNMENT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`。Full-domain scan与缺失的mode4 reserve强耦合；phase11 table/live-guard reset是首个dependency-closed correction。

> **2026-08-30 Results outcome-host writer seam结果：** `CLIENT-CPP-RESULTS-OUTCOME-HOST-WRITER-SEAM-001 / FOCUSED_TEST_PASS / RESULTS_OUTCOME_HOST_WRITER_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_92_92 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Terminal observation与post-battle navigation已有独立writer；behavior/字段/schema/reserve/marker未改。

> **2026-08-30 Results outcome/host seam审计：** `GOVERNANCE-S0-RESULTS-OUTCOME-HOST-SEAM-SELECTION-001 / ANALYSIS_COMPLETE / RESULTS_OUTCOME_HOST_WRITER_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`。Terminal guard、winner、Results navigation/table、reserve bridge和unconsumed `PendingHostAction`已分组；C++ full-domain scan、reserve spawn、live-guard reset仍为后续alignment gate。

> **2026-08-30 Results scene host-tick结果：** `CLIENT-CPP-RESULTS-SCENE-HOST-TICK-ALIGNMENT-001 / FOCUSED_TEST_PASS / RESULTS_SCENE_HOST_TICK_ALIGNMENT_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_3_3 / RELATED_90_90 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Test-first `0/3`；final focused `3/3`、related `90/90`、fresh SelfCheck与Server双配置全回归通过；schema/math/package未改。

> **2026-08-30 Results host/kernel audit：** `GOVERNANCE-S0-CUT-G-RESULTS-HOST-KERNEL-BOUNDARY-001 / ANALYSIS_COMPLETE / RESULTS_HOST_KERNEL_BOUNDARY_MAPPED / RESULTS_SCENE_HOST_TICK_ALIGNMENT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`。C++ host-global Results/full-tick order、Unity early return、reserve bridge和projection coupling已映射。

> **2026-08-30 roster/label shared owner：** `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_ROSTER_LABEL_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_6_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。Single source/GUID、58-line exact corpus、direct+locked0.6.0、compile0、11/11+87/87+SelfCheck及Server双配置通过。

> **Roster/label contract：** 58 lines，SHA `F4DB5DA03345C08EC1854F67B2146EC47CE2E9EF22BF2290036AF11CABF89FD2`，PowerShell/Node一致；无Client source/build/Unity动作。

> **2026-08-30 roster/label seam：** `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-BOOTSTRAP-SEAM-001 / FOCUSED_TEST_PASS / ROSTER_LABEL_BOOTSTRAP_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / SOURCE_SEAM_ONLY / S0_NOT_VERIFIED`。compile0、5/5、10/10、87/87和fresh SelfCheck通过；package/results/root/marker未改。

> **2026-08-30 Cut F boundary：** `GOVERNANCE-S0-CUT-F-ROSTER-RESULTS-BOUNDARY-001 / ANALYSIS_COMPLETE / ROSTER_LABEL_BOOTSTRAP_SEAM_SELECTED / RESULTS_HOST_SPLIT_REQUIRED / GOVERNANCE_CLOSED / READ_ONLY`。Slot/label与Results host/kernel边界已映射；无Client source/build/Unity动作。

> **2026-08-30 Cut E shared-owner：** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_WORLD_SCALAR_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_5_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / S0_NOT_VERIFIED`。Single source/GUID、direct+locked0.5.0、compile0、10/10+83/83+SelfCheck、Server双配置通过；marker false。

> **2026-08-30 Cut E scalar contract:** `GOVERNANCE-S0-WORLD-SCALAR-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED / READ_ONLY`。18 lines、SHA `1A1C2E...E554`、field order `9+10+4+8+22`均通过；未改Client source。

> **2026-08-30 Cut E scalar seam:** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001 / FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`。compile0、focused5/5、related83/83、fresh SelfCheck通过。

> **2026-08-30 Cut E boundary audit:** `GOVERNANCE-S0-CUT-E-WORLD-CORE-BOUNDARY-001 / ANALYSIS_COMPLETE / WORLD_SCALAR_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`。Broad root/content/entity moves rejected；no source/build/Unity action。

> **Cut E scalar lifecycle anchor:** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SHARED-OWNER-001` = `CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_WORLD_SCALAR_OWNER_READY`.

> **2026-08-30 Cut D shared-owner result:** `CLIENT-FORMAL-KERNEL-REST-STATE-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_REST_STATE_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。唯一source/GUID现由Server-owned Core持有；`0.4.0` direct+locked、Unity compile0、1/1+26/26+17/17+21/21、fresh SelfCheck及Server双配置全回归通过。

> **2026-08-30 历史Client授权（未来使用已于2026-08-31 supersede）：** 它仍是当时已关闭包的授权证据，但不能授权Queue0cu或后继包；G-21要求每个未来Client包取得新的具名批准。

> **2026-08-30 Cut D rest seam result:** `CLIENT-FORMAL-KERNEL-REST-STATE-SEAM-001 / FOCUSED_TEST_PASS / CUT_D_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`。57-line digest/all hashes、dense/sparse order+warmed0B、Unity compile、related38/38（S0 8/8+lockstep9/9）、extra21/21、fresh SelfCheck与Server Release通过；未移动source/GUID，未改package/version、battle/tick、recovery schema或marker。后续shared-owner move须新具名授权。

> **2026-08-30 Cut C shared owner result:** `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`。三份BCL source/GUID现由Server-owned Core单一持有；`0.3.0` direct/locked artifact、Unity package1/1+related33/33+S0 8/8+lockstep9/9+fresh SelfCheck、Server全回归与边界审计通过。RuntimeSlotTable/Registry/Rest、battle rules、30Hz、recovery、formal AI与marker未改；下一Queue只读审计不授权Client源码。

> **2026-08-30 Web preview presentation result:** `WEB-PREVIEW-PRESENTATION-002 / RUNTIME_PENDING / BUILD_PASS / FOCUSED_TEST_PASS / PRESENTATION_ONLY`。DatSkillFlow 主预览现提供30/60/120Hz表现，使用精确位置、相邻Tick、lineage/relation/motion fail-closed gates，并把sprite/shadow表现与authority overlay/编辑分离；30Hz逻辑、DAT、Native CLI、server save、Unity/C++未改。最终build `20260830084617618-18ef901e469444d9b80e355a62838458`；focused23/23、unit315 pass+1 skip、nonbuild integration78/78、manifest/server25/25、Ledger PASS。localhost浏览器权限拒绝，E4待用户观察，不能标VERIFIED。

> **2026-08-30 Cut C seam result:** `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001 / FOCUSED_TEST_PASS / SLOT_LIFECYCLE_SEAM_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`。Client-owned BCL provisional claim→required rest side effect→commit/rollback已拥有本地Generation与canonical per-slot allocationEpoch，structural witness消费canonical epoch。Unity seam5/5、related regressions、fresh SelfCheck、S0 8/8、lockstep9/9、.NET Debug/Release与Server全链通过。后续shared-owner move须新的具名授权；formal AI、snapshot/recovery、marker和阶段状态未晋升。

> **2026-08-30 StageSpawn rest correction result:** `CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001 / FOCUSED_TEST_PASS / STAGE_SPAWN_REST_ALIGNMENT_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / S0_NOT_VERIFIED`. 成功StageSpawn现按C++清ARest、VRest victim row和attacker column；冲突lease在零rest mutation、lease仍有效、无pool leak、无成功allocation event下fail closed。Unity compile `error CS=0`、focused `2/2`、fresh SelfCheck、S0 `8/8`和lockstep `9/9`通过；普通registration/pass未改，S0/S5/marker未晋升。

> **2026-08-30 StageSpawn rest authority audit:** `GOVERNANCE-S0-STAGE-SPAWN-REST-ALIGNMENT-PREREQUISITE-001 / ANALYSIS_COMPLETE / CPP_CLEAR_ON_SUCCESS_AUTHORITY / UNITY_PRESERVE_MISMATCH_CONFIRMED / GOVERNANCE_CLOSED / NO_SOURCE_CHANGE`. 审计关闭时识别出具名Client gate；该gate随后已由`CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001`按focused范围关闭。审计本身未修改/运行Client，S0仍NOT_VERIFIED。

> **2026-08-30 FrameInput shared-owner result:** `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001 / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_FRAME_INPUT_OWNER_READY / GOVERNANCE_CLOSED`. 单一FrameInput source/GUID现由Server-owned `Runtime/Abstractions`持有；`0.2.0` direct/locked-artifact、Unity2/2+48/48+S0 8/8+lockstep9/9+SelfCheck、Server Debug/Release和双Ledger通过。Capture/helpers、battle/tick/Input Actions/wire/transport/recovery/formal AI/marker未改，S0仍NOT_VERIFIED。下一步先只读闭合Cut C slot/lifecycle identity。

> **2026-08-30 FrameInput seam result:** `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001 / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / FRAME_INPUT_SEAM_READY / GOVERNANCE_CLOSED`. Public value/hash、Client capture、reusable preallocation与dense trace已分离；Unity compile0、seam4/4、related44/44、S0 8/8、existing9/9、fresh SelfCheck、warmed0B和Ledger均通过。该包当时未移动source；后续shared-owner包现已独立授权并关闭，formal marker仍false，S0仍NOT_VERIFIED。

> **2026-08-30 shared RNG owner result:** `CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001 / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_RNG_OWNER_READY`. The single source/GUID now resides in the Server-owned UPM/.NET package; vector/direct/artifact consumers, Unity1/1、S0 8/8、existing9/9、fresh SelfCheck和ledger通过。Battle/tick/Scene/resources/Input Actions/transport/recovery/S1/marker未改；S0仍NOT_VERIFIED。

> **2026-08-30 S0 continuity result:** `S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001 / FOCUSED_TEST_PASS / CLIENT_TEST_ONLY / TEN_DOMAIN_CONTINUITY_READY / S0_NOT_VERIFIED`. MCP new1/1, S0 8/8, existing9/9均0 failed/skipped；fresh self-check PASS，error CS0。只改现有S0 Editor fixture；真实角色逐tick输入消费与十named hashes三world连续性已证实，shared formal Kernel/C++ mapping仍待。

> **2026-08-30 S0-WITNESS-001 result:** `FOCUSED_TEST_PASS / CLIENT_S0_WITNESS_READY / S0_NOT_VERIFIED`. Unity MCP连接`gameplay-ability-system-for-unity@b1b02287`；当前S0 fixture `7/7`、existing lockstep `9/9`，均0 failed/skipped；fresh self-check为PASS，Console `error CS`为0。未新增Client源码diff，未触碰用户Scene/地图/背景资源、Input Actions、30 Hz、battle rules、transport或recovery。下一门槛是独立formal shared-Kernel包，不得将本结果扩张为S0 VERIFIED。

> 最后更新：2026-08-30
> 状态口径：只记录已检查的事实、明确推断和未知项；不以聊天历史作为唯一项目记忆。


## 当前阶段

- **2026-08-29 WORKER-UNITY-BOUNDARY-001 已关闭当前卡死**：`VERIFIED / SAFE-SYNC-FALLBACK`。Renderer-bound world 现在以原因码 `unity-presentation-bindings-are-still-attached` 禁止启动 dedicated worker并继续同步主线程tick；pure-logic worker合同保留。Unity worker整类job `881e133b32ae4d3f82043dc29ecec66d` 20/20 PASS；真实Play至tick2860为unpaused/failure=null，结构统计已发生66次Free，`Dedicated simulation worker failed`与`EnsureRunningOnMainThread`均0条。该结论不代表Unity-bound worker或其性能已实现；未来重启worker必须独立设计主线程presentation detach/release。
- **2026-08-26 MAPCFG-005 已写入代码与资产**：`CODE_WRITTEN / COMPILE_PENDING`。用户确认删除 `Desert01_Presentation`，将背景图并入 `Desert01_Boundary`，并从地图资产数据中删除 `boundaryName` / 多边形 `name`。当前收敛为单一 Boundary Asset：`mapId/displayName/revision/backgroundSprite/boundaries(polygons/verticesWorld)`；加载到既有 `BoundaryWall` 时仅在内存生成序号名以保留共享运行时兼容接口。已修改 BoundaryDefinition/Catalog/Bootstrap/BoundaryWallManager、四个 MAPCFG focused Editor tests、Desert01/Catalog 资产，并删除独立 Presentation 脚本/资源。静态契约检查、`git diff --check` 和 MAPCFG-005 Ledger 覆盖校验已通过；Unity 正式程序集和 focused test 仍待。
- **2026-08-26 MAPCFG-005 验证边界**：Unity Editor 修复前实际编译日志发现 `BattleMapBoundaryDefinition.cs` 4 个 `CS0122`，现已通过构造函数 deep-copy 修复；随后复用 Unity 生成的 Roslyn 参数完成交叉编译，退出码 `0`，输出仅有工程既有 warnings。当前 Unity Editor PID `37088` 的 `Assembly-CSharp*.dll` 仍停留在 2026-08-25 17:24:38/17:24:39，修复后的正式程序集尚未生成；22:52:28 的 `BattleRuntimeSelfCheck=PASS` 仍是旧程序集结果，不能计入本包。临时 dotnet wrapper 因生成工程所需 `Temp\\bin\\Debug` firstpass/package DLL 缺失而 CS0006，未进入项目源代码编译；该结果不作 compile 结论。不得把 MAPCFG-005 标为 `COMPILE_PASS`、`FOCUSED_TEST_PASS` 或 `VERIFIED`。
- **2026-08-26 MAPCFG-005 范围边界**：不删除共享 `BoundaryData` / `PolygonData` 的历史兼容字段，不改 BoundaryWall 几何、tick、输入、RNG、checksum、Camera、服务器或 C++；删除仅限 Presentation 类型/资源及其 Catalog/runtime 引用，`Desert01_Boundary.asset` 不再保存名称字段。真实 Battle Scene/Play 验收仍需在代码级验证后单独判断。
- **2026-08-25 MAPCFG-004 代码级验证完成**：`FOCUSED_TEST_PASS / RUNTIME_PENDING / DEPLOYMENT INPUT PENDING`。Unity import/compile 完成；P4 focused job `51942ac652474e6c9ba42427a93ba44a` 为4/4 PASS，P1–P4 cross-phase job `50c3e1586f5145e18b6d990662b920b0` 为14/14 PASS，既有 BattleRuntimeSelfCheck result 于17:33:25写入PASS。空配置继续零mutation且不触发 P4 Stage refresh；实际Map prepare才在角色创建前刷新Stage。没有创建真实Map Asset/MapId/Scene/Bg，也未跑Play/Player；当前只缺用户配置资产与引用后的真实Scene验收及本轮final governance。
- **2026-08-25 MAPCFG-004 治理验证**：`RUNTIME_PENDING / DEPLOYMENT INPUT PENDING`。`Tools/Validate-ChangeLedger.ps1` 已通过（105 条 Record、141 个 governed code diff covered），P4 scoped diff也已通过（只有既有 LF→CRLF 提示、无 whitespace error）；当前仅等待用户配置真实 Map Asset/MapId/Inspector 引用后才可进行的 Scene/Play 验收。不得将当前证据写成真实地图已部署。
- **2026-08-25 MAPCFG-004 代码已写**：`CODE_WRITTEN / COMPILE PENDING / DEPLOYMENT INPUT PENDING`。已新增 optional `BattleBootstrap` Catalog+MapId+Boundary manager+同一world Bg renderer 配置、prepare/clear、App/BattleTest 的 fail-close startup gate和四项内存 focused test。空配置仍为零 mutation 的 legacy fallback，且不触发 P4 新增 Stage refresh；只有实际 map prepare 成功时才会在角色创建前刷新 Stage snapshot。未写真实 Asset/MapId/Scene/Bg、Camera/Transform/PPU、DAT、C++、服务器或战斗规则；compile/test/self-check/治理验证均待。正式部署仍必须等待用户配置资产和引用。
- **2026-08-25 MAPCFG-004 预实施**：`IN_PROGRESS / PRE-CODE / DEPLOYMENT INPUT PENDING`。只读发现当前没有生产 `BattleMapBoundaryDefinition`、`BattleMapPresentationDefinition` 或 `BattleMapCatalog` Asset，因此不会猜测正式 MapId、生成默认 Asset 或覆盖当前 Bg/Scene。P4 仅先实现 optional startup config：Catalog+MapId+Bg Renderer 全部配置时，在角色创建/解除暂停前加载；两项均空则保留 legacy fallback；任何半配置或无效依赖 fail-close。Task、Change Record、Ledger、Handoff 和计划已在代码前建立；实际 Map deployment/Play验收等待用户配置。
- **2026-08-25 MAPCFG-003 预实施**：`IN_PROGRESS / PRE-CODE`。P3 只补 Asset ↔ Scene 的 explicit authoring：world X/Y deep copy、MapId 可见、Load/Apply 按钮、Undo/dirty、名称/数量 mismatch fail-close；不会自动保存/覆盖、不会创建/删除用户 walls、不会在 runtime source active 时写 authoring Scene。只读确认现有 JSON export/BoundaryWallEditor/Manager inspector 足够复用。Task、Change Record、Ledger、Handoff 与父计划已在代码前建立；尚未写 MAPCFG-003 C#、Scene、Asset 实例、Bootstrap、C++ 或 battle logic。
- **2026-08-25 MAPCFG-003 代码已写**：`CODE_WRITTEN / VERIFICATION PENDING`。Asset deep-copy replace、wall world X/Y capture、Manager explicit Load/Apply、runtime-source guard、Undo/dirty 和 Inspector MapId/confirmation UI 已写，另有三项 focused Editor tests；没有保存或修改实际 Scene/Asset、没有改几何、Bootstrap、Camera、Bg、C++ 或 battle logic。下一步只做 static/Unity focused 验证与治理校验；P4 integration 仍排除。
- **2026-08-25 MAPCFG-003 focused 结果**：`FOCUSED_TEST_PASS / CROSS-PHASE REGRESSION PENDING`。Unity重编译后 P3 focused job `5e4b965f9e7b4452a5c6e236117b673a` 为3/3 PASS，验证 explicit round trip/deep copy、name mismatch fail-close、runtime carrier active guard；static audit 未找到 SaveAssets/SaveScene，scoped diff通过。P1/P2 shared bridge回归与final governance仍待；P4 integration不在本包。
- **2026-08-25 MAPCFG-003 结果**：`FOCUSED_TEST_PASS / P4 READY / MANUAL-INSPECTOR PENDING`。cross-phase job `63182377db004cb084fc830402bbb878` 为10/10 PASS，覆盖P1/P2/P3所有当前 focused contracts；P3写入后 existing self-check result 于16:26:35为PASS。静态审计无SaveAssets/SaveScene，且没有写Scene/Asset实例；final ledger validator（104 Record / 139 governed diff covered）与scoped diff通过。P3仅完成可测试的Editor authoring bridge；真实用户Inspector点击和MapId/Catalog/Bootstrap/Player集成仍归P4。
- **2026-08-25 MAPCFG-001 结果**：FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED。Boundary Asset、Presentation Asset、Catalog 与内存 focused Editor test 已写；真实 Unity import/compile 后，job a0b70302cb314b0cbb0a6b6d3fee0457 为 4/4 PASS，Console 无 MAPCFG C# error；`Tools/Validate-ChangeLedger.ps1` 实际通过（102 条 Record、135 个当前 governed code diff 均有记录覆盖），scoped diff check 也通过。BoundaryWall 几何、Scene、runtime、Camera、Bg、GameConfig、C++、网络和 lockstep 均未改。P1 尚未接入 runtime，因此未运行 BattleRuntimeSelfCheck/Play Mode；下一包为 MAPCFG-002，仅让现有 BoundaryWallManager 加载选中 Boundary Asset 并保持所有现有 API 语义。
- **2026-08-25 MAPCFG-002 结果**：`RUNTIME_PENDING / P3 READY / P4 INTEGRATION PENDING`。首次 Unity compile 的 test-only NUnit collection overload CS1503 已留档并最小替换为 local loop；后续 Unity assembly 已重编译，focused job `850175a9e86141f680f03e2bcb26f7b5` 为 3/3 PASS，覆盖 query parity、union、deterministic random、Stage bounds、failed-load retention 和 explicit clear fallback。现有 `BattleRuntimeSelfCheck` 的项目结果文件于 15:53:47 写入 `PASS`；最终 ledger validator（103 Record / 138 governed diff covered）与scoped diff也通过。没有改现有几何函数、Scene、Asset 实例、Bootstrap、Camera、Bg、C++ 或 battle logic。未运行 Play Mode，真实 MapId/Catalog/Bootstrap/Scene/Player 接线仍只属于 P4。
- **2026-08-25 Map ID + BoundaryWall 配置化计划**：BATTLE-MAP-BOUNDARY-ASSET-001 / IN_PROGRESS / P1 FOCUSED_TEST_PASS / P2 PRE-CODE。用户已澄清可行走区域就是现有 BoundaryWall 与 BoundaryWallManager 已实现的任意多边形，不是矩形，也不是待新增的 C++ battle physics。本任务只把当前多边形 world X/Y 数据按 MapId 保存为 BattleMapBoundaryDefinition，并用同 MapId 的 BattleMapPresentationDefinition 保存背景/表现资源；启动时按 MapId 让现有 BoundaryWallManager 加载边界并保持 IsPointWalkable、IsRectWalkable、随机采样和 legacy Stage bounds 的现有语义。此前 BATTLE-MAP-ASSET-ARCHITECTURE-001 的 C++ audit、矩形首发、StageFingerprint 和 M0 至 M7 范围在未写代码前已 SUPERSEDED。当前规范为 Assets/NTSD/Docs/battle-map-boundary-asset-configuration-plan.md；MAPCFG-002 的独立 Task / Change Record / Ledger / State / Handoff 已完成，下一动作可在其范围内修改 C#。P1 没有改 Scene、Asset 实例、DAT、C++ 或服务器。
- **2026-08-24 外部 Unity compile recovery**：`BUILD-LOCKSTEP-TEST-COMPILE-001 / CODE_WRITTEN`。背景 aspect 修正验证触发全项目编译后，先发现 `InProcessLockstepChecksumWitness.cs` 未被 AssetDatabase 纳入；已通过 Unity 自身 asset refresh 自动生成 `.meta` 并解除该 runtime CS0246。当前仅剩未跟踪的 `InProcessLockstepAuthoritySessionEditorTests.cs` 缺少其 test-local `EmptyController` helper；已补私有无输入 `ILF2Controller` fixture，绝不改 production lockstep、battle runtime或scene。compile / discovery通过后恢复 `CAMERA-PLATFORM-BACKGROUND-001` 的视觉验证。
- **2026-08-25 当前用户授权的 Unity 表现包**：`CAMERA-PLATFORM-BACKGROUND-001 / FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`。同一 world `Bg (2)` map、固定视觉Camera frame与Android最终黑色 overlay 保持；Edit Mode 可开关实时取景已实际用临时 Sprite A/B replacement→private Update→Camera frame→baseline restore 通过（job `78c18d4f2b3246f99ab4b024dfc1e3f6` 21/21）。`XueYuan` duplicate 仍留在用户 Scene 但 source-owner guard 会 fail closed，避免竞争写相机；当前 hierarchy 的 ScenesCamera/Bg 中心一致。不得删/保存 Scene、改 PPU、背景 Transform、Camera.rect/aspect/follow、安全区、battle/lockstep/input/checksum/Stage writer。用户真实 Bg Inspector 换图、Scene/Game有角色、Desktop/Mobile Player与Android真机仍待。
- **2026-08-24 Server bootstrap 实际完成（覆盖以下旧环境阻塞事实）**：`I:\GitHub\Unity_GAS\NTSD_Server` 已是独立 Git/.NET 10 workspace；`S0-SERVER-BOOTSTRAP-001` 在 Server 自己的 Record/Ledger/State/Handoff 下完成两次 bootstrap、Debug/Release `0 warning/0 error` build、四项 self-hosted tests、架构边界检查、Ledger validator 与 no-network local health run。状态是 `FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`，不是 S0 `VERIFIED`。Unity Client 当时未修改、编译、测试或验证，`S0-INPROC-AUTHORITY-001` 当时保持 `CODE_WRITTEN / USER-DIRECTED HOLD`。bootstrap 的 .NET 10/目录/sandbox blocker 已解除；本条中的逐包等待规则是历史状态，当前由顶部 `GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002` 覆盖。
- **2026-08-24 Server-only authority-session 实际完成**：`S0-SERVER-INMEMORY-AUTHORITY-001 = FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`。generic immutable frame、StartBarrier、authority-first fixed replica sequence、checksum first-difference/fail-closed 与 tests 内 TestKernel 已写；Debug/Release build 0 warning/0 error、四项 self-hosted tests、no-network local run、Ledger 与 static audit 均通过。它不是正式 NTSD BattleKernel、没有 Unity/cross-runtime checksum，也没有启动 S1。
- **2026-08-24 S0 Client validation-only 当前证据**：用户明确允许既有 Unity S0 的读取、编译、focused test 和 `BattleRuntimeSelfCheck`，但禁止 Client 代码/场景/资源/配置修改。现有 Editor 刷新后的 `Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 晚于 S0 source；Editor.log 未匹配 C# compile-error 模式，`BattleRuntimeSelfCheck` request 于17:07:33写入 `PASS`。用户随后在 EditMode Test Runner 实际运行 S0 Fixture，截图为 5/5 pass、0 fail，并运行 `BattleLockstepSessionEditorTests`，可见九项均通过。状态是 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`，不是 formal S0 或 `VERIFIED`；跨 runtime 是 S5，不是 S0 gate。
- **2026-08-24 S0-WITNESS-001 / CODE_WRITTEN**：用户授权的最小 Client runtime/test 范围内已完成 mismatch-only witness 接线：两个 InProcess runtime 文件、新 witness 文件和既有 S0 Editor fixture。normal tick 继续只走 aggregate checksum；首次 mismatch 才 capture structured snapshots，并锁存固定域序、RNG、slot/generation 和双方 snapshots。新增 RNG、slot reuse/generation、real test-only entity 三 world cases。尚未获得 Unity compile/test/self-check；未改 battle rules、30 Hz、Scene、资源、配置、S1、Socket、DB、transport 或公网。任何额外 authored script 仍须重新说明范围。
- **2026-08-24 S0-WITNESS-001 编译观察**：当前已打开 Unity 仍未导入新增 witness `.cs`（无 `.meta`、ScriptAssemblies 时间早于源码）。以进程级 `DOTNET_CLI_HOME` 的静态 build 只能看到生成 `.csproj` 未包含新文件，继而在两处引用报 `CS0246`；这不是 Unity/source 编译结论。不得手改 generated csproj/meta 或启动第二个 Editor；下一步由当前 Editor 执行正常 refresh/import 后取得真实编译证据。
- **2026-08-24 S0 validation Ledger 外部结果**：`Tools/Validate-ChangeLedger.ps1` 失败于三个非 S0 的 `BattleBackgroundPlatform*` authored script diff 及 `CAMERA-PLATFORM-BACKGROUND-001` 声明不一致。没有修改、清理或收编这些文件；这是 S0 Ledger PASS 的外部治理缺口，不否定已取得的 compile/self-check 证据。
- **2026-08-24 服务器优先顺序（覆盖本条之前的客户端下一步）**：用户要求暂停修改客户端，先专注独立服务端脚本。`S0-INPROC-AUTHORITY-001` 保持 `CODE_WRITTEN / USER-DIRECTED HOLD`；其 Unity 编译、focused、自检和多 world runtime 延后，不得晋升或删除。独立 Server solution、generic authority-session/TestKernel、build/test/run/审计闭环已由上方两条实际证据完成；当前下一门是 `CLIENT_INTEGRATION_REQUIRED`，在恢复 Unity adapter 与跨端 checksum 前不能把 S0 标为 `VERIFIED`。
- **2026-08-24 持续目标已建立**：当前目标线程 `01a0324a-1bc3-7702-9787-b5e1ccff5111 / ACTIVE`。每次上下文恢复都必须先读服务器 progress Resume Card，再按当前服务器 Work Package 继续；每个包持续更新 Task Contract、Change Record、Ledger、STATE 和服务器进度证据。原先“每个 Client 包暂停等待批准”的历史规则已由顶部 `GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002` 取代；仍须写 `CLIENT_INTEGRATION_REQUIRED` 和事前独立 Task/Change。
- **2026-08-24 历史服务器 Work Package（已被上方实际完成证据覆盖）**：`S0-SERVER-BOOTSTRAP-001 / PREIMPLEMENTATION / READY_WHEN_WORKSPACE_WRITABLE` 是目录和 .NET 10 仍缺失时的预实施记录；它不再是当前事实。现已存在独立 Server repository、.NET SDK `10.0.400` 与完成的 bootstrap/in-memory packages；Unity Client 仍冻结。
- **2026-08-24 历史服务器目标阻塞（已解除）**：连续三轮目录/SDK 缺失的原始 blocker 已由当前 Server workspace、.NET 10 与两项完成包解除；不得把这段历史写回当前阻塞。
- **2026-08-24 历史权限重载证据（已解除）**：旧 sandbox access-denied 探测只说明当时任务未重载 writable root；当前 Server 文件、独立 Git 与测试证据已经存在，不能再把它当作当前 blocker。

- **2026-08-23 WEB-CADENCE-001当前状态**：`RUNTIME_PENDING`。已在 `Tools/DatSkillFlowWeb` 新增独立
  `render-cadence.html`、pure sampler、只读 server flag 与专用 launcher；三栏共用当前
  `ntsd_cpp + NTSD 2.4.1` Native preview trace，只插值 presentation position/camera，绝不改
  frame/DAT wait/opoint/hit/logic。build、48项focused、实际OID2 `open→16-tick preview→close` 与 403
  `read-only-mode` 写拦截与Change Ledger validator均通过；Canvas 人工视觉验收仍待。正常 `index.html`、DAT 编辑/保存、C++、Unity、资源均未改。
  全量 `npm test` 为392 pass、2个既有 main.ts 静态正则失败；详见`WEB-CADENCE-001` Change Record 和 handoff。

- **Milestone**：C++ Release → Unity 战斗场景重新对齐。
- **顶层执行目标**：按 `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R1～R8
  依赖顺序完成 C++ Release → Unity 战斗场景重新对齐；该目标持续有效。每个实际脚本批次仍必须是
  此目标下独立、可回滚的 Work Package，并先建立 Task Contract 与 Change Record；顶层目标不能
  跳过其分层验收或保护边界。根据 `D-009`，计划内 Work Package 连续推进，不再逐包重复等待确认；
  只有真实范围扩大、authority / 保护边界或用户 Change Request 才停止。
- **当前阶段**：C++→Unity 全量差异盘点的静态 source 阶段已完成；R1-SOURCE-001～007 已建立 COV-001～006 合同、唯一差异总台账、依赖图、future repair batches 与分层验收矩阵。`R2-PASS-01`、`R2-PASS-02` 都处于 **RUNTIME_PENDING**；两者仅取得 source、编译和 focused self-check 证据，不是完整 battle 对齐。
- **2026-08-23 当前R07A结果（覆盖下方旧D-RENDER-002状态）**：`D-SCHED-009 + D-RENDER-002 =
  UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。actual collision/hit→frozen publication→same-tick live
  writeback→formal central materialization→Late幂等→next-tick producer/RNG及no-publication生命周期已由worker
  Play tick843～846通过；compile0、18/18、178/178、13/13、20:25:11 self-check、final Console0与
  ledger82/97均PASS。R07B/R07C/R08未执行。
- **2026-08-22 历史R6结果（已被上条R07A覆盖）**：`D-RENDER-002 / R6-PRES-005 / RUNTIME_PENDING`。
  当时Unity已写入RenderDispatch immediate frozen-cycle finalize、CentralOnly no-publication direct advance及
  `[0,5,38,39]`/unavailable/idempotence matrix，但尚无Play证书；该证据演进见R07A收口记录。
- **2026-08-22 R6 adapter认证收口**：`A-RENDER-002 / R6-PRES-006`与`A-RENDER-003 / R6-PRES-007`均为`RUNTIME_PENDING` no-code certification。1.5×只作用body/held表现几何并由scale-delta保持wpoint重合，不进入逻辑pixel/world；fixed-world tick清零release camera/RenderOffset，safe-area只写presentation camera。两项existing fixture均由19:49:12 fresh full self-check实际覆盖并PASS。真实DAT/atlas held锚点、URP safe-area/scene边缘仍留R8 PlayMode；snapshot restore→PreFrame前直接发布可达性为UNKNOWN。
- **2026-08-22 R6自动证据层结论**：D-RENDER-001～005及A-RENDER-001～003已完成source/现有自动证据闭合，所有相关项仍仅`RUNTIME_PENDING`，不是C++ runtime/PlayMode视觉证书。按主计划现在可进入R7逐项优化重新认证；R8继续承担真实战斗场景验收。
- **2026-08-22 当前R7结果**：`D-PERF-001 / R7-PERF-001 / RUNTIME_PENDING`。death-cleanup private proof producer与T14 cross-pass consumer已删除；同点whole-pass proof、three-pass writer、participant filter及public stress/report schema保持。用户Refresh并恢复UnityMCP session后，fresh Unity DLL为20:16:44/45且晚于20:02 source；focused EditMode job `09948d3e3e314d84ab80791d0d2b2070`为15/15 PASS，实际覆盖same-slot current kind2 oracle与两项warmed 0 B；20:22:37 full `BattleRuntimeSelfCheck=PASS`。`B-R7-PERF-001-01`已解决，旧19:49:12 PASS未复用。Play Mode/C++ runtime trace仍待，不能宣称完整VERIFIED。
- **2026-08-22 R7-LATE结果**：`D-LATE-001 / R7-LATE-001 / RUNTIME_PENDING`。entity三段reload与world-owned 4×217+1×218 writer已落地；missing target frame0/8000 HitStun140、最低空槽、34-call RNG、missing217/218、no-slot、generation与高低cursor矩阵均通过。fresh Unity DLL为20:41:14，20:42:47 full self-check=`PASS`，warmed no-slot 512次为0 B/0 RNG。首次CS0234与第二次6×CS0122均已留在Change Record并最小修复。真实DAT Play Mode、GameObject pool表现与C++ runtime trace仍待，不能宣称完整VERIFIED。
- **2026-08-22 R7 Frame/Recovery只读预检**：已建立`R7-FRAME-01-frame-tick-recovery-soa-recertification-preflight-20260822.md`，没有修改脚本。除既有`D-MOV-005`外未发现新confirmed difference；recovery与FrameTick主要顺序/公式映射闭合，但current DAT oid51/52 identity、invalid DAT jump flag恢复性仍为INFERRED/UNKNOWN。必须在R7-PERF-001验收后独立认证，不能由旧Unity A/B测试升级为C++ VERIFIED。
- **2026-08-22 R7-FRAME认证结果**：`R7-FRAME-001 / RUNTIME_PENDING / NO-CODE`。current DAT inventory确认state2000只在type2/type4 weapon，D-MOV-005继续为exact route `INFERRED not reachable`；exact identity writer inventory未发现OID51/52 shell/current-DAT分离。fresh focused job `7b5d94953fca4cdb8947aaa2350277ca`为22/22 PASS，覆盖Recovery/FrameTick legacy-vs-data-oriented、fallback及warmed 0 B；20:42:47 full self-check仍PASS。无脚本改动；mod DAT、identity未来分离、invalid DAT jump flags、Play Mode与C++ trace仍未关闭。
- **2026-08-22 R7 AI旧测试合同修正完成**：`R7-AI-TEST-001 / VERIFIED / TEST-ONLY`。初始UnityMCP job `6fdd44f773344cffbce04404bfddfd86` 捕获旧dead断言expected 0 / actual 1；C++ `prepare_ai_input`与`R3-AI-LIFE-001`确认active HP=0 AI没有self-HP early return。fixture已拆为dead eligible/applied/context-bind与coordinate zero-attempt；production AI未改。fresh Editor DLL 21:01:39、Console error 0、exact job `8c74d8e0a76e427fac3fd7920f5ac234` 2/2、AI sensing/profile job `5c6bad85dc0b43c2a6949d03cfd256fc` 111/111、21:04:52 full self-check及validator/diff均PASS。该VERIFIED只裁决测试合同，`R3-AI-LIFE-001`与完整AI gameplay仍为`RUNTIME_PENDING`。
- **2026-08-22 R7 AI sensing认证结果**：`R7-AI-01 / RUNTIME_PENDING / NO-PRODUCTION-CODE`。C++ `input_handler.cpp:1209-1235,1615-1898` 的first10 move-mode、ground/air target、ground-derived best/lane、cache retain/refresh、team guard与slot20+ special scan已映射到Unity fallback/SoA/indexed/unified authority；生产profile确认为`DataOrientedCanonical`。除上述stale Editor fixture外未发现新的production source-confirmed difference；exact 2/2、AI sensing/profile 111/111及21:04:52 full self-check PASS。`input_handler.cpp:1900+`完整OID decision tree、真实Play Mode、C++ trace与>399 capacity extension语义仍未关闭，下一包必须独立为`R7-AI-02`。
- **2026-08-22 R7-AI-02 decision chain inventory**：`SOURCE-CONFIRMED DIFFERENCE / NO GAMEPLAY CHANGE`。C++ `input_handler.cpp:2055-2204` 的outer random gate内有39个有序helper/call positions；Unity Legacy与DataOriented均只保留positions1–6，缺失7–27与29–37共30 positions，并把现有28/38/39错误放到gate外。另确认optimized snapshot缺OID11 frame290 side-effect所需current frame`hit_j`。现有decision job `3eaff2c1bb474565b2dd4c66d02c49db` 75/75 PASS只证明两条Unity路径共享缩减oracle，不能关闭C++差异。已登记`D-INP-007A/B`、`D-INP-008`、`D-INP-009`并拆为02A～02F；当前未修改production，下一步必须先做02A authority fixture/dispatcher合同。
- **2026-08-22 R7 broadphase认证结果**：`R7-BROAD-01 / RUNTIME_PENDING / NO-CODE`。C++ slot pair→双方向→ITR/BDY exact顺序与Unity BruteForce、role-aware authority-ordinal sort、双方向exact、degenerate fallback和RNG restore已映射；fresh jobs `b5ea30da3c4e42468977e3ab10868fe6` 9/9、`7798184d88024764971712a9a780029e` 58/58、`201e1b9127004d349b14b06df2aa4e6b` 16/16，合计83/83 PASS。focused suite后full self-check在R3-INP-01连续失败，但无代码变化且domain reload后22:13:06恢复PASS，登记`D-TEST-001`静态测试污染。另登记`D-PERF-002`：production GameConfig为空且resolver默认BruteForce，普通NTSD_Battle尚未部署LooseQuadtree；stress显式backend不等于production接线。Play Mode/C++ trace仍待。
- **2026-08-22 R7 frozen/worker认证结果**：`R7-PRES-WORK-01 / RUNTIME_PENDING / NO-PRODUCTION-CODE`。C++ render observation point、Unity frozen capture、latest/world/generation materialization gate与worker publication/ack single-flight已完成source mapping；fresh positive jobs `ab2811b35d8e42f9b0ce8ed4733ed0ed` 13/13、`26be6db261c54e45ae8c15f5cf1a5a11` 11/11、`7e64d65b61924459b9419fd1d5d4bc34` 6/6、`3789a22c55504027b33b0204c6e5f96e` 16/16，合计46/46 PASS。production确认为CentralOnly+dedicated worker+maxCatchUp=1。完整worker suite及fresh-domain exact test都暴露`D-TEST-002`旧current-key清零断言错误；production current key=1与C++一致。另登记`D-TEST-003` joint driver/central/ack覆盖缺口与`D-PERF-003` single-flight部署边界。本包未改脚本；fresh-domain Unity Console编译错误为0，22:32:29 full self-check PASS。下一步为pool/slot/dynamic capacity只读盘点。
- **2026-08-22 R7 pool/slot/capacity盘点结果**：`R7-CAP-01 / INVENTORIED / D-CAP-001 OPEN / NO CODE CHANGE`。C++ slot50 lowest-free与Unity min-heap/page/generation/pending-release映射闭合；focused job `4cc1de5fb20b49609ee0824cd64c4af4` 44/44 PASS，覆盖Mobile 1000、lowest reuse、generation/snapshot、logic pool families、pooled reset、sealed rejection与warmed 0 B。`PoolMaxSize=200`仅warning。confirmed差异是DesktopExtended seal后拒绝page growth，Windows默认512因此battle-time存在prepared-capacity hard cap，违背文档“动态、无production hard cap”保护条款。必须先做R7-CAP-01A容量/0B/admission合同决策，当前不改代码。fresh-domain Unity Console为0条error/warning，22:45:05 full self-check PASS。R7计划列出的优化组至此已全部完成inventory；下一步先汇总repair WPs，不直接进入实现。
- **2026-08-22 R7完整inventory checkpoint**：R7八组优化盘点全部完成，汇总见`RESEARCH/R7-INVENTORY-SUMMARY-20260822.md`。未关闭的gameplay/data差异为`D-INP-007A/B`、`D-INP-008`；coverage/test为`D-INP-009`、`D-TEST-001/002/003`；deployment/architecture为`D-PERF-002/003`、`D-CAP-001`。修复序列已固定在`TASKS/R7-REPAIR-SEQUENCE-after-complete-inventory.md`。当前开始第一个实施包`R7-AI-02A`，它只能建立C++ source-derived 39-position/gate/RNG test oracle，不允许修改production AI。
- **2026-08-22 R7-AI-02A结果**：`R7-AI-002A / VERIFIED / TEST-ONLY RED-WITNESS CONTRACT`。39-position ordinary contract 1 PASS/2 Explicit skipped；position7 job `ae7bf5e441c845628067227aacd36c81`捕获expected DRJ=3/actual0，position28 job `8ba6322f7a984f669afd80610bad5c5e`捕获expected DUA=0/actual3。首次fixture漏计boundary RNG draw及AssetDatabase未导入的total0请求均已作废留档。fresh Editor DLL 22:57:58、Console 0 error、existing AI job `0417660e5b6440c98d93e2c0fb7c8ae1` 75/75、23:01:13 full self-check PASS。production未改，`D-INP-007A/B`仍open；下一包为`R7-AI-02B` HitJ data contract。
- **2026-08-22 R7-AI-02B结果**：`R7-AI-002B / RUNTIME_PENDING`。C++ `ai_frame_hit_j`读取current logical frame DAT `hit_j`、缺失为0；Unity现已为snapshot与frame-motion canonical store补齐HitJ。DAT只在fallback capture或frame bind/write边界解析，UnifiedAuthority consumer只读SoA projection；Frame pending同点发布HitJ，full/refresh comparison与grow/copy均覆盖。最终focused job `d0670d95986c41e7b115b8a77754d23b` 4/4、AI regression `a525edc1f1c64bfe854f3d3218bd1d4e` 212/212、warmed 0 B、fresh Unity compile 0 error、23:43:36 fresh-domain self-check PASS。23:34:04同domain self-check曾在既有`D-TEST-001`失败并已留档。OID11 helper仍未接，真实DAT PlayMode/02F/C++ trace待后续；下一包为02C。
- **2026-08-23 R7-AI-02C结果**：`R7-AI-002C / RUNTIME_PENDING / UNWIRED MODULE`。positions7–16已实现为非partial实例`AiCharacterDecisionModule`；kernel私有RNG clone已无行为地提取为共享值类型，默认dispatcher未调用module。source-derived focused job `b65adcac443844c183272c984934d061` 19/19、clean AI baseline `12463c4731a24aa4ae9919f96599f720` 212/212、warmed 0 B、fresh Unity compile 0 error、00:10:58 full self-check PASS。组合job `202c6992fb784d219c02d1318f605068`只在两个02A Explicit red witnesses按预期FAIL，证明position7/28默认差异仍存在。下一包为02D；02F前不得接线。
- **2026-08-23 R7-AI-02D结果**：`R7-AI-002D / RUNTIME_PENDING / UNWIRED MODULE`。positions17–28已加入现有非partial实例module；position21使用显式scan域并保持400-slot source fixture、strict-farthest/first-tie，position21/22继续，position24/26保留dynamic-modulus RNG顺序。首轮job `4c3e6f659c384014a0eefa180d6ae5c6`唯一失败是OID19漏计position26 path-B前序draw，留档修正后focused `5d265876f2e24159879cd881e7218d80` 26/26、AI regression `3cd69caca0f546338f1ced0500cb4062` 238/238、warmed 0 B、generated Editor build/Unity compile 0 error、00:33:49 fresh-domain self-check PASS。job `0173e28d95bf44ab9df97facc81193cc`两个02A red witness仍按预期FAIL，证明默认dispatcher与position28 production位置未改；下一包为02E。
- **2026-08-23 R7-AI-02E结果**：`R7-AI-002E / RUNTIME_PENDING / UNWIRED MODULE`。positions29–37已加入现有非partial实例module；position30保留first-20 first-match/no-obj-type filter，position31 frame263/264写jump后继续，position34保留first-100/self-inclusion与gate命中后无目标也return true。首轮job `2c095df1751a442c99d61d7c26a3f1db`两个失败是OID5/14漏计position29前序draw，留档修正后focused `1a2716b9caee4fa8bfd6285fc0c3f738` 31/31、AI regression `0eddc4e7c54840d3b5db41d035b63eb3` 238/238、warmed 0 B、generated Editor build/Unity compile 0 error、00:48:12 fresh-domain self-check PASS。job `7a865915ba984168abe0636da7bac54c`两个02A red witness仍按预期FAIL，证明默认dispatcher与production position28未改；02C～02E unwired模块已齐，下一包为02F联合接线合同。
- **2026-08-23 R7-AI-02F结果**：`R7-AI-002F / RUNTIME_PENDING`。Legacy与DataOriented已原子接入outer-gated positions1–39，positions28/38/39不再gate外重复执行；snapshot持久module、pass级shared rows、构造期预分配fallback、RNG trace与matched-position shadow均闭合。self<400保留400域，extended self使用完整capacity，first20/100不扩展。首次夹具前序RNG、旧shared计数、隐式InputHistory分配与fallback预热问题均已留档修正。final authority 3/3、full dispatcher 5/5、fixed-seed production profile-pair 1/1、AI矩阵286/286、warmed 0 B、Unity compile 0 error。02:03:05同domain self-check复现既有D-TEST-001静态污染；domain reload及最终test-only cleanup后，02:07:58 final fresh full self-check PASS。`D-INP-007A/B`的代码级差异已关闭，`D-INP-008/009`自动证据已闭合；真实AI PlayMode、R8与C++ trace仍待，不能宣称完整AI/battle VERIFIED。
- **2026-08-23 R7-TEST-002结果**：`R7-TEST-002 / VERIFIED / TEST-ONLY`。C++ `InputHandler::poll`确认首次Left+Attack后current key=1、Prev=0；旧worker fixture两条0断言与注释已修为current=1，cooldown/history/publication/ack断言保持。exact job `86e6bddd257f4e18bb37433941f1a916` 1/1、class job `41f7b4803c754635b0d7c16abaf73754` 17/17、compile 0 error、02:14:36 fresh full self-check PASS。production未改；下一包为R7-TEST-003。
- **2026-08-23 R7-TEST-003结果**：`R7-TEST-003 / VERIFIED / TEST-ONLY`。formal driver双tickfixture已联合覆盖`buildPresentation=true` worker frozen publication、CentralOnly exact-tick物化、ack/finalization、next-tick unblock与new frame/generation，并验证host不反写原publication。exact job `8f7e88df654449e38a6ac8df97bb6faa` 1/1、worker+central job `acfb083ac4fc458e999a9715b4f45dca` 31/31、dotnet/Unity compile 0 error；focused后force scripts domain reload，02:27:37 fresh full self-check PASS。production worker/driver/render、single-flight、catch-up和gameplay均未修改；下一包为R7-TEST-001静态污染隔离。
- **2026-08-23 当前执行包状态更新**：`R7-TEST-001 / CODE_WRITTEN / TEST-ONLY`。fresh-domain二分已把D-TEST-001锁到`AiDecisionSoAShadowEditorTests.SharedShadow_BuildsOnceAndRefreshesLowSlotBeforeHighSlotEvaluation`：该owner 1/1 PASS后same-domain self-check于02:52:32在R3-INP-01失败。owner测试现已直接witness missing-frame绑定的静态`LF2FrameCache.EmptyFrame.state`被写14，并用finally恢复原值；production AI/frame/input/scheduler未改。当前等待compile及owner/class/286 matrix→same-domain self-check。
- **2026-08-23 R7-TEST-001隐藏依赖补充**：owner cleanup后的exact owner 1/1→same-domain self-check已于02:57:57 PASS；但完整class首次运行暴露`UnifiedAuthority_AscendingRefreshMakesLowVisibleToHighWithoutReverseEarlyVisibility` expected14/actual0，fresh-domain exact也失败。该fixture过去依赖owner遗留的sentinel14；其自身shared-shadow hook没有形成data-oriented canonical-store refresh witness。仍在同一test-only Record内改用现有character-input mutation override并清理sentinel，production不改。
- **2026-08-23 R7-TEST-001结果**：`VERIFIED / TEST-ONLY`。D-TEST-001已二分到shared-shadow owner对静态`LF2FrameCache.EmptyFrame.state`的未恢复写，并发现unified ascending fixture依赖该污染。owner/dependent现均显式own+finally恢复sentinel；dependent使用character-input mutation override形成canonical post-input full refresh。final dependent exact `b8e926eb862a4c4a83ed3124180f3267` 1/1、class `4a05c94370434bddbd1e2afc38425c9e` 66/66、AI matrix `c90b67cf6eb740dfb2ed2715f56dbaf4` 286/286；class后03:03:54、AI matrix后03:06:15同域self-check均PASS，final fresh compile 0 error与03:07:32 self-check PASS。production未改；下一包为R7-BROAD-02 decision matrix。
- **2026-08-23 当前执行包**：`R7-BROAD-02 / IN_PROGRESS / NO CHANGE`。production broadphase仍为空配置→BruteForce。synthetic 1000 fixture已有499,500→500 pair reduction且候选/RNG一致，但历史真实1000-AI harness本身强制Loose且没有current-build同输入Brute/Loose A/B，因此不授权切默认。先fresh复跑83项parity与same-domain self-check，再形成retain/defer决策；本包不改代码/配置。
- **2026-08-23 R7-BROAD-02决策**：`DECISION COMPLETE / RETAIN BRUTEFORCE / NO CHANGE`。fresh role-aware/formal/Loose/participant job `623e91b88792432a87bccd0969b08ba9` 80/80、AirRole nearest `01bea6cea2e340e088683226c81cf713` 8/8，合计88/88；随后same-domain full self-check于03:13:57 PASS。synthetic pair reduction成立，但current-build真实production Brute/Loose A/B、R8 scene parity和real fallback distribution未闭合；历史1000-AI harness本身已是Loose仍未达30Hz。因此保持GameConfig空→BruteForce，未来切换必须独立配置Record。下一包R7-CAP-01A。
- **2026-08-23 R7-CAP-01A结果**：`DECISION COMPLETE / CURRENT CODE CONFORMS / NO CODE`。合同固定为Desktop无固定产品active cap，但每局在unsealed loading/reset/preflight边界按页准备有限、可配置预算；active battle seal后strict 0 B，超预算deterministic reject，不临时unseal/new。Authority400与Mobile1000不变。fresh jobs `fdf01d6739ac47748158eb42d6d81926` 11/11与`e61ed948fc544caf8cc93b31f7859126` 33/33，合计44/44 PASS；03:19:45同域full self-check PASS。现有production符合，D-CAP-001按合同澄清关闭，R7-CAP-01B不需要实施。R7 orders1–11至此关闭，下一阶段R8。
- **2026-08-23 当前执行包**：`R8-WP01 / IN_PROGRESS / CERTIFICATION-ONLY`。已建立current-worktree R8分层矩阵：01A fresh compile/self-check/EditMode；01B移动/输入；01C关系/opoint/lifecycle；01D CentralOnly可见性；01E 1000 active/0GC/30Hz；01F Windows Mono/IL2CPP；01G汇总。UnityMCP已确认Session Active/Configured、socket6401，非项目探测错误已清空。旧U9只作历史baseline，不作为fresh R8证书；任何脚本修复必须先独立Task/Change Record。
- **2026-08-23 R8-WP01A阻塞与修复准备**：fresh compile 0 error/0 warning、03:28:51 full self-check PASS；但full EditMode job `20fcc884b4114ee9a1a3b7f1667c641c`在1357项后FAILED。至少25条capped failure均为BattleHitExecutionPlan ShadowCompare `writerDiff=0x70000000000000`；fresh exact `8d6f29aa8d8043958b29abcf58096e6e` 2/2 FAILED，排除顺序污染。mask对应TargetHp/HPBound(or PP)/ComboCountVic；production R4-HIT-001/003已写，shadow projection未同步。已建立`R8-TEST-001 / PLANNED`，只允许修诊断投影，不改production damage。
- **2026-08-23 R8-TEST-001代码状态**：`CODE_WRITTEN / DIAGNOSTIC-ONLY`。已在BattleEcsHitExecutionPlan同步normal Light/Heavy/Throw adjusted vital/stat和standard/state-sync/D1/active-D1 type3 kind0 raw vital/stat projection；Drink/non-converted kind9不变，production writer/tests未改。尚未取得compile/test证据。
- **2026-08-23 R8-TEST-001首次验证与范围修订**：fresh compile 0 error/0 warning；converted-kind9 exact job `91c41aff34a746faa4517462e090bda1` 2/2 PASS。整类job `9e09666033394a0b8cdb530135d85da7` 178项仅余`StandardType3DamageSupportsDeadAirTarget` expected HP0/actual-10；R4-HIT-001明确kind0 injury仍写HP，因此这是旧断言。已在同Record增加该test path，只允许把HP期望改为-10，frame/fall/production不变。
- **2026-08-23 R8-TEST-001 focused结果 / R8-TEST-002准备**：dead-air exact `0bbeee8428f8406bb8f8ee06b09ba9c9` 1/1、hit-plan class `69e73f14e34c428eb54803db3327cf85` 178/178 PASS；full job `246be3d87338446ea7a877b13f7f88f5`中原hit failures清零，但1357项最终仅W07 structural一项失败。fresh exact `f453a20619b34ef0afe3716a902d7629` 1/1 FAILED。W07仍期待invalid positive link清TargetSlot/HeldWeapon，而R5-LINK-001/C++只清LinkState。已建立`R8-TEST-002 / PLANNED`，只改W07 test fixture，production不改。
- **2026-08-23 R8-TEST-002代码状态**：`CODE_WRITTEN / TEST-ONLY`。W07 fixture与event assertions已从旧0/-1/-1同步为LinkState/Target/Held=0/1/1，target reverse仍2/0；方法名同步。production link/event producer未改，尚未取得compile/test证据。
- **2026-08-23 R8-TEST-002编译与工具状态**：`COMPILE_PASS / TEST-ONLY`。Editor DLL晚于source，dotnet为0 error/18 existing warnings；validator 56 Records/55 governed files PASS。UnityMCP 6401 listener在本次domain reload后未恢复，Unity PID2880仍存活/响应，Editor log未见error CS但有Unity内部TaskCanceledException。等待用户在MCP面板重新Start Session后继续exact/class/full/self-check；禁止启动第二个Editor。
- **2026-08-23 R8 MCP恢复点（覆盖上条旧PID）**：旧Editor已结束；新国际版Unity 2022.3.62f3 PID36240于07:24打开正确project，Package Manager/AssetDatabase/Tundra 2.23s均成功，当前无C# compiler error。新实例尚未启动MCP Session，6401无listener，故`B-R8-MCP-001`成立。所有离线工作已完成；用户只需在MCP For Unity面板点击`Start Session`，随后直接从R8-TEST-002 exact→class→full→self-check恢复，不重做代码或定位。
- **2026-08-23 R8-WP01A最终自动基线**：MCP恢复后，R8-TEST-002 exact `ad10828ee9d741aa8c2068c1ad7db6c8` 1/1、class `4101eded225e493aa48ad1f4549e6d54` 4/4、full EditMode `6a6336d0e1e94abd9585110358012ca5` 1357/1357 PASS（0 failed/0 skipped，170.1558765s）；同域full self-check 07:31:17、强制域重载后fresh self-check 07:32:39均PASS。R8-TEST-001/002均以diagnostic/test-only VERIFIED关闭，B-R8-MCP-001已解除。下一步进入R8真实Play Mode/central/capacity/Player认证；不得把自动基线扩大为battle已对齐。
- **2026-08-23 R8真实Play Mode首个阻塞**：`R8-PLAY-001 / IN_PROGRESS / EDITOR-DIAGNOSTIC-ONLY`。MCP重新确认Session Active/Configured后进入`NTSD_Battle` Play Mode，Console持续报告`NTSDHitboxGizmos.cs:47`把纯C# `LF2Entity`用于`GetComponentInParent<T>`的`ArgumentException`。已停止Play并在脚本修改前建立Task/Change Record/Ledger/handoff；下一步只通过现有`LF2ObjectRenderer.LogicObject`绑定修复selection解析，再做compile/self-check/Play Mode异常清零。gameplay/CentralOnly/C++ authority均未授权改动。
- **2026-08-23 R8-PLAY-001代码状态（覆盖上条执行状态）**：`CODE_WRITTEN / EDITOR-DIAGNOSTIC-ONLY`。selected gizmo路径已改为先向父级、再向子级读取现有`LF2ObjectRenderer.LogicObject as LF2Entity`，不再把纯C#对象当Unity Component；非selected world snapshot、碰撞盒数学、gameplay和CentralOnly均未改。尚未取得fresh compile/self-check/Play Mode证据。
- **2026-08-23 R8-PLAY-001最终结果（覆盖上条状态）**：`VERIFIED / EDITOR-DIAGNOSTIC-ONLY`。force scripts reload后Console 0 error/warning，07:40:57 fresh self-check PASS；清空Console进入`NTSD_Battle`等待15秒后原`GetComponent<LF2Entity>`异常与全部error/warning均为0。hierarchy确认两个active实体及各自EntityModel renderer binding；validator 57 Records/56 governed files PASS。此结果只关闭gizmo诊断污染，不验证input或CentralOnly可见性。用户随后明确报告实时按钮组合无法释放技能；`D-INP-006 / R3-PHY-01`此前仍是UNKNOWN，现进入R8-WP01B独立first-difference诊断，不得把旧单序列注入成功当作当前物理输入证书。
- **2026-08-23 R8-WP01B输入现状**：`R8-INP-01 / PLANNED / DIAGNOSIS-FIRST`。asset静态确认Player_1为W/S/A/D/J/K/L；C++每game tick按current held生成prev/new-edge。Unity source确认local provider只在tick submission采held、direct callback packet随后被丢弃，dedicated worker single-flight又要求publication/presentation ack后才采下一tick；现有test只覆盖按住跨采样点，不覆盖低帧/in-flight多边沿或真实组合。以上是source风险和coverage gap，不是已证明根因。下一步必须记录InputAction→FrameInputSet→roster→Runtime key/cd/combo/frame的first difference；未闭合前不改技能、DAT或组合窗口。
- **2026-08-23 R8-INP-01 neutral runtime checkpoint**：transition期间的tick0瞬时读数已由`get_editor_state.is_changing=true`证明无效；transition完成后fresh Play为tick681、object8、Roster两名human正确绑定slot0/1、paused=false、dedicated worker active/no failure、LastAppliedFrameInput含player0/1 neutral，CentralOnly UsesCentralPixels=true且Console 0 error。故稳定bootstrap/roster/global pause已排除。非改项目的自动物理按键注入受Codex桌面隔离阻断；MCP execute_code又因Roslyn未安装/CodeDom命令过长不可用。按总计划边界，下一步需用户确认后先建`R8-INP-001A` diagnostic/test-only Change Record，不得直接改production input或技能。
- **2026-08-23 R8-INP-01 first difference（覆盖上条下一步）**：继续只读authority后已排除J/K/L crossed mapping差异：C++ `DEFAULT_P1`本身按internal field order用L/J/K。确定差异为`D-INP-010 / R3-COMBO-001 PLANNED`：C++九combo字段按引用即时持久化；Unity resolver复制local并在`comboDja!=3`、valid/failed DJA、guard、Unk328等绝大多数return前不写回。现有`CheckComboLocalShadowCommitContracts`与`CheckStaggeredNarutoDefendDownJumpInput`还明确把该缺陷写成green oracle。Task/Research/Change Record/Ledger/Decision/handoff均已在脚本修改前建立；尚未改脚本。按当前目标边界等待用户明确实施确认；不得先改physical input、worker、DAT或Naruto专项。
- **2026-08-23 R3-COMBO-001阻塞状态（覆盖上条package状态）**：`BLOCKED / B-R3-COMBO-001-01`。source first difference、最小resolver/test范围、rollback和验收矩阵已完整闭合，validator 58 Records/56 governed files PASS；尚无本包脚本diff。当前顶层目标要求R3+包记录后等待用户确认，连续目标续跑未收到明确批准，继续只读也无新证据价值。恢复条件：用户明确批准实施`R3-COMBO-001`；恢复后直接改resolver并修正stale oracle，不重做定位。
- **2026-08-23 R3-COMBO-001实施恢复（覆盖上条package状态）**：`IN_PROGRESS`。用户已明确回复“同意修改，继续处理”，`B-R3-COMBO-001-01`解除。继续严格按既有Task/Change Record实施resolver九combo字段by-ref即时持久化和source-conflicting测试修正；physical mapping、FrameInputSet、worker、DAT、Naruto专项、opoint与render均不在本包。
- **2026-08-23 R3-COMBO-001代码状态（覆盖上条package状态）**：`CODE_WRITTEN`。resolver现直接`ref input.Combo*`执行八方向与DJA wrapper；self-check已改为C++ source-derived early-return状态和Naruto物理L→S→K跨tick正向触发。尚未编译/运行；不得把此状态表述为修复通过。
- **2026-08-23 R3-COMBO-001编译状态（覆盖上条package状态）**：`COMPILE_PASS`。Unity 2022.3.62f3 fresh Tundra 4.72s成功，目标0 error，Assembly-CSharp晚于源码；既存nullable/unused warnings独立。focused/self-check/Play Mode仍待验。
- **2026-08-23 R3-COMBO-001首次自检反馈（覆盖上条package状态）**：`CODE_WRITTEN`。08:08:18 full self-check在OID51 missing-target stale oracle真实FAIL；C++ trigger path明确在frame-jump调用后清零DJA。missing/valid target两条旧transactional-discard断言已改为private/runtime 0，需重新编译与复跑。
- **2026-08-23 R3-COMBO-001二次编译（覆盖上条package状态）**：`COMPILE_PASS`。OID51两条断言修正后fresh Tundra 2.28s、目标0 error、DLL晚于源码；full self-check复跑与Play Mode仍待。
- **2026-08-23 R3-COMBO-001第二次自检反馈（覆盖上条package状态）**：`CODE_WRITTEN`。08:10:17 full self-check在oid6 guard旧transaction-discard断言FAIL；按C++ wrapper顺序已将guard/release分别改为ordinary0/DJA3与ordinary0/DJA0。需重新编译复跑。
- **2026-08-23 R3-COMBO-001三次编译（覆盖上条package状态）**：`COMPILE_PASS`。oid6 guard/release修正后fresh Tundra 2.47s、目标0 error、DLL晚于源码；full self-check再次复跑待验。
- **2026-08-23 R3-COMBO-001第三次自检反馈（覆盖上条package状态）**：`CODE_WRITTEN`。08:12:09 full self-check在held-right partial旧断言FAIL；实际frame102/combo1/cooldowns0符合C++顺序。同组right/left两条均已改为step1持久化，后续interrupt负向断言保留。需重新编译复跑。
- **2026-08-23 R3-COMBO-001四次编译（覆盖上条package状态）**：`COMPILE_PASS`。right/left partial修正后fresh Tundra 2.06s、目标0 error、DLL晚于源码；full self-check再次复跑待验。
- **2026-08-23 R3-COMBO-001自检通过（覆盖上条package状态）**：`FOCUSED_TEST_PASS`。fresh full `BattleRuntimeSelfCheck`于08:14:02 PASS，覆盖本包跨tick、early branch、guard、missing/valid、same-tick与L→S→K合同；此前三次stale-oracle FAIL均保留在Record。EditMode input regression与真实Play Mode仍待，不能提升为完整对齐。
- **2026-08-23 R3-COMBO-001 EditMode反馈（当前修正）**：job `ab3e2977fee04f888730e1f44464c443`完成47个目标测试，1个FAIL为AI resolver fixture陈旧`ComboDra` expected2、actual3（不是DJA）。测试脚本先纳入Record，再把断言改为source-equivalent 3；需重新编译/复跑。full self-check PASS事实保持，但EditMode回归尚未通过。
- **2026-08-23 R3-COMBO-001 Editor重编译（覆盖当前package状态）**：`COMPILE_PASS`。Editor fixture修正后fresh Tundra 1.49s成功写入Assembly-CSharp-Editor，目标0 error；同一47项input矩阵复跑待验。
- **2026-08-23 R3-COMBO-001 EditMode通过（覆盖当前package状态）**：`FOCUSED_TEST_PASS`。job `135495e273a646539f7b42eca9b8611b`为47/47 PASS、0 failed/skipped，覆盖input pass/store/provider/delayed packet/crossed mapping/warmed allocation回归；08:14:02 full self-check仍为fresh PASS。真实Naruto组合Play Mode待验。
- **2026-08-23 R3-COMBO-001 Play probe前置**：真实`NTSD_Battle` bootstrap已完成并生成两个id2角色；MCP动态CodeDom因引用命令行过长、Roslyn不可用而无法查询纯C#实体。Editor-only显式菜单探针已在修改前加入Record，拟通过真实角色InputBuffer和真实30Hz tick记录L→S→K frame/combo/object-count；尚未写探针代码。
- **2026-08-23 R3-COMBO-001 Play probe代码（覆盖当前package状态）**：`CODE_WRITTEN`。Editor-only显式菜单探针已写，排入真实场景first player的L/S/K语义并记录DDJ/frame/cooldown/object count到Temp JSON；不自动运行、不进入生产pass。尚未编译/运行。
- **2026-08-23 R3-COMBO-001 Play probe编译（覆盖当前package状态）**：`COMPILE_PASS`。probe fresh Tundra 3.62s、目标0 error、Assembly-CSharp-Editor晚于源码；尚未运行真实场景probe。
- **2026-08-23 R3-COMBO-001首个Play probe反馈（覆盖当前package状态）**：`IN_PROGRESS`。08:28:20真实场景probe因直接排SimInputBuffer事件被canonical FrameInputSet边界丢弃而FAIL，tick314–316 cooldown/combos均0；这证明探针绕层，不裁决gameplay。下一版改用Input System Keyboard设备事件走完整生产输入链。
- **2026-08-23 R3-COMBO-001 device probe代码（覆盖当前package状态）**：`CODE_WRITTEN`。probe现按DDJ状态逐步排Input System Keyboard L→S→K，经过action callback与canonical FrameInputSet，并在命中authored hit_Dj后释放。尚未重新编译/运行。
- **2026-08-23 R3-COMBO-001 device probe编译（覆盖当前package状态）**：`COMPILE_PASS`。device-state probe fresh Tundra 1.75s、目标0 error、Assembly-CSharp-Editor晚于源码；真实场景复跑待。
- **2026-08-23 R3-COMBO-001 L/S/K Play通过（覆盖当前package状态）**：`IN_PROGRESS`。08:32:12 real-scene synthetic InputSystem device probe PASS：tick613/614/615为DDJ1/2/3，tick626进入Naruto authored frame271并清零，后续frame272/273/274，objects 8→20。完整callback→FrameInputSet→resolver→skill chain已覆盖；physical L→front→J仍待补跑，用户实体键盘操作仍属最终人工复核。
- **2026-08-23 R3-COMBO-001 forward probe代码（覆盖当前package状态）**：`CODE_WRITTEN`。同一device probe已泛化为按当前朝向L→A/D→J并观察DLA/DRA→authored hit_Fa，使用独立Temp result；尚未重编译/运行。
- **2026-08-23 R3-COMBO-001 forward probe编译（覆盖当前package状态）**：`COMPILE_PASS`。generic forward probe fresh Tundra 1.18s、目标0 error、Assembly-CSharp-Editor晚于源码；真实场景运行待。
- **2026-08-23 R3-COMBO-001 forward Play通过（覆盖当前package状态）**：`RUNTIME_PENDING`。08:35:39 real-scene InputSystem L→D→J PASS：tick496/497/498 DRA1/2/3，tick509进入Naruto authored frame263并清零，后续264→283→284，objects 7→8。DDJ与ordinary direction wrapper均有production-chain Play证据；final self-check/validator/diff待。
- **2026-08-23 R3-COMBO-001关闭（覆盖当前package状态）**：`VERIFIED`。C++ source by-ref合同、Unity compile 0 error、08:37:09 final self-check PASS、input EditMode 47/47、real-scene InputSystem L/S/K→frame271与L/D/J→frame263、validator 58/58、scoped diff均PASS。该结论仅关闭D-INP-010；R1-WP02 full trace仍BLOCKED，用户实体键盘/窗口焦点edge仍属D-INP-006/R8人工复核。
- **2026-08-22 R7只读预检**：已登记`D-PERF-001`。PreInteraction cross-pass proof在death cleanup后缓存neutral结论，但其后collision/held writer可在同slot改变frame/CPoint/link，消费端只检查occupancy/pending epoch，存在confirmed stale-content skip；C++ T14会读取object consume后的当前状态。推荐R7-PERF-001禁用production cross-pass cache、保留T14当点whole-pass proof。R6-PRES-005 fresh自动验收门已关闭，但正式实施前仍须建立独立Task/Change Record，并先完成R6剩余adapter source认证。
- **2026-08-22 R7 late只读预检**：已登记`D-LATE-001`。C++ late state-special同调用支持9995→4000→8000 reload chain，并在最终state9996/attacking1生成4×OID217+1×OID218；Unity transform提前return、没有9996 writer且exact gate错误skip。现有GT-11零RNG/零spawn断言是旧C#结论，后续必须supersede。正式实施必须建立独立world structural Task/Change Record，不能只修改skip gate。
- **2026-08-22 R6-PRES-04保持活跃**：`R6-PRES-04 / RUNTIME_PENDING` no-code adapter certification；CentralOnly fail-closed ownership已通过source/full self-check，真实URP PlayMode/C++ trace仍待。
- **2026-08-22 R6-PRES-003保持活跃**：`R6-PRES-003 / RUNTIME_PENDING` 已修production shadow cache current-DAT identity；fresh compile/full self-check/validator通过，PlayMode/C++ trace仍待。
- **2026-08-22 R6-PRES-002保持活跃**：`R6-PRES-002 / RUNTIME_PENDING` 仍覆盖BuildCommands direct shadow gate的current-DAT修复；R6-PRES-003只补production visibility cache writer，两者均缺PlayMode/C++ trace，不互相替代。
- **2026-08-22 R6-PRES-01结果**：`RUNTIME_PENDING` no-code certification。C++ active slot→stable signed-Z→same-Z slot painter order与Unity CentralOnly slot capture→stable radix/fallback→indexed rank已闭合；per-entity shadow/body/overlay/hit-record及segment顺序保持。command writer job `5561fce764bc4baa8804ae37ca929417`为6/6 PASS；17:49:18 full self-check=`PASS`。没有修改脚本；C++ trace/PlayMode/GPU像素仍待。
- **2026-08-22 R5-LIFE-01B结果**：`RUNTIME_PENDING`。普通PendingFlushDestroy→slot/generation release→old-object finalization已认证为等价adapter；production FirstPresentationTick仅Reset=0；新增`D-LIFE-001`记录oid7/8→51 dormant partner结构差异，因partner<20且正式battle-time allocator从20/50起，暂为`INFERRED safe adapter`。本包没有修改任何production/test脚本。UnityMCP force scripts refresh/compile request完成（无C# diff，DLL保持17:14:38）；focused EditMode job `582b9e9212264d39b4377b72d7e0374d`为19/19 PASS，17:49:18 full self-check=`PASS`。C++ trace、真实Play Mode和R6 visual仍待。
- **2026-08-22 R5-LIFE-001A结果**：`D-SCHED-012 cursor subset / R5-LIFE-001A / RUNTIME_PENDING`。MobileExtended和DesktopExtended-growth的source700→child900 same-pass、source700→child600 next-pass矩阵已通过；production allocator/registry/pass/profile未改，existing lowest-free fixture继续通过。17:10旧程序集PASS已作废；UnityMCP force refresh后fresh Tundra 23.19s、Assembly-CSharp 17:14:38、无error CS，17:15:48 full self-check=`PASS`。
- **2026-08-22 R5-OP-001结果**：`D-OP-001 / R5-OP-001 / RUNTIME_PENDING`。C++ release birth history=0与late spawn order已映射到Unity four initializer/cache adapter；four-type production factory fixture验证birth current=action/history=0、next snapshot history=current。首次16:54 request PASS因Assembly-CSharp仍停在16:05被判定为stale并作废；UnityMCP force refresh后fresh Tundra 23.19s、Assembly-CSharp 17:14:38、无`error CS`，17:15:48 full self-check=`PASS`。C++ trace/real PlayMode待验。
- **R3 代码闭环状态**：R3的可实施纯脚本子包已全部形成最小代码闭环，分别保持 `RUNTIME_PENDING`；D-MOV-005是 current exact route `INFERRED` not reachable，不改代码。R3物理键位 Play Mode、joint scenario与C++ trace仍是独立验收 backlog，但不阻断开始R4 source preflight。`R1-WP02`仍为 **BLOCKED**。
- **R2 验收覆盖审计**：已完成只读审计，见 `RESEARCH/R2-ACCEPTANCE-COVERAGE-AUDIT-20260821.md`。
  现有 self-check 对 empty tick、single-character poll、two-held pass、candidate→CPoint、Z clamp、
  mode2 tail 和 production skill-object 有分散覆盖，但缺一份统一 R2 scheduler joint fixture；
  `R2-VERIFY-01` 仅被建议、尚未授权，也没有创建脚本 Change Record。
- **权威当前队列（2026-08-22，本段优先于下方历史追加记录）**：
- **2026-08-22 当前独立代码包（覆盖下方过期 active 表述）**：`R5-CPT-002 / RUNTIME_PENDING`。
  `R5-CPT-004` 已在 09:27:38 以 full self-check PASS 关闭 code-level phase-owner 前置条件；
  当前只允许在 `BattleCpointWriter.ApplyHeldInjury` 的既有 positive injury branch补 C++
  `weapon.cpp:50-69` 的 valid `Unk344=1/2` global kill/damage stats，并更新
  `BattleRuntimeSelfCheck` 的专用 matrix。C++ global stats 不以 holder 存在为 gate；negative injury、
  already-attacking、invalid index 均不得写。最小 writer及shared lethal + six-case matrix已通过；
  Unity compile error CS=0、full self-check于09:44:35 PASS，final ledger/scoped diff亦PASS。C++ trace / Play Mode仍待；下一步按连续队列
  已完成 `D-CPT-003 / R5-CPT-003` 的 source preflight、Task Contract、Change Record和handoff，状态为
  `RUNTIME_PENDING`；source澄清 active mismatch必须跳过decrease/actions但保留throw tail/dircontrol，
  且throw读取fallback current frame0 geometry。另已登记 `D-CPT-005`：valid decrease-negative escape
  在C++仍可能进入throw tail，而Unity direct return；它不属于当前R5-CPT-003，必须另建合同。Unity compile
  error CS=0、full self-check于09:59:58 PASS，最终ledger/scoped diff待本次文档更新后重跑。现已建立
  `D-CPT-005 / R5-CPT-005 / RUNTIME_PENDING` 的source preflight、Task Contract、Change Record和handoff。
  valid escape现保留hitcount/knockback后以skipActions/fallback-frame继续C++ tail，focused assertions已写；
  Unity 2022.3.62f3 build success、无error CS，request-file full self-check于16:09:37 PASS；C++ trace /
  Play Mode仍待，且不能回改R5-CPT-003 mismatch。`D-CPT-003`、pass order、held/link、
  opoint、input、collision、render、DAT/scene、array capacity、C++ authority、trace及 Play Mode一律排除。
  该包完成后最高只能是 `RUNTIME_PENDING`。
- **2026-08-22 当前执行覆盖说明**：`R5-CPT-001 / PLANNED` 是当前唯一脚本写入包。C++ release
  CPoint relation、decrease escape、aaction/taction/jaction 与 weapon current-frame held-vaction branches
  只写 frame / explicit 字段；Unity `BattleCpointWriter` 的七处对应 callsite额外清
  `Runtime.FrameWaitCounter`。source preflight、Task Contract、Change Record 与最小 existing CPoint
  fixture设计均已建立；允许脚本范围仅为 `BattleCpointWriter` 和 `BattleRuntimeSelfCheck`。
  `D-CPT-002` injury global stats 与新登记的 `D-CPT-003` reciprocal mismatch control flow 必须保持独立，
  未写脚本前不得扩大。R1-WP02 仍只阻塞 C++ full trace，不阻塞本包。
- **2026-08-22 当前执行状态更新（覆盖上条 PLANNED）**：`R5-CPT-001 / CODE_WRITTEN` 已将
  `BattleCpointWriter` 内合同列明的七处 immediate-reset CPoint callsite收窄为已有 raw CPoint writer；
  existing CPoint self-check已改为 FWC sentinel preservation，并补充 missing caught-slot fallback。
  当前只等待 ledger/diff 与 Unity compile/self-check；未改 `D-CPT-002`、`D-CPT-003`、CPoint pass order、
  throw、held/link、opoint、input、collision、render、DAT/scene 或 C++ authority。
- **2026-08-22 当前执行结果（覆盖上条 CODE_WRITTEN）**：`R5-CPT-001 / RUNTIME_PENDING` 已通过
  ledger validator、scoped diff、Unity scripts refresh后的 `error CS=0` 与 full
  `BattleRuntimeSelfCheck`（`Temp/NTSD_BattleRuntimeSelfCheck.result` 于09:08:02为`PASS`）。
  首次自检发现专用CPoint helper会拒绝 C++ 允许的missing positive raw frame133，已在同合同内改用
  raw direct writer而非回退FWC reset。C++ full trace和真实Play Mode未取得；`D-CPT-002`、
  `D-CPT-003`、pass order及其它链路依然独立。
- **2026-08-22 D-CPT-002 预检结论**：global kill/damage stats 的字段映射和条件已由
  `weapon.cpp:50-75`、`entity_collision.cpp:57-61`、Unity `BattleRuntimeState` 3-slot arrays、
  `LF2Entity.Unk344` 与现有 normal-hit writer闭合；但发现 `D-CPT-004`：
  Unity `RunKind1` 在 C++ cpoint pass本不应执行的阶段先调用 `SyncCaughtByCpoint`，later
  `SyncHeldCpoint` 又能再次调用它。action可清attacking，因此存在双伤害/错时点风险。
  所以 `D-CPT-002` 不能直接补 stats；当前开始 `D-CPT-004` 的独立 source preflight，未改脚本。
- **2026-08-22 当前独立代码包**：`R5-CPT-004 / PLANNED` 的 source preflight、Task Contract和
  Change Record已建立。允许范围仅为 `BattleCpointWriter.RunKind1` 的 early
  `SyncCaughtByCpoint` owner transfer及相应 `BattleRuntimeSelfCheck` joint fixture；不能改
  `D-CPT-002` stats、`D-CPT-003` flow或 PreInteraction pass order。 
- **2026-08-22 当前执行状态更新（覆盖上条 PLANNED）**：`R5-CPT-004 / CODE_WRITTEN` 已删除
  `RunKind1` 的唯一 early `SyncCaughtByCpoint` call，且添加 actual `PreInteractionTickAll`
  no-action/action-state9/action-nonstate9 phase fixture。未写 global stats、未改 pass order或其它
  CPoint chain；当前等待 ledger/diff、Unity compile与full self-check。
- **2026-08-22 R5-CPT-004 完成代码级闭环**：`R5-CPT-004 / RUNTIME_PENDING` 已通过
  ledger/scoped diff、Unity `error CS=0` 与full self-check（09:27:38 `PASS`）。现有三个
  `PreInteractionTickAll` case证明 injury owner在current weapon-sync；移除early position后，
  decrease escape的C++ raw-position knockback修正为`-4`并通过。C++ trace / Play Mode未取得；
  `D-CPT-002` stats可现在建立独立合同，`D-CPT-003`仍独立。
  1. `R5-LINK-001 / RUNTIME_PENDING` 已完成当前代码级闭环。它只处理 invalid positive-link 时
     forward holder 字段保持：C++只清`LinkState`，Unity当前还清`TargetSlotIndex`/`HeldWeaponStableId`。Task
     Contract、Change Record和source preflight已经建立；Legacy/DataOriented/shadow expected与focused fixture的
     最小改动已写入，Unity compile `error CS`=0、2026-08-22 07:32:40 full self-check PASS、focused EditMode
     `BattleEcsPositiveLinkValidationPassEditorTests` 8/8 PASS。C++ trace与真实Play Mode仍待，不能写为完整对齐。
  2. `R5-LINK-002 / RUNTIME_PENDING` 已完成当前代码级闭环。它只处理invalid negative-held
     relation时child `HolderStableId`保持：C++两个held pass都只清child `LinkState`，Unity shared invalid branch
     还清`HolderStableId`。two-pass source preflight、Task Contract与Change Record已经建立；single-field writer、
     self-check和focused Editor test已写入，Unity compile `error CS`=0、2026-08-22 07:46:36 full self-check PASS、
     focused EditMode `SimulationQueryAndLinkModuleEditorTests` 2/2 PASS。C++ trace与真实Play Mode仍待，不能写为完整对齐。
   3. `R5-HOLD-001 / RUNTIME_PENDING` 已完成当前代码级闭环。它只处理type2 held throw的
      `FrameDelay`保持：C++两轮pass都先复制holder delay、branch本身不覆盖；Unity generic与real weapon writer
      都在复制后写成1。dual-writer source preflight、Task Contract与Change Record已经建立；two-writer removal与
      generic/real holder-delay fixture已写入，Unity compile `error CS`=0、2026-08-22 08:01:15 full self-check PASS。
      C++ trace与真实Play Mode仍待，不能写为完整对齐。
   4. `R5-HOLD-002 / RUNTIME_PENDING` 已完成当前代码级闭环。C++两轮held throw只在type1/4/6
      写`spawner_slot`，type2没有该写；Unity real weapon shared throw helper却对type2也写
      `SpawnerEntityIndex`。source preflight、Task Contract与Change Record已经建立；type1/4/6 stamp、type2 no-write
      与existing fixture已写入，Unity compile `error CS`=0、2026-08-22 08:25:40 full self-check PASS。C++ trace与真实
      Play Mode仍待，不能写为完整对齐。同一helper的`PickerStableId=holder slot`已另记`D-HOLD-003`，不合并入本包。
   5. `R5-HOLD-003 / RUNTIME_PENDING` 已完成当前代码级闭环。C++ reset、normal pickup和两轮held throw
      都不写`picker_idx`；release-listed frame advance target selection是该字段的合法后续writer。Unity shared throw
      helper却为type1/2/4/6写`PickerStableId=holder slot`。source preflight、Task Contract与Change Record已建立；
      当前已只移除该一处writer并扩展existing held fixture，Unity compile `error CS`=0、2026-08-22 08:39:22 full self-check PASS。
      C++ trace与真实Play Mode仍待，不能写为完整对齐。
   6. `R4-HIT-004 / RUNTIME_PENDING` 不是当前代码写入包，而是仍需在未来关闭 C++ trace / Play Mode 的已写入
      证据包。它只覆盖 normal current-DAT type1/type2/type4/type6 weapon victim 的`HitConfirm2`与
      `RelationTeam`首次写入时点：从 common writer early write 延后至已有 weapon tail。C++ `collision.cpp:559-632`
      source contract、Unity middle-helper read audit、Task Contract和Change Record均已建立；最小 writer 改动与
      four-branch real-hit fixture已通过；UnityMCP refresh后的`error CS`=0，2026-08-22 07:10:20 full self-check=`PASS`。
      首次type2-ground fixture失败（confirm=0/frame=0/flight=100）已定位为oid998命中`data.txt` type5 catalog
      definition；改为02C已用的无catalog-override test OID后通过，未扩大production scope。
   7. `R1-WP02 / BLOCKED` 仅阻塞 C++ full trace 获取；它不阻塞已获 D-009 授权、且已有独立合同的 R4/R5 最小包。
     只有需要扩大到 negative link、CPoint/WeaponSync、held/release、slot/generation、pass ordering 或 C++ authority
     的情况，才停止并建立新的合同。
- **R4-HIT-005 只读结论**：已确认C++按current `char_data->obj_type`分发，Unity shared Character-DAT与
  SpecialAttack的两个target dispatcher仍有CLR shell优先分支；“weapon shell + current type3”已存在于test-only
  adaptation，但正式asset attack-candidate可达性为`UNKNOWN`。因正确修复需通用current-DAT target adapter并跨多个
  dispatcher，当前记为`INFERRED / no gameplay change`，不阻断后续已登记R5最小包。
  CLR weapon shell被shared Character-DAT resolver以non-weapon current DAT分发至`LF2Weapon.Hit`的路径保持
  `UNKNOWN`、独立待处理，严禁合并进本包。计划内常规子包按D-009连续推进；仅真实范围/authority/compile/self-check
  failure才停止。
- **当前执行步骤**：`R4-COL-01 / R4-COL-001`、`R4-COL-02 / R4-COL-002`、`R4-COL-03 / R4-COL-003`、`R4-COL-04A / R4-COL-004A`、`R4-COL-04B / R4-COL-004B`和`R4-COL-05A / R4-COL-005A`均达到 `RUNTIME_PENDING`。05A已把 common writer的kind1/3共同Character gate收窄为kind3，并以 frozen candidate正/负矩阵验证；最终 Unity compile `error CS`=0、2026-08-22 05:05:17 +08:00 full self-check PASS，首次测试 CS0165已留档。`D-COL-005B` 的只读调查已完成：C++ case1=generic grab、pickup=kind2/7、正式input只处理type0，但VDC编码DAT使non-character kind1 asset/key-producer可达性为 `UNKNOWN / no gameplay change`；不阻断主线。`D-HIT-001 / R4-HIT-001` 已完成最小脚本闭环：type3 four-field vital/stat writer位于tail前，lethal focused fixture通过；Unity compile `error CS`=0，full self-check PASS（2026-08-22 05:26:41 +08:00），状态`RUNTIME_PENDING`。`D-HIT-002` 的kind10/11、kind16、weapon-victim和weapon-attacker raw-frame writer已拆为`R4-HIT-02A`～`02D`；`R4-HIT-002A` 已完成two-callsite最小替换与exact/shared focused matrix，Unity compile `error CS`=0、full self-check PASS（2026-08-22 05:43:54 +08:00），状态`RUNTIME_PENDING`。按D-009当前自动进入 `R4-HIT-02B` 的独立合同准备。 
- **当前验证步骤**：`R3-INP-001` 已完成 scripts compile、filtered C# error query 和 request self-check；下一步只能以独立 Record / fixture 关闭 R3 joint input、Play Mode 或可用 C++ trace，不能把现有 PASS 扩大为完整对齐。R2 的 joint fixture 缺口仍记录在 `R2-ACCEPTANCE-COVERAGE-AUDIT`，但不阻断已批准的后续最小包。
- **2026-08-22 连续推进修正（覆盖上方“当前执行步骤”中的过期下一步表述）**：`R4-HIT-002B / PLANNED` 的kind16 source preflight、Task Contract与Change Record已经建立；当前没有脚本写入。下一动作是严格按该合同完成唯一`ApplyKind16` callsite及existing exact/shared fixture的最小改动，而不是停在02A完成处。
- **后续阶段状态**：R2-PASS-01/02 的 joint fixture / Play Mode / trace 验收仍待后续依赖；R3 的 `R3-INP-01`、`R3-INP-02`、`R3-HOLD-INP-01`、`R3-AI-LIFE-01`、`R3-INP-03A`、`R3-INP-04`、`R3-AI-TGT-01`、`R3-FRAME-01A`、`R3-LAND-01`、`R3-SYNC-RESP-01`和`R3-FRAME-02A`均为 `RUNTIME_PENDING`；D-MOV-005为 current exact route `INFERRED` not reachable，R3-PHY-01保持用户Play Mode/asset `UNKNOWN`。R4 已开始，`D-COL-001 / R4-COL-001`、`D-COL-002 / R4-COL-002`、`D-COL-003 / R4-COL-003`、`D-COL-004A / R4-COL-004A`、`D-COL-004B / R4-COL-004B`和`R4-HIT-002A`均为 `RUNTIME_PENDING`；04B的active landing direct-hit已移除，dormant held immediate query仍为 `INFERRED`；D-COL-005B为 `UNKNOWN / no gameplay change`。`D-HIT-002`已有静态writer-family拆分，接下来是`R4-HIT-02B`独立合同；02B～02D、D-HIT-003、R5～R8均未开始。
- **禁止直接开始**：不得启动 C++ instrumentation / 构建 / 配置修改、Unity trace、comparator，或计划外的技能/gameplay / 性能 / Play Mode 扩张。计划内 R2～R8 子包可按 `D-009` 连续推进，但每一包仍须先建立独立 Task Contract 与 Change Record。

## 脚本改动留痕机制

- **机制状态**：已启用；根 `AGENTS.md` 已加入 13.1 规则，`CHANGE-LEDGER.md`、Record 模板和只读 validator 已通过初始自检。
- **最近治理 Change ID**：`OPS-TRACE-001`，状态 `VERIFIED`；范围仅限留痕机制文档与 `Tools/Validate-ChangeLedger.ps1`，未包含 Unity/C++ gameplay。
- **当前活跃 Change ID**：`R4-HIT-002A / RUNTIME_PENDING`。它只覆盖 exact/shared character kind10/11 的 `frame=182` raw-write side effect：两处 resolver已由`ImmediateFrame`换为现有raw writer，self-check已验证PN、attacking、wait、frame-data mirror和既有stats；Unity compile `error CS`=0与05:43:54 full self-check PASS已取得。C++ trace、真实Play Mode和joint frame/presentation仍未关闭。接下来要建立`R4-HIT-02B`的独立Record，不能扩大02A范围。`R4-HIT-001 / RUNTIME_PENDING`已在`BattleDamageWriter.ApplySpecialAttackDamage`的kind0 type3 target route补齐HP、HPBound、ComboCountVic、DamageStats，且位于既有type3 tail之前；未复用type0-only kill/holder-combo score，也未改candidate、weapon、CPoint、held/link、newborn/opoint、scheduler、input、AI或render。`R4-COL-005A / RUNTIME_PENDING`、`R4-COL-004B / RUNTIME_PENDING`、`R4-COL-004A / RUNTIME_PENDING`、`R4-COL-003`、`R4-COL-002`、`R4-COL-001` 与 `R3-FRAME-002A-001`均为已留痕的既有记录。`R3-SYNC-RESP-001 / RUNTIME_PENDING`、`R3-LAND-001 / RUNTIME_PENDING`、`R3-FRAME-001A / RUNTIME_PENDING` 与其余仍为 `RUNTIME_PENDING` 的 `R3-AI-TGT-001`、`R3-INP-004-001`、`R3-INP-003A-001`、`R3-AI-LIFE-001`、`R3-HOLD-INP-001`、`R3-INP-002`、`R3-INP-001`、`R2-SCHED-002`、`R2-SCHED-001`均必须继续留痕。physical binding / C++ trace / joint Play Mode均未关闭，`D-INP-006`、`g_init_stats` / F7与其余R3+均不在既有 Record范围内。
- **2026-08-22 当前活跃 Record 修正（覆盖上方旧 active 表述）**：`R4-HIT-002B / CODE_WRITTEN` 是唯一当前 active Change ID。它已将 `BattleDamageWriter.ApplyKind16` 的implicit `ImmediateFrame(MpDrain)`收窄为raw writer，并扩展existing exact/shared kind16 fixture以验证PN/wait保留与显式`AttackingCounter=0`；当前等待实际Unity compile/self-check。所有其他kind、writer、projection、战斗模块和C++ authority均不在范围内。
- **2026-08-22 02B 最终状态（覆盖上条 active 表述）**：`R4-HIT-002B / RUNTIME_PENDING` 已取得Unity compile `error CS`=0与05:58:02 full self-check `PASS`；actual/shared fixture验证frame/Data mirror=200、PN=71、wait=17、explicit attacking=0和既有vital/stat/vrest/link/held结果。C++ trace与真实Play Mode仍未关闭。按D-009，下一个动作是建立`R4-HIT-02C`的独立source contract/Change Record；不要把02B结论扩展到weapon writer。
- **2026-08-22 02C 当前状态（覆盖上条下一步表述）**：`R4-HIT-002C / PLANNED` 的C++ weapon-victim raw-frame source preflight、Task Contract与Change Record已经建立，尚未改脚本。当前只允许在`ApplyKind0WeaponVictimTail`与focused fixture中处理type1/type4/type6/type2的PN/attacking/wait/RNG合同。
- **2026-08-22 02C 执行中修正（覆盖上条状态）**：`R4-HIT-002C / IN_PROGRESS` 已静态确认同一canonical `ApplyWeaponDamage` 对damageable weapon固定执行raw knockdown（180/186）后再进入weapon-tail raw final-frame；本包只会将该knockdown一处与tail四处的implicit helper替换为raw writer。focused fixture设计覆盖type1、type4、type6、type2-ground、type2-air，锁定PN、attacking、wait和总RNG call count；尚未改脚本或运行Unity验证。
- **2026-08-22 02C 代码已写（覆盖上条状态）**：`R4-HIT-002C / CODE_WRITTEN` 已将canonical `ApplyWeaponDamage`的knockdown一处及`ApplyKind0WeaponVictimTail`的tail四处替换为`DirectWriteRawFramePreserveWaitCounter`，保留C++所需的raw 180/186→raw final-frame顺序。新增五分支真实`LF2Weapon.Hit`夹具，覆盖type1、type4、type6、type2-ground、type2-air并锁定frame/Data、PN、attacking、wait、HitConfirm2、relation、自身vrest与RNG总数；尚未运行Unity compile/self-check，不能写为已对齐。
- **2026-08-22 02C 当前状态（覆盖上条状态）**：`R4-HIT-002C / RUNTIME_PENDING` 已通过UnityMCP script refresh后的compile `error CS`=0与full `BattleRuntimeSelfCheck`（结果文件2026-08-22 06:20:15 +08:00为`PASS`）。五分支real-hit fixture通过；Console仅保留既有rest-binding negative control，不存在C# compiler error或02C fixture失败。C++ trace仍BLOCKED、Play Mode未做，故不得扩大为完整weapon/R4或C++ runtime已对齐。
- **2026-08-22 02D 当前状态**：`R4-HIT-002D / PLANNED` 已只读闭合normal weapon-attacker source contract：state3000必须在generic victim knockdown前处理current-DAT oid209 skipReset，state1002在later位置处理；当前Unity helper顺序相反且漏skip，且两处`ImmediateFrame`均有raw-frame外副作用。独立Task Contract/Change Record/预检已建立，尚未改脚本、未运行Unity验证。
- **2026-08-22 02D 执行中修正（覆盖上条状态）**：`R4-HIT-002D / IN_PROGRESS` 的五类真实`LF2Weapon.Hit`夹具设计已闭合：state1002、state3000 normal、state3000→frame10 state1002 order witness、oid209 Karasu skip、oid209/frame40 skip。下一步仅在Record列明的两份脚本内实现局部writer拆分，仍未改脚本。
- **2026-08-22 02D 代码已写（覆盖上条状态）**：`R4-HIT-002D / CODE_WRITTEN` 已在`ApplyWeaponDamage`内把state3000移动到generic victim knockdown前，并从原attacker-response中拆出state1002的later raw writer。state3000实现只在non-character weapon victim下使用C++ oid209 skipReset，raw10后保留显式attacking/Vx/Vz；state1002 raw random16后保留Vx/Vy/type4 knockback且不清attacking。五类真实`LF2Weapon.Hit`fixture已写，尚未运行Unity compile/self-check，不能写为已对齐。
- **2026-08-22 02D 当前状态（覆盖上条状态）**：`R4-HIT-002D / RUNTIME_PENDING` 已通过UnityMCP script refresh后的compile `error CS`=0与full `BattleRuntimeSelfCheck`（结果文件2026-08-22 06:36:40 +08:00为`PASS`）。五类real-hit fixture通过；Console仅保留两个既有rest-binding negative control，不存在C# compiler error或02D fixture失败。C++ trace仍BLOCKED、Play Mode未做，故不得扩大为完整weapon/R4或C++ runtime已对齐。
- **2026-08-22 R4-HIT-003 当前状态**：`R4-HIT-003 / PLANNED` 已只读闭合normal weapon vital/stat source contract：type1/2/4 full hurt使用FallDamageDiv-adjusted vital/stat后才写raw durability，type6 reaction只写raw durability。type0-only kill/holder score必须排除；early HitConfirm2/RelationTeam另记`D-HIT-004`。独立Task Contract/Change Record/预检已建立，尚未改脚本、未运行Unity验证。
- **2026-08-22 R4-HIT-003 执行中修正（覆盖上条状态）**：`R4-HIT-003 / IN_PROGRESS` 的real-hit fixture设计已闭合：type1/2/4 scaled nonlethal、type2 lethal with holder、type4 bdefend100、type6 reaction control。下一步只改Record列明的weapon writer/local helper与self-check，不碰D-HIT-004或其他模块。
- **2026-08-22 R4-HIT-003 代码已写（覆盖上条状态）**：`R4-HIT-003 / CODE_WRITTEN` 已在`ApplyWeaponDamage`中按C++相对顺序写damage-effect → type1/2/4 scaled vital/stat → raw durability，并新增专用helper排除type0 kill/holder score；type6仍不进vital helper。真实nonlethal/lethal/bdefend100/type6 fixture已写，尚未运行Unity compile/self-check，不能写为已对齐。
- **2026-08-22 R4-HIT-003 当前状态（覆盖上条状态）**：`R4-HIT-003 / RUNTIME_PENDING` 已通过UnityMCP script refresh后的compile `error CS`=0与full `BattleRuntimeSelfCheck`（结果文件2026-08-22 06:50:08 +08:00为`PASS`）。real-hit vital/durability fixture通过；全量Console仅保留两个既有rest-binding negative control。一次filter MCP socket重连错误为tool transport，不是Unity错误。C++ trace仍BLOCKED、Play Mode未做，故不得扩大为完整weapon/R4或C++ runtime已对齐。
- **约束**：从本账本激活后，任何自编写脚本改动都必须先建立 Change Record，再修改代码；修改后必须同步 Ledger、STATE、handoff 和真实验证证据。
- **Git 边界**：未安装或启用 Git hook，未修改 `.git/config`、`.git/hooks` 或 GitHub Desktop 工作流。未来是否启用 pre-commit hook 由用户单独决定。

## VERIFIED

- 唯一行为权威为 `J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 C++ live battle runtime；入口为 `src/entity/game_tick.cpp` 的 `game_tick(...)`。该目录、入口和 `Makefile` 均已在本机读取；Makefile 列入 game tick、frame advance、physics、collision collect/collision、hit、weapon、cpoint、input 与 renderer 模块。
- Unity 实现入口 `SimulationTickDriver`、`NTSDBattleTickSystem`、`SimulationWorld`、`FrameInputSet` 与 `BattleRuntimeSelfCheck` 均在当前工作树中存在。
- 根 `AGENTS.md`、主要对齐 ledger、交接 ledger、中央渲染计划和统一架构计划均已具有或已补齐“C++ release live path 为最终 authority”的治理口径。
- R1-WP01 规划阶段没有修改 `Assets/NTSD/Scripts/` 下的 gameplay、C++ runtime、测试实现、DAT 或资源；
  后续 R2-SCHED-001 的两处 Unity 脚本修改及其证据见本文件第 36 条和对应 Change Record，二者不得混淆。
- 已完成 R1-WP01 的 C++ checkpoint 合同、Unity source-pass 静态 crosswalk、三方 trace schema、固定 fixture/input journal 合同、first-difference 输出格式和后续 R1 工作包拆分，详见 `docs/ai/TASKS/R1-WP01-trace-contract-planning.md`。
- 已静态确认 C++ `game_tick(...)` 的主要边界，以及 Unity `NTSDBattleTickSystem` / `SimulationWorld.Passes.partial.cs` 中的当前 pass 与 CPoint/WeaponSync 调度位置；这是源代码定位事实，不是运行时对齐证据。
- 已静态确认 `Tools/NTSDParity` 的 README、AuthorityTraceCommand、TraceCompareCommand、Authority400 manifest，以及 Unity `BattleParitySnapshot` / `BattleParityTraceEditor` 以 C# / Unity 历史 parity 为前提。它们只能作为格式、夹具、回归或诊断材料。
- 用户已明确 D-006：C++ Release runtime 在 R1 中不可修改；所有 C++ trace 只能通过既有外部只读观察通道获取，采集结果和比较资料必须写在非 authority 目录。
- 已静态确认 Unity 交付边界的实现事实：`BattleRuntimeProfilePolicy` 定义 `Authority400=400`、`MobileExtended=1050 slot / 1000 active`、`DesktopExtended` 的 page-normalized 初始容量（默认 512）和 `int.MaxValue` active 合同；`SimulationWorld.Registry.partial.cs` 只允许 `DesktopExtended` 动态扩容。
- 已静态确认中央表现边界：`CentralOnly` 会抑制 Legacy materializer、以中央已发布 frame/command 为显示来源，并跳过不会贡献中央 command 的逐实体 renderer shell；Legacy `SpriteRenderer` 容量 guard 明确只是临时兼容限制。上述是 Unity 实现边界事实，不是 C++ 行为对齐证书。
- 已修订重新对齐总计划：R1 的必经主线现在是“C++ source behavior contract → Unity source-pass crosswalk → 差异清单 → 子流程验收矩阵”；R1-WP02 的只读 full trace 保持 BLOCKED，但不再被表述为源码盘点的启动门槛。
- R1-SOURCE-001 已完成静态 main-tick contract 与 Unity pass crosswalk：C++ T00–T18、Unity 30 段调度、D-SCHED-001～012 已写入 research 文档。D-SCHED-001～003 是已确认的静态顺序差异；其运行时行为影响仍待后续 R1 模块和联合验收，不得写成已修复或已验证。
- R2-SCHED-001 已只修改 `NTSDBattleTickSystem` 和 `BattleRuntimeSelfCheck`：Unity 调度静态顺序已核验为 first clamp → held#1 → snapshot/pair/candidate → character/random/object consume → candidate cleanup → CPoint/WeaponSync → positive link → second clamp → held#2。通过现有 Unity Editor 的 UnityMCP `refresh_unity(force/scripts/compile)` 完成 domain reload，更新后的 `Assembly-CSharp.dll` 时间戳为 2026-08-21 22:10:02；UnityMCP Console 返回 0 error，之后 `BattleRuntimeSelfCheck` request result 于 22:12:49 返回 PASS。以上是编译和 focused self-check 事实，不是 C++ runtime / joint fixture / Play Mode 对齐证书。
- R1-SOURCE-002 已完成 C++ post-cooldown callback、human/AI input、combo/direct action、F1/F2 gate 与 Unity packet/input-pass crosswalk。`D-SCHED-005` 已由 `R3-INP-001`、`D-SCHED-010` 已由 `R3-INP-002`、`D-INP-001` 已由 `R3-HOLD-INP-001`、`D-INP-002` 已由 `R3-AI-LIFE-001` 写入各自最小 Unity adapter；四者均通过对应 local static、UnityMCP compile（filtered `error CS`=0）与 request self-check。它们仍没有 R3 joint fixture、C++ runtime trace 或 Play Mode 验收，严禁写成已对齐。`D-INP-003`～`006` 仍未处理。
- R1-SOURCE-003 已完成 C++ F00–F09 的 frame/physics/movement/lifecycle crosswalk：state400/401/500/501、frame advance、character/non-character physics、state12/13/18 与武器 landing、state9998、respawn、Z clamp、late frame tick 都有 source mapping。D-MOV-001～005 已登记；均是静态差异或可达性待验，未做 runtime 验收。详见 `R1-SOURCE-003-unity-crosswalk-and-diff.md` 与对应 handoff。
- R1-SOURCE-004 已完成 C++ candidate collect、collision/hit consume、grab/weapon interaction 的静态 source contract 与 Unity crosswalk。D-COL-001～005、D-HIT-001～003 已登记；CPoint、held/link、opoint/lifecycle producer/consumer 的未闭合部分明确移交 R1-SOURCE-005。全部仍为静态结论，未做 runtime 验收。
- R1-SOURCE-005 已完成两轮 negative held、CPoint / weapon sync、positive/negative link、normal late opoint、slot/newborn / free-reset 生命周期的静态 source contract 与 Unity crosswalk。D-SCHED-004、D-LINK-001～002、D-HOLD-001～002、D-CPT-001～002、D-OP-001 已登记；TrackerFlag/TrackerParent 的 C++ auxiliary-field mapping 与 structural lifecycle joint fixture 仍为 UNKNOWN。全部仍未做 runtime 验收。
- R1-SOURCE-006 已完成 C++ release render callback、renderer active/Z painter order、shadow/body/spark side effect、camera/perspective display contract，与 Unity RenderDispatch、BattlePresentation、CentralOnly、Texture2DArray/dynamic Mesh/URP command path 的静态 crosswalk。D-RENDER-001～005 与 A-RENDER-001～004 已登记；中央渲染、1.5× visual scale、fixed-world logic camera 与扩展容量均为保护边界，不构成回退 Legacy 的授权。全部仍未做 runtime/visual 验收。
- R1-SOURCE-007 已完成 COV-001～006 的全量静态收口：所有 D-/A-条目均进入唯一总登记册，UNKNOWN 有最小补证路径，producer->consumer 依赖、future repair batches 与分层验收矩阵均已写入。此结论是“静态盘点完成”，不是 runtime/Play Mode/trace 验收完成。
- 已建立 `R1-SOURCE-ALL-DIFF-REGISTER.md` 作为跨 Work Package 的全量差异总索引；当前收录 D-SCHED、D-INP、D-MOV、D-COL、D-HIT、D-LINK、D-HOLD、D-CPT、D-OP、D-RENDER 和 A-RENDER 条目。它已静态收口；其 UNKNOWN 和待测试项不允许被当前条目数掩盖或伪造为“已对齐”。
- 已建立 `R1-SOURCE-INVENTORY-COVERAGE-MATRIX.md`，并为 R1-SOURCE-005（CPoint / held /
  link / opoint / lifecycle）、R1-SOURCE-006（render handoff）和 R1-SOURCE-007（汇总 /
  依赖图 / 验收矩阵）建立独立 Task Contract。001～007 均已完成静态 source 审计；任何
  gameplay 仍未对齐或验收。
- 已静态确认 `J:\QQFile\NTSD2.4\ntsd_release\ntsd_new.exe` 存在，长度为 957,072 bytes，SHA-256 为 `9F2C56875F6ADC786C159D3483ABD596191D22405F46812D1A3CD286B5E92C5D`，最后写入时间为 2026-06-12 14:38:01。
- 已静态确认现有 source/binary 可见线索：release binary 包含 `NTSD_DEBUG_TICK`、`NTSD_RNG_SEED`、`NTSD_BATTLE_P1_OID`、`NTSD_BATTLE_P2_OID`、`NTSD_BATTLE_STAGE` 和 `diag_auto_result.txt` 字符串；`main.cpp` 的 release 分支静态调用 `bootstrap_direct_battle`，而 `game_tick.cpp` 的 `NTSD_DEBUG_TICK` 路径向 stderr 输出有限 phase/position 信息。
- 已静态确认 `main(int /*argc*/, char* /*argv*/[])` 不消费命令行参数；运行循环使用 SDL keyboard state 与 Windows `GetAsyncKeyState`，不是现成的逐 tick input journal/replay 接口。
- 已静态确认已检查的 source 诊断路径包含相对文件名的 append 写入，例如 `diag_auto_result.txt`；C++ authority 根目录当前已有该诊断文件。未启动 executable，因此本 WP 没有向 C++ authority 目录写入任何内容。

## INFERRED

- 现有 C#/Unity self-check、Authority400 diagnostic trace、fast-path proof 与 1000 AI/0 GC 数据，仍适合作为后续回归、性能或诊断输入；在取得同场景 C++ release trace 前，它们不构成 C++ 行为对齐证书。
- 现有的 C# 基线资料可帮助 R1 对齐命名、夹具和 Unity 对应 pass，但不能缩短 C++ live-path 调用链核验。
- Unity 当前在 `PreInteractionTickAll` 中于 candidate collect 前运行 CPoint/WeaponSync，而 C++ `game_tick(...)` 将其静态放在 object collision 之后；这只是静态时序风险，尚未由同 fixture 的 C++ runtime trace 证明为行为 mismatch。
- Unity fallback / optimized 的现有诊断开关可能可用于 R1 producer，但其完整性、独立性和与 worker 路径的关系尚未验证。
- release binary 中存在的 debug/environment 字符串提示可能有部分外部观察能力，但 source/executable 完整 build identity、运行时开关副作用、输出覆盖范围和外部工作目录兼容性均未运行验证；不能把字符串存在当成可用 trace 的 VERIFIED 结论。

## UNKNOWN / 未完成验证

- 尚未建立 C++/Unity 可比较的同 schema tick trace；尚未取得 first-difference witness。
- 尚未逐条用 C++ release live 调用链审计历史 C# 结论；R0 仅完成证据治理，R1-WP01 仅完成 trace 合同规划。
- R0/R1-WP01 自身没有执行 Unity 编译、`BattleRuntimeSelfCheck`、Play Mode、C++ release 构建或 C++ runtime trace；后续 R2-SCHED-001/002 已各自取得 Unity scripts compile 与 focused self-check PASS，证据见相应 Change Record，但 Play Mode、C++ release 构建与 C++ runtime trace 仍未执行。
- 项目记录版本为 Unity `2022.3.62f3`，而旧测试示例路径使用 `2022.3.4f1c1`；R1 或任何测试任务开始前需确认实际 Editor 可执行路径。
- C++ Release 现有的只读 trace/日志/进程观察通道、其可观测字段、非 authority 输出重定向、可重复输入方式、RNG call-count 与 fixture bootstrap 尚未闭合。
- C++ / Unity 的 DAT 语义 mapping、stage no-data 合同、initial-state digest、Unity fallback/optimized producer 切换边界、C++ camera/perspective 的最终可比较性均尚未闭合。
- 未找到已经文档化且证明可从未修改 `ntsd_new.exe` 输出 R1 full schema（tick/pass/slot/field/candidate/consume/lifecycle/render handoff）的只读采集通道。
- 未确认在不修改 C++、不向 authority 目录写入的前提下，是否能以非 authority working directory 启动 runtime 并保留资源加载；现有 source 有相对诊断写入，不能擅自尝试。
- 未确认可以将每个逻辑 tick 的 held/pressed/released 输入以可重复、可验证方式送入 release runtime；现有入口是实时 SDL/物理键盘轮询。
- `ntsd_new.exe` 的最后写入时间早于 `src/core/main.cpp`；没有找到把当前 source tree / Makefile 与该 executable 精确绑定的 build manifest，因此 source-to-executable identity 未闭合。

## 当前代码与工作树状态

- 当前 Git 分支：`NTSD_2_4_C++`。
- R0 新增/修改仅限工作流与文档治理文件，详见 `docs/ai/HANDOFFS/HANDOFF-R0-bootstrap-authority-migration.md`。
- R1-WP01 仅新增 `docs/ai/TASKS/R1-WP01-trace-contract-planning.md`，并更新本状态、决策与 R1 handoff；未触碰任何 gameplay 或 C++ 文件。
- R1-WP02 只读准备新增 `docs/ai/HANDOFFS/HANDOFF-R1-WP02-readonly-trace-preparation.md`，并只更新 R1 文档/状态/决策；未启动或修改 C++ Release runtime。
- 本次计划修订只更新重新对齐总计划、本状态和决策记录；未触碰任何 Unity/C++ gameplay、测试实现、DAT、场景或资源。
- R1-WP01 开始时工作树已包含与本任务无关的场景、项目设置、资源 meta、文档修改及未跟踪 `.claude/`、`docs/` 内容；本 Work Package 未回退、移动或清理它们。

## 阻塞与下一步

- **2026-08-23 R8-WP01C规划（最新状态）**：`R8-WP01C-production-combat-object-certification.md`
  已建立，状态为 `PLANNED / APPROVAL PENDING / CERTIFICATION-ONLY`。宽泛的对象交互/生命周期
  Play Mode范围已拆为01 opoint/newborn/basic lifecycle、02 pickup/held/throw/landing、03 grab/CPoint/link、
  04 collision/hit/damage、05 death/respawn、06 random weapon/late special/effect、07 synthesis。
  本次仅修改Task/STATE/handoff文档，未修改脚本、场景、资源或C++ authority，也未运行Unity。
  首个可执行包为`R8-WP01C-01`，必须等待用户明确批准；若执行中需要probe/test脚本，必须先建立
  独立Change Record。R1-WP02 full trace继续BLOCKED，R8-WP01D/E/F/G均未开始。
- **2026-08-23 R8-WP01C-01恢复（覆盖上条审批等待）**：用户已明确回复“批准执行 R8-WP01C-01，
  恢复目标”。只读preflight确认既有W05仅为EditMode结构证据，不能提供live `NTSD_Battle` S4；因此
  `R8-OPLIFE-001 / IN_PROGRESS` 已建立，允许新增唯一Editor-only显式Play探针，目标为type0/1/3/5
  production opoint、出生frame/Prev2、slot/generation、high/low scan cursor、release/reuse及cleanup。
  production gameplay/factory/pool/pass/DAT/scene均不在范围。当前未发现运行中的Unity Editor；这只是
  compile/Play运行前置，尚不是BLOCKED或gameplay失败。
- **2026-08-23 R8-OPLIFE-001代码已写（覆盖上条状态）**：Record现为`CODE_WRITTEN`。已新增
  `BattleOpointLifecyclePlayModeProbeEditor.cs`及meta；它只在显式菜单触发，暂停live driver并在worker
  idle边界使用正式catalog/factory/structural writer/slot/pool，记录四类birth、release/generation reuse及
  high/low scan cursor，finally清理probe-owned对象并恢复pause。普通dotnet build为0 error但当前旧Unity
  csproj尚未收录新文件，因此不能作为probe编译证据；fresh Unity compile与Play尚未运行。
- **2026-08-23 R8-OPLIFE-001编译通过（覆盖上条状态）**：UnityMCP socket 6401执行force-all
  refresh后导入新脚本；`Assembly-CSharp-Editor.dll`于09:01:25更新且晚于源码，Editor.log为Tundra
  success/domain reload，直接Console error查询为0。Record提升到`COMPILE_PASS`；active scene确认为
  `NTSD_Battle`，Play probe/self-check/final validator仍待。
- **2026-08-23 R8-OPLIFE-001运行证据（覆盖上条状态）**：Record现为`FOCUSED_TEST_PASS`。
  live production Play result于09:05:09 PASS：worker active，OID33/120/203/999为正确type/CLR、birth
  frame/runtime/Prev2=0，同一slot53 generation=1/3/5/7且release拒绝old handle；high 52→53在tick357
  same-pass执行，low 53→52在tick358保持attacking0并于tick359变1；cleanup后object6/claimed4/
  render-pool2/logic-pool4全部恢复。W05 focused job `3b8e08105d0946bca58d88e5ed6ef990`
  8/8 PASS，09:06:51 full self-check PASS，Play后Console 0 error/warning。final validator/diff待运行；
  C++ full trace和extended>399 real Play仍未关闭。
- **2026-08-23 R8-WP01C-01最终状态（覆盖上条）**：`R8-OPLIFE-001 / VERIFIED`，只裁决01的
  Unity S4。final ledger validator PASS（59 records / 59 governed code files），scoped diff check PASS。
  persistent evidence为`R8-WP01C-01-opoint-lifecycle-runtime-evidence-20260823.md`。WP01C整体仍
  `IN_PROGRESS`；下一独立包是02 pickup/held/throw/landing，状态`APPROVAL PENDING`。不得把01扩大为
  full C++ trace、extended>399 Play、整个R8或完整战斗对齐。
- **2026-08-23 R8-WP01C-02启动（覆盖上条的 approval 状态）**：用户已明确批准并恢复目标。
  `R8-HOLDPLAY-001 / PLANNED` 已建立，唯一允许脚本范围为新增 Editor-only
  `BattleHeldWeaponLifecyclePlayModeProbeEditor.cs`，用于 live pickup→held/wpoint→type1/2/4/6
  throw→landing/no-immediate-hit S4 认证；发现 production first-difference 时必须登记 repair WP并停止，
  不得在认证包顺手修 gameplay。
- **2026-08-23 技能图片用户观察**：新增 `D-RENDER-006 / USER-REPORTED / REPRODUCTION_PENDING`。
  现有 R6 自动证据没有认证真实 DAT 的技能 pic→sheet/slice/UV 内容；该项归尚未开始的 `R8-WP01D`，
  不是 WP01C-01/02 已通过后的遗漏修复。未取得具体角色/技能/tick/frame/pic 重现前，根因保持 UNKNOWN，
  不修改 render。
- **2026-08-23 R8-HOLDPLAY-001代码已写（覆盖上条代码状态）**：Record现为`CODE_WRITTEN`。
  新增Editor-only explicit Play probe及meta；它在live worker idle/driver paused边界，用data.txt真实OID
  120/150/121/122的type身份和确定性wpoint/frame夹具调用production pickup、held/throw、landing writer，
  并加入overlap target no-immediate-hit及best-effort cleanup。尚无fresh Unity compile、Play、focused或
  self-check证据，不得报告02通过。
- **2026-08-23 R8-HOLDPLAY-001首次Play失败**：tick623的type1 pickup在探针入口被拒绝；定位为探针误用
  只服务shared Character-DAT shell的静态resolver，而非真实`LF2Character` production resolver。cleanup完整恢复
  object9/claimed7/render-pool2/logic-pool7。探针已最小改用真实角色resolver并补地面态前置；production未改，
  fresh compile/重跑待执行。
- **2026-08-23 R8-HOLDPLAY-001第二次Play失败**：type1/type2整链已通过，type4反弹的attacking失败
  源于探针sentinel写在`ImmediateFrame`之前，并非landing writer差异；已移到frame初始化之后。同时该次
  触发时仍为tick0/empty world/worker inactive，故不构成有效S4。探针现先等待tick>0且world/claimed非0，
  再暂停并采基线；production未改，fresh compile/第三次Play待执行。
- **2026-08-23 R8-WP01C-02最终状态（覆盖上条）**：`R8-HOLDPLAY-001 / VERIFIED`，只裁决02的
  Unity S4。final Play 09:37:31在tick1/worker active下通过OID120/150/121/122四type的pickup、held
  wpoint、throw、landing与overlap no-immediate-hit；cleanup恢复object4/claimed2/render2/logic2。
  source 09:36:23 < Editor DLL 09:36:40，focused job `36440d545fe64659ae3c73ff1febf03c`
  23/23，09:38:54 full self-check PASS，清空预期负向日志后Console 0 error/warning，validator 60/60与
  diff PASS。WP01C整体仍IN_PROGRESS；03为APPROVAL PENDING。C++ S5、手动具体武器流程和
  D-RENDER-006/WP01D仍未关闭。
- **2026-08-23 R8-WP01D启动 / D-RENDER-006 first-difference**：用户明确批准并要求不以具体
  角色、技能或OID特判。C++ release `game_tick.cpp:352-383`确认state8000 writer是`unk_318=140`，
  `renderer.cpp:581-624`先raw pic999隐藏再做pic+offset；Unity `LF2Entity.ApplyStateDataTransform`
  错写`HitStop=140`，`GetRenderPicIndex`还会先对999加offset，既有self-check错误保护HitStop合同。
  `R8-WP01D-01 / R8-SPRITEMAP-001 / CODE_WRITTEN`已建立；通用writer/raw-hidden和陈旧oracle已最小
  写入，没有新增角色/技能/OID分支，validator PASS，fresh Unity compile/self-check待执行。23个可读DAT range/BMP
  静态矩阵grid mappingDiff=0，故row/col尚不是当前已证明根因；all-loaded-DAT catalog/slice/UV/Play仍待。
- **2026-08-23 R8-SPRITEMAP-001首次自动验证**：fresh `Assembly-CSharp.dll` 10:09:51且Console C# error=0；
  10:11:07 full self-check在另一个既有GT-10“authority HitStop=140”陈旧断言FAIL。全文件检查又找到
  GT-11 chain/missing-target两处同源错误oracle，已统一改为`HitStop=0 + RenderPicOffset=140`并保留其余
  结构断言。当前状态仍`CODE_WRITTEN`，必须重新fresh compile/self-check；首次FAIL已留档，不能报通过。
- **2026-08-23 R8-SPRITEMAP-001自动收口（覆盖上条当前状态）**：source 10:12:16 < fresh
  `Assembly-CSharp.dll` 10:12:32，Console C# error=0；10:13:22 full self-check PASS。自检内两条既有
  negative registry fixture error读取后已清空，最终Console error/warning=0。Record现为`RUNTIME_PENDING`：
  通用字段/raw-hidden已取得source/compile/self-check，但all-loaded-DAT catalog/slice/UV、CentralOnly live
  command、Game/Scene像素与C++ full trace仍未关闭，不能把D-RENDER-006写成VERIFIED。
- **2026-08-23 R8-WP01D-02启动**：为避免单角色/技能样本掩盖通用差异，已建立
  `R8-SPRITEMAP-002 / PLANNED`。唯一允许脚本是新增Editor-only Play probe：枚举全部loaded DAT/frame、
  C++ expected range/rect、catalog entry、central binding/slice/page/UV，并动态从实际state8000 writer选择
  target生成live CentralOnly command；不得硬编码候选OID。production与GPU pixel修复不在本包，当前未改脚本。
- **2026-08-23 R8-SPRITEMAP-002代码写入（覆盖上条代码状态）**：Record现为`CODE_WRITTEN`。
  新增probe/meta已实现全loaded-DAT/catalog/binding矩阵、动态actual-state8000 target、live snapshot/entity
  command/logical key及cleanup JSON；候选由数据排序动态选择，没有固定角色/技能/OID。production脚本0改动；
  validator 62 Records / 61 governed files PASS，fresh Unity compile/Play尚未执行。
- **2026-08-23 R8-SPRITEMAP-002编译（覆盖上条当前状态）**：Record现为`COMPILE_PASS`。Unity
  2022.3.62f3 force scripts reload完成且Editor恢复idle，Console C# error=0；Play全DAT/catalog/binding/
  state8000 command与cleanup尚未执行，不能写成focused/runtime通过。
- **2026-08-23 R8-SPRITEMAP-002菜单传输修正（覆盖上条当前状态）**：Record退回`CODE_WRITTEN`。
  MCP 9.6.9将中文菜单路径传输为乱码，首次`ExecuteMenuItem`未进入探针；这是Editor自动化入口问题，
  不是渲染审计失败。probe新增调用同一入口的ASCII菜单别名，无production改动；必须重新fresh compile。
- **2026-08-23 R8-SPRITEMAP-002首次真实编译失败（覆盖上条验证状态）**：确认主Editor为PID36240/
  port6401，6400/6402为Unity worker；full asset refresh后新script进入Editor csproj，并暴露line558
  CS1061：`BattleSpriteEntry`无`MatchesCommand`。probe现改为按现有公开descriptor合同逐字段比较；
  production未改，Record仍`CODE_WRITTEN`，待重编译。
- **2026-08-23 R8-SPRITEMAP-002编译收口（覆盖上条当前状态）**：Record现为`COMPILE_PASS`。
  修正后source 02:29:06早于`Assembly-CSharp-Editor.dll` 02:29:21，主Editor port6401 idle且Console
  error=0；下一步执行真实Play全DAT audit，尚不能写成runtime通过。
- **2026-08-23 D-RENDER-006第二个通用first difference**：`R8-SPRITEMAP-002`首次真实Play已枚举
  100 loaded definitions、4373 catalog entries、232 ranges与6674 authored frames，累计1301 differences；
  首批均为`CPP_SOURCE_DESCRIPTOR_MISMATCH`，呈横向换行后持续错一格。只读C++确认parser原样保存
  `row`，loading把`sr.row`作为`SpriteSheet.cols`，renderer按`localPic % row`/`localPic / row`取图；
  Unity `ResolveEffectiveGrid`却按BMP尺寸猜测并默认把`col`当横向列。已建立
  `R8-WP01D-03 / R8-SPRITEMAP-003 / PLANNED`，只允许通用row-horizontal repair与同源test修正。
  首次probe cleanup 0->4/0->2是tick0 baseline采集过早，另在002修正；没有角色/技能/OID分支。
- **2026-08-23 R8-SPRITEMAP-003代码写入（覆盖上条003状态）**：003现为`CODE_WRITTEN`。
  production resolver已删除物理尺寸交换heuristic，固定将DAT row映射为horizontal columns、col映射为
  vertical rows；self-check加入非对称row3/col2换行边界并修正旧grid oracle。002 probe同时把baseline移到
  battle-ready/worker-idle，并拆分state8000候选缺失阶段。未增加角色/技能/OID/frame/resource特判；
  尚未fresh compile/self-check/Play复跑。
- **2026-08-23 R8-SPRITEMAP-003首次self-check失败**：fresh compile 0 error后full self-check在
  `CheckSpriteFileRangeParsingContracts`失败，flash fixture仍按`col`建synthetic texture width、按`row`建
  height，因此合法catalog key `(214,0)`成为hole。该fixture现改为C++ row-horizontal尺寸；parser/range/
  overlap职责及production未扩大。两条rest registry error仍为既有负向控制；待重新编译/self-check。
- **2026-08-23 R8-SPRITEMAP-003自动验证收口（覆盖上条当前状态）**：flash fixture修正后fresh
  compile 0 error，10:41:17 full self-check PASS；两条rest registry error是既有负向夹具。003现为
  `FOCUSED_TEST_PASS`，002 baseline/state8000分型也fresh compile通过。下一步必须clean Play重载全部
  production DAT/BMP并复跑4373-entry矩阵，不能仅凭self-check写成runtime通过。
- **2026-08-23 R8-WP01D第二次全DAT Play**：修复后catalog 4933，首次1301条
  `CPP_SOURCE_DESCRIPTOR_MISMATCH`已清零，cleanup从4/2恢复到4/2。剩余229条全为range超出实际网格的
  `VISIBLE_FRAME_CATALOG_ENTRY_MISSING`；样本均是C++ source rect完全落在BMP外，Unity hole与C++无可见
  像素等价。002 probe已改为只有rect仍与source sheet相交才报missing，并统计fully-outside数量；
  state8000为0 authored source，明确SKIPPED而非PASS。002退回`CODE_WRITTEN`待第三次Play；003暂不升级。
- **2026-08-23 R8-SPRITEMAP-002 fully-outside探针首次编译失败**：line424/429因局部变量名与
  同一for scope后续变量冲突产生两条CS0136；已只重命名诊断局部变量，production与判断公式未改。
  Play实际未进入，待重新fresh compile。
- **2026-08-23 R8-WP01D第三次全DAT Play**：probe编译修正后实际运行，60个fully-outside已正确
  排除，剩余169个missing引用仍与纹理边缘相交。只读原BMP检查表明实际C++ rect交集全为黑色
  colorkey；summon的绿色像素只在格间separator列、没有进入任何rect。002现新增全路径通用BMP交集
  像素检查与短生命周期cache：仅非黑交集继续报差异，禁止文件名/OID特判；待重新compile/Play。
- **2026-08-23 D-RENDER-006第三个通用first difference**：第四次全DAT Play在通用BMP交集像素
  过滤后仅余2个非黑可见missing entry，cleanup PASS。C++只以declared range判localPic合法，`row`只作
  横向cols，不用`row*col`限帧，并由blit裁剪partial source；Unity仍以row*col分配且partial为hole。
  003因4933-entry source descriptor mismatch=0升级`RUNTIME_PENDING`；已建立
  `R8-WP01D-04 / R8-SPRITEMAP-004 / PLANNED`，使用range长度、source intersection与通用adjusted pivot；
  证据中的ID/frame/path不得进入实现分支。
- **2026-08-23 R8-SPRITEMAP-004代码写入（覆盖上条004状态）**：004现为`CODE_WRITTEN`。
  rect builder按declared range分配、与source bounds求intersection，并以完整frame锚点和clip offset计算
  adjusted pivot；prewarm Sprite与catalog显式pivot共用合同。self-check锁定range>row*col、79×4 partial与
  negative pivot；002 probe同步expected clipped rect/pivot。Mesh/Shader/gameplay未改，无角色/技能/OID/
  frame/file特判；尚未compile/self-check/Play。
- **2026-08-23 R8-SPRITEMAP-004首次self-check失败**：fresh compile 0 error；P2 partial组合断言仍把
  weapon6/weapon3的2px下一行/42px右列交集当hole，期望count40/7而新C++合同为50/8。已只修
  同源test为精确clipped rect与后续hole；production未改，首次FAIL保留，待重编译/self-check。
- **2026-08-23 R8-SPRITEMAP-004自动与Play收口（覆盖上条004当前状态）**：fresh compile 0 error，
  10:59:06 full self-check PASS；final all-DAT Play枚举100 definitions、232 ranges、6674 frames、
  5537 catalog entries和23 clipped引用，source/path/rect/pivot/binding differences=0，cleanup 4/2→4/2。
  loaded data无authored state8000 source，live witness明确`SKIPPED_NO_AUTHORED_SOURCE`。002与004均升级
  `RUNTIME_PENDING`；未把catalog PASS冒充GPU/Game/Scene或C++ full trace。
- **2026-08-23 R8-WP01D-05启动 / 旧P8-C生产夹具边界**：focused resolver/atlas/mesh job
  `608b9f8515a646fb97ecd2a5c36c4707` 29/29 PASS。P8-C Play GPU矩阵中synthetic legacy-central pixels、
  Texture2DArray UV、透明排序、4097 chunk与missing-resource均PASS；production case因旧harness强制要求
  opoint实体拥有逐对象`LF2ObjectRenderer`而FAIL，和当前批准的logic-only + Central snapshot架构冲突，
  不能裁决production像素。已建立`R8-SPRITEMAP-005 / IN_PROGRESS`，只新增Editor probe，对全部catalog
  source→central binding实际像素及统一GPU command做通用验证；不得恢复Legacy owner或加入专项分支。
- **2026-08-23 R8-SPRITEMAP-005最终状态（覆盖上条005当前状态）**：`VERIFIED`，仅裁决当前
  logic-only CentralOnly架构下的全catalog GPU/binding像素与dynamic Mesh witness。final Play读取232张
  source textures、30个Texture2DArray slices，5537/5537 entries与84,327,319 pixels全部匹配，
  source/central hash均`8ECA0CBA6D4724D1`且同域重复一致，0 differences。final动态可见partial为
  450×5、pivot(0.5,-28)，Legacy/Central 340/340 pixels、mean/max=0/0，cleanup 4/2→4/2。首次全透明partial与第二次负pivot
  视口中心导致witness为0像素，均只修Editor probe且保留失败事实；production代码0改动，无专项分支。
  final editor DLL 11:34:48且compiler error=0，focused job `ecaf8255752e4515bbcc76787c61aba3`
  35/35，11:37:22
  full self-check PASS。D-RENDER-006整体仍`RUNTIME_PENDING`：真实Game/Scene最终可见性/挂点/层级、
  当前loaded data无authored state8000 witness及C++ full trace尚未关闭。
- **2026-08-23 R8-WP01D-06启动 / Game空实体画面**：fresh Game screenshot
  `Temp/R8-WP01D-06/R8-WP01D-06-game.png`显示HUD与背景正常，但无战斗实体；Scene截图因当前
  180×936窄viewport只作环境证据。由于descriptor和84,327,319 GPU binding pixels均0差异，首差范围
  转到正式`snapshot→command→resolver→segment/chunk→URP submission→camera`。已建立
  `R8-SPRITEMAP-006 / IN_PROGRESS`，只新增Editor live diagnostic枚举全部claimed slots，不改production、
  scene/URP asset或任何角色/技能/OID/frame/file分支。
- **2026-08-23 R8-SPRITEMAP-006最终状态（覆盖上条006当前状态）**：`VERIFIED`，仅裁决Editor
  diagnostic与当前Game submission证据。第一次tick1采样的`NO_SNAPSHOT_ENTITIES`在扩展worker/pending/
  immutable-plan字段并延后采样后被证明为过早采样；final tick257报告覆盖3个claimed slots，形成3个
  snapshot、6个source/resolved commands、1 chunk、1 segment和1 draw，plan simulation/display tick均257、
  非stale、无refusal/worker failure，cleanup恢复。fresh Game截图已实际显示角色、武器和阴影；没有
  production first difference，故未修改worker、central renderer、URP、scene或camera。source 11:47:36 <
  Editor DLL 11:47:58，compiler error 0，11:53:16 full self-check PASS。D-RENDER-006整体仍为
  `RUNTIME_PENDING`：当前180×936 Scene View不能裁决logic-only中央实体可观察性，loaded data无authored
  state8000 live witness，C++ full trace继续BLOCKED。
- **2026-08-23 R8-WP01D-07启动**：只读确认production `CanRenderCamera`明确允许Play Mode Base
  `CameraType.SceneView`，focused materialization test也证明最新world publication完成后SceneView可取得
  current lease；当前空Scene截图更可能是viewport/观察坐标问题，不构成renderer差异。已建立
  `R8-SPRITEMAP-007 / PLANNED`，只允许新增Editor-only probe，用真实SceneView camera对齐world camera、
  cullingMask=0与透明RT隔离中央像素，记录gate/lease/pixels/cleanup。未修改production、scene、URP、
  DAT/BMP或C++，若probe发现首差必须另立repair Record。
- **2026-08-23 R8-SPRITEMAP-007代码已写（覆盖上条007状态）**：Record现为`CODE_WRITTEN`。
  Editor-only probe等待current plan与worker idle，记录真实SceneView gate/current lease，暂存camera状态后将
  投影对齐world camera，在960宽白色隔离RT、cullingMask=0下执行实际SceneView render，输出isolated PNG、
  non-clear pixels与hash并恢复camera/driver/world；白底确保黑色阴影不会被黑底证据漏计。production、scene、URP、DAT/BMP、
  Legacy owner与C++均0改动；尚未compile/Play/self-check。
- **2026-08-23 R8-SPRITEMAP-007最终状态（覆盖上条007当前状态）**：`VERIFIED`，仅裁决Play Mode
  SceneView camera/central pixel S4。source 12:03:53 < Editor DLL 12:04:04，compiler error 0；focused job
  `9dfeda6b0663429a9caf20df64048fb9` 13/13。clean Play真实`SceneCamera/CameraType.SceneView`的production
  gate与current lease均true，tick2/generation3 current plan含4 source/resolved commands、1 segment；960×540
  白底isolated render得到575 non-clear pixels、hash `C292967D753744C2`。objects 4→4、claimed 2→2，
  camera/driver恢复，Play Console 0 error，12:05:47 full self-check PASS。首次黑底522像素证据因黑色阴影
  不可观察而由白底final supersede；两次均无production改动。先前空Scene截图是窄viewport和logic-only
  Transform观察方式不足，不是renderer首差。D-RENDER-006现只剩loaded data无authored state8000 live
  witness与C++ full trace两项证据缺口。
- **2026-08-23 R8-WP01C-03获批并启动**：用户明确回复`批准执行 R8-WP01C-03，恢复目标`。
  只读复核确认`R5-CPT-001～005`与`R5-LINK-001～002`的production修复均已写并有source/compile/
  self-check证据，当前缺口是live production world joint S4，没有发现新的静态差异。已建立
  `R8-GRABPLAY-001 / PLANNED`，只允许新增Editor-only Play probe，覆盖valid grab+held injury/global stats、
  reciprocal mismatch throw、negative escape+dircontrol、positive/negative link residue及first-held→
  PreInteraction→positive-link→second-held逐pass表。production、DAT/scene、render与C++禁止修改；发现首差
  必须另拆repair。
- **2026-08-23 R8-GRABPLAY-001代码已写（覆盖上条03状态）**：Record现为`CODE_WRITTEN`。
  Editor-only probe在paused live production world使用正式grab writer和first-held→PreInteraction→positive-link→
  second-held pass，覆盖lethal held injury/global stats/FWC/position、reciprocal mismatch fallback throw、
  negative-duration escape+dircontrol+FramePostProcess、positive/negative residue，并恢复global stats、实体、池与
  pause。production gameplay/scheduler、DAT/scene、render与C++均0改动；尚未compile/Play/focused/self-check。
- **2026-08-23 R8-GRABPLAY-001首次Play**：fresh compile 0、positive-link 8/8、negative-link 2/2；
  clean Play全部行为断言PASS，valid grab四pass、mismatch throw、escape dircontrol和link residue均符合合同，
  cleanup/global stats恢复。发现仅Editor报告取样时点不严谨：postprocess后Knockback已清0，后续negative-held
  又把positive target link从-5清0。已只在probe中提前保存两组观察值，production 0改动；首次PASS保留，
  final evidence必须重新compile/clean Play。
- **2026-08-23 R8-GRABPLAY-001最终状态（覆盖上条03当前状态）**：`VERIFIED`，仅裁决WP01C-03
  Unity production Play S4。final source 12:22:01 < Editor DLL 12:22:18，C# error0；positive-link job
  `2e1446b473a64aef81ca80fd9b69d30d` 8/8、negative-link job
  `aa8d155711ac4ee5a9fc48862bf2fe42` 2/2。clean Play在worker active下tick16→17：valid kind3 grab、
  first-held无damage、PreInteraction唯一lethal injury/stat/position、后续无重复；mismatch fallback throw、
  negative escape+dircontrol+postprocess和正负link residue全部PASS。objects4→4、claimed2→2、pools2→2、
  global stats恢复、Console error0；12:23:59 full self-check PASS。production0改动。C++ full trace继续BLOCKED，
  WP01C-04～07未由03关闭。
- **2026-08-23 R8-WP01C-04获批并启动**：用户明确回复`批准执行 R8-WP01C-04，恢复目标`。
  只读复核确认C++ Release顺序为snapshot/collect→type0 consume→random-weapon boundary→type>0 consume，
  Unity `R4-COL-001～003/005A`与`R4-HIT-001～004`已有production修复和compile/self-check证据，当前
  缺口是同一live production world的character/weapon/special、candidate order、caught/effect21/HitConfirm2
  gate/abort、vital/stat/durability/vrest联合S4。已建立`R8-HITPLAY-001 / IN_PROGRESS`；只允许新增
  Editor-only通用Play probe和治理/证据文档，production、DAT/scene、render与C++禁止修改。发现首差必须
  单独登记repair并停止，不得在认证探针中顺手修复。
- **2026-08-23 R8-HITPLAY-001代码已写（覆盖上条04当前状态）**：Record为`CODE_WRITTEN`。
  Editor-only probe已编码10个frozen candidate，覆盖三类正向命中、HitConfirm2/caught/effect21、kind10 raw
  frame和character→random-weapon→object pass边界，并备份/恢复RNG、stats、sounds、baseline pair rests、
  hit-plan mode、实体/池/pause。production gameplay/scheduler、DAT/scene/render与C++均0改动；尚未compile/
  focused/Play/self-check，不得报告04通过。
- **2026-08-23 R8-HITPLAY-001首次Play（probe-only失败）**：fresh compile 0、hit focused 178/178、
  W06 11/11、role-aware 9/9已通过。首次Play在进入任何gameplay pass前因探针尝试在非reset boundary切
  `ShadowCompare`而由production正确拒绝；实体/池/stats/RNG/sounds/rest均恢复，失败不构成行为首差。
  已只修probe为观察启动时既有hit-plan mode，不再改变mode；需重新compile/clean Play，当前仍非完成。
- **2026-08-23 R8-HITPLAY-001第二次Play（probe-only失败）**：完整passes已运行，所有此前behavior断言
  通过；最后报告读取`DamageStats/KillStats[3]`时因live数组只有0～2而越界。cleanup全恢复，未形成
  gameplay first-difference。probe改为special与character共用合法槽1，按pass验证+10→累计+20且kill
  只增加一次；需第三次compile/clean Play，production仍0改动。
- **2026-08-23 R8-HITPLAY-001最终状态（覆盖上条04当前状态）**：`VERIFIED`，仅裁决WP01C-04
  Unity production Play S4。final compile0；hit focused178/178、W06 11/11、role-aware9/9。clean Play冻结
  10个candidate：character5→-5、weapon100→80/durability100→90、special100→90；HitConfirm2/effect21
  整attacker abort、caught first-only skip、kind10 raw frame182和character→random no-op→object边界均PASS。
  objects4→4、claimed2→2、pools2→2，RNG/stats/sounds/baseline rests/mode/pause恢复，Console0 error；
  13:19:39 self-check与validator PASS。production0改动。当前hit-plan mode=Disabled、worker inactive，故不声称
  本轮ShadowCompare/worker-active或C++ full trace。05～07未由04关闭。
- **2026-08-23 R8-WP01C连续授权（覆盖05 approval pending）**：用户明确要求“直接推进WP01C剩余的
  三项即可，不需要我批准”。因此05→06→07按固定依赖顺序连续执行，不再逐包等待批准；每包仍须独立
  Task/Change Record、fresh compile、focused、Play/self-check与治理。production first-difference、gameplay
  repair、scene/资源/架构变更和任何C++ authority运行/构建/写入仍是停止条件。当前进入05。
- **2026-08-23 R8-WP01C-05启动**：只读source preflight确认AI-before-cleanup、state14 hit-stop、
  no-count/stored-count/free、integer average/RNG与OID998字段合同已闭合；现有Unity production映射未发现
  新静态首差。已建立`R8-DEATHPLAY-001 / PLANNED`，只允许Editor-only live probe和治理/证据文档；
  production、DAT/scene、render与C++均禁止修改。首差必须独立登记并停止。
- **2026-08-23 R8-DEATHPLAY-001代码已写**：Record为`CODE_WRITTEN`。Editor-only probe覆盖HP=0 AI
  input、state14 arm/decrement、no-count stale integer+RNG、stored-count OID998、free和relation/link writer边界，
  并恢复RNG/sounds/entity/slot/pool/pause。production0改动；尚未compile/Play/focused/self-check。
- **2026-08-23 R8-DEATHPLAY-001最终状态（覆盖上条05状态）**：`VERIFIED`，只裁决WP01C-05 Unity
  production Play S4。fresh all-scope compile0；AI85/85，W05 exact1/1与isolated8/8。clean Play完成HP=0
  AI、state14 0→30→4、no-count/stored/free；stale integer平均(130,30)，两次RNG后expected/actual(147,39)；
  stored OID998/action6 slot50字段正确。objects4→4、claimed2→2、pools2→2，RNG/sounds/pause恢复，Console0；
  13:52:04 self-check与validator PASS，production0改动。组合focused一次W05B静态污染失败已由isolated PASS
  复核且未改production。worker inactive、C++ full trace仍BLOCKED；06按连续授权直接进入。
- **2026-08-23 R8-WP01C-06启动**：source preflight闭合natural random、9995→4000→8000→9996、
  4×217+1×218、lowest-slot/RNG/exhaustion；R7旧HitStun140口径已确认由R8 RenderPicOffset140纠正。
  已建立`R8-LATEPLAY-001 / PLANNED`，只允许Editor-only Play probe和治理证据；production/C++ 0改动。
- **2026-08-23 R8-LATEPLAY-001代码已写**：Record为`CODE_WRITTEN`。probe覆盖live catalog natural random、
  live 9996五子、logic-only full chain与authority400 exhaustion；cleanup恢复live baseline，production0改动。
  尚未compile/focused/Play/self-check。
- **2026-08-23 R8-LATEPLAY-001最终状态（覆盖上条06状态）**：`VERIFIED`，只裁决WP01C-06 Unity
  production Play S4。compile0、focused14/14；worker-active final Play中natural 9 candidates选OID122 slot50、
  position(1314,-500,509)、8 RNG；live 9996生成4×217+1×218、34 RNG；synthetic full chain到OID901/
  state9996/offset140；authority400满350动态槽时natural1/late0 RNG、0 spawn。objects/claimed/pools、
  RNG/sounds/pause恢复，Console0，14:08:15 self-check和validator PASS，production0改动。C++ trace BLOCKED；
  按连续授权进入07 synthesis。
- **2026-08-23 R8-WP01C-07 / WP01C最终状态**：`COMPLETE / 01～06 VERIFIED（Unity S4） / 07
  SYNTHESIS COMPLETE`。六包production producer→consumer矩阵、cleanup、fresh compile/focused/full self-check/
  ledger均有持久证据，认证probe对production gameplay改动0。最终汇总见`R8-WP01C-07-synthesis.md`。
  D-COL-004、D-COL-005B、D-HIT未覆盖分支/D-HIT-005、D-LIFE-001、C++ full trace及WP01D/E/F/G仍独立；
  不得把WP01C完成扩大为R8或完整战斗逻辑已对齐。
- **B-R8-WP01C-06-TEARDOWN-01（非阻断战斗S4）**：WP06 probe结束前active object/slot/pools与RNG/sounds
  均恢复、Console0；退出Play后两个AutoCreated manager触发scene cleanup warning。无probe对照Play→Stop为
  0 error，定位为probe新增inactive renderer的Editor teardown hygiene；未修改production/反射pool内部。
- **2026-08-23 R8-WP01D最终边界**：`COMPLETE AT AVAILABLE EVIDENCE / FULL CLOSURE BLOCKED`。
  01～07已取得state8000/row/range修复、5537 catalog、84,327,319 GPU pixels、Game和SceneView限定范围S4；
  `B-R8-WP01D-08-01`为loaded DAT无authored state8000，`B-R8-WP01D-08-02`为R1-WP02 full trace。
  不改DAT、不绕过C++只读；D-RENDER-006保持MAX AVAILABLE S4。WP01E/F/G可继续，当前进入WP01E。
- **2026-08-23 R8-WP01E启动**：已建立`R8-WP01E-current-build-capacity-performance-certification.md`，
  状态`PLANNED / CERTIFICATION-ONLY / NO SCRIPT CHANGE`。现有harness具备1000 production GameObject、
  MobileExtended/capacity、0 B/Gen collection、central draw/pixel、hash与teardown指标；历史报告不作fresh证据。
  顺序固定为fresh工具/容量基线→Dispersed/Combat短样本validity gate→两组各1800 tick正式60秒门→
  DesktopExtended合同复核。任一首差停止认证并另建修复包，本包内不顺手改脚本。
- **2026-08-23 R8-WP01E E-01/E-02结果**：E-01 fresh compile0、focused job
  `2dda595036944c708bfd11f32204ba1e` 290/290、14:25:44 self-check PASS。E-02首次Combat1000在
  0 sampled tick/0 stress entity/无report时失败：processor于Bootstrap初始加载窗口把driver/world partial
  footprint+missing lazy pool误判为managed runtime invalid，两次Play均在ready前重启并fail-closed。该结果
  是harness lifecycle first failure，不是性能/GC/capacity/gameplay结论。WP01E现`BLOCKED AT E-02`；
  `R8-WP01E-R01 / R8-PERFBOOT-001 / PLANNED / APPROVAL PENDING`已登记，只允许修正restart decision
  事实输入及pure-policy tests，不改pool/Bootstrap/gameplay/C++。
- **2026-08-23 R8-PERFBOOT-001获批**：用户明确回复`批准执行 R8-WP01E-R01 / R8-PERFBOOT-001，
  恢复目标`。Record现`IN_PROGRESS`；只允许修改`ProductionEntityStressWindow.cs`的restart decision caller
  和`ProductionEntityStressEditorTests.cs`的pure-policy matrix，不改pool/Bootstrap/gameplay/C++。
- **2026-08-23 R8-PERFBOOT-001代码已写**：Record为`CODE_WRITTEN`。restart policy现在以Bootstrap
  ready而非driver/world partial footprint裁决“服务应已完整”，并允许clean restart后的新Play同样等待初始
  Bootstrap；ready-invalid、previously-healthy invalid与一次retry fail-closed保持。7分支pure-policy matrix
  已写；尚未compile/focused/Play复跑。
- **2026-08-23 R8-PERFBOOT-001代码级验证**：fresh Editor DLL 14:33:57、Console error0；focused job
  `2bcc822ceddb45f9955a3041a3ade51f` 263/263 PASS；14:35:20 full self-check PASS。Record现
  `FOCUSED_TEST_PASS`，等待完全相同Combat1000 capacity-pressure smoke的Play复跑，尚未VERIFIED。
- **2026-08-23 R8-PERFBOOT-001最终状态**：`VERIFIED`。同一Combat1000请求现不再premature restart，
  1000 active、30 warmup、180 sampled完整执行；logic Avg/P95 21.199/23.797ms、0 B/0 collection、capacity
  critical0、central 1 draw/179 pixel frames、hash非空、teardown全恢复。pool/Bootstrap/scene/gameplay/C++
  0改动。WP01E E-02 Combat短样本通过，但visible frame Avg/P95 38.949/39.025ms、frame GC平均7128.94B
  仍是正式60秒门风险；下一步Dispersed短样本，不得宣称30FPS完成。
- **2026-08-23 R8-WP01E E-02短矩阵**：Combat与Dispersed均1000 production objects、180/180 sampled、
  logic0B/0 collection、capacity critical0、central1 draw/SetPass4、hash与teardown PASS。Combat logic
  Avg/P95=21.199/23.797ms，Dispersed=21.432/24.771ms；但visible frame分别38.949/39.025ms与
  38.309/44.265ms，均未达到正式30FPS门。E-02只关闭validity，现进入E-03两组1800-tick completed-frame
  timing正式报告，不先猜优化模块。
- **2026-08-23 R8-WP01E最终状态**：`VERIFIED / UNITY EDITOR CURRENT BUILD`。两组120+1800正式门：
  Dispersed logic/visible/main P95=18.575/25.525/25.286ms；Combat=19.044/33.058/26.901ms；Render/GPU
  P95均低于4ms。两组logic0B、Gen0/1/2=0、capacity critical0、central1 draw/SetPass约4、hash与teardown
  PASS。Desktop/Mobile/pool/slot/generation focused job`f554c03eb363475b808fe622d350c9a3` 299/299；
  fresh Legacy/Data同180-tick的input/RNG/metadata/world/slots/aRest/vRest/stats/events/overall/workload/roster
  12项hash全部相同；14:51:50 final self-check PASS。Editor frame recorder仍有非战斗Editor-side allocations但
  60秒内0 collection且P99<30KB；Player hard gate归WP01F。下一包WP01F Windows Mono/IL2CPP，需先建合同。
- **2026-08-23 R8-WP01F规划**：`R8-WP01F-windows-mono-il2cpp-player-certification.md`已建立，状态
  `PLANNED / APPROVAL PENDING`。国际版2022.3.62f3的WindowsStandaloneSupport/IL2CPP模块存在，当前
  Standalone默认IL2CPP；现有build tool仅旧U9 Mono。`R8-PLAYERBUILD-001 / PLANNED`只允许在该Editor
  build tool提取共享helper、新增Mono/IL2CPP独立输出并保持ProjectSettings/Burst finally恢复；批准前不改脚本、
  不build/run Player。
- **2026-08-23 R8-PLAYERBUILD-001获批**：用户明确回复`批准执行 R8-WP01F / R8-PLAYERBUILD-001，
  恢复目标`。Record现`IN_PROGRESS`；只允许修改现有Editor build tool的双backend入口/独立Temp输出与
  共享finally恢复，不改runtime/scene/ProjectSettings默认值/C++。
- **2026-08-23 R8-PLAYERBUILD-001代码已写**：Record现`CODE_WRITTEN`。现有Editor build tool已新增R8
  Mono/IL2CPP菜单、共享Windows build helper、backend独立Temp输出和明确BuildReport日志，并保留旧U9
  Mono菜单兼容别名；未改runtime/scene/ProjectSettings默认值/C++。fresh compile、两后端build/run和hash
  比较均仍待执行，不能写为认证完成。
- **2026-08-23 R8-PLAYERBUILD-001编译通过**：Record现`COMPILE_PASS`。UnityMCP force refresh后Console
  0 error，scoped diff-check与Change Ledger validator通过；构建前ProjectSettings/Burst hash已登记。Mono与
  IL2CPP均尚未build/run，WP01F仍未认证。
- **2026-08-23 R8-WP01F Mono中间结果**：Mono BuildReport Succeeded；可见D3D11 Player正式运行exit0，
  1000实体、30+180 tick、MobileExtended/DataOrientedCanonical、Player hard四边界0 B、0 collection、
  capacity0、CentralOnly draw1/pixels179与teardown restored均通过。隐藏窗口负控制因SRP不提交按预期exit3，
  不作为正式结果。IL2CPP未build/run，因此WP01F仍在进行中。
- **2026-08-23 R8-WP01F用户停止决定**：用户明确指示`IL2CPP Player 不会有任何问题，不要做相关处理`。
  当前不再构建、运行、诊断、修复或认证IL2CPP，不把Codex沙箱进程结果作为gameplay差异/blocker，也不据此
  修改Unity脚本或配置。`R8-PLAYERBUILD-001`现`ABANDONED`，已写helper与Temp artifacts保持原样；WP01F
  不标VERIFIED，工作返回C++ Release→Unity C#战斗逻辑主线。
- **2026-08-23 R8-WP01G综合完成**：68个D-ID已按20/20/19/7/2无遗漏分类，集合校验register=68、
  synthesis=68、missing0、extra0；scoped diff-check和Change Ledger validator（73 records/73 governed
  code files）PASS。本包未改Unity脚本、scene或config。当前没有可直接修改的未关闭source-confirmed代码
  差异；下一推荐是`R8-WP01G-R01`只读闭合R2的`D-SCHED-006/008`，R3+运行证据/修复包仍须用户批准。
- **2026-08-23 R8-WP01G-R01完成（更正WP01G初次综合）**：`D-SCHED-006`已闭合为
  `SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING`；两次current-character-DAT
  Z clamp的时点、slot顺序、double/int写入一致，高槽为批准adapter。`D-SCHED-008`已确认
  `SOURCE-CONFIRMED CONDITIONAL DIFFERENCE / UNFIXED`：normal completed tick无consume-end→tail reader，
  但F1/step-wait render后early return跳过tail时，C++保留candidate carrier到下一tick继续append，Unity
  已EndConsumption且下一collect无条件reset，可能改变count/20-cap/order/RNG/consume。当前分类更正为
  A20/B20/C20/D5/E2/F1=68。已建立`R2-CANDIDATE-TAIL-01`预实施Task；本只读包未改/运行Unity或C++。
- **2026-08-23 R2-CANDIDATE-TAIL-01只读实施预检**：精确修复不能只移动End或只清count；StoreOnly/
  LegacyOracle切换、fallback restart、attacker generation、target-slot current occupant与20条ordered entries要求
  一个query-owned fixed-slab retention store，在prebattle capacity阶段预分配，pause时capture、next collect按
  current producer mode seed后继续append、真实tail后清理。该方案跨scheduler/query/store/tail/test，当前状态
  `PLANNED / APPROVAL PENDING`；尚未创建Change Record、未修改脚本。
- **2026-08-23 R8-WP01G-R01B / D-STEP-001 source closure**：`D-STEP-001`从UNKNOWN更新为
  `SOURCE-CONFIRMED DIFFERENCE / POLICY DECISION REQUIRED / UNFIXED`。C++ release main在BATTLE outer
  frame按A→B→C down-edge写process-global flag1/progress3；flag1时F1 wait仍skip完整input callback，但不再
  render后return，会继续postprocess/late/tail。Unity无flag/progress/deterministic command producer，scheduler
  恒走flag0分支。当前68项分类更正为A20/B20/C20/D4/E2/F2。已建立`R3-STEP-01` policy Task，因属于
  R3+ schema/input/architecture选择而停在用户policy批准边界；本包脚本0改动。
- **D-STEP对D-SCHED-008依赖**：candidate retention必须由明确`willSkipPostFrameTail` predicate触发；
  当前Unity未实现unlock时它等价于stepWait，未来flag1+stepWait必须normal clear，不能retain。该合同已写入
  `R2-CANDIDATE-TAIL-01`，避免R2修复硬编码裸stepWait后被R3再次返工。
- **2026-08-23 debug-step范围决定**：用户明确不需要F1/F2战斗调试步进。按`D-015`，`D-STEP-001`与
  仅由debug tail-skip触发的`D-SCHED-008`改列为批准省略的debug-only行为；`R2-CANDIDATE-TAIL-01`与
  `R3-STEP-01`不执行，不再阻塞normal combat主线。
- **2026-08-23 当前执行包**：`R8-WP01G-R02 / IN_PROGRESS / SOURCE-FIRST`。本包只处理
  `D-MOV-005`、`D-COL-005B`、`D-HIT-005`、`D-LIFE-001`，顺序固定；先闭合C++→Unity source与
  production reachability，只有source-confirmed且正式可达差异才在独立Change Record后修改脚本。
- **2026-08-23 D-MOV-005实施开始**：`R8-MOV-005-001 / IN_PROGRESS`。current正式DAT仍仅type2/type4
  weapon state2000走已等价fallback；exact current type0通用pass缺C++ state2000 facing writer。Task/Record/Ledger
  已先建立，下一步只允许补exact branch与正/零/负Vx fixture，尚未写脚本或取得compile证据。
- **2026-08-23 D-MOV-005代码已写**：`R8-MOV-005-001 / CODE_WRITTEN`。exact pass已在C++对应时点
  以通用state2000+Vx规则写朝向，focused Editor fixture覆盖正/零/负Vx与exact ownership；scoped diff-check
  通过。Unity compile/focused/full self-check仍待，不得标完成。
- **2026-08-23 D-MOV-005代码闭环**：`R8-MOV-005-001 / RUNTIME_PENDING`。fresh compile0，focused job
  `e5e283e740cc49e597c99b7ef994c419`为1/1 PASS，16:14:46 full self-check及74/74 validator PASS。正式
  type0 state2000 Play不可达且C++ trace BLOCKED，故不写runtime VERIFIED。`R8-WP01G-R02`进入`D-COL-005B`。
- **2026-08-23 D-COL-005B实施开始**：`R8-COL-005B-001 / IN_PROGRESS`。block-aware全DAT扫描确认
  `itr kind1=0`；但C++ generic runtime-key selector/case1 grab与Unity CLR-key gate/weapon case1 pickup为明确
  代码合同差异。Task/Record/Ledger已建立，只允许修这两个kind1分支与focused self-check，尚未写脚本。
- **2026-08-23 D-COL-005B代码已写**：`R8-COL-005B-001 / CODE_WRITTEN`。kind1 selector现读通用
  entity runtime keys，weapon case1现进generic grab；actual weapon attacker collect→object consume矩阵已加入，
  scoped diff-check通过。Unity compile/full self-check仍待，不得标完成。
- **2026-08-23 D-COL-005B代码闭环**：`R8-COL-005B-001 / RUNTIME_PENDING`。fresh compile0、actual
  weapon attacker self-check、16:21:57 full self-check及75/75 validator PASS。current DAT block-aware inventory
  `itr kind1=0`，故production Play不可得且C++ trace BLOCKED。`R8-WP01G-R02`进入`D-HIT-005`。
- **2026-08-23 D-HIT-005实施开始**：`R8-HIT-005-001 / IN_PROGRESS`。source/crosswalk确认四类attacker
  consumer未共享current-DAT target dispatcher，weapon/type3 helper被历史CLR victim签名限制，type5 mismatch缺
  common kind0入口。Task/Record/Ledger已建立；尚未写脚本或取得compile证据。
- **2026-08-23 D-HIT-005代码已写**：`R8-HIT-005-001 / CODE_WRITTEN`。四attacker consumer现共享
  current-DAT-first dispatcher；generic weapon/type3/type5 writer与三类shell mismatch矩阵已写，scoped diff-check
  通过。Unity compile/focused/full self-check仍待，不得标完成。
- **2026-08-23 D-HIT-005首次自检失败留痕**：fresh compile0后，full self-check在旧
  `BATTLE-AUDIT4-04`失败；其断言要求type3 tail，但fixture类型固定返回current type0，旧CLR SpecialAttack
  priority曾掩盖矛盾。仅把fixture改为真实current type3 shell；production dispatcher不回退，重验待执行。
- **2026-08-23 D-HIT-005第二次自检合同修正**：真实type3 shell仍未进入burning；C++ source确认type3
  effect switch只有current DAT已变成type0时才进入frame203。普通type3应保持frame20/motion reset。旧断言现按
  source改为frame20/HitConfirm2/motion矩阵；production不回退。
- **2026-08-23 D-HIT-005代码闭环**：`R8-HIT-005-001 / RUNTIME_PENDING`。fresh compile0，第三次full
  self-check PASS，focused job `9411895645354ca4a241d2a84d8525a5`为178/178 PASS。四attacker统一
  current-DAT-first dispatch，matching CLR壳保持exact writer，mismatch走generic typed writer。production
  mismatch Play夹具不可得且C++ full trace BLOCKED，故不写runtime VERIFIED。`R8-WP01G-R02`进入`D-LIFE-001`。
- **2026-08-23 D-LIFE-001复核开始**：`R8-LIFE-001 / IN_PROGRESS / NO-GAMEPLAY-CHANGE-EXPECTED`。
  C++ live merge/split与全部battle-time allocator域、Unity dormant/slot/reset/query/presentation crosswalk已重新闭合；
  当前未发现production差异。下一步只运行现有OID5152 focused/full回归并更新证据；若失败才停止并新建Change Record。
- **2026-08-23 D-LIFE-001复核闭环**：`R8-LIFE-001 / RUNTIME_PENDING / APPROVED UNITY ADAPTER`。
  `OidMergeDormant`保留partner原slot/generation与C++ low-slot inactive固定数组行为在当前battle allocator域等价，
  无production脚本修改。focused job `04ddfe7fa44b4f92beb0618d0f269a13`为32/32 PASS；同代码状态full
  self-check PASS并执行七组OID5152矩阵。真实Play/C++ trace未取得，不写VERIFIED。
- **2026-08-23 R8-WP01G-R02四项收口**：四项均已处理到当前最高证据层；MOV/COL/HIT为最小代码修复后
  `RUNTIME_PENDING`，LIFE为no-code approved adapter的`RUNTIME_PENDING`。本包停止，不自动进入后续D-ID。
  global Change Ledger validator被任务外`WEB-CADENCE-001`的non-governed/unrecorded diff阻塞；未修改该用户工作。
- **2026-08-23 R8-WP01G-R03获批并启动**：用户明确批准`physical input → movement → interaction joint
  runtime certification`并恢复总目标。Task/Handoff已建立；当前无脚本Change。执行顺序为真实InputSystem
  DDJ/DRA(DLA)→movement/jump/landing→held/grab/cpoint/collision/hit联合Play。缺少F2 probe时必须先建test-only
  Change Record，不能直接修改脚本；C++、T8、IL2CPP、Android、服务器、F1/F2 debug继续排除。
- **2026-08-23 R03 F1结果与F2探针启动**：fresh Play中DDJ physical L/S/K于tick424/425/426形成
  combo1/2/3并在tick437进入frame271，DRA physical L/D/J于tick1104/1105/1106形成1/2/3并在tick1117
  进入frame263，二者PASS。现有probe不覆盖position/velocity/landing，已在脚本改动前建立
  `R8-JOINTMOVE-PROBE-001 / IN_PROGRESS / TEST-ONLY`，只允许新增Editor-only D/K联合探针。
- **2026-08-23 R03 F2探针代码已写**：`R8-JOINTMOVE-PROBE-001 / CODE_WRITTEN / TEST-ONLY`。
  探针只由ASCII Editor菜单触发，queue physical D/K并读取FrameInputSet/runtime/position/velocity/landing；
  未改production脚本。fresh compile、Play report与self-check尚待。
- **2026-08-23 R03 F2首次探针失败留痕**：新脚本首次未被`scripts` scope导入，改用`scope=all`后
  Editor assembly fresh生成；ready Play随后在neutral tick773后RightQueued超时，FrameInputSet始终neutral。
  同会话existing DRA亦step1=-1，故暂判synthetic首键注入时点问题而非movement production first difference。
  同一test-only Record现把neutral-ready的首个D移到menu调用上下文，并补Action/device诊断，重验待。
- **2026-08-23 R03 F2第二次探针失败更正**：live trace已证明Right与physical K到达runtime：tick1279
  Right edge，tick1283 canonical Defend64/KeyDefend/CdJump5/frame210，tick1286 frame212 Vx8/Vy-16.3，
  tick1287 airborne。失败源于probe误等canonical Jump32；按既有crossed input合同改等Defend64，production不改。
- **2026-08-23 R03 F2真实Play通过**：`R8-JOINTMOVE-PROBE-001 / FOCUSED_TEST_PASS / TEST-ONLY`。
  fresh compile0；tick1080 Right edge，tick1084 physical K对应canonical Defend edge，tick1088 airborne，
  tick1091 release，tick1108 landing。DAT writer写`jump_distance=8`/`jump_height=-16.3`，首个airborne
  样本Vx7/Vy-14.6，X 775→949，对象数8→8，五项checkpoint全部true。F2报告PASS；R03继续F3。
- **2026-08-23 R03 F3联合Play通过**：held weapon报告覆盖type1/2/4/6 pickup/held/throw/landing且无
  immediate hit；grab/CPoint报告覆盖held injury、统计、mismatch throw、escape与link residue；collision/hit
  报告以10 candidates覆盖character/weapon/special、vrest、durability与abort。三者均PASS，world entity、
  claimed slot、object/logic pool及临时全局状态恢复基线。未观察到production gameplay first difference；
  R03进入fresh compile/focused/full self-check/治理收口。
- **2026-08-23 R8-WP01G-R03完成到当前可用证据**：final fresh compile/Console 0 error；8个相关
  EditMode类257/257 PASS（job `a26ba1e3136f4c73b2a17c4bd105a866`）；full `BattleRuntimeSelfCheck`
  17:17:19 PASS；Change Ledger validator PASS（78 records / 93 governed code files）；scoped diff/whitespace
  check PASS。`R8-JOINTMOVE-PROBE-001`升级`VERIFIED / TEST-ONLY`。本包没有production gameplay改动，
  没有观察到first difference；C++ executable/full trace仍BLOCKED，故不宣称全部战斗逻辑完整对齐。
- **2026-08-23 R03证据可重复性更正**：完成后审计发现DRA Temp结果曾被后续首键失败覆盖；fresh Play
  重新运行DDJ与F2也分别在L/D首键进入FrameInputSet前超时。production未变化，两个不同首键共同失败，
  当前首差为一次性`QueueStateEvent`与Editor/InputSystem采样边界，不是已定位的gameplay差异。已在脚本
  修改前建立`R8-JOINTINPUT-PROBE-002 / IN_PROGRESS / TEST-ONLY`，只允许最多8次release→press物理
  状态脉冲并记录attempt；R03临时重开，三份current报告重新PASS前不得维持完成结论。
- **2026-08-23 R03输入证据探针修正已写**：`R8-JOINTINPUT-PROBE-002 / CODE_WRITTEN / TEST-ONLY`。
  DDJ/DRA的L/方向/动作三段与F2的D/D+K两段，现仅在canonical edge未出现时按tick交替release→press，
  每阶段最多8次并输出attempt。未调用InputSystem.Update、未写runtime/buffer/frame/motion；compile/Play待。
- **2026-08-23 R03输入证据fresh重跑PASS**：`R8-JOINTINPUT-PROBE-002 / FOCUSED_TEST_PASS`。
  compile0；F2 D/K attempt2/1于tick1049/1053/1057/1077完成right/jump/air/land；DDJ attempt1/1/1于
  tick1603/1604/1616到frame271；DRA attempt1/1/1于tick2225/2226/2238到frame263。当前三份Temp
  报告均fresh PASS。D-INP-006升级Unity InputSystem S4 PASS；真实人手硬件/窗口焦点edge仍用户待验。
- **2026-08-23 R03证据可重复性最终收口**：`R8-JOINTINPUT-PROBE-002 / VERIFIED / TEST-ONLY`。
  首次focused为256/257且只失败未被本Change触碰的W05B generation；W05隔离8/8后同8类fresh复跑
  257/257 PASS（job `bf16f84db0b346809407bfe7a01dbc83`）。full self-check 17:33:15 PASS、
  Console error0、Change Ledger validator79 records/93 code files PASS。R03重新标记完成；production0改动。
- **2026-08-23 下一包审计**：68项register复核后，`D-INP-006`已由R03提升Unity InputSystem S4；
  当前最早且仍缺正常战斗联合Play证据的完整链是`D-INP-005`与`D-INP-007A/B/008/009`的AI
  sensing→39-position decision/RNG→FrameInputSet→movement/skill/opoint→hit。已建立
  `R8-WP01G-R04-ai-sensing-decision-action-joint-runtime.md / PLANNED / APPROVAL PENDING`；批准前
  不运行R04、不新增probe、不修改production。
- **2026-08-23 AI对齐范围决定**：用户明确不执行R04，未来将AI改成更适配Unity的状态树或行为树。
  `R8-WP01G-R04`现`ABANDONED BY USER / NO EXECUTION`；`D-INP-005/007A/007B/008/009`改列
  `USER-DEFERRED / NOT AN ALIGNMENT BACKLOG`。现有AI代码/测试不回退、不删除；未来AI只保留固定tick
  canonical FrameInputSet接入合同，不要求复刻C++ sensing/39-position/RNG算法。下一步审计非AI剩余项。
- **2026-08-23 非AI剩余审计完成**：当前无新发现的source-confirmed未实现normal-combat代码差异。
  可继续取得Unity运行证据的11个D-ID分4组：G1 candidate/PreInteraction（SCHED-007、PERF-001），
  G2 negative-link/P1P2（INP-001、INP-004），G3 merge/split（LIFE-001），G4 central handoff/writeback
  （SCHED-009、RENDER-001..005）。另有5个current DAT/fixture不可达exact分支、D-INP-006人手硬件待用户、
  F7/F8/F9 function-key debug policy和R1-WP02 full trace blocker。推荐下一包先做G1；尚未建立/执行R05。
- **2026-08-23 R8-WP01G-R05获批启动**：用户明确批准candidate/PreInteraction adapter joint runtime
  certification并恢复目标。Task/Handoff已建立；顺序固定`D-SCHED-007`→`D-PERF-001`。当前只读盘点
  C++/Unity/现有A/B工具，脚本0改动；若缺probe必须先建独立test-only Change Record。
- **2026-08-23 R05联合运行发现诊断误报并建Change**：fresh candidate相关EditMode为9/9、58/58、185/185，
  PreInteraction为15/15；collision与grab/CPoint live Play均PASS。相同seed的50-AI current与forced-legacy
  最终parity hash同为`fdf240f5...bef1`且两侧zero-GC PASS；current侧35/35 store authority、35/35 oracle、
  mismatch/invalid/fallback均0，却因validator把consume期间entry read与consume后的carrier count要求严格相等而
  `SmokeFailed`。已建立`R8-CANDSTORE-DIAG-001 / IN_PROGRESS / TEST-HARNESS ONLY`；不改gameplay。
- **2026-08-23 R05 candidate stress诊断修正已写**：`R8-CANDSTORE-DIAG-001 / CODE_WRITTEN`。
  validator现把consume后carrier candidate sum作为entry reads下界而非精确值，并新增extra/equal/below三段
  回归断言；collector/store/consume/PreInteraction production均未修改。fresh compile与重跑待执行。
- **2026-08-23 R8-WP01G-R05收口**：`D-SCHED-007`与`D-PERF-001`均达`UNITY JOINT S4 PASS /
  C++ FULL TRACE BLOCKED`。candidate focused9/9+58/58、consume185/185、PreInteraction15/15；collision与
  grab/CPoint live Play PASS。相同seed的50-AI current/forced-legacy均SmokePassed，20项parity/lockstep hash
  全等，双方zero-GC与cleanup PASS；current为35/35 store+oracle、mismatch/invalid/fallback0。
  `R8-CANDSTORE-DIAG-001 / VERIFIED / TEST-HARNESS ONLY`；fresh stress Editor256/256、self-check18:35:05、
  Console0、ledger80/94 PASS。production gameplay 0改动；R1-WP02 full trace限制保留。
- **2026-08-23 R06/G2只读预检**：G2必须拆分。`D-INP-001`的自然current-type0 negative-link writer为
  opoint kind2，且C++明确把child设为AI-controlled；按用户AI范围决定，不再作为非AI Play backlog，现有
  source-correct eligibility代码保留。`D-INP-004`发现新的source-confirmed production差异：C++ P2有方向键+
  numpad3/1/2完整输入，Unity `Player_2` action map只有Move，三个action lookup均为null；旧手工packet fixture
  只证明packet后routing，不能关闭physical source。已建立`R8-WP01G-R06-p1p2-physical-input-runtime.md /
  PLANNED / APPROVAL PENDING`；本次脚本/asset/Play 0改动。
- **2026-08-23 G4只读预检**：`D-SCHED-009`与`D-RENDER-001..005`未发现新的source-confirmed
  production实现缺口；现有普通Central Game/SceneView submission已有S4，但special writeback、liveness/
  identity/visibility和fail-closed ownership仍缺联合Play证书。为避免大探针，G4拆为R07A/R07B/R07C。
  已建立`R8-WP01G-R07A-render-writeback-joint-runtime.md / PLANNED / APPROVAL PENDING`，第一包只处理
  `D-SCHED-009 + D-RENDER-002` actual hit producer→frozen spark→same-tick writeback→next-tick capacity/RNG；
  本次脚本/scene/asset/Play 0改动。
- **2026-08-23 G4后续包可行性闭合**：正式`data.txt`包含OID7/8/51/223/224，pending/dormant/
  generation/death/effect/hit-stop均有production producer；CentralOnly已有Editor-only feature registration、
  failure plan与submission lease，可在不改URP asset下设计四态证书。已建立R07B
  `D-RENDER-003/004/005`与R07C`D-RENDER-001`独立Task/Handoff，均为`PLANNED / APPROVAL PENDING /
  NO EXECUTION`。禁止直接写lifecycle/visibility结果字段、改DAT/URP或恢复Legacy制造PASS。
- **2026-08-23 G3真实Play可行性闭合**：`LF2States.Running==2`，正式`data.txt`含OID7/8/51；
  low-slot self/partner、same-team/proximity/HP前置可在测试初始边界配置，merge/dormant/split必须由完整tick的
  OID maintenance产生。已建立`R8-WP01G-R08-oid5152-merge-split-central-runtime.md / PLANNED /
  APPROVAL PENDING`；split优先真实DJA，DJA不可达时完整推进4500 fixed ticks，禁止直接写Unk338=0。
  本次脚本/DAT/scene/Play 0改动。
- **2026-08-23 R06获批启动**：用户明确批准`R8-WP01G-R06`并恢复目标。现状复核确认Player_2只有Move，
  auto-generated wrapper也只有Move；`.inputactions.meta`为`generateWrapperCode:1`。已在写入前建立
  `R8-P2INPUT-001 / IN_PROGRESS`，范围仅为P2 Attack/Jump/Defend+numpad1/2/3、正规wrapper生成、focused
  test与two-player physical Play probe；P1/crossed mapping/8-slot及所有保护边界不变。
- **2026-08-23 R06代码已写**：`R8-P2INPUT-001 / CODE_WRITTEN`。Player_2新增Attack/Jump/Defend及
  numpad1/2/3 exact binding；Unity Input System generator已正规更新wrapper，未手改生成代码。新增asset/action
  聚焦测试与11-case two-player physical Play probe；probe只queue KeyboardState并观察正式FrameInputSet/
  roster/runtime，不直接写packet或runtime。新脚本compile/focused/Play/self-check/validator待执行。
- **2026-08-23 R8-WP01G-R06收口**：`D-INP-004`达到`UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE
  BLOCKED`。fresh compile0；focused2/2、input regression47/47 PASS；未保存two-human Play clone由正式
  bootstrap/object pool/roster创建slot0/1，11/11 physical press/held/release/no-cross PASS，stable100/101保持。
  full self-check 19:37:29 PASS，Play结束前Console error0；Change Ledger validator 81 records/96 governed
  code files与scoped diff-check PASS。`R8-P2INPUT-001 / VERIFIED`；R07A/B/C、R08仍未执行，R1-WP02
  full trace仍BLOCKED。
- **2026-08-23 R8-WP01G-R07A获批启动**：用户明确批准render pass/hit-record writeback联合运行时认证。
  C++只读source与Unity crosswalk复核未发现新的静态production差异；existing exact baseline job
  `7ec88f1aa50f4f93af44990ad9a08dd6`为2/2 PASS（worker lifecycle、10-slot full RNG gate）。现有证据仍缺
  actual collision producer的完整tick Play链，故已在写入前建立`R8-HITWRITEBACK-001`并只新增Editor-only
  probe。probe现已写入；第一次scripts-only refresh的0 error未覆盖无`.meta`的新文件，full asset refresh首次
  实际导入发现probe内CS0102、随后CS0165，均已只在test-only probe中修复；fresh compile现为0 error，
  Change状态为`COMPILE_PASS`。Play联合报告、expanded focused/self-check与validator仍待执行，
  R07B/R07C/R08继续未执行。
- **2026-08-23 R07A首轮Play**：场景就绪后于worker path `startTick=1510`跑到真实hit producer；报告因
  probe错误要求presentation writeback同步更新Unity-only `LastAdvanceTick`而FAIL。source复核确认C++合同与
  `FinalizePublishedHitRecordCycle`只要求age/tail推进；该字段由另一API维护，不能作为本包权威断言。
  `R8-HITWRITEBACK-001`现为`RUNTIME_PENDING`，只删除该test-only越界断言后重跑；production未改。
- **2026-08-23 R07A第二轮Play**：删除`LastAdvanceTick`越界断言后，worker Play已通过actual hit、live age、
  owner/cycle与RNG前置；下一FAIL来自probe误要求纯worker `PublishedFrame`已经materialize commands。既有worker
  contract明确该frame必须保持未物化，中央宿主随后在`CurrentPixelFramePlan.CapturedFrame`物化。下一步只把
  probe检查移到正式central captured frame可用后，不调用self-check materializer，production未改。
- **2026-08-23 R07A第四轮Play**：正式central captured frame等待修正有效；tick1018已完整证明actual hit、
  RNG+2、frozen/live age、central command与Late幂等。tick1019同一pair未重复追加，属于正式hit-rest边界；
  probe不会清rest或直接写hit，下一步只在加载阶段预建独立攻击者并逐tick启用新pair以验下一tick追加。
- **2026-08-23 R07A第五轮Play**：轮换独立attacker但共享victim仍在第二tick保留1条record，受击者侧状态/
  交互资格会抑制立即重复命中。下一步改为4组预建独立pair逐tick启用；旧victim记录仍留在world参与
  publication/no-publication生命周期，不清任何rest/状态，不直接写record。
- **2026-08-23 R8-WP01G-R07A收口**：4组独立pair的production worker Play tick843～846 PASS：published
  owner/record/command为1/2/3，frozen ages`[0]`/`[1,0]`/`[2,1,0]`，live ages`[1]`/`[2,1]`/
  `[3,2,1]`；no-publication保持cycle845并推进`[4,3,2,1]`。每tick exact2 RNG、Late幂等、warmed
  allocation violation delta0，cleanup全恢复。compile0；worker18/18、hit178/178、central13/13、20:25:11
  self-check PASS；final Console0；ledger82/97 PASS。`R8-HITWRITEBACK-001 / VERIFIED`；
  `D-SCHED-009 + D-RENDER-002 = UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。R07B/R07C/R08未执行。
- **2026-08-23 R07B恢复审计/合同纠正**：实施批准仍未获得，未改脚本、未运行专项Play。只读复用盘点
  确认WP01C opoint lifecycle可提供pending/release/generation producer，WP01D central probes可提供
  command/pixel观察；同时发现R07B验收曾要求OID7/8→51 dormant/split却把R08列为out-of-scope。现已纠正：
  R07B只处理`D-RENDER-003` pending/generation/T+1子集与`D-RENDER-004/005`；dormant/split只归R08，
  R08完成前不得整体关闭`D-RENDER-003`。R07B继续`APPROVAL PENDING / NO EXECUTION`。
- **2026-08-23 R07B获批执行**：用户明确批准`R8-WP01G-R07B`并恢复总目标。只读预检确认正式
  FrameLogic pending producer、RenderDispatch→Late opoint的T/T+1边界、正式data.txt OID223/224和production
  opoint factory均可用；现有producer probe与central diagnostic probe不能形成同一handle/generation的联合报告，
  因此已在脚本写入前登记`R8-RENDERLIVE-001 / IN_PROGRESS`，只允许新增Editor-only联合Play probe。
  production gameplay/DAT/scene/renderer保持不改；dormant/split仍只归R08。
- **2026-08-23 R8-WP01G-R07B收口**：`R8-RENDERLIVE-001 / VERIFIED / TEST-ONLY`，production 0改动。
  fresh sync full-tick Play tick202→203 PASS：OID225/frame51使`slot51/gen1` pending/free，Late OID999同槽
  `gen2`；T冻结拒绝旧/新句柄，T+1只接受新generation并恢复body/shadow。正式OID223/224 body均有
  snapshot/command/resource/submission，shadow snapshot存在但current-DAT gate返回`CommandSuppressed`且无
  command/submission；baseline正式角色body/shadow均提交。actual Z 376/375按`ZInt→slot`排序，同Z tie由
  focused覆盖。focused24/24+9/9+worker18/18、21:47:26 self-check、final Console0、ledger83/98 PASS。
  `D-RENDER-003`仅pending/generation/T+1子集达到Unity S4，dormant/split仍归R08；`D-RENDER-004/005 =
  UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。R07C/R08仍未执行。
- **2026-08-23 R8-WP01G-R07C获批启动**：用户明确批准`R8-WP01G-R07C`并恢复总目标。只读预检确认
  exact cold→current→last-good→replacement ownership self-check、Game/SceneView current pixels、Editor-only
  ready/stale publication与submission lease均可复用；尚无已确认production renderer差异。已在脚本写入前建立
  `R8-CENTRALOWN-001 / IN_PROGRESS / TEST-ONLY`，只允许新增Editor-only联合Play probe；不得修改URP asset、
  scene、material asset、production registration、gameplay或Legacy owner。cold若无法安全形成则保留exact
  self-check并诚实标记，不破坏live feature注册制造PASS。
- **2026-08-23 R07C test-only代码已写**：`R8-CENTRALOWN-001 / CODE_WRITTEN`。新Editor-only probe
  在真实current plan上采集isolated Game-camera pixels，通过既有stale self-check boundary保留last-good，
  持旧submission lease后用render-only dispatch发布replacement并检查generation/retire/lease、legacy suppression、
  checksum及feature/material/draw-mode恢复；cold明确只由exact self-check覆盖。production renderer/gameplay/
  URP asset/scene/material均0改动；fresh compile、focused、Play、自检和validator待执行。
- **2026-08-23 R07C Play启动隔离诊断**：两次Play均在probe request消费前被Bootstrap阻塞；根因是禁用
  Domain Reload时EditMode central测试/编辑器URP回调留下active submission，`BeginBattleAllocationSeal`容量预热
  拒绝resize。未改production初始化；只在`R8-CENTRALOWN-001` test-only probe增加prepare-play菜单，调用既有
  `ResetRuntime()`后在同一菜单回调立即进入Play，避免下一个EditMode render重新发布。该reset不作为cold证据。
- **2026-08-23 R07C首次完整导入失败已留痕**：此前scripts-only refresh未导入apply_patch新增文件；full asset
  refresh首次编译发现probe误写`using NTSD.Simulation.Input`，而`FrameInputSet`位于`NTSD.Simulation`。已只删除
  该错误using；production未改，fresh full refresh待重跑。
- **2026-08-23 R07C第二次完整导入失败已留痕**：修正FrameInputSet using后，probe对`NTSDRenderSpace`
  缺少`NTSD.Animation` namespace；已只补正确using，production仍0改动，fresh full refresh继续重跑。
- **2026-08-23 R07C首轮场景Play失败已留痕**：prepare-play成功绕过静态submission预热阻塞，场景运行到
  tick195、4 objects/2 slots、feature/material均注册；但Game视图没有主动消费worker PublishedFrame，probe一直
  等不到current plan。已只在test-only probe于PublishedFrame/feature就绪后调用公开`PrepareFrame(world)`，
  等价于中央宿主消费，再由真实URP Game camera取像素；production未改，待重新编译/Play。
- **2026-08-23 R07C第二轮场景Play失败已留痕**：到tick201仍无current；既有Game Visibility诊断确认当前
  driver由外层手动推进且PublishedFrame为空，PrepareFrame没有输入。probe现只在该空边界调用一次当前tick的
  公开`world.RenderDispatchAll(currentTick)`建立正式表现快照，再PrepareFrame；不推进gameplay、不写实体字段，
  production仍0改动。
- **2026-08-23 R07C第三轮场景Play失败已留痕**：RenderDispatch后仍等不到current，确认PrepareFrame进入
  readiness拒绝但旧probe未记录reason。已只增强test-only失败报告：首次拒绝立即写plan/diagnostic reason、
  PublishedFrame tick、feature/material readiness，不再超时猜测；production未改。
- **2026-08-23 R07C第四轮场景Play失败已留痕**：仍timeout且未进入PrepareFrame拒绝分支，确认停在首次
  BMP/catalog加载前置。原1200 Editor updates低于同项目R07B的12000；已只把R07C test-only等待上限对齐
  到12000，不改变runtime gate或production代码。
- **2026-08-23 R07C第五轮Play启动隔离失败已留痕**：菜单Reset到异步EnterPlay之间仍有Editor URP
  重发布窗口，Bootstrap再次遇到active submission。未改production Bootstrap；test-only入口改为一次性武装
  `playModeStateChanged`，在ExitingEditMode和首帧Start前的EnteredPlayMode各Reset一次，随后立即解除武装。
- **2026-08-23 R07C第六轮Play失败已留痕**：启动隔离已成功，但timeout计数从tick0、BMP/catalog加载前
  开始，并在feature/object刚就绪的update先于readiness分支触发。已只把计数移动到tick/object/CentralOnly/
  feature均ready之后；资源加载不再消耗current-plan等待预算，production未改。
- **2026-08-23 R07C第七轮Play失败已留痕**：scene-ready后仍超时且没有PrepareFrame拒绝报告，确认只剩
  跨update paused门；初始化链会再次SetPaused(false)，probe反复返回。current/stale/replacement在同一Editor
  主线程回调同步完成且worker in-flight单独受控，故只删除该paused前置，pause开始/结束状态仍保持不变。
- **2026-08-23 R07C第八轮Play首差已留痕**：首次进入联合捕获，current owner/tick/gen/lease/Legacy
  suppression与cleanup正确，但source/resolved/segment均0、isolated pixel0，说明实体已注册而sprite/catalog命令
  尚未就绪。已只增加source/resolved/segment>0表现就绪门，再开始四态；production未改。
- **2026-08-23 R07C第九轮Play全链首PASS**：current/stale/replacement各259 isolated pixels且hash一致；
  stale display tick200/gen201，replacement tick201/gen202；checksum不变，old retire/reject/release、feature/material/
  draw-mode和world cleanup均true。replacement lease/draw字段因在Camera.Render前采集显示false/0，已只把该诊断
  移到渲染后重采并增加lease/segment/draw>0断言，待最终重跑；production0改动。
- **2026-08-23 R07C最终收口为BLOCKED**：最终三态Play PASS：current211/211 gen212、stale212/211
  gen212、replacement212/212 gen213；各4/4 commands、1 segment/draw、259px、hash
  `AE3AFF1E932B491E`，lease/retire/release、checksum `7C369C0D79EF47BA`与cleanup全PASS。cold exact
  self-check PASS、Play未运行。final normal Play同时出现`B-R8-R07C-01`：异步加载期间已有active central
  submission，后续`BeginBattleAllocationSeal→PrepareBattleCapacity`拒绝resize。production first-difference
  stop condition已触发；`R8-CENTRALOWN-001 / BLOCKED`，R07C整体BLOCKED。fresh compile0、focused29/29、
  22:45:37 self-check PASS、clear后Console0、ledger84/99 PASS，但均不能覆盖该Play异常。已建立
  `R8-WP01G-R07C-R01 / PLANNED / APPROVAL PENDING / NO EXECUTION`；未批准前不改production、不进R08。
- **2026-08-23 R07C-R01获批启动**：用户明确批准production repair并恢复总目标。caller审计确认直接场景
  由BattleTestBootstrap初始化且会创建AppManager；菜单/additive场景由既有AppManager初始化而test bootstrap
  立即skip，两条入口互斥，不是double-seal。first difference是BattleBootstrap camera可在loading期间发布
  submission，而两条入口都在实体装配后才BeginSeal；BeginSeal本身也缺少already-sealed早退。已在脚本写入前
  建立`R8-CENTRALSEAL-001 / IN_PROGRESS`，计划Awake disable→seal后Enable→unpause，并补strict idempotence；
  不削弱submission lease/resize保护、不改gameplay。
- **2026-08-23 R07C-R01代码已写**：`R8-CENTRALSEAL-001 / CODE_WRITTEN`。BattleBootstrap Awake
  首帧前disable/clear；AppManager与BattleTestBootstrap均改为BeginSeal后、unpause前Enable；BeginSeal对allocation
  gate+runtime capacity均sealed严格no-op。新增Awake lifecycle与active submission后重复seal focused tests。
  submission resize/lease保护、gameplay、URP asset/scene/DAT均未改；compile/runtime证据待执行。
- **2026-08-23 R07C-R01执行中修正**：第一版Awake disable虽取得compile0、focused20/20、self-check PASS、
  normal Play Console0与R07C PASS，但用户观察到Play时Camera被关闭；该副作用不接受。已先更新
  `R8-CENTRALSEAL-001`合同，改为Camera保持启用、首次BeginSeal在presentation capacity prepare前清退旧central
  publication、重复seal仍strict no-op；R07C探针改为等待seal完成。修正版代码与全部验收待执行。
- **2026-08-23 R07C-R01最终收口**：`R8-CENTRALSEAL-001 / VERIFIED`。最终production只在
  `AppManager/BattleTestBootstrap`把Enable移到seal后、在首次`BeginBattleAllocationSeal`的presentation capacity
  prepare前清退旧central publication，并对双sealed重复调用strict no-op；`BattleBootstrap`第一版Awake-disable已
  撤回且无净diff。fresh compile0；focused job `4cd77be4f1664b329a1e6f3b8167cfc9` 20/20；23:13:13
  full self-check PASS；normal Play直接读取`ScenesCamera.enabled=true`且Console0。final R07C为current
  214/214/gen216、stale215/214/gen216、replacement215/215/gen217，三态4/4/1/1、259px、hash
  `AE3AFF1E932B491E`、checksum/cleanup PASS、Console0。Combat1000为30 warmup+180 sample、1000
  entities/slots、Avg/P95/Max 19.121/21.687/23.805ms、0 B/tick、0 collection、cleanup restored。
  `B-R8-R07C-01`关闭，`R8-CENTRALOWN-001 / VERIFIED`；validator 85 records / 103 governed code files PASS。
  C++ full trace仍BLOCKED，R08未启动，T8/AI/IL2CPP/Android/服务器边界不变。
- **2026-08-23 R08获批启动**：用户明确批准`R8-WP01G-R08`并恢复总目标。再次只读核对C++
  `game_tick.cpp:1008-1154`、Unity `Oid5152RuntimeMaintenanceAll`与正式data.txt OID7/8/51，未发现执行前
  必须修改production gameplay的新差异。已在任何脚本写入前建立`R8-MERGESPLIT-001 / IN_PROGRESS /
  TEST-ONLY`；下一步只读确认running/DJA reachability后新增Editor-only production Play probe。禁止直接写
  merge/dormant/split结果或Unk338=0；first difference必须停止并拆repair。R1-WP02、T8、AI、IL2CPP、
  Android和服务器边界不变。
- **2026-08-23 R08只读可达性阻塞**：`B-R8-R08-01`。当前`Assets/NTSD/Config/data.txt`声明
  OID7/8/51，但`Config/chars/rock_lee.dat`、`chiyo.dat`、`sasori.dat`均不存在；正式loader没有fallback并会
  跳过wrapper，无法满足R08 production factory前置。相邻`I:\GitHub\Unity_GAS\ntsd_proto`虽有同名加密DAT，
  但Task禁止新增/修改DAT且其Unity适配来源未确认，未复制/解密。`R8-MERGESPLIT-001 / BLOCKED /
  NO SCRIPT WRITTEN`；probe与专项Play均未开始，production/C++均0改动。恢复条件为用户恢复三份Unity DAT，
  或明确确认合法Unity资产来源与允许部署方式。
- **2026-08-23 B-R8-R08-01资源身份correction**：初次只检索`ntsd_release`源码树而未见DAT；扩大到
  实际运行根目录后，在`J:\QQFile\NTSD2.4\chars`找到OID7/8/51正式runtime DAT。它们与
  `I:\GitHub\Unity_GAS\ntsd_proto\ntsd_assets\chars`同名文件的长度和SHA-256全部不同，故后者不能作为
  权威/Unity适配资源直接补入。两侧均保持只读，未复制/解密/写入；blocker与恢复条件不变：需要用户恢复
  当前Unity适配版三份DAT，或明确其合法来源/部署方式。
- **2026-08-24 type0资源恢复获批**：用户指定DAT源`J:\QQFile\NTSD 2.4.1\chars`、BMP源
  `J:\QQFile\NTSD 2.4.1\sprite`，目标为`Config/Character`和`Sprite/Character/<dat-basename>`，并要求
  同步改data.txt与DAT bmp_begin路径。已在资源写入前建立`R8-CHARASSET-001 / PLANNED / RESOURCE-ONLY`及
  独立Task；先完整预检，禁止覆盖已有资源或改任何DAT战斗字段。此用户资源部署授权取代R08中“DAT零改动”的
  默认限制，但不授权C++、gameplay、T8或其他范围变更。
- **2026-08-24 type0资源恢复与验证完成**：`R8-CHARASSET-001 / VERIFIED / RESOURCE-ONLY`。已恢复37份
  缺失type0 DAT、182个去重BMP；`data.txt` 42个type0全部指向`Assets/NTSD/Config/Character/`，并以运行时同一
  decryptor/parser复核DAT内227条BMP路径全部存在。Unity全资源导入compiler error=0；定向资源测试job
  `34a8a483ff314b82b65e9df5f4aaaf0e` 1/1 PASS；`NTSD_Battle`正常Play 20秒Console error/warning=0。
  `B-R8-R08-01`资源前置关闭、R08变为READY；但merge/dormant/split probe与任何production gameplay改动均尚未执行。
- **2026-08-24 type0资源测试 Change已收口**：`R8-CHARASSET-TEST-001 / VERIFIED`。它只覆盖type0
  DAT/BMP资源可读性，不改gameplay；全资源导入compiler error=0，定向EditMode job
  `34a8a483ff314b82b65e9df5f4aaaf0e` 1/1 PASS，正常`NTSD_Battle` Play Console error/warning=0。其余R08
  merge/dormant/split行为保持未执行。
- **2026-08-24 R08恢复执行**：用户明确恢复目标；`R8-MERGESPLIT-001 / IN_PROGRESS / TEST-ONLY`。
  B-R8-R08-01已关闭；只读DAT确认OID7/8正式state2 frames为9/10/11/19，OID51 frame290为
  `state15/wait2/next999`且无hit_ja。C++与Unity均确认merged DJA完成后走`Unk328==1→Unk338=0`，下一M1
  在frame290自然split；不需要4500 tick或测试直接写cooldown。下一步只允许新增已登记的Editor-only production
  Play probe；production gameplay、DAT、C++、T8、AI及架构边界不变。
- **2026-08-24 R08新first difference停止**：`R8-MERGESPLIT-001 / BLOCKED / B-R8-R08-02`。probe在一次
  probe-only类型修正后fresh compile0，但正式Play尚未创建OID7/8 fixture，production sprite prewarm先抛
  `Duplicate battle sprite key (56,112)`。正式OID56 DAT的106-120与112-200范围重叠；42个type0仅此一组。
  C++ `renderer.cpp:590-606`按声明顺序首匹配后break，Unity当前异步sheet覆盖与catalog duplicate guard不支持该
  first-declared-wins语义。已建立`R8-SPRITERANGE-001 / PLANNED / APPROVAL PENDING`通用repair；未获批准前
  不改production。R08 merge/dormant/DJA/split仍未运行，不能标完成。
- **2026-08-24 B-R8-R08-02全目录补充审计**：已用项目正式DAT解密密钥/减法算法只读解析`data.txt`全部137个
  对象，137/137成功、共347条`file(lo-hi)`范围，只有OID56的106-120与112-200重叠（交集112-120）。这证明
  当前catalog阻塞的已知数据范围是单一输入实例，但production缺口是通用first-declared ownership合同，不能按
  OID56硬编码。`R8-SPRITERANGE-001`仍为`PLANNED / APPROVAL PENDING / NO CODE WRITTEN`；R08保持BLOCKED。
- **2026-08-24 R08-R01获批恢复**：用户明确批准`R8-WP01G-R08-R01 / R8-SPRITERANGE-001`并恢复总目标。
  Change状态改为`IN_PROGRESS / PRE-CODE`；只允许在`CharacterAnimtorManager`实现通用first-declared ownership、
  新增focused test并执行既定回归。DAT、C++、CentralOnly/atlas架构、gameplay和R08探针验收标准保持不改。
- **2026-08-24 R08-R01代码已写**：`R8-SPRITERANGE-001 / CODE_WRITTEN`。`CharacterAnimtorManager`在并行sheet
  调度前按DAT files顺序建立first-owner集合，sheet任务和catalog构建共同使用该集合；later range即使先完成也不能
  覆盖前序owner。新增独立overlap focused test；builder duplicate guard、DAT/C++/CentralOnly/atlas/gameplay/R08
  探针均未改。compile、focused、正常Play、R08重跑和self-check尚未执行，不能标完成。
- **2026-08-24 R08-R01编译通过**：首次完整导入仅新test因NUnit链式`Does.Contain(int)`产生5条CS1503，已
  留痕并改为布尔Contains断言。第二次force-all后Editor DLL晚于source，UnityMCP Console `error CS`=0、全部
  error=0；`R8-SPRITERANGE-001 / COMPILE_PASS`。focused、正常Play、R08和self-check仍待执行。
- **2026-08-24 R08-R01 focused通过**：overlap job `64acdbff4e2f46aeafc519eed0f68d2b`为2/2 PASS；
  existing common-atlas/catalog-resolver/device-policy job `3da7ae8f160a4e7cacf1a6e84a1c1dc5`为29/29 PASS。
  Change推进为`FOCUSED_TEST_PASS`；normal Play、R08 merge/split与full self-check仍未运行。
- **2026-08-24 R08-R01 normal Play与R08恢复**：`NTSD_Battle`正常Play 25秒error/warning0，旧
  duplicate `(56,112)`未再出现。全DAT映射探针审计137 definitions、12487 catalog entries，没有OID56 range/source
  mismatch；整体唯一FAIL为独立state8000 dynamic command witness且`workerPath=false`，不作为本Change PASS。
  R08随后已真正执行OID7/8 fixture：OID7→51/frame290、HP/HPBound/PP/metadata和OID8 dormant正确，cleanup恢复。
- **2026-08-24 R08 probe合同修正中**：首轮probe被另一Editor request poller清pause；已只在当前probe运行期重新
  断言pause。随后旧断言误把Unity `ObjectCount`当纯逻辑实体数；structural delta证明合体tick没有额外spawn/register，
  而既有Unity adapter中每个production character同时贡献logic+shell。probe已改为记录post-fixture count并严格验证
  dormant后减1。最新代码fresh compile0，但当前Unity Play主线程高CPU、7.7GB且所有非ping MCP命令在内部30秒
  timeout，尚未完成该版R08重跑/full self-check；未强杀Editor或启动第二实例。
- **2026-08-24 R08真实split first difference停止**：用户退出Play后恢复目标，R08依次纠正test-only的
  logic+shell ObjectCount、动态stage Z、同tick physics终值与canonical FrameInputSet输入合同。正式OID51 frame290
  `hit_ja=0`，C++/self-check均证明DJA不可提前清cooldown，故按合同分批推进4500完整tick。merge runtime、OID51
  Central body与dormant suppression均通过；final maintenance进入`partner.Reset()`时，relation/link setter向已排除的
  AI unified row发布并抛`stale slot generation after commit`。这是production first difference `B-R8-R08-03`，
  split/cleanup未完成。`R8-MERGESPLIT-001 / BLOCKED`；已建立`R8-WP01G-R08-R02 / R8-AIROWGEN-001 /
  PLANNED / APPROVAL PENDING / NO CODE WRITTEN`。批准前不得改production、削弱ValidateRow或改变slot generation。
- **2026-08-24 R08-R02只读预检闭合**：未修改production。已确认slot/store generation并未被错误推进；异常来自
  CharacterInput激活的unified publisher持续到RuntimeMaintenance，而dormant partner不在当前Included row、四类store
  仍以原generation绑定。推荐最小repair是通用`row-membership invalidation`：merge进入dormant前和split reset前结束
  当前publisher，下一tick因publisher inactive强制full rebuild；不unbind store、不吞ValidateRow、不增generation、
  不release slot。focused矩阵必须验证merge排除、split恢复和next-tick原generation重纳入。Change仍为
  `R8-AIROWGEN-001 / PLANNED / APPROVAL PENDING / NO CODE WRITTEN`。
- **2026-08-24 R08-R02获批恢复**：用户明确批准`R8-WP01G-R08-R02 / R8-AIROWGEN-001`并恢复总目标。
  Change推进为`IN_PROGRESS / USER APPROVED / PRE-CODE`；只允许先写focused旧实现复现，再实现通用merge/split
  row-membership invalidation。generation、allocator、store owner绑定、ValidateRow、AI策略及其他模块保持不改。
- **2026-08-24 R08-R02 focused reproduction已写**：仅修改existing
  `AiDecisionSoAShadowEditorTests`，新增merge不得roll-forward dormant row与split原generation reset/reactivate两条用例；
  production尚未改。下一步先compile并在旧实现上取得预期FAIL，再实施repair。
- **2026-08-24 R08-R02旧实现失败与代码写入**：focused job `aebfc0fa94ad4b3bac8d2b0230aee229`
  的split用例精确复现Play同一stale-row异常；merge fixture前置已修正。production最小repair已写：publisher新增
  row-membership invalidation，occupancy API复用；merge进入dormant前与split partner.Reset前各失效current pass。
  `ValidateRow`、store绑定、generation、allocator、AI策略均未改。Change=`CODE_WRITTEN`。
- **2026-08-24 R08-R02编译外部阻塞**：production写入后的force-all发现S0 HOLD文件
  `InProcessLockstepAuthoritySessionEditorTests.cs:168/175`两条既有CS0019（`int % SimulationInputButtons`）。R02不越权
  修改S0；最新R02尚未进入DLL，compile/focused/self-check/R08均不得标通过。
- **2026-08-24 S0 syntax-only阻塞关闭 / R08-R02编译通过**：在`S0-INPROC-AUTHORITY-001`既有Record下仅为
  `tick % N switch`增加两处括号，不改变S0输入序列或HOLD。force-all后Editor DLL晚于R02 source，Console全部
  error=0；`R8-AIROWGEN-001 / COMPILE_PASS`。focused/self-check/R08仍待。
- **2026-08-24 R08-R02 focused通过**：新增2/2、unified authority21/21、CharacterInput live-slot/0-GC37/37
  均PASS；`R8-AIROWGEN-001 / FOCUSED_TEST_PASS`。整类扩大运行唯一`Position38 predicted-DUA`失败，独立重跑仍
  失败且不经过OID5152/membership路径，保留为独立既有AI fixture问题，不用它否定或证明本Change。下一步full
  self-check与R08 4500-tick Play。
- **2026-08-24 R08-R02 full self-check外部阻塞**：request结果`01:33:25Z`在OID5152检查前由独立
  `R-HC-01 / CheckDeployableResolvedGeometryRisks`失败，来源为恢复DAT中的`bdy h=-999`和`itr w=0`未分类形状。
  本Change不处理DAT/parser/geometry；不能标self-check PASS。继续执行直接R08 4500-tick Play验收。
- **2026-08-24 R08-R02 / R08最终Unity S4通过**：`R8-AIROWGEN-001`关闭了split reset的stale unified-row
  production异常。最终`Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json`于`01:48:32Z`写入PASS：
  4500 tick、OID7/8→51→7/8、dormant、原slot0/10+generation1、当前HP/HPBound各半95/95、tick末
  frame113/state8、Central merged/dormant/split visibility均通过。split局部ObjectCount `14→15`、claimed `8→8`；
  generation-safe cleanup释放5个post-baseline实体，最终world/claimed/object pool/logic pool恢复`2/1/1/1`，RNG恢复、
  cleanup error为空。`R8-AIROWGEN-001`与`R8-MERGESPLIT-001`均为`VERIFIED`，`B-R8-R08-03`关闭。
  full self-check仍被独立`R-HC-01`前置阻塞，R1-WP02 full trace仍BLOCKED；退出Play后有1条Unity scene-close warning，
  故不声明本轮warning0，也不扩大成全C++ runtime完整认证。
  最终`git diff --check`无whitespace error，Change Ledger validator PASS（91 records / 111 governed files）。
- **2026-08-24 post-R08剩余审计与下一包准备**：原11个可执行非AI D-ID现均已达到Unity S4或明确
  source-deferred边界；`D-LIFE-001`与`D-RENDER-003`由R08提升为`UNITY JOINT S4 PASS / C++ FULL TRACE
  BLOCKED`。当前最前置可行动项是验证基础设施`R-HC-01`：正式OID58 frame75/76和OID10 frame75/76/77
  含5个`w21/h-999`倒置body。只读C++与Unity production均保留raw `y2=y1+h`和strict overlap；普通小itr
  不命中，但跨过倒置两端点的大itr仍会命中。旧self-check分类缺失而非已确认gameplay差异。已建立`R8-WP01G-R08-R03`、
  `R8-GEOMETRYCHECK-001 / PLANNED / APPROVAL PENDING / NO SCRIPT CHANGE`与handoff；批准前不改self-check。
- **2026-08-24 R08-R03获批恢复**：用户明确批准`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001`并恢复目标。
  Change推进为`IN_PROGRESS / USER APPROVED / PRE-CODE`；只允许修改self-check的negative-height body分类和
  production collector夹具，DAT/parser/production collision及其他模块保持0改动。
- **2026-08-24 R08-R03 pre-code authority correction**：进一步按strict不等式代入确认negative-height rect并非
  全局inert；大itr若同时跨过其两个倒置端点仍会命中。Task/Record/Handoff已更正为同时验证普通不命中与跨端点
  命中、并覆盖左右朝向；production仍0改动。
- **2026-08-24 R08-R03代码已写**：`R8-GEOMETRYCHECK-001 / CODE_WRITTEN`。仅self-check增加精确5-entry
  negative-height body分类与ordinary/enclosing × right/left production collector四矩阵；其他non-positive geometry
  继续fail closed。production collision、DAT、parser及其他脚本0改动；compile/self-check/回归尚未运行。
- **2026-08-24 R08-R03验证完成 / 新first difference停止**：fresh compile0；full self-check实际越过R-HC-01，
  日志为137 definitions、82200 frames、90 zero-width itr、5 known negative-height body、0 unexpected/other，四个
  ordinary/enclosing × right/left production collector断言全部通过。`R8-GEOMETRYCHECK-001 / VERIFIED`，R-HC-01关闭。
  随后独立`CheckMovementDatLoadingContracts`因仍读取已迁移删除的`AnimationConfig/Mingren/naruto.dat`失败；这是
  test fixture path first difference，不属于几何包。已拆`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001 / PLANNED /
  APPROVAL PENDING / NO SCRIPT CHANGE`；批准前不顺手修改路径。
- **2026-08-24 R08-R04获批恢复**：用户明确批准`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001`并恢复目标。
  Change推进为`IN_PROGRESS / USER APPROVED / PRE-CODE`；只允许让self-check按objectId读取当前
  `ObjectDefinition.file`并替换旧硬编码callsite，production catalog/loader、data.txt、DAT和gameplay保持0改动。
- **2026-08-24 R08-R04代码已写**：`R8-DATFIXTUREPATH-001 / CODE_WRITTEN`。self-check新增objectId→当前
  `ObjectDefinition.file`→production resolver overload，11个production DAT callsite已迁移；旧AnimationConfig与
  FrameConfig clone literal清零。decrypt/parser/converter与字段断言未改，production/资源0改动；验证待执行。
- **2026-08-24 R08-R04验证完成**：fresh compile0；CharacterAssetDeployment job
  `75849d918dec46d88b01f1253cecec63` 1/1 PASS；`Temp/NTSD_BattleRuntimeSelfCheck.result`于`02:27:38Z`
  写入PASS。movement、Naruto DDJ、sprite range、weapon原字段断言均通过；预期负向fixture error日志清空后最终
  Console0。`R8-DATFIXTUREPATH-001 / VERIFIED`，production/资源0改动，没有暴露新的first difference。

- **R1-WP01 阻塞**：无；规划已完成。
- **B-R1-WP02-01 — trace coverage blocker**：未发现能从未修改 release runtime 取得 R1 full schema 的既有外部通道。`NTSD_DEBUG_TICK` / 相对诊断文件是局部日志线索，不覆盖统一 checkpoint 合同。
- **B-R1-WP02-02 — deterministic input blocker**：没有发现现成逐 tick input journal/replay 或 non-interactive CLI；入口忽略 argv 且依赖 SDL/物理键盘。
- **B-R1-WP02-03 — authority write-safety blocker**：已有诊断路径以相对文件 append 写入；非 authority working directory 的资源加载和无写入保证尚未验证，不能为验证而冒险启动。
- **B-R1-WP02-04 — source/executable identity blocker**：当前 source/Makefile 与实际 `ntsd_new.exe` 没有可验证的精确 build identity。
- **停止结论（仅 R1-WP02）**：按照 D-006 和 R1-WP02 Task Contract，该 Work Package 已停止。不得修改 C++、不得改用 debug/diagnostic executable、不得开始 Unity trace 或 comparator 以绕过 blocker。
- **R1 主线边界**：C++ 源码行为合同、Unity 静态 crosswalk、差异登记和验收设计不依赖 R1-WP02 自动 full trace；R1-SOURCE-001～007 已完成静态盘点。历史上 R1 closure 曾要求确认 R2-PASS-01；当前该门槛已由用户的连续执行授权和 `D-009` 取代，R2～R8 仍须遵守各自 Task Contract / Change Record / 分层验收，但不再逐包停止。
- **R1-WP02 恢复条件**：用户提供或确认一个现有的、可从未修改 `ntsd_new.exe` 使用的只读采集/输入方案，并能同时解决非 authority 输出、最小 fixture/run identity 与所需 trace 覆盖；否则 R1-WP02 保持 BLOCKED。

## 2026-08-24 — R8-WP01G-R09 final evidence reconciliation planned

- R08-R04完成后进行只读现状审计：R05～R08可执行非AI联合证据均已到当前允许层级，完整self-check亦恢复PASS；
- 发现父编排、68项D-ID登记册与旧synthesis仍含被R07B/R08/R03/R04取代的历史pending/blocked文本，需独立
  文档包统一校正，不能据此重复修改gameplay；
- 已建立`R8-WP01G-R09-final-evidence-reconciliation.md`与对应handoff；状态为
  `PLANNED / APPROVAL PENDING / DOCUMENT-ONLY / NO SCRIPT CHANGE`；
- 本包批准后只做证据对账、集合校验和文档一致性验证；若发现新source-confirmed gameplay差异，只登记独立
  后续Task/Change，不在R09中修改脚本；
- `R1-WP02` full trace继续BLOCKED，T8默认stage.dat暂缓，AI C++ parity、F1/F2调试步进与IL2CPP保持用户排除。

## 2026-08-24 — R8-WP01G-R09 approved and resumed

- 用户明确批准`R8-WP01G-R09`并恢复目标；
- 状态推进为`IN_PROGRESS / USER APPROVED / DOCUMENT-ONLY / NO SCRIPT CHANGE`；
- 当前开始逐项对账68个D-ID与R05～R08最新证据；不运行Unity/C++，不修改脚本、scene、config、资源或
  已批准的Unity适配边界。

## 2026-08-24 — R8-WP01G-R09 complete at approved Unity evidence

- 68项最终对账完成：43项Unity S4/runtime覆盖、5项exact witness不可得、1项source等价/full trace缺失、
  9项用户排除/未来替换、1项debug-key policy、3项approved adapter/config、6项test/worker/performance；
  合计68、missing0、extra0、duplicate0；
- `D-LIFE-001`与`D-RENDER-003`依据R08/R07B更新为Unity joint S4；F1/F2三项按用户决定退出normal-combat
  backlog但保留source difference；`R8-SPRITERANGE-001`已有七层证据并升级VERIFIED；
- R8父编排、all-diff register、synthesis、post-AI residual audit、总计划、Task和handoff已统一；
- 当前没有新的normal-combat、production-reachable、source-confirmed、Unity-unimplemented脚本差异；
- R09脚本/scene/config/resource/C++改动0，未运行Unity、Player、性能或C++ executable；
- R8只可宣称“批准范围和当前可取得Unity证据层完成”。R1-WP02 full trace仍BLOCKED，T8与其他边界继续保留。
- final verification：68/68、missing0、extra0、duplicate0；Change Ledger validator 93 records / 111 governed
  code files PASS；R09 scoped diff check PASS（仅既有LF→CRLF提示）。

## 2026-08-24 — R11/R12 acceptance and F7/F8/F9 closure authorized

- 用户授权执行仍有意义的验收测试，并新增按模式控制的F7/F8/F9；全部通过后允许结束当前目标；
- 新只读DAT盘点更正旧R09事实：C++与Unity正式DAT现有8个authored state8xxx frame，因此旧
  `authored state8000=0`证据已过期；`R8-WP01G-R11 / R8-AUTHOREDSTATE-PLAY-001`将补production full-tick Play；
- 正式state2000共38帧但没有type0角色，故不再等待不存在的type0样板；使用正式weapon/object样板验收fallback；
- OID999有效body frame399在正式producer/next/opoint不可达，CLR/current-DAT mismatch在C++统一Entity中不存在，
  两者继续以source+synthetic fixture裁决，不伪造production Play；
- `R8-WP01G-R12 / R8-FUNCTIONKEYMODE-001`已获用户授权并进入`IN_PROGRESS / PRE-CODE`：只实现F7/F8/F9，
  由GameConfig的gameModeId+battleGameModeId规则控制，仅LocalFreeRun物理捕获，tick边界消费；
- 当前活跃Change：`R8-AUTHOREDSTATE-PLAY-001`、`R8-FUNCTIONKEYMODE-001`；脚本尚未修改；
- R1-WP02 full trace仍BLOCKED，T8暂缓，F1/F2、AI C++ parity、Android、服务器、IL2CPP保持排除。

## 2026-08-24 — R11/R12 verified and approved goal closed

- `R8-AUTHOREDSTATE-PLAY-001 / VERIFIED`：R11 production Play于12:22:17 PASS。OID150 state2000
  正/负Vx朝向通过；OID32 state8032得到DAT32/frame0/offset140/effective pic140，主线程materialize后的
  Central body command/catalog/UV通过；cleanup恢复基线；production gameplay/DAT/C++零改动；
- `R8-FUNCTIONKEYMODE-001 / VERIFIED`：GameConfig exact 0/1白名单、LocalFreeRun-only physical edge latch、
  tick边界request、F7 postframe、F8/F9 Mode2复用以及checksum/parity/snapshot/restore均已落地；
- R12 production Play于12:26:06 PASS：F7 tick1581四项500，F8 tick1582生成9个，F9 tick1583清理
  7/7个tail时仍合格候选，2个已在此前转换类型；request与cleanup通过；
- focused jobs：`7c4e0d2675f74d12aacca145f75aa302` 4/4 PASS；
  `dca455601f2a4997be98eae4baaa7db8` 18/18 PASS；
- `Temp/NTSD_BattleRuntimeSelfCheck.result`于12:28:41为PASS；fresh compile无C# error；
- Change Ledger validator PASS（95 records / 122 governed code files），scoped `git diff --check` PASS；
- 当前没有活跃的正常战斗脚本Change；批准范围内目标完成。R1-WP02 full trace继续BLOCKED，T8、F1/F2、
  A→B→C、AI C++ parity、Android、服务器、IL2CPP继续排除，不能扩大为C++ executable full-trace认证。

## 2026-08-24 — CAMERA-PRESENTATION-REMOVE-001 verified

- 用户要求移除`BattleCameraSafeArea`的safe-area、viewport布局/视野、follow/边界和调试逻辑；
- 用户追加保留：背景 bounds 驱动的正交尺寸自适应；不改写相机位置；
- Change Record、Task与handoff已在脚本修改前建立；
- 当前只读审计确认该脚本没有外部脚本调用，但`NTSD_Battle`场景保留组件序列化引用；
- 最终实施仅保留同名组件及上述背景尺寸适配；不修改scene、`NTSDRenderSpace`、URP、战斗runtime或C++。

### 中间代码状态（已被用户范围修正取代）

- 脚本曾暂时收缩为无运行逻辑兼容标记；该状态不作为最终交付；
- 最终将保留背景自适应尺寸，其余safe area、viewport布局、follow/边界、camera offset与Editor/GUI/Gizmo仍删除；
- 最终最小代码只按background bounds与aspect更新`orthographicSize`，不改Transform；
- Unity fresh compile（14:44:27）、Scene组件解析、背景 bounds→相机尺寸数值、短Play bootstrap、filtered Console、Ledger validator和scoped diff均已通过；
- 当前Change=`CAMERA-PRESENTATION-REMOVE-001 / VERIFIED`。没有修改scene、`NTSDRenderSpace`、URP、战斗runtime或C++。

## 2026-08-24 — CAMERA-BACKGROUND-FITMODE-001 superseded

- 用户要求新增“背景全面覆盖视野”模式，解决当前相机完整显示宽背景时的上下镂空；
- 已在代码修改前建立 Change Record、Task 与 Handoff；
- 计划仅加入`ContainBackground`与`CoverViewport`的`orthographicSize`公式选择，默认`CoverViewport`；
- 它不拉伸背景，因而会裁切宽背景的左右边缘；不改Transform、viewport、安全区、follow、URP、scene或战斗runtime；
- 已写最小代码：私有`BackgroundFitMode`+序列化下拉字段，`Contain`选择`max`、`Cover`选择`min`；
- fresh compile（15:05:20）、当前场景`backgroundFitMode=CoverViewport`、背景→`orthographicSize=7.05703163`数值、Transform不变与短Play均已通过；
- 用户已确认Cover会裁切原始背景内容，不能作为交付；当前Change=`CAMERA-BACKGROUND-FITMODE-001 / SUPERSEDED`；
- 新Change=`CAMERA-BACKGROUND-NOCROP-FIT-001 / CODE_WRITTEN`已以“完整显示 / 全面覆盖但保留全部内容”的无裁切方案替代；
- 当前已写base-scale捕获/恢复与必要轴向伸缩；compile、无裁切审计、临时Play的Stretch→Contain→Stretch切换、bounds数值、Transform不变和bootstrap均通过；
- 用户新增架构约束：后续联机对战下不得由`BattleCameraSafeArea`写背景Transform/localScale；
- 当前Change=`CAMERA-BACKGROUND-NOCROP-FIT-001 / ROLLED_BACK`；compile、编辑器/Play背景`scale=(1,1,1)`、bootstrap与filtered Console均已通过；
- 无裁切全覆盖必须另行采用纯渲染层方案，本轮不实现。agent未保存scene；现有`NTSD_Battle.unity`的大范围用户diff不含本Change的mode/base-scale/stretch字段。


Goal18????PLANNED/TEST_FIRST?`NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-PRODUCTION-001`, `NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001`, `NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001`???I1?I2?I3?I3?I1/I2?????Temp/Goal18_*???????Goal17?????

Goal18 I1 `NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-PRODUCTION-001` IN_PROGRESS/TEST_FIRST???RED9FAIL???13?RED?I2/I3?PLANNED???????

Goal18 I1 NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-PRODUCTION-001 FOCUSED_TEST_PASS/13_OF_13?I2 NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001??IN_PROGRESS/TEST_FIRST????????API stub??RED??????I3?PLANNED?????

Goal18 I2 NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001 FOCUSED_TEST_PASS147/147/WARMED0B?I3 NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001 IN_PROGRESS/TEST_FIRST???I1/I2????

## Goal18F 收尾追加（2026-09-12，VERIFIED，限定 impact 三包）
本追加更正此前 Goal18 的 PLANNED/IN_PROGRESS/CODE_WRITTEN、SHARED_PENDING 等恢复状态；历史段落、乱码、失败和证据均保留。用户本轮授权为断点续传，没有重做或回退 I1/I2/I3。
- I1 `NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-PRODUCTION-001`：VERIFIED。Authority 为 battle_world.cpp:5405-5531 的 environment/+0x90/+0x164 写入；瞬态 HitPlan 捕获/投影/mask，复用既有 catch-source。历史 RED 11 FAIL/2 PASS；本次共享 focused 13/13 PASS。
- I2 `NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001`：VERIFIED。Authority 同段的 owner/type/immunity/respond/motion 有序事务；纯计划 resolver，拒绝零写、对象 Y 步长 2.3、字符 3.0 与除法位模式。历史 RED 执行147项，报告至少25失败且 capped，精确失败总数未知，禁止写成147 FAIL；本次147/147 PASS，warmed0B。
- I3 `NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001`：由 CODE_WRITTEN 经本次 FOCUSED_TEST_PASS 推进为 VERIFIED。Authority 为 hit_candidates.cpp:134-256、battle_world.cpp:4540-4580/5270-5309/5405-5531、game_session.cpp:4171-4182；concrete/generic/legacy/HitPlan 共用既有 shared writer。历史 RED137 FAIL/57 PASS，本次194/194 PASS，Shadow valid/0 mismatch。当前 Record C++ 原文与现场源码逐字匹配，正式 EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；见 Temp/Goal18F_AuthorityIdentity.json。

F1：原四项旧 focused 新鲜4/4 PASS，原失败源码守卫在续传前已经修好；第二份旧 XML 实为3/3，不能误报它覆盖四项。证据 Temp/Goal18_F1_OldFocused_Result.json 与 Goal18_F1_OldFocused.xml。
F2：Temp/Goal18_PlayF_attempt1.json 与 attempt2.json 均 PASS/cleanup=true；第二次补全 PP/rest 观测。current Tayuya OID36/frame243，经真实 SimulationTickDriver，seed424242、empty input、采样 tick6/7，kind11 只有 index3 候选。environment=-20、catch_source8242、impact_source50、action182；kind10 Y=-2/Vy=-6，kind11 Y=-3.75、Vy=-8.9，Vx/Vz=/1.07 位模式匹配。HP/PP/rest/delay/WeaponCount 与 impact 边界 RNG 保持。98个 C++/Unity 比较字段 firstDifference=null，详见 Temp/Goal18_PlayF_Comparison2.json、Goal18_AuthorityTrace.json 与 Goal18_AuthorityProvenance.json。
见证边界：C++ harness 调用当前 playable core 的几何候选与 impact 阶段，恢复 Unity 采样边界 CRT 状态、使用冻结 Direction-B 帧夹具并平移 Z 原点，排除无关实体；不是完整 C++ GameSession host replay。PP/sourceArest/targetArest/targetVrest 由 Unity 新鲜采样证明不变，未宣称这些字段由 C++ 输出。RNG 不抽仅指 impact 阶段，整个 driver tick 仍有其他既有 RNG 消费。object2.3、自定义 respond 与 kind17/18 由 focused 夹具覆盖；kind17/18 为 PLAY_NOT_PERFORMED_NO_PRODUCER。

F3：唯一共享 B6 job032efcd73b5949f1bada58270878a741 执行964=610前置+354新增，原始963 PASS/1 FAIL；失败为旧 impact 名称守卫，已按原批类内期望授权只修改四个字符串。定向完整守卫4/4 + refill9/9，于 jobb3527a23f1324eb7872d68f1e64eb9dc 合计13/13 PASS；首次定向启动0tests超时单独保留。合并最新定向结果后964个用例均有PASS证据，未进行第二次整批B6，也未篡改原963/964 XML。全部前置92/80/17/72/24/23/32/140与Goal17三包13/4/10覆盖数量核对一致，见 Temp/Goal18F_B6_Coverage.json。full SelfCheck于2026-09-12 08:37:27Z新鲜PASS；汇总 Temp/Goal18F_Regression.json，原始 Temp/Goal18F_B6.xml、Goal18F_GuardAndRefill.xml、Goal18F_SelfCheck.result。
双构建实际命令：dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly；Editor同命令。均exit0/0error，warnings47/104；最终Editor增量后再次0error，见 Temp/Goal18F_RuntimeBuild.txt、Goal18F_EditorBuild_Final.txt。Unity重载后无编译错误，SelfCheck Console7条均预期registration/rest负向夹具，不声称Console0。
Tools/Validate-ChangeLedger.ps1通过452 records/2 governed code files；最终文件 Temp/Goal18F_Validator.txt。PowerShell默认把Git全局ignore不可读与CRLF warning当作终止错误，因此仅对子进程追加 core.excludesFile=NUL、core.safecrlf=false，保留既有safe.directory设置；不写.git/config，不绕读受限文件。历史Record非当前diff警告保留。
SelfCheck曾切换为空场景，已通过既有编辑器重新打开 NTSD_Battle，isDirty=false/root13；Scene SHA仍为D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11，见Temp/Goal18F_FinalSceneRestored.json。

F4：本次只改两个授权测试文件（probe增加8行、旧impact守卫4个字符串）与治理文档；无生产脚本变更，无schema/RNG写入/HP/PP/rest/delay主体修改，无FluteForce/+11复活。三个既有UI图片修改与.claude用户目录保留。无git add/commit/push。全部历史中文乱码只保留，本次追加为UTF-8正常中文；metadata状态和Ledger当前状态更新，不重写历史事实。回滚仍仅限获批后反向本次测试/文档增量，不回退既有三包。
三包 VERIFIED 仅指本轮批准的 impact 范围与上述分层证据，不代表整个战斗系统完全对齐；既有 USER_HOLD、内容Direction-B与默认stage资产暂缓继续有效，不启动后继任务。

Goal19 / 2026-09-12 / PLANNED / TEST_FIRST: `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`, `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001`. W1 then M1; shared regression once at batch end. User authorization limited to declared paths. Records/tasks established before scripts. Evidence Temp/Goal19_*.

Goal19 progress: `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001` FOCUSED_TEST_PASS / 771_OF_771 / canonical current-weapon Play32PASS / LegacyPlay pending; `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001` PLANNED, no M1 script edits. Shared regression not yet run. Evidence Temp/Goal19_W1_*.

Goal19 W1 `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001` RUNTIME_PENDING: focused771PASS + dual current weapon Play32/32, 512 fields no difference, cleanup/asset hashes pass; only shared batch gates pending. M1 `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED.

Goal19 pre-shared gate: W1 and M1 RUNTIME_PENDING solely for shared batch acceptance; W1 focused771 + dualPlay32/32/512fields equal; M1 focused22 + real rendering Play +0B stable high-water path. Shared regression will run once using Temp/Goal19_SharedRegressionPlan.json.

## Goal19 closure / 2026-09-12 / VERIFIED (scoped)

W1 `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`: shared nine-field nonzero/fallback selector; canonical delegates without selection behavior change; Legacy relation/stats and exact held call-site gates/counters/direct writes corrected; old table bool action consumers retired, NTSDSpec API unchanged. Authority input_routing.cpp808-879/997-1093/1163-1169/1200-1313/1350-1490, formal EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; README_SOURCE + playable build inclusion checked. RED672=209FAIL/463PASS, plus separate boundary RED2/6,1/2,4/10. Final focused771/771. Real driver NTSD_Battle Naruto2 + current weapons122/123, profiles32/32 each, seed424242/tick21..52, 512 compared fields firstDifference=null, cleanuptrue. Nonzero stats remain fixture-only; current zero stats and pic999 content mean fallback/field proof, no physical keyboard or weapon pixel claim.

M1 `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001`: RED10=7PASS/3FAIL, one actual MinMaxAABB at growth plus three index-range overlap warnings and NaN accepted. Not falsified. Selected batch SetSubMeshes candidate with reusable value-only staging, current finite vertex upload first, batch only changed counts/ranges, stable active-prefix and stale-tail handling retained. No Build/CreateMesh/index template changes; no log filtering or enlarged bounds. Final focused22/22 (14new+8existing), includes original3 SelfCheck groups covering the8 reported sites, cache recovery and NaN/+Inf/-Inf. Stable high-water4096/active1,256Builds:0B and4.930078125us average in this environment. Real normal Play tick2759 observed; full1920x1080 battle image visually verified in Temp/Goal19_M1_RenderPlay_Camera.png (byte-identical to first screenshot); original composited capture2errors retained, not a mesh failure. No GPU performance claim from zero EditorStats.

Exactly one shared regression job637100deade44a0eb4bf25d0d9950b9c:1778/1778 PASS = originalB6964 + W1771 + M114 + existingbackend8 + refill9 + nativeground12. Original B6 class counts unchanged; no old case missing. Full SelfCheck requested once, fresh PASS at2026-09-12T10:09:52Z. Actual builds: `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly` and Editor equivalent, both exit0/errors0, warnings47/104. Validator passes with explicit RepositoryRoot and process-only Git warning settings; first default-parameter invocation failure retained, validator source/.git unchanged. Final Console9errors =7expected registration/rest negative fixtures +2composited screenshot-tool errors; MinMaxAABB0/overlap0, do not claim Console0.

Scene NTSD_Battle remains loaded, isDirty=false/root13; SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Existing user modifications preserved; no unexpected script paths or staged files. No schema/NTSDSpec/Gen/Plugins/input sampling/content/Scene changes, no git add/commit/push. These facts close only Goal19 scope, not full battle parity or outstanding content strategy.

Authoritative batch evidence index: Temp/Goal19_FinalSummary.json; raw RED/focused/shared XML, Play comparisons, screenshots, Console, build and validator files linked there. Earlier PLANNED/CODE_WRITTEN/RUNTIME_PENDING entries are superseded by this closure, while their failures/corrections remain preserved.

Goal20 R1 `NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001` / IN_PROGRESS / TEST_FIRST. Contract and Record established before scripts; exact scope in docs/ai/TASKS/NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001.md. R2 waits for R1; R3-R5 preflight only. Evidence Temp/Goal20_*. Schema/carriers preserved; no Git mutation.

Goal20 R1 CODE_WRITTEN/focused10of10 (new5+kind5existing5), runtime/shared pending. R2 `NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001` IN_PROGRESS/TEST_FIRST; exact Task/Record created before scripts.

Goal20 progress: `NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001` FOCUSED_TEST_PASS (new5+kind5existing5); `NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001` FOCUSED_TEST_PASS (new6+G16P3existing140). R1RED3FAIL after2reachablePASS; R2 correctedRED6FAIL. Play/shared/build/validator pending.

Goal20 `NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED. Exact Task/Record created; main agent controls Unity serial validation. Schema/reserved shape unchanged. Evidence Temp/Goal20_*.

Goal20 `NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED. Exact Task/Record created; main agent controls Unity serial validation. Schema/reserved shape unchanged. Evidence Temp/Goal20_*.

Goal20 R1/R2 RUNTIME_PENDING solely for batch shared gates; focused10/146PASS and targeted PlayPASS (G16 two actual pickup witnesses, current51/279->213/0 OPoint relationship seam, fulltick/unregister reserved0/null, objects4->4). Temp/Goal20_R12_PlaySummary.json defines precise boundary; no full skill/keyboard claim.

Goal20 R5 `NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED; Task/Record and fresh52production references/22files callgraph Temp/Goal20_R5_Callgraph.json established. True lifecycle defaults99/-1 retained, root-copy/stage/held/legacy-holder stats behavior retired only after RED. Main owns integration and Unity serial execution.

Goal20 R1-R5 RUNTIME_PENDING for shared batch gates. R1focused10/R2focused146/R3focused5/R4focused13/R5focused157 all PASS; R3 native normalized firstDifference=null and currentPlayPASS, R4 currentPlay4PASS only ReleaseTick differs, R1/R2 targetedPlayPASS. Shared1824 planned once (B6 1735+37), fullSelfCheck/two builds/finalvalidator pending.

Goal20 final package statuses:

- `NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 3FAIL after factory reachability2PASS; focused10/10.

- `NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 6FAIL; focused146/146.

- `NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 3FAIL/2PASS; focused5/5; native normalized firstDifference/firstChecksumDifference/firstMotionDifference null.

- `NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 9FAIL/4PASS; focused13/13; current Play4/4 only retired carrier differs.

- `NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 7FAIL/1PASS; focused157/157; fresh production references52/22files to29/13files, only defaults/carrier/diagnostics remain.

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.

NTSD28-Q06-TYPE3-BDEFEND-EDITOR-ORACLE-001 VERIFIED / TEST_ORACLE_ONLY (13/13 PASS)。两条旧type3 Editor字段观测修订；原失败保留，生产不改，详见同ID Record。

NTSD28-Q06-NATIVE-INPUT-MISSING-STATE-ROUTING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：准确Change Record已建立，Tools+单测试+BCAW限定范围，生产尚未改。

NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确Record已建立，生产未改；三BCAW符号及单Source/Editor测试。
> 当前BATCH-04/Q07 `NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001`仅资源暂存VERIFIED：正式对象DAT330、对象引用PNG1010和catalog/index/fusion三文件共1343/46,594,829逐hash一致；GameConfig未切换，Unity candidate/实际加载/Play待验，Q07未交付。见Q07 READINESS.md/Task/Record。Q06本地出口保持，Q08～Q12/R及例外不变；旧资源/Scene/非战斗未动，禁computer-use。
> 当前Q07 `NTSD28-Q07-WINDOWS-PLAYER-RUNTIME-001`限定VERIFIED_PLAYER_CONTENT_BOOTSTRAP_ONLY：真实Windows Mono构建0错、正式侧载1343、独立Player exit0/PASS，正式源与三owner/World4/关闭零残留通过；13条旧音频目录缺失留Q10。GameConfig生产空根、Q07切换及自然技能/表现仍待，Q06 DELIVERED_SCOPED保持。见WINDOWS-PLAYER-RUNTIME-ACCEPTANCE.md。
> 当前Q07 `NTSD28-Q07-PRODUCTION-CONTENT-ROOT-SWITCH-001` IN_PROGRESS：修改前Task/Change已建，准确GameConfig单字段+既有Development Player测试脚本序列化根模式；目标默认正式DAT/角色图片，不改Scene、旧资源或非战斗。
> 当前Q07 `NTSD28-Q07-SERIALIZED-MENU-CALLER-001` IN_PROGRESS：共享正式GameConfig的菜单预热caller聚焦验证；准确Task/Change已建，仅既有Editor Play探针单脚本，生产/Scene/旧资源不改。
> 当前Q07 `NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001` IN_PROGRESS：正式Naruto防前攻自然输入到OID33定向Play，修改前精确Task/Change已建，仅既有Editor物理键探针测试脚本；不改生产/Scene/资源，旧引用审计与Q06出口保持。
> 当前Q07旧资源动态审计：正式根下`GameDataManager`初始化仍读取旧`data.txt`，旧背景0条、正式data.txt背景24条；正式对象发布保留旧背景表，Battle准备的`RuntimeStageCount`静态路径为0，Q08结果阶段条件可能受影响。仅静态调用链/文件证据，正式playable与Unity同条件运行时待验；见`artifacts/diagnostics/NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001/DYNAMIC-REACHABILITY.md`。旧data.txt/DAT/图片删除授权0，Q06本地出口不变，Q07/总目标ACTIVE。
> 当前Q07 `NTSD28-Q07-NARUTO-CLONE-CENTRAL-PIXEL-WITNESS-001` 限定VERIFIED：前两次探针FAIL保留，fresh q07-clone-pixel-3与同轮物理键q07-naruto-clone-5真实Play PASS；tick12 OID33 pic1中央命令、生产相机全图2583/投影区1087非清屏像素。Editor退出、Battle Scene dirtyfalse/root14、双Scene SHA保持，Ledger651/13 PASS。仅单例中央命令到相机像素，EXE像素/排序/阴影/整技能和Q07出口仍待；详同ID ACCEPTANCE.md，禁computer-use。
> 当前Q07 `NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001` 限定VERIFIED：移除GameDataManager初始化旧`data.txt`隐式读取，空根显式加载保留；聚焦EditMode两次1/1、正式完整发布1/1、序列化根menu Play q07-lazy-menu-1 PASS（World4/三owner/关闭零残留）。最终空覆盖删除后重新编译/聚焦通过，完整发布/Play为该无行为改动前的等效路径；详ACCEPTANCE。Q08正式背景/结果计数待权威见证，旧资源删授权0，Q06本地出口保持。
> 当前Q08 `NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001 / IN_PROGRESS`：正式F4整应用关闭效果的精确Task/Change已建立，限定SimulationTickDriver、AppManager及定向Player探针/构建入口；脚本实施与Player验证待。Editor不得被F4退出，录像save-pending保护因Unity尚无对应owner不冒称已闭。Q07首Scene决定、Q08结果计数独立待办，Q06本地出口保持。
> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / FOCUSED_TEST_PASS_ISOLATED`：同SHA隔离Unity副本的正式PNG EditMode 1/1及预览类11/11 PASS，含旧BMP网格；XML已归档。原Editor外部Scene重载弹窗及可见预览报告/图片仍待，不能宣称预览或Q07整体完成。详同包PROGRESS；Q06限定出口保持，禁computer-use。
> Q08 `NTSD28-Q08-STORY-SELECTOR-RED-001 / FOCUSED_TEST_PASS`纠正：真实菜单未配置stage campaign，Unity MatchConfig没有正式story mission/child成对字段；此前4/1把StageProgressionValid当正式选关的测试预期不成立，原XML保留且已撤回缺陷解释。校正后隔离Unity 5/5PASS，Scene哈希不变；正式story selector仍未接，直接战斗的group/timing首差可继续。详STORY-SELECTOR-CALLER-CORRECTION。Q08未闭。
> Q08 `NTSD28-Q08-NATIVE-RESULT-CARRIER-001 / RUNTIME_PENDING`：正式组/预战斗计时独立载体及roster2/aggregate26/checksum29已写，聚焦隔离Unity 8/8、快照恢复/非法mask 1/1、相邻结果2/2+3/3、SelfCheck日志PASS。Q05目标schema断言PASS但全类2/6（clone缺场景1、旧raw47预期3），不能报全绿。原Scene SHA保持；真实战斗/关闭重进、>=144有效combatant继续输入、旧结果UI过早phase11激活与mode4 reserve仍待，Q08未闭。详NATIVE-RESULT-CARRIER-001/ACCEPTANCE-PENDING。
> 当前Q08 `NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001 / RUNTIME_PENDING`：正式结果>=144参战槽位held Attack/Jump当tick跳350已接入native载体；隔离Unity定向RED0/2→10/10、相邻3/3、SelfCheck日志PASS、原Battle Scene SHA保持。真实自然结果继续/原Editor、旧UI101及phase3跳战斗仍待；详ACCEPTANCE-PENDING。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-TRANSITION-FREEZE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：正式SHA文件匹配、隔离build exit0、mode4完整会话349→350→后继tick两次同SHA，所测实体字段/双RNG稳定且last_tick为空。仅mode4源码见证，Unity完整tick RED、普通state2/battle-only与host集成待；详TRANSITION-HOST-AUDIT REPORT。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-TRANSITION-FREEZE-UNITY-RED-001 / RUNTIME_PENDING_INTENTIONAL_RED`：mode4正式349→350→next冻结双跑已有；隔离Unity完整tick在350/transition202后命中FrameSequence首差预期349实际350，下一tick断言未达，生产未改。下一需结果页101/切场owner一致性方案及准确Task/Change，不能仅早退。详TRANSITION-HOST-AUDIT REPORT。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-ORDINARY-REMATCH-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：正式SHA文件匹配，独立C++ build/普通及battle-only完整会话双跑同SHA；两者350都不推进战斗，后继调用分别进selection/state1同World与重建战斗/原死者HP500。mode4 202独立，Unity host/结果UI101仍待。详TRANSITION-HOST-AUDIT REPORT；Q08未闭、Q06出口保持，禁computer-use。
> Q08范围纠正：按对齐总表§1.2/P-19/G-08，完整原生结果页与选人表现是用户批准排除项；旧Unity结果页phase11显示不单独构成必须改到101的视觉缺口。101逻辑结果记录、350切场/冻结和旧UI对战斗状态的影响仍须对齐/隔离。此前相反的“101 UI consumer”验收表述已由TRANSITION-HOST-AUDIT REPORT Scope correction取代；Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-RESULT-PAGE-COMBAT-INPUT-ISOLATION-001 / RUNTIME_PENDING`：正式350前combat输入已与旧结果页`IsActive`三处战斗门槛解耦；隔离Unity同tick RED0/1→结果场景4/4、相邻继续输入2/2 PASS。首次完整SelfCheck旧BATTLE-AUDIT7-F6预期失败留证，精确校正后fresh完整PASS，Ledger664/6PASS、Scene SHA不变。结果页post-world P1/P2保留；真实战斗与350切场host/冻结未验。详PAGE-COMBAT-INPUT-ACCEPTANCE；Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-NATIVE-TRANSITION-COMBAT-FREEZE-001 / RUNTIME_PENDING`：正式350切场当tick及后继旧战斗核心已冻结；隔离Unity新增RED0/2→目标3/3、相邻Q08/Results17/17、完整SelfCheck PASS，正式EXE/source/Scene SHA一致。driver的host tick/ApplyFrameInputSet和AppManager命令消费仍未接，不能称完整切场；详TRANSITION-COMBAT-FREEZE-ACCEPTANCE。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001 / RUNTIME_PENDING`：核心350冻结后driver自动/显式/暂停F2后继旧World tick与输入准入已守住；隔离Unity RED0/1→目标1/1、相邻25/25、完整SelfCheck PASS。AppManager普通选人/battle-only重开及28/128/202 route、真实Play/replay仍待；详HOST-TICK-ADMISSION-ACCEPTANCE。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-ORDINARY-RESULT-SELECTION-HOST-001 / RUNTIME_PENDING`：普通命令2已由driver主线程交AppManager有序卸载→现有选人；临时Scene Play三分支3/3、相邻28/28及完整SelfCheck通过，Ledger通过，Scene SHA不变。原Menu→Battle端到端因BuildSettings空表仍待；battle-only和28/128/202未路由，Q08未闭/Q06出口保持，禁computer-use。详同ID Record及`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/ORDINARY-SELECT-ACCEPTANCE.md`。
> Q08 battle-only只读owner审计：正式EXE/game_session/main新鲜SHA、显式启动位、Unity直接Battle Scene的`BattleTestBootstrap`/AppManager状态已核对；首次正式源码重建后battle-only位写false，既有夹具未覆盖第二次结果。下一先做第二轮native动态见证与直接Scene Unity trace，随后建立显式模式/RNG和输入相位重开合同；详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-OWNER-AND-SECOND-CYCLE-AUDIT.md`。Q08未闭，Q06出口保持，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001 / IN_PROGRESS`：独立Task/Record已建并登记，拟只在隔离原生测试树和新诊断夹具加入第一次重建后的第二次结果见证。J:正式源/Unity脚本/Scene/资源不改；双跑前不把源码静态false写入当实测第二轮行为。Q08未闭，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：J:正式`game_session.cpp`与隔离源SHA一致，focused build exit0、两次完整会话输出同SHA；第一次battle-only结果后直接重建且位变false，第二次350切场后进普通selection/state1，World及原死者HP0保持。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/SECOND-CYCLE-SOURCE-WITNESS.md`。Unity显式owner/首重开及EXE可见第二轮未验；Q08未闭、Q06出口保持，禁computer-use。
> `NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001 / IN_PROGRESS`：准确测试Task/Change先建；下一仅在隔离Unity真实Battle Scene Play观测`BattleTestBootstrap`与结果命令2冻结/待处理，不改生产/Scene/资源。首轮重开需后续独立显式mode/RNG/input合同，Q08未闭，禁computer-use。
> `NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001 / VERIFIED_DIAGNOSTIC_ONLY`：隔离Unity真实Battle Scene D3D11 Play1/1，正式内容bootstrap完成；AppManager `[TestBootstrap]`为MenuMain、无Menu Scene，注入native命令2后五帧旧tick不增、命令保留。前两轮因短等待/NullGfx4096阻断非逻辑首差；原Scene SHA不变。详DIRECT-BATTLE-RESULT-PLAY-ACCEPTANCE。首轮rematch仍待显式mode/RNG/input合同，Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001 / IN_PROGRESS`：准确诊断Task/Record已建；首次rematch的NativeRandom两路及input phase在restore和默认BGM draw边界需聚焦实测，不把Unity现有空World重建/seed重置当正确实现。J:正式源/Unity生产/Scene/资源不动，Q08未闭，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：隔离聚焦build0、正式runtime双跑exit0/同SHA；首轮重开CRT及input phase保留，默认BGM在恢复后同步抽取一次。仅原生source模型，Unity host与正式EXE可见行为未验；详Q08 RESULT-TRANSITION-HOST-AUDIT-001/RNG-PHASE-SOURCE-WITNESS.md。Q08未闭，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001 / IN_PROGRESS`：直接Battle Scene首轮重开真实Play RED Task/Change已先建，仅新Editor测试路径。源双跑已证首轮重开/双RNG+输入阶段，现有真实Play证命令2待处理。下一在隔离Unity聚焦运行，生产尚未改；Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001 / TARGET_RED_CONFIRMED`：隔离真实Battle Scene D3D11 Play在正式内容bootstrap后目标断言失败，XML total1/passed0/failed1，命令2之后旧World 15秒同实例；无CS/NullGfx阻断、Scene SHA保持。报告Q08 RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-REMATCH-UNITY-RED.md。新roster/tick断言未到达，生产host未改；Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001 / IN_PROGRESS`：首轮直接战斗重开生产Task/Change已先建，准确driver、direct bootstrap、目标RED与旧诊断测试4路径。源双跑RNG/次轮及Unity真实Play目标RED已归档；当前尚未改生产。守十一阶段关闭/map真清/同Scene角色再生/随机与输入phase恢复后BGM一次抽取，普通Menu保持；次轮无Menu选人另依赖。Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001 / RUNTIME_PENDING`：隔离真实Battle Scene正式内容首轮自动重开与同步RNG/input/旧World清空Play2/2、修订旧诊断1/1、相邻普通路由3/3、完整SelfCheck PASS，原/克隆Scene hash不变。源双跑及改前RED保留，报告Q08 RESULT-TRANSITION-HOST-AUDIT-001/FIRST-REMATCH-HOST-ACCEPTANCE-PENDING.md。次轮无Menu选人、自然KO/EXE可见仍待；Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-SECOND-BATTLE-ONLY-RESULT-UNITY-RED-001 / IN_PROGRESS`：次轮battle-only结果真实Scene Play测试Task/Change已先建，仅新Editor测试路径。正式source双跑证首轮一次直重开后次轮350→下步普通选择transition1/同World；Unity首轮限定通过，次轮目标RED待。Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001 / IN_PROGRESS`：次轮direct结果状态2→1准确Task/Change已先建，仅driver/bootstrap/新测试3脚本。正式source双跑同World/transition1，Unity临时host目标RED2≠1；正式内容真实Scene两次图片加载OOM，未得逻辑结论。先接逻辑边界再聚焦代理/相邻验，真实Play仍待；Q08未闭/Q06出口保持，禁computer-use。
> Q08第二轮直接战斗结果 `NTSD28-Q08-SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001 / RUNTIME_PENDING`：正式source二轮同World进入selection/state1；Unity显式direct owner第二轮命令2→1已写，隔离无图临时host目标RED→Play1/1 PASS，普通Menu+Battle相邻3/3 PASS。正式内容Battle Scene两次图片加载OOM，无第二轮真实场景、自然KO或选择前端验收；详Q08 RESULT-TRANSITION-HOST-AUDIT-001/SECOND-BATTLE-LOGICAL-SELECTION-ACCEPTANCE-PENDING.md。Q08未闭，Q06限定出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001 / PLANNED`：Q08正式资源Scene两次在atlas页Texture2D创建处OOM、无第二轮逻辑XML；仅opt-in诊断`BattleAtlasResources.cs`的准确Task/Change已建，拟记录页数/array决策/页进度并外部采样进程内存。尚未改脚本或再跑Scene；Q08未闭，Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001 / RUNTIME_PENDING`：隔离正式内容Scene opt-in trace得145张2048²页、array 2.43GB超512MiB预算拒绝、第87页完成/第88页创建OOM，进程私有字节两秒采样峰值18.0GB；无XML/结果逻辑结论。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ALLOCATION-TRACE-RESULT.md。仅一脚本诊断开关，下一另立资源生命周期/环境Task；Q08未闭、Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001 / PLANNED`：已按145页/第88页OOM证据建立准确两脚本Task/Change；仅当array被capability policy拒绝时拟逐页CPU拼装/上传，保持同plan/像素/绑定/array fallback。尚未改生产；先多页等价测试后正式内容Scene+内存采样。Q08未闭，Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001 / RUNTIME_PENDING`：仅array拒绝分支逐页CPU拼装/上传，原数组路径保留；两页逐像素等价及绑定聚焦3/3 PASS。正式内容Scene第88→119页、采样峰值18.02→17.40GB但仍OOM/无XML；需独立source texture/atlas双重驻留策略与真实表现验收，不能关闭Q08。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ORDERED-STREAMING-PARTIAL.md。Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-BUDGET-SOURCE-BINDING-001 / PLANNED`：正式Auto145页2.43GB超既有AtlasMemoryBudget512MiB，ordered逐页后仍第119页OOM；准确policy/manager/atlas测试3脚本Task/Change已建。拟Auto超预算保留现有SourceTexture2D中央绑定，显式atlas模式与小内容保持；未改生产，需源绑定/排序/像素与真实Scene验收。Q08未闭、Q06出口保持，禁computer-use。

> 本项进展修正为`RUNTIME_PENDING`：三脚本已实施；隔离Unity聚焦4/4、正式内容D3D11 Battle Scene第二轮1/1且进程exit0，私有字节采样峰值13,242,007,552。早期统一入口遗漏所致RED3/4保留。代表像素/绘制顺序及正式EXE可见画面仍待，不关闭Q08或重开Q06。详Change Record及Q08报告，禁computer-use。

> `NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001 / PLANNED`：正式内容SourceTexture2D回退的结构绑定/顺序与结果流已验，真实相机像素仍待。单个战斗Play测试的Task/Change已建，尚未改脚本；只验证实际生产相机出图及政策/指纹，不冒称正式EXE逐像素一致。Q08/总目标未闭、Q06本地出口保持，禁computer-use。

> Q08正式源纹理相机见证（2026-09-22）：`NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001 / RUNTIME_PENDING`在隔离非batch D3D11 Editor NUnit1/1PASS，正式指纹、Auto145页2.43GB超512MiB预算、SourceTexture2D、中央tick34/4命令及960×540图2015非白像素有证；原项目现有Editor无第二进程亦产生同hash PNG/JSON，原NUnit回调结果未落盘，不能报原Editor测试PASS。原项目同时Q07物理Naruto探针首键8次未入FrameInputSet而FAIL，与源纹理像素分开审。Scene磁盘SHA保持；正式EXE像素/全角色与技能未验，Q08/总目标未闭、Q06本地出口保持。详`artifacts/diagnostics/NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001/ACCEPTANCE-PENDING.md`，禁computer-use。

> NTSD28-R15-VERSIONED-TRACE-HEADER-001 / IN_PROGRESS（2026-09-22）：修改前准确Task/Change及Ledger已建立，限定Unity trace emitter、parity validator/self-test、两项正式根Editor断言。目标是V1/V2严格头与17/25/28/2/2、17/26/29/2/2精确schema身份；native V2诊断捕获、原项目Unity编译/运行及同seed/input验收仍另待。Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-VERSIONED-TRACE-HEADER-001 / CODE_WRITTEN（2026-09-22）：Unity emitter严格分V1无mode及V2五组件，parity工具严格验证版本/哈希/旧17-25-28与当前17-26-29 schema并在header拒绝跨版本；Release build0错、自测157/157、Ledger682通过。正式根Q05/Q06两Editor断言已迁V2，review发现的fallback跨V1/V2弱断言已纠正；原项目Unity程序集仍旧，五脚本未在原Editor编译/运行。native source-model仍V1/25/28、正式新V2双端同seed/input比较未做，另有固定V1 Play探针需独立回访；Q07/R15未闭，禁computer-use/第二Unity项目。证据artifacts/diagnostics/NTSD28-R15-VERSIONED-TRACE-HEADER-001/parity-self-test.json及同ID Change Record。

> NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001 / IN_PROGRESS（2026-09-22）：修改前准确Task/Change和Ledger已建，限定九个战斗验证脚本十处正式根旧V1固定身份文字为当前V2独立向量；不改输入/行为/Scene/生产资源/非战斗。原Editor编译/探针待，Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001 / CODE_WRITTEN（2026-09-22）：九个战斗测试/探针十处正式根固定V1值精确改为已复核V2 semantic FF1218FF...及projection 9F40EB3FFF1812FF；旧值源码搜索0，diff仅十行预期文字。纯源正式/暂存向量检查PASS，原Editor程序集/探针仍未编译运行。native V2 source-model下一包的只读准确边界见artifacts/diagnostics/NTSD28-R15-NATIVE-MODE-CAPTURE-CONTRACT-001/REPORT.md；正式源码不改、旧native V1捕获不重标。Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001 / IN_PROGRESS（2026-09-22）：修改前准确Task/Change及Ledger已建，唯一代码路径为Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp；正式playable源码/EXE只读，旧V1 JSONL不重标。目标新鲜source-model正式mode双DAT五组件V2、实际session.config完整核对、17/26/29当前trace兼容声明及bundle失败门槛；原项目Unity编译/同条件双端仍待。见同ID Task和R15 NATIVE-MODE-CAPTURE-CONTRACT报告。Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY（2026-09-22）：仅仓库诊断runner读取正式mode父/子DAT并核对实际GameSession完整combo配置；未改J:正式源码/Unity生产/Scene。final-build 0错，正式3tick主/domain/B2双跑各逐字节同；五组件V2原始/语义/投影与正式及暂存Unity纯投影一致，三个validator PASS。无mode V1保留；缺子/残缺/初始化及运行中路径、优先级、字节变化均失效并标三路bundle。证据artifacts/diagnostics/NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001/及同ID Record。仍仅SOURCE_MODEL_DIAGNOSTIC_ONLY；原项目Unity新程序集/同seed双端tick首差/正式EXE可见表现未验，Q07/R15未闭，禁computer-use/第二Unity项目。
> `NTSD28-Q09-WORDS-INPUT-IDENTITY-001 / PLANNED`：已建立精确Task/Change/Ledger，拟只在视觉候选中版本化正式resource.dat索引16..21及六图指纹/新鲜度，不改规则身份或菜单；脚本尚未修改。原Editor新程序集仍待。
> `NTSD28-Q09-WORDS-INPUT-IDENTITY-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS / ORIGINAL_UNITY_PENDING`：按预先Task/Change仅改视觉候选与新聚焦测试/meta，正式resource.dat索引16..21及六图进入可选V2视觉身份/新鲜度；无resource保留V1。原项目离线runtime/Editor编译0错、compiled actual projection正式/暂存同指纹及四夹具反例PASS；原Editor NUnit/Play、实际WORDS发布/画面待。
> `NTSD28-Q09-WORDS-PUBLICATION-001 / COMPILE_PASS / FOCUSED_RESULT_PENDING`：脚本前Task/Change/Ledger已建；仅改`CharacterAnimtorManager.cs`加新聚焦Editor测试/meta，正式6表经现有预热/目录/atlas/所有权原子发布。原项目Editor最终测试程序集08:11:44Z晚于源码且重载成功；一次性测试请求已消费但结果文件未产生，不能判PASS/FAIL。先查同一运行终态，避免并发重跑。alpha/画面、取消、关闭仍待；详本包`ORIGINAL-EDITOR-RUN-PENDING.md`。`I:\UnityPreject\test`不用于本目标；禁第二Editor/computer-use。
> Q08 `NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001 / CODE_WRITTEN`：正式其余三处KO生产者已在原有单次统计旁追加逐次事件，shared writer保留无源slot的-1/1000默认值；现有三组聚焦fixture已加事件断言。Task/Change均在脚本改前建立；Ledger PASS，原项目Temp-only runtime+Editor离线编译0错且测试入编译输入。原Editor编译/NUnit、SelfCheck、同seed、Play与关闭待验。原Editor WORDS测试终态未明，不并发重发；仅原项目、禁computer-use/第二Unity。
> Q08/Q09 `#killtext`寿命交接只读审计：正式配置存在时即便bound0也按`times`裁剪，配置缺失则不裁剪；Unity现每tick固定70是具体选中子表适配，不是通用规则。Q09先按mode双DAT身份发布可选完整配置，Q08再消费寿命；详`artifacts/diagnostics/NTSD28-Q08-Q09-KILLTEXT-LIFETIME-HANDOFF-AUDIT-001/REPORT.md`。无本轮脚本改动/新运行验证，Q08/Q09仍开放；原Editor PID33236程序集仍旧、WORDS请求无结果。
> Q09 `NTSD28-Q09-KILLTEXT-MODE-INPUT-001 / CODE_WRITTEN`：`#killtext`完整输入已从mode双DAT已捕获child字节投影，缺配置与bound0区分，重复mode/id、七图路径/双sound保留，既有V2 raw身份不变。原项目Temp-only离线runtime+Editor编译0错，新测试入输入；反射直调编译后parser正式值/反例通过。原Editor NUnit、图片身份/发布、World tick与画面待；WORDS请求无终态，不并发TestRunner/第二项目/computer-use。
> Q08/Q09 `NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001 / CODE_WRITTEN`：已从已发布mode child首tick前接World可选寿命，Q08结果后仅配置存在时按该值裁剪；reset/core snapshot13/aggregate28/lockstep checksum31/Unity extended及lockstep parity v3已接，frozen Authority400 v3不变。原项目Temp-only离线runtime+Editor编译0错，Ledger PASS；原Editor NUnit/SelfCheck/同seed/Play与R15 schema复核待。WORDS旧请求无终态，不并发TestRunner/第二Unity/computer-use。
> Q09 `NTSD28-Q09-KILL-ICON-INPUT-IDENTITY-001 / COMPILE_PASS`：正式mode七槽图标路径/存在字节入V3视觉身份与发布前复核，无feed保留V1/V2；compiled正式/暂存指纹相等。原Editor 09:44Z成功编译且导入新测试，但域重载/聚焦NUnit/全候选/发布画面仍待；旧WORDS TestRunner已RunStarted无结果，不并发重发。Q09/R17开放。详本包ACCEPTANCE-PENDING；此前CODE_WRITTEN是历史快照。
> Q09击倒提示下一接线依赖（2026-09-22，只读）：正式10槽名字/8括号标志不同于Unity本地4名字；原Scene HUDCamera Canvas与world base camera中央渲染分属不同表现入口，直接加entity command不能证明正式HUD后层级。先建battle-only名字输入与可冻结行快照，再用原Scene定向坐标/层级probe决定screen consumer；不改GameLocalSettings、Canvas现有HUDBg x30或非战斗UI。详Q09 ROW-PROJECTION-AUDIT报告追加章节；运行/像素待。
> Q09 `NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / PLANNED`（2026-09-22）：已在脚本修改前建准确Task/Change/Ledger，限定MatchConfig战斗名字输入、Host已发布feed交接、Presentation帧/新投影/聚焦测试五路径；当前**尚未改脚本**。正式行顺序、+29/+30/+31和缺失/排除行距、名字/字节宽度、worker/frozen-copy已列验收。原Editor WORDS测试已启动但终态未证，先不并发重跑；Q09图文/原EXE像素仍待，原项目/无computer-use。
> Q09 `NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS`（2026-09-22）：按上述改前Task/Change五路径写入战斗名字/显示flag、mode feed首tick交接、有序关闭清除、正式行投影与冻结帧复制/worker发布；新聚焦测试覆盖窗口边缘、排除/缺actor、名称与门槛等。原项目Temp-only targets离线runtime+Editor编译0错，`git diff --check`通过；**原Editor程序集仍旧，focused NUnit、原EXE同tick和实际图文/像素均未验**。WORDS旧运行无结果，未并发发新请求；Q09/R17仍开。详同ID ACCEPTANCE-PENDING。
> Q09原Editor状态与screen层级回访（2026-09-22 18:20）：本地桥端口6404可回答，但`get_editor_state`连续两次返回09:44:49 UTC、`sequence=9`旧快照；不能以其中`tests.is_running=false`判旧WORDS测试已终态。PID33236活、结果文件缺、DLL仍17:44；未发新TestRunner。Scene YAML只读映射HUDCamera深度0、world base深度-1、Canvas子序HUD→ControBg/Combo/按钮/HP；HUD后控制前的battle-only层是待Play验证候选，不修改用户HUDBg x30或Scene。详WORDS ORIGINAL-EDITOR-RUN-PENDING和ROW-PROJECTION-AUDIT，Q09/R17运行验收待。
> 2026-09-22 Q09行快照名字输入修正：`NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS`，正式playable 10槽、≤10字节/项、无NUL约束已写入原项目战斗投影及聚焦反例；diff check与Ledger通过。原Editor NUnit/正式同tick/屏幕行未验，Q09/R17及总目标开放。详同包ACCEPTANCE-PENDING；原目标项目仅`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。
> 2026-09-23 状态更正：当前 Win32 进程中原项目没有运行中的 Unity Editor；下面 PID33236/导入 worker 的描述均为 2026-09-22 历史状态。运行中的 `I:\UnityPreject\test` 属独立项目，不作为原项目验收。Q09 图标当前修订仍仅离线编译通过、原 Editor 编译/聚焦测试待。Q10 新增只读首差审计：正式 GameSession 在本 tick 新击倒事件后、裁剪前追加 mode 音频；Unity 已捕获 `sound1/sound2` 但未生产该事件，详 `NTSD28-Q10-KNOCKOUT-AUDIO-HANDOFF-AUDIT-001/REPORT.md`。Q09/Q10和总目标开放。
> 2026-09-23 Q07原项目Scene入口：用户确认Menu index0、Battle index1；仅`ProjectSettings/EditorBuildSettings.asset::m_Scenes`两条记录。原Editor刷新后真实Menu回调Play `q07-menu-scene-closure-2.json` PASS：正式Naruto OID2进入Additive Battle，三发布方一致，退出回Menu且Stopped/池借用0；两个Scene SHA保持。首次未刷新预检FAIL保留。`NTSD28-Q07-PRODUCTION-SCENE-LIST-001 / FOCUSED_TEST_PASS`；Player冷启动/Android/完整Q07仍待，不使用独立`I:\UnityPreject\test`。

> 2026-09-23 Q07 `kind.dat`后继只读合同审计完成：正式EXE/SHA、playable闭包和已暂存229字节DAT核对。正式候选过滤要求目标type3且非kind9，Unity固定值辅助函数缺targetType；正式type3变身与Unity常量匹配当前单记录，但parser、输入/语义指纹、发布新鲜度及两consumer尚无生产接线。正式catalog 330对象无OID209，因此默认实战首差未证。详`NTSD28-Q07-KIND-DAT-READER-AND-IDENTITY-CONTRACT-AUDIT-001/REPORT.md`；下一先建精确parser/identity Task/Change，不改非战斗或旧资源，Q07保持开放。

> 2026-09-23 Q07准确新脚本前置：`NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001 / IN_PROGRESS`，Task/Change先建，仅四个新脚本路径（kind模型、parser、选源输入、聚焦测试），不改既有固定值consumer/身份/Scene。正式kind.dat已同SHA暂存；本包编译/聚焦证据待，Q07仍开放。

> 2026-09-23 Q07 parser/选源限定完成：`NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001 / FOCUSED_TEST_PASS`，原项目Editor PID173216编译后首轮job d36a4356151c4e779244b16fd5091f5b 13/13PASS、最终job 803b671356754cc0b1c7ff170719e493 24/24PASS，正式暂存kind.dat和缺失fallback语义相等、输入身份不同；畸形/选源/100记录/新鲜度有覆盖。四meta GUID唯一、双Scene SHA保持。尚未纳入联合内容身份、prepared World、候选/变身consumer及R15，Q07/总目标开放；下一精确identity/publication Task/Change。
> 2026-09-23 Q07 kind 身份/World下一独立包：`NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001 / PLANNED`已先建Task、Change与改前审计，七组件V3、旧V1/V2保留、候选新鲜度、prepared World封存与trace版本门槛明确；脚本尚未改。正式native源模型仍V2，V3同seed比较待独立后继。禁computer-use/第二Unity项目；Q07/R15开放。
> 2026-09-23 Q07 kind 身份/World子包 `NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001 / FOCUSED_TEST_PASS`：原Editor编译，Kind七组件V3/无mode KIND_ONLY、候选新鲜度和World封存已接，定向3/3+16/16+17/17、parity自测162/162、Ledger通过；Menu/Battle Scene SHA未变。V2作为历史向量保留，正式native仍V2，正式Battle Play、同seed trace、kind候选/变身consumer及Q07整体未验。旧Q06生成JSON被测试写为当前V3指纹，保留并在Record标明。下一先接正式kind战斗consumer，再做R15 native V3同版本比较；禁computer-use/第二Unity项目。
> 2026-09-23 Q07 kind V3真实菜单发布补证：原Editor `q07-kind-v3-menu-smoke.json` PASS，V3正式指纹随Menu预热→Naruto OID2→Additive BattleRunning，三owner key相同，退出Menu后Stopped/借用0且Scene SHA保持。该Play见证发布链，不证明World kind consumer对齐、Player冷启动或native V3同seed。`NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001 / FOCUSED_TEST_PASS`，Q07/R15/总目标开放；下一kind候选/变身consumer。
> 2026-09-23 Q07下个脚本包 `NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001 / PLANNED`：改前Task/Change已建，准确范围为kind锁定fallback、World只读规则、BruteForce候选、BattleDamageWriter type3变身、ECS投影和B5聚焦测试。正式目标type3/effect/respond/kind9门与记录顺序/frame零→40已从playable核对；当前默认330对象不含209，必须用改表/受控类型对证明，不以默认Scene无差断言闭合。原Editor仅一实例；禁computer-use/第二Unity项目，Q07/R15开放。
> 2026-09-23 Q07当前游标：`NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001 / FOCUSED_TEST_PASS`。正式kind表已由prepared World或精确锁定fallback进入候选门和type3变身/ECS投影；原项目Editor编译，聚焦B5 10/10、ECS ShadowCompare 2/2 PASS。缺帧999经有效RED后已按正式源码保留原始动作写入。默认正式catalog无OID209，受控kind依赖真实Scene、Player冷启动及R15 native V3同seed trace未验；Q07/总目标仍开放。下一先做R15 native V3源模型与双端trace，再做kind依赖Scene证据；详Change Record及对齐总表0.11游标。禁computer-use/第二Unity项目；用户既有dirty与保存Scene保持。
> 2026-09-23 R15下一包 `NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001 / PLANNED`：已建独立Task/Change，准确只改仓库诊断runner；正式host选择kind、Unity V3身份及当前V2-only首差已核。下一先做runner改动和正式内容新捕获，再补Unity同seed双端trace；Q07/R15/总目标开放，禁computer-use/第二项目，保留dirty与历史捕获。
> 2026-09-23 R15新状态 NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001 / FOCUSED_TEST_PASS：正式源码模型新V3双跑逐字节一致、三路source validator通过；原Editor同seed3tick raw300/300、B2联合流相等。B0 Unity tick多aiAcceptedTrace而严格validator报tick-properties，domain未比较。正式kind依赖Scene、Player冷启动、更多场景和正式EXE行为仍待，Q07/R15/总目标开放。下一先处理B0契约/producer归属，再做kind依赖真实战斗；详artifact REPORT及对齐总表0.11游标。原Scene SHA不变、禁computer-use/第二项目。
> 2026-09-23 R15 B0下一包 NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001 / PLANNED：当前正式/Unity同seed B0比较首阻为Unity专属aiAcceptedTrace额外字段；已建准确工具两文件Task/Change，正式共享六字段和旧v1保持，先聚焦RED再严格校验扩展并复比。Q07/R15/总目标开放，禁computer-use/第二项目。
> 2026-09-23 R15 B0工具包 NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001 / FOCUSED_TEST_PASS：只对Unity生产者可选aiAcceptedTrace做严格五字段校验，正式B0共享六字段及旧捕获不变；RED15例1失败后最终16/16。原同seed3tick B0共享input/slots/lifecycle相等、RNG source/Unity流拓扑不同；不能宣称RNG等价。Q07/R15/总目标开放，下一Q07 kind依赖真实运行及更广trace；详REPORT/Record和总表0.11游标。禁computer-use/第二项目。
> 2026-09-23 Q07定向包 `NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001 / VERIFIED`：仅新增原项目Editor Play诊断脚本；原Editor编译后，保存Battle Scene正式selected kind表 effect209/frame40、临时type3 OID213→206在完整driver tick5→6触发目标OID206→213/action40/队伍owner和锁存转移，报告PASS。注销后World4/池借用2保持，Editor返回EditMode，双Scene SHA未变。此为受控合成对，不是自然技能/Player/正式EXE画面对齐；Q07/总目标开放。详Task/Record/result.json。
> 2026-09-23 R15输入分支 `NTSD28-R15-STANDING-ATTACK-V3-TRACE-001 / VERIFIED_SCOPED_TRACE`：正式源码模型双跑同SHA三流，原Editor正式根同seed J攻击三tick raw300/300零差、B2相等，tick2动作65及同步RNG 0x82/2/result1对齐；B0共享input/slots/lifecycle相等、RNG拓扑不同。只产生诊断文件，无代码/Scene/资源改动；source-model certificate false，Player冷启动/kind依赖trace/正式EXE可见行为待，Q07/R15/总目标开放。详同ID REPORT。
> 2026-09-23 Q07 Player内容闭包前置 `NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001 / COMPILE_PASS`：原项目暂存正式根1371非meta/46883057字节逐项对正式同路径SHA一致，旧Windows后处理器仍按1343/46594829硬门，当前Menu-first Player构建会被拒。已在修改脚本前声明只新增冻结V2 manifest并调整该后处理器3常量；旧manifest/Scene/菜单/战斗生产不改。编译/Player待，Q07开放。
> 2026-09-23 R15 B0 v2诊断契约包 `NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001 / IN_PROGRESS` 已在脚本修改前建立Task/Change/Ledger。仅声明四个Tools诊断文件，v1同epoch改ID拒绝保持，v2拟增严格`object-id-change`事件并拒绝混版比较；producer与Unity战斗代码不在此包，Q07/R15/总目标仍开放。详对应Task/Record。
> 2026-09-23 R15源码模型v2采集包 `NTSD28-R15-B0-SOURCE-V2-PRODUCER-001 / IN_PROGRESS` 已在脚本修改前建立Task/Change/Ledger，仅声明诊断runner一文件；默认v1保持，显式v2将记录同epoch对象ID变化。待编译/双跑/校验，Unity producer与同场景对照仍未做；Q07/R15/总目标开放。
> 2026-09-23 R15 Unity B0 v2采集包 `NTSD28-R15-B0-UNITY-V2-PRODUCER-001 / IN_PROGRESS` 已在脚本修改前建立Task/Change/Ledger，仅声明Editor采集与聚焦测试两文件；默认v1保留，显式v2严格记录同epoch ID变化。type3/action场景和跨端对照仍是独立后继；Q07/R15/总目标开放。
> 2026-09-23 R15 `NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001 / IN_PROGRESS`：已在脚本修改前建立Task/Change/Ledger，精确只声明Unity Editor raw capture及其聚焦测试两脚本。正式场景OID213/action176与OID206/action0均为type3；当前捕获器强制LF2Character且忽略action，是同seed kind消费trace的初态阻断。下一实施真实type3诊断shell与action初始化，然后原Editor聚焦测试及main/B2/B0首差对照；Q07/R15/总目标开放。
> 2026-09-23 R15 `NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001 / FOCUSED_TEST_PASS / FIRST_DIFFERENCE_FOUND`：原Editor诊断捕获编译、定向3/3，正式源码模型及Unity同场景main/B0v2/B2三tick严格有效。tick1正式仅slot1 OID213/action40，Unity额外slot50 OID206/action40；B0独有birth/B2实体数1对2。下一个生产候选是non-character hit_Fa7错误克隆及运动/顺序全链，须独立Task/Change与原Editor受影响分支验收；正式EXE行为未证，Q07/R15/总目标开放。
> 2026-09-23 Q07下一准确生产包 `NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001 / PLANNED`：已按R15首差先建Task/Change/Ledger，只声明LF2Entity与新聚焦测试。正式non-character behavior7缺预指定目标时不克隆，目标存在时完整运动/动作顺序待RED和实现；当前生产脚本尚未在此包修改。Q07/R15/总目标开放。
> 2026-09-23 Q07 `NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001 / FOCUSED_TEST_PASS`：原Editor聚焦RED2/2→最终GREEN2/2、相关非角色/AI回归2/2。独立review发现的float常量和过早整数Y同步已修，角色路径未改。冻结kind/type3同seed三tickmain150字段0差、B2相等、B0共享域相等且RNG拓扑仍不同；有目标完整tick previous-Y及正式EXE/Player可见对照待。双Scene哈希保持，Q07/R15/总目标开放；详同ID REPORT/Record。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001 / PLANNED`：正式OID875/action55的dvy1在AI与physics之间，当前局部AI→physics测试Y=-21.6不是真实完整tick预期。脚本前Task/Change/Ledger已建，仅声明仓库诊断runner opt-in与既有Q07 Editor测试；将用正式源码模型/Unity真实driver同初值捕获目标存在完整tick previous-Y和首差，不改正式源码/生产/Scene。Q07/R15/总目标开放。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001 / CODE_WRITTEN`：先前PLANNED说明中的“扩既有runner”在脚本前修正为新单用途runner，既有R15脏runner未改。正式源码模型Stage23合法三tick已编译捕获：tick1 action55/Vy5.2/YInt-20/previousY-30，tick2 action60/previousY-20，tick3 action61/实体3。原Editor首测因初版2tick/Z100/120违反诊断入口合同而在战斗前FAIL，夹具已同步修为3tick/Z600/620；Unity当前编译/重跑待Editor退出别的Play切换。Q07/R15/总目标开放。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001 / CODE_WRITTEN / FIRST_DIFFERENCE_FOUND`：正式源码模型OID875/action55、目标槽0、初始Vy3.8完整tick1为Vy5.2/YInt-20；原项目Unity真实Driver同初态为Vy6.2/YInt-19。初态及X/Z/action/目标槽一致，tick2 Y差为后继。静态调用链定位原生FrameMotion后type3 NativePhysics内旧非角色帧推进再次应用dvy1；未改生产。聚焦比较器已改数值比较但尚待重跑。一次桥接过滤参数误用触发全量EditMode job `807dbc7e717a4928bb4be19941856920`，现仍运行且有无关失败；等终态后只用`testNames`复跑目标例。Q07/R15/总目标开放，Q10局部成果保留。详本包REPORT/Record。
> 2026-09-23 Q07下一生产包 `NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001 / PLANNED`：已在生产脚本前建Task/Change/Ledger，精确只声明SimulationWorld原生物理pass的旧帧速度抑制，保持原生FrameMotion单一所有权。先等误发全量job终态并以正确`testNames`确认数值RED，再实施/跑狭窄回归；生产尚未改，Q07/R15/总目标开放。
> 2026-09-23 Q07最新 `NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001 / FOCUSED_TEST_PASS`：原Editor旧进程退出，误发全量job最后可见7706/8259、无终态，不作验收；新原项目Editor以正确testNames精确RED 0/1（tick1 YInt正式-20/Unity-19）→World原生物理pass抑制重复dvy后GREEN 1/1，相邻5/5。正式paired源码重建与同一冻结kind/type3三tick原Editor主raw150字段0差、B2相等、B0共享域相等且RNG拓扑差仍在；Scene双SHA不变。两Change Record已更新，生产仅SimulationWorld一方法、诊断测试仅二进制浮点容差。tick3 OPoint、EXE可见/Player及Q07/R15整体仍待；详本包REPORT。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001 / PLANNED`：脚本前Task/Change/Ledger已建；仅声明原Editor聚焦测试新增完整第3 tick诊断及Temp异常记录。源模型tick3 action61/实体3；Unity旧pool.Get异常尚不能证明缺Renderer，因为测试World已设logic-only。待新鲜精确testNames、完整栈和World前置，Q07/R15/总目标开放。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001 / FOCUSED_TEST_PASS`：原Editor精确第3 tick测试1/1、相邻两tick测试1/1。正式源码模型与Unity所比字段一致：OID875/action61、YInt-15、preciseY-15.4、previousY-15、实体3。先前pool.Get异常是无容量封存时StepOneTick清除logic-only；封存后旧EditMode池未Awake导致关闭失败，夹具补齐后有序关闭到ObjectPoolQuiesced、World/slots/borrowers均0。Console error0、双Scene SHA不变。仅改Q07测试脚本；正式EXE可见/Player及Q07/R15整体开放，Q10成果保持。详本包REPORT/Record。
> 2026-09-23 `NTSD28-USER-CAMERA-VIEWPORT-SCALE-001 / IN_PROGRESS`：用户看过鸣人48像素同tick位移与正式1333、Unity 16:9时2048视野对照后，要求按比例修正。先限定Battle背景表现相机视口与聚焦测试；不动逻辑速度、Scene、DAT，固定中心长跑边界单独评估。Task/Change/Ledger 已在脚本前登记。
> 2026-09-23 用户更正：`NTSD28-USER-CAMERA-VIEWPORT-SCALE-001 / ROLLED_BACK`。用户说的“按比例”是保留原相机尺寸、调整角色奔跑距离，先前误解为改相机。相机/测试两脚本inverse patch后Git diff为空，Battle Scene SHA不变。之前相机21/21仅是临时版本，不可作最终证据；跑距新方案须面对正式/Unity同tick逻辑位移已相等这一约束。
> 2026-09-23 D-024下一精确包`NTSD28-USER-OPOINT-BIRTH-RATIO-001 / PLANNED / EDITOR_JOB_PENDING`：正式playable OPoint子实体位置=父实体整数坐标+相对X/Y/Z偏移（Z含+1）；Unity late OPoint的World逻辑与组件`LF2ObjectPointFactory`两条实际materializer均仍加原始偏移，必须同步处理，不能整体放大最终绝对坐标。已先建并修正Task/Change/Ledger所有权，目标仅缩放late子实体相对X/Z出生偏移并同步precise/int，DAT/速度/Y/其他spawn不改。原Editor误发全量EditMode job `5192eb9e273741398e88a56379e3e9ff`仍未证终态，脚本与新测试待其结束后实施。总目标及D-024开放。
> 2026-09-23 用户螺旋丸时机优先包 `NTSD28-USER-RASENGAN-WINDOW-PARITY-001 / IN_PROGRESS`：脚本前Task/Change/Ledger已建，只声明新Editor诊断测试，用正式Logan内容和原项目Unity Driver跑241起始32tick、对照源码模型J攻击tick25/26边界。正式EXE可见/物理Play待；Q07新包暂后置，DAT不改。
> 2026-09-23 用户螺旋丸时机优先包 `NTSD28-USER-RASENGAN-WINDOW-PARITY-001 / FOCUSED_TEST_PASS / PHYSICAL_PLAY_PENDING`：原项目Editor job c8aa5cebb03e4879aa90beee82ec67f1 精确3/3、每组三十二完整tick；鸣人action/counter/MP与正式源码模型全同，J于场景tick25开始按2tick两端都在完成tick26转301、tick26才按均错过。太晚后OID434动作397在完成tick30运动X源0/Unity550，另入战斗对齐首差。夹具对象池owner修正后有序关闭对象/槽/borrower全0；真实物理输入及正式EXE可见未验，技能问题未收尾，Q07新包后置，DAT未改。
> 2026-09-23 此包末轮Ledger 716 Record PASS，Battle Scene SHA保持9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0、DAT/Scene工作区无任务改动。原Editor随后一次Play请求只见changing阶段及桥接陈旧快照，未运行物理按键探针；stop后新鲜idle EditMode。此为下轮新Play证据前的历史快照。
> 2026-09-23 鸣人持续D+K落地奔跑局部Play出口：原Editor日志显示Bootstrap完成后执行held探针，首次FAIL发现探针每tick释放/重按使canonical按钮有而2tu样本持续为0；仅修held探针分支后同原Battle Scene真实Play PASS，一次D/一次K，right725/jump730/airborne736/landing756，动作215→215→0→7。落地后3tick精确X位移Unity11.778944736/2048与正式源码模型7.666666667/1333均为0.005751437859画面宽；原Editor编译、正式源码40tick严格校验、Ledger PASS，Scene SHA不变且DAT/Scene无diff。只证明鸣人16:9该序列比例，正式EXE可见/其他实体D-024仍开放。下一优先螺旋丸真实物理J与画面时序，Q07新包后置。详LANDING-RATIO-PLAY.md。
> 2026-09-23 用户技能物理输入优先包 `NTSD28-USER-RASENGAN-PHYSICAL-PLAY-001 / IN_PROGRESS`：脚本前Task/Change/Ledger已建，仅声明新Editor Play诊断菜单；将使用原项目已启动Battle Scene的鸣人OID2，Play-only置正式动作241，在首次可见253后排入物理J，逐tick记录画面动作/FrameInputSet/MP和wall time。正式EXE可见与任意生产修复仍待；DAT、Scene、相机、非战斗代码不改，Q07新包后置。
> 2026-09-23 用户两项回归优先状态：鸣人持续D+K落地奔跑在原Battle Scene Play的3tick精确屏幕距离比例已与正式源码模型一致，仅16:9样本局部PASS；全实体D-024、正式EXE可见待。螺旋丸转手里剑的受控32tick三时点与源码模型一致，原项目Play首次253和第二次253后物理J均下tick经FrameInputSet转301；正式DAT动作254已无hit_a。第二问题用户体感未复现，正式EXE画面截止/真实晚按相位尚未核对，生产修复未实施，不可称已修好。`NTSD28-USER-RASENGAN-PHYSICAL-PLAY-001 / FOCUSED_TEST_PASS / USER_SYMPTOM_OPEN`；DAT、Scene未改，Q07新包仍后置。详FULL-WINDOW-COMPARISON.md。
> 2026-09-24 D-024只读状态契约复核：已确认OID219同当前间距异历史速度与fusion阈值两个非体感首差仍未修，Q07继续后置。`COORDINATE-CARRIER-STATE-VERSION-GATE.md`记录双坐标载体的runtime reset/copy、entity/raw快照17、aggregate28、checksum31、历史旧版测试风险；未修改脚本或资源，未运行新测试。
> 2026-09-24 D-024位置写入只读门槛继续推进：已核对`SyncIntegerPosition`在Battle整数不变时早退、`PhysicsState.BindRuntime`可能带旧Runtime历史重绑、OPoint物化带显式整数时跳过Sync，以及独立`BattleEcsCharacterStageZPass`写Z/ZInt。对应正式`clamp_type0_stage_depth`已核对；`POSITION-WRITER-OWNERSHIP-GATE.md`增精确入口、三项回归反例与待确认非角色Z/held路径。未改脚本/DAT/Scene/相机，OID219/fusion和双坐标实现仍开放，Q07后置。
> 2026-09-24 D-024 StageZ独立边界见证已在脚本修改前立Task/Change/Ledger：NTSD28-USER-STAGE-DEPTH-HISTORY-WITNESS-001仅声明现有StageZ Editor测试文件的两例，raw Z300+40在正式180..350内，配置全景投影Z放大后生产物理位置触350边界。将经原项目Editor验证实际机械积分和StageZ写入；生产/DAT/Scene/相机/非战斗不在范围。OID219/fusion未修、Q07暂停。
> 2026-09-24 D-024 WPoint姿态见证已在脚本前立Task/Change/Ledger：NTSD28-USER-WPOINT-POSE-VIEW-WITNESS-001只扩既有Q06 held源row1测试，原版持有者X300→held X311/Z251；双视口让持有者先经生产物理raw Vx48移动，再经生产HeldObjectProcessAll检验局部X间距11不随全景位移倍率放大。测试尚未运行，DAT/生产/Scene/相机/非战斗不在范围；OID219/fusion仍开放，Q07后置。
> 2026-09-24 非角色早期StageZ源码顺序差异转入NTSD28-USER-NONCHAR-EARLY-STAGE-Z-001 / IN_PROGRESS：正式`clamp_type0_stage_depth`两次遍历全部active并按type0/非type0用0/±1深度边界；Unity对应两次StageZ只收type0，非角色至后续PreFrame才裁。脚本前Task/Change/Ledger限定StageZ默认/Legacy两生产入口与两现有Editor测试，先registered World RED再改生产，保持DAT/Scene/相机/非战斗及D-024物理比例。OID219/fusion仍待，Q07后置。
> 2026-09-24 D-024 WPoint武器组件分支双视野见证PLANNED：`NTSD28-USER-WPOINT-WEAPON-COMPONENT-VIEW-WITNESS-001`已在脚本前立Task/Change/Ledger，仅扩既有Q06 held测试，复用正式source row21/type1；持有者raw Vx48后经实际HeldObjectProcessAll，检查两视野的原始局部挂点间距11。DAT/生产/Scene/相机/非战斗不在范围；OID219/fusion与Q07仍开放。
> 2026-09-24 D-024 CPoint持有姿态双视野见证PLANNED：`NTSD28-USER-CPOINT-HELD-POSE-VIEW-WITNESS-001`已在脚本修改前建Task/Change/Ledger，仅扩既有Q06 CPoint Editor测试并复用正式source row1。持有者raw Vx48后调用实际`RunWeaponSyncHeldStep10`，预计两视野保持局部间距25/Y4/Z249。DAT/生产/Scene/相机/非战斗不在范围；OID219/fusion仍开放，Q07后置。
> 2026-09-24 D-024 CPoint投掷姿态双视野见证PLANNED：`NTSD28-USER-CPOINT-THROW-POSE-VIEW-WITNESS-001`脚本前已立Task/Change/Ledger，仅扩既有Q06 throw测试并复用正式source row0。投掷者raw Vx48后经实际RunCpointAdvanceStep10，预计两视野局部X差11/Y-24/Z200；DAT/生产/Scene/相机/非战斗不在范围。OID219/fusion仍开放，Q07后置。
> 2026-09-24 D-024 kind14 threshold test package PLANNED before script edit: NTSD28-USER-KIND14-PRECISE-THRESHOLD-WITNESS-001 owns only one BattleHitExecutionPlanEditorTests method, precise attacker X5.5/int5 versus control X6.5/int6 with target X0/Vx>0 and production candidate/dispatch. Formal precise +5 condition differs from current Unity integer condition at X5.5. Original Editor focused evidence pending; no DAT/Scene/nonbattle change, Q07 paused.
> 2026-09-24 D-024 kind14 precise-coordinate correction PLANNED before script edit: NTSD28-USER-KIND14-PRECISE-COORDINATES-001 owns registered writer, hit-plan projection, three integer fallbacks and the focused test assertion. Existing original Editor witness job cbe5118f13804bc0ad603aa023dba18d is 2/2 characterization; X5.5/int5 gives no Unity block despite source precise >5. Run formal RED then correct operands, keep D-024 source-history carrier separate; Q07 paused.
> 2026-09-24 D-024 文档完整性恢复：本轮追加状态行的字节写入操作使本文件及当前 handoff 正文变为 NUL。已先备份损坏字节文件，再从 03:18 的完整 Git 对象快照恢复正文；以下补记 03:18 后的 Task/Change Record 与实测进度。此恢复不改变战斗代码或 Scene；两文档需重新通过 Change Ledger 校验。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-WEAPON-FRAGMENTS-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`：正式内置和 DAT 武器碎片出生源坐标写入父实体源整数加原始 dx/dz，物理比例输出保持。原 Editor RED 初始化4例失败、GREEN 12/12，通过编译且 Console 0 error。源坐标读取者未启用，完整 Driver/Play/EXE 和 OID219/fusion 仍未闭，Q07 暂缓；详 Task/Record。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-STATE9996-BIRTH-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`：正式五子实体源 X/Z 生成为父源整数加原始 X 随机值、Z+1；原 Editor RED 初始化2例失败、GREEN 6/6。物理比例和随机消耗未变，详 Task/Record。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-STATE18-BIRTH-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`：state18/19 粒子源精确坐标按父源精确值加原始偏移、源整数继承父源整数；原 Editor RED 2例失败、GREEN 6/6，详 Task/Record。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-PARTICIPANT-BIRTH-001 / RUNTIME_PENDING / CARRIER_NOT_ACTIVE`：菜单入战斗 participant 的原始绝对出生坐标写源精确/整数；原 Editor RED 2/2失败、GREEN 2/2通过。真实 Menu→Battle Play 未验，详 Task/Record。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-KIND1-RANDOM-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`：kind1 随机原始 X/Z 偏移写入独立源整数，正式整数重投影精确坐标；原 Editor RED 3例失败、GREEN 14/14，四次 RNG 消耗和物理比例输出保持，详 Task/Record。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-BIRTH-FOUNDATION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`：OPoint task、World/component 物化及 late stage 绝对出生增加独立源坐标历史。聚焦 Editor 22/22；原 Battle Scene Play 的真实池化 Renderer 出生探针 PASS：源 X/Z 整数(-42,126)、战斗 X/Z 整数(312,259)，tick/checksum/借用数不变，Scene SHA 不变。此前探针错误地期望源精确 X 保留小数，已按正式 kind1 整数重投影修正测试，详 Task/Record 与 renderer-play-pass.json。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-JSON-CHECKSUM-001 / FOCUSED_TEST_PASS`：独立源坐标进入 JSON/lockstep 诊断投影并有聚焦检查；这是历史载体，尚未将源坐标读者接入正式规则。详 Task/Record。

> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-STATE-001 / FOCUSED_TEST_PASS`：实体独立源坐标和边界 flag 历史加入 runtime、snapshot/checksum；聚焦检查通过，仍不宣称 OID219/fusion 全链等价。详 Task/Record。
> 2026-09-24 `NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001 / FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / EXE_EMPTY_SLOT_PENDING`：正式 OID875/action55 预分配空槽目标在 Unity 原 Editor RED 见证 Vx 应为-1.4 实际0；active 实体为空时改读可寻址 raw slot XInt/ZInt，GREEN 2/2；随后完整 SelfCheck 经另一测试断言修正后 PASS。正式 EXE 空槽直观行为和完整 Driver/D-024 双坐标仍待验。详 Task/Record 与 ACCEPTANCE.md。

> 2026-09-24 `NTSD28-USER-HITFA7-NONCHAR-SELF-CHECK-CORRECTION-001 / FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / TEST_ONLY`：FL-02/HITFA-7 非角色行为7旧自检误期望同 OID clone，占用满槽；正式源码该分支不生成 clone。仅修自检断言，原 Editor 完整 SelfCheck PASS，生产逻辑未动。详 Task/Record。
> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-REVIVAL-EFFECT-BIRTH-001 / PLANNED / CARRIER_NOT_ACTIVE`：正式 playable 续命 OID998/action6 从宿主原版整数 X/Z 出生且 Z+1，Unity 当前即时 task 只带已放大的物理整数、源历史缺失。准确 Task/Change 已建，下一步原项目 Editor RED/GREEN 实际工厂路径；普通复活平均/RNG 单独处理，Q07 暂缓。
> 2026-09-24 `NTSD28-USER-SOURCE-COORDINATE-REVIVAL-EFFECT-BIRTH-001 / CODE_WRITTEN / COMPILE_PENDING / CARRIER_NOT_ACTIVE`：正式续命OID998/action6源整数出生写入已按 Task 限定落到现有即时task，原项目实际factory聚焦夹具已写。一次桥接请求把筛选字段放在错误JSON层级，意外启动8420项EditMode作业`2ed78c0b98214a5ba14ec92250d3b9f8`；该作业不是本包验收，仍在运行，不并发发新测试。待其终态后刷新编译并只跑C07聚焦组。Scene/DAT/非战斗未改，Ledger当前PASS。普通复活源坐标sum_x/RNG首差另列门禁，Q07暂缓。
> 2026-09-24 authority correction CODE_WRITTEN: `NTSD28-USER-KIND14-INTEGER-OPERAND-AUTHORITY-CORRECTION-001` found that playable `BattleWorld28::resolve_special_relation_hit` kind14 compares integer `Position28::x/z`, not precise doubles. The prior precise-operand package is superseded as a parity conclusion. Six Unity branches plus focused expectations are corrected; original Editor PID288224 broad job remains nonterminal/bridge-timeout, C: free 0, compile/NUnit pending. D-024/Q07 remain open.
# 2026-09-24 D-024 source physics writer package

`NTSD28-USER-SOURCE-COORDINATE-PHYSICS-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: original Editor source physics7/7, weapon identity/type3 extras12/12, adjacent ratio11/11; ledger755 PASS and Battle Scene SHA unchanged. This closes only the source-rule physics writer. Source kind14 flag producers, teleport/stage/relation history and OID219/fusion readers remain open; Q07 and D-024 are not delivered.
# 2026-09-24 D-024 source kind14 flag producer

`NTSD28-USER-SOURCE-KIND14-DIRECTION-FLAGS-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: original Editor strict threshold/central/fallback/consume7/7 and adjacent ECS kind14 2/2 PASS. Physical flags unchanged; source gameplay readers, stage/teleport/relation history, formal Play/EXE and Q07 remain gated.
# 2026-09-24 D-024 source teleport writer

`NTSD28-USER-SOURCE-TELEPORT-WRITER-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: original Editor final C05 class13/13 PASS with divergent source/physical anchors and no-target snap. Physical view-offset output stays; target-ranking source operand remains a separate gate. Source gameplay readers and Q07 remain inactive/paused.
# 2026-09-24 D-024 source stage-depth writer

`NTSD28-USER-SOURCE-STAGE-DEPTH-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: original Editor new source-depth6/6, adjacent StageZ9/9 including no-allocation. Source stage X, other position writers, Driver/Play/EXE and source gameplay readers remain open; Q07 paused.
# 2026-09-24 D-024 source character stage X

`NTSD28-USER-SOURCE-CHARACTER-STAGE-X-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: original Editor new source X11/11 and adjacent PreFrame6/6 including zero allocation. Noncharacter edge destruction is a separate D-024 lifecycle gate; source readers and Q07 remain gated.

Noncharacter X-edge destruction under D-024 remains undecided; 2026-09-24 async user preference pending: formal source-rule crossing preserves native lifetime but may leave view, physical crossing preserves edge but shortens lifetime. Do not alter its gate until clarified. Continue independent relation/respawn/source-writer work.
# 2026-09-24 D-024 source kind8 relation position

`NTSD28-USER-SOURCE-KIND8-RELATION-POSITION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: original Editor final kind8 production class15/15 including next physics integer-sync. Physical output and source-domain decision readers remain unchanged; legacy direct reachability, other relation writers and Q07 remain open.
2026-09-24 D-024 `NTSD28-USER-SOURCE-GRAB-RELATION-POSITION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: shared kind1/kind3 grab source-rule X pair with original Editor final23/23 PASS. Source-domain facing/selection, other CPoint/WPoint relation writers, full Driver/Play/EXE and Q07 remain gated.
2026-09-24 D-024 `NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: held CPoint source-rule X/Z from catcher integer anchor; original Editor settlement class7/7 PASS. Nonzero CPoint Z static physical branch difference has no proven official-content reachability; other relation writers/full Driver/Play/EXE and Q07 remain open.
2026-09-24 D-024 `NTSD28-USER-SOURCE-CPOINT-THROW-POSITION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: formal integer throw X source-rule placement, original Editor full throw class8/8 PASS. WPoint/source-facing, Driver/Play/EXE and Q07 remain open.
2026-09-24 D-024 `NTSD28-USER-SOURCE-WPOINT-HELD-POSITION-001 / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: canonical generic/weapon source WPoint X/Z, original Editor two-view4/4 + adjacent full tick2/2 + incomplete1/1. Direct character-link and cover2 official reachability, Play/EXE and Q07 remain gated.
2026-09-24 D-024 `NTSD28-USER-SOURCE-REVIVAL-AVERAGE-001 / FOCUSED_TEST_PASS / PLAY_PENDING`: formal source peer `sumX` gate and one RNG pair, source/physical precise outputs, original Editor revival11/11 PASS. Real participant Driver/Play/EXE and Q07 remain open.
2026-09-24 D-024 `NTSD28-USER-SOURCE-HITFA5-TARGET-VELOCITY-001 / FOCUSED_TEST_PASS / PLAY_PENDING`: formal indexed OID219 frame51 moved target Vx3/4 and child source birth, original Editor8/8 PASS. Full Driver/Play/EXE and Q07 remain open.
2026-09-24 D-024 `NTSD28-USER-SOURCE-FUSION-DISTANCE-001 / FOCUSED_TEST_PASS / PLAY_PENDING`: source X/Z fusion gate, dual midpoint and split copy, original Editor X2/2 + formal records4/4 + Z1/1 PASS. Full Driver/Play/EXE and Q07 remain open.
2026-09-24 D-024 current next gate: source readers for revival average, indexed OID219 hit_Fa5 and fusion are partially active with focused original-Editor tests, not full parity. Real participant Driver/Scene Play, remaining source writers/readers and formal EXE comparison remain; noncharacter X-edge death awaits prior user preference. Q07 paused.
2026-09-24 D-024 broad self-check after current WPoint/revival/OID219/fusion patches: original project's running Editor consumed standard request and wrote fresh `Temp/NTSD_BattleRuntimeSelfCheck.result`=`PASS` at local12:11:00. This does not close Play/EXE or Q07.
2026-09-24 D-024 revival scoped original Battle Scene Play: existing request runner 10 controlled temporary-World cases `Passed` local12:13:41; Editor exit non-Play/idle, Scene disk SHA unchanged. Natural participant Driver/EXE and Q07 remain open.
> 2026-09-24 D-024 teleport ranking package `NTSD28-USER-SOURCE-TELEPORT-TARGET-001 / IN_PROGRESS`: formal state400/401 ranks source integer X/Z, Unity currently ranks scaled physical X/Z. Task/Change created before script edits; divergent RED/GREEN and original Editor focused acceptance pending. Q07 paused; no DAT/Scene/nonbattle changes.
> 2026-09-24 D-024 teleport target ranking `NTSD28-USER-SOURCE-TELEPORT-TARGET-001 / FOCUSED_TEST_PASS`: original Editor state400/401 divergent RED job7cadeedd both selected wrong physical-ranked target (expected collision Y -21, actual -42); dual-ranking correction GREEN job551d9463 C05 15/15. Incomplete source fallback preserved; full Driver/natural Play/formal EXE pending. Q07 paused.
> 2026-09-24 D-024 grab facing `NTSD28-USER-SOURCE-GRAB-FACING-001 / IN_PROGRESS`: formal kind1/kind3 paired relation determines facing from source integer X; Unity currently reads physical X. Task/Change created before script edits. Divergent RED/GREEN pending; Q07 paused.
> 2026-09-24 D-024 grab facing `NTSD28-USER-SOURCE-GRAB-FACING-001 / FOCUSED_TEST_PASS`: original Editor divergent RED kind1/kind3 both expected attacker right, actual left; source-complete facing correction GREEN B6 25/25 including no-allocation. Full Driver/natural Play/formal EXE pending; Q07 paused.
> 2026-09-24 D-024 next P0 static candidate: Unity AI snapshot X/Z currently comes from physical runtime ints, while playable native_ai target distance/threshold reads formal source positions. Requires live-path/snapshot contract and RED witness before any AI modification. Teleport/grab focused passes do not close D-024; Q07 paused.
> 2026-09-24 active D-024 object AI target package `NTSD28-USER-SOURCE-OBJECT-AI-TARGET-001 / IN_PROGRESS`: playable noncharacter hit_Fa target scan uses source X/Z, Unity ranks physical X/Z. Task/Change registered before script edits; divergent RED/GREEN and original Editor acceptance pending. Q07 paused.
> 2026-09-24 D-024 noncharacter AI target `NTSD28-USER-SOURCE-OBJECT-AI-TARGET-001 / FOCUSED_TEST_PASS`: playable source-nearest slot0 versus Unity physical slot1 confirmed RED in original Editor; dual-ranking correction with incomplete-history fallback final B0 9/9 GREEN. Character AI snapshots/full Driver/natural Play/EXE open. Q07 paused.
> 2026-09-24 D-024 character AI read-only mapping: configured production profile is DataOrientedCanonical; physical X/Z enters SoA initial rows, decision owned rows, refresh and legacy fallback, with input consumers through world.X/Z. Requires a full two-profile position contract and divergent target RED before AI modification. Object hit_Fa focused fix is separate; Q07 paused.
> 2026-09-24 active D-024 test-only character AI nearest witness `NTSD28-USER-SOURCE-CHARACTER-AI-NEAREST-WITNESS-001 / IN_PROGRESS`: Task/Change registered before test edits; compare initialized source-nearest slot1 with physical-nearest slot2 under configured DataOriented and Legacy profiles. No production AI changes; Q07 paused.
> 2026-09-24 D-024 character AI nearest target `NTSD28-USER-SOURCE-CHARACTER-AI-NEAREST-WITNESS-001 / FOCUSED_TEST_PASS / CONFIRMED_DEFECT`: original Editor formal-expected RED both DataOriented/Legacy (source-nearest slot1, physical-selected slot2); labelled current-defect characterization final 2/2 PASS. Production AI untouched; multi-path contract and new Task/Change required. Q07 paused.
> 2026-09-24 D-024 character AI contract mapped in `CHARACTER-AI-SOURCE-POSITION-CONTRACT.md`: source/physical first difference confirmed in both profiles, initial/refresh/fallback/unified publisher and broadphase owners enumerated. No production AI edit or parity claim; next bounded implementation Task/Change and source/fallback GREEN. Q07 paused.
> 2026-09-24 `NTSD28-USER-SOURCE-CHARACTER-AI-DOMAIN-001 / IN_PROGRESS`: Task/Change declared before AI production edits. Two-profile `CharacterInputAll` confirmed slot2 physical target; formal source slot1. Pass-scoped coherent source domain with incomplete-history physical fallback and unified refresh is the bounded implementation. Q07 paused.
> 2026-09-24 latest D-024 character AI: `NTSD28-USER-SOURCE-CHARACTER-AI-DOMAIN-001 / FOCUSED_TEST_PASS` makes complete-source AI passes read formal source-rule integer X/Z and incomplete-source passes consistently use physical X/Z in both production profiles. Original Editor actual input and nearest 4/4, adjacent candidate 13/13 and 128 warm inputs 0 B allocation 1/1 PASS. Full Driver/natural Play/formal EXE and all source writers remain open; noncharacter X-edge decision pending. Q07 remains paused. See Change Record for exact evidence.
> 2026-09-24 `NTSD28-USER-D024-NONCHAR-X-BOUNDARY-WITNESS-001 / FOCUSED_TEST_PASS` (diagnostic only): original Editor formal-expected RED 3/3 (ordinary X50 cull false vs true; protected OID122 X50/790 stayed vs formal100/700), retained defect-characterization 3/3 PASS. Production boundary unchanged; source-rule versus physical destruction choice, full Driver/Play/EXE and Q07 remain open. See Task/Record and D024-CURRENT-GATE.md.
> 2026-09-24 D-025 `NTSD28-USER-NONCHAR-WALKABLE-TTL-001 / IN_PROGRESS`: user confirmed project walkable polygon, noncharacter-only, 10 seconds battle logic time. Task/Change declared before code. Worker-safe map geometry snapshot, entity timer/reset/snapshot/checksum, focused Editor and safe Play are implementation gates. No DAT/Scene/nonbattle edit authorized; Q07 after D-025.

> 2026-09-24 D-025 update: `NTSD28-USER-NONCHAR-WALKABLE-TTL-001 / RUNTIME_PENDING`. Worker-safe polygon snapshot and 304-elapsed-tick noncharacter timer are written; timer participates in reset/copy/snapshot/checksum. Original Editor focused 8/8, adjacent 29/29 and 9/9, fresh full BattleRuntimeSelfCheck PASS. Actual Battle Scene map capture, worker tick, F5 and observable removal still await Play proof. No DAT/Scene/nonbattle edits in this package; Q07 remains after D-025 gate. See Task/Change Record.
> 2026-09-24 Q09/P-04 next bounded probe `NTSD28-Q09-SAME-Z-FORMAL-SPRITE-PIXEL-WITNESS-001 / PLANNED`: use formal OID120/121 drawable frame0 sprites in original Battle Scene, natural central publication and controlled world-camera GPU overlap comparison. Both Battle Scene cameras are user-confirmed enabled; current Scene SHA 9409F2...3B3A39 and diff empty. Task/Change/Ledger exist before any test script edit. Legacy and formal EXE same-condition A/B remain parent exits; no pixel claim yet.

> 2026-09-24 `NTSD28-BATTLE-UI-SKELETON-001 / IN_PROGRESS`：按用户要求先搭 `NTSD_Battle` 战斗 UI 骨架，预留 TEngine `UIModule` 的窗口接入。当前只登记并准备新增三个 presentation-only UI 脚本；不改 Scene、Prefab、Input Actions、DAT、战斗逻辑或 `BattleBootstrap` 的临时 Canvas 处理。用户后续在 Unity 中创建 `BattleMainUI` Prefab 并完成 Inspector 绑定后，再接 UIModule 打开/关闭和真实角色状态来源。详见 `docs/ai/TASKS/NTSD28-BATTLE-UI-SKELETON-001.md` 与 Change Record。
> 2026-09-24 `NTSD28-BATTLE-UI-SKELETON-001 / CODE_WRITTEN`：三个 presentation-only UI 脚本和 `Battle/` 文件夹 meta 已写入；临时 Unity/TMP/uGUI stub compile 与 `Tools/Validate-ChangeLedger.ps1` 通过。Unity assembly compile、`BattleMainUI` Prefab、TEngine UIModule 打开/关闭和 Battle Scene Play 仍待用户完成 Prefab 绑定后验证；不改 Scene、Input Actions、DAT、战斗逻辑或 `BattleBootstrap`。
> 2026-09-24 `NTSD28-BATTLE-UI-SKELETON-001 / SUPERSEDED`：用户重新整理 `NTSD_Battle` HUD，确认不存在八个角色 Slot；旧根视图和槽位抽象不再作为当前方案。
> 2026-09-24 `NTSD28-BATTLE-UI-HUD-REWORK-001 / IN_PROGRESS`：按当前场景的单角色资源区、ComboPanel、BattleControls 重做 UI 骨架。只移除旧 `BattleMainUIView` 场景挂载，保留用户 HUD 布局；新增 `BattleHudView`、`BattleComboView`、`BattleControlsView`，等待 Inspector 绑定和 Unity 编译/运行验证。详见新 Task/Change Record。
> 2026-09-24 `NTSD28-BATTLE-UI-HUD-REWORK-001 / CODE_WRITTEN`：旧八槽位脚本已从 active implementation 撤掉；`BattleUiContracts` 已收敛为单角色 HUD、Combo、Controls 三组状态；三个 view 已写入，当前 HUD 的旧 `BattleMainUIView` 挂载已移除。静态 HUD stub compile、窄范围 diff 检查、Change Ledger 通过；用户 Inspector 绑定、Unity compile、UIModule 和 Battle Scene Play 仍待完成。
> 2026-09-24 `NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001 / IN_PROGRESS`：用户确认 `NTSD_Battle` Canvas 为场景常驻、默认显示，不转换为 prefab；HUD 与 Combo 继续使用场景 `MonoBehaviour` view。当前只补 HUD/Combo 的显式状态处理和 ComboBg/ComboTemp/ComboArraw 场景引用；`BattleControlsView` 的按键实现等待用户说明。TEngine `UIModule` 不进入本次场景常驻 UI 改动。
> 2026-09-24 `NTSD28-Q09-SAME-Z-FORMAL-SPRITE-PIXEL-WITNESS-001 / CODE_WRITTEN`：新增单个 Editor-only 原 Scene 探针，正式 OID120/121 通过生产 OPoint factory 出生，在暂停且 worker 空闲时以不同诊断表现 tick 取得 baseline/A/B/重叠四次中央计划、原世界相机受控 GPU 像素与清理；尚未编译或 Play，不得报告遮挡通过。相机启用和 Scene 哈希必须复核。
> 2026-09-24 `NTSD28-Q09-SAME-Z-FORMAL-SPRITE-PIXEL-WITNESS-001 / VERIFIED_CONTROLLED_CENTRAL_GPU_ONLY`：原Editor定向Play使用正式OID120/121同Z240，body命令slot52/index1先于slot51/index3；两正式body屏幕交集的11/11可区分像素为后绘OID120颜色、0相反/混合，独立PNG读取复核。对象/slot/两池/暂停恢复、Editor退出、Battle Scene SHA/diff不变。旧全屏误匹配PASS已废弃。natural full-tick、Legacy与正式EXE A/B仍待，P-04/Q09开放。
> 2026-09-24 Q09/P-04 `NTSD28-Q09-SAME-Z-NATURAL-TICK-PIXEL-WITNESS-001 / RUNTIME_PENDING`: original Editor compile0; real tick5→6 formal OID120/121 frame1/pic1 equal-Z central commands ordered. The revised four-separate-Play method obtained a `baseline` PASS_CAPTURE under Scene SHA9C7EDD...BA6CFD7D with RNG/slots/cleanup recorded; before `a`, parallel UI work saved Scene to 711FE77B...E5D605457. Cross-version four-image comparison stopped; natural GPU pixel verdict still absent. Earlier controlled 11/11 remains separate; Legacy/EXE open. Preserve UI Scene changes and enabled Cameras. See PROGRESS.
> 2026-09-24 最新Q08更正：`NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001 / FOCUSED_TEST_PASS`。原Unity Editor精确EditMode job `2475f46d16f44bc0a4a6c1201b1bd384` 1/1 PASS、Console error 0；合成kind0碰撞在tick内致死但结果timer仍0、次tick为1。文中同ID `COMPILE_PENDING` 是旧快照；物理输入Play/正式EXE画面及整个Q08出口仍待，生产代码未改。
> 2026-09-24 执行优先级更正：对齐表旧R15 type3/action诊断游标已过期，非角色hit_Fa7额外出生首差已有后继生产修复与三tick限定证据。D-025非角色离区10秒用户例外已限定VERIFIED，覆盖旧即时X销毁时机选择；D-024其余源坐标读者/写者及整链仍开放，故Q07按用户“先非体感检验”要求后置。原Editor Battle Scene当前dirty，Q09自然四图Play须等干净且单一Scene SHA后重跑，不覆盖并行UI改动。Q08致命命中计时1/1限定PASS；总目标仍ACTIVE。
> 2026-09-24 Q09/P-04自然同Z CentralOnly GPU限定出口 `NTSD28-Q09-SAME-Z-NATURAL-TICK-PIXEL-WITNESS-001 / VERIFIED`：原Battle Scene同SHA四次独立Play完整tick5→6，各自清理/退出、Scene哈希不变。四图独立全交集82/82像素符合OID120后绘制模型，44排除相反顺序、0支持相反；稳定非白底色和5个过滤边缘解释旧原位精确色字段0。原图/JSON/公式见 `scene-471396E7-series/ANALYSIS.md`。原Editor最终非Play且Scene clean。Legacy/正式EXE同条件、Q09其它P项、D-024、Q07～Q12与总目标仍开放；文中同ID `RUNTIME_PENDING` 为旧快照。
> 2026-09-24 D-024 AI子体原项目Play请求11/12：显式battle mode0修正早先夹具错误，AI tick75选中玩家；600完整Driver tick无持久owner1子体，但记录到的Lee含OPoint帧61/486只产生正式OID814 `pic999/wait0`纯音效，所以不构成生成链生产故障。源carrier0缺失、ordered shutdown、pool零借用、双Scene SHA保持。Task/Record已更正；改用正式非音效可达动作后再验，D-024和Q07总门槛仍开放。
> 2026-09-24 Q07预览子包限定关闭：旧`serialized-owner-classification.csv`记的Sasuke BMP Scene引用1已由正式`sasu.png` GUID/y881重绑取代。原项目Editor当前干净Battle Scene现场菜单预览PASS，JSON/PNG归档并已看图，原Editor定向预览类11/11 PASS、Console0、双Scene SHA未变。`NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / VERIFIED`只针对禁用作者预览；旧BMP删除与Q07整组仍开放。此前“原Editor Reload待”为历史快照。
> 2026-09-24 Q07旧图片可达性现状：当前正式根的菜单/战斗预热使用Logan候选，原Editor正式type-0 DAT/图片SHA部署测试1/1 PASS；旧索引383 GUID在指定NTSD序列化文件0命中。然而空根旧`data.txt`/DAT/图片fallback和Editor刷新仍可达，旧文件删除授权0。2026-09-22报告称部署测试遍历旧资源已过期，纠正证据在`NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001/CURRENT-DYNAMIC-REACHABILITY-20260924.md`。Q07/D-024及总目标保持开放。
