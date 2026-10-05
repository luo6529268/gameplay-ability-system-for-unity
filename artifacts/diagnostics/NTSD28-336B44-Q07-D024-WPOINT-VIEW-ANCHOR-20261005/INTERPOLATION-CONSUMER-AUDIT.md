# 持有挂点修复后的插值消费者核查

2026-10-05：`STATIC_CONTRACT_REVIEW / NO_NEW_CONFIRMED_FIRST_DIFFERENCE`。本轮只读当前正式playable源码与Unity可达消费者，复用上一轮[原Scene两tick命令证据](SCENE-ACCEPTANCE.md)；没有追加C#、生产改动、Play或测试。该结果不能代替R120持有武器的实际画面/GPU验证。

## 正式规则与实际构建参与性

当前根正式EXE身份按本目录输入manifest核对为336B44。`source/ntsd28_playable/scripts/build.ps1`的正式`Ntsd28Playable.exe`参数列表第707行包括`src/presentation_interpolation.cpp`，第737行`Target=playable`在该构建之后返回。本审阅使用该正式构建闭包文件，不引用测试版或旧C#规则。

`sample_render_presentation28`依次要求相邻tick、slot/object/generation身份一致、六项关系一致、三轴位移连续。六项关系为owner、linked parent、linked child、catch target、catch source、interaction state；任何变化均跳过该实体插值。稳定关系下，对每个实体独立计算`lround(previous+(current-previous)*alpha)-lround(current)`，alpha限定0～1；没有按父实体强制覆盖子实体位置的分支。`interpolate_render_snapshot28`对本体加X及Y+Z、影子仅加X/Z。

因此拾取或释放时关系变化导致本体离散切换，是正式规则；稳定持有时的角色/武器也各自取整，不能仅因中间显示相位两端不严格同点，就添加共用规则之外的“强制跟手”。需先与同相位正式输出比较，确认非例外首差。

## Unity消费者闭环

| 正式合同 | Unity实际路径 | 当前静态结论 |
| --- | --- | --- |
| 相邻tick/身份/六关系/连续性门 | `BattlePresentationMotionState`冻结source精确X/Z、Y、原生速度及相同六关系；`BattlePresentationMotionSampler.Sample`按相同顺序比较 | 对应；另有source位置未初始化的保护门，不把未初始化对象当正式同态载体。 |
| 源空间lround后取delta | `RoundedDelta`使用AwayFromZero；Sample再用共用X/Y/Z倍率投影显示delta | 对应当前正式取整及用户D-024比例；没有先放大坐标再取整。 |
| 完整tick本体挂点补偿 | `LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels`→不可变`HeldVisualAttachmentOffsetPixels`→`MaterializeCommands` | 上一轮原Scene实测publication/plan tick7、双方WPoint差0/0，证据复用。 |
| 呈现插值只改显示 | `BattleCentralRenderSystem.MaterializeLatestPublishedFrame`先物化命令，再调用`DisplayMotion.Prepare/ApplyToCapturedCommands` | 共用Y倍率来自SpatialProjection；挂点补偿没有在此重复计算或再乘倍率。 |
| 本体Y+Z/地面Z | `ToWorldBody`使用ViewY+ViewZ；`ToWorldGround`仅ViewZ；`WithPresentationOffsets`只更新命令位置及显示挂点 | 对应；没有把命令位置或插值结果反写runtime。 |

本轮额外核对`LF2Entity.RunSharedNonCharacterDatFrameAdvance`：原生Vx用于源位移/门槛，显示额外X才乘共用倍率。当前MotionState的MotionX/Y/Z读取原生runtime速度，不应再除显示倍率；不为此前未经证实的“速度已经放大”假说改连续性门。

已有`BattlePresentationMotionSamplerEditorTests`包含取整先于倍率、全局Y只投影显示、六关系变化和非连续位移保护的断言。本轮仅阅读覆盖范围，**未运行这些测试**，不将“测试代码存在”写成新PASS。原Scene持有挂点只证明完整tick；其它显示相位和GPU仍按总表真实非例外首差/终验必要条件触发，不因未知重复旧样本。

## 下一步边界

未发现本核查可裁决的新首差，故不新增强制ONE、不添加持有武器特例或角色矩阵。总表现有F04剧情部署hold、C054等自然入口条件仍保持，不能借本审阅恢复阴性扫描；Q07/Q09/Q12及总目标开放。输入文件/所读范围/哈希见同目录`interpolation-review-inputs.json`，保留本轮阅读版本。
