# Q09/F02 当前 playable 插值相位 WARP 限定见证（2026-10-03）

结论：同一个 F02 相对 tick39，在当前 336B44 对应 playable 的生产插值函数取 `alpha=0.26` 后，OID600/pic1 的原生图格左边从未插值的 `560` 变为 `530`，中心从 `584` 变为 `554`。这与原 Unity Battle Scene 保存中央命令所反推的 `-30` 个正式源像素显示位移一致。以各自画面中的鸣人和 Tayuya 中心为参照、再按 `1920/1333` 换算，Unity 武器中心相对鸣人的位置残差为 `-0.36` 输出像素，相对 Tayuya 为 `-0.89` 输出像素。先前拿未插值正式图与插值 Unity 图相减的约41像素差值，不能作为武器真实位移错误。此证据只关闭 **F02 保存帧的量化锚点/当前 playable WARP 诊断子门**；正式根 EXE 实际 Present、精确同 alpha 的 Unity 运行时见证、阴影/像素同态、Q09/Q12 和总目标仍开放。

## 输入、权威与输出

- 正式根 EXE 身份 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；本诊断使用声明对应且进入 playable 构建闭包的 `GameSession28`、`presentation_interpolation.cpp`、`D3D11Renderer28` 和正式 `resources/runtime`。本轮只扩项目内已有的 [F02 C++ 诊断工具](../../../Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp) 的独立开关，不更改正式 EXE/源码或 Unity 生产文件。输入为 mode0、seed `0x28A55A5A`、背景1、Z400、鸣人2/Tayuya36/武器600、既有 F02 拾取/轻投/高速释放/kind10 链；tick39 取 tick38 与39快照作插值。
- [alpha0.26 样本](alpha026-20261003-01/tick39-alpha026-sample.txt) 记录 `adjacent=1`、`interpolated_entities=5`、武器 `delta.x=-30`。[五实体几何](alpha026-20261003-01/tick39-alpha026-sprites.csv) 记录武器 slot2/OID600/pic1 左530、宽48，鸣人左163/宽79、Tayuya左491/宽79，OID219/pic3左148、pic8左155；五者均使用正式 PNG 路径。原 [未插值几何](../NTSD28-336B44-Q09-F02-FORMAL-OFFSCREEN-001/source-bg1-z400-20261003-02/tick39-sprites.csv) 的武器左560、pic8左157。新增 [alpha0.26 WARP PNG](alpha026-20261003-01/tick39-alpha026-offscreen.png) 为1333×730、436,184字节、SHA-256 `F89473EA85EE411465027BEBA5B4640FC7A23D0854AEFA079466B02E143B4A86`；已实际打开目检，黑色OID219特效及青色点仍绘出。
- 原 Unity [保存的中央命令与同 tick 样本](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-central-alpha-20261003-03/00058.json) 中，三个对应实体底部中心 world X 分别为鸣人`-8.8950`、Tayuya`-3.8500`、武器`-3.4984`。保存的 [Game View PNG](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-central-alpha-20261003-03/game-view-tick39.png) 是1920×1080；项目使用自有背景和固定2048宽视口，不能用整屏逐像素相减。

## 相对锚点核对

正式 alpha0.26 几何的中心为鸣人`202.5`、Tayuya`530.5`、武器`554`。Unity 当前相机 X−1.79、正交半宽10.24，1920宽视口的输出中心由上述保存命令得到鸣人`293.90625`、Tayuya`766.875`、武器`799.8375`。

| 同帧中心间距 | 正式原生 × `1920/1333` | Unity 保存中央命令输出 | Unity−正式 |
| --- | ---: | ---: | ---: |
| 武器−鸣人 | `506.28657` | `505.93125` | `−0.35532` 输出像素 |
| 武器−Tayuya | `33.84846` | `32.96250` | `−0.88596` 输出像素 |

`alpha=0.26` 落在由 Unity 保存命令反推出的约 `[0.24909,0.27342)` 区间，故两侧取得相同的**整数显示位移**。旧 Unity 图没有独立记录精确 `displayAlpha`，不能声称两张 PNG 的连续采样时间点精确相同；表格只检验该帧量化后的实体中心间距，不覆盖图格大小、过滤、阴影、火花、遮挡或音频。

## 编译、回归与边界

- 在新 `build-20261003-01` 中以旧 WARP 编译参数加入当前 playable `presentation_interpolation.cpp` 编译，`g++` 退出0，`compile-output.txt` 无诊断。新开关 `--offscreen-interp-tick39` 退出0；旧无开关、旧 `--offscreen-tick39` 各在独立新目录退出0。三次运行的 `summary.txt`、`source-ticks.csv`、`opponent-ticks.csv`、`relation-hits.csv`、`source-packets.lfr` 各5/5逐字节等于先前 BG1/Z400 基线；旧离屏开关的五行几何和 PNG 也逐字节同旧输出，PNG SHA 仍为 `437E550EB87F575140085D5C7E503CEE51B721FAB4C59B4676D2BC6F5EA09123`。没有修改模拟、LFR或旧离屏行为。
- 该 PNG 是**对应正式 playable 源码的 WARP 无窗口图**，不是正式根 EXE 的 GPU Present；Unity 侧使用既有原 Battle Scene 保存证据，本轮没有重新进入 Editor。下一 Q09 真实画面验收应在原 Editor 直接记录 `displayAlpha`，将相同连续 alpha 输入 playable 并比较局部实体/阴影像素，同时保留用户指定的背景和固定视口例外。不能因本子门通过而宣称 Q09/F02 全链完成。
