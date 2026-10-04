# Q09/F02 原 Battle Scene 的 alpha=1 同相画面见证（2026-10-04）

状态：`VERIFIED_SCOPED_UNITY_ALPHA_ONE_ANCHORS / Q09_OPEN`。正式规则身份为根目录 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；画面参照是其对应 playable `presentation_interpolation.cpp` / `d3d11_renderer.cpp` 用正式资源生成的 tick39、未插值 `alpha=1` WARP 离屏图。这个辅助图不是根正式 EXE 实际 GPU Present。Unity 使用项目自有背景和固定 2048 源像素宽视口，故只配对实体的相对画面锚点，不把两张整屏当作可逐像素相减的相同画布。

原项目 Editor PID105896/本地 Unity-MCP 6401 在单一干净 Menu、非 Play、未编译时启动唯一请求 `f02-alpha-one-20261004-01`。请求复用已证的 mode0/difficulty0、seed `0x28A55A5A`、Z400、鸣人 OID2/多由也 OID36/武器 OID600 拾取→轻投→kind10→高速释放链，生产 `SimulationTickDriver` 完成 45 tick。新 opt-in 仅在暂停的相对 tick39 等到中央帧的**实际建帧 alpha=1**才发 Game View 截图；结果直接记录 `simulationTick=displayTick=frameTick=44`、`generation=90`、`builtDisplayAlpha=1.0`，中央 10 条命令。相对 tick30/39 两张真实 Game View PNG 均为 1920×1080；目检 tick39 武器位于多由也右上方，左侧 OID219 黑格与此前已归因的正式素材一致。

| 相对实体中心 | 正式 raw WARP 的原生中心间距换算为 1920/1333 输出像素 | Unity alpha=1 中央命令间距 | Unity−正式 |
|---|---:|---:|---:|
| 武器−鸣人 | 549.497 | 549.141 | −0.357 输出像素 |
| 武器−多由也 | 77.059 | 76.172 | −0.887 输出像素 |

正式 raw WARP 几何为武器中心 X584、鸣人 X202.5、多由也 X530.5；Unity 同 tick 中央 Entity 命令的 world X 分别为 `-3.0375/-8.8950/-3.8500`。Unity 命令从旧中间插值帧的武器 `-3.4984` 变为未插值的 `-3.0375`，与生产投影公式一致。配对计算、文件 SHA、严格旧新数组比对及关闭值在 [comparison.json](comparison.json)；原始 [Editor 结果](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-alpha-one-20261004-01/00059.json)、[tick39 Game View](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-alpha-one-20261004-01/game-view-tick39.png)、[正式 playable raw WARP](../NTSD28-336B44-Q09-F02-FORMAL-OFFSCREEN-001/source-bg1-z400-20261003-02/tick39-offscreen.png)均保留。这里的 <1 像素相对锚点残差是本例的测量结果，不替代对图格采样、阴影、其它位置或整场视觉的验收。

新结果与先前正式根/Unity已配对的原 Scene run-07 做严格数组比较：46 组（初始+45 tick）`samples` **46/46 完全相同**，47 条 `eventRows` **47/47 完全相同**；因此沿用既有的正式根选定战斗字段 **2346/2346** 证据，不重复全测试矩阵。结束时有序关闭 `Completed / RuntimeMapCleared`，World 对象、槽、池借用、活动池对象和 Sprite 均为 0；原 Editor 回到单一干净 Menu、非 Play、非编译。Battle/Menu/GameConfig/Mode 四保护文件在**本次运行前后** SHA 相同。Menu 文件与 2026-10-03 旧 run-07 的跨日哈希不同，属于已有受保护的其它工作，不将其掩作跨日同一文件。

验证：生成 `Assembly-CSharp-Editor.csproj` 两次编译均 0 错/297 警告；原 Editor 两次刷新后 DLL 时间晚于脚本，Editor.log 最新构建无 C# 错；唯一 Play 结果 `CAPTURED / DONE`。`Tools/Validate-ChangeLedger.ps1` 返回 0 / `PASSED`，1213 Records、6 个当前差异代码文件均覆盖，[最终验证日志](change-ledger-validation.txt)保留；现有历史 Record 的非差异路径警告不影响本包。本包脚本与共用文档的局部 `git diff --check` 返回 0；全工作树此前的 `git diff --check` 因用户 Menu Scene 原有行尾空格返回 1，本包没有修改那些行。

只关闭 F02 **当前 Unity alpha=1 画面锚点与当前 playable raw WARP 同相比较**。正式根 EXE 的实际 GPU Present、完整背景与视口例外之外的更多可比像素、真人物理按键自然整链、Q07/Q09/Q12及总目标仍开放。未修改 DAT、图片、Scene、相机、生产代码或非战斗逻辑。
