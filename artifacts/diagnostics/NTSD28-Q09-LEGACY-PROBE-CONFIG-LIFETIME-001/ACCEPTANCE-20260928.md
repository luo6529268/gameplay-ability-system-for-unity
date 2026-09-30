# Q09 探针 GameConfig 已加载副本清理限定结果（2026-09-28）

状态：`VERIFIED_SCOPED_MANUAL_RETIREMENT_ONLY`；Change `NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001` 保持 `RUNTIME_PENDING`，因为自动 `EnteredEditMode` 退场后的孤立对象清理尚未另做反复创建/销毁证明。此包只有 Editor 测试脚本诊断入口和内存对象清理，没有生产脚本或磁盘 Asset/Scene 改动。

两次原 Battle Scene Q09/P-02 Play 在采样前读到非 CentralOnly World；保存的 `GameConfig.asset` 序列化为 CentralOnly，且两个 Q09 请求文件均无 active `requested/running`。首次 EditMode 检查发现 `GameConfig.Instance == null`；第二次 [`idle-config-inspection.txt`](idle-config-inspection.txt) 列出三个 `GameConfig(Clone)`：每个都是非 Asset、LegacyOnly、`DontSave` 且通过现有 `IsProbeConfigClone` 身份门，另有一个保存的 CentralOnly Asset。`BattleTestBootstrap` 在单例为空时会从 `Resources.FindObjectsOfTypeAll<GameConfig>()` 取第一项；该清单中的前三项均为 Legacy 副本，因此是当前两次后端前置失败的具体可达原因。单凭此前单例为空不能排除这条 fallback。

专用 EditMode 菜单动作先遍历所有已加载 GameConfig；只有全部非 Asset 对象均通过精确探针副本校验、且单例为空/保存 Asset/同类副本时才继续。动作结果见 [`idle-config-retirement.txt`](idle-config-retirement.txt)：`retired=3`、`remainingProbeClones=0`、`singletonIsSavedAsset=True`。随后原 Battle Scene 一次聚焦 Play 以 CentralOnly 完成 30/60/120 FPS 展示采样；退出 Play 后单例仍是保存的 Asset，已加载 GameConfig 仅剩该 Asset，Battle Scene `isDirty=false`。P-02 同 tick 校验和见[独立验收](../NTSD28-Q09-P02-VISIBLE-CONSUMERS-001/FPS-CHECKSUM-PLAY-ACCEPTANCE-20260928.md)。

生成 Editor 工程两次构建均 0 错误/199 警告，原 Editor Refresh 导入并执行菜单。保存的 GameConfig Asset SHA-256 `0527D737...CB8EA7`、Battle Scene `2EE465D8...B48B77A`、Menu Scene `785F828C...81E13` 前后保持。仅移除本次诊断证实的三个内存探针副本，未删除文件、修改 DAT 数值或改变非战斗逻辑。未来 opt-in Legacy Play 是否继续产生游离副本及其自动归还仍为本 Change 的开放项；不能把一次手动清理写成完整生命周期修复。

收尾时原 Editor 已返回 `NTSD_Menu`、idle、非 Play，Menu Scene clean。
