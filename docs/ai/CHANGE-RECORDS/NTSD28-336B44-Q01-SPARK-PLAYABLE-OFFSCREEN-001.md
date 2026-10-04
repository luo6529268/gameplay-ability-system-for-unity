<!-- CHANGE-RECORD
id: NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c040_all_natural_triad_probe.cpp
authority: 336B44 formal root identity and matching playable GameSession snapshot/D3D11 renderer
evidence: docs/ai/TASKS/NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001.md
-->

# Q01 当前 playable SPARK 离屏画面见证

脚本修改前建立。现有 C040 正式首阳矩阵与原 Unity Battle Scene 的同一第25tick已有命中/动作配对，Unity 已保存真实 Game View 可见 SPARK；正式 SPARK.png 原文件逐 SHA 同版。当前缺当前 playable 渲染路径在同条件同tick消费 SPARK 的离屏图证据。仅扩既有 C++ 诊断的新 opt-in，旧113案无开关行为不变；不修改正式源码/EXE、Unity生产、DAT、图、Scene或非战斗。

预期新增符号：独立开关解析、单案选择、tick25快照/绘制命令导出、局部 D3D11 离屏渲染。新增输出只进独立不存在目录，旧文件不覆盖。`D3D11Renderer28` 必须先析构再 `CoUninitialize`。验收与前向回滚见同ID Task；实际文件/符号、编译/运行、图像核验、失败和未验项随后追加。

2026-10-04 代码已写：实际仅修改声明的 `c040_all_natural_triad_probe.cpp`。`main` 新开关精确接受 `--offscreen-first-positive`，只选既有首阳 `b9-17-k19-x540-g560`；无开关仍按旧113案建表和循环。新分支在tick25从当前 `GameSession28::snapshot(false)` 输出 SPARK/有序Entity命令 CSV，并在已有 D3D11 实现绘制1333×730 PNG；renderer局部析构后才 `CoUninitialize`。无正式源码/EXE、DAT/PNG、Unity生产/Scene或非战斗修改；`git diff --check` scoped exit0。编译与运行待。

首轮窄验证：新工具以当前336B44对应 playable 闭包编译exit0，独立单案运行exit0，`cases=1 positive_cases=1`，tick25快照有一条drawable/available spark命令（另有一条不可绘制记录），D3D11生成1333×730 PNG；新LFR SHA `AFAC056E...BC1`与旧矩阵首阳LFR相同。目检整图不能精确归因火花像素，故按Task补一张同快照无spark消融对照；首轮PNG/CSV及EXE保留原件，新编译/运行用唯一目录。当前只到`COMPILE_PASS`，尚未宣称可见像素对齐。

第二轮限定验证：同一C++代码路径的第二个独立编译/单案运行exit0；代码只给快照副本清空sparks，再用正式renderer生成无火花图，不反写World/原快照。两图像素差73，包围盒x566..574/y338..355，完全位于正式SPARK pic0 99×79区域；新增颜色4/4等于正式图块非黑颜色。新单案三CSV逐字节等于旧矩阵同案，LFR及有火花图也逐SHA等于第一轮；当前root EXE GPU/Unity精确同相位整图未验，旧无开关113案未重跑。仅此离屏子门`VERIFIED`，Q01/Q09/Q12开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001/REPORT.md)。最终ChangeLedger validator与scoped diff待记录。

交付检查更正：`Tools/Validate-ChangeLedger.ps1` exit0/PASSED，1218 Records、当前10个受治理代码差异文件；[输出](../../../artifacts/diagnostics/NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001/change-ledger-validation.txt)保留。声明脚本/总表/Ledger/STATE/handoff 的 scoped `git diff --check` exit0，第二轮g++ stderr空。旧113案未重跑符合本包同案窄回归口径；未验证默认全矩阵新二进制与旧全矩阵逐字节同态，不把其扩成全量证书。
