# 不同描边共用材质：验证报告

2026-10-08，Change ID：TMP-VERTEX-EFFECTS-SHARED-20261008。

实际结果：run04 Unity 退出 0，在 Unity 2022.3.62f3 的 Built-in Editor 渲染两个 TMP 文本。左侧 outlineWidth=0.04 / 红色，右侧 outlineWidth=0.18 / 蓝色；它们绑定同一份材质，实际 CanvasRenderer 材质也相同。

证据：run04/verification.json、run04/shared-material-different-outlines.png、run04/unity-verification.log。

通过项：两份不同 UV4/Tangent；20 次效果修改后材质 ID 不变；一个使用者禁用时另一个继续使用，重新启用与复制对象继续共享；最后使用者禁用时适配材质释放；原始材质的描边宽度/颜色/图集保持不变。图片红色 481 像素、蓝色 4588 像素、白色 5722 像素，实际人工查看可见不同描边。

产品修改：TextMeshProUV 用共享适配缓存替换逐文本 fontMaterial/uniform 路径；配套 Shader 标记通过 Resource Material 的 GUID 引用准确识别；主 Pass 不设置 LightMode，允许 Built-in 使用其顶点效果，URP 按官方默认解释为 SRPDefaultUnlit。

编译：目标项目引用下，Editor 与 Player 条件单脚本 Roslyn 编译均退出 0。git diff --check 通过；ChangeLedger 验证通过（该工具只覆盖 Assets/NTSD/Scripts 与 Tools，TMP 修改在独立审计 Record 正文及 package-code-path 登记）。

历史：前三轮共享材质及网格参数已正确，图片没有彩色描边；诊断 Shader 证明 GPU 通道有效，移除主 Pass 的显式 SRP LightMode 后 run04 通过。由标签规则与修改前后图像推断，原先 Built-in 没有使用该顶点效果主 Pass；未通过 Frame Debugger 抓取回落 Pass。原始失败结果保留。

限度：未测当前目标场景的实际 Draw Call 数、URP 图像、Play Mode、Player Build 或设备。该结果不代表所有遮罩、字体图集及 Canvas 层级都能合成一个 Draw Call。
