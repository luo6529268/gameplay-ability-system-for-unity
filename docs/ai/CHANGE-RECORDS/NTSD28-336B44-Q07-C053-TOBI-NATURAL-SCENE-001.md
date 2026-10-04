<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053TobiNaturalBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and playable GameSession28 Tobi OID0 natural hit_Fa OPoint chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001.md
-->

# Q07/C053 Tobi 正确 OID0 自然出生原场景探针

2026-10-04 限定验收：原Editor唯一clean Battle Scene执行一次14完整生产Driver tick，独立原件`artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/tobi-jump-natural-01.json`保存，OID251在tick11/slot50/action51自然出生。正式源CSV/Unity八项映射及状态字段×14tick的[配对](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/tobi-jump-natural-01-paired.json)为112/112零差；正式源/根此前同例40tick所选320/320同。原Editor退出非Play/Scene clean，Battle/Menu/GameConfig/ModeAsset本轮前后SHA一致；独立EditMode菜单读回[残留](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/tobi-jump-natural-01-postplay.json)为`SCOPED_PASS`：1个原Scene序列化Driver中World非空0、Pool组件0、Battle SHA未变。新脚本原Editor DLL及生成工程0错/299 warning，Change Ledger待末次验证。`VERIFIED`仅指此探针及14tick自然出生子门；物理键、后26tick全World、自然action0双Uj、其它C053和Q07/Q12保持开放。[完整报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/REPORT.md)。

2026-10-04 二次脚本修改前登记：14tick原场景证据已出，112/112所选字段零差、Scene clean且四SHA不变；EditMode里`liveDriversAfter=1`对应序列化原Scene Driver，但此前探针未读其`World`。仅在本Record已声明脚本加独立 `Post Play Residual` Editor 菜单，读取不可创建的现有Driver/Pool和结果原件哈希，写唯一新JSON；不重新Play、不修改原结果/Scene或任何生产代码。编译、菜单读回后再决定关闭残留子门，旧结果保留。

2026-10-04 编译门：原Editor `refresh_unity(mode=force,scope=all)`后新`.meta`生成且`Assembly-CSharp-Editor.csproj`第380行收录本脚本；原Editor DLL UTC 06:58:43晚于脚本06:54:35，MCP回到非Play/非编译/无测试、唯一Battle Scene clean。生成Editor工程重新`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0、0 error/299 warning；先前生成工程0错但未收录新文件，只保留历史，不算本脚本编译。原Scene四保护SHA已读取，真实Play尚待。

2026-10-04 实际脚本已写：新增所声明的 `NTSD28Q07C053TobiNaturalBattlePlayProbeEditor`，仅有菜单入口、Play clone roster OID0/2、正式源初态/seed/mode0设置、14个离散生产Driver tick采样和EditMode退出哈希/残留计数；使用`FileMode.CreateNew`拒绝结果覆盖，没有请求文件或DAT/生产改动。脚本与新增.meta待原Editor导入；编译、真实Play、正式CSV逐tick配对、退出保护结果均尚未运行。回滚边界仍按Task，保留并行用户内容。

脚本修改前登记。需求与当前状态见[Task](../TASKS/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001.md)。本 Change 只增加一个原 Editor 测试脚本，复用已有 Battle clone/bootstrap、正式角色内容、离散输入、生产 Driver 和退出验证范式；不添加任何生产游戏分支或 DAT 特判。

预期副作用：原 Editor 单次 Play clone 里暂时设正式 OID0/2 roster 并推进14 tick，写独立 JSON 结果。Scene/Prefab/GameConfig/ModeAsset 不保存，旧结果不覆盖；并行 UI、HUD、其它任务的未提交文件不触碰。无关脚本、音频、DAT、角色图与非战斗逻辑均非本 Change 路径。

验收、风险和回滚见 Task；实际代码符号、编译、聚焦运行时与场景清理证据须在修改后追加。用户批准的正式内容与地图/模式 DAT 排除项不改变。状态在真实运行前不能超过`COMPILE_PASS/RUNTIME_PENDING`，不得把正式源/根阳性冒称Unity已验。
