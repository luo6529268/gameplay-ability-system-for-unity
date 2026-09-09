# Task Contract — NTSD28-B2-PROXY-INTEGRATION-AUDIT-001

> 状态：`VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在把0x21-byte block接入production前，闭合 Unity runtime字段、combo语义、proxy控制字段、AI producer
与两遍遍历顺序的crosswalk，禁止把旧9个`ComboD*`机械当作native10-byte bank。

## 已观察 crosswalk

- current/previous七键可复用runtime `Key*`/`Prev*`，但物理顺序必须固定为up/down/left/right/
  attack/jump/defend，不能按字段声明或enum巧合复制。
- native七个edge window对应Unity现有实际物理键映射：right=`CdRight`、left=`CdLeft`、up=`CdUp`、
  down=`CdDown`、attack=`CdDefend`、jump=`CdAttack`、defend=`CdJump`。
- native defend re-entry byte与Unity `CdDefendLock`同源：frame110/114刷新3、input每tick递减。
- native combo bank是10 bytes：水平attack、水平jump、up attack/up jump、down attack/down jump、
  J-K、J-L、L-K-J、K-L。Unity旧runtime只有9个`ComboD*`，且左右横向分别占字段；不是1:1布局。
- Unity没有native proxy tail，也没有`input_proxy_counter_14c/source_slot_178/enabled_17c`。
  authority counter只在eligible positive-HP entity body递减；counter<=0时enabled清零是unconditional tail。
- authority每tick第一遍升序完成non-character hit_Fa、AI producer、human/AI sampling并冻结slot0..7录像；
  第二遍才做proxy和input routing。Unity当前`CharacterInputAll`在同一逐实体循环内交错AI decision与combo，
  高slot AI source在低slot proxy时尚未生产，不能直接插入copy call。

## 实施拆包

1. `B2-NATIVE-INPUT-STATE-CARRIER`：为每runtime提供exact edge/current/previous/combo10/tail及
   proxy control字段，闭合reset/deep-copy/snapshot/checksum；旧字段暂保留。
2. `B2-NATIVE-COMBO-BRIDGE`：按2.8十状态机迁移edge/history/combo，并只在明确映射处桥接旧frame action。
3. `B2-PROXY-CONTROL-LIFECYCLE`：补counter/source/enabled writers、positive-HP decrement和unconditional cleanup。
4. `B2-AI-SAMPLE-PROXY-TWO-PASS`：拆AI producer/sample prepass、recording freeze、proxy copy、routing pass，
   覆盖lower proxy→higher AI source、invalid/dead/non-type0 source和same-tick action。
5. joint trace包验证phase/proxy/input history/call order；之后才可把proxy写为aligned。

## 边界

本包只读；不改runtime、AI、combo、hit writer、snapshot/checksum、DAT/Scene/资源。后续每包独立
Task/Change/test-first，不能一次性重写整个CharacterInputAll。

