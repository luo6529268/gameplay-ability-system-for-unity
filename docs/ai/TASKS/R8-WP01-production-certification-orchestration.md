# R8-WP01 — production battle certification orchestration

> 日期：2026-08-23
> 状态：`COMPLETE AT APPROVED UNITY EVIDENCE / R1-WP02 FULL TRACE BLOCKED`

## Goal

在当前工作树、当前国际版Unity 2022.3.62f3和production配置上建立R8分层证书，逐项把R2～R7的
`RUNTIME_PENDING`结果推进到当前可取得的最高证据层；不能把旧U9报告、self-check、性能数据或
被阻塞的C++ full trace替代真实战斗验收。

## Scope

1. fresh Unity compile、完整`BattleRuntimeSelfCheck`和相关EditMode回归基线；
2. `NTSD_Battle`真实Play Mode下的移动/输入、对象交互、碰撞/命中、opoint/lifecycle子流程；
3. `CentralOnly`下entity/shadow/hit-record可见性、排序、挂点和无Legacy回退；
4. current-build 1000 production entity容量、0 B、Gen0/1/2、capacity fault与30 Hz门；
5. Windows Mono作为已取得的附加Player证据；IL2CPP按用户最新决定排除，不作为当前gameplay门；
6. 为每个失败输出最小重现步骤和对应D-ID；需要脚本修复时停止该验收项，先建立新的Task/Change Record。

## Authority / evidence

- 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release`的release live source；只读，不运行、构建或写入；
- Unity当前production runtime与中央表现是被验收对象；
- `R1-SOURCE-007-subflow-acceptance-matrix.md`定义S0～S5证据层；
- 2026-08-20 U9、旧C# parity与历史Player报告只作为历史基线，不能直接晋升当前工作树；
- R1-WP02 C++ full trace继续`BLOCKED`，因此S5不可伪造。

## Work packages

| WP | 范围 | 最低通过证据 | 状态 |
|---|---|---|---|
| R8-WP01A | 当前域清理、fresh compile、self-check、完整/聚焦EditMode | 0 compile error；self-check PASS；测试无失败 | VERIFIED：full EditMode 1357/1357；fresh self-check/compile PASS |
| R8-WP01B | walk/run/jump/turn/landing、WSADJKL、组合键、AI input | 同场景步骤、tick/按键、实际结果；自动或人工S4 | D-INP-010 VERIFIED：InputSystem DDJ→271、DRA→263；D-INP-006用户实体键盘/窗口焦点edge仍待人工 |
| R8-WP01C | character/weapon/special/effect、pickup/held/throw/grab/death/respawn/random weapon/opoint | 每组producer→consumer真实场景证据 | COMPLETE：01～06 VERIFIED（Unity S4）；07 synthesis完成；未关闭D-ID显式转交 |
| R8-WP01D | CentralOnly entity/shadow/hit-record、排序、挂点、可见性、真实DAT技能图片与pic→sheet/slice/UV | 正常可见Game/Scene/Player；技能图片内容/帧/UV正确；central draw>0；Legacy未成为production owner | COMPLETE AT AVAILABLE EVIDENCE / BLOCKED：01～07限定范围S4完成；B-R8-WP01D-08-01无authored state8000样本、08-02 C++ full trace阻塞。 |
| R8-WP01E | MobileExtended 1000 active与DesktopExtended capacity合同 | current-build 1000实体；正式窗口0 B/0 GC/0 fault；30 Hz目标 | VERIFIED Editor current-build：两组1800 tick、frame/main P95<33.333、0B/0 collection、capacity0、central/cleanup PASS；A/B 12 hashes equal |
| R8-WP01F | Windows Player附加认证 | 用户已明确排除IL2CPP后续处理；不得作为当前gameplay gate | STOPPED BY USER：R8-PLAYERBUILD-001 ABANDONED；不继续build/run/诊断/修复，不标双runtime VERIFIED |
| R8-WP01G | 证据汇总与差异登记 | 每个D-ID明确VERIFIED/RUNTIME_PENDING/UNKNOWN/BLOCKED及下一可实施包 | COMPLETE AT APPROVED EVIDENCE：R05 candidate/PreInteraction、R06 P1/P2、R07A/B/C central、R08 merge/split与R03/R04基础设施均闭合；R09完成68项最终对账。AI/F1F2/IL2CPP按用户范围排除，exact DAT witness与C++ full trace边界保留。 |

## Acceptance discipline

- 编译、自检、EditMode、Play Mode、Player和C++ trace必须分别记录，不能互相替代；
- 单个技能成功不代表模块或整场战斗完成；
- Play Mode只验证实际执行到的角色、按键、对象和状态序列；
- 性能报告必须同时记录逻辑tick、完整帧、GC、capacity、central draw与清理恢复；
- full trace不可用时，最终结论必须明确“C++ runtime full-trace证书未取得”；
- R8批准范围现已完成到当前可取得Unity证据层；仍不得把它宣称为C++ executable runtime完整动态对齐。

## Stop conditions

- fresh compile或self-check失败；
- 真实场景出现新的C++ source-contract差异；
- 需要修改gameplay、input、collision、held/link、opoint、render、pool、profile或scene；
- 需要改变长期架构、验收标准或批准的Unity adapter；
- C++ authority需要运行、构建、修改或写入。

触发时记录失败证据并为修复建立独立Work Package；R8其他不依赖项可继续，但不得掩盖失败。

## Out of scope

- T8默认`stage.dat`部署；
- Android真机；
- C++ executable运行、hook、instrumentation或authority目录写入；
- 服务器、Socket、ACK、房间、重连；
- 以R8为名顺手重构战斗或渲染架构。

## Final reconciliation（2026-08-24）

`R8-WP01G-R09`完成68项无遗漏对账：43项有Unity S4/runtime覆盖，5项exact production witness不可得，
1项source等价但full trace缺失，9项用户排除/未来替换，1项调试功能键policy，3项approved adapter/config，
6项test/worker/performance事实；合计68、missing0、extra0、duplicate0。

当前没有新增的正常战斗、production可达、source-confirmed且Unity未实现脚本差异。R8父编排因此在批准范围
和当前可取得Unity证据层收口；`R1-WP02` full trace、T8、authored state8000、部分exact DAT分支和人手硬件edge
仍按各自边界保留，不得包装成已验证。
