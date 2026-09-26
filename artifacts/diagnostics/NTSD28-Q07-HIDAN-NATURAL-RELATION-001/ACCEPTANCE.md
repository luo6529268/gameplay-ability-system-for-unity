# Q07 Hidan natural-input catch relation fields

Status: `FOCUSED_TEST_PASS`; Q07 remains `IN_PROGRESS`.

The original Unity Editor imported and compiled the new focused test after a full asset refresh. It ran the existing formal-content Authority400 full driver with the same action0 natural-input X580/X1200 fixtures used in the 40-tick raw comparison. For every completed tick it read the production `actor.Runtime.CaughtSlotIndex` and `target.Runtime.CatchSourceSlot90` and compared them to the root formal EXE trace's `catchTargetSlot8C` and `catchSourceSlot90` for slots0/1. Each case compared 40 × 2 = 80 values without a mismatch, 160/160 total. X580 includes the reciprocal relation beginning at tick11; X1200 remains unlinked.

The first combined two-case EditMode job `e1056ffed8d44c4e98a0d523988b0769` finished FAILED because a concurrently queried MCP bridge logged `Cannot access a disposed object: System.Net.Sockets.NetworkStream` during X1200. This is preserved as an infrastructure/test-log failure. Two fresh single-case jobs, with no bridge polling during execution, then passed: X1200 job `783fb81f87414f5ab395c5e6d94d57d1` 1/1; X580 job `3cf9438ef53c49cca3b6b43ff65f8fa4` 1/1. The later results establish the focused field check; they do not erase the first failed run.

This fills the relation-field gap left by raw capture. Along with the source/root EXE natural input and original Editor 11-field raw results, it supports this one Hidan 40-tick path. It does not prove physical-key Battle Scene Play, visual pixels, all characters or completion of Q07. Only the new Editor test and meta were added by this package; production, DAT, Scene, config, character images, audio and nonbattle files were not edited.
