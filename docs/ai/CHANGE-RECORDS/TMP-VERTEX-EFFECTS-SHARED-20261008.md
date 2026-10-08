<!-- CHANGE-RECORD
id: TMP-VERTEX-EFFECTS-SHARED-20261008
status: VERIFIED
code-path: NONE
change-kind: GOVERNANCE_ONLY
package-code-path: Assets/TextMeshPro/Scripts/TextMeshProUV.cs
package-code-path: Assets/TextMeshPro/Shaders/TMP_SDF.shader
authority: User explicitly requested different outline styles to share materials using the embedded non-official TMP vertex path on 2026-10-08.
evidence: artifacts/diagnostics/TMP-VERTEX-EFFECTS-SHARED-20261008/
-->
# TMP 逐文字效果共用材质

需求来源：本聊天用户要求修正 TextMeshProUV 的逐文字材质实例路径，恢复不同描边共用材质。

原状：ApplyMaterialProperties 访问 text.fontMaterial 并写入描边/膨胀/投影 uniform，绕过内置 TMP 的 UV3/UV4/Tangent 效果参数。旧 Shader 不读取顶点效果，因此仅删除 uniform 写入会再次导致描边失效。

实施范围：只调整上述脚本和内置 SDF Shader 的识别标记，增加 Resources/TextMeshPro/VertexEffectsMaterial.mat 及 meta 以确保 Player 保留明确引用的配套 Shader。同样更新 PoXiao 的 TextMeshProPortable 副本及其 README。原字体/材质资产和 Scene/Prefab 不需批量替换。

预期行为：配套材质直接复用；旧 SDF 材质按原始材质及 UI 深度模式缓存一份共享适配材质。每个文本的描边/颜色/投影保持在网格中。共享缓存按组件持有数释放，保留 Editor 实时刷新及原公开 API。

风险与依赖：Shader 必须与内置 TMP 的顶点布局一致；特殊非 SDF Shader 不进行转换。遮罩、字体图集及层级仍可能影响最终 UI 合批。这里不承诺整体性能提升或项目场景 Draw Call 数。

验收：静态无 text.fontMaterial 访问或逐文本效果 uniform 写入；两个不同描边的文本绑定同一适配材质、UV4/Tangent 不同、原材质参数不变；在隔离 Unity 2022.3.62f3 项目实际渲染并保存 PNG；目标项目窄编译及 ChangeLedger 检查。

回滚：按 docs/ai/FILE-OPERATIONS/TMP-VERTEX-EFFECTS-SHARED-20261008/RECORD.md 的事前备份作准确补丁恢复。本次保留现有 GUID，不执行 Git 回退、删除或移动。任何后续回滚遵守用户批准要求。

状态：IN_PROGRESS。尚无本次编译/渲染结果。

Validator 范围更正：Validate-ChangeLedger.ps1 仅将 Assets/NTSD/Scripts 与 Tools 列为 governed roots，第三方 TMP 路径不能登记为 governed code-path。首轮检查因此失败。此 Record 是外部依赖变更的治理审计，使用 code-path NONE / GOVERNANCE_ONLY；实际已改的依赖源码由 package-code-path 及正文明确登记，不表示没有修改 TMP 代码，不扩大 validator 的检查范围。

已写代码：逐文字 uniform 写入已移除，共享适配缓存及引用释放已写；Editor/Player 条件离线 C# 编译均退出 0。第一轮隔离 Unity 编译成功、相同材质/不同网格参数成功，但实际 PNG 没有彩色描边（red=0、blue=0），测试失败；原日志和结果保留，继续定位渲染材质绑定。

渲染诊断补充：run02 确认 CanvasRenderer 实际绑定配套 Shader；run03 的诊断 Shader 实际显示两份不同 UV4/Tangent 数据，GPU 传输正常。下一最小修正：内置 SDF 主 Pass 移除显式 SRPDefaultUnlit LightMode，使内置管线可以使用该主 Pass；URP 的无 LightMode 主 Pass 默认仍是 SRPDefaultUnlit（Unity URP 14 官方 Pass tags 文档）。此主 Pass 标签属于同一描边效果修复的必要依赖，Shader 数学及第二个项目特殊 Pass 不变。结果待 run04 实际渲染。

## 最终定向验证

VERIFIED 限定为本次共享材质行为及隔离 Editor 渲染。run04 的 Unity 命令退出 0，verification.json 所有行为项为 true：同一共享材质（ID -1282）/相同实际 CanvasRenderer 材质/两份不同 UV4 和 Tangent/20次修改效果不换材质/禁用启用继续共享/复制对象继承缓存/最后使用者释放/原始描边颜色与图集不变。实际 PNG 检出 481 个红色像素、4588 个蓝色像素和 5722 个白色像素，已人工查看图片，左细红右粗蓝。

编译：Unity bundled dotnet + csc，复用目标项目 Assembly-CSharp.rsp 的引用及条件定义，分别只编译 TextMeshProUV；Editor_EXIT=0，Player_EXIT=0。第一次命令构造的 -out 参数被 PowerShell 拆成多行，产生 CS2021，修正参数数组构造后上述两种模式均通过；它不是产品源码编译错误。

Unity 命令：unity run C:/Users/Logan/AppData/Local/Temp/PortableTMP_VertexEffects_20261008_01a0fe11 --editor-version 2022.3.62f3 --timeout 180 --no-banner --non-interactive -- -executeMethod TMPSharedMaterialVerification.Run -logFile <evidence>/run04/unity-verification.log -tmpEvidencePath <evidence>/run04。

实际源路径：Assets/TextMeshPro/Scripts/TextMeshProUV.cs 的 Refresh/EnsureSharedMaterial/ReleaseSharedMaterial；Assets/TextMeshPro/Shaders/TMP_SDF.shader 的顶点效果标记及主 Pass 标签；新增 Resources/TextMeshPro/VertexEffectsMaterial.mat 与 meta。对应便携副本和 README 已同步，保留 TextMeshProUV 及 Shader 的原有 GUID。

未运行：目标项目当前 NTSD_Menu 的实时 Inspector 操作、Frame Debugger 实际合批计数、URP 实际渲染、Play Mode、Player Build、设备测试。隔离渲染只证明 Built-in Editor 下该共享路径可见，不宣称跨所有管线/设备或整个项目性能提升。第一至三轮原失败证据完整保留。
