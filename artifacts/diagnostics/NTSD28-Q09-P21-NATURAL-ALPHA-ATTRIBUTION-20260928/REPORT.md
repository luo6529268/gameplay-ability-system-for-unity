# P-21 正式 PNG alpha 在自然 Legacy 战斗本体中的限定归因

状态：`VERIFIED_SCOPED_NATURAL_LEGACY_ALPHA_CONSUMPTION / FORMAL_SAME_VIEW_PENDING`。本报告只重读已验收的原 Battle Scene 香燐 OID314/state9997/tick8 本体开、关两张真实 World 相机 PNG 和正式 `c/kar/a/cha4.png`，没有运行新的 Unity Play、修改生产代码或改变资源。源图与 Unity 暂存图 SHA-256 相同；原始四个输入哈希及计算结果见 [result.json](result.json)。原 Play 的完整 Driver、自然子体、相机状态恢复及场景保护证据见 `NTSD28-Q09-P12-KARIN-LEGACY-GPU-PIXEL-001/ACCEPTANCE-20260928.md`。

按正式 DAT 帧50/pic60取 `cha4.png` 左上79×79单元。源单元有1,433个非零alpha像素，alpha最大33/255。两张1280×720相机图仅切换目标本体 `SpriteRenderer.enabled`，共有1,245个RGBA差异像素，左上原点边界为X459–514/Y464–532。以非零源alpha轮廓在X435–484、Y445–489内平移，并比较水平/垂直翻转四种方向，最佳是**不翻转、源单元左上落在屏幕(443,457)**：源非零alpha与相机差异像素交集1,180、并集1,498，IoU=0.7877169559。余下65个变化像素不在这张未滤波源alpha掩码下，不能归因于这里的简单逐点模型。

在上述1,180个交集像素上，用 `round(sourceRGB × sourceAlpha/255 + bodyOffRGB × (1 − sourceAlpha/255))` 预测开本体图，RGB平均绝对误差0.7624；336像素三通道全同，995像素三通道均在2/255以内，最大单通道误差23。若模拟旧的“非零alpha一律改255”处理，相同像素的RGB平均绝对误差94.1497，三通道均在2/255以内的像素为0。实际开/关图的最大单通道差为27，与正式素材低alpha的量级相符。这是实际自然本体画面消费正式半透明度的强限定证据，不是单凭文件哈希或测试专用纹理推断。

模型未解释全部像素，可能涉及采样/边缘混色或其它合成因素；这里没有证明原因，也没有取得正式根 EXE 同状态、同视口的图像。因此 P-21 的 PNG sheet 原alpha处理和**这一例自然 Legacy GPU消费**有证据，中央出口、更多角色/技能、正式根最终混色及 Q09/R17 整体仍开放。用户保留的背景、结果 UI、旧 BMP 和非战斗路径均未触碰。
