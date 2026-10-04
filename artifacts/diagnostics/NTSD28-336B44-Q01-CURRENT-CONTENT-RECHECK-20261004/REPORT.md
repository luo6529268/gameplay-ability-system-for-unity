# Q01 当前正式内容身份与 system 图像消费者回访

2026-10-04，状态：`CURRENT_STAGED_IDENTITY_PASS / SYSTEM_IMAGE_BATTLE_CALLSITE_SCOPED_CLOSED / Q01_VISUAL_EXIT_OPEN`。正式规则身份为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本轮只读正式 `resources/runtime`、Unity `Assets/NTSD/Content/LoganRuntime` 与对应 playable 构建源码，没有复制、编辑或删除 DAT、图片或其它项目资源。

[当前重新计算的逐文件结果](REPORT.json)从先前 338-DAT、1031-PNG 清单逐条读取**两端现存文件**并重算 SHA-256：正式对象/全局 DAT 338/338 与旧正式清单同，Unity 暂存 DAT 338/338 与旧暂存清单同，双方按 CRLF→LF 归一后 338/338 相同；PNG 正式与暂存各 1031/1031 同先前 SHA 且双方 1031/1031 字节相同。缺失、内容漂移均 0。`catalog.csv` 与 `decoded_dat/data/data.txt` 两端各自 SHA 相同。正式目录仍为 405 DAT/1255 PNG，暂存目录仍为 338 DAT/1031 PNG；差额继续受用户背景/模式 DAT 排除、默认 stage 暂缓、项目自有表现例外和实际消费者门约束，不能因为数量差而批量复制或删除。

先前 [未暂存图像 owner 清单](../NTSD28-336B44-Q01-FORMAL-ONLY-OWNER-RECHECK-20261001/missing-sprite-owner-matrix.csv)中，`data/system.dat` 精确引用但 Unity 未暂存的九张是 `sprite/menu/1.png`～`8.png` 和 `sprite/loading/menu_wait.png`。当前 playable 源码的 `GameSession28::load_native_loading_config28` 只读取 `<menu_wait>` 的 `pic` 与 `<loading>` 几何，再把等待图解析为 loading 背景；该路径在战斗进入前的加载流程消费。`<menu_back_1>`、`<menu_back_2>` 在所检 Core/Playable C++ 源码中没有相应字段解析或图像读取，`NativeSystemVisualConfig28::parse_text` 只读取 `spark_w`/`spark_h`；在当前源码中也未找到这八张 `sprite/menu/*.png` 的直接路径读取。故九图没有已证**当前 playable 活跃战斗**消费者，不把菜单/加载图搬进战斗资源。这里仅封闭所检源码的直接调用点；正式根 EXE 的所有隐含条件、其它未检运行时路径以及战斗 Game View 等价性并未由负搜索证明。

当前 Q01 的非排除活跃战斗直接资源仍是 WORDS0～5 与 SPARK 七图，已暂存且逐 SHA 同版；现有 WORDS1 与 SPARK 的 Unity Game View 子证有效。完整战斗画面、其它 Q09 出口和 Q12 总验收仍开放。下一步应沿**已证战斗消费者**找当前可达的画面或逻辑首差，不重测这批未变的 338/1031 文件，也不修改用户排除资源。
