> VERIFIED / DECLARED_OPTIONAL_INPUT_STATE_AND_DESCRIPTOR。SelfCheck/Play1568+864/回放112场景/关闭均通过；仅关闭同Record声明范围，不关闭Q06。下一NATIVE-INPUT-ACTION-COST-FRAME-READERS-001。

# Native input 缺失 state 与声明 state 0 的区分

READY_SOURCE_WITNESS_AND_EXACT_RECORD。归属BATCH-03/Q06；阻塞CPOINT-THROW-NATIVE-RAW-BINDING-001的完整后继tick出口。尚未修改本Task生产代码，必须先独立Change Record。

已观察：CPoint source392立即before/after两profile全部通过；正确native profile/关闭fixture误启用AI/恢复legacy及NativeInputProxy后，完整下一tick两profile各剩196个字段差异，全部为next99隐式空帧+select=true的28行：source保持99，Unity转65。所有其它364行的47bound raw/lifetime/extra一致。证据CPOINT-THROW-NATIVE-RAW-BINDING-001/following-real-input-red，jobe756a0880dca465088333c6e9cb19000（2immediate PASS/2following FAIL）。此前656差异来自夹具前置差异，不能用于本Task生产裁决。

当前权威链：playable SimulationTickDriver28 -> InputRouter28 -> input_routing.cpp route_native_ground_builtins；约907和987两次 frame->values.integer("state").value_or(-1)。与raw capture/其它pass取state默认0不同，不得全局改变frame.state默认值。

Unity对应：BattleCharacterActionWriter.RouteNativeGroundBuiltins / RouteNativeAirDashRedirectBuiltins直接读frame.state（约584/689/715）。LF2FrameCache.NativeZeroFrames使用默认state0、UsesLoganFrameNumbers=true。正式ConvertLoganFrameData保存rawProperties但缺失state仍typed0。因此输入state语义可能需按正式field presence读取。只凭frameName字符串辨识隐式帧不可接受；也不能只修99硬编码。

下一步骤：
1. 准确追踪完整ground/airdash/rowing分支及state重读点，核对run accumulator在state未声明时仍递减的源顺序，避免一个早退漏掉非state副作用。
2. 补源向量，包含未声明frame、已声明但无state、声明state0、其它state、高帧、非法destination；明确非零run accumulator、输入/随机及资源副作用。可扩当前runner或独立runner，先声明准确Tools路径；不可把source/C++现象推广为未经验证的全部输入行为。
3. 建独立Change与测试；保留旧Unity138/手写fixture兼容入口，正式Logan field absence按源处理。解析无效token的规则需读取实际decoder，不凭ContainsKey推断全部合法性。
4. 原CPoint392完整driver回访到0差异后，返回父Task完成SelfCheck、两factory真实Play、回放、有序关闭/场景保护；不得重做已通过的source392或立即矩阵，除非源向量或生产变更确有需要。

禁止修改全局frame state默认、GAS/Mono框架、非战斗、Scene/资源/Server。保留33ms与pass顺序、十一阶段关闭、raw3缺口、Q07未部署、用户HUDBg30、stage USER_HOLD与例外。总目标继续ACTIVE。

IN_PROGRESS / FOCUSED_PASS_RUNTIME_PENDING：已建精确Record/source432，两个生产路径修复并通过432两profile；父392 fulltick原28差异全部清除。旧Ground与AirDash回归已过，需fresh SelfCheck和父Play/replay/关闭。方向/Jump/Defend特殊交叉与double-tap未由432证明，不扩大为全部输入路由。
