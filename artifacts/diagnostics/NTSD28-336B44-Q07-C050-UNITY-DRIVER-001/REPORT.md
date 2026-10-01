# C050 原 Unity 完整 Driver 第 2 tick 首差

状态：`FIRST_DIFFERENCE_FIXED_IN_SCOPED_RAW / SCENE_PLAY_PENDING`。生产修复属独立 `NTSD28-336B44-Q07-C050-VERTICAL-SKIP-001`，C050/Q07与总目标开放。

正式源/根对照见相邻 `C050-HIDAN-BDY50-REACH-001/REPORT.md`。严格新版诊断 schema `ntsd28-336b44-q07-c050-bdy50/1.0`，场景 JSON 明确336B44 EXE SHA、正式 catalog SHA `0AAB4A0F...7A66181E6FE`、OID24/action37/X500 对 OID56/action259/X520或X1200、Z400/HP500/MP500、seed682973786、mode0、3tick中性输入。原项目已打开的 Unity Editor 导入新版脚本 DLL（脚本保存14:00:34、程序集14:03:22），生成C#工程0错；请求式 raw capture 近/远结果均 `PASS`，每个3完成tick。DAT、Scene和生产代码在此诊断子包未改。

正式根同LFR tick1～3与原Unity raw字段比较：每例两实体×动作/HP/Vy×3tick=18个字段。X520首差 `tick2/slot1/action`：正式根259，Unity186；tick3同差，HP在tick2均500→465、Vy均0，攻击者动作一致，计2/18差。X1200无命中，18/18同态。此为受控完整Driver的战斗动作首差，不是原Battle Scene Play或自然物理按键证书。Unity自有stage把角色Z从400钳至542，正式根仍为400；该项目地图例外未纳入上面的战斗动作/HP/Vy字段比较，不把Z差归因C050。

第一次raw头的 `scenarioReferenceExeSha256` 正确为336B44，但旧通用 `formalAuthorityExeSha256` 字段仍写历史 B1E13 常量；已在同一诊断 Record 内加入只对本新版schema生效的头字段更正，须原Editor重编后以新输出重跑近/远，再把本报告状态推进。旧输出保留原件，不静默改写。

2026-10-01 后继实际结果（覆盖上段待验快照）：修后原Editor已重编同一新版schema及共用writer；`unity-x520-green` 和 `unity-x1200-green` 两份请求结果均 `PASS`，各输出三个完成tick。两份新头的 `formalAuthorityExeSha256` 与场景引用均为所选336B44。逐tick比较两实体各自action/HP/Vy共18字段/例：近18/18、远18/18与正式根LFR相同，首差0。近距目标tick2、3 action均259/HP465/Vy0，修前两tick action186；攻击者与远距对照字段保持同态。此处仅证明选定战斗字段和三tick入口；Unity自有stage的Z钳位仍是独立用户例外，原Battle Scene Play、自然物理键及完整World仍待。
