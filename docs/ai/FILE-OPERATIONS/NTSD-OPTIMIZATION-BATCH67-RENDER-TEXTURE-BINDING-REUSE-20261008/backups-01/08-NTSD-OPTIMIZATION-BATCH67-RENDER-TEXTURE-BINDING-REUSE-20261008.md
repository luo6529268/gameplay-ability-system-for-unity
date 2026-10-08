# 第67批：现有中央RenderPass纹理属性准备复用资格

状态：READY / IMPLEMENTATION_NOT_STARTED / GAIN_UNPROVEN。仅第66批NO_GAIN后的下一必要有界判断；未写C#、未编译/测试/测量、不增加45已执行批数，不宣称收益或阶段完成。依据有效六项合同0—8节、[66场内证据](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH66-COARSE-PROOF-REUSE-20261008/NEXT-ACTION-FINDINGS.md)。本项属于H07现有中央提交CPU路径的候选资格，不重开已通过的M03评估/Foot交付。

## 假设与排除

BattleRenderFeature.BattleRenderPass.Execute当前每有效segment都Clear/SetTexture同一MPB然后录制DrawMesh。只考察连续相同texture-property key（BindingMode与实际绑定Texture引用）能否复用已准备的相同MPB；每segment原mesh/submesh/material/pass/Matrix/命令顺序及DrawMesh数量不变，Material切换不改变本块仅纹理属性的值，仍必须验证。TextureArray identity不能用slice代替，slice继续由原顶点表达；SourceTexture2D与AtlasPageTexture2D继续使用实际segment.Texture，mode变化保守重建。

源码重复操作不是已测大热点，实际重复率/CPU收益未知，更不能推导GPU draw/SetPass下降或逻辑P95<33ms。本项不做兼容run合批、跨chunk/segment合并、GPU instancing、资源布局/bank/格式、dirty上传、插值/排序/first-visible修改或EXT1专项M0；PERF/ATLAS/Mono/EXT1状态保持。Role-aware推广仍未授权，H06及其已完成评估不重开。

## 实施前准确范围

另建Change NTSD-OPT-H07-RENDER-TEXTURE-BINDING-REUSE-067、独立Operation及初始dirty副本/逐保护清单，不能沿用66副本。

1. Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs：默认false诊断opt-in；将原segment MPB准备集中成可验证的最小helper，局部state仅同一次Execute存活，首次/Texture或mode变化必须Clear+SetTexture，相同准备值可复用。每次Execute重置，不跨camera/slot/lease/CommandBuffer缓存；Foot及Health保持原分支、binding和order。只必要primitive资格计数，不改CPU/GPU lease释放或CommandBuffer owner；准确新符号在script前Record冻结。
2. Assets/NTSD/Scripts/Test/Editor/BattleCentralSameSamplePixelEditorTests.cs：在原现成纹理/quad oracle/MPB/RenderTexture夹具新增SegmentTextureBindingReuse_具名test-first；不能改弱原断言或重跑全部历史。保留原测试及输出地址，新测试输出唯一67目录。

无Query/Driver/Suite/Observer/Harness/Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server/并行TMP写域。Q06仅状态/hash，不读方法体。需要实景接线时必须另建准确后继Task/Change及副本，本Task不自动授权其它脚本。

## 必要验收与成本判断

- 默认OFF；第一有效segment必准备；相同key复用、actual Texture身份变化和所有binding-mode转换重建；非法segment沿原skip且不新增draw；不同material但同texture、Foot→body→Health、下一camera/slot/CommandBuffer都保持完整属性值，不遗留另一texture-property槽。
- 保持每physical segment的mesh/submesh/material/pass/Matrix/order/draw数；利用现成独立quad像素oracle，仅实际受本helper影响的三binding modes/两draw modes A/B，固定同publication样本。不读取活跃排序器实现；Q06排序/first-visible全域未知继续待确认。
- 先反射/接口缺失的真实RED，再最小GREEN；固定用例数在Record写脚本前冻结，0case/编译失败不算RED。只新检查及直接影响旧检查，不重测66/no-gain、其它模式或全历史。
- 唯一局部CPU命令录制成本对照：固定相同有效segment顺序/数量/几何/材质/纹理，重复绑定和交替绑定两个固定pattern，双方DrawMesh相同；每侧4warm+8sample balanced、保存全部raw样本和实际绑定准备次数，不执行GPU capture/Profiler/专项M0。不能把只测helper或少录draw当整段收益；局部结果非实际AI/实景FPS/0GC认证。若成本不清或无收益则NOT_ADMITTED，不重复找PASS。
- 只stack/primitive状态、现有MPB/CommandBuffer复用，无新Texture/Sprite/材质/实例buffer/数组/owner或热扩容。租约与十一阶段owner不变，Execute返回/CPUlease0不证明GPU完成。局部test不关闭原H11完整相机0GC、61字节预算UNKNOWN或H07千人正式33ms/drop0门。

出口仅SCOPED_CORRECTNESS＋LOCAL_GAIN_SIGNAL或NO_GAIN/NOT_ADMITTED，不是H07完成。保留失败/无收益原件，原Editor安全窗口/import/同job/源码指纹/Scene unchanged/ChangeLedger及准确回滚审计必须通过。真正更大范围方案另授权，不借本项解冻专项或默认切换。
