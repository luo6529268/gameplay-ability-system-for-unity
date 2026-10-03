# Q09/F02 鸣人黑格与 BattleControls 归属（2026-10-03）

结论：原 Battle Scene 的鸣人黑色单元格不是 `BattleControls` 摇杆 UI 绘制出来的。相对 tick39 的同一暂停逻辑状态下，临时隐藏并恢复该 GameObject 后，黑格仍在；摇杆原来遮住的黑格下半部分反而暴露。该运行的有效战斗像素模式为 `CentralOnly`。具体中央绘制命令、图集 slice、材质或采样数据的第一差异尚未证实，Q09 战斗表现不关闭。

证据：原项目唯一 Editor 从干净 Menu 进入原 Battle Scene，使用独立 opt-in `f02-black-cell-20261003-01`。同一 run 的 [普通 tick39 图](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-black-cell-20261003-01/game-view-tick39.png) 与 [BattleControls 暂隐图](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-black-cell-20261003-01/game-view-tick39-no-controls.png) 均为 1920×1080 PNG；后者 SHA-256 `B51FC41405A8A24C245A6E78F37FAB651E641A7409C1FD904BB4C51F`。在左下 `(180..449,760..959)` 检索各 RGB 通道≤3 的最大连通区，普通图为 `(216,806)` 起 `124×101`、面积 9,420；暂隐图为 `(216,806)` 起 `127×123`、面积 15,146。消失的是摇杆图像，黑格仍在；面积变大由原被 UI 遮挡的黑像素可见，与视觉检查一致。

正式 `vfs/c/nar/nar.png` 的 pic1/pic95 源格各 `79×79`，只读解码分别有 5,220/5,058 个 alpha=0 像素；源 PNG 不是整格不透明黑图。当前 probe 的 `actor.Sprite.CurrentEntry` 在 `CentralOnly` 下未绑定，报告中的 `narutoSpriteSourceSheetPath`/`narutoSpritePixelRect` 因而为空，不能把这一字段误当成图集已核验。正式 DAT 内容未改。

[最终报告](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-black-cell-20261003-01/00059.json) 为 `CAPTURED / DONE`、空错误；普通两图及暂隐图已验有效尺寸/哈希。`before`、`after`、46组状态样本、47组事件均与原无截图run-07序列化值完全相同；临时 UI 关闭及截图未改变已声明的完整45tick模拟。探针恢复了 BattleControls，Play 有序关闭后 World对象/运行槽/池借用/活动对象及Sprite均0，pool quiesced、World detached，回干净 Menu，四保护哈希稳定，原 Editor 再次 idle/nonPlay。

后续应在 Q09 范围对同一 `CentralOnly` 帧的 Naruto Entity 绘制命令、catalog key/源格、图集页与实际 GPU alpha 输出做一条最短路径的首差诊断，再决定生产修复。当前没有证据支持改 DAT、原版图片或 BattleControls UI，也没有取得正式 EXE 的同条件像素图。
