# Q09/P-20 正式内容 pic 范围分支限定核对

状态：`READ_ONLY_AUTHORED_PIC_RANGE_MAP / P20_AGGREGATE_OPEN`。本项核对当前正式对象 DAT 的原始 pic 与累计 sheet 容量，不评价缺文件、未定义 action、动态帧可达性或最终 GPU 画面。

2026-09-29 重新核对正式根 `NTSD2.8-Logan.exe` SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。只读扫描其 `resources/runtime/decoded_dat` 下全部 405 份 `.dat`。其中 `data/resource.dat`、`data/frame/INKHUD.dat`、`data/frame/INKHUD2.dat` 是三份非对象特殊格式；对其余 402 份逐个提取 `<frame> ... <frame_end>` 块，并在每块内解析整数 `pic:`。共找到 **55,347** 个标准帧块，**55,347/55,347 均有 pic**，最小值 **0**、最大值 **1088**、负值 **0**；其中 `pic: 999` 有 **4,587** 个。全目录独立文本搜索 `pic:\s*-[0-9]+` 亦无命中。

正式配对源码 `source/ntsd28_core/src/rendering/render_snapshot.cpp::RenderSnapshotBuilder28::build` 在取得帧记录后计算 `base_pic + revive_visual_runtime_318`，`SpriteFrameResolver28::resolve` 对负结果不绘制。Unity `LF2Entity.GetRenderPicIndex` 对原始负 pic 提前返回，而其余整数先加 `Runtime.RenderPicOffset`。这两个实现对**假设存在的原始负 pic + 正偏移**顺序不同；但当前 402 份标准对象 DAT 没有原始负 pic 或缺 pic 的帧，故此分支目前没有由这些内容触发的证据，不能据此新增生产特例。正 pic 的偏移路径、`pic999` 隐藏路径和缺单张本体 sheet 各按已存专项证据处理。

同一批正式内容的 [机器可读汇总](range-summary.json) 与 [84行精确清单](out-of-range-non999-frames.csv) 进一步按正式 `dat_parser.cpp::project_sprite_sheet` 的累计 `max(row*col,0)` 容量扫描：55,347帧中 **50,676** 个原始pic在本对象声明范围内，**4,671** 个不在；后者为 `pic999` **4,587** 个，以及其它正值 **84** 个，分布在 `data/data.txt` 已索引的 **21/21** 份对象DAT。正式resolver对这些原始越界pic不发布本体；当前正式复活偏移写者只写正数或清零，不会把原始已越过末尾的pic移回范围。Unity Logan内容把同一累计范围交给生产 `BattleSpriteCatalog`；中央命令要求目录键，Legacy生产目录路径 `LF2Sprite.ResolvePicManagedOnly` 在键不存在时清除当前图。因此这84个原始越界帧也没有“继续显示上一帧本体”的静态证据；尚未做84帧动态可达/同条件Play，不能把此静态映射写成整域验收。

正式 `render_snapshot.cpp` 的额外本体可见门允许 mode1 且对象BMP `effect:1` 绕过 `render_phase_008 > -25`；本次402份标准DAT的 `<bmp_begin>...<bmp_end>` 扫描中，BMP `effect:1` 文件数为 **0**。这只排除当前标准内容对该特例的静态触发，不宣称未来内容或其它mode分支已验。

限制：文本块提取不是正式 DAT parser 或动态帧可达性证明。三份特殊格式未参与标准帧统计；未来换内容、新 DAT、动态定义或未定义 action 时须重新判定。正值越界帧的自然命中、对象生命周期、缺图和正式根 EXE 自身窗口画面仍属于 P-20 后续门槛。本项没有运行 Unity/Play，也没有修改源码、DAT、图片、Scene、Asset 或非战斗逻辑。
