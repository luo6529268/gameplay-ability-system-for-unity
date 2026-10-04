# NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001

状态：`VERIFIED_SCOPED_PLAYABLE_OFFSCREEN`。父目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-01 Q01`，回链 Q09 战斗表现。

权威与前置：当前正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；对应 playable `GameSession28::step`、`GameSession28::snapshot`、`D3D11Renderer28::render` 及正式 `resources/runtime`。既有 C040 普通初态三人首阳 `b9-17-k19-x540-g560` 在相对 tick25 命中，原 Unity Battle Scene 已保存同条件 SPARK Game View 和资源 SHA。根 EXE 本体的 Present 不由自编工具代替。

唯一代码路径：`Tools/NTSD28Q07Diagnostics/c040_all_natural_triad_probe.cpp`。只加 `--offscreen-first-positive` 可选入口，使其仅运行上述一案 40 完整 tick，并在 tick25 保存正式快照中 SPARK/实体/绘制命令几何及 1333×730 D3D11 离屏 PNG；旧无参数入口、113案矩阵及旧输出不变。只用全新结果目录，禁止覆盖既有 CSV/LFR/PNG 或编译产物。不修改正式发行 EXE/源码、DAT/图片、Unity 脚本/Scene/相机、菜单或非战斗内容。

验收：核实正式根/正式 SPARK 身份；从当前 playable 闭包编译新诊断且0错误；新入口唯一案应保持既有 tick25 的命中与动作/HP/关系结果，输出可解析的命令/资源路径、有效 PNG；将新入口相同案的 CSV/LFR 与旧矩阵相同案逐值/逐字节比较，避免诊断改变规则。若源快照没有 SPARK、D3D失败或图中不可见，保留原件并记为首差或条件阴性，不凭预期修生产。正式/Unity不同背景和视口只按同实体相对锚点或局部颜色键比较，不作整屏逐像素同态。改后跑最窄 C++ 构建/新入口、ChangeLedger validator 和 scoped diff check。

风险与回滚：COM 生命周期在 renderer 析构后结束；旧矩阵必须独立不变。若诊断失败只反向修正此工具的增量，不通过 Git restore/reset 或删除用户文件；新证据原件保留。此 Task 最多关闭 playable 离屏 SPARK 的限定子门，Q01/Q09/Q12及正式根 GPU 仍开放。

首次运行补充判据：首个新入口已出现一个 drawable/resource-available 的 SPARK 和有效PNG，但整图目检不足以单独证明该图中哪些像素由火花贡献。保持第一轮原件不覆盖；在同一tick快照另用当前D3D11 renderer绘一张仅移除spark绘制命令的独立对照图，比较两图差分。仅此诊断消融，不修改战斗快照或生产规则；新编译与结果均用第二个唯一目录。

2026-10-04 出口：两轮编译/单案运行exit0；正式第25tick可绘制SPARK pic0，D3D11 1333×730两图仅73像素差异，4种新增颜色均来自正式SPARK第0图块。新单案3 CSV与旧矩阵同案逐字节相同、LFR同SHA，旧113案未重跑。只关闭当前playable此案离屏可见子门；根EXE实际Present、精确Unity同相位像素及Q01/Q09/Q12仍开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001/REPORT.md)。
