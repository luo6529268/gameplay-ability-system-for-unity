# Q07/C050 特殊链接跳垂直反应原 Battle Scene 验证

> 2026-10-01 最新：`VERIFIED / SCOPED_SCENE_PASS`。原 Battle Scene 近 X520、远 X1200 各三生产 tick，与当前 336B44 正式根分别 24/24 选定字段零差；两次退出 Play、Scene clean、四保护 SHA 稳、LoganRuntime 无 Git 差异。只关闭本 Task；物理键自然选招、完整 World/表现等待，父 C050/Q07/总目标开放。[限定验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/ACCEPTANCE.md)。

> 历史快照：`CODE_WRITTEN / ORIGINAL_EDITOR_COMPILE_PENDING`；其下 `PLANNED` 行为脚本前记录，不代表当前状态。

状态：`PLANNED`。父 C050/Q07 与总目标开放。当前规则权威为根正式 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` EXE 及对应 playable 源码；正式非排除 DAT 用 NTSD 2.8-Logan。已证原件：[正式源/根近远入口](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001/REPORT.md)、[原 Unity 完整 Driver 修前首差与修后对照](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-UNITY-DRIVER-001/REPORT.md)。近距 X520 在 tick2 HP465/Vy0/action259，远距 X1200 无命中；修前 Unity 近距错误写 fall action186，通用 writer 已修且 Driver 近远各18/18同根。原 Scene Play 尚未验证。

唯一脚本范围：新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C050VerticalSkipScenePlayProbeEditor.cs` 及 Unity 生成的同名 meta；复用既有 C051 Scene 探针的只读/受控模式，不改生产战斗代码、DAT 数值、图片、Scene、Prefab、GameConfig、模式 Asset 或非战斗功能。不使用 computer-use，不启动第二个 Unity 项目。若预检发现原 Editor 正在操作、Scene dirty、请求或目标结果已存在，就不启动。

同初态：原 Battle Scene Play clone 在 BattleTestBootstrap.Start 前设 roster OID24/56；稳定暂停后把 actor/action37/X500 与目标/action259/X520 或 X1200、双方 Z400、HP/MP500、面右、队伍1/2、mode0、seed682973786、InputPhase0 按正式内容配置。用生产 `SimulationTickDriver.StepOneTick` 推进3个中性输入逻辑 tick，记录父/目标动作、目标HP、Vy、FrameDelay(motion hold)及根对照所需最小字段。近距 tick2/3 目标 action259、tick2 HP465/Vy0；远距前3 tick 无命中。若同 tick 首差，保存原件后诊断，不在探针中伪造结果。

启动只通过原 Editor 本地 Unity-MCP 桥刷新脚本和已记录的唯一新 `Temp` 请求入口；此请求会被探针自动删除，因此在创建前按 `docs/ai/file-removal-audit-contract.md` 建立精确路径、载荷、SHA、原状态与恢复源清单。结果用新路径 `FileMode.CreateNew`，不覆盖旧诊断。验收是原 Editor 导入编译0错、近/远真实Scene各3tick与当前正式根目标字段逐tick同态、两次正常退出/Scene clean、Menu/Battle/GameConfig/Mode Asset四SHA不变、LoganRuntime Git无差异。此包只关闭受控Scene，不推断自然物理按键、完整World/表现或整个C050/Q07完成。

回滚：仅本包新增Editor探针与meta，须在具体操作前依文件操作合同审计并获适用批准；结果和旧文件保留。

验收：原 Editor 新脚本及 meta 已导入，生成 Editor 工程 0 错；近远两轮 Scene 原始结果、逐字段比较和正常退出/资源保护证据均保存于上述验收目录。两次临时请求消费已按 `NTSD28-C050-SCENE-REQUEST-20261001-001` 事前/事后审计。没有运行全量 SelfCheck，本包没有修改生产逻辑。
