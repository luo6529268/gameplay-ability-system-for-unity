# Q09/P-12 OID998 无 owner 视口 fallback：配对正式 Session 定向报告

状态：`VERIFIED_SCOPED_PAIRED_SOURCE_SESSION`。本报告仅关闭 `NTSD28-Q09-P12-ETC998-SESSION-001` 的配对源码诊断出口；Q07、Q09/P-12 的 Unity 双表现出口和总对齐仍开放。

权威身份：正式根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。诊断链接其配对 playable 构建闭包，读取正式 `resources/runtime`，通过完整 `GameSession28::step()` 自然触发 state14 的续接复活；未直接创建 OID998、改 DAT、改正式源码或改 Unity 战斗代码。

| 运行 | 受控初态 | tick1 完整 Session / render 结果 | 判定 |
|---|---|---|---|
| run-01 v1 | host X0、Y0 | `continuation_armed=1`、`presentation_spawned=1`，但末态无 OID998/sprite；X1329 正例通过 | 整组 exit4，原件保留 |
| run-02 v2 | host X50、Y0 | 同样事件已发而末态无 OID998/sprite；X1329 正例通过 | 整组 exit4，原件保留 |
| run-03 v3 左 | host X50、Y-20 | tick1 OID998/action6/state9997/owner-1、slot50，物理/render facing0，camera X0，sprite left0/top431/width79；fallback 预期 left0，三处 stage offstage cull counter 均0 | PASS |
| run-03 v3 右 | host X1329、Y0 | tick1 同一自然续接得到 OID998/action6/state9997/owner-1、slot50，物理/render facing0，camera X0，sprite left1253/top451/width79；fallback 预期 `1333-1-79=1253`，三处 cull counter 均0 | PASS |

两正例 tick2 均自然推进至 action7/state9997，左右夹取持续匹配。v1/v2 低 X 消失与正式 `BattleWorld28::settle_ordinary_stage_bounds()` 对地面非角色对象 X<100 的清除规则相符；旧 TSV 未采集 cull counter，因此**不能把该规则记作旧运行已直接测得的清除原因**。v3 空中样本避开此条件，仍经过真实续接生成路径。右例和左例使用不同受控 Y，这是两个边界分支的定向见证，不能宣称同一世界的全状态等价。

验证：g++ C++17 v3 编译 exit0；`run-03` 退出0，`left=1 right=1`；对已存在的 run-03 再执行时退出3，前后两个 TSV SHA 均不变。`Tools/Validate-ChangeLedger.ps1` exit0，限定文档 `git diff --check` exit0。validator 中其它历史 Record 声明路径不在当前 diff 的 WARNING 已保留日志，非本诊断错误。原始文件及编译参数保存在本目录。关键 SHA-256：诊断源 `4939A8AEEAC9898FE05E9DDDFDE819601464A3387C9A10C6202A54BB18E96F1E`；v3 EXE `D32D35AF731EB3664141C24077E54337FB2F7AE2D31137B19588A4C1B4A8F988`；左 TSV `CCDDA92281DD836B1B502D2107F81A21C8FAC18841D9D0E3AB5002E7223EB3DC`；右 TSV `0A7887C008CBBA242E1AB90E029AAD8C8E1E6918E0D73D0F0AE8A2169626AF65`。

限制与后继：这是配对正式源码加正式资源的受控 Session/render snapshot 证据，**不是根正式 EXE 可见像素，也不是原 Unity Editor 的中央、Legacy 或 GPU 画面验收**。待原 Editor 安全恢复后，仅针对已有 `state9997` 共用投影包运行聚焦三例和原 Battle 的 owner 有效/无 owner 正反例；旧 Q07 普通复活矩阵不重跑。当前 Editor 未保存场景状态且 MCP 不可用，不能启动第二实例或覆盖现场。
