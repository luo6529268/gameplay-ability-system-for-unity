# Q09 鸣人／鼬全局 tick14 代表画面（2026-10-05）

## 限定结论

原 Unity Editor 的 `NTSD_Battle` Scene 中，鸣人 OID2 与鼬 OID9 从生产 Driver **全局 tick0** 出发，提交 14 行零玩家输入，停在全局 tick14；中央发布帧、像素计划及截图完成时均为 tick14。两人的末帧动作3、HP500/MP200 与当前正式根 EXE 真实暂停窗口 tick14 标题所见字段一致；源 X500/620、Z400 是 Unity 记录及双方指定初态，正式 GUI 标题不显示 X/Z。随后取得的[正式根逐 tick 回放](../NTSD28-336B44-Q09-GLOBAL-TICK14-ROOT-TRACE-001/REPORT.md)在 tick1 起将两人 Z 钳为542，Unity 项目地图保持400，属于已批准的地图边界差异；所选其余字段逐 tick 相同。合成 Game View 中可见两名角色本体及足下阴影，左右关系与正式截图一致。此为一例同全局 tick 的有限表现见证；不证明全 World、所有资源、其它动作或整个 Q09/Q12 已对齐，也不能按两套不同背景、固定相机和 HUD 直接逐像素裁决。

## 权威及原件

- 正式根 `NTSD2.8-Logan.exe` SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；[正式 GUI tick14 报告](../NTSD28-336B44-Q09-TICK-BOUND-CAPTURE-20261005-v3/REPORT.md)、[1280×720 客户区 PNG](../NTSD28-336B44-Q09-TICK-BOUND-CAPTURE-20261005-v3/formal-paused-client.png)，PNG SHA-256 `96DA6CF6E9EC4675984D981EF9108A0B4E3A5073D29C0CB02F2AC69794DC3450`。正式 GUI 报告的整项 `passed:false` 来自未执行手动输入，只把稳定暂停标题与截图用于本单例。
- [Unity 成功结果 JSON](idle-global14-336b44-20261005-061031-920004.json)、[1920×1080 Game View PNG](idle-global14-336b44-20261005-061031-920004.png)，后者 SHA-256 `049F7811527E6AC13C6BD26E0E6DDCF7DE53E59B89CEECF228308C8603A3FC20`。同一 JSON 记录 `startTick=0`、`endTick=14`、14 行 `submittedP1Buttons=0`、`publishedTick=planTick=screenshotTickAfter=14`，并记录每行 Unity 角色位置、动作、HP/MP、RNG 及发布 tick。动作序列在 Unity 为 tick1–3 动作0、4–7 动作1、8–11 动作2、12–14 动作3；正式截图标题只证明 tick14 的动作。后续正式根回放才提供 tick1～14 的所选字段逐 tick 对照；详上方正式根报告。
- Unity 使用项目 `LoganRuntime` 正式内容根及项目自有 `battleMode=0`。配置初态为鸣人/鼬源 X500/620、Z400、双方 HP500/MP200，seed0 并补正式 BGM 同步 RNG 调用点。首帧 `initialObjectCount=4`；正式窗口没有导出完整 World 实体表，所以不宣称所有对象数量/身份相同。

### `initialObjectCount=4` 的口径回访（只读）

这里的 `SimulationWorld.ObjectCount` 调用 `SimulationRegistryModule.CountActiveObjects`，逐个统计所有已注册的 `ISimObject`，**并非仅统计逻辑战斗实体**。原 Scene 的 `BattleTestBootstrap.SetupTestCharacters` 依次为两个角色取得 `LF2ObjectRenderer` 和 `LF2Character`；`LF2ObjectRenderer.OnEnable` 向 World 注册渲染器，`LF2Character.ModuleBind` 注册逻辑角色。按这条可达注册链，两个角色加各自渲染器正好解释 `ObjectCount=4`；渲染器另用 Unity Instance ID，不消耗逻辑实体 StableId。正式 `GameSession28::reset` 为两个 combatant 各调用一次 `BattleWorld28::spawn_at`，正式 GUI 报告在 tick14 给出 `lastSpriteCount=2`，但没有完整 World 成员导出。**因此 4 对 2 不是同口径对象数首差**，也不由静态链推断 Unity 成员表已经逐项与正式版匹配；本轮不为该计数另开生产修复或重复 Play。
- 首轮[失败结果 JSON](idle-global14-336b44-20261005-060529-834416.json)停在 tick0、0 测量行：探针早于异步内容发布检查；`FAIL / DONE`、退出 Play 和 Scene clean 均有记录。只修正 Editor 探针的等待顺序并改用第二个唯一请求，原失败与旧全局19报告均保留。此失败不是生产规则首差。

## 运行安全与边界

生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 在最终脚本修改后 0 error/301 warning；原 Editor 经 MCP `refresh_unity` 导入，随后在原 Battle Scene 执行唯一有效重试。成功结果记录 `exitedPlay=true`、`sceneCleanAfter=true`；前后 Scene SHA-256 均为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。退出后 MCP 只读复查仍为 `NTSD_Battle`、`isDirty=false`，磁盘 SHA 再算相同。Console 后来有一条 MCP client handler 的 disposed-object 传输错误；它不是 C# 编译结果，也未影响已完成的 Play/退出证据。

本次只在既有 Editor 测试探针增加独立请求分支；未改生产、DAT、PNG、Scene、Prefab、ProjectSettings 或非战斗逻辑。用户明确保留的完整背景/固定相机、普通 HUD 及暂不绘制名字是跨窗口像素差异的排除项；现有两图只能人工确认本体、足下阴影和左右关系，不足以给出精确非例外像素误差。没有观察到可定位的生产表现首差，因此不启动修复包或扩大角色矩阵。Q09、Q12 和总目标继续按总表的 `FIRST_DIFF_ONLY` 条件门开放。

交付前运行 `Tools/Validate-ChangeLedger.ps1`，退出码0，报告 `PASSED`、1264份 Record、当前代码差异1文件且被本 Change ID 覆盖；[完整日志](change-ledger-validation-20261005.log)含旧 Record 指向不在当前 diff 中的历史警告。`git -c core.safecrlf=false diff --check` 退出码0。未运行全量 EditMode/SelfCheck或其它角色 Play，因为本包只改测试探针且已有对应原 Scene 定向结果。
