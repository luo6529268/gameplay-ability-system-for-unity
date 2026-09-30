# G0 — 336B44 发行身份与 playable 闭包

状态：`USER_SELECTED / IDENTITY_AND_SOURCE_CLOSURE_VERIFIED / RUNTIME_PARITY_NOT_YET_PROVEN`。2026-09-30 用户明确选择当前根目录含 35 项限定修复的发行版作为后续 Unity 战斗规则权威，并要求重建对齐文档。此次只读检查正式目录；编译输出写入独立系统 Temp，不覆盖发行 EXE。

| 对象 | 本次实测 |
| --- | --- |
| 正式根 `NTSD2.8-Logan.exe` | 5,469,312 bytes；SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；FileVersion `2.8.3.3`；ProductVersion `2.8.3.3-development`；OriginalFilename `Ntsd28Playable.exe` |
| 启动器 | SHA-256 `50CE23E2787CF7B000CE93C42DECC811F87EA0D84FAC016FBB99B28FBFE9FB91` |
| `source/README_SOURCE.md` | SHA-256 `C0BA44DB4A9037E5086348668F1046A83A760D351F2BB2D6062E724676BB3D85`；声明该源码快照对应当前发行 EXE，源码不参与普通启动 |
| `source` | 159 files / 4,371,509 bytes；规范化树 SHA-256 `2924DDD8C153EE34378081578D21D7EF9EB7E8BDF73799695C89C56DC3418CA1`；排除 build 产物后与 `RELEASE_INFO.md` 一致 |
| `resources/runtime` | 2,656 files / 101,099,439 bytes；规范化树 SHA-256 `F3EA4516BD903F10131217A856947F7C0F91DB72F506F6E2A7F88D6281F3FD41`；与 `RELEASE_INFO.md` 一致 |
| `build.ps1 -Target playable` | 28 Core C++、10 playable C++、44 core/playable header，共 82 文件；路径排序后逐文件 `relative/path + NUL + raw bytes` SHA-256 `B97DF3C5BB75058D13FD9CCC7A542536BB956A1F496A9140BD98C06D6F8638AA` |
| source-capture 子闭包 | 28 Core C++、Session/Selection/Scenario 3 C++、44 header，共 75 文件；同算法 SHA-256 `066A8CDEEBB7A48706111443126E8F6EFCFD140B692E85EF9645149C8868520C` |

闭包算法用已逐树 SHA 复原的旧 B1E13 源码作反向控制：旧 82 文件复算为已公布的 `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`；旧 75 文件复算为已公布的 `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`。新 `build.ps1` 的 playable 源文件清单和旧版相同，仅新增输出目录选项；35 项变更中的 Core/Session 与头文件实际进入该闭包，`offscreen_gate_main.cpp`、测试源码和 `test.ps1` 不进入正式 playable。此处的文件参与性不等于某个分支在普通战斗中可达。

新源码 `build.ps1 -Target playable -OutputRoot <独立系统 Temp>` 用 GCC 15.1.0 执行，返回码 0，生成 5,469,312 bytes 的 `Ntsd28Playable.exe`，SHA-256 `C21DC498EF377FCB5AA149B17FC470865A6666B47CE915A81D2DD157AEAD0D1A`。它与正式根 EXE **不是逐字节同一产物**，不得取代正式行为权威；本次只把它作为新 82 文件源码闭包可编译的证据，未据此证明编译产物行为与正式根完全相等。`RELEASE_INFO.md` 的发行前 900 Tick 冒烟和单测是发布说明记录，本次没有重跑，也不把它当作 Unity 对齐证书。

旧版 B1E13 的根路径身份已被用户选择的新版取代；旧 EXE 及其源码的局部证书只在明确比较字段/时序未变且新版本补证后复用。`resources/runtime` 与更新前相同是发布说明声明，本次没有旧 runtime 备份树可独立复算；当前 runtime 身份已独立复核。后续规则验收必须以 336B44 正式根可观察输出、当前 playable live source 和当前资源为准，逐包给出 Unity 首差与验证层级。背景、两类模式 DAT、结果页设置/重赛等用户例外不因换版进入任务。
