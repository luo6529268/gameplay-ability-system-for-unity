# Q09/P-12 state9997 香燐 owner 分支：原 Battle 中央命令复验

状态：`SCOPED_CENTRAL_PLAY_PASS / LEGACY_AND_PIXEL_PENDING`。这是原 Unity Editor 的生产完整 Driver 定向验证，不能据此关闭 P-12/Q09。

在原项目唯一 Unity 2022.3.62f3 Editor PID11944 的 clean `NTSD_Battle.unity` 中，复用既有可选 `NTSD28Q09KarinState9997BattlePlayProbeEditor`。旧 `Temp/NTSD28_Q09_Karin9997_20260928_03.request.json` 已逐字节另存 `karin-request03-before-postfix.json`（SHA-256 `74CE768DDB675CC09BE1D36C36B4F58230D972C134C088C6410E698F14D601C2`）；只在其固定请求路径提交新 runId `karin-x500-state9997-body-postfix-04`，请求原件另存 `karin-request03-postfix-submitted.json`（SHA-256 `94A7C9F9B2D227B2B26CDCC2061A4827B78755FA70C89E94CBE8C2D9D6F7332B`）。没有覆盖旧结果或修改探针 C#。

探针在正式所选 LoganRuntime 内容、项目自有 mode0 etc-mode1 与 CentralOnly 下，暂停原 Battle Driver 后于空闲槽8建立正式香燐 OID77/action415、owner8、物理左向夹具。完整 Driver 第3步（world tick8）自然生成 OID314/action50/state9997/owner8、slot50；冻结展示帧仍 `snapshotFlipX=true`，中央提交的实体 body 命令变为 `commandFlipX=false`（显示右向）。这是修改前相同夹具曾观察到 `commandFlipX=true` 的朝向首差的定向复验；没有修改逻辑朝向。`commandX=-4.309965133666992` 世界单位，按该轮活动视口 `Left=-11.997465133666993`、`UnitsPerPixelX≈0.01` 换算为展示 pivot X≈`768.7500`；角色物理 X≈`768.1920`，源规则 X500。活动可见展示范围约`[-3.25346,2044.74658]`，项目可行走范围另计，不能用其替代视口。

原始探针结果 `artifacts/diagnostics/NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-x500-state9997-body-postfix-04.json` SHA-256 `61B0560BD539BEB433C291EFA10743566BD6EE77FD24C2D3F813BE9B502288CF`，状态 `NO_FACING_DIFFERENCE`。`assetEtcMode=catalogEtcMode=1`，二者指纹相同；中央命令已提交、childPublished=true。探针回收夹具与子体，World对象/槽/池借用从4/2/2恢复4/2/2，暂停状态恢复。结束后 Editor 回到 idle/non-Play，唯一加载的 Battle Scene `isDirty=false`；Battle/Menu磁盘SHA分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`。

限定：探针在位置/方向可控的正式角色帧起点插入夹具，随后走完整生产 Driver；不是自然玩家按键入招、正式根EXE同画面或 Legacy SpriteRenderer/GPU目标像素。它证明 owner 有效且 etc-mode1 的中央命令朝向首差已消除，并显示逻辑朝向仍左。无 owner 的 OID998 fallback、mode0/无效 owner 的原 Battle 负例、Legacy和像素验收继续开放；不要重复旧 Q07 普通复活结果矩阵。

变更账本验证 `Tools/Validate-ChangeLedger.ps1` exit0，限定文档 `git diff --check` exit0；详细输出保存在同目录 `validate-after-central-play.log`。这是静态审计，不代替上述Play证据。
