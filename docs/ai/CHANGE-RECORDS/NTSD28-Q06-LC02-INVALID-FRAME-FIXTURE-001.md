<!-- CHANGE-RECORD
id: NTSD28-Q06-LC02-INVALID-FRAME-FIXTURE-001
status: VERIFIED
change-kind: LC02_NATIVE_TERMINAL_FIXTURE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Formal C25 step_frame_slot terminal action >=999; implicit frame857 valid after Q06 Native accessor migration.
evidence: Fresh full SelfCheck stopped LC02; fixture constructor uses legacy MaxFrameIdExclusive=857 as invalid action.
-->

# LC02失效帧夹具修正

准确单文件FrameLifecycleDestroyProbeSelfCheckEntity构造的两个857阈值改为显式终止动作1000；LC02原不调用DestroyEvent、不发声音、释放slot断言全部保持。857已经是合法隐式零帧，不能再用作invalid夹具。无生产改动或新关闭职责，原FAIL存WEAPON-PIECE-TRANSACTION artifacts。验收完整SelfCheck，失败按下一首差异记录；回滚仅两个赋值且需批准。

构造器两赋值已改1000，LC02原断言保留；待完整SelfCheck。

复验更正：仅改1000后完整SelfCheck仍LC02失败（第二原FAIL已存）。该probe还继承LF2Entity空SimFrameTick且没有DAT Wrapper，原先依赖旧tail直接检查857；不是实际frame producer。保持LC02为纯cleanup consumer fixture，构造器显式提供NativeLifecycleResolutionPending/code1000作为该consumer输入，原不调用DestroyEvent/不发声/释放slot断言不变。真实producer与完整Late已由父15项/原2676矩阵验证，不能将此mock宣称新的producer证据。准确路径仍一个文件同一构造器，新增两runtime赋值在编辑前声明。

当前VERIFIED / TEST_FIXTURE_ONLY：最后完整SelfCheck实际已越过此fixture，停在后续GT08；不表示整套SelfCheck通过。完整运行原FAIL及五次请求/结果见父WEAPON-PIECE-TRANSACTION artifacts。
