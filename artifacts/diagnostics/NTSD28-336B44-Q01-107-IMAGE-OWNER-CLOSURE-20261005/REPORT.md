# Q01 对象 DAT 1013 图与生产候选 906 图的差额归属

状态：`READ_ONLY_OWNER_GAP_CLOSED / Q01_BATTLE_VISUAL_EXIT_OPEN`（2026-10-05）。本轮仅核对当前正式 336B44 对应的 DAT 引用、playable live source 和两端现存 PNG；没有改 DAT、PNG、`.meta`、生产代码、Scene 或非战斗内容。

现有 [对象图引用清单](../NTSD28-336B44-Q01-INDEXED-CONTENT-IDENTITY-20261001/object-image-references.csv)有 1013 个不同 PNG 路径。当前 `LoganVisualContentCandidate.Capture` 收集 `BuildCharacterFrameConfigsFromCatalog` 的 `files`、`head`、`small`，以路径去重后恰为 906 张；这与[原 Editor 隔离发布结果](../NTSD28-336B44-Q01-PRODUCTION-CANDIDATE-EDITOR-20261001/REPORT.md)的 906 一致。按同一文件中的引用角色做集合差，余下 **107 个路径**恰为 `smallb` 独占 104、`<menu_face>` 的 `layer:pic` 独占 3，没有第三类。逐文件从正式 `resources/runtime/vfs` 和 Unity `LoganRuntime/vfs` 重新计算 SHA-256，107/107 均存在且双方原文件 SHA 相同。[107 项清单](owner-manifest.csv) SHA-256 为 `57B096573141279B7CABE372D0CE8E7DEE8236EAE43B8F251E5288684A7F8A61`。

当前正式 playable 调用链将 `smallb` 用作原生战斗 HUD 头像：`render_snapshot.cpp` 约 1417 行先读 `bmp.smallb`，缺失才回退 `bmp.small`。此头像的原生 HUD 表现属于用户已保留 Unity 自有 HUD 的 P-17 例外。三张 `layer:pic` 位于对象 DAT 的 `<menu_face>` 段；`GameSession28::selection_snapshot()` 在 `game_session.cpp` 约 3746 行填充选人图层，`d3d11_renderer.cpp` 约 1240 行绘制，属于本战斗任务排除的选人流程。它们的文件内容仍已按 D-023 暂存，未因表现例外删除。

因此 `1013 - 906 = 107` 是**候选消费者范围差**，不是缺失图片或战斗生产候选漏掉 107 个当前非例外帧图；不能据此新增一批加载修复、替换已暂存 PNG 或重跑 107 个角色 Play。此结论只关闭这项数量差的 owner 归属；它不证明 906 张候选图都已在 Game View 自然显示、原生 HUD 与 Unity HUD 画面相同，或 Q09/Q12 整场表现完全对齐。正式 EXE 与 Unity 同帧画面等其余条件仍按当前总表处理。
