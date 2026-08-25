# R8-WP01G-R09 — final evidence reconciliation

> 日期：2026-08-24  
> 总登记：`68 D-ID`  
> 状态：`COMPLETE AT APPROVED UNITY EVIDENCE / C++ FULL TRACE BLOCKED`  
> 脚本、scene、config、resource、C++改动：`0`

## 1. Verdict

以当前工作树、68项总登记册、R05～R08-R04的Task/Change/evidence/result和用户范围决定重新逐项核对后：

1. 当前没有新增的“正常战斗主线、production可达、C++ release source已确认、Unity尚未实现”的脚本差异；
2. `D-LIFE-001`已由R08正式OID7/8→51→7/8完整tick Play闭合到Unity S4；
3. `D-RENDER-003`的pending/generation/T+1由R07B闭合，dormant/split由R08闭合，整体达到Unity S4；
4. `D-SCHED-008`、`D-SCHED-010`、`D-STEP-001`只属于用户明确不需要的F1/F2调试步进路径；source差异历史保留，
   但不再作为正常战斗对齐backlog；
5. `R8-SPRITERANGE-001`已满足原Task全部验证层：source、focused2/2、atlas29/29、fresh compile0、normal Play
   Console0、R08 PASS、后续完整self-check PASS和Ledger validator PASS，可从`FOCUSED_TEST_PASS`升为`VERIFIED`；
6. 五个exact分支仍因current DAT/fixture不可达而只能保持代码/自检/有限S4证据；不得修改DAT或写角色特例制造PASS；
7. `R1-WP02` C++ executable full trace继续`BLOCKED`。因此不能把本结论写成“C++ runtime完整动态对齐”。

## 2. Exhaustive 68-ID classification

### A. Unity S4或明确runtime证据已覆盖的正常战斗子范围（43）

`D-SCHED-001`、`D-SCHED-002`、`D-SCHED-003`、`D-SCHED-004`、`D-SCHED-005`、`D-SCHED-007`、
`D-SCHED-009`、`D-INP-002`、`D-INP-003`、`D-INP-004`、`D-INP-006`、`D-INP-010`、
`D-MOV-001`、`D-MOV-002`、`D-MOV-003`、`D-MOV-004`、`D-COL-001`、`D-COL-002`、`D-COL-003`、
`D-COL-004B`、`D-HIT-001`、`D-HIT-002`、`D-HIT-003`、`D-HIT-004`、`D-LINK-001`、`D-LINK-002`、
`D-HOLD-001`、`D-HOLD-002`、`D-HOLD-003`、`D-CPT-001`、`D-CPT-002`、`D-CPT-003`、`D-CPT-004`、
`D-CPT-005`、`D-OP-001`、`D-LIFE-001`、`D-RENDER-001`、`D-RENDER-002`、`D-RENDER-003`、
`D-RENDER-004`、`D-RENDER-005`、`D-PERF-001`、`D-LATE-001`。

这些状态只覆盖各自实际执行到的producer→consumer、分支和场景。未进入live matrix的kind/type分支不自动升级；
S5仍受R1-WP02阻塞。

### B. 实现已闭合，但current DAT/fixture无法提供exact production witness（5）

`D-MOV-005`、`D-COL-004`、`D-COL-005`（特指05B）、`D-HIT-005`、`D-RENDER-006`。

- type0 state2000、valid-geometry oid999、non-character kind1、CLR/current-DAT mismatch、authored state8000在当前
  production数据中不可得；
- 它们已有source、focused/self-check或相关S4证据，但不能伪造exact Play证书；
- 当前没有证据要求继续修改production gameplay。

### C. Source合同静态等价，缺C++ runtime trace（1）

`D-SCHED-006`：两次Z clamp的对象筛选、canonical字段、时点与approved extended-capacity adapter已闭合；
没有C++ full trace，保持source-closed/runtime-pending边界。

### D. 用户明确排除或未来替换，不属于当前正常战斗对齐backlog（9）

