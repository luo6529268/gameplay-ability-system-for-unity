# Q07 鸣人螺旋丸自然组合键 Play 限定验收（2026-09-25）

范围：原 Unity 项目、原 Editor、`NTSD_Battle` Scene 中由 `BattleTestBootstrap` 创建的鸣人 OID2；InputSystem 键盘设备事件按防御 L → 面朝方向 D/A → 跳跃 K → 攻击 J 送入现有输入链。未直接设置角色 frame/combo 或构造 FrameInputSet。诊断在 Live Play 暂停 LocalFreeRun 后，每次 Editor update 用正式 `StepOneTick(ignorePaused: true)` 和本地输入提供者推进一个 tick，以免同一 Editor update 的合法双 tick 追帧漏掉窗口；退出时恢复原暂停状态并释放按键。此方法验证逻辑 tick/输入路由，不验证正常 wall-clock 节奏、人手按键或正式 EXE 画面。

当前正式 playable 源码 `source/ntsd28_core/src/simulation/simulation_tick_driver.cpp` 的 `SimulationTickDriver28::step` 先推进 2tu 输入相位，然后 `InputRouter28::sample_pending` 仅在相位 0 取本地待输入。正式鸣人 DAT 的 253 有续按攻击入口、254 已没有该入口。因此“第二个 253 画面 tick 后才排队 J 必然转 301”的 Task 原预期过宽；是否转出还取决于下一 tick 的 2tu 相位。这里的正式规则结论来自 playable 源码与正式 DAT；本轮未取得独立正式 EXE 自然物理键录像。

| 场景 | 原 Editor 观察 | 结果 |
|---|---|---|
| 第一 253 后 J | tick626/628/630 依次见防、方向、技能；首个 253=tick657；J 在 tick658 进入 FrameInputSet 并被 native proxy 采样，同 tick 转 action301 | PASS |
| 第二 253 后 J | 首/次 253=tick1125/1126；J 在 tick1127/action254/相位1进入 FrameInputSet，tick1128/相位0才由 native proxy 采样；连续 12 tick 未转 301 | PASS，符合正式 2tu 相位门，原预期已纠正 |
| 254 后 J | 首/次 253=tick233/234，254=tick235；J 在 tick236 被采样，连续 12 tick 未转 301 | PASS |

原始唯一结果（均保留在未跟踪 Temp 目录，SHA-256）：

- `Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/natural-first253-20260925T064335096-72fd614e9c5b44dba1da66b611ffd31f.json` — `782EBD60CC4D7C13AB83BFFC7B595113A14C3E14EF9E217B0C0461E49C7E6CCC`
- `Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/natural-second253-20260925T064859633-1a3e3d0bc5494f6280c18ba969996d80.json` — `FAA4BC4D3D2D3A5FF4CC9E28690D496E95E58949DF2B346BE97AEF1FD7A0473B`
- `Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/natural-after254-20260925T064435075-d52cf9348d894fe9b49a4cf64ad417aa.json` — `E405A23D3316B1D660CC6A2653D0C39D81720877E06AF836D6BE00514B8F7FBF`

先前 FAIL 也原样保留：初版 Editor.update 抽样跨过两个 tick；之后数次在异步 `BattleTestBootstrap.SetupTestCharacters` 完成前启动，预检 actor=null；第二 253 的首次有效结果按旧“必然转换”预期 FAIL，原始 tick 记录促成相位门纠正。这些不能当作战斗生产失败。

最后脚本修改后原 Editor 导入编译为 0 error；第二 253 复跑 PASS，首个 253/254 后用例在此前同脚本的单 tick 输入机制上 PASS，末次修改仅增加相位字段和第二 253 分支判定，未重跑二者。Editor 已退出 Play；Battle/Menu/GameConfig 磁盘 SHA-256 分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`。未编辑生产战斗逻辑、DAT、图片、Scene、Prefab、ProjectSettings 或非战斗脚本。Q07/R18、正式 EXE 同输入自然可见表现和最终集成仍开放。
