<!-- CHANGE-RECORD
id: NTSD28-Q05-WORLD-CLOCK-PHASE-FIXTURE-001
status: VERIFIED
change-kind: TEST_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
authority: Formal playable simulation_tick_driver.cpp native resource/frame/slot tail; retired positive-link phase contract.
evidence: JOINT-SNAPSHOT-CHECKSUM-VERSION first-related-267-PASS-9-FAIL.xml; test.before.txt
-->

# World clock phase索引旧夹具

IN_PROGRESS / TEST_ONLY。版本相关首次276中1个旧FullTick_RecordsC23AndC24BeforeC25Skeleton失败，既有Unity删除positive-link phase后硬编码index23已是FramePostProcess。当前正式simulation_tick_driver.cpp末尾明确finalize_horizontal_hit_impulses→begin_native_resource_tick→begin_frame_tick→ascending slot loop；Unity PreFrameBounds→FramePostProcess→NativeResourceTick→NativeFrameTick→LateEntityUpdate次序保持，不能为测试恢复退休pass。

仅该测试定位唯一PreFrameBounds后断言这5项相邻顺序，避免将早先其他phase数量当C23/C24语义。既有schema断言由JOINT-SNAPSHOT-CHECKSUM-VERSION负责，本Record不改其数值。保留原FAIL；验收最窄此类加相关回归及SelfCheck。无production/资源/Scene/非战斗更改，禁止computer-use；回滚需批准，按此preimage差量且保留已升版本。

## 复验更正

第一次修正漏掉同方法末尾的FrameAdvance index28及总数34；最终相关287中286PASS/此1FAIL留证。现FrameAdvance也以boundsIndex+5检查，正常phase总数按已退休positive-link的33保留严格断言，不用删断言或改变生产使其通过。下一仅该精确用例复验。

## 出口

VERIFIED_TEST_ONLY：精确用例1/1通过，原287中的1FAIL闭合，完整SelfCheck PASS；生产phase保持，历史失败未删除。
