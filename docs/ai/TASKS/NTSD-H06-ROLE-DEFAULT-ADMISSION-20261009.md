> 2026-10-09 Role限定准入完成；当前生产测试后Editor通信查询超时，第二项仅只读准备等待安全状态恢复。第一项报告/validator已落盘，未恢复旧Goal。

# H06 Role 限定准入与默认接线
状态 IN_PROGRESS；用户2026-10-09两项具名授权，第一项。D1=A有效。

阶段1 Change NTSD-H06-ROLE-ADMISSION-WINDOWS-001：只写Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs（新增BeginScopedRoleAdmission、BuildScopedRoleAdmissionRequest及BeginSuite/BuildCurrentRequest/IsOwnedTerminalOrAbsent的冷入口接线、SuiteState scopedRoleAdmissionWindow字段）和新增H06RoleDefaultAdmissionEditorTests.cs/meta。不改原request标准/GC observer/owner/关闭。新状态为Editor Suite owner的一枚冷模式bool，capacity=1，finally清starting标记；持久状态沿用SessionKey由原终态清理；无战斗runtime缓存，无十一阶段新增owner，原owner phase1阻止请求、phase2join、phase4unseal、phase5publication、phase7/8回收、phase9world、phase10pool、phase11map全部复用。

最小验证：新请求与旧81除output完全等价、同布局配对只差collector/output；现有PostBindingsRole请求/冷入口/observer聚焦。随后原Editor同版四窗：D Brute/Role、C Brute/Role，各120warm+180sample，不开1800。原GameConfig AI明确DataOrientedCanonical、collision空；固定Sakura1000、seed、AI/输入/渲染和其它flags，只切collector。完整Driver可靠GC前后校准、actual1000AI、完整末tick快照、实际mode/fallback/abort、容量/关闭/Scene和设置恢复均验；父子计时不加，均值不推P95。

复用：FindEffect100/千人0GC和原四形状其余0GC；四场景32tick完整域一致及关闭；精确指纹核验后不重跑。25/82各守scope；95OFF且GCFAIL未豁免。新测量失败或无收益保持Brute，定位且不重复同版长窗刷PASS；扩大修复/规则/合同需用户决定。

阶段2：必要门通过才独立生产Change修改CollisionBroadphaseBackendResolver.Resolve的空配置默认；显式合法override/config优先、非法安全回退Brute和forceBrute保持，SimulationWorld测试构造器默认不动。届时补准确Task路径/符号和操作备份，验证实际production source→World→query及关闭重进；当前尚未授权该阶段直接执行（条件未满足）。

回滚只本Task增量和真实dirty备份，先记录，不用HEAD覆盖用户工作。风险：旧Suite有固定输出/共享terminal，必须新目录与owner识别；窗口诊断不是H07/Android证书。最终独立IMPLEMENTATION-RECORDS/H06-ROLE-DEFAULT-ADMISSION.md、唯一进度/依赖/Change更新，validator。旧Goal/34项不恢复。

## 生产阶段启动（必要前门已通过）
四窗完整Driver稳态0GC、同末态、实际1000AI、容量/关闭及收益通过；准确数值measurements-summary.json，settings after job8958ed43108f4a2eb80ffd2f0ef7eca7 1PASS。新Change NTSD-H06-ROLE-PRODUCTION-DEFAULT-001。准确写域BattleRuntimeProfile.cs::CollisionBroadphaseBackendResolver.Resolve，空/null两配置默认LooseQuadtree；已识别合法值优先逻辑不变，未解析非空（含空白）继续Brute。GameConfig.cs仅collision tooltip同步（不改AI tooltip/Asset）。新H06RoleDefaultAdmissionEditorTests增加resolver矩阵及实际ProductionSource/Driver.CreateProductionWorld/RecreateWorld→World→query、显式Brute与两次有序关闭重建验证。SimulationWorld默认构造仍Brute，旧Explicit历史准备门不改。无新增runtime状态/cache/owner/capacity，关闭流程原样；失败则记录并停止推广，不扩大规则。

受影响当前回归补充：BattleBruteProductionAdmissionEditorTests.cs::H06RoleAdmission_EditorReleasedAndDefaultStillBrute为非Explicit当前默认断言，必须同步命名/预期为Role（只该方法），避免留下已过时失败测试；历史报告/产物不改。准确备份promotion-legacy-test.json已完成。
