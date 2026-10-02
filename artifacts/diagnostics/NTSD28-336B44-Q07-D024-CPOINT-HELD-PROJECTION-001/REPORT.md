# Q07/D-024 CPOINT 持有距离比例修正（2026-10-02）

状态：`VERIFIED_SCOPED_CPOINT_HELD_PROJECTION`。正式战斗规则依据根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live source。项目固定完整背景相机和按视口比例放大战斗距离是用户批准的 D-024 例外；正式 CPOINT 仍定义源规则坐标，DAT 数值未改。

原 Battle Scene 同一 OID75/action355 → OID2/X650 自然链使用独立红、绿请求各运行 60 tick。`BattleCpointWriter.SyncHeldPosition` 现在在源规则持有坐标写完后，使用 `SimulationWorld.SpatialProjection.SourceDeltaToViewX/Z` 转换抓取者到被抓者的相对位移，以抓取者物理整数位置为锚点写物理 X/Z。源规则、Y、关系和生命周期写法不变。

| 实际读数 | 修复前 | 修复后 | 目标与解释 |
| --- | ---: | ---: | --- |
| tick2 源规则整数 X 间距 | 9 | 9 | 正式源规则不变 |
| tick2 物理 X 间距 | 8.50 | 13.327457 | 9 × (2048/1333) = 13.827457；锚点量化差 0.5 像素 |
| tick2 源规则整数 Z 间距 | -1 | -1 | 正式源规则不变 |
| tick2 物理 Z 间距 | -1.232877 | -1.810959 | -1 × (1152/730) = -1.578082；量化差约 0.233 像素 |

真正的持续持有为相对 tick2～55，共 54 tick；tick56～60 已进入投掷动作，尽管抓取者 `caughtSlot` 暂未清零，不能作为持有几何误差统计。54 个持有 tick 以双方源规则**整数间距**乘世界比例为目标，绿证书最大绝对偏差 X `0.936610`、Z `0.232877` 输出像素，均在 1 像素整数锚点量化范围内。红/绿两端每 tick 两实体的 24 个共有源/战斗字段合计 **2880/2880 相同**，本次改变没有造成这些字段的首差。红色原件保留，新证据为 `bee75-a355-x650-spatial-witness-02.json`；此前红色诊断报告中的“59 个持有 tick”应按本报告更正为“54 个持有 tick + 5 个投掷 tick”。

生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` 退出 0、0 error；原 Editor 刷新后实际编译并在原 Battle Scene Play，结果 `CAPTURED / DONE`、退出 Play、Scene clean，Battle Scene SHA 前后同为 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`。相邻 CPOINT/throw 位置 EditMode 聚焦测试 `1/1 PASS`，完整 `BattleRuntimeSelfCheck.RunAllChecksStatic()` 返回 `PASS`。Menu、GameConfig、模式 Asset、红色结果等 5 个保护文件 SHA `5/5` 不变，见 `protected-before.json` 与 `protected-after.json`。SelfCheck 旧结果先复制为 `selfcheck-prior-result.txt`，请求式入口随后清理 Temp 旧结果并写入新的 `selfcheck-green-result.txt`；两份均为 PASS，同 SHA `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`。这是受控临时结果替换，未删项目资产或用户文件。

本证书只关闭 **CPOINT 持有物理相对位移**的选定自然链比例门。其它武器/持有挂点、抓取接触距离、更多角色与边缘碰撞仍需按可达性检查；Q07、Q09 与总目标继续开放。没有正式 EXE 与 Unity 整场逐像素或手动体感等价结论。

交付检查：`Tools/Validate-ChangeLedger.ps1` 退出0并报告 `Change ledger validation PASSED`；`git diff --check` 退出0。完整SelfCheck引起本地桥监听端口重新绑定到6400，已用同一原Editor PID105896复核 idle、非Play、非编译和当前Battle Scene；并未启动第二个验证项目。
