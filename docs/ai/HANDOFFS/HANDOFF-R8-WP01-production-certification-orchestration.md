# HANDOFF — R8-WP01 production certification orchestration

> 日期：2026-08-23
> 状态：`IN_PROGRESS / NO GAMEPLAY CHANGE`

## Current

- R7 repair orders 1–11全部关闭；
- UnityMCP Session Active/Configured，socket 6401，直接`read_console`已成功；
- 不支持的MCP探测命令产生的6条工具错误已清空，随后Console为0条error/warning；
- R8合同与current-worktree证据矩阵已建立，尚未把历史U9结果升级为fresh证据。

## Next

R8-WP01A已完成；gizmo异常由`R8-PLAY-001 / VERIFIED`关闭。当前转入`R8-INP-01`：用户报告真实
按钮组合无法释放技能，先按InputAction→FrameInputSet→roster→Runtime key/cd/combo/frame获取first
difference。source已确认provider held-only capture/direct callback discard/worker single-flight风险，但尚未裁决
根因；不得直接修改技能/DAT。任何脚本probe或修复仍须先建立独立Change Record。

Fresh neutral Play补证已排除battle未启动：transition完成后tick681/object8、两human roster绑定正常、
paused=false、worker active/no failure、neutral FrameInputSet含player0/1、CentralOnly pixel plan有效、Console
0 error。自动物理键注入在Codex sandbox桌面不可见；MCP execute_code无可用编译器。下一步需确认
`R8-INP-001A` test/diagnostic-only probe，不与production修复合并。

后续只读authority审计找到的`D-INP-010`已由`R3-COMBO-001 / VERIFIED`关闭：resolver九combo字段按C++
引用语义即时持久化，fresh compile/final self-check、input EditMode47/47与两组real-scene InputSystem probe
均PASS。DDJ为L/S/K→1/2/3→Naruto frame271、objects8→20；ordinary为L/D/J→DRA1/2/3→frame263。
D-INP-006用户实体键盘/窗口焦点edge仍独立，不被device-state probe冒充关闭。

下一独立认证包`R8-WP01C`的合同已经建立，并拆成01 opoint/newborn/basic lifecycle、02 pickup/held/
throw/landing、03 grab/CPoint/link、04 collision/hit/damage、05 death/respawn、06 random/late effect、
07 synthesis。首个可执行包是`R8-WP01C-01`；当前状态为`PLANNED / APPROVAL PENDING`，没有运行
Play Mode或修改脚本。之后才是`R8-WP01D` CentralOnly可见性/排序/挂点。按总计划R3+审批边界，
必须等待用户明确批准01；当前不得继续修改gameplay。

最新覆盖：`R8-WP01C-01 / R8-OPLIFE-001`已取得fresh compile、W05 8/8、09:06:51 self-check、
live production Play S4与final governance PASS，状态`VERIFIED`（仅01范围）。下一包为
`R8-WP01C-02 pickup/held/throw/weapon landing`，当前`APPROVAL PENDING`；不得自动启动。

2026-08-23最新覆盖：用户已批准`R8-WP01D / D-RENDER-006`并拒绝任何角色/技能/OID专项补丁。
C++通用source first-difference已经闭合：state8000应写`unk_318/RenderPicOffset=140`且raw pic999在
offset前隐藏；Unity错误写HitStop并先对999加offset，旧self-check还保护该错误合同。
`R8-WP01D-01 / R8-SPRITEMAP-001 / RUNTIME_PENDING`已建立，通用字段/隐藏顺序与陈旧oracle已最小写入，
没有角色/技能/OID分支；fresh compile 0 error、10:13:22 self-check PASS。后续必须
再执行all-loaded-DAT catalog/slice/UV/CentralOnly command矩阵，不能用单个技能成功宣称完成。

## Persistent blockers / exclusions

- R1-WP02 full C++ trace：BLOCKED；
- T8默认stage.dat：暂缓；
- Android真机：用户负责；
- C++ authority：只读，不运行/构建/修改/写入。

## 2026-08-23 WP01C / WP01D / WP01E update

- WP01C-01～06均为`VERIFIED（Unity S4限定范围）`，07 synthesis为`COMPLETE`；
- WP01D为`COMPLETE AT AVAILABLE EVIDENCE / FULL CLOSURE BLOCKED`，阻塞仅为loaded DAT无authored
  state8000样本与R1-WP02 full trace；不修改DAT，不阻塞E/F/G；
- WP01E当前Task为`R8-WP01E-current-build-capacity-performance-certification.md`，状态
  `PLANNED / CERTIFICATION-ONLY / NO SCRIPT CHANGE`；
