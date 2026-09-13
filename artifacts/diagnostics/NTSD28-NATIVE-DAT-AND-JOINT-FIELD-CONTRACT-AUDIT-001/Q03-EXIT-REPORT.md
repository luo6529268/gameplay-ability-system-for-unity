# Q03 出口：字段与联合迁移合同

2026-09-13。状态 **DELIVERED_CONTRACT_ONLY / IMPLEMENTATION_PENDING**。本出口关闭Q03审计与合同职责，允许进入Q04；不表示B6/B7/B11已对齐，也不表示schema或资源已经迁移。

## 逐项出口核对

| Q03要求 | 已检查证据 | 结论与后继 |
|---|---|---|
| 六DAT九frame完整根因 | focused-comparison.json、Q03-PROGRESS-REPORT | hir第三处明确为CPoint drain；WPoint额外字段及数字key按native准入。Q05修数据入口，不能统一吞错 |
| CPoint27类型/default/presence/alias/复制 | decoder-contracts.json、VERSION-IDENTITY-AND-CAPTURE-CONTRACT、既有CPoint owner审计及本轮live reader | 24int+3float32；X/Y/Z以原版逻辑位置单位、throw速度以每逻辑tick位置增量，action/flags/resource值原样；未找到reader的recover/hurt只保留内容，不发明效果。Q05完整Value/adapter/canonical，Q06消费 |
| 数值边界与packed ITR | NUMERIC-DECODE-WITNESS-001 REPORT/native.tsv/comparison.json | 37原版输入双跑一致；333值对当前Unity有113差异，strict int/ITR首整数/finite float分开；37/37语法成功不冒充语义一致 |
| OPoint24完整生成去向 | OPOINT-AND-HELD-DEPTH-CONTRACT | 两条factory/initializer/PostInit与task copy/reset、随机/队伍/HP/复活/link、失败终止及0..998零frame规则已写明；Q05数据、Q06行为 |
| WPoint/held strength | 同上与Q03-PROGRESS-REPORT | WPoint9项保持原合同；strength需index+19项，candidate深度从holder当前WPoint选择武器definition行，消费替换不同阶段 |
| BDY最终几何/ITR depth | COLLISION-GEOMETRY-WITNESS-001 REPORT/comparison | 14用例×3正式collector模式，15同/27异；BDY深度/presence与ITR raw/effective值合同明确。Q06另验held/dense/cache/parallel及真实场景 |
| +2F8 owner/default/producer/reader | JOINT-FIELD-MATRIX、VERSION-IDENTITY-AND-CAPTURE-CONTRACT、native/Unity源身份 | 独立int默认/reset -1，held-release精确writer及两个object-AI reader；不复用Spawner或character AI OwnerSlot |
| mass/reserved全载体与alias | reserved-alias-reference-matrix.json、JOINT-FIELD-MATRIX、已有mass owner审计 | 五类实体reserved+任务holderCopySlot；初始化/复制/重置/ECS/hash/测试分类，不把271测试命中当生产reader |
| 剩余Oscillate reader与base shell | 本轮重新读Goal17 Task最终节和当前LF2LivingObject/LF2Entity/BaseShell | 更正此前base-shell保持1的暂定结论。producer已退休；Q04只退ProcessEffects剩余reader，Q05去掉2项载体并base1→2 |
| 联合版本和旧snapshot拒绝 | D-022及VERSION-IDENTITY-AND-CAPTURE-CONTRACT | 12→13、20→21、23→24、character1→2、base1→2；其他payload目前保持；旧midbattle拒绝、seed/input重放与同版本比较，不加adapter |
| 身份与队列/capture边界 | 同上、semantic-identity-test-vectors、实际World/Session/Ring/PendingEvent入口 | 解码语义版本摘要与raw身份分开；OPoint双owner队列非空拒绝capture/restore，不Flush不丢弃；复用Preparing捕获owner，不创建singleton |
| Trace comparator可用性与版本 | TraceContract/EntityFieldContract/RawEntityCaptureComparator、native与Unity exporter实际读取 | +2F8新字段使49→50；trace相关v2→v3，raw/source wrapper相关v1→v2，其他无变子schema保持；原6项MISSING不得无证据晋升 |
| 可执行后继、验证和回滚 | Q04两个独立Task、VERSION合同、0.11队列/R13/R15 | Q04先剩余行为，Q05一次窗口，Q06分consumer，Q07内容迁移。每次脚本前新建精确Record，实际验证后才能提升实现状态 |

## Q03确定的实施顺序

1. Q04-A `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001`：去掉尚存mass摩擦gate，保留字段直到Q05，验证真实grounded判断、速度/落地其他行为不变。
2. Q04-B `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001`：移除旧Oscillate晚帧读写，保留其他Effect行为；其旧producer不再重做。两个包各自有准确测试与证据。
3. Q05 `NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001`：按本出口完整清单建立事前Task/Record和code-path，统一发布内容形状、reserved/两类shell清理、+2F8、identity/capture guards及trace/schema；不能用临时版本部署再开第二次窗口。所有文件与测试scope在修改前声明。
4. Q06按OPoint、CPoint、held-depth/geometry、+2F8 AI、slot/资源尾部独立行为包接线，沿R回访；Q07在其出口后执行正式资源迁移。默认stage.dat暂缓、音频/图片例外、33ms、十一阶段、Unity/GAS和非战斗边界继续保持。

合同冻结不代表要求在Q04提前实现Q05数据字段或Q06全部行为；高风险快照、纯内容和生产消费的验证分别按所处阶段完成。几何和数值见证的差异是实施验收输入，不是把已通过的旧整域结论从头再做。

## 验证口径与保护

Q03所用编译/运行事实：两个新增native工具实际编译；几何真实Unity Editor capture42项；数值与DAT源链接.NET编译0warning/error及实际输入输出；输入/source身份和native双跑稳定。完整报告明确哪些是source-model、哪些是真实Editor，未伪称正式EXE Play或整体parity。

工作树中的production修改仍来自已声明Q02，Q03仅新增诊断工具/测试和文档。正式资源/schema未修改。numeric工具台账验证为472 records/45 governed code files；没有提交、push或删除用户文件。R13/R15仅完成合同/身份子条件PARTIAL_RETURN，后续实际迁移和运行时证据仍待。

总目标继续ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02尚未完成。下一唯一实施入口是Q04-A Task；实施前先创建其Change Record，不再回头重复Q01/Q02或Q03已交付的诊断。
