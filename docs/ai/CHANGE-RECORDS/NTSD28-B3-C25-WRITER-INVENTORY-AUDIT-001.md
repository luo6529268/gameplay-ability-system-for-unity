# NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001 — C25 writer inventory

<!-- CHANGE-RECORD
id: NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Promoted NTSD 2.8-Logan C25 nested live-slot tail; EXE B1E13AE1, playable closure 39DDDA15.
evidence: AUTHORITY-C25A-P-SOURCE-CLOSED / UNITY-THREE-OWNER-INVENTORY-CLOSED / LIVE-SLOT-BIRTH-SEMANTICS-CONFIRMED / NO-SCRIPT-CHANGE / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / C25A-P-WRITERS-MAPPED / NEXT-C25-SKELETON`

## 已观察事实

- Authority 在 C23/C24 后只建立一次 `for slot=0..maximum_slots-1` 动态扫描；每个仍存活 slot 依次完成 C25a～p。
- Unity 当前虽由 `BattleLateEntityLifecycleModule.Run` 动态升序重取 occupant，并已有高 slot 同 tick、低 slot 下一 tick 的 focused 证据，但同一实体的 writer 分散在此前的 `SerialTickAll`、此模块和其后的 `EntityPostFrameTailAll`。
- Unity `CurrentWaveStage` 与 `RenderDispatch` 位于 serial 与 late/post-tail 之间；因此当前 presentation 不是 Authority completed-tick 边界。
- Unity 当前在 OPoint 之前执行 encoded/terminal frame exit，在整个 loop 结束才 flush pending mutation；Authority 则先 OPoint、state18 粒子、previous-action commit、weapon fragments，再逐 slot lifecycle，并在 lifecycle 消费 slot 时跳过 healing。

## 分类结果

- `PARTIAL_PLUS_EXTRA`：C25a、C25b、C25c/e、C25g/h/j、C25k、C25l/m/o/p 均存在近似或历史 writer，但顺序、门控或字段集合不完整。
- `MISSING`：C25d 原生 display 四字段、C25f `native_computer_state_1b8` 正式 carrier/writer、C25i native armor HP/recovery timer、C25n 完整 built-in + DAT weapon fragments。
- `LEGACY_UNROUTED`：type3 serial state-entry/state15/death 与全局 state9998 cleanup 在当前 Authority C25 API 中没有可直接继承的同名 owner；实施时必须由当前 Authority 行为证明其归属，不能原样前置。
- `UNITY_EXTRA_OUTSIDE_C25`：F7 init-stats、hit-candidate/transient-MP 清理、用户 C15 随机掉武器和 Stage/Results/Render 属于独立 session/pass owner，不能塞入 C25p。

## 实施路由

1. `C25-SKELETON`：建立一个生产 C25 phase，紧接 C24；保留 live-slot 动态 cursor，Stage/Render 后移到 C25 之后；不把分段 global scan 伪装成完成。
2. `C25A-B`：精确 definition transition 与 special clone；移除 state9995/4000 历史 extra 对 C25a 的污染。
3. `C25C-E`：完整 resource pre/display/post 字段、双 phase 消费和前后可见性。
4. `C25F-J`：computer state、frame step、reaction timer bank、armor recovery、attacker rest。
5. `C25K-P`：frame-zero OPoint、state18 particles、previous action、weapon fragments、逐 slot lifecycle、healing。
6. C25 完整后再建立 C26 combo expiry 与 Session post/presentation completed-tick 边界。

## 验证

本包为只读治理审计，没有修改脚本或运行行为。完成了 Authority/Unity source closure、现有测试边界核对，并将详细证据写入独立 manifest。
