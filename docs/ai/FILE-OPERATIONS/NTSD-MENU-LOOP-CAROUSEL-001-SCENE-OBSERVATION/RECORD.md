Operation/Event ID: NTSD-MENU-LOOP-CAROUSEL-001-SCENE-OBSERVATION
Status: UNKNOWN_CAUSE
Change: NTSD-MENU-LOOP-CAROUSEL-001
Observed preflight and repeated intermediate Scene SHA: E8BBC47AE14799BB87DB8B14D9FDB6B9D6CD546E0FA62F47A7DD83E2685D2CBD.
Final Scene SHA: 5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D.
Final manage_scene/get_active: NTSD_Menu, clean (isDirty=false), eight roots. Original Menu Play test finished and XML Passed.
Current bytes preserved as NTSD_Menu.observed.unity with timestamp/hash JSON. Initial Scene was not overwritten/backed up by this task, so no claimed byte-level diff against E8 baseline.
This task never called SaveScene, scene save, Git restore, or scene serialization edits. Its named UnityTest opens the saved Menu only from a clean empty Test Runner scene, enters/exits Play, performs runtime menu event tests. Test framework/editor/user/other process writer is not identified by this observation.
All further Editor operations stopped. No restoration or reload; ask project/user to identify whether this saved change is intended before more Editor operations. Do not attribute causality from time proximity.
