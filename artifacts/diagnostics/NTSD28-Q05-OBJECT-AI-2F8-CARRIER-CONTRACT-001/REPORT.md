# Q05 独立+2F8载体与raw恢复联验

状态 FOCUSED_TEST_PASS / VERIFIED_CARRIER_AND_RAW_RESTORE_ONLY / INTERMEDIATE_UNPUBLISHED_Q05_WINDOW。Q05仍IN_PROGRESS，总目标FULL_ALIGNMENT_INCOMPLETE；不是新正式schema/baseline或AI行为完成。

## 实际变化

NTSDEntityRuntime新增ObjectAiExcludedGroupSourceSlot2F8，int32/default -1，与Spawner/Owner/RelationOwner独立。默认、Reset、canonical copy已覆盖；ECS独立数组默认/clear -1、capture/identity验证/runtime fingerprint纳入；lockstep checksum及parity追加同名明确字段。claimed实体与materialized raw分别捕获，不用stable id或固定group替代原始slot。

原版字段object_ai_excluded_group_source_slot_2f8及held release/common AI读取入口和正式build参与性已核对，5项源/正式EXE身份无漂移。当前只实现载体：Q06才将type1/4/6且parent WPoint dvx非零的held writer及common AI实时slot组读取接上，现有Spawner/Owner业务不变。

## 发现并闭合的必要恢复缺口

新测试在Authority400和MobileExtended都证明raw字段37被成功捕获，但修改成370后，完整restore仍留下370。恢复循环曾在复制任何payload前跳过未占用slot；独立NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001随后用旧X/HP/Spawner/InputHistory证明同一问题，避免只为新字段打补丁。

修复现在独立处理所有已捕获raw payload和claimed entity。已存在目标raw而源缺席时恢复默认，源存在而目的页不存在时仅materialize必要raw页；warm路径不分配。恢复前检查所有present snapshot payload的canonical数组存储及目的现有raw，错误在修改世界前拒绝，不Flush、不创建singleton。完整copy复用既有方法，无新网络/恢复策略或生命周期阶段。

## 证据

- 主包RED11/11 FAIL，字段缺失；第一次相关34测试中3FAIL：2为raw恢复，1为Authority400诊断输出的错误测试预期。Authority400 full parity保持原有实体/default投影，extended才输出原始raw值；已纠正测试分层，两profile仍都验证raw改变checksum，原失败保存。
- raw子包RED4/4 FAIL：旧raw.X777未恢复12.5，损坏snapshot raw仍返回true；修复后两profile位置/HP/Spawner/数组及完整checksum恢复一致，损坏raw snapshot拒绝且世界不变。
- 最终job62766860024a4edebce34a419618a0a6：46/46 PASS（主包11/raw4/既有snapshot-restore-checksum-ECS31），包含int32极值、不与owner/spawner别名、snapshot副本隔离、ECS差异/fingerprint/clear、warm capture/restore no allocation。
- 完整SelfCheck新请求2026-09-13T08:56:17.3698284Z，08:57:07Z结果PASS且晚于请求。最终Unity error CS0，实际编辑器编译/重载和测试成功。没有新增真实Play或native held/AI对照轨迹。
- 保护3059：3001相同/40既有或声明变化/18既有Foot缺失；相对前包新增6个准确production路径（主包4+raw修复2），无新缺失。两个新测试另在记录内。Scene SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持，isDirty=false/root14。任务外Foot删除及blue/red/yellow目录不恢复/清理。
- Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path通过，490 Records / 110 governed code files；最终版本结果回填ledger-final。

## 后继与限制

下一唯一Task NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001（先准确Record），再清五reserved。已完成+2F8载体和raw修复不重做。新字段checksum集合已变，但版本仍entity12/aggregate20/checksum23/character1/base1，仅同Q05未发布中间状态；父步骤3 raw/semantic identity/双OPoint空队列guard、步骤4统一13/21/24/2/2和trace映射、步骤5旧版本拒绝/新replay/Play仍必须完成。禁止跨版本交换、提前发布或部署Q07。

R13/R15保留待对应carrier/schema证据；+2F8 writer/AI须Q06有真实输入/生命周期证据再关闭，不能把默认-1当对齐。Unity/GAS、非战斗、33ms/3ms、十一阶段、stage.dat USER_HOLD及用户例外保持。用户禁止computer-use，本轮检测仅桥接/日志/结果/进程。
