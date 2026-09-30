# Q09/P-08 自然受击到 Legacy 出血像素限定验收

状态：`VERIFIED_SCOPED_NATURAL_LEGACY_PIXEL`，对应 `NTSD28-Q09-BPOINT-NATURAL-LEGACY-PIXEL-001`。此证据只将已独立通过的自然命中和 Legacy 绘制连接为同一次原 Battle Scene Play；P-08、Q09、BATCH-05及总目标仍开放，Q07/BATCH-04不受本项影响。

权威与输入：正式根 EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`，配对 playable `render_snapshot.cpp` 的本体后 bpoint 顺序与 `d3d11_renderer.cpp` 共用展示位移。原项目保存的 `NTSD_Battle.unity` 使用正式 LoganRuntime Naruto/Ita DAT，测试期仅以精确命名的内存 `GameConfig` 副本预选 LegacyOnly。P1鸣人经 Input System 物理 J，正式 Ita OID9 作为敌对池化测试体初始化 HP180、物理及源规则 X/Z；该自然分支仅在夹具初始化时写一次 HP，之后由完整生产 Driver 逐 tick 推进，最多80 tick。旧受控 Legacy 请求文件及原始结果未改，新独立请求为 `Temp/NTSD28_Q09_BPointNaturalLegacy.request.json`。

原始结果 `natural-ita-j-legacy-20260928-01.json` 为 `PASS_NATURAL_HIT_LEGACY_PIXEL`：两个已应用物理攻击 tick、鸣人正式攻击帧出现；完整 Driver 的 World tick12 将 Ita HP180→160，跨过 baseHP500 的166门槛。受击帧没有被误要求显示 bpoint；到 tick26 Ita 回到 frame0、bpoint1，Legacy 发布命令中 body index5、唯一 mark index6，标记尺寸1×3，生产专用 SpriteRenderer 活动1/拒绝0。此 Play 共推进21个受控完整 tick。报告的 `bodyVisible` 是旧受控分支字段，在本自然分支未赋值；自然分支实际有独立 body SpriteRenderer 非空/启用断言，不能把该默认 false 解读为隐藏。

同一暂停逻辑帧，仅暂时禁用该精确 mark Renderer 捕获基线图，再恢复并捕获标记图；角色帧、HP与相机配置均不变。原探针给出的底左坐标ROI为 x[798,803)、y[200,207)，新增红像素3。独立用 Pillow 读两张1920×1080 PNG，并将 Unity 底左 y 换算为 PNG 顶左 y 后，在同一阈值下复算为3个像素：底左坐标 `(800,203)`、`(800,204)`、`(800,205)`。首次未翻转 y 的读图计算为0，属于离线坐标原点错误，纠正后与探针一致。两图和JSON均保留；JSON SHA-256 `167883A8039B8B9DDA3200809D75DAD5E29DF4EDCC8CB2D80A1B2C1533391136`，高/低PNG分别为 `EDD7FB09F6355D91C375560138F2420FA22DF1DAD444860D51A4BFF4B438DBBB` / `D379C40CB3D5B1EEB3594B430ABCF0DBB3F8AD5B6A97DA760E433A018F8449BF`。

退出后请求状态为 `requested=false/running=false`，原 Editor MCP 报 idle、非Play、Battle Scene `isDirty=false`。临时体释放，对象4→4、槽2→2、池借用2→2，摄像机恢复；Battle/Menu/GameConfig/ProjectBattleModeConfig 的 SHA-256 分别保持 `2EE465D8...B48B77A`、`785F828C...1B81E13`、`0527D737...D074CB8EA7`、`B57CFEF3...C1EDD85B82`，四路径Git-clean。现有只读菜单于10:27:17Z确认 GameConfig单例为保存的CentralOnly Asset、loaded=1、probeClone=0。生成Editor项目编译0错/201警告，原Editor Tundra成功导入，未启动第二Unity或使用computer-use。最终 Change Ledger与diff检查结果记于Change Record。

限制：Ita是测试期临时敌对角色，不证明其自然阵容选择；对照的是同一Unity帧隐藏/显示标记的GPU贡献，不是正式EXE相同状态、相同视口像素。P-08的正式EXE可比视图与其余画面门、Q09汇总和Q12整场仍需独立验收。生产脚本、DAT数值、图片、Scene、保存的Asset、输入绑定及非战斗逻辑未因本包改变。

最终留痕：`Tools/Validate-ChangeLedger.ps1` 使用PowerShell 7退出码0，报告 `Change ledger validation PASSED`（975 Records、12 governed dirty code paths），带历史Record警告的完整输出见 `Temp/NTSD28-Q09-BPOINT-NATURAL-LEGACY-PIXEL-001-ledger-final.log`；`git -c core.safecrlf=false diff --check`退出码0。
