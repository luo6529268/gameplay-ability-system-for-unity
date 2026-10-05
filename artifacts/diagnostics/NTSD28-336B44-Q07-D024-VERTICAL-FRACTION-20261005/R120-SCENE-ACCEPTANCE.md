# D-024 共用 Y 投影：原 Battle Scene R120 中间命令

状态：`SCOPED_ORIGINAL_SCENE_R120_COMMAND_PASS`。2026-10-05，原项目 Unity 2022.3.62f3 Editor 经现有 MCP 刷新后，在保存的 `NTSD_Battle` 执行一次既有 Guren OID84→OID85 的 32 tick 正例。未启动第二个项目、未使用 computer-use。用户确认的 `BattleVisualScale=1.5` 显示尺寸保留，本轮只修改既有 Editor 探针，不改生产、DAT、图片、Scene 或非战斗功能。

[本次原始 JSON](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-r120-20261005-01.json)为 `PASS/DONE`，全局 tick5→37。相对23/全局28读取生产 `BattleCentralRenderSystem.PrepareFrame`：先将 Play clone 的显示策略临时设为120，再在同逻辑 tick 读取30策略完整位置；两次均 finally 恢复原策略。使用生产自然显示时钟，没有注入 alpha、反射修改时间、睡眠或忙等。

| 字段 | 实测或独立公式 |
| --- | ---: |
| 相邻 motion tick | 27→28 |
| OID85 / slot / action | 85 / 51 / 212 |
| 前→后源 preciseY | -22→-20.3 |
| 前→后源 preciseZ | 402→402 |
| 实际中间 alpha | 0.0156545455392932 |
| 正式 lround 的中间 Y / 完整 Y | -22 / -20 |
| 源 Y 差 | -2 |
| 统一 Y 倍率 | 1152/730 |
| 本体预期差，视图像素 | 3.1561643835616437 |
| 本体观测差，视图像素 | 3.1561852206927217 |
| 观测减预期 | 0.000020837131078 |
| 地面阴影差，视图像素 | 0 |

两个计划均已物化、发布/计划/捕获 tick28，每个都有一条本体和一条阴影命令。0<alpha<1 且源 Y 差非零，证明本样例实际经过中间插值；源 Z 恒定时只有本体高度随 Y 变化，阴影不随 Y。取整按当前正式 playable `presentation_interpolation.cpp` 的 preciseY→lround→相对完整位置 deltaY，之后只应用统一视图倍率。1.5倍图像显示尺寸不参与高度位移倍率。

本次完整32个 `samples` 与[先前修复后原 Scene](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-green-20261005-01.json)逐项严格相同。已只读复核该基线及其源CSV、正式根 trace 的 SHA 均未变，因而复用[既有1690项源/Unity、1658项正式根所选字段对照](LANDING-REUSE.md)，不称本轮重新运行了正式 EXE。正式根独立 CRT 初态未随 LFR 携带的限制保持；此区间没有新增 CRT 消费，也没有全 World/checksum 一致证书。[本轮逐文件身份及独立计算](r120-scene-evidence-20261005.json)保留原始中间值、全样本同值和误差。

生成 Editor 命令 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 退出0，301 warnings/0 errors；原 Editor MCP `refresh_unity` 后 DLL 晚于脚本且 Console0error。退出后 Editor 非Play/idle/非编译/无测试运行，Battle Scene clean/root11，磁盘 SHA 前后保持 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。临时请求按[预登记 Operation](../../../docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001/RECORD.md)恢复原68字节，SHA保持 `7486F5BB3126869EC1D32C6033B68C6A94A19797573BCD0AEE21ED220B985BF1`；原件和备份保留。

这关闭本 Task 的单次原 Scene R120 **命令出口**，不证明设备实际120FPS、GPU Present、其它 alpha/实体关系或正式 GUI 同帧逐像素。落地画面及原 Scene 空中普通命中仍按总表的实际首差/终验条件门处理，不因此添加角色矩阵或重跑已过命中六例。父生产 Record 保持 `RUNTIME_PENDING`，Q07/Q09/Q12及总目标仍开放。

交付前 `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 退出0，`Change ledger validation PASSED`，1267份Record/18个当前脚本差异均覆盖；`git diff --check` 退出0。本轮没有运行全套NUnit或SelfCheck，原Scene采样是已声明的单次32tick定向验证。
