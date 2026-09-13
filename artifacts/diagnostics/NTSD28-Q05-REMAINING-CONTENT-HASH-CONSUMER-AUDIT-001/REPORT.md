# Q05 内容hash消费者核对出口

VERIFIED_AUDIT_ONLY / TRACE_IDENTITY_GAPS_CONFIRMED。本轮只读生产代码，原版资源不改。hash-callsite-inventory.txt与source-inventory.json保留全量检索及当前文件SHA；检索不是行为验收，结论来自下面实际reader/caller检查。Unity/GAS/非战斗、Scene和资源保持；禁止computer-use。

| 边界 | 当前实际来源/消费 | 判断与动作 |
|---|---|---|
| 源DAT完整身份 | LoganObjectCatalog.Entry.DatSha256；catalog完整SHA、registry顺序和每条DAT路径/SHA → DefinitionFingerprint；LoganContentIdentity V2 tag/NUL/raw32 → SHA/LE ulong | 覆盖正式catalog可达DAT的完整原文字节，包括FrameSounds顺序、numeric表示、27/24/40/19/9和metadata；不等于全部405非indexed文件均为playable内容。保持现算法。 |
| 解码profile与cache/pub | UsesLoganFrameNumbers由native parser/converter路径设置；V2 tag定义这套精度/字段语义，ContentIdentity进入catalog/candidate SourceCacheKey与两个publisher；CreateLocalValidationSessionIdentity投影进既有catalog槽 | 不用进程GetHashCode，不再造第二个全DAT每tick摘要。新语义仍须遵守V2合同；同raw更改decoder必须变tag。当前只证明已接本地验证，不宣称外部Server自动使用。 |
| 视觉输入身份 | LoganVisualContentCandidate在定义输入上增加raw图片hash，BMPLoader消费前验证实际bytes | 不把图片hash塞入实体状态checksum；正式资源迁移和整场视觉仍Q07/Q09。 |
| frame/meta值 | LF2FrameData六double/profile/三int/有序FrameSounds；LoganDefinitionMetadata只读BMP/stats/armors/piece结构；既有typed-frame及header/armor-piece capture逐字段验证 | Animation/DatParser/Simulation没有发现另一套逐frame/metadata完整值hash生产入口需要补这批字段。源+解码身份负责定义一致性。不是宣称所有public frame/list在任意代码中都不可变；若允许runtime改definition需另建合同。 |
| 有效ITR命中保护 | BattleEcsHitExecutionPlan.Fingerprint(InteractionArea)/Fingerprint(in ItrProjection)由命中预检/commit/replay调用 | 两重载均含z、hasGeometry、drain/sound/cover及其余有效ITR字段/float；不是完整frame身份。不需要把FrameSounds、BMP或CPoint定义加入命中ITR投影。 |
| 护甲值摘要 | LF2ArmorData.ComputeFingerprint64完整当前标量/列表/sound；全仓caller仅NTSD28B5Type1ArmorDataContractEditorTests | 现有测试用值指纹，不是catalog发布缺口，不擅自接入每tick。 |
| ECS runtime保护 | BattleEcsWorld/BattleRuntimeFingerprint计算runtime字段；独立ObjectAiExcludedGroupSourceSlot2F8已在hash；旧退休字段已去 | 跟踪可变runtime，与definition不同。保持字段集合；联合版本后继。 |
| 每tick checksum/快照 | BattleLockstepChecksumModule.AppendSlots读取FrameCache.Wrapper.characterId/ObjectId和当前runtime；BattleParitySnapshot同类状态投影；snapshot各域identityFingerprint复用session | 不嵌完整DAT；同content identity+同schema是比较前提。现12/20/23/1/1是未发布中间态，必须升13/21/24/2/2并验旧拒绝。 |
| BDY模板cache | BruteForceSceneQuery按LF2FrameData引用键、当前帧body template，既有清理；RuntimeHelpers.GetHashCode只用于引用相等字典 | 不是跨进程内容身份；BDY几何规则差异留Q06，不能在本轮hash审计改碰撞算法。 |
| 旧投影诊断 | Tools/NTSD28ContentAudit记录AST frame fields+subblocks；完整typed额外字段由NTSD28Q05TypedFrameEditorTests/metadata capture覆盖；Tools/NTSDParity.JsonProjection是历史反射工具 | 旧138审计仅迁移前回归，不得把旧AST/37int投影当完整native typed身份。无需为了旧诊断修改正式规则。 |
| 当前trace工具（明确缺口） | TraceContract.ContentPolicy仍strategy-pending；TraceComparator只要求content.policy+manifestSha256，内容不同时结束为content-strategy-pending | 与D-023冲突；须在v3新合同同步加入source/raw/decode/semantic/schema证据并严格拒绝不兼容身份，不改历史文件来伪造一致。 |
| raw capture（明确缺口） | NTSD28UnityEntityRawCapture v1/49/43，EntityFieldContract49；wrapper/comparator旧版本，缺+2F8 | 同窗口raw/source wrapper v2、50字段，新字段读独立2F8；原6MISSING保持，producer/AI仍Q06，raw不是正式EXE证书。 |

新增frame六double/profile/centerz/chp/cmp/有序sounds及metadata无需另一份生产hash实现；剩余缺口集中在联合版本与trace头部/consumer接线。该结论限定于当前检索源码和既有调用路径，不证明整场结果一致，不晋升任何旧capture。

下一动作进入父步骤4的同一联合窗口：先完成5个snapshot/checksum版本常量及其直接测试消费者、新旧版本拒绝；再完成冻结trace/raw/source wrapper+2F8及完整身份比较。两部分均未通过前保持INTERMEDIATE_UNPUBLISHED，不发布baseline/Q07；步骤5全量capture-restore-replay/Play仍待。不得修改外部Server/协议布局或把此审计当运行时PASS。
