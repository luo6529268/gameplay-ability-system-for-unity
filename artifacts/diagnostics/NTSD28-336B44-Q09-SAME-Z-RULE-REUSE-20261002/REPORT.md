# Q09/P-04 同 Z 排序：336B44 规则复核与旧像素证书边界

本报告只读复核正式版 render handoff 和当前 Unity 源码，不运行新的 Game View/GPU 捕获。根正式 EXE 的本轮 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；`render_snapshot.cpp` 属当前 playable 构建闭包。旧源码取自已归档的 B1E13 更新前源码快照，不用它裁决新版规则。

逐字节比较旧快照 `ntsd28_core/src/rendering/render_snapshot.cpp` 与当前 336B44 同路径，文件完整 SHA 分别为 `BC19BBF00C6E75DA8D6214B89D873733DD6D2527CAC6B4C33479A9EF1AA2AAFF` 和 `B383FB681951E61BD9B5DA7FD42CB4AEA78838A3E07DD4354DBFE8CF7741BBB9`。差异只落在可选 combo command 的资格/显示时限，以及原生 battle timer 来源。`RenderSnapshotBuilder28::build` 末段的 `entity_commands` stable_sort 区块两版逐字节相同，区块 SHA-256 均为 `4D77F38936240D650389533CA4F7C381E3328C10277AD63D474E21721316F895`（1431 字节）：先按 `depth_order` 升序，同深度按物理 `slot` 降序，再按 phase_order。该规则不会因为 35 项新版变动而需要重新实现。

当前 Unity `BattlePresentationShadowBuild.cs` 的同 Z radix run 逆序分支仍按既有 `NTSD28-Q09-SAME-Z-PAINTER-ORDER-001` 合同使较大物理槽先绘制；此文件和 `SimulationStageRenderModule.cs` 在当前工作树无修改。旧版原 Battle Scene 四个独立 Play 的 CentralOnly GPU 证书测得82个可辨交叠像素均符合 OID120 较晚绘制，0个支持相反顺序。它证明的是旧场景 SHA `471396E7692BA0C4B5CC8C2CD1355D4CF88B8AB2ACF23A6CAE6BF1E6475BC7B9` 下所测 tick6 与像素；本轮现有 Battle Scene 磁盘 SHA 为 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`，故旧 PNG/像素不能充当当前场景的新版实际画面证书。旧证书也没有涵盖 Legacy 输出、formal EXE 实际像素、插值或其它 Z tie。

裁决：**新版正式同 Z 逻辑排序规则已核、Unity 静态实现同向；当前原 Battle Scene 的新版像素验收仍待。** Q09/P-04 与总目标开放。后继运行应使用版本化的新结果/PNG路径，保护旧四图和旧结果原件，先核当前 Scene clean/Camera enabled，再做当前场景自然完整 tick 的相同四图或更直接的可辨交叠像素见证；不得覆盖既有 `Temp/NTSD28_Q09_SameZNaturalTickPixel.*.result.json` 或旧图。

旧四图证据：`artifacts/diagnostics/NTSD28-Q09-SAME-Z-NATURAL-TICK-PIXEL-WITNESS-001/scene-471396E7-series/ANALYSIS.md`。本轮未改正式源码、Unity 生产/测试脚本、DAT、图片、Scene/Prefab、相机或非战斗代码。
