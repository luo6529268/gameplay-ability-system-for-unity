Review the P8-D v4 implementation for correctness and regressions. Do not edit files.

Scope:
- Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkWindow.cs

Focus on false PASS paths, completed-frame correlation, sample completeness, profiler counter semantics, memory fallback honesty, leak gating, A/B backend restore, runner exit code, and tests. Verify that presenter-local counts are never used as actual draw calls. Report severity-rated findings with exact paths/lines; do not suggest completion if a P1 remains.
