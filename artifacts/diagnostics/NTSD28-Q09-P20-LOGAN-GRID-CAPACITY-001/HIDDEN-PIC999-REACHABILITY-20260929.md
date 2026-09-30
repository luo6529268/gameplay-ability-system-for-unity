# Q09/P-20 当前正式内容的 pic999 隐藏分支限定审计

状态：`STATIC_SCOPED / PIC999_CONTENT_BRANCH_ONLY`。此项只收窄 P-20 的一个内容分支；P-20、Q09 和 BATCH-05 保持开放。

正式配对 playable 的 `SpriteFrameResolver28::resolve` 在 `render_snapshot.cpp:1104–1148` 只会从声明图片范围解析图片；`RenderSnapshotBuilder28::build` 在 `:1579–1585` 将当前帧 `pic` 加上 `revive_visual_runtime_318` 后解析。该 runtime 字段在正式配对核心的现有写者中初始化/清零为 0，复活续接仅在 `visual > 0` 时写入正数（`battle_world.cpp:2776,8429`）。Unity `LF2Entity.GetRenderPicIndex` 对原始 `pic999` 提前返回 999，`BattlePresentationShadowBuild.ResolveSpriteCapture` 不发布该索引；两端实现顺序不同，故仅凭代码不能宣称通用等价。

只读扫描正式 `resources/runtime/decoded_dat` 的 405 个 DAT，并交叉核对现存 `NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content.jsonl`：后者记录 773 个 sheet，最大声明末尾 pic 为 559（`c/nar/ncl.dat`）。405 个 DAT 原文中的 `pic: 999` 共 4587 处、涉及 286 个 DAT；没有声明 sheet 覆盖 999 或更高 pic。既有诊断投影中的三个 `parseSuccess=false` 文件（`data/frame/INKHUD.dat`、`INKHUD2.dat`、`data/resource.dat`）均未投影出 sheet；原文字面 `file(x-y)` 扫描也未见覆盖 999 的范围。在当前已确认的非负视觉偏移条件下，原始 `pic999` 在正式配对快照和 Unity 都没有可解析的本体图片。

这不证明负 pic 加偏移、缺失文件、无资源/terminal 帧、正式根 EXE GPU 或 Unity 自然画面整体相同。后续只在这些分支出现正式可达场景或同状态首差时做聚焦对照；不重跑已通过的容量四例或全角色矩阵。此审计未运行 Unity、Play 或正式 EXE，也未修改代码、DAT、图片、Scene 和配置。
