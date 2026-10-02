# Q01 `resource.dat` 图片的新版 playable 直接消费者矩阵（2026-10-02）

状态：`READ_ONLY_DIRECT_CONSUMER_SCOPED / Q01_NONOBJECT_VISUAL_PENDING`。正式规则身份为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；本项只读当前 `resources/runtime/decoded_dat/data/resource.dat`、对应 playable 源码、Unity 当前 `LoganRuntime`，没有修改或搬运 DAT/PNG、Scene、脚本和配置。

[55 条逐路径矩阵](resource-55-path-matrix.csv)从当前正式 `resource.dat` 的 `<bmp_begin>` 48 张及独立 `<frame>` 7 张逐条解析，重读正式/Unity 暂存 `vfs` 文件计算 SHA。48 张主表中，暂存7、缺41；7 张 `<frame>` 暂存均缺。正式端55/55均存在。`NativeResourceCatalog28` 只解析前48张主表，头文件明确 `<frame>` 七图不属于当前固定表；因此不把 7 张附加帧图误算成原生 index48～54。

按**当前 playable 源码里的直接解析调用**分类 48 个主表索引：

| 类别 | 索引/数量 | 当前内容和范围 |
| --- | --- | --- |
| 活跃战斗明确读取 | 16～21、43；7张 | `GameSession28::initialize` 将 WORDS0～5 和 SPARK 交给 render snapshot；Unity `LoganVisualContentCandidate.NativeWordsInput`/`NativeSparkInput` 也按相同索引捕获。7/7 暂存，7/7 与正式原始 PNG SHA 相同。此项是磁盘身份和静态消费关系；既有隔离发布证据另见 Q01 原报告，非 Game View 像素。 |
| 原生战斗结果板 | 24～26、28～31；7张 | `GameSession28::snapshot` 在 mode0/result_visible 为 scoreboard 绑定，正式可达但当前用户将原生结果图文排除于战斗对齐范围。战斗计时/胜负事件仍由 Q08 验；不因图缺失复制结果页资源。 |
| 剧情结果 | 3；1张 | 只在已激活剧情任务结算条件下绑定。默认 `stage.dat` 部署用户暂缓，不把剧情入口当普通战斗画面。 |
| 选人流程 | 0、32～38、41、42；10张 | `GameSession28::selection_snapshot` 的背景、光标、头像及人数提示；当前战斗场景之外。 |
| 加载流程 | 47；1张 | `load_native_loading_config28` 消费；当前战斗模拟之外。 |
| 本轮未找到 playable 直接解析点 | 22张 | 仅说明所检 `game_session.cpp` 的 `NativeResourceCatalog28::resolved_path`/固定索引调用与已检 renderer 传递路径没有对它们建立明确战斗消费者，**不推断正式 EXE 不显示、不自动判排除**。若正式可观察战斗画面出现，需按该帧再定位/补证。 |

独立 `<frame>` 的7张均在 Unity 暂存根缺失；当前 `NativeResourceCatalog28` 明确不收纳它们，所检 playable 直接消费者未建模。它们保持 `UNRESOLVED_CONSUMER`，不与48张主表混淆。当前正式 `sprite/combo_hits.png` 与暂存版本 SHA 均为 `1D398C0AE0AE01495B7FF5FEC78B7978690CFE53B98D10913E1BCEC92426AD48`；它来自可选 combo 配置，不属于上述 `resource.dat` 55 图。所选正式根 LFR 样本的 `nativeComboRuntimeDisplayEnabled49FD8=false`，仅说明该样本默认门关闭，不替代其它配置验收。

代码核对点：正式 `source/ntsd28_core/include/ntsd28/native_resource_catalog.h` 定义48槽及16～21/32/43/47常量；正式 `source/ntsd28_playable/src/game_session.cpp` 约1216～1245为战斗 WORDS/SPARK，约3316～3389为 render snapshot/结果板/剧情结果，约3508～3525为选人快照，约367～424为加载；Unity `Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs` 的 `NativeWordsInput.Capture`/`NativeSparkInput.Capture` 使用相同战斗索引。此为当前源码可达静态分类，不是根 EXE 截图或 Unity 视觉等价证明。

前一份 [114张正式独有非背景图 owner 清单](../NTSD28-336B44-Q01-FORMAL-ONLY-OWNER-RECHECK-20261001/REPORT.md)还包括菜单/等待图、模式 DAT 例外、原生 HUD、minibar 等；本矩阵只细化其 `resource.dat` 48主表+7附加帧。Q01 的非例外视觉、无直接解析点的22张及 `<frame>` 七图仍按真实画面/消费者开放；Q09、Q12 也不能由7/7磁盘身份自动关闭。
