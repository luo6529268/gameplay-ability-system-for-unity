# C023/C024 新版正式根 EXE 帧推进对照

当前状态：`VERIFIED_SCOPED_ROOT_TRACE`。诊断限定出口已取得；父 C023 的自然生产入口与原场景出口、父 C024 的原场景出口分别维护，不能由本报告直接宣称整个 Q07 完成。

唯一正式 EXE 精确 SHA256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。所有根回放直接调用该文件，未晋升源码诊断 EXE。诊断使用对应 playable GameSession 完整生产 tick；BG1/Z400 仅用于原版诊断，没有向 Unity 导入原版背景或模式 DAT。

| 案例 | 触发与用途 | 正式根结果 | 声明字段比较 |
|---|---|---|---|
| jump | 鸣人普通跳跃；未触发 C023，属于非触发控制 | exit0 / playback PASS | 1251 / 1251 |
| ground | 鸣人地面 idle 控制 | exit0 / playback PASS | 1248 / 1248 |
| kar433 | 香燐433→434自然 OPoint315/43→44→50→300；tick10 按 next1320 选314 | exit0 / playback PASS | 2288 / 2288 |
| kar434 | 直接初始434没有生成上述子体；保留可达性反例 | exit0 / playback PASS | 707 / 707 |
| air20 | **受控初始**鸣人 action0/Y-20/Vy0；tick1 state0→212 | exit0 / playback PASS | 1248 / 1248 |
| air40 | **受控初始**鸣人 action0/Y-40/Vy0；tick1 state0→212 | exit0 / playback PASS | 1248 / 1248 |

比较仅覆盖 tick1..32：每个活跃源实体的14字段（OID/type/action/state/counter/XYZ/三速度/HP/owner/team），5个 RNG 标量字段（CRT调用数、同步counter/index/calls/site）及每条 frame event 的 from/to/rawNext。声明范围内实体集合相同；打印速度的容差为绝对1e-6，以上六例实测速度差为0。未据此宣称完整 World checksum、画面、物理按键或全类型通过。

第一版比较明确发现并保留两类载体观察：根在 LFR EOF 处多输出 tick33，该 tick 没有源样本，不纳入已声明32tick；LFR携带同步随机数表，却不携带独立 CRT seed，源指定682973786、根加载配置的默认seed0，CRT state每tick分别1758127634/3374725112。不能声称这两个字段对齐，也没有据此修改生产随机数逻辑。本案例的 CRT调用数未变，C024使用同步流，相关counter/index/calls/site及选帧全部一致。初次 `comparison.json`/`comparison-summary.json` 不覆盖；范围明确后的比较在 `comparison-scoped-v2.json` 和 `comparison-scoped-summary-v2.json`。两受控空中例独立使用 `comparison-scoped.json`。

正式 parser 读取330对象。首版发现器只查负局部Y偏移，候选0；扩为全部偏移后有10条“子体state0且定义212”的前驱 OPoint 路线（`discovery-v4/airborne-opoint-routes.csv`），尚不证明其父体在触发时实际位于空中。唯一有界 Dei-air 按键尝试在32tick内只走普通跳跃/空中攻击，未自然触发空中state0；报告中的泛化“selected212”计数也包含普通211→212，不能当C023门见证。C023的两个明确0→212根见证来自受控初始Y，自然生产链仍待。

编译历史完整保留：v1诊断误用不存在的 EntityState28.team，编译失败；随后改读已确认 battle_group。v2/v3/v4/v5编译exit0、无诊断；每版 argv、源码快照与输出分开保存。v2录四自然/控制案例，v3负偏移发现，v4全偏移发现及受控空中例，v5唯一 Dei-air 有界尝试。当前工具源码与v5快照对应，既有产物不伪装成最新版重录。

正式与 Unity staged 的 nar/kar/sas/dei/cla 五个 DAT 分别逐 SHA相同，见 `content-hashes-validated.json`。源和根进程结果、声明比较、失败观察均在各案例目录。生产脚本、DAT、Scene、配置 Asset、非战斗逻辑未修改。原 Scene C024 另由 `NTSD28-336B44-Q07-C024-RELATIVE-SCENE-PLAY-001` 验收。


原Scene补证：2026-09-30 C024相对next门限定关闭：正式Karin433→434自然OPoint315/43→44→50→300；tick8/9为300/counter0/1，tick10以next1320选314/counter0，同步调用恰增1/site0x452390。336B44根正例32tick2288声明字段一致，直接434无生成控制707字段一致；原Battle Scene唯一relative-next-scene-01 PASS/DONE，tick5→37，125实体行14字段及32tick六个源/Unity RNG标量共1942/1942，首差0、速度差0。原Editoridle/nonPlay/noncompiling，exit/Scene clean/四保护SHA稳。既有聚焦4/4、完整tick1/1和相邻证据有效；本轮只新增诊断，不改生产/DAT/Scene/Asset/非战斗。CRT初始seed未被LFR携带与根EOF33排除均保留，非全World/checksum或物理键选招证书。Q07/总目标仍开。


最终检查（2026-09-30）：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS，1062条Record/22个受治理代码文件，输出在RELATIVE-SCENE-PLAY-001/ledger-check.txt；`git -c core.safecrlf=false diff --check`退出0。诊断C#在原Editor编译0错，Scene01完成32tick、1942声明字段通过/正常退出/四SHA稳。源诊断各版本实际compile结果留证，当前源码对应v5快照。未跑无关角色/全量测试，既有聚焦证据没有被新生产改动失效。本轮新增诊断和证据/进度文档，没有生产脚本、DAT、Scene、配置Asset或非战斗改动。总目标ACTIVE，C023原Scene/自然门、F02及其余总表项继续开放。


2026-09-30后继自然入口补证：未修改既有v5工具，使用已声明spawn模式84/388，源与336B44根32tick1982字段一致、exit0/PASS，速度差0。地面普通Guren388→395→389产生619/260/Y-40；619无普通重力，267→268于tick21自然生成85/0/Y-22，同tick C25执行0→212/counter1/Vy0。此前jump/Dei-air未触发及只受控初态已证的事实保留，但“不存在自然证据”的旧恢复措辞由本条覆盖。新证据在guren388-airborne-child目录；原Scene另建AIRBORNE-SCENE-PLAY-001，未验收前父C023仍RUNTIME_PENDING。
