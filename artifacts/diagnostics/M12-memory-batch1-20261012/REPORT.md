# 图片加载内存第一批实施结果

已完成本次授权的源码修改、原 Editor 窄编译和聚焦验证。状态 FOCUSED_TEST_PASS；真实 Scene、P1、Android 和整进程峰值未验收。

Task: NTSD-M12-IMAGE-MEMORY-BATCH1-20261012  
Change: NTSD-M12-IMAGE-MEMORY-BATCH1-001  
Operation: OP-NTSD-M12-IMAGE-MEMORY-BATCH1-20261012

## 已改内容

1. 内部 native PNG 读取由 RGBA byte[] → Color[] → Color32[] 改为 RGBA byte[] → Color32[]；正式原图像素/透明度/上下方向不变。body 共享和逐声明、head/small、SPARK、WORDS、kill icon、match 图集重新绑定均走紧凑 PNG。PrepareBattleSheetPixels 直接复用该 Color32[]，统计不重复计入同一数组。公开旧 loader/legacy BMP 保留。
2. Atlas array 和有序页 Texture2D fallback 只申请一张托管 Color32[] 页，逐页清零、拼装、挤边和上传。2048×2048 RGBA32 页为 16 MiB；N 页的该类托管页数组分配量从 N×16 MiB 降为 16 MiB。Unity 自己的全部 array CPU staging、GPU 页面、失败 array 与 fallback 重叠仍存在，Apply(false,true) 后才交还 CPU 可读副本。
3. Native 整批预检在首个像素解码前核输入 SHA/header/dimensions，以实际 physical reuse ON/OFF 口径累计 body/UI/common、保留 Color32、clamp 单元、4 个并发 decode 工作区、上传暂存、旧 publication 与图集。Match rebind 在所有像素读取前检查所选 body 和 common，已驻留纹理不重复算新 source GPU；可读 GetPixels32 副本和 native 再解码仍计入。实际布局完成后，在图集分配前再次核预算；超限拒绝本批，保留旧 publication。

## 预算口径和剩余内存

当前工程暂定 resident 512 MiB / loading 768 MiB。它们是被检查的图片资源估算阈值，不是手机可用 RAM、操作系统杀进程阈值或经过真机证明的安全值；128 MiB / 419.4 MiB 原 job 权重未提升，旧 byte gate 仍 DEFAULT_OFF。

预算对 eligible source/clamp 在未知像素哈希时保守按每源一整页预留，再受既有 atlas capability budget 限制，实际 atlas 超预留拒绝。可能出现保守拒绝；完整大目录可在解码前被拒绝，本次没有加载全目录验证。

仍有：PNG decoder 的 RGBA byte[] 与 Color32[] 转换瞬间双份；完整源图 RGBA 与上传暂存；源 Texture2D 与 atlas GPU 并存；Unity/图形驱动内部副本和销毁延迟；已退休旧资源及托管 GC 延迟；legacy BMP/公开旧兼容 Color[]；Sprite/mesh/catalog 等元数据、借用的既有资产及非图片进程内存。检查覆盖本图片事务可推导的分配，不保证 Unity 或整进程物理峰值。未实施离线/构建前分页，因此超大单图本身仍然昂贵。

分配计算示例：1024×1024 PNG 可省去 16 MiB 的 Color[]；最终 Color32[] 仍为 4 MiB，decoder 的 RGBA 输出转换瞬间仍约 4 MiB。这是源码数组尺寸计算，不是实测 RAM 降幅。没有新性能/ABBA数字，也不把旧数据晋升到本版 SHA。

## 实际验证

- 原项目 Unity 2022.3.62f3，同一已有 Editor，经项目既有 stdio transport 6402 连接。HTTP 8080 不通；没有启动第二 Editor、安装/修改包、执行动态 C# 或调用 P1 菜单。
- 窄编译：初次新脚本尚未导入导致 3 条 CS0246，原件保留；只导入该新脚本后完成编译/reload。最终 review 补 match 路径后，新测试缺 namespace 的 CS0246 保留并限定修复；修正后新 Runtime/Editor DLL 与 domain reload 确认，Console 0 error/warning。
- 第一版独立 10/10 聚焦通过；review 后最终独立 12/12 通过，0 failed/0 skipped，job 18cd851285b64abca719b67b09f4760e，执行时间 1.569527 秒。测试 inventory 10754 是发现数量，实际仅执行这 12 个用例。
- 用例覆盖 RGBA/透明 RGB、palette/tRNS、方向及旧转换一致；长度/同长度字节变化拒绝；跨页透明清零/挤边与旧 oracle 一致；实际 array 和 fallback 上传；累计/重复实例/UI CPU/clamp/旧资源重叠/溢出/loading/实际 atlas 超预留拒绝；rebind 只计新副本与拒绝发生在 pixel provider 之前。
- 最终 Editor idle/nonPlay/noncompile/nontest；原 NTSD_Menu active/loaded/clean/root8。Console 0 error/warning；两 Scene 和 ProjectSettings/EditorSettings 字节不变。
- 2353 个保护路径逐项 bytes/SHA 无漂移，含原图/DAT和现有无关 dirty；1869 个旧 artifact 只核 size/mtime，均无漂移。原诊断/Q/F/P1 测试文件的全部旧 135746 字节 prefix 与 d408f359324adedea3304ffc0b3c4b378f548872a27220cf133fb04336098bfa 完全相同。
- ChangeLedger validator 另记实际返回结果。没有运行 Play/P1、旧 Q/F、完整 SelfCheck/全 suite、Player、Android、Profiler 或构建前分页。

## 原件索引

- before-manifest.json / rebind-review-01/before-manifest.json / closeout-preimages.json：真实改前备份及治理前件。
- implementation.diff：第一版最小增量；rebind-review-01/implementation.diff：重新绑定补齐增量；final-source.diff：最终相对本任务原状的源码差异。
- compile-observation-01.json / new-script-import-01.json / compile-observation-02.json / compile-final-identity.json：初始缺 import 与成功编译分别保留。
- rebind-review-01/compile-final.json / compile-correction.json / compile-correction-final.json：新增 test namespace 错误、修正和最终编译身份。
- focused-result-01.json / rebind-review-01/focused-result.json：各版实际每用例结果。
- editor-after-final.json / protection-after.json / git-final-audit.json / after-manifest.json：最终 Scene/Console/保护/实际字节证据。

## 准备阶段纠正

首次备份范围过宽，曾只为 SHA 读取历史 artifact 内容（含旧 raw），没有输出、解析或重播内容。已停止该脚本，未在那次中断前修改源码/治理；后续保护改为旧 artifact 元数据检查及准确源/资产 SHA。此边界错误保留在 preparation-correction.json，未追溯批准。普通字符串定位/编译导入/namespace 及 PowerShell validator 命令错误均保留，不伪称原次成功。

## 历史准备占位

# NTSD-M12-IMAGE-MEMORY-BATCH1-20261012

IN_PROGRESS; preimages verified, implementation pending.

## 最终治理校验回执

Tools/Validate-ChangeLedger.ps1 在 bundled PowerShell 7 下实际执行，exit 0 / PASSED；全日志保留 4433 条 WARNING，未顺带清理历史记录。Git 最终增量无范围外路径、暂存为空。证据 validator-final.json / validator-final.log / git-final-audit.json。
