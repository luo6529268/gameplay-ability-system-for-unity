# M03 段内纹理尺寸复用：实现与有限评估

> **2026-10-10 M03段内UV尺寸候选有限任务完成：本版不采用／默认OFF。** NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001 ABANDONED；原Editor8/8＋补充5/5（含恢复加强与真实prepared三绑定像素），唯一完整Build16窗/512有效校准scope事件0，载荷/容量/发布不变；Source2D门PASS、array均值/尾部门FAIL，两负控PASS，联合收益未过，不重测/推广。Task NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010；报告 artifacts/diagnostics/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/REPORT.md。M03父项/H07/Android仍OPEN，H07输入/producer暂缓、95OFF、H06不续/旧Goal取消保持。下方为此前时点。


## 问题与原代码
同一prepared segment每有效quad原来都读Texture.width/height。微测只提示native调用可减少，非生产收益。改前片段取自Operation/before实际dirty字节：
```csharp
                if (resource.Texture != null && uv.width > 0f && uv.height > 0f)
                {
                    float insetU = Mathf.Min(0.5f / resource.Texture.width, uv.width * 0.5f);
                    float insetV = Mathf.Min(0.5f / resource.Texture.height, uv.height * 0.5f);
                    uvSampleBounds = new Vector4(uv.xMin + insetU, uv.yMin + insetV,
                        uv.xMax - insetU, uv.yMax - insetV);
                }
```


## 新代码
只利用既有canAppend；首quad原路，首有效append准备两个int，后续同段复用：
```csharp
                            if (canAppend && reuseSegmentDimensions &&
                                resource.Texture != null && resource.NormalizedUv.width > 0f &&
                                resource.NormalizedUv.height > 0f)
                            {
                                if (!segmentDimensionsReady)
                                {
                                    segmentTextureWidth = resource.Texture.width;
                                    segmentTextureHeight = resource.Texture.height;
                                    segmentDimensionsReady = true;
#if UNITY_EDITOR
                                    if (countUvDimensionReads)
                                        lastUvDimensionReadPairs++;
#endif
                                }
                                preparedWidth = segmentTextureWidth;
                                preparedHeight = segmentTextureHeight;
                            }
```


WriteQuad共用geometry/vertex/UV公式，尺寸来源改为prepared int或原getter。局部每Build失效；实例默认OFF、Clear/Dispose关闭，无跨Build缓存或新关闭阶段。

## 为什么这样改、解决了什么
连续4096quad实际尺寸读取由每quad一组减到2组，4097跨chunk为3组。完整native顶点44字节载荷、index/submesh/segment/bounds/diagnostics及publication等价，三绑定像素30次/276480像素误差0。机制成立，不直接推出完整Build收益。

## 实际成本与结论
唯一四布局ABBA，4000quad、8warm+32sample/窗、512完整Build scope0事件、可靠前后校准有效，容量/Mesh/publication保持。Source2D ONmean4.797206/4.776253低于OFF4.948259/4.978794；array ON1 4.987853高于OFF4.757259/4.917888且maxON P95/P99超OFF，联合收益门FAIL。两个负控通过，Strict更快不归因候选。本版不采用/默认OFF，不改判据、不重跑或自动改为source-only。

## 行为与完成边界
prepared长段、slice例外、page/texture/material/binding/unresolved断段，Strict/custom、invalid/null/destroyed、中途及跨Build resize/replacement、0/1/4096/4097、大到小/空、native Mesh恢复、容量/NaN异常、Clear/Dispose已覆盖。加强恢复明确4097 resolved，替代旧弱同名覆盖。像素反序负控有效，像素测试不称0GC。

有限候选评估结束，不代表准入或M03父项关闭。无生产默认、实景/Driver/1800/Android/GPU证明，H07输入约12ms仍未解决。下一仅独立审查报告。

## 证据与审计
- [报告](../../../../artifacts/diagnostics/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/REPORT.md)
- [原始成本](../../../../artifacts/diagnostics/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/full-build-cost.json)
- [Task](../../TASKS/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010.md)
- [Change](../../CHANGE-RECORDS/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001.md)
- [改前备份和操作](../../FILE-OPERATIONS/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/RECORD.md)
- [单项方案](../../../../Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md)
- [唯一进度](../../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)
