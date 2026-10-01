# C023 原 Battle Scene 自然空中 idle 分身帧门

> **最新状态 VERIFIED_SCOPED（下方历史保留）：** 2026-09-30 C023 限定关闭（覆盖此前自然/Scene待验）：Guren84从地面受控初始388按正式DAT自然生成619/Y-40，tick21由267→268生成85/action0/Y-22，同tick按计数后转212/counter1/Vy0；tick22 counter0/Vy1.7，后续自然重力/落地/AI均纳入32tick。源/336B44正式根1982声明字段相同/exit0/PASS；原Battle Scene唯一airborne-idle-scene-02 PASS/DONE，107活跃实体行×14字段和32tick六RNG标量共1690/1690、首差0/速度差0。原EditorTundra9.30s0错，idle/nonPlay/noncompiling；正常exit/Scene clean/四保护SHA不变。Scene01 local出生PASS但完整1690字段FAIL62保留：先RNGtick24，再实体tick28。Scene02实测World初始difficulty2，与正式root0不同；只在临时诊断World统一0后整段通过，生产AI未修、默认配置未改。这是fixture口径修正，非新增生产缺陷。既有完整tick5/5、相邻C0223/3/C25G4/4和type3控制复用，不重跑无关测试。关闭共享空中state0后计数转212门；不是物理键选388/所有类型自然入口/全World/checksum/画面或整场证书。Q07与总目标继续ACTIVE，下一G1为F02自然高速武器进入state1000的根/原Scene出口。


状态：`RUNTIME_PENDING`。原 Editor 已编译新增诊断（Tundra4.07s、0错），重载后 MCP 确认 idle/nonPlay/原Battle active，唯一 `airborne-idle-scene-01` 于 13:43:38 UTC 启动。等待同一次32tick结果与正常退出，不因观察超时重新启动。

当前正式根证据：普通地面红莲初始技能388→395→389，按原 DAT 在 Y=-40 生成619/action260。type3物理不施加普通重力，水晶267→268于tick21按原DAT生成85/action0/Y=-22。出生同tick的源C25事件明确0→212、counter1、Vy0；随后tick22 counter0/Vy1.7并按实际重力自然落地。源与336B44正式根32tick1982声明字段一致、根exit0/PASS、速度差0。此前普通jump/Dei-air未触发、受控Y-20/-40两例已证的历史保留，但缺少自然生产入口的旧措辞已被此证据补齐。

源/根限定比较：14个活跃实体字段、5个RNG标量（排除LFR未携带独立CRTseed的state）、每个frame事件3字段；根EOF额外tick33不纳入源已声明32ticks，不声称完整World checksum。证据在 `../NTSD28-336B44-Q07-C023-C024-ROOT-FRAME-001/guren388-airborne-child/` 的源CSV/LFR、root过程/trace/report和comparison-scoped.json。诊断工具未修改，实际运行的是既有v5的spawn84/388模式。

原 Scene 验收范围：正式staged84/2 roster从地面388/0初态开始，中性输入完整Driver32ticks，自然生成619/85；14个roster/619/85实体字段和6个源/Unity RNG标量。初始化动作选择是受控条件，物理键选388与整个画面/所有原Scene实体不属于这份证书；绝不设置分身Y、动作212、计数或roll。探针的birth/next tick断言与离线逐字段对照均通过后，才关闭C023限定门。

退出前后必须核对 Scene clean 和 Battle/Menu/GameConfig/ProjectBattleModeConfig 四SHA；原报告、失败与超时观察保留。没有生产脚本、DAT、Scene或非战斗改动。仍待实际结果。


Scene01 已终态 local PASS/DONE，32ticks107实体行，出生tick21/slot51；正常退出/Sceneclean/四保护SHA相同。离线完整1690字段 **FAIL/62差异**：chronological版首差tick24 customCalls22 vs21；首实体差tick28/action213 vs215，最大速度差14。LocalPASS只覆盖出生与次tick，不能关闭父门。第一次comparison数组按实体后RNG排序，因此firstDifference显示tick28；v2-chronological独立文件校正真正最早tick24，原文件保留。

正式root trace直接声明difficultyLevel4A0C30=0/battleMode0，Unity缺省difficulty2是待验证的夹具差异。只在Play副本World统一difficulty0并记录before/effective、Battle/Localmode/AiPhaseGate；没有生产、DAT、序列化Asset改动。第二次probe原Editor已Tundra9.30s编译0错，等待完成重载后唯一Scene02；若仍差继续writer审计，不按猜测修生产。


Final validation: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot current checkout -> PASS1063records,1governedcodefile covered by AIRBORNE-SCENE-PLAY-001; full output ledger-check-final.txt. git -c core.safecrlf=false diff --check -> exit0. OriginalMCPfresh editor-final-state.json confirms originalBattleactive/idle/nonPlay/noncompiling; scene02-protected-hashes.json four SHA equal. Two PNGmeta importer changes observed during Play remain preserved/unattributed; no manual importer edit or rollback, not included in four-protected-asset claim. Existing unrelated untrackedB11files remain untouched. No new production, DAT, Scene or configAsset changes; no unrelated tests rerun.
