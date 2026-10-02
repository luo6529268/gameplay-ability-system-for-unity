# Q09/P-13 韩抓取后的项目背景震动：新版限定验收（2026-10-02）

> **2026-10-02 后续更正：** 下方 194/196 与李两处动作差是旧探针从世界 tick5 后增角色、首步恰处采样相位0，而正式根 fresh LFR 首步相位1所致。只修正诊断初相位后的原 Scene 新请求与正式根在相同相对 tick1～50 的 8 个选定字段达到 400/400；韩145/149/背景偏移/归零也变为 tick4/10/16/21 同步。旧原件和本报告旧计数保留为当时未配对相位的事实，不再作为生产规则首差。新的限定报告见 [相位配对证据](../NTSD28-336B44-Q09-P13-INPUT-PHASE-PAIR-001/REPORT.md)；正式背景 Z 钳位和正式可见像素出口仍开放。

状态：`UNITY_NATURAL_SCENE_BACKGROUND_PIXEL_SCOPED_PASS / FORMAL_VISIBLE_PIXEL_PENDING / Q09_OPEN`。本轮使用原项目、原 `NTSD_Battle.unity` 和既有请求式 Editor 探针；没有修改生产脚本、DAT、角色图片、背景、模式 Asset 或 Scene。根目录正式 EXE 的本轮 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

## Unity 原场景实测

- 新建的唯一请求 `Temp/NTSD28_Q09_HanEarthquakeBattlePlay.request.json` 在执行后由既有探针写为 `requested:false`，未删除或覆盖旧请求。输入为韩 OID726、李 OID7、初始 action0、源 X500/520、源 Z400、按现有 `BattleSpatialProjection` 投影进入项目可行走区域；完整 Driver 的前两 tick 攻击、接着两 tick 跳跃映射沿用该探针。
- [原场景 JSON](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-336b44-20261002-01.json) 为 `PASS_SCOPED_PLAY`，SHA-256 `2798C94C56E11A46E75085210ECC50907896C2E433A829DF0719E5FC1B14C127`。50 个 Driver tick 中，韩相对 tick3 进入145，tick9 抓取到149，tick15 进入150且运行时/冻结帧均发布背景偏移 `(2,0)`，tick20 进入151并归零。角色源 X 分别从500/520变成547/508，抓取后李到507。每个 tick 的运行时与冻结帧偏移相同；有序关闭完成、池借用数0。
- 同一原场景三个相机截图：[基线](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-336b44-20261002-01-baseline.png) SHA `2A6BEF08008A0B60F838FC616B1C0096ABBF6E3CF8676F2291DF295FAB58F24E`、[偏移](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-336b44-20261002-01-active.png) SHA `2070FE74FFE798D541691B9D1C9CFCC9D45F21D86F6D3C813BC3722390A33B0E`、[归零](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-336b44-20261002-01-reset.png) SHA `259FD1916001FF0B72FB063B6E47C7D97B5AC8F4DC4234A7CA66BC7C4E391A18`，均为1024×576。独立逐RGBA比对：天空行0～199的基线与归零逐像素相同；偏移图相对基线向右移1个输出像素后，该区域逐像素相同。地面行280～389同样在移1像素后逐像素相同。偏移前后未经平移的天空差114194像素。这与项目2048源像素宽完整背景压到1024截图、源偏移2像素对应输出1像素一致。角色区因动作变化不能用三图直接证明角色像素不动；探针记录 Map Transform、bounds、相机位置与尺寸均未改，材质/相机捕获状态已恢复。
- 原 Editor PID105896 结束后 idle、非Play、非编译；Scene clean。Battle/Menu/GameConfig/ProjectBattleModeConfig 四个磁盘 SHA 仍分别为 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`；`LoganRuntime` 的限定 Git 状态为空。

## 当前正式版对照及边界

- [正式根 headless LFR 报告](formal-root-report.json) SHA `F428AF1B37B474D2572CED5EB00B992AA7A1D4EDDEC9D6209F3451649D9ED28E`：正式 336B44 EXE 使用已有韩/李 X500/520、Z400、同攻击/跳跃 LFR 输入，`passed=true`、`failureCode=0`、声明50 tick；[原始 JSONL trace](formal-root-trace.csv) SHA `88EB01193C79B6C8B9A66544957987AC6ECBDE718557BC8A23596F17D81EA490`。文件扩展名 `.csv` 是正式 trace 参数沿用的旧命名，内容是 JSONL。报告本身标明 `nativeParityClaim=false`，也没有逐帧背景偏移或 GUI 像素。
- Unity 的相对 tick1～49 对应正式根 tick2～50，韩/李 action 与源 X 四字段计196项，有194项相同；正式 tick4、8 的李 action 分别为1、2，Unity对应仍为0、1。抓取形成后的相对 tick10～49 四字段160/160相同，正式根进入韩149/150/151分别为tick10/16/21，Unity为相对tick9/15/20。这个1 tick位相差与早期两处李动作差必须保留，不能称整条战斗同 tick 完全对齐。
- 另一个不可忽略的条件差：正式背景23把初始源 Z400 在 tick1 钳至542；项目自有可行走区域允许源 Z400，其物理 Z631.2328767 落在项目 stage237～760 内。这是用户保留项目地图/背景的范围差，不能把两边的整场初态或全像素直接拼成同场证书。此前 B1E13 版的配对 CSV 只作历史输入线索，本轮新版判断以上述根 336B44 的新回放和原 Unity Scene 为准。
- 当前已证明**原项目自然技能能使项目背景绘制平移并归零**。正式 EXE 的实际可见帧、同可比视口像素、角色/阴影与背景独立的合成画面，以及上述早期李动作/位相差的规则归因仍待。P-13/Q09、Q07 与最终总目标均保持开放；不恢复原版背景/模式 DAT，不改 DAT 数值。
