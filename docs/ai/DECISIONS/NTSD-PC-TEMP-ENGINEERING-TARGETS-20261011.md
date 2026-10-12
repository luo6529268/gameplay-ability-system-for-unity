# PC 暂定工程目标与首轮 H10 预算

状态：`PC_TARGETS_ADOPTED`；暂定工程验收目标，尚未认证的配置。2026-10-11 人类在讨论会话 `01a11fe2-bfaa-72f3-a9ed-c8260f63b193` 对具体提案回复“可以，我觉得没什么问题”。本决定更新此前预算 UNDECIDED 的当前状态，历史记录不删。

| 档位 | 已批准的候选配置 | 未认证部分 |
|---|---|---|
| 基础 | Windows 11 x64、8GB RAM、SSD、CPU 初筛4核8线程、1280×720 / 60FPS | 核显为兼容候选，具体型号待验证；不是已经发布的最低配置 |
| 推荐 | Windows 11 x64、16GB RAM、SSD、CPU 初筛6核12线程、1920×1080 / 60FPS | GPU 具体型号待验证 |

核数不是性能证明，验收须记录实际 CPU 型号、功耗及持续性能。60FPS 是显示目标，正常战斗逻辑间隔保持精确33ms。当前开发机不能据此认证8GB配置。

| 首轮 H10 门 | 上限/条件 | 口径 |
|---|---|---|
| prepared AudioClip 对象常驻 | ≤320 MiB（335544320 B） | owned/borrowed原clip及输出按真实对象ID去重；13个nonmono不重复计 |
| 整个游戏进程稳态 | Private Bytes/private commit ≤2 GiB（2147483648 B） | 独立游戏进程门；Editor不能替代 |
| 切换过渡采样峰 | Private Bytes/private commit ≤3 GiB（3221225472 B） | 标明采样间隔；不是绝对瞬时峰证明 |
| 三轮关闭 | 每局owned音频归零、不能逐轮积累 | generic借用遵循既有合同，正常跨轮复用单列 |

MiB=2^20，GiB=2^30。decoder/staging/voice/temp 另列并进入整进程评价；320MiB不是整个音频子系统峰值。Private Bytes 与 working set 不相加；working set、物理余量及GPU分别列。2/3GiB整进程可行性尚无实测，不能为了通过测试自行提高预算。

没有授权增加生产拒绝阈值、裁剪catalog、lazy加载、格式或音质变化。旧218.313758MiB只在当前输入指纹对应后复用到对象预算域，不能扩大为整进程或设备证明。当前Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011；H10父项保持开放，H07/Android未通过。
