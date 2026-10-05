# D-024 共用 Y 投影：原 Battle Scene 有限验收（2026-10-05）

> **2026-10-05 普通伤害提交邻例补证：** [空中纵向边界](Y-HIT-RESPONSE.md)在原Editor精确6/6 PASS；两视口下相交才10HP/HitCount1/CRT2及普通角色+3/-3停顿/arest4/vrest1，接触/分离无副作用，源Y保持。只有新增测试与专用助手，没有生产改动；它比此前候选边界证据多覆盖了C14提交，但不替代原Scene/正式根空中命中或完整字段证书。后续按总表条件门，父Record仍RUNTIME_PENDING。

> **2026-10-05 后续证据更正：** [自然落地复用核查](LANDING-REUSE.md)确认修复后既有32tick已经包含相对27落地 Y0/Vy0/action215及相对28后续action213；源/Unity1690项、正式根所选1658项均无首差，修前/后32个规则样本相同。下方“未覆盖实体着地后的源 Y/floor 相邻状态”作废，但落地画面命令没有采集，不能据此关闭落地 GPU 表现门。自然 kind0 火花另由 [SPARK-ACCEPTANCE](SPARK-ACCEPTANCE.md)限定通过，覆盖本报告建立时的“火花未修”快照；Y向真实命中及原Scene中间alpha仍未知，不自动追加测试。以下状态和限制按报告建立时的历史快照阅读。

状态：`FOCUSED_AND_SCENE_PASS / LANDING_HIT_AND_SPARK_PENDING`。当前权威正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；用户保留 Unity 固定完整视野、项目地图及 `BattleVisualScale=1.5` 本体图片尺寸。本报告只比较由生产 Battle Scene 生成的共用画面命令及相同的规则样本，不把它当作正式 GUI 与 Unity GPU 逐像素同态。

2026-10-05 增量：[原 Editor 精确 `testNames` job 原件](original-editor-y-boundary-20261005.json) `4c7b0ca838de4a4c8ed3939629255fa0` 的参数化 `VerticalBodyBoundaryKeepsSourceCandidateResultsAcrossViewScale` 为 2/2 PASS（identity 与 2048×1152 各一例；每例含源 Y29 相交、Y30 接触、Y31 分离，直接查询和 ForceRoleAware 生产收集均匹配）。生成 Editor 工程 301 warnings/0 errors，MCP 刷新后 Console 0 error、Battle Scene clean/non-Play。该证据只覆盖真实候选生成的纵向边界，实际命中响应与其它纵向出口仍待。

另复用[现有普通角色负 floor 落地聚焦原件](original-editor-landing-neighbor-20261005.json)：原 Editor 精确 job `c5e986e6501a412eb26f7685dc28e0e0` 1/1 PASS，`Y=-12,Vy=3` 跨至 floor `-10` 后测试断言 Y=-10、Vy=0、命中帧转移仍成立。它没有配置固定视口、也不是原 Scene Play，故只作为源 Y 规则未被本轮呈现改动扰动的邻例，不关闭实际战斗落地画面门。

源自然正例：正式 playable `render_snapshot.cpp` 本体 `screen_top=sourceZ+sourceY-centery`、影子只随地面 Z；[改前原 Scene 原件](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-20261005-01.json)和[改后唯一原 Scene 原件](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-green-20261005-01.json)均在相对 tick21/23 为 OID85/slot51/action212、source Z402、source Y -22→-20，中央实体/阴影各1命令、alpha1，发布/计划与 global tick26/28 一致。两个结果均 `PASS/DONE`、完整 Play tick5→37。改前/改后 `samples` 32项 JSON 结构逐项相同，故观察到的规则字段及本例 RNG 无首差；并不声称所有规则字段或人物全局同态。

上述字段与距离由[逐字段比较 JSON](prepost-comparison-20261005.json)保存，两份原 Scene 结果原件均只读保留。

| 同动作两帧 | 改前 Unity 本体减阴影 | 改后 Unity 本体减阴影 |
| --- | ---: | ---: |
| 相对 tick21、源 Y=-22 | 21.9999313 视图 px | 34.7177505 视图 px |
| 相对 tick23、源 Y=-20 | 19.9999809 视图 px | 31.5615654 视图 px |
| 绝对移动距离 | 1.9999504 px | 3.1561852 px |

正式 730 高视野中同源 Y 差2对应 `2/730` 画面高度。当前 Unity 1152 高视野中目标距离是 `2×1152/730 = 3.1561644` px；改后实测与目标相差 `0.0000208` px（float/世界坐标换算），距离占画面高度约 `3.1561852/1152`，与正式比例相符。修复只投影全局 Y；角色 sprite 局部宽高仍用批准的 1.5 倍视觉比例。

验证层级：先写两条 RED 断言，生成 Editor 工程按预期 6 个新 API 缺失错误；生产代码接线后 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 0 error。原 Unity Editor MCP `refresh_unity(force/all/compile=request)` 后 Console 0 error；两个精确 EditMode `testNames` 首次 job `931538c555074604970def79d9713154` 2/2 PASS，普通 body Y 端点/full-height sentinel 第三条 job `7100f1e72947440b8623ab0a45f039e5` 1/1 PASS。无全套 suite。Play 后重查两 job 仍 `succeeded`、完成计数2和1；MCP 的详细 `result` 在域重载后不可再取，[重查原始状态](original-editor-focused-20261005.json)只保留成功/计数，逐测试 PASS 来自运行完成时的原 Editor MCP 响应。

原 Editor 在 Play 前后为单个已保存 `NTSD_Battle.unity`，Play 退出后非 Play/idle、Scene clean，Console 0 error；磁盘 SHA 前后均为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。临时请求文件按[独立覆盖/恢复审计](../../../docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q07-D024-SCENE-REQUEST-20261005-001/RECORD.md)恢复至操作前逐字节哈希，新结果 SHA `F3D090762B3508CB21AE3442DCE44CA41AABFE39A6B3739F64A3BBA75AF3C5DF`，没有覆盖旧结果。

限制：本 Scene 32 tick 未覆盖实体着地后的源 Y/floor 相邻状态，也没有在真实命中中验证 Y 向 bdy/itr 后续伤害响应；聚焦体积和新候选测试已覆盖普通 endpoint、full-height sentinel 与纵向候选边界，但不等于实际命中。R120 中间 alpha 有采样器聚焦测试、未取得该 OID85 的原 Scene 画面；ECS kind0 火花 `viewZ+sourceY` 混合坐标还未修。正式 GUI 同输入 GPU 截图、所有非例外表现、Q07/Q09/Q12及总目标均继续开放。下一步只补有区分力的着地/命中邻例与活跃火花出生出口，不扩成全部角色矩阵。
