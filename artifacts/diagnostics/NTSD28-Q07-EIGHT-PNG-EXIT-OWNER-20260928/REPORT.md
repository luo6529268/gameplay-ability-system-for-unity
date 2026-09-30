# Q07 八张未暂存 PNG 的出口归属核对（2026-09-28）

状态：`VERIFIED_SCOPED_CONSUMER_CLASSIFICATION / Q07_OPEN`。本轮只读核对正式内容、配对 playable 源码和既有逐文件清单；未运行 Unity/正式 EXE 的画面，也未复制图片、修改 DAT 或删除旧资源。

正式根 `NTSD2.8-Logan.exe` 本轮 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。此前 [114 图逐项审计](../NTSD28-Q07-REMAINING-SPRITE-CONSUMER-AUDIT-001/REPORT-20260926.md)已将 106 张归入具名用户例外或受保护的 UI/结果用途，余下八张仍须区分声明与选中战斗消费，不能按缺图计数部署。

| 图片 | 当前证据与出口 |
| --- | --- |
| `sprite/UI/extra/{recording_background,human,BG1o1,BG2o2,branch,branch2,player}.png` 七张 | 正式 `decoded_dat/data/resource.dat:53-61` 只在 `<frame>` 段声明。配对 `native_resource_catalog.cpp:46-80` 仅读取 `<bmp_begin>` 至 `<bmp_end>` 的索引表；现有 playable 调用者审计未证明七张有选中战斗消费者。标记为 `NO_SELECTED_Q07_BATTLE_CONSUMER_PROVEN`，保留条件回访 R17；日后发现具体生产读者再按单一路径重开，不称永远无用。 |
| `sprite/UI/PAUSE.png` | 同文件第 24 行处于索引表（索引 22），但所检 `d3d11_renderer.cpp:2292-2296` 在暂停时绘制纯色条，既有直接索引消费者清单也无 22。若正式根 EXE 可见暂停画面出现非例外首差，则交 Q09 表现/资源门；目前不以 Q07 缺失战斗图要求复制。 |

既有直接索引审计确认生产战斗所需 `WORDS0..5` 与 `SPARK` 七张在所选 Unity VFS 中齐备；另有非排除 DAT 所声明 Sprite Sheet 703/703 同 SHA、703/703 唯一有效 GUID 的独立核对。上述证据只缩小当前 Q07 图片待办，**不证明全部运行时绑定、技能自然可达或正式版/Unity 画面一致**。Q07 仍按总表 §0.14.4 聚焦新证明可达的引用首差、自然技能首差和 D-024 共用坐标域；Q08 模式结果、Q09 表现、Q10 音频、Q12 全角色整场各守原出口。旧 521 项继续 `deleteAuthorized=false`。
