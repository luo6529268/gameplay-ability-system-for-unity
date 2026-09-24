# Q09 P-04 原 Battle Scene 同 Z 发布顺序见证（2026-09-24）

本包状态：`VERIFIED_SCENE_ORDER_ONLY`；父项 P-04/Q09 仍开放。正式 EXE SHA-256 复核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。正式 playable `render_snapshot.cpp` 最终 `entity_commands` 同深度按物理 slot 降序；此包只验证 Unity 原 Scene 生产 World 的对应发布顺序。

新增 Editor-only 请求探针 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09SameZSceneOrderPlayProbeEditor.cs`；不修改生产脚本、DAT、图片、Scene、Prefab、相机或项目配置。探针在当前原 `NTSD_Battle.unity` Play、CentralOnly、tick≥5 的生产 World 中暂停并等待 worker 安全边界，临时注册两个同 Z 实体，调用生产 `RenderDispatchAll` 并物化 published painter order，最后回收并恢复暂停。

原 Editor 编译完成：新脚本/`.meta` 已导入，`Assembly-CSharp-Editor.dll` 时间晚于脚本修改，Editor 返回 idle/is_compiling=false，未见 `error CS`。第一次请求在探针原 45 秒启动期限内仍处于 BattleTestBootstrap 内容加载，`first-startup-timeout.json` 为前置未到位 FAIL；不作为绘制首差。把期限改为有界 180 秒并增加失败时 World/tick 诊断后，第二次请求 `scene-order-pass.json` 为 PASS：

| 观察 | 值 |
|---|---|
| Scene / backend / tick | `Assets/NTSD/Scene/NTSD_Battle.unity` / `CentralOnly` / 5 |
| 两个有效 runtime handle | slot 50 generation 1；slot 51 generation 1 |
| 同 Z | 240 / 240 |
| 物化 painter rank | slot 51 → 0；slot 50 → 1 |
| 生产排序号 | slot 51 → 1；slot 50 → 5 |
| 对象和占用 slot | 4/2 → 临时夹具 → 4/2，cleanup PASS |
| 暂停状态 | 恢复 PASS；请求完成后 Editor 已退出 Play |

两夹具是 logic-only、pic999，没有实际可绘制 body；`fixtureGpuSubmissionWitnessed=false`，`publishedCommandCount=0`。探针显式调用生产 `MaterializePresentationOrder`，尚未证明自然 `LateUpdate`/central flush 会在同帧自动完成物化。因此本包**不证明**中央 GPU 提交、遮挡像素、Legacy 表现出口、正式 EXE 同场景画面或 Q09 完全对齐。清理断言只覆盖对象数、占用 slot 数与暂停状态，并未逐项证明 allocator generation 游标、所有快照或完整有序关闭。后续 P-04 需使用两张可区分且重叠区域不透明的正式资源做实际提交和像素归属验证；不能仅以“非白像素存在”代替前后顺序。

原 Battle Scene 磁盘 SHA-256 在本包前后均为 `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`；Menu Scene 的既有并行哈希 `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` 未由本包改变。未启动第二个 Unity Editor、未使用 computer-use。完整验证结果为 `scene-order-pass.json`，首次启动等待结果保留为 `first-startup-timeout.json`。

交付前补查：上述 Battle Scene 哈希是探针退出后立即取得的观察。此后文件于 20:14:05 被另一次写入，当前哈希变成 `7D7286D5CBCDE396AFCA3BBA9ABBC199577D0833033603C71D627E3BD470E567`；Git diff 仅有两个 Camera 的 `m_Enabled: 1 → 0`。本探针代码没有写 Scene，且结果文件时间为 20:12:16；**无法仅凭时间判定后续写入者**。保留该并行变化，不回退、不将当前 Scene 称作仍未变化。后继实际像素验证须先复核这两个相机的当前意图与有效取景条件。

只读定位两处 Scene 差异对应 HUDCamera 和 ScenesCamera 的 Camera 组件 m_Enabled，二者 GameObject 仍 active；中央渲染的 BattleCentralRenderSystem 要求 world camera enabled。已异步询问用户此设置是否有意，未修改 Scene。
