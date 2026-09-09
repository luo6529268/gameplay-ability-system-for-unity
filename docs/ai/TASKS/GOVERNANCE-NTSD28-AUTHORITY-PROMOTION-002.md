# Task Contract — GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002

> 状态：`VERIFIED / USER_CONFIRMED / AUTHORITY_PROMOTED / B0-B3-IMPACT-CLASSIFIED`
> 建立日期：2026-09-04
> 上游：`GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001`

## 用户决定与目标

用户确认指定目录中的变化是其修复 NTSD 2.8-Logan Bug 后产生的新版，并明确要求继续处理。
本包将当前根 `NTSD2.8-Logan.exe` 及 `source/README_SOURCE.md` 声明对应、实际进入 playable
构建闭包的当前源码正式晋升为唯一战斗行为权威，解除 identity drift 阻塞，并重新基线 B0～当前 B3
所有依赖正式 EXE/source 身份或 pass 顺序的合同。

## 晋升身份

- 根目录：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan`
- EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`
- 82-file playable C++/header closure manifest：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`
- 75-file authority source-capture manifest：`07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`
- `simulation_tick_driver.cpp`：`4AA2CA63BDFFB0D9C7D2148354C70BE8EA28697969CEDA2693EBC247A8BC2187`
- `game_session.cpp`：`9CCB6E12A97FCB9FF95DF2FE95BC33F24C0AC32E9F956E41D23E4C14165E4189`
- playable `build.ps1`：`AA8EBF4520D3C36BD5ADBB5B5BB5750957C5C71826AD64ECC63F0E81A23F292E`

旧 `1277B70B...DAF75` EXE 与 `C59BD8D3...2D75` source manifest 只保留为历史基线，不再有裁决权。
最初漂移审计的`5F2E5B41...5FA9`只是旧工具漏掉两个新增data source后的73-file子集；不得作为正式
capture manifest或完整playable closure manifest。

## 允许范围

- 更新 `AGENTS.md`、当前权威入口、总表、Ledger、STATE、handoff、Decision、B3 manifest 与相关
  Task/Change Record。
- 更新 workspace trace/capture 工具中正式 EXE 身份常量，使新产出的诊断证据指向新版权威。
- 只读比较当前 authority playable live source；authority 目录零写入。
- 对受影响的 Unity pass contract/sequence 另建 test-first Change 后再修改，不在本治理包中混入行为改动。

## 验收

1. 所有当前恢复入口都只把新版身份写为当前唯一权威，旧身份明确为历史。
2. drift Record 以用户决定闭合；B3 audit 恢复，不再等待用户裁决。
3. workspace capture/trace contract 的正式 EXE identity 更新并通过其最窄自检/构建。
4. 建立旧/新权威差异影响矩阵，B0～B2未受影响项与需重证项分开，B3 pass 顺序不沿用旧表。
5. `Tools/Validate-ChangeLedger.ps1` 通过；authority 目录没有任何写入。

## 回滚

若用户以后再次指定其他正式 artifact，以新的显式 governance decision supersede；不得删除本次用户晋升和
旧→新身份变化事实。workspace 工具常量可按新的正式身份机械更新，authority 文件始终不回写。
