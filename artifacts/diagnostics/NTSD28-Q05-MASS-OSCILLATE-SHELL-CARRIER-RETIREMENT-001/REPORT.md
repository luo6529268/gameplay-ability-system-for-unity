# Q05 Mass/Oscillate载体退休限定交付

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / CARRIER_REMOVAL_VERIFIED / JOINT_SCHEMA_PENDING。Q05与总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE；不是整阶段发布证书。

## 清理范围

准确21脚本（8生产、12既有测试/探针、1新测试）：CharacterMechanicsContext移除mass字段/构造参数，20处调用逐项删除原第4参，保留runtime/frame/width/minSpeed/gravity顺序。LF2Character._mass/MassForFrameAdvance、初始化查询及CharacterShell Mass capture/restore删除；LF2Entity/ECS不再传递旧mass。

LF2EffectState的Oscillate/OscillateDirection及Reset、EntityBaseShell两成员/capture、LF2Entity.restore移除。Num、速度、Stuck、Blink、Super、TimeIn/Out、BlinkCounter、Sprite偏移、真实TrackerParent和持有/OPoint shell状态保持。

NTSDSpec仅移除Mass/Oscillate字段、构造参数、表内对应值与两个旧查询；剩余历史兼容表/API保持，未删除文件或变为空类。当前生产不再查询NTSDSpec；既有compat selector诊断仍通过。目标关键词在Animation/Simulation生产中仅剩明确保留的NTSDGlobal.Default.Machanics.Mass常量，无消费，未顺手删无关API。

## 证据

- 新RED14：13FAIL/1PASS（字段仍存在/五参构造缺失；有效Effect Reset原已通过）。GREEN job351f7ead02f7424b9cecff18dad9d36f共890/890 PASS，15个定向类覆盖新absence/构造、原Q04 native物理8行、真实Character/ECS（对象ID0/100/150）、效果优先级/Blink/Timeout/deferred velocity、旧API退休与compat selector、两shell/full snapshot/restore/2F8/raw/no allocation等。
- 已不存在的mass/振幅sentinel用例按数据模型变更修订：Q04核心见证只跑一次，Character/ECS采用对象ID而非已删除mass字段；效果消费者变化真实初始Sprite偏移7/-3/0，保留完整动作和偏移检查。旧producer只合并仅由退休sentinel区分的重复案例，保留effectNumber/优先级/全部有效payload。原artifact和20份事前脚本副本保留，未篡改旧通过/失败事实。
- CharacterMechanics.StepBattleLogic、LF2LivingObject.ProcessEffects与EffectCreate主体在统一换行后逐字一致，rule-body-stability.json保存摘要。7条Q04原版EXE/源码身份均未漂移。
- 完整SelfCheck请求2026-09-13T09:15:51.1530406Z，09:16:30Z结果PASS且mtime晚于请求。之后只修订Play诊断夹具，production与SelfCheck调用路径未再改变。
- Play效果探针PASS：当前真实场景slot0/CentralOnly，production presentation snapshot、偏移保持、Blink、Timeout、deferred motion和cleanup通过；未绑定SpriteRenderer，不宣称验证了未执行的组件绘制检查。
- 首轮新运动Play夹具把ID0/100/150整tick假定相同，ID100 X506而预期505（速度4符合物理结果）。这不是原版对照首差，也未证明是本包回归。保留play-first-id-specific.json，不修改规则。改用同ID0的forward/reverse/stationary三向量后Play PASS，tick192→193的地面位置/速度及随后落地检查通过，cleanupPassed=true。纯Dynamics多ID检查仍保留。该证据为实际driver中的注入角色，不能代替自然按键/技能整链验收。
- 两次Play均通过桥接进入/退出，最终编辑器非Play，Scene isDirty=false/root14，文件SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持。error CS0。MCP is_changing在isPlaying时也会为true（当前实现使用isPlayingOrWillChangePlaymode），因此以实际probe结果判断可执行状态，没有使用computer-use。
- 保护3059：2988相同/53已存在或准确声明差异/18既有Foot缺失，无新缺失；新增13个基线差异路径均被本Record覆盖。Foot任务外目录/删除保留，资源与Scene未由本包修改。
- 账本校验通过，491 Records /124 governed code files；最终结果回填ledger-final.txt。没有新manager/queue/worker或shutdown阶段。

## 后继

下一唯一Task NTSD28-Q05-FIVE-RESERVED-CARRIER-RETIREMENT-001，继续同Q05步骤2清GrabbedBy、ReleaseTick、HolderCopySlotIndex/任务别名、TrackerFlag、WeaponState及copy/reset/ECS/hash/diagnostic。保留有效TrackerParent/Owner/Spawner/+2F8与frame state API，不重复本包或Q04行为退休。

Character/base shell形状已经删除旧字段，版本号暂仍1/1，只是批准的同Q05未发布中间态；必须在步骤4与entity13/aggregate21/checksum24统一升2/2。步骤3 identity/双OPoint队列guard、步骤5旧版本拒绝/新回放/Play仍待，不允许跨版本交换或提前Q07资源部署。R13仅字段删除子条件PARTIAL_RETURN，联合schema和其余reserved未关闭；R15仍等待版本/identity证据。

Unity/GAS、非战斗、33ms/3ms、十一阶段、默认stage.dat USER_HOLD及批准例外保持；禁止computer-use。正式DAT/角色图片迁移、Q06新规则消费、Q09/Q10表现及最终完整对齐仍未完成。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，491 Records / 124 governed code files，ledger-final.txt已保存。
