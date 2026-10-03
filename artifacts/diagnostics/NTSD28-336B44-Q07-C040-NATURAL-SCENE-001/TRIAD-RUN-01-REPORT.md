# Q07/C040 自来也—奇拉比—凯受控三人链：原 Battle Scene 配对

状态：`VERIFIED_SCOPED_ENTITY_FIELDS / ROOT_LFR_CRT_SEED_LIMITATION`。本次只关闭声明的受控初始动作、离散中性输入与原 Battle Scene 实体字段/关闭出口；C040、Q07 和总目标仍开放。

权威为根目录正式 `NTSD2.8-Logan.exe`，SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，以及对应 playable 源码。正式诊断以背景 1 取得与 Unity 自有地图共同有效的 Z400 域；这不部署或采用原版背景。自来也 OID21/action415/X500、奇拉比 OID75/action73/X620、凯 OID97/action0/X640，Y0/Z400、HP/MP500、mode0、seed682973786，三者逐 tick 中性输入。正式三份解码 DAT 与 Unity `Assets/NTSD/Content/LoganRuntime/decoded_dat` 的对应文件逐 SHA-256 相同；DAT 数值未修改。

原项目 Editor 在干净 Menu 预检后执行一次请求式原 `NTSD_Battle` Play，生产 `SimulationTickDriver` 完成 16 tick。Unity 与正式根 LFR trace 的 3 槽 × 16 tick × 11 个实体字段（OID、动作、状态、源整数 XYZ、HP、hold、抓取双方槽、队伍）**528/528 相同，首差 0**。正式源码 RNG CSV 与 Unity 的 5 个 RNG 字段 × 16 tick **80/80 相同**。tick1 正式源码 Bee→Guy 护甲命中一次；Unity 奇拉比 hold3、凯 HP497。tick2 正式源码自来也 kind3 抓取一次、关系 active/synchronized；Unity 奇拉比 action130/hold2，自来也 catchTarget=1、奇拉比 catchSource=0。正式根此前该受控正例选定 9 字段 × 16 tick 为 144/144；本次 Unity 比较扩到 11 字段。

D-024 固定完整背景投影使用 X 比例 2048/1333=1.536384096024006、Z 比例 1152/730=1.5780821917808219。原 Scene 逐 tick 视图坐标与精确源坐标投影最大残差 X0.579145、Z0.232877 输出像素，属于当前整数/亚像素出口的范围。未以画面位置回写战斗源坐标。

**已定位的回放载体差异：** 正式根 LFR 回放的 `rng.crtState` 与正式源码诊断及 Unity 从 tick1 起不同：根 tick1 为 2270971442，源码/Unity 为 2524509468；16 tick 全部不同。`GameSessionLfrPlayback28` 恢复 LFR 中的同步随机表/索引，但不恢复原会话 CRT seed；回放 `BattleConfig28.random_seed` 默认 0，源码诊断与 Unity 明确用 682973786。`NativeRandom28::reset_from_seed` 先生成 3000 个同步表字节；按正式 `Msvcr80Random28` 公式，seed0/682973786 走 3000 次分别得 3374725112/1758127634，再到 tick1 的 3002 次分别得 **2270971442/2524509468**，逐值解释了观测首差。根报告本身声明 `nativeParityClaim=false`。其余四个 RNG 字段与 Unity 相同；此差异不是本例已证的 Unity 生产首差，但 LFR 载体不能提供完整 CRT 同态验收，后续涉及 CRT 消费的场景必须另配相同 seed 的正式运行证据。

现有有序关闭报告为 `Completed / RuntimeMapCleared`，World 对象、runtime 槽、池借用、活动池对象和 Sprite 均 0，pool quiesced、World detached；退出 Play 后回到干净 `NTSD_Menu`，四项保护 SHA 稳定。生成 Editor 工程编译 0 error、270 warning；原 Editor 完成导入与本次 Play。未执行物理按键整链或 Game View 像素验收，不能以本例关闭其它 C040/Q07 出口。

原始 Scene JSON：[`c040-triad-scene-20261003-01.json`](c040-triad-scene-20261003-01.json)。逐字段配对与输入 SHA：[`paired-triad-scene-20261003-01.json`](paired-triad-scene-20261003-01.json)。正式源/根前置及两反例：[`TRIAD-ARMOR-REACH-001/REPORT.md`](../NTSD28-336B44-Q07-C040-TRIAD-ARMOR-REACH-001/REPORT.md)。
