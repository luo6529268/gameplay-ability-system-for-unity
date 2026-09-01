# Task Contract — CLIENT-CPP-RESULTS-SCENE-HOST-TICK-ALIGNMENT-001

> 状态：`FOCUSED_TEST_PASS / RESULTS_SCENE_HOST_TICK_ALIGNMENT_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_3_3 / RELATED_90_90 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

按C++ release `SceneState::RESULTS`顺序修正Unity：Results active时先完成完整world tick且不poll battle entity human input，再从同一immutable `FrameInputSet.PressedButtons`消费P1/P2结果菜单edge。只允许Server Task列出的四个runtime文件、focused test和必要SelfCheck callsite；不改结果数学/schema/package/30Hz/Input Actions/marker。

关闭证据：test-first `0/3`；final focused `3/3`、related `90/90`、Unity compile0、fresh SelfCheck PASS、Server Debug/Release和四suite双配置通过。Results math/schema/package/30Hz/Input Actions/marker均未改。
