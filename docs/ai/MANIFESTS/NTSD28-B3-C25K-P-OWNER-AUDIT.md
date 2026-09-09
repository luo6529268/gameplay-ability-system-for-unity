# NTSD 2.8-Logan C25k-p owner audit

> Change ID：`NTSD28-B3-C25K-P-OWNER-AUDIT-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`  
> Authority：formal EXE `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`

## 1. 同槽顺序合同

```text
C25k frame-counter-zero OPoint
  -> C25l previous-action based state18/19 particles
  -> C25m previous_action_078 = post-frame action
  -> C25n broken weapon pieces + sound + arm lifecycle
  -> C25o resolve pending lifecycle
       -> pending consumed: continue next slot
       -> survivor: C25p healing
```

扫描是动态升序live-slot：k/l/n分配的高slot可在同tick被后续扫描；已越过的低slot复用留到下一tick。

## 2. 逐项矩阵

| ID | Authority gate / effect | Unity当前owner | 已确认差异 | 路由 |
|---|---|---|---|---|
| C25k | active、非terminal pending、frame counter==0；每次zero entry均可生成 | `ProcessLateOpointSegment`→factory | counter gate已有；early frame exit/death-opoint在前，另有type0 FrameDelay gate | C25K/M独立包；terminal carrier依赖B7 |
| C25l | old `previous_action_078`为state18/19；持续态受global delay和1/4 sync RNG；离开态7个；每粒四次sync RNG | `SpawnLateTransitionEffects` branch2 | 执行过晚；混合branch1；缺global delay；需锁定slot exhaustion/RNG | C25L独立包，资源缺口回H/B11 |
| C25m | l后立即写`previous_action_078=action` | `MirrorLatePrevFrame` | 字段绑定正确，位置过晚；多个旧Prev reader仍在前置tail | C25K/M先拆reader再commit |
| C25n | type1/2/4/6且weapon_hp<0；sound、built-in OID999、DAT weapon_piece、sync RNG，标terminal lifecycle | `TryRunLatePostOpointCleanupPhase` | 只有sound+PendingFlushDestroy，碎片/RNG/DAT均缺 | B7+B10+B11/H前置后独立实现 |
| C25o | pending 11xx/12xx reset action0/runtime code；其他<0或>=999 despawn；pending即消费slot | `HandleFrameTickExit` + loop-end deferred flush | 执行早于k；无正式pending/code carrier；physical free不在同槽闭合 | B7 carrier/关系安全清理后实现 |
| C25p | survivor、type0、HP>0；encoded/ordinary timer每8恢复8；state1700 arm1100 | `EntityPostFrameTailAll` + ECS post-tail | 算法近似正确但为loop后global scan、位于random-drop后并混入F7/cleanup | `NTSD28-B3-C25P-HEALING-OWNER-001`先行 |

## 3. C25p安全迁移边界

- 只在当前live-slot occupant仍active时执行。
- 只接受current DAT type0且HP>0；non-character即使有timer也不推进。
- encoded timer与ordinary timer保持两个独立字段；到上限/整千完成规则不合并。
- state1700在同一次p尾部最后arm为1100。
- `EntityPostFrameTailAll`仍保留F7 init-stats、hit carrier清理、transient字段清理和snapshot职责，但不得再重复healing。

## 4. 未授权边界

不得在本审计或C25p包中创建正式terminal carrier、覆盖DAT/资源、实现weapon-piece表、改变音频、Scene、Input或Authority。用户批准的随机掉武器例外保持原样。
