# NTSD28-BATTLE-SPRITE-RED-LINE-20261006
Status IN_PROGRESS
User screenshot reports a red horizontal line beneath Naruto; inspect actual DAT source rect and GPU sampling, fix shared confirmed display path. Preserve original PNG straight RGBA, DAT width/height/pivot and battle collision/movement/scale. No color-key red deletion, source image/DAT/Scene/importer/ProjectSettings/thirdparty/nonbattle edits. Initially diagnostic new Editor test/probe only; production path and backup added before any exact fix. Original saved Battle idle confirmed by user and MCP, no computer-use/second Editor/full roster/full suite. GPU before-after plus representative rect/alpha boundary checks and original scene targeted witness; owner-generated Mesh/Texture/RenderTexture only local finally cleanup or existing ordered shutdown, no runtime manager/world field/worker/shutdown stage.

Observed screenshot red row y191 x77..187111px RGB199/47/0. Current native source nar.png799x1600, authored separator RGB201/47/0 at topY79. Declared79x79 rect excludes row79 statically. Need actual GPU / live resource proof.

---

# 修复终验
Status: SCOPED_CROP_SAMPLING_FIX_VERIFIED

用户报告鸣人脚下红线，本批已复现并修复共用裁剪边缘采样问题，未改 DAT 或原图。

## 已确认原因

- 附件脚下红线y191、x77..187，111像素，RGB199/47/0。
- 正式内容nar.png799×1600，79×79单元间分隔线RGB201/47/0。DAT及运行时四个站立裁剪矩形均避开分隔线，框内此分隔色像素数0。
- 修前真实 BattleDynamicMeshBackend + 两个正式shader GPU实测，在目标Y16.5半像素位置采到框外红线118像素。原图、2048页图集、Texture Array全部复现。缩放仍用项目现有1.5。
- GPU边缘取样越过cell边界，不需要删除红色或改变DAT尺寸。

## 修复

BattleDynamicMeshBackend保留原几何、UV插值、pivot和视觉缩放，额外传入裁剪框内首尾像素中心的安全取样范围。BattleCentralTransparent及Array只clamp实际查图坐标，框内坐标保持，避免相邻cell。无有效texture/rect的几何夹具及未携带属性的Mesh保持既有行为；不写回战斗逻辑。

共用入口覆盖使用本后端的角色、武器、技能和其他战斗对象，没有角色ID特判。Vertex stride28→44字节，每quad额外64字节；既有零托管分配、分段/边界检查通过，未做移动设备性能认证。

## 实际验证

| 检查 | 结果 |
| --- | --- |
| 原Editor脚本编译、MCP Assets/Refresh | 通过，最终Editor assembly14:49:27 |
| 修前exact7 | CPU4通过，GPU3预期失败各118红线像素；job940837ed8f5a4db78a5f2c3854649d0c |
| 修后当前exact18 | 18/18通过；job0e92e1e8a29643589f7b53458ace4bd3 |
| 修后源图/页图集/Array及翻转GPU | 五case共105亚像素位置，框外红线0 |
| 单像素裁剪、正常红色、128半透明 | GPU混合符合原RGBA，没有去红算法 |
| 原有Mesh边界/分段/零分配 | 八项通过，旧断言未改 |
| 原saved Battle Scene01 | 自然启动，保留原角色/初态，tick5暂停；Naruto visual2/pic1，SourceTexture2D，stride44，中央提交已发生 |
| 原Battle实际mesh/material/texture GPU readback | 21亚像素取景位置，可见非空鸣人，框外红线最大0 |
| 退出 | 原11阶段正常关闭，objects/slots/borrowers0，Editor非Play/空闲 |
| 场景保护 | clean，SHA253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010前后同 |

命令入口：原Editor nativeMCP execute_menu_item Assets/Refresh；run_tests带上述三个精确class过滤；NTSD/Validation/Character Red Line Scene Probe。9004是发现总数，summary实跑18；没有全套/全角色，没有第二Editor或computer-use。

## 证据

- 修前GPU：gpu-mode0-offset10-c82bb7ad7bb345ecb6271ccb944b7ab7.png
- 当前修后同窗口：gpu-fixed-mode0-flipFalseFalse-offset10-1da477fb19254545aa7eb971708fbda5.png
- 原Battle实际Mesh：original-central-mesh-half-pixel.png
- 原场景：original-battle-scene-01.json
- 最终Editor：scene-final-editor-state-01.json / scene-final-active-01.json

original-battle-game.png来自Editor poll的ScreenCapture，返回垂直反转且带Editor控件的窗口内容，**不作为完整Game View验收证据**。原件保留，没有为了出图修改相机/Scene。原Battle实际Mesh GPU输出是本问题的运行证据；不宣称全相机、全角色、设备、正式版同帧画面全面一致。

## 更正与留痕

新增测试flipY参数与现有构造器不符，改用BattleSpriteRenderState；Scene probe漏namespace import已补。首次修后9项新检查通过，8项旧Mesh夹具因null texture失败，补兼容门后当前18全过。失败原件保留，旧测试断言未改。

Validator01失败为shader metadata：已有validator只识别Scripts/Tools的code-path；改用既有asset-path约定，shader职责和逐文件备份仍完整，validator未改。最终结果见下方审计追加。

操作记录docs/ai/FILE-OPERATIONS/NTSD28-BATTLE-SPRITE-RED-LINE-20261006，生产3文件改前字节备份/哈希、5053资源/Scene/config/settings保护清单齐全。未删除/移动文件，DAT/PNG/importer/Scene/ProjectSettings/InputAction/音频/非战斗未改，没有Git恢复/提交/push。

旧对齐目标保持用户确认的限定收尾；本独立问题不引出全角色或新总表任务。

Final audit: ChangeLedger02 PASS(exit0), git diff --check PASS(exit0), protected5053/5053 unchanged, production3 before-byte backups SHA match. Exact scripts/shader after SHA in operation after-manifest.json. Initial validator failure and failed test candidates retained. No edits after accepted18/Scene01.
