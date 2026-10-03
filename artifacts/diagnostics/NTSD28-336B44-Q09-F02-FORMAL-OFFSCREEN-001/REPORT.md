# F02 tick39 当前 playable D3D11 离屏图（2026-10-03）

结论：对应当前正式 336B44 的 playable `GameSession28` 自然 F02 源码会话，在相对 tick39 交给生产 `D3D11Renderer28` 后，WARP 无窗口输出中**实际出现黑色特效块**。这把先前的“源码按 alpha 应绘制黑色”推进为当前 playable 源码的实际 GPU 离屏画面证据。正式根 EXE 的实际 Present/GPU 截图仍未取得；项目 Unity 使用自有背景及 2048 宽固定视口，因此两张整屏不能作逐像素同态判断。

## 输入与输出

- 当前根 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。诊断只编译对应 `source/ntsd28_core`、`source/ntsd28_playable` 及现有工具；没有更改根 EXE、权威源码、正式资源或 Unity 生产内容。使用正式 `resources/runtime`，mode0、seed `0x28A55A5A`、正式背景1/Z400，仅供与项目自有地图取得共同规则纵深；鸣人OID2、Tayuya OID36、武器OID600，正式既有条件输入在tick18拾取、24轻投、30高速释放、31～35 kind10、39 F02返回。
- [成功源码运行](source-bg1-z400-20261003-02/summary.txt) exit0；[原生 tick39 离屏图](source-bg1-z400-20261003-02/tick39-offscreen.png) 为 `1333×730`、SHA-256 `437E550EB87F575140085D5C7E503CEE51B721FAB4C59B4676D2BC6F5EA09123`。实际画面在左侧呈黑色方块并露出青色特效星点。
- [同帧 sprite 几何](source-bg1-z400-20261003-02/tick39-sprites.csv) 恰五行：鸣人 slot0/pic1、Tayuya slot1/pic31、OID600 slot2/pic1、OID219 slot51/pic3 与 slot50/pic8。两个 OID219 的正式画面矩形分别是 `(left148,top350,81×82)` 和 `(left157,top356,81×82)`；在 PNG 左侧局部 `(120..259,320..459)` 按 RGB 各通道≤3 做四邻接，最大黑色连通区为 `(148,350)..(237,437)`、7,680 像素。直接抽样 `y350` 的 `x147` 为背景、`x148..228` 黑色、`x237` 背景；`y356` 第二格把黑色扩展至 `x237`。
- [正式根 trace](../NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/bg1-z400-root-01/root-trace.jsonl) 同相对 tick39 的 `render.sprites=5`，五实体分别为 slot0/OID2/pic1、slot1/OID36/pic31、slot2/OID600/pic1、slot51/OID219/pic3、slot50/OID219/pic8。此项配的是可见实体身份和图格，不冒充根 EXE GPU 输出。Unity [同帧截图/中央命令诊断](../NTSD28-336B44-Q09-F02-CENTRAL-ALPHA-FIRST-DIFF-001/REPORT.md) 在不同背景/视口下也有相同两个 OID219/pic3/8，且预测/实测黑格左上为 `(216,806)`。

## 诊断失败与回归

第一版编译退出0，但 [首轮目录](source-bg1-z400-20261003-01/) 在写出 PNG 和五行几何后以 Windows `0xC0000005` 退出；CSV/LFR 未完成，因此首轮 PNG **不作为通过证据**。原因在诊断工具的 `D3D11Renderer28` 局部对象生存期越过 `CoUninitialize`；仅调整其局部作用域，在 COM 反初始化前释放 renderer。首轮全部原件保留，未归因战斗规则或正式渲染器。

修正后 [build-02](build-20261003-02/compile-output.txt) g++ 编译 exit0、无诊断；新 opt-in 和另一个 [旧无开关回归](old-flag-regression-20261003-01/summary.txt) 各自 exit0。与先前 BG1/Z400 正式源码基线相比，两次新运行的 `summary.txt`、`source-ticks.csv`、`opponent-ticks.csv`、`relation-hits.csv`、`source-packets.lfr` **各5/5逐字节相同**；开关没有改变模拟和 LFR。新增 PNG、几何和源码工具的接线不参与正式 playable 构建闭包。

本 Change 只关闭“对应 playable 源码实际离屏绘制黑格”的子门。Q09 其他阴影/火花/遮挡/插值、F02 全链、正式根 EXE 同条件 GPU 画面、Q12 及总目标仍开放。不应为去掉这块正式源码也会画出的黑色修改 DAT、图片或 Unity 通用 alpha 规则。
