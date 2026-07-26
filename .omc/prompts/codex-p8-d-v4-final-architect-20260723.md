# P8-D v4 final architecture verification

Review the current, latest source for the Unity NTSD central battle-render benchmark v4. This is a read-only architecture audit; do not edit files.

The previous short probe correctly returned INCOMPLETE because `Memory/Texture Memory` produced zero for an owned Texture2D workload. An executor is independently fixing that issue, so distinguish the current concrete bug from broader design findings. Inspect the actual files and repository context rather than trusting older reports.

Required checks:

1. The mandatory metric registry is closed: missing, duplicate, unknown, stale, zero/invalid values, and wrong-generation samples cannot produce PASS.
2. FrameTiming and ProfilerRecorder data are generation-owned and exactly one completed-frame sample is admitted per requested sample.
3. Decide explicitly whether a globally available `drawCalls == 0` must be rejected for the benchmark workload.
4. Current-scene metrics not actually measured must be Missing/Incomplete rather than Fail or Pass.
5. Suite exceptions restore the prior backend and dispose active sessions without masking the original exception.
6. Leak/teardown gates cover presenter disposal, deferred Unity destruction, owned managed/graphics resources, and cannot pass with missing evidence.
7. Central and Legacy runs use equivalent workload/input/runtime checksums and enforce exact expected sample counts.
8. Windows Development Player and Editor matrix paths exercise the same v4 policy.

Report findings first, with P0/P1/P2 severity, exact file and line references, and concrete fixes. End with explicit counts. Completion requires P0=0, P1=0, P2=0; stylistic P3 notes may be separate.