`D-SCHED-008`、`D-SCHED-010`、`D-STEP-001`、`D-INP-001`、`D-INP-005`、`D-INP-007A`、
`D-INP-007B`、`D-INP-008`、`D-INP-009`。

- 前三项只属于F1/F2调试步进/解锁路径；用户明确不需要；
- 其中自然type0 negative-link路径来自AI child，随AI范围决定不伪造human Play；
- 五项AI parity由用户决定未来改为Unity状态树/行为树；必须继续通过30Hz `FrameInputSet`接入，不得直写runtime。

### E. 非正常战斗调试功能键，策略未要求实现（1）

`D-SCHED-011`：normal tail已闭合；剩余F7/F8/F9分别是满HP/PP、随机武器和清weapon picker的release功能键。
用户未要求实现，故保持policy boundary；它不是普通战斗tick差异修复。

### F. 已批准Unity adapter或未来配置决策（3）

`D-SCHED-012`、`D-CAP-001`、`D-PERF-002`。

保持Authority400仅作对照、MobileExtended 1000、DesktopExtended无固定产品active cap、sealed 0B容量合同以及
当前BruteForce production配置。未来切换Loose Quadtree必须另立配置/性能/parity包。

### G. 测试、worker与性能基础设施事实已验证（6）

`D-TEST-001`、`D-TEST-002`、`D-TEST-003`、`D-PERF-003`、`D-PERF-004`、`D-PERF-005`。

这些证据证明测试隔离、worker publication、single-flight、初始化和Editor current-build 1000实体/30FPS/0GC合同；
它们不能单独裁决C++ gameplay。

## 3. Count reconciliation

| 分类 | 数量 |
|---|---:|
| A Unity S4/runtime覆盖 | 43 |
| B exact production witness不可得 | 5 |
| C source等价、trace缺失 | 1 |
| D 用户排除/未来替换 | 9 |
| E 调试功能键policy边界 | 1 |
| F approved adapter/config decision | 3 |
| G test/worker/performance事实 | 6 |
| **合计** | **68** |

集合校验要求：register=68、reconciliation=68、missing=0、extra=0、duplicate=0。

## 4. R8 package reconciliation

| WP | 最终状态 | 边界 |
|---|---|---|
| WP01A | VERIFIED | compile/self-check/EditMode基线 |
| WP01B | COMPLETE AT AUTOMATED EVIDENCE | InputSystem/组合键S4；人手键盘/窗口焦点由用户验收 |
| WP01C | COMPLETE / UNITY S4 | 01～06通过，07 synthesis完成；不扩大未触达分支 |
| WP01D | COMPLETE AT AVAILABLE EVIDENCE | 无authored state8000；C++ full trace blocked |
| WP01E | VERIFIED EDITOR CURRENT BUILD | 1000实体、30FPS门、0B/0GC；Android不在范围 |
| WP01F | ABANDONED BY USER | IL2CPP后续处理明确排除 |
| WP01G | COMPLETE AT APPROVED EVIDENCE | R05～R08-R04闭合；AI/F1F2按用户范围退出 |

## 5. Persistent boundaries

- C++ authority只读，未运行、构建、修改、复制或写入；
- `R1-WP02` full trace继续BLOCKED，B-R1-WP02-01～04保持；
- T8默认`stage.dat`继续暂缓；
- AI未来设计、F1/F2、F7～F9、IL2CPP、Android和服务器不因R8收口自动进入当前主线；
- CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×visual scale、fixed-world camera、扩展容量、30Hz/
  FrameInputSet、SoA/ECS、pool/worker/0-GC均未回退；
- 本报告可以支持“R8在批准范围及当前可取得Unity证据层完成”，不能支持“整个C++ executable runtime已完整对齐”。

## 6. New gameplay difference decision

本轮没有发现新的可实施production gameplay first difference，因此不建立新的脚本Change Record或修复Task。
若未来用户提供新的具体正常战斗复现、authored缺失分支资源或安全C++ trace方案，必须从新的first difference开始
建立独立Task/Change，不能回写或模糊本轮证据。
