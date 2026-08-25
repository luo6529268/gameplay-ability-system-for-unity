# HANDOFF — R8-WP01D generic sprite mapping / D-RENDER-006

> 日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE / FULL CLOSURE BLOCKED`

## 2026-08-23 final boundary

- 01～07已完成当前资源允许的source/catalog/GPU/Game/SceneView限定范围S4；
- `B-R8-WP01D-08-01`：loaded DAT中authored state8000 frame=0，无法取得真实资源witness；
- `B-R8-WP01D-08-02`：R1-WP02 C++ full trace继续BLOCKED；
- 不修改DAT伪造样本，不运行/修改C++，不继续新增render probe；
- WP01E/F/G可独立继续；完整说明见`TASKS/R8-WP01D-08-synthesis-and-blocked-boundary.md`。

## Current

- 用户已批准 `R8-WP01D / D-RENDER-006`，并明确拒绝角色/技能/OID专项处理；
- C++ release只读source链已闭合：raw pic999 gate → `pic + unk_318` → declared range → local pic →
  row-as-horizontal source rect；loading/parser/renderer均参与release Makefile；
- Unity current DAT identity、catalog key、central binding和command都已有通用路径；
- first-difference：state8000 C++写`unk_318=140`，Unity错误写`HitStop=140`；Unity raw pic999还会先加offset；
- 现有self-check把错误HitStop写入作为正向断言，必须随production最小修复一起更正；
- 通用writer/raw-hidden与陈旧oracle已最小写入；尚未取得fresh Unity compile/self-check。
- 首次fresh compile为0 C# error，但10:11:07 full self-check在另一处GT-10旧HitStop140断言FAIL；继续搜索
  找到GT-11 chain/missing-target两处同源oracle，均已最小修正。需要再次fresh compile/self-check，首次FAIL
  不得被覆盖或表述为通过。

## Active Change

`R8-SPRITEMAP-007 / VERIFIED`（006已在Editor diagnostic与Game submission范围VERIFIED；005只裁决GPU S4；
001～004的production修复均不再扩大）

只允许修改：

- `LF2Entity.ApplyStateDataTransform`
- `LF2Entity.GetRenderPicIndex`
- `BattleRuntimeSelfCheck`对应source-derived fixture

不得修改任何角色/技能/OID专项类、DAT/BMP/scene/resource或中央渲染架构。

## Static matrix result

对仓库当前可直接读取的23个DAT file-range及BMP尺寸做了无写入矩阵：当前Unity grid heuristic在这些
定义上的最终横向列数与C++一致，`mappingDiff=0`；两项range大于物理grid属于partial sheet，现有Unity
按实际边界留hole。该结果只说明当前23项未从row/col产生first-difference，不证明所有运行时已加载DAT、
catalog、slice/UV或实际GPU像素正确。

## Next

1. `R8-SPRITEMAP-002 / CODE_WRITTEN`：首次Play已取得1301 generic differences；修正tick0 baseline与
   state8000 no-candidate分型后，在003完成后重跑；
2. `R8-SPRITEMAP-003 / CODE_WRITTEN`：C++ parser→loading→SpriteSheet→renderer证明DAT `row`固定为
   横向列；通用grid resolver与同源self-check已写，没有角色/技能/OID分支；
3. 002 baseline/state8000分型已fresh compile；003 compile 0 error且10:41:17 full self-check PASS；
   下一步clean Play重跑全DAT catalog/slice/UV/CentralOnly command；
4. 只有全DAT差异收口、真实Game/Scene视觉和必要GPU像素证据取得后，才能关闭D-RENDER-006。

003首次fresh compile已0 error；full self-check随后暴露flash parser fixture仍以col-horizontal构造synthetic
texture，已按同一C++合同修正并保留首次FAIL，当前必须重新fresh compile/self-check。

第二次全DAT Play中原1301 source-rect mismatch已全部清零、catalog 4933、cleanup 4/2恢复；剩余229项
来自declared range尾部且C++ rect完全落在source texture外，Unity不发布catalog entry与C++不可见等价。
probe现只对仍与source sheet相交的missing entry报差异并单列fully-outside计数，待compile/第三次Play。

第三次Play把60个fully-outside排除后余169个clipped引用。BMP只读像素复核证明各实际C++ rect的交集
只有黑色colorkey；绿色仅在separator列。probe已泛化为对任意missing entry扫描BMP交集像素，全黑计
等价不可见、非黑才报差异；无文件名/OID特判，待compile/第四次Play。

第四次Play仅余2个非黑可见missing entry，证明第三个通用差异：Unity错误用`row*col`限制localPic并把
partial rect留hole；C++以declared range允许localPic且blit裁剪source。`R8-SPRITEMAP-004 / PLANNED`
将只实现range-length、bounds intersection和adjusted pivot；现有dynamic Mesh已支持任意float pivot，
不需要改Mesh/Shader。003已以source mismatch=0提升`RUNTIME_PENDING`。

004现为`CODE_WRITTEN`：declared-range长度、source intersection、adjusted pivot、catalog explicit-pivot
overload与source-derived fixture均已写；002 expected rect/pivot同步。production没有任何证据对象特判，
下一步必须fresh compile、full self-check和第五次全DAT Play。

覆盖更新：004已fresh compile 0、10:59:06 full self-check PASS；final all-DAT Play为5537 entries、
23 clipped references、0 differences、cleanup 4/2→4/2。002/004现为`RUNTIME_PENDING`。focused
resolver/atlas/mesh job `608b9f8515a646fb97ecd2a5c36c4707` 29/29 PASS。

P8-C GPU Play报告中全部synthetic像素/array UV/order/chunk案例PASS；production case FAIL于旧harness的
逐实体GameObject/renderer前提，而当前正式架构使用logic-only materialization + central snapshot。
下一步`R8-SPRITEMAP-005`只新增Editor-only全catalog source→binding像素与统一GPU command probe，
不恢复Legacy owner、不改production、不按角色/技能/OID/frame/file处理。

005 final：232 source textures、30 array slices、5537 entries、84,327,319 pixels全部matched，
source/central hash=`8ECA0CBA6D4724D1`且同域重复一致，0 differences；final动态可见partial
450×5 / pivot(0.5,-28)通过正式resolver+dynamic Mesh，Legacy/Central 340/340、mean/max=0/0；
cleanup 4/2→4/2，focused `ecaf8255752e4515bbcc76787c61aba3` 35/35，11:37:22 self-check PASS。
005可在限定范围写VERIFIED；
D-RENDER-006整体仍待真实Game/Scene、state8000 authored witness与C++ full trace。

最新Game截图显示HUD/背景正常而战斗实体不可见，证明Game/Scene层尚未关闭。006只诊断全部active slot的
Entity/Shadow command、resource、segment、submitted与camera/URP环境；若发现production首差，必须另建
Change Record再修，禁止在006顺手修改或按具体对象处理。

006 final覆盖：第一次tick1空结果由延后到tick257的扩展证据取代。final为3 snapshots、6 source/resolved
commands、1 chunk/segment/draw，current immutable plan、worker/pending publication、submission与cleanup均正常，
`firstDifference=NO_DIAGNOSTIC_DIFFERENCE`。fresh
`Temp/R8-WP01D-06/R8-WP01D-06-game-current.png`可见角色、武器和阴影；因此不建立production repair，
不得凭旧tick1结果修改worker/central/URP/camera。11:53:16 full self-check PASS。

当前真正未关闭的是：180×936窄Scene viewport无法裁决logic-only实体的Scene View可观察性；loaded data没有
authored state8000 live witness；C++ full trace继续BLOCKED。三者均是证据/观察缺口，不是已经确认的新
production sprite mapping差异。

下一包007只验证第一项：production gate和existing test已经允许Play Mode SceneView，现新增Editor-only
probe，用真实SceneView camera、world-camera投影、空culling mask和透明RT隔离中央feature像素。若有像素，
关闭Scene View证据缺口；若无像素，只登记generic first-difference并拆production repair，不在007改renderer。

007 final：fresh compile 0、focused `9dfeda6b0663429a9caf20df64048fb9` 13/13；真实
`SceneCamera/CameraType.SceneView` gate/lease均true，tick2 current plan为4 source/resolved commands、1 segment；
960×540白底isolated SceneView render为575 non-clear pixels，hash `C292967D753744C2`，cleanup与12:05:47
self-check PASS。SceneView链没有production first-difference，先前空截图不可再用于修改renderer。

D-RENDER-006当前仅余：loaded data无authored state8000 live witness；R1-WP02 C++ full trace BLOCKED。

## First all-DAT Play audit

- 100 loaded definitions、232 ranges、4373 catalog entries、6674 authored frames；
- 6016 visible、658 raw pic999 hidden、272 authored range misses、0 referenced out-of-bounds rect；
- Central binding：4369 Texture2DArray、4 SourceTexture2D、0 ordered page；
- 1301 generic differences；记录的64条首差均为`CPP_SOURCE_DESCRIPTOR_MISMATCH`；
- 典型结构：DAT row=3/col=2时，Unity在localPic2已经换行，而C++仍位于第三横向cell；
- cleanup 0->4/0->2是probe在battle-ready前记录baseline，不是world leak；
- dynamic state8000 candidate未取得，下一版probe必须输出缺失阶段而不是混合成单一reason。

## First repair verification

- source 10:12:16 < `Assembly-CSharp.dll` 10:12:32；Console compile error=0；
- 10:13:22 full `BattleRuntimeSelfCheck=PASS`；
- self-check产生的两条既有negative registry error读取后已清空，最终Console error/warning=0；
- first repair只到source/compile/self-check，尚无all-DAT或Play视觉证书。

## Persistent boundaries

- C++ authority只读，不运行、构建、修改、复制或写入；
- R1-WP02 full trace保持BLOCKED；
- CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×、fixed-world、容量、30 Hz/FrameInputSet、
  SoA/ECS/pool/worker/0-GC均不可回退；
- T8默认stage.dat、Android、1000 AI、Player和服务器排除；
- WP01C-03～07仍独立，不能因本render包改变状态。
