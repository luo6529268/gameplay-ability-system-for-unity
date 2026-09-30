# Q09/P-08 同一等 HP LFR 配对回放 WARP 限定验收

状态：`VERIFIED_SCOPED_PAIRED_LFR_WARP / P-08_OPEN`。本包只证明同一 22 tick 等 HP LFR 经正式版对应 playable 源码配对回放后，tick22 的出血标记在 WARP 画面产生三个红像素。**没有捕获正式根 EXE 本身的 GPU 帧，也没有 Unity 同初态、同视口 A/B；P-08/Q09/BATCH-05 与总目标不关闭。**

输入沿用前包未修改的 `ita_equal_hp_source_packets.lfr`，SHA-256 `ABD2B83A72E7DC7496228CE56BD2310E3AC69C67BECCC69354A7FA50416C5C55`。配对 `GameSessionLfrPlayback28` 显式恢复已证动作0/0、朝向0/1、MP500/500，并读正式 `resources/runtime`。前22个声明 tick 的鸣人动作、鼬动作/当前HP/基础HP与录制源逐项比较 [88/88 相同](source-playback-field-comparison.json)。LFR 管理器还有第23次终步；工具先保留 tick22 快照，再执行终步并验证最终 headers，结果 `PASS`，见 [运行日志](run-03.log)。tick22 鼬站立动作0、HP10/基础HP30，快照唯一标记的阈值10、宽高1×3、红 `0x00ff0000`、左上角 `(461,595)`，视口1333×730。

配对 `D3D11Renderer28` WARP 从同一个 tick22 快照绘制[标记开启](ita-equal-hp-replay-mark-on.png)；只在快照副本去掉那条标记命令后绘制[标记关闭](ita-equal-hp-replay-mark-off.png)，World及正式 DAT 未改。独立 [RGB 像素计算](pixel-analysis-02.json) 找到恰三个差异像素 `(461,595)`、`(461,596)`、`(461,597)`：开启均为不透明红 `(255,0,0,255)`，关闭均为背景色 `(106,118,121,255)`；其余画面无差异。PNG SHA-256 分别为 `04FCC3DDBD971B51B318D249B8786F72F79049B144947C0182F1A8070B048424` 和 `737D4F516458602BB76E868C4FF8FB318A2164717C9349C161C4BE5422EE3A07`。

失败原样保留：首次 `compile.log` 因 `std::vector` 迭代器构造被 C++ 解析为函数声明失败；改正后 `compile-02.log` 编译通过，但首轮 `run.log` 错把迭代器读完等同 `istream.eof()` 而报 LFR 读取失败；`compile-03.log` 编译通过，`run-02.log` 到 tick22 后因只执行22次、没有第23终步而误报最终头失败。最终 `compile-04.log` 为空、`compile-exit-04.txt=0`，`run-exit-03.txt=0`。第一次 [RGBA 边界计算](pixel-analysis.json) 因 Alpha 通道无差异而给出0像素假阴性，直接像素采样发现问题后改用 RGB，修正结果另存 `pixel-analysis-02.json`，不覆盖原始失败。所有这些失败均属于新诊断代码或分析脚本，不是已证战斗规则差异。

正式根 EXE SHA-256 保持 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig SHA 分别保持 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。新增 Tools 脚本 SHA `6FC1660981687E498F1CA49FEA1E859AB754853827CC66204BEEEC08F53CF595`。`Tools/Validate-ChangeLedger.ps1` exit0，日志首行 `Change ledger validation PASSED`，第67行覆盖新增脚本；`git -c core.safecrlf=false diff --check` exit0，新建代码/Task/Record无尾随空白。没有改 DAT、图片、Unity 生产、Scene、项目模式或非战斗代码，也没有另开 Unity Editor；此 Tools-only 子包无需重跑已通过的 Unity Play。

用同一输出路径重试时工具在读 LFR 前以 exit3 拒绝覆盖，两张 PNG 的 SHA 保持不变；原文见 [覆盖保护日志](overwrite-check.log)。最终文档更新后 Ledger validator 仍 exit0，见 [最终校验日志](ledger-validator-final.log)。

下一步仍须取得根 EXE 自身可观察标记画面或明确无法从正式发行界面取得的边界，并做原 Unity 战斗场景同状态/同视口对照；不能由本包的配对 GPU 三像素推定跨端整画面相等。
