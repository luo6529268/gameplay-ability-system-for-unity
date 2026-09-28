# Q09/P-02 自然持武器未饱和展示相位：限定证据

状态：`RUNTIME_PENDING / UNSATURATED_COMMAND_WITNESS / PIXEL_OWNER_PENDING`。本包实际取得相邻帧 120 FPS 插值相位与武器中央命令移动，但**没有**取得可归因于武器的合成画面像素，也没有关闭 P-02/Q09。两次原项目 Battle Scene 请求的原始 JSON/PNG 均保留；不要把报告顶层 FAIL 改写成 PASS。

| 请求 | 实际结果 | 解释 |
|---|---|---|
| `q09-held-unsat-20260928-a` | 自然拾取 tick6、持有空攻 tick20，两图与武器命令均 alpha1/X不变，顶层 FAIL。 | 测试先于相机渲染改展示时钟仍来不及；不能据此判断插值生产错误。 |
| `q09-held-unsat-20260928-b` | 自然拾取 tick6、站立11、空中18、持有动作30 tick20；发布帧前一运动 tick19→当前20。World 相机 `beginCameraRendering` 回调每次1次，真实渲染 alpha **0.2055303028→0.7051363640**，武器 OID120 中央命令 X **-10.0540142059→-9.8850116730**。两次均是当前武器句柄/资源命令，且相机 submission 增加；两次捕获的 World parity checksum 同为 `e54a96d35593761bc585fd9bf8418b1b0507a20e78b75dded3dd080ebec2bf0b`。 | 未饱和相邻帧**命令**采样前置成立。顶层仍为 FAIL：探针恢复原展示时钟时又物化了新的正常 alpha1 画面，末尾旧断言误查这份尚未由相机提交的新画面，报 `NotSubmitted`。两个受控相位内的相机/资源提交检查已分别通过。 |

独立解码两个 `960×540` PNG：SHA 分别为 `DC1F119F55163B6E4797FB86573C31874E676E46EDE6C854097E8570DBDD3598` 与 `0A525F0F5E00C0584B38B49DB62A04CFE1CB9C7342F1B129B291401265A4D7BD`。逐像素 RGBA 有 **839** 处差异；将 Unity 自下而上的投影 Y 转成 PNG 自上而下的 Y 后，早/晚武器投影矩形并集 `x=[75,118), y=[212,250)` 内有 **412** 处差异，外部427。两相位该并集均被其它命令的保守投影矩形完全覆盖，`exclusiveArea=0`，所以这412处**不能归属为武器像素**；前一轮用未翻转 Y 检查得0的中间计算已废弃，不作为结果。

两轮结束均恢复临时输入焦点策略，原 Editor 离开 Play；`-b` 场景 clean，Battle Scene SHA 前后均为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，有序关闭、World对象/槽/池借用均0。探针随后仅把相机提交诊断捕获时点移到时钟恢复之前，生成 Editor 工程再次编译0错误/190警告；**此修正未重跑自然 Play，不覆盖 `-b` 顶层 FAIL**。原 Editor 原位重载与四保护 SHA、账本/diff 检查另见 Change Record 最终验证；本包不更改生产、DAT、Scene、模式 Asset 或非战斗逻辑。

最终检查：原 Editor PID11944 原位重载后的 `Assembly-CSharp-Editor.dll` 时间晚于修正源码，桥接状态为 `NTSD_Battle`、idle、非Play、无编译；Battle/Menu Scene、InputSystem 设置、ProjectBattleModeConfig 四项 SHA 与本轮保护值均相同。`Tools/Validate-ChangeLedger.ps1` 返回 PASSED（948 Records、17 个 diff code files），`git -c core.safecrlf=false diff --check` 返回0。既有持武器请求文件已消费为 `requested:false`，无待执行重复请求。

下一 P-02 独立门需在同一冻结帧做目标命令“有/无”受控渲染差分，证明被持武器真正贡献的像素，再比较两个未饱和相位；不能靠全图差分或完全重叠的矩形推断。正式 EXE 同视口画面、30/60/120 全速率、普通合成 Game View 仍归后继验收，不重跑已过的逻辑24tick与Q07实体代表例。
