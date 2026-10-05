# Q09 自定义阴影内容可达性只读核查（2026-10-05）

状态：`SOURCE_AND_CONTENT_SCOPE_CHECK / NO_OBJECT_CUSTOM_SHADOW_TRIGGER`。本项只读取当前 336B44 正式 playable 源码、正式 `resources/runtime/decoded_dat` 与 Unity 暂存 `LoganRuntime`，未运行 Editor/Play、未修改脚本、DAT、PNG、Scene 或相机。正式根 EXE SHA-256 本轮重算为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

正式 `source/ntsd28_core/src/rendering/render_snapshot.cpp` 的实体阴影分支（约 1594–1630 行）默认使用 `config.background->shadow_path/width/height`，只有实体 definition 的 BMP 块出现 `shadow_pic` 时才换用自定义图，并可随之读取实体 `shadowsize`。Unity 当前 `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs` 的中央战斗命令（约 2985、3054–3085 行）使用项目配置的 `CommonShadowBinding`，没有逐实体 `shadow_pic` 出口。

对当前正式 `resources/runtime/decoded_dat` 的全部 `.dat` 按大小写不敏感字段名搜索，`shadow_pic` 命中文件数 **0**；`shadowsize` 命中 **24**，全部是 `b/*/b.dat` 背景表。对 Unity `Assets/NTSD/Content/LoganRuntime` 中 `.dat` 同法搜索，两字段命中均为 **0**。因此，在当前正式内容与用户排除原版背景 DAT 的范围内，不能把正式源码的实体自定义阴影图分支列为现有对象的必做迁移或可见首差；项目自己的背景/阴影配置继续按用户例外保留。

这个负证仅针对当前内容字段的可达性，**不证明**普通阴影图的像素、位置、排序、显示门和运动插值全部对齐。已有鸣人/鼬同全局 tick14 正式窗口及 Unity Game View 只证明双方本体和足下阴影可见；若后来出现可复现的非例外阴影首差，再追相应共用表现消费者。新内容若引入对象 `shadow_pic`，也须重新审计，不可沿用本次零命中结论。
