# NTSD28-336B44-Q12-REPRESENTATIVE-MATRIX-001

2026-10-04 最新裁决：第五代表出口的“同冻结版退出后重进”子门已由[独立原 Scene 两轮见证](../../../artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001/REPORT.md)限定通过，第一/二轮 live tick 419/256、各 2 名角色，退出后 Scene Driver.World 空、活动池对象/sprite 0、Scene clean；六个保护身份全程相同。先前 Dynamic＋临时失焦路由合取也已实际失败并精确回滚，属于合成输入诊断，不是战斗规则首差或重进前置。下面旧“合取尚未运行/重进待输入”的段落是历史快照，以本段和子门状态表为准。Q12 五个代表出口（第五项拆为退出与重进）已有各自**限定**证据；正式 EXE 同帧 Present 像素、真人手按、a7 自身实播及任何未覆盖角色/模式仍未知，不能把本 Task 状态解读成“游戏表现完全一致”。只由可复现的非例外首差触发后续定向项，不再安排全套案例。

状态：`SCOPED_REPRESENTATIVE_PASS / FORMAL_PRESENT_UNKNOWN`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；唯一调度入口为 [336B44 对齐总表](../../../Assets/NTSD/Docs/ntsd28-logan-336b44-vs-unity-battle-alignment.md)文首。此包是去重后的最终集成验收，不把 50 份未关闭 Change Record 重新展开为 50 次 Play。

2026-10-04 证据复用复核：自然鸣人首253后合成物理 J 由原 Scene 生产输入/完整 Driver 在下一 tick 转301，正式源码从防御消费起动作/PP/combo1/相位132/132同；该例暴露并修正了 Q07 测试入口造成的重复三键换算。原 Scene 运行内四SHA同、非Play/clean。这是合成键盘事件，不是人手或可见技能时差验收。已有 C053 受控40 tick五槽1136/1136及随机状态/调用/索引200/200、C040 原 Game View 第25 tick与规则640/640配对、078正式战斗 cue 的生产 AudioSource 播放阳性，分别复用为战斗、画面和声音的限定样本；各原报告的初态、版本及未知边界继续有效。a7自身实际播放没有证据，作为内容相关的条件触发项保留，不再为了重复证明共用播放器而必跑。独立有序关闭已有 C056 双子体原 Scene 零残留证据；同冻结版退出后重进已由两轮独立见证限定通过。后续方向 UI 共享写者的[影响复核](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DIRECTION-WRITER-IMPACT-20261004/REPORT.md)未发现新首差，但未重跑改动后的完整鸣人序列。

同日重进尝试：[原件与安全停点](../../../artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-ATTEMPT-20261004/REPORT.md)。首轮原 Scene Play 的合成 L 已排队，但30 tick 的 `FrameInputSet` 全0，八次有限脉冲失败；这是设备→战斗包前的不可比输入，不是战斗规则首差。停止 Play 后即时 Scene clean/四SHA稳；有界重试前 Scene 后来变 dirty 且 Battle 磁盘SHA变化，安全断言拒绝第二轮。没有保存/覆盖/回退 Scene，也没有证明同冻结版重进。需待 Scene 保存且闲置后重冻身份，先确认合成 L 真进入战斗包再继续。

Q12后继[输入首断点诊断](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DYNAMIC-INPUT-DIAG-20261004/REPORT.md)：单独显式Dynamic更新仍30tick输入包全0；键盘L在16tick呈pressed，P1 Defend Action持续`Waiting`、本地held=0。合取临时失焦路由的诊断代码已编译/原Editor导入，但两次菜单均因Play非活动前置退出，`focusSettingsApplied=false`，尚无合取运行结果。同期另一路BattleControls Scene序列化改动改变磁盘SHA；保留并行工作，只有Editor独占/Scene clean稳定后再跑一次合取。重进仍`PENDING_INPUT_CHAIN`，不打开其它角色/资源任务。

后续用户保存Scene后，第二次相同原Scene Play仍30tick输入全0；临时Editor探针失焦策略/绑定键盘试验第三次也全0，尽管记录设备ID与焦点设置恢复阳性。试验脚本已精确撤回，[Change](../CHANGE-RECORDS/NTSD28-336B44-Q12-UNFOCUSED-PHYSICAL-PROBE-001.md)为`ROLLED_BACK`，不能把假设提升为生产修复。当前真正依赖是只读定位事件→P1 Action回调→`CharacterInputModule`缓冲首个断点，然后才决定重进如何继续；不再盲跑同一探针。

