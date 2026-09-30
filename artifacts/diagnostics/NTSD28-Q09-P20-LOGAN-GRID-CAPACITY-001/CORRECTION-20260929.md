# P-20 Hidan pic119 解释更正

本文件覆盖本目录 `ACCEPTANCE.md` 中“飞段 frame430/pic119 被首张声明sheet容量拒绝，后续sheet不兜底，因此正式版不发布本体”的结论。该结论把文本 `file(62-126)` 当成运行时 `first_pic/last_pic`，漏读了正式 `dat_parser.cpp::project_sprite_sheet` 的累计容量投影。正式 Hidan 运行时 hid2 为62–116，hid6为117–128；pic119归hid6，正式根自然frame430 trace 记录两个 Hidan 的 `render.sprites=2`，与可解析本体一致。

既有 `BuildIndexedSpriteRects` 对**真正超出运行时 sheet 容量的局部索引**拒绝、普通越图 CLAMP 和聚焦测试中通用容量行为仍是有限证据；原 Hidan 文本范围测试会改用本地正式 decoded DAT 实际投影。原 Unity Battle 两轮自然frame430的中央 actor 命令 pic119 已记录，误判的 `OBSERVED_DIFFERENCE` 不作为生产首差。实际 Unity source sheet 和两端像素尚待比较；不能从此次纠错宣称 Q09 完成。
