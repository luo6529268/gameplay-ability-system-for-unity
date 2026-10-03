# Q07 / D-024 连续帧直接位移限定验收（2026-10-03）

状态：`VERIFIED_SCOPED_DIRECT_MOTION`。只关闭正式可达 OID92/action580 的连续直接位移与共用 X/Z 出口在共同可走区域内的比例子门；Q07、D-024 全实体、Q09 像素与 Q12 最终验收继续开放。

## 权威与原始首差

- 根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，且当前 playable 源码会话使用正式 `resources/runtime`。正式 OID92 的 `s/0/iru.dat` action580 为 `dz:4 wait:20 next:580`；项目 LoganRuntime 同文件 SHA-256 `7F03687F974B57353E83EF98C8CE3AEBBB9E233EBD67E565FA0E323EAA61837A` 与正式版相同，未改 DAT。
- 初始 Z400、24 完整 tick 的正式源码与根回放选定动作、整数/精确 Z 共75/75相同，正式源 Z400→496；[原始对比](root-source-comparison.json)。原 Editor 改动前配置2048×1152视口、直接帧运动聚焦 RED，tick4画面投影差首次超过1像素，24次最大为7.416438356164349像素；默认1:1视口通过。[RED 24次](unity-red-24-job-result.json)。
- 原因是 `LF2Entity.ApplyNativeFrameMotionTail` 对每次 `dx/dz` 从已取整的物理 X/Z 整数位重新起步。源规则坐标仍按正式整数基底递推；两域独立造成配置视口的累计画面偏差。

## 修正与定向验证

- 仅在源规则位置已初始化的共用直接帧运动路径，物理 X/Z 从精确位置累加本次 DAT 位移乘统一世界比例；未初始化源坐标的既有整数基底路径保留。原生规则坐标、DAT、相机、Scene、GameConfig、模式 Asset、非战斗代码均未改；无角色/OID 特判。平台后直接位移聚焦夹具按已初始化/未初始化源域分别断言精确/整数画面合成，新增重复左右 X 24 次用例。
- 生成 Editor 工程最后编译0错误；原项目 Editor 刷新后帧运动类33/33 PASS（含正式 OID92/Z、左右 X、平台叠加和历史原生帧夹具）。[完整聚焦结果](unity-green-class-job-result.json)。
- 改后以共同可走区初始 Z380 复测：正式 playable 源码 action580 在24 tick 为 Z380→476，根正式 EXE LFR报告 `passed=true/failureCode=0`，所选动作/整数Z/精确Z 75/75零差。[源根对比](root-source-comparison-03.json)。原 Battle Scene 两人完整生产 Driver 初始+24tick×2槽×8字段400/400与根 trace 零差；物理Z相对统一 `SourceToViewZ` 的最大误差 `1.1368683772161603e-13`输出像素。[场景逐字段对比](root-unity-scene-comparison-03.json)。原 Editor 回干净 Menu、四保护文件 SHA 稳，World/槽/池借用/活动对象/Sprite全0、pool quiesced、有序关闭完成。正式根报告的 `nativeParityClaim=false` 为其通用声明，本报告仅主张列出的选定字段同态。
- 原 Editor 完整 `BattleRuntimeSelfCheck` 于17:29:36写出 `PASS`；MCP菜单调用回执超时，实际结果文件时间晚于调用，已另存 [`selfcheck-result-20261003.txt`](selfcheck-result-20261003.txt)。旧临时结果是2026-10-02的4字节`PASS`，调用前已逐字复制为 [`prior-selfcheck-result-20261002.txt`](prior-selfcheck-result-20261002.txt)；菜单仅覆盖 `Temp/NTSD_BattleRuntimeSelfCheck.result`，没有删除项目资产。

## 地图边界与证据范围

- 保留初始 Z400 的原 Battle Scene 第一轮 `MISMATCH`：前0～20tick 两槽八字段336/336同根；tick21正式背景1继续 Z484，而项目自有地图把源位置限制到精确 Z481.59722222222223；该地图/背景差异符合用户保留项目地图的边界，不能靠部署原版背景消除。tick21～24还观察到物理投影偏差增长至2.8273972602742106像素，需在后续独立 D-024 地图边界出口区分允许的规则边界例外和边界后画面残差。[边界首差](root-unity-scene-comparison-01-boundary.json)。该首轮也正常有序关闭、四SHA稳定，失败原件未覆盖。
- 初始 Z300 的正式诊断在tick1被正式背景下界移到Z375，不能作为共同区域样本；[原始源码结果](source-run-02/source-ticks.csv)保留。Z380复测是前置条件修正，不是对首差的回退。
- 本包的视图数值与原 Battle Scene 生产 Driver 已证；正式 EXE 实际GPU画面、完整玩家物理键链、所有实体/位移出口与整场表现未证，保持Q09/Q12和总目标开放。
