# Q09/P-13 地震状态生产与冻结帧发布：限定验收

> 2026-09-27 续证更正：下方“Map尚未消费”是本包当时的快照，后续项目Map隔离GPU测试已通过。原Battle Scene专用worker因Unity角色表现绑定而未启动，与B1既有审计一致，故worker独立Play不再列为当前Q09出口；自然inline Battle Play像素及正式根同帧可见对照仍待。详 `artifacts/diagnostics/NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001/ACCEPTANCE.md`。

状态：`RUNTIME_PENDING`。此包只接正式 `NativeEarthquake28::step` 的 owner/偏移状态，不包含项目背景绘制、真实 Battle Play 像素或正式根 EXE 同帧像素。Q07、Q08、Q09、Q10 与总目标状态按总表各自的出口维护。

正式依据：根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；配对 playable `GameSession28::step` 在 World/KO 后、render snapshot 前调用地震扫描；`NativeEarthquake28::step` 按 slot 升序扫描 `state/10000==5`，owner 消失或退出 `[50000,60000)` 时只释放 owner 而保留两个偏移。当前正式 Han 自然 action0→抓取→frame150/151 的配对 Session 偏移为 `(2,0)→(0,0)`，正式根 LFR 的 400/400 是实体/关系字段，不含地震偏移或像素。

本包把 `NTSD28EarthquakeRuntimeState` 放进战斗 Runtime，并在成功 tick 的结果/KO 尾部之后、RenderDispatch 之前推进，即使该 tick 不构建表现帧也推进。Core/aggregate 快照、恢复校验、checksum 及 `BattlePresentationFrame` 冻结复制/复位均携带此状态。生成的 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 构建均为 0 error；它们只证明生成工程编译。

原项目 Unity Editor 刷新后的精确 EditMode job `9a34378e4636499b8b03680a97bc997b`：`NTSD28Q09EarthquakeStatePublicationEditorTests` 三项 `Passed 3/3`、failed 0、skipped 0、总耗时 2.5459293 秒。三个用例分别证实升序 owner/释放偏移与 Reset、快照恢复/checksum/冻结帧，以及成功无表现帧 tick 释放 owner 而保留偏移。最初两项的 job `792b04746f3b47f8b537a93fede15018` 在首次查询时 `Passed 2/2`；domain reload 后再次查询只有 succeeded 元数据而 `result=null`，因此最终证据以本次 3/3 实时返回为准。没有修前 RED 运行，不能声称 RED→GREEN。

原 `NTSD_Menu.unity` 与 `NTSD_Battle.unity` 磁盘 SHA-256 分别保持 `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` 和 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`。`git diff --check` 退出 0；`Tools/Validate-ChangeLedger.ps1` 退出 0，938 条 Record、当前 10 个差异代码文件均覆盖。DAT 数值、背景/模式 DAT、Scene、相机、实体逻辑/画面坐标和非战斗代码未由此包修改。

剩余出口：项目自有 Map Sprite 尚未消费冻结的偏移；独立 worker live publication、真实 Battle Play 项目背景像素与正式根同帧像素仍未验。下一 Task 应只处理项目背景视觉位移并保留固定相机/完整背景；背景 `SpriteRenderer.bounds` 是当前相机取景输入，直接移动背景 Transform 会改取景，不能据此宣称地震画面对齐。P-13/Q09/BATCH-05/总目标均保持开放。
