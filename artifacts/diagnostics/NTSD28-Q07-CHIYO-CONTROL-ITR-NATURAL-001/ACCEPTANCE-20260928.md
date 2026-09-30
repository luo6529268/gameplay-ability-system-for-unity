# 千代 Uj→傀儡控制型 ITR 自然链限定验收（2026-09-28）

`NTSD28-Q07-CHIYO-CONTROL-ITR-NATURAL-001 / VERIFIED_SCOPED_ROUTE`。本包只运行正式 playable 配对源码的 `GameSession28` 和 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 的根正式 EXE headless LFR 回放；没有运行 Unity Play，也没有修改生产规则、DAT、场景或项目模式 Asset。根回放报告 `nativeParityClaim=false`，以下 90/90 仅指明列出的动作/对象计数，不是整状态对齐证书。

共同初态：千代 OID8/slot0 `(x500,z650)`，鸣人 OID2/slot1 `(x1200,z650)`，双方 team1/2、mode0、背景23、seed `682973786`、BGM 选项2、MP500。普通 sampled input 是 tick1–2 defend、3–4 depth_up、5–6 jump，其后全释放；无强制动作、spawn 或碰撞写入。GCC 15.1.0 以正式 playable 构建清单的 28 个 core 和四个 playable `.cpp`、`-municode -lz` 编译最终诊断成功。首个编译因误用不存在的 `InputKey28::up` 失败，第二次仅漏链接参数，失败原日志分别保留为 `compile.log`、`compile-v2.log`；修正后的 `compile-v3.log` 和扩展 HP 参数的 `compile-hp.log` 均成功。第一次执行误把 `decoded_dat` 当 extracted root，未找到 `catalog.csv`；随后使用正式 `resources/runtime` 双根，原失败不作规则证据。

| 初始千代 HP | 正式源码/根 EXE 共同观测 | 归因与范围 |
|---|---|---|
| 500（run2） | tick6 进入 action318，MP500→400；90 tick 无 OID419/OID854；源码与根回放千代动作 90/90 相同。根报告 `passed=true/failureCode0`。 | `c/chi/chi.dat:1438-1439` 的目标 frame301 编码 `state:1150318`；正式 `input_routing.cpp:341-366` 在 HP>150 时重定向至318，同时仍按 frame301 收取 MP100。这个满血负例是已证前置条件，不是输入时机问题。 |
| 150（run3） | tick6–7 action301，tick8 起 560，tick18 OID419 首生，tick27 OID854 首生；90 tick 无 OID854/frame407。根报告 `passed=true/failureCode0`。源码/根回放千代动作、OID419 数量、OID854 数量及 frame407 指示各 90/90 相同。 | HP>150 重定向不成立，普通输入及 OPoint 链自然通过。傀儡在此固定远距对手条件下仅出现动作 0/5/6/7/108/109/210/211/212/215/311/312；没有触发通向400→407的 kind8 条件，不能宣称控制型 ITR 在该自然战斗中被消费。 |

run2 根报告 SHA-256 `C1E91DCE6EBFA0AC803E0F49228E44673E8B437693AD514C3E3DDE4FCE02FAEE`。run3 源 CSV/LFR、根报告/trace 的 SHA-256 依次为 `0F4BE3BF0A674189F8E2E906554B1C02EA864577ACC13EB700840FA08C288231`、`70FB7C46976EAE5D99E500F847327A279C7FB01F43D86ADFF9B892FE537B63AE`、`2539EF7F19ECEE7070BE2DB065E8192E1E997DA75E507AD5F33B63F48E1C2C16`、`261CFC56A51E28F1925B7E72A09126EF4B2F2C7E7C26FE9EED5AE6D6D7D1CA8C`。旧原 Editor 控制型 ITR 作者帧双 profile 3/3 PASS 仍是独立聚焦证据，不由本案例升级为自然/正式 EXE 同条件候选证据。

**下一步裁决：** 当前不存在据此可改的生产首差。保留已修控制型 ITR 候选准入及其 `RUNTIME_PENDING` 限定，不为追 frame407 盲试目标位置或全角色矩阵。Q07 继续按总表 §0.14.4 处理新证明可达的内容/引用或同状态首差；Q08 项目模式/结果出口可独立推进。若将来正式自然战斗实际到达 frame407，再以当时同状态候选/结果开一个聚焦回访。用户暂缓的默认 stage DAT 不能为 KonanAngel 案例私自部署。

范围保持：本包新增一个 Tools C++ 诊断和 Task/Change/证据文档；`git diff --check` 退出0，`Tools/Validate-ChangeLedger.ps1` 返回 `PASSED`（981 Records，17 个当前代码差异被覆盖，历史不在本次 diff 的记录产生既有 warning）。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 的 SHA-256 分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。未运行 Unity 编译、SelfCheck 或 Play；本包不把这些缺失验收写成通过。
