<!-- CHANGE-RECORD
id: NTSD-MENU-FONT-3500-PLUS-REBIND-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/MenuCarouselVisualEditorTests.cs
authority: User 2026-10-04 requests changing MenuFont3500MigrationEditor to use JifengBladeArtSC-3500-Plus SDF.asset throughout
evidence: docs/ai/TASKS/NTSD-MENU-FONT-3500-PLUS-REBIND-001.md
-->

# NTSD-MENU-FONT-3500-PLUS-REBIND-001

原状：菜单已由前一 Change 迁至 3500，当前 Menu Scene 仍有 26 个 3500 TMP 字体引用、1 个输入字段引用、1 个 Plus TMP 引用。现有工具仍以 0480 为旧字体、3500 为目标，且六个自定义材质修复入口仍指向 3500。Plus 是用户新建的未跟踪资产，只有 10 个预载字形；源 TTF 可覆盖现有 3500 字库的 59 个字形。场景、旧 3500 SDF、工具和治理文档已有用户修改，均保留。

预期修改：将迁移输入改为 3500、目标改为 Plus，菜单入口、图集材质处理、迁移前快照清单和诊断文本同步更新；菜单视觉测试引用 Plus GUID。只在原 Editor 的安全门槛满足后，用编辑器 API 修改 Plus 字库和 NTSD_Menu 场景，不直接改 YAML；不修改旧 3500 资产或 Battle。

副作用与边界：Plus 图集可能重建、Scene TMP/输入与场景材质贴图将重绑。需要保留已绑定 Plus 的文字及自定义颜色、描边。不可覆盖用户新建资产的当前字形而不先保存其精确备份；场景若内存脏或磁盘 SHA 改变，停止迁移。

验收：生成 Editor 编译 0 error；迁移工具预检字形且带 SHA 守护；可执行时 Scene 3500 序列化引用清零、Plus 绑定覆盖目标组件、菜单 Play 用例通过；未做的 Play/设备验收标明。完整 Task 和逐文件备份见 `artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/prechange-manifest.json`。

回滚：仅在核对最新文件状态后从本任务逐文件备份恢复，避免误覆盖之后的用户编辑；文档追加纠正。旧 3500 SDF 不动，能按原 GUID 重新迁回但需另立受控操作。

实施结果（2026-10-04）：`MenuFont3500MigrationEditor` 的输入改为现用 3500 SDF、输出改为 3500-Plus SDF，命令改为 `NTSD/UI/Rebind Menu Font 3500 Plus`，准备清单移至本 Change 目录。移除针对旧 0480→3500 的第二修复入口，主迁移会统一扫描当前单一 Menu Scene 的场景材质，将 3500 命名材质的 `_MainTex` 重绑至 Plus 且保留其它材质参数；已绑定 Plus 的 TMP 文字纳入字形预检。测试中新建标签的 GUID 改为 Plus GUID `c10a07540530f01408fcb83191a6f0f3`。脚本仍保留已验证的“临时字图集预检→完整重建目标图集”路径，避免嵌入 TMP fork 在追加字形时产生重复字形索引。

验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 最终完整构建 0 error、297 warnings，见 `artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/compile-final.txt`。首轮原 Editor 刷新期间另一任务的 C056 探针脚本出现 CS0136，暂时阻断 Unity 编译；该任务随后修复，完整构建通过，原 Editor 重新加载后 Console 查到 0 error。隔离 C056 的补充构建也 0 error，收据 `compile-isolated.txt`，仅用于说明本工具本身编译。新命令尚未调用，因此没有 Plus 图集重建、场景写入、Play 视觉或运行时验证；按 `COMPILE_PASS` 报告。

操作门槛与未执行：在准备单一 Menu Scene 操作时，原 Editor 进入另一任务的 Battle Play 验证。为避免抢占共享 Editor，本轮不切换场景。当前 `NTSD_Menu.unity` SHA `F01144C9...C45116329F` 与 Plus SDF SHA `DF656A96...8A83A83` 均与前镜像相同；`prepared-manifest.json` 已按这两份精确备份生成，后续只在非 Play、Editor 无其它任务占用、单一干净 Menu Scene 且哈希仍匹配时可调用。Plus 字体运行时字形和全部场景绑定仍待验证；菜单当前仍含原 3500 引用。完整逐文件前后清单在同 ID `prechange-manifest.json` / `postchange-manifest.json`，治理文档并发变更保留原状。

交付审计：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` 退出码 0，输出 `Change ledger validation PASSED`，详 `artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/change-ledger-validation.txt`；本任务脚本与测试、治理文档的 `git diff --check` 退出码 0。未跑 Menu Play 或对 Plus 的画面验收。
