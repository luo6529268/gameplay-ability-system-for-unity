# Task Contract — GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001

> 状态：`SUPERSEDED / USER_DECISION_RECEIVED / PROMOTED_BY_GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002`  
> 建立日期：2026-09-04  
> 触发包：`NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001`

## 目标

记录当前指定authority根的物理EXE与源码已偏离用户确认的固定身份，防止上下文压缩后把未经确认的新构建
自动晋升为权威。等待用户在“恢复原锁定artifact”或“明确晋升当前artifact并重新基线”之间作出决定。

## 只读证据

- 合同锁定EXE SHA-256：`1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`。
- 当前根EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；
  size `5,466,646`，LastWriteTime `2026-09-04 20:45:19 +08:00`。
- 当前指定根递归只发现这一份EXE；固定SHA旧EXE不在该root。
- `source/README_SOURCE.md` SHA仍为`C0BA44DB...BB3D85`，但当前73-file source manifest为
  `5F2E5B41CAD5194B24C4253047087551B21B48FC0B12954029B340A9F86C5FA9`，不同于B0冻结的
  `C59BD8D3264B5CBF15EDBCFE2BAE64BC0F3BBC41926BEF6A4723EC2F571F2D75`。
- 当前`simulation_tick_driver.cpp`=`4AA2CA63...2187`、`game_session.cpp`=`9CCB6E12...4189`、
  playable `build.ps1`=`AA8EBF45...292E`；均不同于B3入口记录的`3CD804D3...C97`、
  `9F80AC96...2C7`、`4E480B88...40F6`。

## 强制边界

- 未获用户明确确认前，当前`B1E13AE1...` EXE和`5F2E5B41...` source不得成为规则authority。
- 不修改、移动、覆盖、构建或运行authority目录；本审计全程只读。
- B3不再进行新的production代码修改；已完成C01改动保留，不擅自回退。
- 若用户选择恢复旧artifact，先重新核验固定SHA/source manifest再恢复B3。
- 若用户选择晋升当前artifact，必须先更新CURRENT-AUTHORITY/AGENTS/总表固定身份，并对B0～当前B3
  受source delta影响的结论做显式rebaseline；不能只改一个hash继续沿用旧pass表。

## 回滚

用户裁决后以新的governance decision supersede本记录；不得删除本次drift事实。

## 用户裁决

2026-09-04 用户确认当前身份变化来自其对 NTSD 2.8-Logan 的 Bug 修复，并要求继续处理。当前 artifact
已由 `GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002` 晋升；本任务的等待条件已解除，本记录只保留漂移发现事实。
