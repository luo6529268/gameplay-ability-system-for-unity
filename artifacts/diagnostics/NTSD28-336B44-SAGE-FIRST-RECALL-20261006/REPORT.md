# P1首次仙人分身召回核查

用户P1、移动或等待后首次失败/第二次成功。旧P2解释不适用；旧理想输入/重置位置帧和资源的成功不排除报告。保留原Battle启动World/actor，代表性24Down+36neutral后召回（具体方向/时长未报告），同场必要时第二次，无生产改动。Original Editor compile errorCS0；原Scene探针正在预热。权威仅根正式336B44；候选source只用于定位消费者，不定义未观察规则。


---

# P1首次仙人分身召回修复

状态：VERIFIED，限用户报告的“分身准备后移动／等待，第一次召回失败”与本批共用正纵深受击框修复。旧对齐总目标仍为USER_ACCEPTED_SCOPED_CLOSURE，旧P2连续击飞边界修复保持，不恢复全角色任务矩阵。

1. 原因：Logan nar401的bdy已解析ZWidth999，但Unity此前四条direct/cached候选／命中入口在进入body loop前，只用ITR默认半深度15拒绝。广筛也把body压成Z点。移动后纵深差超过15，分身无法进入334握手，鸣人进入428失败动作并打掉分身。未修改DAT、技能时长、输入窗口或鸣人特殊规则。
2. 修复：BruteForceSceneQuery统一BodyDepthOverlaps，逐个同时重叠的body计入正ZWidth；exact-cache保留每个body深度；正深度帧走既有body范围fallback；formal/union广筛、shadow与即时volume计入正宽度。没有设置body深度的普通候选严格边界、原ITR默认和旧full-height出口保持。Body内其他不重叠宽框不得污染命中。无新增manager/queue/生命周期阶段。

| 证据 | 实际结果 | 范围 |
|---|---|---|
| 正式根EXE SHA336B44 headless移动等待对照 | 准备167；24Down+36neutral，relativeZ48；clone334234、owner402236、Sage99 251 | 实际正式EXE。LegacyLFR终止头原expected2/got99，exit46/reportfalse保留，不称整体回放PASS |
| 修前Unity完整Driver | f23a5a2f697d407b8c3e019b739b00cb FAIL1/1；首次234clone仍327，235owner428，236clone受伤；600tick无99 | 实際自然技能输入，批准Logan内容。旧schema起点Z450失败另留，不当作游戏首差 |
| 通用几何RED | 43549b8b2e574036aacb0679ba285205 6/6均预期失败 | 三collector × identity/projected，正深度body漏命中 |
| 修后原Editor聚焦GREEN | 1b0ce8f831604ce78063939fa48f2153 PASS17/17，零跳过 | 6positive-body含不重叠宽body阴性＋6default near＋3body源序＋2延迟完整Driver |
| 完整Driver正式握手对照 | 两坐标首轮99均251；handshake-comparison.json 16/16声明状态相同 | owner/clone的oid/action/state/counter/owner，在234/235/236/251四时点。非完整World/RNG/所有状态等价 |
| 原saved Battle实际InputAction | original-default-scene03-pass.json PASS，run606d097cd7654b8f8194623bf1b02a87，250ticks，attempt1，firstAttemptTransformed=true，中央可见99 | 原出生及Frame/World/模式/PP均未重置；准备166，真实Down24＋neutral36，主角至project地图边界后relativeZ28仍首轮成功。Manual完整Driver观察，不证明LocalFreeRun所有Host节奏或GPU像素 |
| 编译与退出 | Console errorCS0；11阶段关闭完成；objects/slots/borrowers0，非Play、Scene clean，Scene/config SHA不变 | 原Editor PID19040，MCP6402 |
| 审计 | Change Ledger1289份Record/15code paths PASS；681保护文件及正式EXE稳定，diff-check PASS | 未改DAT/Scene/InputAction/资源/非战斗；before备份保存已有未提交内容 |

坐标／资源字段边界：正式对照初始Z450，Unity完整Driver因项目地图规则为Z542，delta48一致；原Battle出生sourceZ452.4479，项目下界限制末Z481.5972，delta28。原Battle世界tick5/InputPhase1/帧1起算，因此探针250与受控251不作完全相同初态的逐tick主张。旧诊断UnityRuntime.MP500不能直接当正式mp400的差异：实际原SceneRuntime.PP400承接技能消费；完整Driver旧rows未记PP，故资源不在本次16项对照内。不声称模式资源差异已确认。

保留的诊断失败：两次前期scene01/02均tick0键捕获None，未进入仙人流程；scene03先通过MCP Window/General/Game获得Game View焦点，完成真实InputAction。InputSystem.defaultUpdateType在GameView无焦点时选择Editor，故此前错误是探针输入环境问题，不判定游戏失败。重载后旧registry6401由另一进程占用，原Editor实际6402；先前刷新／状态超时原件保留。WindowsPowerShell默认RepositoryRoot错误留ledger-body-depth-check.log，正确pwsh显式RepositoryRoot命令随后PASS。不改变自动化/包。

验证命令入口：
- 原Editor TCP MCP refresh_unity(mode=force,scope=scripts,compile=request)；run_tests(mode=EditMode,test_names=四具名方法)，get_test_job按ID读最终summary。
- execute_menu_item(Window/General/Game)，execute_menu_item(NTSD/Validation/Battle/Naruto First Sage Recall Probe)。
- pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity。
- git diff --check；literal-path SHA256保护审计。
- 正式argv/input/trace/stderr保留formal-move-wait60-control，未重建／替换EXE。

修改／恢复记录：docs/ai/CHANGE-RECORDS/NTSD28-336B44-BODY-DEPTH-CANDIDATE-001.md与SAGE-FIRST-RECALL-PROBE-001.md；docs/ai/FILE-OPERATIONS/NTSD28-336B44-BODY-DEPTH-CANDIDATE-20261006/与SAGE-FIRST-RECALL-20261006/。只按授权反转自有hunk，不重置已有未提交内容。

未验证项：所有角色/技能穷举、inclusive-edge/负depth/ITR.z规则、所有LocalFreeRun时序、GPU逐像素、用户未报告的精确方向/等待长度；不由本次PASS推出。独立审查未发现阻塞问题，没有为这些未知项追加排期。
