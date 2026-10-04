# NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001

状态：`RUNTIME_PENDING / THIRD_SCENE_BLEED_COMMAND_PASS / RANDOM_TABLE_LIMIT`。父项：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 Q09/P-08`，继续开放。

2026-10-04 后继Q07共用三键修复后，第三轮原Scene22tick恢复目标HP10及一条1×3血点命令；六字段130/132同。余下tick2/3动作60/65因LFR manager同步随机表与Unity seed0表不同，不能据此判生产首差。四保护SHA前后同、Scene退出clean；本Task不再因旧上游110动作重复扫描，画面像素/同表和Q09总门仍未验。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/REPORT.md)。

权威：用户选定正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable `GameSession28::step`、`RenderSnapshotBuilder28`。当前根 EXE 对已有等 HP LFR 的 22 个声明 tick 回放成功，鸣人动作/X 与鼬动作/X/当前HP/基础HP六字段 132/132 对录制源相同；根 trace 本身不输出 bleed command/GPU 标记，`nativeParityClaim=false`，其 CRT 初态等价 seed0。正式/Unity 鼬 DAT SHA 同版。见 `artifacts/diagnostics/NTSD28-336B44-Q09-P08-EQUAL-HP-ROOT-LFR-001/` 与上一 P-08 报告。

任务只新增 opt-in Editor 诊断 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs` 及 `.meta`。原 Battle Scene Play clone 在 `BattleTestBootstrap.Start` 前将测试参战者指定为鸣人 OID2/slot0、鼬 OID9/slot1；战斗内容发布后暂停完整 Driver，在两实体上恢复根 LFR 的已声明局部初态：动作0/0、源 X500/540、Z650、当前/基础 HP500/500 与30/30、MP500、右/左朝向、关系组1/2、模式0、难度0、输入相位0、native clock重置、native RNG seed0。不得写正式 DAT 或改生产角色出生逻辑。新探针记录初态和每 tick 的完整输入、所选实体字段、RNG与血点命令，按根的前两 tick 攻击键输入推进22个完整 Driver tick；仅在本轮字段/命令具备时采原相机截图。源/根/Unity对比单独分析，不在探针里硬编码预期轨迹。

前置保护：检查唯一原 Editor idle/non-Play、单一保存且 clean Battle Scene；记录Battle/Menu/GameConfig/ProjectBattleModeConfig与新脚本目标状态。只从原项目运行；不用 computer-use、第二Unity项目或跨帧渲染状态写回逻辑。保留现有用户/其它任务未提交修改。

出口：生成 Editor 工程编译0错、原 Editor 新程序集导入0脚本错误；单次有界 Play 保存原始22 tick/首差、目标受击/站立血点中央命令与可见画面（若到达），即使失败也保留原始结果。比较根/Unity相同局部初态六字段的每 tick 首差，单列 RNG/World/视口差异；不能把局部相等扩成全World或正式根GPU逐像素一致。运行后退出Play、Battle Scene clean、四保护SHA不变、原Editor回运行前clean Menu；按实际结果更新 P-08/Q09。不得为了使测试通过修改DAT、生产逻辑或用户非战斗配置。

2026-10-04 实际：正式根 LFR 22tick 已复跑。原 Battle Scene 第一轮22tick获得目标 HP30→10/站立血点1条、1×3/相机PNG，退出 clean；所选六字段130/132，RNG44/44。2处动作差异来自探针误将前2tick提交为 Jump32，而正式根输入为 Attack16。探针已改，生成 Editor 编译0错；原 Editor 第二次刷新后长时间报告 compiling，程序集尚未更新，故同输入第二轮与回Menu尚未完成。原始输出和当前边界见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/REPORT.md)。

回滚：仅对本诊断脚本做前向补丁或另立弃用记录，不使用Git restore/reset，不覆盖旧报告。新证据采用唯一 runId，不重用已有输出路径。
2026-10-04 修正 Attack 的第二轮已在原 Battle Scene 完成22个生产 Driver tick：正式根 tick2 鸣人动作60，Unity 动作110；正式 tick8 鼬HP10/tick22一条1×3血点，Unity目标全程HP30/血点0。首差在共享人类三键投影，不在血点绘制写者；原Scene退出clean、四保护文件本轮 SHA 稳，结果为 `artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/ita-equal-hp-336b44-unity-02.json`。Q09/P-08转为依赖 `NTSD28-336B44-Q07-HUMAN-BUTTON-NATIVE-INGRESS-001`：其修复后若动作/HP同态而血点仍有差异，才重新打开绘制任务。第一轮 Jump 误输入的血点阳性仍只作历史夹具证据；不把当前无血点另立表现缺陷。
