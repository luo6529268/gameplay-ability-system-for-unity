# Q05 一次协调的内容/runtime/schema迁移

状态 IN_PROGRESS / Q05-A2_CONTENT_MODELS（Q04-A/B限定VERIFIED）。用户总目标及D-022已授权此批；不再询问是否整体切换DAT。已保存115个候选路径inventory；当前脚本变更由Q05-A1准确子Record覆盖，其余候选路径未自动取得修改scope。

## 必须读取的冻结合同

- Q03工件目录的Q03-EXIT-REPORT.md、VERSION-IDENTITY-AND-CAPTURE-CONTRACT.md（含base-shell1→2更正）、OPOINT-AND-HELD-DEPTH-CONTRACT.md、JOINT-FIELD-MATRIX.md、decoder-contracts.json。
- Q03 numeric37 native.tsv/comparison、geometry14×3结果、semantic-identity-test-vectors.json；Q01六DAT九frame原始输入/字段差异。
- Q04-A与Q04-B最终Record及实际测试证据。不得重做已退休gate/producer/reader。

## 窗口内顺序与出口

0. 按冻结字段清单生成当前准确code-path及符号/调用者表，检查工作树，建立唯一父Change及必要的独立子Record；每个实际改动脚本先声明，不用目录glob替代路径。检查现有Unity编译状态和snapshot测试基线。此步骤只核对实施路径，不从头重做Q03。
1. 新版来源数值准入与内容值：strict int、ITR首整数、finite float32（包括hex/指数、负零/subnormal/舍入），CPoint27/alias独立与canonical bits，OPoint24与task完整值复制，BDY zwidth/geometry presence，weapon_strength index+19，以及WPoint原版未知字段/lexer准入。保持原始AST/输入身份；不得用吞frame、默认补零或手写假projection让差异消失。
2. 删除已退休载体：mass、五类reserved及holderCopySlot任务别名、旧Oscillate两项；准确移除alias/default/reset/copy/ECS/hash/diagnostic/测试引用。保留有效TrackerParentHandle/Owner/Spawner等独立合同。新+2F8建立独立int32/default -1及完整claimed/raw slot/复制/复用链；新语义producer和AI读取按Q06逐包接线。
3. 完成raw与解码语义身份分层、native candidate/cache/publication版本，以及本地验证会话的确定catalogFingerprint绑定；snapshot双OPoint owner空队列guard/禁止tick中capture或restore，失败不Flush、不丢任务、不建singleton。
4. 在同一个协调窗口统一版本：entity12→13、aggregate20→21、checksum23→24、character-shell1→2、base-shell1→2；trace及raw/source wrapper按冻结合同同步。其他未变payload和wire1保持。字段可用性如实记录，旧6个MISSING不能无证据晋升。
5. 编译、聚焦与相关回归、完整SelfCheck、旧midbattle拒绝、新capture→restore→同seed/input replay→checksum、pool slot复用、零残留、真实Play及Scene dirty unchanged验收。明确区分内容合同通过与新producer尚未接线，不能宣布整个新内容可用；Q06/Q07继续按原依赖推进。

允许逐步写入和编译中间状态，但不得将半迁移版本发布为新正式baseline、启动Q07正式资源部署或额外开第二次不兼容窗口。最终脚本Record/ledger要覆盖所有关联字面量和测试；保留旧capture历史身份，不覆写旧证据。

## 范围、风险与回滚

候选范围来自Q03明确的Animation/DatParser/Simulation/Host/Lockstep/Diagnostics及其测试、现有NTSD28Parity/AuthorityTrace/ContentAudit工具；修改前逐文件列Record。当前尚无脚本授权清单的外部Server/package/Gen/Plugins代码保持只读。不得借本批做Mono/Core架构拆分、GAS重构、Scene/Input Actions/第三方更新或网络功能扩建。

风险集中于类型/复制遗漏、同tick读写与版本混用、输入准入扩散到非战斗caller；用固定native见证、旧内容加载回归、场景/资源hash、严格身份与版本拒绝验证。33ms、十一阶段和非战斗行为保持。正式DAT/角色图的部署与精确旧资源处置留Q07；默认stage.dat仍暂缓。

回滚只在用户批准后逆向本窗口准确增量、保留其他未提交工作；不恢复旧midbattle adapter、不用破坏性Git命令或删除未知文件。本Task的准备不是Q05已实施。Q04-B实际出口后将状态改为READY，下一动作是事前inventory/Record。

