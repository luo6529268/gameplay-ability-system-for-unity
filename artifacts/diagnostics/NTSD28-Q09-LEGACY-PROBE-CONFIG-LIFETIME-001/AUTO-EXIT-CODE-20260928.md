# Q09 探针退出时副本归还：代码与导入状态（2026-09-28）

状态 `RUNTIME_PENDING`。此前手动清理已使三个已加载 LegacyOnly 探针 GameConfig 副本归零并让 P-02 的中央 30/60/120 FPS 验收通过，但原 `EnteredEditMode` 回调只读取静态 `GameConfig.Instance`；它在退出 Play 时可为空，无法看到仍被 Unity Resources 保留的 `DontSave` 副本。

本轮只改 `NTSD28Q09KarinState9997BattlePlayProbeEditor.RestoreProbeConfigAfterPlay`。回调仍只在 `EnteredEditMode` 运行；先确认保存的 GameConfig Asset、静态单例身份和所有已加载非 Asset GameConfig 对象，再处理通过现有 `IsProbeConfigClone` 条件的对象。若发现任何不认识的对象或没有探针副本，就不改变内存状态；只有存在精确副本时才归还这些临时对象、绑定保存 Asset，并写 `auto-exit-retirement.txt`。两个 Q09 探针生成的旧式副本满足同一身份门，故由这个已登记的 Editor-only owner 做退出孤儿清理。没有修改 `BattleTestBootstrap`、生产配置、DAT、Scene 或非战斗代码。

生成 Editor 工程构建为 0 错误/199 警告。原 Editor MCP Refresh 后的 `Assembly-CSharp-Editor.dll` 时间晚于源文件；随后只读 EditMode 清单为 `GameConfig.Instance=null`，已加载对象仅保存的 CentralOnly `GameConfig` Asset 一份，探针副本0。GameConfig Asset SHA-256 `0527D737...CB8EA7`、Battle Scene `2EE465D8...B48B77A`、Menu Scene `785F828C...81E13` 未改变。

这只证明代码已导入及干净基线。尚未新建一份 opt-in Legacy 探针副本并实际退出 Play，因此 `auto-exit-retirement.txt` 目前不应有本次的成功行；还需一次定向 Legacy Play→退出→读取归还计数和下一次 Central-only 入口，才能将自动归还报为已验证。不要为这项再运行全角色或技能矩阵。
