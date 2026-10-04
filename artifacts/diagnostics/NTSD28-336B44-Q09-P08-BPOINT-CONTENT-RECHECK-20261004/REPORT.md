# Q09/P-08 当前 336B44 BPoint 内容分支回访

2026-10-04。结论：`CURRENT_CONTENT_DEFAULT_BRANCH_CONFIRMED / RUNTIME_AND_GPU_PENDING`，仅确认新版正式内容进入血点默认值分支，不关闭 P-08、Q09 或 Q12。原 Editor 当前为单一 Battle 场景、idle、非 Play；本轮没有启动 Play，也没有修改 DAT、图片、生产脚本或 Scene。

身份：正式根 `NTSD2.8-Logan.exe` 本轮重算 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。Q01 [现存内容逐文件复核](../NTSD28-336B44-Q01-CURRENT-CONTENT-RECHECK-20261004/REPORT.md)已证正式/Unity 暂存 338 DAT 经 CRLF→LF 归一同版、1031 PNG 逐原始 SHA 同版。

对正式 `resources/runtime/decoded_dat` 全部 405 个 `.dat` 的 `bpoint: ... bpoint_end:` 跨行块重新扫描：318 块，只在 `c/ita/ita.dat`（231）与 `c/sasu/sasu.dat`（87）；`respond`、`w`、`h`、`rect` 在所有块中均未出现。全树没有 `bleed_hp` 字段。当前 playable 闭包 `render_snapshot.cpp` 的 `bpoint` 路径因而使用 base_max_hp/3、1×3、默认红色；Unity `BattleBloodPointValue` 的 X/Y 契约及 `BattlePresentationShadowBuild` 的默认门槛、1×3 红色命令对应当前正式内容的值分支。C++ 保留额外字段读取，当前内容不触发；不能凭源码存在的休眠分支修改生产 schema 或 DAT。

早期 [P-08 内容审计](../NTSD28-Q09-P08-BPOINT-FORMAL-CONTENT-SCOPE-20260929/REPORT.md)绑定 B1E13，今天的重扫仅把**正式内容默认值分支**更新为 336B44 证据。旧 [Unity 自然鸣人 J→鼬命中](../NTSD28-Q09-BPOINT-NATURAL-HIT-PLAY-001/ACCEPTANCE.md)和 [playable WARP 标记](../NTSD28-Q09-P08-FORMAL-NATURAL-BLEED-WARP-001/ACCEPTANCE.md)仍按其旧版本标签阅读，不能自动晋升为新版根 EXE 与 Unity 同状态 GPU 对照。当前 Q09 真正剩余的是同初态/同 tick 的新版根可观察帧与原 Unity Game View 配对，以及该链的显示位置、遮挡和像素；若没有已证新首差，不重复旧自然测试矩阵。
