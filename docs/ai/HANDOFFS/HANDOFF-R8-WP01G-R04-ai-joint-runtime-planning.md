# HANDOFF — R8-WP01G-R04 AI joint runtime planning

> 日期：2026-08-23  
> 状态：`ABANDONED BY USER / NO EXECUTION`

## Current fact

- R03 current F1/F2/F3报告可重复PASS，final compile/focused/self-check/validator已收口；
- `D-INP-006`已有Unity InputSystem S4，人手硬件/窗口焦点由用户验收；
- `D-INP-005`与`D-INP-007A/B/008/009`已有source mapping、自动矩阵和Unity内部A/B证据，
  但真实AI角色sensing→decision→action→hit仍`RUNTIME_PENDING`；
- C++ full trace仍BLOCKED；C++ authority保持只读。

## Proposed package

`R8-WP01G-R04 — AI sensing → decision → action → hit joint runtime certification`

只认证真实AI联合链，不预设production修改。若缺probe，必须在R04获批后先建test-only Change Record；
若发现first difference，停止并建立独立修复包。

## Protected boundaries

不改C++、DAT、30Hz、FrameInputSet、worker、SoA/ECS、容量、CentralOnly、对象池、T8、IL2CPP、
Android、服务器或F1/F2 debug；不把1000 AI性能当行为对齐证据。

## Next action

R04不再执行。后续重新审计非AI正常战斗逻辑；未来状态树/行为树只需接入canonical `FrameInputSet`
固定tick边界，不要求与C++ AI decision tree保持行为一致。
