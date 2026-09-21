# NTSD28-Q06-FRAME-MOTION-TAIL-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。
权威：当前正式EXE B1E13AE17...和playable closure07CD47；battle_world.cpp apply_frame_motion、frame_motion.cpp FrameMotion28::apply。
纠正此前platform文档推断：自身dvx/dvy/dvz正式按integer读取，不是float缺口；linked平台读取number是独立合同。既有Unity整数kernel保留。确认缺口为DelayTimer134正值速度乘0.25与随后float dx/dy/dz positional tail。
准确初始范围仅：
- `Tools/NTSD28AuthorityTrace/frame_motion_tail_witness.cpp`
- `Tools/NTSD28AuthorityTrace/validate_frame_motion_tail_witness.py`

实际source API组合见证，公开DAT/initial/after；测试自身int对fraction、delay正零负、facing、positional单轴/多轴以及linked-before-own-positional顺序。双跑身份相同及独立手算table校验。未证明fulltick/EXE画面。
Unity生产/测试尚未声明；先取得source结果后追加精确路径。无新runtime owner/关闭阶段/schema变更；不改正式源码、资源、Scene、非战斗功能，不用computer-use。现有平台与Destroy-owner未关闭门槛保留。
风险：整数与float读取混淆会误改已对齐速度kernel，本包必须锁定此区别。回滚仅在授权后撤销本包精确新增内容，保留dirty工作。
退出：source见证→Unity RED→完整tail接入→受影响分支/真实following回放与稳定包验收，不以source或单元检查称完成。

## Unity RED precise scope

Add Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs only. Same 12 source cases through actual World/factory, actual frame-motion entry, initial and final value comparison; cleanup via existing ordered shutdown. No production edit yet.

Production exact addition: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs, ApplyNativeFrameMotionForWorldPass tail call and private helper only. RED 5/12 pass, 7 expected tail failures. No new state or lifetime owner; no schema change.

Current checkpoint: FOCUSED_TEST_PASS, source12/61 + Unity17/17. Remaining actual following/replay and stable package runtime gates; not VERIFIED. See UNITY-FOCUSED.md.

Following extension: existing source CPP only, opt-in --following rows1/7/9 via actual SimulationTickDriver28 and same definitions. Preserve immediate pinned source bytes. Platform shadow audit recorded separately at parent SHADOW-CONSUMER-AUDIT.md; Q09 display return explicit, no shadow production edit here.

Renderer extension exact existing fixture NTSD28Q06FrameMotionTailEditorTests.cs plus FRAME-MOTION-TAIL-owned new Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformMotionPlayProbe.cs. Actual factory/Renderer and same source expected values; no display parity claim.

VERIFIED bounded frame-motion tail; ACCEPTANCE.md. Parent platform fulltick/replay and Q09 actual shadow display not included.
