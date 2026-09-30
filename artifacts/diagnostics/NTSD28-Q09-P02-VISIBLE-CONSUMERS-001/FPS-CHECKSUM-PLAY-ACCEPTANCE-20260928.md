# Q09/P-02 30/60/120 FPS 展示采样校验和限定验收（2026-09-28）

状态：`VERIFIED_SCOPED_FPS_CHECKSUM_ONLY`；父项 `NTSD28-Q09-P02-VISIBLE-CONSUMERS-001 / RUNTIME_PENDING`、Q09/BATCH-05 和总目标继续开放。原项目原 Editor、保存的 `NTSD_Battle` Scene、Play 内生产 World、同一暂停逻辑 tick；探针只控制每档展示帧间隔和相邻已发布 motion row，不修改生产脚本、DAT、相机或场景。

原始证据为 [`live-play-result.txt`](live-play-result.txt)。两次前置失败分别保存在 [`fps-checksum-first-fail-backend-20260928.txt`](fps-checksum-first-fail-backend-20260928.txt) 与 [`fps-checksum-second-fail-backend-20260928.txt`](fps-checksum-second-fail-backend-20260928.txt)：World 后端均为 LegacyOnly，故在任何 FPS 采样前因探针要求 CentralOnly 而停止；这不是展示采样结果。独立的 [`NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001`](../NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001/idle-config-inspection.txt) 查到三个已加载的探针 LegacyOnly GameConfig 副本，精确清理后再进行最终一次 Play。原有被覆盖的跟踪结果已逐字节保存在 [`prior-live-play-result-before-fps-checksum-20260928.txt`](prior-live-play-result-before-fps-checksum-20260928.txt)。

| 展示 FPS | 首次与后续中央实体 X | 同 tick World parity checksum（首次 = 后续） | 观察 |
|---|---|---|---|
| 30 | `-3.6799652576446533` = `-3.6799652576446533` | `994bbd663288105bee1ec380b78c91721a0302dafb0da2ecf914824a25893519` | generation 6 保持、离散 |
| 60 | `-3.6772420406341553` → `-3.3699653148651123` | `bba87569bc6e858b3be4da3f220f16ca2a28c6bc991b8192306b87ee34e0b09b` | generation 7→11、采样 |
| 120 | `-3.3772420883178711` → `-3.0699653625488281` | `7bba89da36857b5baaaa08bb66e7585e334cd183ef144098ccf4b704e9bc7c21` | generation 12→16、采样 |

三档之间的哈希不同，因为原探针每档依次控制角色移动 20 个规则源单位；比较对象是**每档自身第一次与随后一次展示采样**。探针同时断言暂停逻辑 tick、角色规则源 X 和放大后的画面 X 未被展示采样改变。后续 Legacy 兼容分支仅观察到 Transform X `-3.06724214553833`→`-2.7599654197692871`，当次没有 Legacy shadow renderer，不能算 Legacy 生产像素或阴影验收。最终原始结果为 `PASS`。

生成 `Assembly-CSharp-Editor.csproj` 构建 0 错误/199 警告；原 Editor 通过 MCP Refresh 导入新探针后运行，退出 Play 时 idle、非 Play，活动 Battle Scene `isDirty=false`。保存的 GameConfig Asset SHA-256 `0527D737...CB8EA7`、Battle Scene `2EE465D8...B48B77A`、Menu Scene `785F828C...81E13` 保持不变。`git diff --check` 通过。未执行全角色/技能矩阵。自然合成 GPU 像素、持有武器、Legacy 生产 body/shadow 像素及正式根 EXE 同视口对照仍需各自出口证据。

验收后将原 Editor 恢复到本轮开始时的 `NTSD_Menu`，状态 idle、非 Play，Menu Scene `isDirty=false`；没有保存任一 Scene。
