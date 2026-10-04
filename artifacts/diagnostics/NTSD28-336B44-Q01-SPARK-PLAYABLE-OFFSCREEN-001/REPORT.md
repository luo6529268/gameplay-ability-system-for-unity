# Q01/C040 正式 playable SPARK 离屏见证（2026-10-04）

**限定结果：当前 playable 的自然命中 SPARK 绘制可见。** 正式根 `NTSD2.8-Logan.exe` SHA-256 再核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 `resources/runtime/vfs/sprite/UI/SPARK.png` SHA-256 为 `15D8843E0CE87FF63F46DFF7170D30C23BAEA0F2799434B26717AADFD5EC881B`，与 Unity 已暂存文件同版。本报告使用与发行版对应且进入 playable 构建闭包的 `GameSession28` 与 `D3D11Renderer28`，**不把自编离屏工具当作根 EXE 实际 Present**。

仅扩既有 C040 诊断工具的 `--offscreen-first-positive`。它只运行已由正式源/根和原 Unity Battle Scene 配对的普通初态首阳 `b9-17-k19-x540-g560`，mode0/seed0、角都25/奇拉比75/凯97均action0，40个离散输入完整tick；第25 tick 奇拉比护甲命中与角都kind3抓取同轮发生。没有运行其余112案，也没有修改正式源码/EXE、DAT/图片、Unity生产脚本、Scene或菜单。

当前 playable 闭包加现有D3D11 renderer用 g++ 编译两轮均 exit0；第二轮新增同快照火花消融图。两轮单案均exit0，`cases=1 positive_cases=1`。第二轮 `source-ticks.csv`、`source-rng.csv` 和 `summary.csv` 各自与旧113案矩阵中的**同一案**逐字节相同；`first-positive.lfr` 与旧同案及第一轮均SHA `AFAC056E5D16E81C839360CB9B41EE851F225741FE8158ABA614091158CE4BC1`。第一/第二轮有火花PNG也逐字节相同，SHA `06A246284DBA3A3D91DE2B3ACAE0F01DCDE88C984A58AF7BBE0FF9A85CA62B55`。旧无开关113案没有重跑；代码路径仅增独立单案开关，不能把同案回归说成全部矩阵实测。

第25 tick 正式快照的 [SPARK记录](first-positive-20261004-02/tick25-sparks.csv) 有两条：ID19为不可绘制/资源不可用，ID0为 `drawable=1`、`resource_available=1`、源图块`(0,0,99,79)`、屏幕锚点`(559,352)`；[有序绘制命令](first-positive-20261004-02/tick25-entity-commands.csv)包含后者。原生D3D11得到1333×730的[有火花图](first-positive-20261004-02/tick25-offscreen.png)，再对**同一快照的副本**清空spark集合，以同一renderer实现生成[无火花图](first-positive-20261004-02/tick25-no-spark.png)。两图仅73像素不同，差异包围盒`x566..574,y338..355`，均在该99×79火花区域内；新增像素的4种RGBA颜色与正式SPARK第0图块全部4种非黑颜色严格相同。无火花PNG SHA `CF7A2477FE523E20FC0B6575157BF537196A0E7ADA8350301392CEEEA9BB19DC`；[逐项比较数据](comparison-20261004-02.json)另存。整图肉眼不易分辨被角色遮挡的小火花，因此用同帧消融差分确定其实际像素贡献。

原 Unity Battle Scene 同一输入链已有第25 tick 的实际1920×1080 [Game View SPARK见证](../NTSD28-336B44-Q01-SPARK-NATURAL-GAMEVIEW-001/REPORT.md)：正式同SHA图块被中央命令消费，命令投影邻域中116个特征色像素。本次证明当前 playable 的生产快照→D3D11路径也在该命中帧绘制正式SPARK，但两侧采用不同背景、镜头/视口与遮挡；不能由两个局部颜色计数断言逐像素形状、相对锚点或正式根 EXE GPU完全相同。Q01其余非对象图、Q09更广表现、Q12整场仍开放。

验证文件均写到新唯一目录，第一轮取图和第二轮消融原件并存；无旧结果覆盖或删除。`Tools/Validate-ChangeLedger.ps1` 实际 exit0/PASSED，1218 Records、当前10个受治理代码差异文件；[完整输出](change-ledger-validation.txt)已留存。相关已跟踪路径 scoped `git diff --check` exit0，第二轮 g++ stderr 0 字节。原 Unity Editor 本轮未运行，因此没有新的原场景/设备证书。
