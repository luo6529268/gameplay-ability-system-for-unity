# D-024 平台阴影高度比例：限定验收

Change：`NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-001`。2026-10-05；结论为 `VERIFIED (scoped shadow display conversion)`，不关闭 Q07、Q09、Q12 或总目标。

正式根 EXE 本轮再次只读核对为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 playable 闭包中的 `battle_world.cpp` operation30 写入源高度 `render_shadow_offset_10c`，`render_snapshot.cpp` 在源 Z 上消费该高度。用户 D-024 要求位移按画面比例处理；实体图片尺寸仍按用户确认保留 1.5 倍。

## 改动与首差

修前中央 `BattlePresentationShadowBuild.BuildCommands` 与 Legacy `LF2Entity.UpdateShadow` 将源10C直接加到已投影的 view Z。修后两个阴影显示出口均通过既有 `BattleSpatialProjection.SourceDeltaToViewY` 消费源高度；中央使用冻结 frame 的投影，Legacy 使用已注册 World 的投影，无 World 时恒等。源10C、平台链接/规则、排序 Z、DAT、图片、Scene 和非战斗均不在本次生产增量中。

只有两个生产位置表达式改变。测试参数化既有两个方法，各含恒等/+23与固定视野/-50，共4项。中央旧断言包含本就消费平台10C的姓名牌，恒等案也失败；仅排除用户范围外 OverlayGlyph 的位置比较，保留类型、排序、源值、唯一阴影和实际阴影位置断言，没有改姓名牌生产。

## 编译与原 Editor 定向测试

生成工程命令为 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`。

- RED 构建退出0、301 warnings/0 errors；原 Editor MCP 刷新后 job `b60f56e93a2a48318a7a6170af014ea6` 实际执行4项，1通过/3失败。两个固定视野出口失败，恒等 Legacy 通过；第三失败是上述姓名牌旧断言。MCP 最终 result 为 null，计数来自 completed=4 与三项 failures_so_far；8841为发现总数。
- 生产修改后的构建退出0、334 warnings/0 errors；同两方法 job `6eaa10789c534d5eb3902db8d1767b47` 正式汇总4/4通过、0失败，duration0.8665422秒，原件 [editor-green-result.json](editor-green-result.json)。
- 原 Scene 消费者探针的生成构建退出0、301 warnings/0 errors，10.82秒；原 Editor MCP 刷新后程序集时间晚于脚本，Console0 error。没有运行整类或全套测试。

## 原 Battle Scene 消费者

使用既有 OID36/56/2 自然平台输入前31个生产 Driver tick，限定 runId 前缀 `d024-shadow-height-`；原96tick模式及其它模式保持。新增诊断仅读取实际中央 Shadow 命令，同tick发布/计划、alpha1、slot2唯一命令；暂用30显示策略后 finally 恢复。

原件为 [d024-shadow-height-20261005-01.json](../NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-SCENE-001/d024-shadow-height-20261005-01.json)，`CAPTURED/DONE`，全局5→36、31tick、正常退出。

| 相对tick / 全局tick | 源10C | 实际视图高度偏移px | 独立1152/730期望px | 残差px |
| --- | --- | --- | --- | --- |
| 29 / 34 | -50 | -78.90410599643229 | -78.9041095890411 | 0.00000359261 |
| 30 / 35 | -58 | -91.5287515117694 | -91.52876712328766 | 0.00001561152 |
| 31 / 36 | -64 | -100.99725949013069 | -100.9972602739726 | 0.000000783842 |

各项 publication/plan 均与该全局tick相同、alpha1、Shadow command count1。源高度不修改，视图高度符合统一比例，最大残差小于0.000016px。

[scene-prefix-comparison.json](scene-prefix-comparison.json) 将旧自然96tick记录的前31tick与本次全部已有序列化规则 samples 逐叶比较：2945/2945相同、首差0。覆盖记录中的输入、实体和 RNG 字段；不冒充全 World 校验。

正常有序关闭为 `Completed/RuntimeMapCleared`，World对象、runtime槽、pool借用、活动池对象、活动sprite均0，World解绑、Pool quiesced。独立 MCP 后验原 Editor 非Play、原 Battle Scene clean/root11、Console0 error；Battle/Menu/GameConfig/ProjectBattleModeConfig 四SHA前后同。Battle SHA为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`；结果 SHA为 `F7517C2D07A9978427EF4E7FDDEBEE9C07C94774D2E76E88EE4B4E947F87E220`。

临时请求按 [Operation](../../../docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q09-D024-SHADOW-REQUEST-20261005-001/RECORD.md) 管理：原本不存在，CreateNew后由探针消费为 requested=false并保留；没有删除、覆盖旧请求或待恢复的原字节。[操作后原件](../NTSD28-336B44-Q09-D024-SHADOW-REQUEST-20261005-001/after-manifest.json) 保存非Play/clean/哈希及终态。

## 审阅与边界

独立只读代理对照 before 备份确认两个生产文件仅两个阴影消费表达式改变，冻结源值不变、没有新 service 或源状态写入；两个测试期望使用独立1152/730公式，探针限定前缀且显示策略 finally 恢复，未发现本范围阻断问题。正常运行关闭有实际零残留见证；既有失败退出路径未改变，不能由本次正常运行宣称其所有失败条件都已验收。

本次没有 GPU 像素截图、真人按键、Legacy 原 Scene 自然阴影截图、完整 SelfCheck 或全角色/全场景矩阵。恒等/固定视野两出口及自然中央命令出口已有各自证据；其它条件只在实际非例外首差时触发。`NONCHAR-HITFA7-CONDITION.md` 仅为另一个静态比例候选，尚无自然同初态首差，不据此生成必跑任务。

进度记录收尾后的 `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity` 退出0、PASSED，1271份全仓Record、diff中22个受治理脚本均被覆盖；`git diff --check` 退出0。新版同命名范围实际逐文件复核218份Record、57份未关闭；当前调度REUSE43/TRIGGER14/P0=DEP=ONE=0。此全仓覆盖检查不表示本次修改了22个脚本，本次精确脚本范围仍为Record预声明的4个。
