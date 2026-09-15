> VERIFIED / DECLARED_NATIVE_INPUT_COST_AND_FOLLOWING_TICK。最终证据及历史纠正见同ID Record；下文初始状态保留历史，不覆盖本出口。

> 当前IN_PROGRESS：源151/两profile费用端点0差异，完整tick路径隔离见同ID Change Record；下方READY为初始历史。

# Native input 动作准入及成本帧读取

READY_LIVE_SOURCE_MAPPING。归属BATCH-03/Q06 / NATIVE-FRAME-RUNTIME-READER-MIGRATION。前置CPOINT-THROW-NATIVE-RAW-BINDING和NATIVE-INPUT-MISSING-STATE-ROUTING已各按声明范围验证；不要重做它们的source/立即/fulltick/回放/Play。

当前只读定位：BattleCharacterActionWriter.ApplyNativeInputActionCore约195/203仍以旧HasFrame/GetFrameDataById决定目标准入与源帧；RouteNativeRowingRedirect约836/854仍旧成本帧查询和动作存在门；NativeBuiltinMpCost约1585仍旧getter。WriteNativeInputActionUnchecked已迁移Native getter，不重复改它。

源入口为当前playable input_routing.cpp：apply_action约317、assign_native_clamped_resource_action679、assign_native_gated_resource_action702、route_native_rowing_redirect726，以及InputRouter28实际调用者。追踪requested signed action→999归一化→初始成本帧→编码state重定向→HP/PP/有效上限费用→fallback→最终raw写入的完整顺序，不以局部getter更换替代原子事务。

下一步骤：
1. 列出上述Unity符号全部实际caller及对应source branches；确认当前cached frame与成本源frame的身份和读取时点。
2. 独立Task/Change预先登记准确脚本后，构建源见证，覆盖隐式/声明低高帧、声明/缺失999、999与负号特例、无效值、资源门槛及编码重定向/fallback。复用现有费用/字段合同，不凭旧测试重写source。
3. Unity test-first，分别测原生声明内容与旧typed兼容入口，关注原始费用仍来自何帧、fallback未归一化值及伤害/统计副作用。
4. 按实际差异实施最小battle改动，完成source/compile/focused/SelfCheck/必要真实Play及回放验收，再返回reader总表其余held/生成/display项。Q07仍需完整前置出口，不能因这个Task建立而提前部署资源。

已知验证边界：前一optional-state432只覆盖attack、非零run accumulator及有限action/state组合，没有证明方向/Jump/Defend所有交叉前置或double-tap递归；需要时在正确owner补向量，不能标为已全对齐。

禁止全局替换getter/修改frame.state默认、非战斗/GAS/Mono/Scene/资源/Server。保留33ms/3ms、十一阶段关闭、HUDBg30、raw3缺口、跨Worldepoch审计、stage USER_HOLD和已批准例外。总目标ACTIVE。
