# Q01 `resource.dat` 独立 `<frame>` 七图的读取边界（2026-10-03）

状态：`READ_ONLY_PLAYABLE_SOURCE_BOUNDARY / NO_CONFIRMED_BATTLE_CONSUMER`。规则及资源身份仍以根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 和对应 playable 源码/`resources/runtime` 为准。本次只读，不修改、复制或删除 DAT、PNG、Unity Scene、脚本或配置。

正式 `resources/runtime/decoded_dat/data/resource.dat:1-50` 的 `<bmp_begin>…<bmp_end>` 主表含 48 个 `pic:`，第 53–61 行另起 `<frame>…<frame_end>`，列出 `sprite/UI/extra/` 下七图：`recording_background`、`human`、`BG1o1`、`BG2o2`、`branch`、`branch2`、`player`。这七张正式文件均在位，Unity 暂存目录未部署；逐项路径与 SHA 见已有 [v3 矩阵](resource-55-path-matrix-v3.csv)。

当前 playable 的 `source/ntsd28_core/src/rendering/native_resource_catalog.cpp:39-81` 只在遇到 `<bmp_begin>` 时解析并收入至多 48 个索引；外层遍历不会把随后 `<frame>` 的 `pic:` 放入 `entries_`，`resolved_path()` 只能按该索引表读取。`source/ntsd28_playable/src/game_session.cpp:1217-1244` 加载此 catalog，并由固定索引选择战斗 WORDS0～5/SPARK。另一个同名概念 `NativeFrameHudCatalog28` 在 `game_session.cpp:1280-1306` 读取的是独立 `data/frame.dat` 及其选中的 `data/frame_ui` 子 DAT，不读取 `resource.dat` 后面的七张图。上述源文件均在当前 playable 构建闭包中。

对当前 `source/ntsd28_core/src`、`source/ntsd28_playable/src` 逐文件名/`sprite/UI/extra` 检索，无这七个虚拟路径的直接引用；对正式 `resources/runtime/decoded_dat` 检索，七个路径只出现在上述 `resource.dat` 第 54–60 行。这证明**所检当前源码路径未给出七图的战斗消费者**，不能把它们当作战斗必需图片盲目部署。静态检索不证明正式 EXE 在所有运行条件下绝不使用它们，也不裁决旧原生 UI 或历史版本。若以后出现正式 EXE 可见战斗画面或新可达间接调用，再按具体消费者重开。

这项边界不关闭 Q01：主表仍有 19 张未找到明确直接消费者；索引 27 `SCORE_BOARD4` 属用户排除的结果图，消费者仍未证；非例外 Game View 和其它已声明出口继续开放。既有 v3 矩阵保持原件，未改动其 55 项原始路径与哈希。无资源删除授权产生。
