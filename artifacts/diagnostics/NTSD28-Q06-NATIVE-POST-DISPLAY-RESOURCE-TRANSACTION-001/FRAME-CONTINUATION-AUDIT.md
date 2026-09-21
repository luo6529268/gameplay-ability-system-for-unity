# Real frame continuation contract (2026-09-21)

Read-only source/Unity audit; this document is not runtime proof.

Formal current closure: simulation_tick_driver.cpp around 989-1047 invokes display, post resources, reads current action for computer state, step_frame_slot, reaction timers, and finally commits previous_action_078 after state18 work. frame_machine.cpp27-35 resets counter and commits action_latch only when current action differs, then increments counter. For a new action with wait100, the first continued frame has counter1. battle_world.cpp8628 onward reads the previous latch state before frame_machine and applies transition side effects afterward.

Unity LF2Character.SimFrameTick -> LF2Entity.RunCommonFrameTick -> active native C25 transaction. LF2Entity.cs6498 onward obtains current Frame.N native descriptor, preserves old Trans.WaitCounter until entry detection, rebinds Frame.D, resets/increments AttackingCounter and commits latch. LateEntityLifecycleModule wraps this in Begin/EndNativeC25FrameTickForWorldPass and then advances reaction timer and commits Frame.Prev. Therefore a real frame_0mp transition must be validated with an ordinary LF2Character, synchronized initial Frame.N/Runtime.Frame, and real Late pass; the earlier FrameProbe override proves only placement.

Representative expected result: action900, descriptor900, latch900, counter1, previous078=900, timer1->0, with display retaining pre-post values. Use a nonterminal ordinary frame and no opoints so unrelated lifecycle branches do not obscure this seam. No production change is justified by this static audit alone.

Replay representative should use actual prepared catalog/factory and existing snapshot APIs, compare result/checksum after restoration with identical inputs, and retain source-derived expectation assertions. No new persistent fields or schema are required.
