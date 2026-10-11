# H08 同publication源图共享：代码对照与验证阻塞

**2026-10-10：BLOCKED_VALIDATION_HARNESS，候选默认OFF；内存减少未测。** 首组8/8、补充1/3、空管理器入口诊断1/1各自保留，不能合并为全部PASS。

## 问题及改前来源

父项H-08；CharacterAnimtorManager.LoadCharacterSpritesCoreAsync按每声明调用ProcessAndCreateSpritesForCandidateAsync，同一路径重复verified decode、像素处理、Texture上传和sourcePixels。当前330定义、773有效引用、703路径、56重复路径已实查；184792652B是header RGBA32重复payload估计，不是本候选实测节省。

真实改前字节位于[Operation备份](../../FILE-OPERATIONS/NTSD-H08-NATIVE-SHEET-SHARING-20261010/before/Assets__NTSD__Scripts__Animation__Manager__CharacterAnimtorManager.cs)，SHA256 e90facd3e95f815f9266d124086096032b9796c91d4adc5987857901dcf93f99。完整局部diff见报告目录，不以HEAD替代dirty来源。

## 真实代码前后

改前协调入口（片段）：

```csharp
foreach (var (characterId, fileInfo, ownedEffectivePics) in allFileInfos)
{
    if (native != null && !CanCompleteSpritePrewarmInvocation(invocation)) break;
    if (native != null && native.Candidate.IsMissingBodySheet(fileInfo.filePath))
        continue;
```

候选ON协调入口（片段）：

```csharp
if (native != null && native.ReuseNativeBodySheets)
{
    foreach (NativeBodySheetGroup group in BuildNativeBodySheetGroups(allFileInfos, native.Candidate))
    {
        if (!CanCompleteSpritePrewarmInvocation(invocation)) break;
```

实际分组键：

```csharp
string path = BattleAtlasLayoutPlanner.NormalizePath(Path.GetFullPath(item.fileInfo.filePath));
string hash = candidate.GetImageSha256(item.fileInfo.filePath);
// All members belong to this one captured native source and processing policy.
var key = (path, hash);
```

新增实例诊断bool默认false，operation开始时快照。ON按完整规范路径和expectedSHA，在同一加载内合并源纹理/processedPixels；每声明保留独立grid、rect、pivot、ownedPic、clamp和Sprite。OFF/legacy保留原路，抽出的共同Sprite循环与原文本一致。不存在跨publication缓存。

## 为什么这样改、已解决与未知

源文件相同不等于Sprite裁切相同。仅共享整图可以消除重复源图创建，同时继续独立裁切。实际小夹具worker/多definition coordinator已命中共享，PNG/BMP/legacy与几何、catalog、像素、ownership、取消和部分lease回收门通过；三种绑定真实绘制子断言通过。

但当前内容四组内存对照没有启动，不能给出实测内存/总驻留/峰值/CPU提升数字。旧Apply(false,true)和atlas按path去重已存在，本次没有把它们重复算成果。

## 保持的行为与失败边界

HashSet owner、取消staging、generation/原子发布与最后lease退休保留；共享解码失败影响全部成员声明，声明裁切失败计个数且本组不物化。新增组表/plan只有加载调用寿命，容量由引用/组成员数界定；phase1取消、workerJoin后phase5回收沿旧链，顶层十一阶段不改。

补充测试在DestroyImmediate后开关复位断言失败，随后的已加载sheet/atlas释放断言未执行。空管理器OFF/ON对照均未收到EditMode回调，显式调用原OnDestroy两次才得到disposed=true/flag=false/staging0。这定位了夹具入口问题，不能替代真实销毁证明；此前coordinator最后一代publication是否全释放仍未知，Scene clean不等于native对象零残留。

另一个missing-body测试在加载器前因Temp/NTSD28PngAlpha/fixture.dat缺失中断。其行为门未验，不认定生产规则错误，也未恢复/清理旧数据。

## 完成条件及下一步

原合同要求正确性、关闭、四组实测资源数量/对象内存联合通过。本轮不满足，保持默认OFF并具名停止。后续只建议先审查自包含缺图夹具、可触达生命周期入口和精确资源清理方案，再决定恢复本任务；不拿反射调用替代真实关闭、不删原失败、不重复内存组找PASS。

H08父项、H07/Android最终门未关闭；旧Role/FindEffect、M03候选OFF、H07/producer暂缓和95OFF保持。无Scene/资源/Settings写入、Q06读取或旧Goal恢复。

## 审计入口

- [完整报告与实际命令、原始结果](../../../../artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-20261010/REPORT.md)
- [Task](../../TASKS/NTSD-H08-NATIVE-SHEET-SHARING-20261010.md)
- [Change Record](../../CHANGE-RECORDS/NTSD-H08-NATIVE-SHEET-SHARING-001.md)
- [Operation与真实dirty备份](../../FILE-OPERATIONS/NTSD-H08-NATIVE-SHEET-SHARING-20261010/RECORD.md)
- [H08单项卡](../../../../Assets/NTSD/Docs/android-mobile-readiness/h-08-prebaked-visual-content-memory.md)
- [唯一进度](../../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)
