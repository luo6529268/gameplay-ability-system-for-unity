# Q09/P-07 李 J,L 正式快照命令限定验收

状态：`VERIFIED_SCOPED_FORMAL_SNAPSHOT_COMMANDS`。正式根 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；诊断只编译、运行其配对 playable 构建闭包中的 `GameSession28::step/snapshot` 与 core `RenderSnapshotBuilder28`，不替换根 EXE。

新工具以既有正式李 J@tick2、L@tick3–4 场景配置运行 13 个完整 Session tick，并取 tick5–13 的渲染快照。g++15.1.0 编译正式 28 个 core 源文件、`game_session.cpp`、`selection_flow.cpp` 与新工具，exit 0、无编译诊断；完整参数逐项见 `compile-argv.txt`。首次执行把 `decoded_dat` 子目录误作含 `catalog.csv` 的 Session root，初始化 exit 4、`run.log` 与 `run-exit.txt` 保留；更正为正式 `resources/runtime` 后，`run-attempt2.log` exit 0，未修改任何正式资源。再次以同名输出运行得到预期 exit 3，TSV SHA 前后相同，见 `no-overwrite-result.txt`。

正式配对 Session tick6：五个 owner0 的 OID204 在 slots51–55、action20/pic28，各有本体 sprite 命令；子体阴影命令合计 **0**，普通对象阴影命令 **3**。tick6–13 每 tick 均为五个可见子体本体、零个子体阴影；普通阴影始终至少2条。独立读取先前正式根 EXE LFR tick6–13 记录，子体 slot/action/pic 共 **40/40** 与新快照一致、首差0，逐行结果见 `lfr-comparison.txt`。该比较只覆盖这些字段，不能提升为根 EXE 的 render snapshot 或 GPU 像素证明。

原 Unity Battle Scene 的已保存自然李 J,L Play 记录 `lee-q09-p07-shadow-01.json` 在其 tick12 也有五个 OID204 本体中央命令、五个 BMP `shadow:1`、零个子体阴影命令，并有五个普通阴影控制命令及本体相机像素。正式与 Unity 的起始场景和 tick 编号不同，本次对照的是 **共用字段禁显与普通阴影仍可绘制**，不是严格同初态逐像素 A/B。固定全景与项目背景仍按用户例外保留；P-07、Q09、BATCH-05 和总目标未关闭。后续正式 EXE 同状态可见画面与项目 Game view 仍需独立验收，不重跑已过的原 Unity Play。

新工具源码 SHA-256 `652DDFD946AEE5B42604B290B66A10ECFE512E5C6B31FA75716A0057325E14C0`；诊断 EXE `C7CC64008E927182750A5224F63AD1A6E4AB6E5F5023731B4A1EA4BBF48FB41D`；TSV `ACACEBAC6779C3C402E62B5AA11A6F5342BE5735E9A724F5B8B6B66CF59C8D2B`。原 Unity Play JSON SHA `E5FF782D96120E30CFC3EF4F9D256A6EA9B7A2F8E02C94B2F345D391CC4FF1B6`。保护 Battle/Menu Scene SHA-256 仍为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` / `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`。未启动第二 Unity、未进 Play、未修改生产脚本、DAT/PNG/Scene/非战斗文件。
