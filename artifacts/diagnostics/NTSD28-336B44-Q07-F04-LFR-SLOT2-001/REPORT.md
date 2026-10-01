# Q07/F04 正式 OID600 槽2 LFR 载体限定验收

状态：`VERIFIED_SCOPED_TRANSPORT`；父 F04 仍 `UNITY_FOCUSED_PASS / RUNTIME_PENDING / STORY_CONTENT_DEFINED_ENTRY / USER_STAGE_ASSET_HOLD`。本包只证明受控普通战斗初态中的 OID600/type4/action0 可以通过正式 LFR 回放载体传给当前正式根 EXE；不证明剧情自然入场、玩家使用、E4 写入、精确回满或 Unity 画面。

权威身份：正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 `resources/runtime/decoded_dat/w/6.dat` SHA-256 `2641D21E09CD222B50036559496D266B73037BB7F3CF23F7B5A6590FCE5C118A`。诊断只链接该发行版声明的 playable Core/GameSession/LFR 源码，不改正式 EXE 或 DAT；自编诊断 EXE SHA-256 `675FF7EEFC966A38815B502F925406EC29F4941114EC8FA9DCA2BAA345CBC764` 不是行为权威。

受控初态：普通 mode0、seed2833、背景23，Naruto OID2/slot0/X500/Z650/team1、Lee OID7/slot1/X1200/Z650/team2、正式治疗道具 OID600/type4/slot2/action0/X500/Y-20/Z650/team1/HP250，三者均通过 `GameSession28::initialize` 配置生成；不在初始化后手设动作、位置或速度。与前一槽50探针相比，此初态的槽2可由正式 LFR 记录，并不冒充项目剧情道具的自然生成。

[唯一新增源探针](../../../Tools/NTSD28Q07Diagnostics/f04_oid600_lfr_slot2_probe.cpp)完成12个中性输入完整 tick，`GameSessionLfr28` 录制出的 [LFR](source-run-v2/slot2-source-packets.lfr) SHA-256 `47FC0B86BBEA21E69D3B070EC975DBB5BEA2D09063559A5DCC2341C21C09B192`。同源码的独立 `GameSessionLfrPlayback28` 重建初态并运行；三槽在初始及12个 tick 的声明采样 [39/39 无差异](source-run-v2/source-vs-local-playback.csv)，采样含 active/OID/type/action/XYZ/HP/owner/group/E4。正式桥接在12个录制 tick 后还声明一个零输入终行；诊断消费它后最终头校验通过。第一个运行版在消费终行前校验而 exit4，保留 [失败输出](source-run-v1-stderr.txt) 与原 CSV；这是探针调用时点错误，非实体首差。首次编译因宽字符入口漏 `-municode` 链接失败；[v3 参数](compile-argv-v3.txt) 修正后编译 exit0、stderr 0 行。

同一 LFR 原样交给 SHA 已核的正式根 EXE。其 [报告](root-run-v1/root-report.json) `passed=true / failureCode=0 / declaredTicks=12 / completedTicks=13 / nativeParityClaim=false`；根 trace 共14行（初始、12录制 tick、零输入终行）。独立脚本将源 CSV 与 [根 trace](root-run-v1/root-trace.jsonl) 的 tick0～12、三槽、10项字段逐项比较，[390/390 无差异](root-run-v1/root-vs-source-comparison.json)，含 active/OID/type/action/XYZ/HP/owner/team。根 trace 的 OID600 在 tick12 为 action62，并在 slot50 显示 OID219/action50，tick13 后者为 action51；这只是根侧可观察的下一链前驱，本包没有源侧子体逐 tick 配对或 E4/治疗事件证书。

未运行 Unity Editor、Play、SelfCheck或视觉/设备验证；没有修改生产 C#、Scene、DAT、图片、背景、模式或 stage 部署。下一独立 F04 机制包可复用本载体，延长正式完整 tick 并逐项跟踪 OID219、目标槽/E4、精确回满及再次受伤；只有取得源/根/Unity同条件证据后才允许提升 F04 状态。本包不得替代剧情场景入口或 Q07 整组验收。

交付检查：`Tools/Validate-ChangeLedger.ps1` exit0，报告 `PASSED`（1112 Record、55 个当前代码差异文件均有覆盖；对其他历史 Record 的非当前 diff 路径有警告）；本包四个新文本文件 UTF-8/尾空格/末行检查通过，相关已跟踪文档 `git diff --check` exit0。未执行 Unity 编译或 Play，故这些检查不为 F04 运行时对齐背书。
