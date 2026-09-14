# state9998额外删除退休结果

COMPILE_PASS / FOCUSED_PARTIAL / SCOPED_PLAY_PASS；完整对齐未完成。准确五脚本，唯一生产行为变化是去掉SimulationWorld.SerialTickAll末尾CleanupState9998Entities及私有函数，另外两生产文件仅修过时注释。Serial其它职责、主pass次序、对象池和有序关闭未改。SelfCheck GT09改为保留实体，无新增字段/schema。

原完整C++ driver224场景/672tick全存活，frame/lifecycle无错误，输入/状态控制与重复stdout核验见SOURCE-DRIVER-WITNESS REPORT。Unity RED七类型都失败，104个场景曾被额外删除；修复后224场景删除差异0。联合29项通过28项，最后单类复验6/7；失败断言保留，type0 HP0的16场景×3tick仍有48次动作/状态差异。出生HP/bound/base、revive与display字段补齐后仍为beforeSerial=186，定位到C25上游而非本扫描。下一必须核验RunLateDeathOpointPreCleanupPhase及持有关系，不能只以存活代替动作对齐。

真实NTSD_Battle旧内容Scene：tick5当前实体frame descriptor设为9998供实际SerialTickAll消费，未改共享DAT/cache；调用后实体存活且state9998仍在，checksum恢复4→4。Play-pass.json与Play-shutdown.json证明随后World/slot/logic/render borrowers均0、两帧Stopped、Editor已退出Play。该证据仅针对Serial读取/清理，不是完整input/physics tick、HP0动作或正式DAT/图片表现验收。

完整SelfCheck最新已越过GT08和更新后的GT09，仍在CheckStateTransformLandingMatrix type2高速落地方向失败：frame0/vx4/vy-5/durability19已观察，dir left与旧right预期冲突，须独立源码核验。结果保存在SelfCheck-next-landing-fail.result。未声称全自检通过。

下一唯一Task NTSD28-Q06-C25-DEAD-CHARACTER-EXTRA-BOUNCE-AUDIT-001。随后处理上述落地方向首差异，再回fragment OID0/999准入及完整driver slot边缘/父frame与资源Q07。15/23/26/2/2和raw47/3不变，总目标/Q06 ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗/Unity-GAS架构修改，无提交/push/文件删除。