- 先复跑fresh compile/focused/self-check和短样本validity gate，再运行Dispersed1000与Combat1000各
  120 warmup + 1800 sampled ticks；正式门同时要求1000 production GameObject、Avg/P95<=33.333ms、
  0 B/0 Gen0-2 collection/0 capacity fault、central draw/pixel有效、hash存在和teardown restored；
- 任一失败只保存first failure并另建修复Work Package，不在认证包内顺手改production。

### WP01E first failure / approval boundary

- E-01通过：compile0、focused 290/290、14:25:44 self-check PASS；
- E-02 Combat1000尚未进入采样：initial partial runtime为driver/world true、pool false；request processor把
  partial service footprint误当作已预期完整runtime，第一次clean restart、第二次retry-limit failure；
- report未生成，不能写成1000 AI帧率/GC失败；
- evidence：`RESEARCH/R8-WP01E-first-validity-failure-20260823.md`；
- repair：`R8-WP01E-R01 / R8-PERFBOOT-001 / PLANNED / APPROVAL PENDING`；
- 最小方案只把restart decision的expected信号改为Bootstrap ready事实并补pure-policy matrix；不预建scene
  service、不改pool/Bootstrap/gameplay。批准前停止脚本修改。

用户随后已明确批准`R8-WP01E-R01 / R8-PERFBOOT-001`并恢复目标；Record为`IN_PROGRESS`。上述approval
boundary已解除，但代码范围和stop conditions不变。

`R8-PERFBOOT-001`现已`VERIFIED`：compile0、focused263/263、14:35:20 self-check，同一Combat1000
请求1000 active/180 sampled/logic0B/capacity0/central/teardown PASS；未改pool/Bootstrap/gameplay。
WP01E Combat短样本validity通过，但visible frame Avg/P95=38.949/39.025ms，正式30FPS未关闭；下一步
Dispersed1000短样本。

Dispersed短样本也通过1000 active/180 sampled/logic0B/capacity/central/cleanup；logic Avg/P95
21.432/24.771ms。两组visible frame Avg/P95均约38～44ms，短样本不关闭30FPS。当前进入E-03：
Combat/Dispersed各120 warmup+1800 sampled，并启用completed-frame timing后再裁决瓶颈和修复包。

WP01E现已`VERIFIED / UNITY EDITOR CURRENT BUILD`：Dispersed/Combat formal visible P95分别25.525/
33.058ms，main P95 25.286/26.901ms，logic P95 18.575/19.044ms；两组1800 sampled、logic0B、0
collection、capacity0、central/cleanup PASS。Desktop容量focused299/299；Legacy/Data 12项hash全等；
14:51:50 self-check PASS。Editor frame allocation仅作observational记录，Player hard gate归WP01F。下一包
WP01F Windows Mono/IL2CPP，脚本/配置修复仍须独立Record。

WP01F合同与`R8-PLAYERBUILD-001 / PLANNED / APPROVAL PENDING`现已建立。现有tool仅U9 Mono；批准后只
提取共享Windows helper、新增Mono/IL2CPP独立Temp输出，并在finally恢复backend/frame timing/background/
Burst。随后用同一Combat1000 request运行两Player、比较12项hash。批准前不改脚本、不build/run。

用户随后已明确批准`R8-WP01F / R8-PLAYERBUILD-001`并恢复目标；Record为`IN_PROGRESS`。approval
boundary已解除，代码范围和stop conditions不变。

`R8-PLAYERBUILD-001`现为`CODE_WRITTEN`：R8 Mono/IL2CPP菜单、共享Windows helper、独立Temp输出、
BuildReport日志和旧U9兼容别名已写入唯一登记的Editor build tool。尚未fresh compile，也未build/run任何
Player；后续必须先编译和validator，再按Mono、IL2CPP顺序执行同一Combat1000合同。

用户最新明确排除IL2CPP后续处理：不得继续build/run/诊断/修复或据Codex沙箱Player结果修改Unity。
`R8-PLAYERBUILD-001`现`ABANDONED`，已写helper/Temp artifacts保持原样且不标双runtime VERIFIED；工作返回
C++ Release→Unity C#战斗逻辑差异主线。

当前进入`R8-WP01G / DOCUMENT-AND-EVIDENCE ONLY`。全量register现有68个D-ID；初筛未发现尚未关闭的
source-confirmed Unity code difference。剩余以runtime/trace/reachability证据缺口为主，必须先综合分类并
形成后续source调查/Play证据包，禁止直接改gameplay。
