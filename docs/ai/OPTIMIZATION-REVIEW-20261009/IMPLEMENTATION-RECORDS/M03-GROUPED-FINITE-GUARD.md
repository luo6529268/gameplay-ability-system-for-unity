# M03 分组有限值检查：代码前后与有限评估

**2026-10-10：本版不采用，默认OFF，COMPLETED_NO_ADMISSION。** 实际候选13/13正确性/容量/异常/恢复通过，完整Build唯一8窗256scope分配事件0；2000quad收益门失败、4097通过，联合未达事前门。Change NTSD-M03-GROUPED-FINITE-GUARD-001 ABANDONED，M03父项开放。未删除候选或原件，未承诺生产收益。

## 原来是什么

任务前真实dirty备份中的 BattleDynamicMeshBackend.BattleMeshChunk.Upload 在native mesh变更前逐顶点扫描Position.xyz。一个quad四顶点共12个分量，WriteQuad实际重复写入left/bottom/z/top/right五个标量。

```csharp
for (int vertexIndex = 0; vertexIndex < activeVertices; vertexIndex++)
{
    Vector3 position = vertices[vertexIndex].Position;
    if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
        float.IsNaN(position.y) || float.IsInfinity(position.y) ||
        float.IsNaN(position.z) || float.IsInfinity(position.z))
    {
        throw new ArgumentException("Battle mesh vertex positions must be finite.");
    }
}
```

改前来源：docs/ai/FILE-OPERATIONS/NTSD-M03-GROUPED-FINITE-GUARD-20261010/before/Assets__NTSD__Scripts__Animation__Rendering__BattleDynamicMeshBackend.cs（SHA256 f003c6d15ad6271e7d24ac84c2f70c9b5a328eabed40f40658416a5c7eb75bcf）。它包含原有未提交UV候选，不能用HEAD替代。

## 改成了什么

实例冷属性 GroupedFinitePositionValidationEnabled 默认false，OFF保留上面的原循环；ON在同一Upload位置逐quad检查已经写入staging的五个独立值。核心读取为：

```csharp
for (int vertexIndex = 0; vertexIndex < activeVertices; vertexIndex += VerticesPerQuad)
{
    Vector3 position = vertices[vertexIndex].Position;
    float top = vertices[vertexIndex + 1].Position.y;
    float right = vertices[vertexIndex + 2].Position.x;
    if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
        float.IsNaN(position.y) || float.IsInfinity(position.y) ||
        float.IsNaN(position.z) || float.IsInfinity(position.z) ||
        float.IsNaN(top) || float.IsInfinity(top) ||
        float.IsNaN(right) || float.IsInfinity(right))
    {
        throw new ArgumentException("Battle mesh vertex positions must be finite.");
    }
}
```

五值均来自已生成顶点，所以仍可拒绝有限输入运算溢出。检查点不前移，保留异常发生前已成功chunk的native上传语义。WriteQuad、资源、stride44、UV、索引和native上传段不改。只有一个backend owner bool；已有第5阶段Clear及Dispose置false，无新cache/queue资源。

## 解决了什么、为什么未采用

数学上消除了矩形重复坐标检查；真实OFF/ON三绑定、两绘制模式、完整44byte输出、边界/封印容量、NaN/Inf/算术溢出、跨chunk失败和Clear恢复13case全部通过。

完整Build（包括native上传）测量与局部scan不同。两布局各OFF/ON/ON/OFF、每窗8warm+32sample，可靠前后校准的256scope全0分配事件。2000quad第二ON均值2.433021875ms高于较快OFF2.401609375ms，最坏P95/P99为3.5605/3.5987ms，也高于OFF2.5703/2.8655ms，因此失败；4097布局三门通过。固定合同要求两个布局联合通过，故本版结束采用，不重跑找PASS。不能将先前局部scan差值当完整Build或游戏提升。

## 保持与限制

完整payload、publication字段和容量均不变；异常类型/消息/失败chunk native不变，前chunk已完成API计数相同。不开启旧UV候选。私有矩形writer不变量是前提；反射只改v3.z的任意损坏负控可让分组算法漏检，未来新增writer须重评，不能当作任意顶点数组验证器。

复用未改writer/上传/shader的旧prepared像素证据，未重跑像素。未运行完整千人Driver、120+180/120+1800阶段验收或Android/GPU。零event不是byte量/回收停顿，也不覆盖整游戏。源/程序集6指纹稳定，2442保护无漂移，10备份有效，原Editor Menu clean8。

## 恢复与审计

- [报告和原始结果](../../../../artifacts/diagnostics/NTSD-M03-GROUPED-FINITE-GUARD-20261010/REPORT.md)
- [Task](../../TASKS/NTSD-M03-GROUPED-FINITE-GUARD-20261010.md)
- [Change](../../CHANGE-RECORDS/NTSD-M03-GROUPED-FINITE-GUARD-001.md)
- [Operation](../../FILE-OPERATIONS/NTSD-M03-GROUPED-FINITE-GUARD-20261010/RECORD.md)
- [单项方案](../../../../Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md)
- [唯一进度](../../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)

下一只审阅此次不采用的证据。任何新方案或扩大范围由用户决定；本记录不启动后继专项。