Q04-B出口已满足；下一从步骤0准确文件清单与Record开始，Q05尚未改脚本。


2026-09-13执行更正：已从步骤0推进到数值decoder子项；以上“尚未改脚本/下一开始”是启动前历史。现有schema与正式资源仍未变，后继模型/身份/runtime/版本步骤未实施。


Q05-A1已限定VERIFIED_NUMERIC_HELPER_ONLY（REPORT同ID）；下一唯一Task NTSD28-Q05-LOGAN-CONTENT-MODEL-INTEGRATION-001，READY_FOR_EXACT_PRECHANGE_RECORD。父Q05保持IN_PROGRESS，尚未迁移模型/版本/资源。


2026-09-13当前：A1限定VERIFIED；A2的CPoint27已写且110 focused/完整SelfCheck通过，NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001保持SOURCE_INTEGRATION_PENDING。下一OPoint24 Task，后继BDY/strength/入口/身份/版本仍未完成；不从顶部旧启动语句重做已完成内容。


当前增量：OPoint24 NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001已FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING，98项/完整SelfCheck通过。下一Geometry内容Task准确Record；CPoint与OPoint都保持未关闭回访，不重复模型实现，不把旧投影或旧来源当作新身份。


当前Geometry NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001已FOCUSED_TEST_PASS，native88/完整375/SelfCheck通过，来源及算法未接。独立4旧夹具修正已VERIFIED_TEST_ONLY。下一ITR40/strength19 Task，CPoint/OPoint/Geometry三项保持来源/identity/Play回访，不重复模型实现或重跑已关闭审计。


当前：单记录ITR40/strength19 NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001已FOCUSED_TEST_PASS，489/SelfCheck通过；表头1..9/重复/caption合同更正及真实FieldBag词法区别已记录。下一Native strength table admission Task，非重新做字段模型。Scene文件额外变化ORIGIN_PENDING已询问用户，不能覆盖或写基线未变；parser独立工作可继续，Play出口须确认。

当前帧内容接线 NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 已限定交付（55348/330/557 focused/SelfCheck）。完整Q05步骤1仍须 NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 测量definition header域，避免只以frame投影/可构建认证全部内容；然后继续既定步骤2～4。新FrameSounds/profile、centerz/chp/cmp和六double必须入semantic identity；Q06分reader motion/resource、Q09 centerz、Q10音频仍有明确回访，不得丢失。

Definition头部审计 NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 已限定交付；具体数值/缺载体与排除项见REPORT。下一 NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001，完成Q05步骤1的metadata门槛再身份/载体/版本；不重写18armor及已匹配sequence/weapon sound，不恢复平台取景/HUD/选择流程排除。

当前出口更正：NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001与前BMP/stats、typed frame、strength及模型子包完成本父Q05步骤1的来源门槛，SOURCE_MODEL_FOCUSED_PASS（非runtime全对齐）。下一唯一NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，依父步骤2→3→4→5，不直接跳identity或部署。CPoint/OPoint/Geometry/ITR旧SOURCE_INTEGRATION_PENDING已有typed frame/manager证据满足来源子条件；identity/schema/consumer/Play仍待。禁止computer-use；任务外Foot18删除与新目录、Scene旧精度差异保护。

当前：NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001和NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001已FOCUSED_TEST_PASS，46/SelfCheck通过；独立+2F8字段名ObjectAiExcludedGroupSourceSlot2F8，source/owner不别名，producer/AI在Q06。下一NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001，再五reserved；全Q05版本/身份/guard/Play仍待，保持未发布中间态。禁止computer-use。

最新出口：NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001已FOCUSED_TEST_PASS/SCOPED_PLAY_PASS（890/SelfCheck/两probe），Mass/Oscillate存储删除，规则核心主体保持。下一唯一NTSD28-Q05-FIVE-RESERVED-CARRIER-RETIREMENT-001；五reserved、identity/guard/版本/回放仍待，当前不发布中间payload。禁止computer-use。

当前NTSD28-Q05-GRABBEDBY-TRACKERFLAG-CARRIER-RETIREMENT-001已407/SelfCheck/scoped Play限定通过；下一唯一NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001，余WeaponState/HolderCopy及identity/guard/联合版本/回放保留。禁止computer-use。
