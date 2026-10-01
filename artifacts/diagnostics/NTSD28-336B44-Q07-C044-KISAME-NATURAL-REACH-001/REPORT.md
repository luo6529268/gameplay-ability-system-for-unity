# C044 OID17 负 decrease 自然跨零可达性

状态：`FORMAL_SOURCE_ROOT_NATURAL_CROSS_ZERO_VERIFIED / UNITY_NATURAL_SCENE_PENDING`。正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 `resources/runtime/decoded_dat/c/kis/kis.dat` SHA-256 `42365D2812B3E91C10598DDBD003FA53AAB9E38364A2335D6403540F19A9B6B4`。原版 DAT 数值未改。

诊断用当前 playable 的 `GameSession28::step()`、正式 `resources/runtime`、种子 682973786、mode0、背景1；鬼鲛 OID17 从 X500/Z400/action314 或 316 开始，鸣人 OID2 从 X550 或 X1200/Z400/action0 开始，双方 HP/MP500，后续160 tick 输入均中性。近距两例 tick1 自然 kind3 抓取，tick6 进入负 `decrease:-7` 帧循环，tick46 timeout 8→1，tick47 以 action124 将 1→-6 并触发 release，投掷数0；该 tick 受害者动作181、HP430、pending 冲量 X4/Y-3/贡献数1。远距 action314/X1200 全程无抓取或释放；action316/X1200 源码控制亦无抓取。

三例源诊断各生成逐 tick CSV、RNG CSV 与 LFR。正式根 EXE 用对应源 LFR 独立回放 action314/X550、316/X550、314/X1200；三个 report 均 `passed=true`、`failureCode=0`、`completedTicks=161`，trace 为初态、160逻辑tick、额外EOF共162行。[比较结果](source-root-comparison-v2.json)核对双方动作/计数、位置、速度、HP、抓取双方槽、timeout 以及 CRT 调用数与同步 RNG 四字段，共20字段×160tick×3例=9600/9600无数值首差。近距各有5个 JSON 浮点打印末位差，绝对差均低于 `1e-10`；正式根独立 CRT 内部 state 与额外 EOF tick 不在本次同态断言中。根内置 `nativeParityClaim=false` 说明 LFR 回放报告本身不宣称完整 native parity；本报告仅主张列出的逐 tick 字段和自然跨零结果。

第一次诊断根目录误传导致 `decoded_dat` 未找到，第二次导致 `catalog.csv` 未找到，均在初始化前退出；诊断器已改为双参数正式 `resources/runtime`，第三次编译0诊断、四组源运行成功，失败目录保留。下一步应另立原 Battle Scene 同初态探针，以这条已证可达自然链验证 Unity 共用 writer 的 pending 与下一 pass 速度、动作计数、释放结果；这项 Unity 自然运行尚未完成，故 C044 父门、Q07 和总目标保持开放。未改 Unity 生产、DAT、Scene、Prefab 或非战斗流程。
