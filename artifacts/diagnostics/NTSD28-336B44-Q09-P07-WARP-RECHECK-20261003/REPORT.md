# Q09/P-07：336B44 李的 OID204 离屏绘制复核

状态：`VERIFIED_SCOPED_CURRENT_PLAYABLE_WARP`。本报告只把旧 B1E13 版本的李子体绘制证据，在当前 336B44 playable 源码和正式资源上重新验证；不将旧版结果自动晋升为新版整项验收。

当前正式根 `NTSD2.8-Logan.exe` 在运行前核得 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。沿用既有只读诊断 `Tools/NTSD28Q09Diagnostics/lee_shadow_warp_probe.cpp` 和旧包的编译参数，重新链接当前 `source/ntsd28_core`、`source/ntsd28_playable` 以及 D3D11 渲染器；仅将输出 EXE 改到本目录。编译 [exit0](compile-exit.txt)，[编译输出](compile-output.txt) 为空，未修改正式源码、Unity 代码或资源。

输入为正式 `resources/runtime`、seed 682973786、mode0、背景23，李 OID7/X500/Z650 对鸣人 OID2/X1200/Z650，李在 tick2 攻击、tick3–4 防御；这是旧版报告的同一受控案例。首次把第二个 `complete_vfs_root` 参数误传为 `resources/runtime/vfs`，缺少该根下的 `decoded_dat`，初始化 exit5，原件保留在 [run-output.txt](run-output.txt) 和 [run-exit.txt](run-exit.txt)。将该参数更正为 `resources/runtime` 后，[运行输出](run-02-output.txt) exit0：tick6 有 5 个 owner0/OID204 子体、5 个子体本体 sprite 命令、0 个子体 shadow 命令、3 个普通 shadow 对照，camera `(0,0)`，视口 `1333×730`。

同一 tick6 快照由当前 D3D11Renderer28/WARP 分别绘制完整场景与只在快照副本中移除五个子体 sprite 的场景，两次之间没有再推进逻辑 tick。两张 `1333×730` PNG 相差 590 个 RGBA 像素，左上原点包围框为 `[402,630,444,651]`。[完整画面](tick6-full.png) SHA-256 `CCF0D55DF9D7DE0D3C7303E82839323872DD5751FC50A19B50264753932CA353`，[移除子体画面](tick6-without-children.png) SHA-256 `F9B9809B752E434F5CB596406A4AC5322E56237280CFDBA0C58AF8A972516AE8`；两文件分别与 [B1E13 旧包](../NTSD28-Q09-P07-PAIRED-WARP-LEE-001/ACCEPTANCE.md) 的对应 PNG **逐字节同 SHA**。这证明当前源码/资源在此受控绘制出口没有产生新首差，也证明五个子体本体像素实际进入当前离屏渲染器。

证据边界：这是重新编译的当前 playable 源码 WARP 输出，不是根正式 EXE 的 Present/GPU 截图；也没有本次同初态、同 tick 的原 Unity Battle Scene Game View。项目使用自己的背景及固定完整视口，不能把两边整屏作逐像素等价声明。P-07 的根 EXE/Unity 可比场景画面、Q09 其它非例外表现及 Q12 继续开放。此次没有进入 Unity Play、改 Scene、DAT、图片、相机、生产脚本或非战斗模块。

## 当前正式根回放与快照命令补证

随后用当前 336B44 根 EXE 对历史同输入 LFR 做无窗口回放，[原始参数](root-argv.json)、[根报告](root-report.json)和[根 trace](root-trace.jsonl)均另存本目录；exit0、`passed=true`、`failureCode=0`、声明45 tick/完成46行（含终行），但正式报告的 `nativeParityClaim=false` 保持原样，不扩写为完整内存状态同态。该 LFR 是旧版受控输入载体，本次权威行为来自当前根 EXE 的新回放输出。

另将既有快照探针重新链接当前 336B44 Core/`GameSession28`，编译 [exit0](snapshot-compile-exit.txt)，[运行](snapshot-run-output.txt) exit0；新 [tick5–13 快照 TSV](lee-shadow-snapshot-336b44.tsv) SHA-256 `ACACEBAC6779C3C402E62B5AA11A6F5342BE5735E9A724F5B8B6B66CF59C8D2B`，与旧版 TSV 逐字节同 SHA。独立[逐行比较](root-snapshot-comparison.json)显示 tick6–13 的 40 个 OID204 子体按槽位核对 `oid/owner/action/pic` 为 **40/40 行、160/160 字段一致**，8/8 tick 的 sprite 总数一致、首差无；tick6 根 trace 的 slots51–55 均为 action20/pic28。源码快照直接记录该帧 5 个子体本体命令、0 个子体 shadow 命令；根 trace 只有 sprite 总数，没有逐条阴影命令身份。这把 P-07 的受控实体/图格证据更新到当前正式根，仍不能声称根 EXE 实际 GPU 阴影或与 Unity 同场景像素已对齐。

当前正式 `resources/runtime/decoded_dat/a/cha/cha.dat` 与 Unity `Assets/NTSD/Content/LoganRuntime` 同路径 SHA-256 均为 `20C0704FC53B3D18D77FE61566F0B7445C31DAD9C5D8197C5444F960E89E6692`；对应 `vfs/a/cha/cha.png` 双侧均为 `BF69A255C2782014D430664BEDD23E41B961D4E40789B3E360ADAFA3715DF352`。这是本次重新量得的内容身份，不能代替当前 Unity Play 的运行时绑定或画面观察。
