# Q07 非排除 DAT 已声明 Sprite Sheet 的字节闭包

状态：`VERIFIED_STATIC_DECLARED_SHEET_BYTES_ONLY / Q07_OPEN`。这是 Q07 内容引用的只读核对，不改变战斗规则、DAT 数值、图片、Scene、相机或非战斗功能，也不是可见画面或全部自然技能验收。

正式根 `NTSD2.8-Logan.exe` 本轮实际 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。输入为项目已有的正式内容解析诊断清单 `artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content.jsonl`，SHA-256 `5F5C6C34CE907BB6D7855B8A1E69D410B3CD577807F202332B8A0DA30ED0146F`，其每行标注 `SOURCE_MODEL_DIAGNOSTIC_ONLY`。该清单不是正式 EXE 的 live render trace，以下结果仅用于声明引用和原字节闭包。

按用户排除的 `b/*`、`data/bg_mode.dat`、`data/bg/*`、`data/mode.dat`、`data/mode/ntsd.dat` 过滤后有 353 份 DAT；此诊断 parser 成功解析其中 350 份。失败的三份是 `data/frame/INKHUD.dat`、`data/frame/INKHUD2.dat`、`data/resource.dat`，不把它们当作“无引用”。从 350 份成功解析的 DAT 中提取 `sprites[].path`，去重后有 **703** 个 Sprite Sheet 路径。对每个路径分别在正式 `resources/runtime/vfs` 和 Unity 所选 `Assets/NTSD/Content/LoganRuntime/vfs` 核实文件并逐文件计算 SHA-256：**正式缺失 0、Unity 缺失 0、同路径 SHA 不同 0**。每个路径、声明它的 DAT、两侧 SHA 和缺失标志记录于 `declared-sheet-sha.json`（SHA-256 `58555F8D2A24B8E08C5259272A61B267BF156DFCD23128D2386A5D9D0AF3DA4E`）。没有复制或改写资源。

三个特殊格式文件另做原字节与 owner 核对：正式及 Unity `data/resource.dat` 同 SHA `B8E31B33A2D42924C57BB6DEC8571F494DD1E3FDDC38916A77471119526090E6`，且正式 `native_resource_catalog.cpp`、Unity `LoganVisualContentCandidate.cs` 均有专用读取路径；正式及 Unity 活动 `data/frame/INKHUD.dat` 同 SHA `54D0B199F8D305C2B97718620A9C8E639FC7D5DEB09FDE62194E2D1D18DFF9D0`。`INKHUD2.dat` 正式 SHA `0475B1F160A96A0A0E5ECCAF6FEC6129AF7F209CB73001978E675EBF94BFB2D0`，Unity 当前未暂存；既有正式 playable 审计证明活动 frame HUD 索引固定为 0、`INKHUD2` 属索引 1，见 `NTSD28-Q07-BACKGROUND-HUD-CALLER-AUDIT-001/REPORT.md`。这不是对专用 parser、资源索引或原生 HUD 画面的新运行时验收，也不要求复制用户排除或尚未证明可达的图。

这个结果消除了“350 份可解析非排除 DAT 的已声明 Sprite Sheet 仍缺字节”的当前候选，却没有证明特殊格式内各 resource 索引、静态归属但消费者未证的 8 张图片、运行时 catalog/GUID 绑定、所有 frame 的资源可见性或真实画面。它也不授权删除旧 521 项；受保护辅助读者和既有 `deleteAuthorized=false` 保持。Q07 的新可达内容/自然技能出口及 D-024 共用碰撞域仍未闭，Q09/P-20 缺资源与隐藏 pic 的 fail/skip 矩阵另归 Q09，不用本静态核对代替。

本轮只读原版 EXE、正式 VFS、现有 Unity VFS 和已有解析清单。未运行 Unity 编译、SelfCheck 或 Play；这些验证对字节核对无新增断言价值，后续只在出现新的可达消费者或实际首差时定向运行。
