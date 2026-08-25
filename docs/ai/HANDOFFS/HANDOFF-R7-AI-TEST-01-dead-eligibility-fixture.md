# HANDOFF — R7-AI-TEST-01 dead eligibility fixture

> 日期：2026-08-22  
> 状态：`VERIFIED / TEST-ONLY`  
> Change ID：`R7-AI-TEST-001`

## Current

R7 AI source复核期间，UnityMCP job `6fdd44f773344cffbce04404bfddfd86` 揭示一条旧 Editor
断言仍把 active `HP=0` AI 当成 input-ineligible；这与 C++ `prepare_ai_input` 和
`R3-AI-LIFE-001` 冲突。已在任何测试脚本写入前建立独立 Task / Change Record，并已将dead/coordinate
fixture拆成两个职责独立的测试；production AI未改。

fresh Editor DLL为21:01:39，Console error 0；exact job
`8c74d8e0a76e427fac3fd7920f5ac234` 2/2 PASS，AI sensing/profile job
`5c6bad85dc0b43c2a6949d03cfd256fc` 111/111 PASS，21:04:52 full self-check PASS，
validator/scoped diff PASS。

## Allowed next

本Change ID无需继续修改。恢复R7 AI source认证时必须把sensing/index与完整decision tree拆包；不得将本测试
VERIFIED解释为AI gameplay或C++ runtime VERIFIED。

## Stop

不得借此修改 production AI、RNG、target/special scan、profile、pass order或将 R3-AI-LIFE-001
升级为 C++ runtime VERIFIED。
