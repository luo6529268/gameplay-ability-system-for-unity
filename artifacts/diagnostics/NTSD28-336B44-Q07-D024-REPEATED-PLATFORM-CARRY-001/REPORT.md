# Q07/D-024 链接平台重复搬运比例：聚焦修复与待验出口（2026-10-03）

状态：`FOCUSED_TEST_PASS / RUNTIME_PENDING`。本报告只证明共用平台搬运写者在合成但生产可调用的注册平台/乘客条件下，连续 X/Z 画面距离保持 D-024 统一比例；不关闭正式自然可达、原 Battle Scene 完整 Driver、根正式 EXE 行为或 Q07 父项。正式规则和内容权威根 EXE SHA-256 本轮重核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

## 首差与共用修复

当前 playable `source/ntsd28_core/src/simulation/battle_world.cpp::apply_frame_motion` 第1724–1777行在链接平台非零 `dvx/dvz` 时，均从乘客当前**规则整数**位置重建精确源位置，再按原生取整。Unity `LF2Entity.ApplyLinkedPlatformMotion` 先前虽已给单次物理搬运乘固定全背景比例，每次却从**物理整数**位置重基，导致分开取整的连续误差。上轮共用直接帧尾修复明确将链接平台留为独立出口，本轮没有重做平台候选或 pass 顺序。

新聚焦测试在原 Editor 中先按正式源字段规则设置注册平台、乘客及每步+4/+2的链接搬运，连续24次。默认视口左右两例通过；2048×1152视口右/左两例 RED，X 最大偏差分别为 `3.6241560390097334` 和 `3.070517629407334` 输出像素，超过1像素门槛。[RED任务原件](unity-red-result.json) 的任务ID是`20c52ca156b0436cb04ca97b80a13f29`。本测试是合成平台条件，不是正式根自然场景；测试X首差在前，未从RED单独断言Z表现。

生产只改 `LF2Entity.ApplyLinkedPlatformMotion` 的非零X/Z最终出口：有规则源位置时，先沿原规则整数基底更新精确规则位置，再把该步精确规则**变化量**经World现有X/Z比例累加到物理精确位置；无源位置时保留原整数物理基底。原有帧DV、左右翻转、Y/参考高度、源规则取整、物理取整、链接关系与平台→直接帧顺序不变，无OID或角色分支。两个既有单次夹具故意把物理与源锚点分开，物理预期已按它们实际精确源差更新，源规则预期未变。DAT、地图、相机、Scene及非战斗脚本未改。

## 验证与范围

- 生成的 `Assembly-CSharp-Editor.csproj` 改前0错/275警告、改后0错/306警告；原 Editor 脚本刷新后帧运动整类 **39/39 PASS**，包含新增四例、旧单次平台、平台后延迟直接帧、正式OID92/736与原生夹具。[GREEN任务原件](unity-green-class-result.json) 的任务ID为`c95b368c514d45e7928f6a3ad452e2af`。结果在MCP输出连接断开前已落盘，JSON内任务`status=succeeded`且`result.summary`为39通过/0失败。
- 原Editor完整 `BattleRuntimeSelfCheck` 菜单回执超时，但它在本次调用后于18:25:33写出新`PASS`，后续18:27:04仍为`PASS`；[覆盖前副本](selfcheck-before.txt)与[本轮结果](selfcheck-after.txt)分别保留。回执超时不当作自检失败，也不把结果文件当作 Scene/正式根证书。
- 菜单调用前后 Menu与Battle磁盘SHA分别保持`5D79DBB7...EC0052D`、`93448372...7BF60`。MCP恢复后的只读查询显示Menu `isDirty=false`、根对象9个；此前域重载后的查询是8个。多出的 `BoundaryWallManager_AutoCreated` 曾在上一批出现，当前内存生命周期/直接创建链未查明，保留现场，不擅删。GameConfig与ProjectBattleModeConfig当前SHA分别为`0527D737...B8EA7`、`B57CFEF3...B82`（后两者本轮只查当前值，不主张本轮前后配对）。
- [当前正式DAT静态枚举](current-formal-platform-itr-frames.json)在`decoded_dat/*.dat`的帧体中找到平台关系ITR kind30/40/50/60共17帧，均为`c/hid/rea.dat`的kind50；只有frame182声明非零`dvx:-3`，其ITR位置为`y:564000`，没有帧声明非零`dvz`。这只是当前解码DAT文本清单，**不证明该frame的链接碰撞自然可达，也不证明运行时不会动态创建其它合法平台状态**。不能用本合成24次例子冒充正式资源的实际连续发生次数。

下一证据门为确定当前正式内容/输入是否让非零链接平台搬运在 playable live path 自然发生；若可达，再做同初态正式源/根与原Battle Scene完整Driver逐tick及真实视图、有序关闭验收。若当前内容无自然阳性，保留本共享写者聚焦证据，并将自然场景标条件性待证。Q07/D-024全实体、Q09/Q12和总目标仍开放。
