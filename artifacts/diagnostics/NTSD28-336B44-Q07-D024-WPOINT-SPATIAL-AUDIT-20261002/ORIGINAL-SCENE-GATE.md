# 原 Battle Scene 物理按键拾取验证的当前门槛

2026-10-02 用既有 `NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor` 的新唯一 `naruto-physical-x190-336b44-wpoint-green-01` 请求在原 Battle Scene运行，未改探针或场景。结果 `OBSERVED_DIFFERENCE`、`EXITING`、50相对tick；`firstPickupTick=-1`，因此**没有进入本次WPOINT持有写者**，不能拿它判定修复成功或失败。原件为 `artifacts/diagnostics/NTSD28-Q07-NARUTO-PHYSICAL-PICKUP-PLAY-001/naruto-physical-x190-336b44-wpoint-green-01.json`。

同一物理J按键，旧2026-09-26结果 `naruto-physical-x190-20260926-e.json` 相对tick1已action115/link101并拾取；当前新版原Scene相对tick1 action60/link0、tick3 action511，武器保持frame64/link0。两次P1都记录canonical `Jump`，说明首个差异发生在拾取之前的动作选择/时序；**不能归因于仅在已经持有时执行的本包WPOINT位置改动**，也不能按旧结果推断当前物理键必拾取。旧测试证书仍按旧版/旧Scene记录保留。该入口的动作选择应作为独立Q07输入/状态首差审计，不在本包改按键、DAT或动作。

两个请求的出生角色/武器逻辑初态、mode0和输入初相位相同，但旧Scene SHA为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，当前Scene SHA为 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`；不能仅据两个版本化结果把动作差归因为生产回归。现有探针不带像素分支时也不为临时武器初始化源规则坐标（`pixelSourceBirthInitialized=false`），即便拾取也不足以验证本包的双源初始化投影分支。下一Scene证书须先具备正式可达拾取和武器源位置已初始化两个前置条件。

本轮原Editor完成有序退出、`sceneCleanAfter=true`，Battle Scene SHA前后均 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`；六保护文件重新计算6/6未变。正式Play的武器持有比例出口仍 `RUNTIME_PENDING`；24tick生成Editor/原Editor完整Driver聚焦结果单独计证。
