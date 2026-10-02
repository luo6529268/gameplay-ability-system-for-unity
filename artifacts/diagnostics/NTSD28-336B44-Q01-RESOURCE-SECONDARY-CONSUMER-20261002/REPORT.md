# Q01 `resource.dat` 次级路径消费者更正（2026-10-02）

状态：`READ_ONLY_SELECTION_OWNER_CORRECTION / Q01_NONOBJECT_VISUAL_PENDING`。权威仍为根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live path。本轮未修改、复制或删除 DAT、PNG、Unity 脚本、Scene 或配置。

前一份 [55 项矩阵](../NTSD28-336B44-Q01-RESOURCE-PLAYABLE-CONSUMER-20261002/REPORT.md)只按 `NativeResourceCatalog28::resolved_path` 的固定索引调用分类。复核正式 playable 其它路径后，原列为 `NO_DIRECT_PLAYABLE_RESOLVER_FOUND` 的索引 7、8 有明确的**按虚拟路径直接读取**消费者：

| `resource.dat` 索引 | 图 | 正式调用链 | 归属 |
| --- | --- | --- | --- |
| 7 | `sprite/UI/BG_RANDOM.png` | `GameSession28::selection_snapshot()` 的随机背景分支，`game_session.cpp:3586-3589`，以 `resolved(...)` 设置 `background_preview_source_path`；`d3d11_renderer.cpp:1399` 读取纹理。 | 选人后的背景预览，不属于战斗场景。 |
| 8 | `sprite/UI/BG_FRAME.png` | `GameSession28::selection_snapshot()` 在 `game_session.cpp:3563-3564` 设置 `background_preview_frame_source_path`；`d3d11_renderer.cpp:1400-1401` 读取纹理。 | 同一选人预览框，不属于战斗场景。 |

`game_session.cpp:3560-3562` 还说明这两个固定路径对应资源表索引 7/8。它们的读取不经过 `NativeResourceCatalog28::resolved_path`，因此旧矩阵的“未找到直接解析点”只适用于所检索的固定索引路径，不能泛化为无 playable 消费者。`game_session.cpp` 和 `d3d11_renderer.cpp` 均在当前 336B44 playable 源码闭包内；此结论是源码中的选人消费链，不是正式 EXE 实际截图或 Unity 画面验证。

[修订版 55 项矩阵](resource-55-path-matrix-v2.csv)只更改上述两行的 `consumerScope`，原矩阵保留。主表 48 项现为：活跃战斗直接读取 7、原生结果板用户排除 7、剧情结果暂缓 1、选人流程 12、加载流程 1、尚无明确 playable 直接消费者 20；另有独立 `<frame>` 七图不在原生 48 项索引表，仍标消费者未证。正式端 55/55 文件在位、Unity 暂存 7/55、活跃战斗七图 7/7 逐 SHA 同版等原结论不变。

后续 Q01 不因选人图缺失搬入战斗资源；20 张主表及七张独立帧图保持真实消费者/画面条件门。尤其“未找到源码直接消费者”不是“正式 EXE 永不显示”的证明。Q01 非例外 Game View、Q09/Q12 仍开放。