权威与冻结边界：正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、对应 playable live source 和正式非排除战斗资源；Unity 以当前原项目 `NTSD_Battle` Scene、生产 Driver、LoganRuntime 战斗内容及项目自有地图/模式 Asset 运行。每次运行前记录相关脚本程序集、Scene、模式 Asset 与正式资源身份；版本或 Scene 状态变化后不得把两次结果称为同一冻结版。

仅保留五个代表性出口：

1. 鸣人自然防→前→跳→螺旋丸后续攻击，用既有原场景合成键盘设备探针记录物理动作映射、`FrameInputSet`、正式 proxy 采样相位、动作与 PP。首 253 成功窗口是一条正例；较晚窗口仅在本条正例或玩家复现出现首差时扩到必要边界。合成设备不冒称真人手按。
2. 一条当前正式根/源码与 Unity 同内容、同 seed、同输入和同 tick 的战斗结果/对象链；只比较该链实际消费的随机状态/调用/索引及影响结果的随机取值。已有 C053 受控40 tick五槽与随机标量配对可作限定样本，但没有证明整张随机表字节相同，也不代表自然选招；新首差涉及随机取值时才追加被消费项证据，不重建无关整表。
3. 一例非用户例外的战斗表现：复用 C040 同一生产 tick 的真实 Game View 与正式规则快照配对。该图只证 Unity 实际可见角色、地面标记和无明显缺图；正式 EXE 同帧 Present 像素未取得，故画面一致性仍为未知，后续只对可复现的非例外视觉首差增加可比证据。完整背景、固定相机、地图和 UI 例外不做跨视口逐像素门。
4. 一例实际战斗 cue：复用 078 自然事件、正式 clip、生产 `AudioSource.isPlaying=true` 的原 Scene 阳性。a7已导入 `AudioClip`，自身实播未知；只有该 cue 在玩家场景出现可复现缺声、或共用播放器/路径写者改变时才做 a7 定向回访，不逐约 970 个路径核声卡 PCM。
5. 同一版本的战斗有序退出、重进及残留核查；C056 原 Scene 双子体有序关闭零残留复用为退出子门，重进子门由原 Scene 两轮生产 World/tick/退出见证限定通过。复用现有 shutdown/World/pool 探针与场景状态，不清理用户对象或保存 Scene。

| 代表子门 | 当前状态 | 证据边界 |
| --- | --- | --- |
| 鸣人首253后续J | `PASS_SCOPED` | 原Scene合成物理键、正式源码选定字段132/132；真人手按与画面时差未知。 |
| 战斗结果/对象链 | `EVIDENCE_REUSE_SCOPED` | C053受控40tick五槽1136/1136、随机标量200/200；非自然选招、非整表同字节。 |
| 非例外战斗画面 | `EVIDENCE_REUSE_PARTIAL` | C040同tick原Game View与规则640/640；正式同帧Present未证，不能称画面完全一致。 |
| 实际战斗cue | `EVIDENCE_REUSE_SCOPED` | 078自然正式clip生产voice播放；a7自身实播未知、按首差触发。 |
| 有序退出 | `EVIDENCE_REUSE_SCOPED` | C056双子体关闭后World/slot/pool/Sprite零残留。 |
| 同冻结版重进 | `PASS_SCOPED` | 原Scene同六身份连续两轮生产World/tick/角色活跃，退出后World解绑、活动池对象/sprite为0且Scene clean；详Q12生命周期见证报告。 |

执行规则：先复用总表已有 C040、D-024、Q07 三键、Q08、Q09、Q10 证据，只运行缺失的最小代表例。原 Editor 必须非 Play、编译/测试空闲且 Scene clean；运行前后核对受保护文件 SHA。探针只写唯一新结果文件，保留失败原件；未达到前置则不启动 Play。若出现可裁决的正式同初态首差，回到共用 owner 建独立 Task/Change 后最小修复，不加角色特判、不改 DAT 数值、不扩菜单/结果页/原版背景或模式 DAT。除已完成的独立 Editor-only 生命周期见证外，本包未编辑生产脚本、Scene、Prefab 或用户已有修改。各子门按表中限定状态单独判断，不能用编译或导入状态替代运行时结论。
