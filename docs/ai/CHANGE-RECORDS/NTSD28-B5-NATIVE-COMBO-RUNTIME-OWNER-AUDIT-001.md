# NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan SimulationTickDriver28 native combo producers/expiry plus playable native_combo_hud mapping; EXE B1E13AE1, closure 39DDDA15.
evidence: read-only audit froze separate entity/world carriers, shared post-dispatch ordinary producer, C25-post expiry, B6 caughtact dependency, B10 presentation and H activation boundaries; five packages defined; no behavior writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED`

只读确认字段生命周期、mode值入口、普通命中与caughtact producer写入面、C25后expiry owner、
snapshot/checksum/restore及B10表现边界；当前不修改任何代码、内容或Scene。

## Owner结论

- entity专用字段必须新增，不能复用`ComboD*`、`ComboCountAtk/Vic`；随runtime canonical copy、reset、raw slot、
  ECS mirror、entity snapshot、checksum/parity和restore闭合。
- selected-mode tuple属于world deterministic scalar；carrier默认`recordPresent=false`，测试可显式注入正式tuple；
  H决定是否把Authority内容设为正式默认。
- ordinary producer唯一共享接点是`BattleHitCandidateSequenceRunner`中普通Damage dispatch成功之后；它自然覆盖
  character/object两pass及legacy/DataOriented，并排除first-BDY early success。必须按捕获slot回查active source，
  以保留OID201等同tick已despawn时的native fail-visible no-op。
- last tick使用当前逻辑process tick；非type0 source只允许一次`OwnerSlotIndex`解引用，missing/inactive fail closed。
- expiry owner是`NTSDBattleTickSystem`的C25 `LateEntityUpdate`之后、session post-core之前；
  `NTSD28BattlePassOrder.CoreComboExpire`已有描述但尚无调用。
- caughtact producer必须消费B6真实settlement injury event；HUD/命中数字只读逻辑字段，归B10。

下一包：`NTSD28-B5-NATIVE-COMBO-CARRIERS-001`。
