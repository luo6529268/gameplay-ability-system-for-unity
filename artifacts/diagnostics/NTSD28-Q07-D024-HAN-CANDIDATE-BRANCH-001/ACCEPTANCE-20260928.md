# Q07 / D-024 Han→Lee 候选分支限定验收

状态：`RUNTIME_PENDING / POST_TICK_PREDICATE_CAPTURED`。本包确认原 Battle Scene 当前候选收集器与帧后谓词复算；**没有**直接记录收集器内部首次 return 分支，也没有选择或修改碰撞判定域。

正式对照：当前根正式 EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。同一 Han OID726 action146 / Lee OID7，配对 playable 的近距 X520 候选1并抓取；根正式 EXE 新远距 X580 回放 50 tick 所比 400/400 字段与配对源相同，action146 候选0，Han X535。两距离此前 Han 运动所比 140/140 字段一致。正式近距 X547 是抓取后位置；近距候选前 X535 是由相同前段运动与正式 pass 顺序支持的推断，不是根 EXE 直接读取值。详情见同父包 `NATIVE-DISTANCE-CONTROL-20260928.md`。

Unity 原项目两次独立请求保留原始结果：`q07-han-branch-20260928-a.json` 得到 `OBSERVED_CACHE_LIMIT`，说明当前实际收集器为 `ForceBruteForce`，RoleAware 缓存不适用；`q07-han-branch-20260928-b.json` 得到 scoped capture。后者在绝对 tick14、Han collision action146 记录源规则 X535、实际物理 XInt553、Lee X520、selectedCandidates0。两次均有序关闭、borrowers0，Battle Scene SHA 前后为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`。

`-b` 的反射读取发生在完整 tick **之后**，World checksum 前后同为 `ce621c14d283c18a9eb25c8014f3d7ad10c0b5537eeffb518e21e74e82b52ada`。同一已保留碰撞帧上重新调用生产谓词得到 pair allowed=true、has ITR=true、target has body=true、coarse pass=true、kind3 ITR allowed=true、kind3 `HitsTarget=false`。这支持物理坐标下 kind3 精确矩形不相交的机制，但仍是帧后复算。旧原始 JSON 的 `bruteFirstRejectedStage=AttackerCarrier` **不可作为首次拒绝证据**：`RunInteractionPhase` 已调用 `EndCollisionCandidateConsumption()`，它清空 `_candidateCache`，使后读 `IsCandidateAttackerCarrierForCurrentTick` 可能变成 false。原 JSON 不覆盖；探针源码现将此字段更名为 `brutePostTickFirstFalsePredicate` 并标注 `AfterCandidateConsumption_PostTickReevaluation` 与限制。未再跑 Play 仅为字段更名生成新 JSON。

验证：生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 返回0，189警告、0错误；原 Editor PID11944 原位刷新、`Assembly-CSharp-Editor.dll` 时间晚于探针源码，回到 `NTSD_Battle` 非 Play / idle。Battle/Menu/InputSettings/ProjectBattleModeConfig 四个保护 SHA 与前值一致。本包仅改 Editor 可选诊断和记录，未改战斗生产、DAT 数据、资源、Scene 或非战斗逻辑。完整 Q07 与 D-024 碰撞域策略仍开放；后续若需确认首次 return，应在候选收集当时加入独立、受控的只读见证，并先立新的精确 Task/Change。

## 同轮续证：活动缓存仍存在时的回调

脚本修改前已在本包 Task/Change 追加精确范围。只复用已有 `BeforeCollisionCandidateStoreFinalCompareForSelfCheck` 回调，原 Editor 原位编译后唯一新请求 `q07-han-branch-20260928-c.json` 于真实 Battle Scene 完成，原始文件未覆盖。回调位于 `CollectCollisionCandidates` 的 brute 收集之后、候选消费和 `EndCollisionCandidateConsumption` 之前；仅独立 Q07 请求安装，`finally` 恢复原回调。请求消费为 false，Editor 末态同 Scene idle/non-Play，有序关闭 true、借用0、四保护 SHA 同上。生成 Editor 构建再次0错误/189警告，原 Editor DLL 时间晚于新源码。

完成 tick14 的 `inCollectionCandidateBranch`：`ForceBruteForce`、Han collision action146、源 X535 / 物理 XInt553、Lee X520；活动 `_candidateCache` 有 Han（整个 cache 1 项、Han 候选列表0项），`IsCandidateAttackerCarrierForCurrentTick=true`、pair allowed/有ITR/目标有BDY/粗筛/kind3许可均 true，同碰撞帧的 kind3 `HitsTarget=false`。完整 tick 后 `candidateBranch` 的 carrier=false、kind3=false、World checksum 读前后相同。两相位相邻证据直接排除了“收集时因 Han 不是载体而拒绝”这个旧解释，确认帧后 false 来自消费后的状态变化。

`HitsTarget` 是只读复算而非该 brute 分支的实际调用；实际 `RecordOverlappingBodyCandidates` 使用相同 `CollisionZInt`、`ItrWorldRect`、`BodyWorldRect` 和 `Overlap` 原语，并在不重叠时跳过候选。故当前可证明同阶段精确几何不相交且候选列表为空，**不能把复算名称说成实际执行过的 first-return 行号**。顶层原始状态 `OBSERVED_CACHE_LIMIT` 是旧 post-tick `firstLimit` 汇总结果；它不否定新嵌套回调字段已捕获。该限定诊断已通过，Q07/D-024生产策略、自然技能抓取、Q09自然地震画面仍开放；下一动作不是重测旧四类代表例，而是在既有碰撞域选择明确后立通用生产修复与配对 RED/GREEN。
